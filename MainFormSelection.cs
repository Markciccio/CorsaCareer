using System.Text.Json;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// La selezione a più giornate dentro la carriera: apertura, lancio delle prove
/// e registrazione dei referti.
///
/// Sta in un file suo perché è un percorso a sé: una selezione occupa giorni
/// consecutivi, produce un referto per giornata e sfocia in un verdetto che può
/// cambiare categoria. Riusa il pianificatore, il preset e l'import già
/// esistenti — cambia solo dove finisce il risultato.
/// </summary>
public sealed partial class MainForm
{
    /// <summary>La selezione aperta, se ce n'è una.</summary>
    private SelectionTrial? OpenSelection => career.Selections.FirstOrDefault(x => x.IsOpen);

    /// <summary>
    /// La simulazione debug è disponibile solo dove lo è già per le altre
    /// sessioni: serve a chi non ha Assetto Corsa, e resta sempre dichiarata.
    /// </summary>
    private static bool DebugSimulationAvailable => true;

    /// <summary>
    /// Risolve una giornata di selezione con il simulatore di debug, marcandola
    /// come simulata. Nessun referto reale viene coinvolto.
    /// </summary>
    private void SimulateSelectionDay(SelectionTrial trial, SelectionDayResult day)
    {
        var isRace = day.Kind == SelectionDayKind.EvaluationRace;
        var plan = CurrentSessionPlan("selection");
        var simulated = BuildSimulatedResult(!isRace, trial.TrackId, trial.CarId, plan, SimulationBias.Natural);
        career.SimulatedSessions++;
        // La quota si paga anche in debug: il vincolo economico non è un effetto
        // grafico, e saltarlo renderebbe la prova gratuita.
        if (day.Day == 1 && trial.NetCost > 0)
        {
            // Anche in debug la quota si paga davvero, e non può portare la
            // cassa sotto zero.
            if (CareerWallet.TryPay(career, trial.NetCost, "la selezione").Paid)
                career.EntryFeesPaid += trial.NetCost;
        }
        RecordSelectionDay(trial, day, simulated, resultFile: "", resultHash: "", simulated: true);
    }

    /// <summary>
    /// Apre una selezione se il pilota è pronto per il salto e non ne ha già una
    /// in corso. Non la impone: diventa un'opportunità che resta nell'agenda.
    /// </summary>
    private SelectionTrial? OfferSelectionIfDue()
    {
        if (OpenSelection != null) return OpenSelection;
        if (contentIndex.Tracks.Count == 0) return null;
        var rung = CareerLadder.Current(career, contentIndex.Cars);
        var definition = SelectionCatalog.ForRung(rung);
        if (definition == null) return null;

        // Una selezione è un provino serio: arriva quando c'è qualcosa da
        // guardare. Senza gare disputate nessun programma spende quattro giorni
        // su un pilota di cui non sa niente.
        if (career.Races < 3) return null;
        var profile = career.ReputationProfile ?? new ReputationProfile();
        if (profile.TeamTrust < 30 && career.Wins == 0 && career.Podiums == 0) return null;

        // Già affrontata e conclusa: si ripresenta solo se era rimasta aperta
        // una seconda possibilità.
        var previous = career.Selections
            .Where(x => x.Id.StartsWith(definition.Id, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (previous.Any(x => x.Outcome == nameof(SelectionOutcome.Promoted))) return null;
        var secondChance = previous.Any(x => x.Outcome == nameof(SelectionOutcome.SecondChance));
        if (previous.Count > 0 && !secondChance) return null;

        var car = contentIndex.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .FirstOrDefault(x => CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Id
                .Equals(definition.TargetRungId, StringComparison.OrdinalIgnoreCase));
        // Senza una vettura della categoria di destinazione la selezione sarebbe
        // una promessa che i contenuti installati non possono mantenere.
        if (car == null) return null;

        // Chi ha già convinto il paddock paga meno: la copertura è il primo
        // segnale concreto che qualcuno sta investendo sul pilota.
        var covered = profile.TeamTrust >= 60 ? 50 : profile.TeamTrust >= 45 ? 25 : 0;
        var trial = SelectionCatalog.Build(definition, contentIndex.Tracks, car.Id,
            career.StoryDate.AddDays(14), previous.Count + 1, covered);
        career.Selections.Add(trial);
        career.Headline = $"{career.Driver} è convocato alla {definition.Title.ToLowerInvariant()} di {definition.OrganisedBy}.";
        career.News.Add(career.Headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = trial.StartDate, Type = "SELECTION_CALLED",
            Headline = career.Headline, Track = trial.TrackId, Importance = 78
        });
        CareerLog.Info("selezione", $"convocazione: {trial.Id} a {trial.TrackName}" +
            (trial.TrackSubstituted ? " (sede sostituita)" : "") + $", costo netto € {trial.NetCost}");
        SaveCareer();
        return trial;
    }

    /// <summary>Apre la schermata della selezione e agisce sulla scelta del giocatore.</summary>
    private void OpenSelectionScreen()
    {
        var trial = OpenSelection ?? OfferSelectionIfDue();
        if (trial == null)
        {
            CareerMessages.Show(null, "Nessuna selezione aperta in questo momento. Le convocazioni arrivano dopo qualche gara, quando il paddock ha dati su cui decidere.",
                "CorsaCareer — selezioni", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (BlockIfPending("Selezione")) return;

        using var dialog = new SelectionDialog(trial, career.Cash, DebugSimulationAvailable);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ChosenDay == null) { RefreshUi(); return; }
        if (dialog.UseDebug) SimulateSelectionDay(trial, dialog.ChosenDay);
        else LaunchSelectionDay(trial, dialog.ChosenDay);
    }

    /// <summary>
    /// Manda in pista una giornata di selezione. Usa lo stesso percorso del test
    /// privato: prove libere, nessun punto, referto reale.
    /// </summary>
    private void LaunchSelectionDay(SelectionTrial trial, SelectionDayResult day)
    {
        var uiAutomation = Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") == "1";
        if (awaitingResult) return;
        if (string.IsNullOrWhiteSpace(trial.TrackId) || string.IsNullOrWhiteSpace(trial.CarId))
        {
            CareerMessages.Show(null, "La selezione non ha una sede o una vettura valide fra i contenuti installati.",
                "CorsaCareer — selezione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        // La quota si paga una volta, all'ingresso: le giornate successive sono
        // già comprese.
        if (day.Day == 1 && trial.NetCost > 0)
        {
            var assessment = CareerFinances.Assess(career.Cash, trial.NetCost, 0, 0, 0);
            if (!assessment.Affordable)
            {
                CareerMessages.Show(null, $"La quota della selezione non è sostenibile: {assessment.RiskReason}.",
                    "CorsaCareer — selezione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Tutte le uscite passano dal portafoglio centrale: evita che un
            // percorso legacy possa portare il budget sotto zero o registrare
            // una quota diversa da quella effettivamente pagata.
            var payment = CareerWallet.TryPay(career, trial.NetCost, $"la selezione · {trial.Title}");
            if (!payment.Paid)
            {
                CareerMessages.Show(null, payment.Refusal ?? "Quota non sostenibile.",
                    "CorsaCareer — selezione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            career.EntryFeesPaid += payment.Amount;
        }

        // La gara di valutazione ha una griglia; le altre giornate sono prove.
        var isRace = day.Kind == SelectionDayKind.EvaluationRace;
        var selectedCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(trial.CarId, StringComparison.OrdinalIgnoreCase));
        var grid = RaceGridSelector.Select(trial.CarId, selectedCar?.Category ?? "", contentIndex.Cars);
        if (isRace && grid.Candidates.Length < 2)
        {
            CareerMessages.Show(null, "La gara di valutazione richiede almeno un avversario reale. Installa una seconda auto da competizione compatibile, poi riprova.",
                "CorsaCareer — griglia insufficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var plan = BuildSessionPlan(trial.TrackId, trial.CarId, grid.UsedMixedFallback, testSession: !isRace);
        var presetJson = isRace
            ? ContentManagerPresetBuilder.Build(trial.CarId, trial.TrackId, plan, grid.Candidates,
                Math.Min(grid.OpponentCount, Math.Max(1, trial.Candidates - 1)))
            : ContentManagerPresetBuilder.BuildTest(trial.CarId, trial.TrackId, plan);

        var presetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AcTools Content Manager", "Presets", "Quick Drive");
        Directory.CreateDirectory(presetDir);
        var presetPath = Path.Combine(presetDir,
            $"CorsaCareer - Selezione {trial.Attempt:00} G{day.Day:00} {SelectionDayResult.KindLabel(day.Kind)}.cmpreset");
        File.WriteAllText(presetPath, presetJson);
        if (!ContentManagerPresetValidator.TryValidate(presetPath, out var presetError))
        {
            CareerMessages.Show(null, $"Il preset della prova non ha superato il controllo: {presetError}.\n\nNessuna sessione è stata registrata.",
                "CorsaCareer — preset non valido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        pendingMode = "selection"; awaitingResult = true; launchTimeUtc = DateTime.UtcNow;
        var resultFile = AssettoCorsaResultLocator.FindLatestExisting();
        pendingResultHash = HashFile(resultFile);
        ArchiveSessionPlan(plan, "selection", trial.TrackId, trial.CarId);
        var pending = new
        {
            mode = "selection", selection = trial.Id, day = day.Day, kind = day.Kind.ToString(),
            track = trial.TrackId, car = trial.CarId, preset = presetPath, launchUtc = launchTimeUtc,
            resultHashBeforeLaunch = pendingResultHash, driver = career.Driver,
            format = plan.FormatLabel, weather = plan.WeatherId, temperature = plan.TemperatureC
        };
        File.WriteAllText(Path.Combine(saveDir, "pending_weekend.json"),
            JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true }));
        SaveCareer();
        CareerLog.Info("selezione", $"prova avviata: {trial.Id} giorno {day.Day} ({day.Kind}) a {trial.TrackName}");
        if (uiAutomation) { RefreshUi(); return; }
        var cmPath = LocateContentManager();
        if (string.IsNullOrWhiteSpace(cmPath))
        {
            CareerMessages.Show(null, $"Prova preparata, ma Content Manager non è stato trovato.\n\n{presetPath}",
                "CorsaCareer — selezione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshUi();
            return;
        }
        try
        {
            OpenContentManagerPreset(cmPath, presetPath);
            RefreshUi();
            if (saveStatus != null)
                saveStatus.Text = $"Selezione · giorno {day.Day} ({SelectionDayResult.KindLabel(day.Kind)}) aperto in Content Manager · in attesa del referto.";
        }
        catch (Exception error)
        {
            CareerMessages.Show(null, $"Impossibile aprire la prova: {error.Message}",
                "CorsaCareer — selezione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshUi();
        }
    }

    /// <summary>
    /// Registra il referto di una giornata e, se era l'ultima, applica il
    /// verdetto. I dati vengono dal referto: qui non si inventa nulla.
    /// </summary>
    private void RecordSelectionDay(SelectionTrial trial, SelectionDayResult day, ImportedRaceResult imported,
        string resultFile, string resultHash, bool simulated)
    {
        var target = RefreshEvaluationTargetFor(trial.TrackId, trial.CarId);
        day.Completed = true;
        day.Simulated = simulated;
        day.Track = imported.Track;
        day.Car = string.IsNullOrWhiteSpace(imported.Car) ? trial.CarId : imported.Car;
        day.StoryDate = career.StoryDate;
        day.Laps = imported.Laps;
        day.BestLapMilliseconds = imported.BestLapMilliseconds;
        day.TargetLapMilliseconds = target;
        day.PenaltySeconds = imported.PenaltySeconds;
        day.Damage = imported.Damage;
        day.Position = imported.Position;
        day.FieldSize = imported.Classification.Count;
        day.Dnf = imported.Dnf;
        day.ResultSha256 = resultHash;
        day.Score = SelectionEngine.ScoreDay(day, out var verdict);
        day.Verdict = verdict;

        // Ogni giornata è un fatto: entra nella storia anche prima del verdetto.
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = day.StoryDate, Type = "SELECTION_DAY",
            Headline = $"{trial.Title} · giorno {day.Day} ({SelectionDayResult.KindLabel(day.Kind)}): {day.Score}/100 — {day.Verdict}"
                       + (simulated ? " [simulato]" : ""),
            Track = trial.TrackId, Importance = 60
        });
        // Le giornate successive slittano dopo quella appena svolta: la selezione
        // resta un blocco di giorni consecutivi anche se il giocatore la spezza.
        var following = trial.Days.Where(x => !x.Completed && x.Day > day.Day).ToList();
        for (var i = 0; i < following.Count; i++) following[i].StoryDate = day.StoryDate.AddDays(i + 1);
        career.StoryDate = day.StoryDate.AddDays(1);

        CareerLog.Info("selezione",
            $"{trial.Id} giorno {day.Day}: {day.Score}/100 ({day.Verdict}), giri {day.Laps}, " +
            $"miglior giro {day.BestLapMilliseconds} ms, riferimento {target} ms" + (simulated ? ", simulato" : ""));

        if (trial.AllDaysDone) CloseSelection(trial);
        else SaveCareer();
        RefreshUi();

        using var dialog = new SelectionDialog(trial, career.Cash, DebugSimulationAvailable);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.ChosenDay != null)
        {
            if (dialog.UseDebug) SimulateSelectionDay(trial, dialog.ChosenDay);
            else LaunchSelectionDay(trial, dialog.ChosenDay);
        }
    }

    /// <summary>Applica il verdetto finale: promozione, seconda possibilità o rifiuto.</summary>
    private void CloseSelection(SelectionTrial trial)
    {
        var report = SelectionEngine.Assess(trial);
        trial.Outcome = report.Outcome.ToString();
        trial.Status = SelectionTrial.StatusClosed;
        var rung = CareerLadder.ById(trial.TargetRungId);

        switch (report.Outcome)
        {
            case SelectionOutcome.Promoted:
                // Il sedile diventa un'offerta reale: la firma resta una scelta
                // del giocatore, non un passaggio automatico.
                var offer = new TeamOffer
                {
                    Team = trial.OrganisedBy, Car = trial.CarId, Category = rung.Tier,
                    Sponsor = career.Sponsor, Teammate = career.Teammate,
                    Salary = 0, Years = 1, Prestige = Math.Max(career.SeatPrestige, 40),
                    Objective = $"Prima stagione in {rung.Name.ToLowerInvariant()}: arrivare in fondo e imparare",
                    Origin = $"Sedile conquistato alla {trial.Title}"
                };
                career.Offers ??= [];
                career.Offers.Insert(0, offer);
                career.Headline = $"{career.Driver} supera la selezione di {trial.OrganisedBy}: {rung.Name.ToLowerInvariant()} a disposizione.";
                break;
            case SelectionOutcome.SecondChance:
                career.Headline = $"{career.Driver} non convince del tutto alla {trial.Title}: {trial.OrganisedBy} lascia aperta una seconda prova.";
                break;
            default:
                career.Headline = $"{career.Driver} non supera la selezione di {trial.OrganisedBy}. La strada per {rung.Name.ToLowerInvariant()} resta chiusa, per ora.";
                break;
        }

        career.News.Add(career.Headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate,
            Type = report.Outcome switch
            {
                SelectionOutcome.Promoted => "SELECTION_PASSED",
                SelectionOutcome.SecondChance => "SELECTION_SECOND_CHANCE",
                _ => "SELECTION_FAILED"
            },
            Headline = career.Headline + (report.Simulated ? " [verdetto con prove simulate]" : ""),
            Track = trial.TrackId, Importance = 86
        });
        CareerLog.Info("selezione", $"{trial.Id} conclusa: {report.Overall}/100 → {report.Outcome}" +
            (report.Simulated ? " (con prove simulate)" : ""));
        SoundtrackService.PlayForMood(report.Outcome == SelectionOutcome.Promoted ? "firma" : "crisi");
        SaveCareer();

        CareerMessages.Show(null, report.Describe() + "\n\n" + report.TeamVerdict,
            $"CorsaCareer — {trial.Title}", MessageBoxButtons.OK,
            report.Outcome == SelectionOutcome.Promoted ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        OpenCareerArticle(career.Events.LastOrDefault(x => x.Type.StartsWith("SELECTION_", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Il riferimento della combinazione pista/vettura di una selezione, senza
    /// toccare quello della valutazione rookie in corso.
    /// </summary>
    private int RefreshEvaluationTargetFor(string trackId, string carId)
    {
        var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(trackId, StringComparison.OrdinalIgnoreCase));
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(carId, StringComparison.OrdinalIgnoreCase));
        var measured = career.TestHistory
            .Where(x => x.BestLapMilliseconds > 0
                && RaceImportIdentity.TracksMatch(trackId, x.Track)
                && x.Car.Equals(carId, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.BestLapMilliseconds)
            .DefaultIfEmpty(0)
            .Min();
        return RookieTargetEngine.For(track?.LengthMeters ?? 0, car?.Category ?? "", car?.PowerHp ?? 0,
            car?.MassKg ?? 0, measured, track?.Category ?? "permanent").TargetMilliseconds;
    }
}
