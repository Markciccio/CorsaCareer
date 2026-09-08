using System.Text;

namespace CorsaCareer1991;

public sealed record BrowserPortalContent(string Title, string Standfirst, string ArticleHtml, string AudioText, string[] Headlines);

/// <summary>
/// Adattatore fra il motore narrativo e i consumatori esistenti (portale
/// browser, home, servizi audio).
///
/// La versione precedente conteneva quattro paragrafi scritti a mano, con il
/// nome del pilota cablato nel testo e una biografia inventata (officina, padre
/// meccanico, kart) che non esisteva nei dati della carriera. Ora il testo arriva
/// da <see cref="NarrativeEngine"/>, che compone dai soli fatti verificati e
/// varia struttura e formulazioni a ogni articolo.
/// </summary>
public static class CareerArticleBuilder
{
    public static BrowserPortalContent Build(CareerState career, CareerEventRecord? story, IReadOnlyList<Round>? calendar = null)
    {
        var article = story?.Type is "EVALUATION_PASSED" or "EVALUATION_REVIEW"
            ? BuildEvaluationFeature(career, story)
            : NarrativeEngine.Compose(career, story, calendar);
        PhraseBank.Remember(career, article.UsedPhrases);
        AttachClassification(career, story, article);
        var text = ComposePlainText(article);
        return new BrowserPortalContent(article.Title, article.Standfirst, text, text, Headlines(career, article));
    }

    /// <summary>
    /// Aggiunge la classifica completa quando l'articolo racconta una gara.
    ///
    /// Il testo generato parlava solo del pilota: nessun nome degli altri in
    /// griglia, nessun ordine d'arrivo. Il referto lo sa già — ogni gara lo
    /// salva in <see cref="RaceHistoryEntry.Classification"/> — mancava solo
    /// portarlo nell'articolo.
    /// </summary>
    private static void AttachClassification(CareerState career, CareerEventRecord? story, NewsArticle article)
    {
        if (story == null) return;
        var race = career.RaceHistory
            .Where(x => x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase) && x.Classification.Count > 1)
            .OrderByDescending(x => x.DateUtc)
            .FirstOrDefault();
        if (race == null) return;
        var partenti = race.Classification.Count;
        var vincitore = race.Classification.OrderBy(x => x.Position).FirstOrDefault();
        var sintesi = new List<string>();
        if (vincitore != null && !vincitore.IsPlayer)
            sintesi.Add($"Vince {vincitore.Name} davanti a {partenti - 1} avversari.");
        else if (vincitore != null)
            sintesi.Add($"Vittoria di {career.Driver} su {partenti} al via.");
        sintesi.Add(race.Dnf
            ? $"{career.Driver} si ritira dopo {race.Laps} giri."
            : $"{career.Driver} chiude P{race.Position} su {partenti}"
              + (race.StartingPosition > 0 ? $", partito P{race.StartingPosition}" : "")
              + (race.GapMilliseconds > 0 ? $", a {race.GapMilliseconds / 1000d:0.000} s dal vincitore" : "")
              + ".");
        if (race.BestLapMilliseconds > 0) sintesi.Add($"Miglior giro personale {RookieTargetEngine.Format(race.BestLapMilliseconds)}.");
        if (race.PenaltySeconds > 0) sintesi.Add($"Penalità: {race.PenaltySeconds:0} s.");
        if (race.Points > 0) sintesi.Add($"Punti conquistati: {race.Points}.");
        if (race.Prize > 0) sintesi.Add($"Premio: € {race.Prize:N0}.");
        article.Paragraphs.Add("COME È ANDATA\n" + string.Join(" ", sintesi));

        var righe = race.Classification
            .OrderBy(x => x.Position)
            .Select(x => $"P{x.Position} {x.Name}{(string.IsNullOrWhiteSpace(x.Car) ? "" : $" ({UiText.Car(x.Car)})")}" + (x.IsPlayer ? " — tu" : ""));
        article.Paragraphs.Add("CLASSIFICA DELLA GARA\n" + string.Join("\n", righe));
    }

    private static NewsArticle BuildEvaluationFeature(CareerState career, CareerEventRecord story)
    {
        var test = career.TestHistory.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        var target = test?.TargetLapMilliseconds > 0 ? test.TargetLapMilliseconds : career.EvaluationTargetMilliseconds;
        var lap = test?.BestLapMilliseconds > 0 ? RookieTargetEngine.Format(test.BestLapMilliseconds) : "nessun giro valido";
        var gap = test?.BestLapMilliseconds > 0 && target > 0 ? test.BestLapMilliseconds - target : 0;
        var gapText = test?.BestLapMilliseconds > 0 && target > 0 ? $"{(gap >= 0 ? "+" : "")}{gap / 1000.0:0.000} secondi" : "nessun confronto possibile";
        var passed = story.Type == "EVALUATION_PASSED";
        var cast = career.StoryCast ?? [];
        var manager = cast.FirstOrDefault(x => x.Id == StoryCastService.Manager)?.Name ?? "Marta Rinaldi";
        var mechanic = cast.FirstOrDefault(x => x.Id == StoryCastService.Mechanic)?.Name ?? "Gianni Valli";
        var rival = cast.FirstOrDefault(x => x.Id == StoryCastService.Rival)?.Name ?? "Nico Valenti";
        var interest = string.Join(", ", (career.TeamInterests ?? []).OrderByDescending(x => x.Value).Take(2).Select(x => $"{x.Team} ({x.Value}%: {x.Status.ToLowerInvariant()})"));
        var article = new NewsArticle
        {
            Kicker = "ROOKIE · TEST DI VALUTAZIONE",
            Title = passed ? $"{career.Driver}, un giro che apre il paddock" : $"{career.Driver} resta fuori soglia, ma non fuori dal gioco",
            Standfirst = $"A {test?.Track ?? story.Track}, con {UiText.Car(test?.Car ?? career.Car)}, il rookie ha fermato il cronometro a {lap}. Il riferimento era {RookieTargetEngine.Format(target)}: scarto {gapText}.",
            Byline = $"di {career.Journalist?.Name ?? "Noa Minazuki"} · {career.Journalist?.Publication ?? "Grand Prix CorsaCareer"}",
            DateLine = $"{test?.Track ?? story.Track} · {story.StoryDate:d MMMM yyyy}",
            CoverageLevel = 5, Impact = passed ? 82 : 62, Rating = Math.Clamp(career.RookieEvaluationScore / 10, 1, 10)
        };
        article.Paragraphs.Add($"Non c'era una griglia da battere né un premio da incassare. C'era una sola vettura, {UiText.Car(test?.Car ?? career.Car)}, e un dossier ancora vuoto: {career.Races} gare disputate, {career.Wins} vittorie, € {career.Cash:N0} in cassa. Per {career.Driver}, senza contratto e senza uno sponsor alle spalle, questo era il tipo di giornata che decide se un nome resta nei corridoi del paddock o arriva sulla scrivania di qualcuno.");
        article.Paragraphs.Add($"Il dato centrale è il crono: {lap} contro {RookieTargetEngine.Format(target)}. Lo scarto di {gapText} non è stato letto da solo. Il programma ha registrato {test?.Laps ?? 0} giri; la fiducia dei team è ora {career.TeamRelation}/100 e il bilancio aggiornato è di € {career.Cash:N0}. {manager}, responsabile del programma rookie, ha seguito il test insieme a {mechanic}, il meccanico che ha preparato la macchina; sullo sfondo c'era anche {rival}, il primo riferimento diretto del pilota.");
        article.Paragraphs.Add(passed
            ? "Il verdetto non è un contratto automatico. È più interessante: il diritto di essere discusso. Il test ha superato la soglia tecnica e ha spostato la conversazione dal cronometro al mercato, senza cancellare il fatto che una stagione intera richiede budget, costanza e risultati ripetibili."
            : "Il verdetto non cancella il progetto. Il tempo non ha raggiunto la soglia, ma il referto conserva un riferimento concreto su cui lavorare. Il prossimo appuntamento non sarà una consolazione: sarà un test di conferma o di recupero, con meno margine per gli errori e con il budget che continua a pesare su ogni scelta.");
        article.Paragraphs.Add($"Sul mercato, i segnali iniziali arrivano da {interest}. Per ora è interesse, non un'offerta: ciascuna squadra vuole capire se il giro di oggi è ripetibile, se il pilota sa restituire indicazioni tecniche e se potrà portare risorse senza trasformarsi in un semplice sedile pagante.");
        article.Paragraphs.Add($"Il passato professionale di {career.Driver} è ancora breve per definizione: nessuna gara ufficiale, nessun podio da difendere. Proprio per questo il prossimo passo conta più del titolo. Un secondo test convincente può aprire un invito o una trattativa; un'altra prova opaca costringerà il rookie a cercare chilometri, sostegno economico e pazienza nelle formule minori.");
        article.Verdict = passed ? "Prospettiva: il paddock ha aperto una porta. Adesso il risultato deve diventare continuità." : "Prospettiva: il cronometro ha detto no per ora; la carriera risponde con un nuovo appuntamento, non con una scorciatoia.";
        article.Sidebar = [$"Tempo: {lap}", $"Riferimento: {RookieTargetEngine.Format(target)}", $"Scarto: {gapText}", $"Cassa: € {career.Cash:N0}", $"Fiducia paddock: {career.TeamRelation}/100"];
        return article;
    }

    /// <summary>Versione che non aggiorna la memoria: utile per le anteprime.</summary>
    public static NewsArticle Preview(CareerState career, CareerEventRecord? story, IReadOnlyList<Round>? calendar = null) =>
        NarrativeEngine.Compose(career, story, calendar);

    public static string ComposePlainText(NewsArticle article)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(article.Kicker)) builder.AppendLine(article.Kicker);
        builder.AppendLine(article.Title);
        if (!string.IsNullOrWhiteSpace(article.Standfirst)) builder.AppendLine(article.Standfirst);
        if (!string.IsNullOrWhiteSpace(article.Byline)) builder.AppendLine(article.Byline);
        if (!string.IsNullOrWhiteSpace(article.DateLine)) builder.AppendLine(article.DateLine);
        builder.AppendLine();
        foreach (var paragraph in article.Paragraphs)
        {
            builder.AppendLine(paragraph);
            builder.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(article.Verdict)) builder.AppendLine(article.Verdict);
        if (article.Sidebar.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("SCHEDA DEL WEEKEND");
            foreach (var item in article.Sidebar) builder.AppendLine("· " + item);
        }
        return builder.ToString().TrimEnd();
    }

    private static string[] Headlines(CareerState career, NewsArticle article)
    {
        // Titoli di richiamo della colonna laterale: derivano dallo stato reale
        // della carriera, non da un elenco fisso.
        var items = new List<string>();
        if (career.Standings.Count > 0)
        {
            var leader = career.Standings.OrderByDescending(x => x.Points).First();
            items.Add($"Classifica: {leader.Driver} guida con {leader.Points} punti");
        }
        if (career.Offers.Count > 0)
            items.Add($"Mercato: {career.Offers.Count} offerta/e sul tavolo di {career.Driver}");
        // "In attesa di sponsor" e uno stato, non un'azienda: scritto cosi
        // l'articolo diceva "In attesa di sponsor: obiettivo stagionale...",
        // come se il segnaposto fosse il nome di chi paga.
        if (career.SponsorObjectiveStatus != "In corso" && OpportunityContext.HasSponsor(career.Sponsor))
            items.Add($"{career.Sponsor}: obiettivo stagionale {career.SponsorObjectiveStatus.ToLowerInvariant()}");
        var rival = career.Rivalries.OrderByDescending(x => x.Level).FirstOrDefault();
        if (rival != null && rival.Level >= 30)
            items.Add($"Rivalità con {rival.Rival}: intensità {rival.Level}/100");
        if (career.TestHistory.Count > 0)
        {
            var test = career.TestHistory[^1];
            items.Add($"Test: {test.Track}, miglior giro {(test.BestLapMilliseconds > 0 ? RookieTargetEngine.Format(test.BestLapMilliseconds) : "non disponibile")}");
        }
        if (career.ActivityHistory.Count > 0)
        {
            var activity = career.ActivityHistory[^1];
            items.Add($"Fuori dalla pista: {activity.Name}, {(activity.Success ? "riuscita" : "non riuscita")}");
        }
        if (article.Rating > 0) items.Add($"Pagella del weekend: {article.Rating}/10");
        if (items.Count == 0) items.Add($"{career.Team}: il progetto attende il primo referto reale");
        return items.Take(6).ToArray();
    }
}
