<#
.SYNOPSIS
    Downloads the partner catalogue (https://yhkhleb.ru/production/) into the project.

.DESCRIPTION
    Walks the partner category pages, reads the product name and photo link from the card
    markup, saves the images to src/SweetBridge.Web/wwwroot/images/products and writes the
    download list to docs/photo-import/source-catalog.json.

    Photos are used with the owner's permission. Requests are throttled so the partner site
    is not overloaded. After the first run call tools/resize-product-photos.ps1 to shrink
    the files.

.PARAMETER Output
    Folder for the images (default: src/SweetBridge.Web/wwwroot/images/products).

.PARAMETER DelayMs
    Pause between requests in milliseconds.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/import-product-photos.ps1
#>

[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://yhkhleb.ru',
    [string]$CatalogUrl = 'https://yhkhleb.ru/production/',
    [string]$Output = 'src/SweetBridge.Web/wwwroot/images/products',
    [int]$DelayMs = 400
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$outputDir = Join-Path $repoRoot $Output
$manifestDir = Join-Path $repoRoot 'docs/photo-import'
New-Item -ItemType Directory -Force -Path $outputDir, $manifestDir | Out-Null

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$session.UserAgent = 'SweetBridgeCatalogImport/1.0'

function Get-Html([string]$url) {
    Write-Host ("fetching " + $url)
    Start-Sleep -Milliseconds $DelayMs
    return (Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 60 -WebSession $session).Content
}

function ConvertTo-Slug([string]$text) {
    $map = @{
        'а' = 'a'; 'б' = 'b'; 'в' = 'v'; 'г' = 'g'; 'д' = 'd'; 'е' = 'e'; 'ё' = 'e'; 'ж' = 'zh'
        'з' = 'z'; 'и' = 'i'; 'й' = 'y'; 'к' = 'k'; 'л' = 'l'; 'м' = 'm'; 'н' = 'n'; 'о' = 'o'
        'п' = 'p'; 'р' = 'r'; 'с' = 's'; 'т' = 't'; 'у' = 'u'; 'ф' = 'f'; 'х' = 'h'; 'ц' = 'c'
        'ч' = 'ch'; 'ш' = 'sh'; 'щ' = 'sch'; 'ъ' = ''; 'ы' = 'y'; 'ь' = ''; 'э' = 'e'; 'ю' = 'yu'; 'я' = 'ya'
    }

    $result = -join ($text.ToLower().ToCharArray() | ForEach-Object {
        $key = [string]$_
        if ($map.ContainsKey($key)) { $map[$key] }
        elseif ([char]::IsLetterOrDigit($_)) { $key }
        elseif ($key -eq ' ') { '-' }
        else { '' }
    })

    return ($result -replace '-{2,}', '-').Trim('-')
}

# 1. Partner categories
$catalogHtml = Get-Html $CatalogUrl
$categories = [regex]::Matches($catalogHtml, 'href="(/production/[^"#?]+/)"') |
    ForEach-Object { $_.Groups[1].Value } |
    Sort-Object -Unique

Write-Host ("categories: " + $categories.Count)

# 2. Products and photos
$items = New-Object System.Collections.Generic.List[object]

foreach ($category in $categories) {
    $html = Get-Html ($BaseUrl + $category)

    foreach ($card in [regex]::Matches($html, '(?s)<a[^>]*class="catalog-card[^"]*"[^>]*>')) {
        $markup = $card.Value

        $nameMatch = [regex]::Match($markup, 'data-name="(?<name>[^"]+)"')
        if (-not $nameMatch.Success) { continue }

        $name = [System.Net.WebUtility]::HtmlDecode($nameMatch.Groups['name'].Value).Trim()
        if ([string]::IsNullOrWhiteSpace($name)) { continue }

        $image = $null
        $gallery = [regex]::Match($markup, 'data-gallery="(?<json>[^"]+)"')
        if ($gallery.Success) {
            try {
                $decoded = [System.Net.WebUtility]::HtmlDecode($gallery.Groups['json'].Value)
                $first = ($decoded | ConvertFrom-Json | Select-Object -First 1)
                if ($first -is [string] -and $first -match '^/upload/') { $image = $first }
            }
            catch { }
        }

        if (-not $image) {
            $img = [regex]::Match($markup, '<img[^>]+src="(?<src>/upload/[^"]+)"')
            if ($img.Success) { $image = $img.Groups['src'].Value }
        }

        if (-not $image) { continue }

        $items.Add([pscustomobject]@{
                Category = $category.Trim('/')
                Name = $name
                ImageUrl = $BaseUrl + $image
            })
    }
}

Write-Host ("products: " + $items.Count)

# 3. Download
$downloaded = New-Object System.Collections.Generic.List[object]

foreach ($item in $items) {
    $fileName = (ConvertTo-Slug $item.Name) + '.jpg'
    $target = Join-Path $outputDir $fileName
    $record = [pscustomobject]@{
        Category = $item.Category
        Name = $item.Name
        File = 'images/products/' + $fileName
        Source = $item.ImageUrl
        Cached = (Test-Path $target)
    }

    if (-not $record.Cached) {
        try {
            Start-Sleep -Milliseconds $DelayMs
            Invoke-WebRequest -Uri $item.ImageUrl -OutFile $target -UseBasicParsing -TimeoutSec 60 `
                -WebSession $session -Headers @{ Referer = $CatalogUrl }
            Write-Host ("  saved " + $fileName)
        }
        catch {
            Write-Warning ("failed " + $item.ImageUrl + ": " + $_.Exception.Message)
            if (Test-Path $target) { Remove-Item $target -Force }
            continue
        }
    }

    $downloaded.Add($record)
}

# 4. Manifest
$manifest = [pscustomobject]@{
    Source = $CatalogUrl
    ImportedAt = (Get-Date).ToString('s')
    Note = 'Photos of the partner production; used with the owner permission.'
    Images = $downloaded
}

$manifestPath = Join-Path $manifestDir 'source-catalog.json'
[System.IO.File]::WriteAllText(
    $manifestPath,
    ($manifest | ConvertTo-Json -Depth 5),
    (New-Object System.Text.UTF8Encoding($false)))

Write-Host ""
Write-Host ("files: " + $downloaded.Count)
Write-Host ("manifest: docs/photo-import/source-catalog.json")
Write-Host "next step: powershell -ExecutionPolicy Bypass -File tools/resize-product-photos.ps1"