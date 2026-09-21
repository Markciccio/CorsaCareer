using System.Text;

namespace CorsaCareer;

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
        var article = IsRaceStory(story)
            ? BuildRaceFeature(career, story!)
            : story?.Type is "EVALUATION_PASSED" or "EVALUATION_REVIEW"
            ? BuildEvaluationFeature(career, story)
            : NarrativeEngine.Compose(career, story, calendar);
        PhraseBank.Remember(career, article.UsedPhrases);
        AttachClassification(career, story, article);
        var text = ComposePlainText(article);
        return new BrowserPortalContent(article.Title, article.Standfirst, ComposePortalArticle(article), text, Headlines(career, article));
    }

    private static bool IsRaceStory(CareerEventRecord? story) => story?.Type is
        "FIRST_VICTORY" or "VICTORY" or "FIRST_PODIUM" or "PODIUM" or
        "RETIREMENT" or "RACE_FINISHED" or "INVITATION_BREAKTHROUGH" or
        "INVITATION_DEBUT" or "INVITATION_SETBACK" or "WILDCARD";

    /// <summary>Articolo lineare del referto: cronaca, box e prospettiva.</summary>
    private static NewsArticle BuildRaceFeature(CareerState career, CareerEventRecord story)
    {
        var race = career.RaceHistory
            .Where(x => x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.DateUtc)
            .FirstOrDefault();
        if (race == null) return NarrativeEngine.Compose(career, story);
        var driver = DisplayName(career.Driver);
        var track = UiText.Track(race.Track);
        var car = UiText.Car(race.Car);
        var firstWin = story.Type == "FIRST_VICTORY";
        var positionText = race.Dnf ? "si è ritirato" : $"ha chiuso P{race.Position} su {Math.Max(1, race.Classification.Count)} partenti";
        var article = new NewsArticle
        {
            Kicker = race.Dnf ? "GARA · REFERTO" : race.Position == 1 ? "GARA · VITTORIA" : "GARA · CRONACA",
            Title = race.Dnf
                ? $"Gara interrotta per {driver}: il box cerca risposte a {track}"
                : race.Position == 1
                    ? (firstWin ? $"{driver} sorprende al debutto: prima vittoria a {track}" : $"{driver} vince a {track} e conferma la crescita")
                    : race.Position <= 3 ? $"{driver} sale sul podio a {track}: il lavoro comincia a pagare"
                    : $"{driver} chiude P{Math.Max(1, race.Position)} a {track}: una gara utile per crescere",
            Standfirst = race.Dnf
                ? $"Il referto consegna un ritiro dopo {race.Laps} giri con la {car}; il team prepara il lavoro per il prossimo appuntamento."
                : race.Position == 1
                    ? $"Il debuttante {driver} vince la gara sul circuito di {track} con la {car}. Un risultato netto, costruito sulla posizione e non soltanto sul cronometro."
                    : $"Il debuttante {driver} {positionText} sul circuito di {track} con la {car}. Il risultato entra nella storia della stagione e offre al box un riferimento concreto.",
            Byline = $"di {career.Journalist?.Name ?? "Noa Minazuki"} · {career.Journalist?.Publication ?? "Grand Prix CorsaCareer"}",
            DateLine = $"{track} · {story.StoryDate:d MMMM yyyy}",
            CoverageLevel = race.Position == 1 ? 5 : race.Position <= 3 ? 4 : 3,
            Impact = story.Importance,
            Rating = NarrativeEngine.Rating(StoryFacts.From(career, story)),
            Angles = ["cronaca-gara"]
        };
        var cronaca = race.Dnf
            ? $"Il weekend di {driver} si è chiuso prima della bandiera a scacchi. A {track}, la {car} ha completato {race.Laps} giri prima del ritiro: nessuna posizione utile, ma un referto abbastanza preciso da indicare al box dove cominciare l'analisi."
            : race.Position == 1
                ? $"Il debuttante {driver} ha vinto la gara sul circuito di {track}. Partito {(race.StartingPosition > 0 ? $"da P{race.StartingPosition}" : "da una posizione non registrata")}, ha portato la {car} al traguardo dopo {race.Laps} giri, davanti a {Math.Max(0, race.Classification.Count - 1)} avversari."
                : $"Sul circuito di {track}, {driver} ha chiuso in P{race.Position} con la {car}. Partito {(race.StartingPosition > 0 ? $"da P{race.StartingPosition}" : "da una posizione non registrata")}, ha completato {race.Laps} giri davanti a {Math.Max(0, race.Classification.Count - race.Position)} avversari e ha trasformato la gara in un riferimento concreto per il team.";
        article.Paragraphs.Add(cronaca);
        var dettagli = new List<string>();
        if (race.BestLapMilliseconds > 0) dettagli.Add($"Il miglior giro è stato {RookieTargetEngine.Format(race.BestLapMilliseconds)}");
        if (race.GapMilliseconds > 0) dettagli.Add($"il distacco dal vincitore {race.GapMilliseconds / 1000d:0.000} secondi");
        if (race.PenaltySeconds > 0) dettagli.Add($"con {race.PenaltySeconds:0} secondi di penalità");
        article.Paragraphs.Add(dettagli.Count == 0
            ? "La classifica resta il dato principale della giornata: il risultato viene valutato sulla posizione finale, come in una gara reale."
            : string.Join(", ", dettagli) + ". Sono i numeri che il team userà nel debrief tecnico, insieme alla posizione finale.");
        var manager = career.StoryCast?.FirstOrDefault(x => x.Id == StoryCastService.Manager)?.Name ?? "Rei Kisaragi";
        var friend = career.StoryCast?.FirstOrDefault(x => x.Id == StoryCastService.Friend)?.Name ?? "Haru Senda";
        article.Paragraphs.Add(race.Dnf
            ? $"Nel box {manager} ha chiesto di ricostruire l'accaduto senza cercare alibi, mentre {friend} ha invitato il pilota a non leggere il ritiro come una bocciatura. Il tono è quello di una squadra che prepara la risposta, non di un gruppo che archivia la stagione."
            : $"«Ottima prova del debuttante», è stato il primo commento di {manager}. {friend} ha sottolineato la capacità di restare davanti fino al traguardo: una lettura semplice, coerente con il risultato e con quello che si è visto in pista.");
        article.Paragraphs.Add(race.Dnf
            ? $"Il prossimo appuntamento dirà se si è trattato di un episodio o dell'inizio di un problema. Per {driver}, la priorità è tornare a completare una gara."
            : $"La vittoria porta {driver} al centro dell'attenzione, ma non chiude il percorso. Adesso serviranno continuità, una seconda prestazione credibile e la capacità di confermarsi quando gli avversari correranno sapendo chi battere.");
        article.Verdict = race.Dnf ? "Verdetto: giornata da analizzare e lasciarsi alle spalle." : race.Position == 1 ? "Verdetto: una vittoria pesa davvero quando diventa l'inizio di una serie." : "Verdetto: risultato concreto, ora serve continuità.";
        var influencer = Math.Clamp(career.ReputationProfile?.PublicPopularity ?? career.Fanbase, 0, 100);
        article.Sidebar = [$"Risultato: {positionText}", $"Circuito: {track}", $"Auto: {car}", $"Budget: € {career.Cash:N0}", $"Livello influencer: {influencer}/100"];
        return article;
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
        if (!article.Angles.Contains("cronaca-gara", StringComparer.OrdinalIgnoreCase))
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
        var track = UiText.Track(test?.Track ?? story.Track);
        var driver = DisplayName(career.Driver);
        var article = new NewsArticle
        {
            Kicker = "ROOKIE · TEST DI VALUTAZIONE",
            Title = passed ? $"{driver} sorprende al debutto: un giro che fa notizia a {track}" : $"{driver} resta fuori soglia, ma il progetto resta aperto",
            Standfirst = passed
                ? $"Il rookie firma {lap} con la {UiText.Car(test?.Car ?? career.Car)}, oltre {Math.Abs(gap) / 1000.0:0.000} secondi sotto il riferimento della sessione. Un segnale importante per le prossime scelte di carriera."
                : $"Il rookie chiude il test in {lap} con la {UiText.Car(test?.Car ?? career.Car)}. Il riferimento della sessione era {RookieTargetEngine.Format(target)}: il margine resta da colmare, ma il dossier offre già una base concreta.",
            Byline = $"di {career.Journalist?.Name ?? "Noa Minazuki"} · {career.Journalist?.Publication ?? "Grand Prix CorsaCareer"}",
            DateLine = $"{track} · {story.StoryDate:d MMMM yyyy}",
            CoverageLevel = 5, Impact = passed ? 82 : 62, Rating = Math.Clamp(career.RookieEvaluationScore / 10, 1, 10)
        };
        article.Paragraphs.Add($"Un debutto che non è passato inosservato. Nei test disputati al {track}, {driver} ha fatto segnare il miglior tempo della sessione alla guida della {UiText.Car(test?.Car ?? career.Car)}, fermando il cronometro su {lap}.");
        article.Paragraphs.Add($"Il confronto con il riferimento, fissato a {RookieTargetEngine.Format(target)}, rende la misura della prestazione: {gapText}. Il programma ha registrato {test?.Laps ?? 0} giri, sufficienti per prendere confidenza con vettura e tracciato e per lasciare un primo dato tecnico sul tavolo.");
        article.Paragraphs.Add($"Per {driver} era la prima uscita nel programma rookie, senza un contratto né uno sponsor alle spalle. Il test non assegna punti o premi, ma è un passaggio concreto per attirare l'attenzione dei team e costruire credibilità. A seguire il lavoro in pista c'erano {manager}, responsabile del programma rookie, {mechanic}, il meccanico della vettura, e {rival}, primo riferimento diretto del pilota.");
        var influencer = Math.Clamp(career.ReputationProfile?.PublicPopularity ?? career.Fanbase, 0, 100);
        article.Paragraphs.Add($"Dopo questa prova, il livello influencer sale a {influencer}/100, mentre il budget disponibile resta di € {career.Cash:N0}. Sul mercato i primi segnali arrivano da {interest}: interesse, non ancora un sedile garantito.");
        article.Paragraphs.Add(passed
            ? $"Non è il momento di parlare di un contratto sicuro, ma il messaggio lasciato dalla pista è chiaro: {driver} ha iniziato la propria avventura con un tempo che merita attenzione. Per trasformare il debutto in un'opportunità concreta serviranno continuità, risultati e la capacità di confermarsi nelle prossime uscite."
            : $"Il cronometro non ha ancora aperto tutte le porte, ma il test ha fissato una base reale da cui ripartire. Per trasformarla in un'opportunità concreta serviranno chilometri, continuità e un'altra prestazione convincente.");
        article.Verdict = passed ? "Prospettiva: l'esordio ha acceso l'attenzione; adesso serve conferma." : "Prospettiva: il progetto resta aperto, ma la prossima uscita peserà di più.";
        article.Sidebar = [$"Tempo: {lap}", $"Riferimento: {RookieTargetEngine.Format(target)}", $"Scarto: {gapText}", $"Cassa: € {career.Cash:N0}", $"Livello influencer: {influencer}/100"];
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

    private static string ComposePortalArticle(NewsArticle article)
    {
        var builder = new StringBuilder();
        foreach (var paragraph in article.Paragraphs)
        {
            builder.AppendLine(paragraph);
            builder.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(article.Verdict)) builder.AppendLine(article.Verdict);
        if (!string.IsNullOrWhiteSpace(article.Byline)) builder.AppendLine(article.Byline);
        if (!string.IsNullOrWhiteSpace(article.DateLine)) builder.AppendLine(article.DateLine);
        return builder.ToString().TrimEnd();
    }

    private static string DisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Il rookie";
        return System.Globalization.CultureInfo.GetCultureInfo("it-IT").TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());
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
