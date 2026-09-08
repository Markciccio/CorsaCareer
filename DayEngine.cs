namespace CorsaCareer1991;

/// <summary>Che cosa è successo svolgendo un'attività.</summary>
public sealed class DayReport
{
    public DayActivity Activity { get; init; } = new();
    public DayOutcome Outcome { get; init; } = new();
    /// <summary>Le variazioni applicate davvero, dopo i limiti.</summary>
    public List<string> Applied { get; init; } = [];
    public bool Refused { get; init; }
    public string Refusal { get; init; } = "";
}

/// <summary>
/// Il motore della giornata: applica un'attività allo stato della carriera e fa
/// passare il giorno.
///
/// Tiene separate tre cose che prima erano confuse: la <em>popolarità</em> è il
/// pubblico, la <em>reputazione sportiva</em> è come ti vede il paddock, la
/// <em>fiducia del team</em> è il rapporto con una squadra precisa. Un incontro
/// con i tifosi muove la prima, una vittoria la seconda, un buon test la terza.
/// </summary>
public static class DayEngine
{
    /// <summary>
    /// Svolge un'attività. Se non è possibile — ore, soldi, già fatta — non
    /// cambia niente e dice perché.
    /// </summary>
    public static DayReport Perform(CareerState career, DayPlan day, DayActivity activity)
    {
        if (!DriverDay.CanDo(day, activity, career.Cash, out var refusal))
            return new DayReport { Activity = activity, Refused = true, Refusal = refusal };

        // Il costo si paga prima: se non si può pagare, l'attività non avviene.
        if (activity.Cost > 0)
        {
            var payment = CareerWallet.TryPay(career, activity.Cost, activity.Name);
            if (!payment.Paid)
                return new DayReport { Activity = activity, Refused = true, Refusal = payment.Refusal };
        }

        var seed = DriverDay.SeedFor(career.Driver ?? "", day.Date, activity.Id);
        var outcome = DriverDay.Resolve(activity, career, seed);
        var applied = new List<string>();

        var profile = career.ReputationProfile ??= new ReputationProfile();
        foreach (var effect in outcome.Effects)
        {
            switch (effect.Kind)
            {
                case DayEffectKind.Fitness:
                    var fitnessBefore = career.Fitness;
                    career.Fitness = Math.Clamp(career.Fitness + effect.Amount, 0, DriverDay.MaxFitness);
                    if (career.Fitness != fitnessBefore)
                        applied.Add($"forma {Signed(career.Fitness - fitnessBefore)} (ora {career.Fitness})");
                    break;

                case DayEffectKind.Fatigue:
                    var fatigueBefore = career.Fatigue;
                    career.Fatigue = Math.Clamp(career.Fatigue + effect.Amount, 0, DriverDay.MaxFatigue);
                    if (career.Fatigue != fatigueBefore)
                        applied.Add($"stanchezza {Signed(career.Fatigue - fatigueBefore)} (ora {career.Fatigue})");
                    break;

                case DayEffectKind.Popularity:
                    var popBefore = profile.PublicPopularity;
                    profile.PublicPopularity = Math.Clamp(profile.PublicPopularity + effect.Amount, 0, 100);
                    if (profile.PublicPopularity != popBefore)
                        applied.Add($"livello influencer {Signed(profile.PublicPopularity - popBefore)} (ora {profile.PublicPopularity})");
                    break;

                case DayEffectKind.SportingReputation:
                    var repBefore = profile.SportingPrestige;
                    profile.SportingPrestige = Math.Clamp(profile.SportingPrestige + effect.Amount, 0, 100);
                    if (profile.SportingPrestige != repBefore)
                        applied.Add($"reputazione sportiva {Signed(profile.SportingPrestige - repBefore)} (ora {profile.SportingPrestige})");
                    break;

                case DayEffectKind.Money:
                    if (effect.Amount >= 0)
                    {
                        career.Cash += effect.Amount;
                        if (activity.Actor == DayActor.Agent) career.SponsorMoney += effect.Amount;
                        applied.Add($"€ {Signed(effect.Amount)}");
                    }
                    // Le uscite sono già state pagate col costo dell'attività:
                    // sottrarle di nuovo qui le farebbe pagare due volte.
                    break;
            }
        }

        // Le schermate storiche leggono ancora i campi aggregati: non devono
        // mostrare valori diversi dalla nuova scheda forma/reputazione.
        profile.SyncLegacyFields(career);

        // Le ore si consumano comunque: anche un pomeriggio andato male è un
        // pomeriggio speso.
        if (activity.Actor == DayActor.Driver) day.DriverHoursLeft -= activity.Hours;
        else day.AgentHoursLeft -= activity.Hours;
        day.Done.Add(activity.Id);

        // Anche le attività della giornata moderna devono lasciare una traccia
        // nell'archivio: altrimenti dopo il salvataggio sembrano non essere mai
        // avvenute e il giornale non può raccontarle.
        var effects = outcome.Effects;
        career.ActivityHistory.Add(new ActivityRecord
        {
            Season = career.Season,
            Round = career.Round + 1,
            StoryDate = day.Date,
            ActivityId = activity.Id,
            Name = activity.Name,
            Success = !outcome.IsSetback,
            Story = outcome.Line,
            Days = 0,
            Reputation = effects.Where(x => x.Kind == DayEffectKind.SportingReputation).Sum(x => x.Amount),
            Fanbase = effects.Where(x => x.Kind == DayEffectKind.Popularity).Sum(x => x.Amount),
            TeamRelation = 0,
            SponsorRelation = 0,
            Fatigue = effects.Where(x => x.Kind == DayEffectKind.Fatigue).Sum(x => x.Amount),
            Money = effects.Where(x => x.Kind == DayEffectKind.Money).Sum(x => x.Amount) - Math.Max(0, activity.Cost)
        });
        career.News.Add($"{activity.Name}: {outcome.Line}");

        return new DayReport { Activity = activity, Outcome = outcome, Applied = applied };
    }

    /// <summary>
    /// Passa al giorno successivo. La notte recupera un po' di stanchezza, ma
    /// non azzera: chi si consuma per una settimana arriva consumato alla gara.
    /// </summary>
    public static DayPlan Advance(CareerState career, DayPlan today)
    {
        career.Fatigue = Math.Clamp(career.Fatigue - NightRecovery, 0, DriverDay.MaxFatigue);
        career.StoryDate = today.Date.AddDays(1);
        if (career.DaysUntilNextRound > 0) career.DaysUntilNextRound--;

        var next = new DayPlan
        {
            Date = career.StoryDate,
            DriverHoursLeft = DriverDay.DriverHours,
            AgentHoursLeft = DriverDay.AgentHours
        };
        career.Today = next;
        return next;
    }

    /// <summary>Stanchezza recuperata dormendo. Non basta a compensare una giornata piena.</summary>
    public const int NightRecovery = 12;

    /// <summary>
    /// Le attività che oggi si possono ancora fare, con il motivo per cui una
    /// non è disponibile.
    /// </summary>
    public static List<(DayActivity Activity, bool Available, string Reason)> Available(
        CareerState career, DayPlan day, DayActor actor)
    {
        var catalogue = actor == DayActor.Driver ? DayActivityCatalog.ForDriver() : DayActivityCatalog.ForAgent();
        return catalogue
            .Select(x => (x, DriverDay.CanDo(day, x, career.Cash, out var reason), reason))
            .ToList();
    }

    private static string Signed(int value) => value >= 0 ? $"+{value}" : value.ToString();
}
