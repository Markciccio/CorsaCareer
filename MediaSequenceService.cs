namespace CorsaCareer;

/// <summary>
/// Costruisce le rassegne del Media Center senza dipendere dalla UI.
/// L'ordinamento consigliato segue la storia; le altre rassegne mostrano il
/// servizio più recente per primo, come un portale di notizie.
/// </summary>
public static class MediaSequenceService
{
    public static IReadOnlyList<CareerEventRecord> Build(IEnumerable<CareerEventRecord> source, string mode)
        => Build(source, mode, "Tutti i media");

    public static IReadOnlyList<CareerEventRecord> Build(IEnumerable<CareerEventRecord> source, string mode, string filter)
    {
        var events = ApplyFilter(source ?? Enumerable.Empty<CareerEventRecord>(), filter);
        IEnumerable<CareerEventRecord> query = mode switch
        {
            "Weekend e gare" => events.Where(x => IsRace(x.Type) || x.Type.Equals("SCREENSHOT_CAPTURED", StringComparison.OrdinalIgnoreCase)),
            "Test e sviluppo" => events.Where(x => x.Type.Contains("TEST", StringComparison.OrdinalIgnoreCase) || x.Type.Equals("SCREENSHOT_CAPTURED", StringComparison.OrdinalIgnoreCase)),
            "Mercato e carriera" => events.Where(x => x.Type.Contains("CONTRACT", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("TEAM", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("PROMOTION", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("MARKET", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("PROFILE", StringComparison.OrdinalIgnoreCase) || x.Type.Equals("PADDOCK_ROSTER", StringComparison.OrdinalIgnoreCase)),
            "Archivio (archiviati)" => events.Where(x => x.Archived),
            "Timeline completa" => events,
            _ => events.Where(x => !x.Archived).OrderByDescending(x => x.Importance).ThenByDescending(x => x.DateUtc)
        };
        if (!mode.Equals("Archivio (archiviati)", StringComparison.OrdinalIgnoreCase) && !mode.Equals("Timeline completa", StringComparison.OrdinalIgnoreCase)) query = query.Where(x => !x.Archived);
        return mode.Equals("Rassegna consigliata", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.StoryDate == default ? x.DateUtc : x.StoryDate.ToUniversalTime()).ThenBy(x => x.DateUtc).ThenByDescending(x => x.Importance).ToList()
            : query.OrderByDescending(x => x.DateUtc).ToList();
    }

    public static IReadOnlyList<string> Filters { get; } = ["Tutti i media", "Foto e screenshot", "Gare", "Test", "Mercato", "Traguardi"];

    private static IEnumerable<CareerEventRecord> ApplyFilter(IEnumerable<CareerEventRecord> source, string filter)
    {
        return filter switch
        {
            "Foto e screenshot" => source.Where(x => !string.IsNullOrWhiteSpace(x.PhotoPath) || x.Type.Contains("PHOTO", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("SCREENSHOT", StringComparison.OrdinalIgnoreCase)),
            "Gare" => source.Where(x => IsRace(x.Type)),
            "Test" => source.Where(x => x.Type.Contains("TEST", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("TRACK", StringComparison.OrdinalIgnoreCase)),
            "Mercato" => source.Where(x => x.Type.Contains("CONTRACT", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("TEAM", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("MARKET", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("SPONSOR", StringComparison.OrdinalIgnoreCase)),
            "Traguardi" => source.Where(x => x.Type.Contains("FIRST_", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("VICTORY", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("PODIUM", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("MILESTONE", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("PROMOTION", StringComparison.OrdinalIgnoreCase) || x.Type.Contains("SEASON_AWARD", StringComparison.OrdinalIgnoreCase)),
            _ => source
        };
    }

    public static string Description(string mode, int count) => mode switch
    {
        "Rassegna consigliata" => count == 0 ? "La redazione attende il primo fatto verificato." : "La storia del paddock, montata in ordine: apri il primo servizio e usa Avanti per seguire il filo.",
        "Weekend e gare" => "Briefing, sessione reale, risultato e conseguenze del weekend.",
        "Test e sviluppo" => "Il dietro le quinte tecnico: prove, vettura e immagini realmente archiviate.",
        "Mercato e carriera" => "Contratti, sponsor, compagni e scelte che cambiano il futuro del pilota.",
        "Archivio (archiviati)" => "Servizi messi da parte: non sono cancellati e possono essere riascoltati.",
        _ => "Timeline completa della carriera, con indice e lettura singola dei servizi."
    };

    private static bool IsRace(string type) => type.Equals("RACE_FINISHED", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_OR_NEXT_VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_PODIUM", StringComparison.OrdinalIgnoreCase)
        || type.Equals("PODIUM", StringComparison.OrdinalIgnoreCase)
        || type.Equals("MILESTONE_RACE", StringComparison.OrdinalIgnoreCase)
        || type.Equals("RETIREMENT", StringComparison.OrdinalIgnoreCase)
        || type.Equals("RIVALRY_DUEL", StringComparison.OrdinalIgnoreCase)
        || type.Equals("RIVALRY_ESCALATION", StringComparison.OrdinalIgnoreCase)
        || type.Equals("TEAMMATE_DUEL", StringComparison.OrdinalIgnoreCase);
}
