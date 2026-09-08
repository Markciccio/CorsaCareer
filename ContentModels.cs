using System.Text.Json;
using System.Text.RegularExpressions;

namespace CorsaCareer;

public sealed class ContentCarRecord
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "special";
    public int Confidence { get; set; }
    public string Brand { get; set; } = "";
    public int PowerHp { get; set; }
    public int MassKg { get; set; }
    public int ModelYear { get; set; }
    public List<string> Skins { get; set; } = [];
    public string SourcePath { get; set; } = "";
}

public sealed class ContentTrackRecord
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string Layout { get; set; } = "";
    public string Category { get; set; } = "permanent";
    public int Confidence { get; set; }
    // Lunghezza dichiarata dal circuito installato: serve a calcolare i giri di
    // gara su una distanza reale invece di usare un numero fisso. 0 = non nota.
    public int LengthMeters { get; set; }
    public int Pitboxes { get; set; }
    public string SourcePath { get; set; } = "";
}

public sealed class ContentWeatherRecord
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string SourcePath { get; set; } = "";
}

public sealed class ContentIndexRecord
{
    public int SchemaVersion { get; set; } = 4;
    public DateTime ScannedUtc { get; set; }
    public string AssettoCorsaRoot { get; set; } = "";
    public List<ContentCarRecord> Cars { get; set; } = [];
    public List<ContentTrackRecord> Tracks { get; set; } = [];
    public List<ContentWeatherRecord> Weathers { get; set; } = [];
    public Dictionary<string, string> CategoryOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ContentPackageRecord> InstalledPackages { get; set; } = [];
    public List<string> ScanWarnings { get; set; } = [];
}

public sealed class ContentPackageRecord
{
    public string Source { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public int InstalledFiles { get; set; }
    public DateTime InstalledUtc { get; set; }
}

public static class ContentScanner
{
    public static ContentIndexRecord Scan(string root, IDictionary<string, string>? overrides = null)
    {
        var result = new ContentIndexRecord { ScannedUtc = DateTime.UtcNow, AssettoCorsaRoot = root, CategoryOverrides = overrides == null ? new(StringComparer.OrdinalIgnoreCase) : new Dictionary<string, string>(overrides, StringComparer.OrdinalIgnoreCase) };
        var carsDir = Path.Combine(root, "content", "cars");
        foreach (var dir in SafeDirectories(carsDir, result.ScanWarnings, "auto"))
        {
            try { result.Cars.Add(ReadCar(dir, result.ScanWarnings)); } catch (IOException) { result.ScanWarnings.Add($"Auto non leggibile: {Path.GetFileName(dir)}"); }
            catch (UnauthorizedAccessException) { result.ScanWarnings.Add($"Auto non accessibile: {Path.GetFileName(dir)}"); }
            // Rete di sicurezza: un metadato con una forma inattesa deve costare
            // una voce saltata, non l'avvio dell'applicazione.
            catch (Exception error) { result.ScanWarnings.Add($"Auto ignorata per metadati inattesi ({error.GetType().Name}): {Path.GetFileName(dir)}"); }
        }
        var tracksDir = Path.Combine(root, "content", "tracks");
        foreach (var dir in SafeDirectories(tracksDir, result.ScanWarnings, "circuito"))
        {
            try { result.Tracks.Add(ReadTrack(dir, result.ScanWarnings)); } catch (IOException) { result.ScanWarnings.Add($"Circuito non leggibile: {Path.GetFileName(dir)}"); }
            catch (UnauthorizedAccessException) { result.ScanWarnings.Add($"Circuito non accessibile: {Path.GetFileName(dir)}"); }
            catch (Exception error) { result.ScanWarnings.Add($"Circuito ignorato per metadati inattesi ({error.GetType().Name}): {Path.GetFileName(dir)}"); }
        }
        // Il meteo utilizzabile è solo quello realmente installato: senza questa
        // lista il pianificatore non può proporre condizioni diverse dal sereno.
        var weatherDir = Path.Combine(root, "content", "weather");
        foreach (var dir in SafeDirectories(weatherDir, result.ScanWarnings, "meteo"))
        {
            var id = Path.GetFileName(dir) ?? "";
            if (string.IsNullOrWhiteSpace(id)) continue;
            var name = "";
            try
            {
                var iniPath = Path.Combine(dir, "weather.ini");
                if (File.Exists(iniPath))
                {
                    var match = Regex.Match(File.ReadAllText(iniPath), @"^\s*NAME\s*=\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    if (match.Success) name = match.Groups[1].Value.Trim();
                }
            }
            catch (IOException) { result.ScanWarnings.Add($"Meteo non leggibile: {id}"); }
            catch (UnauthorizedAccessException) { result.ScanWarnings.Add($"Meteo non accessibile: {id}"); }
            result.Weathers.Add(new ContentWeatherRecord { Id = id, Name = string.IsNullOrWhiteSpace(name) ? id.Replace("_", " ") : name, SourcePath = dir });
        }
        result.Weathers = result.Weathers.OrderBy(x => x.Id).ToList();
        foreach (var car in result.Cars) if (result.CategoryOverrides.TryGetValue(car.Id, out var overrideCategory) && !string.IsNullOrWhiteSpace(overrideCategory)) { car.Category = overrideCategory; car.Confidence = 100; }
        result.Cars = result.Cars.OrderBy(x => x.Category).ThenBy(x => x.Name).ToList();
        result.Tracks = result.Tracks.OrderBy(x => x.Name).ThenBy(x => x.Layout).ToList();
        return result;
    }

    private static ContentCarRecord ReadCar(string dir, List<string> warnings)
    {
        var id = Path.GetFileName(dir) ?? ""; var text = id.Replace("_", " ");
        var metadataPath = Path.Combine(dir, "ui", "ui_car.json");
        var metadata = ReadJson(metadataPath);
        if (File.Exists(metadataPath) && metadata.ValueKind == JsonValueKind.Undefined) warnings.Add($"Metadati auto in JSON non standard: fallback metadati applicato — {metadataPath}");
        var name = ReadString(metadata, "name", "display_name");
        var brand = ReadString(metadata, "brand", "manufacturer");
        if (metadata.ValueKind == JsonValueKind.Undefined)
        {
            name = ReadLooseString(metadataPath, "name", "display_name");
            brand = ReadLooseString(metadataPath, "brand", "manufacturer");
        }
        var combined = $"{id} {name} {brand}".ToLowerInvariant();
        var declaredCategory = ReadDeclaredCategory(metadata);
        if (string.IsNullOrWhiteSpace(declaredCategory) && metadata.ValueKind == JsonValueKind.Undefined) declaredCategory = ReadLooseCategory(metadataPath);
        string category;
        int confidence;
        if (TryDeclaredCategory(declaredCategory, out var declared)) { category = declared; confidence = 100; }
        else category = ClassifyCar(combined, out confidence);
        var skinsDir = Path.Combine(dir, "skins");
        var skins = SafeDirectories(skinsDir, warnings, "livree").Select(Path.GetFileName).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().OrderBy(x => x).ToList();
        var modelYear = ReadInt(metadata, "year", "model_year", "year_produced");
        if (modelYear == 0 && metadata.ValueKind == JsonValueKind.Undefined) modelYear = ReadLooseInt(metadataPath, "year", "model_year", "year_produced");
        return new ContentCarRecord { Id = id, Name = string.IsNullOrWhiteSpace(name) ? text : name, Brand = brand, Category = category, Confidence = confidence, PowerHp = ReadInt(metadata, "bhp", "power", "power_hp"), MassKg = ReadInt(metadata, "weight", "mass"), ModelYear = modelYear, Skins = skins, SourcePath = dir };
    }

    private static IEnumerable<string> SafeDirectories(string path, List<string> warnings, string kind)
    {
        try { return Directory.Exists(path) ? Directory.EnumerateDirectories(path).ToArray() : Array.Empty<string>(); }
        catch (IOException) { warnings.Add($"Cartella {kind} non leggibile: {path}"); return Array.Empty<string>(); }
        catch (UnauthorizedAccessException) { warnings.Add($"Cartella {kind} non accessibile: {path}"); return Array.Empty<string>(); }
    }

    private static ContentTrackRecord ReadTrack(string dir, List<string> warnings)
    {
        var id = Path.GetFileName(dir) ?? "";
        var metadataPath = Path.Combine(dir, "ui", "ui_track.json");
        var metadata = ReadJson(metadataPath);
        if (File.Exists(metadataPath) && metadata.ValueKind == JsonValueKind.Undefined) warnings.Add($"Metadati circuito in JSON non standard: fallback metadati applicato — {metadataPath}");
        var name = ReadString(metadata, "name", "display_name"); var country = ReadString(metadata, "country");
        if (metadata.ValueKind == JsonValueKind.Undefined)
        {
            name = ReadLooseString(metadataPath, "name", "display_name");
            country = ReadLooseString(metadataPath, "country", "location");
        }
        var combined = $"{id} {name}".ToLowerInvariant();
        var category = combined.Contains("kart") ? "kartodromo" : combined.Contains("street") || combined.Contains("monaco") ? "cittadino" : combined.Contains("hill") ? "hillclimb" : "permanent";
        var lengthMeters = ReadTrackLength(metadata, metadataPath);
        var pitboxes = ReadFlexibleInt(metadata, metadataPath, "pitboxes");
        // I circuiti multi-layout tengono i metadati in ui/<layout>/ui_track.json:
        // senza questo fallback la lunghezza resterebbe sconosciuta.
        if (lengthMeters == 0 || pitboxes == 0)
            foreach (var layoutDir in SafeDirectories(Path.Combine(dir, "ui"), warnings, "layout circuito"))
            {
                var layoutPath = Path.Combine(layoutDir, "ui_track.json");
                if (!File.Exists(layoutPath)) continue;
                var layoutMetadata = ReadJson(layoutPath);
                if (lengthMeters == 0) lengthMeters = ReadTrackLength(layoutMetadata, layoutPath);
                if (pitboxes == 0) pitboxes = ReadFlexibleInt(layoutMetadata, layoutPath, "pitboxes");
                if (lengthMeters > 0 && pitboxes > 0) break;
            }
        return new ContentTrackRecord { Id = id, Name = string.IsNullOrWhiteSpace(name) ? id.Replace("_", " ") : name, Country = country, Layout = id, Category = category, Confidence = country.Length > 0 || name.Length > 0 ? 70 : 35, LengthMeters = lengthMeters, Pitboxes = pitboxes, SourcePath = dir };
    }

    // La lunghezza AC è dichiarata in modi diversi: numero, stringa con virgola,
    // chilometri ("5.793") o metri ("20832"). Normalizza sempre in metri.
    public static int NormalizeTrackLength(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0;
        var cleaned = Regex.Replace(raw.Trim().ToLowerInvariant(), @"[^0-9,\.]", "");
        if (cleaned.Length == 0) return 0;
        cleaned = cleaned.Replace(',', '.');
        var firstDot = cleaned.IndexOf('.');
        if (firstDot >= 0) cleaned = cleaned[..(firstDot + 1)] + cleaned[(firstDot + 1)..].Replace(".", "");
        if (!double.TryParse(cleaned, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value) || value <= 0) return 0;
        // Sotto i 100 il valore è espresso in chilometri; sopra è già in metri.
        var meters = value < 100 ? value * 1000 : value;
        return meters is > 200 and < 60000 ? (int)Math.Round(meters) : 0;
    }

    private static int ReadTrackLength(JsonElement metadata, string metadataPath)
    {
        if (metadata.ValueKind == JsonValueKind.Object && metadata.TryGetProperty("length", out var declared))
        {
            if (declared.ValueKind == JsonValueKind.String) { var parsed = NormalizeTrackLength(declared.GetString() ?? ""); if (parsed > 0) return parsed; }
            if (declared.ValueKind == JsonValueKind.Number && declared.TryGetDouble(out var numeric)) { var parsed = NormalizeTrackLength(numeric.ToString(System.Globalization.CultureInfo.InvariantCulture)); if (parsed > 0) return parsed; }
        }
        return NormalizeTrackLength(ReadLooseRaw(metadataPath, "length"));
    }

    private static int ReadFlexibleInt(JsonElement metadata, string metadataPath, string name)
    {
        if (metadata.ValueKind == JsonValueKind.Object && metadata.TryGetProperty(name, out var declared))
        {
            if (declared.ValueKind == JsonValueKind.Number && declared.TryGetInt32(out var numeric)) return numeric;
            if (declared.ValueKind == JsonValueKind.String && int.TryParse(declared.GetString(), out var parsed)) return parsed;
        }
        return int.TryParse(Regex.Replace(ReadLooseRaw(metadataPath, name), @"[^0-9]", ""), out var loose) ? loose : 0;
    }

    private static string ReadLooseRaw(string path, string name)
    {
        try
        {
            if (!File.Exists(path)) return "";
            var match = Regex.Match(File.ReadAllText(path), $"\\\"{Regex.Escape(name)}\\\"\\s*:\\s*\\\"?([^\\\",}}\\r\\n]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }
        catch { return ""; }
    }

    private static string ClassifyCar(string value, out int confidence)
    {
        // Prima le eccezioni con numeri: "f2004" non deve diventare F2 solo
        // perché contiene la sequenza di caratteri f2.
        if (value.Contains("f2004") || value.Contains("sf70") || value.Contains("sf90") || value.Contains("f1")) { confidence = 90; return "formula"; }
        if (value.Contains("787b") || value.Contains("lola") || value.Contains("prototype")) { confidence = 85; return "prototype"; }
        if (value.Contains("250 gto") || value.Contains("250_gto") || value.Contains("288 gto") || value.Contains("288_gto") || value.Contains("312 67") || value.Contains("312_67") || value.Contains("312t")) { confidence = 85; return "historic"; }
        var rules = new (string key, string category)[] { ("kart", "kart"), ("formula junior", "formula junior"), ("formula 2", "formula 2"), ("formula", "formula"), ("f4", "formula 4"), ("f3", "formula 3"), ("gt3", "GT3"), ("gt4", "GT4"), ("gt2", "GT2"), ("gt1", "GT1"), ("tcr", "TCR"), ("touring", "touring"), ("lmp", "LMP"), ("hypercar", "hypercar"), ("cup", "cup"), ("historic", "historic") };
        foreach (var (key, category) in rules) if (value.Contains(key, StringComparison.OrdinalIgnoreCase)) { confidence = 90; return category; }
        // Chi non si riconosce non finisce in "special", che vuol dire esclusa
        // dalle gare: finisce in "touring". La stragrande maggioranza delle
        // auto installate da chi gioca sono berline e coupe da corsa — Civic,
        // AE86, MX5, Altezza — e il nome non lo dice quasi mai. Prima
        // sparivano tutte dalla carriera.
        if (value.Contains("road") || value.Contains("street") || value.Contains("z4")
            || value.Contains("458") || value.Contains("599")) { confidence = 40; return "road"; }
        confidence = 20; return "touring";
    }

    private static bool TryDeclaredCategory(string value, out string category)
    {
        category = "";
        if (string.IsNullOrWhiteSpace(value)) return false;
        // I tag di Assetto Corsa arrivano come "#Kart", "#GT3", "#street": il
        // cancelletto faceva fallire il confronto esatto, e un kart dichiarato
        // tale dal contenuto non veniva riconosciuto come kart.
        var normalized = value.Trim().TrimStart('#').ToLowerInvariant().Replace("_", " ").Trim();
        var known = new[] { "kart", "formula junior", "formula 4", "formula 3", "formula 2", "formula", "gt4", "gt3", "gt2", "gt1", "tcr", "touring", "cup", "prototype", "lmp", "hypercar", "historic", "road", "special" };
        var match = known.FirstOrDefault(x => normalized.Equals(x, StringComparison.OrdinalIgnoreCase));
        if (match == null) return false;
        category = match switch { "formula 4" => "formula 4", "formula junior" => "formula junior", _ => match };
        return true;
    }

    private static string ReadDeclaredCategory(JsonElement metadata)
    {
        var direct = ReadString(metadata, "category", "class", "discipline");
        if (TryDeclaredCategory(direct, out var directCategory)) return directCategory;
        if (metadata.ValueKind == JsonValueKind.Object && metadata.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Array)
            foreach (var tag in tags.EnumerateArray())
                if (tag.ValueKind == JsonValueKind.String && TryDeclaredCategory(tag.GetString() ?? "", out var tagCategory)) return tagCategory;
        return "";
    }

    private static JsonElement ReadJson(string path)
    {
        try { return File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone() : default; } catch { return default; }
    }
    private static string ReadLooseString(string path, params string[] names)
    {
        try
        {
            var text = File.ReadAllText(path);
            foreach (var name in names)
            {
                var match = Regex.Match(text, $"\\\"{Regex.Escape(name)}\\\"\\s*:\\s*\\\"([^\\\"]*)", RegexOptions.Singleline);
                if (match.Success) return match.Groups[1].Value.Replace("\\n", " ").Trim();
            }
        }
        catch { }
        return "";
    }
    private static int ReadLooseInt(string path, params string[] names)
    {
        try
        {
            var text = File.ReadAllText(path);
            foreach (var name in names)
            {
                var match = Regex.Match(text, $"\\\"{Regex.Escape(name)}\\\"\\s*:\\s*(\\d{{4}})", RegexOptions.Singleline);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var parsed)) return parsed;
            }
        }
        catch { }
        return 0;
    }
    private static string ReadLooseCategory(string path)
    {
        var direct = ReadLooseString(path, "category", "class", "discipline");
        if (TryDeclaredCategory(direct, out var category)) return category;
        try
        {
            var text = File.ReadAllText(path);
            var tags = Regex.Match(text, "\\\"tags\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (tags.Success)
                foreach (Match tag in Regex.Matches(tags.Groups[1].Value, "\\\"([^\\\"]+)\\\""))
                    if (TryDeclaredCategory(tag.Groups[1].Value, out category)) return category;
        }
        catch { }
        return "";
    }
    private static string ReadString(JsonElement json, params string[] names) { if (json.ValueKind != JsonValueKind.Object) return ""; foreach (var n in names) if (json.TryGetProperty(n, out var p) && p.ValueKind == JsonValueKind.String) return p.GetString() ?? ""; return ""; }
    /// <summary>
    /// Legge un intero da un metadato Assetto Corsa. I file ufficiali e le mod
    /// dichiarano spesso i valori numerici come stringhe ("power": "550"), e su
    /// un elemento di tipo String <c>TryGetInt32</c> non restituisce false: solleva
    /// un'eccezione. Prima questo bastava a far crollare l'intera scansione, e
    /// quindi l'avvio dell'applicazione.
    /// </summary>
    private static int ReadInt(JsonElement json, params string[] names)
    {
        if (json.ValueKind != JsonValueKind.Object) return 0;

        // Assetto Corsa mette i dati tecnici in "specs": "bhp" per la potenza,
        // "weight" per il peso — sono le voci che Content Manager mostra.
        // Cercandoli solo nella radice ogni auto vera risultava di 0 cavalli e
        // 0 chili, e il gradino di carriera veniva deciso su dati inesistenti.
        if (json.TryGetProperty("specs", out var specs) && specs.ValueKind == JsonValueKind.Object)
        {
            var inSpecs = ReadInt(specs, names);
            if (inSpecs > 0) return inSpecs;
        }

        foreach (var name in names)
        {
            if (!json.TryGetProperty(name, out var property)) continue;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number)) return number;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var real)) return (int)Math.Round(real);
            if (property.ValueKind != JsonValueKind.String) continue;
            var digits = Regex.Match(property.GetString() ?? "", @"-?\d+");
            if (digits.Success && int.TryParse(digits.Value, out var parsed)) return parsed;
        }
        return 0;
    }
}
