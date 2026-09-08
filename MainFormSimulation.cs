using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Simulazione del risultato: permette di far avanzare la carriera senza aprire
/// Assetto Corsa.
///
/// Passa per lo <em>stesso</em> percorso del referto reale — stesso oggetto
/// risultato, stessa applicazione delle conseguenze, stesso giornale — cosi non
/// esiste una seconda catena da tenere allineata. L'unica differenza e la
/// provenienza, che viene scritta nello storico e dichiarata nel portale: una
/// carriera di prova deve restare distinguibile da una giocata davvero.
/// </summary>
public sealed partial class MainForm
{
    /// <summary>
    /// Provenienza del risultato che si sta archiviando. Vale per il singolo
    /// inserimento e torna subito al referto reale.
    /// </summary>
    private string pendingSourceKind = ResultProvenance.AssettoCorsa;

    private void SimulateResultNow(SimulationBias bias = SimulationBias.Natural)
    {
        if (!awaitingResult)
        {
            CareerMessages.Show(null, 
                "Non c'è una sessione preparata. Prepara prima un weekend o un test: la simulazione sostituisce la guida, non la programmazione della sessione.",
                "CorsaCareer — simula il risultato", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var isTest = pendingMode.Equals("test", StringComparison.OrdinalIgnoreCase);
        var plan = CurrentSessionPlan(isTest ? "test" : pendingMode);
        var track = career.Round < rounds.Count ? rounds[career.Round].Track : plan?.Track ?? "";

        var conferma = CareerMessages.Ask(null, 
            BuildConfirmationText(isTest, track, plan, bias),
            "CorsaCareer — simula il risultato",
            MessageBoxButtons.YesNo, DialogResult.Yes);
        if (conferma != DialogResult.Yes) return;

        var imported = ExecuteSimulatedResult(bias, out var esito);
        if (imported == null) return;

        // L'esito si legge nella schermata, non in una finestra da chiudere: si
        // avvisa solo se c'è davvero qualcosa che il portale non mostra già.
        if (!string.IsNullOrWhiteSpace(esito))
            CareerMessages.Show(null, esito, isTest ? "CorsaCareer — test simulato" : "CorsaCareer — gara simulata",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Il cuore della simulazione, separato dalla conferma a schermo: la
    /// conferma è per chi gioca, questo metodo è per chi vuole richiamare la
    /// stessa identica logica di produzione — ad esempio un banco di debug —
    /// senza dover cliccare una finestra modale.
    /// </summary>
    private ImportedRaceResult? ExecuteSimulatedResult(SimulationBias bias, out string outcomeMessage)
    {
        outcomeMessage = "";
        if (!awaitingResult) return null;

        var isTest = pendingMode.Equals("test", StringComparison.OrdinalIgnoreCase);
        var plan = CurrentSessionPlan(isTest ? "test" : pendingMode);
        var track = career.Round < rounds.Count ? rounds[career.Round].Track : plan?.Track ?? "";
        var car = string.IsNullOrWhiteSpace(career.Car) ? plan?.Car ?? "" : career.Car;
        var imported = BuildSimulatedResult(isTest, track, car, plan, bias);

        pendingSourceKind = ResultProvenance.Simulated;
        try
        {
            career.SimulatedSessions++;
            if (isTest)
            {
                CompletePendingWeekend();
                RecordTest(imported, "", "");
                CareerLog.Info("simulazione", $"test simulato: {track} · miglior giro {imported.BestLapMilliseconds} ms");
                // Nessun avviso: che il risultato sia simulato lo ha appena
                // deciso il giocatore, ed e' dichiarato nella testata e nel
                // diario. Fermare il gioco per ripeterlo e' solo un ostacolo.
                outcomeMessage = "";
            }
            else if (pendingMode.Equals("invitation", StringComparison.OrdinalIgnoreCase))
            {
                RecordInvitation(imported, "", "");
                CompletePendingWeekend();
                outcomeMessage = "";
            }
            else
            {
                ApplyClassification(imported.Classification, imported.Track, imported.QualificationPosition);
                Record(imported, "", "");
                CompletePendingWeekend();
                CareerLog.Info("simulazione", $"gara simulata: {track} · P{imported.Position}");
                outcomeMessage = "";
            }
        }
        finally
        {
            // Torna subito al referto reale: la provenienza simulata non deve
            // sopravvivere all'inserimento che l'ha richiesta.
            pendingSourceKind = ResultProvenance.AssettoCorsa;
        }

        SaveCareer();
        // Il banco UI automatico ridisegna esplicitamente dopo ogni passo;
        // evitare il refresh sincrono qui impedisce che un timer/Activated
        // apra finestre modali mentre la riflessione sta avanzando.
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") RefreshUi();
        return imported;
    }

    /// <summary>
    /// Risolve la sessione appena preparata quando Assetto Corsa non e
    /// installato su questo computer.
    ///
    /// Non chiede niente e non mostra avvisi: il pilota ha premuto il pulsante
    /// rosso e la carriera deve andare avanti. Prima, senza Content Manager, il
    /// programma si fermava dicendo "sessione preparata ma CM non trovato" e
    /// serviva un secondo pulsante di debug per sbloccarla.
    ///
    /// Passa dalla stessa catena del referto reale — stesso risultato, stesse
    /// conseguenze, stesso giornale — e la provenienza resta dichiarata come
    /// simulata nello storico, nel calendario e nel portale: una carriera
    /// giocata davvero non deve poter essere confusa con una di prova.
    /// </summary>
    private void ResolvePendingSessionWithoutAssettoCorsa()
    {
        if (!awaitingResult) return;
        var imported = ExecuteSimulatedResult(SimulationBias.Natural, out _);
        if (imported == null)
        {
            // Se la risoluzione non produce niente la sessione resta aperta, ma
            // il motivo deve essere scritto: altrimenti il pulsante rosso non
            // fa nulla e non si capisce perche.
            CareerLog.Warn("sessione", "la sessione non ha potuto essere risolta dal programma.");
            return;
        }
        RefreshUi();
        if (saveStatus != null)
            saveStatus.Text = "Sessione risolta dal programma · Assetto Corsa non installato su questo computer.";
    }

    /// <summary>
    /// Livello del gruppo quando non c'è un piano già preparato: stessa curva
    /// del preset, così simulazione e gara vera non raccontano due difficoltà
    /// diverse.
    /// </summary>
    private double StimaLivelloIa(string carId)
    {
        var record = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(carId, StringComparison.OrdinalIgnoreCase));
        var perGradino = record == null
            ? 0
            : SessionPlanner.AiLevelForStep(CareerLadder.ForCar(record.Category, record.PowerHp, record.MassKg).Step);
        var baseLevel = perGradino > 0 ? perGradino : SessionPlanner.DefaultAiLevel(career.Tier);
        // Stesso tetto del preset: il livello del campionato aggiunge poco a
        // una categoria d'ingresso, altrimenti una Formula Vee finiva a 96.
        return Math.Min(baseLevel + 2.0, baseLevel + SessionPlanner.LevelBonus(career.ChampionshipLevel));
    }

    private string BuildConfirmationText(bool isTest, string track, SessionPlanRecord? plan, SimulationBias bias)
    {
        var cosa = isTest ? "Il test" : "La gara";
        var righe = new List<string>
        {
            $"{cosa} di {NarrativeEngine.Capitalize(track)} verrà risolta dal programma, senza guidare.",
            "",
            bias switch
            {
                SimulationBias.Positive => "ESITO FORZATO: giornata buona. Serve a provare la catena delle conseguenze.",
                SimulationBias.Negative => "ESITO FORZATO: giornata negativa, con possibilità di ritiro.",
                _ => "Il risultato dipende dallo stato reale della carriera — fiducia dei team, prestigio, esperienza, competitività della vettura, stanchezza."
            },
            "",
            "Verrà però archiviato come SIMULATO:",
            "  · la voce nello storico e nella timeline lo dichiara;",
            "  · il portale segnala che la carriera contiene risultati simulati.",
            ""
        };
        if (plan != null)
        {
            righe.Add($"SESSIONE PREPARATA: {plan.FormatLabel} · IA {plan.AiLevel:0}% · {plan.WeatherLabel}");
            righe.Add($"AUTO: {(string.IsNullOrWhiteSpace(career.Car) ? plan.Car : career.Car)} · PISTA: {NarrativeEngine.Capitalize(track)}");
            righe.Add($"DISTANZA: {Math.Max(1, plan.RaceLaps)} giri · TEMPO OBIETTIVO: {(career.EvaluationTargetMilliseconds > 0 ? FormatLap(career.EvaluationTargetMilliseconds) : "calcolato dal contenuto installato")}");
        }
        else
        {
            righe.Add($"AUTO: {UiText.Car(career.Car)} · PISTA: {NarrativeEngine.Capitalize(track)}");
            righe.Add($"TEMPO OBIETTIVO: {(career.EvaluationTargetMilliseconds > 0 ? FormatLap(career.EvaluationTargetMilliseconds) : "calcolato dal contenuto installato")}");
        }
        righe.Add("La simulazione usa questi dati e passa dalla stessa catena del referto reale, ma la fonte resterà DEBUG · SIMULATO.");
        righe.Add("");
        righe.Add("Procedere?");
        return string.Join("\n", righe);
    }

    /// <summary>
    /// Traduce lo stato reale della carriera negli ingressi del simulatore. Non
    /// aggiunge niente che non sia gia nella carriera.
    /// </summary>
    private ImportedRaceResult BuildSimulatedResult(bool isTest, string track, string car, SessionPlanRecord? plan, SimulationBias bias)
    {
        var profile = career.ReputationProfile ?? new ReputationProfile();
        var trackRecord = contentIndex.Tracks.FirstOrDefault(x =>
            string.Equals(x.Id, track, StringComparison.OrdinalIgnoreCase));
        var carRecord = contentIndex.Cars.FirstOrDefault(x =>
            string.Equals(x.Id, car, StringComparison.OrdinalIgnoreCase));

        var reference = RookieTargetEngine.For(
            trackRecord?.LengthMeters ?? 0,
            carRecord?.Category ?? "",
            carRecord?.PowerHp ?? 0,
            carRecord?.MassKg ?? 0,
            career.EvaluationTargetMilliseconds,
            trackRecord?.Category ?? "permanent").TargetMilliseconds;

        var fieldSize = career.RaceHistory.Count > 0 && career.RaceHistory[^1].Classification.Count > 1
            ? career.RaceHistory[^1].Classification.Count
            : DefaultFieldSize(career.Tier);

        var input = new RaceSimulator.Input
        {
            Track = track,
            Car = car,
            // Il gruppo di questo campionato, in questa stagione: gli stessi
            // avversari da un round all'altro.
            RosterSeed = HashCode.Combine(career.Season, career.Championship ?? "", career.Car ?? ""),
            PlayerName = string.IsNullOrWhiteSpace(career.Driver) ? "Pilota" : career.Driver,
            TeammateName = career.Teammate ?? "",
            FieldSize = fieldSize,
            Laps = Math.Max(1, plan?.RaceLaps ?? 10),
            // Senza un piano preparato il livello si stima dal gradino, non da una
            // costante: 90 e' il valore di una categoria regionale e in kart
            // rendeva la gara persa in partenza.
            // Senza un piano preparato il livello si stima dal gradino reale
            // della vettura, non dal tier: è la stessa curva usata dal preset.
            AiLevel = plan?.AiLevel > 0
                ? plan.AiLevel
                : StimaLivelloIa(car),
            ReferenceLapMilliseconds = reference,
            CarCompetitiveness = CarCompetitiveness(car),
            TeamTrust = profile.TeamTrust,
            SportingPrestige = profile.SportingPrestige,
            Professionalism = profile.Professionalism,
            Fatigue = career.Fatigue,
            Fitness = career.Fitness,
            RacesCompleted = career.Races,
            // L'esperienza che conta è quella maturata su questo tipo di
            // vettura: chi sale di categoria è di nuovo un esordiente rispetto
            // a chi ci corre da anni, ed è così che una carriera resta dura
            // anche in alto.
            RacesInCategory = (career.RaceHistory ?? [])
                .Count(x => !string.IsNullOrWhiteSpace(x.Car)
                            && x.Car.Equals(car, StringComparison.OrdinalIgnoreCase)),
            ChampionshipLevel = career.ChampionshipLevel,
            Talent = career.Talent ?? new DriverTalent(),
            // La dote sul bagnato conta solo quando piove davvero: il meteo
            // del weekend è già stato deciso dal pianificatore.
            Wet = (plan?.WeatherLabel ?? "").Contains("pioggia", StringComparison.OrdinalIgnoreCase)
                  || (plan?.WeatherLabel ?? "").Contains("bagnat", StringComparison.OrdinalIgnoreCase),
            IsTest = isTest,
            Bias = bias,
            // Il tentativo entra nel seme: ripetere la stessa sessione dopo un
            // ritiro non ridà lo stesso esito, ma la singola sessione resta
            // riproducibile.
            Seed = RaceSimulator.SeedFor(career.Driver ?? "", career.Season, career.Round + 1, track,
                career.SimulatedSessions)
        };

        var result = RaceSimulator.Simulate(input);
        // Nel banco DEBUG "giornata buona" deve essere utile alla storia: per
        // un test significa chiudere il giro sotto il riferimento calcolato
        // dai contenuti. La correzione resta confinata alla simulazione e non
        // tocca mai i referti Assetto Corsa reali.
        if (isTest && bias == SimulationBias.Positive && reference > 0 && result.BestLapMilliseconds > reference)
        {
            result = new ImportedRaceResult
            {
                Track = result.Track,
                PlayerName = result.PlayerName,
                Car = result.Car,
                Position = result.Position,
                StartingPosition = result.StartingPosition,
                Laps = result.Laps,
                BestLapMilliseconds = Math.Max(1, (int)Math.Round(reference * 0.94)),
                GapMilliseconds = result.GapMilliseconds,
                PitStops = result.PitStops,
                PenaltySeconds = result.PenaltySeconds,
                Damage = result.Damage,
                QualificationPosition = result.QualificationPosition,
                SessionName = result.SessionName,
                Dnf = result.Dnf,
                Classification = result.Classification
            };
        }
        return result;
    }

    /// <summary>Dimensione tipica del gruppo quando non c'è uno storico da cui dedurla.</summary>
    private static int DefaultFieldSize(string tier) => tier switch
    {
        "Formula / top tier" => 20,
        "Categoria avanzata" => 16,
        "Categoria regionale" => 14,
        _ => 12
    };
}
