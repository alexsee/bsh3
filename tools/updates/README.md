# WinUI Preview update feed

The Preview app checks this URL only:

`https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml`

That URL returned HTTP 404 when this feed was added. Upload `tools/updates/auto_updater_beta.xml` to that path after each Preview release.

The `<url>` value must be the Inno artifact whose name ends in `-winui-win64.exe` (the file `tools/setup/Setup-WinUI.iss` writes, also attached to the GitHub release). Host the executable on `download.brightbits.de` or another public URL, then put that URL in `<url>`. Do not point the feed at the stable WinForms installer (`backupservicehome-<version>-win64.exe`).

Replace `REPLACE_WITH_VERSION` in both `<version>` and `<url>` before uploading. It must be a version AutoUpdater.NET can compare, and it must be greater than the assembly version of the installed Preview (`<Version>` in `src/BSH.MainApp/BSH.MainApp.csproj`, or the release tag such as `3.9.6`). Do not upload the placeholder unchanged. A version that is not newer than the installed build is not offered.

A missing feed or any other failed automatic check is logged and does not show a dialog. A manual Check for Updates click shows “Could not check for updates” / “Update-Prüfung nicht möglich” instead.

Login behavior when both products are installed is described in `tools/setup/Side-by-Side.md`.
