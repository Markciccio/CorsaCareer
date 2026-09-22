namespace CorsaCareer;

/// <summary>Metadati leggibili per scegliere una tavola senza affidarsi al nome tecnico del file.</summary>
public static class IllustrationCatalog
{
    /// <param name="Vehicles">Mezzi chiaramente visibili nella tavola, anche sullo sfondo.</param>
    public sealed record IllustrationInfo(string Title, string Tags, string Vehicles, string Description);

    /// <summary>
    /// Biblioteca consultabile anche dal motore narrativo: ogni tavola dichiara
    /// disciplina, luogo, momento e funzione drammatica, non solo un nome file.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IllustrationInfo> Library = new Dictionary<string, IllustrationInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["campionato-kart-monoposto.jpg"] = new("Squadra kart festeggia in officina", "copertina, kart, giappone, squadra, officina, vittoria, festa", "kart da competizione in primo piano", "Sei ragazzi festeggiano intorno a un kart in un'officina aperta sulla strada: squadra al completo, pilota al volante. Copertina del ramo di carriera KART → MONOPOSTO, che comincia proprio dal kart. Il nome del file diceva «formula-minore»: non lo era.")
        , ["japan-kart-dawn-garage-rookie-mechanic.png"] = new("Prima dell'alba nel box", "home, sigla, kart, giappone, rookie, meccanico, officina, pioggia, gavetta, alba", "kart da competizione usato; ricambi di kart sul banco", "Rookie e meccanico riparano un kart usato in un piccolo circuito giapponese dopo la pioggia. Per prologhi, preparazione, budget basso e fiducia costruita a mano."),
        ["japan-junior-formula-rain-test-step-up.png"] = new("Primo salto in formula", "home, formula-minore, giappone, test, pioggia, rookie, meccanico, promozione, tensione", "monoposto junior a ruote scoperte; kart sullo sfondo del paddock", "Il pilota entra in una monoposto junior durante un test bagnato. Per debutti in formula, secondi test, cambi categoria e obiettivi tecnici."),
        ["japan-touring-sunset-rival-duel.png"] = new("Duello turismo al tramonto", "home, touring, giappone, rivale, gara, duello, tramonto, sorpasso, tensione", "due vetture turismo da gara", "Due turismo originali combattono affiancate sul bagnato. Per qualifiche, rivalità, sorpassi, gare punto a punto e commenti provocatori."),
        ["japan-endurance-night-rain-pitstop.png"] = new("Pit stop sotto la pioggia", "home, gt, endurance, giappone, notte, pitstop, strategia, pioggia, squadra, pressione", "GT endurance; altre GT nel box sullo sfondo", "Una GT rientra ai box di notte mentre squadra e stratega lavorano sotto la pioggia. Per endurance, strategie, crisi, soste e responsabilità del team.")
        , ["manga-kart-scrapyard-four-stroke-generator-first-test.png"] = new("Kart dei rottami, primo motore", "manga, kart, giappone, gavetta, rottami, motore-quattro-tempi, generatore, officina, primo-test, budget", "kart artigianale da gara; piccolo motore quattro tempi da generatore; telaio e ricambi usati", "Un ragazzo e il suo mentore osservano un kart ricostruito fra ricambi e rottami. Per l’origine assoluta della carriera, recupero componenti, budget minimo e prime prove.")
        , ["manga-kart-first-rain-test-repaired-chassis.png"] = new("Primo test sotto la pioggia", "manga, kart, giappone, rookie, test, pioggia, alba, meccanico, telaio-riparato, quattro-tempi", "kart da gara artigianale con motore quattro tempi esposto", "Il kart recuperato torna in pista su asfalto bagnato mentre il mentore osserva dal box. Per test iniziali, apprendimento, errori e primi segnali di passo.")
        , ["manga-sae-kart-local-victory-celebration.png"] = new("Sae vince al kartodromo", "manga, kart, Sae, vittoria, squadra, podio, circuito-locale, giappone, entusiasmo", "kart quattro tempi da gara; bandiera a scacchi", "Sae festeggia una vittoria locale assieme a un team piccolo. Per risultati P1, rivalità positiva, felicità contagiosa e premi che permettono di correre ancora.")
        , ["manga-tooru-sponsor-rejection-kart-budget-rain.png"] = new("Tooru e il no dello sponsor", "manga, kart, Tooru, sponsor, budget, rifiuto, pioggia, ufficio, rimorchio, crisi", "kart da gara usato su rimorchio", "Tooru esce da un contatto sponsor non andato a buon fine, con il kart e il rimorchio sullo sfondo. Per rifiuti, mancanza di budget, trattative e scelte fuori dalla pista.")
        , ["manga-formula-junior-new-team-garage-entry.png"] = new("Ingresso nel team Formula Junior", "manga, formula-minore, Formula-Junior, squadra, garage, trasportatore, telemetria, rivale, promozione", "monoposto junior a ruote scoperte; altra monoposto sullo sfondo", "La prima monoposto arriva dal camion davanti al rookie e ai nuovi meccanici. Per ingresso in team, test di Formula Junior, promozione e confronto con il rivale.")
        , ["manga-endurance-gt3-night-pitstop-rookie-strategy.png"] = new("Strategia GT3 nella notte", "manga, GT3, endurance, pitstop, notte, pioggia, strategia, team, rookie", "GT3 endurance; gomme slick; attrezzatura pit stop", "Un team endurance lavora su una GT3 sotto la pioggia mentre il rookie ascolta la strategia. Per stints, pit stop, scelta gomme, pressione e gare di durata.")
        , ["manga-touring-fuji-rain-first-corner-riku-duel.png"] = new("Duello turismo sotto il Fuji", "manga, turismo, Fuji, giappone, pioggia, Riku, rivale, partenza, primo-giro, duello", "due vetture turismo compatte da gara con roll-bar e gomme slick", "Due turismo sono affiancate sul rettilineo bagnato del Fuji con il monte sullo sfondo. Per gare turismo in Giappone, primi giri, rivalità dirette e partenze ad alta tensione.")
        , ["manga-formula-suzuka-rain-esses-late-braking-rival.png"] = new("Formula sotto la pioggia alle Esse", "manga, formula-minore, Suzuka, giappone, pioggia, esses, frenata, rivale, duello, gara", "due monoposto Formula 2-style a ruote scoperte con pneumatici da bagnato", "Due monoposto originali si contendono le Esse bagnate di Suzuka. Per un duello in Formula, una frenata rischiosa, un confronto con il rivale o una gara in cui il coraggio va gestito.")
        , ["manga-miki-tokyo-gt4-sponsor-budget-negotiation.png"] = new("Miki tratta il budget GT4 a Tokyo", "manga, Miki, sponsor, trattativa, budget, Tokyo, giappone, GT4, team, contratto", "GT4 da competizione; rimorchio del team e attrezzatura officina", "Miki tratta con il responsabile di una piccola squadra GT4 in un'officina di Tokyo, mentre una vettura e il rimorchio sono pronti sullo sfondo. Per sponsor, costi di stagione, negoziazioni e offerte da valutare.")
        , ["character-tooru-relieved-small-sponsor-kart-paddock.png"] = new("Tooru trova un piccolo spiraglio", "manga, ritratto-personaggio, Tooru, sponsor, kart, paddock, pioggia, sollievo, budget", "kart su rimorchio sullo sfondo", "Tooru riceve una risposta finalmente positiva, ma ancora fragile, accanto al paddock bagnato. Per piccoli sponsor, sollievo dopo un rifiuto, budget che torna a respirare e dialoghi di speranza cauta.")
        , ["manga-sae-formula4-suzuka-first-test-determined.png"] = new("Sae prima del test Formula 4", "manga, Sae, formula-minore, Formula-4, Suzuka, giappone, test, pioggia, determinazione, garage", "monoposto Formula 4 bianco-rossa a ruote scoperte; casco e attrezzatura box", "Sae si infila i guanti davanti alla sua prima Formula 4 in un box bagnato di Suzuka. Per esordi, timore trasformato in concentrazione, test tecnici e promozioni dalla gavetta.")
        , ["manga-touring-fuji-rain-haru-telemetry-garage.png"] = new("Haru legge il duello al Fuji", "manga, Haru, turismo, Fuji, giappone, telemetria, garage, pioggia, analisi, rivale", "vettura turismo blu con roll-bar; seconda turismo bianca sullo sfondo", "Haru confronta le tracce di telemetria in un garage affacciato sul Fuji bagnato. Per analisi post-sessione, strategie, scelte di assetto e momenti in cui i dati spiegano un distacco.")
        , ["manga-gt3-endurance-japan-dawn-second-place-team.png"] = new("Secondo posto GT3 all'alba", "manga, GT3, endurance, giappone, alba, secondo-posto, traguardo, squadra, pioggia, risultato", "GT3 rosso-nera; altra GT3 sul pit lane", "Il team festeggia un secondo posto conquistato fino all'ultima ora, mentre il rookie esce esausto dalla GT3. Per piazzamenti importanti, premi, fiducia costruita e vittorie mancate di un soffio.")
        , ["manga-shigeru-motegi-formula3-night-qualifying-coaching.png"] = new("Shigeru prepara la qualifica a Motegi", "manga, Shigeru, Formula-3, formula-minore, Motegi, giappone, qualifica, notte, pioggia, ingegnere, coaching", "monoposto Formula 3 nera a ruote scoperte; seconda Formula 3 sullo sfondo; gomme da bagnato", "Shigeru Kanda indica la telemetria al rookie già seduto nella Formula 3 prima della qualifica notturna di Motegi. Per coaching tecnico, ansia prima del giro secco, dati e fiducia dell'ingegnere.")
        , ["manga-rei-suzuka-formula4-practice-crash-frustrated.png"] = new("Rei dopo l'errore in Formula 4", "manga, Rei, Formula-4, formula-minore, Suzuka, giappone, incidente, practice, pioggia, frustrazione, responsabilità", "Formula 4 bianco-rossa con ala anteriore danneggiata; ricambi e gomme box", "Rei Kisaragi affronta il rookie dopo un errore in prova che ha rotto l'ala della Formula 4. Per incidenti senza retorica, costi dei danni, richiamo del manager e possibilità di imparare.")
        , ["manga-tooru-osaka-kart-sponsor-autoparts-negotiation.png"] = new("Tooru propone il kart a Osaka", "manga, Tooru, sponsor, trattativa, kart, Osaka, giappone, ricambi, budget, rifiuto, speranza", "kart quattro tempi rosso-bianco; furgone trasporto kart; ricambi automobilistici", "Tooru presenta il progetto a un ricambista di Osaka sotto la pioggia, con il kart sullo sfondo. Per cercare micro-sponsor, spiegare i costi, ricevere un rifiuto o strappare un primo incontro.")
        , ["manga-kart-first-track-test-genji-timing-dawn.png"] = new("Il primo test cronometrato", "manga, kart, primo-test, Genji, cronometro, giappone, alba, pioggia, rookie, gavetta", "kart artigianale quattro tempi rosso-bianco; carrello attrezzi e ricambi", "Genji cronometra dalla barriera mentre il rookie porta in pista il kart ricostruito. È la tavola da usare al primo test reale, per mostrare il tempo da battere e la distanza ancora da colmare.")
        , ["manga-kart-first-team-choice-rei-tooru-genji-paddock.png"] = new("La prima scelta di squadra", "manga, kart, scelta-team, Rei, Tooru, Genji, paddock, giappone, contratto, offerta, budget", "tre kart da gara di piccole squadre, uno per tenda; caschi e cavalletti", "Rei consegna una cartellina al rookie davanti a tre piccole squadre kart; Genji valuta e Tooru fa i conti. Per il primo bivio, offerte diverse, scelta del team e posto da pilota-cliente.")
        , ["manga-kart-first-race-riku-sae-first-corner.png"] = new("La prima gara: Riku contro il rookie", "manga, kart, prima-gara, Riku, Sae, rivale, primo-giro, pioggia, sorpasso, giappone", "kart quattro tempi rosso-bianco; kart nero-giallo del rivale; gruppo di kart sullo sfondo", "Il rookie e Riku Hayase entrano affiancati nella prima curva bagnata mentre Sae guarda dal muretto. Per il primo weekend, partenze difficili, rivalità e risultati che modificano fiducia e sponsor.")
        , ["manga-kart-first-sponsor-agreement-local-workshop.png"] = new("Il primo accordo con uno sponsor", "manga, kart, primo-sponsor, accordo, officina, budget, giappone, rookie, Genji, Tooru", "kart quattro tempi rosso-bianco; casco, ricambi e componenti officina", "Il rookie stringe la mano a un piccolo sponsor locale nell'officina, con Tooru e il kart alle spalle. Per un accordo iniziale, un budget minimo ma decisivo, e la prima prova che qualcuno crede nel progetto.")


        // --- catalogazione completata. Le voci qui sotto sono state scritte
        // guardando le tavole: il campo dei mezzi visibili non si puo dedurre
        // dal nome, e sbagliarlo rimette una monoposto sopra una gara di kart.
        , ["manga-01-rookie-dawn.png"] = new("L'alba del debuttante", "esordio, alba, kart, circuito minore, meccanico, gavetta", "kart a quattro tempi consumato; furgone officina; gomme usate", "Il ragazzo si allaccia il casco all'alba accanto a un kart rimesso insieme, il meccanico guarda la pista bagnata. Per aperture di carriera e primo capitolo.")
        , ["manga-05-first-invitation.png"] = new("In griglia fra le monoposto", "invito, griglia, monoposto, storico, partenza, folla", "monoposto a ruote scoperte anni Sessanta, griglia intera", "Il pilota in abitacolo mentre il meccanico dà le ultime indicazioni, in una griglia di monoposto storiche. NON usare per il kart: mostra un'altra categoria.")
        , ["manga-08-financial-crisis.png"] = new("I conti in officina", "crisi, denaro, officina, notte, kart, ricevute", "kart smontato sul cavalletto; motore a terra", "Il ragazzo conta le banconote mentre il meccanico lavora sul kart smontato. Per difficolta economiche e cassa vuota.")
        , ["manga-sponsor-table.png"] = new("Il tavolo delle trattative", "sponsor, trattativa, ufficio, contratto, pioggia", "", "Tre persone attorno a un tavolo con documenti e un casco appoggiato. Nessun mezzo visibile: va bene in qualunque categoria.")
        , ["manga-13-champion.png"] = new("La sera del titolo", "titolo, traguardo, tramonto, coppa, meccanico, quiete", "", "Due piloti seduti sul muretto al tramonto con la coppa accanto, il meccanico porta il caffe. Nessun mezzo visibile.")
        , ["manga-03-first-rival.png"] = new("Il primo rivale", "rivalita, confronto, paddock", "", "Due piloti che si misurano prima di correre. Per l'apparire di un avversario diretto.")
        , ["manga-04-first-setback.png"] = new("La prima battuta d'arresto", "sconfitta, delusione, box", "", "Il momento dopo un risultato mancato. Per fallimenti e occasioni perse.")
        , ["manga-06-first-contract.png"] = new("La firma", "contratto, firma, accordo", "", "Carte sul tavolo e una decisione da prendere. Per firme e sedili accettati.")
        , ["manga-07-teammate-duel.png"] = new("Il duello interno", "compagno di squadra, tensione, box", "", "Due piloti della stessa squadra che si guardano. Per confronti col compagno.")
        , ["manga-09-first-podium.png"] = new("Il primo podio", "podio, risultato, festeggiamento", "", "Il primo piazzamento che conta. Per podi e risultati di rilievo.")
        , ["manga-10-first-victory.png"] = new("La prima vittoria", "vittoria, trionfo, traguardo", "", "Il momento in cui si vince per la prima volta. Per vittorie e svolte.")
        , ["manga-11-promotion.png"] = new("Il passaggio di categoria", "promozione, salto, nuovo capitolo", "", "Il gradino successivo della scala. Per promozioni e cambi di categoria.")
        , ["manga-12-professional.png"] = new("Il professionista", "professionismo, stipendio, maturita", "", "Il pilota non paga piu per correre. Per contratti professionistici.")
        , ["manga-night-garage.png"] = new("Officina di notte", "officina, notte, riparazione, silenzio", "", "Il lavoro che nessuno vede, dopo una giornata storta. Per riparazioni e riflessione.")
        , ["manga-rival-grid.png"] = new("In griglia", "griglia, partenza, avversari, attesa", "", "L'attesa prima del via. Per partenze e vigilie di gara.")
        , ["calendar-rookie-rain.png"] = new("Giornata di pioggia", "pioggia, attesa, rookie", "", "Una giornata bagnata in un circuito minore. Per test sotto la pioggia.")
        , ["manga-alt-14.png"] = new("Tavola alternativa", "generica, paddock", "", "Tavola di riserva senza un momento assegnato.")
        , ["manga-endurance-prototype-engine-failure-night.png"] = new("Il motore che cede", "endurance, prototipo, guasto, notte, ritiro", "prototipo da endurance", "Un guasto che chiude la gara prima del tempo. Per ritiri tecnici in endurance.")
        , ["manga-formula-junior-first-podium-rival.png"] = new("Podio in formula", "formula, podio, rivale", "monoposto junior a ruote scoperte", "Un podio in formula d'ingresso, col rivale accanto. Per primi risultati in monoposto.")
        , ["manga-formula-junior-recovery-night-telemetry.png"] = new("Telemetria notturna", "formula, telemetria, notte, analisi", "monoposto junior nel box", "Il lavoro sui dati dopo una giornata storta. Per analisi tecniche.")
        , ["manga-gt3-endurance-victory-dawn-team.png"] = new("Vittoria all'alba", "endurance, gt3, vittoria, alba, squadra", "vettura GT3", "La vittoria dopo una gara lunghissima. Per successi in endurance.")
        , ["manga-gt3-first-test-japan-paddock.png"] = new("Il primo test in GT", "gt3, test, paddock, salto di categoria", "vettura GT3 nel paddock", "Il primo contatto con una GT da competizione. Per passaggi alle vetture chiuse.")
        , ["manga-gt3-night-pitstop-strategy-rain.png"] = new("Sosta sotto la pioggia", "gt3, sosta, pioggia, notte, strategia", "vettura GT3 ai box", "Una sosta decisiva sul bagnato. Per momenti tecnici in gare di durata.")
        , ["manga-japan-freeroam-transfer-team-van-kart-dawn.png"] = new("Il trasferimento", "kart, viaggio, furgone, alba, trasferta", "kart caricato su un furgone", "Il viaggio verso il circuito prima dell'alba. Per trasferte fra gare.")
        , ["manga-kart-first-sponsor-agreement-celebration.png"] = new("L'accordo firmato", "kart, sponsor, festeggiamento, sollievo", "kart sullo sfondo", "Il sollievo dopo un si che non era scontato. Per sponsorizzazioni ottenute.")
        , ["manga-sponsor-meeting-kart-budget-office.png"] = new("Il budget del kart", "sponsor, budget, ufficio, kart, trattativa", "kart citato nei documenti", "Trattativa per finanziare una stagione di kart. Per ricerca sponsor al primo gradino.")
        , ["manga-team-entry-formula-junior-garage.png"] = new("L'ingresso in squadra", "formula, squadra, box, presentazione", "monoposto junior nel box", "Il primo giorno in una squadra di formula. Per ingressi in monoposto.")
        , ["manga-touring-first-race-grid-rookie.png"] = new("Griglia turismo", "turismo, griglia, esordio, partenza", "vetture da turismo in griglia", "L'esordio in una gara di turismo. Per debutti in vetture chiuse.")
        , ["manga-touring-missed-victory-puncture-rain.png"] = new("La vittoria persa", "turismo, foratura, sfortuna, pioggia", "vettura da turismo con gomma a terra", "Una vittoria svanita per una foratura. Per sconfitte non dovute a errori.")
        , ["character-genji-arakawa-celebrating.png"] = new("Genji Arakawa — celebrating", "personaggio, ritratto, genji, celebrating", "", "Genji Arakawa, meccanico e mentore. Espressione: celebrating. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-genji-arakawa-frustrated.png"] = new("Genji Arakawa — frustrated", "personaggio, ritratto, genji, frustrated", "", "Genji Arakawa, meccanico e mentore. Espressione: frustrated. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-genji-arakawa-kart-suspension-dawn.png"] = new("Genji Arakawa — kart suspension dawn", "personaggio, ritratto, genji, kart suspension dawn", "", "Genji Arakawa, meccanico e mentore. Espressione: kart suspension dawn. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-genji-arakawa-proud.png"] = new("Genji Arakawa — proud", "personaggio, ritratto, genji, proud", "", "Genji Arakawa, meccanico e mentore. Espressione: proud. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-genji-arakawa-worried.png"] = new("Genji Arakawa — worried", "personaggio, ritratto, genji, worried", "", "Genji Arakawa, meccanico e mentore. Espressione: worried. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-genji-arakawa.png"] = new("Genji Arakawa — ritratto neutro", "personaggio, ritratto, genji, ritratto neutro", "", "Genji Arakawa, meccanico e mentore. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-haru-senda with paper.png"] = new("Haru Senda — with paper", "personaggio, ritratto, haru, with paper", "", "Haru Senda, procuratore e cercatore di sponsor. Espressione: with paper. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-haru-senda-anxious-data.png"] = new("Haru Senda — anxious data", "personaggio, ritratto, haru, anxious data", "", "Haru Senda, procuratore e cercatore di sponsor. Espressione: anxious data. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-haru-senda-podium-elated.png"] = new("Haru Senda — podium elated", "personaggio, ritratto, haru, podium elated", "", "Haru Senda, procuratore e cercatore di sponsor. Espressione: podium elated. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-haru-senda-relieved-small-sponsor-kart-paddock.png"] = new("Haru Senda — relieved small sponsor kart paddock", "personaggio, ritratto, haru, relieved small sponsor kart paddock", "", "Haru Senda, procuratore e cercatore di sponsor. Espressione: relieved small sponsor kart paddock. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-haru-senda.png"] = new("Haru Senda — ritratto neutro", "personaggio, ritratto, haru, ritratto neutro", "", "Haru Senda, procuratore e cercatore di sponsor. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-miki-arisawa-rejection-composed.png"] = new("Miki Arisawa — rejection composed", "personaggio, ritratto, miki, rejection composed", "", "Miki Arisawa, responsabile commerciale. Espressione: rejection composed. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-miki-arisawa-sponsor-relieved.png"] = new("Miki Arisawa — sponsor relieved", "personaggio, ritratto, miki, sponsor relieved", "", "Miki Arisawa, responsabile commerciale. Espressione: sponsor relieved. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-miki-arisawa-sponsor-worried.png"] = new("Miki Arisawa — sponsor worried", "personaggio, ritratto, miki, sponsor worried", "", "Miki Arisawa, responsabile commerciale. Espressione: sponsor worried. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-miki-arisawa.png"] = new("Miki Arisawa — ritratto neutro", "personaggio, ritratto, miki, ritratto neutro", "", "Miki Arisawa, responsabile commerciale. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-minato-nakahara-touring-penalty.png"] = new("Minato Nakahara — touring penalty", "personaggio, ritratto, minato, touring penalty", "", "Minato Nakahara, pilota di turismo. Espressione: touring penalty. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-minoru-nakahara.png"] = new("Minoru Nakahara — ritratto neutro", "personaggio, ritratto, minoru, ritratto neutro", "", "Minoru Nakahara, dirigente di squadra. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-noa-minazuki-gt-lead-excited.png"] = new("Noa Minazuki — gt lead excited", "personaggio, ritratto, noa, gt lead excited", "", "Noa Minazuki, pilota di GT. Espressione: gt lead excited. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-noa-minazuki.png"] = new("Noa Minazuki — ritratto neutro", "personaggio, ritratto, noa, ritratto neutro", "", "Noa Minazuki, pilota di GT. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-rei-kisaragi-concerned.png"] = new("Rei Kisaragi — concerned", "personaggio, ritratto, rei, concerned", "", "Rei Kisaragi, responsabile del programma rookie. Espressione: concerned. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-rei-kisaragi-first-contract-cautious.png"] = new("Rei Kisaragi — first contract cautious", "personaggio, ritratto, rei, first contract cautious", "", "Rei Kisaragi, responsabile del programma rookie. Espressione: first contract cautious. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-rei-kisaragi-relieved.png"] = new("Rei Kisaragi — relieved", "personaggio, ritratto, rei, relieved", "", "Rei Kisaragi, responsabile del programma rookie. Espressione: relieved. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-rei-kisaragi-stern.png"] = new("Rei Kisaragi — stern", "personaggio, ritratto, rei, stern", "", "Rei Kisaragi, responsabile del programma rookie. Espressione: stern. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-riku-hayase-angry.png"] = new("Riku Hayase — angry", "personaggio, ritratto, riku, angry", "", "Riku Hayase, rivale diretto. Espressione: angry. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-riku-hayase-defeated.png"] = new("Riku Hayase — defeated", "personaggio, ritratto, riku, defeated", "", "Riku Hayase, rivale diretto. Espressione: defeated. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-riku-hayase-formula-qualifying-smug.png"] = new("Riku Hayase — formula qualifying smug", "personaggio, ritratto, riku, formula qualifying smug", "", "Riku Hayase, rivale diretto. Espressione: formula qualifying smug. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-riku-hayase-smug.png"] = new("Riku Hayase — smug", "personaggio, ritratto, riku, smug", "", "Riku Hayase, rivale diretto. Espressione: smug. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-riku-hayase-taunting.png"] = new("Riku Hayase — taunting", "personaggio, ritratto, riku, taunting", "", "Riku Hayase, rivale diretto. Espressione: taunting. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-riku-hayase.png"] = new("Riku Hayase — ritratto neutro", "personaggio, ritratto, riku, ritratto neutro", "", "Riku Hayase, rivale diretto. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-shigeo-kanda-breakthrough-formula.png"] = new("Shigeo Kanda — breakthrough formula", "personaggio, ritratto, shigeo, breakthrough formula", "", "Shigeo Kanda, tecnico e osservatore. Espressione: breakthrough formula. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["character-shigeo-kanda.png"] = new("Shigeo Kanda — ritratto neutro", "personaggio, ritratto, shigeo, ritratto neutro", "", "Shigeo Kanda, tecnico e osservatore. Espressione: ritratto neutro. Ritratto: nessun mezzo visibile, utilizzabile in qualunque categoria.")
        , ["team-aozora-endurance.png"] = new("Stemma · Aozora Endurance", "logo, stemma, squadra, endurance", "", "Stemma di Aozora Endurance (endurance). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-hoshi-kart-works.png"] = new("Stemma · Hoshi Kart Works", "logo, stemma, squadra, kart", "", "Stemma di Hoshi Kart Works (kart). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-kitanami-prototype.png"] = new("Stemma · Kitanami Prototype", "logo, stemma, squadra, prototipi", "", "Stemma di Kitanami Prototype (prototipi). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-kuroda-junior-racing.png"] = new("Stemma · Kuroda Junior Racing", "logo, stemma, squadra, formula junior", "", "Stemma di Kuroda Junior Racing (formula junior). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-logos-8-kart-formula-touring-gt-endurance-sheet.png"] = new("Stemma · Squadra", "logo, stemma, squadra, generica", "", "Stemma di Squadra (generica). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-logos-kart-formula-touring-endurance-sheet.png"] = new("Stemma · Squadra", "logo, stemma, squadra, generica", "", "Stemma di Squadra (generica). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-minato-apex-kart.png"] = new("Stemma · Minato Apex Kart", "logo, stemma, squadra, kart", "", "Stemma di Minato Apex Kart (kart). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-orion-night-racing.png"] = new("Stemma · Orion Night Racing", "logo, stemma, squadra, endurance", "", "Stemma di Orion Night Racing (endurance). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-raijin-touring.png"] = new("Stemma · Raijin Touring", "logo, stemma, squadra, turismo", "", "Stemma di Raijin Touring (turismo). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-red-maple-junior.png"] = new("Stemma · Red Maple Junior", "logo, stemma, squadra, formula junior", "", "Stemma di Red Maple Junior (formula junior). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-sakura-gt-alliance.png"] = new("Stemma · Sakura GT Alliance", "logo, stemma, squadra, GT", "", "Stemma di Sakura GT Alliance (GT). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-seishin-touring-squad.png"] = new("Stemma · Seishin Touring Squad", "logo, stemma, squadra, turismo", "", "Stemma di Seishin Touring Squad (turismo). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-silver-crane-formula.png"] = new("Stemma · Silver Crane Formula", "logo, stemma, squadra, formula", "", "Stemma di Silver Crane Formula (formula). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["team-takumi-works.png"] = new("Stemma · Takumi Works", "logo, stemma, squadra, kart", "", "Stemma di Takumi Works (kart). E' un marchio, non una scena: non va usato come illustrazione di un momento della carriera.")
        , ["anime-archive-001.png"] = new("Prima di scendere in pista", "manga, kart, quattro-tempi, giappone, alba, rookie, mentore, kartodromo, gavetta, preparazione", "kart quattro tempi usato in primo piano; furgone e attrezzatura del paddock", "Il rookie si allaccia il casco accanto al kart mentre il mentore osserva la pista bagnata all'alba. Per l'inizio assoluto della carriera, il primo test e i momenti in cui tutto deve ancora cominciare.")
        , ["anime-archive-002.png"] = new("Test cronometrato in formula", "manga, formula-minore, test, cronometro, mentore, tramonto, giappone, gara, rivale", "monoposto a ruote scoperte rosso-bianca; seconda monoposto in lontananza", "Il pilota attacca una curva in monoposto mentre il mentore cronometra dal muretto al tramonto. Per test in formula, giri di riferimento, promozioni e prove sotto osservazione.")
        , ["anime-archive-003.png"] = new("Il rivale nel box", "manga, kart, rivale, box, tramonto, confronto, giappone, mentore, tensione", "telaio di kart in primo piano; caschi e attrezzatura da officina", "Il rookie e il mentore fissano il rivale appoggiato al banco nel box, con la pista bagnata alle spalle. Per rivalita' che nascono, provocazioni silenziose e confronti prima di una gara.")
        , ["anime-archive-004.png"] = new("Sconfitta sotto la pioggia", "manga, kart, sconfitta, pioggia, notte, mentore, delusione, giappone, ripartenza", "kart fermo sotto la pioggia sullo sfondo; gomme e casco appoggiati a terra", "Il pilota siede sulle gomme a testa bassa e il mentore gli tende la mano sotto la pioggia. Per gare andate male, ritiri, delusioni e il momento in cui qualcuno decide di rimanere.")
        , ["anime-archive-050.png"] = new("Riunione tecnica del team", "manga, formula-alta, riunione, dati, telemetria, squadra, ingegnere, strategia, contratto", "monoposto disegnata sulla lavagna; monoposto in pista fuori dalla finestra", "Una responsabile tecnica illustra i grafici al direttore mentre due giovani piloti ascoltano. Per analisi di stagione, strategie, valutazioni di mercato e riunioni in cui si decide il futuro di un pilota.")
        , ["anime-archive-005.png"] = new("Griglia di partenza in formula", "manga, tavola-narrativa, carriera, giappone, formula-minore, gara", "monoposto junior a ruote scoperte", "Il rookie attende il verde in monoposto mentre il mentore si sporge per le ultime istruzioni; la griglia e piena di monoposto e il pubblico riempie le tribune. Per partenze, prime gare in formula e attese cariche di tensione.")
        , ["anime-archive-006.png"] = new("La firma in officina", "manga, tavola-narrativa, carriera, giappone, paddock, contratto", "nessun mezzo obbligatorio", "Il pilota firma un foglio su un banco da lavoro mentre il mentore e il responsabile della squadra osservano; una vettura coperta aspetta sullo sfondo. Per contratti, accordi con piccole squadre e decisioni prese lontano dalla pista.")
        , ["anime-archive-007.png"] = new("Confronto dei dati fra compagni", "manga, tavola-narrativa, carriera, giappone, formula-minore, dati", "monoposto junior a ruote scoperte", "Due piloti confrontano tabulati di telemetria in un box con due monoposto in preparazione e il monitor dei tempi acceso. Per analisi tecniche, confronto interno alla squadra e ricerca del riferimento.")
    };

    /// <summary>
    /// Le altre 125 tavole d'archivio, guardate una per una.
    ///
    /// Fino a qui la posizione di una tavola d'archivio si calcolava dal
    /// numero nel nome del file — «anime-archive-093» diventava categoria
    /// «93 % 6», momento «93 / 6 % 7» — un segnaposto dichiaratamente
    /// provvisorio, mai confermato. Guardando le centotrentasei tavole una
    /// per una il segnaposto si e' rivelato quasi sempre sbagliato: kart
    /// etichettate formula, turismo etichettato GT, due fogli di soli loghi
    /// delle squadre trattati come scene con personaggi. Qui ogni tavola
    /// dichiara chi si vede, l'eta' approssimativa, l'azione e il mezzo (se
    /// c'e'), cosi' come si vede guardandola — non come suggerisce il numero.
    ///
    /// Le tavole 001-007 e 050 restano nella libreria sopra, dove erano gia'
    /// descritte a mano; questa ne copre il resto. 014, 085 e 086 mancano di
    /// proposito: sono fogli di loghi delle squadre, non scene, e Describe()
    /// li esclude esplicitamente invece di dare loro una categoria a caso.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IllustrationInfo> ArchiveLibrary = new Dictionary<string, IllustrationInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["anime-archive-008.jpg"] = new("Giovane uomo solo alla scrivania sotto una lampada, fra attrezzi da officina", "manga, archivio, tavola-narrativa, carriera, giappone, officina", "nessun mezzo", "Giovane uomo (meccanico o pilota) seduto solo a una scrivania illuminata da una lampada, circondato da attrezzi da officina: momento riflessivo."),
        ["anime-archive-009.jpg"] = new("Pilota adolescente in abitacolo riceve istruzioni al muretto, folla", "manga, archivio, tavola-narrativa, carriera, giappone, formula-minore, gara", "monoposto a ruote scoperte", "Pilota adolescente seduto in abitacolo riceve istruzioni gestuali da due uomini al muretto box, folla dietro le transenne: briefing prima della partenza."),
        ["anime-archive-010.jpg"] = new("Pilota adulto in monoposto, tramonto, bandiera a scacchi sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, formula-alta, gara", "monoposto a ruote scoperte", "Pilota adulto alla guida di una monoposto affusolata al tramonto, motivo a scacchi sulle barriere: momento di gara."),
        ["anime-archive-011.jpg"] = new("Uomo adulto osserva un giovane pilota in un’officina moderna, folla dietro i vetri", "manga, archivio, tavola-narrativa, carriera, giappone, formula-alta, officina", "monoposto o prototipo (silhouette)", "Un uomo adulto (team manager) osserva a braccia conserte un giovane pilota in un’officina moderna e illuminata, folla dietro le vetrate: valutazione o test importante."),
        ["anime-archive-012.jpg"] = new("Uomo maturo e giovane pilota camminano nel paddock fra fotografi, monoposto e folla", "manga, archivio, tavola-narrativa, carriera, giappone, formula-vertice, trasferta", "monoposto da vertice", "Un uomo maturo (dirigente) e un giovane pilota camminano nel paddock affollato di fotografi accanto a una monoposto di alto livello: presentazione ufficiale a un pubblico ampio."),
        ["anime-archive-013.jpg"] = new("Due uomini adulti su un circuito vuoto al tramonto, uno regge un trofeo", "manga, archivio, tavola-narrativa, carriera, giappone, vittoria", "nessun mezzo", "Due uomini adulti (mentore e pilota) fermi su un circuito vuoto al tramonto, uno tiene un trofeo: momento di bilancio, non una gara in corso."),
        ["anime-archive-015.jpg"] = new("Due adolescenti camminano lungo il muretto box al tramonto con i caschi in mano", "manga, archivio, tavola-narrativa, carriera, giappone, compagno", "nessun mezzo in primo piano", "Due adolescenti (pilota e compagno/rivale) camminano lungo il muretto box al tramonto, caschi in mano: momento di cameratismo fuori pista."),
        ["anime-archive-016.jpg"] = new("Famiglia riunita a tavola la sera, casco sul tavolo, si discute un accordo", "manga, archivio, tavola-narrativa, carriera, giappone, contratto", "nessun mezzo", "Una famiglia (adulti e un adolescente) seduta a tavola la sera con un casco appoggiato al centro: si discute una decisione di carriera in casa."),
        ["anime-archive-017.jpg"] = new("Pilota adolescente in kart, notte, rivale in kart blu alle spalle", "manga, archivio, tavola-narrativa, carriera, giappone, kart, gara", "due kart da gara", "Pilota adolescente alla guida di un kart di notte sotto le luci dell’impianto, un rivale in kart blu lo insegue da vicino: duello in pista."),
        ["anime-archive-018.jpg"] = new("Due adolescenti studiano una mappa/tracciato vicino a un telaio di kart, notte", "manga, archivio, tavola-narrativa, carriera, giappone, kart, dati", "telaio di kart", "Due adolescenti (pilota e compagno) studiano un tracciato o dei dati vicino a un telaio di kart smontato, di notte: preparazione tecnica."),
        ["anime-archive-019.jpg"] = new("Giovane adulto cammina da solo con il casco su una strada di campagna al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, trasferta", "furgone del team sullo sfondo", "Un pilota giovane adulto cammina da solo lungo una strada di campagna al tramonto, casco in mano, furgone del team parcheggiato dietro: trasferta o momento di solitudine."),
        ["anime-archive-020.jpg"] = new("Pilota adolescente guida un kart sotto la pioggia, mentore cronometra dal muretto", "manga, archivio, tavola-narrativa, carriera, giappone, kart, test", "kart da competizione", "Pilota adolescente guida un kart sotto la pioggia; un mentore adulto cronometra dal bordo pista con un ombrello: sessione di test bagnata."),
        ["anime-archive-021.jpg"] = new("Meccanico adulto lavora da solo di notte su un telaio di kart, sotto una lampada", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "telaio di kart", "Un meccanico adulto lavora da solo, di notte, su un telaio di kart smontato sotto la luce di una lampada da officina."),
        ["anime-archive-022.jpg"] = new("Tre adulti a un tavolo con un casco, si discute un accordo", "manga, archivio, tavola-narrativa, carriera, giappone, contratto", "nessun mezzo", "Tre adulti (pilota, team manager, un terzo) seduti a un tavolo con un casco davanti: trattativa o firma di un accordo."),
        ["anime-archive-023.jpg"] = new("Ritratto di donna adulta, capelli scuri corti, sguardo deciso", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una donna adulta con capelli scuri a caschetto e sguardo deciso: personaggio dello staff/management (stile Rei Kisaragi)."),
        ["anime-archive-024.jpg"] = new("Ritratto di uomo maturo, capelli grigi, espressione da mentore esperto", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo maturo con capelli grigi e giacca da officina: il mentore/meccanico esperto (stile Genji Arakawa)."),
        ["anime-archive-025.jpg"] = new("Ritratto di giovane uomo adulto, capelli scuri, sorriso sicuro, porto sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un giovane uomo adulto con capelli scuri e sorriso sicuro, sfondo di porto/circuito: personaggio del paddock, forse un rivale o un contatto sponsor."),
        ["anime-archive-026.jpg"] = new("Ritratto di giovane uomo, capelli ricci, occhi chiari, tuta da corsa blu", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un giovane pilota con capelli ricci e tuta da corsa blu: compagno di squadra o rivale straniero."),
        ["anime-archive-027.jpg"] = new("Ritratto di giovane uomo con quaderno in mano, sorridente, officina sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un giovane uomo con un quaderno in mano, sorridente, officina sullo sfondo: potrebbe essere l’agente al lavoro sui dati."),
        ["anime-archive-028.jpg"] = new("Ritratto di giovane donna con auricolare, paddock sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con auricolare da comunicazioni, paddock sullo sfondo: ingegnere di pista o addetta al muretto."),
        ["anime-archive-029.jpg"] = new("Ritratto di uomo adulto con occhiali e cuffie, ingegnere", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo adulto con occhiali e cuffie da comunicazione: ingegnere di telemetria."),
        ["anime-archive-030.jpg"] = new("Ritratto di uomo maturo con capelli grigi, sguardo severo, tribune sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo maturo con capelli grigi e sguardo severo, tribune del circuito sullo sfondo: team manager o direttore sportivo."),
        ["anime-archive-031.jpg"] = new("Ritratto di giovane donna con macchina fotografica, paddock", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con macchina fotografica al collo, paddock sullo sfondo: fotografa o giornalista sportiva."),
        ["anime-archive-032.jpg"] = new("Gruppo di adolescenti festeggia con bandiere a scacchi su un molo al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, vittoria", "nessun mezzo in primo piano", "Un gruppo di adolescenti festeggia con bandiere a scacchi su un molo al tramonto: celebrazione di squadra dopo un risultato, nessun mezzo inquadrato."),
        ["anime-archive-033.jpg"] = new("Giovane adulto seduto a terra sotto la pioggia con il casco, furgone sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, sconfitta", "furgone del team", "Un pilota giovane adulto siede a terra sotto la pioggia con il casco accanto, furgone del team sullo sfondo: momento di sconforto dopo un weekend andato male."),
        ["anime-archive-034.jpg"] = new("Due piloti adolescenti tagliano il traguardo appaiati in kart, bandiera a scacchi", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "due kart da gara", "Due piloti adolescenti tagliano il traguardo appaiati in kart sotto la bandiera a scacchi, folla in tribuna: arrivo punto a punto."),
        ["anime-archive-035.jpg"] = new("Gruppo di adolescenti al tavolo box con computer portatili, kart sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, kart, dati", "kart sullo sfondo", "Un gruppo di adolescenti (squadra) analizza dati al tavolo box con computer portatili, un kart visibile alle spalle."),
        ["anime-archive-036.jpg"] = new("Ritratto di giovane donna, capelli scuri corti, espressione preoccupata", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con capelli scuri corti ed espressione preoccupata: personaggio femminile del cast in un momento di tensione."),
        ["anime-archive-037.jpg"] = new("Ritratto di giovane uomo, capelli scuri, sorriso, tramonto in pista", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un giovane uomo con capelli scuri e sorriso rilassato, tramonto sullo sfondo di un circuito."),
        ["anime-archive-038.jpg"] = new("Ritratto di uomo maturo, capelli grigi, tiene un pezzo meccanico in mano", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "chiave o pezzo meccanico in mano", "Ritratto di un uomo maturo con capelli grigi che tiene un pezzo meccanico in mano, officina sullo sfondo: il meccanico/mentore al lavoro (stile Genji)."),
        ["anime-archive-039.jpg"] = new("Ritratto di uomo maturo, capelli grigi, sorriso caloroso, garage al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo maturo con capelli grigi e sorriso caloroso, garage al tramonto: il mentore in un momento sereno."),
        ["anime-archive-040.jpg"] = new("Pilota adolescente con espressione tesa e determinata, kart sfocato dietro", "manga, archivio, tavola-narrativa, carriera, giappone, kart, gara", "kart in movimento sullo sfondo", "Ritratto ravvicinato di un pilota adolescente con espressione tesa e determinata durante una gara in kart, pista sfocata alle spalle."),
        ["anime-archive-041.jpg"] = new("Pilota adolescente in tuta da kart giallo-nera, corsia box", "manga, archivio, tavola-narrativa, carriera, giappone, kart, ritratto", "kart sullo sfondo", "Ritratto a mezzo busto di un pilota adolescente in tuta da corsa giallo-nera, corsia box e altri kart alle spalle."),
        ["anime-archive-042.jpg"] = new("Giovane adulto cammina da solo su una tribuna vuota sotto la pioggia, borsa in spalla", "manga, archivio, tavola-narrativa, carriera, giappone, trasferta", "nessun mezzo", "Un pilota giovane adulto cammina da solo su una tribuna vuota sotto la pioggia con una borsa in spalla: partenza o arrivo malinconico."),
        ["anime-archive-043.jpg"] = new("Due meccanici adulti lavorano su due kart affiancati in garage", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "due kart in manutenzione", "Due meccanici adulti lavorano fianco a fianco su due kart in un garage, sotto la pioggia visibile fuori."),
        ["anime-archive-044.jpg"] = new("Sequenza a fumetti: un giovane pilota disegna e discute strategie a tavolino", "manga, archivio, tavola-narrativa, carriera, giappone, dati", "schizzi tecnici su carta", "Sequenza in stile fumetto: un giovane pilota disegna schizzi e discute strategie a tavolino con un compagno, sfondo paddock."),
        ["anime-archive-045.jpg"] = new("Due piloti adolescenti in tuta antipioggia restano accanto ai loro kart, prima della gara", "manga, archivio, tavola-narrativa, carriera, giappone, kart, gara", "due kart sotto la pioggia", "Due piloti adolescenti in tuta antipioggia rossa e gialla restano accanto ai loro kart in attesa della partenza sotto la pioggia."),
        ["anime-archive-046.jpg"] = new("Squadra riunita davanti a schermi con grafici di dati in un ufficio tecnico", "manga, archivio, tavola-narrativa, carriera, giappone, dati", "nessun mezzo, solo schermi", "Una squadra (adulti e un giovane pilota) analizza grafici e dati su grandi schermi in un ufficio tecnico: riunione di strategia."),
        ["anime-archive-047.jpg"] = new("Squadra di adolescenti festeggia con un trofeo al tramonto, kart in primo piano", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "kart in primo piano", "Una squadra di adolescenti festeggia con un trofeo al tramonto, un kart in primo piano: vittoria di campionato giovanile."),
        ["anime-archive-048.jpg"] = new("Giovane uomo solo alla scrivania di notte in citta’, casco accanto, appunti", "manga, archivio, tavola-narrativa, carriera, giappone, officina", "casco appoggiato al tavolo", "Un giovane pilota siede solo a una scrivania di notte con vista sulla citta’, casco e appunti accanto: momento di riflessione dopo il lavoro."),
        ["anime-archive-049.jpg"] = new("Furgone con carrello porta-kart su una strada costiera al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, trasferta", "furgone e carrello portamezzi", "Un furgone con carrello porta-kart percorre una strada costiera al tramonto: trasferta verso il prossimo appuntamento."),
        ["anime-archive-051.jpg"] = new("Pilota adulto in monoposto su strada di montagna, cielo sereno", "manga, archivio, tavola-narrativa, carriera, giappone, formula-alta, gara", "monoposto a ruote scoperte", "Un pilota adulto guida una monoposto bianca su una strada di montagna sotto un cielo sereno: sessione di gara o test in quota."),
        ["anime-archive-052.jpg"] = new("Due persone adulte preparano un kart all’alba su un tracciato in collina", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart in preparazione", "Due persone adulte (pilota e meccanico) preparano un kart a bordo di una strada di collina all’alba, luce calda del mattino."),
        ["anime-archive-053.jpg"] = new("Pilota e meccanico adulti osservano note tecniche accanto a una monoposto ai box", "manga, archivio, tavola-narrativa, carriera, giappone, formula-alta, dati", "monoposto a ruote scoperte", "Un pilota e un meccanico adulti osservano appunti tecnici accanto a una monoposto ferma ai box: analisi post-sessione."),
        ["anime-archive-054.jpg"] = new("Due vetture GT/turismo corrono affiancate al tramonto, scia di polvere", "manga, archivio, tavola-narrativa, carriera, giappone, gt, gara", "due vetture GT da gara", "Due vetture da competizione con carrozzeria chiusa corrono affiancate al tramonto, atmosfera drammatica da lotta per la posizione."),
        ["anime-archive-055.jpg"] = new("Squadra lavora di notte su una vettura GT sotto i fari, pioggia", "manga, archivio, tavola-narrativa, carriera, giappone, gt, officina", "vettura GT/sportiva chiusa", "Una squadra lavora di notte su una vettura da competizione con carrozzeria chiusa, sotto i fari e la pioggia: intervento ai box notturno."),
        ["anime-archive-056.jpg"] = new("Ritratto di donna adulta, capelli scuri corti, tailleur scuro, tablet in mano", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una donna adulta con capelli scuri corti e tailleur scuro, tablet in mano: manager o scout (stile Rei Kisaragi)."),
        ["anime-archive-057.jpg"] = new("Ritratto di donna adulta, capelli scuri corti, sorriso, paddock alle spalle", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una donna adulta con capelli scuri corti e leggero sorriso, paddock sfocato alle spalle."),
        ["anime-archive-058.jpg"] = new("Ritratto di uomo maturo, capelli grigi, mano al mento, gesto deciso", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo maturo con capelli grigi in un gesto deciso, mano al mento: il mentore che da’ un consiglio importante (stile Genji)."),
        ["anime-archive-059.jpg"] = new("Ritratto di uomo maturo, capelli grigi, sguardo concentrato, tiene una chiave", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "chiave inglese in mano", "Ritratto di un uomo maturo con capelli grigi, sguardo concentrato, tiene una chiave in mano: momento tecnico del mentore."),
        ["anime-archive-060.jpg"] = new("Pilota adolescente sorridente in tuta giallo-nera, kart e folla sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, kart, ritratto", "kart sullo sfondo", "Ritratto di un pilota adolescente sorridente in tuta da kart giallo-nera, kart e folla di paddock alle spalle."),
        ["anime-archive-061.jpg"] = new("Pilota adolescente con sguardo determinato, tramonto su un circuito", "manga, archivio, tavola-narrativa, carriera, giappone, kart, ritratto", "nessuno, ritratto ambientato", "Ritratto di un pilota adolescente con sguardo determinato, tramonto su un circuito alle spalle."),
        ["anime-archive-062.jpg"] = new("Ritratto di giovane donna, capelli rossi, foulard giallo, al telefono di notte", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con capelli rossi e foulard giallo, al telefono in un ufficio di notte: addetta stampa o sponsor."),
        ["anime-archive-063.jpg"] = new("Pilota adolescente scrive su un quaderno vicino a un kart, garage", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart in garage", "Un pilota adolescente scrive appunti su un quaderno seduto vicino a un kart, dentro un garage."),
        ["anime-archive-064.jpg"] = new("Ritratto di giovane donna, capelli rossi, foulard giallo, quaderno in mano, pioggia", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con capelli rossi e foulard giallo, quaderno in mano sotto la pioggia, gruppo sullo sfondo."),
        ["anime-archive-065.jpg"] = new("Pilota adolescente festeggia sventolando fogli, kart e folla al traguardo", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "kart al traguardo", "Un pilota adolescente festeggia sventolando dei fogli (risultati) accanto al kart al traguardo, folla in tribuna."),
        ["anime-archive-066.jpg"] = new("Ritratto di giovane donna, capelli rossi, giacca scura, circuito al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con capelli rossi e giacca scura, circuito al tramonto sullo sfondo."),
        ["anime-archive-067.jpg"] = new("Ritratto di uomo adulto con occhiali, cuffie, gesto da indicare dati", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo adulto con occhiali e cuffie da ingegnere, gesto di chi indica un dato su uno schermo."),
        ["anime-archive-068.jpg"] = new("Ritratto di giovane donna, capelli scuri, giacca da team, notte in paddock", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una giovane donna con capelli scuri e giacca da team, paddock notturno sullo sfondo."),
        ["anime-archive-069.jpg"] = new("Ritratto di uomo maturo, capelli grigi, gesto deciso col dito, tablet in mano", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di un uomo maturo con capelli grigi, gesto deciso col dito indice, tablet in mano: direttore che da’ un’istruzione."),
        ["anime-archive-070.jpg"] = new("Pilota adolescente in tuta giallo-nera indica qualcosa fuori scena, kart alle spalle", "manga, archivio, tavola-narrativa, carriera, giappone, kart, ritratto", "kart alle spalle", "Ritratto di un pilota adolescente in tuta giallo-nera mentre indica qualcosa fuori campo, kart e box alle spalle."),
        ["anime-archive-071.jpg"] = new("Ritratto di donna adulta, capelli scuri, tailleur scuro, cartella sottobraccio", "manga, archivio, tavola-narrativa, carriera, giappone, ritratto", "nessuno, ritratto", "Ritratto di una donna adulta con capelli scuri e tailleur scuro, cartella sottobraccio: manager pronta a una riunione."),
        ["anime-archive-072.jpg"] = new("Uomo maturo lavora su un kart all’imbrunire, luce calda", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart in manutenzione", "Un meccanico/mentore maturo lavora chino su un kart all’imbrunire, luce calda di fine giornata."),
        ["anime-archive-073.jpg"] = new("Tre adulti mostrano una giacca/livrea da corsa in paddock, bandiera visibile", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, livrea/giacca in mostra", "Tre adulti mostrano una giacca da corsa con nuova livrea in paddock: presentazione di uno sponsor o di una nuova squadra."),
        ["anime-archive-074.jpg"] = new("Giovane donna e uomo maturo discutono con casco e documenti su un tavolo", "manga, archivio, tavola-narrativa, carriera, giappone, contratto", "casco sul tavolo", "Una giovane donna e un uomo maturo discutono seduti a un tavolo con un casco e documenti: trattativa commerciale o contrattuale."),
        ["anime-archive-075.jpg"] = new("Pilota adolescente al muretto box reagisce mentre una vettura turismo passa sotto la pioggia", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, gara", "vettura turismo/TCR da gara", "Un pilota adolescente al muretto box reagisce con tensione mentre una vettura turismo bianca passa sotto la pioggia notturna: momento di gara ad alta tensione."),
        ["anime-archive-076.jpg"] = new("Fotografa scatta mentre un team festeggia con una vettura GT al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, gt, vittoria", "vettura GT", "Una fotografa scatta immagini mentre un team festeggia un risultato con una vettura GT al tramonto."),
        ["anime-archive-077.jpg"] = new("Due adulti guardano un tablet a un tavolo, componenti tecnici sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, dati", "componenti tecnici sullo sfondo, non un mezzo intero", "Due adulti (probabilmente pilota e ingegnere) osservano dati su un tablet a un tavolo, componenti meccanici sullo sfondo."),
        ["anime-archive-078.jpg"] = new("Gruppo di adolescenti riuniti attorno a un kart sotto un tendone", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart sotto il tendone del paddock", "Un gruppo di adolescenti (squadra) si riunisce attorno a un kart sotto il tendone del paddock: lavoro di squadra prima della gara."),
        ["anime-archive-079.jpg"] = new("Squadra lavora di notte su una vettura sportiva chiusa in un garage buio", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, officina", "vettura sportiva chiusa (stile coupe’ da corsa)", "Una squadra lavora di notte su una vettura sportiva a carrozzeria chiusa in un garage scarsamente illuminato: intervento tecnico serale."),
        ["anime-archive-080.jpg"] = new("Furgone con carrello porta-mezzi su una strada costiera al tramonto, pilota a bordo", "manga, archivio, tavola-narrativa, carriera, giappone, trasferta", "furgone e carrello portamezzi", "Un furgone con carrello porta-mezzi percorre una strada costiera al tramonto, un giovane pilota visibile al finestrino: trasferta verso il prossimo weekend."),
        ["anime-archive-081.jpg"] = new("Tavola in bianco e nero: due adolescenti festeggiano su un podio davanti alla folla", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "kart accennati sullo sfondo", "Tavola in stile flashback bianco e nero: due piloti adolescenti festeggiano su un podio davanti a una folla, prima vittoria dell’era kart."),
        ["anime-archive-082.jpg"] = new("Due adulti accanto a un prototipo da endurance di notte sotto la pioggia", "manga, archivio, tavola-narrativa, carriera, giappone, endurance, pioggia", "prototipo endurance", "Due persone adulte accanto a un prototipo da corse endurance, di notte, sotto una pioggia intensa: attesa o guasto durante una gara di durata."),
        ["anime-archive-083.jpg"] = new("Squadra lavora su una vettura turismo con livrea a righe in corsia box, giorno", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, officina", "vettura turismo con livrea a righe", "Una squadra lavora di giorno su una vettura turismo con livrea bianca e rossa a righe, ferma in corsia box."),
        ["anime-archive-084.jpg"] = new("Squadra spinge una vettura GT moderna sotto la pioggia al crepuscolo", "manga, archivio, tavola-narrativa, carriera, giappone, gt, pioggia", "vettura GT moderna", "Una squadra spinge una vettura GT moderna e affusolata sotto la pioggia al crepuscolo, corsia box bagnata."),
        ["anime-archive-087.jpg"] = new("Monoposto vista da dietro sotto la pioggia intensa, pilota adulto alla guida", "manga, archivio, tavola-narrativa, carriera, giappone, formula-alta, pioggia", "monoposto a ruote scoperte", "Una monoposto vista da dietro solca la pista sotto una pioggia intensa, pilota adulto alla guida: momento di gara drammatico."),
        ["anime-archive-088.jpg"] = new("Due telai di kart in riparazione in un’officina spoglia", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "due telai di kart", "Due telai di kart smontati sono in riparazione in un’officina spoglia e poco illuminata."),
        ["anime-archive-089.jpg"] = new("Due adulti preparano un kart su una strada di collina al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart in preparazione", "Due persone adulte (pilota e mentore) preparano un kart a bordo di una strada di collina al tramonto."),
        ["anime-archive-090.jpg"] = new("Pilota adolescente festeggia tagliando il traguardo in kart, squadra esulta", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "kart al traguardo", "Un pilota adolescente festeggia tagliando il traguardo in kart, la squadra esulta a bordo pista."),
        ["anime-archive-091.jpg"] = new("Pilota adolescente seduto appoggiato al kart con il casco, in garage", "manga, archivio, tavola-narrativa, carriera, giappone, kart, riposo", "kart in garage", "Un pilota adolescente siede appoggiato al proprio kart con il casco accanto, momento di pausa in garage."),
        ["anime-archive-092.jpg"] = new("Pilota adolescente con asciugamano e casco, squadra carica il kart su un furgone", "manga, archivio, tavola-narrativa, carriera, giappone, kart, trasferta", "kart caricato su furgone", "Un pilota adolescente con asciugamano al collo e casco in mano osserva la squadra caricare il kart su un furgone: fine weekend."),
        ["anime-archive-093.jpg"] = new("Due meccanici adulti lavorano su una vettura turismo blu in garage buio", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, officina", "vettura turismo (stile GT-R)", "Due meccanici adulti lavorano con attrezzi su una vettura turismo blu scuro in un garage poco illuminato."),
        ["anime-archive-094.jpg"] = new("Due vetture turismo d’epoca affiancate, montagna sullo sfondo, giorno", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, gara", "due vetture turismo (stile AE86)", "Due vetture turismo con livree bianco-rosse sono affiancate su un rettilineo con una montagna sullo sfondo: rivalita’ diretta."),
        ["anime-archive-095.jpg"] = new("Due monoposto corrono affiancate sotto la pioggia con una montagna sullo sfondo", "manga, archivio, tavola-narrativa, carriera, giappone, formula-alta, pioggia", "due monoposto a ruote scoperte", "Due monoposto corrono ruota a ruota sotto la pioggia con una montagna innevata sullo sfondo: duello di gara."),
        ["anime-archive-096.jpg"] = new("Uomo e donna adulti guardano un laptop, vettura turismo visibile fuori dalla finestra", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, dati", "vettura turismo visibile fuori", "Un uomo e una donna adulti analizzano dati su un laptop in ufficio, una vettura turismo parcheggiata visibile fuori dalla finestra."),
        ["anime-archive-097.jpg"] = new("Giovane uomo con cappuccio guarda il telefono per strada, di sera", "manga, archivio, tavola-narrativa, carriera, giappone, riposo", "nessun mezzo", "Un giovane pilota con felpa a cappuccio guarda il telefono per strada, di sera: momento privato fuori dalla pista."),
        ["anime-archive-098.jpg"] = new("Pilota donna giovane adulta in tuta da corsa completa, guanti, monoposto rossa", "manga, archivio, tavola-narrativa, carriera, giappone, formula-minore, ritratto", "monoposto a ruote scoperte", "Ritratto di una pilota donna (giovane adulta) in tuta da corsa bianca e rossa con guanti, accanto a una monoposto: rivale o compagna di squadra."),
        ["anime-archive-099.jpg"] = new("Uomo adulto legge dati a monitor multipli, vettura turismo blu visibile in garage", "manga, archivio, tavola-narrativa, carriera, giappone, turismo, dati", "vettura turismo (stile GT-R) sullo sfondo", "Un uomo adulto analizza dati su piu’ monitor in un ufficio tecnico, una vettura turismo blu visibile attraverso la vetrata del garage."),
        ["anime-archive-100.jpg"] = new("Due piloti adolescenti in tuta rossa festeggiano al tramonto con bandiera a scacchi", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "kart accennato sullo sfondo", "Due piloti adolescenti in tuta da corsa rossa festeggiano abbracciandosi al tramonto, bandiera a scacchi in mano."),
        ["anime-archive-101.jpg"] = new("Due uomini adulti discutono documenti dentro un furgone, di notte", "manga, archivio, tavola-narrativa, carriera, giappone, dati", "furgone del team", "Due uomini adulti discutono dei documenti seduti dentro un furgone del team, di notte: pianificazione della trasferta."),
        ["anime-archive-102.jpg"] = new("Tavola in bianco e nero: due persone lavorano su un kart in un box vuoto", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart in manutenzione", "Tavola in stile bianco e nero: due persone lavorano su un kart in un box vuoto e silenzioso."),
        ["anime-archive-103.jpg"] = new("Due persone sedute vicine in un abitacolo stretto, conversazione ravvicinata", "manga, archivio, tavola-narrativa, carriera, giappone, kart, dati", "abitacolo di kart o vettura compatta", "Due persone (mentore e giovane pilota) sedute vicine in un abitacolo stretto: conversazione tecnica ravvicinata prima di scendere in pista."),
        ["anime-archive-104.jpg"] = new("Pilota adolescente guida un kart su strada di montagna bagnata, mentore con cronometro", "manga, archivio, tavola-narrativa, carriera, giappone, kart, test", "kart da competizione", "Un pilota adolescente guida un kart su una strada di montagna bagnata, un mentore osserva con cronometro dal bordo pista."),
        ["anime-archive-105.jpg"] = new("Gruppo di adulti e un adolescente si riuniscono sotto tende da paddock, giorno", "manga, archivio, tavola-narrativa, carriera, giappone, riposo", "nessun mezzo in primo piano", "Un gruppo misto di adulti e un adolescente si riunisce sotto tende da paddock colorate: pausa o briefing informale."),
        ["anime-archive-106.jpg"] = new("Due piloti adolescenti in kart, caschi ravvicinati, duello in curva", "manga, archivio, tavola-narrativa, carriera, giappone, kart, gara", "due kart da gara", "Due piloti adolescenti si contendono la posizione in kart, caschi quasi a contatto: duello serrato in curva."),
        ["anime-archive-107.jpg"] = new("Meccanico adulto stringe la mano a un pilota adolescente in garage, casco sul tavolo", "manga, archivio, tavola-narrativa, carriera, giappone, kart, contratto", "kart sullo sfondo", "Un meccanico/team manager adulto stringe la mano a un pilota adolescente in un garage, casco appoggiato sul tavolo: accordo raggiunto."),
        ["anime-archive-108.jpg"] = new("Pilota adolescente accovacciato ed esausto vicino al kart di notte, pioggia", "manga, archivio, tavola-narrativa, carriera, giappone, kart, sconfitta", "kart fermo di notte", "Un pilota adolescente siede accovacciato ed esausto accanto al proprio kart, di notte, sotto la pioggia: dopo una gara pesante."),
        ["anime-archive-109.jpg"] = new("Kart caricato su un piccolo furgone lungo una strada di montagna al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, kart, trasferta", "kart caricato su furgone", "Un kart e’ caricato su un piccolo furgone che percorre una strada di montagna al tramonto: trasferta verso casa."),
        ["anime-archive-110.jpg"] = new("Tre adolescenti festeggiano un podio con trofeo, kart in primo piano", "manga, archivio, tavola-narrativa, carriera, giappone, kart, vittoria", "kart in primo piano", "Tre adolescenti (pilota, compagno e sostenitrice) festeggiano un podio con trofeo alzato, kart in primo piano."),
        ["anime-archive-111.jpg"] = new("Haru adolescente e un negoziante adulto parlano a un banco di frutta e verdura", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, bancarella di un negozio", "Haru (adolescente, l’agente del pilota) parla con un negoziante adulto a un banco di frutta e verdura: visita di sponsorizzazione a un negozio di quartiere."),
        ["anime-archive-112.jpg"] = new("Haru adolescente mostra un dossier a un negoziante adulto in un negozio di ricambi", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, negozio di ricambi", "Haru (adolescente) mostra un dossier/fotografie a un negoziante adulto in un negozio di ricambi auto: trattativa di sponsorizzazione."),
        ["anime-archive-113.jpg"] = new("Haru adolescente parla con il cuoco di un ristorante di sushi, al bancone", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, bancone sushi", "Haru (adolescente) parla con il cuoco adulto di un ristorante di sushi seduto al bancone, fumo dai piatti: trattativa di sponsorizzazione a cena."),
        ["anime-archive-114.jpg"] = new("Haru adolescente mostra un album fotografico a un uomo adulto in un negozio di ricambi moto", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, negozio di ricambi", "Haru (adolescente) mostra un album fotografico a un uomo adulto in un negozio di ricambi per moto/kart: presentazione del pilota a un potenziale sponsor."),
        ["anime-archive-115.jpg"] = new("Haru adolescente mostra un dossier con foto di gara a un negoziante adulto", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, negozio", "Haru (adolescente) mostra un dossier con foto di gara a un negoziante adulto: trattativa di sponsorizzazione."),
        ["anime-archive-116.jpg"] = new("Giovane uomo con felpa blu mostra un dossier a un fruttivendolo adulto, mercato", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, bancarella di frutta", "Un giovane (Haru) con felpa blu mostra un dossier a un fruttivendolo adulto davanti a una bancarella di frutta: visita commerciale di quartiere."),
        ["anime-archive-117.jpg"] = new("Giovane uomo controlla dati su un tablet al bancone di un negozio di alimentari, sera", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, negozio", "Un giovane (Haru) controlla dati su un tablet seduto al bancone di un piccolo negozio di alimentari, di sera."),
        ["anime-archive-118.jpg"] = new("Giovane uomo con documenti in un ristorante illuminato da lanterne, casco sul bancone", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "casco sul bancone", "Un giovane (Haru) con documenti in mano in un ristorante illuminato da lanterne, un casco appoggiato sul bancone: trattativa informale a cena."),
        ["anime-archive-119.jpg"] = new("Giovane uomo mostra un dossier a un negoziante maturo circondato da ricambi", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, officina/negozio", "Un giovane (Haru) mostra un dossier a un negoziante maturo circondato da scaffali di ricambi: presentazione del progetto sportivo."),
        ["anime-archive-120.jpg"] = new("Giovane uomo mostra un dossier con il casco in mano in un negozio di pneumatici", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "casco in mano", "Un giovane (Haru) mostra un dossier tenendo il casco del pilota in un negozio di pneumatici, scaffali di gomme sullo sfondo."),
        ["anime-archive-121.jpg"] = new("Fumetto: uomo adulto in giacca fa un gesto di rifiuto, giovane a testa bassa", "manga, archivio, tavola-narrativa, carriera, giappone, contratto", "nessun mezzo", "Sequenza a fumetti: un uomo adulto in giacca fa un gesto di rifiuto verso un giovane pilota a testa bassa: una trattativa che va male."),
        ["anime-archive-122.jpg"] = new("Fumetto in bianco e nero: giovane seduto alla scrivania con un dossier, ufficio spoglio", "manga, archivio, tavola-narrativa, carriera, giappone, officina", "dossier con foto di vettura", "Sequenza in bianco e nero: un giovane siede solo a una scrivania spoglia con un dossier aperto: momento di attesa o rilettura di un rifiuto."),
        ["anime-archive-123.jpg"] = new("Fumetto: uomo maturo sorride e stringe la mano a un giovane, casco sul tavolo", "manga, archivio, tavola-narrativa, carriera, giappone, contratto", "casco sul tavolo", "Sequenza a fumetti: un uomo maturo sorride e stringe la mano a un giovane pilota, casco appoggiato sul tavolo: accordo raggiunto con successo."),
        ["anime-archive-124.jpg"] = new("Fumetto: pilota adolescente in tuta rossa si allena su tapis roulant e a terra", "manga, archivio, tavola-narrativa, carriera, giappone, kart, allenamento", "nessun mezzo, palestra", "Sequenza a fumetti: un pilota adolescente in tuta rossa e bianca si allena su un tapis roulant e fa flessioni: preparazione fisica."),
        ["anime-archive-125.jpg"] = new("Fumetto: pilota adolescente in tuta rossa corre lungo una strada al tramonto", "manga, archivio, tavola-narrativa, carriera, giappone, kart, allenamento", "nessun mezzo", "Sequenza a fumetti: un pilota adolescente in tuta rossa e bianca corre lungo una strada costiera al tramonto con vista su un lago: allenamento."),
        ["anime-archive-126.jpg"] = new("Fumetto: pilota adolescente si prepara con un compagno, schizzi tecnici", "manga, archivio, tavola-narrativa, carriera, giappone, kart, preparazione", "nessun mezzo, schizzi tecnici", "Sequenza a fumetti: un pilota adolescente si prepara insieme a un compagno adulto, schizzi tecnici del mezzo sullo sfondo."),
        ["anime-archive-127.jpg"] = new("Tuta da corsa rossa appoggiata su una sedia con appunti e una tazza di caffe’", "manga, archivio, tavola-narrativa, carriera, giappone, kart, preparazione", "tuta da corsa, nessun mezzo", "Una tuta da corsa rossa e bianca e’ appoggiata su una sedia accanto ad appunti e una tazza di caffe’: attesa prima della gara."),
        ["anime-archive-128.jpg"] = new("Fumetto: pilota adolescente in tuta rossa con un gruppo di bambini e un kart", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "kart in primo piano", "Sequenza a fumetti: un pilota adolescente in tuta rossa e bianca posa con un gruppo di bambini e un kart: giornata promozionale o di reclutamento."),
        ["anime-archive-129.jpg"] = new("Fumetto: pilota adolescente controlla dati su un tablet vicino a un kart", "manga, archivio, tavola-narrativa, carriera, giappone, kart, dati", "kart in primo piano", "Sequenza a fumetti: un pilota adolescente in tuta rossa controlla dati su un tablet seduto vicino a un kart."),
        ["anime-archive-130.jpg"] = new("Fumetto: pilota adolescente con famiglia/squadra in un mercato di quartiere", "manga, archivio, tavola-narrativa, carriera, giappone, kart, trasferta", "nessun mezzo, mercato", "Sequenza a fumetti: un pilota adolescente in tuta da corsa cammina con famiglia e squadra in un mercato di quartiere: momento di vita fuori pista."),
        ["anime-archive-131.jpg"] = new("Fumetto: pilota adolescente lavora con un meccanico maturo su componenti meccanici", "manga, archivio, tavola-narrativa, carriera, giappone, kart, officina", "componenti meccanici", "Sequenza a fumetti: un pilota adolescente in tuta rossa lavora fianco a fianco con un meccanico maturo su componenti meccanici."),
        ["anime-archive-132.jpg"] = new("Giovane uomo con felpa stringe la mano a un meccanico adulto in un negozio di pneumatici", "manga, archivio, tavola-narrativa, carriera, giappone, contratto", "nessun mezzo, negozio", "Un giovane (Haru) con felpa blu stringe la mano a un meccanico adulto in un negozio di pneumatici: accordo di sponsorizzazione concluso."),
        ["anime-archive-133.jpg"] = new("Giovane uomo mostra un dossier a un anziano fruttivendolo in un mercato di quartiere", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "nessun mezzo, bancarella", "Un giovane (Haru) mostra un dossier a un anziano fruttivendolo in un mercato di quartiere, cassette di frutta intorno."),
        ["anime-archive-134.jpg"] = new("Giovane uomo discute con il cuoco di un ristorante di sushi, casco sul bancone", "manga, archivio, tavola-narrativa, carriera, giappone, sponsor", "casco sul bancone", "Un giovane (Haru) discute animatamente con il cuoco adulto di un ristorante di sushi, casco del pilota appoggiato sul bancone: trattativa di sponsorizzazione."),
        ["anime-archive-135.jpg"] = new("Fumetto: giovane uomo parla con un negoziante in un negozio di ricambi, piu’ vignette", "manga, archivio, tavola-narrativa, carriera, giappone, officina", "nessun mezzo, negozio", "Sequenza a fumetti: un giovane (Haru) parla con un negoziante adulto in un negozio di ricambi, sequenza di piu’ vignette."),
        ["anime-archive-136.jpg"] = new("Giovane uomo cammina da solo di notte davanti a una vetrina illuminata, aria pensierosa", "manga, archivio, tavola-narrativa, carriera, giappone, sconfitta", "nessun mezzo", "Un giovane (Haru) cammina da solo di notte davanti a una vetrina illuminata, aria pensierosa: dopo una trattativa andata male."),
    };

    /// <summary>I tre fogli di loghi delle squadre: non sono scene e non entrano nella rotazione narrativa.</summary>
    private static readonly HashSet<string> NonScene = new(StringComparer.OrdinalIgnoreCase)
    {
        "anime-archive-014.jpg", "anime-archive-085.jpg", "anime-archive-086.jpg"
    };

    private static readonly IReadOnlyDictionary<string, string> Contexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["home-generic-01.png"] = "SIGLA HOME · rookie davanti al primo bivio · atmosfera: alba e possibilità",
        ["home-generic-02.png"] = "SIGLA HOME · lavoro nel box · atmosfera: concentrazione e gavetta",
        ["home-generic-03.png"] = "SIGLA HOME · prima trattativa · atmosfera: sponsor e rischio",
        ["home-generic-04.png"] = "SIGLA HOME · rivali sulla griglia · atmosfera: tensione prima del via",
        ["home-generic-05.png"] = "SIGLA HOME · lettura dei dati · atmosfera: analisi e crescita",
        ["home-generic-06.png"] = "SIGLA HOME · primo traguardo · atmosfera: gioia conquistata",
        ["home-generic-07.png"] = "SIGLA HOME · viaggio notturno · atmosfera: sacrificio e perseveranza",
        ["home-generic-08.png"] = "SIGLA HOME · trasferimento verso il circuito · atmosfera: strada e ambizione",
        ["home-generic-09.png"] = "SIGLA HOME · test di formula minore · atmosfera: salto di categoria"
        , ["activity-pilota-palestra.png"] = "ATTIVITÀ · palestra · pilota rookie · nessun mezzo visibile"
        , ["activity-pilota-corsa-aperto.png"] = "ATTIVITÀ · corsa all'aperto · pilota rookie · pista e kart sullo sfondo"
        , ["activity-pilota-massaggio-fisioterapia.png"] = "ATTIVITÀ · fisioterapia · recupero fisico · nessun mezzo visibile"
        , ["activity-pilota-riposo-casa.png"] = "ATTIVITÀ · riposo a casa · scelta di non spendere · nessun mezzo visibile"
        , ["activity-pilota-tifosi.png"] = "ATTIVITÀ · incontro con i tifosi · kart e paddock sullo sfondo"
        , ["activity-pilota-social.png"] = "ATTIVITÀ · post sui social · smartphone · nessun mezzo visibile"
        , ["activity-pilota-pr.png"] = "ATTIVITÀ · relazioni pubbliche · evento locale · nessun mezzo visibile"
        , ["activity-pilota-lavoro.png"] = "ATTIVITÀ · giornata in officina · pilota rookie · kart smontato e ricambi"
        , ["haru-visit-ferramenta-sponsor.png"] = "SPONSOR · Haru in ferramenta · utensili e ricambi · nessun kart in primo piano"
        , ["haru-visit-fruttivendolo-sponsor.png"] = "SPONSOR · Haru dal fruttivendolo · negozio alimentari · nessun mezzo"
        , ["haru-visit-ramen-kizuna-sponsor.png"] = "SPONSOR · Haru da Ramen Kizuna · trattoria di quartiere · nessun mezzo"
        , ["haru-visit-sushi-poke-sponsor.png"] = "SPONSOR · Haru in locale sushi/poke · nessun mezzo"
        , ["haru-visit-gommista-sponsor.png"] = "SPONSOR · Haru dal gommista · pneumatici e cerchi kart"
        , ["haru-visit-agenzia-assicurativa-rifiuto.png"] = "SPONSOR · Haru in agenzia assicurativa · ufficio e rifiuto"
        , ["haru-sponsor-respinto-sulla-porta.png"] = "SPONSOR · Haru respinto sulla porta · rifiuto · nessun mezzo"
        , ["haru-accordo-stretta-mano-nuovo.png"] = "SPONSOR · Haru chiude l'accordo · stretta di mano · kart in officina"
    };

    public static string Context(string path)
    {
        var file = Path.GetFileName(path);
        var info = Describe(path);
        if (info != null) return $"{info.Title} · TAG: {info.Tags} · MEZZI: {info.Vehicles} · {info.Description}";
        if (Contexts.TryGetValue(file, out var context)) return context;
        return file switch
        {
            var x when x.Contains("kart", StringComparison.OrdinalIgnoreCase) => "KART · gavetta e primi test",
            var x when x.Contains("formula", StringComparison.OrdinalIgnoreCase) => "FORMULA · salto di categoria",
            var x when x.Contains("touring", StringComparison.OrdinalIgnoreCase) => "TURISMO · duello ruota a ruota",
            var x when x.Contains("gt", StringComparison.OrdinalIgnoreCase) => "GT · gara endurance e gestione gomme",
            var x when x.Contains("endurance", StringComparison.OrdinalIgnoreCase) => "ENDURANCE · strategia e resistenza",
            _ => "TAVOLA NARRATIVA · illustrazione originale della carriera"
        };
    }

    /// <summary>
    /// Tutte le tavole installate, ciascuna con la propria scheda.
    ///
    /// Si legge una volta e resta in memoria: la cartella delle tavole non
    /// cambia mentre il programma gira.
    /// </summary>
    private static IReadOnlyList<(string File, IllustrationInfo Info)>? tutte;

    /// <summary>
    /// Le tavole che dichiarano davvero sia la disciplina sia il momento.
    ///
    /// Diverso da <see cref="Find"/>, che a un momento senza tavole ripiega
    /// sulla disciplina — giusto per giocare, inutile per sapere che cosa
    /// manca da disegnare. Serve al collaudo dei contenuti.
    /// </summary>
    public static int QuanteCoprono(string disciplina, string momento)
    {
        bool Ha((string File, IllustrationInfo Info) x, string tag) =>
            x.Info.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase) ||
            x.Info.Title.Contains(tag, StringComparison.OrdinalIgnoreCase);
        return Tutte().Count(x => Ha(x, disciplina) && Ha(x, momento));
    }

    private static IReadOnlyList<(string File, IllustrationInfo Info)> Tutte()
    {
        if (tutte != null) return tutte;
        var cartella = AssetPaths.Root;
        var elenco = new List<(string, IllustrationInfo)>();
        try
        {
            // Anche i JPEG: le tavole convertite per occupare dieci volte meno
            // sono le stesse immagini, e il catalogo le riconosce dal nome.
            if (Directory.Exists(cartella))
                foreach (var file in Directory.EnumerateFiles(cartella, "*.*", SearchOption.AllDirectories)
                             .Where(x => x.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                         || x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
                {
                    // I loghi delle squadre non sono tavole narrative.
                    if (file.Contains("team-logos", StringComparison.OrdinalIgnoreCase)) continue;
                    // Nemmeno le icone dell'interfaccia: sono arredo della
                    // schermata, non scene. Senza questa esclusione entravano
                    // nel catalogo per il solo nome — «oggi-sponsor.png»
                    // prendeva il tag sponsor — e potevano finire come
                    // illustrazione di una trattativa.
                    if (file.Contains("ui-icons", StringComparison.OrdinalIgnoreCase)) continue;
                    var relativo = Path.GetRelativePath(cartella, file).Replace('\\', '/');
                    var info = Describe(file);
                    if (info != null) elenco.Add((relativo, info));
                }
        }
        catch { }
        tutte = elenco;
        return tutte;
    }

    /// <summary>
    /// Le tavole che parlano di una certa disciplina e di un certo momento,
    /// ordinate in modo stabile ma diverso da carriera a carriera.
    ///
    /// <paramref name="disciplina"/> e <paramref name="momento"/> sono tag: si
    /// preferiscono le tavole che hanno entrambi, poi quelle con la sola
    /// disciplina. Se non c'e' niente di coerente si restituisce una lista
    /// vuota — meglio nessuna tavola che una tavola sbagliata, perche' una GT
    /// sopra una carriera di kart racconta un'altra storia.
    /// </summary>
    /// <summary>Le categorie: una tavola che ne dichiara una non è mai neutra.</summary>
    private static readonly string[] Categorie =
        ["kart", "turismo", "formula-minore", "formula-alta", "formula-vertice", "gt", "endurance"];

    private static bool DichiaraUnaCategoria(IllustrationInfo info) =>
        Categorie.Any(x => info.Tags.Contains(x, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Vero se questa tavola puo' comparire per la disciplina indicata: o non
    /// dichiara nessuna categoria (e' fuori pista, vale sempre), o dichiara
    /// proprio quella. Usata da chi pesca dalla libreria fuori da Find, per
    /// esempio la rotazione delle attivita' quotidiane.
    /// </summary>
    public static bool FitsDiscipline(string path, string disciplina)
    {
        var info = Describe(path);
        if (info == null) return false;
        if (!DichiaraUnaCategoria(info)) return true;
        return !string.IsNullOrWhiteSpace(disciplina) && info.Tags.Contains(disciplina, StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> Find(string disciplina, string? momento = null, int seme = 0, int quante = 6)
    {
        bool Ha(IllustrationInfo i, string? tag) =>
            string.IsNullOrWhiteSpace(tag) ||
            i.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase) ||
            i.Title.Contains(tag, StringComparison.OrdinalIgnoreCase);

        var candidate = Tutte().Where(x => Ha(x.Info, disciplina)).ToList();

        // Le tavole fuori pista valgono per qualunque disciplina, ma solo se
        // raccontano il momento richiesto: un'officina di notte sta bene sopra
        // «officina» in kart come in GT, mentre non ha niente da dire sopra una
        // gara. Entrano quindi soltanto insieme al momento, mai da sole.
        // Una tavola vale come neutra solo se non dichiara NESSUNA categoria.
        //
        // Prima bastava che non fosse della categoria richiesta: una tavola di
        // kart marcata «fuori pista» superava il controllo quando si chiedeva
        // GT, e finiva sopra una gara GT. È il mescolamento che questa
        // biblioteca esiste per impedire — se una tavola sa di che categoria è,
        // quella categoria vale, e altrove non si usa.
        var neutre = string.IsNullOrWhiteSpace(momento)
            ? []
            : Tutte().Where(x => x.Info.Tags.Contains("fuori-pista", StringComparison.OrdinalIgnoreCase)
                                 && !DichiaraUnaCategoria(x.Info)
                                 && Ha(x.Info, momento)).ToList();

        if (candidate.Count == 0 && neutre.Count == 0) return [];

        // La categoria viene prima, sempre.
        //
        // Prima le tavole di categoria e quelle neutre venivano unite e poi
        // rimescolate insieme: sopra una gara di kart finivano una palestra o
        // una visita in officina, perché il rimescolamento cancellava la
        // priorità. Le neutre servono a non restare mai senza immagine, non a
        // sostituire quella giusta: si ordinano a parte e si accodano.
        var perCategoria = candidate.Where(x => Ha(x.Info, momento))
            .OrderBy(x => Impronta(x.File, seme))
            .Select(x => x.File);
        var diRiserva = neutre
            .OrderBy(x => Impronta(x.File, seme))
            .Select(x => x.File);

        var scelte = perCategoria.Concat(diRiserva).ToList();
        // Nessuna tavola con quel momento: si resta nella categoria, che è
        // comunque coerente, invece di ripiegare su una scena qualunque.
        if (scelte.Count == 0)
            scelte = candidate.OrderBy(x => Impronta(x.File, seme)).Select(x => x.File).ToList();

        return scelte.Take(Math.Max(1, quante)).ToList();
    }

    /// <summary>Un numero stabile per un file e un seme: nessun caso, nessun Random.</summary>
    private static long Impronta(string file, int seme)
    {
        var hash = 2166136261L ^ seme;
        foreach (var c in file) { hash ^= char.ToLowerInvariant(c); hash *= 16777619; hash &= 0xFFFFFFFFL; }
        return hash;
    }

    public static IllustrationInfo? Describe(string path)
    {
        var file = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(file)) return null;
        if (Library.TryGetValue(file, out var explicitInfo)) return explicitInfo;

        // Lo stesso ripiego sull'estensione di AssetPaths.File, qui dentro.
        //
        // Le tavole della libreria curata a mano sono citate con estensione
        // .png, ma ogni tavola convertita in .jpg per pesare dieci volte meno
        // ha mantenuto lo stesso nome. Confrontando la stringa intera,
        // "kart-dawn-garage.jpg" non trovava mai "kart-dawn-garage.png": la
        // ricerca falliva SEMPRE per estensione sbagliata, e centotrentuno
        // delle centotrentadue voci scritte a mano — quasi tutta la libreria —
        // non venivano mai lette. Ogni tavola ripiegava sul riconoscimento
        // generico dal nome, anche quelle per cui qualcuno aveva gia' scritto
        // la scheda giusta.
        var nomeBase = Path.GetFileNameWithoutExtension(file);
        foreach (var estensione in new[] { ".png", ".jpg", ".jpeg", ".webp" })
            if (Library.TryGetValue(nomeBase + estensione, out var viaEstensione))
                return viaEstensione;

        if (Contexts.TryGetValue(file, out var context))
            return new IllustrationInfo(context.Split('·')[0].Trim(), "home, sigla, carriera", "mezzi variabili: verificare la tavola", context);
        foreach (var estensione in new[] { ".png", ".jpg", ".jpeg", ".webp" })
            if (Contexts.TryGetValue(nomeBase + estensione, out var contestoViaEstensione))
                return new IllustrationInfo(contestoViaEstensione.Split('·')[0].Trim(), "home, sigla, carriera", "mezzi variabili: verificare la tavola", contestoViaEstensione);

        // I fogli di soli loghi non sono scene: niente disciplina inventata.
        if (NonScene.Contains(file)) return null;
        if (ArchiveLibrary.TryGetValue(file, out var archiveInfo)) return archiveInfo;
        foreach (var estensione in new[] { ".png", ".jpg", ".jpeg", ".webp" })
            if (ArchiveLibrary.TryGetValue(nomeBase + estensione, out var archiveViaEstensione))
                return archiveViaEstensione;

        var stem = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

        var tags = new List<string> { "manga", "tavola-narrativa", "carriera" };
        void Tag(string token, string tag) { if (stem.Contains(token, StringComparison.Ordinal) && !tags.Contains(tag)) tags.Add(tag); }
        Tag("character", "ritratto-personaggio"); Tag("portrait", "ritratto-personaggio");
        Tag("kart", "kart"); Tag("rookie", "rookie");
        // Le tre altezze della formula sono categorie diverse, e vanno
        // riconosciute PRIMA del generico: un file chiamato
        // «campionato-formula-vertice» finiva etichettato formula-minore —
        // cioe' la tavola della Formula 1 compariva sopra una Formula 4, e
        // quella della Formula 1 restava introvabile.
        var altaOVertice = stem.Contains("formula-alta", StringComparison.Ordinal)
                           || stem.Contains("formula-vertice", StringComparison.Ordinal);
        Tag("formula-alta", "formula-alta");
        Tag("formula-vertice", "formula-vertice");
        Tag("formula1", "formula-vertice"); Tag("formula-1", "formula-vertice");
        Tag("formula2", "formula-alta"); Tag("formula-2", "formula-alta");
        Tag("formula3", "formula-alta"); Tag("formula-3", "formula-alta");
        if (!altaOVertice) Tag("formula", "formula-minore");
        Tag("touring", "turismo"); Tag("turismo", "turismo");
        Tag("gt", "gt"); Tag("endurance", "endurance");
        Tag("test", "test"); Tag("garage", "officina"); Tag("pit", "pitstop"); Tag("grid", "griglia");
        Tag("rival", "rivale"); Tag("sponsor", "sponsor"); Tag("contract", "contratto");
        Tag("victory", "vittoria"); Tag("podium", "podio"); Tag("champion", "campionato");
        Tag("rain", "pioggia"); Tag("night", "notte"); Tag("dawn", "alba"); Tag("travel", "trasferta");
        Tag("japan", "giappone"); Tag("crisis", "crisi"); Tag("financial", "budget");

        // I nomi in italiano: le tavole aggiunte dopo li usano, e senza queste
        // voci restavano descritte come "tavola paddock", cioe' inservibili per
        // chi deve scegliere l'immagine giusta per il momento giusto.
        Tag("gara", "gara"); Tag("qualifica", "qualifica"); Tag("prova", "test");
        Tag("vittoria", "vittoria"); Tag("podio", "podio"); Tag("primo-punto", "punti");
        Tag("campionato", "campionato"); Tag("debutto", "debutto"); Tag("griglia", "griglia");
        Tag("pioggia", "pioggia"); Tag("bagnat", "pioggia"); Tag("notte", "notte");
        Tag("alba", "alba"); Tag("estate", "estate"); Tag("tramonto", "tramonto");
        Tag("rivale", "rivale"); Tag("duello", "duello"); Tag("sorpasso", "sorpasso");
        Tag("incidente", "incidente"); Tag("danneg", "danno"); Tag("foratura", "guasto");
        Tag("delusione", "sconfitta"); Tag("mancato", "sconfitta"); Tag("stanco", "fatica");
        // I momenti nominati direttamente nel nome del file. Mancavano, e le
        // tavole disegnate apposta per un momento non venivano mai trovate
        // proprio per quel momento.
        Tag("sconfitta", "sconfitta"); Tag("scuola", "scuola"); Tag("classe", "scuola");
        Tag("officina", "officina"); Tag("vertice", "formula-vertice");
        Tag("contratto", "contratto"); Tag("accordo", "contratto"); Tag("firma", "contratto");
        Tag("rifiuto", "rifiuto"); Tag("respint", "rifiuto"); Tag("proposta", "trattativa");
        Tag("trattativa", "trattativa"); Tag("visita", "trattativa"); Tag("incontro", "trattativa");
        Tag("pagamento", "budget"); Tag("budget", "budget"); Tag("costi", "budget");
        Tag("attivita", "attivita-pilota"); Tag("palestra", "allenamento");
        Tag("corsa", "allenamento"); Tag("fisioterapia", "recupero");
        Tag("massaggio", "recupero"); Tag("riposo", "riposo");
        Tag("social", "comunicazione"); Tag("stampa", "comunicazione");
        Tag("relazioni-pubbliche", "comunicazione"); Tag("autograf", "tifosi");
        Tag("tifosi", "tifosi"); Tag("bambini", "tifosi");
        Tag("trasferta", "trasferta"); Tag("treno", "trasferta");
        Tag("aeroporto", "trasferta"); Tag("van", "trasferta");
        Tag("officina", "officina"); Tag("telemetria", "dati");
        Tag("calendario", "agenda"); Tag("decisione", "scelta");
        Tag("rottami", "gavetta"); Tag("casco-usato", "gavetta");
        Tag("lavoro", "lavoro");

        // Le cento tavole di kart numerate (kart-001 … kart-100).
        //
        // Portano un vocabolario che la tabella sopra non copriva: sessantatre
        // su cento uscivano con il solo tag «kart» e nessun momento, quindi
        // entravano soltanto come ripiego quando non c'era di meglio — cioe'
        // quasi mai. Qui il loro lessico viene ricondotto agli stessi tag che
        // il motore narrativo gia' interroga.
        //
        // --- quello che succede in pista
        Tag("partenza", "gara"); Tag("semaforo", "griglia"); Tag("bandiera", "gara");
        Tag("arrivo", "gara"); Tag("gruppo", "gara"); Tag("kerb", "gara");
        Tag("chicane", "gara"); Tag("parc", "gara"); Tag("ultimo-giro", "gara");
        Tag("attacco", "duello"); Tag("difesa", "duello"); Tag("testacoda", "incidente");
        Tag("beffardo", "rivale"); Tag("speedway", "gara"); Tag("circuito", "gara");
        // --- prove, dati, verifiche
        Tag("prove", "test"); Tag("verifica", "test"); Tag("traiettoria", "test");
        Tag("stopwatch", "dati"); Tag("pitwall", "dati"); Tag("briefing", "dati");
        Tag("temperature", "dati"); Tag("meteo", "dati"); Tag("bollettino", "dati");
        Tag("focus", "test"); Tag("allenamento", "allenamento");
        // --- officina e guasti
        Tag("motore", "officina"); Tag("gomme", "officina"); Tag("rifornimento", "officina");
        Tag("volante", "officina"); Tag("regolazione", "officina"); Tag("riparazione", "officina");
        Tag("catena", "officina"); Tag("asse", "officina"); Tag("smontato", "officina");
        Tag("carrello", "officina"); Tag("montaggio", "officina"); Tag("ricambi", "officina");
        Tag("consegna", "officina"); Tag("freni", "officina"); Tag("meccanico", "officina");
        Tag("scrutineering", "officina"); Tag("pesata", "officina"); Tag("transponder", "officina");
        Tag("guasto", "guasto"); Tag("rotto", "guasto"); Tag("smontat", "officina");
        // --- risultati
        Tag("premiazione", "vittoria"); Tag("medaglia", "vittoria"); Tag("festa", "vittoria");
        Tag("sconfitta", "sconfitta"); Tag("frustrat", "sconfitta");
        // --- soldi, sponsor, trattative. Haru e' il personaggio che gira a
        // cercare sponsorizzazioni: una tavola con il suo nome e' sempre una
        // trattativa, comunque vada a finire.
        Tag("haru", "trattativa"); Tag("gommista", "trattativa"); Tag("alimentari", "trattativa");
        Tag("assicurazione", "trattativa"); Tag("telefonate", "trattativa");
        Tag("scelta", "trattativa"); Tag("nuovo-team", "contratto");
        Tag("presentazione", "contratto"); Tag("livrea", "sponsor"); Tag("decal", "sponsor");
        Tag("promozione", "contratto"); Tag("acquisto", "budget"); Tag("usato", "gavetta");
        // --- persone e luoghi
        Tag("compagno", "compagno"); Tag("supporto", "vita-privata");
        Tag("famiglia", "vita-privata"); Tag("mentor", "ritratto-personaggio");
        Tag("nervoso", "ritratto-personaggio"); Tag("sollevato", "ritratto-personaggio");
        Tag("gradinata", "tifosi"); Tag("trasferi", "trasferta");
        Tag("montagn", "trasferta"); Tag("montano", "trasferta"); Tag("highland", "trasferta");
        Tag("costiero", "trasferta"); Tag("santuario", "trasferta"); Tag("notturn", "notte");
        Tag("fari", "notte");

        // I nomi in inglese delle tavole aggiunte per ultime.
        //
        // La tabella sopra riconosce quasi solo termini italiani: duecento
        // tavole nuove chiamate «...-race-...», «...-finish-line-...»,
        // «...-workshop-...» finivano quindi senza nessun momento e restavano
        // fuori dalla rotazione, viste come generico "paddock". Qui vengono
        // ricondotte agli stessi tag già usati dal motore narrativo.
        //
        // Il confronto è su parole intere, non su sottostringhe: cercando
        // "race" dentro il nome, «team-embrace-season-finale» diventava una
        // gara, e "train" trasformava «training-gym» in una trasferta.
        var parti = stem.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        void Parola(string token, string tag)
        {
            if (Array.IndexOf(parti, token) >= 0 && !tags.Contains(tag)) tags.Add(tag);
        }
        // «race» da sola non basta a fare una gara: compare in «pre-race»,
        // «race-team», «race-trip», che sono attesa, squadra e viaggio. Vale
        // solo quando la tavola non è dichiaratamente un prima o un altrove.
        var attorno = parti.Contains("pre") || parti.Contains("before") || parti.Contains("after")
                      || parti.Contains("team") || parti.Contains("trip") || parti.Contains("gym")
                      || parti.Contains("garage") || parti.Contains("workshop") || parti.Contains("office");
        if (!attorno) { Parola("race", "gara"); Parola("racing", "gara"); }
        Parola("weekend", "gara");
        Parola("sprint", "gara"); Parola("finish", "gara"); Parola("lap", "gara");
        Parola("qualifying", "qualifica"); Parola("session", "test");
        Parola("practice", "test"); Parola("preparation", "test");
        Parola("trophy", "vittoria"); Parola("celebration", "vittoria");
        Parola("winner", "vittoria"); Parola("points", "punti");
        Parola("duel", "duello"); Parola("overtake", "sorpasso");
        Parola("defends", "duello"); Parola("defense", "duello");
        Parola("teammate", "compagno"); Parola("crash", "incidente");
        Parola("retirement", "sconfitta"); Parola("failure", "guasto");
        Parola("repair", "officina"); Parola("repairs", "officina");
        Parola("workshop", "officina"); Parola("mechanic", "officina");
        Parola("suspension", "officina"); Parola("tire", "officina");
        Parola("fuel", "officina");
        Parola("setup", "dati"); Parola("engineer", "dati"); Parola("telemetry", "dati");
        Parola("notebook", "dati"); Parola("radio", "dati"); Parola("strategy", "dati");
        Parola("lane", "pitstop"); Parola("restart", "griglia");
        Parola("transporter", "trasferta"); Parola("transfer", "trasferta");
        Parola("train", "trasferta"); Parola("arrival", "trasferta");
        Parola("hotel", "trasferta"); Parola("handshake", "contratto");
        Parola("office", "trattativa"); Parola("negotiation", "trattativa");
        Parola("season", "campionato"); Parola("finale", "campionato");
        Parola("milestone", "campionato"); Parola("wet", "pioggia");
        Parola("spray", "pioggia"); Parola("storm", "pioggia"); Parola("umbrella", "pioggia");
        Parola("headlights", "notte"); Parola("floodlights", "notte");
        Parola("evening", "notte"); Parola("sunrise", "alba"); Parola("morning", "alba");
        Parola("sunset", "tramonto"); Parola("dusk", "tramonto");
        Parola("family", "vita-privata"); Parola("ramen", "vita-privata");
        Parola("encouragement", "vita-privata"); Parola("coach", "allenamento");
        Parola("training", "allenamento"); Parola("gym", "allenamento");
        Parola("prototype", "endurance"); Parola("junior", "formula-minore");
        Parola("category", "promozione"); Parola("professional", "promozione");
        // «pit» da solo e' la corsia dei box; dentro «pitwall» e' il muretto,
        // che e' un'altra cosa — per questo va cercato come parola intera.
        Parola("pit", "officina");
        // I circuiti giapponesi delle tavole numerate: il nome del tracciato
        // dice che si sta correndo li'.
        Parola("suzuka", "gara"); Parola("fuji", "gara"); Parola("nanbu", "gara");
        Parola("akagi", "gara"); Parola("hakuba", "gara");

        // Le tavole che non dipendono dalla categoria.
        //
        // Un'officina di notte, una cena di squadra, un ufficio sponsor o un
        // viaggio in treno raccontano lo stesso momento che si corra in kart o
        // in GT: non mostrano una vettura in azione, quindi non possono
        // contraddire la disciplina. Senza questo segno restavano invisibili a
        // ogni ricerca per disciplina — oltre centocinquanta tavole mai usate.
        //
        // È un elenco di luoghi e oggetti dichiaratamente fuori pista, non il
        // contrario: marcare come neutra una tavola perché "non sembra di
        // categoria" rimetterebbe una monoposto sopra una gara di kart, che è
        // esattamente l'errore che questa biblioteca esiste per evitare.
        foreach (var luogo in new[]
        {
            "garage", "workshop", "office", "hotel", "motel", "home", "apartment",
            "bedroom", "kitchen", "ramen", "breakfast", "meal", "gym", "station",
            // «transporter» e «van» sono stati tolti apposta: il camion del
            // team viaggia con la vettura, e quelle tavole mostrano quasi
            // sempre l'auto — quindi hanno una categoria, e non sono neutre.
            "platform", "train", "table", "shelf", "wall",
            "board", "notebook", "screen", "projector", "radio", "interview",
            "briefing", "review", "scrapbook", "newspaper", "clipping", "diary",
            "calendar", "itinerary", "handshake", "contract", "sponsor", "trophy",
            "helmet", "gloves", "glove", "boot", "jacket", "scarf", "badge",
            "case", "supplies", "awning", "tarpaulin", "shutters", "umbrella",
            "farewell", "family", "call", "photo", "photograph", "tower",
            "officina", "ufficio", "casa", "treno", "stazione", "palestra",
            "contratto", "sponsor-contratto", "calendario", "riposo"
        })
            Parola(luogo, "fuori-pista");

        // Le discipline in italiano e le sigle di categoria.
        Tag("turismo", "turismo"); Tag("formula1", "formula-vertice");
        Tag("formula2", "formula-alta"); Tag("formula3", "formula-alta");
        Tag("formula4", "formula-minore"); Tag("gt3", "gt"); Tag("gt4", "gt");
        Tag("pilota", "ritratto-personaggio");

        var discipline = tags.Contains("endurance") ? "endurance"
            : tags.Contains("gt") ? "GT"
            : tags.Contains("turismo") ? "turismo"
            : tags.Contains("formula-vertice") ? "formula di vertice"
            : tags.Contains("formula-alta") ? "formula superiore"
            : tags.Contains("formula-minore") ? "formula minore"
            : tags.Contains("kart") ? "kart"
            : tags.Contains("attivita-pilota") ? "vita del pilota"
            : tags.Contains("trattativa") || tags.Contains("sponsor") ? "sponsor"
            : "paddock";
        var vehicles = tags.Contains("endurance") ? "prototipo o GT endurance; vetture di classe sullo sfondo" :
            tags.Contains("gt") ? "GT da competizione" :
            tags.Contains("turismo") ? "vettura turismo da gara" :
            tags.Contains("formula-vertice") ? "monoposto di vertice a ruote scoperte" :
            tags.Contains("formula-alta") ? "monoposto di categoria superiore a ruote scoperte" :
            tags.Contains("formula-minore") ? "monoposto junior a ruote scoperte" :
            tags.Contains("kart") ? "kart da gara" :
            tags.Contains("ritratto-personaggio") ? "nessun mezzo obbligatorio; possibile paddock sullo sfondo" : "mezzi non identificati";
        var moment = tags.Contains("vittoria") || tags.Contains("podio") ? "un risultato conquistato" : tags.Contains("crisi") || tags.Contains("budget") ? "una fase difficile" : tags.Contains("test") ? "un test decisivo" : tags.Contains("rivale") ? "una rivalità" : tags.Contains("ritratto-personaggio") ? "un dialogo fra personaggi" : "un passaggio di carriera";
        var title = string.Join(" ", stem.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x is not "manga" and not "home" and not "generic" and not "character")
            .Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
        if (string.IsNullOrWhiteSpace(title)) title = "Tavola narrativa";
        return new IllustrationInfo(title, string.Join(", ", tags), vehicles, $"Tavola {discipline} per {moment}. I tag, i mezzi e il nome file permettono al racconto di proporla soltanto in contesti coerenti.");
    }
}
