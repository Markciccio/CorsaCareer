namespace CorsaCareer1991;

/// <summary>
/// Esito di un tentativo di spesa.
/// </summary>
public sealed class PaymentResult
{
    public bool Paid { get; init; }
    public int Amount { get; init; }
    public int BalanceAfter { get; init; }
    /// <summary>Perché non è stato possibile pagare. Vuoto se il pagamento è andato.</summary>
    public string Refusal { get; init; } = "";
}

/// <summary>
/// La cassa della carriera.
///
/// Il denaro usciva da cinque punti diversi del programma e uno solo controllava
/// il saldo: bastava iscriversi a una gara per finire a meno milleseicento euro.
/// Un patrimonio negativo non significa niente — non esiste un prestito, non
/// esiste un debito da ripagare — ed è il tipo di stato che rende insensato
/// tutto quello che ci si costruisce sopra.
///
/// Ogni uscita passa da qui. Se i soldi non ci sono, la spesa non avviene e
/// viene detto perché.
/// </summary>
public static class CareerWallet
{
    /// <summary>
    /// Paga, se possibile. Restituisce l'esito senza mai portare la cassa sotto
    /// zero: un pagamento che non si può sostenere semplicemente non avviene.
    /// </summary>
    public static PaymentResult TryPay(CareerState career, int amount, string reason)
    {
        if (amount <= 0)
            return new PaymentResult { Paid = true, Amount = 0, BalanceAfter = career.Cash };

        if (amount > career.Cash)
        {
            var missing = amount - career.Cash;
            return new PaymentResult
            {
                Paid = false,
                Amount = amount,
                BalanceAfter = career.Cash,
                Refusal = $"Servono € {amount:N0} per {reason} e in cassa ce ne sono € {career.Cash:N0}: mancano € {missing:N0}."
            };
        }

        career.Cash -= amount;
        return new PaymentResult { Paid = true, Amount = amount, BalanceAfter = career.Cash };
    }

    /// <summary>
    /// Vero se il pilota può permettersi una spesa restando sopra la soglia di
    /// sopravvivenza. Serve a decidere se proporre un'opportunità, non a
    /// impedirla: si può scegliere di rischiare, ma va detto.
    /// </summary>
    public static bool CanAffordComfortably(CareerState career, int amount) =>
        career.Cash - amount >= CareerFinances.SurvivalFloor;

    /// <summary>
    /// L'avviso da mostrare quando la cassa è al limite. Stringa vuota quando
    /// non c'è niente da segnalare: un avviso permanente smette di essere letto.
    /// </summary>
    public static string Warning(CareerState career)
    {
        var cash = career.Cash;
        if (cash < 0)
            // Non deve poter accadere: se accade, va detto invece di nasconderlo.
            return $"Il conto è in rosso di € {Math.Abs(cash):N0}. Questo non dovrebbe succedere: segnala la cosa.";
        if (cash < CareerFinances.SurvivalFloor)
            return "Non resta abbastanza per iscriversi a niente. Cerca uno sponsor o guadagna qualcosa "
                   + "con le attività fra i weekend: senza soldi non si scende in pista.";
        if (cash < CareerFinances.SurvivalFloor * 3)
            return "La cassa è quasi finita. Un'altra iscrizione e resti a piedi: converrebbe trovare "
                   + "uno sponsor prima di impegnare altri soldi.";
        return "";
    }

    /// <summary>Vero se la situazione economica richiede un avviso visibile.</summary>
    public static bool NeedsWarning(CareerState career) => Warning(career).Length > 0;
}
