namespace CorsaCareer1991;

/// <summary>
/// Le discipline verso cui una carriera puo salire.
///
/// Non esiste una cima sola. Dal kart si puo puntare alla monoposto, alle
/// vetture chiuse da durata o al turismo: dipende da cosa sceglie il pilota e,
/// prima ancora, da quali vetture sono davvero installate in Assetto Corsa. Una carriera che promette la Formula 1 quando fra i contenuti
/// non c'e una sola monoposto sta mentendo.
/// </summary>
public enum LadderPath
{
    /// <summary>Il tronco comune: kart. Da qui si sceglie.</summary>
    Karting,
    /// <summary>Monoposto, fino alla categoria regina disponibile.</summary>
    SingleSeater,
    /// <summary>Vetture chiuse da durata: GT e prototipi.</summary>
    Endurance,
    /// <summary>Turismo e vetture da circuito derivate dalla serie.</summary>
    Touring
}

/// <summary>
/// Un gradino: un livello dentro una disciplina.
/// </summary>
public sealed class LadderRung
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public LadderPath Path { get; init; }
    /// <summary>Altezza assoluta: 1 e il fondo del kart, 7 il vertice di una disciplina.</summary>
    public int Step { get; init; }
    /// <summary>La categoria meccanica corrispondente: decide premi, quote e punteggi.</summary>
    public string Tier { get; init; } = "Rookie";
    public string Description { get; init; } = "";
    public string Company { get; init; } = "";
}

/// <summary>
/// La scala della carriera: dal fondo del kart fino alla vetta che i contenuti
/// installati permettono di raggiungere.
///
/// Il gradino non si deduce dal solo nome della categoria dichiarata nel
/// pacchetto: fra un kart da noleggio a quattro tempi e un 125 con il cambio
/// passa una carriera intera, e la differenza sta nella potenza reale.
/// </summary>
public static class CareerLadder
{
    public const string FourStroke = "kart-4t";
    public const string TwoStroke = "kart-2t";
    public const string Shifter = "kart-125";

    public static readonly IReadOnlyList<LadderRung> Rungs =
    [
        // --- il tronco comune
        new()
        {
            Id = FourStroke, Step = 1, Path = LadderPath.Karting, Tier = "Rookie",
            Name = "Kart a quattro tempi",
            Description = "Il fondo assoluto: motori lenti, piste di paese, iscrizioni da poche centinaia di euro.",
            Company = "Ragazzi al primo casco, padri che spingono il kart a mano, nessuno che guardi."
        },
        new()
        {
            Id = TwoStroke, Step = 2, Path = LadderPath.Karting, Tier = "Rookie",
            Name = "Kart a due tempi",
            Description = "Il primo mezzo che fa davvero paura. Qui si separa chi corre da chi si diverte.",
            Company = "Chi ha deciso che vuole fare questo, e comincia a spendere soldi che non ha."
        },
        new()
        {
            Id = Shifter, Step = 3, Path = LadderPath.Karting, Tier = "Categoria regionale",
            Name = "Kart 125 con cambio",
            Description = "L'ultimo gradino del kart: potenza vera, gomme vere, avversari che hanno già vinto qualcosa.",
            Company = "Piloti che gli osservatori vengono davvero a guardare."
        },

        // --- monoposto
        new()
        {
            Id = "formula-4", Step = 4, Path = LadderPath.SingleSeater, Tier = "Categoria regionale",
            Name = "Formula d'ingresso",
            Description = "La prima monoposto: ali, telaio, e un budget che il kart non chiedeva per una stagione intera.",
            Company = "Chi ha una famiglia che paga, chi ha uno sponsor, e i pochi arrivati vincendo."
        },
        new()
        {
            Id = "formula-3", Step = 5, Path = LadderPath.SingleSeater, Tier = "Categoria avanzata",
            Name = "Formula nazionale",
            Description = "Il livello in cui i risultati cominciano a valere fuori dal proprio Paese.",
            Company = "Piloti sostenuti da programmi giovani, con procuratori veri."
        },
        new()
        {
            Id = "formula-2", Step = 6, Path = LadderPath.SingleSeater, Tier = "Categoria avanzata",
            Name = "Formula internazionale",
            Description = "L'anticamera: da qui si passa, oppure si resta piloti di categoria per sempre.",
            Company = "Venti piloti, e ogni anno due o tre posti veri più in alto."
        },
        new()
        {
            Id = "formula-1", Step = 7, Path = LadderPath.SingleSeater, Tier = "Formula / top tier",
            Name = "Formula 1",
            Description = "La categoria regina delle monoposto.",
            Company = "I venti migliori al mondo."
        },

        // --- vetture chiuse da durata
        new()
        {
            Id = "gt4", Step = 4, Path = LadderPath.Endurance, Tier = "Categoria regionale",
            Name = "GT d'ingresso",
            Description = "Vetture chiuse, gare lunghe, il primo mondo in cui si può guadagnare invece di pagare.",
            Company = "Gentleman driver che pagano, e professionisti pagati per portarli sul podio."
        },
        new()
        {
            Id = "gt3", Step = 5, Path = LadderPath.Endurance, Tier = "Categoria avanzata",
            Name = "GT internazionale",
            Description = "Il livello dove le case costruttrici mettono i propri piloti ufficiali.",
            Company = "Equipaggi misti, strategie di squadra, notti intere."
        },
        new()
        {
            Id = "prototipi", Step = 6, Path = LadderPath.Endurance, Tier = "Categoria avanzata",
            Name = "Prototipi",
            Description = "Vetture costruite solo per correre, senza nessuna parentela con la strada.",
            Company = "Piloti che dalla monoposto sono passati qui, e non tornano indietro."
        },
        new()
        {
            Id = "hypercar", Step = 7, Path = LadderPath.Endurance, Tier = "Formula / top tier",
            Name = "Mondiale endurance",
            Description = "La vetta delle vetture chiuse: le gare che si misurano in ore, non in giri.",
            Company = "Equipaggi ufficiali delle case, i nomi che si leggono a Le Mans."
        },

        // --- turismo
        new()
        {
            Id = "cup", Step = 4, Path = LadderPath.Touring, Tier = "Categoria regionale",
            Name = "Trofeo monomarca",
            Description = "Vetture identiche: qui non ci sono scuse tecniche, conta solo chi guida.",
            Company = "Piloti che si giocano tutto sul decimo, perché il mezzo è lo stesso per tutti."
        },
        new()
        {
            Id = "tcr", Step = 5, Path = LadderPath.Touring, Tier = "Categoria avanzata",
            Name = "Turismo internazionale",
            Description = "Contatto, strategia, gare corte e cattive.",
            Company = "Professionisti del turismo, gente che vive di questo da vent'anni."
        },

    ];

    /// <summary>
    /// Quanti gradini ha la scala delle categorie. Il piu' basso e' 1.
    ///
    /// I tre kart contano come tre gradini distinti e non come uno solo: fra un
    /// quattro tempi da noleggio e un 125 con cambio c'e' piu' differenza che
    /// fra due monoposto vicine, e la gavetta si misura li'.
    /// </summary>
    public const int Steps = 7;

    /// <summary>
    /// L'altezza di categoria detta come la legge il giocatore: «CATEGORIA 3 DI 7».
    /// Sta accanto al livello di campionato, che e' un'altra scala.
    /// </summary>
    public static string LevelLabel(LadderRung rung) =>
        $"CATEGORIA {Math.Clamp(rung.Step, 1, Steps)} DI {Steps}";

    public static LadderRung ById(string? id) =>
        Rungs.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)) ?? Rungs[0];

    /// <summary>
    /// Il gradino di una vettura installata. Conta la potenza, non solo il nome
    /// della categoria: un kart puo essere un quattro tempi da noleggio o un 125
    /// con il cambio, e sono due carriere diverse.
    /// </summary>
    public static LadderRung ForCar(string? category, int powerHp, int massKg)
    {
        var c = (category ?? "").Trim().ToLowerInvariant();

        // 60 cavalli e non 45: un KZ125 da gara ne dichiara fino a 54, e sopra
        // la vecchia soglia usciva dal ramo dei kart per finire nel ripiego,
        // che lo rimandava al gradino dei due tempi. Il gradino del 125 con
        // cambio restava vuoto proprio per le vetture fatte per occuparlo.
        //
        // A tenere stretto il riconoscimento basta la massa: un kart sta sotto
        // i 220 kg, qualunque vettura chiusa ne pesa il triplo.
        if (c.Contains("kart") || (powerHp > 0 && powerHp <= 60 && massKg > 0 && massKg <= 220))
        {
            if (powerHp > 0 && powerHp <= 15) return ById(FourStroke);
            if (powerHp >= 30) return ById(Shifter);
            // Senza potenza dichiarata decide la sigla: "kz", "125" e
            // "shifter" sono i nomi con cui il kart con cambio viene
            // distribuito, e senza questo un KZ finiva un gradino sotto.
            if (powerHp <= 0 && (c.Contains("kz") || c.Contains("125") || c.Contains("shifter")))
                return ById(Shifter);
            return ById(TwoStroke);
        }

        // Il riconoscimento è per contenuto, non per uguaglianza esatta.
        //
        // Le categorie dei pacchetti reali di Assetto Corsa non sono un elenco
        // chiuso: si trovano "GT3", "gt3_cup", "LMP1", "GTE", "Formula 1",
        // "Open Wheeler", "Formula RSS 2000". Con il confronto esatto tutte
        // queste finivano nel ripiego per sola potenza, e una Formula 1 con
        // oltre settecento cavalli veniva classificata hypercar — cioè su
        // un'altra carriera. Qui si cerca la sigla dentro il nome, in ordine
        // di specificità: la voce più precisa vince sulla più generica, così
        // "gt3 cup" resta una GT3 e non un monomarca.
        bool Ha(params string[] chiavi) => chiavi.Any(x => c.Contains(x, StringComparison.Ordinal));

        // --- vertice delle vetture chiuse
        if (Ha("hypercar", "gt1", "group c", "can-am")) return ById("hypercar");
        if (Ha("lmp", "prototype", "prototipo", "sport prototype")) return ById("prototipi");

        // --- GT: gt4 prima di gt3 non serve (sono sigle distinte), ma
        // entrambe devono precedere "cup", che compare nei nomi dei monomarca
        // GT ("Porsche GT3 Cup").
        if (Ha("gt4")) return ById("gt4");
        if (Ha("gt3", "gt2", "gte", "gt500", "gt300")) return ById("gt3");

        // --- monoposto: la sigla se c'è, altrimenti la potenza.
        if (Ha("formula", "open wheel", "monoposto", "single seater", "indy"))
        {
            if (Ha("formula 1", "formula1", "f1", "grand prix")) return ById("formula-1");
            if (Ha("formula 2", "formula2", "f2")) return ById("formula-2");
            if (Ha("formula 3", "formula3", "f3")) return ById("formula-3");
            if (Ha("formula 4", "formula4", "f4", "junior", "vee", "ford")) return ById("formula-4");
            // Una monoposto generica non è una Formula 1: la potenza dice di
            // che gradino si tratta. Tatuus e Formula Vee stanno intorno ai
            // 130 cavalli, una monoposto di vertice ne ha più di seicento —
            // prima finivano tutte insieme nel gradino più alto, saltando
            // metà della carriera e mettendo una Vee in griglia con una RB9.
            return powerHp >= 600 ? ById("formula-1")
                : powerHp >= 380 ? ById("formula-2")
                : powerHp >= 200 ? ById("formula-3")
                : ById("formula-4");
        }

        // --- turismo. Fra le turismo la distanza è enorme: una MX5 da 130
        // cavalli e una NSX GT500 da 500 non stanno sullo stesso gradino.
        if (Ha("tcr", "wtcc", "btcc", "dtm", "super taikyu")) return ById("tcr");
        if (Ha("cup", "trofeo", "monomarca", "one make")) return ById("cup");
        if (Ha("touring", "turismo", "sedan", "saloon"))
            return powerHp >= 300 ? ById("tcr") : ById("cup");

        // --- niente di riconoscibile: decide la potenza, come prima.
        return powerHp >= 700 ? ById("hypercar")
            : powerHp >= 500 ? ById("gt3")
            : powerHp >= 250 ? ById("cup")
            : powerHp >= 150 ? ById("formula-4")
            : ById(TwoStroke);
    }

    public static LadderRung Current(CareerState career, IReadOnlyList<ContentCarRecord> cars)
    {
        var car = cars.FirstOrDefault(x => string.Equals(x.Id, career.Car, StringComparison.OrdinalIgnoreCase));
        if (car != null) return ForCar(car.Category, car.PowerHp, car.MassKg);
        return career.Tier switch
        {
            "Formula / top tier" => ById("formula-1"),
            "Categoria avanzata" => ById("formula-3"),
            "Categoria regionale" => ById("formula-4"),
            _ => ById(FourStroke)
        };
    }

    // --------------------------------------------------- i gradini che esistono

    /// <summary>
    /// I gradini che i contenuti installati riempiono davvero, in ordine.
    ///
    /// La scala ne prevede sette, ma nessuna installazione li ha tutti: in un
    /// catalogo si trova una Formula 3 e poi direttamente una Formula 1, senza
    /// niente in mezzo. Il gradino vuoto non va contato, altrimenti la carriera
    /// si ferma davanti a un buco che non e colpa del pilota.
    /// </summary>
    public static IReadOnlyList<int> PopulatedSteps(IReadOnlyList<ContentCarRecord> cars) =>
        cars.Where(ContentCategoryRules.IsRaceable)
            .Select(x => ForCar(x.Category, x.PowerHp, x.MassKg).Step)
            .Distinct().OrderBy(x => x).ToList();

    /// <summary>
    /// Il primo gradino sopra a quello attuale fra quelli che esistono davvero.
    /// Se sopra non c'e piu niente, restituisce il gradino attuale.
    /// </summary>
    public static int NextPopulatedStep(int currentStep, IReadOnlyList<ContentCarRecord> cars)
    {
        foreach (var s in PopulatedSteps(cars)) if (s > currentStep) return s;
        return currentStep;
    }

    /// <summary>
    /// Se una vettura e a portata: il gradino attuale o il primo pieno sopra.
    /// Un salto solo per volta, ma contato sui gradini veri.
    /// </summary>
    public static bool WithinReach(int step, int currentStep, IReadOnlyList<ContentCarRecord> cars) =>
        step >= currentStep && step <= NextPopulatedStep(currentStep, cars);

    // ------------------------------------------------------------- le vette

    /// <summary>
    /// Le discipline che i contenuti installati permettono davvero di percorrere,
    /// con il gradino piu alto raggiungibile in ciascuna. Il kart non compare: e
    /// il tronco comune, non una destinazione.
    /// </summary>
    public static IReadOnlyList<LadderRung> ReachableSummits(IReadOnlyList<ContentCarRecord> cars)
    {
        var raggiunti = cars
            .Where(ContentCategoryRules.IsRaceable)
            .Select(x => ForCar(x.Category, x.PowerHp, x.MassKg))
            .Where(x => x.Path != LadderPath.Karting)
            .GroupBy(x => x.Path)
            .Select(g => g.OrderByDescending(x => x.Step).First())
            .OrderByDescending(x => x.Step)
            .ToList();
        return raggiunti;
    }

    /// <summary>
    /// La vetta della disciplina che il pilota sta percorrendo, fra quelle che i
    /// contenuti installati rendono possibili. Restituisce null se il pilota e
    /// ancora sul tronco comune o se non c'e nulla di installato oltre il kart.
    /// </summary>
    public static LadderRung? SummitFor(LadderRung current, IReadOnlyList<ContentCarRecord> cars)
    {
        var vette = ReachableSummits(cars);
        if (vette.Count == 0) return null;
        if (current.Path == LadderPath.Karting) return null;
        return vette.FirstOrDefault(x => x.Path == current.Path);
    }

    /// <summary>
    /// Quanto manca alla vetta, detto come lo direbbe un cronista.
    ///
    /// La destinazione non e fissa: dipende dalla disciplina scelta e da cosa e
    /// installato. Finche il pilota e nel kart, le strade sono ancora tutte
    /// aperte, e la frase lo dice invece di promettere una categoria che
    /// potrebbe non esistere fra i contenuti.
    /// </summary>
    public static string DistanceToTop(LadderRung current, IReadOnlyList<ContentCarRecord> cars)
    {
        var vette = ReachableSummits(cars);

        if (current.Path == LadderPath.Karting)
        {
            if (vette.Count == 0) return "Dove porti questa carriera dipende da cosa installerai.";
            var nomi = vette.Take(3).Select(x => x.Name).ToList();
            return nomi.Count == 1
                ? $"Da qui si può arrivare a una cosa sola, con quello che è installato: {nomi[0]}."
                : $"Da qui le strade sono aperte: {string.Join(", ", nomi.Take(nomi.Count - 1))} o {nomi[^1]}.";
        }

        var vetta = vette.FirstOrDefault(x => x.Path == current.Path);
        if (vetta == null || vetta.Step <= current.Step) return "Più in alto di così, con questi contenuti, non si va.";

        var mancanti = vetta.Step - current.Step;
        return mancanti == 1
            ? $"Un gradino a {vetta.Name}. Uno solo, ed è quello che quasi nessuno sale."
            : $"{mancanti} gradini a {vetta.Name}.";
    }

    /// <summary>Nome leggibile di una disciplina.</summary>
    /// <summary>
    /// Le vetture che appartengono alla strada scelta al bivio.
    ///
    /// Il kart resta di tutti: è il tronco comune da cui ogni carriera parte.
    /// Sopra, chi ha scelto le monoposto deve ricevere monoposto — era il
    /// difetto per cui una carriera passava da kart a trofeo turismo, poi a
    /// GT3, poi a formula, cambiando mestiere ogni stagione perché a decidere
    /// era quale vettura risultasse meno potente fra quelle installate.
    ///
    /// Se su quella strada non c'è niente di installato si torna all'elenco
    /// completo: meglio una proposta fuori strada che una carriera ferma.
    /// </summary>
    public static List<ContentCarRecord> OnPath(IEnumerable<ContentCarRecord> cars, string? chosenPath)
    {
        var elenco = cars.ToList();
        if (!Enum.TryParse<LadderPath>(chosenPath, ignoreCase: true, out var strada)) return elenco;
        var proprie = elenco
            .Where(x =>
            {
                var gradino = ForCar(x.Category, x.PowerHp, x.MassKg);
                return gradino.Path == strada || gradino.Path == LadderPath.Karting;
            })
            .ToList();
        return proprie.Count > 0 ? proprie : elenco;
    }

    public static string PathName(LadderPath path) => path switch
    {
        LadderPath.SingleSeater => "monoposto",
        LadderPath.Endurance => "vetture chiuse da durata",
        LadderPath.Touring => "turismo",
        _ => "kart"
    };
}
