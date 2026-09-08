namespace CorsaCareer1991;

/// <summary>
/// Un personaggio del racconto. Non possiede risultati o bonus segreti: serve
/// soltanto a far ricordare chi ha reagito a un momento della carriera.
/// </summary>
public sealed class StoryCharacter
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Trait { get; set; } = "";
    public string Relationship { get; set; } = "";
    public int Trust { get; set; } = 40;
    public string LastReaction { get; set; } = "";
    public DateTime LastReactionDate { get; set; }
    public List<string> Memory { get; set; } = [];
}

public static class StoryCastService
{
    public const string Manager = "marta-rinaldi";
    public const string Mechanic = "gianni-valli";
    public const string Rival = "nico-valenti";
    // Figure originali ricorrenti: archetipi autonomi, non copie di personaggi
    // esistenti. Entrano nel paddock quando la carriera si allarga oltre il test.
    public const string Friend = "nori-senda";
    public const string Strategist = "mina-arisawa";
    public const string Engineer = "shigeo-kanda";
    public const string Coordinator = "minato-nakahara";
    public const string Journalist = "noa-minazuki";

    public static void Ensure(CareerState career)
    {
        career.StoryCast ??= [];
        Add(career, Manager, "Rei Kisaragi", "responsabile del programma rookie", "misura le parole, non i decimi", "ti ha concesso un solo primo test");
        Add(career, Mechanic, "Genji Arakawa", "meccanico e mentore", "parla poco, ascolta il motore", "ha preparato la vettura con cui inizi");
        Add(career, Rival, "Riku Hayase", "rookie rivale", "non abbassa mai lo sguardo", "è il primo nome con cui sarai confrontato");
        Add(career, Friend, "Haru Senda", "amico e osservatore dati", "trasforma ogni giro in appunti utili", "è quello che resta quando il paddock si svuota");
        Add(career, Strategist, "Miki Arisawa", "stratega e referente sponsor", "vede il costo nascosto di ogni decisione", "collega risultati, budget e opportunità");
        Add(career, Engineer, "Shigeru Kanda", "ingegnere di pista", "non discute con i dati", "insegna a tradurre il feeling in numeri");
        Add(career, Coordinator, "Minoru Nakahara", "coordinatore del campionato", "severo, ma uguale con tutti", "decide regole, finestre e accesso alle griglie");
        Add(career, Journalist, "Noa Minazuki", "giornalista e fotografa", "fa domande prima di scattare", "racconta ciò che il cronometro non riesce a spiegare");
        foreach (var character in career.StoryCast)
        {
            character.Memory ??= [];
            if (character.Memory.Count == 0)
                Remember(character, character.Relationship, character.Trust, career.StoryDate);
        }
    }

    public static StoryCharacter Get(CareerState career, string id)
    {
        Ensure(career);
        return career.StoryCast.First(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Memoria narrativa persistente: non modifica risultati o motori sportivi.</summary>
    public static void Remember(CareerState career, string id, string memory, int trustDelta = 0)
    {
        Ensure(career);
        var character = Get(career, id);
        Remember(character, memory, Math.Clamp(character.Trust + trustDelta, 0, 100), career.StoryDate);
    }

    private static void Remember(StoryCharacter character, string memory, int trust, DateTime date)
    {
        if (!string.IsNullOrWhiteSpace(memory))
        {
            character.Memory ??= [];
            character.Memory.RemoveAll(x => x.Equals(memory, StringComparison.OrdinalIgnoreCase));
            character.Memory.Insert(0, memory.Trim());
            if (character.Memory.Count > 8) character.Memory.RemoveRange(8, character.Memory.Count - 8);
            character.LastReaction = memory.Trim();
        }
        character.Trust = trust;
        character.LastReactionDate = date;
    }

    private static void Add(CareerState career, string id, string name, string role, string trait, string relationship)
    {
        if (career.StoryCast.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase))) return;
        career.StoryCast.Add(new StoryCharacter { Id = id, Name = name, Role = role, Trait = trait, Relationship = relationship });
    }
}
