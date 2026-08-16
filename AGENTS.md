# AGENTS.md

Backup Service Home 3 is a **Windows-only** .NET 10 desktop backup application (C#).
It has two UI shells (WinUI 3 `BSH.MainApp`, legacy WinForms `BSH.Main`), a shared
`BSH.Engine`, a VSS Windows service (`BSH.Service`), preview/control libraries, and an
NUnit test project (`BSH.Test`). See `src/ARCHITECTURE.md` for the full codemap. The
official build/test/release pipeline runs exclusively on `windows-latest`
(`.github/workflows/dotnet-desktop-build.yml`).

## Cursor Cloud specific instructions

The Cursor Cloud VM is **Linux**, but every project targets a Windows TFM
(`net10.0-windows` / `net10.0-windows10.0.22621.0`). Full development (running the GUI
shells, running the test suite, VSS, MSIX packaging) requires Windows. This environment
supports building most of the solution and running the core engine's data/security
layers, which is enough for engine-level development.

### Toolchain
- .NET SDK `10.0.302` (pinned in `src/global.json`) is installed at `~/.dotnet` and added
  to `PATH`/`DOTNET_ROOT` via `~/.bashrc`. The startup update script runs `dotnet restore`.
- Run all `dotnet` commands from the `src/` directory (that is where the `.sln` lives).

### Building on Linux (key gotchas)
- Windows-targeted projects need `-p:EnableWindowsTargeting=true` on every
  `restore`/`build`, otherwise you get `NETSDK1100`.
- Build the solution with `-p:Platform=x64`. Do **not** use `-a x64` on the solution: it
  sets a RID and fails with `NETSDK1134` ("Building a solution with a specific
  RuntimeIdentifier is not supported"). `-a x64` is fine on a single project.
- These build successfully on Linux: `BSH.Engine`, `BSH.Service`, `BSH.Service.Shared`,
  `BSH.Controls`, `BSH.Main`, `SmartPreview`, `PreviewHandlerFramework`,
  `PreviewHandlerHost`.
- `BSH.MainApp` (WinUI 3) does **not** build on Linux — its XAML compiler
  (`Microsoft.UI.Xaml.Markup.Compiler`) is Windows-only. Because `BSH.Test` references
  `BSH.MainApp`, the test project cannot build on Linux either.
- A full solution/project **build** rewrites `packages.lock.json` files to add `linux-x64`
  RID entries. A plain `dotnet restore` does not. Revert any `packages.lock.json` changes
  before committing (`git checkout -- src/**/packages.lock.json`); they are Linux build
  artifacts, not intended changes. CI restores with `--locked-mode -a x64` on Windows.

### Tests on Linux
- The NUnit suite cannot run here: (1) `BSH.Test` references the WinUI app (won't build),
  and (2) `net*-windows` assemblies are stamped with a Windows `TargetPlatform`, which
  NUnit honors by **skipping every test** on non-Windows ("Only supported on Windows7.0").
- Run `dotnet test -p:Platform=x64` on Windows for the real suite.

### What runs on Linux vs. what needs Windows
- Runs on Linux: `BSH.Engine` core data/security layers — SQLite DB creation +
  migrations (`DbClientFactory`), `ConfigurationManager`, `QueryManager`,
  `Security.Hash` (MD5), `Security.Encryption` (AES). `System.Data.SQLite.Core` ships a
  `linux-x64` native interop, so the SQLite layer works.
- Needs Windows: a full `BackupJob.BackupAsync` (calls `kernel32!SetThreadExecutionState`
  via `Win32Stuff.KeepSystemAwake`), VSS (`AlphaVSS` + named-pipe RPC to `BSH.Service`),
  FTP password crypto (DPAPI `ProtectedData` in `Security/Network.cs`), USB watch (WMI),
  and all WinForms/WPF/WinUI runtime.

### Lint / format
- Roslyn analyzers (`Microsoft.CodeAnalysis.NetAnalyzers`) run during `dotnet build`; the
  authoritative static analysis in CI is SonarCloud. `dotnet format` is available but the
  repo is not clean against its whitespace/style rules, so `dotnet format
  --verify-no-changes` is not the project's lint gate — rely on the build analyzers.
