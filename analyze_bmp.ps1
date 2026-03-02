Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile("C:\Users\xzw\AppData\Roaming\OnTopReplica\bitblt_capture_debug.png")
Write-Host "BitBlt capture size: $($bmp.Width)x$($bmp.Height)"

$orangeCount = 0
$redCount = 0
$allBlack = $true
$sampleColors = @()

for ($y = 0; $y -lt $bmp.Height; $y += 3) {
    for ($x = 0; $x -lt $bmp.Width; $x += 3) {
        $c = $bmp.GetPixel($x, $y)
        if ($c.R -gt 2 -or $c.G -gt 2 -or $c.B -gt 2) { $allBlack = $false }
        if ([Math]::Abs($c.R - 255) -le 30 -and [Math]::Abs($c.G - 128) -le 30 -and [Math]::Abs($c.B - 64) -le 30) {
            $orangeCount++
            if ($sampleColors.Count -lt 5) { $sampleColors += "($x,$y):RGB($($c.R),$($c.G),$($c.B))" }
        }
        if ([Math]::Abs($c.R - 255) -le 30 -and $c.G -le 30 -and $c.B -le 30) {
            $redCount++
        }
    }
}

Write-Host "AllBlack: $allBlack"
Write-Host "Orange match count (tol 30): $orangeCount"
Write-Host "Red match count (tol 30): $redCount"
Write-Host "Sample orange pixels: $($sampleColors -join ', ')"

# Also get top colors
$colors = @{}
for ($y = 0; $y -lt $bmp.Height; $y += 5) {
    for ($x = 0; $x -lt $bmp.Width; $x += 5) {
        $c = $bmp.GetPixel($x, $y)
        $key = "$($c.R),$($c.G),$($c.B)"
        if (-not $colors.ContainsKey($key)) { $colors[$key] = 0 }
        $colors[$key]++
    }
}
Write-Host "`nTop 15 most common colors:"
$colors.GetEnumerator() | Sort-Object -Property Value -Descending | Select-Object -First 15 | ForEach-Object {
    Write-Host "  RGB($($_.Key)) count=$($_.Value)"
}

$bmp.Dispose()
