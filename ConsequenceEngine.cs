namespace CorsaCareer;

/// <summary>Una conseguenza applicata allo stato, con la sua motivazione.</summary>
public sealed record Consequence(string Kind, string Text, int Amount = 0);

/// <summary>Esito completo di un evento, già applicato allo stato della carriera.</summary>
public sealed class ConsequenceReport
{
    public List<Consequence> Applied { get; set; } = [];
    public ReputationShift Reputation { get; set; } = new();
    public int CashDelta { get; set; }
    public List<ConditionalPromise> Honoured { get; set; } = [];
    public List<ConditionalPromise> Failed { get; set; } = [];
    public List<string> ExpiredOpportunities { get; set; } = [];
    public bool EnteredFinancialTrouble { get; set; }

    public string Explain() => Applied.Count == 0
        ? "Nessuna conseguenza registrata."
        : string.Join("\n", Applied.Select(x => "• " + x.Text));
}

/// <summary>
/// Motore delle conseguenze: l'unico punto in cui un esito modifica lo stato.
///
/// Regola del progetto: la narrazione è la rappresentazione dello stato, non una
/// storia scollegata. Se un articolo dice che uno sponsor versa 50.000 €, quei
/// 50.000 € passano da qui; se dice che la fiducia del team è calata, il valore
/// scende qui. Prima queste modifiche erano sparse dentro RecordInternal e non
/// esisteva un posto in cui verificarle.
/// </summary>
public static class ConsequenceEngine
{
    /// <summary>
    /// Applica le conseguenze di un risultato di gara reale: reputazione su tutte
    /// le dimensioni, promesse condizionali, scadenza delle opportunità e stato
    /// economico.
    /// </summary>
    public static ConsequenceReport ApplyRaceResult(CareerState career, RaceHistoryEntry race, int carCompetitiveness)
    {
        var report = new ConsequenceReport();
        career.ReputationProfile ??= new ReputationProfile();
        var fieldSize = Math.Max(1, race.Classification.Count);

        var shift = ReputationDynamics.ForRace(new ReputationDynamics.Input
        {
            Position = race.Position,
            FieldSize = fieldSize,
            Dnf = race.Dnf,
            Abandoned = race.Abandoned,
            QualifyingPosition = race.QualificationPosition,
            StartingPosition = race.StartingPosition,
            TeammatePosition = race.TeammatePosition,
            ExpectedPosition = ReputationEngine.ExpectedPosition(career.SeatPrestige, fieldSize),
            Tier = career.Tier,
            CarCompetitiveness = carCompetitiveness,
            Damage = race.Damage,
            PenaltySeconds = race.PenaltySeconds,
            PointlessStreak = CountPointlessStreak(career, race)
        }, career.ReputationProfile);

        career.ReputationProfile = ReputationDynamics.Apply(career.ReputationProfile, shift);
        report.Reputation = shift;
        if (shift.Deltas.Count > 0) report.Applied.Add(new Consequence("reputazione", $"Reputazione: {shift.Summary()}"));

        // Il valore sintetico resta allineato: è quello usato dal resto del codice.
        career.Reputation = career.ReputationProfile.Overall;
        career.TeamRelation = career.ReputationProfile.TeamTrust;
        career.SponsorRelation = career.ReputationProfile.SponsorAppeal;

        VerifyPromises(career, race, report);
        ExpireOpportunities(career, report);

        if (CareerFinances.InTrouble(career.Cash))
        {
            report.EnteredFinancialTrouble = true;
            report.Applied.Add(new Consequence("economia",
                $"Patrimonio sotto la soglia di sopravvivenza (€ {career.Cash:N0}): restano accessibili solo le opportunità economiche."));
        }
        return report;
    }

    /// <summary>Conseguenze di una prova: incidono su fiducia e affidabilità, non sui punti.</summary>
    public static ConsequenceReport ApplyTestResult(CareerState career, bool passed, bool close, bool noTimedLap)
    {
        var report = new ConsequenceReport();
        career.ReputationProfile ??= new ReputationProfile();
        var shift = ReputationDynamics.ForTest(passed, close, noTimedLap);
        career.ReputationProfile = ReputationDynamics.Apply(career.ReputationProfile, shift);
        report.Reputation = shift;
        career.Reputation = career.ReputationProfile.Overall;
        career.TeamRelation = career.ReputationProfile.TeamTrust;
        career.SponsorRelation = career.ReputationProfile.SponsorAppeal;
        if (shift.Deltas.Count > 0) report.Applied.Add(new Consequence("reputazione", $"Reputazione: {shift.Summary()}"));
        ExpireOpportunities(career, report);
        return report;
    }

    /// <summary>
    /// Accetta un'opportunità: paga il costo, incassa quanto è immediato, registra
    /// la promessa collegata. Restituisce null se il patrimonio non basta — una
    /// proposta non accessibile non deve poter essere accettata.
    /// </summary>
    public static ConsequenceReport? AcceptOpportunity(CareerState career, Opportunity opportunity)
    {
        var assessment = opportunity.Assess(career.Cash);
        if (!assessment.Affordable) return null;

        var report = new ConsequenceReport();
        career.Opportunities ??= [];
        career.Promises ??= [];

        var cost = opportunity.NetCost;
        if (cost > 0)
        {
            // Una spesa non sostenibile non avviene: la cassa non va sotto zero.
            if (!CareerWallet.TryPay(career, cost, opportunity.Title).Paid) return null;
            career.EntryFeesPaid += cost;
            report.CashDelta -= cost;
            report.Applied.Add(new Consequence("costo", $"Pagati € {cost:N0} per «{opportunity.Title}»", -cost));
            if (opportunity.CoveredPercent > 0)
                report.Applied.Add(new Consequence("copertura",
                    $"{opportunity.ProposedBy} ha coperto il {opportunity.CoveredPercent}% del costo (€ {opportunity.Cost - cost:N0})"));
        }

        // Sponsorizzazioni ed eventi promozionali pagano subito.
        if (opportunity.Kind is OpportunityKind.SponsorDeal or OpportunityKind.PromotionalEvent)
        {
            var income = opportunity.LikelyReturn;
            career.Cash += income;
            // Anche l'ingaggio promozionale è denaro commerciale: lasciandolo
            // fuori dal totale, il bilancio della carriera non tornava e il
            // grosso degli incassi risultava da nessuna parte.
            career.SponsorMoney += income;
            report.CashDelta += income;
            report.Applied.Add(new Consequence("incasso", $"Incassati € {income:N0} da {opportunity.ProposedBy}", income));
            if (opportunity.Kind == OpportunityKind.SponsorDeal)
            {
                var previous = career.Sponsor;
                career.Sponsor = opportunity.ProposedBy;
                career.SponsorSignedAppeal = career.ReputationProfile?.SponsorAppeal ?? 0;
                report.Applied.Add(new Consequence("sponsor", OpportunityContext.HasSponsor(previous)
                    ? $"{opportunity.ProposedBy} prende il posto di {previous} come sponsor del pilota"
                    : $"{opportunity.ProposedBy} diventa lo sponsor del pilota"));
            }
        }

        if (opportunity.Salary > 0)
            report.Applied.Add(new Consequence("stipendio", $"Contratto con stipendio di € {opportunity.Salary:N0}"));

        if (opportunity.Promise != null)
        {
            var promise = opportunity.Promise;
            promise.LinkedOpportunityId = opportunity.Id;
            promise.Status = ConditionalPromise.StatusOpen;
            if (!career.Promises.Any(x => x.Id.Equals(promise.Id, StringComparison.OrdinalIgnoreCase)))
            {
                career.Promises.Add(promise);
                report.Applied.Add(new Consequence("promessa",
                    $"{promise.PromisedBy}: {promise.Description} se riuscirai a {promise.ConditionText} entro il {NarrativeCalendar.Format(promise.Deadline)}"));
            }
        }

        opportunity.Status = Opportunity.StatusAccepted;
        opportunity.ClosedStoryDate = career.StoryDate;
        return report;
    }

    /// <summary>
    /// Rifiuta un'opportunità. Non è gratis: chi ha proposto se lo ricorda, e
    /// rifiutare un sedile pagato costa credibilità più che rifiutare una gara open.
    /// </summary>
    public static ConsequenceReport DeclineOpportunity(CareerState career, Opportunity opportunity)
    {
        var report = new ConsequenceReport();
        career.ReputationProfile ??= new ReputationProfile();
        opportunity.Status = Opportunity.StatusDeclined;
        opportunity.ClosedStoryDate = career.StoryDate;

        var penalty = opportunity.Kind switch
        {
            OpportunityKind.ProfessionalSeat => -6,
            OpportunityKind.SubstituteDrive => -5,
            OpportunityKind.PartiallyFundedSeat => -3,
            OpportunityKind.FundedTest => -2,
            _ => 0
        };
        if (penalty != 0)
        {
            var shift = new ReputationShift();
            shift.Add(ReputationKind.TeamTrust, penalty, $"proposta di {opportunity.ProposedBy} rifiutata");
            career.ReputationProfile = ReputationDynamics.Apply(career.ReputationProfile, shift);
            career.TeamRelation = career.ReputationProfile.TeamTrust;
            career.Reputation = career.ReputationProfile.Overall;
            report.Reputation = shift;
            report.Applied.Add(new Consequence("rifiuto", $"{opportunity.ProposedBy} registra il rifiuto: fiducia dei team {penalty:+#;-#;0}"));
        }
        else report.Applied.Add(new Consequence("rifiuto", $"Proposta «{opportunity.Title}» rifiutata: nessuna conseguenza sui rapporti"));
        return report;
    }

    // --------------------------------------------------------------- interni

    private static void VerifyPromises(CareerState career, RaceHistoryEntry race, ConsequenceReport report)
    {
        career.Promises ??= [];
        var fieldSize = Math.Max(1, race.Classification.Count);
        foreach (var promise in career.Promises.Where(x => x.IsOpen).ToList())
        {
            if (promise.IsSatisfiedBy(race.Position, fieldSize, race.Dnf))
            {
                promise.Status = ConditionalPromise.StatusHonoured;
                report.Honoured.Add(promise);
                if (promise.RewardCash > 0)
                {
                    career.Cash += promise.RewardCash;
                    career.SponsorMoney += promise.RewardCash;
                    report.CashDelta += promise.RewardCash;
                }
                if (promise.RewardSeasonCoverage > 0) career.SeasonCoveragePercent = Math.Max(career.SeasonCoveragePercent, promise.RewardSeasonCoverage);
                report.Applied.Add(new Consequence("promessa-mantenuta",
                    $"Promessa mantenuta da {promise.PromisedBy}: {promise.RewardDescription}" +
                    (promise.RewardCash > 0 ? $" (€ {promise.RewardCash:N0})" : ""), promise.RewardCash));
                continue;
            }
            if (race.StoryDate > promise.Deadline)
            {
                promise.Status = ConditionalPromise.StatusExpired;
                report.Failed.Add(promise);
                report.Applied.Add(new Consequence("promessa-scaduta",
                    $"Scaduta la promessa di {promise.PromisedBy}: {promise.ConditionText} non è stato raggiunto entro il termine"));
            }
        }
    }

    private static void ExpireOpportunities(CareerState career, ConsequenceReport report)
    {
        career.Opportunities ??= [];
        foreach (var opportunity in career.Opportunities.Where(x => x.IsOpen).ToList())
        {
            if (career.StoryDate <= opportunity.Deadline) continue;
            opportunity.Status = Opportunity.StatusExpired;
            opportunity.ClosedStoryDate = career.StoryDate;
            report.ExpiredOpportunities.Add(opportunity.Title);
            report.Applied.Add(new Consequence("occasione-persa",
                $"Scaduta senza risposta: «{opportunity.Title}» di {opportunity.ProposedBy}"));
        }
    }

    private static int CountPointlessStreak(CareerState career, RaceHistoryEntry current)
    {
        var streak = 0;
        foreach (var race in career.RaceHistory.Where(x => !ReferenceEquals(x, current)).AsEnumerable().Reverse())
        {
            if (race.Points > 0 && !race.Dnf) break;
            streak++;
        }
        return streak;
    }
}
