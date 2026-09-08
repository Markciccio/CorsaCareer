# ---------------------------------------------------------------------------
# Lanciatore di CorsaCareer.
#
# Ricompila e poi avvia. Il passaggio di compilazione non e' un dettaglio: le
# immagini vengono lette da "assets" accanto all'eseguibile, non dalla cartella
# del progetto. Avviare l'exe senza ricompilare mostra le tavole della build
# precedente, quindi ogni file aggiunto in assets\ resterebbe invisibile.
#
# Parametri:
#   -Debug          usa la configurazione Debug invece di Release
#   -NuovaCarriera  salvataggio isolato e Assetto Corsa finto (per le prove)
#   -Accantona      mette da parte la carriera attuale e ne comincia una nuova
# ---------------------------------------------------------------------------

param(
    [switch]$Debug,
    [switch]$NuovaCarriera,
    [switch]$Accantona
)

$ErrorActionPreference = 'Stop'
$progetto = $PSScriptRoot
Set-Location $progetto

$configurazione = if ($Debug) { 'Debug' } else { 'Release' }

Write-Host ''
Write-Host 'CORSACAREER - AVVIO' -ForegroundColor Cyan
Write-Host ('-' * 62)

# --- l'SDK ----------------------------------------------------------------
# Il dotnet nel PATH puo' essere il solo runtime, o una cartella SDK vuota:
# va cercato un SDK vero, altrimenti la compilazione fallisce con un messaggio
# che non spiega niente.
function Find-DotnetSdk {
    $candidati = @(
        "$env:LOCALAPPDATA\Microsoft\dotnet",
        "$env:USERPROFILE\.dotnet",
        "$env:ProgramFiles\dotnet"
    )
    foreach ($radice in $candidati) {
        $exe = Join-Path $radice 'dotnet.exe'
        $sdk = Join-Path $radice 'sdk'
        if ((Test-Path -LiteralPath $exe) -and (Test-Path -LiteralPath $sdk)) {
            if (@(Get-ChildItem -LiteralPath $sdk -Directory -ErrorAction SilentlyContinue).Count -gt 0) {
                return $radice
            }
        }
    }
    return $null
}

$sdk = Find-DotnetSdk
if (-not $sdk) {
    Write-Host 'SDK .NET non trovato.' -ForegroundColor Red
    Write-Host 'Installalo da https://dotnet.microsoft.com/download (versione 9).'
    Write-Host ''
    Read-Host 'Premi INVIO per chiudere'
    exit 1
}
$dotnet = Join-Path $sdk 'dotnet.exe'
$env:DOTNET_ROOT = $sdk
$env:PATH = "$sdk;$env:PATH"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

# --- la compilazione ------------------------------------------------------
# Serve anche a copiare in output le immagini nuove: e' il motivo per cui il
# lanciatore compila invece di avviare direttamente l'eseguibile.
$immagini = @(Get-ChildItem -LiteralPath (Join-Path $progetto 'assets') -Filter *.png -Recurse -File -ErrorAction SilentlyContinue).Count
Write-Host "Compilazione $configurazione in corso ($immagini immagini in assets)..."

$log = & $dotnet build (Join-Path $progetto 'CorsaCareer1991.csproj') -c $configurazione -v q --nologo 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "COMPILAZIONE FALLITA - l'applicazione non viene avviata." -ForegroundColor Red
    Write-Host ''
    $log | Select-Object -Last 20 | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkGray }
    Write-Host ''
    Read-Host 'Premi INVIO per chiudere'
    exit 1
}

$exe = Join-Path $progetto "bin\$configurazione\net9.0-windows\CorsaCareer1991.exe"
if (-not (Test-Path -LiteralPath $exe)) {
    Write-Host "Eseguibile non trovato dopo la compilazione:" -ForegroundColor Red
    Write-Host "  $exe"
    Write-Host ''
    Read-Host 'Premi INVIO per chiudere'
    exit 1
}

$copiate = @(Get-ChildItem -LiteralPath (Join-Path (Split-Path $exe) 'assets') -Filter *.png -Recurse -File -ErrorAction SilentlyContinue).Count
Write-Host "Compilazione riuscita - $copiate immagini disponibili all'avvio." -ForegroundColor Green

# --- la modalita' ---------------------------------------------------------
if ($NuovaCarriera) {
    # Ogni avvio di prova riceve una cartella diversa: le carriere precedenti
    # restano intatte e nessun salvataggio reale viene sovrascritto.
    $prova = Join-Path $progetto ("debug-carriera\nuova-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Force -Path $prova | Out-Null
    $env:CORSACAREER_HOME = $prova
    $acFinto = Join-Path $progetto 'ac-finto'
    if (Test-Path -LiteralPath $acFinto) { $env:CORSACAREER_AC_ROOT = $acFinto }
    Write-Host "Carriera di prova isolata in: $prova" -ForegroundColor DarkYellow
}
else {
    # Carriera reale: i salvataggi restano sotto Documenti, come da installer.
    $salvataggi = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Assetto Corsa\CorsaCareer'

    # L'avvio normale RIPRENDE la carriera dove era rimasta.
    #
    # Prima ogni avvio accantonava il salvataggio e ricominciava da un pilota
    # nuovo: in una settimana di prove erano diciotto cartelle
    # "CorsaCareer-precedente-*" per quattrocentocinquanta megabyte, e nessuna
    # di esse veniva mai piu' riaperta. Chi vuole ricominciare ha
    # -NuovaCarriera, che isola la prova in debug-carriera senza toccare
    # niente; chi vuole conservare la carriera attuale prima di cambiarla usa
    # -Accantona.
    if ($Accantona -and (Test-Path -LiteralPath $salvataggi)) {
        $accantonata = "$salvataggi-precedente-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Move-Item -LiteralPath $salvataggi -Destination $accantonata
        Write-Host "Carriera precedente accantonata in: $accantonata" -ForegroundColor DarkYellow
    }
    elseif (Test-Path -LiteralPath (Join-Path $salvataggi 'career.json')) {
        Write-Host 'Si riprende la carriera salvata.' -ForegroundColor DarkGray
    }

    New-Item -ItemType Directory -Force -Path $salvataggi,
        (Join-Path $salvataggi 'media\captures'),
        (Join-Path $salvataggi 'media\manual'),
        (Join-Path $salvataggi 'media\magazine') | Out-Null
    Write-Host "Salvataggi: $salvataggi"

    # Il progetto nasce per chi non ha Assetto Corsa installato ("va simulata
    # la gara"): senza questo, su un PC senza AC il programma non trova auto
    # né circuiti e la carriera reale resta bloccata su "CONTENUTI MANCANTI".
    # Se non c'è già un override esplicito e non risulta un'installazione Steam
    # vera, si usa il catalogo simulato incluso nel progetto — le stesse auto e
    # piste finte con cui gira il collaudo automatico, non un progetto a parte.
    if (-not $env:CORSACAREER_AC_ROOT) {
        $steamCandidati = @(
            'C:\Program Files (x86)\Steam\steamapps\common\assettocorsa',
            (Join-Path $env:ProgramFiles 'Steam\steamapps\common\assettocorsa')
        )
        $acReale = $steamCandidati | Where-Object { Test-Path (Join-Path $_ 'content\cars') }
        if (-not $acReale) {
            $acFinto = Join-Path $progetto 'ac-finto'
            if (Test-Path -LiteralPath $acFinto) {
                $env:CORSACAREER_AC_ROOT = $acFinto
                Write-Host 'Assetto Corsa non trovato: uso il catalogo auto/piste simulato (ac-finto).' -ForegroundColor DarkYellow
            }
        }
    }
}

Write-Host 'Avvio...' -ForegroundColor Cyan
Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe)
Start-Sleep -Seconds 2
