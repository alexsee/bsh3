# Beta operations, support, and rollback

This runbook covers the WinUI beta channel. Complete its release-specific checks in `beta-release-signoff.md`.

## Update-feed verification

The WinUI client selects between:

- Stable: `https://updates.brightbits.de/backup_service_home/v4/auto_updater.xml`
- Beta: `https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml`

Before inviting testers:

1. Fetch both URLs from a machine outside the publishing environment. HTTP errors, redirects to HTML, invalid XML, or an inaccessible installer are blockers.
2. Confirm the stable feed does not advertise a prerelease and the beta feed advertises the intended beta tag and WinUI installer.
3. Install stable with beta downloads disabled and verify it stays on stable. Enable beta downloads in WinUI and verify the candidate is offered.
4. Disable beta downloads again and verify subsequent checks use the stable feed.
5. Record the feed contents, check time, and downloaded installer SHA-256 in the sign-off record.

Verification on 2026-09-04: both configured v4 URLs returned HTTP 404. Gate 3 therefore remains blocked until they publish valid feeds and the checks above pass.

## Beta labeling and participant notes

- Publish the GitHub release as a prerelease with `beta` in its tag, title, and first paragraph.
- Confirm **Extras and Support → About** shows the same prerelease identifier as the installer and release.
- State that the beta must use duplicated/non-primary datasets until Gate 4.
- Tell participants to perform a small verification restore after their first backup.
- Link this support procedure and describe how to report a blocking restore or data-integrity failure.

## Support collection

Ask for the minimum evidence needed:

1. Exact version from **About**, Windows version, UI language, backend, and reproduction steps.
2. Screenshot of the visible error/status without personal paths where practical.
3. The dated file opened by **Extras and Support → Event Logs**: `%AppData%\Alexosoft\Backup Service Home 3\log{yyyyMMdd}.txt`.
4. Only when database diagnosis is necessary, a copy of `%AppData%\Alexosoft\Backup Service Home 3\backupservicehome.bshdb` made while the app is closed.

Logs and the database may contain usernames, paths, filenames, server names, and protected credential material. Never ask for a password, encryption key, raw FTP credential, or customer backup payload. Obtain consent, use a private access-controlled transfer, restrict retention, and delete the evidence when the issue is resolved. Do not attach AppData artifacts to a public GitHub issue.

## Stop-ship and rollback

Stop rollout for suspected data loss, an unrestorable backup, database corruption, unsafe credential exposure, or repeated cancellation/media-removal corruption.

1. Remove or disable the beta-feed entry without changing the stable feed.
2. Preserve the affected installer, SHA-256, feed XML, logs, and issue link for diagnosis.
3. Keep the previous stable installer available. Do not tell users to downgrade until database compatibility has been decided.
4. Before a rehearsal, close the app and copy the entire AppData directory to a protected location.
5. If the beta opened or migrated the database, treat rollback as **not supported by default**. Restore the pre-upgrade AppData copy before starting stable, or ship a forward-compatible fix. Never point an older binary at a newer database without an explicit compatibility test.
6. After rollback, verify the service is running, open the browser, restore a pre-beta version to a new directory, and compare hashes.
7. Publish a notice covering affected versions, symptoms, mitigation, data-preservation instructions, and the next update.

Record who performed the rehearsal, the stable/beta versions, schema version, result, and evidence in the candidate sign-off.

