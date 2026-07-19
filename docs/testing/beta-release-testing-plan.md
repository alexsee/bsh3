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

## Progress (as of #569)

Landed on `main` via [#569](https://github.com/alexsee/bsh3/pull/569):

| Item | Location |
|------|----------|
| `RestoreJob` unit suite (`StorageMock`) | `src/BSH.Test/RestoreTests.cs` |
| Extended restore-side `StorageMock` | `src/BSH.Test/Mocks/StorageMock.cs` |
| Real-FS backup↔restore integration | `src/BSH.Test/Integration/FileSystemRestoreIntegrationTests.cs` (`[Category("Integration")]`) |
| Release workflow test gate | `.github/workflows/dotnet-desktop-release.yml` (build + `dotnet test` before Inno Setup) |

**Still open for beta readiness:** manual golden-path QA, packaging/shell decision, remaining P1 gaps (DB migrations, delete+restore, FTP/scheduler), optional UI E2E.

---

## Product context (release risk)

| Fact | Implication for beta |
|------|----------------------|
| Shared engine in `BSH.Engine` powers both shells | Engine regressions hit all users; prioritize engine tests |
| WinUI (`BSH.MainApp`) is the current UI direction | Beta QA must exercise WinUI golden paths end-to-end |
| Installer (`tools/setup/Setup.iss`) still launches **WinForms** `BSH.Main.exe` | Clarify which shell ships in beta; packaging mismatch is a release risk |
| Storage: local FS + FTP; metadata in SQLite; VSS via `BSH.Service` | Real I/O beyond local FS (FTP), media, and VSS still need scenario coverage |
| Auto-update supports stable/beta feeds | Beta channel must be isolatable and roll-backable |

**Decision needed before beta tagging:** Does beta ship WinUI as the installed entry point, WinForms with WinUI as optional/preview, or both? Test matrices below assume **WinUI is the beta UX under test**; adjust packaging gates accordingly.

---

## Current automated coverage (baseline)

**Project:** `src/BSH.Test` (NUnit only)  
**CI:** `.github/workflows/dotnet-desktop-build.yml` runs `dotnet test` on PRs/main (Windows, x64, coverage + SonarCloud).  
**Release workflow:** `.github/workflows/dotnet-desktop-release.yml` now **builds and runs tests** before publish / Inno Setup (landed in #569).

Strength by area:

| Area | Coverage | Notes |
|------|----------|-------|
| `BackupJob` (full/incremental, cancel, compress, encrypt, long path, VSS retry mock) | Strong | Mostly `StorageMock` |
| **`RestoreJob`** (routing, overwrite, cancel, medium fail, incremental links) | **Strong (unit)** | `RestoreTests` + `StorageMock` (#569) |
| **`FileSystemStorage` backup↔restore** | **Strong (integration)** | Plain / compressed / encrypted / incremental (#569) |
| File collector exclusions | Strong | Paths, types, size, masks, system folders |
| `QueryManager` | Strong | Versions, search, restore path resolution |
| Config / schedule policy / schedule settings | Moderate | Policy math + persistence |
| Delete / edit jobs | Moderate / thin | FileType routing, encryption metadata |
| Disk space preflight helpers | Moderate | Pure logic |
| `JobSessionRunner` / WinUI orchestration parity | Moderate | Preflight, battery pause, notifications |
| Browser VM / update service | Thin | Favorites, feed preference |
| **`FTPStorage`** | **Gap** | Real I/O untested |
| DB migrations | Gap | |
| Quartz scheduler end-to-end | Gap | |
| Real VSS / `BSH.Service` | Gap | Mock only |
| WinUI UI / XAML / installer | **None** | No UI E2E |

---

## Testing strategy (pyramid for beta)

```
                 ┌─────────────────┐
                 │ Manual / beta   │  Scenario QA + dogfood   ← next focus
                 │   field tests   │
                 ├─────────────────┤
                 │ Smoke E2E       │  Optional WinUI UI (none yet)
                 ├─────────────────┤
                 │ Integration     │  Real FS backup↔restore  ✅ landed
                 ├─────────────────┤
                 │ Unit (engine)   │  Backup + RestoreJob     ✅ landed
                 └─────────────────┘
```

For a backup product, **integration tests that prove backup → restore bit-identity** remain more valuable than broad UI automation. E2E should stay small and stable.

---

## Phase 0 — Make beta “testable” (prerequisites)

Before inviting customers:

1. **Freeze a beta build pipeline**
   - Tag pattern already exists: `v*.*.*-beta*`
   - ~~Add a test gate to the release workflow~~ ✅ Landed in #569 (`dotnet test` before Inno Setup).
2. **Pin shell + version** *(open)*
   - Document which EXE the beta installer starts.
   - Ensure beta update feed points only at beta tags.
3. **Crash / diagnostic baseline** *(open)*
   - Serilog file location documented for support.
   - Known-good DB backup path: `%AppData%\Alexosoft\Backup Service Home 3\`.
4. **Test data kit** *(open)*
   - Small fixture tree: nested folders, empty folder, Unicode name, long path (>260), locked file (open in Notepad), large file (~500MB optional), junction/symlink if supported.
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

### P1 — Strongly recommended for beta hardening *(remaining)*

| Test | Type | Why | Approach |
|------|------|-----|----------|
| **DB migration smoke** | Unit/integration | Schema upgrades break existing customers | Open fixture DBs at prior schema versions; assert `DbMigrationService` reaches current |
| **Delete version then restore remaining** | Unit | Metadata/orphan cleanup | Backup 2 versions → delete one → restore other |
| **Long-path restore** | Unit | Known debt (`_LONGFILES_` vs `_LONG_FILES_`) | Extend existing long-path backup case through restore |
| **Disk space preflight → abort** | Unit + light orchestration | Avoid half-written backups | Already have helpers; add session-level abort assertion if missing |
| Deeper `FileSystemStorage` API coverage (delete/rename/`\\?\`) | Integration | Beyond backup↔restore happy path | Extend integration category |

### P2 — Nice to have (post-beta or parallel)

| Test | Type | Notes |
|------|------|-------|
| FTP storage with local FTP stub / Testcontainers-like fixture | Integration | Harder in CI; optional nightly |
| Scheduler trigger fires and invokes backup (Quartz) | Integration | Time-travel or short interval + wait |
| `UsbWatchService` media-appear path | Integration | Device simulation is OS-heavy |
| WinForms ↔ WinUI orchestration parity expansion | Unit | Continue `WinUiOrchestrationParityTests` pattern |
| NetArchTest layer rules | Unit | Per `layered-architecture-evolution.md` |

### Explicitly out of scope for “easy” automation

- Full AlphaVSS / real locked Outlook PST in CI (manual + dogfood)
- Installer/service registration (manual + one smoke script on release VM)
- Visual XAML layout regression suites (high cost, low early ROI)

---

## Phase 2 — Basic E2E proposal

Headless engine golden paths for local FS backup↔restore are covered by the integration suite (#569). Remaining E2E gap is **WinUI shell / installer** automation.

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

Automate what we can; **manually** validate what backups actually need in the wild. This is the **primary remaining work** before inviting customers.

### Golden path (every beta build)

1. Clean install (or side-by-side AppData reset)  
2. Setup wizard: sources, local target, compression on/off  
3. Manual full backup  
4. Change files → incremental  
5. Browse versions → search → favorite  
6. Restore file + folder to original and alternate paths  
7. Delete a version  
8. Uninstall / upgrade from previous beta without DB loss  

### Risk scenarios (must pass before public beta)

| Scenario | Why critical |
|----------|----------------|
| Encrypted backup restore with wrong password | Security + support load |
| USB target ejected mid-backup | Data integrity / wait-for-media |
| Low disk space on target | Abort vs corrupt version |
| Locked file (Office doc open) + VSS service installed | Everyday Windows reality |
| Long paths / deep trees | Historical bug surface |
| Unicode / special characters in filenames | Locale (DE/EN) |
| Battery / laptop on battery with pause setting | Scheduled reliability |
| Scheduled overnight backup + retention cleanup | “Set and forget” promise |
| FTP target backup + restore (lab only) | Second backend |
| Upgrade from last stable / last beta AppData | Migration safety |

### Compatibility matrix (minimum)

- Windows 10 22H2 and Windows 11 (current) x64  
- Local NTFS external drive + folder-on-system-drive  
- German and English UI  
- Fresh install + upgrade install  

### Roles

| Role | Responsibility |
|------|----------------|
| Dev | Remaining P1 automated gaps; fix P0 bugs |
| QA / dogfooders | Phase 3 checklist on beta builds |
| Maintainer | Tag gating, feed config, rollback decision |

---

## Safe launch plan (quality gates)

Backup software fails loudly in customer trust. Launch beta as a **controlled channel**, not a silent default for stable users.

### Gate 1 — Engineering (block tag)

- [x] PR CI green (`dotnet-desktop-build`) — required ongoing  
- [x] **Release workflow runs the test suite and fails the release on test failure** (#569)  
- [x] P0 restore unit + FS integration tests merged and green (#569)  
- [ ] No open P0 bugs on: data loss, restore failure, DB corruption, cancel leaving inconsistent state  

### Gate 2 — Internal dogfood (block public beta)

- [ ] ≥3 internal machines on beta for several days of real schedules  
- [ ] Golden path + risk scenarios signed off  
- [ ] At least one full encrypted backup ↔ restore verified bit-for-bit on real FS *(automated happy path exists; still dogfood on real media/USB)*  
- [ ] Installer installs/starts intended shell; `BSH.Service` starts; VSS path smoke-tested  

### Gate 3 — Closed beta (limited customers)

- [ ] Opt-in beta feed only (Autoupdater beta channel)  
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
4. **Retention/delete are destructive** — require confirmation in UI; add automated tests that delete doesn’t remove shared incremental payloads incorrectly.  
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
4. **Manual golden-path checklist** executed on first `v*-beta*` build. ← **next**  
5. **P1 hardening:** DB migration smoke, delete+restore, long-path restore.  
6. **Optional:** WinAppDriver smoke for setup wizard + one backup button.  

### Effort sketch (technical, not calendar)

| Work item | Touch surface | Status / risk |
|-----------|---------------|---------------|
| RestoreTests | `BSH.Test` + `StorageMock` | ✅ Done |
| FS integration | Temp dirs + `FileSystemStorage` | ✅ Done |
| Release CI test step | Release workflow YAML | ✅ Done |
| Manual / dogfood QA | Installer + real media | Open — highest remaining risk |
| WinAppDriver E2E | New project + CI image deps | Optional; high flake/setup cost |

---

## Beta exit criteria

Ship / widen beta only when:

1. Automated: P0 restore suite + existing suite green on the release tag build. ✅ *(mechanism + tests landed; still required green on each tag)*  
2. Manual: golden path + encrypted restore + cancel integrity + media-missing signed off on WinUI.  
3. Ops: beta feed isolated; rollback path rehearsed once.  
4. Product: shell packaging matches what testers install.  
5. Trust: release notes state backup/restore verification steps for participants.

---

## Appendix A — Mapping critical features → test layers

| Feature | Unit | Integration | E2E/Manual |
|---------|------|-------------|------------|
| Full/incremental backup | ✅ | ✅ local FS (#569) | Manual large trees |
| Restore | ✅ (#569) | ✅ local FS (#569) | Golden path / WinUI |
| Compression/encryption | ✅ routing + ✅ FS round-trip | ✅ (#569) | Manual password UX |
| Delete version / file | Partial | After delete+restore P1 | Browser UX |
| Edit/decrypt | Thin | Optional | Manual |
| Exclusions | ✅ | Spot-check | Manual |
| Schedule + retention | Policy unit | Optional Quartz | Overnight dogfood |
| FTP | ❌ | Nightly/lab | Lab manual |
| VSS | Mock only | — | Manual locked files |
| Disk space / media wait | Helpers / session | — | Manual USB |
| Browser / search | Thin VM | — | Manual |
| Updates (beta/stable) | Partial unit | — | Manual feed switch |
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
| `src/BSH.Test/Integration/FileSystemRestoreIntegrationTests.cs` | Real-FS backup↔restore |
| `src/BSH.Test/Mocks/StorageMock.cs` | Shared backup/restore storage mock |
| `src/BSH.Engine/Jobs/RestoreJob.cs` | Restore implementation |
| `src/BSH.Engine/Storage/` | FS/FTP adapters |
| `src/BSH.MainApp/` | WinUI shell under beta |
| `.github/workflows/dotnet-desktop-build.yml` | PR test gate |
| `.github/workflows/dotnet-desktop-release.yml` | Tag → test → installer |
| `tools/setup/Setup.iss` | What customers actually install |
