using System.Text.Json;

namespace CorsaCareer;

/// <summary>
/// Traduce un <see cref="SessionPlan"/> nel formato Quick Drive letto da Content
/// Manager. Il preset era duplicato fra weekend e test con valori cablati: qui
/// esiste una sola sorgente, verificabile dai test senza aprire la UI.
/// </summary>
public static class ContentManagerPresetBuilder
{
    public static string BuildGrid(string playerCar, IReadOnlyList<string> candidates, int opponents, SessionPlan plan)
    {
        // Content Manager distingue fra la modalità monomarca e la griglia
        // manuale. Dodici copie dello stesso ID in modalità manuale sono
        // ambigue: alcune versioni le riducono a una sola voce e la gara
        // parte con zero avversari. Usiamo quindi `same_car` per il monomarca
        // e manteniamo solo gli ID distinti per una griglia mista.
        var carIds = (candidates.Count > 0 ? candidates : new[] { playerCar })
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (carIds.Count == 0) carIds.Add(playerCar);
        if (!carIds.Contains(playerCar, StringComparer.OrdinalIgnoreCase))
            carIds.Insert(0, playerCar);
        var modeId = carIds.Count == 1 ? "same_car" : "manual";
        var effectiveOpponents = Math.Clamp(opponents, 1, 19);
        return JsonSerializer.Serialize(new
        {
            ModeId = modeId,
            FilterValue = "",
            CarIds = carIds,
            ShuffleCandidates = false,
            VarietyLimitation = 0,
            OpponentsNumber = effectiveOpponents,
            StartingPosition = 1,
            AiLevel = plan.AiLevel,
            AiLevelMin = plan.AiLevelMin,
            AiLevelArrangeRandom = 0.0,
            AiLevelArrangeReverse = false,
            AiLevelArrangePowerRatio = false,
            AiAggression = plan.AiAggression,
            AiAggressionMin = plan.AiAggressionMin,
            AiAggressionArrangeRandom = 0.0,
            AiAggressionArrangeReverse = false
        });
    }

    public static string BuildModeData(SessionPlan plan, string grid) => JsonSerializer.Serialize(new
    {
        PracticeLength = plan.PracticeMinutes,
        QualificationLength = plan.QualifyingMinutes,
        Penalties = true,
        JumpStartPenalty = 0,
        LapsNumber = plan.RaceLaps,
        RaceGridSerialized = grid,
        Version = 2
    });

    public static string BuildPracticeModeData() => JsonSerializer.Serialize(new
    {
        Penalties = true,
        PlayerBallast = 0,
        PlayerRestrictor = 0,
        StartType = "pit"
    });

    // L'open day resta una sessione Practice.  La griglia degli avversari è
    // un dato della modalità Weekend e, se inserita qui, fa rifiutare il
    // preset a Content Manager (che richiede StartType per QuickDrive Practice).
    public static string BuildOpenDayPracticeModeData(SessionPlan plan) => JsonSerializer.Serialize(new
    {
        PracticeLength = plan.PracticeMinutes,
        Penalties = true,
        PlayerBallast = 0,
        PlayerRestrictor = 0,
        StartType = "pit"
    });

    /// <summary>Preset Quick Drive Practice: nessuna griglia e nessuna gara.</summary>
    public static string BuildTest(string carId, string trackId, SessionPlan plan)
    {
        var preset = new
        {
            Mode = "/Pages/Drive/QuickDrive_Practice.xaml",
            ModeData = BuildPracticeModeData(),
            CarId = carId,
            TrackId = trackId,
            WeatherId = plan.WeatherId,
            RealConditions = false,
            Temperature = plan.TemperatureC,
            Time = plan.TimeOfDaySeconds,
            TimeMultipler = 1,
            udt = false,
            dtv = DateTime.Now.ToString("o"),
            tpc = false,
            ico = false,
            wsf = plan.WindSpeedMin,
            wst = plan.WindSpeedMax,
            wd = plan.WindDirection,
            rws = false,
            rwd = false,
            rcTimezones = true,
            rcManTime = false,
            rcManWind = false,
            rcLw = false,
            rte = false,
            rti = false,
            crt = false
        };
        return JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = false });
    }

    /// <summary>
    /// Open day nel kartodromo: resta una sessione Practice, ma porta nel
    /// preset la griglia mista così Content Manager può popolare la pista con
    /// altri concorrenti della categoria invece di lasciare il pilota solo.
    /// </summary>
    public static string BuildOpenDay(string carId, string trackId, SessionPlan plan, IReadOnlyList<string> candidates, int opponents)
    {
        var preset = new
        {
            Mode = "/Pages/Drive/QuickDrive_Practice.xaml",
            ModeData = BuildOpenDayPracticeModeData(plan),
            CarId = carId,
            TrackId = trackId,
            WeatherId = plan.WeatherId,
            RealConditions = false,
            Temperature = plan.TemperatureC,
            Time = plan.TimeOfDaySeconds,
            TimeMultipler = 1,
            udt = false,
            dtv = DateTime.Now.ToString("o"),
            tpc = false,
            ico = false,
            wsf = plan.WindSpeedMin,
            wst = plan.WindSpeedMax,
            wd = plan.WindDirection,
            rws = false,
            rwd = false,
            rcTimezones = true,
            rcManTime = false,
            rcManWind = false,
            rcLw = false,
            rte = false,
            rti = false,
            crt = false
        };
        return JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = false });
    }

    public static string Build(string carId, string trackId, SessionPlan plan, IReadOnlyList<string> candidates, int opponents)
    {
        var grid = BuildGrid(carId, candidates, opponents, plan);
        var preset = new
        {
            Mode = "/Pages/Drive/QuickDrive_Weekend.xaml",
            ModeData = BuildModeData(plan, grid),
            CarId = carId,
            TrackId = trackId,
            WeatherId = plan.WeatherId,
            RealConditions = false,
            Temperature = plan.TemperatureC,
            Time = plan.TimeOfDaySeconds,
            TimeMultipler = 1,
            udt = false,
            dtv = DateTime.Now.ToString("o"),
            tpc = false,
            ico = false,
            wsf = plan.WindSpeedMin,
            wst = plan.WindSpeedMax,
            wd = plan.WindDirection,
            rws = false,
            rwd = false,
            rcTimezones = true,
            rcManTime = false,
            rcManWind = false,
            rcLw = false,
            rte = false,
            rti = false,
            crt = false
        };
        return JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = false });
    }
}
