namespace CorsaCareer;

/// <summary>La definizione di una selezione: i dati, non il testo.</summary>
public sealed record SelectionDefinition(
    string Id,
    string Title,
    string OrganisedBy,
    /// <summary>Il gradino che questa selezione apre.</summary>
    string TargetRungId,
    /// <summary>Circuito richiesto dal programma. Può non essere installato.</summary>
    string PreferredTrackId,
    string PreferredTrackName,
    /// <summary>Caratteristiche del tracciato, per scegliere un rimpiazzo sensato.</summary>
    string[] TrackTags,
    int PreferredTrackLengthMeters,
    /// <summary>Le giornate previste, in ordine.</summary>
    SelectionDayKind[] Days,
    int Candidates,
    int Seats,
    /// <summary>Quanto pesa nel racconto: decide il tono degli articoli.</summary>
    int Prestige,
    string Standfirst);

/// <summary>
/// Le selezioni che una carriera può incontrare.
///
/// Sono lo snodo fra il kart e la monoposto, e più in alto fra una categoria e
/// la successiva: il momento in cui qualcuno decide se investire su un pilota.
/// Stanno qui come dati e non come codice sparso, così aggiungerne una non
/// richiede toccare la logica.
///
/// I circuiti indicati sono una preferenza, non un requisito: se non sono
/// installati la prova si svolge altrove e la sostituzione viene dichiarata.
/// </summary>
public static class SelectionCatalog
{
    /// <summary>Il provino che apre la prima monoposto: il salto che vale una carriera.</summary>
    public const string FirstFormula = "selezione-monoposto";
    /// <summary>La selezione per la categoria nazionale.</summary>
    public const string NationalFormula = "selezione-nazionale";

    public static readonly IReadOnlyList<SelectionDefinition> All =
    [
        new(
            Id: FirstFormula,
            Title: "Selezione monoposto · programma giovani",
            OrganisedBy: "Kaido Motorsport",
            TargetRungId: "formula-4",
            // Un tracciato corto e tecnico: è lì che si vede se un kartista sa
            // gestire una monoposto, non su un circuito veloce dove il motore
            // copre gli errori.
            PreferredTrackId: "fuji_short",
            PreferredTrackName: "Fuji Short Course",
            TrackTags: ["short", "corto", "kart", "tecnico", "club"],
            PreferredTrackLengthMeters: 4400,
            Days:
            [
                SelectionDayKind.Familiarisation,
                SelectionDayKind.Consistency,
                SelectionDayKind.TimeAttack,
                SelectionDayKind.EvaluationRace
            ],
            Candidates: 12,
            Seats: 2,
            Prestige: 55,
            Standfirst: "Quattro giorni per decidere se un kartista può guidare una monoposto. " +
                        "Dodici candidati, due sedili, e nessuno che regali niente."),

        new(
            Id: NationalFormula,
            Title: "Selezione formula nazionale",
            OrganisedBy: "Minato Apex Racing",
            TargetRungId: "formula-3",
            PreferredTrackId: "fuji",
            PreferredTrackName: "Fuji Speedway",
            TrackTags: ["permanent", "fuji", "gp"],
            PreferredTrackLengthMeters: 4560,
            Days:
            [
                SelectionDayKind.Consistency,
                SelectionDayKind.TimeAttack,
                SelectionDayKind.EvaluationRace
            ],
            Candidates: 16,
            Seats: 3,
            Prestige: 72,
            Standfirst: "Il livello in cui i risultati cominciano a valere fuori dai confini. " +
                        "Qui i candidati arrivano già con una stagione vera alle spalle.")
    ];

    public static SelectionDefinition? ById(string? id) =>
        All.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// La selezione che si apre a un pilota che sta su un certo gradino, se ce
    /// n'è una. Il kart porta alla monoposto; la monoposto d'ingresso alla
    /// nazionale.
    /// </summary>
    public static SelectionDefinition? ForRung(LadderRung rung) => rung.Id switch
    {
        CareerLadder.Shifter => ById(FirstFormula),
        CareerLadder.TwoStroke => ById(FirstFormula),
        "formula-4" => ById(NationalFormula),
        _ => null
    };

    /// <summary>
    /// Costo a carico del pilota per una selezione, dal gradino di destinazione.
    /// Una selezione costa più di un test privato: sono più giornate, e chi la
    /// organizza sa quanto vale il posto che mette in gioco.
    /// </summary>
    public static int CostFor(SelectionDefinition definition)
    {
        var rung = CareerLadder.ById(definition.TargetRungId);
        var days = Math.Max(1, definition.Days.Length);
        return CareerFinances.TestFeeForStep(rung.Step) * days;
    }

    /// <summary>
    /// Crea la selezione pronta da giocare: sede risolta sui contenuti reali,
    /// giornate predisposte, costo calcolato.
    /// </summary>
    public static SelectionTrial Build(SelectionDefinition definition, IReadOnlyList<ContentTrackRecord> tracks,
        string carId, DateTime start, int attempt = 1, int coveredPercent = 0)
    {
        var choice = TrackPreference.Resolve(tracks, definition.PreferredTrackId, definition.TrackTags,
            definition.PreferredTrackLengthMeters, definition.PreferredTrackName);
        var rung = CareerLadder.ById(definition.TargetRungId);
        var trial = new SelectionTrial
        {
            Id = $"{definition.Id}-a{attempt}",
            Title = definition.Title,
            OrganisedBy = definition.OrganisedBy,
            PreferredTrackId = definition.PreferredTrackId,
            TrackId = choice.Track?.Id ?? "",
            TrackName = choice.Track?.Name ?? definition.PreferredTrackName,
            TrackSubstituted = choice.Substituted,
            TrackSubstitutionNote = choice.Note,
            CarId = carId,
            TargetRungId = definition.TargetRungId,
            Tier = rung.Tier,
            StartDate = start,
            Cost = CostFor(definition),
            CoveredPercent = coveredPercent,
            Candidates = definition.Candidates,
            Seats = definition.Seats,
            Attempt = attempt
        };
        for (var i = 0; i < definition.Days.Length; i++)
            trial.Days.Add(new SelectionDayResult
            {
                Kind = definition.Days[i],
                Day = i + 1,
                // Una giornata al giorno: la selezione occupa giorni consecutivi
                // e va vista nel calendario come un blocco, non come un punto.
                StoryDate = start.AddDays(i),
                Track = trial.TrackId,
                Car = carId
            });
        return trial;
    }
}
