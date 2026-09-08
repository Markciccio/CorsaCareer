# CorsaCareer — obiettivo operativo

Costruire un simulatore di carriera automobilistica narrativo, dal kart alle categorie professionistiche. Assetto Corsa fornisce i risultati reali; il debug usa risultati **SIMULATI** dichiarati.

## Regola di lavoro

Una funzione non è “fatta” perché compila o perché il codice sembra corretto. È fatta solo dopo che un test riproducibile o un walkthrough la esegue fino all'esito atteso, senza errori di dati o UI.

## Ordine obbligatorio

1. **UI stabile** — nessun testo tagliato, sovrapposto o duplicato; data di oggi, prossimo evento e i tre valori chiave sempre leggibili.
2. **Ciclo di gioco funzionante** — profilo → calendario → attività → test/gara → costi e premi → risultato → conseguenze → calendario aggiornato.
3. **Narrazione** — scene anime, dialoghi e articoli usano soltanto dati dell'evento appena concluso.
4. **Evoluzione** — nuove categorie, team, sponsor, immagini e integrazione Assetto Corsa solo dopo i tre punti sopra.

## Regole di prodotto

- I valori sempre visibili sono: **Budget**, **Forma fisica**, **Fiducia nel paddock**. Il budget non diventa mai negativo.
- Prima di ogni evento mostrare: saldo attuale, iscrizione, trasferta, rischio/riparazioni, premi possibili e saldo minimo/massimo. Il consuntivo deve coincidere con il preventivo o spiegare ogni differenza.
- Ogni giorno ha ore separate per pilota e Haru. Attività e sponsor cambiano valori con effetti dichiarati; non inventano risultati di pista.
- Ogni scelta team crea immediatamente un calendario di eventi compatibili con categoria e vettura.
- Risultati reali e simulati restano distinguibili in home, calendario, archivio, articolo e dialoghi.
- Le scene importanti si aprono in anime full-screen, con immagini contestuali, testo leggibile e tono coerente dei personaggi.

## Gate di verifica per ogni modifica

- Build Debug e Release senza errori né warning.
- Test riproducibile del caso modificato; se non esiste, crearlo prima di dichiarare il passaggio riuscito.
- Controllo visivo della schermata a 1920×1080: nessun clipping, sovrapposizione, testo duplicato o pulsante irraggiungibile.
- Annotare in `progress.md`: caso eseguito, risultato, screenshot/prova e bug rimasti.

## Definizione di completamento del ciclo base

Una nuova carriera di **Marco Ruga #27** deve completare senza interventi manuali sui dati: primo test, attività, contatto sponsor, firma team, gara con preventivo e consuntivo coerenti, risultato leggibile, scena anime, articolo e prossimo appuntamento nel calendario.

Finché questo percorso non passa dall'inizio alla fine, non si aggiungono nuove funzionalità e non si dichiara alcuna fase conclusa.
