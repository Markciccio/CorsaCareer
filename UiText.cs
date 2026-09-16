namespace CorsaCareer;

/// <summary>Formatting helpers for user-facing labels. Internal content IDs never leak into the UI.</summary>
public static class UiText
{
    public static string Car(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return "vettura non indicata";
        var key = id.Trim().ToLowerInvariant();
        return key switch
        {
            "kart_rental_4t" => "Kart Rental 4T",
            "kart_kz_125" => "Kart KZ 125",
             "kart_oki_2t" => "Kart OKI 2T",
            _ => Humanize(key)
        };
    }

    public static string Track(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return "circuito non indicato";
        var key = id.Trim().ToLowerInvariant();
        return key switch
        {
            "90sgdsp_mobara_twin_circuit" => "Mobara Twin Circuit",
            "90sgdsp_mobara_twin_circuit-forward" => "Mobara Twin Circuit — tracciato forward",
            "90sgdsp_mobara_twin_circuit-reverse" => "Mobara Twin Circuit — tracciato reverse",
            _ => Humanize(key)
        };
    }

    private static string Humanize(string value)
    {
        var words = value.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words.Select(w => w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w[1..]));
    }
}
