# WinUI Preview update feed

The Preview app checks this URL only:

`https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml`

That URL returned HTTP 404 when this feed was added. Upload `tools/updates/auto_updater_beta.xml` to that path after each Preview release.

The `<url>` value must point to the Preview installer built by `tools/setup/Setup-WinUI.iss`, named `backupservicehome-<version>-win64.exe` and attached to the `preview-v<version>` GitHub release. Host the executable on `download.brightbits.de` or another public URL, then put that URL in `<url>`. Stable and Preview installers use the same filename format, so select the artifact from the Preview release.

Set `<version>` to the release's numeric `InfoVersion`, which the release script also sets as the app's assembly version. For example, `4.0.0-beta1` uses `4.0.0.1`, and `4.0.0-beta2` uses `4.0.0.2`. Do not put a `-beta` suffix in the feed's `<version>`.

AutoUpdater.NET offers an update only when the feed version is greater than the installed assembly version. Keep numeric versions increasing, including when moving from beta to a final release: `4.0.0.0` would be lower than `4.0.0.2`.

A missing feed or any other failed automatic check is logged and does not show a dialog. A manual Check for Updates click shows “Could not check for updates” / “Update-Prüfung nicht möglich” instead.

Login behavior when both products are installed is described in `tools/setup/Side-by-Side.md`.
