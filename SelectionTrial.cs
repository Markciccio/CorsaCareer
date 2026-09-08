namespace CorsaCareer1991;

/// <summary>
/// Una giornata di una selezione a più prove.
///
/// Una selezione non è una gara: è un provino su più giorni, in cui un team
/// guarda cose diverse in giorni diversi. Il primo giorno conta l'adattamento,
/// il secondo la costanza, il terzo il tempo puro, il quarto come si sta in
/// mezzo agli altri. Un pilota veloce e inaffidabile e un pilota lento e
/// preciso non devono ricevere lo stesso giudizio, ed è per questo che le
/// giornate vengono pesate separatamente invece di produrre un unico numero.
/// </summary>
public enum SelectionDayKind
{
    /// <summary>Familiarizzazione: quanto migliora dal primo all'ultimo giro.</summary>
    Familiarisation,
    /// <summary>Long run: quanto sono vicini fra loro i giri.</summary>
    Consistency,
    /// <summary>Time attack: il tempo puro contro il riferimento.</summary>
    TimeAttack,
    /// <summary>Gara di valutazione: il risultato in mezzo agli altri.</summary>
    EvaluationRace
}

/// <summary>Il referto di una giornata, con i soli dati che Assetto Corsa fornisce davvero.</summary>
public sealed class SelectionDayResult
{
    public SelectionDayKind Kind { get; set; }
    public int Day { get; set; }
    public string Track { get; set; } = "";
    public string Car { get; set; } = "";
    public DateTime StoryDate { get; set; }
    /// <summary>Vero quando la giornata è stata svolta e ha un referto.</summary>
    public bool Completed { get; set; }
    /// <summary>Vero se il referto viene dalla modalità debug e non da una sessione reale.</summary>
    public bool Simulated { get; set; }

    public int Laps { get; set; }
    public int BestLapMilliseconds { get; set; }
    /// <summary>Riferimento della combinazione pista/vettura al momento della prova.</summary>
    public int TargetLapMilliseconds { get; set; }
    /// <summary>Somma delle penalità: è la proxy della guida pulita che il referto offre.</summary>
    public double PenaltySeconds { get; set; }
    /// <summary>Danno riportato, come lo dichiara il referto.</summary>
    public double Damage { get; set; }
    /// <summary>Solo per la gara di valutazione.</summary>
    public int Position { get; set; }
    public int FieldSize { get; set; }
    public bool Dnf { get; set; }

    /// <summary>Punteggio della giornata, 0-100. Assegnato dal motore, non dal referto.</summary>
    public int Score { get; set; }
    public string Verdict { get; set; } = "";
    public string ResultSha256 { get; set; } = "";

    public static string KindLabel(SelectionDayKind kind) => kind switch
    {
        SelectionDayKind.Familiarisation => "Familiarizzazione",
        SelectionDayKind.Consistency => "Prova di costanza",
        SelectionDayKind.TimeAttack => "Time attack",
        _ => "Gara di valutazione"
    };

    /// <summary>Cosa guarda il team in questa giornata: va detto prima di scendere in pista.</summary>
    public static string KindBrief(SelectionDayKind kind) => kind switch
    {
        SelectionDayKind.Familiarisation =>
            "Non serve il giro record. Serve arrivare in fondo alla sessione più veloce di come hai cominciato.",
        SelectionDayKind.Consistency =>
            "Giri uguali, uno dietro l'altro. Un giro fenomenale e cinque sbagliati qui valgono meno di sei giri onesti.",
        SelectionDayKind.TimeAttack =>
            "Adesso conta il cronometro, e conta contro un riferimento preciso.",
        _ => "In mezzo agli altri. Non è più una prova contro il tempo: è una gara."
    };
}

/// <summary>Il giudizio complessivo della selezione.</summary>
public enum SelectionOutcome
{
    /// <summary>Prove non ancora concluse.</summary>
    Pending,
    /// <summary>Promosso: il sedile è suo.</summary>
    Promoted,
    /// <summary>Non promosso, ma il team vuole rivederlo.</summary>
    SecondChance,
    /// <summary>Respinto.</summary>
    Rejected
}

/// <summary>
/// Una selezione a più giornate: lo snodo in cui una carriera cambia livello.
///
/// Il punteggio finale non è la media dei giorni. Velocità, costanza e
/// adattamento vengono tenuti separati perché un team li usa in modo diverso:
/// si può essere presi per il passo puro nonostante qualche errore, o scartati
/// pur essendo velocissimi se non si arriva in fondo a un long run.
/// </summary>
public sealed class SelectionTrial
{
    public string Id { get; set; } = "";
    /// <summary>Il nome della selezione, quello che il giocatore legge.</summary>
    public string Title { get; set; } = "";
    /// <summary>Chi la organizza: un team, un campionato, un programma giovani.</summary>
    public string OrganisedBy { get; set; } = "";
    /// <summary>La pista richiesta dal programma, se ne esige una precisa.</summary>
    public string PreferredTrackId { get; set; } = "";
    /// <summary>La pista realmente usata: può differire, e in quel caso va dichiarato.</summary>
    public string TrackId { get; set; } = "";
    public string TrackName { get; set; } = "";
    /// <summary>Vero quando la pista richiesta non era installata e si è usata un'altra.</summary>
    public bool TrackSubstituted { get; set; }
    public string TrackSubstitutionNote { get; set; } = "";
    public string CarId { get; set; } = "";
    /// <summary>Il gradino della scala che questa selezione apre.</summary>
    public string TargetRungId { get; set; } = "";
    public string Tier { get; set; } = "Categoria regionale";

    public DateTime StartDate { get; set; }
    /// <summary>Costo complessivo a carico del pilota, al netto della copertura.</summary>
    public int Cost { get; set; }
    public int CoveredPercent { get; set; }
    /// <summary>Quanti candidati si giocano il posto: cambia il peso della gara finale.</summary>
    public int Candidates { get; set; } = 8;
    /// <summary>Quanti posti mette in gioco il programma.</summary>
    public int Seats { get; set; } = 1;

    public List<SelectionDayResult> Days { get; set; } = [];
    public string Status { get; set; } = StatusOpen;
    public string Outcome { get; set; } = nameof(SelectionOutcome.Pending);
    /// <summary>Quante volte il pilota ha già affrontato questa selezione.</summary>
    public int Attempt { get; set; } = 1;

    public const string StatusOpen = "In corso";
    public const string StatusClosed = "Conclusa";
    public const string StatusAbandoned = "Abbandonata";

    public int NetCost => (int)Math.Round(Cost * (100 - Math.Clamp(CoveredPercent, 0, 100)) / 100.0);
    public bool IsOpen => Status.Equals(StatusOpen, StringComparison.OrdinalIgnoreCase);
    public SelectionDayResult? NextDay => Days.FirstOrDefault(x => !x.Completed);
    public bool AllDaysDone => Days.Count > 0 && Days.All(x => x.Completed);
    /// <summary>Vero se una sola giornata è stata risolta in debug: il verdetto va marcato.</summary>
    public bool AnySimulated => Days.Any(x => x.Completed && x.Simulated);
}
