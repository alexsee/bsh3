; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AppName=Backup Service Home
AppVerName=Backup Service Home 3.8
AppPublisher=Alexander Seeliger Software
AppPublisherURL=https://www.brightbits.de/
AppSupportURL=https://www.brightbits.de/
AppUpdatesURL=https://www.brightbits.de/
DefaultDirName={pf}\Alexosoft\Backup Service Home 3
DefaultGroupName=Backup Service Home 3
LicenseFile=Freewarelizenz.rtf
OutputDir=output
OutputBaseFilename=backupservicehome-3.8.4.0-win64
Compression=lzma/ultra64
SolidCompression=true
AppCopyright=Alexander Seeliger
AppID={{5979B77A-9AE6-4E75-AED8-283C5E16C02D}
InternalCompressLevel=ultra64
ShowLanguageDialog=no
WindowVisible=false
AlwaysShowDirOnReadyPage=true
AlwaysShowGroupOnReadyPage=true
VersionInfoVersion=3.8.4.0
AppVersion=3.8.4.0
VersionInfoCompany=Alexander Seeliger Software
VersionInfoCopyright=Alexander Seeliger
MinVersion=0,6.1.7600
UninstallDisplayIcon={app}\BSH.Main.exe
VersionInfoProductVersion=3.8.4.0
AppMutex=BackupServiceHome3
UsePreviousAppDir=true
DisableWelcomePage=false

[Languages]
Name: german; MessagesFile: compiler:Languages\German.isl

[Icons]
Name: "{group}\Backup Service Home"; Filename: "{app}\BSH.Main.exe"; Parameters: "/config"

[Run]
Filename: "net"; Parameters: "stop ""Backup Service Home-Dienst"""; Flags: runhidden
Filename: "{win}\Microsoft.NET\Framework64\v4.0.30319\installutil.exe"; Parameters: """{app}\BSH.Service.exe"""; WorkingDir: "{app}"; Flags: runhidden runascurrentuser; StatusMsg: "Backup Service Home-Dienst wird eingerichtet..."
Filename: "net"; Parameters: "start ""Backup Service Home-Dienst"""; Flags: runascurrentuser runhidden; StatusMsg: "Backup Service Home-Dienst wird gestartet..."
Filename: "{app}\BSH.Main.exe"; Flags: nowait postinstall skipifsilent; Description: "{cm:LaunchProgram,Backup Service Home}"

[Registry]
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "BackupServiceHome3Run"; ValueData: "{app}\BSH.Main.exe"; Flags: uninsdeletevalue

[UninstallRun]
Filename: "net"; Parameters: "stop ""Backup Service Home-Dienst"""; Flags: runhidden
Filename: "{win}\Microsoft.NET\Framework64\v4.0.30319\installutil.exe"; Parameters: "/U ""{app}\BSH.Service.exe"""; WorkingDir: "{app}"; Flags: runhidden

[ThirdParty]
CompileLogMethod=append

[Files]
Source: "..\..\output\BSH.Main.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Main.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Service.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Service.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Service.Shared.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\C4F.DevKit.PreviewHandler.PreviewHandlerFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\C4F.DevKit.PreviewHandler.PreviewHandlerFramework.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\C4F.DevKit.PreviewHandler.PreviewHandlerHost.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\C4F.DevKit.PreviewHandler.PreviewHandlerHost.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\DotNetZip.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\FluentFTP.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Interop.WMPLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Humanizer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Microsoft.WindowsAPICodePack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Microsoft.WindowsAPICodePack.Shell.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Quartz.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Serilog.Settings.AppSettings.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Serilog.Sinks.Debug.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\ServiceWire.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\SmartPreview.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\SmartPreview.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\System.Diagnostics.DiagnosticSource.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\de\Humanizer.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "..\..\output\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\updateSystemDotNet.Controller.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\AlphaVSS.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\AlphaVSS.x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\AlphaVSS.x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\App.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\AxInterop.WMPLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Controls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Engine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\BSH.Engine.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\output\de-DE\BSH.Main.resources.dll"; DestDir: "{app}\de-DE"; Flags: ignoreversion
Source: "..\..\output\x64\SQLite.Interop.dll"; DestDir: "{app}\x64"; Flags: ignoreversion

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
	var ErrorCode: Integer;
begin
	if CurStep = ssInstall then begin
		shellExec('open', 'net', 'stop "Backup Service Home-Dienst"', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
	end;
end;
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1
//    'v4.6.2'        .NET Framework 4.6.2
//    'v4.7'          .NET Framework 4.7
//    'v4.7.1'        .NET Framework 4.7.1
//    'v4.7.2'        .NET Framework 4.7.2
//    'v4.8'          .NET Framework 4.8
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
          'v4.6.1': versionRelease := 394254; // 394271 before Win10 November Update
          'v4.6.2': versionRelease := 394802; // 394806 before Win10 Anniversary Update
          'v4.7':   versionRelease := 460798; // 460805 before Win10 Creators Update
          'v4.7.1': versionRelease := 461308; // 461310 before Win10 Fall Creators Update
          'v4.7.2': versionRelease := 461808; // 461814 before Win10 April 2018 Update
          'v4.8':   versionRelease := 528040; // 528049 before Win10 May 2019 Update
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4.8', 0) then begin
        MsgBox('Backup Service Home ben�tigt das Microsoft .NET Framework 4.8.'#13#13
            'Bitte benutzen Sie Windows Update um das .NET Framework zu installieren,'#13
            'und starten Sie danach das Setup erneut.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;

[_ISTool]
UseAbsolutePaths=true
