# Job System Design

## Purpose and scope
This document describes how backup jobs are orchestrated and executed across the application. It focuses on runtime flow, boundaries, data contracts, and invariants for:
- backup
- restore
- delete backup version(s)
- delete file(s) across versions
- modify/decrypt existing backups

It covers `BSH.Engine` job execution plus orchestration in both UI shells (`BSH.MainApp` and `BSH.Main`).

## Design goals
- Keep all backup semantics in one engine (`BSH.Engine`) used by both shells.
- Present a uniform job lifecycle (`JobState`, `IJobReport`) regardless of operation type.
- Support cancelable long-running jobs with user-visible progress and conflict handling.
- Support multiple storage backends via `IStorage` without branching in callers.
- Keep DB metadata and storage payload updates consistent through transactional phases.

## Non-goals
- UI layout and dialog design.
- Full database schema reference beyond job-relevant tables.
- Preview subsystem details.

## High-level architecture

### 1) Orchestration layer (shell-specific)
- WinUI shell: `BSH.MainApp.Services.JobService` + `StatusService` + `ScheduledBackupService` + `OrchestrationService`.
- WinForms shell: `BSH.Main.Modules.BackupController` + `StatusController` + `BackupLogic`.

Responsibilities:
- gate execution (one active task, media available, password available)
- own cancellation token lifecycle
- invoke engine service (`IBackupService`)
- coordinate UI affordances (status window, overwrite prompts, exception dialogs)

### 2) Engine service layer (shared)
- `Brightbits.BSH.Engine.Services.BackupService` is the single entrypoint for all job types.
- It constructs concrete job objects (`BackupJob`, `RestoreJob`, `DeleteJob`, `DeleteSingleJob`, `EditJob`) and attaches an `IJobReport` observer.

Responsibilities:
- run one engine task at a time (`currentTask` guard)
- construct jobs with storage/db/query/config dependencies
- dispatch operation intent (`ActionType`) to observers

### 3) Job execution layer (shared)
- Base class: `Brightbits.BSH.Engine.Jobs.Job`
- Concrete jobs: `BackupJob`, `RestoreJob`, `DeleteJob`, `DeleteSingleJob`, `EditJob`

Responsibilities:
- perform operation-specific workflow
- publish progress/state/events through observer callbacks
- apply common post-processing: update free space, upload database file, surface exceptions

### 4) Infrastructure boundaries
- DB boundary: `IDbClientFactory` / `DbClient` (`System.Data.SQLite`).
- Storage boundary: `IStorage` implementations (`FileSystemStorage`, `FtpStorage`) via `StorageFactory`.
- Optional VSS boundary: `VolumeShadowCopyService` -> named pipe RPC (`IVSSRemoteObject`) -> `BSH.Service`.

## Core contracts and state model

### Command/event contracts
- Job command surface: `IBackupService`
- Status/event surface: `IJobReport`
- UI observer surface: `IStatusReport` (shell-specific, fed by `StatusService`/`StatusController`)

### Job states
`JobState` values:
- `NOT_STARTED`
- `RUNNING`
- `CANCELED` (defined but currently not emitted by concrete jobs)
- `ERROR`
- `FINISHED`

### Operation types
`ActionType` values map user/system intent to status observers:
- `Backup`, `Restore`, `Delete`, `Modify`, `Check`, `Preview`

### Overwrite negotiation (restore)
- User policy enum: `FileOverwrite` (`Ask`, `Overwrite`, `DontCopy`)
- Per-conflict response enum: `RequestOverwriteResult`
- Persistent per-run choice (`OverwriteAll` / `NoOverwriteAll`) is stored in status service/controller and reused.

## Data model used by jobs
Primary tables touched by jobs:
- `versiontable`: backup versions (`versionID`, `versionDate`, `versionStatus`, metadata).
- `filetable`: logical file identity (`fileID`, path/name).
- `fileversiontable`: physical version instances (`fileversionID`, `filePackage`, size, timestamps, `fileType`, `longfilename`).
- `filelink`: many-to-many link from version -> fileversion.
- `foldertable` + `folderlink`: empty-folder preservation.
- `folderjunctiontable`: localized/display-name mapping for junction-style folders.
- `configuration`: runtime settings consumed by job logic.

### `fileType` encoding invariant
`BackupJob` writes file representation as:
- Local/file-system storage: `1` plain, `2` compressed, `6` encrypted
- FTP storage: `3` plain, `4` compressed, `5` encrypted

Restore/delete/edit jobs must preserve this mapping.

## End-to-end flows

### A) Manual backup flow
1. Shell service/controller creates a new cancellation token.
2. Preconditions:
- no active task (`StatusService.IsTaskRunning` / `StatusController.IsTaskRunning`)
- media reachable (`CheckMediaAsync`, optionally wait dialog)
- password available if encryption enabled
3. Shell invokes `IBackupService.StartBackup(...)` with `IJobReport`.
4. `BackupService` creates `BackupJob` and starts background task.
5. `BackupJob` executes phases:
- check medium, validate source folder
- begin DB transaction
- create new version row
- enumerate files with exclusion handlers (`FileCollectorService`)
- process empty folders
- process each file:
  - detect unchanged files for incremental linking
  - copy to storage (plain/compress/encrypt)
  - fallback path: retry with VSS for difficult I/O cases
  - persist `fileversiontable` + `filelink`
- cancellation/error handling:
  - on cancel: rollback DB + remove new version directory
  - if medium becomes unwritable: rollback and mark error
- commit behavior:
  - if new files exist: commit
  - else: rollback and attempt “refresh” by directory rename + version date update
- update configuration counters and free-space info
- close DB pools and upload DB file to storage (`UpdateDatabaseOnStorage`)
6. Observer receives final `JobState` and exception list.

### B) Restore flow
1. Shell validates preconditions and calls `StartRestore`.
2. `RestoreJob` resolves target set:
- single file restore (name/path filter)
- folder restore (`filePath LIKE ...`)
3. Destination resolution:
- if destination omitted, derives from stored backup source roots
- otherwise uses explicit destination
4. Per file:
- optional overwrite negotiation via `RequestOverwrite`
- copy from storage based on `fileType`
- restore timestamps
- continue on per-file failures; collect `FileExceptionEntry`
5. Restore empty folders from `foldertable` / `folderlink`.
6. Emit final state (`ERROR` if any file failures else `FINISHED`).

### C) Delete version flow
1. Shell calls `StartDelete(version)`.
2. `DeleteJob`:
- medium check, open storage, begin DB transaction
- query file versions unique to the target version
- delete physical files from storage
- delete metadata rows (`fileversiontable`, `filelink`, orphan cleanup in `filetable`/folders)
- mark `versionStatus = 1`
- commit
- for file-system storage, attempt cleanup of unreferenced version directories
- bump storage-generation marker (`OldBackupPrevent`)
- upload updated database to storage
3. Emit exceptions and terminal state.

### D) Delete single file(s) across versions
1. Shell calls `StartDeleteSingle(fileFilter, pathFilter)`.
2. `DeleteSingleJob`:
- resolves `fileID` set by exact file or path filter
- deletes all corresponding physical versions from storage
- removes linked DB metadata
- commits and uploads DB file
- emits aggregated errors

### E) Modify/decrypt flow
1. Shell calls `StartEdit` after password precondition.
2. `EditJob` iterates all backup files and decrypts encrypted payloads in place (`storage.DecryptOnStorage`).
3. It rewrites `fileType` values from encrypted variants to plain variants.
4. Sets configuration encryption flags to disabled and uploads DB update.

## Scheduling and automatic triggers

### Scheduler implementation
- Shared scheduler engine: `SchedulerService` (Quartz).
- Supports once/hourly/daily/weekly/monthly scheduling through wrapper methods.
- Scheduled entries come from `schedule` table (`timType`, `timDate`).

### WinUI (`BSH.MainApp`) scheduled flow
- `ScheduledBackupService.StartAsync` selects mode based on `TaskType`.
- `Auto` mode schedules hourly backup (`ScheduleAutoBackup`) and runs cleanup policy.
- `Schedule` mode loads DB schedule entries and registers Quartz triggers.
- On trigger, service runs silent backup via `IJobService` and optional retention cleanup.

### WinForms (`BSH.Main`) additional trigger path
- In addition to scheduler triggers, legacy shell can start backup when backup drive appears (`UsbWatchService`, `DoBackupWhenDriveIsAvailable`).

## Concurrency and cancellation model
- One active engine task is enforced in `BackupService` via `currentTask` status checks.
- Shells independently guard against concurrent starts using status services/controllers.
- Cancellation is cooperative via `CancellationTokenSource` created per operation run.
- `BackupJob` and `RestoreJob` poll for cancellation during per-file loops.
- Multi-item shell loops (multi-restore, multi-delete) break when cancellation is signaled.

## Error handling and recovery behavior

### Failure granularity
- Per-file failures are collected in `FileErrorList` and reported after operation.
- Operation-level fatal failures (medium unavailable, DB update failure) abort job.

### Recovery semantics
- Backup path prioritizes consistency:
- rollback DB transaction on cancel/critical failure
- delete partially written version directory on rollback paths
- upload DB to storage only after local DB writes are finalized

### Media availability gating
- `CheckMedia` performs backend-specific checks and includes short caching for non-FTP checks.
- Optional wait-for-media loops exist in both shells (`WaitForMediaService`).

## Architectural invariants
- Concrete jobs never directly call UI frameworks; all UI interaction is through `IJobReport` callbacks.
- Storage-specific behavior is isolated to `IStorage` implementations; job code selects by capability/type mapping, not by ad-hoc shell logic.
- Job metadata is persisted in SQLite before becoming visible to user queries.
- Backup source selection and exclusion policies are evaluated before copy, not during restore.
- Database upload to backup medium is part of job completion for mutating operations.

## Extension points
- Add new storage backends by implementing `IStorage` and extending `StorageFactory`.
- Add/adjust file/folder exclusion behavior via `IFileExclusion` / `IFolderExclusion` in file collector setup.
- Add new job types by extending `Job` and wiring through `IBackupService` + shell orchestration.
- Add new observer consumers by implementing shell `IStatusReport` and registering with status service/controller.

## Known design debt and risks
- `BackupService` uses `Task.Factory.StartNew(async ...)` in multiple methods, which produces nested task behavior and non-obvious completion semantics.
- `JobState.CANCELED` exists but current concrete jobs typically signal cancellation as `FINISHED` plus canceled status text.
- There is duplication between WinForms and WinUI orchestration logic (media/password/preflight, batch loops).
- `DeleteSingleJob` uses `_LONG_FILES` while other jobs use `_LONGFILES_`; this inconsistency can affect long-path cleanup correctness.
- Some SQL in jobs is string-interpolated; query composition should continue moving toward parameterized forms for safety and maintainability.

## Suggested future refactors
1. Replace `StartNew(async ...)` with `Task.Run`/direct async pipelines and normalize task-return semantics.
2. Introduce a shared shell-agnostic orchestration helper to remove WinForms/WinUI duplication.
3. Standardize cancellation terminal state (`CANCELED`) and telemetry around abort reasons.
4. Centralize file path conventions (`_LONGFILES_` and naming rules) in one utility.
5. Separate retention policy logic from scheduling trigger plumbing for simpler testing.
