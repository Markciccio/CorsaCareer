namespace CorsaCareer;

public sealed record RaceGridSelection(string[] Candidates, bool UsedMixedFallback)
{
    public int DistinctCandidateCount => Candidates.Distinct(StringComparer.OrdinalIgnoreCase).Count();

    // Content Manager può schierare più istanze della stessa auto (con skin
    // diverse quando ci sono), quindi una sola auto distinta è una griglia
    // monomarca valida. Zero auto candidate, invece, non è mai una gara.
    public int OpponentCount => DistinctCandidateCount > 0 ? 7 : 0;
}

public static class RaceGridSelector
{
    public static RaceGridSelection Select(string playerCar, string category, IEnumerable<ContentCarRecord> installedCars)
    {
        var cars = installedCars.Where(x => !string.IsNullOrWhiteSpace(x.Id)).ToList();
        var player = cars.FirstOrDefault(x => x.Id.Equals(playerCar, StringComparison.OrdinalIgnoreCase));
        var isKart = category.Contains("kart", StringComparison.OrdinalIgnoreCase);

        // Nel kart la parità viene prima della varietà: prima cerchiamo lo
        // stesso gradino, poi un altro kart con rapporto peso/potenza vicino.
        // La vecchia scelta restituiva dodici copie dell'auto del pilota, ma
        // Content Manager le riduceva a una sola voce e avviava una gara senza
        // avversari.
        if (isKart)
        {
            var playerRung = CareerLadder.ForCar(player?.Category ?? category, player?.PowerHp ?? 0, player?.MassKg ?? 0).Id;
            var sameRung = cars
                .Where(x => !x.Id.Equals(playerCar, StringComparison.OrdinalIgnoreCase)
                    && CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Id.Equals(playerRung, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => RatioDistance(player, x))
                .Select(x => x.Id)
                .ToArray();
            if (sameRung.Length > 0)
                return new RaceGridSelection(RepeatToGrid(playerCar, EnsurePlayerFirst(playerCar, sameRung)), false);

            var nearby = cars
                .Where(x => !x.Id.Equals(playerCar, StringComparison.OrdinalIgnoreCase)
                    && x.Category.Contains("kart", StringComparison.OrdinalIgnoreCase)
                    && RatioDistance(player, x) <= 0.30)
                .OrderBy(x => RatioDistance(player, x))
                .Select(x => x.Id)
                .ToArray();
            if (nearby.Length > 0)
                return new RaceGridSelection(RepeatToGrid(playerCar, EnsurePlayerFirst(playerCar, nearby)), true);

            // Nessun kart installato ha un rapporto vicino: non degradare a
            // un DAP o a una Lada solo per riempire la griglia. Un monomarca
            // con la vettura del pilota è sempre più corretto e Content
            // Manager lo gestisce con ModeId=same_car.
            return new RaceGridSelection(RepeatToGrid(playerCar, new[] { playerCar }), false);
        }

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

    private static double RatioDistance(ContentCarRecord? left, ContentCarRecord right)
    {
        if (left == null || left.PowerHp <= 0 || left.MassKg <= 0 || right.PowerHp <= 0 || right.MassKg <= 0)
            return 1.0;
        var a = left.PowerHp / (double)left.MassKg;
        var b = right.PowerHp / (double)right.MassKg;
        return Math.Abs(a - b) / Math.Max(a, b);
    }
}
