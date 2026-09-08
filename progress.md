Original prompt: creare un generatore di carriera automobilistica con Assetto Corsa, da zero a professionista, coinvolgente come un manga sportivo e accompagnato da molte immagini in stile fumetto.

## Iterazione corrente

- Impostata una direzione visiva originale a inchiostro e acquerello, senza copiare personaggi o tavole esistenti.
- Generate tre tavole coerenti: alba del rookie, primo test, incontro con il primo rivale.
- Integrate le tavole nelle aperture dei capitoli e nella selezione editoriale degli eventi iniziali.
- Conservata la simulazione come strumento di debug per i PC senza Assetto Corsa; ogni risultato resta marcato come simulato.
- Aggiunto `CONTINUA LA STORIA`, che porta al test, all'invito, al mercato, al referto o al weekend successivo in base allo stato.
- Esteso il collaudo visivo con il rendering dell'apertura illustrata del Capitolo I.
- Verifica completa superata: build pulita, 173 gruppi verdi, UI a tre risoluzioni e carriera fino alle prime vittorie.
- Libreria estesa da 3 a 13 tavole: sconfitta, invito, contratto, duello col compagno, crisi economica, podio, vittoria, promozione, professionismo e titolo.
- Aggiunto l'`Atlante illustrato`, consultabile dalla home, con distinzione fra momenti già vissuti e anteprime del percorso.
- La pipeline editoriale sceglie ora una tavola specifica per ciascuna famiglia di eventi; le immagini AI restano sempre dichiarate come interpretazioni e mai fotografie.
- Capitolo I in corso: inserito cast persistente (Marta Rinaldi, Gianni Valli, Nico Valenti), briefing prima del rookie test e scene diverse dopo offerta, test di conferma, invito o recupero. Nessuna scena modifica risultati o reputazione.
- Aggiunto il `Centro carriera`: una schermata unica per prossimo capitolo, calendario e gara, sessione Assetto Corsa, squadra e relazioni, sponsor e budget, proposte d'ingaggio e strada verso il professionismo. Ogni riquadro porta alla sezione operativa già esistente e l'azione principale resta contestuale.
- Il centro carriera esplicita anche la differenza fra referto reale di Assetto Corsa e simulazione di debug. Render visivo dedicato verificato a 1360×820.

## Prossimi passi suggeriti

- Aggiungere cast persistente (mentore, manager, rivale) con memoria e scene brevi dopo i risultati.
- Creare tavole dedicate a sconfitta, primo podio, prima vittoria, crisi e promozione.
- Trasformare il comando principale della home in una vera azione contestuale “Continua la storia”.
- Verificare il ciclo reale su un PC con Assetto Corsa quando disponibile.

## Aggiornamento immagini e calendario

- Calendario trasformato in archivio full-screen editoriale a due pannelli: cronologia colorata, dossier con dati tecnici e conseguenze, legenda e prossimo appuntamento.
- Aggiunta tavola originale `calendar-rookie-rain.png` al dossier del calendario, con crop panoramico senza bande nere.
- Generate e collegate tre nuove tavole originali: `manga-sponsor-table.png`, `manga-rival-grid.png`, `manga-night-garage.png`.
- Le nuove tavole entrano nelle scene del Capitolo I e nella mappa `EditorialAssets`, così gli articoli usano immagini coerenti per test, sponsor, rivalità, inviti e crisi.
- Render UI e verifica rapida ripetuti dopo l'inserimento immagini: 173 gruppi superati, build Release senza errori o avvisi.
- Il collaudo visuale ora include anche `market-scouting.png`: tre offerte sintetiche con loghi, radar d'interesse e tavola narrativa cambiano insieme al dossier selezionato.
- Collaudo completo finale dell'iterazione: compilazione Release, test funzionali, render multi-risoluzione e carriera fino alle prime vittorie tutti superati; 173 gruppi verdi, 1 verifica saltata prevista.
- Il cast ora ha memoria persistente: fiducia, ultima reazione e otto appunti per personaggio vengono salvati e aggiornati da test e gare con dati concreti (pista, tempo, posizione, punti e premio).
- Il Paddock usa questa memoria nella scheda delle persone e ridimensiona le due colonne sulla finestra full-screen; aggiunto render dedicato `paddock-memory.png`.
- Verifica aggiornata: 174 gruppi superati, inclusa la persistenza JSON della memoria del meccanico, senza errori di build.
- Rifinitura grafica del Paddock: controlli standard sostituiti da selezione piloti owner-draw, colori del design system, pulsanti coerenti e due colonne responsive; render controllato senza sovrapposizioni dopo la correzione dell'intestazione.
- Media Center reso responsive: feed editoriale a schede, articolo, immagine e barra azioni si ridimensionano insieme alla finestra; il render ora usa un evento di test sintetico e un'immagine manga disponibile, con distinzione esplicita fra catture AC reali e illustrazioni dichiarate.
- Corretta la testata del Media Center e verificato il layout a 1600×900: nessuna sovrapposizione, immagini visibili e navigazione dell'edizione attiva.
- Collaudo conclusivo dell'iterazione: Media Center incluso nel render con evento sintetico, tavola editoriale e barra azioni; 174 gruppi superati, build Release pulita.
- Gli articoli di gara ora includono anche l'angolazione `persone`: quando manager o meccanico hanno una reazione memorizzata, il pezzo cita nomi e memoria del box insieme ai dati del referto. Aggiunta verifica narrativa dedicata.
- Collaudo dopo la correzione: 175 gruppi superati, 1 saltato previsto; build, UI multi-risoluzione e carriera fino alle prime vittorie verdi.
- Opportunità e Agenda hanno ora un root responsive a tre righe: testata con budget/rapporti, corpo decisionale e barra azioni restano sempre visibili. Il render del dossier mostra costi, copertura, ritorno, rischio e scadenza senza perdere i comandi di accettazione/rifiuto.
- Aggiunto render dell'Agenda con catalogo attività, scelta a rischio, diario e pulsante di esecuzione: la testata con giorni liberi e budget e la barra inferiore restano visibili a 1600×900.
- La conferma della simulazione DEBUG ora espone prima del sì auto, pista, formato, IA, meteo, giri, tempo-obiettivo e catena di provenienza; il risultato continua a passare dallo stesso percorso del referto reale.
- Suite completa dopo l'estensione del briefing DEBUG: 175 gruppi superati. Il banco UIAdvance automatico è stato interrotto perché rimasto in attesa di una modale, senza modificare la build; il test funzionale principale resta verde.
- Il banco UIAdvance è stato reso non-modale in DEBUG: fixture `ac-finto`, home temporanea e guardie sui refresh/dialoghi del verdetto; una sequenza autonoma ha prodotto le schermate `00-partenza`, `01-passo` e `02-passo`.
- Il centro carriera ora mostra una tavola manga nel pannello “La strada”, affiancata alla progressione Rookie Test → Professionismo; render verificato senza coprire testo o azioni.
- Il verificatore controlla Debug e Release separatamente: il launcher desktop usa la Release aggiornata e l'avvio `.bat` è stato provato con una nuova carriera isolata.
# 2026-08-23 — pacchetto tavole alternative

- Creato un archivio di 50 immagini narrative aggiuntive per le scene.
- Le prime tavole sono illustrazioni originali generate con la skill `imagegen`; le varianti successive sono derivate in modo non distruttivo per offrire tagli e trattamenti cromatici differenti.
- `EditorialAssets.ForEvent` ora sceglie una tavola alternativa in modo deterministico per gli eventi con pista/titolo, mantenendo il fallback editoriale storico e la riproducibilità dell’archivio.
# 2026-08-23 — cast illustrato del Capitolo I

- Aggiunti nove ritratti originali: Marta, Gianni, Nico, Alex e cinque nuovi archetipi ricorrenti (Nori Senda, Mina Arisawa, Shigeo Kanda, Minato Nakahara, Noa Minazuki).
- Il cast persistente ora conserva ruoli, relazioni e memorie anche per i nuovi personaggi.
- Il Capitolo I mostra un ritratto accanto a ogni scheda del personaggio e a ogni battuta pronunciata, con asset AI dichiarati come illustrazioni narrative.
# 2026-08-23 — colonna sonora locale

- Copiati 31 brani MP3 dalla cartella Download in `soundtrack/`, senza rimuovere né alterare gli originali.
- Aggiunto `soundtrack/README.md` con una prima mappa narrativa per alba, concentrazione, griglia, sponsor, vittoria e crisi.
- La cartella viene inclusa automaticamente nelle build Debug/Release; il lettore integrato con selezione dinamica per capitolo sarà il prossimo passaggio.
# 2026-08-23 — lettore soundtrack integrato

- Aggiunto `SoundtrackService` con riproduzione locale MP3, pausa, ripresa, stop e volume.
- Aggiunta finestra `SOUNDTRACK · LIVE PADDOCK` raggiungibile dalla testata della home.
- La selezione iniziale suggerisce il brano in base al tipo di evento (test, griglia, sponsor, vittoria, crisi, alba), senza avvio automatico.
- Verifica rapida completata: 176 gruppi superati, 1 saltato.
# 2026-08-23 — musica dinamica e introduzione illustrata

- Implementato cambio brano per atmosfera con sfumatura: alba, concentrazione, griglia, sponsor, vittoria e crisi.
- L’introduzione guidata ora usa una grafica editoriale più curata, font gerarchici, immagine grande e tavola diversa per ogni capitolo/nuova carriera.
- Corretto il layout dei ritratti nei dialoghi del Capitolo I: il testo non viene più coperto dall’immagine.
- Verifica rapida: 176 gruppi superati, 1 saltato.

# 2026-08-23 — tavole vive e dialoghi a fumetto

- Il Capitolo I sostituisce l'elenco anagrafico del cast con battute a fumetto: ritratto grande, balloon leggibile e disposizione alternata delle vignette.
- Aggiunte espressioni narrative originali per Rei, Genji e Riku (preoccupazione, sollievo, orgoglio, rabbia e sicurezza), scelte in base all'esito del test.
- La home usa ora soltanto tavole coerenti con lo stato corrente della carriera; ogni immagine resta per 10 secondi e passa alla seguente con dissolvenza.
- L'introduzione dei capitoli è stata ricostruita con font sans moderno, tavole tematiche in dissolvenza e testo che si compone riga per riga mantenendo visibili quelle già scritte.

# 2026-08-23 — correzione difetti audio e collaudo

- `SoundtrackService` ha ora uno stato reale: `Active` distingue il canale aperto dal backend usato (prima la domanda veniva posta a `fallback`, nullo sul percorso MCI, e ogni refresh della Home riavviava il brano).
- `Pause`/`Resume` non sono più lo stesso comando: in ffplay `p` è un interruttore, quindi due `Resume` di fila mettevano in pausa. Ora sono idempotenti e la finestra soundtrack usa un solo pulsante con etichetta che segue lo stato.
- Lo slider del volume funziona anche sugli MP3: ffplay accetta il livello solo all'avvio, quindi il brano viene riavviato al valore nuovo. Il gesto è ammortizzato (320 ms) per non produrre una raffica di riavvii.
- Rimosso `FadeIn`: scriveva dieci volte `0` su ffplay, che non è uno step di dissolvenza ma il comando che *alza* il volume — la musica saliva da sola dopo l'avvio.
- I duplicati `Titolo (n).mp3` vengono raggruppati: la rotazione per atmosfera pescava fino a quattro volte lo stesso brano.
- Il prologo non cede più la musica alla Home quando si premeva Spazio: quel tasto mostra il testo e nient'altro. La narrazione registrata ora esclude il tema musicale invece di sovrapporsi.
- Rimossa la corsa fra i tre avvii concorrenti all'apertura (RefreshUi, `Shown`, `PhaseIntroDialog`): il tema d'avvio lo decide chi apre la scena.
- Ripristinate tre tavole citate dal codice ma assenti da `assets/`: `manga-02-first-test.png`, `manga-05-first-invitation.png`, `manga-10-first-victory.png`. Le scene Prelude e Invitation restavano senza immagine.
- Trovata la causa dei collaudi «rimasti in attesa di una modale»: `UiRender` non impostava `CORSACAREER_UI_AUTOMATION`, quindi un costruttore di dialogo avviava ffplay con `-loop 0`, processo che non termina da solo e teneva in vita il rendering. Il divieto è ora dentro i servizi audio, non nei chiamanti. La fase interfaccia è passata da 657s a 51s.
- Corretti: perdita di handle GDI sui ritratti del Capitolo I, `Esc` che valeva come conferma nel prelude (il chiamante lo legge come «procedi»), e le altezze delle Label misurate con `MeasureString` (GDI+) ma disegnate con `TextRenderer` (GDI).
- Collaudo completo: 4 fasi verdi, 175 gruppi superati, 2 saltati, build Debug e Release senza errori né avvisi.

# 2026-08-23 — lanciatore, dissolvenza, fumetti e percorso di carriera

- Aggiunto `Avvia CorsaCareer.bat` sul desktop con `Avvia-CorsaCareer.ps1`: **ricompila e poi avvia**. Non è un dettaglio: le immagini vengono lette da `assets` accanto all'eseguibile, quindi avviare l'exe senza compilare mostrava le tavole della build precedente e ogni file aggiunto restava invisibile. Il lanciatore cerca un SDK vero, si ferma con un messaggio leggibile se la compilazione fallisce e dichiara quante immagini sono disponibili. Opzioni `-Debug` e `-NuovaCarriera`.
- Corretto il difetto della dissolvenza che sembrava un breve ridimensionamento: il fotogramma era forzato a 1600×900 (16:9) mentre **nessuna** delle 48 tavole è 16:9 (sono 1:1, 3:2, 2:3 e 5:4). Il `PictureBox` in modalità Zoom riceveva un formato diverso da quello vero e stirava l'immagine per tutta la transizione. Ora il fotogramma segue le proporzioni della tavola mostrata, sia nella home sia nell'introduzione dei capitoli (dove `Max(from,to)` inventava un formato di nessuna delle due).
- Corretti i fumetti troncati del Capitolo I: la colonna del ritratto è larga 220px ma l'altezza della card veniva calcolata su 196, e la larghezza usata era quella provvisoria letta prima che lo `SplitContainer` fosse disposto. Aggiunto `RefitText`, che rimisura card e paragrafi quando la larghezza reale è nota e tiene conto della barra di scorrimento.
- La firma di un contratto ha ora un tema dedicato: `CONTRACT_SIGNING`, `TEAM_CHANGE` e `PROMOTION` usano l'atmosfera `firma` (`Proud Momentum`) invece di finire in `sponsor`, lo stesso tema di una trattativa di routine. Il brano entra sul momento della firma, non al refresh successivo.
- Trovato e corretto lo stato senza uscita segnalato dall'utente («dopo che scelgo un team non succede nulla»): `AlignToEntryLevel` attivava il contratto senza aprire il campionato né allineare `CareerPhase`. La carriera reale risultava `ContractActive: True` con `CareerPhase: Evaluation`, `TeamHistory: 0` e **zero round** in calendario. Ora quel percorso genera il calendario, porta la fase ad `Active` e registra la firma; aggiunta anche una riparazione al caricamento per le carriere già bloccate, verificata su una copia del salvataggio reale (da 0 a 6 round con piste e date).
- Chiuso un difetto trovato durante le prove: ffplay sopravviveva a un arresto anomalo dell'applicazione, continuando a suonare senza finestra. I player audio sono ora legati alla vita del processo con un Job Object, quindi li chiude il sistema operativo in qualunque modo l'app finisca.
- `UiRender` e il rendering di `UiWalkthrough` impostano `CORSACAREER_UI_AUTOMATION`: era la causa vera dei collaudi «rimasti in attesa di una modale».
- Aggiunto `tests/PercorsoCheck`: segue in autonomia valutazione → offerte → firma → calendario → sponsor → proposte → gare → giudizio del team, con 27 verifiche, e dichiara quale anello manca invece di limitarsi a stampare. Incluso in `verifica.ps1` come fase dedicata.
- Collaudo completo: 5 fasi verdi, 175 gruppi superati, 2 saltati, build Debug e Release senza errori né avvisi.

# 2026-08-23 — selezione a più giornate (arco «Capeta»)

Analisi prima di scrivere: gran parte di ciò che serviva **esisteva già** e non è stato riscritto — `CareerLadder` (13 gradini, kart 4t → 2t → 125 → formula, riconosciuti dalla potenza reale), quote per gradino (120 € → 380 € → 900 € → 9.000 €), `StartingCash = 900`, `ConditionalPromise` («se ottieni X allora Y», verificata sui referti), `ReputationProfile` multidimensionale, `TeamInterest` a soglie. Mancava lo **snodo**: il provino su più giorni che decide un salto di categoria.

- Aggiunto `SelectionTrial` + `SelectionEngine`: quattro tipi di giornata (familiarizzazione, costanza, time attack, gara di valutazione) con punteggi separati e pesi che si redistribuiscono sulle prove realmente svolte. Una giornata mancante non vale zero: vale «non pervenuta».
- Il verdetto usa **solo** dati che il referto di Assetto Corsa fornisce davvero: giri, miglior tempo, penalità, danno, posizione. Il referto non espone track limits né conteggio incidenti, quindi la pulizia di guida è dichiarata come proxy su penalità e danno invece di essere inventata.
- Un passo fuori categoria (oltre il 4% dal riferimento) non viene compensato dalla costanza: il sedile non arriva comunque. Il calcolo è sempre mostrato riga per riga — un verdetto che cambia una carriera non può essere un numero senza spiegazione.
- Aggiunto `TrackPreference`: la sede richiesta viene cercata per id e per nome, poi si ripiega su un tracciato affine per caratteristiche e lunghezza, **dichiarando** la sostituzione. Prima esisteva solo un fallback inline hardcoded su «monza».
- Aggiunto `SelectionCatalog` come dati: la selezione monoposto (4 giornate, 12 candidati, 2 posti) e quella nazionale (3 giornate). Le piste indicate sono una preferenza, non un requisito.
- `MainFormSelection` riusa la pipeline esistente (pianificatore, preset, import): cambia solo dove finisce il referto. La quota si paga anche in debug, e una prova simulata marca il verdetto come tale. La promozione crea un'**offerta** reale: la firma resta una scelta del giocatore.
- Innesti nell'interfaccia: convocazione annunciata, pulsante principale che dice a che giornata si è arrivati, schermata con le prove svolte accanto al briefing della prossima.
- Il render ha fatto emergere sei difetti di impaginazione (ordine invertito dei `Dock=Top`, righe a larghezza provvisoria, verdetto troncato, dossier senza ritorno a capo, pulsanti tagliati, nota della sede duplicata): tutti corretti e riverificati a schermo.
- `tests/PercorsoCheck` esteso a 48 verifiche: sostituzione della sede, selezione a metà che non produce verdetto, prova senza giri validi che non promuove, ciclo di vita completo.
- Collaudo completo: 5 fasi verdi, 175 gruppi superati, 2 saltati, Debug e Release senza errori né avvisi.

# 2026-08-23 — il passo attuale raccontato, e home ripulita

- Aggiunto `StepBriefing`: il pannello «ADESSO» non è più un elenco di dati ma un racconto con gerarchia — kicker, titolo grande, due paragrafi, la richiesta in grassetto, il rischio in giallo e i dati tecnici relegati in una riga sotto una linea. Ogni variante nasce dallo stato reale: primo test assoluto, tentativo successivo, invito, round di campionato, offerte sul tavolo, nessun appuntamento, sessione aperta, giornata di selezione.
- Il testo del primo test è quello chiesto dall'utente: «fino a adesso c'erano solo intenzioni… il cronometro è l'unica cosa che conta», con la cifra in cassa citata come vincolo reale.
- Home ripulita invece di compressa, come richiesto: rimossa la card «Assetto Corsa» (due righe fisse e un pulsante, nessuna informazione sullo stato) e sostituita con «Contenuti», che dice quante vetture da gara e quanti circuiti sono davvero installati. Il comando «Contenuti e sessione» resta.
- Eliminate le ripetizioni: «€ 32.000 in cassa» compariva tre volte nella stessa schermata. Ora la cifra sta nella riga dati del passo, e la card sponsor mostra lo stato del bilancio.
- Altezze ridistribuite: il passo attuale passa dal 29% al 48% della colonna, sottratto al calendario che mostrava un riquadro mezzo vuoto.
- Corretto un difetto reale trovato strada facendo: `continueStory` veniva **riassegnato** dal pannello «sezioni del portale», lasciando orfano il pulsante della card. Ora il comando della home ha un campo proprio (`homeContinue`) allineato alla stessa etichetta.
- Nota sul metodo: il pulsante sembrava mancante nei render, e dopo alcuni tentativi a vuoto una diagnostica temporanea ha mostrato che era visibile e posizionato correttamente (Y=502, larghezza 974) — `DrawToBitmap` non disegna quel pannello. Utile ricordarlo: l'assenza di un controllo in un render non è di per sé un difetto.
- Collaudo completo: 5 fasi verdi, 175 gruppi superati, 2 saltati.

# 2026-08-23 — controllo dopo le modifiche manuali dell'utente

- L'archivio immagini aggiuntive è stato rimosso: **nessuna rottura**. I riferimenti residui degradano da soli — `EditorialAssets.ForEvent` usa `File.Exists` e ricade sulla tavola editoriale storica, `NarrationService.LaunchArtwork` usa `Directory.Exists` e ricade sulle tavole di capitolo. Il codice morto resta innocuo ma andrà tolto quando si tocca quei file.
- Loghi trovati: sono due **fogli** in `assets/`, non file singoli — `team-logos-kart-formula-touring-endurance-sheet.png` (4 loghi, griglia 2×2) e `team-logos-8-kart-formula-touring-gt-endurance-sheet.png` (8 loghi, griglia 4×2), entrambi 1536×1024. Dodici squadre in tutto, con nome e disciplina leggibili. Per usarli come simbolo di squadra vanno ritagliati in PNG singoli e associati ai nomi generati a runtime: non ancora fatto.
- Verificata la nuova logica economica dell'utente: `IsClientSeat`/`IsClientDriver`, `RaceFee`, `TeamSupportPercent`, `SponsorBudget`. È coerente — un sedile cliente azzera `ContractSalary` e `ContractActive`, quindi `EconomyEngine.ForRace` non versa alcuno stipendio, mentre i premi in pista restano. Le offerte d'ingresso sono ora tutte sedili a pagamento con `Salary = 0`: la contraddizione «mi pagano per correre da esordiente» è risolta.
- Aggiunto `AnnounceClientSeat`: comprare un sedile non dava alcuna conferma (il ramo cliente esce con `return` prima dell'annuncio del contratto). Il riquadro usa parole diverse da una firma — quota versata, supporto del team, premi possibili, e «se il risultato non arriva, la quota resta spesa».
- `tests/PercorsoCheck` esteso a 59 verifiche con il passo «chi paga per correre non incassa uno stipendio»: quota intera senza supporto, quota ridotta col supporto, stipendio 0 per il cliente, stipendio ripartito sui round per il contrattualizzato, e i due modelli che restano distinti.
- Collaudo completo: 5 fasi verdi, 175 gruppi superati, 2 saltati.

# 2026-08-23 — loghi delle scuderie e tavole della Home legate al passo

- Ritagliati i 12 loghi generati dall'utente dai due fogli in `assets/team logos/` (griglie 4×2 e 2×2 su 1536×1024) in PNG singoli, dentro `assets/team-logos/` (senza spazio, è il nome che il codice cerca).
- Aggiunto `TeamLogoCatalog`: 12 scuderie con nome, logo e disciplina/gradino della scala (Hoshi e Minato nel kart; Kuroda, Red Maple, Silver Crane, Orion in monoposto; Seishin, Raijin, Takumi nel turismo; Aozora, Sakura, Kitanami nell'endurance). `TeamLogoCatalog.Pick` assegna squadre in modo stabile per carriera, coerenti con il gradino del pilota — niente più offerte da un team endurance a un kartista.
- `BuildOffers` usa ora i nomi e i motti del catalogo al posto dei tre nomi hardcoded ("Rookie Motorsport"/"Northline Racing"/"Apex Competition" restano solo come fallback se il catalogo non produce nulla). `MarketDialog` mostra il simbolo della squadra nella trattativa invece di un'illustrazione generica scelta per sottostringa del nome.
- Corretto un difetto di impaginazione simile ai precedenti: le tavole della Home ruotavano fra 13-15 immagini, di cui solo 4-6 legate al momento — le nove tavole generiche entravano sempre nell'elenco. `HomeArtworkFiles` parte ora dal passo raccontato in «ADESSO» (sessione aperta, selezione in corso, offerte sul tavolo, invito, test, round, vittoria, crisi economica) invece che dall'ultimo evento archiviato, e la sequenza si limita a 3 tavole pertinenti; le generiche restano solo come riserva. La sequenza si rifà quando il passo cambia davvero, non a ogni refresh.
- `tests/PercorsoCheck` esteso a 68 verifiche: ogni scuderia del catalogo ha il proprio logo installato, i nomi sono unici, ogni disciplina ha almeno una squadra, e i gradini della scala trovano sempre candidati credibili.
- Collaudo completo: 5 fasi verdi, 175 gruppi superati, 2 saltati.

# 2026-08-23 — la firma produce qualcosa, home semplificata, audio più basso

- **Difetto grave corretto: la firma non faceva niente.** `GenerateSeasonSchedule` usciva in silenzio quando un calendario esisteva già — gli ID dei round sono stabili (`round-s01-NN`) e `Append` li scartava tutti come duplicati. Accadeva sistematicamente perché la riparazione al caricamento genera un calendario *prima* della firma. Ora i round non ancora disputati della stagione corrente vengono sostituiti con quelli della categoria appena firmata, mentre quelli già corsi restano perché sono storia. Verificato: 6 round diventano 8 passando a categoria regionale.
- Aggiunto il riconoscimento della firma: una schermata con categoria, vettura, compagno, contratto, obiettivo, numero di round e dove si comincia, più la cassa. Prima non c'era alcun segnale e il pulsante sembrava rotto. La tavola della home viene ricalcolata (`RestartHomeArtworkSequence`), così non resta quella del pilota senza contratto.
- Home semplificata su indicazione dell'utente: le card «Squadra», «Soldi e sponsor», «Ingaggi» e «Contenuti» sono diventate una barra di navigazione sotto la testata. Nella colonna restano solo il passo attuale (62% dell'altezza) e il calendario. Il comando principale era schiacciato in una striscia rossa illeggibile.
- Rimossa la fascia in basso a sinistra con capitolo, obiettivo e rischio: ripeteva il pannello «ADESSO» con parole diverse e sotto la tavola sembrava la didascalia di un'altra schermata.
- `GhostButton` misura ora la larghezza sul testo: le etichette della testata erano troncate («IMPOSTA…», «CENTR…»).
- Il «quando» è passato dai dati tecnici a una riga in evidenza sotto il titolo, raccontato invece che elencato: «Si scende in pista a Cremona fra 10 giorni, mercoledì 22 marzo». Una data secca non diceva se c'era tempo per prepararsi.
- Audio riequilibrato: il sottofondo passa da 585 a 260 su 1000 (`BackgroundVolume`), il prologo a 150 (`IntroVolume`) perché lì si legge, e la narrazione da 120 a 100 — oltre 100 ffplay amplifica e distorce. Il commento diceva «metà del livello della Home» quando era un ottavo.
- `tests/PercorsoCheck` esteso a 51 verifiche, con il caso «firma con un calendario già presente» che riproduce il difetto segnalato.
- Collaudo completo: 5 fasi verdi, 175 gruppi superati, 2 saltati.

# 2026-08-26 — calendario operativo nella Home

- Il calendario della Home espone ora tre comandi sempre visibili: `AVANTI 1 GIORNO`, `+7 GIORNI` e `VAI AL PROSSIMO APPUNTAMENTO`.
- L'avanzamento rispetta la storia: un salto di una settimana si ferma sulla data del prossimo test o gara, mentre il comando dedicato porta esattamente a quell'appuntamento. Non è possibile saltare oltre un evento ancora da disputare.
- I comandi sono sincronizzati con la data corrente e vengono disabilitati quando l'appuntamento è oggi o non esistono date future. Ogni giorno attraversato aggiorna il piano giornaliero, le ore disponibili e l'archivio, quindi salva lo stato della carriera.
- Corretto anche il testo del suggerimento del comando settimanale, che ora dichiara esplicitamente che il salto si ferma sulla data dell'evento.
- Verifica di compilazione: Debug e Release completate con 0 errori e 0 avvisi. Le 5 suite comportamentali opzionali restano non eseguibili perché i relativi sorgenti non sono presenti nel repository.

# 2026-08-26 — fase 1 UI coerente e firma scuderia operativa

- Tutor della Home reso autosizing: data di oggi, prossimo appuntamento e consiglio restano leggibili anche con testi lunghi, senza sovrapposizioni.
- Archivio/calendario Home reso autosizing con titolo e dettaglio su righe separate; rimossi ellissi e altezze fisse che tagliavano date e descrizioni.
- Riquadro `BUDGET DISPONIBILE` riallineato con forma e fiducia: valore più leggibile, righe dimensionate correttamente e movimenti senza testo troncato.
- Pannello del weekend ampliato per contenere tutti i comandi (continua, attività, sponsorizzazioni, opportunità e simulazione) senza che i controlli inferiori venissero coperti.
- Corretto il flusso funzionale della firma: `ChooseOffer`, `SignOffer` e `AcceptOpportunity` impostano categoria, vettura, data e stagione prima di generare il calendario. Le gare future ora vengono create coerentemente con il team scelto; gli eventi già disputati restano storia.
- Debug di compilazione: Debug e Release completate con 0 errori e 0 avvisi. Le suite comportamentali/UI opzionali non sono presenti nel repository, quindi la verifica automatica resta parziale e non viene dichiarata come superata.
- Controllo statico del flusso di firma: `ChooseOffer`, `SignOffer` e `AcceptOpportunity` assegnano i dati della stagione prima della chiamata a `GenerateSeasonSchedule` (3/3 controlli superati).
