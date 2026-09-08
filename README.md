# CorsaCareer - programma esterno

Career Engine v8.

> **Passaggio al PC con Assetto Corsa:** tutto quello che serve per collegare la
> carriera al simulatore vero — giunture, preset di Content Manager, parametri
> che ci viaggiano dentro, ritorno del referto, collaudo in pista e lavoro che
> resta — è in [PASSAGGIO-PC-ASSETTO-CORSA.md](PASSAGGIO-PC-ASSETTO-CORSA.md).

Questa è la prima demo desktop autonoma. Non richiede di aprire l’app Python in Assetto Corsa: gestisce carriera, calendario, media, budget, sponsor e risultati in una propria finestra. Al primo avvio costruisce il campionato usando i circuiti e le auto installati.

La direzione narrativa e mediatica completa è descritta in [CAREER_STORY_SPEC.md](CAREER_STORY_SPEC.md): il racconto dal debutto all'eventuale Formula 1, articoli basati sui fatti, memoria giornalistica e archivio fotografico sono requisiti del prodotto, non contenuti decorativi. La redazione propone i profili narrativi originali di Mario Poltroni o Ezio Zermignani e il commento tecnico di Cally Ragazzoni, ex pilota svizzero del mondo narrativo dell’app; non vengono imitate persone o voci reali.

Il nuovo stile **Cronaca classica analitica**, ispirato alla struttura delle telecronache storiche condivise dall'utente, ordina il servizio in tempi, distacchi, tecnica e voce del box. È un profilo originale: non riproduce frasi, voce o manierismi di telecronisti reali. Dal pannello Redazione si può scegliere anche `Paddock e interviste` o `Redazione moderna`.

Per l'audio, `Impostazioni` ora salva davvero la scelta e il pulsante principale la usa. È disponibile anche `Browser — voce del browser`: apre una pagina locale con le voci italiane esposte da Edge/Chrome e controlli **Avvia/riprendi** e **Ferma**. Questa è l'unica integrazione affidabile senza API cloud o chiavi a pagamento; le voci restano quelle offerte dal browser installato.

La pagina browser è ora una vera edizione digitale fittizia CorsaCareer Motorsport: testata, menu, ticker, notizie laterali, articolo principale, dossier e controlli audio. Le voci vengono filtrate per lingua italiana e ordinate dando priorità a `it-IT`; il pulsante `Prova voce` consente di scartare subito una voce con pronuncia/accento non adatto.

La home dell'applicazione è stata trasformata in un portale-diario: cronologia e prossimo appuntamento, risultati reali, classifica, riepilogo tecnico, galleria fotografica e una colonna di link agli articoli (storia del pilota, ultimo servizio, test, mercato, classifiche e archivio). Le immagini vengono prese dagli asset locali inclusi nel pacchetto e aperte in anteprima con un clic.

Un profilo nuovo parte ora dalla **rookie evaluation**: nessun contratto preassegnato, una vettura installata per il primo test, un tempo-obiettivo e un percorso a esito reale. Il risultato importato da Assetto Corsa aggiorna il miglior riferimento: se il tempo è sufficiente compaiono le prime offerte di gare/campionati minori; se è vicino viene proposto un altro test; se è lontano la valutazione resta aperta. Solo dopo aver convinto il paddock viene firmato il primo contratto.

All'apertura di una nuova carriera viene pubblicato il numero zero come storia browser guidata: dossier del pilota, percorso dalle categorie minori e briefing del primo test con soglia e criteri di valutazione. Un indice laterale, la barra di avanzamento e i comandi avanti/indietro accompagnano l'utente un capitolo alla volta; ogni capitolo può essere ascoltato separatamente. Il collegamento `Storia guidata` nella home permette di rivederla.

## Avvio

Per un’installazione locale senza configurazioni manuali, copia la cartella `publish` e fai clic destro su `CorsaCareer-Install.ps1`, quindi scegli **Esegui con PowerShell**. Lo script crea le cartelle dati e media in `Documenti\Assetto Corsa\CorsaCareer`, prepara il collegamento `Corsa Career.lnk` sul desktop e avvia il manager. Non modifica Assetto Corsa e non scarica contenuti senza una richiesta esplicita dal programma.

Eseguire `CorsaCareer.exe` dalla cartella `publish` oppure usare il collegamento `Corsa Career.lnk` sul Desktop. I salvataggi sono in `Documenti\Assetto Corsa\CorsaCareer` (con backup versionati in `backups`); dal pannello `Carriere salvate` puoi ripristinare l’ultimo backup con conferma. La scrittura è atomica e all’avvio viene recuperato un eventuale file temporaneo valido. Lo schema corrente è versionato e migra automaticamente le carriere precedenti, inclusi dati sponsor e redazione.
Oltre ai salvataggi dopo ogni operazione importante, il manager esegue un autosave atomico silenzioso ogni 60 secondi; l’autosave non crea una cascata di backup, mentre le modifiche importanti continuano a generare copie versionate.
Durante un weekend reale pendente sono bloccati cambio carriera, nuova carriera, modifica profilo e mercato: il risultato successivo deve restare associato alla sessione e al pilota che l’hanno generato.

Prima di `Nuova carriera`, se esistono risultati o eventi, CorsaCareer chiede se creare una copia nominata nella cartella `careers`; la nuova carriera non sovrascrive quindi il percorso precedente senza conferma.

## Formato reale del weekend

Il weekend non ha più un formato fisso. `SessionPlanner` costruisce ogni sessione dai contenuti realmente installati e la stessa carriera allo stesso round produce sempre le stesse condizioni (nessun `Random`: il weekend resta riproducibile e verificabile dai test).

- **Distanza**: calcolata sulla lunghezza dichiarata del circuito, con un obiettivo in chilometri per categoria (circa 20 km per il kart, 60 km per turismo/GT4/formule minori, 100–120 km per GT3/prototipi/F3-F2, 140–160 km per GT1/LMP/hypercar/formula). Se `ui_track.json` non dichiara `length`, il numero di giri usa un fallback per categoria e il fallback viene **dichiarato** invece di spacciare una distanza inventata. La lunghezza viene letta anche dai circuiti multi-layout (`ui/<layout>/ui_track.json`) e normalizzata da chilometri, virgole o metri.
- **Meteo**: scelto solo fra i preset presenti in `content/weather`, con pesi che dipendono dal mese della data narrativa: l'inverno porta più nuvole e nebbia, l'estate più sereno. Se nessun meteo installato corrisponde alla stagione, viene usato un fallback dichiarato.
- **Orario e temperatura**: l'ora di partenza varia per round e resta entro le 08:00–18:00 supportate da Assetto Corsa senza CSP; la temperatura deriva da mese, ora e copertura nuvolosa. Anche il vento varia.
- **Round di durata**: una stagione con almeno quattro appuntamenti e una categoria adatta (GT, prototipi, storiche) assegna una gara di durata al circuito più lungo del calendario, con partenza pomeridiana e distanza aumentata.
- **AI**: livello e aggressività derivano dalla categoria e dal profilo scelto, non più da `95` e `0` cablati. Il turismo corre più a contatto delle formule; una griglia mista allarga la forbice fra i piloti AI.
- **Formato del round**: ogni appuntamento archivia un unico risultato di gara per
  round; qualifiche, sprint o sessioni aggiuntive vengono conservate nel referto
  quando Assetto Corsa le fornisce, senza duplicare il round nel calendario.

Da `Impostazioni` → `WEEKEND E DIFFICOLTÀ` si scelgono il profilo di difficoltà (`Assistita`, `Realistica`, `Professionale`, `Massima`), la distanza di gara (`Breve`, `Realistica`, `Lunga`) e la calibrazione automatica dell'AI. La calibrazione osserva **solo i referti reali già importati** e agisce esclusivamente sul livello AI del preset successivo: non è rubber banding, non modifica punti, posizioni o risultati archiviati, ed è limitata fra −10 e +6 punti. La home mostra in anteprima il formato del prossimo weekend calcolato dagli stessi dati che finiranno nel preset.

Le condizioni effettivamente scritte nel preset vengono archiviate nella carriera (`SessionPlans`) e nel referto di gara/test, così giornale, timeline e servizi audio possono citare meteo, temperatura, orario e distanza come fatti verificabili e non come colore aggiunto dopo.

`Prepara e apri Content Manager` crea un preset Quick Drive `.cmpreset` nella cartella dei preset di Content Manager, salva `pending_weekend.json` e usa la URI `acmanager://race/quick?presetFile=...` per avviare direttamente il preset, senza la scelta intermedia “Apply preset / Just Go”. Assetto Corsa riceve così auto, circuito, griglia e formato già preparati, invece di ricadere in una Practice predefinita. Il programma usa il materiale disponibile e notifica ogni fallback. Dopo una gara realmente conclusa, il timer importa `race_out.json`, aggiorna classifica, budget, sponsor, piloti AI, rivalità, media e salvataggio. La chiusura del `pending_weekend.json` avviene solo dopo la registrazione completa del test o della gara; se una registrazione fallisce, la sessione resta pendente e può essere ritentata senza perdere l’associazione. Il referto archiviato conserva anche SHA-256: se lo stesso file viene rilevato in un nuovo tentativo della medesima sessione, CorsaCareer evita di duplicare gara o test.
Se un weekend resta pendente e nel frattempo auto o circuito vengono rimossi, il preset viene rifiutato prima della riapertura in Content Manager; il risultato non può essere associato a contenuti diversi da quelli preparati.

Anche `Briefing e test` prepara una sessione reale solo con un'auto da competizione installata; se il salvataggio punta a un'auto stradale, viene scelta un'auto da gara disponibile oppure il test viene bloccato con una notifica. Il test usa lo stesso avvio diretto `race/quick` del weekend, ma con un preset di pratica/test senza gara o qualifica; questo comando è coperto da un test automatico dedicato. Il preset del test passa dallo stesso `ContentManagerPresetValidator` del weekend: prima veniva scritto senza controllo.
Per i test il parser accetta anche un `race_out.json` con sola telemetria (`bestLaps`/giri) e senza classifica: archivia il test senza posizione, punti o premio. Una gara, invece, viene accettata solo con `raceResult` completo.

L'auto del contratto viene conservata nel profilo e usata per costruire il preset del weekend; se non è più installata, il programma avvisa e propone esplicitamente un fallback disponibile.

Il parser dei risultati è verificato contro il file reale `Documenti\Assetto Corsa\out\race_out.json` e contro un fixture con qualifica/griglia con `dotnet run --project tests\ParserCheck\ParserCheck.csproj -c Release`. Il locator cerca il referto più recente tra i percorsi AC espliciti rilevati e non considera file non presenti. La build non avanza la carriera durante questo test. I risultati non vengono mai simulati.
Il parser riconosce anche `playerIndex`, `localPlayerIndex` o `player` quando presenti nel referto/adapter; se assenti mantiene il fallback del formato AC che serializza il pilota locale per primo.

Il parser rifiuta inoltre file incompleti o classifiche corrotte e verifica che il referto corrisponda al circuito, all’auto e al weekend pendente creati da CorsaCareer.
La rookie evaluation del debutto usa posizione d’arrivo, qualifica, confronto col compagno e giri completati; un ritiro reale mantiene lo stato “da rivalutare”. I dati mancanti restano neutri e non vengono inventati.
Prima dell’avvio viene memorizzato anche l’hash SHA-256 dell’eventuale `race_out.json` già presente: un file identico non può essere reimportato come nuova gara, anche se il timestamp è ambiguo.

`Aggiorna contenuti` salva un indice locale versionato di auto e circuiti. Al termine della scansione puoi correggere manualmente le categorie in `Revisione contenuti`; gli override vengono conservati in `content-index.json` e riapplicati alle scansioni successive.
La scansione è tollerante verso directory mod danneggiate o non leggibili: salta solo il percorso problematico e conserva gli altri contenuti rilevati. I `ui_car.json` e `ui_track.json` ufficiali con stringhe multilinea non conformi al JSON standard usano un fallback testuale per nome, marca, tag/classe, anno e paese del circuito e vengono segnalati come “JSON non standard”, non come contenuti mancanti.
Gli eventuali salti vengono registrati in `ScanWarnings` e mostrati nella finestra di revisione e nel riepilogo della scansione.
L’indice contenuti usa lo schema 3, con migrazione trasparente dei file precedenti.
Lo stesso vale per `ui_car.json` e `ui_track.json` corrotti: l’identificativo resta rilevato, ma la voce viene marcata con un avviso e con confidenza ridotta quando applicabile.
Quando una mod dichiara `category`, `class` o `discipline` nel proprio `ui_car.json`, CorsaCareer usa quel dato con confidenza 100; solo in assenza del metadato ricorre al nome della vettura/cartella.
Sono riconosciuti anche i tag testuali nell’array `tags` dello stesso file, utile per mod che descrivono la disciplina solo attraverso le etichette.
Se il metadato contiene `year`, `model_year` o `year_produced`, l’anno viene conservato nell’indice e usato per la data d’inizio narrativa; senza questo campo resta il fallback basato sull’ID dell’auto.

La home mostra `CONTENUTI PRONTI` oppure il motivo preciso per cui non è possibile preparare un weekend (auto da gara o circuito mancanti). CorsaCareer non scarica mod da URL non verificati: l’installazione/abilitazione resta in Content Manager, poi basta ripetere `Aggiorna contenuti`.

Il mercato segue la stessa regola: non propone contratti con auto stradali, placeholder o contenuti non installati. Se non esiste ancora un’auto da gara, la firma resta sospesa e viene indicato di aggiornare/installare i contenuti. Le squadre generate dal manager sono marcate esplicitamente come fittizie; non vengono presentate come team reali del mod. Quando l’auto possiede una cartella `skins`, la prima livrea rilevata viene mostrata nel dossier dell’offerta.
Alla firma, la livrea rilevata viene conservata nel profilo del pilota e mostrata anche nella home/paddock; se il mod non espone `skins`, resta dichiarata come non disponibile.
Quando un contratto termina al passaggio di stagione, il mercato viene riaperto automaticamente con offerte compatibili e l’evento viene archiviato come `CONTRACT_EXPIRED`.
All'avvio viene inoltre costruito un roster persistente di paddock (pilota, compagno e avversari) con auto/categoria realmente installate. Il roster contiene identità e attributi iniziali, ma zero gare, punti e vittorie finché Assetto Corsa non fornisce un referto reale.
Il primo popolamento genera anche l'evento editoriale `PADDOCK_ROSTER`, disponibile in diario, timeline, giornale e servizi audio; non è classificato come gara.
La home distingue il numero di piloti AI dal roster totale, che include anche il giocatore; in questo modo il conteggio non attribuisce erroneamente il profilo dell'utente all'intelligenza artificiale.
Il dossier contenuti/storia della home è un pannello scorrevole: sponsor, contratto, economia, roster e arco narrativo restano leggibili anche quando il testo supera l'anteprima iniziale.
Accettare un'offerta dal mercato verifica nuovamente che l'auto sia installata e da gara, rimuove l'offerta accettata, aggiorna sedile/compagno/sponsor/contratto e sincronizza il roster persistente; un'offerta con contenuto rimosso viene rifiutata senza modificare la carriera.
Il teammate precedente resta archiviato come `ex compagno`, mentre il nuovo viene promosso a compagno attuale: la storia dei rapporti non viene sovrascritta.

Da `Aggiorna contenuti` → `Revisione contenuti` il pulsante `Apri Content Manager` porta direttamente al gestore installato; dopo l’installazione si ripete la scansione e solo allora il contenuto entra nel calendario/mercato.
Le date dei round vengono generate a partire dall’anno narrativo associato al contenuto scelto e avanzano di tre settimane tra gli appuntamenti; non viene usato un anno storico fisso scollegato dalla carriera.
La progressione tratta la Formula 2 come categoria avanzata; il livello top è riservato alle auto classificate Formula 1/top, GT1, LMP o hypercar, evitando un salto diretto dalla categoria minore al vertice.
All'avvio, anche una carriera già salvata viene riallineata alla categoria dell'auto installata senza cancellare gare, punti o storico.
Il roster sincronizza inoltre team, auto e categoria del pilota e del compagno attuali dopo ogni riallineamento; gli ex compagni restano nello storico con la squadra precedente.

La stessa schermata permette di scegliere manualmente la radice di Assetto Corsa. Il percorso viene salvato in `content-index.json` e riutilizzato nelle scansioni successive, utile per librerie Steam secondarie o installazioni spostate.

Dopo l’installazione di un pacchetto ZIP verificato, la finestra si chiude e la home viene aggiornata sulla nuova scansione, evitando di mostrare un elenco obsoleto. La riallineatura rigenera anche calendario, categoria d’ingresso, offerte e roster del paddock in base ai contenuti appena rilevati, senza toccare risultati già importati. Le offerte per auto non più installate o non più classificate come da gara vengono rimosse; se necessario il mercato viene ricostruito con sole opportunità realmente disponibili.

`Scarica ZIP verificato` supporta anche pacchetti remoti: richiede URL HTTPS e SHA-256 del modder, limita il download a 256 MB, verifica l’hash e poi usa lo stesso installer sicuro. URL non HTTPS, hash errati e percorsi ZIP non consentiti vengono rifiutati.
Ogni installazione viene inoltre registrata in `content-index.json` con origine, SHA-256, numero di file e data, così la provenienza del contenuto resta consultabile e persistente.
Il pulsante `Archivio pacchetti` nella Revisione contenuti apre il dettaglio completo di queste registrazioni.

Nel dialogo `Mercato e scouting`, ogni sedile ha ora un dossier separato con team, auto, categoria, sponsor, compagno, stipendio, durata, prestigio e obiettivo prima della firma.

Dal Media Center il pulsante `Redazione` permette di personalizzare giornalista, testata, tono e memoria editoriale. La modifica genera un evento persistente e viene usata nelle pagine magazine successive.
I servizi audio riportano la stessa firma editoriale configurata, senza imitare voci reali e senza aggiungere fatti non presenti nel salvataggio.

Da `Revisione contenuti` puoi anche installare un pacchetto ZIP già verificato dall’utente, anche quando l’archivio contiene una sola cartella contenitore (`NomeMod/content/...`); README e anteprime nella radice contenitore vengono ignorati. L’estrazione è limitata alle cartelle Assetto Corsa previste (`content`, `extension`, `apps`, `system`) e blocca percorsi `..` o assoluti; dopo l’installazione la scansione viene aggiornata automaticamente.
L’installazione usa uno staging temporaneo: l’archivio viene estratto e controllato prima della copia nella radice AC, riducendo il rischio di lasciare file parziali in caso di ZIP corrotto o errore di lettura.

## Foto, test e archivio editoriale

Il pulsante `Briefing e test` può aprire un test reale in Content Manager. I dati di una sessione di pratica, quando presenti nel file risultato di Assetto Corsa, vengono archiviati in `TestHistory` senza assegnare punti o avanzare il campionato.

Nel Media Center puoi usare `Importa foto` per una cattura F8 salvata dal simulatore oppure `Cattura AC (3s)`: dopo il conto alla rovescia viene acquisita la finestra reale di Assetto Corsa. Per fotografie esterne usa la camera TV, una camera replay o una visuale esterna prima della cattura; il programma non trasforma una visuale cockpit in una foto esterna. Nei servizi audio il selettore rileva le voci italiane installate in Windows (per esempio Cosimo, Elsa ed eventuali altre voci OneCore/SAPI), oltre alla modalità automatica; la preferenza viene salvata nel profilo e, se una voce scelta non è più installata, torna automaticamente alla migliore disponibile.

Gli screenshot reali hanno priorità negli articoli. Se non esiste una cattura, il portale usa un’illustrazione editoriale dichiarata. Le pagine HTML in `Documenti\Assetto Corsa\CorsaCareer\media\magazine` contengono anche una copia locale dell’immagine e la didascalia della fonte.

Il Media Center non è solo una lista: il selettore propone rassegne editoriali (consigliata, gare, test, mercato, timeline e archivio), mentre il filtro separa tutti i media, foto/screenshot, gare, test, mercato e traguardi. I contenuti scorrono con avanti/indietro. `Archivia media` nasconde l’elemento dalle rassegne attive ma salva `Archived=true` nel career JSON; il contenuto resta recuperabile dalla rassegna Archivio. Rassegna, filtro e ultimo servizio letto vengono salvati, così la narrazione riprende dal punto raggiunto alla riapertura.
La home espone anche `▶ AVVIA RUBRICA TV`, mentre `Edicola e servizi` apre l’elenco editoriale: si seleziona una notizia alla volta e si può leggere la pagina completa, ascoltare il servizio, passare alla pagina precedente/successiva oppure archiviarla.
Il narratore non parte mai da solo: il pulsante in alto alterna `Avvia rubrica TV` e `Pausa / Ferma rubrica`.
La `Rassegna consigliata` è una regia automatica: ordina antefatto, fatto e conseguenze, presenta il tema della sequenza e permette di partire dal primo servizio con `▶ Inizia rassegna`; l’indice laterale serve solo per saltare direttamente a un media.
La costruzione delle rassegne è separata dalla finestra grafica in `MediaSequenceService`: filtri, ordine narrativo e recupero dell’archivio sono quindi coperti da fixture automatiche.
Il giornale e il nuovo pulsante `Ascolta questo` seguono sempre la scheda selezionata nella rassegna: è quindi possibile leggere e ascoltare in ordine lo stesso servizio, senza aprire per errore l'ultimo evento globale.

`Calendario campionato` apre il dossier dei round: distingue il prossimo weekend, i round pianificati, i circuiti mancanti e i weekend conclusi. Per ogni gara conclusa mostra soltanto il referto realmente importato, con posizione, punti, giri, qualifica, giro migliore e articoli collegati.

Dal Paddock il pulsante `Classifiche` apre la graduatoria completa, il dettaglio del pilota selezionato, gli ultimi referti archiviati e le stagioni concluse.

La Timeline verifica anche lo SHA-256 dei file `race_out.json` archiviati: un referto intatto è marcato `verificato`, mentre un file modificato viene segnalato come `VERIFICA FALLITA`. Il controllo è coperto da test automatici per entrambi i casi.
Ogni `RaceHistoryEntry` conserva anche il numero di stagione: il calendario non confonde più risultati di campionati diversi quando la data narrativa attraversa l'anno solare.
La Timeline mostra lo stesso identificatore (`S01`, `S02`...) nel titolo e nel dossier della gara.

Lo sponsor ora ha un obiettivo stagionale persistente: ogni arrivo reale entro il piazzamento richiesto incrementa il contatore, accredita il bonus gara e genera un evento quando il traguardo viene raggiunto. A fine stagione l’obiettivo viene marcato raggiunto o non raggiunto e resta nella timeline.

Dopo ogni gara importata si apre il `Servizio post-gara`: classifica completa del referto, posizione di partenza, qualifica, giri, best lap, distacco, pit stop, penalità, danni e conseguenze economiche. Il servizio consente anche di ascoltare il commento e aprire il Media Center; senza un referto reale questa schermata non viene generata.
Lo storico distingue inoltre primo podio, prima vittoria, vittorie successive e traguardi gara 50/100: queste milestone vengono create esclusivamente quando il contatore deriva da un referto di Assetto Corsa importato.

L’ultimo round apre invece il `Dossier fine stagione`: classifica archiviata, premio, vittorie, sponsor, contratto, budget e prospettiva per la promozione. Il pulsante di avanzamento resta bloccato finché il referto finale reale non è stato acquisito.

Il narratore distingue ora gara, test, firma, mercato, obiettivo sponsor, promozione, riepilogo mensile e fine stagione: un dossier concluso non viene più raccontato come un briefing senza risultato.

Il Media Center offre anche la lettura audio locale. La modalità automatica preferisce `Microsoft Cosimo` e poi `Microsoft Elsa Desktop`, mentre le voci installate possono essere selezionate singolarmente. La sintesi rallenta leggermente il ritmo e inserisce pause/prosodia SSML tra frasi e paragrafi. È una voce Windows migliorata, non l’imitazione di un commentatore reale; se il sistema non dispone di una voce italiana, il testo resta comunque disponibile e l’esportazione WAV usa il fallback installato.
## Provenienza dei risultati

Prima dell'importazione il file viene copiato in `media\results` con metadati e hash SHA-256; la Timeline conserva la fonte esatta usata per il risultato. Se una categoria ha una sola auto installata, viene proposta una griglia mista usando esclusivamente altre auto da competizione presenti e il fallback viene dichiarato.

Per una fotografia sintetica dello stato reale del progetto, consulta [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md).
