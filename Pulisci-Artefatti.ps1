param(
    # I salvataggi restano intatti per impostazione predefinita.
    [switch]$IncludeSaves
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
# Solo materiale rigenerabile: sorgenti, assets, soundtrack, narration e
# salvataggi non vengono toccati. I binari dei test sono copie rigenerabili e
# possono contenere vecchie liste di assets (inclusa la rimossa alternatives).
$folders = @('bin','obj','publish','ui-preview','ui-preview-advance-fixed','ui-preview-debug','ui-preview-passo','ui-preview-selezione','debug-screens','debug-screens-current','avanza-screens','percorso','1','--nologo')
if ($IncludeSaves) { $folders += 'debug-carriera' }
foreach ($name in $folders) {
    $path = Join-Path $root $name
    if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Recurse -Force }
}
$testRoot = Join-Path $root 'tests'
if (Test-Path -LiteralPath $testRoot) {
    Get-ChildItem -LiteralPath $testRoot -Recurse -File -Include *.png,*.mp3 | Remove-Item -Force
}
Get-ChildItem -LiteralPath $root -File -Filter *.png | Remove-Item -Force
if ($IncludeSaves) {
    Write-Host 'Pulizia completata, inclusi i salvataggi debug. Il prossimo avvio ricreera bin/obj e le anteprime.' -ForegroundColor Green
} else {
    Write-Host 'Pulizia completata: salvataggi e asset canonici preservati. Il prossimo avvio ricreera bin/obj e le anteprime.' -ForegroundColor Green
}
