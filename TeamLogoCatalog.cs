namespace CorsaCareer;

/// <summary>
/// Una scuderia con la propria identità visiva.
/// </summary>
public sealed record TeamIdentity(
    string Name,
    /// <summary>Nome del file in assets/team-logos, senza estensione.</summary>
    string Logo,
    /// <summary>Il gradino della scala a cui la squadra appartiene.</summary>
    LadderPath Path,
    /// <summary>Sotto-livello indicativo: serve a non mettere un team GT nel kart.</summary>
    int Step,
    string Motto);

/// <summary>
/// Le scuderie del paddock, con il loro simbolo.
///
/// Prima le squadre erano tre nomi ripetuti in tutto il codice — "Rookie
/// Motorsport", "Kaido Motorsport", "Minato Apex Racing" — senza volto: il
/// mercato mostrava un'illustrazione generica al posto di un'identità. Qui ogni
/// squadra ha un logo proprio, e le squadre appartengono a una disciplina, così
/// un team di kart non compare a offrire un sedile GT.
///
/// I loghi sono ritagli dei fogli originali generati per il progetto e stanno in
/// <c>assets/team-logos</c>. Se un file manca, l'interfaccia continua a
/// funzionare senza immagine: il nome resta.
/// </summary>
public static class TeamLogoCatalog
{
    public static readonly IReadOnlyList<TeamIdentity> Teams =
    [
        // --- kart: il tronco comune
        new("Hoshi Kart Works", "team-hoshi-kart-works", LadderPath.Karting, 1,
            "Officina di paese, motori curati a mano: da qui passano quasi tutti."),
        new("Minato Apex Kart", "team-minato-apex-kart", LadderPath.Karting, 2,
            "Squadra di porto, disciplina da regata: nessun giro buttato."),

        // --- monoposto
        new("Kuroda Junior Racing", "team-kuroda-junior-racing", LadderPath.SingleSeater, 4,
            "Programma giovani serio: chi entra viene misurato ogni settimana."),
        new("Red Maple Junior", "team-red-maple-junior", LadderPath.SingleSeater, 4,
            "Struttura piccola e ambiziosa, vive di piloti che crescono in fretta."),
        new("Silver Crane Formula", "team-silver-crane-formula", LadderPath.SingleSeater, 5,
            "Nome pesante nella categoria nazionale: il sedile si guadagna."),
        new("Orion Night Racing", "team-orion-night-racing", LadderPath.SingleSeater, 6,
            "Lavora sui dati più che sulle sensazioni. Chi non li legge resta fuori."),

        // --- turismo
        new("Seishin Touring Squad", "team-seishin-touring-squad", LadderPath.Touring, 4,
            "Vetture chiuse, gare di contatto: qui si impara a difendere una posizione."),
        new("Raijin Touring", "team-raijin-touring", LadderPath.Touring, 5,
            "Aggressivi in pista e in trattativa. Pagano bene chi porta risultati."),
        new("Takumi Works", "team-takumi-works", LadderPath.Touring, 5,
            "Squadra di meccanici prima che di piloti: la macchina viene sempre prima."),

        // --- durata
        new("Aozora Endurance", "team-aozora-endurance", LadderPath.Endurance, 4,
            "Gare lunghe, equipaggi veri: conta arrivare, non brillare un giro."),
        new("Sakura GT Alliance", "team-sakura-gt-alliance", LadderPath.Endurance, 5,
            "Alleanza di privati con mezzi seri: il posto vale quanto lo si paga."),
        new("Kitanami Prototype", "team-kitanami-prototype", LadderPath.Endurance, 6,
            "Prototipi e notti in pista: il livello più alto che questo paddock offre.")
    ];

    /// <summary>La squadra con questo nome, se è del catalogo.</summary>
    public static TeamIdentity? ByName(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : Teams.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Percorso del logo di una squadra, o stringa vuota se non c'è. Il
    /// chiamante deve gestire l'assenza: i file possono non essere installati.
    /// </summary>
    public static string LogoPath(string? teamName)
    {
        var team = ByName(teamName);
        if (team == null) return "";
        var path = AssetPaths.File("team-logos", team.Logo + ".png");
        return File.Exists(path) ? path : "";
    }

    /// <summary>
    /// Le squadre credibili per un gradino: stessa disciplina, livello vicino.
    /// Un pilota che esce dal kart non deve ricevere offerte da una squadra di
    /// prototipi, e viceversa.
    /// </summary>
    public static IReadOnlyList<TeamIdentity> ForRung(LadderRung rung)
    {
        var same = Teams.Where(x => x.Path == rung.Path && Math.Abs(x.Step - rung.Step) <= 1).ToList();
        if (same.Count > 0) return same;
        // Nessuna corrispondenza esatta: si allarga al gradino, qualunque
        // disciplina, così il mercato non resta mai vuoto.
        var byStep = Teams.Where(x => Math.Abs(x.Step - rung.Step) <= 1).ToList();
        return byStep.Count > 0 ? byStep : Teams;
    }

    /// <summary>
    /// Assegna in modo stabile <paramref name="count"/> squadre a un gradino: la
    /// stessa carriera vede sempre gli stessi nomi, e non compaiono duplicati.
    /// </summary>
    public static IReadOnlyList<TeamIdentity> Pick(LadderRung rung, int count, int seed)
    {
        var pool = ForRung(rung);
        if (pool.Count == 0) return [];
        var ordered = pool
            .OrderBy(x => StableHash($"{seed}|{x.Name}"))
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return ordered.Take(Math.Clamp(count, 1, ordered.Count)).ToList();
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 17;
            foreach (var ch in value) hash = hash * 31 + ch;
            return hash & int.MaxValue;
        }
    }
}
