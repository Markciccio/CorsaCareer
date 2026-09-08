# Passaggio al PC con Assetto Corsa e Content Manager

Documento per chi (agente o persona) riprende questo progetto **sul computer dove
Assetto Corsa e Content Manager sono davvero installati**, e deve fare tutti i
collegamenti fra la carriera e il simulatore.

Qui trovi: cos'è già fatto, dove sono le giunture, quali parametri viaggiano
verso il preset, cosa va verificato in pista e cosa manca. Non serve leggere il
resto della documentazione per lavorare: quello che serve è tutto in questa
pagina, con i riferimenti ai file.

---

## 1. In una riga

CorsaCareer è un generatore di carriera (WinForms, .NET 9) che porta un
pilota giapponese dal kart alle formule. **Tutta l'ossatura esiste già ed è
parametrizzata**: scala di categorie, livelli di campionato, doti del pilota,
economia, calendario, opportunità, narrativa. Sul PC di sviluppo le sessioni
vengono risolte da un simulatore interno perché Assetto Corsa non c'è. Sul PC di
destinazione **la stessa preparazione apre Content Manager** e il risultato
arriva dal referto vero.

**Il codice per farlo c'è già ed è completo.** Il lavoro sul PC di destinazione
non è scrivere l'integrazione: è verificarla in pista, correggere gli ID dei
contenuti reali e tarare il livello IA sui referti veri.

---

## 2. Portarlo su questo computer

Ci sono due strade, e la prima non richiede niente di installato.

### A. Scaricare la build pronta (consigliata)

Dalla pagina **Releases** del repository scarica `CorsaCareer-v1.zip`
(circa 350 MB), scompattalo dove preferisci e fai doppio clic su
`CorsaCareer.exe`.

È una build **autonoma**: contiene il runtime .NET, le illustrazioni, le musiche
e la narrazione. **Non serve installare .NET.** Non tocca Assetto Corsa e non
scarica niente: al primo avvio cerca i contenuti installati e crea la cartella
dei salvataggi in `Documenti\Assetto Corsa\CorsaCareer`.

Se preferisci un collegamento sul desktop, nella cartella c'è
`CorsaCareer-Install.ps1`: clic destro → *Esegui con PowerShell*.

### B. Clonare il repository (per lavorare al codice)

```
git clone <url del repository>
```

Servono **.NET 9 SDK con il carico Windows Desktop** e poi il paragrafo
successivo. Da sviluppo il programma legge le illustrazioni direttamente dalla
cartella `assets` del progetto, senza copiarle: vedi `AssetPaths`.

### Rifare la build da distribuire

```powershell
$sdk = "$env:LOCALAPPDATA\Microsoft\dotnet"
$env:DOTNET_ROOT = $sdk; $env:PATH = "$sdk;$env:PATH"
& "$sdk\dotnet.exe" publish CorsaCareer.csproj -c Release -r win-x64 --self-contained true -o publish
Compress-Archive -Path publish\* -DestinationPath CorsaCareer-v1.zip
```

La pubblicazione **ricopia le illustrazioni** dentro `publish`, al contrario
della compilazione di sviluppo: un'installazione distribuita deve portarsi
dietro le proprie immagini, e lì quella copia è l'unica che c'è.

---

## 2b. Compilare ed eseguire

L'SDK .NET non è nel PATH sul PC di sviluppo. Su quello nuovo, verificalo:

```powershell
$sdk = "$env:LOCALAPPDATA\Microsoft\dotnet"   # oppure "$env:ProgramFiles\dotnet"
$env:DOTNET_ROOT = $sdk
$env:PATH = "$sdk;$env:PATH"
& "$sdk\dotnet.exe" build CorsaCareer.csproj -c Debug
& "$sdk\dotnet.exe" run  --project CorsaCareer.csproj
```

Serve **.NET 9 SDK con il carico Windows Desktop** (`net9.0-windows`, WinForms).
`dotnet` presente nel PATH ma solo runtime non basta: `Find-DotnetSdk` in
`Misura-Carriere.ps1` mostra come individuare quello giusto.

---

## 3. La giuntura: dove la carriera incontra il simulatore

Ci sono **tre** punti di lancio, tutti con la stessa struttura. Sono le uniche
funzioni da guardare per l'integrazione.

| Cosa | File | Riga circa | Preset prodotto |
|---|---|---|---|
| Prova / test | `Program.cs` → `LaunchTestSession` | 1889 | `QuickDrive_Practice` |
| Round di campionato | `Program.cs` → `LaunchWeekend` | 3549 | `QuickDrive_Weekend` |
| Gara su invito | `Program.cs` → `LaunchInvitation` | 3671 | `QuickDrive_Weekend` |

Ognuna esegue esattamente questa sequenza:

1. `BuildSessionPlan(trackId, carId, mixedGrid, testSession)` — `Program.cs:3731`
   costruisce una `SessionRequest` e la passa a `SessionPlanner.Plan`.
2. `ContentManagerPresetBuilder.Build(...)` (o `.BuildTest`) traduce il piano in
   JSON Quick Drive.
3. Il preset viene scritto in
   `%LOCALAPPDATA%\AcTools Content Manager\Presets\Quick Drive\CorsaCareer - S01 GP 03 <GP>.cmpreset`.
4. `ContentManagerPresetValidator.TryValidate(...)` rifiuta un preset incoerente
   **prima** di aprire CM (auto non installata, griglia vuota, pilota fuori
   griglia, avversari incoerenti).
5. Viene salvato `pending_weekend.json` nella cartella dei salvataggi, con
   l'impronta SHA-256 del `race_out.json` **precedente** al lancio.
6. `LocateContentManager()` cerca Content Manager. **Se lo trova**, apre
   `acmanager://race/quick?presetFile=...`. **Se non lo trova**, chiama
   `ResolvePendingSessionWithoutAssettoCorsa()` e la gara viene simulata.

> **Questo è l'interruttore.** Non c'è una impostazione da cambiare: basta che
> Content Manager esista in uno dei percorsi cercati e il programma smette di
> simulare. Sul PC di destinazione **non va toccato nulla** per passare al
> simulatore reale.

### Dove viene cercato Content Manager

`Program.cs:4287` — `LocateContentManager()`:

1. `<Desktop>\Content Manager.exe`
2. `%LOCALAPPDATA%\AcTools Content Manager\Content Manager.exe`
3. `%ProgramFiles(x86)%\Content Manager\Content Manager.exe`
4. qualunque collegamento `.lnk` sul Desktop il cui nome contenga
   "content manager" (risolto via WScript.Shell).

**Se la tua installazione è altrove, aggiungi il percorso qui.** È l'unico punto
da modificare.

### Come viene aperto

`ContentManagerLaunch.cs` usa la URI `acmanager://race/quick?presetFile=<path>`
e **non** il passaggio del `.cmpreset` come argomento: con l'argomento CM mostra
la finestra "Apply preset / Just Go", con la URI legge il preset ed esegue
direttamente `QuickDrive.RunAsync`. Se su questa installazione la URI non è
registrata (handler `acmanager://` mancante), il fallback è passare il percorso
del preset come argomento — ma va accettata la finestra di conferma.

---

## 4. Cosa finisce nel preset, e da dove viene

`SessionPlanner.Plan(SessionRequest) → SessionPlan → ContentManagerPresetBuilder`

### Il livello IA — il parametro che conta di più

`SessionPlanner.cs:392` — la difficoltà nasce dal **gradino della scala di
carriera**, non dal nome della categoria:

| Gradino | Categoria tipo | IA base |
|---|---|---|
| 1 | kart quattro tempi | 76 |
| 2 | kart due tempi | 80 |
| 3 | kart 125 con cambio | 84 |
| 4 | formula d'ingresso, GT4, monomarca | 88 |
| 5 | formula nazionale, GT3, turismo internazionale | 92 |
| 6 | formula internazionale, prototipi | 95 |
| 7 | il vertice (F1, hypercar) | 97 |

La formula completa (`ApplyAi`, `SessionPlanner.cs:416`):

```
base    = min(IAgradino + 2, IAgradino + (livelloCampionato - 1) * 2.5)
livello = base + offsetDifficoltà + calibrazioneDaiReferti
AiLevel = clamp(livello, 70, 100)
AiLevelMin = AiLevel - (2, oppure 5 con griglia mista)
```

- `livelloCampionato` è 1..5 (zona → regionale → nazionale → internazionale →
  vertice) e alza il gruppo **al massimo di 2 punti**: un campionato importante
  non trasforma una Formula Vee in un mondiale.
- `offsetDifficoltà` viene dal profilo scelto dal giocatore:
  Assistita −8, Realistica 0, Professionale +3, Massima +5
  (`SessionProfiles.DifficultyOffset`).
- `calibrazioneDaiReferti` è `career.AiCalibrationOffset`, limitato a
  [−10, +6], attivo solo con `career.AutoCalibrateAi`. **È il gancio pensato
  apposta per il PC con Assetto Corsa**: vedi §8.

L'aggressività parte dalla categoria (`BaseAggression`) moltiplicata per il
profilo di difficoltà; `AiAggressionMin = AiAggression − 12`.

### Il resto del piano

| Campo del preset | Origine |
|---|---|
| `CarId`, `TrackId` | ID della cartella dei contenuti installati |
| `WeatherId` | scelto fra i meteo **realmente installati** (`InstalledWeatherIds`) |
| `Temperature`, `Time`, `wsf`/`wst`/`wd` (vento) | `SessionPlanner`, dalla data della storia e dal circuito |
| `LapsNumber` | distanza del circuito × profilo distanza (Breve 0,55 / Realistica 1,0 / Lunga 1,7) |
| `PracticeLength`, `QualificationLength` | formato della categoria |
| `RaceGridSerialized.CarIds` | `RaceGridSelector.Select` |
| `OpponentsNumber` | `min(candidati − 1, 7)` |
| `AiLevel`, `AiLevelMin`, `AiAggression`, `AiAggressionMin` | tabella qui sopra |

### La griglia

`RaceGridSelector.cs`: prima cerca **almeno due auto della stessa categoria**
dell'auto del pilota. Se non ci sono, ripiega su una griglia mista escludendo
`road`, `special` e `kart`, e segnala `UsedMixedFallback` (che allarga lo spread
IA da 2 a 5 punti).

> **Conseguenza pratica per l'installazione dei contenuti:** una categoria con
> una sola auto produce sempre griglie miste. Per ogni gradino che vuoi
> percorrere, installa **almeno 3-4 auto della stessa categoria**.

---

## 5. I contenuti: come vengono letti e classificati

### Dove

`ContentScanner.Scan(root)` in `ContentModels.cs:63` legge:

```
<root>\content\cars\<id>\ui\ui_car.json      → name, brand, bhp, weight, year, class/tags
<root>\content\cars\<id>\skins\*             → livree
<root>\content\tracks\<id>\ui\ui_track.json  → name, country, length
<root>\content\weather\<id>\weather.ini      → NAME=
```

La radice viene decisa così (`Program.cs:1244`), in ordine:

1. la variabile d'ambiente **`CORSACAREER_AC_ROOT`**, se contiene una cartella `content`;
2. la radice memorizzata nell'indice della scansione precedente;
3. `AssettoCorsaRoot()` — percorsi Steam standard più le librerie lette da
   `libraryfolders.vdf`.

Le auto `road` e `special` sono escluse dalle gare
(`ContentCategoryRules.IsRaceable`).

### La classificazione in gradini

`CareerLadder.ForCar(category, powerHp, massKg)` — `CareerLadder.cs:164`. In
ordine di priorità:

1. **kart**: categoria contenente "kart", **oppure** ≤ 60 CV e ≤ 220 kg.
   Sotto 15 CV → gradino 1; da 30 CV in su → gradino 3; in mezzo → gradino 2.
   Senza potenza dichiarata, le sigle "kz", "125", "shifter" portano al gradino 3.
2. `hypercar`, `gt1`, `group c`, `can-am` → gradino 7
3. `lmp`, `prototype`, `sport prototype` → gradino 6
4. `gt4` → gradino 4 · `gt3`, `gt2`, `gte`, `gt500`, `gt300` → gradino 5
5. **monoposto** (`formula`, `open wheel`, `monoposto`, `single seater`, `indy`):
   sigla se c'è (`f1`/`f2`/`f3`/`f4`, `junior`, `vee`, `ford`), altrimenti la
   potenza: ≥600 → F1, ≥380 → F2, ≥200 → F3, sotto → F4.
6. `tcr`, `wtcc`, `btcc`, `dtm`, `super taikyu` → gradino 5 ·
   `cup`, `trofeo`, `monomarca`, `one make` → gradino 4 ·
   `touring`/`turismo`/`sedan`/`saloon` → 5 se ≥300 CV, altrimenti 4
7. **ripiego per sola potenza**: ≥700 hypercar, ≥500 gt3, ≥250 cup, ≥150 f4,
   sotto kart due tempi.

Il riconoscimento è **per contenuto della stringa**, non per uguaglianza: i
pacchetti reali dichiarano "gt3_cup", "Formula RSS 2000", "Open Wheeler", e
devono comunque finire nel gradino giusto.

> **Se un mod finisce nel gradino sbagliato** non serve toccare il codice: la UI
> ha `ContentReviewDialog` (Program.cs:2315) che permette di forzare la
> categoria di una singola auto. La correzione viene salvata in
> `CategoryOverrides` nell'indice e sopravvive alle riscansioni.

### La regola dei gradini pieni — leggila prima di installare i contenuti

Nessuna installazione riempie tutti e sette i gradini. `CareerLadder` calcola i
**gradini che i contenuti riempiono davvero** (`PopulatedSteps`,
`NextPopulatedStep`, `WithinReach`) e il salto di categoria è sempre "il primo
gradino pieno sopra", mai "il numero successivo".

Senza questa regola una carriera si fermava davanti a un gradino vuoto: nel
catalogo di prova esistono i gradini 1-5 e poi il 7 (RedBull RB9), il 6 non
esiste, e tutte le carriere morivano in Formula 3.

**Cosa installare, per gradino, perché la carriera sia percorribile per intero:**

| Gradino | Serve | Esempi |
|---|---|---|
| 1 | kart ≤ 15 CV | kart da noleggio |
| 2 | kart 15-30 CV | 100cc / DAP |
| 3 | kart ≥ 30 CV o sigla KZ/125/shifter | KZ2, 125 shifter |
| 4 | formula d'ingresso, o GT4, o monomarca | Formula Vee, Tatuus FA01, Cayman GT4, MX5 Cup |
| 5 | formula nazionale, o GT3, o turismo ≥300 CV | Tatuus F3, 911 GT3, GT500 |
| 6 | formula internazionale, o prototipi | F2, Dallara SF23, LMP2 |
| 7 | il vertice | F1, hypercar, LMP1 |

Più i gradini sono pieni, più la salita è graduale. Un buco non blocca più la
carriera, ma la fa saltare di due gradini in un colpo solo.

---

## 6. Il ritorno: dal referto alla carriera

Al termine della sessione:

1. `AssettoCorsaResultLocator` (`AssettoCorsaResultLocator.cs`) cerca il referto in:
   - `Documenti\Assetto Corsa\out\race_out.json` ← il percorso normale
   - `Documenti pubblici\Assetto Corsa\out\race_out.json`
   - `<cartella del programma>\out\race_out.json`
2. `RaceResultParser.TryParse(json, preferredSessionType: 3, expectedPlayerName)`
   legge `players[]` e `sessions[]`, prende la sessione di tipo 3 (gara) e la sua
   `raceResult`. Per i test (Practice) accetta una sessione senza classifica.
3. `RaceImportIdentity.Mismatch(...)` rifiuta un referto di un'altra gara.
   Tollera **solo** il suffisso di layout: `ks_barcelona` nel preset e
   `ks_barcelona-layout_gp` nel referto sono lo stesso circuito.
4. `pendingResultHash` (SHA-256 del referto **prima** del lancio) impedisce di
   importare due volte lo stesso file: se l'impronta non è cambiata, la sessione
   non è stata corsa.
5. `ResultIntegrity` scrive un `.meta.json` accanto al referto e verifica
   l'impronta a ogni consultazione successiva: un referto modificato a mano viene
   segnalato come "VERIFICA FALLITA".
6. `SimulatorWatch` distingue "sta ancora guidando" da "ha chiuso senza referto":
   osserva il processo del simulatore con 3 minuti di tolleranza all'avvio, e
   restituisce `ResultAvailable` / `Running` / `WaitingForStart` /
   `NeverStarted` / `ClosedWithoutResult`.

> **Il primo controllo da fare in pista** è che `race_out.json` venga davvero
> scritto: dipende dall'impostazione di Content Manager per il salvataggio dei
> risultati (Settings → Drive → "Race results"). Se non lo scrive, tutto il
> resto del ritorno non parte.

---

## 7. Variabili d'ambiente

| Variabile | Effetto |
|---|---|
| `CORSACAREER_AC_ROOT` | forza la radice dei contenuti; ha la precedenza su tutto |
| `CORSACAREER_HOME` | sposta la cartella dei salvataggi (normalmente `Documenti\Assetto Corsa\CorsaCareer`) |
| `CORSACAREER_UI_AUTOMATION=1` | modalità non presidiata: nessuna finestra modale, nessuna narrazione, nessun audio. **Il banco di misura la usa; non ha effetti sulla logica di carriera** |
| `CORSACAREER_NO_ANIM` | disattiva le animazioni delle finestre |
| `CORSACAREER_DEMO_DRIVER` | nome del pilota generato automaticamente (serve al banco per avere carriere diverse) |
| `CORSACAREER_FIXTURE` | usata solo da `Misura-Carriere.ps1` e da `tests/DebugRun`: sceglie fra i cataloghi finti `ac-finto` e `ac-vero` |

I due cataloghi finti nella cartella del progetto (`ac-finto/`, `ac-vero/`)
esistono **solo** perché il PC di sviluppo non ha Assetto Corsa. Riproducono la
struttura `content\cars\<id>\ui\ui_car.json`. Sul PC di destinazione non servono
più: `ContentAvailability.IsDebugFixture` li riconosce e impedisce che vengano
presentati al giocatore come contenuti veri.

---

## 8. La calibrazione IA dai referti reali — il lavoro principale in pista

`career.AiCalibrationOffset` esiste, entra nel preset (limitato a [−10, +6]) e
viene citato nelle note del piano, ma **la logica che lo aggiorna dai referti
reali va scritta e tarata su questo PC**: è l'unico numero che non si può
calibrare senza correre davvero.

Il criterio: dopo ogni gara reale, confrontare il piazzamento e il distacco
ottenuti con quelli che il simulatore interno avrebbe previsto per lo stesso
pilota (`RaceSimulator.PlayerSkill`). Se il giocatore vince sistematicamente con
distacchi larghi, l'offset sale; se è costantemente doppiato, scende. Muovilo di
un punto per volta e mai più di una volta a weekend: l'IA di Assetto Corsa cambia
molto fra 92 e 94.

Le tabelle del §4 sono state tarate **contro il simulatore interno**, che è una
approssimazione. Su Assetto Corsa vero i numeri assoluti quasi certamente vanno
ritoccati: quello che deve restare è la **monotonia** — ogni gradino più duro del
precedente — non il valore esatto.

---

## 9. Cosa verificare in pista, in ordine

Una lista di collaudo, dalla più semplice alla più impegnativa. Ognuna è una cosa
che sul PC di sviluppo **non si può provare**.

1. **Scansione dei contenuti.** Avvia il programma e apri la revisione contenuti.
   Ogni auto installata deve comparire con categoria e gradino sensati. Correggi
   con gli override quelle sbagliate; se sbaglia sistematicamente una famiglia di
   mod, aggiungi la sigla in `CareerLadder.ForCar`.
2. **Copertura dei gradini.** Verifica che i gradini 1-7 siano coperti come da
   tabella §5, e che ogni gradino abbia almeno 3-4 auto della stessa categoria
   (altrimenti griglie miste ovunque).
3. **Apertura del preset di prova.** Fai partire un test: CM deve aprirsi in
   Practice, senza la finestra "Apply preset" e senza l'errore "set at least one
   opponent".
4. **Ritorno del referto di prova.** Al rientro il miglior giro deve essere
   registrato e il test chiuso in agenda.
5. **Weekend completo.** Un round di campionato: griglia, qualifica, gara,
   classifica importata, articolo di giornale, reazioni dei personaggi.
6. **Rifiuto del referto estraneo.** Corri una gara qualsiasi fuori da
   CorsaCareer e prova a importarla: deve essere rifiutata per identità diversa.
7. **Sessione abbandonata.** Apri il weekend e chiudi AC senza correre: deve
   essere riconosciuta come `ClosedWithoutResult`, non restare pendente in
   silenzio.
8. **Calibrazione IA.** Corri 5-6 gare sullo stesso gradino e confronta i tuoi
   piazzamenti con quelli che il simulatore interno prevede. Da qui nasce
   l'offset del §8.

---

## 10. I parametri della carriera, tutti in un posto

Perché non vadano cercati file per file. Sono i numeri che disegnano la forma
della carriera; cambiarli cambia il gioco, non l'integrazione.

### Scala delle categorie — `CareerLadder.cs`

7 gradini su 4 percorsi: `Karting` (1-3, tronco comune), poi `SingleSeater`,
`Endurance`, `Touring` (4-7). Vedi §5.

### Scala dei campionati — `ChampionshipLadder.cs`

5 livelli: zona → regionale → nazionale → internazionale → vertice.
`PromotionPosition = 3`: chiudere la stagione nei primi tre promuove al livello
successivo, altrimenti si resta. Indipendente dal gradino di categoria: si può
salire di campionato restando nella stessa categoria, e viceversa.

### Gavetta prima del salto — `OpportunityGenerator.RacesBeforeStepUp`

Gare da correre su un gradino prima che arrivi un'offerta per quello sopra:

| Gradino | Gare |
|---|---|
| 1 | 6 |
| 2 | 10 |
| 3 | 12 |
| 4 | 14 |
| 5 | 16 |
| 6+ | 18 |

Sommate fanno circa 58 gare per arrivare in cima: quattro o cinque stagioni.
È il parametro che decide **la lunghezza della carriera**.

### Promozione per conoscenza — `OpportunityGenerator`

Il salto di campionato che arriva dai contatti (Haru che incontra uno sponsor, il
vecchio capo del meccanico, il contatto della giornalista) richiede:

- `StepUpMinimumInfluencer = 55`
- `StepUpMinimumFitness = 45`
- `StepUpMinimumCash = 40000`
- almeno 4 gare disputate
- `StepUpRaceCooldown = 12` gare dall'ultimo salto per conoscenza

Probabilità `clamp(4 + merito/4, 4, 25)` percento, dove il merito somma
influencer, forma, liquidità e `podi × 3 + vittorie × 2`.

### Doti del pilota — `DriverTalent.cs`

Sei tratti derivati dal nome (deterministici): velocità pura, costanza, bagnato,
duelli, adattamento, affidabilità. Da questi nasce l'archetipo — "Il veloce",
"Il tenace", "Il pilota da pioggia", "Il combattente", "Il mediocre",
"Il pilota pagante", "Il velocista sfortunato", "Il gregario". Entrano nella
simulazione interna (`RaceSimulator.PlayerSkill`) e **non** nel preset: in una
gara vera il talento è quello di chi tiene il volante.

### Momenti che spostano i parametri — `RaceHighlights.cs`

Rimonte, vittorie all'esordio, titoli persi per un ritiro al primo giro: non
muovono solo il budget ma anche forma e seguito. Esempi: rimonta di 10+ posizioni
→ Fitness −6, Influencer +12; vittoria all'esordio → Influencer +18.

### Ritiri — `RaceSimulator.cs`

`BaseRetirementRisk = 0.035`, corretto da stanchezza, professionalità e dalla
dote di affidabilità. Il pilota ha una **prova sua**, indipendente da quella del
gruppo. Vale solo nella simulazione interna: in gara vera i ritiri li decide
Assetto Corsa.

---

## 11. Il banco di misura

`Misura-Carriere.ps1` esegue N carriere complete senza aprire finestre e stampa,
per ognuna: doti, archetipo, fasi con gare/vittorie/podi/posizione media/livello
IA/durata, cambi di vettura, e le medie del campione.

```powershell
.\Misura-Carriere.ps1                    # 6 piloti sul catalogo ac-vero
.\Misura-Carriere.ps1 -Piloti 10
.\Misura-Carriere.ps1 -Catalogo ac-finto
```

Sotto sta `tests\DebugRun`, che pilota `MainForm` fuori schermo con
`CORSACAREER_UI_AUTOMATION=1` e attraversa i pulsanti veri (`ContinueStory`,
`LaunchWeekend`, `AdvanceSeason`, …), non le funzioni interne. Ogni carriera ha
un limite di 500 passi; se lo raggiunge lo dichiara, così una carriera bloccata
non si confonde con un collaudo interrotto.

**Sul PC con Assetto Corsa il banco continua a funzionare e resta utile**:
`CareerMessages.Unattended` fa saltare l'apertura di Content Manager, quindi le
sessioni restano simulate anche lì. È il modo per verificare che una modifica ai
parametri non abbia rotto la forma della carriera, senza correre 60 gare a mano.

---

## 12. File e cartelle

| Percorso | Cosa contiene |
|---|---|
| `Documenti\Assetto Corsa\CorsaCareer\career.json` | lo stato della carriera |
| `…\CorsaCareer\content_index.json` | l'ultima scansione dei contenuti e gli override di categoria |
| `…\CorsaCareer\pending_weekend.json` | la sessione aperta e non ancora importata |
| `%LOCALAPPDATA%\AcTools Content Manager\Presets\Quick Drive\CorsaCareer - *.cmpreset` | i preset generati |
| `Documenti\Assetto Corsa\out\race_out.json` | il referto prodotto da AC |
| `assets\` | illustrazioni, catalogo anime, loghi dei team |
| `soundtrack\`, `narration\` | audio |
| `ac-finto\`, `ac-vero\` | cataloghi finti, solo per il PC senza AC |

---

## 13. Stato del lavoro e cosa resta aperto

**Fatto e verificato al banco:**
- salita monotona sulla scala, senza oscillazioni fra due categorie
- difficoltà crescente per gradino, con il gradino che arriva anche nel preset CM
- piloti tutti diversi per doti e archetipo, compresi i mediocri
- gavetta obbligatoria prima di ogni salto di categoria
- regola dei gradini pieni: un buco nei contenuti non ferma più la carriera
- nessuna retrocessione: un'offerta non può riportare indietro chi è già salito

**Corretto nel giro di debug sull'agenda** (utile saperlo perché tocca il calendario,
che sull'altro PC decide quando si apre Content Manager):
- il calendario non viene più ricostruito a ogni firma (i round venivano spinti
  in avanti di stagione in stagione, da 21 a 56 giorni di distanza);
- le gare open e le sostituzioni non vengono più proposte a chi ha già un round
  in calendario: erano 45 gare su 61 e il campionato non finiva mai;
- dopo una gara, se il campionato non ha più appuntamenti, ne viene pubblicato
  uno: l'agenda non resta vuota;
- una stagione con pochi circuiti compatibili non si accorcia più. Il numero di
  round era limitato ai tracciati adatti alla categoria: nel kart erano due o
  tre, la stagione durava due gare e portava subito una promozione. Adesso i
  circuiti si ripetono, come in qualunque campionato minore vero. **Vale anche
  per l'altro PC**: con pochi tracciati installati per una categoria il
  calendario resta di lunghezza piena invece di ridursi in silenzio.

Il collaudo su una carriera completa è passato da 49 problemi segnalati a 2.

**Aperto, da fare qui:**
1. **Calibrazione IA sui referti reali** (§8) — il lavoro principale.
2. **Fine carriera al vertice**: arrivato al gradino 7 la carriera prosegue senza
   una conclusione. Serve un finale — ritiro, titolo mondiale, fine del contratto.
3. **Dominio in alto**: al banco un pilota forte in Formula 1 vince quasi tutto.
   Con l'IA vera potrebbe non essere un problema; da rimisurare in pista prima di
   toccare i pesi.
4. **Uscite di Haru dagli sponsor come minigioco RPG** — idea approvata, non
   iniziata: arrivo in tempo alla visita, dialogo a scelte multiple, poi
   eventualmente una mappa dall'alto di una cittadina di provincia giapponese.
5. **Scene scolastiche** con Nami e Nobu che promuovono il pilota, con effetto sui
   parametri.
6. **Venti dialoghi in stile Capeta** già scritti dall'utente, da integrare al
   posto di quelli attuali che si somigliano troppo.
