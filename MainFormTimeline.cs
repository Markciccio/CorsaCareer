using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Il tempo della carriera: cosa arriva e cosa c'e stato.
///
/// Prima la colonna di destra era un dossier di etichette e valori — prossima
/// sessione, classifica, note, ultimi risultati — e non si capiva ne dove si
/// stava andando ne da dove si veniva. Erano dati consultabili, non una carriera
/// leggibile.
///
/// Qui diventano due racconti: il calendario degli appuntamenti con le loro date
/// e il motivo per cui esistono, e l'ascesa stagione per stagione con la
/// categoria raggiunta. Entrambi nascono solo da cio che e salvato.
/// </summary>
public sealed partial class MainForm
{
    /// <summary>
    /// Apre il dettaglio tecnico. I testi sono gia composti da RefreshDossier:
    /// non venivano piu mostrati da quando la colonna di destra racconta il
    /// tempo della carriera, ma non sono andati persi.
    /// </summary>
    private void OpenTechnicalDossier()
    {
        using var finestra = new TechnicalDossierDialog(nextSession.Text, standings.Text, dossier.Text, results.Text);
        finestra.ShowDialog(this);
    }

    /// <summary>
    /// Presenta la fase se e nuova. Va chiamata dopo ogni aggiornamento dello
    /// stato: il passaggio di fase nasce da un risultato, non da un comando.
    /// </summary>
    private void AnnouncePhaseIfNew()
    {
        if (announcingPhase) return;
        // Una finestra modale aperta da un aggiornamento blocca qualunque uso non
        // interattivo — il rendering fuori schermo restava fermo per sempre in
        // attesa di un clic che non poteva arrivare. Si presenta solo a una
        // finestra davvero visibile.
        if (CareerMessages.Unattended) return;
        if (!Visible || !IsHandleCreated || WindowState == FormWindowState.Minimized) return;
        if (!CareerPhases.NeedsAnnouncement(career, out var phase, contentIndex.Cars)) return;
        announcingPhase = true;
        try
        {
            career.AnnouncedPhase = phase.Id;
            SaveCareer(createVersionedBackup: false);
            using var intro = new PhaseIntroDialog(phase);
            intro.ShowDialog(this);
            CareerLog.Info("fase", $"presentata la fase «{phase.Title}» ({phase.Id})");
        }
        finally { announcingPhase = false; }
    }

    /// <summary>Evita che un aggiornamento innescato dalla finestra la riapra.</summary>
    private bool announcingPhase;

    /// <summary>
    /// La disciplina non si sceglie da un menu: la decide il sedile che firmi.
    ///
    /// Prima, dopo tre gare di kart, compariva una finestra che chiedeva se
    /// volevi fare il pilota di monoposto, di endurance o di turismo. Arrivava
    /// senza nessuna ragione — nessuna squadra l'aveva proposto, nessuno stava
    /// offrendo niente — e soprattutto non produceva nessun effetto visibile:
    /// si sceglieva «monoposto» e si continuava a correre in kart come prima,
    /// perche' quella scelta serviva solo a filtrare offerte che sarebbero
    /// arrivate mesi dopo. Una domanda importante fatta nel momento sbagliato e
    /// senza conseguenze visibili e' peggio di nessuna domanda.
    ///
    /// Adesso la strada si registra quando il pilota firma davvero per una
    /// vettura fuori dal kart: da quel momento il mercato gli propone la sua
    /// disciplina, e la decisione l'ha presa accettando un sedile, che e' come
    /// funziona nella realta'.
    /// </summary>
    private void RegistraStradaDalSedile()
    {
        if (!string.IsNullOrWhiteSpace(career.ChosenPath)) return;
        var rung = CareerLadder.Current(career, contentIndex.Cars);
        if (rung.Path == LadderPath.Karting) return;
        career.ChosenPath = rung.Path.ToString();
        career.News.Add($"{career.Driver} è ufficialmente un pilota di {CareerLadder.PathName(rung.Path)}.");
        CareerLog.Info("scala", $"strada registrata dal sedile firmato: {career.ChosenPath}");
        SaveCareer(createVersionedBackup: false);
    }

    /// <summary>
    /// Annuncia la convocazione a una selezione, quando arriva. E' il momento in
    /// cui qualcuno decide di guardare davvero il pilota, e va detto invece di
    /// comparire come una riga nell'agenda: da qui passa il salto di categoria.
    /// </summary>
    private void AnnounceSelectionIfCalled()
    {
        if (announcingPhase) return;
        if (CareerMessages.Unattended) return;
        if (!Visible || !IsHandleCreated || WindowState == FormWindowState.Minimized) return;
        // Una selezione gia aperta non va riannunciata: la porta d'ingresso in
        // quel caso e il pulsante principale della home.
        if (OpenSelection != null) return;

        announcingPhase = true;
        try
        {
            var trial = OfferSelectionIfDue();
            if (trial == null) return;
            var rung = CareerLadder.ById(trial.TargetRungId);
            var message = string.Join("\n",
                $"{trial.OrganisedBy} convoca {career.Driver} alla {trial.Title.ToLowerInvariant()}.",
                "",
                $"Sede: {trial.TrackName}",
                trial.TrackSubstituted ? trial.TrackSubstitutionNote : "",
                $"Dal {NarrativeCalendar.Format(trial.StartDate)} · {trial.Days.Count} giornate",
                $"In gioco: {trial.Seats} posti in {rung.Name.ToLowerInvariant()} fra {trial.Candidates} candidati",
                "",
                trial.NetCost > 0
                    ? $"Quota a carico del pilota: € {trial.NetCost:N0}" +
                      (trial.CoveredPercent > 0 ? $" ({trial.CoveredPercent}% coperto da {trial.OrganisedBy})" : "")
                    : "Nessuna quota: il programma copre tutto.",
                "",
                "La selezione si apre dal pulsante principale della carriera.");
            CareerMessages.Show(null, message.Replace("\n\n\n", "\n\n"), "CorsaCareer — convocazione",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            SoundtrackService.PlayForMood("concentrazione");
            OpenCareerArticle(career.Events.LastOrDefault(x => x.Type == "SELECTION_CALLED"));
        }
        finally { announcingPhase = false; }
    }

    private FlowLayoutPanel calendarFlow = new();
    private FlowLayoutPanel ascentFlow = new();
    private Label calendarEmpty = new();

    private static readonly CultureInfo It = CultureInfo.GetCultureInfo("it-IT");

    // ------------------------------------------------------------ calendario

    /// <summary>
    /// I prossimi appuntamenti, con la data e il perche. L'agenda non e
    /// pianificata in anticipo: ogni voce nasce da un risultato precedente, e il
    /// motivo va mostrato, altrimenti sembra un calendario deciso a tavolino.
    /// </summary>
    private void RefreshCalendar()
    {
        calendarFlow.SuspendLayout();
        foreach (Control existing in calendarFlow.Controls) existing.Dispose();
        calendarFlow.Controls.Clear();

        var width = Math.Max(180, calendarFlow.ClientSize.Width - 22);
        var prossimi = (career.Schedule ?? [])
            .Where(x => x.Status == CareerScheduler.StatusPlanned)
            .OrderBy(x => x.Date)
            .Take(5)
            .ToList();

        if (prossimi.Count == 0)
        {
            calendarFlow.Controls.Add(Righe(
                "Nessun appuntamento fissato. Il prossimo nascerà da come andrà questa sessione: l'agenda non è decisa in anticipo.",
                UiTheme.Prose, UiTheme.TextMuted, width, 0, 0));
            calendarFlow.ResumeLayout();
            return;
        }

        var oggi = career.StoryDate;
        foreach (var evento in prossimi)
        {
            var giorni = (evento.Date.Date - oggi.Date).Days;
            var quando = giorni switch
            {
                < 0 => "in ritardo",
                0 => "oggi",
                1 => "domani",
                < 7 => $"fra {giorni} giorni",
                < 14 => "fra una settimana",
                < 60 => $"fra {giorni / 7} settimane",
                _ => $"fra {giorni / 30} mesi"
            };

            var riga = new Panel
            {
                Width = width, BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 12), AutoSize = false, Height = 4
            };
            calendarFlow.Controls.Add(riga);

            calendarFlow.Controls.Add(Righe(
                $"{evento.Date.ToString("d MMMM", It).ToUpperInvariant()}  ·  {quando}",
                UiTheme.Kicker, ColorForEvent(evento.Kind), width, 0, 2));

            var luogo = string.IsNullOrWhiteSpace(evento.TrackName) ? evento.TrackId : evento.TrackName;
            var titolo = evento.Kind switch
            {
                ScheduledEventKind.EvaluationTest => $"Prova di valutazione a {NarrativeEngine.Capitalize(luogo)}",
                ScheduledEventKind.ConfirmationTest => $"Prova di conferma a {NarrativeEngine.Capitalize(luogo)}",
                ScheduledEventKind.Invitation => $"Gara su invito a {NarrativeEngine.Capitalize(luogo)}",
                _ => $"Round {evento.Round} · {NarrativeEngine.Capitalize(luogo)}"
            };
            calendarFlow.Controls.Add(Righe(titolo, UiTheme.ProseStrong, UiTheme.TextPrimary, width, 0, 2));

            // Il motivo per cui l'appuntamento esiste: e la catena causale.
            var perche = !string.IsNullOrWhiteSpace(evento.GeneratedBy) ? evento.GeneratedBy
                : !string.IsNullOrWhiteSpace(evento.Objective) ? evento.Objective : "";
            if (!string.IsNullOrWhiteSpace(perche))
                calendarFlow.Controls.Add(Righe(perche, UiTheme.Small, UiTheme.TextSecondary, width, 0, 2));

            if (evento.EntryFee > 0)
                calendarFlow.Controls.Add(Righe(
                    evento.EntryFeePaid
                        ? $"Quota di € {evento.EntryFee:N0} già versata"
                        : $"Quota da versare: € {evento.EntryFee:N0}",
                    UiTheme.Small, evento.EntryFeePaid ? UiTheme.TextMuted : UiTheme.Warning, width, 0, 0));
        }
        calendarFlow.ResumeLayout();
    }

    private static Color ColorForEvent(ScheduledEventKind kind) => kind switch
    {
        ScheduledEventKind.EvaluationTest => UiTheme.Warning,
        ScheduledEventKind.ConfirmationTest => UiTheme.Warning,
        ScheduledEventKind.Invitation => UiTheme.Info,
        _ => UiTheme.Accent
    };

    // ---------------------------------------------------------------- ascesa

    /// <summary>
    /// L'ascesa: dove era il pilota e dove è arrivato. Ogni stagione archiviata
    /// e una riga; la stagione in corso chiude l'elenco. Serve a rendere
    /// percepibile il movimento della carriera, che in un elenco di numeri non
    /// si vede.
    /// </summary>
    private void RefreshAscent()
    {
        ascentFlow.SuspendLayout();
        foreach (Control existing in ascentFlow.Controls) existing.Dispose();
        ascentFlow.Controls.Clear();

        var width = Math.Max(180, ascentFlow.ClientSize.Width - 22);
        var archivio = (career.SeasonArchive ?? []).OrderBy(x => x.Season).ToList();

        if (archivio.Count == 0 && career.Races == 0)
        {
            ascentFlow.Controls.Add(Righe(
                "La carriera non è ancora cominciata. Questa colonna si riempirà stagione dopo stagione.",
                UiTheme.Prose, UiTheme.TextMuted, width, 0, 0));
            ascentFlow.ResumeLayout();
            return;
        }

        foreach (var stagione in archivio)
        {
            ascentFlow.Controls.Add(Righe($"STAGIONE {stagione.Season}", UiTheme.Kicker, UiTheme.TextMuted, width, 8, 2));
            ascentFlow.Controls.Add(Righe(
                string.IsNullOrWhiteSpace(stagione.Tier) ? stagione.Championship : stagione.Tier,
                UiTheme.ProseStrong, UiTheme.TextPrimary, width, 0, 2));

            var pezzi = new List<string>();
            if (stagione.FinalPosition > 0) pezzi.Add($"{stagione.FinalPosition}° in classifica");
            if (stagione.Wins > 0) pezzi.Add($"{stagione.Wins} {(stagione.Wins == 1 ? "vittoria" : "vittorie")}");
            pezzi.Add($"{stagione.Points} punti");
            if (stagione.Award > 0) pezzi.Add($"premio € {stagione.Award:N0}");
            ascentFlow.Controls.Add(Righe(string.Join(" · ", pezzi), UiTheme.Small, UiTheme.TextSecondary, width, 0, 4));
        }

        // La stagione in corso: e il presente, e si distingue.
        ascentFlow.Controls.Add(Righe($"STAGIONE {career.Season} · IN CORSO", UiTheme.Kicker, UiTheme.Accent, width, 10, 2));
        ascentFlow.Controls.Add(Righe(
            career.ContractActive && career.Team != "Senza contratto" ? $"{career.Tier} con {career.Team}" : career.Tier,
            UiTheme.ProseStrong, UiTheme.TextPrimary, width, 0, 2));

        var correnti = new List<string>();
        if (rounds.Count > 0) correnti.Add($"round {Math.Min(career.Round + 1, rounds.Count)} di {rounds.Count}");
        correnti.Add($"{career.Races} gare in carriera");
        if (career.Wins > 0) correnti.Add($"{career.Wins} {(career.Wins == 1 ? "vittoria" : "vittorie")}");
        if (career.Podiums > 0) correnti.Add($"{career.Podiums} {(career.Podiums == 1 ? "podio" : "podi")}");
        ascentFlow.Controls.Add(Righe(string.Join(" · ", correnti), UiTheme.Small, UiTheme.TextSecondary, width, 0, 6));

        // Come si e mossa la reputazione: la parte della carriera che i numeri di
        // classifica non raccontano.
        var profile = career.ReputationProfile ?? new ReputationProfile();
        ascentFlow.Controls.Add(Righe("COME LO VEDONO", UiTheme.Kicker, UiTheme.TextMuted, width, 10, 4));
        foreach (var (etichetta, valore) in new[]
                 {
                     ("Fiducia dei team", profile.TeamTrust),
                     ("Interesse degli sponsor", profile.SponsorAppeal),
                     ("Considerazione della stampa", profile.PressStanding),
                     ("Prestigio sportivo", profile.SportingPrestige),
                     ("Livello influencer", profile.PublicPopularity),
                     ("Affidabilità", profile.Professionalism)
                 })
            ascentFlow.Controls.Add(BuildGauge(etichetta, valore, width));

        ascentFlow.ResumeLayout();
    }

    /// <summary>
    /// Una dimensione di reputazione come barra: il valore si legge a colpo
    /// d'occhio, cosa che sei numeri in fila non permettono.
    /// </summary>
    private static Control BuildGauge(string label, int value, int width)
    {
        var host = new Panel
        {
            Width = width, Height = 30, BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 4)
        };
        var testo = new Label
        {
            Text = label, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary,
            Dock = DockStyle.Top, Height = 15, UseMnemonic = false
        };
        var numero = new Label
        {
            Text = value.ToString(), Font = UiTheme.Small, ForeColor = UiTheme.ScaleColor(value),
            Dock = DockStyle.Right, Width = 26, TextAlign = ContentAlignment.TopRight
        };
        var barra = new Panel { Dock = DockStyle.Bottom, Height = 5, BackColor = UiTheme.SurfaceRaised };
        var pieno = Math.Clamp(value, 0, 100);
        barra.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(UiTheme.ScaleColor(pieno));
            e.Graphics.FillRectangle(brush, 0, 0, barra.Width * pieno / 100f, barra.Height);
        };
        host.Controls.Add(numero);
        host.Controls.Add(testo);
        host.Controls.Add(barra);
        return host;
    }
}
