using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CorsaCareer1991;

/// <summary>Riproduzione locale non automatica dei brani della carriera.</summary>
public static class SoundtrackService
{
    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    private static extern int mciSendString(string command, IntPtr returnValue, int returnLength, IntPtr callback);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateJobObject(IntPtr security, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(IntPtr job, int infoClass, ref JobObjectExtendedLimitInformation info, int length);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectBasicLimitInformation
    {
        public long PerProcessUserTimeLimit, PerJobUserTimeLimit;
        public int LimitFlags, MinimumWorkingSetSize, MaximumWorkingSetSize, ActiveProcessLimit;
        public IntPtr Affinity;
        public int PriorityClass, SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectExtendedLimitInformation
    {
        public JobObjectBasicLimitInformation BasicLimitInformation;
        public long ReadOperationCount, WriteOperationCount, OtherOperationCount;
        public long ReadTransferCount, WriteTransferCount, OtherTransferCount;
        public UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
    }

    /// <summary>
    /// ffplay viene avviato con "-loop 0" e non termina da solo. Una chiusura
    /// regolare passa da Stop, ma un arresto anomalo (crash, Gestione attività)
    /// lasciava il player in esecuzione: musica senza finestra, invisibile e da
    /// terminare a mano. Il Job Object lo fa chiudere dal sistema operativo
    /// insieme all'applicazione, qualunque sia il modo in cui finisce.
    /// </summary>
    private static readonly IntPtr PlayerJob = CreateKillOnCloseJob();

    private const int ExtendedLimitInformation = 9;
    private const int LimitKillOnJobClose = 0x2000;

    private static IntPtr CreateKillOnCloseJob()
    {
        try
        {
            var job = CreateJobObject(IntPtr.Zero, null);
            if (job == IntPtr.Zero) return IntPtr.Zero;
            var info = new JobObjectExtendedLimitInformation();
            info.BasicLimitInformation.LimitFlags = LimitKillOnJobClose;
            return SetInformationJobObject(job, ExtendedLimitInformation, ref info, Marshal.SizeOf(info))
                ? job
                : IntPtr.Zero;
        }
        catch { return IntPtr.Zero; }
    }

    /// <summary>
    /// Lega un player esterno alla vita dell'applicazione, se il sistema lo
    /// permette. Usato anche dalla narrazione: il Job Object e' unico per
    /// l'intero processo, quindi vale per qualunque figlio audio.
    /// </summary>
    public static void BindToApplicationLifetime(Process? process)
    {
        if (process == null) return;

        // Il Job Object e' la difesa migliore — chiude i figli anche se
        // l'applicazione muore male — ma puo fallire, e prima falliva in
        // silenzio. Vengono tenuti anche i riferimenti, cosi la chiusura
        // regolare non dipende dal sistema operativo.
        lock (Children)
        {
            // HasExited lancia se il processo non ha mai avviato niente o e'
            // gia stato smaltito: interrogare un processo esterno e' sempre
            // un'operazione che puo fallire, e senza protezione faceva cadere
            // l'intera applicazione.
            Children.RemoveAll(x => HasFinished(x));
            Children.Add(process);
        }

        if (PlayerJob == IntPtr.Zero)
        {
            CareerLog.Warn("audio", "Job Object non disponibile: i player esterni verranno chiusi singolarmente.");
            return;
        }
        try
        {
            if (!AssignProcessToJobObject(PlayerJob, process.Handle))
                CareerLog.Warn("audio", $"aggancio al Job Object non riuscito per il processo {process.Id}.");
        }
        catch (Exception error)
        {
            CareerLog.Warn("audio", $"aggancio al Job Object non riuscito: {error.Message}");
        }
    }

    /// <summary>
    /// I player esterni avviati da questo processo. Serve a poterli chiudere
    /// anche quando il Job Object non ha funzionato.
    /// </summary>
    private static readonly List<Process> Children = [];

    /// <summary>
    /// Vero se il processo non e' piu vivo, oppure se non e' possibile saperlo.
    /// Un processo che non risponde va trattato come finito: e' l'unica lettura
    /// che non puo far cadere l'applicazione.
    /// </summary>
    private static bool HasFinished(Process? process)
    {
        if (process == null) return true;
        try { return process.HasExited; }
        catch { return true; }
    }

    /// <summary>
    /// Termina ogni player esterno ancora vivo. Si puo chiamare piu volte: le
    /// vie d'uscita dell'applicazione sono molte e nessuna sa se un'altra
    /// l'ha gia fatto.
    /// </summary>
    public static void KillExternalPlayers()
    {
        lock (Children)
        {
            foreach (var child in Children)
            {
                try
                {
                    if (!HasFinished(child)) child.Kill(entireProcessTree: true);
                }
                catch { }
                try { child.Dispose(); } catch { }
            }
            Children.Clear();
        }
    }

    /// <summary>
    /// Il brano suona una volta e finisce.
    ///
    /// In ripetizione infinita diventava un rumore di fondo continuo, e un
    /// lettore che non termina mai e' anche la causa dei processi audio rimasti
    /// vivi dopo la chiusura del programma: con "-loop 0" ffplay non esce mai
    /// da solo.
    ///
    /// La ripetizione va chiesta ai due lettori con sintassi diverse —
    /// "repeat" per MCI, "-loop 0" per ffplay — e niente le teneva allineate:
    /// per questo la decisione sta qui, in un punto solo.
    /// </summary>
    public const bool LoopSoundtrack = false;

    /// <summary>Comando di ripetizione per MCI.</summary>
    public static string MciRepeatFlag => LoopSoundtrack ? " repeat" : "";

    /// <summary>
    /// Argomento di ripetizione per ffplay. Con "-loop 0" il brano riparte
    /// all'infinito; senza, finisce dopo la prima esecuzione.
    /// </summary>
    public static string FfplayLoopArgument => LoopSoundtrack ? "-loop 0 " : "";

    private const string Alias = "corsacareer_music";
    private static string? current;
    private static Process? fallback;
    private static bool mciOpen;
    private static bool paused;
    private static int currentVolume = BackgroundVolume;
    private static readonly Dictionary<string, int> moodCursor = new(StringComparer.OrdinalIgnoreCase);
    private static string? lastMood;
    public static string LastBackend { get; private set; } = "nessuno";

    /// <summary>
    /// Vero quando un canale audio è davvero aperto, indipendentemente dal
    /// backend. Prima questa domanda veniva posta a <c>fallback</c>, che però
    /// resta null sul percorso MCI: le guardie di rotazione non scattavano mai
    /// e ogni refresh della Home riavviava il brano da capo.
    /// </summary>
    private static bool Active => (fallback != null && !HasFinished(fallback)) || mciOpen;

    /// <summary>
    /// Nei collaudi automatici non deve partire nessun audio: ffplay viene
    /// avviato con "-loop 0" e non termina da solo, quindi un player aperto da
    /// un costruttore di dialogo teneva in vita il processo di rendering finché
    /// non veniva chiuso a mano. Il divieto sta qui, non nei chiamanti: così
    /// nessuna schermata nuova può riaprire il problema per distrazione.
    /// </summary>
    private static bool Muted =>
        string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal)
        || !EnabledByPlayer;

    /// <summary>
    /// La musica e' spenta finche il giocatore non la accende.
    ///
    /// Prima partiva da sola a ogni cambio di schermata, e l'unico modo per
    /// fermarla era una variabile d'ambiente: chi giocava non aveva alcun modo
    /// di zittirla. Il silenzio e' il valore predefinito; il pulsante
    /// SOUNDTRACK nella testata la accende.
    /// </summary>
    /// <summary>
    /// La musica è accesa di default: è parte dell'atmosfera, non un extra.
    ///
    /// L'avevo spenta per fermare i processi audio che sopravvivevano alla
    /// chiusura, ma quello era un difetto diverso — ora chiuso a monte, con i
    /// player terminati esplicitamente e quattro vie d'uscita che li spengono.
    /// Zittire tutto era curare il sintomo togliendo la funzione.
    /// </summary>
    public static bool EnabledByPlayer { get; private set; } = true;

    /// <summary>
    /// Accende o spegne la colonna sonora. Spegnendola ferma subito tutto,
    /// compresi i player esterni: e' il comando che prima non esisteva.
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        EnabledByPlayer = enabled;
        if (!enabled) Stop();
    }

    /// <summary>
    /// Elenco dei brani, letto una volta sola.
    ///
    /// Prima la cartella veniva enumerata a ogni cambio di atmosfera, cioè a
    /// ogni aggiornamento della schermata: accesso al disco sul thread
    /// dell'interfaccia dopo ogni clic. I file non cambiano durante la
    /// partita.
    /// </summary>
    private static IReadOnlyList<string>? cachedTracks;

    public static IReadOnlyList<string> Tracks() => cachedTracks ??= ScanTracks();

    private static IReadOnlyList<string> ScanTracks()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "soundtrack");
        if (!Directory.Exists(root)) return [];
        // La cartella arriva da un download e contiene copie "Titolo (1).mp3"
        // dello stesso brano. Senza raggrupparle la rotazione per atmosfera
        // pescava quattro volte lo stesso pezzo e sembrava non cambiare mai.
        return Directory.GetFiles(root, "*.*", SearchOption.TopDirectoryOnly)
            .Where(x => Path.GetExtension(x).Equals(".mp3", StringComparison.OrdinalIgnoreCase)
                     || Path.GetExtension(x).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            .GroupBy(BaseTitle, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).First())
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Titolo senza il suffisso " (n)" aggiunto dai download ripetuti.</summary>
    private static string BaseTitle(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path).TrimEnd();
        if (!name.EndsWith(')')) return name;
        var open = name.LastIndexOf('(');
        if (open <= 0) return name;
        var inner = name[(open + 1)..^1];
        return inner.Length > 0 && inner.All(char.IsDigit) ? name[..open].TrimEnd() : name;
    }

    public static void Play(string path, int volume = 700)
    {
        if (Muted || !File.Exists(path)) return;
        if (string.Equals(current, path, StringComparison.OrdinalIgnoreCase) && Active && !paused) return;
        var previous = fallback;
        // La navigazione non deve mai lasciare due tracce sovrapposte: quando
        // la destinazione cambia chiudiamo prima il vecchio canale e soltanto
        // dopo avviamo il nuovo, così non c'è un secondo di musica doppia.
        StopProcess(previous);
        mciSendString($"stop {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        mciSendString($"close {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        fallback = null;
        mciOpen = false;
        paused = false;
        current = null;
        currentVolume = Math.Clamp(volume, 0, 1000);
        var escaped = path.Replace("\"", "\"\"");
        // I codec MCI presenti su alcune installazioni Windows possono
        // restituire successo ma non emettere audio per gli MP3 moderni:
        // per questi usiamo direttamente ffplay, verificato sul sistema.
        var open = Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase)
            ? -1
            : mciSendString($"open \"{escaped}\" type mpegvideo alias {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        if (open == 0)
        {
            mciSendString($"setaudio {Alias} volume {currentVolume}", IntPtr.Zero, 0, IntPtr.Zero);
            mciSendString($"play {Alias}{MciRepeatFlag}", IntPtr.Zero, 0, IntPtr.Zero);
            mciOpen = true;
            LastBackend = "MCI";
        }
        else
        {
            var player = FindFfplay();
            if (player != null)
            {
                // ffplay non espone una rampa di volume pilotabile da stdin: il
                // livello va deciso qui, all'avvio, e resta quello per tutta la
                // traccia. Chi vuole cambiarlo passa da SetVolume, che riavvia
                // il processo con il nuovo valore.
                fallback = Process.Start(new ProcessStartInfo
                {
                    FileName = player,
                    Arguments = $"-nodisp -loglevel quiet {FfplayLoopArgument}-volume {currentVolume / 10} \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                });
                BindToApplicationLifetime(fallback);
                LastBackend = fallback != null ? "ffplay" : "non disponibile";
            }
            else LastBackend = "non disponibile";
        }
        // Senza un backend attivo non c'è nulla in riproduzione: registrare il
        // percorso comunque avrebbe fatto credere alle guardie che un canale
        // fosse aperto, bloccando ogni tentativo successivo.
        current = Active ? path : null;
    }

    /// <summary>Vero quando c'è un brano aperto ma in pausa.</summary>
    public static bool IsPaused => Active && paused;

    /// <summary>Vero quando un brano è aperto, in pausa o in riproduzione.</summary>
    public static bool IsLoaded => Active;

    // In ffplay 'p' è un interruttore, non un comando di pausa: mandarlo senza
    // sapere lo stato corrente invertiva il risultato atteso (due Resume di
    // fila mettevano in pausa). Pause e Resume ora sono idempotenti e la
    // posizione viene ricordata solo dove il backend la conserva davvero.
    public static void Pause()
    {
        if (!Active || paused) return;
        if (fallback is { HasExited: false }) TogglePlayback();
        else mciSendString($"pause {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        paused = true;
    }

    public static void Resume()
    {
        if (!Active || !paused) return;
        if (fallback is { HasExited: false }) TogglePlayback();
        else mciSendString($"resume {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        paused = false;
    }

    private static void TogglePlayback()
    {
        try { fallback!.StandardInput.Write('p'); fallback.StandardInput.Flush(); } catch { }
    }

    public static void SetVolume(int volume)
    {
        var wanted = Math.Clamp(volume, 0, 1000);
        if (wanted == currentVolume) return;
        currentVolume = wanted;
        if (mciOpen)
        {
            mciSendString($"setaudio {Alias} volume {wanted}", IntPtr.Zero, 0, IntPtr.Zero);
            return;
        }
        // ffplay accetta il volume soltanto sulla riga di comando. Per non
        // lasciare lo slider senza effetto — è il caso di tutti gli MP3, che
        // passano sempre da qui — il brano corrente viene riavviato al livello
        // nuovo invece di ignorare la richiesta.
        if (fallback is not { HasExited: false } || current == null) return;
        var track = current;
        current = null;
        Play(track, wanted);
    }

    public static void Stop()
    {
        StopProcess(fallback);
        fallback = null;
        // Non basta chiudere il player corrente: un cambio di brano puo aver
        // lasciato indietro un processo che non e' mai stato fermato.
        KillExternalPlayers();
        mciSendString($"stop {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        mciSendString($"close {Alias}", IntPtr.Zero, 0, IntPtr.Zero);
        mciOpen = false;
        paused = false;
        current = null;
        lastMood = null;
        LastBackend = "nessuno";
    }

    /// <summary>
    /// Da chiamare quando un'azione porta a una scena diversa. Si preferisce un
    /// silenzio immediato a due brani sovrapposti: la prossima schermata potra
    /// poi far entrare il proprio tema senza trascinarsi dietro quello prima.
    /// </summary>
    public static void SilenceForNavigation()
    {
        Stop();
    }

    public static string? Current => current;

    /// <summary>
    /// Livello del sottofondo durante il gioco. La musica accompagna il
    /// racconto, non lo copre: 585 su 1000 era un volume da ascolto in primo
    /// piano e sovrastava il testo del prologo e la voce narrata.
    /// </summary>
    public const int BackgroundVolume = 260;

    /// <summary>Livello del prologo: sotto il sottofondo, perché lì si legge.</summary>
    public const int IntroVolume = 150;

    public static void PlayIntro()
    {
        // Il narratore e il testo che si compone riga per riga devono restare
        // davanti: il prologo tiene un livello più basso del gioco normale.
        PlayForMood("intro", IntroVolume);
    }

    /// <summary>
    /// Segna che e' cominciata una scena nuova: al prossimo tema la rotazione
    /// avanza anche se l'atmosfera e' la stessa di prima.
    /// </summary>
    public static void MarkNewScene() => sceneChanged = true;

    private static bool sceneChanged;

    public static string SuggestedMood(string eventType) => eventType.ToUpperInvariant() switch
    {
        var x when x.Contains("VICTORY") || x.Contains("PODIUM") || x.Contains("CHAMPIONSHIP") => "vittoria",
        var x when x.Contains("FINANCIAL") || x.Contains("SETBACK") || x.Contains("WARNING") || x.Contains("RETIRE") => "crisi",
        // La firma di un contratto e il passaggio di squadra sono lo snodo di
        // una carriera, non una trattativa qualsiasi: prima finivano nel tema
        // "sponsor", lo stesso di un incontro di routine. Va prima di SPONSOR
        // e CONTRACT, altrimenti quei rami la intercetterebbero.
        var x when x.Contains("SIGNING") || x.Contains("TEAM_CHANGE") || x.Contains("PROMOTION") => "firma",
        var x when x.Contains("SPONSOR") || x.Contains("CONTRACT") || x.Contains("MARKET") => "sponsor",
        var x when x.Contains("RACE") || x.Contains("GRID") => "griglia",
        var x when x.Contains("TEST") || x.Contains("BRIEFING") => "concentrazione",
        _ => "alba"
    };

    private static string? FindFfplay()
    {
        var fromPath = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator)
            .Select(x => Path.Combine(x, "ffplay.exe")).FirstOrDefault(File.Exists);
        if (fromPath != null) return fromPath;
        var known = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffplay.exe");
        return File.Exists(known) ? known : File.Exists(@"C:\ffmpeg\bin\ffplay.exe") ? @"C:\ffmpeg\bin\ffplay.exe" : null;
    }

    public static void PlayForMood(string mood, int volume = BackgroundVolume)
    {
        if (Muted) return;
        var normalized = mood.ToLowerInvariant();
        // Un refresh della stessa schermata non deve riavviare il brano — ma
        // una scena NUOVA nella stessa atmosfera si: prima, una serie di prove
        // (tutte "concentrazione") lasciava lo stesso brano per ore.
        //
        // Se il brano in corso e' finito, o se questa e una scena nuova, la
        // rotazione avanza.
        if (string.Equals(lastMood, normalized, StringComparison.OrdinalIgnoreCase) && Active && !sceneChanged) return;
        sceneChanged = false;
        var key = normalized switch
        {
            // Ogni atmosfera pesca da una scelta ampia: prima ne elencava due o
            // tre e meta della raccolta non veniva mai usata.
            "intro" => new[] { "Faint Horizon", "Morning Horizon", "Paddock Wire", "Neon Horizon" },
            "vittoria" => new[] { "Proud Momentum", "The Final Lap", "Checkered Skyway" },
            "crisi" => new[] { "Faded Checkered Flag", "Static Between Lines", "Neon Horizon" },
            // La firma merita il brano della consacrazione, non quello delle
            // trattative: e' il momento in cui la carriera cambia livello.
            "firma" => new[] { "Proud Momentum", "Checkered Skyway", "The First Pitch" },
            "sponsor" => new[] { "The First Pitch", "Post-Race Analysis", "Paddock Wire" },
            "griglia" => new[] { "Gridline Rush", "Gridline Groove", "Checkered Skyway", "The Final Lap" },
            "concentrazione" => new[] { "Circuit Focus", "Paddock Wire", "Static Between Lines", "Post-Race Analysis" },
            _ => new[] { "Morning Horizon", "Faint Horizon", "Neon Horizon", "Circuit Focus" }
        };
        var candidates = Tracks()
            .Where(x => key.Any(k => Path.GetFileNameWithoutExtension(x).Contains(k, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (candidates.Length == 0) candidates = Tracks().ToArray();
        if (candidates.Length == 0) return;
        var index = moodCursor.TryGetValue(normalized, out var stored) ? stored % candidates.Length : 0;
        moodCursor[normalized] = (index + 1) % candidates.Length;
        Play(candidates[index], volume);
        // L'atmosfera si considera raggiunta solo se un canale è partito
        // davvero: su un PC senza backend audio la guardia avrebbe altrimenti
        // impedito ogni tentativo successivo.
        lastMood = Active ? normalized : null;
    }

    private static void StopProcess(Process? process)
    {
        if (process == null) return;
        // Il comando di chiusura parte subito: è immediato e garantisce che il
        // brano precedente non resti a suonare sopra il nuovo.
        try { if (!process.HasExited) { process.StandardInput.Write('q'); process.StandardInput.Flush(); } } catch { }
        // L'attesa e l'eventuale terminazione forzata, invece, costavano fino a
        // 250 ms di interfaccia ferma a ogni cambio di atmosfera — cioè dopo
        // ogni clic che aggiornava la schermata. Non serve nessuno sul thread
        // della UI ad aspettare che un player esterno finisca di chiudersi.
        Task.Run(() =>
        {
            try { if (!process.HasExited) process.WaitForExit(250); } catch { }
            try { if (!process.HasExited) process.Kill(true); } catch { }
            try { process.Dispose(); } catch { }
        });
    }
}
