# WinUI Preview update feed

The Preview app checks this URL only:

`https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml`

That URL returned HTTP 404 when this feed was added. Upload `tools/updates/auto_updater_beta.xml` to that path after each Preview release.

The `<url>` value must point to the Preview installer built by `tools/setup/Setup-WinUI.iss`, named `backupservicehome-<version>-win64.exe` and attached to the `preview-v<version>` GitHub release. Host the executable on `download.brightbits.de` or another public URL, then put that URL in `<url>`. Stable and Preview installers use the same filename format, so select the artifact from the Preview release.

The `<version>` value must be a version AutoUpdater.NET can compare, and it must be greater than the assembly version of the installed Preview (`<Version>` in `src/BSH.MainApp/BSH.MainApp.csproj`, or the release tag such as `3.9.6`). A version that is not newer than the installed build is not offered.

A missing feed or any other failed automatic check is logged and does not show a dialog. A manual Check for Updates click shows “Could not check for updates” / “Update-Prüfung nicht möglich” instead.

Login behavior when both products are installed is described in `tools/setup/Side-by-Side.md`.
