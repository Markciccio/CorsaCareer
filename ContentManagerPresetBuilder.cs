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
        var carIds = candidates.Count > 0 ? candidates.ToList() : new List<string> { playerCar };
        return JsonSerializer.Serialize(new
        {
            ModeId = "manual",
            FilterValue = "",
            CarIds = carIds,
            ShuffleCandidates = false,
            VarietyLimitation = 0,
            OpponentsNumber = Math.Clamp(opponents, 0, Math.Max(0, carIds.Count - 1)),
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
