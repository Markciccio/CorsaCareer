$ErrorActionPreference = 'Stop'

# Installer locale e reversibile: non modifica Assetto Corsa e non scarica nulla.
$packageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $packageRoot 'CorsaCareer1991.exe'
if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) {
    throw "CorsaCareer1991.exe non trovato nella cartella del pacchetto: $packageRoot"
}

$saveRoot = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Assetto Corsa\CorsaCareer'
$mediaRoot = Join-Path $saveRoot 'media'
New-Item -ItemType Directory -Force -Path $saveRoot, $mediaRoot, (Join-Path $mediaRoot 'captures'), (Join-Path $mediaRoot 'manual'), (Join-Path $mediaRoot 'magazine') | Out-Null

$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop 'Corsa Career.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $packageRoot
$shortcut.Description = 'CorsaCareer — manager esterno di carriera per Assetto Corsa'
$shortcut.IconLocation = "$exePath,0"
$shortcut.Save()

Write-Host "Installazione completata." -ForegroundColor Green
Write-Host "Dati carriera: $saveRoot"
Write-Host "Collegamento creato: $shortcutPath"
Start-Process -FilePath $exePath -WorkingDirectory $packageRoot
