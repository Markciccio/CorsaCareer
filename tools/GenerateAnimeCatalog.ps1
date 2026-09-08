$root = Split-Path -Parent $PSScriptRoot
$assets = Join-Path $root 'assets'
$out = Join-Path $assets 'anime-catalog.csv'
$rows = [System.Collections.Generic.List[string]]::new()
$rows.Add('file;contesto;categoria;personaggi;mezzi;tag')
Get-ChildItem $assets -File -Filter '*.png' | Sort-Object Name | ForEach-Object {
    $n = $_.BaseName.ToLowerInvariant()
    $category = if ($n -match 'formula') {'formula'} elseif ($n -match 'touring|gt') {'touring-gt'} elseif ($n -match 'endurance') {'endurance'} elseif ($n -match 'kart') {'kart'} else {'anime-scena'}
    $people = if ($n -match 'haru') {'haru-senda'} elseif ($n -match 'rei') {'rei-kisaragi'} elseif ($n -match 'genji') {'genji-arakawa'} elseif ($n -match 'riku') {'riku-hayase'} else {'pilota-team'}
    $vehicle = if ($category -eq 'kart') {'kart'} elseif ($category -eq 'formula') {'formula-single-seater'} elseif ($category -eq 'touring-gt') {'touring-gt'} elseif ($category -eq 'endurance') {'endurance-prototype-gt'} else {'motorsport'}
    $context = ($n -replace '^anime-','' -replace '-',' ')
    $tags = "$category,$people,$vehicle,anime,manga,career" 
    $rows.Add("$($_.Name);$context;$category;$people;$vehicle;$tags")
}
[System.IO.File]::WriteAllLines($out, $rows, [System.Text.UTF8Encoding]::new($false))
Write-Output "Catalogo scritto: $($rows.Count - 1) immagini"
