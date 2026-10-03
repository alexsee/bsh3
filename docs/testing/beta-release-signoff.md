# Beta release sign-off record

Copy this file to the release issue or attach a completed copy to the GitHub prerelease. One record covers one immutable WinUI installer. Do not reuse results after rebuilding the artifact.

## Candidate

| Field | Value |
|---|---|
| Version / tag | |
| Commit SHA | |
| WinUI installer filename | |
| Installer SHA-256 | |
| Release workflow URL | |
| Test-suite result URL | |
| Candidate owner | |
| Test window | |

## Gate 1 — engineering

- [ ] Candidate commit contains current `main` and the beta-plan changes.
- [ ] Release workflow built and tested this exact commit.
- [ ] WinUI installer entry-point contract passed.
- [ ] Open issues were triaged for data loss, restore failure, database corruption, and inconsistent cancellation.
- [ ] Issue #115 (backup/file integrity verification) was explicitly accepted for beta or promoted to a blocker.

Decision: **PASS / FAIL**  
Approver/date:  
Notes and issue links:

## Gate 2 — internal dogfood

Record at least three distinct machines used for several days of real scheduled backups.

| Machine | Tester | OS/build | UI language | Storage/backend | Install path | Dates/duration | Golden path | Risk scenarios | Evidence/issues |
|---|---|---|---|---|---|---|---|---|---|
| 1 | | | | | Fresh install | | | | |
| 2 | | | | | Upgrade from stable | | | | |
| 3 | | | | | Fresh or beta upgrade | | | | |

- [ ] The installed candidate displays its prerelease identifier in **About**.
- [ ] The [WinUI golden path](winui-golden-path.md) passed in English and German across the matrix.
- [ ] The [beta risk scenarios](beta-risk-scenarios.md) passed, with any permitted matrix exceptions recorded above.
- [ ] One encrypted backup and restore on real removable media was compared bit-for-bit.
- [ ] The installed Windows service started and the real VSS locked-file path passed.
- [ ] A tester collected the dated log through **Extras and Support → Event Logs**.

Decision: **PASS / FAIL**  
Approver/date:  
Blocking issues:

## Gate 3 — closed beta operations

- [ ] The stable and beta update feeds both return valid XML and reference only their intended channels.
- [ ] The candidate is marked prerelease and beta in the release title/notes.
- [ ] [Support collection and rollback](beta-operations-runbook.md) was reviewed by the support owner.
- [ ] The previous stable installer remains downloadable.
- [ ] Upgrade and rollback were rehearsed with a copied AppData directory.
- [ ] Participant notes say to use duplicated/non-primary data and explain a verification restore.

Decision: **PASS / FAIL**  
Approver/date:

## Gate 4 — widen beta

- [ ] No unresolved data-loss or restore blockers were reported during closed beta.
- [ ] Crash/error rate is within the threshold defined below.
- [ ] FTP and scheduled-backup scenarios have repeated manual results or lab automation.

Observation window:  
Active installations:  
Unexpected termination threshold:  
Backup/restore failure threshold:  
Observed values and source:  

Decision: **PASS / FAIL**  
Approver/date:

