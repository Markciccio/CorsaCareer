namespace CorsaCareer1991;

public enum OpportunityKind
{
    /// <summary>Gara di categoria minore, economica: la via per ricostruire.</summary>
    EntryRace,
    /// <summary>Gara su invito in una categoria superiore, a pagamento.</summary>
    InvitationRace,
    /// <summary>Test privato pagato dal pilota.</summary>
    PaidTest,
    /// <summary>Test con costo coperto in parte o del tutto dal team.</summary>
    FundedTest,
    /// <summary>Sedile di campionato da pagare interamente.</summary>
    PayDriverSeat,
    /// <summary>Sedile di campionato con una parte della stagione finanziata.</summary>
    PartiallyFundedSeat,
    /// <summary>Sedile professionistico con stipendio.</summary>
    ProfessionalSeat,
    /// <summary>Sostituzione di un pilota indisponibile: arriva solo se i team si fidano.</summary>
    SubstituteDrive,
    /// <summary>Evento promozionale: porta denaro e popolarità, non risultati.</summary>
    PromotionalEvent,
    /// <summary>Offerta di sponsorizzazione: porta denaro ricorrente.</summary>
    SponsorDeal,

    /// <summary>
    /// Un posto nel campionato di livello superiore, arrivato per conoscenza.
    ///
    /// La scala si sale arrivando nei primi tre, ed è la strada sportiva. Ma
    /// un pilota molto seguito, in forma e con del budget, di tanto in tanto
    /// viene cercato lo stesso: uno sponsor che ha visto i suoi post, un ex
    /// capo del meccanico, una giornalista con un contatto. Non è un premio
    /// garantito — è la ragione per cui vale la pena curare immagine, conti e
    /// preparazione anche quando la classifica non basta.
    /// </summary>
    ChampionshipStepUp,

    /// <summary>
    /// Un sedile in un'altra disciplina, allo stesso livello.
    ///
    /// In alto la carriera non è più una scala: è una mappa. Mansell lascia la
    /// Formula 1 campione del mondo e va in Indy; Alonso va a Le Mans e poi
    /// torna in Formula 1; Andretti corre tutto quello che gli capita per
    /// trent'anni. Non è una promozione né una retrocessione — è un'altra
    /// strada allo stesso livello, e ci si può tornare.
    ///
    /// Arriva solo a chi è arrivato in cima e ci ha corso una stagione: un
    /// esordiente che cambia disciplina ogni tre gare non è Andretti, è una
    /// carriera che oscilla.
    /// </summary>
    DisciplineSwitch
}

/// <summary>
/// Promessa condizionale: «se ottieni X, allora Y». Viene salvata e verificata
/// dopo ogni risultato reale, così il giocatore sa per cosa sta correndo e la
/// promessa non resta una frase in un articolo.
/// </summary>
public sealed class ConditionalPromise
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Condizione: posizione massima da ottenere per soddisfarla.</summary>
    public int MaxPosition { get; set; }
    /// <summary>Condizione alternativa: percentuale massima del gruppo (0-100).</summary>
    public int MaxFieldPercent { get; set; }
    /// <summary>Ricompensa in denaro se la promessa viene mantenuta.</summary>
    public int RewardCash { get; set; }
    /// <summary>Copertura offerta sulla stagione successiva, in percentuale.</summary>
    public int RewardSeasonCoverage { get; set; }
    public string RewardDescription { get; set; } = "";
    public string PromisedBy { get; set; } = "";
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = StatusOpen;
    public string LinkedOpportunityId { get; set; } = "";

    public const string StatusOpen = "In attesa";
    public const string StatusHonoured = "Mantenuta";
    public const string StatusFailed = "Non soddisfatta";
    public const string StatusExpired = "Scaduta";

    public bool IsOpen => Status.Equals(StatusOpen, StringComparison.OrdinalIgnoreCase);

    /// <summary>Verifica la condizione su un risultato reale.</summary>
    public bool IsSatisfiedBy(int position, int fieldSize, bool dnf)
    {
        if (dnf || position <= 0) return false;
        if (MaxPosition > 0 && position <= MaxPosition) return true;
        if (MaxFieldPercent > 0 && fieldSize > 1)
        {
            var percent = (position - 1) / (double)(fieldSize - 1) * 100;
            return percent <= MaxFieldPercent;
        }
        return false;
    }

    public string ConditionText => MaxPosition > 0
        ? $"arrivare entro la posizione {MaxPosition}"
        : MaxFieldPercent > 0 ? $"chiudere entro il {MaxFieldPercent}% del gruppo" : "condizione non dichiarata";
}

/// <summary>
/// Un'opportunità concreta proposta al pilota: cosa è, quanto costa, cosa può
/// rendere, e — soprattutto — perché è arrivata.
/// </summary>
public sealed class Opportunity
{
    public string Id { get; set; } = "";
    public OpportunityKind Kind { get; set; }
    public string Title { get; set; } = "";
    /// <summary>Chi la propone: team, organizzatore o sponsor.</summary>
    public string ProposedBy { get; set; } = "";
    /// <summary>La ragione narrativa, costruita da fatti realmente accaduti.</summary>
    public string Justification { get; set; } = "";
    public string Tier { get; set; } = "Rookie";
    public string Category { get; set; } = "";
    public string CarId { get; set; } = "";
    public string TrackId { get; set; } = "";
    public string TrackName { get; set; } = "";
    public DateTime Date { get; set; }
    public DateTime Deadline { get; set; }

    // --- economia
    public int Cost { get; set; }
    /// <summary>Quota coperta da chi propone, in percentuale del costo.</summary>
    public int CoveredPercent { get; set; }
    public int BestCaseReturn { get; set; }
    public int LikelyReturn { get; set; }
    public int WorstCaseReturn { get; set; }
    /// <summary>Stipendio stagionale, se l'opportunità è un sedile professionistico.</summary>
    public int Salary { get; set; }
    public int SeasonRounds { get; set; }

    public string Objective { get; set; } = "";
    public ConditionalPromise? Promise { get; set; }
    public string Status { get; set; } = StatusOpen;

    /// <summary>
    /// Il giorno di storia in cui la proposta si è chiusa — accettata,
    /// rifiutata o scaduta.
    ///
    /// Serve a non riproporre domani quello che si è deciso oggi: senza questa
    /// data il paddock rigenerava ogni giorno le stesse identiche proposte, e
    /// una giornata promozionale accettata ricompariva il mattino dopo. Un
    /// pilota poteva incassarla all'infinito.
    /// </summary>
    public DateTime ClosedStoryDate { get; set; }

    public const string StatusOpen = "Disponibile";
    public const string StatusAccepted = "Accettata";
    public const string StatusDeclined = "Rifiutata";
    public const string StatusExpired = "Scaduta";

    public bool IsOpen => Status.Equals(StatusOpen, StringComparison.OrdinalIgnoreCase);
    /// <summary>Costo effettivo dopo la copertura di chi propone.</summary>
    public int NetCost => (int)Math.Round(Cost * (100 - Math.Clamp(CoveredPercent, 0, 100)) / 100.0);

    /// <summary>
    /// Livello di campionato che questa proposta assegna, se ne assegna uno.
    /// Zero significa «resta dove sei»: vale per tutte le proposte normali.
    /// </summary>
    public int GrantsChampionshipLevel { get; set; }

    /// <summary>
    /// Quanti giorni il paddock lascia passare prima di riproporre una cosa
    /// dello stesso tipo. Non sono numeri di comodo: dicono ogni quanto, in
    /// una carriera vera, ricapita un'occasione di quel genere.
    /// </summary>
    public static int CooldownDays(OpportunityKind kind) => kind switch
    {
        OpportunityKind.PromotionalEvent => 45,
        OpportunityKind.SponsorDeal => 110,
        OpportunityKind.PaidTest => 21,
        OpportunityKind.FundedTest => 21,
        OpportunityKind.EntryRace => 30,
        OpportunityKind.InvitationRace => 32,
        OpportunityKind.SubstituteDrive => 30,
        OpportunityKind.PayDriverSeat => 45,
        OpportunityKind.PartiallyFundedSeat => 45,
        OpportunityKind.ProfessionalSeat => 60,
        OpportunityKind.ChampionshipStepUp => 90,
        OpportunityKind.DisciplineSwitch => 150,
        _ => 30
    };

    public bool IsSeat => Kind is OpportunityKind.PayDriverSeat or OpportunityKind.PartiallyFundedSeat or OpportunityKind.ProfessionalSeat or OpportunityKind.ChampionshipStepUp or OpportunityKind.DisciplineSwitch;
    public bool IsRace => Kind is OpportunityKind.EntryRace or OpportunityKind.InvitationRace or OpportunityKind.SubstituteDrive;
    public bool IsTest => Kind is OpportunityKind.PaidTest or OpportunityKind.FundedTest;

    public static string KindLabel(OpportunityKind kind) => kind switch
    {
        OpportunityKind.EntryRace => "Gara di categoria minore",
        OpportunityKind.InvitationRace => "Gara su invito",
        OpportunityKind.PaidTest => "Test privato a pagamento",
        OpportunityKind.FundedTest => "Test con costo coperto",
        OpportunityKind.PayDriverSeat => "Sedile da finanziare",
        OpportunityKind.PartiallyFundedSeat => "Sedile parzialmente finanziato",
        OpportunityKind.ProfessionalSeat => "Contratto professionistico",
        OpportunityKind.SubstituteDrive => "Sostituzione",
        OpportunityKind.PromotionalEvent => "Evento promozionale",
        OpportunityKind.ChampionshipStepUp => "Salto di campionato",
        _ => "Proposta di sponsorizzazione"
    };

    public FinancialAssessment Assess(int cash) =>
        CareerFinances.Assess(cash, NetCost, BestCaseReturn, LikelyReturn, WorstCaseReturn);

    public string Describe(int cash)
    {
        var lines = new List<string>
        {
            $"{KindLabel(Kind).ToUpperInvariant()} · {Tier}",
            Title,
            "",
            Justification,
            ""
        };
        if (!string.IsNullOrWhiteSpace(TrackName)) lines.Add($"Sede: {TrackName} · {NarrativeCalendar.Format(Date)}");
        if (SeasonRounds > 0) lines.Add($"Stagione: {SeasonRounds} appuntamenti");
        if (CoveredPercent > 0) lines.Add($"Copertura di {ProposedBy}: {CoveredPercent}% del costo (€ {Cost - NetCost:N0})");
        if (Salary > 0) lines.Add($"Stipendio: € {Salary:N0}");
        if (!string.IsNullOrWhiteSpace(Objective)) lines.Add($"Obiettivo: {Objective}");
        lines.Add("");
        lines.Add(Assess(cash).Describe());
        if (Promise != null)
        {
            lines.Add("");
            lines.Add($"PROMESSA DI {Promise.PromisedBy.ToUpperInvariant()}");
            lines.Add($"{Promise.Description} — condizione: {Promise.ConditionText}, entro il {NarrativeCalendar.Format(Promise.Deadline)}.");
        }
        lines.Add("");
        lines.Add($"Scade il {NarrativeCalendar.Format(Deadline)}.");
        return string.Join("\n", lines);
    }
}
