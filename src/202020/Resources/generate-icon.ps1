Add-Type -AssemblyName System.Drawing

function Create-EyeIcon([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # Draw a green circle (eye symbol)
    $penWidth = [Math]::Max(1, [int]($size / 8))
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 76, 175, 80), $penWidth)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 76, 175, 80))

    # Outer circle (eye outline)
    $margin = [int]($size * 0.1)
    $g.DrawEllipse($pen, $margin, $margin, $size - 2 * $margin - 1, $size - 2 * $margin - 1)

    # Inner filled circle (pupil)
    $innerMargin = [int]($size * 0.3)
    $g.FillEllipse($brush, $innerMargin, $innerMargin, $size - 2 * $innerMargin, $size - 2 * $innerMargin)

    $g.Dispose()
    $pen.Dispose()
    $brush.Dispose()
    return $bmp
}

$bmp16 = Create-EyeIcon 16
$bmp32 = Create-EyeIcon 32

$outputPath = Join-Path $PSScriptRoot 'icon.ico'

# Convert bitmaps to PNG byte arrays
$ms16 = New-Object System.IO.MemoryStream
$bmp16.Save($ms16, [System.Drawing.Imaging.ImageFormat]::Png)
$png16 = $ms16.ToArray()
$ms16.Dispose()

$ms32 = New-Object System.IO.MemoryStream
$bmp32.Save($ms32, [System.Drawing.Imaging.ImageFormat]::Png)
$png32 = $ms32.ToArray()
$ms32.Dispose()

$bmp16.Dispose()
$bmp32.Dispose()

# ICO header: 2 images
$headerSize = 6
$entrySize = 16
$numImages = 2
$dataOffset1 = $headerSize + ($entrySize * $numImages)
$dataOffset2 = $dataOffset1 + $png16.Length

$ico = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($ico)

# ICONDIR header
$writer.Write([UInt16]0)          # Reserved
$writer.Write([UInt16]1)          # Type: 1 = ICO
$writer.Write([UInt16]$numImages) # Count

# ICONDIRENTRY 1: 16x16
$writer.Write([byte]16)           # Width
$writer.Write([byte]16)           # Height
$writer.Write([byte]0)            # Color palette
$writer.Write([byte]0)            # Reserved
$writer.Write([UInt16]1)          # Color planes
$writer.Write([UInt16]32)         # Bits per pixel
$writer.Write([UInt32]$png16.Length) # Size of image data
$writer.Write([UInt32]$dataOffset1)  # Offset to data

# ICONDIRENTRY 2: 32x32
$writer.Write([byte]32)
$writer.Write([byte]32)
$writer.Write([byte]0)
$writer.Write([byte]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]32)
$writer.Write([UInt32]$png32.Length)
$writer.Write([UInt32]$dataOffset2)

# Write PNG image data
$writer.Write($png16)
$writer.Write($png32)

$writer.Flush()
[System.IO.File]::WriteAllBytes($outputPath, $ico.ToArray())

$writer.Dispose()
$ico.Dispose()

Write-Host "ICO file created successfully at: $outputPath"
Write-Host "File size: $((Get-Item $outputPath).Length) bytes"
