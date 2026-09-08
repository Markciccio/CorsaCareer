namespace CorsaCareer;

/// <summary>
/// Individua il referto prodotto da una sessione reale di Assetto Corsa.
/// La ricerca è limitata a percorsi espliciti e non interpreta file casuali.
/// </summary>
public static class AssettoCorsaResultLocator
{
    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Assetto Corsa", "out", "race_out.json");

    public static IReadOnlyList<string> CandidatePaths()
    {
        var candidates = new List<string> { DefaultPath };
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var publicDocuments = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
        Add(candidates, Path.Combine(documents, "Assetto Corsa", "out", "race_out.json"));
        Add(candidates, Path.Combine(publicDocuments, "Assetto Corsa", "out", "race_out.json"));
        Add(candidates, Path.Combine(AppContext.BaseDirectory, "out", "race_out.json"));
        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static string FindLatestExisting()
    {
        var file = CandidatePaths()
            .Where(File.Exists)
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.LastWriteTimeUtc)
            .FirstOrDefault();
        return file?.FullName ?? DefaultPath;
    }

    public static string FindLatestExisting(IEnumerable<string> candidates)
    {
        var file = (candidates ?? Enumerable.Empty<string>())
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.LastWriteTimeUtc)
            .FirstOrDefault();
        return file?.FullName ?? "";
    }

    private static void Add(ICollection<string> list, string path)
    {
        if (!string.IsNullOrWhiteSpace(path)) list.Add(path);
    }
}
