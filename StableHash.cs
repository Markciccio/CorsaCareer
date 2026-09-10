namespace CorsaCareer;

/// <summary>
/// Un numero sempre uguale a partire dalle stesse cose.
///
/// Serviva perche' <c>HashCode.Combine</c> in .NET non e' stabile: usa un seme
/// casuale diverso a ogni avvio del processo. Era usato per scegliere il nome e
/// il profilo delle squadre e per comporre la griglia degli avversari, quindi
/// due esecuzioni identiche del banco davano due carriere diverse — al banco si
/// misuravano ventidue stagioni e ventotto vittorie in una, ventitre stagioni e
/// ventitre vittorie nell'altra, con lo stesso pilota e lo stesso binario. Con
/// una misura che non si ripete non si puo' tarare niente: non si distingue
/// l'effetto di una modifica dal rumore.
///
/// E' la stessa funzione (FNV-1a) gia' usata da <c>RaceSimulator.SeedFor</c> e
/// da <c>OpportunityGenerator.StableSeed</c>, messa dove possono usarla tutti.
/// </summary>
public static class StableHash
{
    /// <summary>Il numero stabile che nasce da questi valori, nell'ordine dato.</summary>
    public static int Of(params object?[] parti) => Of(string.Join('|', parti.Select(x => x?.ToString() ?? "")));

    /// <summary>Il numero stabile che nasce da questo testo.</summary>
    public static int Of(string testo)
    {
        unchecked
        {
            var h = 2166136261u;
            foreach (var c in testo) { h ^= c; h *= 16777619u; }
            return (int)(h & 0x7FFFFFFF);
        }
    }
}
