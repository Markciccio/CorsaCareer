namespace CorsaCareer1991;

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

    private static string Humanize(string value)
    {
        var words = value.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words.Select(w => w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w[1..]));
    }
}
