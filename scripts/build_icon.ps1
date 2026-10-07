Add-Type -AssemblyName System.Drawing

$rootDir = if (Test-Path (Join-Path $PSScriptRoot "assets")) { $PSScriptRoot } else { Split-Path $PSScriptRoot -Parent }
$assetsDir = Join-Path $rootDir "assets"
if (-not (Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
}

function Render-ZeroLatencyBitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $scale = $size / 256.0

    # Colors
    $cBeige = [System.Drawing.Color]::FromArgb(212, 208, 200)
    $cBeigeDark = [System.Drawing.Color]::FromArgb(140, 136, 128)
    $cBeigeLight = [System.Drawing.Color]::FromArgb(255, 255, 255)
    $cShadow = [System.Drawing.Color]::FromArgb(80, 80, 80)
    $cDarkBezel = [System.Drawing.Color]::FromArgb(32, 36, 40)

    # Monitor Neck
    $neckBrush = New-Object System.Drawing.SolidBrush($cBeigeDark)
    $g.FillRectangle($neckBrush, (106 * $scale), (184 * $scale), (44 * $scale), (26 * $scale))
    $neckBrush.Dispose()

    # Monitor Stand / Foot
    $footPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    [System.Drawing.PointF[]]$pts = @(
        [System.Drawing.PointF]::new((60 * $scale), (236 * $scale)),
        [System.Drawing.PointF]::new((80 * $scale), (210 * $scale)),
        [System.Drawing.PointF]::new((176 * $scale), (210 * $scale)),
        [System.Drawing.PointF]::new((196 * $scale), (236 * $scale))
    )
    $footPath.AddPolygon($pts)
    $footBrush = New-Object System.Drawing.SolidBrush($cBeige)
    $g.FillPath($footBrush, $footPath)
    $footPen = New-Object System.Drawing.Pen($cShadow, [Math]::Max(1.0, 2.0 * $scale))
    $g.DrawPath($footPen, $footPath)
    $footBrush.Dispose(); $footPen.Dispose(); $footPath.Dispose()

    # Monitor Main Body
    $bodyBrush = New-Object System.Drawing.SolidBrush($cBeige)
    $g.FillRectangle($bodyBrush, (24 * $scale), (18 * $scale), (208 * $scale), (170 * $scale))
    $bodyBrush.Dispose()

    # 3D Bevel Borders
    $penLight = New-Object System.Drawing.Pen($cBeigeLight, [Math]::Max(1.0, 3.0 * $scale))
    $penDark = New-Object System.Drawing.Pen($cShadow, [Math]::Max(1.0, 3.0 * $scale))
    $g.DrawLine($penLight, (24 * $scale), (18 * $scale), (232 * $scale), (18 * $scale))
    $g.DrawLine($penLight, (24 * $scale), (18 * $scale), (24 * $scale), (188 * $scale))
    $g.DrawLine($penDark, (232 * $scale), (18 * $scale), (232 * $scale), (188 * $scale))
    $g.DrawLine($penDark, (24 * $scale), (188 * $scale), (232 * $scale), (188 * $scale))
    $penLight.Dispose(); $penDark.Dispose()

    # Recessed Dark Bezel
    $bezelBrush = New-Object System.Drawing.SolidBrush($cDarkBezel)
    $g.FillRectangle($bezelBrush, (40 * $scale), (32 * $scale), (176 * $scale), (128 * $scale))
    $bezelBrush.Dispose()

    # Screen (CRT Teal Gradient)
    $screenRect = New-Object System.Drawing.RectangleF((44 * $scale), (36 * $scale), (168 * $scale), (120 * $scale))
    $screenGrad = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $screenRect,
        [System.Drawing.Color]::FromArgb(0, 150, 150),
        [System.Drawing.Color]::FromArgb(0, 50, 70),
        [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal
    )
    $g.FillRectangle($screenGrad, $screenRect)
    $screenGrad.Dispose()

    # Lightning Bolt (Low Latency Turbo Speed)
    $boltPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    [System.Drawing.PointF[]]$boltPoints = @(
        [System.Drawing.PointF]::new((130 * $scale), (42 * $scale)),
        [System.Drawing.PointF]::new((86 * $scale), (102 * $scale)),
        [System.Drawing.PointF]::new((124 * $scale), (102 * $scale)),
        [System.Drawing.PointF]::new((100 * $scale), (150 * $scale)),
        [System.Drawing.PointF]::new((166 * $scale), (88 * $scale)),
        [System.Drawing.PointF]::new((130 * $scale), (88 * $scale)),
        [System.Drawing.PointF]::new((154 * $scale), (42 * $scale))
    )
    $boltPath.AddPolygon($boltPoints)

    # Bolt Glow
    $glowPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 255, 140, 0), [Math]::Max(1.0, 4.0 * $scale))
    $g.DrawPath($glowPen, $boltPath)
    $glowPen.Dispose()

    # Bolt Fill (Bright Yellow to Amber Gradient)
    $boltGrad = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $screenRect,
        [System.Drawing.Color]::FromArgb(255, 245, 80),
        [System.Drawing.Color]::FromArgb(255, 120, 0),
        [System.Drawing.Drawing2D.LinearGradientMode]::Vertical
    )
    $g.FillPath($boltGrad, $boltPath)
    $boltGrad.Dispose()

    # Bolt Outline
    $boltOutline = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(20, 20, 20), [Math]::Max(1.0, 1.5 * $scale))
    $g.DrawPath($boltOutline, $boltPath)
    $boltOutline.Dispose(); $boltPath.Dispose()

    # "98" Text on Screen
    if ($size -ge 32) {
        $fontSize = [Math]::Max(6.0, 17.0 * $scale)
        $font98 = New-Object System.Drawing.Font('Arial Black', $fontSize, [System.Drawing.FontStyle]::Bold)
        $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(180, 0, 0, 0))
        $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 255))
        $g.DrawString('98', $font98, $shadowBrush, (168 * $scale), (44 * $scale))
        $g.DrawString('98', $font98, $textBrush, (166 * $scale), (42 * $scale))
        $font98.Dispose(); $shadowBrush.Dispose(); $textBrush.Dispose()
    }

    # Green Power LED
    $ledBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(0, 255, 60))
    $g.FillEllipse($ledBrush, (202 * $scale), (170 * $scale), (8 * $scale), (8 * $scale))
    $ledBrush.Dispose()

    # Power Button
    $btnBrush = New-Object System.Drawing.SolidBrush($cBeigeDark)
    $g.FillRectangle($btnBrush, (170 * $scale), (170 * $scale), (22 * $scale), (8 * $scale))
    $btnBrush.Dispose()

    $g.Dispose()
    return $bmp
}

# 1. Render all sizes
$sizes = @(16, 32, 48, 64, 128, 256)
$bitmaps = @()
$pngByteArrays = @()

foreach ($sz in $sizes) {
    $b = Render-ZeroLatencyBitmap $sz
    $bitmaps += $b
    $ms = New-Object System.IO.MemoryStream
    $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngByteArrays += ,$ms.ToArray()
    $ms.Dispose()
}

# Save 256x256 PNG for preview
$bitmaps[-1].Save((Join-Path $assetsDir "ZeroLatency98_256.png"), [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Saved: assets/ZeroLatency98_256.png"

# 2. Write Multi-Resolution .ICO file
$icoPath = Join-Path $assetsDir "ZeroLatency98.ico"
$fs = New-Object System.IO.FileStream($icoPath, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter($fs)

# ICONDIR header (6 bytes)
$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Type (1 = Icon)
$bw.Write([uint16]$sizes.Count) # Image count

# Calculate offsets
$offset = 6 + (16 * $sizes.Count)

for ($i = 0; $i -lt $sizes.Count; $i++) {
    $sz = $sizes[$i]
    $bytes = $pngByteArrays[$i]

    $w = if ($sz -ge 256) { 0 } else { [byte]$sz }
    $h = if ($sz -ge 256) { 0 } else { [byte]$sz }

    $bw.Write([byte]$w)            # bWidth
    $bw.Write([byte]$h)            # bHeight
    $bw.Write([byte]0)             # bColorCount
    $bw.Write([byte]0)             # bReserved
    $bw.Write([uint16]1)           # wPlanes
    $bw.Write([uint16]32)          # wBitCount
    $bw.Write([uint32]$bytes.Length) # dwBytesInRes
    $bw.Write([uint32]$offset)     # dwImageOffset

    $offset += $bytes.Length
}

# Write PNG payloads
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $bw.Write($pngByteArrays[$i])
}

$bw.Flush()
$bw.Close()
$fs.Dispose()

foreach ($b in $bitmaps) { $b.Dispose() }

$icoSize = (Get-Item $icoPath).Length
Write-Host "Generated: assets/ZeroLatency98.ico ($icoSize bytes, containing 16x16, 32x32, 48x48, 64x64, 128x128, 256x256)"
