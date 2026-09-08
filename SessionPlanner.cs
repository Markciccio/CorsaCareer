namespace CorsaCareer;

/// <summary>
/// Profili scelti dal giocatore. Non alterano mai un risultato già ottenuto:
/// agiscono solo sulla preparazione della sessione successiva.
/// </summary>
public static class SessionProfiles
{
    public const string DifficultyAssisted = "Assistita";
    public const string DifficultyRealistic = "Realistica";
    public const string DifficultyProfessional = "Professionale";
    public const string DifficultyMaximum = "Massima";
    public static readonly string[] Difficulties = [DifficultyAssisted, DifficultyRealistic, DifficultyProfessional, DifficultyMaximum];

    public const string DistanceShort = "Breve";
    public const string DistanceRealistic = "Realistica";
    public const string DistanceLong = "Lunga";
    public static readonly string[] Distances = [DistanceShort, DistanceRealistic, DistanceLong];

    public static int DifficultyOffset(string profile) => profile switch
    {
        DifficultyAssisted => -8,
        DifficultyProfessional => 3,
        DifficultyMaximum => 5,
        _ => 0
    };

    public static double DistanceMultiplier(string profile) => profile switch
    {
        DistanceShort => 0.55,
        DistanceLong => 1.7,
        _ => 1.0
    };

    public static double AggressionMultiplier(string profile) => profile switch
    {
        DifficultyAssisted => 0.6,
        DifficultyProfessional => 1.15,
        DifficultyMaximum => 1.3,
        _ => 1.0
    };
}

public sealed class SessionRequest
{
    public string TrackId { get; set; } = "";
    public string TrackName { get; set; } = "";
    public int TrackLengthMeters { get; set; }
    public string TrackCategory { get; set; } = "permanent";
    public string CarCategory { get; set; } = "special";
    public string Tier { get; set; } = "Rookie";

    /// <summary>
    /// Altezza del campionato (1..5). Entra nel livello IA del preset, quindi
    /// vale anche per la gara vera aperta in Content Manager: salire di
    /// campionato deve rendere il gruppo più forte anche in pista, non solo
    /// nella simulazione interna.
    /// </summary>
    public int ChampionshipLevel { get; set; } = 1;

    /// <summary>
    /// Gradino della scala di carriera (1..7), zero se ignoto. Decide il
    /// livello del gruppo meglio del «tier» meccanico: un kart con cambio e una
    /// Formula 4 condividono lo stesso tier ma non lo stesso momento della
    /// carriera.
    /// </summary>
    public int LadderStep { get; set; }

    public int Season { get; set; } = 1;
    public int RoundIndex { get; set; }
    public int RoundCount { get; set; } = 1;
    public DateTime StoryDate { get; set; } = new DateTime(2000, 3, 12);
    public string DifficultyProfile { get; set; } = SessionProfiles.DifficultyRealistic;
    public string DistanceProfile { get; set; } = SessionProfiles.DistanceRealistic;
    public int AiCalibrationOffset { get; set; }
    public IReadOnlyList<string> InstalledWeatherIds { get; set; } = [];
    public bool EnduranceRound { get; set; }
    public bool MixedGrid { get; set; }
    public bool TestSession { get; set; }
}

public sealed class SessionPlan
{
    public string FormatLabel { get; set; } = "";
    public int PracticeMinutes { get; set; }
    public int QualifyingMinutes { get; set; }
    public int RaceLaps { get; set; }
    public int RaceDistanceMeters { get; set; }
    public string WeatherId { get; set; } = "3_clear";
    public string WeatherLabel { get; set; } = "sereno";
    public int CloudCover { get; set; }
    public int TimeOfDaySeconds { get; set; } = 50400;
    public double TemperatureC { get; set; } = 22;
    public double AiLevel { get; set; } = 92;
    public double AiLevelMin { get; set; } = 90;
    public double AiAggression { get; set; } = 35;
    public double AiAggressionMin { get; set; } = 25;
    public double WindSpeedMin { get; set; }
    public double WindSpeedMax { get; set; }
    public double WindDirection { get; set; }
    public bool Endurance { get; set; }
    public List<string> Notes { get; set; } = [];

    public string TimeOfDayLabel => TimeSpan.FromSeconds(TimeOfDaySeconds).ToString(@"hh\:mm");

    public string DistanceLabel => RaceLaps <= 0
        ? "sessione libera"
        : RaceDistanceMeters > 0 ? $"{RaceLaps} giri ≈ {RaceDistanceMeters / 1000.0:0.#} km" : $"{RaceLaps} giri (distanza del circuito non dichiarata)";

    public string Describe() =>
        $"{FormatLabel} · {DistanceLabel}\n" +
        $"Meteo: {WeatherLabel} · {TemperatureC:0.#} °C · ore {TimeOfDayLabel}\n" +
        $"Prove {PracticeMinutes}' · Qualifica {QualifyingMinutes}' · AI {AiLevel:0}% (min {AiLevelMin:0}%) · aggressività {AiAggression:0}%";
}

/// <summary>
/// Il piano effettivamente scritto nel preset, archiviato nella carriera: permette
/// a giornale, timeline e servizi audio di citare condizioni reali del weekend.
/// </summary>
public sealed class SessionPlanRecord
{
    public int Season { get; set; }
    public int Round { get; set; }
    public string Mode { get; set; } = "race";
    public string Track { get; set; } = "";
    public string Car { get; set; } = "";
    public string FormatLabel { get; set; } = "";
    public int RaceLaps { get; set; }
    public int RaceDistanceMeters { get; set; }
    public string WeatherId { get; set; } = "";
    public string WeatherLabel { get; set; } = "";
    public double TemperatureC { get; set; }
    public int TimeOfDaySeconds { get; set; }
    public double AiLevel { get; set; }
    public double AiAggression { get; set; }
    public bool Endurance { get; set; }
    public DateTime PreparedUtc { get; set; }
    public DateTime StoryDate { get; set; }
    public List<string> Notes { get; set; } = [];

    public static SessionPlanRecord From(SessionPlan plan, int season, int round, string mode, string track, string car, DateTime storyDate) => new()
    {
        Season = season, Round = round, Mode = mode, Track = track, Car = car,
        FormatLabel = plan.FormatLabel, RaceLaps = plan.RaceLaps, RaceDistanceMeters = plan.RaceDistanceMeters,
        WeatherId = plan.WeatherId, WeatherLabel = plan.WeatherLabel, TemperatureC = plan.TemperatureC,
        TimeOfDaySeconds = plan.TimeOfDaySeconds, AiLevel = plan.AiLevel, AiAggression = plan.AiAggression,
        Endurance = plan.Endurance, PreparedUtc = DateTime.UtcNow, StoryDate = storyDate,
        Notes = plan.Notes.ToList()
    };
}

/// <summary>
/// Costruisce il formato reale del weekend a partire dai contenuti installati.
/// È deterministico (nessun <c>Random</c>): la stessa carriera allo stesso round
/// produce sempre le stesse condizioni, così il weekend resta riproducibile e
/// verificabile dai test.
/// </summary>
public static class SessionPlanner
{
    // Assetto Corsa senza CSP accetta solo orari fra le 08:00 e le 18:00.
    public const int EarliestTimeOfDay = 8 * 3600;
    public const int LatestTimeOfDay = 18 * 3600;
    public const string FallbackWeatherId = "3_clear";

    public static SessionPlan Plan(SessionRequest request)
    {
        var plan = new SessionPlan();
        var seed = StableSeed(request.Season, request.RoundIndex, request.TrackId, request.CarCategory);
        var endurance = request.EnduranceRound && !request.TestSession;
        plan.Endurance = endurance;

        ApplyWeather(plan, request, seed);
        ApplyTimeOfDay(plan, request, seed, endurance);
        ApplyTemperature(plan, request);
        ApplyWind(plan, seed);
        ApplyAi(plan, request);

        if (request.TestSession)
        {
            plan.FormatLabel = "Sessione di test privato";
            plan.PracticeMinutes = request.DistanceProfile == SessionProfiles.DistanceShort ? 20 : 40;
            plan.QualifyingMinutes = 0;
            plan.RaceLaps = 0;
            plan.RaceDistanceMeters = 0;
            return plan;
        }

        ApplyRaceDistance(plan, request, endurance);
        ApplyPracticeAndQualifying(plan, request, endurance);
        plan.FormatLabel = endurance ? "Gara di durata"
            : plan.RaceDistanceMeters > 0
                ? plan.RaceDistanceMeters >= 90000 ? "Gara lunga" : plan.RaceDistanceMeters >= 45000 ? "Gara media" : "Gara sprint"
                : plan.RaceLaps >= 18 ? "Gara lunga" : plan.RaceLaps >= 10 ? "Gara media" : "Gara sprint";
        return plan;
    }

    /// <summary>
    /// Sceglie il round di durata della stagione: il circuito più lungo del
    /// calendario, solo per le categorie in cui una gara di durata ha senso e
    /// solo se il campionato ha almeno quattro appuntamenti.
    /// </summary>
    public static int EnduranceRoundIndex(IReadOnlyList<ContentTrackRecord> calendar, string carCategory)
    {
        if (calendar.Count < 4) return -1;
        var eligible = new[] { "gt3", "gt2", "gt1", "prototype", "lmp", "hypercar", "historic" };
        if (!eligible.Contains((carCategory ?? "").Trim().ToLowerInvariant())) return -1;
        var longest = -1; var longestLength = 0;
        for (var i = 0; i < calendar.Count; i++)
            if (calendar[i].LengthMeters > longestLength) { longestLength = calendar[i].LengthMeters; longest = i; }
        return longestLength > 0 ? longest : calendar.Count / 2;
    }

    private static void ApplyRaceDistance(SessionPlan plan, SessionRequest request, bool endurance)
    {
        var targetMeters = BaseRaceDistanceMeters(request.CarCategory, request.Tier);
        targetMeters = (int)Math.Round(targetMeters * SessionProfiles.DistanceMultiplier(request.DistanceProfile));
        if (endurance) targetMeters = (int)Math.Round(targetMeters * 2.5);
        if (request.TrackLengthMeters > 0)
        {
            plan.RaceLaps = Math.Clamp((int)Math.Round(targetMeters / (double)request.TrackLengthMeters), 2, 90);
            plan.RaceDistanceMeters = plan.RaceLaps * request.TrackLengthMeters;
        }
        else
        {
            // Senza lunghezza dichiarata non si può calcolare una distanza reale:
            // si usa un numero di giri per categoria e il fallback viene dichiarato.
            plan.RaceLaps = FallbackLaps(request.CarCategory, request.DistanceProfile, endurance);
            plan.RaceDistanceMeters = 0;
            plan.Notes.Add($"Lunghezza di {(string.IsNullOrWhiteSpace(request.TrackName) ? request.TrackId : request.TrackName)} non dichiarata nei metadati: giri calcolati per categoria, non su distanza reale.");
        }
        if (endurance) plan.Notes.Add("Round di durata della stagione: distanza aumentata sul circuito più lungo del calendario.");
    }

    private static int BaseRaceDistanceMeters(string carCategory, string tier) => (carCategory ?? "").Trim().ToLowerInvariant() switch
    {
        "kart" => 20000,
        "cup" or "touring" or "tcr" or "gt4" or "formula 4" or "formula junior" or "historic" => 60000,
        "gt3" or "gt2" or "formula 3" or "prototype" => 100000,
        "formula 2" => 120000,
        "gt1" or "lmp" or "hypercar" => 140000,
        "formula" => 160000,
        _ => tier switch
        {
            "Formula / top tier" => 140000,
            "Categoria avanzata" => 100000,
            "Categoria regionale" => 60000,
            _ => 30000
        }
    };

    private static int FallbackLaps(string carCategory, string distanceProfile, bool endurance)
    {
        var laps = (carCategory ?? "").Trim().ToLowerInvariant() switch
        {
            "kart" => 12,
            "cup" or "touring" or "tcr" or "gt4" or "formula 4" or "formula junior" or "historic" => 14,
            "gt3" or "gt2" or "formula 3" or "prototype" or "formula 2" => 18,
            "gt1" or "lmp" or "hypercar" or "formula" => 22,
            _ => 10
        };
        laps = (int)Math.Round(laps * SessionProfiles.DistanceMultiplier(distanceProfile));
        if (endurance) laps = (int)Math.Round(laps * 2.5);
        return Math.Clamp(laps, 2, 90);
    }

    private static void ApplyPracticeAndQualifying(SessionPlan plan, SessionRequest request, bool endurance)
    {
        var (practice, qualifying) = request.Tier switch
        {
            "Formula / top tier" => (25, 20),
            "Categoria avanzata" => (20, 18),
            "Categoria regionale" => (15, 15),
            _ => (12, 12)
        };
        if (endurance) { practice += 10; qualifying += 5; }
        if (request.DistanceProfile == SessionProfiles.DistanceShort) { practice = Math.Max(5, practice / 2); qualifying = Math.Max(5, qualifying / 2); }
        plan.PracticeMinutes = practice;
        plan.QualifyingMinutes = qualifying;
    }

    private static void ApplyWeather(SessionPlan plan, SessionRequest request, long seed)
    {
        var installed = request.InstalledWeatherIds ?? [];
        var weights = SeasonalWeatherWeights(request.StoryDate.Month);
        var options = new List<(string id, WeatherCatalog.WeatherOption option, int weight)>();
        foreach (var id in installed)
        {
            var option = WeatherCatalog.Match(id);
            if (option == null) continue;
            var weight = weights.TryGetValue(option.Key, out var value) ? value : 0;
            if (weight > 0) options.Add((id, option, weight));
        }
        if (options.Count == 0)
        {
            var fallbackId = installed.FirstOrDefault(x => x.Contains("clear", StringComparison.OrdinalIgnoreCase)) ?? installed.FirstOrDefault() ?? FallbackWeatherId;
            var fallbackOption = WeatherCatalog.Match(fallbackId);
            plan.WeatherId = fallbackId;
            plan.WeatherLabel = fallbackOption?.Label ?? "condizioni dichiarate dal contenuto installato";
            plan.CloudCover = fallbackOption?.CloudCover ?? 0;
            plan.Notes.Add(installed.Count == 0
                ? "Nessun meteo installato rilevato: usato il preset sereno predefinito di Assetto Corsa."
                : $"Nessun meteo installato corrisponde alla stagione narrativa: usato {fallbackId}.");
            return;
        }
        var picked = WeightedPick(options.Select(x => (x.id, x.weight)).ToList(), seed, 0);
        var pickedOption = options.First(x => x.id == picked).option;
        plan.WeatherId = picked;
        plan.WeatherLabel = pickedOption.Label;
        plan.CloudCover = pickedOption.CloudCover;
    }

    private static Dictionary<string, int> SeasonalWeatherWeights(int month) => month switch
    {
        12 or 1 or 2 => new() { ["clear"] = 2, ["mid_clear"] = 2, ["light_clouds"] = 3, ["mid_clouds"] = 4, ["heavy_clouds"] = 4, ["light_fog"] = 3, ["heavy_fog"] = 1 },
        3 or 4 or 10 or 11 => new() { ["clear"] = 3, ["mid_clear"] = 3, ["light_clouds"] = 4, ["mid_clouds"] = 3, ["heavy_clouds"] = 2, ["light_fog"] = 1 },
        _ => new() { ["clear"] = 5, ["mid_clear"] = 4, ["light_clouds"] = 3, ["mid_clouds"] = 2, ["heavy_clouds"] = 1 }
    };

    private static void ApplyTimeOfDay(SessionPlan plan, SessionRequest request, long seed, bool endurance)
    {
        if (endurance)
        {
            // Una gara di durata parte nel pomeriggio per chiudersi con la luce calante.
            plan.TimeOfDaySeconds = Math.Clamp(15 * 3600 + 30 * 60, EarliestTimeOfDay, LatestTimeOfDay);
            plan.Notes.Add("Partenza pomeridiana: la gara di durata si chiude con la luce calante.");
            return;
        }
        var slots = new List<(string id, int weight)> { ("10:30", 2), ("12:00", 2), ("14:00", 4), ("15:30", 3), ("17:00", 2) };
        var picked = WeightedPick(slots, seed, 7);
        var seconds = picked switch
        {
            "10:30" => 10 * 3600 + 1800,
            "12:00" => 12 * 3600,
            "15:30" => 15 * 3600 + 1800,
            "17:00" => 17 * 3600,
            _ => 14 * 3600
        };
        plan.TimeOfDaySeconds = Math.Clamp(seconds, EarliestTimeOfDay, LatestTimeOfDay);
    }

    private static void ApplyTemperature(SessionPlan plan, SessionRequest request)
    {
        var monthBase = request.StoryDate.Month switch
        {
            1 => 8, 2 => 9, 3 => 13, 4 => 16, 5 => 21, 6 => 26,
            7 => 29, 8 => 29, 9 => 24, 10 => 18, 11 => 12, _ => 9
        };
        var hour = plan.TimeOfDaySeconds / 3600.0;
        var timeAdjustment = hour < 11 ? -3.0 : hour < 13 ? 1.0 : hour < 16 ? 2.0 : -1.5;
        var cloudAdjustment = -(plan.CloudCover / 100.0 * 7.0);
        plan.TemperatureC = Math.Round(Math.Clamp(monthBase + timeAdjustment + cloudAdjustment, 4, 38), 1);
    }

    private static void ApplyWind(SessionPlan plan, long seed)
    {
        var gust = (int)(Math.Abs(seed / 13) % 18);
        plan.WindSpeedMin = gust;
        plan.WindSpeedMax = gust + 4 + plan.CloudCover / 25;
        plan.WindDirection = Math.Abs(seed / 29) % 360;
    }

    /// <summary>
    /// Il livello degli avversari per un gradino della scala.
    ///
    /// Sta qui e non dentro ApplyAi perche' serve anche a chi risolve una
    /// sessione senza averla preparata: prima quel percorso usava la costante
    /// 90 — il valore di una categoria regionale — e in kart rendeva la gara
    /// persa prima di cominciare.
    /// </summary>
    /// <summary>
    /// Quanto è più forte il gruppo salendo di campionato.
    ///
    /// La categoria dice che macchina si guida, il livello dice contro chi:
    /// gli stessi kart, in un nazionale, sono guidati da gente che in un
    /// campionato di zona non troveresti. Senza questo il campo cresceva solo
    /// cambiando vettura, e vincere in alto era più facile che in basso.
    /// </summary>
    public static double LevelBonus(int championshipLevel) =>
        Math.Clamp(championshipLevel - 1, 0, ChampionshipLadder.Levels - 1) * 2.5;

    /// <summary>
    /// Il livello del gruppo secondo l'altezza reale nella scala di carriera.
    ///
    /// Con il solo «tier» meccanico il kart 125 con cambio finiva in
    /// «Categoria regionale» insieme a Formula 4 e GT4, quindi con un campo a
    /// novanta: la fase più dura dell'intera carriera capitava all'inizio,
    /// contro un pilota che parte intorno a ottanta. Nel collaudo erano
    /// trentaquattro gare di kart senza mai un podio, e poi vittorie in
    /// Formula 1 — la difficoltà girata al contrario. La salita ora è
    /// monotona: ogni gradino è più duro del precedente.
    /// </summary>
    public static double AiLevelForStep(int ladderStep) => ladderStep switch
    {
        1 => 76.0,  // kart quattro tempi: il fondo
        2 => 80.0,  // kart due tempi
        3 => 84.0,  // kart 125 con cambio
        4 => 88.0,  // formula d'ingresso, GT4, monomarca
        5 => 92.0,  // formula nazionale, GT3, turismo internazionale
        6 => 95.0,  // formula internazionale, prototipi
        7 => 97.0,  // il vertice
        _ => 0.0    // gradino sconosciuto: decide il tier, come prima
    };

    public static double DefaultAiLevel(string? tier) => tier switch
    {
        "Formula / top tier" => 96.0,
        "Categoria avanzata" => 93.0,
        "Categoria regionale" => 90.0,
        // Il gradino d'ingresso mancava da questo elenco e ricadeva nel ramo
        // generico a 86: contro un esordiente — che parte intorno a 76 —
        // significava ultimo posto garantito alla prima gara, per tabella e
        // non per come era andata in pista.
        "Rookie" => 78.0,
        _ => 86.0
    };

    private static void ApplyAi(SessionPlan plan, SessionRequest request)
    {
        // Il livello del campionato alza il gruppo, ma non può trasformare una
        // categoria d'ingresso in un mondiale: in Formula Vee si arrivava a 96
        // perché al valore del gradino (88) si sommava per intero il bonus del
        // livello 5. Il tetto resta quello della categoria più due punti: una
        // monoposto da esordienti non ospita avversari da vertice, per quanto
        // importante sia il campionato che la usa.
        var perGradino = AiLevelForStep(request.LadderStep);
        var perCategoria = perGradino > 0 ? perGradino : DefaultAiLevel(request.Tier);
        var baseLevel = Math.Min(perCategoria + 2.0, perCategoria + LevelBonus(request.ChampionshipLevel));
        var level = baseLevel + SessionProfiles.DifficultyOffset(request.DifficultyProfile) + Math.Clamp(request.AiCalibrationOffset, -10, 6);
        plan.AiLevel = Math.Clamp(Math.Round(level, 0), 70, 100);
        var spread = request.MixedGrid ? 5 : 2;
        plan.AiLevelMin = Math.Clamp(plan.AiLevel - spread, 70, 100);
        var aggression = BaseAggression(request.CarCategory) * SessionProfiles.AggressionMultiplier(request.DifficultyProfile);
        plan.AiAggression = Math.Clamp(Math.Round(aggression, 0), 0, 100);
        plan.AiAggressionMin = Math.Clamp(plan.AiAggression - 12, 0, 100);
        if (request.AiCalibrationOffset != 0)
            plan.Notes.Add($"Calibrazione AI dai referti reali: {(request.AiCalibrationOffset > 0 ? "+" : "")}{request.AiCalibrationOffset} punti sul livello di partenza. Nessun risultato già registrato viene modificato.");
    }

    private static double BaseAggression(string carCategory) => (carCategory ?? "").Trim().ToLowerInvariant() switch
    {
        "kart" => 60,
        "touring" or "tcr" or "cup" => 55,
        "gt4" => 45,
        "gt3" or "gt2" or "gt1" => 40,
        "historic" => 30,
        "prototype" or "lmp" or "hypercar" => 30,
        "formula" or "formula 2" or "formula 3" or "formula 4" or "formula junior" => 25,
        _ => 35
    };

    internal static long StableSeed(int season, int roundIndex, string trackId, string carCategory)
    {
        var text = $"{season}|{roundIndex}|{trackId?.ToLowerInvariant()}|{carCategory?.ToLowerInvariant()}";
        var hash = 2166136261L;
        foreach (var character in text) hash = (hash ^ character) * 16777619 % 2147483647;
        return Math.Abs(hash);
    }

    private static string WeightedPick(List<(string id, int weight)> options, long seed, int salt)
    {
        var total = options.Sum(x => x.weight);
        if (total <= 0) return options.Count > 0 ? options[0].id : "";
        var target = (int)((Math.Abs(seed) + salt * 7919) % total);
        foreach (var (id, weight) in options)
        {
            target -= weight;
            if (target < 0) return id;
        }
        return options[^1].id;
    }
}

public static class WeatherCatalog
{
    public sealed record WeatherOption(string Key, string Label, int CloudCover);

    // L'ordine conta: "mid_clear" va riconosciuto prima di "clear".
    private static readonly WeatherOption[] Known =
    [
        new("heavy_fog", "nebbia fitta", 95),
        new("light_fog", "nebbia leggera", 80),
        new("mid_clear", "sereno velato", 20),
        new("light_clouds", "poche nuvole", 35),
        new("mid_clouds", "nuvoloso", 60),
        new("heavy_clouds", "molto nuvoloso", 85),
        new("clear", "sereno", 0)
    ];

    public static WeatherOption? Match(string weatherId)
    {
        if (string.IsNullOrWhiteSpace(weatherId)) return null;
        var normalized = weatherId.ToLowerInvariant();
        return Known.FirstOrDefault(x => normalized.Contains(x.Key, StringComparison.Ordinal));
    }

    public static string Label(string weatherId) => Match(weatherId)?.Label ?? weatherId;
}

/// <summary>
/// Calibra la difficoltà della sessione successiva osservando solo i referti
/// realmente importati. Non è rubber banding: non tocca punti, posizioni o
/// risultati archiviati, cambia esclusivamente il livello AI del prossimo preset.
/// </summary>
public static class AiCalibration
{
    public const int MinimumOffset = -10;
    public const int MaximumOffset = 6;

    public static int NextOffset(int currentOffset, IEnumerable<(int Position, int FieldSize, bool Dnf)> recentRaces)
    {
        var considered = recentRaces
            .Where(x => !x.Dnf && x.Position > 0 && x.FieldSize > 1)
            .TakeLast(3)
            .ToList();
        if (considered.Count < 2) return Math.Clamp(currentOffset, MinimumOffset, MaximumOffset);
        var averageRatio = considered.Average(x => (x.Position - 1) / (double)(x.FieldSize - 1));
        var delta = averageRatio switch
        {
            <= 0.15 => 2,
            <= 0.30 => 1,
            >= 0.85 => -2,
            >= 0.70 => -1,
            // A meta' gruppo la correzione non serve piu': quella accumulata
            // prima va restituita un punto per volta, altrimenti resta addosso
            // per sempre. Chi dominava nel kart e poi arranca in formula si
            // portava dietro i sei punti presi anni prima.
            _ => currentOffset > 0 ? -1 : currentOffset < 0 ? 1 : 0
        };
        return Math.Clamp(currentOffset + delta, MinimumOffset, MaximumOffset);
    }

    public static string Describe(int offset) => offset == 0
        ? "Livello AI di categoria, nessuna correzione attiva."
        : offset > 0
            ? $"Livello AI aumentato di {offset} punti: i referti reali mostrano un margine costante."
            : $"Livello AI ridotto di {Math.Abs(offset)} punti: i referti reali mostrano un distacco costante.";
}
