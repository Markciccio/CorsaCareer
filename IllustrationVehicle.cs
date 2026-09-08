namespace CorsaCareer;

/// <summary>
/// Che mezzo si vede in una tavola.
///
/// Il difetto: una gara di kart illustrata con monoposto anni Sessanta. Le
/// tavole vengono scelte in base al <em>momento</em> della carriera — un invito,
/// una crisi, una vittoria — e nessuno controllava che il mezzo raffigurato
/// fosse quello che si sta davvero guidando.
///
/// Le descrizioni esistono, ma coprono 13 tavole su 106. Per le altre l'unica
/// informazione disponibile è il nome del file, che per fortuna è descrittivo:
/// «manga-kart-first-race...», «manga-gt3-endurance-victory...». È poco, ma
/// basta a non mettere una monoposto sopra un kart.
/// </summary>
public enum IllustrationVehicle
{
    /// <summary>Nessun mezzo riconoscibile: va bene ovunque.</summary>
    Neutral,
    Kart,
    SingleSeater,
    ClosedCar
}

public static class IllustrationVehicles
{
    /// <summary>
    /// Il mezzo raffigurato, dedotto prima dalla descrizione — quando esiste — e
    /// altrimenti dal nome del file.
    ///
    /// Una tavola senza mezzo riconoscibile è neutra e resta utilizzabile
    /// sempre: sono le scene di paddock, officina, firma, crisi. Toglierle
    /// lascerebbe la carriera senza illustrazioni per metà dei suoi momenti.
    /// </summary>
    public static IllustrationVehicle Of(string fileOrPath)
    {
        var name = Path.GetFileName(fileOrPath ?? "").ToLowerInvariant();
        if (name.Length == 0) return IllustrationVehicle.Neutral;

        // La descrizione, dove c'è, è più affidabile del nome: dichiara i mezzi
        // visibili nella tavola, anche quelli sullo sfondo.
        var declared = IllustrationCatalog.Describe(name)?.Vehicles ?? "";
        var haystack = declared.Length > 0 ? declared.ToLowerInvariant() + " " + name : name;

        // Ordine importante: «formula-junior» contiene «formula», e una tavola
        // che mostra sia kart sia monoposto va classificata per il mezzo
        // principale, che nel nome viene per primo.
        var kart = FirstIndexOf(haystack, "kart");
        var single = FirstIndexOf(haystack, "formula", "monoposto", "ruote scoperte", "open-wheel");
        var closed = FirstIndexOf(haystack, "gt3", "gt4", "gt ", "touring", "turismo", "prototype", "prototipi", "endurance", "hypercar");

        var best = Math.Min(kart, Math.Min(single, closed));
        if (best == int.MaxValue) return IllustrationVehicle.Neutral;
        if (best == kart) return IllustrationVehicle.Kart;
        if (best == single) return IllustrationVehicle.SingleSeater;
        return IllustrationVehicle.ClosedCar;
    }

    private static int FirstIndexOf(string haystack, params string[] needles)
    {
        var best = int.MaxValue;
        foreach (var needle in needles)
        {
            var index = haystack.IndexOf(needle, StringComparison.Ordinal);
            if (index >= 0 && index < best) best = index;
        }
        return best;
    }

    /// <summary>
    /// Vero se la tavola può accompagnare una carriera che corre su questo
    /// gradino della scala. Le tavole neutre passano sempre.
    /// </summary>
    public static bool FitsRung(string fileOrPath, LadderRung rung)
    {
        var vehicle = Of(fileOrPath);
        if (vehicle == IllustrationVehicle.Neutral) return true;

        return rung.Path switch
        {
            LadderPath.Karting => vehicle == IllustrationVehicle.Kart,
            LadderPath.SingleSeater => vehicle == IllustrationVehicle.SingleSeater,
            LadderPath.Endurance or LadderPath.Touring => vehicle == IllustrationVehicle.ClosedCar,
            _ => true
        };
    }

    /// <summary>
    /// Filtra un elenco di tavole tenendo solo quelle coerenti col gradino.
    ///
    /// Se il filtro non lascia niente restituisce l'elenco originale: una
    /// tavola imprecisa è meglio di un riquadro vuoto, e la scelta di quale
    /// male sia minore va fatta qui, una volta, invece che a ogni chiamata.
    /// </summary>
    public static List<string> KeepFitting(IEnumerable<string> files, LadderRung rung)
    {
        var all = files.ToList();
        var fitting = all.Where(x => FitsRung(x, rung)).ToList();
        return fitting.Count > 0 ? fitting : all;
    }
}
