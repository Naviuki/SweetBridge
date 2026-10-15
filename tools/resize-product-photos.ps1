<#
.SYNOPSIS
    Shrinks catalogue photos so the showcase does not blow up the repository.

.DESCRIPTION
    Processes files in src/SweetBridge.Web/wwwroot/images/products: images wider than
    MaxWidth are re-encoded as JPEG with the given quality, the rest stay untouched.
    The script is idempotent and can be run repeatedly.

.PARAMETER MaxWidth
    Maximum image width in pixels (default 1200).

.PARAMETER Quality
    JPEG quality from 1 to 100 (default 85).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/resize-product-photos.ps1 -MaxWidth 1200 -Quality 85
#>

[CmdletBinding()]
param(
    [string]$Path = 'src/SweetBridge.Web/wwwroot/images/products',
    [int]$MaxWidth = 1200,
    [int]$Quality = 85
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repoRoot = Split-Path -Parent $PSScriptRoot
$dir = Join-Path $repoRoot $Path

if (-not (Test-Path $dir)) {
    throw ("folder not found: " + $Path)
}

$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() |
    Where-Object { $_.MimeType -eq 'image/jpeg' } |
    Select-Object -First 1

$files = Get-ChildItem -Path (Join-Path $dir '*') -File |
    Where-Object { $_.Extension -in '.jpg', '.jpeg', '.png', '.webp' }

$changed = 0
$before = 0L
$after = 0L

foreach ($file in $files) {
    $before += $file.Length

    $image = $null
    $temp = $null

    try {
        $image = [System.Drawing.Image]::FromFile($file.FullName)

        if ($image.Width -le $MaxWidth) {
            $after += $file.Length
            $image.Dispose()
            $image = $null
            continue
        }

        $height = [int][Math]::Round($image.Height * $MaxWidth / $image.Width)
        $bitmap = New-Object System.Drawing.Bitmap $MaxWidth, $height

        try {
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            try {
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.Clear([System.Drawing.Color]::FromArgb(255, 255, 255))
                $graphics.DrawImage($image, 0, 0, $MaxWidth, $height)
            }
            finally { $graphics.Dispose() }

            # The source file must be closed before it is replaced, Windows forbids that.
            $image.Dispose()
            $image = $null

            $parameters = New-Object System.Drawing.Imaging.EncoderParameters 1
            $parameters.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter(
                [System.Drawing.Imaging.Encoder]::Quality, [long]$Quality)

            $temp = [System.IO.Path]::GetTempFileName()
            $bitmap.Save($temp, $codec, $parameters)
        }
        finally { $bitmap.Dispose() }

        Copy-Item -Path $temp -Destination $file.FullName -Force
        $changed++
    }
    catch {
        Write-Warning ("skipped " + $file.Name + ": " + $_.Exception.Message)
    }
    finally {
        if ($temp -and (Test-Path $temp)) { Remove-Item $temp -Force }
        if ($image) { $image.Dispose() }
        if (Test-Path $file.FullName) { $after += (Get-Item $file.FullName).Length }
    }
}

Write-Host ""
Write-Host ("resized: " + $changed + " of " + $files.Count)
Write-Host ("before: {0:N1} MB" -f ($before / 1MB))
Write-Host ("after:  {0:N1} MB" -f ($after / 1MB))