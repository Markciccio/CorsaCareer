using System.Security.Cryptography;
using System.Text;

namespace CorsaCareer;

/// <summary>
/// Sceglie una formulazione fra più alternative equivalenti, evitando quelle
/// usate di recente.
///
/// È il meccanismo che rompe la ripetizione: con un seme stabile lo stesso
/// articolo resta identico se riaperto, ma due articoli diversi — o due gare
/// consecutive nella stessa situazione — pescano varianti diverse. La scelta
/// non usa <c>Random</c>: dipende dal seme, quindi è riproducibile e verificabile.
/// </summary>
public sealed class PhraseBank
{
    private readonly long seed;
    private readonly HashSet<string> recent;
    private int cursor;

    public PhraseBank(long seed, IEnumerable<string> recentlyUsed)
    {
        this.seed = seed;
        recent = new HashSet<string>(recentlyUsed ?? [], StringComparer.Ordinal);
    }

    /// <summary>Identificativi delle formulazioni scelte in questa composizione.</summary>
    public List<string> Used { get; } = [];

    public string Pick(string slot, params string[] options)
    {
        if (options.Length == 0) return "";
        cursor++;
        var offset = (int)((seed / 7 + cursor * 31) % options.Length);
        // Primo giro: si cerca una variante non usata di recente.
        for (var attempt = 0; attempt < options.Length; attempt++)
        {
            var index = (offset + attempt) % options.Length;
            var id = Id(slot, index);
            if (recent.Contains(id)) continue;
            Used.Add(id);
            return options[index];
        }
        // Tutte già usate: si riparte dalla prima della rotazione e la si registra
        // comunque, così la memoria continua a scorrere.
        var fallbackIndex = offset % options.Length;
        Used.Add(Id(slot, fallbackIndex));
        return options[fallbackIndex];
    }

    /// <summary>Valore stabile per riordinare elementi a parità di punteggio.</summary>
    public int Shuffle(string key)
    {
        var hash = 2166136261L;
        foreach (var character in key) hash = (hash ^ character) * 16777619 % 2147483647;
        return (int)((hash + seed) % 1000);
    }

    private static string Id(string slot, int index) => $"{slot}#{index}";

    /// <summary>
    /// Aggiorna la memoria della carriera mantenendola limitata: senza il taglio
    /// la lista crescerebbe per sempre dentro il salvataggio.
    /// </summary>
    public static void Remember(CareerState career, IEnumerable<string> used, int capacity = NarrativeEngine.PhraseMemory)
    {
        career.UsedPhrases ??= [];
        foreach (var id in used)
        {
            career.UsedPhrases.Remove(id);
            career.UsedPhrases.Add(id);
        }
        if (career.UsedPhrases.Count > capacity)
            career.UsedPhrases.RemoveRange(0, career.UsedPhrases.Count - capacity);
    }

    /// <summary>Impronta del testo: serve ai test per misurare la varietà prodotta.</summary>
    public static string Fingerprint(string text)
    {
        var normalized = new StringBuilder();
        foreach (var character in text ?? "")
            if (char.IsLetter(character)) normalized.Append(char.ToLowerInvariant(character));
        return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(normalized.ToString())))[..16];
    }
}
