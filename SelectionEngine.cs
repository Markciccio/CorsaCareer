namespace CorsaCareer1991;

/// <summary>La pagella finale di una selezione, con le voci separate.</summary>
public sealed class SelectionReport
{
    /// <summary>Passo puro, dal time attack contro il riferimento.</summary>
    public int Speed { get; set; }
    /// <summary>Costanza, dal long run.</summary>
    public int Consistency { get; set; }
    /// <summary>Adattamento, dal miglioramento nella familiarizzazione.</summary>
    public int Adaptation { get; set; }
    /// <summary>Come si è comportato in mezzo agli altri.</summary>
    public int RaceCraft { get; set; }
    /// <summary>Pulizia: penalità e danni. È una sottrazione, non un merito.</summary>
    public int Cleanliness { get; set; }
    /// <summary>Punteggio complessivo pesato, 0-100.</summary>
    public int Overall { get; set; }
    /// <summary>Distacco percentuale dal riferimento nel time attack. 0 se non disponibile.</summary>
    public double GapPercent { get; set; }
    public SelectionOutcome Outcome { get; set; }
    public string TeamVerdict { get; set; } = "";
    /// <summary>Le righe che spiegano il punteggio: senza queste il verdetto è un oracolo.</summary>
    public List<string> Breakdown { get; set; } = [];
    /// <summary>Vero se una qualsiasi giornata veniva dal debug.</summary>
    public bool Simulated { get; set; }

    public string Describe() => string.Join("\n", Breakdown);
}

/// <summary>
/// Il motore che giudica una selezione a più giornate.
///
/// Regola non negoziabile: valuta soltanto dati che il referto di Assetto Corsa
/// fornisce davvero — giri, miglior tempo, penalità, danno, posizione. Il
/// referto non espone i track limits né il conteggio degli incidenti, quindi
/// quelle voci non vengono inventate: la pulizia di guida si misura su penalità
/// e danno, e viene dichiarata per quello che è.
///
/// Nessuna giornata mancante viene stimata. Una prova non svolta non vale zero:
/// vale "non pervenuta", e il peso si redistribuisce sulle altre.
/// </summary>
public static class SelectionEngine
{
    /// <summary>Sopra questa soglia il programma offre il sedile.</summary>
    public const int PromotionThreshold = 68;
    /// <summary>Sotto la promozione ma sopra questa soglia, il team concede un secondo tentativo.</summary>
    public const int SecondChanceThreshold = 52;
    /// <summary>Oltre questo distacco percentuale dal riferimento il passo non è da categoria.</summary>
    public const double UnacceptableGapPercent = 4.0;

    /// <summary>
    /// Punteggio di una singola giornata, dai dati del suo referto.
    /// Restituisce 0 e un verdetto esplicito quando i dati non bastano.
    /// </summary>
    public static int ScoreDay(SelectionDayResult day, out string verdict)
    {
        if (!day.Completed) { verdict = "Prova non svolta"; return 0; }

        switch (day.Kind)
        {
            case SelectionDayKind.Familiarisation:
                // Adattamento: conta aver girato abbastanza per imparare la pista.
                // Il referto non espone il tempo giro per giro, quindi il
                // progresso non è misurabile direttamente: si misura ciò che
                // c'è, cioè quanti giri sono stati completati e con che passo.
                if (day.Laps <= 0) { verdict = "Nessun giro completato"; return 0; }
                var volume = Math.Clamp(day.Laps * 100 / 12, 20, 100);
                var pace = GapScore(day.BestLapMilliseconds, day.TargetLapMilliseconds, 6.0);
                var adapt = pace > 0 ? (volume * 45 + pace * 55) / 100 : volume;
                verdict = day.Laps >= 10
                    ? $"{day.Laps} giri completati: la sessione è stata sfruttata"
                    : $"solo {day.Laps} giri: poco per conoscere il tracciato";
                return Math.Clamp(adapt, 0, 100);

            case SelectionDayKind.Consistency:
                // Costanza: un long run vero richiede giri in quantità. Un best
                // lap ottimo su tre giri non dimostra di saper tenere il passo.
                if (day.Laps <= 0) { verdict = "Long run non completato"; return 0; }
                var distance = Math.Clamp(day.Laps * 100 / 18, 15, 100);
                var holding = GapScore(day.BestLapMilliseconds, day.TargetLapMilliseconds, 8.0);
                var clean = PenaltyPenalty(day);
                var consistency = holding > 0 ? (distance * 55 + holding * 45) / 100 : distance;
                consistency -= clean;
                verdict = clean > 12
                    ? $"{day.Laps} giri ma {day.PenaltySeconds:0.#}s di penalità: il long run non è stato pulito"
                    : $"{day.Laps} giri portati a termine";
                return Math.Clamp(consistency, 0, 100);

            case SelectionDayKind.TimeAttack:
                // Tempo puro contro il riferimento: qui il referto dà il dato
                // esatto, ed è l'unica giornata in cui il cronometro decide.
                if (day.BestLapMilliseconds <= 0) { verdict = "Nessun giro valido"; return 0; }
                if (day.TargetLapMilliseconds <= 0)
                {
                    verdict = "Riferimento non disponibile: il tempo non è confrontabile";
                    return 50;
                }
                var speed = GapScore(day.BestLapMilliseconds, day.TargetLapMilliseconds, 4.0);
                var gap = GapPercent(day.BestLapMilliseconds, day.TargetLapMilliseconds);
                verdict = gap <= 0
                    ? $"riferimento battuto di {Math.Abs(gap):0.00}%"
                    : $"{gap:+0.00;-0.00}% dal riferimento";
                return speed;

            default:
                // Gara di valutazione: posizione reale nel gruppo reale.
                if (day.Dnf) { verdict = "Ritiro: la gara non è stata portata a termine"; return 15; }
                if (day.Position <= 0 || day.FieldSize <= 1) { verdict = "Posizione non disponibile"; return 40; }
                var placement = Math.Clamp(100 - (day.Position - 1) * 80 / Math.Max(1, day.FieldSize - 1), 15, 100);
                placement -= PenaltyPenalty(day);
                verdict = $"{day.Position}° su {day.FieldSize}";
                return Math.Clamp(placement, 0, 100);
        }
    }

    /// <summary>
    /// La pagella complessiva. I pesi cambiano in base a quali giornate esistono
    /// davvero: una selezione di due giorni non viene giudicata come se le altre
    /// due fossero andate male.
    /// </summary>
    public static SelectionReport Assess(SelectionTrial trial)
    {
        var report = new SelectionReport { Simulated = trial.AnySimulated };
        var done = trial.Days.Where(x => x.Completed).ToList();

        foreach (var day in trial.Days)
        {
            day.Score = ScoreDay(day, out var verdict);
            day.Verdict = verdict;
        }

        if (done.Count == 0)
        {
            report.Outcome = SelectionOutcome.Pending;
            report.TeamVerdict = "Selezione non ancora cominciata.";
            report.Breakdown.Add("Nessuna prova svolta: non c'è niente da giudicare.");
            return report;
        }

        report.Adaptation = ScoreOf(done, SelectionDayKind.Familiarisation);
        report.Consistency = ScoreOf(done, SelectionDayKind.Consistency);
        report.Speed = ScoreOf(done, SelectionDayKind.TimeAttack);
        report.RaceCraft = ScoreOf(done, SelectionDayKind.EvaluationRace);
        report.Cleanliness = Math.Clamp(100 - done.Sum(PenaltyPenalty), 0, 100);

        var timeAttack = done.FirstOrDefault(x => x.Kind == SelectionDayKind.TimeAttack);
        if (timeAttack != null && timeAttack.TargetLapMilliseconds > 0 && timeAttack.BestLapMilliseconds > 0)
            report.GapPercent = GapPercent(timeAttack.BestLapMilliseconds, timeAttack.TargetLapMilliseconds);

        // Pesi: il passo puro pesa più di tutto, ma non basta da solo. Le voci
        // assenti non contano, così il totale resta su 100 anche a giornate
        // mancanti invece di penalizzare chi non ha potuto girare.
        var weights = new List<(int Score, int Weight, string Label)>();
        if (report.Speed > 0) weights.Add((report.Speed, 35, "passo puro"));
        if (report.Consistency > 0) weights.Add((report.Consistency, 27, "costanza"));
        if (report.RaceCraft > 0) weights.Add((report.RaceCraft, 23, "gara"));
        if (report.Adaptation > 0) weights.Add((report.Adaptation, 15, "adattamento"));

        var totalWeight = weights.Sum(x => x.Weight);
        report.Overall = totalWeight > 0
            ? Math.Clamp(weights.Sum(x => x.Score * x.Weight) / totalWeight, 0, 100)
            : 0;

        report.Breakdown.Add($"SELEZIONE · {trial.Title}");
        // La sostituzione della sede viene dichiarata da chi presenta la
        // selezione: ripeterla qui la mostrava due volte nella stessa schermata.
        report.Breakdown.Add($"{trial.TrackName} · {done.Count}/{trial.Days.Count} prove svolte");
        report.Breakdown.Add("");
        foreach (var day in trial.Days)
            report.Breakdown.Add(day.Completed
                ? $"Giorno {day.Day} · {SelectionDayResult.KindLabel(day.Kind),-22} {day.Score,3}/100  —  {day.Verdict}"
                : $"Giorno {day.Day} · {SelectionDayResult.KindLabel(day.Kind),-22}   —  non svolta");
        report.Breakdown.Add("");
        // Il calcolo va mostrato: un verdetto che cambia una carriera non può
        // essere un numero che appare senza spiegazione.
        foreach (var (score, weight, label) in weights)
            report.Breakdown.Add($"{label,-14} {score,3}/100 × {weight}%");
        var penalties = done.Sum(PenaltyPenalty);
        if (penalties > 0) report.Breakdown.Add($"{"penalità",-14} -{penalties} sulle prove interessate");
        report.Breakdown.Add($"{"TOTALE",-14} {report.Overall,3}/100  (soglia sedile {PromotionThreshold})");

        report.Outcome = Decide(report, trial);
        report.TeamVerdict = VerdictText(report, trial);
        if (report.Simulated)
            report.Breakdown.Add("Almeno una prova è stata risolta in modalità debug: il verdetto è dichiarato come simulato.");
        return report;
    }

    private static SelectionOutcome Decide(SelectionReport report, SelectionTrial trial)
    {
        if (!trial.AllDaysDone) return SelectionOutcome.Pending;
        // Un distacco enorme non si compensa con la costanza: se il passo non è
        // da categoria, il sedile non arriva comunque.
        if (report.GapPercent > UnacceptableGapPercent)
            return report.Overall >= SecondChanceThreshold ? SelectionOutcome.SecondChance : SelectionOutcome.Rejected;
        if (report.Overall >= PromotionThreshold) return SelectionOutcome.Promoted;
        if (report.Overall >= SecondChanceThreshold) return SelectionOutcome.SecondChance;
        return SelectionOutcome.Rejected;
    }

    private static string VerdictText(SelectionReport report, SelectionTrial trial) => report.Outcome switch
    {
        SelectionOutcome.Promoted =>
            $"{trial.OrganisedBy} mette a disposizione un sedile: il passo e la condotta sono da categoria.",
        SelectionOutcome.SecondChance => report.GapPercent > UnacceptableGapPercent
            ? $"{trial.OrganisedBy} non chiude la porta, ma il distacco di {report.GapPercent:0.00}% è troppo: serve un altro tentativo."
            : $"{trial.OrganisedBy} non offre il sedile adesso, però vuole rivedere il pilota.",
        SelectionOutcome.Rejected =>
            $"{trial.OrganisedBy} chiude il dossier: per questa categoria non è abbastanza.",
        _ => "Selezione ancora in corso."
    };

    private static int ScoreOf(List<SelectionDayResult> done, SelectionDayKind kind) =>
        done.Where(x => x.Kind == kind).Select(x => x.Score).DefaultIfEmpty(0).Max();

    /// <summary>Distacco percentuale dal riferimento. Negativo se il riferimento è battuto.</summary>
    public static double GapPercent(int bestLap, int target) =>
        target <= 0 || bestLap <= 0 ? 0 : (bestLap - target) / (double)target * 100.0;

    /// <summary>
    /// Punteggio da un distacco: 100 se il riferimento è battuto, e scende fino
    /// a 0 quando il distacco raggiunge <paramref name="tolerancePercent"/>.
    /// Restituisce 0 se i dati non permettono il confronto.
    /// </summary>
    private static int GapScore(int bestLap, int target, double tolerancePercent)
    {
        if (bestLap <= 0 || target <= 0) return 0;
        var gap = GapPercent(bestLap, target);
        if (gap <= 0) return 100;
        return Math.Clamp((int)Math.Round(100 - gap / tolerancePercent * 100), 0, 100);
    }

    /// <summary>
    /// La penalizzazione per guida sporca, dai soli dati disponibili: penalità
    /// in secondi e danno dichiarato. Il referto non conta i track limits, quindi
    /// questa è una proxy e non pretende di essere altro.
    /// </summary>
    private static int PenaltyPenalty(SelectionDayResult day)
    {
        var fromPenalties = (int)Math.Round(Math.Clamp(day.PenaltySeconds, 0, 60) / 2.0);
        var damage = day.Damage > 1 ? day.Damage / 100.0 : day.Damage;
        var fromDamage = (int)Math.Round(Math.Clamp(damage, 0, 1) * 18);
        return Math.Clamp(fromPenalties + fromDamage, 0, 35);
    }
}
