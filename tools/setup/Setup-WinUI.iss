; WinUI Preview installer. Side-by-side with the stable WinForms product (Setup.iss).
; Own AppId, directory, Start Menu group, Run value, and VSS service.
; The Preview service listens on backupservicehome-preview (preview-vss.pipe).
; Install and uninstall only touch the Preview service, not the stable VSS service.

[Setup]
AppName=Backup Service Home 4
AppVerName=Backup Service Home 4 {#ApplicationVersion}
AppPublisher=Alexander Seeliger Software
AppPublisherURL=https://www.brightbits.de/
AppSupportURL=https://www.brightbits.de/
AppUpdatesURL=https://www.brightbits.de/
DefaultDirName={pf}\Alexosoft\Backup Service Home 4
DefaultGroupName=Backup Service Home 4
LicenseFile=apache-license-2.0.rtf
OutputDir=output
OutputBaseFilename=backupservicehome-{#ApplicationVersion}-winui-win64
Compression=lzma/ultra64
SolidCompression=true
AppCopyright=Alexander Seeliger
AppID={{8F4C2B1A-7E63-4D9A-B5C0-6A1E9D3F47B2}}
InternalCompressLevel=ultra64
ShowLanguageDialog=no
WindowVisible=false
AlwaysShowDirOnReadyPage=true
AlwaysShowGroupOnReadyPage=true
VersionInfoVersion={#ApplicationInfoVersion}
AppVersion={#ApplicationVersion}
VersionInfoCompany=Alexander Seeliger Software
VersionInfoCopyright=Alexander Seeliger
MinVersion=10.0.19045
UninstallDisplayIcon={app}\BSH.MainApp.exe
VersionInfoProductVersion={#ApplicationInfoVersion}
AppMutex=BackupServiceHome3Preview
UsePreviousAppDir=true
DisableWelcomePage=false

[Tasks]
Name: "disableStableAutostart"; Description: "Automatischen Start der stabilen Version deaktivieren"; Flags: checkedonce

[Languages]
Name: german; MessagesFile: compiler:Languages\German.isl

[Icons]
Name: "{group}\Backup Service Home 4"; Filename: "{app}\BSH.MainApp.exe"

[Run]
Filename: "net"; Parameters: "stop ""Backup Service Home Preview-Dienst"""; Flags: runhidden
Filename: "sc"; Parameters: "create ""Backup Service Home Preview-Dienst"" binpath=""{app}\BSH.Service.exe"" start=auto"; WorkingDir: "{app}"; Flags: runhidden runascurrentuser; StatusMsg: "Backup Service Home Preview-Dienst wird eingerichtet..."
Filename: "sc"; Parameters: "config ""Backup Service Home Preview-Dienst"" binpath=""{app}\BSH.Service.exe"" start=auto"; WorkingDir: "{app}"; Flags: runhidden runascurrentuser
Filename: "net"; Parameters: "start ""Backup Service Home Preview-Dienst"""; Flags: runascurrentuser runhidden; StatusMsg: "Backup Service Home Preview-Dienst wird gestartet..."
Filename: "{app}\BSH.MainApp.exe"; Flags: nowait postinstall skipifsilent; Description: "{cm:LaunchProgram,Backup Service Home 4}"

[Registry]
; Inno encodes a literal quote as a doubled quote. The value below is "{app}\BSH.MainApp.exe" -autostart,
; which Windows stores as "C:\...\BSH.MainApp.exe" -autostart.
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "BackupServiceHome3PreviewRun"; ValueData: """{app}\BSH.MainApp.exe"" -autostart"; Flags: uninsdeletevalue
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "BackupServiceHome3Run"; Tasks: disableStableAutostart; Flags: deletevalue

[UninstallRun]
Filename: "net"; Parameters: "stop ""Backup Service Home Preview-Dienst"""; Flags: runhidden
Filename: "sc"; Parameters: "delete ""Backup Service Home Preview-Dienst"""; WorkingDir: "{app}"; Flags: runhidden

[Files]
Source: "..\..\output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "BSH.Main.exe,BSH.Main.deps.json,BSH.Main.runtimeconfig.json,BSH.Main.pdb,BSH.Main.resources.dll"
Source: "preview-vss.pipe"; DestDir: "{app}"; DestName: "vss.pipe"; Flags: ignoreversion

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
	var ErrorCode: Integer;
begin
	if CurStep = ssInstall then begin
		shellExec('open', 'net', 'stop "Backup Service Home Preview-Dienst"', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
	end;
end;

[_ISTool]
UseAbsolutePaths=true
