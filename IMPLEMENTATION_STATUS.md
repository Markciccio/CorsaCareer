# CorsaCareer — stato verificabile

## Implementato e verificato localmente

- scansione auto/circuiti installati, categorie, livree, modello e avvisi di lettura;
- fallback per i `ui_car.json` e `ui_track.json` AC multilinea e distinzione tra note informative ed errori di scansione;
- installazione ZIP locale e download HTTPS con SHA-256, staging e controllo percorsi;
- profilo pilota, avatar, data narrativa, progressione e rookie evaluation pesata su arrivo, qualifica, compagno e affidabilità del referto;
- scuderie/compagni/piloti AI persistenti, contratti, sponsor, premi, stipendi e mercato;
- campionati generati dai circuiti disponibili, promozioni e storico stagioni;
- preset Quick Drive Content Manager con auto, circuito e griglia controllati;
- locator e parser `race_out.json` con identità pilota, qualifica, griglia, DNF, telemetria e rifiuto dei file incompleti;
- associazione pendente, hash anti-duplicazione e chiusura transazionale del weekend;
- media center a sequenze con filtri editoriali, archivio, articoli, giornale, screenshot/foto e fonti dichiarate;
- servizi audio locali, catalogo delle voci italiane Windows, scelta voce, esportazione WAV, profili redazione/commentatore e guardia contro dati inventati;
- salvataggi atomici, autosave, backup versionati, slot multipli e ripristino;
- build self-contained Windows e installer PowerShell locale.
- pianificatore di sessione (`SessionPlanner`): distanza di gara dalla lunghezza reale del circuito, meteo limitato ai contenuti installati e pesato sul mese narrativo, orario/temperatura/vento variabili entro i limiti di Assetto Corsa, round di durata, livello e aggressività AI per categoria e profilo, calibrazione AI dai soli referti importati.

## Verifica che richiede l’utente

La suite automatica usa fixture per parser, scanner, contenuti, media e persistenza. Il test live di `race_out.json` resta intenzionalmente `SKIP` finché non viene conclusa una gara reale in Assetto Corsa tramite Content Manager. Solo quel referto può assegnare punti, premi, sponsor o avanzare il campionato.

## Regola di integrità

Nessun risultato viene simulato o inserito manualmente per far avanzare la carriera. Se mancano auto, circuito, griglia, preset o referto verificabile, l’app mostra il motivo e lascia la carriera in attesa.
## Aggiornamento 21 agosto 2026 — profilo di cronaca da trascrizione

- Acquisita una trascrizione storica fornita dall'utente come riferimento editoriale.
- Aggiunto lo stile selezionabile `Cronaca classica analitica`: sequenza tempi/distacchi, lettura tecnica, quindi aggiornamento del box.
- Aggiunti anche `Paddock e interviste` e `Redazione moderna` nel pannello Redazione.
- Il testo generato resta originale e basato sui dati verificati della gara; non copia frasi, voce o manierismi di telecronisti reali.
## Aggiornamento 21 agosto 2026 — portale-diario home

- La home mostra un diario di carriera con round corrente, risultati, classifica, stato tecnico ed eventi futuri/passati.
- Aggiunta galleria di fotografie locali (briefing, test, pioggia, podio) con apertura a clic.
- Aggiunta colonna `Articoli e servizi` con collegamenti rapidi a storia, ultimo servizio, test, mercato, classifiche e archivio.
- La pagina browser resta l'edizione digitale dettagliata; il pulsante principale la apre con un vero articolo narrativo generato dal percorso del pilota.
## Aggiornamento 21 agosto 2026 — rookie evaluation e percorso di ingresso

- Un nuovo profilo non riceve più automaticamente una squadra o un contratto.
- Il manager assegna una prima vettura installata, un circuito e un tempo-obiettivo.
- Il risultato reale del test aggiorna tentativi, miglior giro e stato della valutazione.
- Tempo sufficiente: offerte iniziali e passaggio alla carriera attiva; tempo vicino: altro test; tempo insufficiente: valutazione ancora aperta.
## Aggiornamento — formato reale del weekend (Blocco A)

- Rimossi i valori cablati del preset: la gara non è più sempre di 5 giri, sereno, alle 14:30, con AI 95 e aggressività 0.
- `SessionPlanner` è un modulo puro e deterministico: stessa carriera e stesso round producono sempre le stesse condizioni, quindi il weekend è riproducibile e coperto da fixture.
- `ContentScanner` legge lunghezza e box del circuito (anche dai layout `ui/<layout>/ui_track.json`, normalizzando km/virgole/metri) e i preset meteo installati in `content/weather`. Indice contenuti allo schema 4.
- `ContentManagerPresetBuilder` è l'unica sorgente del preset Quick Drive: elimina la duplicazione fra weekend e test e rende il preset verificabile senza aprire la UI.
- Il preset del test passa ora dallo stesso validatore del weekend.
- Carriera allo schema 9 (poi 10 con i blocchi successivi) con `DifficultyProfile`, `DistanceProfile`, `AutoCalibrateAi`, `AiCalibrationOffset` e archivio `SessionPlans`; le condizioni usate entrano anche in `RaceHistoryEntry` e `TestSessionRecord` come fatti verificabili.
- `AiCalibration` corregge solo il livello AI della sessione successiva, sui referti reali già importati, con limiti −10/+6. Non è rubber banding: nessun risultato archiviato viene modificato.
- Non implementato in questo blocco: Gara 2 (richiede due risultati per round nella macchina a stati del weekend pendente) e pioggia (assente in Assetto Corsa vanilla, dipende da CSP/Sol).
- Suite `SessionPlannerCheck` in `tests/ParserCheck`: 15 gruppi di controlli su distanza, profili, fallback dichiarati, meteo stagionale, determinismo, limiti orario/temperatura, AI, calibrazione, round di durata, test senza gara, coerenza del preset e scansione dei nuovi metadati.
- Verificato in esecuzione: l'SDK .NET è stato installato in locale e la suite completa passa (vedi la sezione finale).

## Aggiornamento — blocchi B–H

Riepilogo sintetico; il documento completo di consegna è [HANDOFF.md](HANDOFF.md).

- **B** — motore di carriera estratto in moduli puri e testati (`ChampionshipRegulations`,
  `ReputationEngine`, `EconomyEngine`, `ProgressionEngine`); punteggi per categoria con
  pole e giro veloce; reputazione da posizione relativa, aspettativa, compagno e categoria;
  danno del referto tradotto in costo di riparazione; premio finale dalla classifica.
  Corretti due bug: le date del calendario che si spostavano dopo ogni gara e l'anello dei
  backup consumato dai salvataggi a vuoto dell'avvio.
- **C** — tempo-obiettivo della rookie evaluation derivato da lunghezza e tipo del
  tracciato e dalle prestazioni della vettura, con precedenza a un tempo realmente girato;
  soglia "vicino" relativa e non più di dieci secondi assoluti.
- **D** — vita fuori dalla pista: sedici attività fra i weekend con costo, giorni, rischio
  dichiarato, due esiti e conseguenze su reputazione, seguito, rapporti, budget e
  condizione. Nessuna può influenzare un risultato di gara, ed esiste un test che lo
  verifica strutturalmente.
- **E** — log su file ruotato, perdita di handle GDI corretta, magazine non più rigenerato
  a ogni autosave.
- **F** — UI ricostruita come portale motorsport: design system, layout impaginato da
  contenitori, `PerMonitorV2`, verificata a 1366×768, 1600×900 e 1920×1080.
- **G** — motore narrativo: fatti verificati, impatto editoriale, angolazioni selezionate
  dai dati, banca di formulazioni con memoria anti-ripetizione e continuità garantita con
  il passato della carriera. Rimossi il nome del pilota cablato e la biografia inventata.
- **H** — pipeline foto con provenienza sempre dichiarata (screenshot reale, import utente,
  illustrazione AI, asset del pacchetto, assenza) e hook per un provider di immagini
  esterno. Nessun download di fotografie dal web da presentare come documenti dell'evento.

### Verifica eseguita

`dotnet build -c Release` e `dotnet run --project tests\ParserCheck\ParserCheck.csproj -c Release`
sono stati eseguiti: build senza errori né avvisi, suite completa verde. Il test live su
`race_out.json` resta `SKIP` in attesa di una gara reale conclusa dall'utente.
