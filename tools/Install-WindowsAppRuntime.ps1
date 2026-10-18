$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# WinUI tests run inside testhost.exe. Register the exact Windows App
# Runtime version restored from the NuGet lock file before loading them.
$lockFile = Get-Content "BSH.MainApp\packages.lock.json" -Raw |
  ConvertFrom-Json

$runtimeVersions = @(
  $lockFile.dependencies.PSObject.Properties.Value |
    ForEach-Object {
      $runtimeDependency =
        $_.PSObject.Properties["Microsoft.WindowsAppSDK.Runtime"]

      if ($runtimeDependency) {
        $runtimeDependency.Value.resolved
      }
    } |
    Sort-Object -Unique
)

if ($runtimeVersions.Count -ne 1) {
  throw "Expected exactly one Microsoft.WindowsAppSDK.Runtime version, found: $($runtimeVersions -join ', ')"
}

$runtimeMsixPath = Join-Path $env:USERPROFILE `
  ".nuget\packages\microsoft.windowsappsdk.runtime\$($runtimeVersions[0])\tools\MSIX\win10-x64"

$inventoryPath = Join-Path $runtimeMsixPath "MSIX.inventory"

if (-not (Test-Path $inventoryPath)) {
  throw "Windows App Runtime package inventory was not found at $inventoryPath"
}

$runtimePackages = @{}

foreach ($inventoryEntry in Get-Content $inventoryPath) {
  $packageFile, $packageFullName = $inventoryEntry -split "=", 2
  $runtimePackages[$packageFile] = $packageFullName
}

$frameworkPackage = @(
  $runtimePackages.Keys |
    Where-Object {
      $_ -match '^Microsoft\.WindowsAppRuntime\.\d+\.msix$'
    }
)

$installOrder = @(
  $frameworkPackage
  @(
    $runtimePackages.Keys |
      Where-Object { $_ -match '\.Main\.\d+\.msix$' }
  )
  @(
    $runtimePackages.Keys |
      Where-Object { $_ -match '\.Singleton\.\d+\.msix$' }
  )
  @(
    $runtimePackages.Keys |
      Where-Object { $_ -match '\.DDLM\.\d+\.msix$' }
  )
)

if ($installOrder.Count -ne $runtimePackages.Count) {
  throw "Could not determine a safe installation order for every Windows App Runtime package."
}

$installedPackages = @(
  Get-AppxPackage |
    Select-Object -ExpandProperty PackageFullName
)

foreach ($packageFile in $installOrder) {
  if ($installedPackages -contains $runtimePackages[$packageFile]) {
    continue
  }

  Add-AppxPackage -Path (Join-Path $runtimeMsixPath $packageFile)
}
