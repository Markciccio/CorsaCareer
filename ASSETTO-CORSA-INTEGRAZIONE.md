# Integrazione con Assetto Corsa e Content Manager

Questo documento è operativo: dice cosa installare, cosa configurare, cosa
controllare e cosa fare quando qualcosa non torna. È scritto sul codice
com'è oggi — ogni percorso e ogni nome di file qui sotto esiste davvero nel
programma, e accanto è indicato il file sorgente che lo decide.

Il documento accanto, `PASSAGGIO-PC-ASSETTO-CORSA.md`, spiega *perché* le cose
sono fatte così e quali parametri governano la carriera. Questo spiega *come*
farle funzionare.

---

## 1. La regola che spiega tutto il resto

CorsaCareer non guida. Prepara una sessione, la consegna ad Assetto Corsa e poi
aspetta il referto che Assetto Corsa scrive alla fine. Nient'altro entra nella
carriera.

```
CorsaCareer                Content Manager            Assetto Corsa
    │                            │                          │
    │ scrive il preset ─────────▶│                          │
    │ (.cmpreset)                │  avvia la sessione ─────▶│
    │                            │                          │  si guida
    │◀───────────── race_out.json ◀─────────────────────────│
    │ verifica e archivia        │                          │
```

Quando Content Manager **non** è installato — come sul PC di sviluppo — il
programma risolve la sessione da solo con il proprio simulatore e lo dichiara:
nello storico, nella timeline e nella testata compare «simulato». La catena è la
stessa: stesso oggetto risultato, stesse conseguenze, stesso giornale
(`MainFormSimulation.cs`). Per questo il passaggio al PC con Assetto Corsa non
richiede di riscrivere niente: basta che Content Manager venga trovato.

---

## 2. Cosa serve sul PC di gioco

| Cosa | Dove | Note |
|---|---|---|
| Assetto Corsa | Steam | Serve il gioco base installato e avviato almeno una volta |
| Content Manager | https://acstuff.ru/app/ | Va bene la versione gratuita; la Full sblocca opzioni non necessarie qui |
| .NET 9 SDK | https://dotnet.microsoft.com/download | Solo per compilare. `CorsaCareer-Install.ps1` lo installa se manca |
| Contenuti (auto e piste) | Cartella di Assetto Corsa | Vedi §5: servono almeno due auto da gara compatibili e un circuito |

---

## 3. Primo avvio, in ordine

1. Copia l'intera cartella `CorsaCareer1991` sul PC di gioco.
2. Apri PowerShell nella cartella ed esegui:

```powershell
.\Avvia-CorsaCareer.ps1
```

Il lanciatore cerca l'SDK, ricompila (serve anche a copiare le immagini nuove
accanto all'eseguibile) e avvia il portale.

3. Alla prima apertura crea il pilota. La carriera reale viene salvata in
   `Documenti\Assetto Corsa\CorsaCareer`.

4. Apri **CONTENUTI** nel portale e controlla la riga di stato: deve dire
   quante auto da gara e quanti circuiti sono stati riconosciuti. Se dice
   «contenuti fittizi di prova», il programma sta ancora usando `ac-finto`:
   vedi §4.

> **Per provare senza toccare la carriera vera**: `.\Avvia-CorsaCareer.ps1 -NuovaCarriera`
> usa una cartella isolata sotto `debug-carriera\` e il catalogo finto.

---

## 4. Dove il programma cerca Assetto Corsa

In quest'ordine (`Program.cs`, `BuildRounds` e `AssettoCorsaRoot`):

1. la variabile d'ambiente **`CORSACAREER_AC_ROOT`**, se la cartella contiene `content\`;
2. la radice salvata nell'indice della carriera precedente (`content-index.json`);
3. i percorsi Steam standard:
   - `C:\Program Files (x86)\Steam\steamapps\common\assettocorsa`
   - `%ProgramFiles%\Steam\steamapps\common\assettocorsa`
   - le librerie aggiuntive lette da `libraryfolders.vdf`.

Se Assetto Corsa è su un altro disco e non viene trovato, fissalo così — una
volta sola, dal pannello di Windows «Variabili d'ambiente», oppure per la
singola sessione:

```powershell
$env:CORSACAREER_AC_ROOT = 'D:\SteamLibrary\steamapps\common\assettocorsa'
.\Avvia-CorsaCareer.ps1
```

Nel portale c'è anche **CONTENUTI → scegli cartella**, che fa la stessa cosa e
la ricorda nell'indice.

---

## 5. I contenuti: cosa installare perché la carriera funzioni

Il programma classifica ogni auto installata in un **gradino** della scala
(`CareerLadder.ForCar`) leggendo `ui_car.json`: la categoria dichiarata
(`class`/`tags`), i cavalli e il peso.

| Gradino | Categoria | Cosa installare (esempi) |
|---|---|---|
| 1 | Kart a quattro tempi | kart da noleggio (≤15 CV) |
| 2 | Kart a due tempi | kart OK/OKJ |
| 3 | Kart 125 con cambio | KZ125, shifter (≥30 CV) |
| 4 | Formula d'ingresso / GT4 / monomarca | Tatuus F4, Formula Vee, Abarth 500 |
| 5 | Formula nazionale / GT3 | Dallara F3, GT3 |
| 6 | Formula internazionale / prototipi | F2, LMP |
| 7 | Vertice | F1, hypercar |

**Regole pratiche da rispettare prima di cominciare una carriera:**

- **Almeno due auto da gara compatibili per ogni gradino che vuoi correre.** Con
  una sola auto il weekend non parte: non esiste una griglia. Il programma lo
  dice con un avviso esplicito («griglia insufficiente»).
- **Non lasciare buchi nella scala.** Se hai il kart e poi solo la Formula 1, la
  carriera salta dal gradino 1 al 7 — il salto è consentito solo verso il primo
  gradino *popolato*, quindi lì diventa l'unica strada possibile.
- **Le auto stradali vengono ignorate** (`ContentCategoryRules.IsRaceable`): non
  entrano né in griglia né nelle offerte.
- **La strada scelta al bivio conta**: dopo il kart il programma propone sedili
  della disciplina scelta (monoposto, durata, turismo). Se installi solo GT, la
  scelta «monoposto» non avrà nulla da offrire e si ripiegherà su quello che c'è.

Dopo ogni installazione di contenuti nuovi: **CONTENUTI → aggiorna**, altrimenti
l'indice resta quello vecchio.

---

## 6. Come viene trovato Content Manager

In quest'ordine (`Program.cs`, `LocateContentManager`):

1. `Desktop\Content Manager.exe`
2. `%LOCALAPPDATA%\AcTools Content Manager\Content Manager.exe`
3. `%ProgramFiles(x86)%\Content Manager\Content Manager.exe`
4. i collegamenti `.lnk` sul Desktop il cui nome contiene «content manager»
   (viene letta la destinazione del collegamento).

**Se nessuno di questi esiste, il programma non si ferma: risolve la sessione da
solo.** È il comportamento voluto su un PC senza Assetto Corsa, ma su quello di
gioco è il difetto da riconoscere per primo. Nel registro compare:

```
[INFO] sessione: Content Manager non installato: la sessione viene risolta dal programma.
```

Se lo vedi sul PC di gioco: metti un collegamento a Content Manager sul Desktop
e riavvia il portale.

---

## 7. Cosa succede quando premi il pulsante rosso

`LaunchWeekend` / `LaunchTestSession` / `LaunchInvitation` in `Program.cs`:

1. **Sceglie auto, circuito e avversari** dai contenuti reali. La griglia viene
   costruita da `RaceGridSelector`: prima le auto della stessa categoria, poi —
   dichiarandolo — una griglia mista.
2. **Costruisce il piano di sessione** (`SessionPlanner`): distanza dalla
   lunghezza del circuito, meteo fra quelli installati, orario, livello IA.
3. **Scrive il preset** di Quick Drive:

```
%LOCALAPPDATA%\AcTools Content Manager\Presets\Quick Drive\CorsaCareer - S01 GP 03 Suzuka.cmpreset
```

4. **Valida il preset** (`ContentManagerPresetValidator`). Se non passa, nessuna
   sessione viene registrata e ti viene detto perché.
5. **Registra il weekend pendente** in `pending_weekend.json` dentro la cartella
   della carriera: modalità, auto, circuito, preset, ora di lancio e
   **l'impronta del `race_out.json` esistente prima del lancio**. È il dato che
   impedisce di far passare un referto vecchio per nuovo.
6. **Apre Content Manager** con:

```
acmanager://race/quick?presetFile=<percorso del preset>
```

Questa URI esegue il preset direttamente, senza fermarsi sulla scelta «Apply
preset / Just Go» (`ContentManagerLaunch.cs`).

---

## 8. Il ritorno: il referto

Finita la sessione, Assetto Corsa scrive il risultato. Il programma lo cerca in
(`AssettoCorsaResultLocator.cs`):

1. `Documenti\Assetto Corsa\out\race_out.json` ← il percorso normale
2. `Documenti pubblici\Assetto Corsa\out\race_out.json`
3. `out\race_out.json` accanto all'eseguibile di CorsaCareer

Un timer controlla ogni 2 secondi (`resultTimer`), e il pulsante **CONTINUA**
forza il controllo subito.

Perché un referto venga accettato devono valere **tutte** queste condizioni
(`TryImportRaceResult`):

| Condizione | Se non è vera |
|---|---|
| Esiste un `pending_weekend.json` | «Risultato reale trovato, ma non esiste un weekend associato» |
| Il file è più recente dell'ora di lancio | Viene ignorato in silenzio (si continua ad aspettare) |
| L'impronta è diversa da quella pre-lancio | «Il race_out.json è identico a quello presente prima dell'avvio» |
| Il JSON contiene una classifica leggibile | «Non contiene una classifica di gara leggibile» |
| Circuito e auto coincidono con quelli lanciati | «Circuito diverso» / «auto diversa» (`RaceImportIdentity`) |
| Sono passate meno di 6 ore dal lancio | Il weekend viene archiviato come non concluso |

**Se dopo una gara il file non compare**: apri una sessione qualunque in Content
Manager, concludila, e controlla se `Documenti\Assetto Corsa\out\race_out.json`
esiste e ha la data di adesso. Se non esiste, il problema è nella
configurazione di Assetto Corsa/Content Manager, non in CorsaCareer: il
programma non può inventarsi un risultato che il simulatore non ha scritto. Nel
frattempo puoi copiare a mano il referto in `out\race_out.json` accanto
all'eseguibile: è uno dei percorsi cercati.

Il registro della carriera scrive ogni controllo:

```
Documenti\Assetto Corsa\CorsaCareer\logs\corsacareer.log
[INFO] import: controllo referto: modalità=race, avvio=..., file=..., aggiornato=...
```

---

## 9. Il livello dell'IA: l'unico parametro da tarare in pista

È il numero che decide se le gare sono una passeggiata o un muro
(`SessionPlanner`). Il piano lo calcola dal gradino di categoria e dal livello
di campionato; poi **si autocorregge dai referti reali**
(`UpdateAiCalibration`): se chiudi sistematicamente davanti a tutti, il livello
sale; se sei sempre ultimo, scende.

- La correzione automatica si accende/spegne da **IMPOSTAZIONI** (`AutoCalibrateAi`).
- Lo scostamento accumulato è `AiCalibrationOffset` nel salvataggio.
- Servono **almeno tre o quattro gare vere** perché la taratura abbia senso: le
  prime due sessioni su un PC nuovo vanno prese come misura, non come giudizio.

---

## 10. Collaudo in pista, in ordine

Da fare la prima volta sul PC con Assetto Corsa. Ogni passo va superato prima
del successivo.

1. **Contenuti** — CONTENUTI mostra il numero giusto di auto da gara e circuiti,
   e non dice «fittizi».
2. **Preset** — premi il pulsante rosso su un test: il file `.cmpreset` compare
   in `Presets\Quick Drive` e Content Manager si apre da solo sulla sessione
   giusta (auto e circuito che il briefing aveva annunciato).
3. **Referto** — concludi il test. Tornando sul portale, entro pochi secondi il
   tempo sul giro compare nella scheda e nella timeline, **senza** la dicitura
   «simulato».
4. **Gara** — stessa cosa con un round di campionato: posizione, punti, premio e
   danni devono comparire nello storico.
5. **Identità** — prova a concludere in Content Manager una sessione *diversa*
   da quella lanciata: il programma deve rifiutarla dicendo «circuito diverso».
   È la prova che nessun referto estraneo può entrare.
6. **Difficoltà** — dopo tre gare guarda se il livello IA si è mosso nella
   direzione giusta.

---

## 11. Quando qualcosa non va

| Sintomo | Causa quasi certa | Cosa fare |
|---|---|---|
| «Sessione risolta dal programma» sul PC di gioco | Content Manager non trovato | Collegamento sul Desktop (§6) |
| Content Manager si apre ma chiede «Apply preset» | Versione che non gestisce la URI `race/quick` | Aggiorna Content Manager |
| «Griglia insufficiente» | Una sola auto compatibile installata | Installa una seconda auto della stessa categoria |
| «Il race_out.json è identico» | La sessione non è stata conclusa davvero | Concludi la gara fino alla bandiera |
| «Circuito diverso» | In CM è stata cambiata pista dopo l'apertura del preset | Rilancia dal portale, non da CM |
| Il weekend resta «in attesa» per sempre | Referto mai scritto | §8, verifica manuale del file |
| Le gare sono troppo facili o impossibili | Livello IA non ancora tarato | §9, corri tre gare e ricontrolla |
| Immagini nuove non compaiono | Avviato l'exe senza ricompilare | Usa sempre `Avvia-CorsaCareer.ps1` |

---

## 12. Variabili d'ambiente

| Variabile | A cosa serve |
|---|---|
| `CORSACAREER_AC_ROOT` | Forza la cartella di Assetto Corsa |
| `CORSACAREER_HOME` | Forza la cartella della carriera (salvataggi, log, media) |
| `CORSACAREER_UI_AUTOMATION=1` | Modalità non presidiata: nessuna finestra modale, gli avvisi finiscono nel registro. La usa il banco di prova |
| `CORSACAREER_DEMO_DRIVER` | Nome del pilota creato automaticamente in modalità non presidiata |

---

## 13. Il banco di prova, senza Assetto Corsa

Serve a verificare la carriera anche su un PC dove non si può guidare.

```powershell
# collaudo completo (compilazione + suite)
.\verifica.ps1

# percorre una carriera intera e racconta cosa succede
dotnet run --project tests\CareerSim\CareerSim.csproj -- --stagioni 8 --pilota Prova
```

`CareerSim` compila **gli stessi sorgenti del gioco** e chiama i metodi veri del
portale: prepara i preset esattamente come farebbe un giocatore, poi risolve la
sessione con il simulatore interno al posto di Assetto Corsa. Quello che qui
arriva in fondo, arriva in fondo anche giocando; quello che qui si blocca, è
bloccato anche per chi gioca.

Con `--casa <cartella>` la carriera simulata resta su disco e può essere aperta
nel portale vero:

```powershell
dotnet run --project tests\CareerSim\CareerSim.csproj -- --stagioni 5 --casa D:\prova-carriera
$env:CORSACAREER_HOME = 'D:\prova-carriera'
.\Avvia-CorsaCareer.ps1
```
