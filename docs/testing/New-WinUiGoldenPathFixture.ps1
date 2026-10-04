# Creates a small source tree for WinUI golden-path QA.
# Does not touch AppData or installers. See docs/testing/winui-golden-path.md.

[CmdletBinding()]
param(
    [string]$Root = (Join-Path $env:TEMP "bsh-winui-golden-path")
)

$ErrorActionPreference = "Stop"

$source = Join-Path $Root "source"
$restore = Join-Path $Root "restore-to"

if (Test-Path $Root) {
    Remove-Item -LiteralPath $Root -Recurse -Force
}

$umlautDir = ([char]0x00E4).ToString() + [char]0x00F6 + [char]0x00FC
$cyrillicFile = ([char]0x0444).ToString() + [char]0x0430 + [char]0x0439 + [char]0x043B + ".txt"
$unicodeDir = Join-Path $source (Join-Path "unicode" $umlautDir)

New-Item -ItemType Directory -Path (Join-Path $source "nested") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $source "empty") | Out-Null
New-Item -ItemType Directory -Path $unicodeDir | Out-Null
New-Item -ItemType Directory -Path $restore | Out-Null

Set-Content -LiteralPath (Join-Path $source "notes.txt") -Value "notes-v1" -Encoding utf8
Set-Content -LiteralPath (Join-Path $source "drop.txt") -Value "drop-me" -Encoding utf8
Set-Content -LiteralPath (Join-Path $source "nested\readme.txt") -Value "nested-readme" -Encoding utf8
Set-Content -LiteralPath (Join-Path $unicodeDir $cyrillicFile) -Value "unicode-content" -Encoding utf8

Write-Host "Golden-path fixture:"
Write-Host "  Source (add this folder in setup): $source"
Write-Host "  Restore-to folder (Restore to...):   $restore"
Write-Host ""
Write-Host "Local target in the wizard is a drive, not this folder."
Write-Host "Pick a drive that does not already have Backups\$env:COMPUTERNAME\$env:USERNAME."
Write-Host ""
Write-Host "If the app is already configured: Extras and Support -> Reset Configuration."
