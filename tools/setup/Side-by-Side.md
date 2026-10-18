# Preview and stable side by side

## Release packaging

From `src`, select exactly one application per build:

```powershell
./build-release-local.ps1 -Channel stable -BuildVersion 3.9.4 -InfoVersion 3.9.4.0
./build-release-local.ps1 -Channel preview -BuildVersion 4.0.0-beta1 -InfoVersion 4.0.0.1
```

This requires the .NET SDK and Inno Setup (`iscc.exe` on PATH). CI uses the same
script with `-NoRestore` after its locked restore and build/test steps.
`BuildVersion` sets the selected app/helper assembly versions and installer version;
`InfoVersion` is the numeric Windows file version. The other application's payload
and installers are left untouched.

Each build cleans and publishes only its selected payload:

- `output/stable`: `BSH.Main` and its dependencies, the VSS service, and SmartPreview.
- `output/preview`: `BSH.MainApp` and its dependencies (including the self-contained
  Windows App Runtime), the VSS service, and SmartPreview.

Only the selected shell and the two helper projects are published; test outputs and
the other shell never enter the payload. Both installers recursively package their
own directory, including satellite resources and native dependencies. The preview
installer also installs `preview-vss.pipe` as `vss.pipe`.

The WinUI project copies `Microsoft.WindowsAppRuntime.Insights.Resource.dll` from
the framework MSIX in the restored Windows App SDK Runtime package into self-contained
builds and publish output. This works around Windows App SDK 2.5.1 notification
registration failures (microsoft/WindowsAppSDK#6774) without installing a separate
runtime. The payload check requires this DLL before compiling the installer.

The payload check runs before Inno Setup and rejects missing helpers, mixed shells,
test assemblies, debug symbols, and Windows App Runtime files in the stable payload.
The selected installer is written to `tools/setup/output` as
`backupservicehome-<version>-win64.exe` for either channel. Each invocation generates
only the selected installer. Building both channels with the same version locally
overwrites that filename; use distinct versions or copy the first installer before
building the other channel.

CI tags select the application independently of its version number:

- `v3.9.4` builds and uploads only stable version `3.9.4`.
- `preview-v4.0.0-beta1` builds and uploads only preview version `4.0.0-beta1`.

`v*` tags always select stable; `preview-v*` tags always select preview. Existing
three-part version and optional `-beta<number>` tag formats are supported. CI derives
the numeric file version by replacing `-beta` with `.`. Manual workflow runs accept
a channel choice and a version directly, without deriving a version from the branch
name. GitHub releases are created or updated only for tag-triggered runs.

## Runtime isolation

The WinUI Preview defaults to `Program Files\brightbits\Backup Service Home 4`.
Its installer requires x64-compatible Windows and uses 64-bit installation mode.
Existing installations retain their previously selected directory; choose the new
directory explicitly or uninstall the old Preview before reinstalling to move it.
The stable WinForms app installs into a different folder, and each app registers its
own Windows service. They share one database file:

`%AppData%\Alexosoft\Backup Service Home 3\backupservicehome.bshdb`

Only one of them may run its engine. The stable app and the Preview both take the mutex `BackupServiceHome3`.

## Login

Each product has its own Run value:

- Stable: `BackupServiceHome3Run`
- Preview: `BackupServiceHome3PreviewRun`, set to `"C:\...\BSH.MainApp.exe" -autostart`

If both values are present, Windows starts both programs at login. The Preview installer shows the optional task “Automatischen Start der stabilen Version deaktivieren”. It is checked the first time (`checkedonce`). Leaving it checked deletes `BackupServiceHome3Run` so only the Preview starts. Uncheck it to keep the stable autostart entry. Uninstalling the Preview does not recreate that entry.

When Preview is started with `-autostart` and the stable app already holds `BackupServiceHome3`, Preview exits without a dialog and writes a log line. A normal launch (Start Menu, shortcut, or no `-autostart`) still shows the “already running” message.

The reverse is unchanged stable behavior: if Preview is already running, it holds `BackupServiceHome3`, and the stable app’s existing single-instance check exits quietly with no message.
