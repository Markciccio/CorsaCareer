namespace CorsaCareer;

/// <summary>
/// L'età del pilota, e cosa comporta.
///
/// Prima non esisteva: il pilota non invecchiava mai. La conseguenza non era
/// solo narrativa — la maturazione cresceva e basta, quindi un pilota con
/// ottanta gare restava per sempre più forte di uno con sessanta, e una
/// carriera arrivata in cima ripeteva la stessa stagione all'infinito. Al banco
/// se ne vedevano diciassette identiche di fila, tutte seconde o terze.
///
/// Una carriera vera ha una forma: si cresce, si arriva al proprio massimo
/// intorno ai trent'anni, si resta lì qualche stagione, poi si comincia a
/// perdere qualcosa e a un certo punto si smette. È quella forma che rende un
/// campionato vinto a trentadue anni diverso da uno vinto a ventidue, e che dà
/// un senso all'ultima stagione.
/// </summary>
public static class DriverAge
{
    /// <summary>Età con cui si comincia, se il profilo non ne dichiara una.</summary>
    public const int EtaIniziale = 16;

    /// <summary>L'età in cui un pilota è al massimo delle sue possibilità.</summary>
    public const int Apice = 29;

    /// <summary>Fino a qui il declino non si vede: sono gli anni della maturità piena.</summary>
    public const int InizioDeclino = 34;

    /// <summary>Oltre questa età correre a certi livelli diventa raro.</summary>
    public const int EtaLimite = 45;

    /// <summary>
    /// Quanto l'età aggiunge o toglie al passo, sulla stessa scala del livello IA.
    ///
    /// Da giovane manca il mestiere, non la velocità: la penalità è modesta e si
    /// chiude in fretta. Dopo l'apice il calo è lento — mezzo punto all'anno —
    /// perché un pilota esperto compensa a lungo con la testa quello che perde
    /// nei riflessi. È solo dopo i quaranta che diventa evidente.
    /// </summary>
    public static double Passo(int eta)
    {
        if (eta <= 0) return 0;
        if (eta < Apice)
        {
            // Da sedici a ventinove: si recupera un punto e mezzo di svantaggio.
            var mancanti = Apice - eta;
            return -Math.Min(3.0, mancanti * 0.23);
        }
        if (eta <= InizioDeclino) return 0;
        var anniOltre = eta - InizioDeclino;
        // Mezzo punto l'anno fino a quaranta, poi il doppio.
        return -(Math.Min(6, anniOltre) * 0.5 + Math.Max(0, anniOltre - 6) * 1.1);
    }

    /// <summary>
    /// Quanto pesa l'età sul recupero fisico: un pilota di quarant'anni si
    /// riprende più lentamente da un weekend.
    /// </summary>
    public static double FattoreRecupero(int eta) =>
        eta <= InizioDeclino ? 1.0 : Math.Max(0.55, 1.0 - (eta - InizioDeclino) * 0.045);

    /// <summary>La fase della carriera, detta come la direbbe un cronista.</summary>
    public static string Fase(int eta) => eta switch
    {
        <= 0 => "",
        < 20 => "un ragazzo",
        < 24 => "un giovane in crescita",
        < Apice => "nel pieno della maturazione",
        <= InizioDeclino => "nel pieno della carriera",
        < 39 => "un veterano che sa ancora vincere",
        < EtaLimite => "a fine carriera",
        _ => "oltre il tempo massimo"
    };

    /// <summary>
    /// Se è arrivato il momento di smettere, e perché.
    ///
    /// Non è solo l'età: si smette quando l'età si somma a una ragione. Chi
    /// vince ancora resta, chi non trova più un sedile e ha superato i quaranta
    /// no. La decisione resta comunque del giocatore — questa funzione dice solo
    /// quando la domanda va posta.
    /// </summary>
    public static string? MotivoDelRitiro(int eta, int stagioniSenzaVittorie, bool senzaSedile, bool alVertice)
    {
        if (eta <= 0) return null;
        if (eta >= EtaLimite)
            return $"Hai {eta} anni. A questo livello non corre più nessuno della tua età, e il tuo corpo te lo sta dicendo da un pezzo.";
        if (eta >= 40 && senzaSedile)
            return $"Hai {eta} anni e nessuna squadra ti ha offerto un sedile. Non è un caso, ed è il modo in cui finiscono quasi tutte le carriere.";
        if (eta >= 38 && stagioniSenzaVittorie >= 4)
            return $"Hai {eta} anni e sono {stagioniSenzaVittorie} stagioni che non vinci. Puoi continuare, ma la domanda è se ha ancora senso.";
        if (eta >= 36 && alVertice && stagioniSenzaVittorie == 0)
            return null; // chi vince al vertice non si ritira: è il momento migliore per restare
        return null;
    }
}
