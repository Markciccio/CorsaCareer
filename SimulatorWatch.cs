using System.Diagnostics;

namespace CorsaCareer1991;

public enum SimulatorVerdict
{
    /// <summary>Il referto è disponibile: decide la normale importazione.</summary>
    ResultAvailable,
    /// <summary>Il simulatore è in esecuzione: sessione in corso.</summary>
    Running,
    /// <summary>Sessione lanciata da poco: il simulatore potrebbe non essere ancora partito.</summary>
    WaitingForStart,
    /// <summary>Il simulatore non è mai comparso: probabilmente non è stato avviato.</summary>
    NeverStarted,
    /// <summary>Il simulatore è stato visto in esecuzione e poi è terminato senza referto.</summary>
    ClosedWithoutResult
}

/// <summary>
/// Rileva se una sessione preparata da CorsaCareer si è chiusa senza produrre un
/// referto.
///
/// Prima l'unico segnale era il file dei risultati: se Assetto Corsa non scriveva
/// nulla, l'applicazione non poteva distinguere «sta ancora guidando» da «ha
/// chiuso tutto un'ora fa», e il weekend restava pendente in silenzio.
///
/// Osservare il processo del simulatore aggiunge un segnale certo: se è stato
/// visto in esecuzione e poi è terminato senza referto, la sessione è finita
/// senza risultato. La macchina a stati è pura — riceve osservazioni, non le
/// raccoglie — così il comportamento è verificabile senza avviare processi.
/// </summary>
public sealed class SimulatorWatch
{
    /// <summary>
    /// Attesa concessa prima di considerare che il simulatore non sia partito.
    /// Content Manager e Assetto Corsa possono impiegare parecchio a caricare, e
    /// scambiare un avvio lento per un abbandono sarebbe peggio del problema.
    /// </summary>
    public static readonly TimeSpan DefaultStartupGrace = TimeSpan.FromMinutes(3);

    private readonly DateTime launchUtc;
    private readonly TimeSpan startupGrace;

    public SimulatorWatch(DateTime launchUtc, TimeSpan? startupGrace = null)
    {
        this.launchUtc = launchUtc;
        this.startupGrace = startupGrace ?? DefaultStartupGrace;
    }

    /// <summary>Vero se il simulatore è stato osservato almeno una volta in esecuzione.</summary>
    public bool EverSeenRunning { get; private set; }

    public SimulatorVerdict Observe(bool simulatorRunning, bool resultAvailable, DateTime nowUtc)
    {
        // Un referto disponibile vince su tutto: la sessione ha prodotto un esito.
        if (resultAvailable) return SimulatorVerdict.ResultAvailable;
        if (simulatorRunning)
        {
            EverSeenRunning = true;
            return SimulatorVerdict.Running;
        }
        // Visto partire e ora chiuso, senza referto: la sessione è finita a vuoto.
        if (EverSeenRunning) return SimulatorVerdict.ClosedWithoutResult;
        return nowUtc - launchUtc < startupGrace ? SimulatorVerdict.WaitingForStart : SimulatorVerdict.NeverStarted;
    }

    /// <summary>
    /// Riavvio della stessa sessione: il simulatore va di nuovo atteso, altrimenti
    /// il primo giro di osservazione dopo la riapertura verrebbe letto come un
    /// nuovo abbandono.
    /// </summary>
    public void Rearm() => EverSeenRunning = false;

    public static string Describe(SimulatorVerdict verdict) => verdict switch
    {
        SimulatorVerdict.ResultAvailable => "referto disponibile",
        SimulatorVerdict.Running => "sessione in corso in Assetto Corsa",
        SimulatorVerdict.WaitingForStart => "in attesa dell'avvio del simulatore",
        SimulatorVerdict.NeverStarted => "il simulatore non è mai stato avviato",
        _ => "il simulatore è stato chiuso senza produrre un referto"
    };
}

/// <summary>
/// Sonda del processo del simulatore. È l'unica parte impura del rilevamento e
/// non solleva eccezioni: se l'elenco dei processi non è consultabile risponde
/// «non in esecuzione», così un ambiente ristretto non blocca la carriera.
/// </summary>
public static class SimulatorProcess
{
    /// <summary>Nomi con cui Assetto Corsa può comparire fra i processi.</summary>
    public static readonly string[] KnownNames = ["acs", "acs_x64", "AssettoCorsa"];

    public static bool IsRunning()
    {
        foreach (var name in KnownNames)
        {
            try
            {
                var found = Process.GetProcessesByName(name);
                try { if (found.Length > 0) return true; }
                finally { foreach (var process in found) process.Dispose(); }
            }
            catch (Exception error)
            {
                CareerLog.Warn("simulatore", $"elenco processi non consultabile per «{name}»: {error.Message}");
                return false;
            }
        }
        return false;
    }
}
