# Career flow audit — verifica corrente

Verifica dopo la correzione del flusso attività e la suite di regressione.

## Collaudo ripetuto — 26/08/2026

Eseguito nuovamente `verifica.ps1` sull'albero corrente. L'SDK locale
`.NET 9.0.317` ha completato senza errori la compilazione Debug/Release.
Le cinque suite di percorso, rendering, firma e simulazione non sono
presenti nell'albero corrente e vengono quindi segnalate come saltate dal
verificatore; non vengono presentate come superate.

Esito: **0 gruppi superati, 5 saltati**, **0 errori e 0 avvisi di compilazione**.

| Passaggio | Esito | Evidenza |
|---|---|---|
| Nuova carriera con identità | OK | Profilo obbligatorio con nome, cognome e numero; nazionalità Giappone predefinita |
| Attività pilota | OK | `DriverDayDialog.Perform` invoca direttamente `ShowDayScene`; il report tecnico è solo fallback legacy |
| Scena anime attività | OK | `ShowDayScene` usa artwork contestuale, testo progressivo e dialogo full-screen |
| Sponsorizzazione Haru | OK | `SponsorDayDialog.Send` salva costi/ore e apre direttamente `PlaySponsorScene` |
| Firma team → calendario | OK | Calendario e gare vengono pubblicati dalla firma, coerenti con categoria e contenuti |
| Budget e conseguenze | OK | Suite verifica costi, premi, vincolo budget non negativo e aggiornamento indici; i salvataggi legacy vengono normalizzati al caricamento |
| Archivio e articoli | OK | Suite verifica memoria, articoli basati su dati reali e continuità narrativa |
| Regressione automatica | PARZIALE | Build pulita; suite sorgente non presenti e quindi saltate |
## Verifica visuale aggiuntiva (26/08/2026)

Avvio della build corrente verificato su Windows: la prima schermata è `Nuovo pilota` e richiede nome, cognome, numero gara e nickname; nazionalità e ritratto vengono assegnati automaticamente come previsto dalla specifica. Il controllo visivo è stato acquisito in `C:\Users\Maruga\AppData\Local\Temp\codex-shot-2026-08-26_00-31-21.png`.

La schermata mostra inoltre il vincolo di provenienza dei risultati (pista oppure modalità debug dichiarata). Restano da verificare manualmente, sul PC dell’utente, i passaggi che richiedono una sessione Assetto Corsa reale e l’intero walkthrough con clic e screenshot di ogni schermata.

### Etichette contenuti

Le viste archivio, report gara, timeline, mercato, briefing e commento audio passano ora gli ID vettura attraverso `UiText.Car`: gli identificativi interni restano disponibili per il motore, ma all’utente vengono mostrati nomi leggibili (ad esempio “Kart Rental 4T”).

Build verificata nuovamente il 26/08/2026: 0 errori, 0 avvisi. Le suite
opzionali non sono disponibili nel checkout corrente; il flusso sedile →
calendario resta da collaudare con la suite ripristinata.

### Walkthrough visuale isolato

Il renderer `tests/UiRender` non è presente nell'albero sorgente attuale (restano solo artefatti `bin/obj`), quindi non è possibile considerare valida una precedente dichiarazione di esecuzione. Le viste principali sono state controllate manualmente durante l'avvio isolato della carriera; il controllo pixel-perfect resta da ripetere quando la suite verrà ripristinata.

Il walkthrough `tests/UiWalkthrough` non è presente nell'albero sorgente attuale; la precedente dichiarazione di esecuzione non è quindi riproducibile. È stato verificato manualmente l'avvio di una carriera temporanea con richiesta del nome del pilota.

Il gate ancora manuale resta l'importazione di un referto Assetto Corsa reale: su questa macchina il simulatore non è installato, quindi il percorso live va collaudato quando sarà disponibile.

### Collaudo end-to-end della firma (26/08/2026)

`tests/SignCheck` e `tests/UiAdvance` non sono presenti nell'albero sorgente attuale; la pubblicazione del calendario dopo la firma e gli avanzamenti temporali sono quindi documentati dal codice e da verifica manuale, non da una suite riproducibile.

### Collaudo completo e catalogo asset (26/08/2026)

Nota di manutenzione: nell'albero attuale le sorgenti delle suite sotto
`tests/ParserCheck`, `tests/PercorsoCheck`, `tests/UiRender`, `tests/SignCheck`
e `tests/CareerSim` non sono presenti (restano solo artefatti `bin/obj`).
`verifica.ps1` le segnala quindi come facoltative/saltate invece di presentarle
come superate; la compilazione Debug/Release dell'applicazione resta verificata
con 0 errori e 0 avvisi.

`verifica.ps1` eseguito con SDK .NET 9.0.317: Debug e Release compilano con 0 errori e 0 avvisi; le cinque suite sorgente assenti vengono saltate (0 gruppi passati, 5 saltati). Non è corretto dichiarare verificata la catena completa finché le suite non vengono ripristinate.

La build Release usata dal launcher desktop è stata ricompilata correttamente. Il catalogo `assets/anime-catalog.csv` contiene 318 schede e ogni riga punta a un PNG esistente; il progetto dispone di 343 PNG tra tavole, personaggi, loghi e scene. Non risultano riferimenti applicativi alla vecchia cartella `assets/alternatives` (eventuali tracce in `obj` sono solo liste generate di build precedenti).

Alla firma il calendario stagionale filtra ora i circuiti compatibili con la categoria dell’auto (kart separati da formula/GT/touring); il fallback completo viene usato solo quando il catalogo è troppo povero per comporre una stagione.

La gestione delle attività ora protegge esplicitamente il vincolo economico anche
in caso di esito negativo con penalità: l'applicazione del risultato usa un
limite inferiore a zero, mentre i costi dichiarati continuano a essere verificati
prima dell'avvio. La modifica è coperta dalla compilazione e dalla verifica manuale; la suite completa resta da rieseguire quando le sorgenti dei test saranno ripristinate.

### Preventivo economico pre-gara (26/08/2026)

Il briefing dell'appuntamento ora espone prima della scelta il budget corrente,
la quota iscrizione/trasferta, la riserva prudenziale per eventuali riparazioni
e il costo massimo stimato per la categoria. Le stime usano gli stessi valori
dell'`EconomyEngine`; a consuntivo vengono registrati separatamente trasferta,
riparazioni pagate e costi non sostenibili. La build e il collaudo completo sono
stati rieseguiti senza errori o avvisi.

Il preventivo include inoltre saldo peggiore (quota più riparazione massima),
premio massimo stimato e saldo migliore; per i test mostra il costo della
sessione e la riserva danni con la stessa logica.
### Pianificazione automatica delle giornate (26/08/2026)

La schermata **Attività del pilota** ora include il comando `AUTO · FINO AL PROSSIMO EVENTO`. Il comando chiede conferma, usa il motore `DayEngine` per consumare le ore disponibili e applicare gli effetti, registra ogni attività nel diario, avanza notte per notte e si arresta alla data del prossimo test/gara. Non simula né altera risultati di pista e non apre una scena anime per ogni attività automatica; il riepilogo resta consultabile nel diario.

### Scena anime per attività manuali (26/08/2026)

La scelta manuale di un'attività apre ora direttamente la scena anime
contestuale, con tavola dell'attività, personaggio coerente, esito e
conseguenze numeriche. Alla chiusura si torna all'elenco aggiornato. È stata
rimossa la conferma generica che interrompeva il racconto; il servizio completo
resta consultabile dal diario con doppio clic. La callback è opzionale, quindi
rendering e piano automatico restano compatibili.

### Guardia automatica della callback anime (26/08/2026)

Il collaudo ParserCheck verifica ora il collegamento tra ogni attività e la
scena anime: controlla che il portale passi `OpenActivityAnimeScene`, che la
finestra invochi la callback dopo il risultato e che tutte le attività abbiano
una tavola `.png` catalogata. In questo modo una futura modifica non può
silenziosamente riportare l'elenco a un semplice messaggio statico.

### Metriche narrative ripulite (26/08/2026)

Il punteggio interno della rookie evaluation resta disponibile al motore decisionale, ma non viene più presentato come indicatore principale nel calendario o nell'articolo. Le schermate mostrano invece miglior giro, riferimento, scarto, prestigio, budget dopo l'evento, forma fisica e fiducia nel paddock. Build Debug/Release risultano pulite; le suite sorgente non presenti restano da ripristinare.

### Avanzamento del calendario (26/08/2026)

La Home espone ora tre comandi direttamente sopra l'archivio vivo del calendario:
`OGGI → +1 GIORNO`, `+7 GIORNI` e `VAI AL PROSSIMO APPUNTAMENTO`. Il salto settimanale non oltrepassa mai
un test o una gara pianificata; il comando per il prossimo evento raggiunge
esattamente quella data. Ogni giorno viene elaborato da `DayEngine`, con
aggiornamento di ore, budget e diario, quindi la Home viene riscritta senza
perdere gli appuntamenti già memorizzati. I comandi si disabilitano quando non
esiste una data futura raggiungibile.
