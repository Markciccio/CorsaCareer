using System.Globalization;
using Microsoft.Win32;

namespace CorsaCareer;

public static class NarrationVoiceCatalog
{
    public const string Automatic = "Automatica (migliore voce italiana)";
    public const string Browser = "Browser — Google italiano (it-IT)";

    /// <summary>
    /// Servizio di lettura predefinito: la voce italiana esposta dal browser da
    /// Google. È una voce del motore Web Speech (Chrome/Edge), non una voce SAPI
    /// installata in Windows, quindi vive nella pagina locale del portale.
    /// </summary>
    public const string PreferredBrowserVoice = "Google italiano";
    public const string PreferredBrowserLanguage = "it-IT";

    // Velocità di lettura, aumentata del 10% rispetto ai valori precedenti
    // (0,88 nel browser e -8% di prosodia nel percorso Windows).
    public const double PreviousBrowserSpeechRate = 0.88;
    public const double BrowserSpeechRate = 0.97;
    public const double PreviousWindowsProsodyFactor = 0.92;
    public const string WindowsProsodyRate = "+1%";

    /// <summary>Valore da interpolare nel JavaScript della pagina locale.</summary>
    public static string BrowserRateLiteral => BrowserSpeechRate.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>
    /// Regola canonica di ordinamento delle voci del browser: prima la voce
    /// italiana di Google, poi le altre `it-IT`, poi le restanti varianti
    /// italiane. Il JavaScript della pagina applica lo stesso criterio; questa
    /// versione esiste per poterlo verificare con un test.
    /// </summary>
    public static IReadOnlyList<string> OrderBrowserVoices(IEnumerable<(string Name, string Lang)> voices) =>
        voices
            .Where(x => (x.Lang ?? "").StartsWith("it", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => BrowserVoiceRank(x.Name, x.Lang))
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Name)
            .ToList();

    public static int BrowserVoiceRank(string name, string language)
    {
        var isItalian = string.Equals(language, PreferredBrowserLanguage, StringComparison.OrdinalIgnoreCase);
        var isGoogle = (name ?? "").Contains("google", StringComparison.OrdinalIgnoreCase);
        if (isGoogle && isItalian) return 0;
        if (isItalian) return 1;
        if (isGoogle) return 2;
        return 3;
    }

    public static IReadOnlyList<string> ItalianVoiceNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                foreach (var root in new[] { @"SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens", @"SOFTWARE\Microsoft\Speech\Voices\Tokens" })
                using (var tokens = baseKey.OpenSubKey(root))
                {
                    if (tokens == null) continue;
                    foreach (var tokenName in tokens.GetSubKeyNames())
                    using (var token = tokens.OpenSubKey(tokenName))
                    {
                        var culture = token?.GetValue("Language")?.ToString() ?? token?.GetValue("Locale")?.ToString() ?? "";
                        var display = token?.GetValue("")?.ToString() ?? tokenName;
                        if (culture.Contains("it", StringComparison.OrdinalIgnoreCase) || tokenName.Contains("Italian", StringComparison.OrdinalIgnoreCase) || display.Contains("Italian", StringComparison.OrdinalIgnoreCase)) names.Add(display);
                    }
                }
            }
            catch { }
        }
        return names.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static bool IsHighQuality(string name)
        => name.Contains("Natural", StringComparison.OrdinalIgnoreCase)
        || name.Contains("Neural", StringComparison.OrdinalIgnoreCase)
        || name.Contains("Online", StringComparison.OrdinalIgnoreCase);
}
