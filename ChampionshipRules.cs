namespace CorsaCareer1991;

/// <summary>
/// Regolamento punti di un campionato. Prima esisteva un solo sistema cablato
/// (10-6-4-3-2-1 sui primi sei) usato per ogni categoria: qui il sistema dipende
/// dalla disciplina, come nei campionati reali.
/// </summary>
public sealed record ChampionshipRules(string Name, int[] PointsTable, bool PolePoint, bool FastestLapPoint)
{
    public int ScoringPositions => PointsTable.Length;
}

public static class ChampionshipRegulations
{
    // Sistema storico: è quello autentico per una carriera ambientata a inizio
    // anni Novanta e resta usato dalle categorie storiche.
    private static readonly int[] Historic = [10, 6, 4, 3, 2, 1];
    // Sistema moderno FIA, usato da formule, GT e prototipi contemporanei.
    private static readonly int[] Modern = [25, 18, 15, 12, 10, 8, 6, 4, 2, 1];
    // Il karting premia una coda più lunga di classificati.
    private static readonly int[] Karting = [25, 20, 16, 13, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
    // Turismo e monomarca: punteggi più compressi, molti piloti a punti.
    private static readonly int[] Touring = [20, 15, 12, 10, 8, 6, 4, 3, 2, 1];

    public static ChampionshipRules ForCategory(string category, string tier)
    {
        var normalized = (category ?? "").Trim().ToLowerInvariant();
        return normalized switch
        {
            "historic" => new ChampionshipRules("Regolamento storico", Historic, false, false),
            "kart" => new ChampionshipRules("Regolamento karting", Karting, false, false),
            "touring" or "tcr" or "cup" => new ChampionshipRules("Regolamento turismo", Touring, false, false),
            "formula" or "formula 2" or "formula 3" or "formula 4" or "formula junior" =>
                new ChampionshipRules("Regolamento formule", Modern, true, normalized == "formula"),
            "gt1" or "gt2" or "gt3" or "gt4" or "lmp" or "hypercar" or "prototype" =>
                new ChampionshipRules("Regolamento GT e prototipi", Modern, false, false),
            _ => tier switch
            {
                "Formula / top tier" => new ChampionshipRules("Regolamento top", Modern, true, true),
                "Categoria avanzata" => new ChampionshipRules("Regolamento avanzato", Modern, false, false),
                "Categoria regionale" => new ChampionshipRules("Regolamento regionale", Touring, false, false),
                _ => new ChampionshipRules("Regolamento base", Historic, false, false)
            }
        };
    }

    /// <summary>Punti di classifica per la posizione d'arrivo. Un ritiro non produce punti.</summary>
    public static int PointsForPosition(ChampionshipRules rules, int position, bool dnf)
    {
        if (dnf || position <= 0) return 0;
        return position <= rules.PointsTable.Length ? rules.PointsTable[position - 1] : 0;
    }

    /// <summary>
    /// Punti bonus previsti dal regolamento: pole e giro veloce. Il giro veloce
    /// segue la regola moderna e vale solo per chi finisce nelle posizioni a punti.
    /// </summary>
    public static int BonusPoints(ChampionshipRules rules, int position, int qualifyingPosition, bool hasFastestLap, bool dnf)
    {
        if (dnf || position <= 0) return 0;
        var bonus = 0;
        if (rules.PolePoint && qualifyingPosition == 1) bonus++;
        if (rules.FastestLapPoint && hasFastestLap && position <= rules.PointsTable.Length) bonus++;
        return bonus;
    }

    public static int TotalPoints(ChampionshipRules rules, int position, int qualifyingPosition, bool hasFastestLap, bool dnf) =>
        PointsForPosition(rules, position, dnf) + BonusPoints(rules, position, qualifyingPosition, hasFastestLap, dnf);

    /// <summary>
    /// Chi ha segnato il giro veloce nel referto. Restituisce una stringa vuota se
    /// il file non contiene tempi utilizzabili: il punto non viene assegnato a caso.
    /// </summary>
    public static string FastestLapDriver(IEnumerable<ImportedDriverResult> classification)
    {
        var best = classification.Where(x => x.BestLapMilliseconds > 0).OrderBy(x => x.BestLapMilliseconds).FirstOrDefault();
        return best?.Name ?? "";
    }

    public static string Describe(ChampionshipRules rules)
    {
        var bonus = new List<string>();
        if (rules.PolePoint) bonus.Add("1 punto per la pole");
        if (rules.FastestLapPoint) bonus.Add("1 punto per il giro veloce nelle posizioni a punti");
        var table = string.Join("-", rules.PointsTable);
        return bonus.Count == 0 ? $"{rules.Name}: {table}" : $"{rules.Name}: {table} · {string.Join(" · ", bonus)}";
    }
}
