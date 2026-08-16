# Confirms a release-style publish plus both installer scripts.
param(
    [Parameter(Mandatory = $true)]
    [string] $PublishDir
)

$ErrorActionPreference = "Stop"

$winFormsIssPath = Join-Path $PSScriptRoot "Setup.iss"
$winUiIssPath = Join-Path $PSScriptRoot "Setup-WinUI.iss"
$publishDir = Resolve-Path $PublishDir
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

Write-Host "Shipped installers: WinForms (Setup.iss -> BSH.Main.exe) and WinUI (Setup-WinUI.iss -> BSH.MainApp.exe)."
Write-Host "Windows App Runtime: $runtimeRoot"
