namespace CorsaCareer;

public static class RaceImportIdentity
{
    public static string? Mismatch(string expectedTrack, string actualTrack, string expectedCar, string actualCar)
    {
        // Assetto Corsa scrive alcuni layout nel referto con un suffisso
        // (per esempio ks_barcelona-layout_gp oppure mobara-forward), mentre
        // Content Manager salva nel preset l'ID del circuito base. È lo stesso
        // contenuto, non una pista diversa; riconosciamo solo i nomi layout
        // convenzionali e non qualunque suffisso, così il controllo continua a
        // respingere referti estranei.
        if (!TracksMatch(expectedTrack, actualTrack)) return $"circuito diverso: atteso '{expectedTrack}', trovato '{actualTrack}'";
        if (!string.IsNullOrWhiteSpace(expectedCar) && !string.Equals(expectedCar, actualCar, StringComparison.OrdinalIgnoreCase)) return $"auto diversa: attesa '{expectedCar}', trovata '{actualCar}'";
        return null;
    }

    public static bool TracksMatch(string expectedTrack, string actualTrack)
    {
        if (string.Equals(expectedTrack, actualTrack, StringComparison.OrdinalIgnoreCase)) return true;
        var expected = expectedTrack.Trim();
        var actual = actualTrack.Trim();
        return IsLayoutOf(expected, actual) || IsLayoutOf(actual, expected);
    }

    private static bool IsLayoutOf(string baseTrack, string candidate)
    {
        if (candidate.StartsWith(baseTrack + "-layout_", StringComparison.OrdinalIgnoreCase)) return true;
        var suffix = candidate.StartsWith(baseTrack + "-", StringComparison.OrdinalIgnoreCase)
            ? candidate[(baseTrack.Length + 1)..]
            : "";
        return suffix.Equals("forward", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("reverse", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("short", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("long", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("gp", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("normal", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("national", StringComparison.OrdinalIgnoreCase)
            || suffix.Equals("club", StringComparison.OrdinalIgnoreCase);
    }
}
