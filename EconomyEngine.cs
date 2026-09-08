namespace CorsaCareer1991;

public sealed class RaceEconomyInput
{
    public string Tier { get; set; } = "Rookie";
    public int Position { get; set; }
    public int FieldSize { get; set; } = 1;
    public bool Dnf { get; set; }
    /// <summary>Danno riportato dal referto Assetto Corsa (0 = nessun danno).</summary>
    public double Damage { get; set; }
    public int ContractSalary { get; set; }
    public int RoundsInSeason { get; set; } = 1;
    public int SponsorBonus { get; set; }
    public bool SponsorQualified { get; set; }
    public bool Endurance { get; set; }

    /// <summary>
    /// Gradino della scala su cui si corre (1 = kart da noleggio, 7 = vertice).
    ///
    /// I premi erano legati al solo «tier», che è una scala di quattro voci per
    /// sette gradini: dentro «Categoria avanzata» stavano insieme la Formula 3
    /// e i prototipi, e una gara di formula nazionale pagava ottantamila euro.
    /// Il banco di prova trovava carriere con cinque milioni in cassa a metà
    /// scala, dove nessuna scelta economica contava più niente. Zero significa
    /// «non dichiarato»: si torna al tier, come prima.
    /// </summary>
    public int LadderStep { get; set; }

    /// <summary>
    /// Vero quando il pilota è ingaggiato dalla squadra, non un cliente che
    /// paga per correre.
    /// </summary>
    public bool EmployedDriver { get; set; }
}

public sealed class RaceEconomy
{
    public int Prize { get; set; }
    public int Salary { get; set; }
    public int SponsorAward { get; set; }
    public int DamageCost { get; set; }
    public int LogisticsCost { get; set; }
    public int Income => Prize + Salary + SponsorAward;
    public int Costs => DamageCost + LogisticsCost;
    public int Net => Income - Costs;
    public List<string> Lines { get; set; } = [];

    public string Describe() => string.Join("\n", Lines);
}

/// <summary>
/// Economia del weekend. Il modello precedente accreditava 180.000 per una
/// vittoria e 70.000 per qualunque altro arrivo, identici in Rookie e in Formula,
/// e non addebitava nulla: il danno letto dal referto non aveva conseguenze e il
/// budget non era mai un vincolo. Qui premi, costi e premio finale scalano con
/// categoria, gruppo affrontato e risultato reale.
/// </summary>
public static class EconomyEngine
{
    public static RaceEconomy ForRace(RaceEconomyInput input)
    {
        var economy = new RaceEconomy();
        var fieldSize = Math.Max(1, input.FieldSize);
        var basePrize = BasePrize(input.Tier, input.LadderStep);
        var enduranceFactor = input.Endurance ? 1.6 : 1.0;
        // Un premio guadagnato in un gruppo numeroso vale più di uno ottenuto
        // contro due avversari.
        var fieldFactor = Math.Min(1.2, 0.55 + fieldSize * 0.05);

        if (input.Dnf)
        {
            economy.Prize = 0;
            economy.Lines.Add("Ritiro: nessun premio gara.");
        }
        else
        {
            economy.Prize = (int)Math.Round(basePrize * PositionFactor(input.Position, fieldSize) * fieldFactor * enduranceFactor / 500.0) * 500;
            // Il montepremi è della squadra, non del pilota.
            //
            // Chi paga per correre tiene quello che vince: è il suo unico
            // ritorno. Chi è ingaggiato prende invece lo stipendio, e del
            // premio gli resta una quota. Prendendolo tutto, un pilota
            // professionista accumulava milioni che nella carriera non avevano
            // nessun impiego, e il denaro smetteva di essere un vincolo.
            if (input.EmployedDriver)
            {
                economy.Prize = (int)Math.Round(economy.Prize * 0.30 / 100.0) * 100;
                economy.Lines.Add($"Quota pilota sul premio P{input.Position}: € {economy.Prize:N0} (il resto alla squadra)");
            }
            else economy.Lines.Add($"Premio gara P{input.Position}: € {economy.Prize:N0}");
        }

        economy.Salary = Math.Max(0, input.ContractSalary / Math.Max(1, input.RoundsInSeason));
        if (economy.Salary > 0) economy.Lines.Add($"Quota stipendio: € {economy.Salary:N0}");

        // Il bonus dello sponsor non può valere più di due gare vinte.
        //
        // Era una cifra fissa (trentamila euro dalla creazione della carriera)
        // pagata a ogni risultato utile, anche in kart: un ragazzino incassava
        // per un piazzamento cento volte il premio della gara, e in una
        // stagione arrivava a duecentomila euro. È da lì che venivano i milioni
        // trovati dal banco di prova a metà scala. Uno sponsor paga in
        // proporzione alla vetrina, cioè alla categoria.
        var tettoSponsor = Math.Max(150, (int)Math.Round(basePrize * 1.2));
        economy.SponsorAward = input.SponsorQualified ? Math.Clamp(input.SponsorBonus, 0, tettoSponsor) : 0;
        if (economy.SponsorAward > 0) economy.Lines.Add($"Bonus sponsor: € {economy.SponsorAward:N0}");

        economy.DamageCost = DamageCost(input.Tier, input.Damage);
        if (economy.DamageCost > 0) economy.Lines.Add($"Riparazioni dal referto (danno {input.Damage:0.##}): € -{economy.DamageCost:N0}");

        economy.LogisticsCost = (int)Math.Round(LogisticsCost(input.Tier) * enduranceFactor / 100.0) * 100;
        if (economy.LogisticsCost > 0) economy.Lines.Add($"Trasferta e squadra: € -{economy.LogisticsCost:N0}");

        economy.Lines.Add($"Saldo del weekend: € {economy.Net:N0}");
        return economy;
    }

    // I premi seguono le quote: in una gara di club si vincono poche centinaia
    // di euro, e servono a pagare la gara successiva, non a fare capitale.
    private static int BasePrize(string tier, int ladderStep = 0) => ladderStep switch
    {
        // Il premio cresce con la categoria, non a scatti di tier: fra un kart
        // da noleggio e una monoposto internazionale ci sono tre ordini di
        // grandezza, e sono quelli che rendono ogni quota d'iscrizione una
        // decisione invece di una formalità.
        1 => 300,
        2 => 700,
        3 => 1500,
        4 => 6000,
        5 => 20000,
        6 => 60000,
        >= 7 => 200000,
        _ => tier switch
        {
            "Formula / top tier" => 200000,
            "Categoria avanzata" => 80000,
            "Categoria regionale" => 18000,
            _ => 900
        }
    };

    private static int LogisticsCost(string tier) => tier switch
    {
        "Formula / top tier" => 45000,
        "Categoria avanzata" => 18000,
        "Categoria regionale" => 7000,
        _ => 2500
    };

    /// <summary>Stima prudenziale della riparazione massima per la categoria.</summary>
    public static int MaximumDamageCost(string tier) => tier switch
    {
        "Formula / top tier" => 120000,
        "Categoria avanzata" => 45000,
        "Categoria regionale" => 15000,
        _ => 5000
    };

    /// <summary>Costo logistico standard prima di conoscere il referto.</summary>
    public static int StandardLogisticsCost(string tier, bool endurance = false)
    {
        var factor = endurance ? 1.6 : 1.0;
        return (int)Math.Round(LogisticsCost(tier) * factor / 100.0) * 100;
    }

    private static double PositionFactor(int position, int fieldSize)
    {
        if (position <= 0) return 0;
        return position switch
        {
            1 => 1.0,
            2 => 0.72,
            3 => 0.58,
            4 => 0.48,
            5 => 0.40,
            6 => 0.34,
            _ => Math.Max(0.08, 0.34 - (position - 6) * 0.03)
        };
    }

    private static int DamageCost(string tier, double damage)
    {
        if (damage <= 0) return 0;
        // Il referto AC esprime il danno su scale diverse a seconda della build:
        // normalizziamo su 0..1 prima di applicare un costo, così un valore
        // percentuale non produce fatture assurde.
        var normalized = damage > 1 ? Math.Min(1.0, damage / 100.0) : damage;
        var full = tier switch
        {
            "Formula / top tier" => 120000,
            "Categoria avanzata" => 45000,
            "Categoria regionale" => 15000,
            _ => 5000
        };
        return (int)Math.Round(full * normalized / 500.0) * 500;
    }

    /// <summary>
    /// Premio finale di campionato: dipende dalla posizione in classifica e dalla
    /// categoria, non da una soglia di punti assoluta che non scala col calendario.
    /// </summary>
    public static int SeasonAward(string tier, int finalPosition, int fieldSize, int ladderStep = 0)
    {
        // Il premio finale segue la categoria come quello di gara: legato al
        // solo «tier» valeva trecentoquarantamila euro in formula nazionale, e
        // da solo copriva due stagioni di spese.
        var basePrize = BasePrize(tier, ladderStep) * 2;
        if (finalPosition <= 0) return (int)Math.Round(basePrize * 0.1 / 1000.0) * 1000;
        var factor = finalPosition switch
        {
            1 => 3.0,
            2 => 2.0,
            3 => 1.5,
            4 => 1.1,
            5 => 0.9,
            _ => Math.Max(0.15, 0.9 - (finalPosition - 5) * 0.12)
        };
        if (fieldSize > 1 && finalPosition > fieldSize) factor = 0.15;
        return (int)Math.Round(basePrize * factor / 1000.0) * 1000;
    }

    /// <summary>Costo di una stagione fuori pista: usato dalle attività del calendario.</summary>
    public static int SeasonBudgetPressure(string tier, int roundsInSeason) =>
        LogisticsCost(tier) * Math.Max(1, roundsInSeason);
}
