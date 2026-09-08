namespace CorsaCareer;

/// <summary>Cosa si decide davanti a una gara.</summary>
public enum RaceDecision
{
    /// <summary>Si corre: la quota si paga e la sessione si prepara.</summary>
    Race,
    /// <summary>Si salta: i soldi restano, ma il paddock se ne accorge.</summary>
    Skip,
    /// <summary>Nessuna decisione: si torna indietro senza conseguenze.</summary>
    Postpone
}

/// <summary>
/// Le conseguenze di saltare una gara.
///
/// Saltare deve essere una scelta vera, non una rinuncia: a volte è la decisione
/// intelligente — se fra cinque giorni c'è un test che conta e la cassa non
/// regge entrambi. Ma non può essere gratis, o si salterebbe sempre.
///
/// Il costo è di reputazione, non di denaro: chi non corre viene dimenticato.
/// </summary>
public sealed class SkipCost
{
    public int Popularity { get; init; }
    public int SportingReputation { get; init; }
    public int TeamTrust { get; init; }
    /// <summary>Come viene raccontato.</summary>
    public string Line { get; init; } = "";
    /// <summary>Le voci del costo, una per riga.</summary>
    public List<string> Details { get; init; } = [];
}

/// <summary>
/// La decisione davanti a una gara.
///
/// Il punto è che avere i soldi non basta a rendere ovvia la scelta. Con 650 € in
/// cassa, una gara da 250 € oggi e un test da 500 € fra cinque giorni, correre
/// oggi significa rinunciare al test. È il tipo di decisione che rende una
/// carriera una carriera.
/// </summary>
public static class RaceChoice
{
    /// <summary>
    /// Cosa costa saltare questa gara. Più il pilota è agli inizi, più pesa:
    /// chi non ha ancora un nome non può permettersi di sparire.
    /// </summary>
    public static SkipCost CostOfSkipping(CareerState career, ScheduledEvent race)
    {
        var profile = career.ReputationProfile ?? new ReputationProfile();
        var details = new List<string>();

        // Chi è sconosciuto perde di più: sparire quando nessuno ti conosce
        // significa tornare al punto di partenza.
        var popularity = profile.PublicPopularity < 20 ? -4 : -2;
        var reputation = career.Races < 3 ? -5 : -3;
        details.Add($"livello influencer {popularity}");
        details.Add($"reputazione sportiva {reputation}");

        // Se c'è un team che ha proposto la gara, se ne accorge.
        var teamTrust = 0;
        if (!string.IsNullOrWhiteSpace(race.ProposedBy))
        {
            teamTrust = -6;
            details.Add($"fiducia di {race.ProposedBy} {teamTrust}");
        }

        return new SkipCost
        {
            Popularity = popularity,
            SportingReputation = reputation,
            TeamTrust = teamTrust,
            Details = details,
            Line = string.IsNullOrWhiteSpace(race.ProposedBy)
                ? "Il weekend passa senza di te. Nessuno se ne lamenta ad alta voce, e questo è il problema."
                : $"{race.ProposedBy} aveva tenuto il posto. Ti diranno che capiscono, e sarà vero solo in parte."
        };
    }

    /// <summary>Applica il costo di aver saltato.</summary>
    public static void ApplySkip(CareerState career, ScheduledEvent race)
    {
        var cost = CostOfSkipping(career, race);
        var profile = career.ReputationProfile ??= new ReputationProfile();
        profile.PublicPopularity = Math.Clamp(profile.PublicPopularity + cost.Popularity, 0, 100);
        profile.SportingPrestige = Math.Clamp(profile.SportingPrestige + cost.SportingReputation, 0, 100);
        if (cost.TeamTrust != 0)
            profile.TeamTrust = Math.Clamp(profile.TeamTrust + cost.TeamTrust, 0, 100);

        race.Status = CareerScheduler.StatusSkipped;
        career.Results.Add($"Gara saltata: {race.TrackName}");
    }

    /// <summary>
    /// Il ragionamento da mostrare prima di decidere: cosa resta in cassa dopo, e
    /// se questo compromette gli impegni già in calendario.
    ///
    /// È la parte che rende la scelta interessante: senza, «ho i soldi» sarebbe
    /// sempre la risposta.
    /// </summary>
    public static List<string> Considerations(CareerState career, ScheduledEvent race, int entryFee)
    {
        var notes = new List<string>();
        var after = career.Cash - entryFee;
        notes.Add($"Dopo l'iscrizione resterebbero € {after:N0}.");

        // Gli impegni successivi che quella spesa metterebbe a rischio.
        var upcoming = (career.Schedule ?? [])
            .Where(x => x.IsPlanned && x.Date > race.Date && x.EntryFee > 0 && !x.EntryFeePaid)
            .OrderBy(x => x.Date)
            .Take(2)
            .ToList();

        foreach (var next in upcoming)
        {
            var days = Math.Max(0, (next.Date.Date - race.Date.Date).Days);
            if (next.EntryFee > after)
                notes.Add($"Fra {days} giorni c'è {next.TrackName} a € {next.EntryFee:N0}: correndo oggi non te lo potresti permettere.");
            else
                notes.Add($"Fra {days} giorni c'è {next.TrackName} a € {next.EntryFee:N0}: resterebbe sostenibile.");
        }

        if (upcoming.Count == 0)
            notes.Add("Non ci sono altri impegni già fissati con una quota da pagare.");

        // La condizione fisica: correre stanchi rende meno.
        if (career.Fatigue >= DriverDay.TiredThreshold)
            notes.Add($"Sei {DriverDay.ConditionLabel(career.Fitness, career.Fatigue)}: il ritmo calerebbe nella seconda metà.");

        return notes;
    }
}
