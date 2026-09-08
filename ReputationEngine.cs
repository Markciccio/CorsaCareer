namespace CorsaCareer1991;

public sealed class ReputationInput
{
    public int Position { get; set; }
    public int FieldSize { get; set; } = 1;
    public bool Dnf { get; set; }
    public int QualifyingPosition { get; set; }
    public int TeammatePosition { get; set; }
    public int CurrentReputation { get; set; }
    public string Tier { get; set; } = "Rookie";
    /// <summary>Posizione attesa dal contratto/prestigio del sedile. 0 = nessuna aspettativa dichiarata.</summary>
    public int ExpectedPosition { get; set; }
}

public sealed record ReputationChange(int Delta, int Reputation, List<string> Reasons)
{
    public string Explain() => Reasons.Count == 0 ? "Nessun effetto sulla reputazione." : string.Join(" · ", Reasons);
}

/// <summary>
/// Traduce un referto reale in variazione di reputazione. Il modello precedente
/// assegnava +8 alla vittoria e +2 a qualunque altro arrivo, rendendo un secondo
/// posto equivalente a un ventesimo. Qui contano posizione relativa al gruppo,
/// aspettativa del sedile, confronto col compagno, qualifica e categoria.
/// </summary>
public static class ReputationEngine
{
    public static ReputationChange Evaluate(ReputationInput input)
    {
        var reasons = new List<string>();
        var fieldSize = Math.Max(1, input.FieldSize);
        double delta;

        if (input.Dnf)
        {
            delta = -4;
            reasons.Add("ritiro: -4");
        }
        else
        {
            // Posizione relativa: 0 = vittoria, 1 = ultimo. Un piazzamento va
            // giudicato sul gruppo affrontato, non su un numero assoluto.
            var ratio = fieldSize <= 1 ? 0.0 : (input.Position - 1) / (double)(fieldSize - 1);
            delta = input.Position switch
            {
                1 => 10,
                2 => 7,
                3 => 6,
                _ => ratio <= 0.25 ? 4 : ratio <= 0.5 ? 2 : ratio <= 0.75 ? 0 : -2
            };
            reasons.Add($"arrivo P{input.Position} su {fieldSize}: {delta:+0;-0;0}");
        }

        if (input.ExpectedPosition > 0 && !input.Dnf)
        {
            // Battere l'aspettativa del sedile pesa più del piazzamento assoluto:
            // un quinto posto con una macchina da decimo è un risultato.
            var margin = input.ExpectedPosition - input.Position;
            var expectationBonus = margin switch
            {
                >= 5 => 4,
                >= 2 => 2,
                >= 0 => 1,
                >= -3 => -1,
                _ => -3
            };
            delta += expectationBonus;
            reasons.Add($"aspettativa P{input.ExpectedPosition}: {expectationBonus:+0;-0;0}");
        }

        if (input.TeammatePosition > 0 && input.Position > 0)
        {
            var teammateBonus = input.Position < input.TeammatePosition ? 3 : input.Position > input.TeammatePosition ? -3 : 0;
            if (teammateBonus != 0)
            {
                delta += teammateBonus;
                reasons.Add($"confronto col compagno: {teammateBonus:+0;-0;0}");
            }
        }

        if (input.QualifyingPosition is > 0 and <= 3 && !input.Dnf)
        {
            delta += 2;
            reasons.Add("qualifica nei primi tre: +2");
        }

        var tierWeight = TierWeight(input.Tier);
        if (Math.Abs(tierWeight - 1.0) > 0.001)
        {
            delta *= tierWeight;
            reasons.Add($"categoria {input.Tier}: ×{tierWeight:0.##}");
        }

        // Rendimenti decrescenti: costruire gli ultimi punti di reputazione deve
        // costare più dei primi, mentre le perdite restano piene.
        if (delta > 0)
        {
            var damping = Math.Clamp(1.0 - input.CurrentReputation / 130.0, 0.25, 1.0);
            if (damping < 0.999)
            {
                delta *= damping;
                reasons.Add($"reputazione già a {input.CurrentReputation}/100: ×{damping:0.##}");
            }
        }

        var rounded = (int)Math.Round(delta, MidpointRounding.AwayFromZero);
        var reputation = Math.Clamp(input.CurrentReputation + rounded, 0, 100);
        var applied = reputation - input.CurrentReputation;
        return new ReputationChange(applied, reputation, reasons);
    }

    private static double TierWeight(string tier) => tier switch
    {
        "Formula / top tier" => 1.4,
        "Categoria avanzata" => 1.2,
        "Categoria regionale" => 1.0,
        _ => 0.85
    };

    /// <summary>
    /// Posizione attesa da un sedile: più il progetto è prestigioso, più in alto
    /// deve arrivare il pilota perché il risultato sia considerato normale.
    /// </summary>
    public static int ExpectedPosition(int prestige, int fieldSize)
    {
        if (fieldSize <= 1) return 0;
        var clampedPrestige = Math.Clamp(prestige, 0, 100);
        // Prestigio 90 → attesa vicino alla vittoria; prestigio 20 → metà gruppo bassa.
        var expected = (int)Math.Round(1 + (100 - clampedPrestige) / 100.0 * (fieldSize - 1) * 0.8);
        return Math.Clamp(expected, 1, fieldSize);
    }
}
