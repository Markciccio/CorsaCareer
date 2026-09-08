namespace CorsaCareer1991;

/// <summary>Come si è chiusa una sessione preparata da CorsaCareer ma non conclusa.</summary>
public enum WithdrawalKind
{
    /// <summary>Il referto è arrivato ma non contiene alcun giro cronometrato.</summary>
    NoTimedLap,
    /// <summary>Il pilota ha abbandonato il weekend: è un ritiro a tutti gli effetti.</summary>
    DriverWithdrawal,
    /// <summary>Annullamento dichiarato per problema tecnico: non consuma il round.</summary>
    TechnicalAnnulment
}

public sealed class WithdrawalOutcome
{
    /// <summary>Vero se il round o il tentativo viene comunque consumato.</summary>
    public bool ConsumesAttempt { get; set; }
    public int Reputation { get; set; }
    public int TeamRelation { get; set; }
    public int SponsorRelation { get; set; }
    public int LogisticsCost { get; set; }
    public string Headline { get; set; } = "";
    public string EventType { get; set; } = "";
    public int Importance { get; set; }
    public List<string> Reasons { get; set; } = [];

    public string Explain() => Reasons.Count == 0 ? "" : string.Join(" · ", Reasons);
}

/// <summary>
/// Conseguenze di una sessione preparata e non portata a termine.
///
/// Serve a chiudere un buco di coerenza: prima uscire a metà sessione non
/// registrava nulla, quindi si poteva rientrare e uscire all'infinito per
/// ripescare condizioni, griglia o un tentativo di valutazione migliore.
///
/// La regola d'integrità del progetto resta intatta: qui non viene inventato
/// nessun tempo e nessuna posizione d'arrivo. Viene registrato un fatto che
/// l'applicazione conosce per certo — ha preparato e lanciato quella sessione, e
/// quella sessione non ha prodotto un risultato utilizzabile. È esattamente un
/// ritiro, e nel motorsport reale un ritiro sta a referto.
/// </summary>
public static class WithdrawalRules
{
    public static WithdrawalOutcome ForTest(WithdrawalKind kind, int attempt)
    {
        var outcome = new WithdrawalOutcome { EventType = "EVALUATION_ABANDONED", Importance = 50 };
        if (kind == WithdrawalKind.TechnicalAnnulment)
        {
            outcome.ConsumesAttempt = false;
            outcome.EventType = "SESSION_ANNULLED";
            outcome.Importance = 18;
            outcome.Headline = "Prova annullata per problema tecnico: nessun tentativo consumato.";
            outcome.Reasons.Add("annullamento tecnico dichiarato");
            return outcome;
        }
        // Una prova iniziata e non completata è un tentativo speso: è il motivo
        // per cui non si può rientrare e uscire fino a ottenere il giro giusto.
        outcome.ConsumesAttempt = true;
        outcome.Reputation = -2;
        outcome.TeamRelation = -3;
        outcome.Reasons.Add($"tentativo {attempt} consumato senza tempo utilizzabile");
        outcome.Reasons.Add("reputazione -2, rapporto col team -3");
        outcome.Headline = kind == WithdrawalKind.NoTimedLap
            ? $"Prova {attempt} chiusa senza alcun giro cronometrato: il tentativo resta a referto."
            : $"Prova {attempt} abbandonata dal pilota: il tentativo viene comunque conteggiato.";
        return outcome;
    }

    public static WithdrawalOutcome ForRace(WithdrawalKind kind, string tier, string grandPrix, int round)
    {
        var outcome = new WithdrawalOutcome { EventType = "RACE_WITHDRAWAL", Importance = 62 };
        if (kind == WithdrawalKind.TechnicalAnnulment)
        {
            outcome.ConsumesAttempt = false;
            outcome.EventType = "SESSION_ANNULLED";
            outcome.Importance = 20;
            outcome.Headline = $"Weekend di {grandPrix} annullato per problema tecnico: il round resta da disputare.";
            outcome.Reasons.Add("annullamento tecnico dichiarato: nessuna conseguenza sportiva");
            return outcome;
        }
        // Il round viene consumato: una gara non si ripete a piacere, e senza
        // questo vincolo il calendario diventa una lotteria da ripescare.
        outcome.ConsumesAttempt = true;
        outcome.Reputation = -6;
        outcome.TeamRelation = -8;
        outcome.SponsorRelation = -6;
        // La trasferta è stata comunque sostenuta.
        outcome.LogisticsCost = LogisticsForTier(tier);
        outcome.Reasons.Add($"round {round} consumato senza referto");
        outcome.Reasons.Add("nessun punto e nessun premio gara");
        outcome.Reasons.Add($"reputazione -6, team -8, sponsor -6, trasferta € -{outcome.LogisticsCost:N0}");
        outcome.Headline = kind == WithdrawalKind.NoTimedLap
            ? $"{grandPrix}: sessione chiusa senza referto utilizzabile, il round va a referto come ritiro."
            : $"{grandPrix}: ritiro dal weekend. Nessun punto, e il round resta disputato.";
        return outcome;
    }

    /// <summary>Costo di trasferta già sostenuto, coerente con l'economia della categoria.</summary>
    public static int LogisticsForTier(string tier) => tier switch
    {
        "Formula / top tier" => 45000,
        "Categoria avanzata" => 18000,
        "Categoria regionale" => 7000,
        _ => 2500
    };

    /// <summary>
    /// Testo della richiesta di conferma. Il giocatore deve sapere prima di
    /// cliccare che l'abbandono ha un costo: è la differenza fra una scelta e
    /// una scappatoia.
    /// </summary>
    public static string ConfirmationText(bool isTest, string grandPrix, string tier)
    {
        if (isTest)
            return "Vuoi abbandonare la prova?\n\n" +
                   "Il tentativo verrà conteggiato comunque, con una penalità di reputazione: " +
                   "una prova iniziata e non completata resta a referto.\n\n" +
                   "Se invece la sessione non è partita per un problema tecnico, scegli l'annullamento tecnico: " +
                   "non consuma il tentativo, ma resta registrato nella carriera.";
        return $"Vuoi ritirarti dal weekend di {grandPrix}?\n\n" +
               $"Il round verrà considerato disputato: nessun punto, nessun premio, " +
               $"reputazione e rapporti in calo, e la trasferta di € {LogisticsForTier(tier):N0} resta a carico.\n\n" +
               "Se invece la sessione non è partita per un problema tecnico, scegli l'annullamento tecnico: " +
               "non consuma il round, ma resta registrato nella carriera.";
    }
}
