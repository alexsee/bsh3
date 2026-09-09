# Beta Release Testing Plan — Backup Service Home 3 (WinUI)

## Purpose

This plan defines how we get the WinUI app (`BSH.MainApp`) to a **robust beta** without compromising quality of a backup system customers rely on every day. It covers:

1. Current automated coverage and remaining gaps
2. Unit/integration work already landed vs still open
3. A minimal, practical E2E layer (today: none beyond headless engine integration)
4. Manual / scenario QA for beta
5. A safe launch strategy (gates, rollout, rollback)

Execution documents: `docs/testing/winui-golden-path.md`,
`docs/testing/beta-risk-scenarios.md`, `docs/testing/beta-release-signoff.md`, and
`docs/testing/beta-operations-runbook.md`. Companion design context:
`docs/design-docs/job-system.md`, `src/ARCHITECTURE.md`.

---

## Progress (as of 2026-09-04, after #615–#629)

### Automated engine coverage

Landed on `main` via [#569](https://github.com/alexsee/bsh3/pull/569), [#590](https://github.com/alexsee/bsh3/pull/590), and [#605](https://github.com/alexsee/bsh3/pull/605):

| Item | PR | Location |
|------|----|----------|
| `RestoreJob` unit suite (`StorageMock`) | #569 | `src/BSH.Test/RestoreTests.cs` |
| Extended restore/delete `StorageMock` | #569, #590 | `src/BSH.Test/Mocks/StorageMock.cs` |
| Real-FS backup↔restore integration | #569 | `src/BSH.Test/Integration/FileSystemRestoreIntegrationTests.cs` |
| Real-FS lifecycle (delete+restore, cancel, unicode, empty folders, decrypt) | #590 | `src/BSH.Test/Integration/FileSystemEngineLifecycleTests.cs` |
| Delete/edit/disk-space abort unit coverage | #590 | `DeleteTests`, `EditTests`, `BackupTests` |
| DB schema upgrade smoke (v1/v8 → current v9; newer DB fails closed) | #605 | `src/BSH.Test/DatabaseSchemaUpgradeTests.cs` |
| Release workflow test gate | #569 | `.github/workflows/dotnet-desktop-release.yml` |

P1 hardening from the original plan is **done**. Remaining automated gaps are P2 (FTP, Quartz, real VSS).

### WinUI product (feature parity + beta packaging)

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
| Preview long-named files from the same `_LONGFILES_` package as restore | [#603](https://github.com/alexsee/bsh3/pull/603) |
| WinUI overview/onboarding refinements | [#604](https://github.com/alexsee/bsh3/pull/604) |
| Collectable WinUI event log (Serilog file sink) | [#606](https://github.com/alexsee/bsh3/pull/606) |
| Beta installer launches WinUI | [#607](https://github.com/alexsee/bsh3/pull/607) |
| Import-path, scheduler, browser, settings, and dialog hardening | [#615](https://github.com/alexsee/bsh3/pull/615)–[#621](https://github.com/alexsee/bsh3/pull/621) |
| Preserve security settings after incomplete deletion | [#627](https://github.com/alexsee/bsh3/pull/627) |
| Protect persisted FTP passwords | [#628](https://github.com/alexsee/bsh3/pull/628) |
| Start one due backup on USB arrival | [#618](https://github.com/alexsee/bsh3/pull/618), [#629](https://github.com/alexsee/bsh3/pull/629) |

**Still open for beta readiness:**

- Sign-off of the golden path and risk scenarios on an immutable installed candidate — primary remaining work
- Live FTP / Quartz trigger / real VSS evidence
- Publishing and verifying the v4 update feeds (both returned HTTP 404 on 2026-09-04)
- Rollback rehearsal and candidate-specific support/release notes
- Optional WinUI UI E2E

---

## Product context (release risk)

| Fact | Implication for beta |
|------|----------------------|
| Shared engine in `BSH.Engine` powers both shells | Engine regressions hit all users; prioritize engine tests |
| WinUI (`BSH.MainApp`) is the current UI direction; feature parity vs WinForms is closed (#521) | Beta QA must still exercise WinUI golden paths **manually** (no UI E2E) |
| Dual installers: `Setup-WinUI.iss` launches `BSH.MainApp.exe`; `Setup.iss` still launches WinForms `BSH.Main.exe` | **Beta testers use the WinUI artifact** (`backupservicehome-*-winui-win64.exe`). WinForms remains a separate release artifact. |
| Storage: local FS + FTP; metadata in SQLite; VSS via `BSH.Service` | Real I/O beyond local FS (FTP), media, and VSS still need scenario coverage |
| Auto-update supports stable/beta feeds (`v4` URLs in WinUI) | Beta channel must be isolatable and roll-backable |

**Packaging decision (landed in #607):** beta ships WinUI as the installed entry point via `Setup-WinUI.iss`. Release CI publishes both the WinForms and WinUI installers. Test matrices below assume **WinUI is the beta UX under test**.

---

## Current automated coverage (baseline)

**Project:** `src/BSH.Test` (NUnit only)  
**CI:** `.github/workflows/dotnet-desktop-build.yml` runs `dotnet test` on PRs/main (Windows, x64, coverage + SonarCloud).  
**Release workflow:** `.github/workflows/dotnet-desktop-release.yml` **builds and runs tests** before publish / Inno Setup (#569), then confirms both installer entry points and builds `Setup.iss` + `Setup-WinUI.iss` (#607).

Strength by area:

| Area | Coverage | Notes |
|------|----------|-------|
| `BackupJob` (full/incremental, cancel, compress, encrypt, long path, VSS retry mock, disk-space abort) | Strong | Mostly `StorageMock`; abort asserted in `BackupTests` (#590) |
| **`RestoreJob`** (routing, overwrite, cancel, medium fail, incremental links, long-path, empty folders) | **Strong (unit)** | `RestoreTests` (#569, #590) |
| **`FileSystemStorage` backup↔restore** | **Strong (integration)** | Plain / compressed / encrypted / incremental (#569) |
| **`FileSystemStorage` lifecycle** | **Strong (integration)** | Delete+restore, cancel, unicode, empty folders, decrypt (#590) |
| File collector exclusions | Strong | Paths, types, size, masks, system folders |
| `QueryManager` | Strong | Versions, search, restore path resolution; long-path preview uses `_LONGFILES_` (#603) |
| Config / schedule policy / schedule settings | Moderate–strong | Policy math, persistence, orchestration refresh, and USB-arrival behavior; no real Quartz trigger |
| Delete / edit jobs | **Strong** | Shared packages, scoped `DeleteSingle`, decrypt paths (#589, #590) |
| Disk space preflight | **Strong** | Helpers + session-level abort (#582, #590) |
| `JobSessionRunner` / WinUI orchestration | Moderate–strong | Preflight, battery pause, notifications, restore-to, switch storage |
| Browser VM / preview / update service | Moderate | Favorites, restore-to, scoped delete, preview, feed preference |
| WinUI DE/EN resources | Strong | `WinUiResourceParityTests` (#579) |
| DB migrations | **Strong (smoke)** | Prior schema v1/v8 → current v9; newer DB fails closed (#605) |
| WinUI event log | **Strong (unit)** | Dated AppData `log.txt` sink (#606) |
| Installer entry points | **Strong (contract)** | WinForms vs WinUI ISS + publish layout (#607) |
| **`FTPStorage`** | **Gap** | Credential persistence/protection is covered; real I/O is untested |
| Quartz scheduler end-to-end | Gap | |
| Real VSS / `BSH.Service` | Gap | Mock only |
| WinUI UI / XAML | **None** | No UI E2E; VM/service tests only |

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
                 │ Unit (engine)   │  Backup / restore / delete / edit / migrations  ✅
                 └─────────────────┘
```

For a backup product, **integration tests that prove backup → restore bit-identity** remain more valuable than broad UI automation. E2E should stay small and stable.

---

## Phase 0 — Make beta “testable” (prerequisites)

Before inviting customers:

1. **Freeze a beta build pipeline**
   - Tag pattern already exists: `v*.*.*-beta*`
   - ~~Add a test gate to the release workflow~~ ✅ Landed in #569 (`dotnet test` before Inno Setup).
2. **Pin shell + version** ✅ #607
   - Beta installer (`tools/setup/Setup-WinUI.iss`) starts `BSH.MainApp.exe` (Start Menu, post-install, Run key).
   - WinForms installer (`tools/setup/Setup.iss`) remains a separate artifact launching `BSH.Main.exe`.
   - Release workflow builds both and uploads `backupservicehome-*-win64.exe` plus `backupservicehome-*-winui-win64.exe`.
   - Contract tests: `InstallerEntryPointTests` + `tools/setup/Confirm-InstallerEntryPoint.ps1`.
   - Ensure beta update feed points only at beta tags. **Blocked:** both configured v4 feed URLs returned HTTP 404 on 2026-09-04; follow `beta-operations-runbook.md`.
3. **Crash / diagnostic baseline** ✅ #584, #606
   - ~~Unhandled exceptions logged and shown in WinUI~~ ✅ #584 (`UnhandledExceptionHandler`).
   - ~~WinUI file logging~~ ✅ #606 (`AppEventLog` Serilog file sink). “Show Event Logs” opens `%AppData%\Alexosoft\Backup Service Home 3\log{yyyyMMdd}.txt`.
   - Support collection and data-handling procedure: `docs/testing/beta-operations-runbook.md`.
4. **Test data kit** ✅ fixture script
   - `docs/testing/New-WinUiGoldenPathFixture.ps1` builds nested folders, an empty folder, Unicode names, and a restore-to directory.
   - Long path / locked file / large file remain optional extras for risk-scenario runs.
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

### P1 — Strongly recommended for beta hardening — ✅ done

| Test | Type | Status | Location / notes |
|------|------|--------|------------------|
| **Delete version then restore remaining** | Unit + integration | ✅ #590 | `DeleteTests`; `FileSystemEngineLifecycleTests` |
| **Long-path restore** | Unit | ✅ #590 | `Restore_LongFileNameUsesLongFilesDirectory` |
| **Disk space preflight → abort** | Unit | ✅ #582, #590 | `Backup_AbortsWhenFreeSpaceClearlyInsufficient` |
| Deeper `FileSystemStorage` lifecycle (delete + decrypt/edit on real FS) | Integration | ✅ #590 | `FileSystemEngineLifecycleTests` (rename is not covered; not exhaustive `\\?\` API coverage) |
| **DB migration smoke** | Unit/integration | ✅ #605 | `DatabaseSchemaUpgradeTests` — fixture DBs at schema v1/v8 reach current (v9); newer DB raises `DatabaseIncompatibleException` |
| Long-path folder convention consistency | Code debt | ✅ #603 | `QueryManager` preview/restore paths use `_LONGFILES_`, matching jobs |

### P2 — Nice to have (post-beta or parallel)

| Test | Type | Status / notes |
|------|------|----------------|
| FTP storage with local FTP stub / Testcontainers-like fixture | Integration | Still open; harder in CI; optional nightly |
| Scheduler trigger fires and invokes backup (Quartz) | Integration | Still open; policy/orchestration tests do not run a real Quartz trigger |
| USB media-arrival orchestration | Unit | ✅ #618/#629 `MediaArrivalBackupWatchTests`; real device remains manual |
| WinForms ↔ WinUI orchestration parity expansion | Unit | Improved (`JobSessionRunnerTests`, notification/switch-storage tests); keep extending as needed |
| WinUI DE/EN resource parity | Unit | ✅ #579 `WinUiResourceParityTests` |
| NetArchTest layer rules | Unit | Still open; per `layered-architecture-evolution.md` |

### Explicitly out of scope for “easy” automation

- Full AlphaVSS / real locked Outlook PST in CI (manual + dogfood)
- Installer/service registration on a live machine (entry-point contract is tested; VSS service start is still manual)
- Visual XAML layout regression suites (high cost, low early ROI)

---

## Phase 2 — Basic E2E proposal

Headless engine golden paths for local FS backup↔restore **and** delete/cancel/unicode lifecycle are covered by the integration suites (#569, #590). Remaining E2E gap is **WinUI shell** automation (installer *entry point* is now a contract test, not a click-through).

Browser/restore/delete **view-model** behavior is unit-tested (`BrowserViewModelTests`, `BrowserPreviewServiceTests`); that is not a substitute for clicking through the shell.

### Recommended stack

| Option | Pros | Cons | Status |
|--------|------|------|--------|
| **A. Local-FS orchestration smoke in `BSH.Test`** (no UI) | Fast, stable, CI-friendly | Bypasses view models, XAML, and `IBackupService` orchestration | ✅ `LocalFileSystemOrchestrationSmokeTests` |
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

Automate what we can; **manually** validate what backups actually need in the wild. Engineering mechanisms for installer, logs, and migrations are in. Candidate sign-off must use the installed WinUI artifact and include fresh-install, upgrade, service/VSS, and rollback evidence.

Click-through: `docs/testing/winui-golden-path.md`. Risk procedures:
`docs/testing/beta-risk-scenarios.md`. Sign-off record:
`docs/testing/beta-release-signoff.md`. Fixture:
`docs/testing/New-WinUiGoldenPathFixture.ps1`. Automated local-FS orchestration smoke:
`LocalFileSystemOrchestrationSmokeTests`.

If the app is already configured, use **Extras and Support → Reset Configuration** instead of reinstalling.

### Golden path (every beta build)

1. Install the immutable WinUI candidate; verify version label and Windows service
2. Setup wizard: sources, local target, **Manual backups** (compression in Settings → Backup Options)  
3. Manual full/first backup from Overview  
4. Change files → incremental  
5. Browse versions → search → favorite → preview  
6. Restore file + folder to original and alternate paths  
7. Delete a version; delete a file from selected versions  
8. Run a stable-to-beta upgrade on a separate matrix machine; rehearse rollback

### Risk scenarios (must pass before public beta)

Detailed steps and pass criteria are in `docs/testing/beta-risk-scenarios.md`.

| Scenario | Why critical |
|----------|----------------|
| Encrypted backup restore with wrong password | Security + support load |
| USB target ejected mid-backup | Data integrity / wait-for-media |
| Low disk space on target | Abort vs corrupt version |
| Locked file (Office doc open) + VSS service installed | Everyday Windows reality |
| Long paths / deep trees | Historical bug surface; jobs + preview now share `_LONGFILES_` (#603) — still dogfood in WinUI |
| Unicode / special characters in filenames | Locale (DE/EN) — automated FS round-trip exists; still dogfood in WinUI |
| Battery / laptop on battery with pause setting | Scheduled reliability |
| Scheduled overnight backup + retention cleanup | “Set and forget” promise |
| FTP target backup + restore (lab only) | Second backend |
| Upgrade from last stable / last beta AppData | Migration safety (schema smoke exists; still dogfood a real AppData upgrade) |
| Theme: light and dark | #598 / #604 landed; verify dialogs/browser/status/overview |

### Compatibility matrix (minimum)

- Windows 10 22H2 and Windows 11 (current) x64  
- Local NTFS external drive + folder-on-system-drive  
- German and English UI  
- Fresh install + upgrade install  

### Roles

| Role | Responsibility |
|------|----------------|
| Dev | Fix P0 bugs found in dogfood; optional P2 coverage (FTP/Quartz/VSS) |
| QA / dogfooders | Phase 3 checklist on WinUI beta builds |
| Maintainer | Tag gating, feed config, rollback decision |

---

## Safe launch plan (quality gates)

Backup software fails loudly in customer trust. Launch beta as a **controlled channel**, not a silent default for stable users.

### Gate 1 — Engineering (block tag)

- [ ] Candidate commit CI green (`dotnet-desktop-build`) — rerun after every plan/code update
- [x] **Release workflow runs the test suite and fails the release on test failure** (#569)  
- [x] P0 restore unit + FS integration tests merged and green (#569)  
- [x] P1 delete+restore, long-path restore, disk-space abort, FS lifecycle (#590)  
- [x] DB migration smoke tests (#605)  
- [x] WinUI file logging (#606)  
- [x] Installer starts the intended shell — WinUI beta artifact (#607)  
- [ ] No open P0 bugs on: data loss, restore failure, DB corruption, cancel leaving inconsistent state *(no such issues currently open on GitHub; still a human sign-off)*

### Gate 2 — Internal dogfood (block public beta)

Use `docs/testing/beta-release-signoff.md`; unchecked boxes cannot be satisfied by the
existence of a test or runbook alone.

- [ ] ≥3 internal machines on beta for several days of real schedules  
- [ ] Golden path + risk scenarios signed off **on WinUI** (using the WinUI installer)  
- [ ] At least one full encrypted backup ↔ restore verified bit-for-bit on real FS *(automated happy path exists; still dogfood on real media/USB)*  
- [ ] WinUI installer installs/starts `BSH.MainApp`; `BSH.Service` starts; VSS path smoke-tested  
- [ ] Testers can attach the AppData log from “Show Event Logs” after a crash/error *(sink exists; still verify on an installed build)*

### Gate 3 — Closed beta (limited customers)

- [ ] Opt-in beta feed only — currently blocked because both configured v4 feed URLs returned HTTP 404 on 2026-09-04
- [ ] Clear “beta” labeling verified in installed About UI and release notes (`ApplicationVersionInfo` now preserves the prerelease label)
- [x] Support path documents private collection of logs and, only when needed, DB without passwords
- [ ] Rollback rehearsed; downgrade is unsupported by default after a DB migration unless compatibility is explicitly proven

### Gate 4 — Open beta / promote toward stable

- [ ] No unresolved data-loss issues from closed beta  
- [ ] Crash/error rate meets a threshold recorded before evaluation in `beta-release-signoff.md`
- [ ] FTP and scheduling scenarios covered by at least lab automation or repeated manual runs  
- [x] Packaging decision finalized — dual artifacts; WinUI is the beta entry point (#607)  

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
6. ~~**Pin installer entry point**~~ ✅ #607 — `Setup-WinUI.iss` launches `BSH.MainApp.exe`; WinForms remains a separate artifact.  
7. ~~**WinUI Serilog file sink**~~ ✅ #606 — `AppEventLog` writes the dated AppData log.  
8. ~~**DB migration smoke tests**~~ ✅ #605 — `DatabaseSchemaUpgradeTests`.  
9. ~~**Refresh after #615–#629**~~ ✅ current branch includes the startup, settings, browser, scheduler/USB, deletion-security, and FTP-credential fixes; affected scenarios are listed in `beta-risk-scenarios.md`.
10. **Restore the v4 stable/beta feeds and verify channel isolation.** Both URLs returned HTTP 404 on 2026-09-04. ← **next**
11. **Build one immutable candidate and complete the three-machine sign-off record**, including FTP, Quartz-backed scheduling, VSS, upgrade, and rollback.
12. **Optional:** WinAppDriver smoke for setup wizard + one backup button.

### Effort sketch (technical, not calendar)

| Work item | Touch surface | Status / risk |
|-----------|---------------|---------------|
| RestoreTests | `BSH.Test` + `StorageMock` | ✅ Done |
| FS restore integration | Temp dirs + `FileSystemStorage` | ✅ Done |
| FS lifecycle integration | `FileSystemEngineLifecycleTests` | ✅ Done |
| Release CI test step | Release workflow YAML | ✅ Done |
| WinUI feature parity | `BSH.MainApp` | ✅ Done vs #521 |
| Installer shell pin | `Setup-WinUI.iss` + contract tests | ✅ Done (#607) |
| WinUI file logging | `AppEventLog` Serilog config | ✅ Done (#606) |
| DB migration smoke | `DbMigrationService` fixtures | ✅ Done (#605) |
| Manual / dogfood QA | Installed WinUI shell + risk matrix | **Open** — procedures and sign-off template landed; execution evidence is required |
| WinAppDriver E2E | New project + CI image deps | Optional; high flake/setup cost |

---

## Beta exit criteria

Ship / widen beta only when:

1. Automated: P0 restore suite + existing suite green on the release tag build. ✅ *(mechanism + tests landed, including P1 lifecycle, migrations, installer contract, and file log; still required green on each tag)*  
2. Manual: golden path + encrypted restore + cancel integrity + media-missing signed off on WinUI.  
3. Ops: both update feeds valid and isolated; rollback path rehearsed once.
4. Product: testers install the WinUI artifact (`*-winui-win64.exe`). ✅ packaging; still verify on a real install  
5. Trust: release notes state backup/restore verification steps for participants.
6. Support: WinUI writes a log file testers can attach and private collection is documented. ✅

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
| Long-path preview/restore | ✅ `_LONGFILES_` (#603) | — | Manual deep trees |
| DB schema upgrade | ✅ (#605) | — | Manual AppData upgrade |
| Updates (beta/stable) | Partial unit | — | Manual feed switch |
| Localization DE/EN | ✅ resource parity | — | Manual UI pass |
| Installer + service | ✅ entry-point contract (#607) | — | Every beta build (service/VSS) |
| Event log | ✅ (#606) | — | Attach after a crash |

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
| `src/BSH.Test/DatabaseSchemaUpgradeTests.cs` | Schema upgrade smoke (v1/v8 → v9) |
| `src/BSH.Test/AppEventLogTests.cs` | WinUI dated log path + sink contract |
| `src/BSH.Test/InstallerEntryPointTests.cs` | WinForms vs WinUI ISS entry-point contract |
| `src/BSH.Test/Integration/FileSystemRestoreIntegrationTests.cs` | Real-FS backup↔restore |
| `src/BSH.Test/Integration/FileSystemEngineLifecycleTests.cs` | Real-FS delete+restore / cancel / unicode |
| `src/BSH.Test/Integration/LocalFileSystemOrchestrationSmokeTests.cs` | `SetupService` + direct local-FS engine-job smoke; not a WinUI E2E test |
| `docs/testing/winui-golden-path.md` | Installed WinUI shell golden-path click-through |
| `docs/testing/beta-risk-scenarios.md` | Manual integrity, scheduling, FTP, VSS, and upgrade scenarios |
| `docs/testing/beta-release-signoff.md` | Per-candidate evidence and release decisions |
| `docs/testing/beta-operations-runbook.md` | Feed checks, safe support collection, and rollback |
| `docs/testing/New-WinUiGoldenPathFixture.ps1` | Source tree for the click-through |
| `src/BSH.Test/Mocks/StorageMock.cs` | Shared backup/restore/delete storage mock |
| `src/BSH.Engine/Jobs/RestoreJob.cs` | Restore implementation |
| `src/BSH.Engine/Database/DbMigrationService.cs` | Schema upgrades |
| `src/BSH.Engine/Storage/` | FS/FTP adapters |
| `src/BSH.MainApp/` | WinUI shell under beta |
| `src/BSH.MainApp/Services/AppEventLog.cs` | Serilog file sink for “Show Event Logs” |
| `src/BSH.MainApp/Services/UnhandledExceptionHandler.cs` | Crash UI |
| `.github/workflows/dotnet-desktop-build.yml` | PR test gate |
| `.github/workflows/dotnet-desktop-release.yml` | Tag → test → both installers |
| `tools/setup/Setup-WinUI.iss` | Beta installer (launches `BSH.MainApp.exe`) |
| `tools/setup/Setup.iss` | WinForms installer artifact (launches `BSH.Main.exe`) |
| `tools/setup/Confirm-InstallerEntryPoint.ps1` | Release-time entry-point check |
