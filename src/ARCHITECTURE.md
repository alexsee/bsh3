# Architecture

## Bird's-eye view
Backup Service Home is a Windows backup system with two UI shells (legacy WinForms and newer WinUI), one shared backup engine, and a small helper Windows service for VSS file access. The product stores metadata in SQLite, writes backup payloads to a target medium (filesystem or FTP), and executes backup/restore/delete jobs with progress callbacks.

This document is intentionally stable and high-level. Use symbol search for the names below (types and files) instead of relying on links.

## Codemap

### Runtime shells
- `BSH.MainApp` (WinUI 3): current UI shell and composition root.
  - Entry/composition: `App` in `App.xaml.cs`.
  - App orchestration: `OrchestrationService`, `ScheduledBackupService`, `JobService`, `StatusService`, `PresentationService`.
  - UI surface: `Views/*`, `ViewModels/*`, `Windows/*`.
- `BSH.Main` (WinForms): legacy shell with equivalent orchestration responsibilities.
  - Entry point: `Program`.
  - Legacy orchestration: `BackupLogic`, `BackupController`, `StatusController`, `PresentationController`, `NotificationController`.

### Core engine (shared business logic)
- `BSH.Engine`: central domain/infrastructure library used by both shells.
  - Facade-style contracts: `IBackupService`, `IConfigurationManager`, `IQueryManager`, `IDbClientFactory`, `IStorageFactory`.
  - Main implementations: `BackupService`, `ConfigurationManager`, `QueryManager`, `DbClientFactory`, `StorageFactory`.
  - Job pipeline: base `Job` plus `BackupJob`, `RestoreJob`, `DeleteJob`, `EditJob`, `DeleteSingleJob`.
  - Reporting boundary: `IJobReport` and `JobState`/`ActionType` updates from engine to UI.
  - Persistence: `Database/*` (SQLite setup and migrations in `DbClientFactory` and `DbMigrationService`).
  - Storage adapters: `Storage/*` (`IStorage`, `FileSystemStorage`, `FtpStorage`).
  - Scheduling and device helpers: `SchedulerService`, `UsbWatchService`, `VolumeShadowCopyService`.
  - File discovery/exclusion: `Services/FileCollector/*`.
  - Security utilities: `Security/Encryption`, `Security/Hash`, `Security/Crypto`.

### Out-of-process VSS helper
- `BSH.Service`: Windows service that hosts named-pipe RPC for VSS copy operations.
  - Host/service wiring: `Program`, `WindowsBackgroundService`.
  - RPC implementation: `VSSService`, `VSS/*`.
- `BSH.Service.Shared`: RPC contract assembly.
  - Shared interface: `IVSSRemoteObject`.

### UI controls and preview subsystem
- `BSH.Controls`: reusable WinForms controls and preview formatting utilities (`UI/*`, `Preview/*`).
- `SmartPreview`: standalone preview app (`Program`, `frmSmartPreview`).
- `PreviewHandlerFramework` + `PreviewHandlerHost`: COM preview-handler framework/host abstractions used by preview features.

### Tests
- `BSH.Test`: NUnit tests centered on engine behavior.
  - Coverage anchors: `BackupTests`, `ConfigurationManagerTests`, `QueryManagerTests`, `Services/FileCollector/*`, `Security/*`.

## Architectural invariants and boundaries
- `BSH.Engine` is the business core; UI projects are orchestration/presentation layers around it.
- Backup operations are modeled as jobs (`BackupJob`, `RestoreJob`, `DeleteJob`, etc.) and are launched through `IBackupService`.
- Storage medium differences are isolated behind `IStorage` + `StorageFactory`; call sites should not branch on filesystem vs FTP behavior.
- VSS access is a process boundary: engine code calls `VolumeShadowCopyService`, which talks to `BSH.Service` through `IVSSRemoteObject` over named pipes.
- Schema creation/migration ownership lives in `BSH.Engine.Database` (`DbClientFactory`, `DbMigrationService`); keep schema evolution there.
- Progress/UI feedback crosses a boundary through `IJobReport`; engine jobs should stay UI-framework-agnostic.
- Preview-handler/preview UI code is separate from backup execution logic (preview projects do not own backup workflows).

## Cross-cutting concerns
- Persistence: SQLite (`backupservicehome.bshdb`) is the system of record for configuration, versions, file metadata, schedules, and migration version.
- Observability: Serilog is used across projects for runtime diagnostics.
- Scheduling: Quartz-backed scheduling is centralized in engine-level `SchedulerService` and consumed by orchestration services.
- Cancellation and long-running work: jobs run asynchronously and use `CancellationToken`; both shells expose cancellation through their orchestration services.
- Localization/resources: UI text and assets are resource-based (`*.resx`, `*.resw`) with German/English variants.
