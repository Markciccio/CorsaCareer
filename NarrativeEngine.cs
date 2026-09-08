using System.Globalization;

namespace CorsaCareer;

public sealed class NewsArticle
{
    public string Kicker { get; set; } = "";
    public string Title { get; set; } = "";
    public string Standfirst { get; set; } = "";
    public List<string> Paragraphs { get; set; } = [];
    public string Verdict { get; set; } = "";
    public int Rating { get; set; }
    public int Impact { get; set; }
    public int CoverageLevel { get; set; }
    public string DateLine { get; set; } = "";
    public string Byline { get; set; } = "";
    public List<string> Angles { get; set; } = [];
    /// <summary>Identificativi delle formulazioni usate: alimentano la memoria anti-ripetizione.</summary>
    public List<string> UsedPhrases { get; set; } = [];
    public List<string> Sidebar { get; set; } = [];

    public string Body => string.Join("\n\n", Paragraphs);
}

/// <summary>
/// Generatore editoriale.
///
/// Il generatore precedente aveva quattro paragrafi fissi, scritti a mano, con il
/// nome del pilota cablato: ogni gara produceva lo stesso articolo, e la
/// biografia raccontata (officina, padre meccanico, kart) non esisteva in
/// <see cref="CareerState"/>. Qui la struttura è composta:
///
/// 1. <see cref="StoryFacts"/> raccoglie solo dati verificati;
/// 2. l'impatto editoriale decide quanto spazio merita la notizia;
/// 3. le angolazioni rilevanti vengono selezionate e ordinate per questo evento;
/// 4. ogni blocco pesca da una banca di formulazioni evitando quelle usate di
///    recente, così due articoli consecutivi non si somigliano;
/// 5. la memoria della carriera (serie negative, gara precedente, stagioni
///    archiviate, giudizi passati) entra nel testo come richiamo verificabile.
/// </summary>
public static class NarrativeEngine
{
    /// <summary>Quante formulazioni recenti ricordare per non ripeterle.</summary>
    public const int PhraseMemory = 60;

    public static NewsArticle Compose(CareerState career, CareerEventRecord? story, IReadOnlyList<Round>? calendar = null)
    {
        var facts = StoryFacts.From(career, story, calendar);
        var memory = career.UsedPhrases ?? [];
        var seed = Seed(career, story, facts);
        var bank = new PhraseBank(seed, memory);
        var article = Compose(facts, bank);
        // La memoria viene aggiornata sul salvataggio: qui restituiamo solo le
        // formulazioni usate, così il generatore resta una funzione pura.
        article.UsedPhrases = bank.Used;
        return article;
    }

    public static NewsArticle Compose(StoryFacts facts, PhraseBank bank)
    {
        var article = new NewsArticle
        {
            Impact = EditorialImpact(facts),
            DateLine = $"{Capitalize(facts.Track)} · {facts.StoryDate.ToString("d MMMM yyyy", It)}",
            Byline = string.IsNullOrWhiteSpace(facts.Journalist) ? "" : $"di {facts.Journalist}{(string.IsNullOrWhiteSpace(facts.Publication) ? "" : $" · {facts.Publication}")}"
        };
        article.CoverageLevel = article.Impact switch
        {
            >= 88 => 5,
            >= 72 => 4,
            >= 55 => 3,
            >= 35 => 2,
            >= 18 => 1,
            _ => 0
        };
        article.Rating = Rating(facts);

        var angles = RankAngles(facts, bank);
        article.Angles = angles.Select(x => x.Name).ToList();

        article.Kicker = Kicker(facts, bank);
        article.Title = Title(facts, bank, angles);
        article.Standfirst = Standfirst(facts, bank, angles);

        // Il numero di blocchi dipende dall'importanza: una gara anonima non
        // produce un grande speciale, una prima vittoria sì.
        var blocks = Math.Clamp(article.CoverageLevel + 1, 2, 6);
        var selected = angles.Take(blocks).ToList();
        // Continuità garantita: ogni articolo deve agganciarsi al passato della
        // carriera, altrimenti la cronaca torna a essere una serie di bollettini
        // slegati. Se nessun blocco di memoria o serie è entrato nella selezione,
        // il più rilevante fra i due prende l'ultimo posto.
        if (!selected.Any(x => x.Name is "memoria" or "serie" or "traguardo"))
        {
            var continuity = angles.FirstOrDefault(x => x.Name is "memoria" or "serie" or "traguardo");
            if (continuity != null)
            {
                if (selected.Count >= blocks && selected.Count > 1) selected[^1] = continuity;
                else selected.Add(continuity);
            }
        }
        // Quando il cast ha una reazione nuova, l'articolo deve restituirla:
        // la memoria delle persone non può restare confinata alla scheda Paddock.
        if (!string.IsNullOrWhiteSpace(facts.ManagerMemory) && !selected.Any(x => x.Name == "persone"))
        {
            var people = angles.FirstOrDefault(x => x.Name == "persone");
            if (people != null)
            {
                if (selected.Count >= blocks && selected.Count > 1) selected[^1] = people;
                else selected.Add(people);
            }
        }
        foreach (var angle in selected)
        {
            var paragraph = angle.Write(facts, bank);
            if (!string.IsNullOrWhiteSpace(paragraph)) article.Paragraphs.Add(paragraph);
        }
        article.Verdict = Verdict(facts, bank);
        article.Sidebar = Sidebar(facts);
        return article;
    }

    private static readonly CultureInfo It = CultureInfo.GetCultureInfo("it-IT");

    // ------------------------------------------------------------ impatto

    /// <summary>
    /// Quanto conta questa notizia. Non è solo la posizione: pesa lo scarto
    /// dall'aspettativa del sedile, la serie precedente, la posta in gioco nel
    /// campionato e le conseguenze economiche e di mercato.
    /// </summary>
    public static int EditorialImpact(StoryFacts f)
    {
        if (!f.HasRace)
        {
            return f.EventType switch
            {
                "EVALUATION_PASSED" => 82,
                "INVITATION_BREAKTHROUGH" => 86,
                "INVITATION_SETBACK" => 70,
                "PROMOTION" => 90,
                "CONTRACT_EXPIRED" or "MARKET_INTEREST" => 62,
                "SPONSOR_OBJECTIVE" => 58,
                "SPONSOR_WARNING" or "TEAM_WARNING" => 66,
                "TRACK_TEST" => 34,
                "ACTIVITY_SETBACK" => 40,
                "ACTIVITY_DONE" => 24,
                "EVALUATION_REVIEW" => 44,
                "NEW_SEASON" => 50,
                _ => 26
            };
        }

        var impact = 20;
        if (f.Abandoned) return 58;
        if (f.Won) impact += f.IsFirstWin ? 70 : 45;
        else if (f.OnPodium) impact += f.IsFirstPodium ? 55 : 28;
        else if (f.Dnf) impact += 26;
        else if (f.Scored) impact += 12;

        // Battere o mancare l'aspettativa del sedile è la notizia vera.
        if (f.ExpectedPosition > 0 && !f.Dnf && f.Position > 0)
        {
            var margin = f.ExpectedPosition - f.Position;
            impact += Math.Clamp(margin * 3, -18, 22);
        }
        // Una serie negativa interrotta vale più di un buon risultato isolato.
        if (f.Scored && f.PointlessStreak >= 3) impact += 16;
        if (!f.Scored && f.PodiumStreak == 0 && f.PointlessStreak >= 3) impact += 8;
        if (f.PodiumStreak >= 3) impact += 10;
        // Posta in gioco.
        if (f.ChampionshipPosition == 1) impact += 12;
        else if (f.ChampionshipPosition is > 1 and <= 3) impact += 6;
        if (f.RoundsLeft <= 2 && f.RoundCount >= 4) impact += 10;
        // Confronto interno.
        if (f.TeammatePosition > 0 && !f.Dnf && Math.Abs(f.Position - f.TeammatePosition) <= 1) impact += 8;
        // Contesto extra-pista.
        if (f.MilestoneRaces > 0) impact += 20;
        if (f.SponsorRelation <= 20 || f.TeamRelation <= 20) impact += 8;
        if (f.Damage > 0.3) impact += 6;
        return Math.Clamp(impact, 0, 100);
    }

    /// <summary>Pagella del weekend, 1-10, dal risultato reale rispetto alle attese.</summary>
    public static int Rating(StoryFacts f)
    {
        if (!f.HasRace) return 0;
        // Un weekend non concluso non è una prestazione da valutare: non riceve pagella.
        if (f.Abandoned) return 0;
        if (f.Dnf) return f.Damage > 0.3 ? 3 : 4;
        if (f.Position <= 0 || f.FieldSize <= 1) return 5;
        var ratio = (f.Position - 1) / (double)Math.Max(1, f.FieldSize - 1);
        var baseRating = 9.0 - ratio * 6.0;
        if (f.ExpectedPosition > 0) baseRating += Math.Clamp((f.ExpectedPosition - f.Position) * 0.2, -1.5, 1.2);
        if (f.TeammatePosition > 0) baseRating += f.Position < f.TeammatePosition ? 0.3 : -0.4;
        if (f.QualifyingPosition == 1) baseRating += 0.2;
        // Il dieci è riservato alla vittoria: un podio, per quanto perfetto,
        // non è un weekend impossibile da migliorare.
        var ceiling = f.Won ? 10.0 : f.OnPodium ? 9.0 : 8.0;
        return (int)Math.Clamp(Math.Round(Math.Min(baseRating, ceiling)), 1, 10);
    }

    // ------------------------------------------------------------ angolazioni

    private static List<StoryAngle> RankAngles(StoryFacts f, PhraseBank bank)
    {
        var angles = StoryAngles.All
            .Select(angle => new { angle, score = angle.Relevance(f) })
            .Where(x => x.score > 0)
            .ToList();
        // A parità di rilevanza l'ordine ruota con il seme: due articoli con la
        // stessa situazione non aprono con lo stesso blocco.
        var rotated = angles
            .OrderByDescending(x => x.score)
            .ThenBy(x => bank.Shuffle(x.angle.Name))
            .Select(x => x.angle)
            .ToList();
        if (rotated.Count == 0) rotated.Add(StoryAngles.Fallback);
        return rotated;
    }

    // ------------------------------------------------------------ testata

    private static string Kicker(StoryFacts f, PhraseBank bank)
    {
        if (!f.HasRace)
            return bank.Pick("kicker-nonrace",
                f.Championship.ToUpperInvariant(),
                $"{f.Tier.ToUpperInvariant()} · PADDOCK",
                "DAL PADDOCK",
                $"STAGIONE {f.Season}",
                "TACCUINO DEL PADDOCK");
        var where = string.IsNullOrWhiteSpace(f.Track) ? f.Championship : f.Track.ToUpperInvariant();
        return bank.Pick("kicker-race",
            $"{where} · ROUND {f.Round + 1}",
            $"{f.Championship.ToUpperInvariant()} · STAGIONE {f.Season}",
            $"IL REFERTO DI {where}",
            $"ROUND {f.Round + 1} DI {Math.Max(f.RoundCount, f.Round + 1)}",
            $"{where} · PAGELLE E CLASSIFICA");
    }

    private static string Title(StoryFacts f, PhraseBank bank, List<StoryAngle> angles)
    {
        var name = string.IsNullOrWhiteSpace(f.Surname) ? f.Driver : f.Surname;
        var place = Capitalize(f.Track);

        if (!f.HasRace) return NonRaceTitle(f, bank, name);

        // Un weekend non concluso non ha un piazzamento da titolare.
        if (f.Abandoned)
            return bank.Pick("title-withdrawal",
                $"{name} si ritira dal weekend di {place}: nessun referto, round comunque speso",
                $"{place} senza risultato: il weekend di {name} finisce prima del referto",
                $"Weekend abbandonato a {place}: per {name} è un round che non torna",
                $"{name} fuori a {place} senza classifica: il conto lo pagano reputazione e budget");

        if (f.Won)
            return f.IsFirstWin
                ? bank.Pick("title-firstwin",
                    $"La prima volta di {name}: {place} consegna il successo che cambia la stagione",
                    $"{name} vince, e non è più una promessa: il primo sigillo arriva a {place}",
                    $"{place}, il giorno in cui {name} ha smesso di aspettare",
                    $"Prima vittoria per {name}: il paddock deve riscrivere le gerarchie",
                    $"{name} rompe il ghiaccio a {place}: il progetto {f.TeamName} ha il suo momento")
                : bank.Pick("title-win",
                    $"{name} domina a {place}: {f.Wins} successi e una classifica che si muove",
                    $"Un'altra firma di {name}: {place} conferma il passo del progetto {f.TeamName}",
                    $"{place} parla chiaro: {name} vince e prende in mano il campionato",
                    $"{name} non sbaglia: vittoria a {place} e messaggio al gruppo",
                    $"Vittoria pesante a {place}: {name} tiene il ritmo che serve al titolo");

        if (f.OnPodium)
            return f.IsFirstPodium
                ? bank.Pick("title-firstpodium",
                    $"Primo podio per {name}: a {place} il paddock inizia a prenderlo sul serio",
                    $"{name} sul podio a {place}: il primo vero segnale della carriera",
                    $"{place} regala a {name} il primo podio: ora le attese cambiano",
                    $"Il salto di qualità di {name}: primo podio nel {f.Championship}")
                : bank.Pick("title-podium",
                    $"{name} ancora sul podio: {place} conferma la crescita",
                    $"P{f.Position} a {place}: {name} costruisce la stagione mattone su mattone",
                    $"{name} tiene il podio a {place} e resta nella lotta",
                    $"{place}: {name} chiude {f.PositionLabel} e il campionato si stringe");

        if (f.Dnf)
            return bank.Pick("title-dnf",
                $"{place} si spegne troppo presto: il ritiro di {name} pesa in classifica",
                $"Un ritiro che costa: {name} fuori a {place}, il box cerca risposte",
                $"{name}, domenica interrotta a {place}: cosa dicono i dati",
                $"{place}, gara finita nel box: per {name} è un weekend da archiviare",
                $"Il {f.Championship} presenta il conto: {name} si ritira a {place}");

        if (f.Scored)
            return bank.Pick("title-points",
                $"{name} porta a casa {f.Points} punti a {place}: solidità prima di tutto",
                $"{place}: {name} chiude {f.PositionLabel} e muove la classifica",
                $"Punti utili per {name} a {place}: la stagione prende forma",
                $"{name} {f.PositionLabel} a {place}: risultato che il box firma volentieri",
                $"{place} premia la costanza: {name} torna nella zona punti");

        return bank.Pick("title-nopoints",
            $"{place} resta muta per {name}: {f.PositionLabel} e nessun punto",
            $"{name} {f.PositionLabel} a {place}: una domenica da capire",
            $"Fuori dai punti a {place}: per {name} conta solo il chilometraggio",
            $"{place}: {name} chiude {f.PositionLabel}, il progetto {f.TeamName} deve accelerare",
            $"Weekend opaco a {place}: {name} paga il passo");
    }

    private static string NonRaceTitle(StoryFacts f, PhraseBank bank, string name) => f.EventType switch
    {
        "EVALUATION_PASSED" => bank.Pick("title-evalpass",
            $"{name} convince il paddock: la valutazione è superata",
            $"Test superato: {name} ha guadagnato il diritto a un sedile",
            $"Il cronometro dà ragione a {name}: la porta del paddock si apre"),
        "EVALUATION_REVIEW" => bank.Pick("title-evalreview",
            $"{name} non basta ancora: il paddock chiede un'altra prova",
            $"Valutazione aperta: a {name} serve un altro giro secco",
            $"Il verdetto resta sospeso: {name} torna in pista per convincere"),
        "INVITATION_BREAKTHROUGH" => bank.Pick("title-invitationgood",
            $"{name} sfrutta l'invito: il risultato riapre il mercato",
            $"Una gara, un segnale: {name} convince il paddock quando conta",
            $"L'invito diventa occasione: {name} rimette il proprio nome sul tavolo"),
        "INVITATION_SETBACK" => bank.Pick("title-invitationbad",
            $"Invito non sfruttato: per {name} il paddock chiede una risposta",
            $"{name} non trasforma l'occasione: ora serve tornare al lavoro",
            $"Il mercato frena dopo l'invito: {name} deve ricostruire credibilità"),
        "INVITATION_DECLINED" => bank.Pick("title-invitationdeclined",
            $"{name} rinuncia all'invito: prima vengono preparazione e budget",
            $"Nessun azzardo: {name} rimanda la gara e torna al lavoro",
            $"L'invito resta sul tavolo, ma {name} sceglie il percorso lungo"),
        "TRACK_TEST" => bank.Pick("title-test",
            $"{name} in pista per lavoro: il test di {Capitalize(f.Track)} non assegna punti",
            $"Giornata tecnica a {Capitalize(f.Track)}: cosa cerca {f.TeamName}",
            $"Test a {Capitalize(f.Track)}: {name} raccoglie dati, non risultati"),
        "PROMOTION" => bank.Pick("title-promotion",
            $"{name} sale di categoria: il {f.Championship} lo aspetta",
            $"Promozione firmata: per {name} comincia un campionato diverso",
            $"Il salto è fatto: {name} entra nel {f.Championship}"),
        "CONTRACT_EXPIRED" => bank.Pick("title-contract",
            $"{name} sul mercato: il contratto con {f.TeamName} è finito",
            $"Fine del rapporto con {f.TeamName}: {name} cerca una nuova casa",
            $"Il mercato si apre per {name}: ora conta chi risponde"),
        "SPONSOR_WARNING" => bank.Pick("title-sponsorwarn",
            $"{f.SponsorName} mette il contratto in revisione: {name} avvisato",
            $"Tensione commerciale: {f.SponsorName} chiede garanzie a {name}"),
        "TEAM_WARNING" => bank.Pick("title-teamwarn",
            $"Rapporto in crisi fra {name} e {f.TeamName}",
            $"{f.TeamName} perde fiducia: la posizione di {name} si complica"),
        "ACTIVITY_SETBACK" => bank.Pick("title-activitybad",
            $"{name} fuori dalla pista: {f.LastActivity} non è andata come previsto",
            $"Passo falso lontano dal circuito per {name}"),
        "ACTIVITY_DONE" => bank.Pick("title-activitygood",
            $"{name} lavora anche fuori dal circuito: {f.LastActivity}",
            $"Fra due weekend: {name} sceglie {f.LastActivity}"),
        // La variante che nomina la squadra vale solo se una squadra c'e:
        // altrimenti il titolo diventava "Senza contratto e Tizio: dove sta
        // andando questo progetto", con un'etichetta di stato al posto di un
        // interlocutore.
        _ => f.HasTeam
            ? bank.Pick("title-generic",
                $"{name}, il punto sulla stagione {f.Season}",
                $"{f.TeamName} e {name}: dove sta andando questo progetto",
                $"Taccuino dal paddock: la situazione di {name}")
            : bank.Pick("title-generic-noteam",
                $"{name}, il punto sulla stagione {f.Season}",
                $"Nessun box alle spalle: dove sta andando la stagione di {name}",
                $"Taccuino dal paddock: la situazione di {name}")
    };

    private static string Standfirst(StoryFacts f, PhraseBank bank, List<StoryAngle> angles)
    {
        if (!f.HasRace)
            return bank.Pick("stand-nonrace",
                $"{f.Championship}, stagione {f.Season}: reputazione {f.Reputation}/100, budget € {f.Cash:N0} e un percorso che deve ancora produrre il prossimo referto.",
                $"Nessun risultato nuovo da registrare: quello che segue nasce dai dati già archiviati della carriera di {f.Driver}.",
                $"Il dossier di {f.Driver} resta aperto: {f.Races} gare disputate, {f.Wins} vittorie, {f.Podiums} podi.");

        if (f.Abandoned)
            return bank.Pick("stand-withdrawal",
                $"Sessione preparata e non conclusa a {Capitalize(f.Track)}: nessuna posizione d'arrivo, nessun punto, round consumato.",
                $"Il round {f.Round + 1} resta a referto come ritiro: non esiste un risultato da attribuire, esiste il fatto che il weekend è stato disputato.",
                $"Zero punti e nessuna classifica da {Capitalize(f.Track)}: il weekend non è arrivato a un referto utilizzabile.");
        var gap = f.GapMilliseconds > 0 ? $", {f.GapMilliseconds / 1000.0:0.###} s dal vincitore" : "";
        var qualifying = f.QualifyingPosition > 0 ? $"qualifica P{f.QualifyingPosition}" : "qualifica non disponibile";
        return bank.Pick("stand-race",
            $"{f.PositionLabel} su {f.FieldSize} al traguardo, {qualifying}, {f.Laps} giri{gap}: il referto di {Capitalize(f.Track)} entra nell'archivio della stagione {f.Season}.",
            // Senza una posizione di classifica reale la frase diceva "una
            // classifica che dice P0 con 0 punti": un dato inventato dal nulla.
            // Se la classifica non c'e, non se ne parla.
            f.ChampionshipPosition > 0
                ? $"Partito P{f.StartingPosition}, arrivato {f.PositionLabel}: {f.Points} punti e una classifica che dice P{f.ChampionshipPosition} con {f.ChampionshipPoints} punti."
                : $"Partito P{f.StartingPosition}, arrivato {f.PositionLabel}: {f.Points} punti in un campionato che non ha ancora una classifica consolidata.",
            $"{f.FormatLabel}{(string.IsNullOrWhiteSpace(f.WeatherLabel) ? "" : $", {f.WeatherLabel}, {f.TemperatureC:0.#} °C")}: ecco cosa dicono i numeri di {Capitalize(f.Track)}.",
            $"Il verdetto di {Capitalize(f.Track)}: {f.PositionLabel}, {f.Points} punti, pagella {Rating(f)}/10 secondo i dati del weekend.");
    }

    private static string Verdict(StoryFacts f, PhraseBank bank)
    {
        if (!f.HasRace) return "";
        // Un weekend non disputato non si giudica come una prestazione: non ha
        // pagella, e nemmeno un commento sul rendimento.
        if (f.Abandoned) return "";
        var rating = Rating(f);
        var body = rating switch
        {
            >= 9 => bank.Pick("verdict-9",
                "Weekend da manuale: poco da correggere, molto da confermare.",
                "Prestazione completa: qualifica, gara e gestione allo stesso livello.",
                "Il tipo di domenica che sposta un giudizio, non solo una classifica."),
            >= 7 => bank.Pick("verdict-7",
                "Solido: il risultato sta dentro il potenziale del pacchetto.",
                "Prestazione affidabile, senza sbavature e con qualcosa da limare.",
                "Buon lavoro: la crescita si vede nei dettagli più che nel piazzamento."),
            >= 5 => bank.Pick("verdict-5",
                "Weekend nella media: né errori gravi né spunti memorabili.",
                "Passo accettabile ma niente di più: serve un salto per cambiare stagione.",
                "Prestazione sufficiente, con margini evidenti in gestione e ritmo."),
            >= 4 => bank.Pick("verdict-4",
                "Domenica insufficiente: il ritmo non c'è stato e il referto lo dice.",
                "Poco da salvare, molto da rivedere nel debrief.",
                "Il weekend lascia più domande che risposte."),
            _ => bank.Pick("verdict-low",
                "Weekend da dimenticare: costi alti e nessun ritorno sportivo.",
                "Bilancio negativo su tutta la linea, dal passo all'affidabilità.",
                "Sotto ogni aspettativa: serve una reazione immediata.")
        };
        return $"Pagella {rating}/10 — {body}";
    }

    private static List<string> Sidebar(StoryFacts f)
    {
        var items = new List<string>();
        if (f.HasRace && f.Abandoned)
        {
            items.Add("Weekend non concluso: nessuna posizione d'arrivo registrata");
            items.Add("Punti 0 · nessun premio gara");
            if (!string.IsNullOrWhiteSpace(f.FormatLabel)) items.Add($"Formato previsto {f.FormatLabel}");
        }
        else if (f.HasRace)
        {
            items.Add($"Arrivo {f.PositionLabel} su {f.FieldSize}");
            if (f.QualifyingPosition > 0) items.Add($"Qualifica P{f.QualifyingPosition}");
            items.Add($"Partenza P{f.StartingPosition}");
            items.Add($"Giri {f.Laps}{(f.PlannedLaps > 0 ? $"/{f.PlannedLaps}" : "")}");
            if (f.BestLapMilliseconds > 0) items.Add($"Miglior giro {RookieTargetEngine.Format(f.BestLapMilliseconds)}");
            if (f.GapMilliseconds > 0) items.Add($"Distacco {f.GapMilliseconds / 1000.0:0.###} s");
            if (f.PitStops > 0) items.Add($"Pit stop {f.PitStops}");
            if (f.PenaltySeconds > 0) items.Add($"Penalità {f.PenaltySeconds:0.#} s");
            if (f.Damage > 0) items.Add($"Danno {f.Damage:0.##}");
            items.Add($"Punti {f.Points}");
            if (!string.IsNullOrWhiteSpace(f.WeatherLabel)) items.Add($"Condizioni {f.WeatherLabel}, {f.TemperatureC:0.#} °C");
            if (f.TeammatePosition > 0) items.Add($"Compagno {f.TeammateName} P{f.TeammatePosition}");
        }
        if (f.ChampionshipPosition > 0) items.Add($"Classifica P{f.ChampionshipPosition} · {f.ChampionshipPoints} punti");
        items.Add($"Carriera {f.Races} gare · {f.Wins} vittorie · {f.Podiums} podi");
        items.Add($"Reputazione {f.Reputation}/100");
        return items;
    }

    // ------------------------------------------------------------ utilità

    private static long Seed(CareerState career, CareerEventRecord? story, StoryFacts facts)
    {
        // Il seme include la posizione dell'evento nella cronologia: due eventi
        // diversi non possono produrre lo stesso articolo, e lo stesso evento
        // resta stabile fra due aperture della stessa pagina.
        var index = story == null ? career.Events.Count : career.Events.IndexOf(story);
        var text = $"{career.Driver}|{facts.Season}|{facts.Round}|{facts.EventType}|{facts.Track}|{index}|{facts.Position}|{facts.Points}";
        var hash = 2166136261L;
        foreach (var character in text) hash = (hash ^ character) * 16777619 % 2147483647;
        return Math.Abs(hash);
    }

    public static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "il circuito";
        var cleaned = value.Replace("_", " ").Trim();
        return char.ToUpper(cleaned[0], It) + cleaned[1..];
    }
}
