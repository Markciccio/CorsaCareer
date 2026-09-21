namespace CorsaCareer;

/// <summary>
/// La tavola che accompagna una scena.
///
/// Prima ogni interlocutore di Haru compariva con lo stesso ritratto — il
/// fruttivendolo, il gommista e l'assicuratore avevano tutti la faccia del
/// meccanico. Qui ogni scena dichiara quale file cerca.
///
/// I file possono non esistere ancora: in quel caso si ripiega su una tavola
/// presente, e la scena funziona lo stesso. Aggiungendo l'immagine con il nome
/// giusto in <c>assets/</c> viene usata senza toccare il codice.
/// </summary>
public static class SceneArtwork
{
    private static int sponsorRotation;
    /// <summary>Tavola per un'attività della giornata, dal suo identificativo.</summary>
    public static string ForActivity(string activityId, int variantSeed = 0)
    {
        var id = (activityId ?? "").ToLowerInvariant();
        var category = id switch
        {
            "palestra" or "corsa" or "kart-training" => "training",
            "studio" or "scuola-kart" or "scuola-sae" or "scuola-tooru" or "scuola-volantini" => "school",
            "social" or "pr" or "instagram-allenamento" or "domande-follower" or "youtube" or "intervista-radio" => "social",
            "massaggio" or "riposo" => "recovery",
            "tifosi" or "lavoro" => "race",
            _ when id.StartsWith("haru-", StringComparison.OrdinalIgnoreCase) => "sponsor",
            _ => ""
        };
        var rotated = PickVariant(category, variantSeed);
        if (!string.IsNullOrWhiteSpace(rotated)) return rotated;
        return id switch
        {
        "palestra" => "activity-pilota-palestra.jpg",
        "corsa" => "activity-pilota-corsa-aperto.jpg",
        "massaggio" => "activity-pilota-massaggio-fisioterapia.jpg",
        "riposo" => "activity-pilota-riposo-casa.jpg",
        "tifosi" => "activity-pilota-tifosi.jpg",
        "social" => "activity-pilota-social.jpg",
        "pr" => "activity-pilota-pr.jpg",
        "lavoro" => "activity-pilota-lavoro.jpg",
        "studio" => "kart-215-rookie-studying-map.jpg",
        "kart-training" => "kart-096-allenamento-team.jpg",
        "haru-officina" => "haru-visita-officina.jpg",
        "haru-ricambi" => "haru-visita-ferramenta.jpg",
        "haru-gomme" => "haru-visita-gommista.jpg",
        "haru-contatti" => "anime-haru-telefonata-sponsor-stazione.jpg",
        "haru-accompagna" => "haru-accordo-stretta-mano-nuovo.jpg",

        // Haru dietro una tastiera: il sito, la pagina, il profilo. La tavola e'
        // la stessa perche' la scena e' la stessa — lui, al computer, di sera.
        "haru-sito" => "character-haru-senda-anxious-data.jpg",
        "haru-pagina" => "character-haru-senda-anxious-data.jpg",
        "haru-instagram" => "character-haru-senda-anxious-data.jpg",

        // Il lavoro d'immagine del pilota.
        "instagram-allenamento" => "anime-pilota-attivita-social-post.jpg",
        "domande-follower" => "activity-pilota-social.jpg",
        "youtube" => "anime-pilota-attivita-relazioni-pubbliche.jpg",
        "intervista-radio" => "activity-pilota-pr.jpg",

        // La scuola e gli amici.
        "scuola-kart" => "scuola-kart.jpg",
        "scuola-sae" => "character-sae-kurihara-scuola.jpg",
        "scuola-tooru" => "character-tooru-inagaki-scuola.jpg",
        "scuola-volantini" => "character-tooru-inagaki-scuola.jpg",

        // Le attività che non hanno ancora una chiave dedicata ricadono sul
        // ritratto del personaggio; scuola e studio hanno invece tavole proprie.
        _ => ""
        };
    }

    /// <summary>Selezione stabile delle tavole compatte generate per la rotazione.</summary>
    private static string PickVariant(string category, int seed)
    {
        if (string.IsNullOrWhiteSpace(category)) return "";
        try
        {
            var dir = Path.Combine(AssetPaths.Root, "variants");
            if (!Directory.Exists(dir)) return "";
            var files = Directory.EnumerateFiles(dir, category + "-*.jpg")
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
            if (files.Length == 0) return "";
            var index = Math.Abs(seed == int.MinValue ? 0 : seed) % files.Length;
            return Path.Combine("variants", Path.GetFileName(files[index]));
        }
        catch { return ""; }
    }

    /// <summary>
    /// Tavola per una visita di Haru, scelta dal tipo di attività commerciale.
    /// Il fruttivendolo e l'assicuratore non sono la stessa scena.
    /// </summary>
    public static string ForSponsorVisit(string trade, bool accepted)
    {
        var t = (trade ?? "").ToLowerInvariant();
        var scene =
            t.Contains("riparazioni") || t.Contains("officina") ? "haru-visita-officina.jpg" :
            t.Contains("trattoria") || t.Contains("ramen") ? "haru-visita-ramen.jpg" :
            t.Contains("sushi") || t.Contains("poke") ? "haru-visita-sushi.jpg" :
            t.Contains("frutta") || t.Contains("alimentari") ? "haru-visita-fruttivendolo.jpg" :
            t.Contains("ferramenta") || t.Contains("utensili") ? "haru-visita-ferramenta.jpg" :
            t.Contains("pneumatici") || t.Contains("assetti") ? "haru-visita-gommista.jpg" :
            t.Contains("assicurativa") || t.Contains("assicurazioni") ? "haru-visita-assicurazioni.jpg" :
            "";

        // Se la scena del luogo non c'è, l'esito ha comunque una sua tavola: un
        // rifiuto e un accordo si raccontano diversamente.
        if (scene.Length == 0 || !Exists(scene))
            scene = accepted ? "haru-accordo-stretta-mano-nuovo.jpg" : "haru-rifiuto-porta.jpg";

        var rotated = PickVariant("sponsor", sponsorRotation++);
        if (!string.IsNullOrWhiteSpace(rotated)) return rotated;
        return Exists(scene) ? scene : "";
    }

    /// <summary>Tavola per il momento in cui si decide se correre.</summary>
    public static string ForRaceChoice() =>
        Exists("decisione-corri-o-salta.png") ? "decisione-corri-o-salta.png" : "";

    /// <summary>
    /// Il ritratto di chi parla. Un interlocutore occasionale non ha un
    /// ritratto proprio: in quel caso la scena mostra il luogo, non la faccia.
    /// </summary>
    public static string PortraitFor(string speaker)
    {
        var s = (speaker ?? "").ToLowerInvariant();
        var stem =
            s.Contains("haru") ? "character-haru-senda" :
            s.Contains("genji") ? "character-genji-arakawa" :
            s.Contains("miki") ? "character-miki-arisawa" :
            s.Contains("rei") ? "character-rei-kisaragi" :
            s.Contains("riku") ? "character-riku-hayase" :
            s.Contains("shigeo") ? "character-shigeo-kanda" :
            s.Contains("noa") ? "character-noa-minazuki" :
            "";
        if (stem.Length == 0) return "";

        // Non tutti i personaggi hanno il ritratto neutro: di Rei Kisaragi
        // esistono solo le varianti con espressione. Puntare al file «base»
        // senza verificarlo lasciava un riquadro vuoto.
        foreach (var extension in new[] { ".jpg", ".png" })
        {
            var neutral = stem + extension;
            if (Exists(neutral)) return neutral;
        }
        return FirstVariantOf(stem);
    }

    /// <summary>
    /// La prima variante disponibile di un personaggio, in ordine stabile: una
    /// scena non deve cambiare ritratto a ogni apertura.
    /// </summary>
    private static string FirstVariantOf(string stem)
    {
        try
        {
            var root = AssetPaths.Root;
            if (!Directory.Exists(root)) return "";
            return Directory.EnumerateFiles(root)
                .Where(path => Path.GetFileName(path).StartsWith(stem, StringComparison.OrdinalIgnoreCase))
                .Where(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault() ?? "";
        }
        catch { return ""; }
    }

    /// <summary>
    /// Vero se la tavola è davvero presente. Serve a non chiedere alla scena un
    /// file che non c'è: senza questo controllo il riquadro resterebbe vuoto.
    /// </summary>
    public static bool Exists(string file)
    {
        if (string.IsNullOrWhiteSpace(file)) return false;
        try { return File.Exists(AssetPaths.File(file)); }
        catch { return false; }
    }

    /// <summary>
    /// Tutti i file che il programma cercherebbe, con il momento a cui servono.
    /// È l'elenco di cosa manca: serve a produrre le tavole senza doverle
    /// dedurre dal codice.
    /// </summary>
    public static IReadOnlyList<(string File, string Moment)> Expected() =>
    [
        ("activity-pilota-palestra.jpg", "Attività del pilota · palestra"),
        ("activity-pilota-corsa-aperto.jpg", "Attività del pilota · corsa"),
        ("activity-pilota-massaggio-fisioterapia.jpg", "Attività del pilota · massaggio"),
        ("activity-pilota-riposo-casa.jpg", "Attività del pilota · riposo"),
        ("activity-pilota-tifosi.jpg", "Attività del pilota · incontro con i tifosi"),
        ("activity-pilota-social.jpg", "Attività del pilota · post sui social"),
        ("activity-pilota-pr.jpg", "Attività del pilota · relazioni pubbliche"),
        ("activity-pilota-lavoro.jpg", "Attività del pilota · giornata di lavoro"),
        ("kart-215-rookie-studying-map.jpg", "Attività del pilota · studiare"),
        ("kart-096-allenamento-team.jpg", "Attività del pilota · turni di allenamento kart"),
        ("haru-visita-officina.jpg", "Haru · officina di quartiere"),
        ("haru-visita-ramen.jpg", "Haru · trattoria di ramen"),
        ("haru-visita-sushi.jpg", "Haru · locale di sushi o poke"),
        ("haru-visita-fruttivendolo.jpg", "Haru · fruttivendolo o alimentari"),
        ("haru-visita-ferramenta.jpg", "Haru · ferramenta e utensili"),
        ("haru-visita-gommista.jpg", "Haru · gommista"),
        ("haru-visita-assicurazioni.jpg", "Haru · agenzia assicurativa"),
        ("haru-rifiuto-porta.jpg", "Haru · il rifiuto sulla porta"),
        ("haru-accordo-stretta-mano-nuovo.jpg", "Haru · l'accordo chiuso"),
        ("decisione-corri-o-salta.png", "Il giorno della gara · corri o salti"),
        ("decisione-calendario.png", "Il pilota davanti al calendario"),
        ("manga-formula1-debutto-griglia.png", "Formula 1 · il debutto in griglia"),
        ("manga-formula1-primo-podio.png", "Formula 1 · il primo podio"),
        ("manga-formula2-ultimo-gradino.png", "Formula internazionale · l'anticamera")
    ];

    /// <summary>Quelle che ancora non esistono.</summary>
    public static List<(string File, string Moment)> Missing() =>
        Expected().Where(x => !Exists(x.File)).ToList();
}
