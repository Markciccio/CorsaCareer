using System.Security.Cryptography;
using System.Text.Json;

namespace CorsaCareer1991;

public enum PhotoProvenance
{
    /// <summary>Screenshot realmente acquisito da Assetto Corsa durante la sessione.</summary>
    AssettoCorsaScreenshot,
    /// <summary>Immagine fornita dall'utente e importata nell'archivio.</summary>
    UserImport,
    /// <summary>Immagine generata da un provider AI: dichiarata come illustrazione.</summary>
    AiIllustration,
    /// <summary>Asset editoriale incluso nel pacchetto: illustrazione, non documento.</summary>
    BundledIllustration,
    /// <summary>Nessuna immagine disponibile.</summary>
    None
}

public sealed class ArticlePhoto
{
    public string Path { get; set; } = "";
    public PhotoProvenance Provenance { get; set; } = PhotoProvenance.None;
    public string Caption { get; set; } = "";
    public string Credit { get; set; } = "";
    /// <summary>Vero solo per immagini che documentano davvero l'evento raccontato.</summary>
    public bool DocumentsEvent { get; set; }
    public string Sha256 { get; set; } = "";
    public string View { get; set; } = "";

    public bool Exists => !string.IsNullOrWhiteSpace(Path) && File.Exists(Path);
}

/// <summary>
/// Sceglie e dichiara l'immagine di un articolo.
///
/// Regola del progetto, mantenuta: l'unica immagine che può essere presentata
/// come documento dell'evento è uno screenshot realmente acquisito da Assetto
/// Corsa durante quella sessione, oppure una foto importata esplicitamente
/// dall'utente. Tutto il resto — asset del pacchetto o immagini generate da un
/// provider AI — è un'illustrazione, e la didascalia lo dichiara.
///
/// Non esiste un percorso che scarichi immagini dal web per presentarle come
/// foto della gara: sarebbero fotografie reali usate per documentare eventi che
/// non sono mai accaduti.
/// </summary>
public static class ArticlePhotoService
{
    public const string AiMetadataKind = "AI_GENERATED_ILLUSTRATION";
    public const string UserMetadataKind = "MANUAL_PHOTO_IMPORT";
    public const string ScreenshotMetadataKind = "ASSETTO_CORSA_SCREENSHOT";

    /// <summary>
    /// Ordine di priorità: screenshot reale della sessione, foto importata
    /// dall'utente, illustrazione AI dichiarata, asset editoriale del pacchetto.
    /// </summary>
    public static ArticlePhoto ForEvent(CareerState career, CareerEventRecord? story, string mediaRoot)
    {
        if (story != null && !string.IsNullOrWhiteSpace(story.PhotoPath) && File.Exists(story.PhotoPath))
            return Describe(story.PhotoPath, story);

        // Un altro referto sullo stesso circuito può avere uno screenshot reale:
        // va dichiarato come archivio, non come foto di questa sessione.
        if (story != null && !string.IsNullOrWhiteSpace(story.Track))
        {
            var archived = career.RaceHistory
                .Where(x => x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.PhotoPath) && File.Exists(x.PhotoPath))
                .OrderByDescending(x => x.DateUtc)
                .FirstOrDefault();
            if (archived != null)
            {
                var photo = Describe(archived.PhotoPath, story);
                photo.DocumentsEvent = false;
                photo.Caption = $"Archivio: {photo.Caption.ToLowerInvariant()} — immagine di una sessione precedente a {NarrativeEngine.Capitalize(story.Track)}, non di questo weekend";
                return photo;
            }
        }

        var aiIllustration = FindAiIllustration(mediaRoot, story);
        if (aiIllustration != null) return aiIllustration;

        // Senza evento agganciato resta l'illustrazione generica del portale,
        // dichiarata come tale: meglio di un riquadro vuoto, e comunque non
        // presentata come fotografia.
        var bundled = story == null
            ? AssetPaths.File("portal-hero-gt2.png")
            : EditorialAssets.ForEvent(story);
        if (!string.IsNullOrWhiteSpace(bundled) && File.Exists(bundled))
        {
            var comic = IsComicArtwork(bundled);
            return new ArticlePhoto
            {
                Path = bundled,
                Provenance = PhotoProvenance.BundledIllustration,
                DocumentsEvent = false,
                Caption = comic
                    ? "Tavola narrativa originale generata con AI — interpreta questa fase, non documenta l'evento"
                    : "Illustrazione editoriale del pacchetto — non è una fotografia di questo evento",
                Credit = comic ? "Archivio illustrato CorsaCareer" : "Asset incluso in CorsaCareer"
            };
        }

        return new ArticlePhoto
        {
            Provenance = PhotoProvenance.None,
            DocumentsEvent = false,
            Caption = "Nessuna immagine disponibile per questo evento"
        };
    }

    public static ArticlePhoto Describe(string path, CareerEventRecord? story = null)
    {
        var photo = new ArticlePhoto { Path = path, View = PhotoSource.View(path), Sha256 = ReadMetadata(path, "sha256") };
        var kind = ReadMetadata(path, "kind");
        var bundledRoot = AssetPaths.Root + Path.DirectorySeparatorChar;
        var isBundled = Path.GetFullPath(path).StartsWith(bundledRoot, StringComparison.OrdinalIgnoreCase);

        if (isBundled)
        {
            var comic = IsComicArtwork(path);
            photo.Provenance = PhotoProvenance.BundledIllustration;
            photo.DocumentsEvent = false;
            photo.Caption = comic
                ? "Tavola narrativa originale generata con AI — interpreta questa fase, non documenta l'evento"
                : "Illustrazione editoriale del pacchetto — non è una fotografia di questo evento";
            photo.Credit = comic ? "Archivio illustrato CorsaCareer" : "Asset incluso in CorsaCareer";
            return photo;
        }
        if (kind.Equals(ScreenshotMetadataKind, StringComparison.OrdinalIgnoreCase))
        {
            photo.Provenance = PhotoProvenance.AssettoCorsaScreenshot;
            photo.DocumentsEvent = true;
            var where = story != null && !string.IsNullOrWhiteSpace(story.Track) ? $" a {NarrativeEngine.Capitalize(story.Track)}" : "";
            photo.Caption = $"Screenshot acquisito da Assetto Corsa durante la sessione{where}" + (string.IsNullOrWhiteSpace(photo.View) ? "" : $" · {photo.View}");
            photo.Credit = "Cattura reale della sessione";
            return photo;
        }
        if (kind.Equals(UserMetadataKind, StringComparison.OrdinalIgnoreCase))
        {
            photo.Provenance = PhotoProvenance.UserImport;
            photo.DocumentsEvent = true;
            photo.Caption = "Fotografia importata dall'utente" + (string.IsNullOrWhiteSpace(photo.View) ? "" : $" · {photo.View}");
            photo.Credit = "Archivio personale del giocatore";
            return photo;
        }
        if (kind.Equals(AiMetadataKind, StringComparison.OrdinalIgnoreCase))
        {
            photo.Provenance = PhotoProvenance.AiIllustration;
            photo.DocumentsEvent = false;
            var prompt = ReadMetadata(path, "prompt");
            var provider = ReadMetadata(path, "provider");
            photo.Caption = "Illustrazione generata da intelligenza artificiale — non documenta l'evento";
            photo.Credit = string.IsNullOrWhiteSpace(provider) ? "Provider AI non dichiarato" : $"Generata con {provider}";
            if (!string.IsNullOrWhiteSpace(prompt)) photo.Caption += $" · richiesta: «{Shorten(prompt, 120)}»";
            return photo;
        }

        photo.Provenance = PhotoProvenance.UserImport;
        photo.DocumentsEvent = false;
        photo.Caption = "Immagine archiviata con provenienza non verificata: non viene presentata come documento dell'evento";
        return photo;
    }

    /// <summary>
    /// Cartella in cui un provider esterno può depositare illustrazioni generate.
    /// CorsaCareer non chiama nessun servizio: se il file esiste e dichiara la
    /// propria provenienza, viene usato come illustrazione.
    /// </summary>
    public static string AiIllustrationDirectory(string mediaRoot) => Path.Combine(mediaRoot, "ai-illustrations");

    private static ArticlePhoto? FindAiIllustration(string mediaRoot, CareerEventRecord? story)
    {
        try
        {
            var directory = AiIllustrationDirectory(mediaRoot);
            if (!Directory.Exists(directory)) return null;
            var files = Directory.EnumerateFiles(directory, "*.*")
                .Where(x => new[] { ".png", ".jpg", ".jpeg", ".webp" }.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
                .ToList();
            if (files.Count == 0) return null;
            // Corrispondenza per tipo di evento o circuito, se il nome del file lo dichiara.
            var preferred = files.FirstOrDefault(x =>
                story != null && !string.IsNullOrWhiteSpace(story.Type) && Path.GetFileName(x).Contains(story.Type, StringComparison.OrdinalIgnoreCase))
                ?? files.FirstOrDefault(x =>
                    story != null && !string.IsNullOrWhiteSpace(story.Track) && Path.GetFileName(x).Contains(story.Track, StringComparison.OrdinalIgnoreCase));
            if (preferred == null) return null;
            return Describe(preferred, story);
        }
        catch (Exception error)
        {
            CareerLog.Warn("foto", $"illustrazioni AI non leggibili: {error.Message}");
            return null;
        }
    }

    /// <summary>
    /// Registra un'immagine nell'archivio con la sua provenienza. È l'unico modo
    /// per far entrare un file nella pipeline: senza sidecar l'immagine resta
    /// classificata come non verificata.
    /// </summary>
    public static ArticlePhoto Register(string sourcePath, string destinationDirectory, PhotoProvenance provenance, string view = "", string prompt = "", string provider = "")
    {
        var kind = provenance switch
        {
            PhotoProvenance.AssettoCorsaScreenshot => ScreenshotMetadataKind,
            PhotoProvenance.UserImport => UserMetadataKind,
            PhotoProvenance.AiIllustration => AiMetadataKind,
            _ => "UNVERIFIED"
        };
        Directory.CreateDirectory(destinationDirectory);
        var destination = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
        if (!string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            File.Copy(sourcePath, destination, true);
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(destination)));
        var metadata = new
        {
            source = sourcePath,
            copiedUtc = DateTime.UtcNow,
            kind,
            view = string.IsNullOrWhiteSpace(view) ? "visuale non dichiarata" : view,
            prompt,
            provider,
            sha256 = hash
        };
        File.WriteAllText(destination + ".meta.json", JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
        return Describe(destination);
    }

    private static string ReadMetadata(string path, string property)
    {
        try
        {
            var metadata = path + ".meta.json";
            if (!File.Exists(metadata)) return "";
            using var document = JsonDocument.Parse(File.ReadAllText(metadata));
            return document.RootElement.TryGetProperty(property, out var value) ? value.GetString() ?? "" : "";
        }
        catch { return ""; }
    }

    private static string Shorten(string value, int length) =>
        string.IsNullOrEmpty(value) || value.Length <= length ? value : value[..length] + "…";

    private static bool IsComicArtwork(string path) =>
        Path.GetFileName(path).StartsWith("manga-", StringComparison.OrdinalIgnoreCase);
}
