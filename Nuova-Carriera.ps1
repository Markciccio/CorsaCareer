param()

$ErrorActionPreference = 'Stop'
$project = $PSScriptRoot
Set-Location -LiteralPath $project

Write-Host 'CorsaCareer - nuova carriera pulita' -ForegroundColor Cyan

# Chiude solo i processi appartenenti all'app e ai suoi player audio. Assetto
# Corsa/Content Manager e le altre applicazioni dell'utente non vengono toccati.
Get-Process -Name 'CorsaCareer','ffplay' -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

# La cartella è isolata dalla carriera reale sotto Documenti: il reset è quindi
# completo ma non distrugge una carriera che l'utente volesse conservare.
$fresh = Join-Path $project 'debug-carriera\nuova'
if (Test-Path -LiteralPath $fresh) {
    Get-ChildItem -LiteralPath $fresh -Force -ErrorAction SilentlyContinue |
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Force -Path $fresh | Out-Null

function Find-DotnetSdk {
    $roots = @("$env:LOCALAPPDATA\Microsoft\dotnet", "$env:USERPROFILE\.dotnet", "$env:ProgramFiles\dotnet")
    foreach ($root in $roots) {
        $exe = Join-Path $root 'dotnet.exe'; $sdkDir = Join-Path $root 'sdk'
        if ((Test-Path -LiteralPath $exe) -and (Test-Path -LiteralPath $sdkDir) -and
            @(Get-ChildItem -LiteralPath $sdkDir -Directory -ErrorAction SilentlyContinue).Count -gt 0) { return $root }
    }
    return $null
}
$sdkRoot = Find-DotnetSdk
if (-not $sdkRoot) { throw 'SDK .NET non trovato: installare .NET SDK 9.' }
$sdk = Join-Path $sdkRoot 'dotnet.exe'
$env:DOTNET_ROOT = $sdkRoot; $env:PATH = "$sdkRoot;$env:PATH"
& $sdk build (Join-Path $project 'CorsaCareer.csproj') -c Release -v q --nologo
if ($LASTEXITCODE -ne 0) { throw 'Compilazione fallita: l''app non viene avviata.' }

$exe = Join-Path $project 'bin\Release\net9.0-windows\CorsaCareer.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Eseguibile non trovato: $exe" }

$env:CORSACAREER_HOME = $fresh
$fakeAc = Join-Path $project 'ac-finto'
if (Test-Path -LiteralPath $fakeAc) { $env:CORSACAREER_AC_ROOT = $fakeAc }
$env:CORSACAREER_UI_AUTOMATION = '0'

Start-Process -FilePath $exe -WorkingDirectory (Split-Path -Parent $exe)
Write-Host "Carriera azzerata e avviata da: $fresh" -ForegroundColor Green
