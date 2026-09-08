# CorsaCareer — specifica narrativa e mediatica

Questa specifica integra l'obiettivo persistente del progetto. La carriera non è una lista di gare: è la storia pubblica e privata di un pilota, dal primo test fino al massimo campionato realmente raggiungibile con i contenuti installati.

## Percorso dal nulla alla Formula 1

Il pilota parte da una rookie evaluation e da opportunità coerenti con auto e circuiti disponibili. Ogni stagione può aprire più percorsi: kart/entry-level, monomarca, turismo, GT, endurance, formule junior, categorie storiche e infine il livello più alto compatibile. La Formula 1 è una possibilità solo quando esistono contenuti e requisiti adeguati; non viene promessa né inventata.

Ogni passaggio nasce da fatti persistenti: risultati, confronto con il compagno, reputazione, affidabilità, sponsor, disponibilità economica, obiettivi raggiunti, test e relazioni. Una stagione negativa può chiudere sedili e sponsor; una stagione eccezionale può produrre offerte e promozioni.

## Racconto della carriera

Il sistema deve conservare una timeline leggibile con debutto, primo contratto, primo test, prima qualifica, primo podio, prima vittoria, titoli, cambi di squadra, rivalità, crisi, ritorni, gara 50/100, record e ritiro. Ogni voce deve collegarsi a un evento reale importato dal simulatore o a una decisione reale del manager.

Il racconto deve distinguere:

- fatti verificati: posizione, tempi, punti, contratto, trasferimento, sponsor e contenuti usati;
- interpretazione editoriale: titolo, analisi, pagella, reazione del paddock e prospettive;
- decisioni del giocatore: firma, rifiuto, test, negoziazione e obiettivi.

Non si devono inventare risultati, vittorie, immagini o dichiarazioni attribuite a eventi mai accaduti.

## Media center

Dopo ogni gara il motore valuta l'importanza dell'evento. Un normale risultato anonimo può restare nello storico; debutti, podi, vittorie, ritiri decisivi, sorpassi in classifica, cambi di squadra e titoli generano articoli più importanti.

Ogni articolo contiene titolo, occhiello, testo, data, campionato, evento, team, pilota, importanza, fatti sorgente e immagine collegata quando disponibile. Il feed deve ricordare il passato: una vittoria dopo una lunga crisi va raccontata diversamente da una vittoria consecutiva.

Esempi di tono:

- `PRIMO PODIO: IL PADDOCK INIZIA A PRENDERE SUL SERIO IL ROOKIE`;
- `QUATTRO PUNTI IN DUE GARE: LA LOTTA PER IL TITOLO CAMBIA VOLTO`;
- `DOPO DUE STAGIONI, IL PILOTA CERCA UNA NUOVA CASA`;
- `UN RITIRO CHE PESA: IL TEAM PERDE TERRENO NELLA CLASSIFICA`.

Il testo locale deve funzionare senza servizi esterni tramite template e fatti strutturati. Un provider AI opzionale potrà solo variare lo stile usando dati verificati e non potrà aggiungere eventi.

## Foto e archivio visivo

Al termine di un evento il programma cerca screenshot reali nella cartella di Assetto Corsa, li associa a pilota, auto, circuito, sessione e data, e li copia nell'archivio della carriera. L'articolo può mostrare la foto della gara, una foto del circuito o un'immagine della vettura già presente localmente. Se non esiste una foto, la card deve dichiararlo senza creare un'immagine falsa.

Il Media Center deve proporre rassegne navigabili, non solo una tabella: “Rassegna consigliata”, “Weekend e gare”, “Test e sviluppo”, “Mercato e carriera”, “Timeline completa” e “Archivio”. L’utente vede un elemento alla volta con anteprima e fonte, può avanzare o tornare indietro e può archiviare un media senza cancellarlo dal salvataggio; i contenuti archiviati restano recuperabili dalla sequenza Archivio.

## Esperienza del pilota

Prima di ogni weekend il manager presenta contesto, aspettative, obiettivi, compagno, rivali, classifica e pressione mediatica. Dopo il weekend mostra analisi, conseguenze economiche, reazioni dello sponsor, rapporto col team, offerte future e articoli. A fine stagione produce un dossier con risultati, classifica, pagelle, foto, articoli, premi, record e prospettive per l'anno successivo.

Il risultato finale deve far ricordare al giocatore non solo `quanto` ha ottenuto, ma `come` è arrivato dal primo sedile fino al vertice.

## Audio e telecronaca

Il manager deve prevedere un commento audio originale in italiano, con tono da telecronaca automobilistica classica e moderna, senza imitare la voce o le frasi riconoscibili di un commentatore reale. Il testo deve essere costruito esclusivamente da eventi verificati:

- presentazione del circuito e del weekend;
- briefing, test e novità tecniche;
- qualifiche, partenza, duelli, sorpassi, pit stop e bandiere quando rilevabili;
- risultato, campionato, sponsor, mercato e conseguenze narrative;
- servizi speciali per debutti, podi, vittorie, ritiri, trasferimenti e titoli.

La pipeline deve separare `AudioScript` dai fatti, con provider locale/offline e provider TTS opzionale. Se l'audio non è disponibile, il testo resta leggibile nel Media Center; la carriera non dipende da servizi online o da una voce specifica.

## Rubrica televisiva e pagine magazine

La home deve funzionare anche come una rubrica motoristica televisiva originale: all'avvio il narratore presenta la data narrativa, il fatto corrente e l'aggiornamento dal paddock; il pulsante `Ascolta il diario` permette di riascoltare il servizio in qualsiasi momento. Il tono richiama l'energia delle rubriche sportive italiane degli anni Ottanta, ma testo e voce devono essere originali e non imitare Zermiani, Poltronieri o altri commentatori identificabili.

Ogni evento importante genera una pagina magazine dedicata: titolo, occhiello, data narrativa, racconto basato sui dati importati, foto reale se disponibile e illustrazione dichiarata quando è AI. Il layout può ricordare una rivista motoristica dell'epoca, ma deve usare una testata e una grafica originali, senza riprodurre pagine o marchi di Autosprint.

## Obiettivo persistente esteso — Assetto Corsa Career Universe

Questa sezione recepisce integralmente il super-prompt di progetto. È il riferimento funzionale per le prossime iterazioni: una schermata esistente non vale come funzione completata finché il flusso dati → logica → persistenza → UI → verifica non è funzionante.

### Visione e contenuti dinamici

Assetto Corsa resta il motore di guida; CorsaCareer è il mondo persistente attorno al pilota. Ogni gara deve avere contesto, aspettative, avversari, conseguenze, memoria e narrazione. Il sistema deve rilevare automaticamente installazione, auto, circuiti e layout, skin/livree, team e metadati, classificare i contenuti con confidenza e costruire un database locale. Le categorie da tentare includono kart, rookie, one-make/cup, storico, turismo/TCR, GT4/GT3/GT2/GT1, prototipi/LMP/hypercar, formule junior/F4/F3/F2, formula storica, road, trackday, hillclimb, speciali. I circuiti conservano nome, paese, layout, lunghezza, tipologia, difficoltà, velocità indicativa e compatibilità; sono riconosciuti, quando possibile, permanenti, cittadini, storici, kartodromi, ovali, hillclimb, test track e fantasy. La classificazione incerta deve poter essere corretta manualmente.

`Aggiorna contenuti` integra nuovi file senza cancellare carriera, storico o foto già esistenti.

### Pilota, scuderie e contratti

Il primo avvio raccoglie nome, cognome, nazionalità, numero, sigla, nickname, avatar/foto, anno di nascita opzionale e livello iniziale. La progressione parte generalmente dal livello più basso disponibile, ma il Career Engine costruisce percorsi alternativi coerenti fino al massimo livello realmente installato (Formula 1 solo se esistono contenuti e requisiti). Il giocatore entra in una scuderia, non seleziona soltanto un'auto: team e livree derivano dai contenuti quando possibile, altrimenti sono team fittizi dichiarati, con logo/livrea, categoria, prestigio, competitività, risorse, reputazione, filosofia, piloti, storico e rapporto col giocatore.

Le offerte reali contengono categoria, auto, durata, stipendio, bonus, obiettivi, posizione attesa, confronto col teammate, prestigio, clausole, rinnovo e buyout. Il giocatore può accettare, rifiutare, negoziare, rinnovare o lasciare; il mercato resta attivo durante la stagione.

### Mondo persistente

I piloti AI hanno identità persistente, nazionalità, età, livello, velocità, costanza, aggressività, esperienza, reputazione, specializzazione, squadra, categoria, storico, risultati, rivalità e rapporto col giocatore. Possono migliorare, peggiorare, cambiare team, salire/scendere, vincere, perdere il sedile o ritirarsi. Il compagno è un personaggio centrale: punti, qualifiche, best lap, podi, vittorie, DNF e posizione media alimentano un rapporto che può essere neutrale, positivo, competitivo, rivalità o antagonismo. Team, manager, costruttori e piloti hanno relazioni narrative oltre ai valori interni.

Ogni stagione conserva nome, anno, categoria, calendario, team, piloti, regolamento, punti, classifiche, obiettivi e prestigio; alla chiusura viene archiviata senza cancellare nulla. La stagione successiva eredita piloti, team, rivalità, campioni, reputazione, statistiche, mercato e memoria mediatica.

### Weekend e integrazione reale

Il weekend presenta briefing, prove, qualifiche, gara, eventuale Gara 2/endurance, circuito, classifica, aspettative, obiettivi, rivali, statistiche e storia recente. Dopo la sessione mostra risultato, confronto teammate, campionato aggiornato, reputazione, conseguenze, articoli e fotografie.

L'adapter deve usare, quando disponibili localmente, Content Manager, preset, file risultati, log, CSP, plugin, shared memory o API. Deve configurare e lanciare la sessione, riconoscere il giocatore e gli avversari e aggiornare la carriera con il minimo inserimento manuale. I risultati restano esclusivamente quelli realmente conclusi in Assetto Corsa: posizione di partenza/arrivo, qualifica, best lap, tempi, distacchi, giri, DNF e, se rilevabili, pit stop, danni, penalità e risultati avversari. Mai risultati simulati, casuali o inseriti per avanzare.

La difficoltà può essere suggerita progressivamente osservando le prestazioni (AI strength, aggressività, categoria e difficoltà campionato), ma non deve usare rubber banding né falsificare risultati.

### Eventi, archi narrativi e memoria

Il Career Event Engine è la fonte comune di statistiche, storia, rivalità, reputazione, media, offerte e storytelling. Deve coprire almeno: avvio carriera, contratto, cambio team, prima gara/pole/podio/vittoria, testa campionato, titolo vinto/perso, rivalità, serie positiva/negativa, teammate battuto, promozione, offerta factory, rumor di mercato e traguardi gara 50/100.

Lo Story Arc Engine riconosce nascita, crescita, cambio, climax e conclusione di archi come prima stagione, primo titolo, rivalità, crisi, riscatto, passaggio GT3, lotta col compagno e rimonta. Ogni arco conserva data d'inizio, protagonisti, eventi collegati, importanza, stato, evoluzione e conclusione.

La timeline principale deve essere consultabile voce per voce, con debutto, podi, vittorie, titoli, trasferimenti, crisi, rivalità, record, 50/100 gare e ritiro. Le rivalità nascono solo da fatti (risultati ravvicinati, battaglie, compagno, campionato, sorpassi/contatti se rilevati) e restano persistenti.

### Giornalista, media e canali

Il mondo include un giornalista persistente con nome, avatar, testata, tono, personalità, memoria e rapporto storico col pilota. Il giornalista scopre la storia dai dati, non la inventa: ogni riferimento a stagioni passate deve essere verificabile nel database.

Pipeline: `CareerData → CareerEvents → StoryArcs → ImportanceScore → StorySelection → Article/VideoScript/Headline → PhotoSelection → MediaFeed`. Livelli di copertura: 0 nessuna notizia, 1 breve, 2 articolo, 3 servizio speciale, 4 grande speciale, 5 documentario storico. I canali possono includere una testata principale, paddock/mercato, approfondimenti, social e TV, ciascuno con tono distinto e nomi originali. Il Media Engine deve produrre speciali per prima vittoria, titolo, passaggio di categoria, 100 gare, cinque anni, rivalità, ultimo weekend col team e fine stagione, recuperando materiale storico.

### Foto, album e archivio

Le fonti sono screenshot reali di Assetto Corsa, catture automatiche, immagini utente, foto vettura e team. Ogni foto conserva stagione, gara, circuito, auto, team, pilota, evento e articolo. L'album filtra per stagione, circuito, auto, team, vittoria, podio ed evento importante; l'archivio storico conserva classifiche, risultati, contratti, squadre, compagni, rivali, articoli, foto, milestone e statistiche.

### Decisioni, reputazione e immersione

Il giocatore deve scegliere davvero tra team debole da prima guida, team forte da seconda guida, promozione anticipata, permanenza, campionato alternativo e cambio costruttore. Velocità, costanza, esperienza, professionalità, prestigio, reputazione generale e reputazione per categoria influenzano offerte, team, mercato, aspettative e interesse media. L'interfaccia deve mostrare conseguenze narrative comprensibili, non solo numeri.

La home ha look premium da videogioco (carriera, team, categoria, round, circuito, classifica, continua, notizie, prossimo evento, rivalità, mercato, stagione e archivio). Le presentazioni cinematiche compaiono solo quando i dati giustificano un evento decisivo.

### Architettura, database, AI opzionale e qualità

Il progetto va evoluto verso moduli separati: `ContentScanner`, `ContentDatabase`, `CareerEngine`, `ChampionshipEngine`, `DriverEngine`, `TeamEngine`, `ContractEngine`, `DriverMarketEngine`, `RaceSessionGenerator`, `AssettoCorsaAdapter`, `ResultImporter`, `CareerEventEngine`, `StoryArcEngine`, `RivalryEngine`, `ReputationEngine`, `MediaEngine`, `JournalistEngine`, `PhotoEngine`, `ArchiveEngine`, `SaveSystem` e UI. La soluzione robusta preferita è SQLite o equivalente con entità Career, Player, Driver, Team, Contract, Car, Track, Championship, Season, Event, Session, Result, Standing, Rivalry, Relationship, CareerEvent, StoryArc, NewsArticle, MediaItem, Photo, Milestone, DriverHistory, TeamHistory e Transfer.

Autosave, backup, migrazioni, versioning, più carriere e recupero da errore sono obbligatori. Un provider AI/LLM è opzionale e mai necessario per il funzionamento base: provider locale, remoto e template ricevono solo un pacchetto di fatti, protagonisti, contesto, arco e tono; non possono modificare il database né aggiungere fatti.

Ogni iterazione deve analizzare il repository, evitare duplicazioni, implementare il miglior blocco successivo, testare regressioni, aggiornare migrazioni e verificare il ciclo completo. Le priorità sono: integrazione reale, persistenza, correttezza, carriera, mondo vivo, storytelling, immersione, UI e polish. Il criterio finale è la memoria: ogni gara lascia dati, conseguenze, immagini e racconti che restano consultabili anni dopo.
## Estensione — simulazione completa della vita professionale

La carriera non deve limitarsi al rapporto fra sessione e classifica: deve simulare il percorso professionale del pilota. Un nuovo profilo parte da una reputazione quasi nulla, da un passato minore solo raccontato come premessa editoriale e dalla necessità di dimostrare il proprio valore nei test. Il Career Engine deve proporre un itinerario leggibile, con bivi e conseguenze:

`rookie evaluation → test comparativo → test di conferma → prima gara minore → stagione completa → promozione o crisi → categoria successiva`.

Ogni tappa ha requisiti verificabili: tempo-obiettivo, confronto con teammate e riferimento del team, affidabilità, professionalità, budget e disponibilità dei contenuti. Un test reale può produrre un'altra prova, un invito a una gara, un contratto da prima guida, un sedile da seconda guida oppure nessuna offerta. La decisione deve essere spiegata con un report e restare nella timeline.

### Media reattivi e giudizio editoriale

Il Media Engine deve reagire alla qualità del risultato, non limitarsi a registrarlo. Per ogni evento calcola un `EditorialImpact` usando posizione, aspettative, distacco dal teammate, serie precedente, importanza del campionato, pressione dello sponsor e conseguenze di mercato.

- Un risultato sorprendente genera titoli entusiastici, pagelle alte, richieste d'intervista e nuove proposte.
- Un risultato normale produce una breve cronaca e mantiene il dossier aperto.
- Un risultato molto negativo genera analisi severe ma fattuali: errori, ritmo, affidabilità, pressione e responsabilità vengono discussi senza inventare incidenti.
- Un ritiro o una crisi ripetuta può generare editoriali critici, perdita di fiducia, sponsor in revisione e rischio di mancato rinnovo.
- Un podio, una vittoria, una rimonta o un titolo produce servizi speciali, copertine, video-script, pagelle, reazioni dei tifosi e aggiornamenti del mercato.

Gli articoli devono cambiare nel tempo: richiamano gli articoli precedenti, ricordano promesse e crisi, confrontano il pilota con il compagno e mostrano quando la redazione cambia giudizio. Ogni frase sui fatti deve provenire da `CareerData`; il tono può essere celebrativo, prudente o severo, ma mai falsificare un risultato.

### Vita fuori dalla pista

Fra i weekend il calendario propone attività con costo, rischio e ritorno reputazionale: incontri con tifosi, autografi, scuole guida, eventi degli sponsor, presentazioni vettura, giornate stampa, riunioni con team manager e ingegneri, test privati, simulatore, coaching tecnico, allenamento fisico, interviste, conferenze, podcast, servizi fotografici e negoziazioni di contratto.

Ogni attività ha una scheda decisionale, una durata nella timeline, un esito e conseguenze su reputazione, denaro, rapporto col team, sponsor, fanbase, stanchezza e pressione mediatica. Un incontro con i tifosi può aumentare il seguito ma sottrarre tempo al simulatore; un evento sponsor può salvare il budget ma peggiorare la preparazione; una dichiarazione infelice può alimentare un articolo critico e una rivalità.

### Sponsor, premi e finanza narrativa

Gli sponsor hanno identità, settore, budget, valori, obiettivi e soglia di tolleranza. Possono chiedere risultati, presenze, contenuti, eventi e comportamento professionale. Il mancato rispetto genera richiami, articoli, revisione del contratto e possibile uscita. Premi di gara e campionato devono essere descritti in comunicati e collegati all'economia della stagione; stipendi, bonus, trasferte, test e preparazione devono rendere il budget una scelta reale.

### Portale-diario come centro della simulazione

La UI principale è il giornale vivo della carriera. La colonna centrale mostra il fatto corrente, risultati, classifica, sintesi degli eventi passati, calendario futuro e stato del percorso. La colonna laterale mostra link agli articoli, servizi, interviste, foto, comunicati sponsor, offerte e archivio. Ogni card apre un documento singolo, navigabile e archiviabile; le foto reali di Assetto Corsa sono collegate al servizio quando disponibili.

Il portale offre una sequenza quotidiana credibile: `data narrativa → agenda del giorno → scelta del pilota → evento/test/gara → referto reale → conseguenze → articolo → reazioni → prossima agenda`. Il narratore resta sempre controllato dall'utente; pagina e testo devono funzionare anche senza audio.
