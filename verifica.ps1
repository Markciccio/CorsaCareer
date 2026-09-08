# ---------------------------------------------------------------------------
# Collaudo completo di CorsaCareer.
#
# Un solo comando che verifica tutto, da eseguire dopo ogni modifica:
#
#     .\verifica.ps1
#
# Controlla in ordine:
#   1. che l'SDK .NET sia raggiungibile;
#   2. che l'applicazione compili senza errori ne avvisi;
#   3. il collaudo di base e tutte le suite di test (parser, motori, carriera);
#   4. che l'interfaccia si disegni alle tre risoluzioni previste;
#   5. che il ciclo di carriera arrivi dalle prime gare alle prime vittorie.
#
# Esce con 0 solo se tutto passa. Qualunque valore diverso da 0 e un problema
# reale da leggere nell'output, non un falso allarme da ignorare.
#
# Parametri:
#   -Rapido   salta l'interfaccia e la simulazione (solo compilazione e test)
# ---------------------------------------------------------------------------

param([switch]$Rapido)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

# --- 1. l'SDK -------------------------------------------------------------
# Il percorso dell'SDK e cambiato fra una sessione e l'altra: va cercato, non
# dato per scontato. Il dotnet in "Program Files" puo essere solo runtime.
function Find-DotnetSdk {
    $candidates = @(
        "$env:LOCALAPPDATA\Microsoft\dotnet",
        "$env:USERPROFILE\.dotnet",
        "$env:ProgramFiles\dotnet"
    )
    foreach ($root in $candidates) {
        if ((Test-Path (Join-Path $root 'dotnet.exe')) -and (Test-Path (Join-Path $root 'sdk'))) {
            if ((Get-ChildItem (Join-Path $root 'sdk') -Directory -ErrorAction SilentlyContinue).Count -gt 0) { return $root }
        }
    }
    return $null
}

$sdk = Find-DotnetSdk
if (-not $sdk) {
    Write-Host "ESITO: SDK .NET non trovato." -ForegroundColor Red
    Write-Host "Installalo con: .\CorsaCareer-Install.ps1  (oppure https://dotnet.microsoft.com/download)"
    exit 1
}
$env:DOTNET_ROOT = $sdk
$env:PATH = "$sdk;$env:PATH"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

$sdkVersion = (Get-ChildItem (Join-Path $sdk 'sdk') -Directory | Select-Object -Last 1).Name
Write-Host ""
Write-Host "CORSACAREER1991 - COLLAUDO COMPLETO" -ForegroundColor Cyan
Write-Host ("-" * 74)
Write-Host "SDK .NET $sdkVersion in $sdk"
Write-Host ""

$fasi = [System.Collections.Generic.List[object]]::new()
$iniziato = Get-Date

function Fase {
    param([string]$Nome, [scriptblock]$Azione, [switch]$Facoltativa)

    Write-Host "► $Nome" -ForegroundColor White
    $t0 = Get-Date
    $uscita = 1
    $output = @()
    try {
        $output = & $Azione 2>&1
        $uscita = $LASTEXITCODE
        if ($null -eq $uscita) { $uscita = 0 }
    }
    catch {
        $output = @($_.Exception.Message)
        $uscita = 1
    }
    $durata = ((Get-Date) - $t0).TotalSeconds

    if ($uscita -eq 0) {
        Write-Host ("  passata in {0:N1}s" -f $durata) -ForegroundColor Green
    }
    elseif ($Facoltativa) {
        Write-Host ("  saltata ({0:N1}s) - non blocca il collaudo" -f $durata) -ForegroundColor DarkYellow
    }
    else {
        Write-Host ("  FALLITA in {0:N1}s" -f $durata) -ForegroundColor Red
        $output | Select-Object -Last 25 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    }

    $fasi.Add([pscustomobject]@{
        Nome = $Nome; Uscita = $uscita; Secondi = $durata
        Facoltativa = [bool]$Facoltativa; Output = $output
    })
    Write-Host ""
    return $output
}

# --- 2. la compilazione ---------------------------------------------------
# Zero errori e zero avvisi in entrambe le configurazioni: il launcher desktop
# usa Release, mentre lo sviluppo quotidiano usa Debug.
Fase "Compilazione applicazione Debug + Release (0 errori, 0 avvisi)" {
    $debugOut = dotnet build CorsaCareer.csproj -v q --nologo 2>&1
    $debugExit = $LASTEXITCODE
    $releaseOut = dotnet build CorsaCareer.csproj -c Release -v q --nologo 2>&1
    $releaseExit = $LASTEXITCODE
    $debugOut
    $releaseOut
    $avvisi = @($debugOut + $releaseOut) | Select-String -Pattern ': warning '
    # La compilazione Debug serve a dimostrare che compila, non a conservare
    # una seconda copia delle tavole: sono ottocento immagini per quasi due
    # gigabyte, ricopiate a ogni collaudo accanto all'eseguibile di prova. Il
    # portale gira in Release, ed e' quella la copia che serve.
    $copiaDebug = Join-Path $PSScriptRoot (Join-Path 'bin' (Join-Path 'Debug' (Join-Path 'net9.0-windows' 'assets')))
    if (Test-Path -LiteralPath $copiaDebug) { Remove-Item -LiteralPath $copiaDebug -Recurse -Force -ErrorAction SilentlyContinue }
    $global:LASTEXITCODE = if ($debugExit -ne 0 -or $releaseExit -ne 0 -or $avvisi) { 1 } else { 0 }
} | Out-Null

# --- 3. i test ------------------------------------------------------------
$parserProject = Join-Path $PSScriptRoot 'tests\ParserCheck\ParserCheck.csproj'
$testOutput = if (Test-Path $parserProject) {
    Fase "Test: collaudo di base, parser referti, motori, ciclo di carriera" {
        dotnet run --project $parserProject -v q 2>&1
    }
} else {
    Fase "Test: collaudo di base, parser referti, motori, ciclo di carriera" -Facoltativa {
        Write-Host 'Suite sorgente assente: eseguire il test quando viene ripristinata.'
        $global:LASTEXITCODE = 1
    }
}

# --- 3b. il percorso di carriera -----------------------------------------
# Segue la catena valutazione -> firma -> campionato -> sponsor -> gare e
# dichiara quale anello manca. Serve a intercettare gli stati senza uscita,
# come un contratto attivo che non apre nessun calendario.
$percorsoProject = Join-Path $PSScriptRoot 'tests\PercorsoCheck\PercorsoCheck.csproj'
if (Test-Path $percorsoProject) {
    Fase "Percorso: dalla valutazione alla fine della stagione" {
        dotnet run --project $percorsoProject -v q 2>&1
    } | Out-Null
} else {
    Fase "Percorso: dalla valutazione alla fine della stagione" -Facoltativa {
        Write-Host 'Suite sorgente assente: percorso non eseguito.'
        $global:LASTEXITCODE = 1
    } | Out-Null
}

$verificheOk = ($testOutput | Select-String -Pattern '^OK ' | Measure-Object).Count
$saltate     = 0
$baseRiga    = $testOutput | Select-String -Pattern 'collaudo di base' | Select-Object -First 1

if (-not $Rapido) {
    # --- 4. l'interfaccia -------------------------------------------------
    # Disegna la finestra fuori schermo alle risoluzioni previste. Non cattura
    # mai lo schermo reale.
    $uiProject = Join-Path $PSScriptRoot 'tests\UiRender\UiRender.csproj'
    if (Test-Path $uiProject) {
        Fase "Interfaccia: disegno a 1366x768, 1600x900, 1920x1080" {
            dotnet run --project $uiProject -v q 2>&1
        } | Out-Null
    } else {
        Fase "Interfaccia: disegno a 1366x768, 1600x900, 1920x1080" -Facoltativa {
            Write-Host 'Suite sorgente assente: rendering non eseguito.'
            $global:LASTEXITCODE = 1
        } | Out-Null
    }

    # --- la firma ---------------------------------------------------------
    # Il difetto piu grave riscontrato giocando: si accetta un sedile e non
    # parte nessuna gara. Vive dentro la UI, quindi va provato aprendo la
    # finestra vera e chiamando il codice di produzione.
    $signProject = Join-Path $PSScriptRoot 'tests\SignCheck\SignCheck.csproj'
    if (Test-Path $signProject) {
        Fase "Firma: un sedile accettato deve produrre gare" {
            dotnet run --project $signProject -v q 2>&1
        } | Out-Null
    } else {
        Fase "Firma: un sedile accettato deve produrre gare" -Facoltativa {
            Write-Host 'Suite sorgente assente: firma non eseguita.'
            $global:LASTEXITCODE = 1
        } | Out-Null
    }

    # --- 5. la carriera ---------------------------------------------------
    # Il ciclo completo deve arrivare alle prime vittorie: se si blocca, e un
    # difetto dei motori, non della fortuna.
    $simProject = Join-Path $PSScriptRoot 'tests\CareerSim\CareerSim.csproj'
    $simOutput = if (Test-Path $simProject) {
        Fase "Carriera: dal debutto alle prime vittorie" {
            dotnet run --project $simProject -v q 2>&1
        }
    } else {
        Fase "Carriera: dal debutto alle prime vittorie" -Facoltativa {
            Write-Host 'Suite sorgente assente: simulazione non eseguita.'
            $global:LASTEXITCODE = 1
        }
    }
    $bilancio = $simOutput | Select-String -Pattern '^Gare \d+ (·|-) vittorie' | Select-Object -First 1
    if ($bilancio) {
        $vittorie = [int]([regex]::Match($bilancio.Line, 'vittorie (\d+)').Groups[1].Value)
        if ($vittorie -lt 1) {
            Write-Host "  ATTENZIONE: la simulazione non arriva a nessuna vittoria." -ForegroundColor Yellow
            Write-Host "  $($bilancio.Line)" -ForegroundColor Yellow
            Write-Host "  E il sintomo di un blocco nel ciclo delle opportunita." -ForegroundColor Yellow
            Write-Host ""
            $fasi[-1].Uscita = 1
        }
    }
}

# --- esito ----------------------------------------------------------------
$falliti = $fasi | Where-Object { $_.Uscita -ne 0 -and -not $_.Facoltativa }
$saltate = @($fasi | Where-Object { $_.Facoltativa }).Count
$totale = ((Get-Date) - $iniziato).TotalSeconds

Write-Host ("-" * 74)
foreach ($f in $fasi) {
    $esito = if ($f.Uscita -eq 0) { "OK     " } elseif ($f.Facoltativa) { "saltata" } else { "FALLITA" }
    $colore = if ($f.Uscita -eq 0) { 'Green' } elseif ($f.Facoltativa) { 'DarkYellow' } else { 'Red' }
    Write-Host ("  {0}  {1}" -f $esito, $f.Nome) -ForegroundColor $colore
}
Write-Host ("-" * 74)
if ($baseRiga) { Write-Host "  $($baseRiga.Line.TrimStart('O','K',' '))" }
Write-Host "  $verificheOk gruppi di verifiche superati, $saltate saltati"
Write-Host ("  tempo totale {0:N1}s" -f $totale)
Write-Host ""

if ($falliti) {
    Write-Host "ESITO: NON SUPERATO - $($falliti.Count) fase/i da correggere." -ForegroundColor Red
    exit 1
}
if ($saltate -gt 0) {
    Write-Host "ESITO: VERIFICA PARZIALE - $saltate suite non eseguite (sorgenti mancanti)." -ForegroundColor Yellow
    Write-Host "La compilazione è pulita, ma il collaudo non può dichiarare tutto a posto finché le suite non vengono ripristinate." -ForegroundColor Yellow
    exit 2
}
Write-Host "ESITO: TUTTO A POSTO." -ForegroundColor Green
exit 0
