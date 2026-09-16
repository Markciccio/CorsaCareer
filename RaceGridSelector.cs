namespace CorsaCareer;

public sealed record RaceGridSelection(string[] Candidates, bool UsedMixedFallback)
{
    // Una gara con due avversari è un test travestito. Content Manager può
    // schierare più istanze della stessa auto (con skin diverse quando ci sono),
    // quindi manteniamo una griglia vera anche nei monomarca piccoli.
    public int OpponentCount => Math.Clamp(Candidates.Length - 1, 0, 19);
}

public static class RaceGridSelector
{
    public static RaceGridSelection Select(string playerCar, string category, IEnumerable<ContentCarRecord> installedCars)
    {
        var cars = installedCars.Where(x => !string.IsNullOrWhiteSpace(x.Id)).ToList();
        var player = cars.FirstOrDefault(x => x.Id.Equals(playerCar, StringComparison.OrdinalIgnoreCase));
        var isKart = category.Contains("kart", StringComparison.OrdinalIgnoreCase);

        // Nel kart la parità viene prima della varietà: peso, potenza e telaio
        // devono essere identici. Non mischiare 4T, 2T e kart con cambio.
        if (isKart)
            return new RaceGridSelection(RepeatToGrid(playerCar, new[] { playerCar }), false);

        var sameCategory = cars.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToArray();
        if (sameCategory.Length >= 2)
            return new RaceGridSelection(RepeatToGrid(playerCar, EnsurePlayerFirst(playerCar, sameCategory)), false);

        var competition = cars
            .Where(x => !x.Id.Equals(playerCar, StringComparison.OrdinalIgnoreCase)
                && !new[] { "road", "special", "kart" }.Contains(x.Category, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id);
        var mixed = EnsurePlayerFirst(playerCar, competition.Prepend(playerCar).ToArray());
        return new RaceGridSelection(RepeatToGrid(playerCar, mixed.Length > 0 ? mixed : new[] { playerCar }), mixed.Length > 1);
    }

    private const int TargetCarsOnGrid = 12;

    private static string[] RepeatToGrid(string playerCar, IReadOnlyList<string> pool)
    {
        var valid = pool.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (valid.Length == 0) valid = new[] { playerCar };
        var grid = new List<string> { playerCar };
        for (var i = 1; i < TargetCarsOnGrid; i++)
            grid.Add(valid[(i - 1) % valid.Length]);
        return grid.ToArray();
    }

    private static string[] EnsurePlayerFirst(string playerCar, IEnumerable<string> ids)
    {
        return new[] { playerCar }.Concat(ids.Where(x => !x.Equals(playerCar, StringComparison.OrdinalIgnoreCase)))
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
