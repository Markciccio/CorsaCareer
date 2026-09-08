# Obiettivo operativo — CorsaCareer

## Direzione del prodotto

Trasforma CorsaCareer in una carriera automobilistica narrativa, originale e credibile: l'ascesa di un pilota sconosciuto, con pochissimi soldi, dai primi test alle categorie professionistiche. Il tono deve avere energia, tensione e crescita personale da sport manga, senza copiare personaggi, scene o trama di opere esistenti.

Non costruire una raccolta di schermate scollegate. Costruire un unico mondo persistente: ogni sessione in pista modifica persone, opportunità, denaro, reputazione, articoli e calendario.

## Pilastri non negoziabili

1. **Risultati veri, storia reattiva.**
   - Con Assetto Corsa disponibile, gare e test usano esclusivamente referti/import reali.
   - Non inventare tempi, posizioni o vittorie nella modalità reale.
   - Il debug può generare risultati fittizi, ma deve essere sempre visibile come `DEBUG · simulato` e attraversare la stessa catena logica del risultato reale.

2. **Partenza difficile.**
   - Inizio senza contratto, con budget minimo, vettura modesta e nessuna reputazione.
   - Prima test, poi opportunità, inviti e categorie minori; non regalare contratti o risultati.
   - Ogni costo, premio, sponsor e stipendio deve avere una spiegazione chiara e una conseguenza.

3. **Carriera realmente causale.**
   - Test → verdetto → relazione/team interest → invito/offerta/nuovo test.
   - Gara → punti/denaro/reputazione → stampa/sponsor/mercato → prossimo appuntamento.
   - Contratto → calendario della stagione, obiettivi, compagno, pressione e possibilità di rinnovo.
   - Interesse di un team non equivale mai automaticamente a contratto.

4. **Personaggi persistenti.**
   - Manager, meccanico/mentore, rivale, compagno, team principal e sponsor devono avere ruolo, interessi, memoria e dialoghi coerenti.
   - I dialoghi devono citare dati realmente prodotti: circuito, auto, tempo, scarto, posizione, denaro, fiducia o obiettivo.
   - Gli articoli devono essere lunghi, leggibili e situati: chi era presente, cosa è accaduto, contesto del pilota, lettura tecnica, effetti e prospettive.

## Esperienza e interfaccia

### Home = hub della carriera

La home deve rispondere immediatamente a quattro domande:

- Chi sono e dove sono nella mia carriera?
- Cosa devo fare adesso?
- Cosa è appena successo e che cosa ha prodotto?
- Chi mi osserva e cosa rischio/posso ottenere?

Deve includere direttamente:

- obiettivo narrativo e sportivo attuale;
- azione principale contestuale;
- squadra e persone;
- budget/sponsor;
- offerte o stato contrattuale;
- **timeline/calendario vivo**: ultimi test/gare con dati e conseguenze, più il prossimo appuntamento;
- accesso a contenuti e sessione Assetto Corsa/debug.

### Pagine di approfondimento

- Tutte full-screen e progettate con layout responsive tramite contenitori, non coordinate fisse.
- Niente finestre vuote, liste tecniche nude o enormi aree inutilizzate.
- Ogni sezione deve avere gerarchia: testata, sintesi, contenuto principale, dettagli e azione possibile.
- Grafica: editoriale motorsport professionale + sensibilità manga originale. Sfondo scuro, accenti rosso/giallo/blu/verde con significato, tipografia leggibile, carte e timeline.
- Loghi e illustrazioni devono essere originali e dichiarati come editoriali quando non documentano un evento reale.

### Sezioni obbligatorie

- **Calendario e archivio:** cronologia permanente di test, inviti, gare, attività e stagioni; ogni record conserva dati, esito, fonte e conseguenze.
- **Mercato e scouting:** sedili, team interest, condizioni, auto, categoria, stipendio, durata, obiettivi, concorrenti e rischio; ogni dato numerico spiegato.
- **Paddock e persone:** schede dei personaggi con rapporto e memoria degli eventi.
- **Sponsor e finanze:** budget, costi, premi, bonus, obiettivi, rapporto e possibile rinnovo/perdita.
- **Giornale/media:** articoli basati su fatti e risultati; archivio datato e consultabile.
- **Assetto Corsa/sessione:** briefing chiaro con pista, vettura, formato, condizioni, tempo/obiettivo e modalità di importazione.

## Dati tecnici: sempre spiegati

Ogni valore deve essere accompagnato dal suo significato e impatto, non solo mostrato.

- Tempo obiettivo: riferimento richiesto e perché conta.
- Scarto: differenza dal riferimento, positiva o negativa.
- Punteggio valutazione: come traduce prestazione, costanza e affidabilità in opportunità.
- Prestigio: visibilità ottenuta e aumento delle aspettative.
- Interesse team: probabilità di un prossimo contatto, non contratto garantito.
- Stipendio: quota annuale garantita, distinta da premi e bonus.
- Fiducia: quanto il team sostiene il pilota; influenza rinnovo, sviluppo e possibilità.
- Budget: denaro effettivamente disponibile; test, iscrizioni e attività devono pesare davvero.

## Ciclo di sviluppo obbligatorio

Per ogni intervento:

1. Leggere la schermata e lo stato esistente prima di cambiare la logica.
2. Implementare una modifica coerente con questo obiettivo, senza regressioni o scorciatoie narrative.
3. Compilare in Release.
4. Chiudere l'eseguibile che blocca la build, se necessario; riavviare sempre la build nuova.
5. Eseguire il percorso debug pertinente, compreso almeno un esito favorevole e uno sfavorevole quando la funzione ha ramificazioni.
6. Verificare che il risultato sia **visibile nella UI**, persistente dopo salvataggio/riavvio e presente nella timeline/articoli/dialoghi quando pertinente.
7. Correggere prima di proseguire se la grafica è vuota, confusa, tagliata o ancora basata su vecchi controlli tecnici.

## Definizione di qualità

Una funzione è completata solo se:

- è giocabile dalla home o da una sezione chiaramente raggiungibile;
- cambia lo stato della carriera in modo persistente;
- spiega conseguenze e numeri in italiano chiaro;
- alimenta il calendario/archivio e, se rilevante, dialoghi e giornale;
- è distinguibile tra risultato reale e debug;
- è compilata, avviata e verificata nella build che l'utente sta guardando.

## Priorità corrente

1. Rendere la home e il calendario davvero gradevoli, narrativi e leggibili.
2. Consolidare persistenza e continuità tra test, calendario, dialoghi e articoli.
3. Rifinire mercato/scouting, sponsor, personaggi e contratti come parti della stessa carriera.
4. Solo dopo: espandere le categorie, le stagioni e l'integrazione Assetto Corsa reale.
