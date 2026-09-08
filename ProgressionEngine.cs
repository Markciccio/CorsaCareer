namespace CorsaCareer1991;

public sealed class ProgressionInput
{
    public int Reputation { get; set; }
    public int FinalChampionshipPosition { get; set; }
    public int ChampionshipFieldSize { get; set; } = 1;
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int SeasonRaces { get; set; }
    public int TeammateRacesWon { get; set; }
    public int TeammateRacesLost { get; set; }
    public bool SponsorObjectiveMet { get; set; }
    public bool ContractObjectiveMet { get; set; }
    public int Cash { get; set; }
    public string CurrentTier { get; set; } = "Rookie";
}

public sealed class PromotionAssessment
{
    public int Score { get; set; }
    public bool Ready { get; set; }
    public string Verdict { get; set; } = "";
    public List<string> Reasons { get; set; } = [];

    public string Explain() => string.Join("\n", Reasons.Select(x => "• " + x));
}

public sealed record TierChoice(string Tier, string Team, string Role, string Objective, int Prestige, int Salary, string Summary);

/// <summary>
/// Decide se il pilota è pronto per la categoria successiva. La versione
/// precedente guardava solo soglie di reputazione (25/45/70) e assegnava
/// d'ufficio auto, squadra e compagno. Qui il giudizio è multi-fattore, è
/// spiegato voce per voce e la scelta finale resta al giocatore.
/// </summary>
public static class ProgressionEngine
{
    public const int ReadyThreshold = 60;

    public static PromotionAssessment Assess(ProgressionInput input)
    {
        var assessment = new PromotionAssessment();
        var score = 0;

        // Reputazione: massimo 30 punti.
        var reputationScore = (int)Math.Round(Math.Clamp(input.Reputation, 0, 100) * 0.30);
        score += reputationScore;
        assessment.Reasons.Add($"Reputazione {input.Reputation}/100 → {reputationScore}/30");

        // Posizione finale in campionato: massimo 25 punti.
        var standingScore = 0;
        if (input.FinalChampionshipPosition > 0)
        {
            var field = Math.Max(1, input.ChampionshipFieldSize);
            var ratio = field <= 1 ? 0.0 : (input.FinalChampionshipPosition - 1) / (double)(field - 1);
            standingScore = (int)Math.Round(Math.Clamp(1.0 - ratio, 0, 1) * 25);
            assessment.Reasons.Add($"Classifica finale P{input.FinalChampionshipPosition} su {field} → {standingScore}/25");
        }
        else assessment.Reasons.Add("Classifica finale non disponibile → 0/25");
        score += standingScore;

        // Vittorie e podi reali: massimo 20 punti.
        var resultScore = Math.Min(20, input.Wins * 6 + input.Podiums * 2);
        score += resultScore;
        assessment.Reasons.Add($"{input.Wins} vittorie e {input.Podiums} podi reali → {resultScore}/20");

        // Confronto diretto col compagno: massimo 15 punti.
        var duels = input.TeammateRacesWon + input.TeammateRacesLost;
        var teammateScore = 0;
        if (duels > 0)
        {
            teammateScore = (int)Math.Round(input.TeammateRacesWon / (double)duels * 15);
            assessment.Reasons.Add($"Confronto col compagno {input.TeammateRacesWon}-{input.TeammateRacesLost} → {teammateScore}/15");
        }
        else assessment.Reasons.Add("Nessun confronto diretto col compagno registrato → 0/15");
        score += teammateScore;

        // Professionalità: obiettivi rispettati, massimo 10 punti.
        var objectiveScore = (input.ContractObjectiveMet ? 6 : 0) + (input.SponsorObjectiveMet ? 4 : 0);
        score += objectiveScore;
        assessment.Reasons.Add($"Obiettivo contratto {(input.ContractObjectiveMet ? "raggiunto" : "non raggiunto")}, sponsor {(input.SponsorObjectiveMet ? "soddisfatto" : "non soddisfatto")} → {objectiveScore}/10");

        // Il budget non dà punti ma può bloccare una promozione costosa.
        var budgetRequired = BudgetRequiredForPromotion(input.CurrentTier);
        var budgetBlocked = input.Cash < budgetRequired;
        if (budgetBlocked) assessment.Reasons.Add($"Budget insufficiente per il salto: servono € {budgetRequired:N0}, disponibili € {input.Cash:N0}");

        // Una stagione troppo corta non è una base credibile per una promozione.
        var tooFewRaces = input.SeasonRaces < 3;
        if (tooFewRaces) assessment.Reasons.Add($"Solo {input.SeasonRaces} gare reali nella stagione: servono almeno 3 per un giudizio");

        assessment.Score = Math.Clamp(score, 0, 100);
        assessment.Ready = assessment.Score >= ReadyThreshold && !budgetBlocked && !tooFewRaces;
        assessment.Verdict = assessment.Ready
            ? $"Promozione meritata ({assessment.Score}/100): il paddock apre la categoria successiva."
            : budgetBlocked
                ? $"Promozione possibile sul campo ({assessment.Score}/100) ma non sostenibile economicamente."
                : tooFewRaces
                    ? $"Giudizio sospeso ({assessment.Score}/100): la stagione non ha abbastanza gare reali."
                    : $"Permanenza in categoria ({assessment.Score}/100): serve una stagione più solida.";
        return assessment;
    }

    public static int BudgetRequiredForPromotion(string currentTier) => currentTier switch
    {
        "Categoria avanzata" => 400000,
        "Categoria regionale" => 150000,
        "Rookie" => 40000,
        _ => 0
    };

    public static int TierRank(string tier) => tier switch
    {
        "Rookie" => 0,
        "Categoria regionale" => 1,
        "Categoria avanzata" => 2,
        "Formula / top tier" => 3,
        _ => 0
    };

    public static string TierForRank(int rank) => rank switch
    {
        1 => "Categoria regionale",
        2 => "Categoria avanzata",
        3 => "Formula / top tier",
        _ => "Rookie"
    };

    /// <summary>
    /// Il livello immediatamente superiore effettivamente disponibile fra i
    /// contenuti installati. Restituisce stringa vuota se non esiste: la carriera
    /// non promette una categoria che il giocatore non possiede.
    /// </summary>
    public static string NextAvailableTier(string currentTier, IEnumerable<string> installedTiers)
    {
        var currentRank = TierRank(currentTier);
        var higher = installedTiers.Select(TierRank).Where(rank => rank > currentRank).ToList();
        return higher.Count == 0 ? "" : TierForRank(higher.Min());
    }

    /// <summary>
    /// Costruisce il bivio reale della fine stagione: restare da prima guida in un
    /// progetto conosciuto oppure salire come seconda guida in un team più forte.
    /// </summary>
    public static List<TierChoice> BuildChoices(string currentTier, string currentTeam, string nextTier, int reputation, int currentSalary, bool promotionReady)
    {
        var choices = new List<TierChoice>
        {
            new(currentTier, currentTeam, "prima guida",
                "Vinci il campionato con la squadra che ti conosce",
                Math.Clamp(reputation + 5, 20, 85),
                (int)Math.Round(currentSalary * 1.15),
                "Resti il riferimento del team: macchina conosciuta, aspettative alte, nessun periodo di adattamento.")
        };
        if (promotionReady && !string.IsNullOrWhiteSpace(nextTier))
            choices.Add(new TierChoice(nextTier, "", "seconda guida",
                "Concludi la stagione davanti almeno metà del gruppo",
                Math.Clamp(reputation + 20, 35, 95),
                (int)Math.Round(currentSalary * 1.6),
                "Salti di categoria in un progetto più forte, ma parti come seconda guida e la macchina è nuova per te."));
        return choices;
    }
}
