namespace CorsaCareer1991;

/// <summary>
/// Le prime volte di una carriera, registrate quando accadono.
///
/// Servono a distinguere ciò che non si può dedurre dai contatori. Con
/// <c>career.Wins &gt; 0</c> non si sa se questa è la prima vittoria assoluta o
/// la prima in una categoria nuova, e sono due cose che si raccontano in modo
/// diverso: la prima capita una volta sola in una vita, la seconda a ogni salto.
///
/// Ogni campo conserva la data narrativa e il luogo, così la narrazione può
/// tornarci sopra anni dopo: «tre anni fa eri arrivato qui con i soldi per una
/// sola occasione». Un campo vuoto significa «non è ancora accaduto», ed è lo
/// stato corretto di una carriera appena iniziata o salvata prima di questa
/// funzione.
/// </summary>
public sealed class CareerFirstRecord
{
    /// <summary>Data narrativa in cui è accaduto. Default = mai accaduto.</summary>
    public DateTime StoryDate { get; set; }
    /// <summary>Dove è accaduto: circuito, paddock o categoria.</summary>
    public string Where { get; set; } = "";
    /// <summary>La categoria in cui è accaduto: serve a distinguere i «primi» di livello.</summary>
    public string Tier { get; set; } = "";
    /// <summary>La stagione: permette di misurare quanto tempo è passato.</summary>
    public int Season { get; set; }
    /// <summary>Una riga di contesto, costruita dai fatti al momento della registrazione.</summary>
    public string Note { get; set; } = "";

    public bool Happened => StoryDate != default;
}

/// <summary>
/// L'archivio delle prime volte. Le chiavi sono stabili: la narrazione le
/// interroga per nome e non per posizione.
/// </summary>
public sealed class CareerFirsts
{
    public const string Test = "primo-test";
    public const string Race = "prima-gara";
    public const string Points = "primi-punti";
    public const string Podium = "primo-podio";
    public const string Win = "prima-vittoria";
    public const string Pole = "prima-pole";
    public const string Dnf = "primo-ritiro";
    public const string Contract = "primo-contratto";
    public const string Salary = "primo-stipendio";
    public const string Sponsor = "primo-sponsor";
    public const string Title = "primo-titolo";
    public const string Selection = "prima-selezione";
    public const string SingleSeater = "prima-monoposto";

    /// <summary>Le prime volte già accadute, per chiave.</summary>
    public Dictionary<string, CareerFirstRecord> Entries { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Has(string key) => Entries.TryGetValue(key, out var entry) && entry.Happened;
    public CareerFirstRecord? Get(string key) => Entries.TryGetValue(key, out var entry) && entry.Happened ? entry : null;

    /// <summary>
    /// Registra una prima volta, se non è già accaduta. Restituisce vero solo
    /// quando è davvero la prima: il chiamante usa l'esito per decidere se
    /// questo evento va raccontato come un debutto.
    /// </summary>
    public bool Register(string key, DateTime storyDate, string where, string tier, int season, string note = "")
    {
        if (Has(key)) return false;
        Entries[key] = new CareerFirstRecord
        {
            StoryDate = storyDate, Where = where ?? "", Tier = tier ?? "", Season = season, Note = note ?? ""
        };
        return true;
    }

    /// <summary>Anni narrativi passati da una prima volta. 0 se non è accaduta.</summary>
    public int YearsSince(string key, DateTime now)
    {
        var entry = Get(key);
        if (entry == null) return 0;
        return Math.Max(0, (int)((now - entry.StoryDate).TotalDays / 365));
    }

    /// <summary>
    /// Vero se il pilota è già stato su questo circuito per una delle sue prime
    /// volte: è l'aggancio della memoria narrativa a un luogo.
    /// </summary>
    public IEnumerable<KeyValuePair<string, CareerFirstRecord>> AtTrack(string? track)
    {
        if (string.IsNullOrWhiteSpace(track)) yield break;
        foreach (var entry in Entries)
            if (entry.Value.Happened && RaceImportIdentity.TracksMatch(track, entry.Value.Where))
                yield return entry;
    }
}
