# CorsaCareer — consegna per la prossima iterazione

Questo documento descrive cosa è stato cambiato, come verificarlo e cosa resta
aperto. È scritto per essere passato direttamente a un altro agente (codex) come
punto di partenza.

## Come verificare tutto: un solo comando

```powershell
.erifica.ps1
```

Va eseguito dopo ogni modifica. Cerca l'SDK da solo (il percorso è già cambiato
fra due sessioni, quindi non va dato per scontato) ed esegue in ordine:

| Fase | Cosa controlla | Blocca? |
|---|---|---|
| Compilazione | l'applicazione compila con **0 errori e 0 avvisi** — un avviso nuovo è un difetto non ancora manifestato | sì |
| Test | collaudo di base + tutte le suite: parser dei referti, motori, ciclo di carriera | sì |
| Interfaccia | la finestra si disegna a 1366×768, 1600×900, 1920×1080 (fuori schermo, non cattura mai lo schermo reale) | sì |
| Carriera | il ciclo arriva davvero dal debutto alle prime vittorie; se resta a zero vittorie segnala un blocco nelle opportunità | sì |

Esce con `0` solo se tutto passa. `.erifica.ps1 -Rapido` salta interfaccia e
simulazione quando serve solo un controllo veloce.

Il collaudo è stato verificato **anche in negativo**: riportando il capitale
iniziale a 250.000 € falliva indicando il punto esatto. Un collaudo che non sa
fallire non serve.

Nota sulla compilazione incrementale: se si ripristina un file con `Copy-Item`,
la data di modifica originale viene ripristinata con esso e il binario vecchio
viene riusato. In quel caso `(Get-Item file.cs).LastWriteTime = Get-Date`.

### Il collaudo di base

`tests/ParserCheck/BaseCheck.cs` — 59 verifiche sui cinque invarianti che
tengono in piedi tutto il resto, eseguite per prime:

1. una carriera nuova nasce coerente (povera, senza contratto, senza risultati);
2. lo stato si salva e si rilegge identico, sei dimensioni di reputazione incluse;
3. **nessun risultato entra in carriera senza un referto reale di Assetto Corsa** —
   referto vuoto, corrotto, senza classifica o di sola prova non producono mai un
   piazzamento, e un abbandono non è mai un piazzamento;
4. i motori sono deterministici: stesso stato, stesse proposte, stessi costi;
5. il denaro resta un vincolo (la cassa non va sotto zero di nascosto) e ogni
   proposta dichiara chi la propone, perché esiste e quando scade.

## Come compilare e verificare a mano

L'SDK .NET non era installato sulla macchina: è stato installato **in locale**,
senza privilegi di amministratore e senza modificare il PATH di sistema.

```powershell
$env:DOTNET_ROOT="$env:LOCALAPPDATA\Microsoft\dotnet"
$env:PATH="$env:DOTNET_ROOT;$env:PATH"

dotnet build CorsaCareer1991.csproj -c Release
dotnet run --project tests\ParserCheck\ParserCheck.csproj -c Release
```

`ParserCheck` è la suite completa: esce con codice 0 se tutto passa. Il test live
su `race_out.json` resta `SKIP` finché non viene conclusa una gara reale.

Per rileggere alcuni articoli generati per intero:

```powershell
$env:CORSACAREER_DUMP_ARTICLES="1"
dotnet run --project tests\ParserCheck\ParserCheck.csproj -c Release
```

Per verificare il layout della UI senza catturare lo schermo (rende la finestra
fuori schermo in un bitmap, a tre risoluzioni):

```powershell
dotnet run --project tests\UiRender\UiRender.csproj -c Release -- .\ui-preview
```

## Architettura dopo il refactoring

La logica di carriera è stata estratta da `MainForm` in moduli puri, senza
dipendenze da WinForms, tutti coperti da fixture.

| Modulo | Responsabilità |
|---|---|
| `SessionPlanner` | Formato reale del weekend: distanza, meteo, orario, temperatura, AI, round di durata |
| `ContentManagerPresetBuilder` | Unica sorgente del preset Quick Drive (gara e test) |
| `ChampionshipRegulations` | Sistemi di punteggio per categoria, pole e giro veloce |
| `ReputationEngine` | Variazione di reputazione da posizione, aspettativa, compagno, qualifica, categoria |
| `EconomyEngine` | Premi, costi di riparazione e trasferta, premio finale di campionato |
| `ProgressionEngine` | Valutazione multi-fattore della promozione e bivio di fine stagione |
| `RookieTargetEngine` | Tempo-obiettivo della valutazione da pista e vettura |
| `OffTrackActivities` | Attività fra i weekend con costo, rischio, esito e conseguenze |
| `NarrativeEngine` + `StoryFacts` + `StoryAngles` + `PhraseBank` | Generazione editoriale non ripetitiva |
| `ArticlePhotoService` | Selezione e dichiarazione della provenienza delle immagini |
| `NarrativeCalendar` | Date di campionato ancorate all'inizio stagione |
| `UiTheme` + `MainFormPortal` | Design system e portale |
| `CareerLog` | Log su file ruotato |

`MainForm` è ora `partial`: `Program.cs` contiene la logica di carriera,
`MainFormPortal.cs` la UI.

## Cosa è cambiato, blocco per blocco

### A — Formato reale del weekend

Prima ogni gara della carriera era identica: `LapsNumber = 5`, `WeatherId = "3_clear"`,
`Time = 52200`, `Temperature = 24.1`, `AiLevel = 95`, `AiAggression = 0`.

Ora la distanza deriva dalla lunghezza reale del circuito verso un obiettivo in
km per categoria; il meteo viene scelto **solo** fra i preset installati in
`content/weather`, pesato sul mese narrativo; orario, temperatura e vento variano
per round entro i limiti di Assetto Corsa vanilla (08:00–18:00); livello e
aggressività AI dipendono da categoria e profilo scelto; una stagione con almeno
quattro round e una categoria adatta assegna una gara di durata al circuito più
lungo. Tutto deterministico: nessun `Random`, stessa carriera e stesso round
producono sempre le stesse condizioni.

`ContentScanner` legge ora `length` e `pitboxes` (anche dai layout
`ui/<layout>/ui_track.json`, normalizzando km/virgole/metri) e indicizza i preset
meteo. Indice contenuti allo **schema 4**.

### B — Motore di carriera estratto e due bug corretti

- Punteggi per categoria invece del solo 10-6-4-3-2-1: storico, moderno FIA,
  turismo, karting; pole e giro veloce dove il regolamento li prevede. Giocatore
  e piloti AI usano **lo stesso** regolamento (prima divergevano).
- Reputazione da posizione relativa al gruppo, aspettativa del sedile, confronto
  col compagno, qualifica e categoria, con rendimenti decrescenti. Prima era
  `+8` alla vittoria e `+2` per qualunque altro arrivo.
- Economia: premi scalati per categoria, posizione e dimensione del gruppo; il
  danno letto dal referto genera un costo di riparazione (prima era parsato e
  ignorato); costi di trasferta; premio finale sulla posizione in classifica
  invece che su soglie di punti assolute.
- Promozione multi-fattore con pagella spiegata voce per voce e **bivio reale**
  (restare da prima guida o salire come seconda guida), invece dell'assegnazione
  d'ufficio di squadra, vettura e compagno.
- **Bug corretto — calendario che si riscriveva:** le date dei round erano
  calcolate da `StoryDate`, che avanza a ogni gara; l'intero calendario, comprese
  le date dei round già disputati, si spostava dopo ogni risultato. L'ancora è
  ora `CareerState.SeasonStartDate`, che cambia solo al passaggio di stagione.
- **Bug corretto — backup che si autodistruggevano:** l'allineamento all'avvio
  chiamava `SaveCareer()` anche senza modifiche, bruciando due copie dell'anello
  di venti backup a ogni lancio. `SaveCareer` confronta ora l'hash del contenuto
  e non fa nulla se la carriera non è cambiata.

### C — Rookie evaluation reale

Il tempo-obiettivo era la costante `125000` (2:05.000), mai ricalcolata: valeva
per un kartodromo da 900 m e per il Nordschleife. Ora deriva da lunghezza e tipo
del tracciato e dalle prestazioni dichiarate della vettura, con correzione dal
rapporto peso/potenza; un tempo realmente girato su quella combinazione ha la
precedenza sulla stima. La soglia "vicino" è relativa (3%) e non più dieci
secondi assoluti. Le stime sono tarate entro il 12% dei tempi reali di
riferimento (Monza GT3 ≈ 1:48, formula ≈ 1:23).

### D — Vita fuori dalla pista

Sezione dello spec che non aveva alcuna controparte nel codice. Sedici attività
(pubblico, sponsor, media, tecnica, preparazione, carriera) con costo, durata in
giorni, rischio dichiarato, due esiti possibili e conseguenze su reputazione,
seguito, rapporto col team, rapporto con lo sponsor, budget e condizione fisica.

**Vincolo di integrità:** nessuna attività può influenzare un risultato di gara.
`ActivityEffect` espone esattamente sei campi e un test verifica che non ne
compaiano altri, proprio per impedire che in futuro si apra un canale verso i
risultati. L'agenda incide su mercato, contratti ed economia, non sulla guida.

### E — Robustezza

`CareerLog` scrive su file ruotato in `logs\corsacareer.log` (prima gli errori
finivano in `Debug.WriteLine`, invisibili in Release). Corretta la perdita di
handle GDI in `RefreshUi` (l'immagine precedente non veniva rilasciata). Il
magazine non viene più rigenerato a ogni autosave.

### F — UI da portale motorsport

La UI era a coordinate assolute su una finestra 1280×820 con pannelli alti 850:
su schermi più piccoli o con scaling DPI diverso da 100% i contenuti venivano
tagliati. Ora esiste un design system (`UiTheme`) e il layout è impaginato da
`TableLayoutPanel`/`Dock`, con `ApplicationHighDpiMode = PerMonitorV2`.

Struttura: testata su due righe, striscia dei dati di carriera, colonna azioni,
colonna editoriale (articolo in primo piano con foto e didascalia di provenienza,
giornale, articoli e servizi), colonna dossier (pilota, prossima sessione,
classifica, dossier/ultimo weekend, stato contenuti), barra di stato.

Verificata a 1366×768, 1600×900 e 1920×1080 tramite `tests/UiRender`, che
disegna la form in un bitmap fuori schermo.

### G — Motore narrativo

Il generatore precedente aveva quattro paragrafi scritti a mano, il nome del
pilota **cablato** nel testo ("Marco Ruga" indipendentemente dalla carriera) e
una biografia inventata (officina di periferia, padre meccanico, kart) che non
esisteva in `CareerState`.

Il nuovo motore compone in cinque passaggi:

1. `StoryFacts` raccoglie **solo** dati verificati dalla carriera;
2. `EditorialImpact` decide quanto spazio merita la notizia (posizione rispetto
   all'aspettativa, serie precedente, posta in gioco, conseguenze economiche);
3. le angolazioni rilevanti vengono selezionate e ordinate per quell'evento
   (risultato, traguardo, serie, campionato, compagno, rivalità, qualifica,
   condizioni, tecnica, economia, sponsor, memoria, fuoripista, prospettiva);
4. `PhraseBank` sceglie fra più formulazioni equivalenti **evitando quelle usate
   di recente** (memoria persistente e limitata in `CareerState.UsedPhrases`);
5. la continuità è garantita: ogni articolo contiene almeno un blocco che
   aggancia il passato della carriera.

Verificato su dieci gare consecutive: 10/10 corpi distinti, 10/10 titoli
distinti, 9/9 articoli con blocco di continuità, 8 che nominano il round
precedente. Lo stesso evento riaperto resta identico.

Una nota sullo stile: lo spec del progetto vieta di imitare commentatori
identificabili, e questa scelta è stata mantenuta. Il testo persegue il mestiere
— ritmo, densità tecnica, memoria, giudizio che cambia — con voce originale.

### H — Foto degli articoli

`ArticlePhotoService` sceglie l'immagine e ne **dichiara sempre la provenienza**:

| Provenienza | Documenta l'evento |
|---|---|
| Screenshot reale acquisito da Assetto Corsa nella sessione | sì |
| Foto importata dall'utente | sì |
| Screenshot d'archivio dello stesso circuito | no, dichiarato come sessione precedente |
| Illustrazione generata da AI | no, dichiarata in didascalia con provider e richiesta |
| Asset editoriale del pacchetto | no, dichiarato come illustrazione |
| Nessuna immagine | dichiarato, senza sostituzioni |

**Immagini dal web: non implementate, per scelta.** Scaricare fotografie reali e
usarle per illustrare eventi mai accaduti di un pilota inventato è esattamente
ciò che lo spec vieta ("se non esiste una foto, la card deve dichiararlo senza
creare un'immagine falsa"), oltre al problema di copyright. Se serve comunque,
la via corretta è passare per `ArticlePhotoService.Register` con
`PhotoProvenance.UserImport` o `AiIllustration`, così l'immagine resta
etichettata come illustrazione e non come documento.

**Hook per un provider AI:** deposita i file generati in
`Documenti\Assetto Corsa\CorsaCareer\media\ai-illustrations`, con il tipo di
evento o il nome del circuito nel nome del file, e registrali con
`ArticlePhotoService.Register(..., PhotoProvenance.AiIllustration, prompt: …, provider: …)`.
CorsaCareer non chiama nessun servizio esterno da sé: serve un provider e una
chiave, che restano fuori dall'app.

## Migrazioni

- Carriera: **schema 10**. Nuovi campi: `SeasonStartDate`, `DifficultyProfile`,
  `DistanceProfile`, `AutoCalibrateAi`, `AiCalibrationOffset`, `SessionPlans`,
  `UsedPhrases`, `SeatPrestige`, `RepairCosts`, `LogisticsCosts`, `Fanbase`,
  `Fatigue`, `TeamRelation`, `SponsorRelation`, `DaysUntilNextRound`,
  `ActivityHistory`, `TeammateRacesWon/Lost`, `EvaluationTargetBasis`,
  `EvaluationTargetTrack`. `SeasonStartDate` viene dedotto dalle gare già
  archiviate della stagione corrente, senza spostare nessun risultato.
- Indice contenuti: **schema 4**, con `LengthMeters`, `Pitboxes`, `Weathers`.
- `RaceHistoryEntry` e `TestSessionRecord` conservano le condizioni realmente
  scritte nel preset.

## Cosa resta aperto

1. **Gara 2.** Non implementata: richiede due risultati per round nella macchina
   a stati del weekend pendente, cioè toccare l'area dove vivono gli invarianti
   di integrità (hash, associazione pendente, chiusura transazionale). Da fare
   con attenzione, non di fretta.
2. **Pioggia.** Assente in Assetto Corsa vanilla; il pianificatore usa solo i
   meteo installati, quindi con Sol/CSP la varietà cresce da sola.
3. **Persistenza su SQLite.** Lo spec la indica come soluzione preferita; oggi
   resta un unico `career.json` con scrittura atomica. Con `Events`, `News` e
   `SessionPlans` che crescono, prima o poi conviene.
4. **Progressione dei piloti AI fra le stagioni.** Il roster è persistente ma le
   statistiche AI derivano solo dalle gare che il giocatore disputa.
5. **Nomi e squadre generati.** Compagni, rivali e team sono ancora nomi fissi
   ("Alex Martin", "Northline Racing"): servirebbero pool per nazionalità.
6. **Test della UI.** `tests/UiRender` verifica che il layout si impagini, non
   che sia corretto: la verifica resta visiva.
7. **Verifica live.** Nessuna gara reale è stata conclusa in Assetto Corsa in
   questa iterazione: il percorso preset → sessione → `race_out.json` → import
   va provato dall'utente. Tutto il resto è coperto da fixture.

## Nota sulla cartella consegnata

La cartella di progetto è `CorsaCareer1991`. Per codex servono solo i file `.cs`,
i `.csproj`, i `.md` e `assets/`. Il resto è rigenerabile ed è escluso da
`.gitignore`:

- `bin/`, `obj/`, `publish/` — output di compilazione (circa 310 MB);
- `ui-preview/` — anteprime rese da `tests/UiRender`;
- i `.png` nella radice — screenshot di verifica delle iterazioni precedenti.

L'SDK .NET è installato in `%LOCALAPPDATA%\Microsoft\dotnet` e **non è nel PATH
di sistema**: va impostato `DOTNET_ROOT` come mostrato all'inizio di questo
documento, oppure installato normalmente da <https://dotnet.microsoft.com/download>.

## Correzione successiva alla prima verifica

La guardia contro i salvataggi a vuoto confrontava l'impronta solo con l'ultimo
salvataggio *del processo corrente*: al primo `SaveCareer` di ogni avvio la
carriera veniva quindi riscritta con un nuovo backup anche se identica, ed era
proprio l'avvio a consumare l'anello. Ora la guardia si inizializza dall'impronta
del file già su disco. Verificato empiricamente: sei costruzioni della form
consecutive producono **zero** backup superflui (prima erano circa tre per avvio).

## Modifiche successive richieste dall'utente

### Servizio di ascolto predefinito e velocità

Il servizio predefinito è ora **Google italiano (it-IT)**, la voce esposta dal
motore Web Speech di Chrome/Edge: vive quindi nella pagina locale del portale,
non fra le voci SAPI installate in Windows. `NarrationVoiceCatalog` contiene la
regola canonica di ordinamento (`OrderBrowserVoices`): prima Google it-IT, poi le
altre it-IT, poi le varianti italiane; il JavaScript delle pagine applica lo
stesso criterio. Se Google non è esposta si ripiega sulla migliore voce it-IT.

La velocità di lettura è aumentata del 10%: `0,88 → 0,97` nel browser e prosodia
SSML da `-8%` a `+1%` nel percorso Windows. I valori precedenti restano nel
codice come costanti (`PreviousBrowserSpeechRate`, `PreviousWindowsProsodyFactor`)
perché un test verifica che l'incremento sia effettivamente del 10%.

### Data d'inizio della carriera

Il default passa dal **1991 al 2023** (`NarrativeCalendar.DefaultStartYear`), con
inizio stagione il 12 marzo. Il 1991 era anteriore alla quasi totalità dei
contenuti installabili. È stata anche rimossa una sentinella fragile: il codice
usava `StoryDate.Year == 1991` come marcatore per "data non ancora derivata", il
che rendeva impossibile giocare deliberatamente una stagione 1991. La derivazione
dall'anno dichiarato dalla vettura resta: una vettura storica apre una carriera
storica. Il ripiego non usa più `DateTime.Today.Year`, così un salvataggio non
dipende dall'orologio della macchina.

### Ritorno al portale dalle pagine web

`PortalFocusServer` apre un `TcpListener` su `127.0.0.1` con porta effimera; le
pagine generate mostrano un pulsante **↩ TORNA AL PORTALE** che lo chiama e
riporta il fuoco alla finestra principale. Non è usato `HttpListener` perché su
Windows richiederebbe una prenotazione URL con privilegi di amministratore.
L'endpoint ascolta solo su loopback, accetta un unico percorso senza parametri,
non legge né restituisce dati della carriera e vive quanto la finestra. Se la
porta non è disponibile le pagine restano valide, semplicemente senza pulsante.

### Carriere salvate: cancellazione e pulizia

`CareerSlotsDialog` consente selezione multipla, mostra data e dimensione di ogni
carriera archiviata, e aggiunge **Cancella selezionate** (con conferma che elenca
i nomi e default sul "no") e **Pulisci vecchi backup** (conserva i cinque più
recenti). La carriera in corso vive in `career.json`, fuori dalla cartella degli
archivi, e non viene mai toccata da queste operazioni.

### Agenda del pilota: decisioni e servizi

L'agenda non è più un elenco di pulsanti. Sei attività (giornata stampa, podcast,
evento sponsor, incontro con i tifosi, debrief con gli ingegneri, negoziazione)
richiedono una **decisione** fra tre linee alternative, ciascuna con il proprio
scostamento di rischio, i propri effetti e il proprio taglio narrativo. La scelta
entra nel seme deterministico, quindi due decisioni diverse nello stesso momento
non possono produrre lo stesso esito.

Ogni voce d'agenda produce inoltre un **servizio** vero: due angolazioni dedicate
(`agenda` e `conseguenze`) raccontano l'attività, la decisione presa e il bilancio
misurabile, e dichiarano che nulla di tutto questo tocca un risultato di gara.
Dall'esito si passa direttamente alla pagina, e il diario dell'agenda riapre un
servizio con un doppio clic. Verificato: otto voci consecutive producono otto
servizi diversi.

### Correzione trovata durante la verifica

L'ispezione della cartella di salvataggio ha mostrato circa tre backup per avvio:
la guardia contro i salvataggi a vuoto confrontava l'impronta solo con l'ultimo
salvataggio del processo corrente, quindi il primo `SaveCareer` di ogni avvio
riscriveva comunque. Ora la guardia si inizializza dall'impronta del file su
disco. Verificato: sei costruzioni consecutive della finestra producono **zero**
backup superflui.

### Sessioni preparate e non concluse

Buco di coerenza chiuso: uscire a metà sessione non registrava nulla, quindi si
poteva rientrare e uscire all'infinito per ripescare condizioni, griglia o un
tentativo di valutazione migliore.

La regola d'integrità resta intatta — non viene inventato nessun tempo e nessuna
posizione d'arrivo. Viene registrato un fatto che l'applicazione conosce per
certo: ha preparato e lanciato quella sessione, e quella sessione non ha prodotto
un referto utilizzabile. È un ritiro, e nel motorsport reale un ritiro sta a
referto.

`WithdrawalRules` è il modulo puro che definisce le conseguenze:

| Caso | Effetto |
|---|---|
| Prova di test senza alcun giro cronometrato | tentativo consumato, reputazione −2, team −3, nessun tempo registrato |
| Prova abbandonata dal pilota | come sopra, con testo distinto |
| Gara abbandonata dal pilota | **round consumato**, reputazione −6, team −8, sponsor −6, trasferta a carico, zero punti e zero premio |
| Annullamento tecnico dichiarato | nessuna penalità, round o tentativo **non** consumato, ma registrato e contato |

Il costo è dichiarato nella conferma **prima** della scelta: è la differenza fra
una decisione e una scappatoia. L'annullamento tecnico esiste perché
l'applicazione non può distinguere un crash reale da una scappatoia; la scelta
resta al giocatore, ma `AbandonedSessions` e `TechnicalAnnulments` sono contati e
mostrati nel dossier della home, quindi il conto è pubblico.

Una gara abbandonata entra in `RaceHistory` con `Position = 0`, `Abandoned = true`
e `SourceKind = "CORSACAREER_WITHDRAWAL"`. Il motore narrativo ha due angolazioni
dedicate (`ritiro-weekend`, `costo-ritiro`) e tutte le angolazioni che
presuppongono un arrivo sono disattivate: nessuna pagella, nessun giudizio di
prestazione, nessuna posizione citata, e la scheda del weekend dichiara
esplicitamente «Weekend non concluso» invece di mostrare un arrivo inesistente.

### Agenda della carriera: da calendario statico a eventi causali

Il difetto: `BuildRounds()` trasformava **ogni circuito installato in un round**,
ricalcolandolo a ogni avvio e indipendentemente dalla fase di carriera. Con dodici
circuiti installati esisteva un calendario da dodici round anche durante la rookie
evaluation, quando il pilota non ha né contratto né campionato. E niente era
causale: nessun esito poteva cambiare quello che veniva dopo.

`CareerScheduler` introduce un'agenda persistente (`CareerState.Schedule`) in cui
esiste **solo ciò che un fatto precedente ha generato**, e ogni voce dichiara la
propria causa in `GeneratedBy`.

```
apertura carriera → 1 prova di valutazione
      ├── superata      → nessun appuntamento; il calendario nasce alla firma
      ├── vicina        → prova di conferma su un circuito diverso
      ├── lontana       → altro tentativo di valutazione
      └── dopo 3 prove  → gara su invito (percorso alternativo)

firma del contratto → calendario del campionato (round per categoria)
fine stagione       → nuovo calendario, circuiti ruotati
```

Il numero di round dipende dalla categoria (Rookie 6, regionale 8, avanzata 10,
top 12), limitato ai circuiti disponibili — non è più «tutti i circuiti». La
selezione ruota con la stagione, quindi due campionati consecutivi hanno calendari
diversi, e resta deterministica.

`rounds` è ora una **vista derivata** dei round di campionato della stagione
corrente, quindi i 41 punti che lo usavano continuano a funzionare. Durante la
valutazione è vuoto — ed è corretto. I consumatori che davano per scontato un
calendario sono stati adeguati: testata, comandi, briefing, lancio del test,
obiettivo della valutazione e indicatore dei contenuti (che ora misura i circuiti
installati, non i round). Le carriere salvate prima dell'agenda la ricostruiscono
dallo storico gare e test, senza spostare nessun risultato.

### Due bug trovati durante la verifica

Per verificare l'agenda dal vivo ho costruito una radice Assetto Corsa finta con
dodici circuiti e quattro vetture. Sono emersi due difetti preesistenti:

1. **Crash all'avvio con metadati numerici in stringa.** `ContentScanner.ReadInt`
   chiamava `TryGetInt32` su un elemento JSON di tipo String: in quel caso
   `System.Text.Json` **solleva** un'eccezione invece di restituire false. Assetto
   Corsa e molte mod scrivono `"power": "550"`, quindi una singola vettura così
   faceva crollare l'intera scansione e con essa l'avvio dell'applicazione. Ora i
   valori in stringa vengono letti (anche con unità, come `"1250 kg"`), e ogni
   voce è protetta da un catch che costa una riga saltata con avviso, non l'avvio.
   Coperto da test di regressione.
2. **`BuildRounds` inghiottiva gli errori in silenzio.** Un `catch { }` nascondeva
   il fallimento del salvataggio dell'indice contenuti, rendendo impossibile
   capire perché i contenuti «sparissero». Ora ogni scansione è tracciata nel log
   con auto, circuiti e meteo rilevati.

### Il giornale diventa il diario della carriera

Prima la colonna editoriale aveva un articolo in primo piano e, sotto, una barra
«articoli e servizi»: un menu di scorciatoie senza date né ordine cronologico. La
carriera non si leggeva come una storia, e sei delle sette voci di quella barra
erano duplicati di sezioni già presenti nella colonna di sinistra.

`CareerDiary` (modulo puro) trasforma gli eventi della carriera in **voci datate**
in ordine cronologico inverso. Ogni voce porta:

- la data completa (`domenica 23 aprile 2023`), con separatore di giornata;
- l'**intervallo** rispetto alla voce precedente (`il giorno prima`, `3 settimane
  prima`, `un anno prima`), che è ciò che dà il senso del tempo che passa;
- un'etichetta di sezione per tipo di fatto (`VITTORIA`, `WEEKEND NON CONCLUSO`,
  `FUORI DALLA PISTA`, `RIVALITÀ`, `AGENDA`…);
- una riga di contesto costruita **solo** da dati registrati (`Pista 03 · P3 ·
  15 punti · 20 giri · sereno, 24,5 °C`), e mai una posizione per un weekend
  abbandonato;
- il collegamento al proprio evento: cliccare la voce apre l'articolo completo.

In testa al diario compare il prossimo appuntamento in agenda con la sua causa:
il diario contiene così anche la parte non ancora scritta. Le voci archiviate sono
escluse, e gli indici restano corretti anche dopo l'esclusione.

L'unica voce non duplicata della vecchia barra («Storia guidata del pilota») è
stata spostata fra i comandi di gestione.

### Bug trovato durante la verifica del diario

Per vedere il diario a schermo ho preparato una carriera con quattro round
disputati, un podio, una rivalità e un weekend abbandonato. Il render ha mostrato
i riquadri dei dati fermi su zero pur avendo la carriera caricata correttamente
(21 punti, 4 gare, reputazione 41).

Causa: `UiTheme.UpdateStatTile` cercava l'etichetta con
`Controls.Find("value", false)`, cioè senza attraversare i figli. Da quando il
riquadro usa un `TableLayoutPanel` interno, l'etichetta non è più figlia diretta
del pannello: la ricerca non trovava nulla e l'aggiornamento non faceva niente in
silenzio. I riquadri mostravano i valori iniziali da quando ho ristrutturato il
componente. Corretto con `Find("value", true)`.

## Evoluzione della modalità carriera — spina dorsale del ciclo di conseguenze

Obiettivo dichiarato: far percepire continuamente che *quello che ho fatto nella
gara precedente ha cambiato ciò che mi sta succedendo adesso*. Sono stati aggiunti
cinque moduli puri, senza toccare lettura referti, test, gare e avanzamento.

### Cosa c'era e cosa mancava

Riutilizzato: `CareerScheduler` (agenda causale), `NarrativeEngine`+`StoryFacts`+
`PhraseBank`, `SessionPlanner`, parser dei referti e regole d'integrità,
`OffTrackActivities` (già il pattern giusto: costo, rischio, esito, conseguenza).

Il buco vero era triplo:
1. le offerte venivano da `BuildOffers()` — tre voci con nomi di team cablati,
   sempre le stesse, più una aggiunta ogni due gare a prescindere dai risultati;
2. la reputazione era **un solo numero**, quindi tutte le porte si aprivano e
   chiudevano insieme;
3. con 250.000 € iniziali e nessuna quota d'iscrizione **non era possibile andare
   in difficoltà economica**, e questo rimuoveva il rischio da ogni decisione.

### Moduli nuovi

| Modulo | Responsabilità |
|---|---|
| `ReputationProfile` / `ReputationDynamics` | Sei dimensioni indipendenti: fiducia dei team, interesse sponsor, popolarità, stampa, prestigio sportivo, affidabilità. Ogni dimensione reagisce a un aspetto diverso dello stesso referto |
| `CareerFinances` | Patrimonio come vincolo: soglia di sopravvivenza, quote per categoria, e il riquadro «patrimonio / costo / guadagno possibile / rischio» prima di ogni scelta |
| `Opportunity` / `ConditionalPromise` | Una proposta con costo, copertura, ritorni, scadenza, obiettivo e la promessa condizionale collegata |
| `OpportunityGenerator` | Genera le proposte **dallo stato reale**: requisiti su risultati, reputazione, denaro, categoria e rapporti. Deterministico |
| `ConsequenceEngine` | Unico punto in cui un esito modifica lo stato: reputazione, denaro, promesse, opportunità scadute, difficoltà economica |

Capitale iniziale portato da 250.000 a **32.000 €**, soglia di sopravvivenza a
8.000 €. Una stagione in categoria avanzata costa 160.000: non è accessibile il
primo giorno, e va costruita.

### Come si chiude il ciclo

- La copertura del costo di un test **è** la misura della fiducia dei team: 0% se
  nessuno crede nel pilota, 100% a fiducia 70+.
- La forma di un sedile dipende da prestigio e fiducia: da pagare interamente →
  parzialmente finanziato → contratto con stipendio.
- Rifiutare pesa solo dove deve: rifiutare un contratto professionistico costa
  fiducia, rifiutare un sedile a pagamento no.
- Le promesse condizionali (`«se chiudi entro il 30% del gruppo…»`) sono salvate e
  **verificate dopo ogni risultato reale**: se sono mantenute il denaro arriva
  davvero e la copertura viene registrata.
- Le opportunità non colte **scadono e si perdono**.
- Sotto la soglia di sopravvivenza restano solo eventi promozionali, sponsor e
  gare open: è il percorso di ricostruzione, non un vicolo cieco.

### Scenario di accettazione, verificato dal test

`CareerLoopCheck.AcceptanceScenario` replica la catena indicata come criterio:

```
28.000 € · sedile F4 da 22.000 → rischio ALTO → rifiutato
tre gare open, due vinte → stampa 18→33, popolarità su
interesse sponsor supera la soglia → offerta da 33.200 €
accettata → il denaro entra realmente in cassa
il test diventa economicamente accessibile → rischio BASSO
```

### Cosa resta aperto

Non implementato in questa iterazione, e va detto: **scuderia propria ed endgame**
(§14), **cast di NPC ricorrenti con memoria** (§11 — esiste `PersistentDriver`
per i piloti, mancano team principal, manager e giornalisti come attori),
**classifica team e sviluppo vettura** (§8 parziale), **mercato sponsor con uscite
e richiami** (§6 parziale: gli sponsor entrano, non escono ancora). Le
opportunità inoltre non sono ancora esposte nel diario datato come voci proprie:
si aprono dal pulsante «Opportunità».
