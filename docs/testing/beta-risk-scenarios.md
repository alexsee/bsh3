# WinUI beta risk scenarios

Run these scenarios against the installed WinUI beta candidate. Record the result in `beta-release-signoff.md`; attach screenshots, logs, hashes, and issue links to the release issue. Stop and open a blocker for data loss, an unrestorable backup, database corruption, or a hang that requires killing the process.

## Data-integrity scenarios

### Encrypted removable-media round trip

1. Create a fixture containing text, binary, Unicode, nested, and empty-folder content. Record SHA-256 hashes of all files.
2. Enable encryption and compression, back up to a removable NTFS device, then disconnect and reconnect it normally.
3. Attempt a restore with a wrong password. It must fail clearly without writing plausible-but-invalid files or changing the stored password.
4. Restore with the correct password to a new directory and compare every file hash and the relative file list.

Pass: wrong-password behavior is safe and the correct-password restore is bit-identical.

### Cancel and media removal

1. Back up a fixture large enough to keep the job active.
2. Cancel once from the WinUI status window. On a separate run, eject or disconnect the target during the write.
3. Reconnect/restart and inspect the browser and target.

Pass: the UI does not hang; no incomplete version is offered as restorable; a subsequent backup and restore succeed.

### Low target space

Run a backup whose estimated requirement clearly exceeds available target space.

Pass: the job aborts before committing a version, explains the cause, and the previous versions still restore.

### Real VSS locked-file path

1. Confirm `Get-Service 'Backup Service Home-Dienst'` reports `Running`.
2. Keep a supported document open with an exclusive/locked write path.
3. Run a backup, close the application holding the file, and restore to a new directory.

Pass: the service/VSS path is visible in the log, the backup completes, and the restored file opens. Record the Windows version and application used to create the lock.

### Long paths and special characters

Back up and restore a tree with paths over 260 characters plus German, Cyrillic, spaces, and punctuation. Preview representative files in the browser.

Pass: browser, preview, and restore agree on the tree and restored hashes match.

## Scheduling and media scenarios

### Battery pause

Enable pause-on-battery, make a backup due, disconnect AC power, then reconnect it.

Pass: the backup does not run on battery and runs once eligible; the UI explains the waiting state.

### Scheduled backup and retention

Configure a short-lived test schedule and retention rule, allow multiple triggers without manually starting a backup, and include an app restart between triggers.

Pass: each due interval produces at most one backup, the next run is refreshed after changing `TaskType`, retention removes only eligible versions, and remaining versions restore.

### USB arrival

Make a removable-media backup due, attach the configured target, remove/reinsert it, and observe the job history.

Pass: arrival starts the due job exactly once and never starts a second concurrent/duplicate job.

## Backend and upgrade scenarios

### FTP lab round trip

Use a dedicated non-production FTP account and test dataset. Configure credentials in WinUI, restart the app, back up, browse, restore to a new directory, and compare hashes. Repeat with the server unavailable during backup and restore.

Pass: credentials persist without plaintext in the configuration database, the online round trip is bit-identical, and outages fail clearly without a committed partial version. Do not attach credentials to evidence.

### Stable-to-beta upgrade

1. On a disposable machine, install the last stable version and create at least two restorable versions.
2. Copy `%AppData%\Alexosoft\Backup Service Home 3` while the app is closed.
3. Install the WinUI beta over stable and verify configuration, schedule, browser history, backup, and restore.
4. Follow the rollback rehearsal in `beta-operations-runbook.md`.

Pass: the existing media and database remain usable and rollback follows the documented compatibility decision.

## UI regression pass for changes after the original plan

- [ ] Cold start and normal restart do not crash.
- [ ] FTP target, encryption mode, media type, and schedule `TaskType` survive Apply and restart.
- [ ] Empty-source browser state, version selection, previous/next navigation, favorites, and preview behave correctly.
- [ ] An incomplete delete/decrypt operation leaves security settings unchanged.
- [ ] Light and dark themes render setup, settings, browser, status, and modal dialogs legibly.

