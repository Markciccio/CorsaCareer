namespace CorsaCareer;

/// <summary>
/// Provenienza di un risultato. Fino a qui esisteva una sola strada: il referto
/// di Assetto Corsa. Il risultato simulato serve a provare la carriera senza
/// aprire il simulatore, e proprio per questo deve restare <em>riconoscibile</em>:
/// una carriera giocata davvero non deve poter essere confusa con una di prova.
/// </summary>
public static class ResultProvenance
{
    public const string AssettoCorsa = "ASSETTO_CORSA_RACE_OUT";
    public const string AssettoCorsaInvitation = "ASSETTO_CORSA_INVITATION";
    public const string Withdrawal = "CORSACAREER_WITHDRAWAL";
    public const string Simulated = "CORSACAREER_SIMULATED";

    /// <summary>Vero se il risultato viene da una sessione realmente disputata.</summary>
    public static bool IsReal(string? sourceKind) =>
        !string.Equals(sourceKind, Simulated, StringComparison.OrdinalIgnoreCase);

    public static string Describe(string? sourceKind) => sourceKind switch
    {
        Simulated => "Risultato simulato dal programma: nessuna sessione disputata.",
        Withdrawal => "Sessione preparata e non conclusa: ritiro registrato.",
        AssettoCorsaInvitation => "Risultato reale importato da Assetto Corsa (gara su invito).",
        _ => "Risultato reale importato da Assetto Corsa."
    };

    /// <summary>Etichetta breve per le schermate.</summary>
    public static string ShortLabel(string? sourceKind) =>
        string.Equals(sourceKind, Simulated, StringComparison.OrdinalIgnoreCase) ? "SIMULATO" : "REALE";
}

/// <summary>
/// Un pilota della griglia simulata, con il proprio passo.
/// </summary>
public sealed record SimulatedDriver(string Name, string Car, double Pace, bool IsPlayer);

/// <summary>
/// Vincolo sull'esito di una sessione simulata. Serve a provare la catena delle
/// conseguenze — reputazione, denaro, promesse, fasi — senza dover ripetere la
/// simulazione finche il caso produce il risultato che si vuole verificare.
///
/// Non impone una posizione: sposta il passo del pilota, e la classifica si
/// ordina di conseguenza. Un esito positivo resta una gara plausibile, non un
/// primo posto scritto a mano.
/// </summary>
public enum SimulationBias
{
    /// <summary>Nessun vincolo: l'esito dipende solo dallo stato della carriera.</summary>
    Natural,
    /// <summary>Giornata buona: passo nettamente sopra il livello del gruppo.</summary>
    Positive,
    /// <summary>Giornata da dimenticare: passo sotto il gruppo, ritiro possibile.</summary>
    Negative
}

/// <summary>
/// Simulazione di una sessione, per provare la carriera senza aprire Assetto
/// Corsa.
///
/// Non e un generatore casuale di piazzamenti: costruisce una griglia con un
/// passo per pilota derivato dal livello IA della sessione, assegna al giocatore
/// un passo derivato dal suo stato reale in carriera (fiducia dei team,
/// prestigio, stanchezza, competitivita della vettura) e poi <em>ordina</em>. Il
/// risultato e quindi una conseguenza dello stato, come quello vero, e non un
/// numero estratto.
///
/// E deterministico: a parita di stato e di seme produce lo stesso esito, cosi
/// una carriera di prova e riproducibile e verificabile.
/// </summary>
public static class RaceSimulator
{
    /// <summary>
    /// Quanto pesa il caso, <em>negli stessi punti</em> della scala del passo (la
    /// stessa del livello IA). Deve stare sull'ordine di grandezza della
    /// distanza fra due piloti vicini: con un valore troppo piccolo l'ordine
    /// d'arrivo coincide sempre con la griglia e la gara non esiste, con uno
    /// troppo grande diventa una lotteria dove lo stato del pilota non conta.
    /// </summary>
    public const double LuckSpread = 2.2;

    /// <summary>Probabilita di ritiro in gara, prima delle correzioni per stanchezza e affidabilita.</summary>
    public const double BaseRetirementRisk = 0.035;

    /// <summary>
    /// Quanto vale un punto di passo in frazione di tempo sul giro. E lo stesso
    /// fattore usato per il miglior giro: distacco e cronometro devono
    /// raccontare la stessa gara.
    /// </summary>
    public const double PaceToLapFraction = 0.0035;

    /// <summary>
    /// Di quanto un esito forzato sposta il passo del pilota. Va oltre la
    /// dispersione del gruppo, altrimenti il vincolo non si vedrebbe.
    /// </summary>
    public const double BiasShift = 14.0;

    public sealed class Input
    {
        public string Track { get; set; } = "";
        public string Car { get; set; } = "";
        public string PlayerName { get; set; } = "Marco Ruga";
        public string TeammateName { get; set; } = "";
        public int FieldSize { get; set; } = 12;
        public int Laps { get; set; } = 10;
        /// <summary>Livello IA della sessione, come scritto nel preset.</summary>
        public double AiLevel { get; set; } = 88;
        /// <summary>Riferimento di giro sul tracciato, in millisecondi.</summary>
        public int ReferenceLapMilliseconds { get; set; } = 100000;
        /// <summary>Competitivita della vettura rispetto al gruppo, 0-100.</summary>
        public int CarCompetitiveness { get; set; } = 50;
        /// <summary>Stato del pilota: sono i valori reali della carriera.</summary>
        public int TeamTrust { get; set; } = 30;
        public int SportingPrestige { get; set; } = 20;
        public int Professionalism { get; set; } = 50;
        public int Fatigue { get; set; }
        /// <summary>Forma fisica del pilota, 0-100.</summary>
        public int Fitness { get; set; } = 50;

        /// <summary>
        /// Gare già corse in questa categoria. È l'esperienza che conta
        /// davvero: quella totale premiava un veterano del kart come se
        /// conoscesse una monoposto.
        /// </summary>
        public int RacesInCategory { get; set; }

        /// <summary>
        /// Altezza del campionato (1..5). Più in alto si corre, meno pesano i
        /// crediti accumulati in basso e più forte è il gruppo.
        /// </summary>
        public int ChampionshipLevel { get; set; } = 1;

        /// <summary>Le doti innate del pilota: cosa sa fare, al netto di tutto il resto.</summary>
        public DriverTalent Talent { get; set; } = new();

        /// <summary>Vero se la sessione si corre sul bagnato: fa contare la dote di pioggia.</summary>
        public bool Wet { get; set; }
        public int RacesCompleted { get; set; }
        public bool IsTest { get; set; }
        /// <summary>Seme: rende l'esito riproducibile.</summary>
        public long Seed { get; set; }

        /// <summary>
        /// Seme del gruppo: identifica i piloti del campionato che si sta
        /// correndo, e resta lo stesso per tutta la stagione.
        ///
        /// Prima gli avversari nascevano dal seme della singola gara: cambiavano
        /// nome a ogni round, quindi nessuno di loro accumulava punti e la
        /// classifica finale era «il giocatore primo su settanta piloti con una
        /// gara ciascuno». Il titolo si vinceva per costruzione, senza avversari.
        /// Un campionato ha invece sempre gli stessi venti, li si impara a
        /// conoscere, e qualcuno di loro è più forte.
        /// </summary>
        public long RosterSeed { get; set; }
        /// <summary>Vincolo sull'esito, per provare la catena delle conseguenze.</summary>
        public SimulationBias Bias { get; set; } = SimulationBias.Natural;
    }

    /// <summary>
    /// Abilita del giocatore sulla stessa scala del livello IA. Cresce con lo
    /// stato reale della carriera: un pilota di cui i team si fidano e che ha
    /// gare nelle gambe va piu forte, uno stanco va piu piano.
    /// </summary>
    public static double PlayerSkill(Input input)
    {
        // La base parte sotto il livello IA tipico: all'inizio si perde.
        var skill = 74.0;
        // Fiducia e prestigio valgono meno man mano che si sale.
        //
        // Erano bonus assoluti: a fine carriera davano +24 fissi, mentre il
        // campo cresceva in tutto di diciotto punti dal kart al vertice. Il
        // risultato era una difficoltà rovesciata — zero vittorie in kart e
        // ventidue su ventotto in Formula 1. Quello che hai costruito in una
        // categoria non si trasferisce intero in quella sopra: in mezzo a
        // gente più forte una buona reputazione conta, ma pesa meno.
        var attrito = 1.0 - Math.Min(0.32, Math.Max(0, input.ChampionshipLevel - 1) * 0.08);
        skill += input.TeamTrust * 0.05 * attrito;
        skill += input.SportingPrestige * 0.03 * attrito;

        // La maturazione: quanto il pilota è cresciuto da quando ha cominciato.
        //
        // È la parte più importante della salita, e prima non esisteva. Il
        // pilota migliorava quasi soltanto attraverso fiducia e prestigio —
        // che però sono un EFFETTO dei risultati — mentre il gruppo diventava
        // più forte di quattro punti a ogni gradino. Chi non trovava subito un
        // risultato restava indietro per sempre: al banco due piloti su sei
        // hanno chiuso novanta gare senza un podio, sempre penultimi. Le gare
        // nelle gambe invece si accumulano comunque, e sono ciò che un pilota
        // guadagna soltanto correndo.
        //
        // La radice quadrata dà la forma giusta: si impara molto nelle prime
        // stagioni e sempre meno dopo. Alle sessanta gare — la gavetta intera
        // dal kart al vertice — vale dodici punti, cioè quasi tutto il
        // dislivello della scala.
        skill += Math.Min(14.0, Math.Sqrt(Math.Max(0, input.RacesCompleted)) * 1.5);
        // L'esperienza è soprattutto quella maturata NELLA categoria in cui si
        // corre: chi sale è quasi un esordiente rispetto a chi ci vive da anni.
        // Ma non riparte da zero — saper correre resta — e senza questa quota
        // di travaso la carriera diventava impossibile: nel collaudo,
        // settantatré gare senza una vittoria.
        // L'adattamento decide quanto in fretta si impara la macchina nuova:
        // chi ce l'ha alta è a suo agio quasi subito, chi ce l'ha bassa ha
        // bisogno di una stagione intera.
        // Il travaso dalla carriera intera è passato alla maturazione qui
        // sopra: questa quota è solo l'ambientamento alla vettura nuova, e si
        // azzera a ogni cambio di categoria. Cinque punti, non dodici — con
        // dodici, chi restava a lungo in una categoria diventava imbattibile
        // al suo interno e poi crollava salendo.
        var passoAdattamento = 0.3 + input.Talent.Adaptability / 100.0 * 0.45;
        skill += Math.Min(5.0, input.RacesInCategory * passoAdattamento);

        // Quello che si è costruito non rende imbattibili.
        //
        // Fiducia, prestigio, gare nelle gambe e ambientamento si sommavano
        // senza tetto: un pilota con cento gare e forma 100 arrivava a un passo
        // di quindici punti sopra il gruppo, e da lì vinceva tutto. Al banco di
        // prova erano cinquantasei vittorie su novantasei gare, con sei
        // stagioni chiuse tutte al primo posto — una carriera in cui la
        // vittoria non è più una notizia.
        //
        // Il mestiere accumulato porta al massimo un paio di punti sopra il
        // livello del gruppo in cui si corre: abbastanza per essere fra i
        // candidati ogni domenica, non abbastanza per vincere senza che
        // servano una buona macchina, la forma giusta e la giornata giusta —
        // che sono le voci aggiunte qui sotto. E salendo di categoria il
        // riferimento sale con il gruppo, quindi ogni gradino ricomincia.
        skill = Math.Min(skill, input.AiLevel + 2.0);

        // Le doti innate: è ciò che distingue un pilota da un altro a parità
        // di squadra, macchina e stagione.
        skill += (input.Talent.RawPace - 50) * 0.12;                 // ±5,4
        if (input.Wet) skill += (input.Talent.WetSkill - 50) * 0.10; // ±4,5 solo con pioggia
        skill += (input.CarCompetitiveness - 50) * 0.06;   // vettura, +/-3
        // Forma e stanchezza insieme, con la stessa formula usata dalla
        // giornata del pilota: allenarsi serve, e arrivare consumati costa.
        skill += DriverDay.PaceShift(input.Fitness, input.Fatigue);
        skill -= (60 - Math.Min(60, input.Professionalism)) * 0.05;

        // Il vincolo si applica sul passo, non sulla posizione: lo scarto e
        // abbastanza ampio da uscire dalla dispersione del gruppo (circa +/-7
        // punti) senza rendere il risultato impossibile.
        skill += input.Bias switch
        {
            SimulationBias.Positive => BiasShift,
            SimulationBias.Negative => -BiasShift,
            _ => 0
        };
        // Nemmeno il pilota piu' dotato esce dal gruppo.
        //
        // Il tetto sul mestiere accumulato veniva applicato prima di doti,
        // forma e vettura, che poi aggiungevano fino a quattordici punti: un
        // pilota con RawPace alto e forma perfetta tornava a vincere il
        // sessanta per cento delle gare e otto campionati su otto. La
        // dispersione degli avversari arriva a sette punti sopra il livello
        // medio: restare sotto quella soglia significa essere il candidato
        // numero uno ogni domenica, non il vincitore annunciato — davanti c'e'
        // sempre qualcuno che nella giornata giusta va piu' forte.
        skill = Math.Min(skill, input.AiLevel + 6.0);
        return Math.Clamp(skill, 40, 118);
    }

    public static ImportedRaceResult Simulate(Input input)
    {
        var fieldSize = Math.Max(2, input.FieldSize);
        var laps = Math.Max(1, input.Laps);
        var reference = Math.Max(20000, input.ReferenceLapMilliseconds);

        var playerSkill = PlayerSkill(input);
        var drivers = BuildField(input, fieldSize, playerSkill);

        // Qualifica: stesso passo, caso diverso. Serve a far esistere una
        // posizione di partenza coerente ma non identica all'arrivo.
        var grid = drivers
            .Select((d, i) => (Driver: d, Pace: d.Pace + Jitter(input.Seed, 7000 + i) * LuckSpread * 0.7))
            .OrderByDescending(x => x.Pace)
            .Select(x => x.Driver)
            .ToList();
        var startingPosition = grid.FindIndex(x => x.IsPlayer) + 1;

        if (input.IsTest)
        {
            // Una prova non ha classifica: ha un giro. E la stessa regola del
            // referto reale, dove una sessione di prova non assegna posizioni.
            var testLap = LapFor(reference, playerSkill, input.AiLevel, Jitter(input.Seed, 991));
            return new ImportedRaceResult
            {
                Track = input.Track, Car = input.Car, PlayerName = input.PlayerName,
                SessionName = "Prova", Position = 0, StartingPosition = 0, QualificationPosition = 0,
                Laps = Math.Max(3, laps), BestLapMilliseconds = testLap,
                Classification = []
            };
        }

        // Gara: il passo di qualifica piu il caso della distanza. Piu giri, meno
        // conta il singolo episodio.
        var raceSpread = LuckSpread * (1.0 + 6.0 / (6.0 + laps));
        var order = drivers
            .Select((d, i) => (Driver: d, Pace: d.Pace + Jitter(input.Seed, 13000 + i) * raceSpread))
            .OrderByDescending(x => x.Pace)
            .ToList();

        var retired = RetirementIndex(input, order.Count, playerSkill, order.FindIndex(x => x.Driver.IsPlayer));
        var classification = new List<ImportedDriverResult>();
        var position = 0;
        foreach (var (driver, _) in order)
        {
            position++;
            classification.Add(new ImportedDriverResult
            {
                Name = driver.Name, Car = driver.Car, Position = position, IsPlayer = driver.IsPlayer
            });
        }

        var playerIndex = classification.FindIndex(x => x.IsPlayer);
        var playerPosition = playerIndex + 1;
        var playerRetired = retired >= 0 && playerIndex == retired;

        var winnerPace = order[0].Pace;
        var playerPace = order[playerIndex].Pace;
        var bestLap = LapFor(reference, playerSkill, input.AiLevel, Jitter(input.Seed, 17));
        // Il distacco nasce dalla differenza di passo sui giri percorsi: e una
        // conseguenza, non un numero deciso a parte. Il fattore e lo stesso che
        // converte i punti di passo in tempo sul giro, altrimenti distacco e
        // miglior giro racconterebbero due gare diverse.
        var gap = playerPosition == 1
            ? 0
            : (int)Math.Round(Math.Max(0.0, winnerPace - playerPace) * PaceToLapFraction * reference * laps);

        return new ImportedRaceResult
        {
            Track = input.Track,
            Car = input.Car,
            PlayerName = input.PlayerName,
            SessionName = "Gara",
            Position = playerRetired ? 0 : playerPosition,
            StartingPosition = startingPosition,
            QualificationPosition = startingPosition,
            Laps = playerRetired ? Math.Max(1, laps / 2) : laps,
            BestLapMilliseconds = bestLap,
            GapMilliseconds = playerRetired ? 0 : Math.Max(0, gap),
            PitStops = laps >= 18 ? 1 : 0,
            PenaltySeconds = Penalty(input.Seed, input.Professionalism),
            Damage = playerRetired ? 0.35 : 0,
            Dnf = playerRetired,
            Classification = classification
        };
    }

    // -------------------------------------------------------------- la griglia

    private static List<SimulatedDriver> BuildField(Input input, int fieldSize, double playerSkill)
    {
        var drivers = new List<SimulatedDriver>(fieldSize)
        {
            new(input.PlayerName, input.Car, playerSkill, true)
        };

        // Gli avversari si distribuiscono attorno al livello IA: alcuni piu
        // forti, la maggioranza vicini, alcuni piu lenti. Senza questa
        // dispersione ogni gara finirebbe con lo stesso ordine.
        var slots = fieldSize - 1;
        var roster = input.RosterSeed != 0 ? input.RosterSeed : input.Seed;
        var teammatePlaced = string.IsNullOrWhiteSpace(input.TeammateName);
        for (var i = 0; i < slots; i++)
        {
            var spread = slots <= 1 ? 0.0 : (i / (double)(slots - 1) - 0.5) * 2.0; // -1 .. +1
            // Il valore del pilota viene dal gruppo — è suo, e resta — mentre
            // la giornata cambia da un weekend all'altro. Prima erano la stessa
            // cosa: nessun avversario aveva un'identità sportiva.
            var pace = input.AiLevel + spread * 7.0
                       + Jitter(roster, 3000 + i) * 2.4
                       + Jitter(input.Seed, 5000 + i) * 1.2;
            string name;
            if (!teammatePlaced && i == slots / 2)
            {
                name = input.TeammateName;
                teammatePlaced = true;
            }
            else
            {
                // Il compagno di squadra e il giocatore sono gia in griglia: un
                // nome generato non deve collidere con loro ne con un altro
                // avversario, altrimenti la classifica ha due piloti uguali.
                name = AiDriverIdentity.NameFor(roster + i * 31, i);
                for (var retry = 1; retry <= 24 && Taken(drivers, input, name); retry++)
                    name = AiDriverIdentity.NameFor(roster + i * 31 + retry * 7717, i + retry);
                if (Taken(drivers, input, name)) name = $"{name} jr";
            }
            drivers.Add(new SimulatedDriver(name, input.Car, pace, false));
        }
        return drivers;
    }

    private static bool Taken(List<SimulatedDriver> drivers, Input input, string name) =>
        drivers.Any(x => AiDriverIdentity.NamesEqual(x.Name, name))
        || AiDriverIdentity.NamesEqual(input.TeammateName, name)
        || AiDriverIdentity.NamesEqual(input.PlayerName, name);

    /// <summary>
    /// Indice del pilota che si ritira, oppure -1. La stanchezza e la scarsa
    /// affidabilita aumentano il rischio: e l'unico punto in cui lo stato del
    /// pilota puo produrre un abbandono.
    /// </summary>
    private static int RetirementIndex(Input input, int fieldSize, double playerSkill, int playerIndex)
    {
        // L'affidabilità è una dote del pilota quanto la velocità: c'è chi
        // porta a casa la macchina e chi la rompe. Con cinquanta il rischio
        // resta quello di sempre; sotto, i ritiri diventano il tratto
        // distintivo di una carriera intera.
        var fragilita = (50 - input.Talent.Reliability) * 0.0016;
        var risk = BaseRetirementRisk
                   + input.Fatigue * 0.0011
                   + Math.Max(0, 60 - input.Professionalism) * 0.0009
                   + Math.Max(-0.03, fragilita);
        // Una giornata forzatamente negativa puo finire con un ritiro; una
        // positiva non deve.
        risk = input.Bias switch
        {
            SimulationBias.Positive => 0,
            SimulationBias.Negative => risk + 0.18,
            _ => risk
        };
        // Il ritiro del pilota ha una prova sua.
        //
        // Prima si sorteggiava se qualcuno si ritirava e poi CHI: con dodici in
        // pista il pilota era coinvolto una volta su dodici, cioè lo 0,3% a
        // gara. Nel banco significava zero ritiri in quattrocentotrentotto
        // gare, e un'affidabilità che non contava niente. Adesso la fragilità
        // del pilota decide la sua gara, non quella del gruppo.
        var rischioPilota = Math.Clamp(risk, 0, 0.35);
        if (Unit(input.Seed, 5549) < rischioPilota) return playerIndex;

        // Il resto del gruppo continua ad avere i suoi guasti, indipendenti.
        var roll = Unit(input.Seed, 5551);
        if (roll >= Math.Clamp(BaseRetirementRisk, 0, 0.2)) return -1;
        var who = (int)(Unit(input.Seed, 5557) * fieldSize);
        var scelto = Math.Clamp(who, 0, fieldSize - 1);
        // Il pilota è già stato valutato sopra: qui si ritira solo un altro.
        return scelto == playerIndex ? -1 : scelto;
    }

    private static int LapFor(int reference, double playerSkill, double aiLevel, double jitter)
    {
        // Piu abilita, giro piu vicino al riferimento. Il riferimento e il passo
        // di un pilota al livello IA della sessione.
        var delta = (aiLevel - playerSkill) * PaceToLapFraction + jitter * 0.004;
        return (int)Math.Round(reference * Math.Clamp(1.0 + delta, 0.94, 1.16));
    }

    private static double Penalty(long seed, int professionalism)
    {
        var roll = Unit(seed, 7717);
        var chance = 0.08 + Math.Max(0, 60 - professionalism) * 0.002;
        return roll < chance ? Math.Round(2 + Unit(seed, 7727) * 8, 1) : 0;
    }

    // ------------------------------------------------------------ determinismo

    /// <summary>
    /// Valore in [0,1) stabile per (seme, canale). Non usa Random: la stessa
    /// carriera simulata due volte deve dare lo stesso esito, altrimenti non e
    /// verificabile.
    /// </summary>
    internal static double Unit(long seed, int channel)
    {
        unchecked
        {
            var h = 2166136261u;
            var value = (ulong)seed ^ ((ulong)channel << 32);
            for (var i = 0; i < 8; i++)
            {
                h ^= (uint)(value & 0xFF);
                h *= 16777619u;
                value >>= 8;
            }
            return (h % 1000000u) / 1000000.0;
        }
    }

    /// <summary>Valore in [-1,1).</summary>
    internal static double Jitter(long seed, int channel) => Unit(seed, channel) * 2.0 - 1.0;

    /// <summary>Seme stabile per un round: la stessa gara non cambia esito se la si riapre.</summary>
    public static long SeedFor(string driver, int season, int round, string track, int attempt) =>
        Hash($"{driver}|{season}|{round}|{track}|{attempt}");

    private static long Hash(string text)
    {
        unchecked
        {
            var h = 1469598103934665603UL;
            foreach (var c in text)
            {
                h ^= c;
                h *= 1099511628211UL;
            }
            return (long)(h & 0x7FFFFFFFFFFFFFFF);
        }
    }
}
