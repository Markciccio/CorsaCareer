namespace CorsaCareer;

/// <summary>
/// Chi svolge un'attività: il pilota, o Haru Senda.
///
/// Sono due agende parallele. Il pilota si allena, incontra i tifosi, riposa;
/// Haru gira a cercare chi paga. Entrambi consumano le proprie ore, e le due
/// colonne non si sommano: un giorno in cui Haru lavora molto non è un giorno in
/// cui il pilota ha fatto qualcosa.
/// </summary>
public enum DayActor
{
    Driver,
    Agent
}

/// <summary>
/// Che cosa un'attività fa crescere. Serve a tenere separate cose che prima
/// erano confuse in un unico valore.
/// </summary>
public enum DayEffectKind
{
    /// <summary>Forma fisica: tiene il ritmo nei long run e riduce gli errori.</summary>
    Fitness,
    /// <summary>Quanto stanco è il pilota. Cresce lavorando, scende riposando.</summary>
    Fatigue,
    /// <summary>Il pubblico: tifosi, social, riconoscibilità.</summary>
    Popularity,
    /// <summary>Come lo vede il paddock: tecnici, osservatori, stampa.</summary>
    SportingReputation,
    /// <summary>Denaro.</summary>
    Money
}

/// <summary>Un effetto dichiarato di un'attività, con il suo verso.</summary>
public sealed record DayEffect(DayEffectKind Kind, int Amount);

/// <summary>
/// Un'attività che si può inserire nella giornata.
///
/// L'esito non è garantito: un post sui social può passare inosservato o
/// funzionare molto bene, e questo dipende da chi sei in quel momento. Le
/// attività con esito certo — riposare, allenarsi — hanno una sola fascia.
/// </summary>
/// <summary>
/// A quale delle due schede della Home appartiene un'attività.
///
/// «Forma fisica» e «Livello influencer» aprivano lo stesso identico elenco:
/// dal riquadro social si finiva a scegliere la palestra. Sono due mestieri
/// diversi e vanno tenuti separati.
/// </summary>
public enum DayFocus
{
    /// <summary>Corpo e recupero: palestra, corsa, fisioterapia, riposo.</summary>
    Fisico,
    /// <summary>Nome e seguito: social, tifosi, stampa, relazioni pubbliche.</summary>
    Immagine,
    /// <summary>Tutto il resto: lavoro pagato, commissioni.</summary>
    Altro
}

public sealed class DayActivity
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    /// <summary>Chi la svolge.</summary>
    public DayActor Actor { get; init; } = DayActor.Driver;
    /// <summary>In quale scheda della Home compare.</summary>
    public DayFocus Focus { get; init; } = DayFocus.Altro;
    /// <summary>Ore consumate della giornata.</summary>
    public int Hours { get; init; } = 1;
    /// <summary>Costo in denaro, se ne ha uno.</summary>
    public int Cost { get; init; }
    /// <summary>Che cosa fa, detto al giocatore prima di sceglierla.</summary>
    public string Promise { get; init; } = "";
    /// <summary>Gli esiti possibili, dal peggiore al migliore.</summary>
    public List<DayOutcome> Outcomes { get; init; } = [];
    /// <summary>Vero se l'esito è sempre lo stesso: allenarsi funziona sempre.</summary>
    public bool IsCertain => Outcomes.Count <= 1;
}

/// <summary>
/// Un esito possibile di un'attività, con quanto è probabile e cosa produce.
/// </summary>
public sealed class DayOutcome
{
    /// <summary>Come viene raccontato: è la frase che il giocatore legge.</summary>
    public string Line { get; init; } = "";
    /// <summary>
    /// Peso relativo dell'esito. Non è una percentuale: viene confrontato con
    /// gli altri esiti della stessa attività, e può essere modificato dallo
    /// stato della carriera.
    /// </summary>
    public int Weight { get; init; } = 1;
    public List<DayEffect> Effects { get; init; } = [];
    /// <summary>Vero se è un esito negativo: serve a raccontarlo come tale.</summary>
    public bool IsSetback { get; init; }
}

/// <summary>
/// La giornata del pilota: quante ore restano, a chi, e cosa è già stato fatto.
///
/// Prima fra un impegno e l'altro non c'era niente da fare, e questo rendeva
/// impossibile perfino indicare un'alternativa quando i soldi mancavano: non
/// esisteva un modo di guadagnare, allenarsi o farsi conoscere. La giornata
/// diventa un turno breve — poche decisioni, non venticinque.
/// </summary>
public sealed class DayPlan
{
    public DateTime Date { get; set; }
    /// <summary>Ore del pilota ancora libere.</summary>
    public int DriverHoursLeft { get; set; } = DriverDay.DriverHours;
    /// <summary>Ore di Haru ancora libere.</summary>
    public int AgentHoursLeft { get; set; } = DriverDay.AgentHours;
    /// <summary>Identificativi delle attività già svolte oggi.</summary>
    public List<string> Done { get; set; } = [];
}

/// <summary>
/// Le regole della giornata.
///
/// Il vincolo che tiene in piedi tutto è la stanchezza: senza, converrebbe
/// sempre riempire ogni ora disponibile, e non ci sarebbe nessuna scelta da
/// fare. Allenarsi stanca; correre stanchi rende meno.
/// </summary>
public static class DriverDay
{
    /// <summary>Ore di una giornata del pilota.</summary>
    public const int DriverHours = 8;

    /// <summary>
    /// Ore di Haru. Meno del pilota: è una persona che ha anche altro da fare, e
    /// due o tre visite al giorno sono già molte.
    /// </summary>
    public const int AgentHours = 6;

    /// <summary>Oltre questa stanchezza le prestazioni cominciano a calare.</summary>
    public const int TiredThreshold = 60;

    /// <summary>Stanchezza massima.</summary>
    public const int MaxFatigue = 100;

    /// <summary>Forma fisica massima.</summary>
    public const int MaxFitness = 100;

    /// <summary>
    /// Restituisce il piano della giornata realmente corrente.
    ///
    /// Tutte le schermate devono passare da qui: un piano creato solo come
    /// fallback locale faceva sembrare disponibili ore che in realtà erano
    /// già state consumate in un'altra schermata.
    /// </summary>
    public static DayPlan EnsureToday(CareerState career)
    {
        if (career.Today == null || career.Today.Date.Date != career.StoryDate.Date)
            career.Today = new DayPlan { Date = career.StoryDate };
        return career.Today;
    }

    /// <summary>
    /// Quanto la condizione del pilota pesa sulla prestazione, come scarto sulla
    /// scala del passo (la stessa del livello IA).
    ///
    /// Non tocca la fisica di Assetto Corsa: entra nella valutazione di una
    /// prestazione e nella simulazione, dove il programma decide da sé.
    /// </summary>
    public static double PaceShift(int fitness, int fatigue)
    {
        // La forma dà, la stanchezza toglie. Una forma alta con molta stanchezza
        // non compensa: si arriva alla gara già consumati.
        var fromFitness = (Math.Clamp(fitness, 0, MaxFitness) - 50) * 0.05;
        var excess = Math.Max(0, Math.Clamp(fatigue, 0, MaxFatigue) - TiredThreshold);
        var fromFatigue = -excess * 0.12;
        return Math.Round(fromFitness + fromFatigue, 2);
    }

    /// <summary>
    /// Come si legge la condizione, in parole. Serve al giocatore per decidere
    /// se conviene riposare prima di una gara.
    /// </summary>
    public static string ConditionLabel(int fitness, int fatigue)
    {
        if (fatigue >= 85) return "esausto";
        if (fatigue >= TiredThreshold) return "stanco";
        if (fitness >= 75) return "in ottima forma";
        if (fitness >= 45) return "in condizione";
        return "fuori forma";
    }

    /// <summary>
    /// Vero se l'attività si può inserire nella giornata: ore sufficienti, soldi
    /// sufficienti, e non già svolta oggi.
    /// </summary>
    public static bool CanDo(DayPlan day, DayActivity activity, int cash, out string refusal)
    {
        refusal = "";
        if (day.Done.Contains(activity.Id, StringComparer.OrdinalIgnoreCase))
        {
            refusal = "Già fatto oggi.";
            return false;
        }

        var left = activity.Actor == DayActor.Driver ? day.DriverHoursLeft : day.AgentHoursLeft;
        if (activity.Hours > left)
        {
            refusal = $"Servono {activity.Hours} ore e ne restano {left}.";
            return false;
        }

        if (activity.Cost > 0 && activity.Cost > cash)
        {
            refusal = $"Costa € {activity.Cost:N0} e in cassa ci sono € {cash:N0}.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sceglie un esito fra quelli possibili, pesando le probabilità con lo
    /// stato della carriera.
    ///
    /// È deterministico rispetto al seme: la stessa giornata giocata due volte
    /// dà lo stesso risultato, altrimenti basterebbe ricaricare per riprovare.
    /// </summary>
    public static DayOutcome Resolve(DayActivity activity, CareerState career, long seed)
    {
        if (activity.Outcomes.Count == 0)
            return new DayOutcome { Line = "Non è successo niente di rilevante." };
        if (activity.Outcomes.Count == 1) return activity.Outcomes[0];

        var profile = career.ReputationProfile ?? new ReputationProfile();
        var weights = new List<int>(activity.Outcomes.Count);
        foreach (var outcome in activity.Outcomes)
        {
            var weight = Math.Max(1, outcome.Weight);
            // Chi è già seguito ottiene di più da un'uscita pubblica; chi non lo
            // è rischia di passare inosservato. È la differenza che rende
            // sensato costruirsi un seguito prima di provarci.
            if (outcome.Effects.Any(x => x.Kind == DayEffectKind.Popularity && x.Amount > 0))
                weight += profile.PublicPopularity / 12;
            if (outcome.IsSetback)
                weight = Math.Max(1, weight - profile.Professionalism / 25);
            weights.Add(weight);
        }

        var total = weights.Sum();
        var roll = (int)(Unit(seed) * total);
        var running = 0;
        for (var i = 0; i < weights.Count; i++)
        {
            running += weights[i];
            if (roll < running) return activity.Outcomes[i];
        }
        return activity.Outcomes[^1];
    }

    /// <summary>Valore in [0,1) stabile per un seme. Nessun Random: l'esito deve essere riproducibile.</summary>
    internal static double Unit(long seed)
    {
        unchecked
        {
            var h = 2166136261u;
            var value = (ulong)seed;
            for (var i = 0; i < 8; i++)
            {
                h ^= (uint)(value & 0xFF);
                h *= 16777619u;
                value >>= 8;
            }
            return (h % 1000000u) / 1000000.0;
        }
    }

    /// <summary>Seme di un'attività in una giornata: stabile, ma diverso ogni giorno.</summary>
    public static long SeedFor(string driver, DateTime date, string activityId) =>
        Hash($"{driver}|{date:yyyyMMdd}|{activityId}");

    private static long Hash(string text)
    {
        unchecked
        {
            var h = 1469598103934665603UL;
            foreach (var c in text) { h ^= c; h *= 1099511628211UL; }
            return (long)(h & 0x7FFFFFFFFFFFFFFF);
        }
    }
}
