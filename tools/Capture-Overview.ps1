param(
    [Parameter(Mandatory)][int]$ProcessId,
    [Parameter(Mandatory)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'

# Capture only the requested app window, excluding the desktop and other apps.
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class OverviewCapture {
    [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
}
'@
[void][OverviewCapture]::SetThreadDpiAwarenessContext([IntPtr](-4))
$appProcess = Get-Process -Id $ProcessId
$handle = $appProcess.MainWindowHandle
if ($handle -eq 0) { throw 'The app has no visible main window.' }
$bounds = New-Object OverviewCapture+Rect
[void][OverviewCapture]::GetWindowRect($handle, [ref]$bounds)
$bitmap = New-Object System.Drawing.Bitmap ($bounds.Right - $bounds.Left), ($bounds.Bottom - $bounds.Top)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
try {
    $dc = $graphics.GetHdc()
    try {
        if (-not [OverviewCapture]::PrintWindow($handle, $dc, 2)) { throw 'Window capture failed.' }
    } finally { $graphics.ReleaseHdc($dc) }
    $bitmap.Save($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath))
} finally { $graphics.Dispose(); $bitmap.Dispose() }
