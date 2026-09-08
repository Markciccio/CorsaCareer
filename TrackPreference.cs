namespace CorsaCareer;

/// <summary>Esito della ricerca di una sede: quale pista si usa, e se è quella richiesta.</summary>
public sealed record TrackChoice(ContentTrackRecord? Track, bool Substituted, string Note)
{
    public bool Found => Track != null;
}

/// <summary>
/// Trova la sede di un evento che ne richiede una precisa.
///
/// Una selezione può essere legata a un circuito specifico: quel tracciato fa
/// parte di ciò che l'evento è. Ma non si può pretendere che sia installato,
/// e nemmeno sostituirlo di nascosto — un provino corso altrove non è lo stesso
/// provino, e il giocatore deve saperlo. Qui si cerca la pista richiesta, poi
/// un equivalente per caratteristiche, e in ogni caso si dichiara cosa è stato
/// scelto e perché.
/// </summary>
public static class TrackPreference
{
    /// <summary>
    /// Cerca <paramref name="preferredId"/>; se manca, ripiega su una pista con
    /// caratteristiche compatibili fra quelle installate.
    /// </summary>
    /// <param name="tags">
    /// Parole che descrivono il tipo di tracciato cercato (per esempio "short",
    /// "tecnico"). Servono a scegliere un rimpiazzo sensato invece del primo
    /// circuito in ordine alfabetico.
    /// </param>
    /// <param name="preferredLengthMeters">
    /// Lunghezza tipica del tracciato richiesto: a parità di tutto, si preferisce
    /// la pista di lunghezza più simile. 0 se non rilevante.
    /// </param>
    public static TrackChoice Resolve(IReadOnlyList<ContentTrackRecord> installed, string preferredId,
        IReadOnlyList<string>? tags = null, int preferredLengthMeters = 0, string? preferredName = null)
    {
        if (installed.Count == 0)
            return new TrackChoice(null, false, "Nessun circuito installato: l'evento non può avere una sede.");

        var exact = installed.FirstOrDefault(x => x.Id.Equals(preferredId, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return new TrackChoice(exact, false, $"{exact.Name}: il circuito previsto dal programma.");

        // Un id come "fuji_short" può essere installato con un id diverso ma
        // riconoscibile nel nome: si prova prima di dichiarare la sostituzione.
        var byName = installed.FirstOrDefault(x =>
            Contains(x.Id, preferredId) || Contains(x.Name, preferredId)
            || (!string.IsNullOrWhiteSpace(preferredName) && Contains(x.Name, preferredName!)));
        if (byName != null)
            return new TrackChoice(byName, false, $"{byName.Name}: corrisponde al circuito previsto fra i contenuti installati.");

        var wanted = preferredName ?? preferredId;
        var candidates = installed
            .Select(track => new { Track = track, Score = Affinity(track, tags, preferredLengthMeters) })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Track.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var chosen = candidates[0].Track;
        return new TrackChoice(chosen, true,
            $"{wanted} non è installato: la prova si svolge a {chosen.Name}, il tracciato disponibile più affine. " +
            "Il confronto con i riferimenti tiene conto della sede reale.");
    }

    /// <summary>Quanto una pista installata assomiglia a quella richiesta.</summary>
    private static int Affinity(ContentTrackRecord track, IReadOnlyList<string>? tags, int preferredLengthMeters)
    {
        var score = 0;
        var haystack = $"{track.Id} {track.Name} {track.Category} {track.Country}".ToLowerInvariant();
        if (tags != null)
            foreach (var tag in tags)
                if (!string.IsNullOrWhiteSpace(tag) && haystack.Contains(tag.ToLowerInvariant(), StringComparison.Ordinal))
                    score += 30;

        if (preferredLengthMeters > 0 && track.LengthMeters > 0)
        {
            // Più la lunghezza è vicina, più il rimpiazzo è credibile: un
            // provino pensato per un tracciato corto non va spostato su un
            // circuito di sette chilometri se esiste alternativa.
            var delta = Math.Abs(track.LengthMeters - preferredLengthMeters);
            score += Math.Max(0, 40 - delta / 100);
        }
        // A parità di resto vince una pista con metadati affidabili.
        score += Math.Clamp(track.Confidence / 10, 0, 10);
        return score;
    }

    private static bool Contains(string? value, string needle)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(needle)) return false;
        var normalizedNeedle = needle.Replace("_", " ").Replace("-", " ").Trim();
        var normalizedValue = value.Replace("_", " ").Replace("-", " ");
        return normalizedValue.Contains(normalizedNeedle, StringComparison.OrdinalIgnoreCase);
    }
}
