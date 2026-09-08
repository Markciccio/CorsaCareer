using System.Text.Json;

namespace CorsaCareer;

public static class PhotoSource
{
    public static string Caption(string path, string eventType = "")
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "Illustrazione generata — non è una foto dell’evento";
        // Le immagini incluse nel pacchetto sono asset editoriali: possono mostrare
        // una vista esterna utile al racconto, ma non sono screenshot della gara.
        var full = Path.GetFullPath(path);
        var bundledAssets = AssetPaths.Root + Path.DirectorySeparatorChar;
        if (full.StartsWith(bundledAssets, StringComparison.OrdinalIgnoreCase))
            return "Illustrazione editoriale generata — vista esterna non acquisita in Assetto Corsa";
        var kind = ReadKind(path);
        if (kind.Equals("ASSETTO_CORSA_SCREENSHOT", StringComparison.OrdinalIgnoreCase)) return "Screenshot reale archiviato da Assetto Corsa";
        if (kind.Equals("MANUAL_PHOTO_IMPORT", StringComparison.OrdinalIgnoreCase)) return "Foto importata dall’utente";
        return "Immagine archiviata — fonte non verificata";
    }

    /// <summary>Reads the camera/view declared in the image sidecar.</summary>
    public static string View(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "";
        try
        {
            var metadata = path + ".meta.json";
            if (!File.Exists(metadata)) return "visuale non dichiarata";
            using var doc = JsonDocument.Parse(File.ReadAllText(metadata));
            if (doc.RootElement.TryGetProperty("view", out var value) && !string.IsNullOrWhiteSpace(value.GetString()))
                return value.GetString()!;
        }
        catch { }
        return "visuale non dichiarata";
    }

    private static string ReadKind(string path)
    {
        try
        {
            var metadata = path + ".meta.json";
            if (!File.Exists(metadata)) return "";
            using var doc = JsonDocument.Parse(File.ReadAllText(metadata));
            return doc.RootElement.TryGetProperty("kind", out var value) ? value.GetString() ?? "" : "";
        }
        catch { return ""; }
    }
}
