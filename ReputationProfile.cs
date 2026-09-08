namespace CorsaCareer;

/// <summary>
/// Reputazione del pilota su più dimensioni indipendenti.
///
/// Prima esisteva un unico valore <c>Reputation</c> (più i rapporti con team e
/// sponsor): non permetteva di distinguere un pilota che i team stimano da uno
/// che il pubblico ama, né di premiare un risultato ottenuto con una vettura
/// debole nel modo in cui lo premierebbe la stampa. Ogni dimensione risponde a
/// cose diverse e apre porte diverse.
/// </summary>
public sealed class ReputationProfile
{
    /// <summary>Fiducia dei team: apre sedili e contratti.</summary>
    public int TeamTrust { get; set; } = 30;
    /// <summary>Interesse degli sponsor: apre finanziamenti.</summary>
    public int SponsorAppeal { get; set; } = 25;
    /// <summary>Popolarità presso il pubblico: apre eventi e ingaggi promozionali.</summary>
    public int PublicPopularity { get; set; } = 10;
    /// <summary>Considerazione della stampa: apre visibilità e attenzione dei talent scout.</summary>
    public int PressStanding { get; set; } = 15;
    /// <summary>Prestigio sportivo: è ciò che conta per le categorie superiori.</summary>
    public int SportingPrestige { get; set; } = 20;
    /// <summary>Affidabilità e professionalità: si perde con ritiri, abbandoni e passi falsi.</summary>
    public int Professionalism { get; set; } = 50;

    public ReputationProfile Clone() => new()
    {
        TeamTrust = TeamTrust, SponsorAppeal = SponsorAppeal, PublicPopularity = PublicPopularity,
        PressStanding = PressStanding, SportingPrestige = SportingPrestige, Professionalism = Professionalism
    };

    /// <summary>Indice sintetico, usato solo dove serve un numero unico di comodo.</summary>
    public int Overall => (int)Math.Round(
        SportingPrestige * 0.30 + TeamTrust * 0.25 + PressStanding * 0.15 +
        SponsorAppeal * 0.15 + PublicPopularity * 0.08 + Professionalism * 0.07);

    /// <summary>
    /// Allinea i campi legacy ancora letti da alcune schede e dai salvataggi
    /// precedenti. Le dimensioni nuove restano la fonte di verità.
    /// </summary>
    public void SyncLegacyFields(CareerState career)
    {
        career.Reputation = Overall;
        career.TeamRelation = Math.Clamp(TeamTrust, 0, 100);
        career.SponsorRelation = Math.Clamp(SponsorAppeal, 0, 100);
        career.Fanbase = Math.Max(0, PublicPopularity);
    }

    public int Of(ReputationKind kind) => kind switch
    {
        ReputationKind.TeamTrust => TeamTrust,
        ReputationKind.SponsorAppeal => SponsorAppeal,
        ReputationKind.PublicPopularity => PublicPopularity,
        ReputationKind.PressStanding => PressStanding,
        ReputationKind.SportingPrestige => SportingPrestige,
        _ => Professionalism
    };

    public void Set(ReputationKind kind, int value)
    {
        value = Math.Clamp(value, 0, 100);
        switch (kind)
        {
            case ReputationKind.TeamTrust: TeamTrust = value; break;
            case ReputationKind.SponsorAppeal: SponsorAppeal = value; break;
            case ReputationKind.PublicPopularity: PublicPopularity = value; break;
            case ReputationKind.PressStanding: PressStanding = value; break;
            case ReputationKind.SportingPrestige: SportingPrestige = value; break;
            default: Professionalism = value; break;
        }
    }

    public static string Label(ReputationKind kind) => kind switch
    {
        ReputationKind.TeamTrust => "fiducia dei team",
        ReputationKind.SponsorAppeal => "interesse degli sponsor",
        ReputationKind.PublicPopularity => "livello influencer",
        ReputationKind.PressStanding => "considerazione della stampa",
        ReputationKind.SportingPrestige => "prestigio sportivo",
        _ => "affidabilità"
    };

    public string Describe() => string.Join(" · ", Enum.GetValues<ReputationKind>()
        .Select(kind => $"{Label(kind)} {Of(kind)}"));
}

public enum ReputationKind
{
    TeamTrust,
    SponsorAppeal,
    PublicPopularity,
    PressStanding,
    SportingPrestige,
    Professionalism
}

public sealed class ReputationShift
{
    public Dictionary<ReputationKind, int> Deltas { get; set; } = [];
    public List<string> Reasons { get; set; } = [];

    public int this[ReputationKind kind] => Deltas.TryGetValue(kind, out var value) ? value : 0;

    public void Add(ReputationKind kind, int delta, string reason)
    {
        if (delta == 0) return;
        Deltas[kind] = this[kind] + delta;
        Reasons.Add($"{ReputationProfile.Label(kind)} {delta:+#;-#;0}: {reason}");
    }

    public string Explain() => Reasons.Count == 0 ? "Nessun effetto sulla reputazione." : string.Join("\n", Reasons.Select(x => "• " + x));

    /// <summary>Riassunto compatto delle sole dimensioni mosse.</summary>
    public string Summary() => Deltas.Count == 0
        ? "nessuna variazione"
        : string.Join(", ", Deltas.OrderByDescending(x => Math.Abs(x.Value))
            .Select(x => $"{ReputationProfile.Label(x.Key)} {x.Value:+#;-#;0}"));
}

/// <summary>
/// Traduce un risultato reale in variazioni su tutte le dimensioni della
/// reputazione. Ogni dimensione reagisce a un aspetto diverso dello stesso
/// referto, ed è questo che rende le opportunità future differenziate.
/// </summary>
public static class ReputationDynamics
{
    public sealed class Input
    {
        public int Position { get; set; }
        public int FieldSize { get; set; } = 1;
        public bool Dnf { get; set; }
        public bool Abandoned { get; set; }
        public int QualifyingPosition { get; set; }
        public int StartingPosition { get; set; }
        public int TeammatePosition { get; set; }
        public int ExpectedPosition { get; set; }
        public string Tier { get; set; } = "Rookie";
        /// <summary>Competitività della vettura, 0-100. Bassa = risultato più notevole.</summary>
        public int CarCompetitiveness { get; set; } = 50;
        public double Damage { get; set; }
        public double PenaltySeconds { get; set; }
        public int PointlessStreak { get; set; }
        public bool WonChampionship { get; set; }
    }

    public static ReputationShift ForRace(Input input, ReputationProfile current)
    {
        var shift = new ReputationShift();
        var field = Math.Max(1, input.FieldSize);

        if (input.Abandoned)
        {
            // Un weekend non concluso è prima di tutto un problema di affidabilità.
            shift.Add(ReputationKind.Professionalism, -8, "weekend abbandonato senza referto");
            shift.Add(ReputationKind.TeamTrust, -6, "la squadra ha speso una trasferta a vuoto");
            shift.Add(ReputationKind.SponsorAppeal, -4, "nessuna visibilità restituita allo sponsor");
            return shift;
        }

        if (input.Dnf)
        {
            shift.Add(ReputationKind.Professionalism, input.Damage > 0.3 ? -6 : -3,
                input.Damage > 0.3 ? "ritiro con danno rilevato dal referto" : "ritiro senza danno");
            shift.Add(ReputationKind.TeamTrust, -3, "gara chiusa senza portare dati utili");
            shift.Add(ReputationKind.SportingPrestige, -2, "nessun risultato da mettere a curriculum");
            return shift;
        }

        var ratio = field <= 1 ? 0.0 : (input.Position - 1) / (double)(field - 1);
        var tierWeight = TierWeight(input.Tier);

        // --- prestigio sportivo: conta il piazzamento in assoluto
        var prestige = input.Position switch
        {
            1 => 9, 2 => 6, 3 => 5,
            _ => ratio <= 0.25 ? 3 : ratio <= 0.5 ? 1 : ratio <= 0.8 ? 0 : -2
        };
        shift.Add(ReputationKind.SportingPrestige, Scale(prestige, tierWeight, current.SportingPrestige),
            $"arrivo P{input.Position} su {field} in {input.Tier}");
        if (input.WonChampionship) shift.Add(ReputationKind.SportingPrestige, 15, "titolo di campionato conquistato");

        // --- stampa: premia ciò che è notevole, non ciò che è previsto
        var pressed = 0;
        if (input.ExpectedPosition > 0 && input.Position < input.ExpectedPosition) pressed += Math.Min(6, (input.ExpectedPosition - input.Position));
        // Un risultato ottenuto con una vettura debole è la notizia per eccellenza.
        if (input.CarCompetitiveness <= 40 && input.Position <= Math.Max(3, field / 3)) pressed += 5;
        if (input.StartingPosition > 0 && input.Position > 0 && input.StartingPosition - input.Position >= 4) pressed += 4;
        if (input.PointlessStreak >= 3 && input.Position <= field / 2) pressed += 3;
        if (pressed > 0) shift.Add(ReputationKind.PressStanding, Scale(pressed, tierWeight, current.PressStanding),
            input.CarCompetitiveness <= 40 ? "risultato notevole con una vettura non competitiva" : "risultato oltre le attese");
        else if (ratio > 0.8) shift.Add(ReputationKind.PressStanding, -2, "prestazione che non lascia traccia");

        // --- popolarità: premia lo spettacolo, cioè i sorpassi e il podio
        var recovered = input.StartingPosition > 0 && input.Position > 0 ? input.StartingPosition - input.Position : 0;
        var popular = 0;
        if (input.Position <= 3) popular += 4;
        if (recovered >= 3) popular += Math.Min(6, recovered);
        if (input.Position == 1) popular += 3;
        // Senza questo un pilota di centro gruppo restava a popolarita zero per
        // sempre, e gli ingaggi promozionali non si sbloccavano mai.
        else if (ratio <= 0.35) popular += 1;
        if (popular > 0) shift.Add(ReputationKind.PublicPopularity, popular,
            recovered >= 3 ? $"{recovered} posizioni recuperate in gara" : "presenza sul podio");

        // --- fiducia dei team: confronto interno e aspettativa del sedile
        if (input.TeammatePosition > 0)
            shift.Add(ReputationKind.TeamTrust, input.Position < input.TeammatePosition ? 5 : -4,
                input.Position < input.TeammatePosition ? "compagno di squadra battuto" : "battuto dal compagno di squadra");
        if (input.ExpectedPosition > 0)
        {
            var margin = input.ExpectedPosition - input.Position;
            if (margin >= 2) shift.Add(ReputationKind.TeamTrust, 3, "aspettativa del sedile superata");
            else if (margin <= -3) shift.Add(ReputationKind.TeamTrust, -3, "aspettativa del sedile mancata");
        }

        // --- sponsor: visibilità, quindi podio e posizioni alte
        // Una vittoria vale più di un podio generico: è la visibilità che uno
        // sponsor compra, e due successi devono bastare ad attirare un'offerta.
        if (input.Position == 1) shift.Add(ReputationKind.SponsorAppeal, 7, "vittoria: massima visibilità per lo sponsor");
        else if (input.Position <= 3) shift.Add(ReputationKind.SponsorAppeal, 5, "podio: visibilità restituita allo sponsor");
        else if (input.Position <= Math.Max(6, field / 3)) shift.Add(ReputationKind.SponsorAppeal, 2, "posizione utile per la visibilità");
        else if (ratio > 0.8) shift.Add(ReputationKind.SponsorAppeal, -2, "nessuna visibilità dalla gara");

        // --- affidabilità: gara pulita o penalità
        if (input.PenaltySeconds > 0) shift.Add(ReputationKind.Professionalism, -3, $"{input.PenaltySeconds:0.#} s di penalità");
        else if (input.Damage <= 0) shift.Add(ReputationKind.Professionalism, 2, "gara completata senza danni né penalità");
        if (input.Damage > 0.3) shift.Add(ReputationKind.Professionalism, -2, "materiale danneggiato");

        return shift;
    }

    /// <summary>Variazioni per un evento che non è una gara (test, agenda, mercato).</summary>
    public static ReputationShift ForTest(bool passed, bool close, bool noTimedLap)
    {
        var shift = new ReputationShift();
        if (noTimedLap)
        {
            shift.Add(ReputationKind.Professionalism, -5, "prova chiusa senza alcun giro cronometrato");
            shift.Add(ReputationKind.TeamTrust, -3, "giornata di test spesa senza dati");
            return shift;
        }
        if (passed)
        {
            shift.Add(ReputationKind.TeamTrust, 8, "riferimento del team centrato nella prova");
            shift.Add(ReputationKind.SportingPrestige, 4, "valutazione superata");
            shift.Add(ReputationKind.PressStanding, 3, "il cronometro ha convinto gli osservatori");
            return shift;
        }
        if (close)
        {
            shift.Add(ReputationKind.TeamTrust, 3, "prova chiusa vicino al riferimento");
            return shift;
        }
        shift.Add(ReputationKind.TeamTrust, -2, "prova lontana dal riferimento richiesto");
        return shift;
    }

    /// <summary>
    /// Applica uno scostamento al profilo con rendimenti decrescenti sui guadagni:
    /// costruire gli ultimi punti di credibilità costa più dei primi.
    /// </summary>
    public static ReputationProfile Apply(ReputationProfile profile, ReputationShift shift)
    {
        var updated = profile.Clone();
        foreach (var (kind, delta) in shift.Deltas)
        {
            var currentValue = updated.Of(kind);
            var applied = delta;
            if (delta > 0)
            {
                var damping = Math.Clamp(1.0 - currentValue / 130.0, 0.25, 1.0);
                applied = Math.Max(1, (int)Math.Round(delta * damping));
            }
            updated.Set(kind, currentValue + applied);
        }
        return updated;
    }

    private static int Scale(int value, double tierWeight, int currentValue)
    {
        if (value == 0) return 0;
        var scaled = value * tierWeight;
        return (int)Math.Round(scaled, MidpointRounding.AwayFromZero);
    }

    private static double TierWeight(string tier) => tier switch
    {
        "Formula / top tier" => 1.4,
        "Categoria avanzata" => 1.2,
        "Categoria regionale" => 1.0,
        _ => 0.8
    };
}
