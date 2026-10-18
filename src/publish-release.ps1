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
$outputRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\output"))

Push-Location $PSScriptRoot
try {
    $shells = @{
        stable = @{ Directory = "stable"; Project = "BSH.Main" }
        preview = @{ Directory = "preview"; Project = "BSH.MainApp" }
    }
    $shell = $shells[$Channel]
    $publishDir = [IO.Path]::GetFullPath((Join-Path $outputRoot $shell.Directory))
    if ([IO.Path]::GetDirectoryName($publishDir) -ne $outputRoot) {
        throw "Publish directory must be a direct child of $outputRoot."
    }

    # A previous build must never contribute files to a new installer.
    if (Test-Path -LiteralPath $publishDir) {
        Remove-Item -LiteralPath $publishDir -Recurse -Force
    }

    # Both shells launch these helpers out of process, so project references
    # alone do not include them. Publish the shell last to keep its config.
    foreach ($project in @("BSH.Service", "SmartPreview", $shell.Project)) {
        $publishArgs = @(
            "publish", "$project\$project.csproj",
            "--configuration", "Release",
            "--output", $publishDir,
            "--runtime", "win-x64",
            "--self-contained", "true",
            "-p:Platform=x64",
            "-p:Version=$BuildVersion",
            "-p:FileVersion=$InfoVersion"
        )
        if ($NoRestore) {
            $publishArgs += "--no-restore"
        }

        & dotnet @publishArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Publishing $project for $($shell.Directory) failed with exit code $LASTEXITCODE."
        }
    }

    Get-ChildItem -LiteralPath $publishDir -Filter *.pdb -Recurse -File |
        Remove-Item -Force

    & "$PSScriptRoot\..\tools\setup\Confirm-InstallerEntryPoint.ps1" -PublishDir $outputRoot -Channel $Channel
}
finally {
    Pop-Location
}
