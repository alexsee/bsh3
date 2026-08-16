# Confirms a release-style publish + Inno Setup script install the WinUI shell.
param(
    [Parameter(Mandatory = $true)]
    [string] $PublishDir
)

$ErrorActionPreference = "Stop"

$setupIssPath = Join-Path $PSScriptRoot "Setup.iss"
$publishDir = Resolve-Path $PublishDir
$iss = Get-Content -Raw -Path $setupIssPath

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

Assert-Contains $iss '{app}\BSH.MainApp.exe' `
    "Setup.iss must launch BSH.MainApp.exe (WinUI) as the installed entry point."

if ($iss -match [regex]::Escape('{app}\BSH.Main.exe')) {
    throw "Setup.iss still launches BSH.Main.exe; the beta entry point must be the WinUI shell."
}

Assert-Contains $iss '{app}\BSH.Service.exe' `
    "Setup.iss must still register the VSS helper service (BSH.Service.exe)."

Assert-File (Join-Path $publishDir "BSH.MainApp.exe") `
    "Release publish did not include the unpackaged WinUI shell (BSH.MainApp.exe) in $publishDir."

Assert-File (Join-Path $publishDir "BSH.Main.exe") `
    "Release publish must still build the WinForms shell (BSH.Main.exe) in $publishDir."

Assert-File (Join-Path $publishDir "BSH.Service.exe") `
    "Release publish did not include the VSS helper (BSH.Service.exe) in $publishDir."

$requiredRuntimeFiles = @(
    "Microsoft.WindowsAppRuntime.dll"
    "Microsoft.WindowsAppRuntime.Bootstrap.dll"
    "Microsoft.ui.xaml.dll"
)

$runtimeRoot = $null

foreach ($fileName in $requiredRuntimeFiles) {
    $runtimeFile = Get-ChildItem -LiteralPath $publishDir -Filter $fileName -Recurse |
        Select-Object -First 1

    if (-not $runtimeFile) {
        throw "Release publish did not include the Windows App Runtime file $fileName in $publishDir."
    }

    if ($fileName -eq "Microsoft.WindowsAppRuntime.dll") {
        $runtimeRoot = $runtimeFile.FullName
    }
}

Write-Host "Installer entry point is BSH.MainApp.exe (WinUI); VSS helper and WinForms shell are still published."
Write-Host "Windows App Runtime: $runtimeRoot"
