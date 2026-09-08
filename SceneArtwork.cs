namespace CorsaCareer1991;

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
    /// <summary>Tavola per un'attività della giornata, dal suo identificativo.</summary>
    public static string ForActivity(string activityId) => activityId switch
    {
        "palestra" => "attivita-palestra.png",
        "corsa" => "attivita-corsa.png",
        "massaggio" => "attivita-massaggio.png",
        "riposo" => "attivita-riposo.png",
        "tifosi" => "attivita-tifosi.png",
        "social" => "attivita-social.png",
        "pr" => "attivita-pr.png",
        "lavoro" => "attivita-lavoro.png",
        "haru-officina" => "haru-visita-officina.png",
        "haru-ricambi" => "haru-visita-ferramenta.png",
        "haru-gomme" => "haru-visita-gommista.png",
        "haru-contatti" => "haru-accordo-stretta-mano-nuovo.png",
        _ => ""
    };

    /// <summary>
    /// Tavola per una visita di Haru, scelta dal tipo di attività commerciale.
    /// Il fruttivendolo e l'assicuratore non sono la stessa scena.
    /// </summary>
    public static string ForSponsorVisit(string trade, bool accepted)
    {
        var t = (trade ?? "").ToLowerInvariant();
        var scene =
            t.Contains("riparazioni") || t.Contains("officina") ? "haru-visita-officina.png" :
            t.Contains("trattoria") || t.Contains("ramen") ? "haru-visita-ramen.png" :
            t.Contains("sushi") || t.Contains("poke") ? "haru-visita-sushi.png" :
            t.Contains("frutta") || t.Contains("alimentari") ? "haru-visita-fruttivendolo.png" :
            t.Contains("ferramenta") || t.Contains("utensili") ? "haru-visita-ferramenta.png" :
            t.Contains("pneumatici") || t.Contains("assetti") ? "haru-visita-gommista.png" :
            t.Contains("assicurativa") || t.Contains("assicurazioni") ? "haru-visita-assicurazioni.png" :
            "";

        // Se la scena del luogo non c'è, l'esito ha comunque una sua tavola: un
        // rifiuto e un accordo si raccontano diversamente.
        if (scene.Length == 0 || !Exists(scene))
            scene = accepted ? "haru-accordo-stretta-mano-nuovo.png" : "haru-rifiuto-porta.png";

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
        var neutral = stem + ".png";
        if (Exists(neutral)) return neutral;
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
            return Directory.EnumerateFiles(root, stem + "*.png")
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
        ("attivita-palestra.png", "Attività del pilota · palestra"),
        ("attivita-corsa.png", "Attività del pilota · corsa"),
        ("attivita-massaggio.png", "Attività del pilota · massaggio"),
        ("attivita-riposo.png", "Attività del pilota · riposo"),
        ("attivita-tifosi.png", "Attività del pilota · incontro con i tifosi"),
        ("attivita-social.png", "Attività del pilota · post sui social"),
        ("attivita-pr.png", "Attività del pilota · relazioni pubbliche"),
        ("attivita-lavoro.png", "Attività del pilota · giornata di lavoro"),
        ("haru-visita-officina.png", "Haru · officina di quartiere"),
        ("haru-visita-ramen.png", "Haru · trattoria di ramen"),
        ("haru-visita-sushi.png", "Haru · locale di sushi o poke"),
        ("haru-visita-fruttivendolo.png", "Haru · fruttivendolo o alimentari"),
        ("haru-visita-ferramenta.png", "Haru · ferramenta e utensili"),
        ("haru-visita-gommista.png", "Haru · gommista"),
        ("haru-visita-assicurazioni.png", "Haru · agenzia assicurativa"),
        ("haru-rifiuto-porta.png", "Haru · il rifiuto sulla porta"),
        ("haru-accordo-stretta-mano-nuovo.png", "Haru · l'accordo chiuso"),
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
