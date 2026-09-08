using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>Archivio visuale: non un elenco tecnico, ma la cronologia leggibile della carriera.</summary>
public sealed class CalendarDialog : CareerDialog
{
    private readonly ListBox timeline = new()
    {
        Dock = DockStyle.Fill, BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary,
        BorderStyle = BorderStyle.None, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 68,
        IntegralHeight = false, Font = UiTheme.Body
    };
    private readonly RichTextBox detail = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
        BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body,
        ScrollBars = RichTextBoxScrollBars.Vertical, Padding = new Padding(12)
    };
    private readonly IReadOnlyList<Round> rounds;
    private readonly CareerState career;
    private readonly List<CalendarItem> items = [];
    private Image? archiveArt;

    public CalendarDialog(CareerState career, IReadOnlyList<Round> rounds, IReadOnlyList<ContentTrackRecord> tracks)
    {
        this.career = career;
        this.rounds = rounds;
        Text = "CorsaCareer — calendario e archivio";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var header = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 16, 38, 12) };
        header.Paint += (_, e) =>
        {
            using var accent = new SolidBrush(UiTheme.Warning); e.Graphics.FillRectangle(accent, 0, 0, header.Width, 4);
            using var line = new Pen(UiTheme.Border); e.Graphics.DrawLine(line, 0, header.Height - 1, header.Width, header.Height - 1);
        };
        header.Controls.Add(new Label { Text = $"Ogni test, gara e appuntamento concluso resta qui.  ·  {career.TestHistory.Count} test archiviati  ·  {career.RaceHistory.Count} gare disputate  ·  stagione attuale S{career.Season:00}", Dock = DockStyle.Fill, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary });
        header.Controls.Add(new Label { Text = "CALENDARIO E ARCHIVIO DELLA CARRIERA", Dock = DockStyle.Top, Height = 42, Font = UiTheme.Headline, ForeColor = UiTheme.Warning });

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 0, 38, 0) };
        footer.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "GIALLO · test e obiettivi     BLU · gare e risultati     VERDE · prossimo appuntamento", Font = UiTheme.Small, ForeColor = UiTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft });
        var close = UiTheme.PrimaryButton("TORNA AL PORTALE"); close.Dock = DockStyle.Right; close.Width = 210; close.Click += (_, _) => Close(); footer.Controls.Add(close);

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(34, 24, 34, 20), BackColor = UiTheme.Background };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        var timelineCard = UiTheme.Card("CRONOLOGIA · DAL PRIMO TEST A OGGI", out var timelineInner, UiTheme.Warning); timelineCard.Dock = DockStyle.Fill; timelineInner.Controls.Add(timeline);
        var detailCard = UiTheme.Card("DOSSIER DELL'APPUNTAMENTO", out var detailInner, UiTheme.Info); detailCard.Dock = DockStyle.Fill;
        var dossierLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = UiTheme.Surface };
        dossierLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38)); dossierLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        var archiveVisual = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Normal };
        var artPath = AssetPaths.File("calendar-rookie-rain.png");
        if (File.Exists(artPath))
        {
            using var source = Image.FromFile(artPath);
            archiveArt = new Bitmap(source);
            // Crop cinematografico: una tavola panoramica riempie la fascia del
            // dossier senza bande nere, mantenendo il pilota nella parte sinistra.
            archiveVisual.Paint += (_, e) =>
            {
                if (archiveArt == null || archiveVisual.Width <= 0 || archiveVisual.Height <= 0) return;
                var targetRatio = archiveVisual.Width / (float)archiveVisual.Height;
                var sourceRatio = archiveArt.Width / (float)archiveArt.Height;
                Rectangle source;
                if (sourceRatio > targetRatio)
                {
                    var sourceWidth = (int)(archiveArt.Height * targetRatio);
                    source = new Rectangle(0, 0, sourceWidth, archiveArt.Height);
                }
                else
                {
                    var sourceHeight = (int)(archiveArt.Width / targetRatio);
                    source = new Rectangle(0, 0, archiveArt.Width, sourceHeight);
                }
                e.Graphics.DrawImage(archiveArt, archiveVisual.ClientRectangle, source, GraphicsUnit.Pixel);
            };
        }
        dossierLayout.Controls.Add(archiveVisual, 0, 0); dossierLayout.Controls.Add(detail, 0, 1);
        detailInner.Controls.Add(dossierLayout);
        grid.Controls.Add(timelineCard, 0, 0); grid.Controls.Add(detailCard, 1, 0);
        Controls.Add(grid); Controls.Add(footer); Controls.Add(header);

        BuildItems();
        foreach (var item in items) timeline.Items.Add(item);
        timeline.DrawItem += DrawTimelineItem;
        timeline.SelectedIndexChanged += (_, _) => ShowItem(timeline.SelectedItem as CalendarItem);
        if (items.Count > 0)
        {
            var nextIndex = items.FindIndex(x => x.IsNext);
            timeline.SelectedIndex = nextIndex >= 0 ? nextIndex : items.Count - 1;
        }
        else detail.Text = "La cronologia è pronta: il primo test entrerà qui con dati, conseguenze e un seguito narrativo.";
        Disposed += (_, _) => archiveArt?.Dispose();
    }

    private void BuildItems()
    {
        foreach (var test in career.TestHistory)
            items.Add(new CalendarItem(test.StoryDate, "TEST", test.Track, string.IsNullOrWhiteSpace(test.Outcome) ? "ARCHIVIATO" : test.Outcome, false, Test: test));
        foreach (var scheduled in career.Schedule)
        {
            var hasTest = scheduled.IsTest && career.TestHistory.Any(x => SameDateAndTrack(x.StoryDate, x.Track, scheduled.Date, scheduled.TrackId, scheduled.TrackName));
            var hasRace = !scheduled.IsTest && career.RaceHistory.Any(x => SameDateAndTrack(x.StoryDate, x.Track, scheduled.Date, scheduled.TrackId, scheduled.TrackName));
            if (hasTest || hasRace) continue;
            var kind = scheduled.IsTest ? "TEST" : scheduled.Kind == ScheduledEventKind.Invitation ? "INVITO" : "GARA";
            items.Add(new CalendarItem(scheduled.Date, kind, scheduled.TrackName, scheduled.IsPlanned ? "IN PROGRAMMA" : scheduled.Status.ToUpperInvariant(), scheduled.IsPlanned, Scheduled: scheduled));
        }
        foreach (var race in career.RaceHistory)
            items.Add(new CalendarItem(race.StoryDate, $"GARA · S{race.Season:00}", race.Track, race.Dnf ? "DNF" : $"P{race.Position}", false, Race: race));
        foreach (var round in rounds.Select((value, index) => (value, index)))
        {
            if (career.RaceHistory.Any(x => x.Season == career.Season && x.Round == round.index + 1)) continue;
            if (career.Schedule.Any(x => x.IsPlanned && x.TrackId.Equals(round.value.Track, StringComparison.OrdinalIgnoreCase))) continue;
            items.Add(new CalendarItem(ParseDate(round.value.Date, career.StoryDate), "GARA", round.value.GrandPrix, round.index == career.Round ? "PROSSIMA" : "PIANIFICATA", round.index == career.Round, Round: round.value));
        }
        items.Sort((a, b) => a.Date.CompareTo(b.Date));
    }

    private void DrawTimelineItem(object? sender, DrawItemEventArgs e)
    {
        e.DrawBackground(); if (e.Index < 0 || e.Index >= items.Count) return;
        var item = items[e.Index]; var selected = (e.State & DrawItemState.Selected) != 0; var accent = AccentFor(item);
        using var background = new SolidBrush(selected ? Color.FromArgb(40, 71, 96) : item.IsNext ? Color.FromArgb(35, 48, 39) : UiTheme.SurfaceRaised);
        e.Graphics.FillRectangle(background, e.Bounds);
        using var strip = new SolidBrush(accent); e.Graphics.FillRectangle(strip, e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height);
        var x = e.Bounds.Left + 14;
        TextRenderer.DrawText(e.Graphics, item.Date.ToString("dd MMM").ToUpperInvariant(), UiTheme.BodyStrong, new Rectangle(x, e.Bounds.Top + 9, 78, 22), Color.White);
        TextRenderer.DrawText(e.Graphics, $"{item.Kind}  ·  {item.Status}", UiTheme.Small, new Rectangle(x + 80, e.Bounds.Top + 10, e.Bounds.Width - 100, 20), accent);
        TextRenderer.DrawText(e.Graphics, item.Title, UiTheme.Body, new Rectangle(x, e.Bounds.Top + 34, e.Bounds.Width - 20, 24), UiTheme.TextSecondary, TextFormatFlags.EndEllipsis);
        using var border = new Pen(selected ? accent : UiTheme.Border); e.Graphics.DrawRectangle(border, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
    }

    private void ShowItem(CalendarItem? item)
    {
        if (item == null) { detail.Text = "Seleziona un appuntamento per aprire il dossier."; return; }
        if (item.Test is { } test)
        {
            var delta = test.TargetLapMilliseconds > 0 && test.BestLapMilliseconds > 0 ? test.BestLapMilliseconds - test.TargetLapMilliseconds : 0;
            detail.Text = $"TEST ARCHIVIATO · {item.Status}\n\n{test.StoryDate:dddd d MMMM yyyy} · {test.Track}\n\nDATI DI PISTA\nAuto: {UiText.Car(test.Car)}\nGiri completati: {test.Laps}\nMiglior tempo: {(test.BestLapMilliseconds > 0 ? FormatLap(test.BestLapMilliseconds) : "nessun giro valido")}\nRiferimento richiesto: {(test.TargetLapMilliseconds > 0 ? FormatLap(test.TargetLapMilliseconds) : "n/d")}\nScarto dal riferimento: {(test.BestLapMilliseconds > 0 && test.TargetLapMilliseconds > 0 ? $"{delta / 1000.0:+0.000;-0.000;0.000} s" : "n/d")}\nFonte: {(test.SourceKind == ResultProvenance.Simulated ? "DEBUG · sessione simulata" : "Assetto Corsa · referto importato")}\n\nCOSA HA PRODOTTO\nPrestigio: {(test.ReputationDelta >= 0 ? "+" : "")}{test.ReputationDelta}\nDenaro: {(test.CashDelta >= 0 ? "+" : "")}€ {test.CashDelta:N0} · saldo dopo: € {test.CashAfter:N0}\nForma fisica: {(test.FitnessDelta >= 0 ? "+" : "")}{test.FitnessDelta} · {test.FitnessAfter}/100\nFiducia paddock: {(test.TrustDelta >= 0 ? "+" : "")}{test.TrustDelta} · {test.TrustAfter}/100\n\n{(string.IsNullOrWhiteSpace(test.ConsequenceSummary) ? "Il verdetto resta archiviato e influenza le prossime occasioni." : test.ConsequenceSummary)}";
            return;
        }
        if (item.Race is { } race)
        {
            var comment = RaceComment(race);
            var consequence = race.Dnf
                ? "Il ritiro brucia la quota e apre un lavoro di recupero: prima di chiedere un altro sedile servono dati e affidabilità."
                : race.Position <= 3
                    ? $"Il podio porta visibilità: {race.Points} punti e un premio di € {race.Prize:N0} cambiano il margine per il prossimo appuntamento."
                    : $"Il risultato non porta punti né premio: il prossimo test dovrà spiegare dove recuperare il distacco senza consumare altra cassa.";
            detail.Text = $"RISULTATO ARCHIVIATO · STAGIONE {race.Season}\n\n{race.StoryDate:dddd d MMMM yyyy} · {race.Track}\n\nDATI DI GARA\nAuto: {UiText.Car(race.Car)}\nEsito: {(race.Dnf ? "ritiro (DNF)" : $"P{race.Position}")} · partenza P{race.StartingPosition}\nGiri: {race.Laps} · miglior giro: {(race.BestLapMilliseconds > 0 ? FormatLap(race.BestLapMilliseconds) : "n/d")}\n\nBILANCIO DEL WEEKEND\nPunti: {race.Points}\nPremio: € {race.Prize:N0}\nBonus sponsor: € {race.SponsorBonus:N0}\nCosti logistica pagati: € {race.LogisticsPaid:N0}\nCosti danni pagati: € {race.DamagePaid:N0}\nCosti non sostenuti: € {race.UnpaidCosts:N0}\nSaldo dopo l'evento: € {race.CashAfter:N0}\nForma fisica: {(race.FitnessDelta >= 0 ? "+" : "")}{race.FitnessDelta} · {race.FitnessAfter}/100\nFiducia paddock: {(race.TrustDelta >= 0 ? "+" : "")}{race.TrustDelta} · {race.TrustAfter}/100\nFonte: {(race.SourceKind == ResultProvenance.Simulated ? "DEBUG · sessione simulata" : "Assetto Corsa · referto importato")}\n\nCOMMENTO DEL PADDOCK\n{comment}\n\nCOSA CAMBIA ADESSO\n{consequence}\n\nIl risultato resta nel curriculum, nella reputazione e nei futuri articoli della carriera.";
            return;
        }
        if (item.Scheduled is { } scheduled)
        {
            detail.Text = $"{item.Kind} · {item.Status}\n\n{scheduled.Date:dddd d MMMM yyyy} · {scheduled.TrackName}\n\nOBIETTIVO\n{scheduled.Objective}\n\nPERCHÉ ESISTE\n{scheduled.GeneratedBy}\n\nQuando sarà concluso, qui compariranno il referto, i numeri e le conseguenze.";
            return;
        }
        detail.Text = $"{item.Kind} · {item.Status}\n\n{item.Title}\nData: {item.Date:dddd d MMMM yyyy}\n\nL'appuntamento entrerà nello storico con il suo risultato reale.";
    }

    private static Color AccentFor(CalendarItem item) => item.IsNext ? UiTheme.Positive : item.Kind.StartsWith("TEST", StringComparison.OrdinalIgnoreCase) ? UiTheme.Warning : UiTheme.Info;
    private static bool SameDateAndTrack(DateTime storedDate, string storedTrack, DateTime scheduledDate, string scheduledTrackId, string scheduledTrackName) => storedDate != default && storedDate.Date == scheduledDate.Date && (storedTrack.Equals(scheduledTrackId, StringComparison.OrdinalIgnoreCase) || storedTrack.Equals(scheduledTrackName, StringComparison.OrdinalIgnoreCase));
    private static DateTime ParseDate(string value, DateTime fallback) => DateTime.TryParse(value, out var parsed) ? parsed : fallback;
    private sealed record CalendarItem(DateTime Date, string Kind, string Title, string Status, bool IsNext, TestSessionRecord? Test = null, ScheduledEvent? Scheduled = null, RaceHistoryEntry? Race = null, Round? Round = null);
    private static string FormatLap(int milliseconds) { var span = TimeSpan.FromMilliseconds(milliseconds); return $"{(int)span.TotalMinutes}:{span.Seconds:00}.{span.Milliseconds:000}"; }
    private static string RaceComment(RaceHistoryEntry race)
    {
        var driver = race.Classification?.FirstOrDefault(x => x.IsPlayer)?.Name ?? "Il pilota";
        if (race.Dnf) return $"A {race.Track} non ha lasciato spazio agli errori: {driver} si è fermato prima della bandiera a scacchi. Il box ora deve capire se il problema è stato tecnico o di gestione del ritmo.";
        if (race.Position == 1) return $"Vittoria a {race.Track}: partenza P{race.StartingPosition}, ritmo sufficiente per trasformare il weekend in una prova concreta. Il paddock non può più archiviarlo come semplice comparsa.";
        if (race.Position <= 3) return $"Podio a {race.Track}: dopo una partenza P{race.StartingPosition}, {driver} ha tenuto il passo fino alla fine. Non è ancora una consacrazione, ma è il tipo di risultato che apre una telefonata.";
        return $"P{race.Position} a {race.Track}: una gara difficile, senza il passo per entrare nella lotta davanti. Il dato utile sono i {race.Laps} giri completati; il lavoro del box sarà trasformarli in velocità, non in scuse.";
    }
}
