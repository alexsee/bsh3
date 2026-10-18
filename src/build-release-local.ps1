param(
    [Parameter(Mandatory = $true)]
    [string] $BuildVersion,

    [Parameter(Mandatory = $true)]
    [string] $InfoVersion,

    [Parameter(Mandatory = $true)]
    [ValidateSet("stable", "preview")]
    [string] $Channel,

    [switch] $NoRestore
)

$ErrorActionPreference = "Stop"

& "$PSScriptRoot\publish-release.ps1" -BuildVersion $BuildVersion -InfoVersion $InfoVersion -Channel $Channel -NoRestore:$NoRestore

$installers = @{ stable = "Setup.iss"; preview = "Setup-WinUI.iss" }
$installer = $installers[$Channel]
& iscc.exe "$PSScriptRoot\..\tools\setup\$installer" /DApplicationVersion=$BuildVersion /DApplicationInfoVersion=$InfoVersion
if ($LASTEXITCODE -ne 0) {
    throw "Building $installer failed with exit code $LASTEXITCODE."
}
