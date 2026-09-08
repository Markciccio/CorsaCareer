using System.Security.Cryptography;
using System.Text.Json;

namespace CorsaCareer;

public static class ResultIntegrity
{
    public static string Describe(string resultFile)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resultFile) || !File.Exists(resultFile + ".meta.json")) return "non disponibile (vecchio salvataggio)";
            using var doc = JsonDocument.Parse(File.ReadAllText(resultFile + ".meta.json"));
            var stored = doc.RootElement.TryGetProperty("sha256", out var value) ? value.GetString() ?? "" : "";
            if (string.IsNullOrWhiteSpace(stored) || !File.Exists(resultFile)) return string.IsNullOrWhiteSpace(stored) ? "non disponibile" : stored;
            using var stream = File.OpenRead(resultFile);
            var current = Convert.ToHexString(SHA256.HashData(stream));
            return stored.Equals(current, StringComparison.OrdinalIgnoreCase) ? $"{stored} (verificato)" : $"{stored} (VERIFICA FALLITA: file modificato)";
        }
        catch { return "non disponibile"; }
    }
}
