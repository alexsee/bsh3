# WinUI Preview update feed

The Preview app checks this URL only:

`https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml`

That URL returned HTTP 404 when this feed was added. Upload `tools/updates/auto_updater_beta.xml` to that path after each Preview release.

The `<url>` value must be the Inno artifact whose name ends in `-winui-win64.exe` (the file `tools/setup/Setup-WinUI.iss` writes, also attached to the GitHub release). Host the executable on `download.brightbits.de` or another public URL, then put that URL in `<url>`. Do not point the feed at the stable WinForms installer (`backupservicehome-<version>-win64.exe`).

`<version>` must be greater than the assembly version of the installed Preview or AutoUpdater.NET will not offer the file. A missing feed or any other failed check is logged and does not show a dialog.
