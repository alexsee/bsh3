param([string]$ScreenshotDirectory = (Join-Path $PSScriptRoot '..\docs\design\overview'))

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# Add a review legend and numbered pointers without changing screenshot content.
foreach ($version in @('before', 'after')) {
    $source = [System.Drawing.Image]::FromFile((Join-Path $ScreenshotDirectory "$version-light.png"))
    $canvas = New-Object System.Drawing.Bitmap $source.Width, ($source.Height + 168)
    $graphics = [System.Drawing.Graphics]::FromImage($canvas)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $ink = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(25, 40, 60))
    $accent = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(0, 90, 180))
    $pen = New-Object System.Drawing.Pen $accent, 3
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::ArrowAnchor
    $heading = New-Object System.Drawing.Font 'Segoe UI', 22, ([System.Drawing.FontStyle]::Bold)
    $numberFont = New-Object System.Drawing.Font 'Segoe UI', 16, ([System.Drawing.FontStyle]::Bold)
    $legendFont = New-Object System.Drawing.Font 'Segoe UI', 16
    try {
        $graphics.Clear([System.Drawing.Color]::White)
        $graphics.FillRectangle($ink, 0, 0, $canvas.Width, 60)
        $graphics.DrawString(($version.ToUpperInvariant() + '  /  WinUI overview'), $heading, [System.Drawing.Brushes]::White, 24, 10)
        $graphics.DrawImage($source, 0, 60, $source.Width, $source.Height)
        $points = if ($version -eq 'before') {
            @(@(105, 105, 48, 100), @(160, 220, 225, 285), @(160, 425, 250, 455), @(915, 425, 1000, 455))
        } else {
            @(@(342, 115, 280, 100), @(440, 215, 510, 283), @(440, 555, 490, 640), @(1055, 555, 1115, 640))
        }
        for ($index = 0; $index -lt $points.Count; $index++) {
            $point = $points[$index]
            $x = $point[0]; $y = $point[1] + 60
            $graphics.DrawLine($pen, $x, $y, $point[2], ($point[3] + 60))
            $graphics.FillEllipse($accent, ($x - 19), ($y - 19), 38, 38)
            $graphics.DrawString([string]($index + 1), $numberFont, [System.Drawing.Brushes]::White, ($x - 9), ($y - 15))
        }
        $legendY = $source.Height + 78
        $graphics.DrawString('1  Navigation and page hierarchy', $legendFont, $ink, 24, $legendY)
        $graphics.DrawString('2  Status, schedule and backup controls', $legendFont, $ink, 790, $legendY)
        $graphics.DrawString('3  Last backup, result and change counts', $legendFont, $ink, 24, ($legendY + 40))
        $graphics.DrawString('4  Storage path, connection and free space', $legendFont, $ink, 790, ($legendY + 40))
        $canvas.Save((Join-Path $ScreenshotDirectory "$version-annotated.png"))
    } finally {
        $graphics.Dispose(); $source.Dispose(); $canvas.Dispose()
        $heading.Dispose(); $numberFont.Dispose(); $legendFont.Dispose()
        $pen.Dispose(); $ink.Dispose(); $accent.Dispose()
    }
}
