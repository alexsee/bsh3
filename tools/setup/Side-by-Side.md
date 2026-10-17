# Preview and stable side by side

The WinUI Preview and the stable WinForms app install into different folders and register different Windows services. They share one database file:

`%AppData%\Alexosoft\Backup Service Home 3\backupservicehome.bshdb`

Only one of them may run its engine. The stable app and the Preview both take the mutex `BackupServiceHome3`.

## Login

Each product has its own Run value:

- Stable: `BackupServiceHome3Run`
- Preview: `BackupServiceHome3PreviewRun`, set to `"C:\...\BSH.MainApp.exe" -autostart`

If both values are present, Windows starts both programs at login. The Preview installer shows the optional task “Automatischen Start der stabilen Version deaktivieren”. It is checked the first time (`checkedonce`). Leaving it checked deletes `BackupServiceHome3Run` so only the Preview starts. Uncheck it to keep the stable autostart entry. Uninstalling the Preview does not recreate that entry.

When Preview is started with `-autostart` and the stable app already holds `BackupServiceHome3`, Preview exits without a dialog and writes a log line. A normal launch (Start Menu, shortcut, or no `-autostart`) still shows the “already running” message.

The reverse is unchanged stable behavior: if Preview is already running, it holds `BackupServiceHome3`, and the stable app’s existing single-instance check exits quietly with no message.
