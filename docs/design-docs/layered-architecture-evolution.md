# Layered Architecture Evolution Plan

## Intent
Define a clean, explicitly layered architecture for Backup Service Home and a practical migration path from the current structure to that target.

The target follows your pattern: a business domain with strict layer boundaries and explicit cross-cutting entry points.

## Target layer model

```mermaid
flowchart TD
    U[Utils]

    subgraph D[Business logic domain]
      P[Providers]
      AW[App Wiring + UI]
      S[Service]
      R[Runtime]
      UI[UI]
      T[Types]
      C[Config]
      REPO[Repo]

      P --> AW
      P --> S
      S --> R
      R --> UI
      R --> AW
      T --> C
      C --> REPO
      REPO --> S
    end

    U --> P
```

## Layer responsibilities and mapping to current codebase

### Utils (cross-cutting primitives)
Responsibility:
- pure helpers and technical primitives (path helpers, date helpers, hashing/encryption primitives, OS wrappers)
- no business decisions

Current mapping:
- `BSH.Engine/Utils/*`
- parts of `BSH.Engine/Security/*`
- low-level OS wrappers like `Win32Stuff`

Target rule:
- consumable by Providers (and only by exception by other layers through ports)

### Types (domain model + contracts)
Responsibility:
- domain enums, records, value objects, status/result contracts
- no I/O, no framework dependencies

Current mapping:
- `BSH.Engine/Models/*`
- `BSH.Engine/ActionType.cs`, `BSH.Engine/TaskType.cs`, `BSH.Engine/Enums.cs`
- operation result types (`JobState`, overwrite enums)

Target rule:
- stable type system shared across all other business layers

### Config (policy + runtime settings abstraction)
Responsibility:
- typed access to configuration and policy
- no SQL/storage details

Current mapping:
- `IConfigurationManager`, `ConfigurationManager`
- shell settings wrappers in `BSH.MainApp.Services.LocalSettingsService`, WinForms `Settings`

Target rule:
- Config depends on Types only
- persistence for config values lives in Repo/Providers

### Repo (data access)
Responsibility:
- all persistence queries and writes
- query/read models and command/write repositories

Current mapping:
- `DbClient`, `DbClientFactory`, `DbMigrationService` (`Database/`)
- version, mutation, and schedule writes in `Repo/BackupMutationRepository` and `Repo/ScheduleRepository`
- jobs read versions through `Repo/VersionQueryRepository` and `QueryManager`
- `QueryManager` still contains SQL
- job classes do not contain SQL

Target rule:
- Repo owns SQL and transaction boundaries
- Services call repos through interfaces only

### Service (use-case orchestration)
Responsibility:
- use cases: backup, restore, delete, modify, schedule planning
- state transitions and business invariants

Current mapping:
- `BackupService`
- substantial logic in `BackupJob`, `RestoreJob`, `DeleteJob`, `DeleteSingleJob`, `EditJob`

Target rule:
- Service depends on Types, Config, Repo, and Provider ports
- no direct UI framework code

### Runtime (execution engine)
Responsibility:
- job execution runtime: cancellation, progress pipeline, retries, conflict policy application
- deterministic orchestration host used by both shells and scheduled triggers

Current mapping:
- `JobRuntime` and `JobSessionRunner` (`Runtime/`) run preflight and the session for both shells
- `BSH.MainApp/Services/JobService.cs` and `BSH.Main/Modules/BackupController.cs` call them
- `BackupService` constructs the concrete jobs and starts them with `Task.Run`
- `StatusService` / `StatusController` receive `IJobReport` updates

Target rule:
- Runtime is the single engine for executing Service commands
- Runtime emits typed events (progress/state/errors) to UI adapters

### UI
Responsibility:
- render state, collect user intent, show dialogs

Current mapping:
- WinUI: `BSH.MainApp/Views`, `ViewModels`, `Windows`
- WinForms: `BSH.Main/Dialogs`, controllers around dialogs

Target rule:
- UI depends on Runtime event contracts, not on Repo or Providers

### App Wiring + UI (composition root)
Responsibility:
- DI wiring, startup lifecycle, binding concrete adapters to ports

Current mapping:
- `BSH.MainApp/App.xaml.cs`
- `BSH.Main/Program.cs` + static wiring in `BackupLogic`

Target rule:
- only composition root may bind interfaces to concrete implementations

### Providers (external system adapters)
Responsibility:
- adapters to external systems: storage, scheduler, VSS service, OS notifications, media watching

Current mapping:
- `IStorageProvider`, implemented by `FileSystemStorage` and `FtpStorage` (`Storage/*`)
- `ISchedulerAdapter` (`SchedulerService`), `IVssClient` (`VolumeShadowCopyClient`), `IMediaWatcher` (`UsbWatchService`)
- ports in `Providers/Ports/`
- `BSH.Service` / `BSH.Service.Shared` for VSS IPC

Target rule:
- Providers implement ports consumed by Service/Runtime
- Providers contain all framework- or protocol-specific code

## Dependency rules (hard boundaries)

1. UI must not reference Repo implementations or SQL types.
2. Service/Runtime must not reference WinUI/WinForms types.
3. Repo must not call UI callbacks directly.
4. Providers must not contain business decisions (only adapter behavior).
5. Types must remain dependency-free (except BCL primitives).
6. Composition root is the only place where concrete classes are wired together.

## Proposed target project layout

- `BSH.Domain.Types`
- `BSH.Domain.Config`
- `BSH.Domain.Repo`
- `BSH.Domain.Service`
- `BSH.Domain.Runtime`
- `BSH.Providers` (with sub-namespaces: Storage, Scheduler, Vss, Os)
- `BSH.App.WinUI` (current `BSH.MainApp`)
- `BSH.App.WinForms` (current `BSH.Main`, optional legacy)

Transitional note:
- Start with namespace/folder boundaries inside `BSH.Engine` before physical project extraction.

## Migration plan (incremental)

### Phase 1: Boundary declaration (no behavior change)
Already in `BSH.Engine`:
- Folders `Repo/`, `Runtime/`, and `Providers/Ports/`.
- Provider ports `IStorageProvider`, `IVssClient`, `ISchedulerAdapter`, and `IMediaWatcher`.
- Job UI calls go through `IJobReport` and `IJobSessionPresenter`. `BSH.Engine` has no WinUI or WinForms framework reference.

The project split under "Proposed target project layout" remains future work.

### Phase 2: Runtime unification
Already in the tree:
- `JobRuntime` and `JobSessionRunner` are called by `JobService` and `BackupController`.
- Preflight (media, password, cancellation gating) runs there for manual and scheduled backups.
- `BackupJob`, `RestoreJob`, and `JobSessionRunner` report `JobState.CANCELED`.

### Phase 3: Repo extraction
Already in the tree:
- `Repo/VersionQueryRepository`, `Repo/BackupMutationRepository`, and `Repo/ScheduleRepository`.
- Job classes contain no SQL.
- `QueryManager` still contains SQL.

### Phase 4: Provider isolation
Already in the tree:
- `FileSystemStorage` and `FtpStorage` implement `IStorageProvider`.
- `SchedulerService` implements `ISchedulerAdapter`.
- `VolumeShadowCopyClient` implements `IVssClient`.
- `UsbWatchService` implements `IMediaWatcher`.
- `BSH.Service` remains the VSS process boundary.

`BackupService` also depends on `IQueryManager`, `IDbClientFactory`, and the repo interfaces.

### Phase 5: App wiring cleanup
- Restrict DI registration and concrete instantiation to composition roots.
- Remove static/global orchestration from legacy pathways where possible.

Exit criteria:
- No `new` of infrastructure classes in domain/service code.

## Cross-cutting boundaries (explicit)

### Observability
- Logging, metrics, trace correlation via a shared telemetry port.
- No direct `Serilog` calls in Types; Service/Runtime logs through abstraction.

### Security
- encryption/hash/password handling via dedicated security service ports.
- password persistence remains app-side adapter detail.

### Time and scheduling
- use a clock abstraction for deterministic behavior in runtime logic.
- scheduler adapter translates schedule entries to triggers.

### Error model
- convert technical exceptions to typed domain failures at layer boundaries.
- UI receives user-facing error categories, not raw provider exceptions.

## Architectural invariants to enforce in CI

1. No reference from UI projects to Repo/Provider concrete assemblies.
2. No reference from Domain Types to any app/provider assembly.
3. No direct SQL outside Repo layer.
4. No WinUI/WinForms namespaces in Domain Runtime/Service.
5. One active job per runtime instance unless explicitly configured for concurrency.

Implementation suggestion:
- add architecture tests (for example with NetArchTest or custom Roslyn analyzers) to enforce namespace/project dependency rules.

## Refactors already in this repository

1. Shared preflight runs in `JobRuntime` and `JobSessionRunner`, called from:
- `BSH.MainApp.Services.JobService` (`BSH.MainApp/Services/JobService.cs`)
- `BSH.Main.Modules.BackupController` (`BSH.Main/Modules/BackupController.cs`)

2. Version, mutation, and schedule SQL lives in `src/BSH.Engine/Repo/`:
- `VersionQueryRepository`
- `BackupMutationRepository`
- `ScheduleRepository`

Job classes do not contain SQL. `QueryManager` still does.

3. Provider ports in `BSH.Engine/Providers/Ports/`:
- `IStorageProvider` (`FileSystemStorage`, `FtpStorage`)
- `ISchedulerAdapter` (`SchedulerService`)
- `IVssClient` (`VolumeShadowCopyClient`)
- `IMediaWatcher` (`UsbWatchService`)

4. `JobState.CANCELED` is reported by `BackupJob`, `RestoreJob`, and `JobSessionRunner`. `JobState.NOT_STARTED` is still the zero value used as idle before the first `ReportState`.

## Outcome
After these phases, the codebase has:
- explicit and enforceable layer boundaries
- one reusable runtime for all execution paths
- isolated infrastructure adapters
- UI shells that are replaceable without touching business logic
- lower duplication and easier testing of job behavior
