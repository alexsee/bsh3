# Confirms the selected payload plus the installer entry points.
param(
    [Parameter(Mandatory = $true)]
    [string] $PublishDir,

    [Parameter(Mandatory = $true)]
    [ValidateSet("stable", "preview")]
    [string] $Channel
)

$ErrorActionPreference = "Stop"

$winFormsIssPath = Join-Path $PSScriptRoot "Setup.iss"
$winUiIssPath = Join-Path $PSScriptRoot "Setup-WinUI.iss"
$publishDir = Resolve-Path $PublishDir
$stableDir = Join-Path $publishDir "stable"
$previewDir = Join-Path $publishDir "preview"
$winFormsIss = Get-Content -Raw -Path $winFormsIssPath
$winUiIss = Get-Content -Raw -Path $winUiIssPath

function Assert-Contains([string] $haystack, [string] $needle, [string] $message) {
    if ($haystack -notlike "*$needle*") {
        throw $message
    }
}

function Assert-File([string] $path, [string] $message) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw $message
    }
}

Assert-Contains $winFormsIss '{app}\BSH.Main.exe' `
    "Setup.iss must keep launching BSH.Main.exe (WinForms) as a separate installer artifact."

if ($winFormsIss -match [regex]::Escape('{app}\BSH.MainApp.exe')) {
    throw "Setup.iss must remain the WinForms installer; WinUI belongs in Setup-WinUI.iss."
}

Assert-Contains $winUiIss '{app}\BSH.MainApp.exe' `
    "Setup-WinUI.iss must launch BSH.MainApp.exe (WinUI) as the beta installer entry point."

if ($winUiIss -match [regex]::Escape('{app}\BSH.Main.exe')) {
    throw "Setup-WinUI.iss still launches BSH.Main.exe; the WinUI artifact must open the WinUI shell."
}

Assert-Contains $winFormsIss '{app}\BSH.Service.exe' `
    "Setup.iss must still register the VSS helper service (BSH.Service.exe)."

Assert-Contains $winUiIss '{app}\BSH.Service.exe' `
    "Setup-WinUI.iss must still register the VSS helper service (BSH.Service.exe)."

Assert-Contains $winUiIss 'Backup Service Home Preview-Dienst' `
    "Setup-WinUI.iss must register its own VSS service."

if ($winUiIss -match 'Backup Service Home-Dienst') {
    throw "Setup-WinUI.iss must not stop or delete the stable VSS service (Backup Service Home-Dienst)."
}

Assert-Contains $winUiIss 'AppMutex=BackupServiceHome3Preview' `
    "Setup-WinUI.iss AppMutex must be the Preview instance mutex."

Assert-Contains $winUiIss 'AppName=Backup Service Home 4' `
    "Setup-WinUI.iss must install side by side under its own name."

Assert-Contains $winUiIss 'ArchitecturesAllowed=x64compatible' `
    "Setup-WinUI.iss must require an x64-compatible system."

Assert-Contains $winUiIss 'ArchitecturesInstallIn64BitMode=x64compatible' `
    "Setup-WinUI.iss must enable 64-bit installation mode."

Assert-Contains $winUiIss 'DefaultDirName={commonpf64}\brightbits\Backup Service Home 4' `
    "Setup-WinUI.iss must use its own installation directory."

Assert-Contains $winUiIss 'DefaultGroupName=Backup Service Home 4' `
    "Setup-WinUI.iss must use its own Start Menu group."

Assert-Contains $winUiIss 'preview-vss.pipe' `
    "Setup-WinUI.iss must ship the Preview VSS pipe name."

Assert-Contains $winUiIss '-autostart' `
    "Setup-WinUI.iss must start the Preview with -autostart."

Assert-Contains $winUiIss 'disableStableAutostart' `
    "Setup-WinUI.iss must offer to disable the stable autostart value."

Assert-Contains $winFormsIss 'Source: "..\..\output\stable\*"' `
    "Setup.iss must package only the stable payload."
Assert-Contains $winUiIss 'Source: "..\..\output\preview\*"' `
    "Setup-WinUI.iss must package only the preview payload."

foreach ($payload in @(
    @{ Channel = "stable"; Directory = $stableDir; Shell = "BSH.Main"; OtherShell = "BSH.MainApp" },
    @{ Channel = "preview"; Directory = $previewDir; Shell = "BSH.MainApp"; OtherShell = "BSH.Main" }
) | Where-Object { $_.Channel -eq $Channel }) {
    Assert-File $payload.Directory "Publish directory is missing: $($payload.Directory)"

    foreach ($executable in @($payload.Shell, "BSH.Service", "SmartPreview")) {
        foreach ($extension in @("exe", "dll", "deps.json", "runtimeconfig.json")) {
            $file = Join-Path $payload.Directory "$executable.$extension"
            Assert-File $file "Release payload is missing $file."
        }
    }

    Assert-File (Join-Path $payload.Directory "BSH.Engine.dll") `
        "Release payload is missing the shared backup engine in $($payload.Directory)."

    $unexpected = @(Get-ChildItem -LiteralPath $payload.Directory -Recurse -File |
        Where-Object {
            $_.Name -like "$($payload.OtherShell).*" -or
            $_.Name -like "BSH.Test.*" -or
            $_.Name -like "testhost.*" -or
            $_.Name -like "nunit*" -or
            $_.Extension -eq ".pdb"
        })
    if ($unexpected.Count -gt 0) {
        throw "Unexpected files in $($payload.Directory): $($unexpected.FullName -join ', ')"
    }
}

if ($Channel -eq "stable") {
    $stableWinUiFiles = @(Get-ChildItem -LiteralPath $stableDir -Recurse -File |
        Where-Object { $_.Name -like "Microsoft.WindowsAppRuntime*" -or $_.Name -eq "Microsoft.ui.xaml.dll" })
    if ($stableWinUiFiles.Count -gt 0) {
        throw "Stable payload contains Windows App Runtime files: $($stableWinUiFiles.FullName -join ', ')"
    }
    Write-Host "Stable payload validated: Setup.iss -> BSH.Main.exe."
    return
}

Assert-File (Join-Path $previewDir "Microsoft.WindowsAppRuntime.Insights.Resource.dll") `
    "Preview payload must include Microsoft.WindowsAppRuntime.Insights.Resource.dll beside BSH.MainApp.exe."

$requiredRuntimeFiles = @(
    "Microsoft.WindowsAppRuntime.dll"
    "Microsoft.WindowsAppRuntime.Bootstrap.dll"
    "Microsoft.ui.xaml.dll"
)

$runtimeRoot = $null

foreach ($fileName in $requiredRuntimeFiles) {
    $runtimeFile = Get-ChildItem -LiteralPath $previewDir -Filter $fileName -Recurse -File |
        Select-Object -First 1

    if (-not $runtimeFile) {
        throw "Release publish did not include the Windows App Runtime file $fileName in $previewDir."
    }

    if ($fileName -eq "Microsoft.WindowsAppRuntime.dll") {
        $runtimeRoot = $runtimeFile.FullName
    }
}

Write-Host "Preview payload validated: Setup-WinUI.iss -> BSH.MainApp.exe."
Write-Host "Windows App Runtime: $runtimeRoot"
