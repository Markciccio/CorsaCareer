using System.Text;

namespace CorsaCareer;

/// <summary>
/// Log su file della carriera.
///
/// Prima gli errori finivano in <c>Debug.WriteLine</c> e i blocchi
/// <c>catch { }</c> non lasciavano traccia: in una build Release un import
/// fallito era invisibile e indiagnosticabile. Qui ogni evento rilevante viene
/// scritto su un file ruotato, senza mai propagare un'eccezione al chiamante.
/// </summary>
public static class CareerLog
{
    private static readonly object Gate = new();
    private const long MaxBytes = 2 * 1024 * 1024;
    private const int KeptFiles = 5;
    private static string directory = "";

    public static string LogFile => string.IsNullOrWhiteSpace(directory) ? "" : Path.Combine(directory, "corsacareer.log");

    public static void Initialize(string saveDirectory)
    {
        try
        {
            directory = Path.Combine(saveDirectory, "logs");
            Directory.CreateDirectory(directory);
            Write("INFO", "sessione", $"CorsaCareer avviato · {Environment.OSVersion} · .NET {Environment.Version}");
        }
        catch { directory = ""; }
    }

    public static void Info(string area, string message) => Write("INFO", area, message);
    public static void Warn(string area, string message) => Write("WARN", area, message);

    public static void Error(string area, string message, Exception? error = null) =>
        Write("ERROR", area, error == null ? message : $"{message} · {error.GetType().Name}: {error.Message}");

    private static void Write(string level, string area, string message)
    {
        if (string.IsNullOrWhiteSpace(directory)) return;
        try
        {
            lock (Gate)
            {
                var file = LogFile;
                Rotate(file);
                File.AppendAllText(file, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {area}: {message}{Environment.NewLine}", Encoding.UTF8);
            }
        }
        catch
        {
            // Il logging non deve mai far fallire un'operazione della carriera.
        }
    }

    private static void Rotate(string file)
    {
        try
        {
            if (!File.Exists(file) || new FileInfo(file).Length < MaxBytes) return;
            var rotated = Path.Combine(directory, $"corsacareer-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.Move(file, rotated, true);
            foreach (var old in Directory.GetFiles(directory, "corsacareer-*.log")
                .OrderByDescending(File.GetLastWriteTimeUtc).Skip(KeptFiles)) File.Delete(old);
        }
        catch { }
    }
}
