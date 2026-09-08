namespace CorsaCareer;

public static class RaceImportIdentity
{
    public static string? Mismatch(string expectedTrack, string actualTrack, string expectedCar, string actualCar)
    {
        // Assetto Corsa scrive alcuni layout nel referto con il suffisso
        // "-layout_*" (per esempio ks_barcelona-layout_gp), mentre Content
        // Manager salva nel preset l'ID del circuito base (ks_barcelona).
        // È lo stesso contenuto, non una pista diversa; non normalizziamo
        // altri prefissi/suffissi per mantenere la protezione anti-referto
        // estraneo.
        if (!TracksMatch(expectedTrack, actualTrack)) return $"circuito diverso: atteso '{expectedTrack}', trovato '{actualTrack}'";
        if (!string.IsNullOrWhiteSpace(expectedCar) && !string.Equals(expectedCar, actualCar, StringComparison.OrdinalIgnoreCase)) return $"auto diversa: attesa '{expectedCar}', trovata '{actualCar}'";
        return null;
    }

    public static bool TracksMatch(string expectedTrack, string actualTrack)
    {
        if (string.Equals(expectedTrack, actualTrack, StringComparison.OrdinalIgnoreCase)) return true;
        var expected = expectedTrack.Trim();
        var actual = actualTrack.Trim();
        return actual.StartsWith(expected + "-layout_", StringComparison.OrdinalIgnoreCase)
            || expected.StartsWith(actual + "-layout_", StringComparison.OrdinalIgnoreCase);
    }
}
