namespace CorsaCareer1991;

public enum FinancialRisk
{
    /// <summary>Il costo incide poco: nessuna conseguenza sulla continuità.</summary>
    Low,
    /// <summary>Incide, ma resta margine per sbagliare.</summary>
    Medium,
    /// <summary>Un esito negativo compromette la stagione.</summary>
    High,
    /// <summary>Il costo lascerebbe il pilota sotto la soglia di sopravvivenza.</summary>
    Prohibitive,
    /// <summary>Non ci sono i soldi: l'opportunità non è acquistabile.</summary>
    Unaffordable
}

/// <summary>Il quadro economico di una decisione, con i numeri che il giocatore deve vedere.</summary>
public sealed class FinancialAssessment
{
    public int Cash { get; set; }
    public int Cost { get; set; }
    public int BestCaseReturn { get; set; }
    public int LikelyReturn { get; set; }
    public int WorstCaseReturn { get; set; }
    public FinancialRisk Risk { get; set; }
    public string RiskReason { get; set; } = "";
    public bool Affordable => Risk != FinancialRisk.Unaffordable;

    public int CashAfterCost => Cash - Cost;
    public int BestCaseBalance => Cash - Cost + BestCaseReturn;
    public int WorstCaseBalance => Cash - Cost + WorstCaseReturn;

    /// <summary>
    /// Il riquadro che precede ogni decisione. È il formato richiesto: patrimonio,
    /// costo, guadagno possibile, rischio — così la scelta è informata.
    /// </summary>
    public string Describe() => string.Join("\n",
        $"Patrimonio attuale     € {Cash:N0}",
        $"Costo dell'opportunità € {Cost:N0}",
        $"Guadagno possibile     € {LikelyReturn:N0}  (massimo € {BestCaseReturn:N0}, minimo € {WorstCaseReturn:N0})",
        $"Saldo nel caso peggiore € {WorstCaseBalance:N0}",
        $"Rischio economico      {CareerFinances.Label(Risk).ToUpperInvariant()} — {RiskReason}");
}

/// <summary>
/// Il denaro come risorsa reale.
///
/// Prima la carriera partiva con 250.000 € e nessuna opportunità aveva un costo
/// d'ingresso: non era possibile andare in difficoltà economica, e questo togliva
/// il rischio da ogni decisione. Qui il patrimonio è un vincolo: sotto la soglia
/// di sopravvivenza le categorie costose diventano inaccessibili e la carriera va
/// ricostruita dal basso.
/// </summary>
public static class CareerFinances
{
    /// <summary>
    /// Patrimonio sotto il quale il pilota non può più sostenere una stagione e
    /// deve ripiegare su opportunità economiche. Non è un game over: è il punto
    /// in cui il percorso cambia.
    /// </summary>
    public const int SurvivalFloor = 250;

    /// <summary>
    /// Capitale iniziale. Deve essere quello di un ragazzo, non di un
    /// investitore: con 32.000 euro si compravano dodici gare il primo giorno e
    /// non restava nessuna salita da fare. Con questa cifra la prima iscrizione
    /// e gia una decisione.
    /// </summary>
    public const int StartingCash = 900;

    public static FinancialAssessment Assess(int cash, int cost, int bestCaseReturn, int likelyReturn, int worstCaseReturn)
    {
        var assessment = new FinancialAssessment
        {
            Cash = cash, Cost = Math.Max(0, cost),
            BestCaseReturn = Math.Max(0, bestCaseReturn),
            LikelyReturn = Math.Max(0, likelyReturn),
            WorstCaseReturn = Math.Max(0, worstCaseReturn)
        };

        if (assessment.Cost == 0)
        {
            assessment.Risk = FinancialRisk.Low;
            assessment.RiskReason = "nessun costo d'ingresso";
            return assessment;
        }
        if (assessment.Cost > cash)
        {
            assessment.Risk = FinancialRisk.Unaffordable;
            assessment.RiskReason = $"mancano € {assessment.Cost - cash:N0}";
            return assessment;
        }

        var share = assessment.Cost / (double)Math.Max(1, cash);
        var worstBalance = assessment.WorstCaseBalance;
        if (worstBalance < SurvivalFloor)
        {
            assessment.Risk = FinancialRisk.Prohibitive;
            assessment.RiskReason = $"nel caso peggiore resterebbero € {worstBalance:N0}, sotto la soglia di € {SurvivalFloor:N0}";
            return assessment;
        }
        // Impegnare quasi metà del patrimonio è rischio alto: sopra questa soglia
        // un esito negativo cambia le opportunità accessibili nei mesi successivi.
        if (share >= 0.45)
        {
            assessment.Risk = FinancialRisk.High;
            assessment.RiskReason = $"impegna il {share * 100:0}% del patrimonio";
            return assessment;
        }
        if (share >= 0.25)
        {
            assessment.Risk = FinancialRisk.Medium;
            assessment.RiskReason = $"impegna il {share * 100:0}% del patrimonio, con margine residuo";
            return assessment;
        }
        assessment.Risk = FinancialRisk.Low;
        assessment.RiskReason = $"impegna solo il {share * 100:0}% del patrimonio";
        return assessment;
    }

    public static string Label(FinancialRisk risk) => risk switch
    {
        FinancialRisk.Low => "basso",
        FinancialRisk.Medium => "medio",
        FinancialRisk.High => "alto",
        FinancialRisk.Prohibitive => "proibitivo",
        _ => "non sostenibile"
    };

    /// <summary>Vero se il pilota è in difficoltà e deve ricostruire dal basso.</summary>
    public static bool InTrouble(int cash) => cash < SurvivalFloor;

    /// <summary>
    /// Stato economico dichiarato, mostrato nel portale: serve a far capire al
    /// giocatore quando il problema non è più sportivo ma finanziario.
    /// </summary>
    public static string Status(int cash) => cash switch
    {
        < SurvivalFloor => "IN DIFFICOLTÀ — servono occasioni economiche per ricostruire",
        < 2000 => "BUDGET RIDOTTO — valuta ogni costo prima di correre",
        < 12000 => "MARGINE MINIMO — il kart è sostenibile, niente di più",
        < 60000 => "SOSTENIBILE — una categoria d'ingresso comincia a essere pensabile",
        < 250000 => "SOLIDO — si può programmare una stagione completa",
        _ => "AMPIO — le categorie superiori sono economicamente accessibili"
    };

    /// <summary>Quota d'iscrizione tipica di una stagione per categoria, quando non è finanziata.</summary>
    public static int SeasonEntryFee(string tier) => tier switch
    {
        "Formula / top tier" => 420000,
        "Categoria avanzata" => 160000,
        "Categoria regionale" => 22000,
        _ => 2400
    };

    /// <summary>
    /// Quota di una gara, per gradino della scala. Le categorie meccaniche sono
    /// quattro e non bastano: fra una gara di club con i kart a quattro tempi e
    /// una di formula d'ingresso passano tre ordini di grandezza, e con un solo
    /// valore per tutto il fondo la salita non si sentiva.
    /// </summary>
    public static int RaceEntryFeeForStep(int step) => step switch
    {
        <= 1 => 120,      // club, kart a quattro tempi
        2 => 380,         // due tempi
        3 => 900,         // 125 con cambio
        4 => 9000,        // prima categoria vera
        5 => 22000,
        6 => 38000,
        _ => 60000
    };

    /// <summary>Giornata di test, per gradino.</summary>
    public static int TestFeeForStep(int step) => step switch
    {
        <= 1 => 60,
        2 => 180,
        3 => 450,
        4 => 4000,
        5 => 9000,
        6 => 16000,
        _ => 26000
    };

    /// <summary>Stagione intera, per gradino.</summary>
    public static int SeasonEntryFeeForStep(int step) => step switch
    {
        <= 1 => 1400,
        2 => 4200,
        3 => 11000,
        4 => 60000,
        5 => 160000,
        6 => 280000,
        _ => 420000
    };

    /// <summary>Quota di una singola gara su invito.</summary>
    public static int RaceEntryFee(string tier) => tier switch
    {
        "Formula / top tier" => 60000,
        "Categoria avanzata" => 22000,
        "Categoria regionale" => 9000,
        _ => 380
    };

    public static int TestFee(string tier) => tier switch
    {
        "Formula / top tier" => 26000,
        "Categoria avanzata" => 9000,
        "Categoria regionale" => 4000,
        _ => 180
    };
}
