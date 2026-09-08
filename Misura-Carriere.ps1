# ---------------------------------------------------------------------------
# Banco di misura delle carriere.
#
# Esegue piu' carriere complete con piloti diversi e stampa, per ognuna, la
# forma della carriera: quante gare per categoria, quanto ci si e' fermati,
# quante vittorie, quanti ritiri, dove si e' arrivati. Serve a capire se i pesi
# scelti producono carriere plausibili, e a confrontarle fra loro.
#
#     .\Misura-Carriere.ps1                 # 6 piloti sul catalogo reale
#     .\Misura-Carriere.ps1 -Piloti 10      # campione piu' ampio
#     .\Misura-Carriere.ps1 -Catalogo ac-finto
# ---------------------------------------------------------------------------

param(
    [int]$Piloti = 6,
    [string]$Catalogo = 'ac-vero'
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Find-DotnetSdk {
    foreach ($root in @("$env:LOCALAPPDATA\Microsoft\dotnet", "$env:USERPROFILE\.dotnet", "$env:ProgramFiles\dotnet")) {
        if ((Test-Path (Join-Path $root 'dotnet.exe')) -and (Test-Path (Join-Path $root 'sdk')) -and
            @(Get-ChildItem (Join-Path $root 'sdk') -Directory -ErrorAction SilentlyContinue).Count -gt 0) { return $root }
    }
    return $null
}
$sdk = Find-DotnetSdk
if (-not $sdk) { throw 'SDK .NET non trovato.' }
$env:DOTNET_ROOT = $sdk; $env:PATH = "$sdk;$env:PATH"
$dotnet = Join-Path $sdk 'dotnet.exe'

$env:CORSACAREER_FIXTURE = $Catalogo
& $dotnet build (Join-Path $PSScriptRoot 'CorsaCareer1991.csproj') -c Debug -v q --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Compilazione fallita.' }

# Nomi diversi: le doti e tutti i motori derivano dal nome, quindi ogni pilota
# produce una carriera diversa. Restano stabili fra un'esecuzione e l'altra,
# cosi due misure dello stesso codice sono confrontabili.
$nomi = @('Akira','Beniamino','Chiara','Daisuke','Emi','Fabio','Goro','Hana','Ippei','Jun',
          'Kenzo','Luca','Mika','Nao','Osamu','Paolo','Quinto','Ryo','Sora','Taro')

Write-Host ''
Write-Host ("BANCO DI MISURA - {0} carriere sul catalogo {1}" -f $Piloti, $Catalogo) -ForegroundColor Cyan
Write-Host ('=' * 100)

$riepilogo = @()

for ($i = 0; $i -lt $Piloti; $i++) {
    $nome = $nomi[$i % $nomi.Count]
    $env:CORSACAREER_DEMO_DRIVER = $nome
    $prima = Get-ChildItem $env:TEMP -Directory -Filter 'corsacareer-debug-*' -ErrorAction SilentlyContinue

    & $dotnet run --project (Join-Path $PSScriptRoot 'tests\DebugRun\DebugRun.csproj') -c Debug 2>&1 | Out-Null

    $cartella = Get-ChildItem $env:TEMP -Directory -Filter 'corsacareer-debug-*' -ErrorAction SilentlyContinue |
        Where-Object { $prima.FullName -notcontains $_.FullName } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $cartella) { Write-Host "  ($nome) nessun salvataggio prodotto" -ForegroundColor DarkYellow; continue }

    $file = Join-Path $cartella.FullName 'career.json'
    if (-not (Test-Path $file)) { Write-Host "  ($nome) career.json assente" -ForegroundColor DarkYellow; continue }
    $c = Get-Content -Raw $file | ConvertFrom-Json

    $gare = @($c.RaceHistory)
    if ($gare.Count -eq 0) { Write-Host "  ($nome) nessuna gara" -ForegroundColor DarkYellow; continue }
    $ord = $gare | Sort-Object { [datetime]$_.StoryDate }
    $da = [datetime]$ord[0].StoryDate; $al = [datetime]$ord[-1].StoryDate
    $anni = [math]::Round(($al - $da).TotalDays / 365.25, 1)
    $ritiri = @($gare | Where-Object { $_.Dnf }).Count

    Write-Host ''
    Write-Host ("PILOTA: {0} Driver  -  {1}" -f $nome, $c.Talent.Archetype) -ForegroundColor Yellow
    Write-Host ("  doti: velocita {0} | costanza {1} | bagnato {2} | duelli {3} | adattamento {4} | affidabilita {5}" -f `
        $c.Talent.RawPace, $c.Talent.Consistency, $c.Talent.WetSkill, $c.Talent.Racecraft, $c.Talent.Adaptability, $c.Talent.Reliability)
    Write-Host ("  carriera: {0} gare | {1} vittorie | {2} podi | {3} ritiri | {4} anni ({5:MMM yy} - {6:MMM yy})" -f `
        $gare.Count, $c.Wins, $c.Podiums, $ritiri, $anni, $da, $al)
    Write-Host ("  arrivato a: {0} (livello {1}/5) | categoria {2}" -f $c.Championship, $c.ChampionshipLevel, $c.Tier)

    # Quante volte si e' cambiata vettura: un valore alto significa che la
    # carriera oscilla invece di salire.
    $cambi = 0; $prec = ''
    foreach ($g in $ord) { if ($g.Car -ne $prec) { $cambi++; $prec = $g.Car } }

    Write-Host "  fasi:"
    $gare | Group-Object Car | Sort-Object { [datetime](($_.Group | Sort-Object { [datetime]$_.StoryDate })[0]).StoryDate } | ForEach-Object {
        $g = $_.Group | Sort-Object { [datetime]$_.StoryDate }
        $v = @($g | Where-Object { -not $_.Dnf -and $_.Position -eq 1 }).Count
        $pod = @($g | Where-Object { -not $_.Dnf -and $_.Position -le 3 }).Count
        $arrivati = @($g | Where-Object { -not $_.Dnf })
        $med = if ($arrivati.Count -gt 0) { [math]::Round(($arrivati | Measure-Object Position -Average).Average, 1) } else { 0 }
        $ia = [math]::Round(($g | Measure-Object AiLevel -Average).Average, 0)
        $mesi = [math]::Round((([datetime]$g[-1].StoryDate) - ([datetime]$g[0].StoryDate)).TotalDays / 30.4, 0)
        Write-Host ("    {0,-20} {1,3} gare | {2,2} vitt | {3,2} podi | pos.med {4,4} | IA {5,3} | {6,2} mesi | {7:MMM yy}-{8:MMM yy}" -f `
            $_.Name, $g.Count, $v, $pod, $med, $ia, $mesi, ([datetime]$g[0].StoryDate), ([datetime]$g[-1].StoryDate))
    }
    Write-Host ("  cambi di vettura: {0}  (alto = la carriera oscilla invece di salire)" -f $cambi)

    $riepilogo += [pscustomobject]@{
        Pilota = $nome; Archetipo = $c.Talent.Archetype; Gare = $gare.Count; Vittorie = $c.Wins
        Ritiri = $ritiri; Anni = $anni; Livello = $c.ChampionshipLevel; Cambi = $cambi
        Categorie = @($gare | Group-Object Car).Count
    }
}

Write-Host ''
Write-Host ('=' * 100)
Write-Host 'RIEPILOGO' -ForegroundColor Cyan
$riepilogo | Format-Table -AutoSize
if ($riepilogo.Count -gt 0) {
    Write-Host ("medie: {0:N0} gare | {1:N1} vittorie | {2:N1} ritiri | {3:N1} anni | {4:N1} categorie | {5:N1} cambi vettura" -f `
        ($riepilogo | Measure-Object Gare -Average).Average,
        ($riepilogo | Measure-Object Vittorie -Average).Average,
        ($riepilogo | Measure-Object Ritiri -Average).Average,
        ($riepilogo | Measure-Object Anni -Average).Average,
        ($riepilogo | Measure-Object Categorie -Average).Average,
        ($riepilogo | Measure-Object Cambi -Average).Average)
}
