namespace CorsaCareer;

/// <summary>
/// Il piccolo pacchetto di contenuti che rende la carriera riproducibile.
/// Non impone un nome di mod: il giocatore può installare la vettura che
/// preferisce oppure dichiarare un equivalente già presente dalla mappa
/// contenuti. Impone però il ruolo sportivo e un numero minimo di piste.
/// </summary>
public sealed record CareerContentRequirement(
    string Id,
    string Label,
    string[] RungIds,
    int MinimumCars = 0,
    string TrackGroup = "",
    int MinimumTracks = 0);

public sealed record CareerContentRequirementStatus(
    CareerContentRequirement Requirement,
    int FoundCars,
    int FoundTracks)
{
    public bool Satisfied => FoundCars >= Requirement.MinimumCars
        && FoundTracks >= Requirement.MinimumTracks;

    public string Progress => Requirement.TrackGroup.Length > 0
        ? $"{FoundTracks}/{Requirement.MinimumTracks} circuiti"
        : $"{FoundCars}/{Requirement.MinimumCars} auto";
}

public sealed class CareerContentReadiness
{
    public IReadOnlyList<CareerContentRequirementStatus> Requirements { get; }
    public bool Ready => Requirements.All(x => x.Satisfied);
    public IReadOnlyList<CareerContentRequirementStatus> Missing => Requirements.Where(x => !x.Satisfied).ToList();

    public CareerContentReadiness(IReadOnlyList<CareerContentRequirementStatus> requirements) => Requirements = requirements;

    public string ShortMessage()
    {
        if (Ready) return "PACCHETTO BASE PRONTO · puoi scegliere una delle due carriere.";
        var missing = string.Join(" · ", Missing.Select(x => $"{x.Requirement.Label} ({x.Progress})"));
        return $"PACCHETTO BASE INCOMPLETO · {missing}";
    }
}

public static class CareerContentRequirements
{
    // Lo scheletro comune alle due direzioni. F2/Super Formula, F1,
    // prototipi e hypercar restano estensioni: se non sono installati la
    // carriera prolunga la categoria precedente invece di rompersi.
    public static IReadOnlyList<CareerContentRequirement> BasePack { get; } =
    [
        // Un solo kart è sufficiente per il monomarca: il preset usa la
        // modalità `same_car` e Content Manager genera gli avversari con
        // skin/istanze diverse. Una seconda auto dello stesso gradino resta
        // consigliata, ma non deve impedire il primo avvio.
        new("kart-4t", "Kart a quattro tempi", [CareerLadder.FourStroke], MinimumCars: 1),
        new("kart-2t", "Kart a due tempi / DAP", [CareerLadder.TwoStroke], MinimumCars: 1),
        new("kart-125", "Kart 125 con cambio", [CareerLadder.Shifter], MinimumCars: 1),
        new("formula-entry", "Formula d'ingresso", ["formula-4"], MinimumCars: 1),
        new("formula-national", "Formula nazionale / F3", ["formula-3"], MinimumCars: 1),
        new("road-rookie", "Utilitaria / track day", [CareerLadder.RoadRookie], MinimumCars: 1),
        new("touring-entry", "Trofeo club o turismo regionale", [CareerLadder.ClubCup, CareerLadder.RegionalTouring], MinimumCars: 1),
        new("gt4", "GT4 / trofeo GT", ["gt4"], MinimumCars: 1),
        new("gt3", "GT3 / turismo internazionale", ["gt3", "tcr"], MinimumCars: 1),
        new("kart-tracks", "Piste kart giapponesi", [], TrackGroup: "kart", MinimumTracks: 3),
        new("road-tracks", "Circuiti permanenti internazionali", [], TrackGroup: "permanent", MinimumTracks: 5)
    ];

    public static CareerContentReadiness Evaluate(ContentIndexRecord index)
    {
        var cars = index.Cars.Where(ContentCategoryRules.IsRaceable).ToList();
        var statuses = BasePack.Select(requirement =>
        {
            var foundCars = requirement.RungIds.Length == 0
                ? 0
                : cars.Count(car => requirement.RungIds.Contains(
                    CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Id,
                    StringComparer.OrdinalIgnoreCase));
            var foundTracks = requirement.TrackGroup.Length == 0
                ? 0
                : index.Tracks.Count(track => TrackBelongsToGroup(track, requirement.TrackGroup));
            return new CareerContentRequirementStatus(requirement, foundCars, foundTracks);
        }).ToList();
        return new CareerContentReadiness(statuses);
    }

    public static bool TrackBelongsToGroup(ContentTrackRecord track, string group)
    {
        var text = $"{track.Id} {track.Name} {track.Layout}".ToLowerInvariant();
        if (group.Equals("kart", StringComparison.OrdinalIgnoreCase))
        {
            if (track.Category.Equals("kartodromo", StringComparison.OrdinalIgnoreCase)) return true;
            return new[] { "kart", "kunitomi", "tokushima", "mobara", "narashino", "shonan", "akagi", "kizuna" }
                .Any(text.Contains);
        }
        if (!group.Equals("permanent", StringComparison.OrdinalIgnoreCase)) return false;
        if (track.Category.Equals("kartodromo", StringComparison.OrdinalIgnoreCase)) return false;
        // Una pista breve è quasi sempre un layout kart o club.
        if (track.LengthMeters is > 0 and <= 1600) return false;
        return true;
    }

    public static string InstallationGuidance(CareerContentReadiness readiness)
    {
        if (readiness.Ready)
            return "Sono presenti tutti i ruoli minimi: 4T, DAP/2T, 125, Formula d'ingresso, F3, utilitaria, turismo, GT4, GT3 e una rotazione di piste.";
        return "Installa le voci mancanti in Assetto Corsa, oppure usa nella mappa il pulsante «USA UN'AUTO GIA INSTALLATA COME EQUIVALENTE». Poi premi «RISCANSIONA».";
    }
}
