using System.IO.Compression;

namespace CorsaCareer;

public static class ContentPackageInstaller
{
    private static readonly string[] AllowedRoots = ["content/", "extension/", "apps/", "system/"];

    public static bool TryInstall(string zipPath, string assettoCorsaRoot, out int installedFiles, out string error)
    {
        installedFiles = 0; error = "";
        string? staging = null;
        try
        {
            if (string.IsNullOrWhiteSpace(zipPath) || !File.Exists(zipPath)) { error = "Pacchetto ZIP non trovato."; return false; }
            if (string.IsNullOrWhiteSpace(assettoCorsaRoot) || !Directory.Exists(assettoCorsaRoot)) { error = "Cartella principale di Assetto Corsa non trovata."; return false; }
            var root = Path.GetFullPath(assettoCorsaRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            using var archive = ZipFile.OpenRead(zipPath);
            var fileEntries = archive.Entries.Where(x => !string.IsNullOrWhiteSpace(x.FullName) && !x.FullName.Replace('\\', '/').EndsWith('/')).ToList();
            var prefix = "";
            var firstSegments = fileEntries.Select(x => x.FullName.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (firstSegments.Any(x => x is "." or ".." || x.Contains(':') || x.StartsWith('/'))) { error = "Il pacchetto contiene un percorso assoluto o non sicuro."; return false; }
            if (firstSegments.Count == 1 && !AllowedRoots.Any(x => firstSegments[0].Equals(x.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))) prefix = firstSegments[0] + "/";
            var plan = new List<(ZipArchiveEntry Entry, string Destination, string Relative)>();
            foreach (var entry in fileEntries)
            {
                var normalized = entry.FullName.Replace('\\', '/');
                if (prefix.Length > 0 && normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) normalized = normalized[prefix.Length..];
                // README, preview images and changelog at the wrapper root are
                // documentation, not Assetto Corsa content: ignore them.
                if (prefix.Length > 0 && !normalized.Contains('/')) continue;
                if (string.IsNullOrWhiteSpace(normalized) || normalized.EndsWith('/')) continue;
                if (normalized.StartsWith('/') || normalized.Contains(':') || normalized.Split('/').Any(x => x == "..") || !AllowedRoots.Any(x => normalized.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
                { error = $"Percorso non consentito nel pacchetto: {entry.FullName}"; return false; }
                var destination = Path.GetFullPath(Path.Combine(assettoCorsaRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
                if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase)) { error = "Il pacchetto tenta di uscire dalla cartella Assetto Corsa."; return false; }
                plan.Add((entry, destination, normalized));
            }
            if (plan.Count == 0) { error = "Il pacchetto non contiene file Assetto Corsa installabili."; return false; }
            staging = Path.Combine(Path.GetTempPath(), "corsacareer-install-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            foreach (var (entry, _, relative) in plan)
            {
                var staged = Path.Combine(staging, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(staged)!);
                entry.ExtractToFile(staged, true);
            }
            foreach (var (_, destination, relative) in plan)
            {
                var staged = Path.Combine(staging, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(staged, destination, true); installedFiles++;
            }
            return true;
        }
        catch (InvalidDataException) { error = "Il file selezionato non è un archivio ZIP valido."; return false; }
        catch (Exception ex) { error = ex.Message; return false; }
        finally
        {
            if (!string.IsNullOrWhiteSpace(staging))
            {
                try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { }
            }
        }
    }
}
