# Beta Release Testing Plan — Backup Service Home 3 (WinUI)

## Purpose

This plan defines how we get the WinUI app (`BSH.MainApp`) to a **robust beta** without compromising quality of a backup system customers rely on every day. It covers:

1. Current automated coverage and remaining gaps
2. Unit/integration work already landed vs still open
3. A minimal, practical E2E layer (today: none beyond headless engine integration)
4. Manual / scenario QA for beta
5. A safe launch strategy (gates, rollout, rollback)

Companion design context: `docs/design-docs/job-system.md`, `src/ARCHITECTURE.md`.

---

## Progress (as of 2026-08-16)

### Automated engine coverage

Landed on `main` via [#569](https://github.com/alexsee/bsh3/pull/569) and [#590](https://github.com/alexsee/bsh3/pull/590):

| Item | PR | Location |
|------|----|----------|
| `RestoreJob` unit suite (`StorageMock`) | #569 | `src/BSH.Test/RestoreTests.cs` |
| Extended restore/delete `StorageMock` | #569, #590 | `src/BSH.Test/Mocks/StorageMock.cs` |
| Real-FS backup↔restore integration | #569 | `src/BSH.Test/Integration/FileSystemRestoreIntegrationTests.cs` |
| Real-FS lifecycle (delete+restore, cancel, unicode, empty folders, decrypt) | #590 | `src/BSH.Test/Integration/FileSystemEngineLifecycleTests.cs` |
| Delete/edit/disk-space abort unit coverage | #590 | `DeleteTests`, `EditTests`, `BackupTests` |
| Release workflow test gate | #569 | `.github/workflows/dotnet-desktop-release.yml` |

P1 hardening from the original plan is **mostly done**. Remaining automated gap: **DB migration smoke tests**.

### WinUI product (feature parity)

WinUI 3 feature-parity PRD [#521](https://github.com/alexsee/bsh3/issues/521) is closed. Notable landings since the last plan update:

| Item | PR |
|------|----|
| Browser file preview | [#570](https://github.com/alexsee/bsh3/pull/570) |
| Persist browser view preferences | [#578](https://github.com/alexsee/bsh3/pull/578) |
| DE/EN localization + resource-parity tests | [#579](https://github.com/alexsee/bsh3/pull/579) |
| Switch storage (empty-medium) | [#580](https://github.com/alexsee/bsh3/pull/580) |
| Restore to a chosen destination; folder restore fix | [#581](https://github.com/alexsee/bsh3/pull/581), [#586](https://github.com/alexsee/bsh3/pull/586) |
| Notification activation | [#583](https://github.com/alexsee/bsh3/pull/583) |
| Unhandled-exception UI | [#584](https://github.com/alexsee/bsh3/pull/584) |
| UNC setup/switch credential probes | [#588](https://github.com/alexsee/bsh3/pull/588) |
| Delete a file from selected backup versions | [#589](https://github.com/alexsee/bsh3/pull/589) |
| Incremental disk-space preflight false-abort fix | [#582](https://github.com/alexsee/bsh3/pull/582) |
| Theme-aware WinUI colors | [#598](https://github.com/alexsee/bsh3/pull/598) |

**Still open for beta readiness:**

- Manual golden-path + risk-scenario QA (primary remaining work)
- Packaging/shell decision (installer still launches WinForms)
- WinUI file logging (exception handler exists; Serilog file sink is not configured, so “Show Event Logs” has nothing to open)
- DB migration smoke tests
- FTP / Quartz scheduler / real VSS coverage
- Optional WinUI UI E2E

---

## Product context (release risk)

| Fact | Implication for beta |
|------|----------------------|
| Shared engine in `BSH.Engine` powers both shells | Engine regressions hit all users; prioritize engine tests |
| WinUI (`BSH.MainApp`) is the current UI direction; feature parity vs WinForms is closed (#521) | Beta QA must still exercise WinUI golden paths **manually** (no UI E2E) |
| Installer (`tools/setup/Setup.iss`) still launches **WinForms** `BSH.Main.exe` | Clarify which shell ships in beta; packaging mismatch is the highest remaining product risk |
| Storage: local FS + FTP; metadata in SQLite; VSS via `BSH.Service` | Real I/O beyond local FS (FTP), media, and VSS still need scenario coverage |
| Auto-update supports stable/beta feeds (`v4` URLs in WinUI) | Beta channel must be isolatable and roll-backable |

**Decision needed before beta tagging:** Does beta ship WinUI as the installed entry point, WinForms with WinUI as optional/preview, or both? Test matrices below assume **WinUI is the beta UX under test**; adjust packaging gates accordingly.

---

## Current automated coverage (baseline)

**Project:** `src/BSH.Test` (NUnit only)  
**CI:** `.github/workflows/dotnet-desktop-build.yml` runs `dotnet test` on PRs/main (Windows, x64, coverage + SonarCloud).  
**Release workflow:** `.github/workflows/dotnet-desktop-release.yml` **builds and runs tests** before publish / Inno Setup (#569).

Strength by area:

| Area | Coverage | Notes |
|------|----------|-------|
| `BackupJob` (full/incremental, cancel, compress, encrypt, long path, VSS retry mock, disk-space abort) | Strong | Mostly `StorageMock`; abort asserted in `BackupTests` (#590) |
| **`RestoreJob`** (routing, overwrite, cancel, medium fail, incremental links, long-path, empty folders) | **Strong (unit)** | `RestoreTests` (#569, #590) |
| **`FileSystemStorage` backup↔restore** | **Strong (integration)** | Plain / compressed / encrypted / incremental (#569) |
| **`FileSystemStorage` lifecycle** | **Strong (integration)** | Delete+restore, cancel, unicode, empty folders, decrypt (#590) |
| File collector exclusions | Strong | Paths, types, size, masks, system folders |
| `QueryManager` | Strong | Versions, search, restore path resolution |
| Config / schedule policy / schedule settings | Moderate | Policy math + persistence |
| Delete / edit jobs | **Strong** | Shared packages, scoped `DeleteSingle`, decrypt paths (#589, #590) |
| Disk space preflight | **Strong** | Helpers + session-level abort (#582, #590) |
| `JobSessionRunner` / WinUI orchestration | Moderate–strong | Preflight, battery pause, notifications, restore-to, switch storage |
| Browser VM / preview / update service | Moderate | Favorites, restore-to, scoped delete, preview, feed preference |
| WinUI DE/EN resources | Strong | `WinUiResourceParityTests` (#579) |
| **`FTPStorage`** | **Gap** | Real I/O untested |
| DB migrations | **Gap** | `DbMigrationService` still has no fixture tests |
| Quartz scheduler end-to-end | Gap | |
| Real VSS / `BSH.Service` | Gap | Mock only |
| WinUI UI / XAML / installer | **None** | No UI E2E; VM/service tests only |

---

## Testing strategy (pyramid for beta)

```
                 ┌─────────────────┐
                 │ Manual / beta   │  Scenario QA + dogfood   ← next focus
                 │   field tests   │
                 ├─────────────────┤
                 │ Smoke E2E       │  Optional WinUI UI (none yet)
                 ├─────────────────┤
                 │ Integration     │  Real FS restore + lifecycle  ✅
                 ├─────────────────┤
                 │ Unit (engine)   │  Backup / restore / delete / edit  ✅
                 └─────────────────┘
```

For a backup product, **integration tests that prove backup → restore bit-identity** remain more valuable than broad UI automation. E2E should stay small and stable.

---

## Phase 0 — Make beta “testable” (prerequisites)

Before inviting customers:

1. **Freeze a beta build pipeline**
   - Tag pattern already exists: `v*.*.*-beta*`
   - ~~Add a test gate to the release workflow~~ ✅ Landed in #569 (`dotnet test` before Inno Setup).
2. **Pin shell + version** *(open — highest remaining product risk)*
   - Document which EXE the beta installer starts.
   - `tools/setup/Setup.iss` still starts `BSH.Main.exe` (WinForms) and writes that path to the startup Run key.
   - Ensure beta update feed points only at beta tags.
3. **Crash / diagnostic baseline** *(partial)*
   - ~~Unhandled exceptions logged and shown in WinUI~~ ✅ #584 (`UnhandledExceptionHandler`).
   - **WinUI file logging still missing:** `Log.Error` is called, but `BSH.MainApp` never configures a Serilog file sink. “Show Event Logs” opens `%AppData%\Alexosoft\Backup Service Home 3\log{yyyyMMdd}.txt`, which WinForms writes and WinUI currently does not.
   - Known-good DB path: `%AppData%\Alexosoft\Backup Service Home 3\`. Support procedure (what to collect besides passwords) still needs a short note.
4. **Test data kit** *(open)*
   - Small fixture tree: nested folders, empty folder, Unicode name, long path (>260), locked file (open in Notepad), large file (~500MB optional), junction/symlink if supported.
   - Automated suite already covers nested/Unicode/empty-folder round-trips on local FS; the kit is for **manual** WinUI/VSS/USB runs.
5. **Exit criteria checklist** (see [Beta exit criteria](#beta-exit-criteria)).

---

## Phase 1 — Easy automated wins (unit + integration)

Prioritized by **customer risk × implementation ease**.

### P0 — Must add before broad beta — ✅ done (#569)

| Test | Type | Status | Location |
|------|------|--------|----------|
| Restore routing / fileType matrix | Unit (`StorageMock`) | ✅ | `RestoreTests` |
| Restore after incremental (linked packages) | Unit | ✅ | `RestoreTests` |
| Restore compressed / encrypted (API routing) | Unit | ✅ | `RestoreTests` |
| Overwrite policies (`Ask` / `Overwrite` / `DontCopy`) | Unit | ✅ | `RestoreTests` |
| Restore when medium unavailable | Unit | ✅ | `RestoreTests` |
| Restore cancel mid-run | Unit | ✅ | `RestoreTests` |
| Backup → restore content equality (plain / compress / encrypt / incremental) | Integration (`FileSystemStorage`) | ✅ | `Integration/FileSystemRestoreIntegrationTests` |
| Release CI runs tests | Process | ✅ | `dotnet-desktop-release.yml` |

### P1 — Strongly recommended for beta hardening

| Test | Type | Status | Location / notes |
|------|------|--------|------------------|
| **Delete version then restore remaining** | Unit + integration | ✅ #590 | `DeleteTests`; `FileSystemEngineLifecycleTests` |
| **Long-path restore** | Unit | ✅ #590 | `Restore_LongFileNameUsesLongFilesDirectory` |
| **Disk space preflight → abort** | Unit | ✅ #582, #590 | `Backup_AbortsWhenFreeSpaceClearlyInsufficient` |
| Deeper `FileSystemStorage` lifecycle (delete/rename/edit on real FS) | Integration | ✅ #590 | `FileSystemEngineLifecycleTests` (not exhaustive `\\?\` API coverage) |
| **DB migration smoke** | Unit/integration | **Open** | Open fixture DBs at prior schema versions; assert `DbMigrationService` reaches current (v9) |
| Long-path folder convention consistency | Code debt | **Open** | `QueryManager.BuildRemoteFilePath` still uses `_LONG_FILES` while jobs use `_LONGFILES_` |

### P2 — Nice to have (post-beta or parallel)

| Test | Type | Status / notes |
|------|------|----------------|
| FTP storage with local FTP stub / Testcontainers-like fixture | Integration | Still open; harder in CI; optional nightly |
| Scheduler trigger fires and invokes backup (Quartz) | Integration | Still open; time-travel or short interval + wait |
| `UsbWatchService` media-appear path | Integration | Device simulation is OS-heavy |
| WinForms ↔ WinUI orchestration parity expansion | Unit | Improved (`JobSessionRunnerTests`, notification/switch-storage tests); keep extending as needed |
| WinUI DE/EN resource parity | Unit | ✅ #579 `WinUiResourceParityTests` |
| NetArchTest layer rules | Unit | Still open; per `layered-architecture-evolution.md` |

### Explicitly out of scope for “easy” automation

- Full AlphaVSS / real locked Outlook PST in CI (manual + dogfood)
- Installer/service registration (manual + one smoke script on release VM)
- Visual XAML layout regression suites (high cost, low early ROI)

---

## Phase 2 — Basic E2E proposal

Headless engine golden paths for local FS backup↔restore **and** delete/cancel/unicode lifecycle are covered by the integration suites (#569, #590). Remaining E2E gap is **WinUI shell / installer** automation.

Browser/restore/delete **view-model** behavior is unit-tested (`BrowserViewModelTests`, `BrowserPreviewServiceTests`); that is not a substitute for clicking through the shell.

### Recommended stack

| Option | Pros | Cons | Status |
|--------|------|------|--------|
| **A. Engine “headless golden path” in `BSH.Test`** (no UI) | Fast, stable, CI-friendly | Doesn’t catch WinUI wiring bugs | ✅ Done for local FS (`Integration/`) |
| **B. WinAppDriver / Appium Windows** against packaged WinUI | True UI E2E | Flaky, App SDK setup cost | Optional next |
| **C. Scripted smoke via public service APIs** (if exposed) | Medium fidelity | Needs test hooks | Use if UI automation slips |

**Proposal for UI / shell smoke (5–8 cases) — still open:**

1. Fresh config → set source + local target → run backup → status FINISHED  
2. Incremental backup after file change → new version appears in browser  
3. Restore single file to alternate folder → content matches  
4. Restore full version with overwrite=Overwrite  
5. Delete oldest version → browser list updates; remaining restore still works  
6. Encryption enabled → backup → restore with password  
7. Medium disconnected → backup/restore surfaces error (no hang)  
8. Cancel running backup → no corrupt version left (DB + storage)

**Where to put them:**

- Engine paths: `BSH.Test/Integration/` with `[Category("Integration")]` ✅  
- UI paths (later): separate `BSH.E2E` + WinAppDriver if adopted  
- Filter in CI:
  ```powershell
  dotnet test ... --filter "Category!=Integration"   # optional fast unit-only
  dotnet test ... --filter "Category=Integration"    # integration
  ```
  Today PR/release CI runs the full suite (including Integration) on Windows.

**Stability rules:**

- No reliance on wall-clock schedules in default CI  
- Unique temp roots per test; delete in teardown  
- No network/FTP in default PR CI  
- Hang-blame already set to 2m in CI — keep tests well under that  

---

## Phase 3 — Manual / beta scenario QA

Automate what we can; **manually** validate what backups actually need in the wild. This is the **primary remaining work** before inviting customers. WinUI now has the golden-path *features*; they have not been signed off on a real beta installer.

### Golden path (every beta build)

1. Clean install (or side-by-side AppData reset)  
2. Setup wizard: sources, local target, compression on/off  
3. Manual full backup  
4. Change files → incremental  
5. Browse versions → search → favorite → preview  
6. Restore file + folder to original and alternate paths  
7. Delete a version; delete a file from selected versions  
8. Uninstall / upgrade from previous beta without DB loss  

### Risk scenarios (must pass before public beta)

| Scenario | Why critical |
|----------|----------------|
| Encrypted backup restore with wrong password | Security + support load |
| USB target ejected mid-backup | Data integrity / wait-for-media |
| Low disk space on target | Abort vs corrupt version |
| Locked file (Office doc open) + VSS service installed | Everyday Windows reality |
| Long paths / deep trees | Historical bug surface (`_LONGFILES_` vs `_LONG_FILES`) |
| Unicode / special characters in filenames | Locale (DE/EN) — automated FS round-trip exists; still dogfood in WinUI |
| Battery / laptop on battery with pause setting | Scheduled reliability |
| Scheduled overnight backup + retention cleanup | “Set and forget” promise |
| FTP target backup + restore (lab only) | Second backend |
| Upgrade from last stable / last beta AppData | Migration safety (no automated migration tests yet) |
| Theme: light and dark | #598 landed; verify dialogs/browser/status |

### Compatibility matrix (minimum)

- Windows 10 22H2 and Windows 11 (current) x64  
- Local NTFS external drive + folder-on-system-drive  
- German and English UI  
- Fresh install + upgrade install  

### Roles

| Role | Responsibility |
|------|----------------|
| Dev | Remaining P1 (DB migrations, WinUI file log); fix P0 bugs; packaging decision |
| QA / dogfooders | Phase 3 checklist on beta builds |
| Maintainer | Tag gating, feed config, rollback decision |

---

## Safe launch plan (quality gates)

Backup software fails loudly in customer trust. Launch beta as a **controlled channel**, not a silent default for stable users.

### Gate 1 — Engineering (block tag)

- [x] PR CI green (`dotnet-desktop-build`) — required ongoing  
- [x] **Release workflow runs the test suite and fails the release on test failure** (#569)  
- [x] P0 restore unit + FS integration tests merged and green (#569)  
- [x] P1 delete+restore, long-path restore, disk-space abort, FS lifecycle (#590)  
- [ ] No open P0 bugs on: data loss, restore failure, DB corruption, cancel leaving inconsistent state *(no such issues currently open on GitHub; still a human sign-off)*  
- [ ] Installer starts the intended shell (still WinForms today)

### Gate 2 — Internal dogfood (block public beta)

- [ ] ≥3 internal machines on beta for several days of real schedules  
- [ ] Golden path + risk scenarios signed off **on WinUI**  
- [ ] At least one full encrypted backup ↔ restore verified bit-for-bit on real FS *(automated happy path exists; still dogfood on real media/USB)*  
- [ ] Installer installs/starts intended shell; `BSH.Service` starts; VSS path smoke-tested  
- [ ] WinUI writes a supportable log file (or “Show Event Logs” is pointed at a real sink)

### Gate 3 — Closed beta (limited customers)

- [ ] Opt-in beta feed only (Autoupdater beta channel; WinUI already has separate v4 stable/beta URLs)  
- [ ] Clear “beta” labeling in UI / about / release notes  
- [ ] Support path: how to collect logs + DB (not passwords)  
- [ ] Rollback: previous stable installer still available; documented downgrade (AppData compatibility notes)  

### Gate 4 — Open beta / promote toward stable

- [ ] No unresolved data-loss issues from closed beta  
- [ ] Crash/error rate acceptable (define threshold after first week of telemetry/logs)  
- [ ] FTP and scheduling scenarios covered by at least lab automation or repeated manual runs  
- [ ] Packaging decision finalized (WinUI vs WinForms entry point)  

### Runtime safety nets (product)

These reduce blast radius even when tests miss something:

1. **Never auto-migrate all stable users to beta.**  
2. **Prefer abort over silent partial success** for medium/DB failures (already aligned with job design).  
3. **Keep DB upload to medium only after local commit** (existing invariant — regression-test it).  
4. **Retention/delete are destructive** — require confirmation in UI; automated tests now cover that delete doesn’t remove shared incremental payloads incorrectly (#590).  
5. **Beta builds should not be the only copy of customer data** — release notes must say beta is for non-primary or duplicated datasets until Gate 4.

### Rollback procedure (short)

1. Disable / unpublish beta feed item.  
2. Publish last known-good build on the appropriate channel.  
3. If DB schema migrated forward incompatibly: document “do not downgrade” or ship a forward-compatible stable; **never break restore of existing media**.  
4. Hotfix notes: symptoms, who are affected, workaround (restore from last good version on medium).

---

## Suggested implementation order (concrete backlog)

1. ~~**Add `RestoreTests.cs`**~~ ✅ #569 — unit suite via `StorageMock`.  
2. ~~**Add real-FS integration category**~~ ✅ #569 — `Integration/FileSystemRestoreIntegrationTests`.  
3. ~~**Wire `dotnet test` into release workflow**~~ ✅ #569.  
4. ~~**P1 engine hardening**~~ ✅ #590 — delete+restore, long-path restore, disk-space abort, lifecycle integration.  
5. ~~**WinUI feature parity**~~ ✅ #521 closed (preview, restore-to, switch storage, localization, crash UI, scoped delete, …).  
6. **Pin installer entry point** to the shell under test (or document a dual-shell beta). ← **next product blocker**  
7. **WinUI Serilog file sink** so unhandled exceptions and “Show Event Logs” produce a collectable log.  
8. **DB migration smoke tests** against prior schema fixtures.  
9. **Manual golden-path checklist** executed on first `v*-beta*` build that actually launches WinUI.  
10. **Optional:** WinAppDriver smoke for setup wizard + one backup button.

### Effort sketch (technical, not calendar)

| Work item | Touch surface | Status / risk |
|-----------|---------------|---------------|
| RestoreTests | `BSH.Test` + `StorageMock` | ✅ Done |
| FS restore integration | Temp dirs + `FileSystemStorage` | ✅ Done |
| FS lifecycle integration | `FileSystemEngineLifecycleTests` | ✅ Done |
| Release CI test step | Release workflow YAML | ✅ Done |
| WinUI feature parity | `BSH.MainApp` | ✅ Done vs #521 |
| Installer shell pin | `tools/setup/Setup.iss` | **Open — highest remaining product risk** |
| WinUI file logging | `BSH.MainApp` Serilog config | Open — supportability |
| DB migration smoke | `DbMigrationService` fixtures | Open — upgrade safety |
| Manual / dogfood QA | Installer + real media | Open — highest remaining *quality* risk |
| WinAppDriver E2E | New project + CI image deps | Optional; high flake/setup cost |

---

## Beta exit criteria

Ship / widen beta only when:

1. Automated: P0 restore suite + existing suite green on the release tag build. ✅ *(mechanism + tests landed, including P1 lifecycle; still required green on each tag)*  
2. Manual: golden path + encrypted restore + cancel integrity + media-missing signed off on WinUI.  
3. Ops: beta feed isolated; rollback path rehearsed once.  
4. Product: shell packaging matches what testers install.  
5. Trust: release notes state backup/restore verification steps for participants.
6. Support: WinUI writes a log file testers can attach (or the Event Log menu is removed/fixed).

---

## Appendix A — Mapping critical features → test layers

| Feature | Unit | Integration | E2E/Manual |
|---------|------|-------------|------------|
| Full/incremental backup | ✅ | ✅ local FS (#569) | Manual large trees |
| Restore | ✅ (#569, #590) | ✅ local FS (#569, #590) | Golden path / WinUI restore-to (#581) |
| Compression/encryption | ✅ routing + ✅ FS round-trip | ✅ (#569) | Manual password UX |
| Delete version / file | ✅ (#589, #590) | ✅ delete+restore (#590) | Browser UX |
| Edit/decrypt | ✅ (#590) | ✅ decrypt then restore (#590) | Manual |
| Exclusions | ✅ | Spot-check | Manual |
| Schedule + retention | Policy unit | Optional Quartz | Overnight dogfood |
| FTP | ❌ | Nightly/lab | Lab manual |
| VSS | Mock only | — | Manual locked files |
| Disk space / media wait | ✅ abort + helpers | — | Manual USB |
| Browser / search / preview | VM + preview unit | — | Manual |
| Updates (beta/stable) | Partial unit | — | Manual feed switch |
| Localization DE/EN | ✅ resource parity | — | Manual UI pass |
| Installer + service | — | — | Every beta build |

## Appendix B — Commands

```powershell
cd src
dotnet test "BSH.Test\BSH.Test.csproj" -c Release -p:Platform=x64

# Optional filters:
dotnet test "BSH.Test\BSH.Test.csproj" -c Release -p:Platform=x64 --filter "FullyQualifiedName~Restore"
dotnet test "BSH.Test\BSH.Test.csproj" -c Release -p:Platform=x64 --filter "Category!=Integration"
dotnet test "BSH.Test\BSH.Test.csproj" -c Release -p:Platform=x64 --filter "Category=Integration"
```

## Appendix C — Related paths

| Path | Role |
|------|------|
| `src/BSH.Test/` | NUnit suite |
| `src/BSH.Test/RestoreTests.cs` | RestoreJob unit tests (`StorageMock`) |
| `src/BSH.Test/DeleteTests.cs` | DeleteJob / DeleteSingleJob unit tests |
| `src/BSH.Test/Integration/FileSystemRestoreIntegrationTests.cs` | Real-FS backup↔restore |
| `src/BSH.Test/Integration/FileSystemEngineLifecycleTests.cs` | Real-FS delete+restore / cancel / unicode |
| `src/BSH.Test/Mocks/StorageMock.cs` | Shared backup/restore/delete storage mock |
| `src/BSH.Engine/Jobs/RestoreJob.cs` | Restore implementation |
| `src/BSH.Engine/Database/DbMigrationService.cs` | Schema upgrades (untested as fixtures) |
| `src/BSH.Engine/Storage/` | FS/FTP adapters |
| `src/BSH.MainApp/` | WinUI shell under beta |
| `src/BSH.MainApp/Services/UnhandledExceptionHandler.cs` | Crash UI (no file sink yet) |
| `.github/workflows/dotnet-desktop-build.yml` | PR test gate |
| `.github/workflows/dotnet-desktop-release.yml` | Tag → test → installer |
| `tools/setup/Setup.iss` | What customers actually install (still WinForms) |
