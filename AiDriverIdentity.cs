namespace CorsaCareer;

/// <summary>Deterministic fictional paddock identity for an AI driver observed in a real AC result.</summary>
public static class AiDriverIdentity
{
    // La carriera si corre in Giappone: squadre, nazionalità e nomi degli
    // avversari erano rimasti italiani, e in griglia comparivano piloti che
    // non c'entravano con l'ambientazione. Restano tutti inventati: nessuno
    // corrisponde a una persona o a una squadra reale.
    private static readonly string[] Teams =
        ["Hoshi Kart Works", "Minato Apex Racing", "Kaido Motorsport", "Tsubasa Works", "Rinkai Racing Team"];
    private static readonly string[] Nationalities = ["Giappone", "Giappone", "Giappone", "Corea del Sud", "Australia"];

    public static string Team(string driver, string car)
        => Teams[Seed(driver, car) % Teams.Length];

    public static string Nationality(string driver)
        => Nationalities[Seed(driver, "nationality") % Nationalities.Length];

    // Nomi per gli avversari generati (griglia simulata). Sono inventati e non
    // corrispondono a piloti reali: la carriera non deve mettere in bocca
    // risultati a persone esistenti.
    private static readonly string[] FirstNames =
    [
        "Sota", "Haruto", "Ren", "Yuto", "Kaito", "Riku", "Souta", "Daiki",
        "Takumi", "Hinata", "Yuma", "Kenta", "Naoki", "Shota", "Tatsuya", "Ryo",
        "Kazuki", "Hayato", "Itsuki", "Mei"
    ];

    private static readonly string[] LastNames =
    [
        "Kurosawa", "Nakajima", "Tachibana", "Fujimoto", "Sakuraba", "Ishikawa", "Morimoto", "Ogawa",
        "Kirishima", "Yanagida", "Hoshino", "Amemiya", "Serizawa", "Kudo", "Nagase", "Takenaka",
        "Onodera", "Mizuhara", "Shirakawa", "Kanzaki", "Tokugawa", "Fukazawa", "Aoyagi", "Sugimura"
    ];

    /// <summary>
    /// Nome stabile per un avversario generato. A parita di seme e posizione
    /// restituisce lo stesso nome, cosi la griglia di una gara simulata non
    /// cambia identita se la si ricostruisce.
    /// </summary>
    public static string NameFor(long seed, int slot)
    {
        // Nome e cognome vanno mescolati separatamente. Ricavando il cognome da
        // una divisione dello stesso valore si perdeva entropia: la griglia
        // usciva piena di "Luca" e "Emanuele".
        var first = FirstNames[(int)(Mix(seed, slot, 0x9E37) % (uint)FirstNames.Length)];
        var last = LastNames[(int)(Mix(seed, slot, 0x85EB) % (uint)LastNames.Length)];
        return $"{first} {last}";
    }

    private static uint Mix(long seed, int slot, uint salt)
    {
        unchecked
        {
            var h = 2166136261u ^ salt;
            var value = (ulong)seed + (ulong)(uint)slot * 2654435761UL;
            for (var i = 0; i < 8; i++)
            {
                h ^= (uint)(value & 0xFF);
                h *= 16777619u;
                value >>= 8;
            }
            return h;
        }
    }

    public static string NormalizeName(string name)
        => string.Join(' ', (name ?? "").Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static bool NamesEqual(string left, string right)
        => string.Equals(NormalizeName(left), NormalizeName(right), StringComparison.OrdinalIgnoreCase);

    private static int Seed(string first, string second)
    {
        var hash = Math.Abs(first.Aggregate(17L, (value, character) => value * 31 + character) + second.Aggregate(7L, (value, character) => value * 29 + character));
        return (int)(hash % 100000);
    }
}
