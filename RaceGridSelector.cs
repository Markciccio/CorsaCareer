namespace CorsaCareer;

public sealed record RaceGridSelection(string[] Candidates, bool UsedMixedFallback)
{
    public int OpponentCount => Math.Clamp(Candidates.Length - 1, 0, 7);
}

public static class RaceGridSelector
{
    public static RaceGridSelection Select(string playerCar, string category, IEnumerable<ContentCarRecord> installedCars)
    {
        var cars = installedCars.Where(x => !string.IsNullOrWhiteSpace(x.Id)).ToList();
        var sameCategory = cars.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToArray();
        if (sameCategory.Length >= 2)
            return new RaceGridSelection(EnsurePlayerFirst(playerCar, sameCategory), false);

        var competition = cars
            .Where(x => !x.Id.Equals(playerCar, StringComparison.OrdinalIgnoreCase)
                && !new[] { "road", "special", "kart" }.Contains(x.Category, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id);
        var mixed = EnsurePlayerFirst(playerCar, competition.Prepend(playerCar).ToArray());
        return new RaceGridSelection(mixed.Length > 0 ? mixed : new[] { playerCar }, mixed.Length > 1);
    }

    private static string[] EnsurePlayerFirst(string playerCar, IEnumerable<string> ids)
    {
        return new[] { playerCar }.Concat(ids.Where(x => !x.Equals(playerCar, StringComparison.OrdinalIgnoreCase)))
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
