using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// Le attività del pilota, giorno per giorno.
///
/// Prende il posto dell'agenda. Prima fra un impegno e l'altro non c'era niente
/// da fare, e per questo non si poteva nemmeno indicare un'alternativa quando i
/// soldi mancavano: non esisteva un modo di guadagnare, allenarsi o farsi
/// conoscere.
///
/// Questa schermata riguarda esclusivamente il pilota. Le attività di Haru
/// hanno una sezione separata, dedicata alle sponsorizzazioni.
/// </summary>
public sealed class DriverDayDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly Action save;
    private readonly Action<DayReport>? onScene;
    private readonly Action? openSponsorDay;

    private readonly FlowLayoutPanel driverColumn = new();
    private readonly Label header = new();
    private readonly Label stats = new();
    private readonly Label indices = new();
    private readonly Label driverHours = new();
    private readonly Button tomorrow = new();
    private readonly Button autoPlan = new();

    /// <summary>
    /// Quale gruppo di attività mostrare. Null significa tutte: è il caso della
    /// sezione «Attività del pilota». I due riquadri della Home passano invece
    /// il proprio, perché aprivano lo stesso elenco e dal social si finiva a
    /// scegliere la palestra.
    /// </summary>
    private readonly DayFocus? focus;

    public DriverDayDialog(CareerState career, Action save, Action<DayReport>? onScene = null, Action? openSponsorDay = null, DayFocus? focus = null)
    {
        this.career = career;
        this.save = save;
        this.onScene = onScene;
        this.openSponsorDay = openSponsorDay;
        this.focus = focus;

        Text = focus switch
        {
            DayFocus.Fisico => "CorsaCareer — preparazione del pilota",
            DayFocus.Immagine => "CorsaCareer — immagine e social",
            _ => "CorsaCareer — attività del pilota"
        };
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        DriverDay.EnsureToday(career);

        Controls.Add(BuildBody());
        Controls.Add(BuildFooter());
        Controls.Add(BuildHeader());
        Refresh_();
    }

    // ------------------------------------------------------------- struttura

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 144, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 14, 40, 12) };
        panel.Controls.Add(new Label
        {
            Text = "ATTIVITÀ DEL PILOTA", Dock = DockStyle.Top, Height = 20,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Warning, UseMnemonic = false
        });
        header.Dock = DockStyle.Top;
        header.Height = 32;
        header.Font = UiTheme.HeadlineSmall;
        header.ForeColor = UiTheme.TextPrimary;
        header.UseMnemonic = false;
        panel.Controls.Add(header);
        stats.Dock = DockStyle.Top;
        stats.Height = 26;
        stats.Font = UiTheme.Body;
        stats.ForeColor = UiTheme.TextSecondary;
        stats.UseMnemonic = false;
        panel.Controls.Add(stats);
        indices.Dock = DockStyle.Top;
        indices.Height = 34;
        indices.Font = new Font(UiTheme.FamilySemibold, 14F, FontStyle.Bold);
        indices.ForeColor = UiTheme.Positive;
        indices.UseMnemonic = false;
        indices.TextAlign = ContentAlignment.MiddleLeft;
        panel.Controls.Add(indices);

        var hoursBox = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(34, 40, 52), Margin = new Padding(0, 4, 0, 0), Padding = new Padding(12, 4, 12, 4) };
        driverHours.Dock = DockStyle.Fill; driverHours.Font = new Font(UiTheme.FamilySemibold, 13F, FontStyle.Bold);
        driverHours.ForeColor = UiTheme.TextPrimary; driverHours.TextAlign = ContentAlignment.MiddleLeft; driverHours.UseMnemonic = false;
        hoursBox.Controls.Add(driverHours);
        panel.Controls.Add(hoursBox);

        // Dock=Top impila in ordine inverso di aggiunta.
        panel.Controls.SetChildIndex(stats, 0);
        panel.Controls.SetChildIndex(indices, 1);
        panel.Controls.SetChildIndex(header, 3);
        panel.Controls.SetChildIndex(hoursBox, 2);
        return panel;
    }

    private Control BuildBody()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1,
            BackColor = UiTheme.Background, Padding = new Padding(30, 16, 24, 10)
        };
        grid.Controls.Add(Column("IL PILOTA", driverColumn, UiTheme.Accent), 0, 0);
        return grid;
    }

    private static Control Column(string title, FlowLayoutPanel flow, Color accent)
    {
        var card = UiTheme.Card(title, out var inner, accent);
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, UiTheme.Gutter, 0);
        flow.Dock = DockStyle.Fill;
        flow.FlowDirection = FlowDirection.TopDown;
        flow.WrapContents = false;
        flow.AutoScroll = true;
        flow.BackColor = Color.Transparent;
        inner.Controls.Add(flow);
        return card;
    }

    private Control BuildFooter()
    {
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 78, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 14, 40, 18) };
        tomorrow.Text = "VAI A DOMANI";
        tomorrow.Dock = DockStyle.Right;
        tomorrow.Width = 260;
        tomorrow.Font = UiTheme.BodyStrong;
        tomorrow.BackColor = UiTheme.Accent;
        tomorrow.ForeColor = Color.White;
        tomorrow.FlatStyle = FlatStyle.Flat;
        tomorrow.FlatAppearance.BorderSize = 0;
        tomorrow.Click += (_, _) => GoToTomorrow();

        var close = UiTheme.SecondaryButton("CHIUDI");
        close.Dock = DockStyle.Right;
        close.Width = 180;
        close.Margin = new Padding(0, 0, 12, 0);
        close.Click += (_, _) => Close();

        if (openSponsorDay != null)
        {
            var sponsor = UiTheme.SecondaryButton("SPONSORIZZAZIONI · HARU SENDA");
            sponsor.Dock = DockStyle.Left;
            sponsor.Width = 310;
            sponsor.Click += (_, _) => { openSponsorDay(); Close(); };
            panel.Controls.Add(sponsor);
        }

        autoPlan.Text = "ATTIVITÀ AUTOMATICHE FINO AL PROSSIMO EVENTO";
        autoPlan.Dock = DockStyle.Left;
        autoPlan.Width = 390;
        autoPlan.Font = UiTheme.BodyStrong;
        autoPlan.BackColor = Color.FromArgb(45, 92, 120);
        autoPlan.ForeColor = Color.White;
        autoPlan.FlatStyle = FlatStyle.Flat;
        autoPlan.FlatAppearance.BorderSize = 0;
        autoPlan.Click += (_, _) => RunAutomaticPlan();
        panel.Controls.Add(autoPlan);

        panel.Controls.Add(tomorrow);
        panel.Controls.Add(close);
        CancelButton = close;
        return panel;
    }

    // ----------------------------------------------------------- contenuto

    private void Refresh_()
    {
        var day = career.Today!;
        header.Text = NarrativeCalendar.Format(day.Date);
        driverHours.Text = $"◉  PILOTA   {day.DriverHoursLeft} / {DriverDay.DriverHours} ORE LIBERE";

        var profile = career.ReputationProfile ?? new ReputationProfile();
        indices.Text = $"FORMA  {career.Fitness}/100     FIDUCIA TEAM  {career.TeamRelation}/100     APPEAL SPONSOR  {career.SponsorRelation}/100";
        stats.Text = $"Budget disponibile € {career.Cash:N0}   ·   stanchezza {career.Fatigue}/100 ({DriverDay.ConditionLabel(career.Fitness, career.Fatigue)})   ·   "
                     + $"livello influencer {profile.PublicPopularity}/100   ·   reputazione sportiva {profile.SportingPrestige}/100";

        Fill(driverColumn, DayActor.Driver);
        autoPlan.Enabled = career.DaysUntilNextRound > 0;
        autoPlan.Text = career.DaysUntilNextRound > 0
            ? $"AUTO · FINO AL PROSSIMO EVENTO ({career.DaysUntilNextRound} GIORNI)"
            : "AUTO · EVENTO PREVISTO OGGI";
    }

    private void Fill(FlowLayoutPanel flow, DayActor actor)
    {
        flow.SuspendLayout();
        foreach (Control existing in flow.Controls) existing.Dispose();
        flow.Controls.Clear();

        var width = Math.Max(300, flow.ClientSize.Width - 28);
        foreach (var (activity, available, reason) in DayEngine.Available(career, career.Today!, actor))
        {
            if (focus != null && activity.Focus != focus) continue;
            flow.Controls.Add(BuildCard(activity, available, reason, width));
        }

        flow.ResumeLayout();
    }

    private Control BuildCard(DayActivity activity, bool available, string reason, int width)
    {
        var card = new Panel
        {
            Width = width, BackColor = UiTheme.SurfaceRaised,
            Padding = new Padding(14, 10, 14, 10), Margin = new Padding(0, 0, 0, 10),
            Cursor = available ? Cursors.Hand : Cursors.Default
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(available ? UiTheme.Border : UiTheme.SurfaceRaised);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var accent = new SolidBrush(available ? UiTheme.Warning : UiTheme.TextMuted);
            e.Graphics.FillRectangle(accent, 0, 0, 3, card.Height);
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent
        };
        var inner = width - 34;
        var colour = available ? UiTheme.TextPrimary : UiTheme.TextMuted;

        var cost = activity.Cost > 0 ? $"  ·  € {activity.Cost:N0}" : "";
        flow.Controls.Add(Line($"{activity.Name}   ({activity.Hours} h{cost})", UiTheme.BodyStrong, colour, inner, 0, 3));
        flow.Controls.Add(Line(activity.Promise, UiTheme.Small,
            available ? UiTheme.TextSecondary : UiTheme.TextMuted, inner, 0, 3));

        // Un'attività con esito incerto lo dichiara: il giocatore deve sapere
        // che sta scommettendo, non scoprirlo dopo.
        if (!activity.IsCertain)
            flow.Controls.Add(Line("Esito non garantito.", UiTheme.Small, UiTheme.Info, inner, 0, 0));
        if (!available)
            flow.Controls.Add(Line(reason, UiTheme.Small, UiTheme.Accent, inner, 0, 0));

        card.Controls.Add(flow);
        card.Height = flow.PreferredSize.Height + card.Padding.Vertical;

        if (available)
        {
            void Do(object? _, EventArgs __) => Perform(activity);
            card.Click += Do;
            flow.Click += Do;
            foreach (Control child in flow.Controls) child.Click += Do;
        }
        return card;
    }

    private static Label Line(string text, Font font, Color color, int width, int above, int below)
    {
        var label = new Label
        {
            Text = text, Font = font, ForeColor = color, AutoSize = false, Width = width,
            BackColor = Color.Transparent, UseMnemonic = false, Margin = new Padding(0, above, 0, below)
        };
        using var g = label.CreateGraphics();
        label.Height = (int)Math.Ceiling(g.MeasureString(text, font, width).Height) + 2;
        return label;
    }

    // ------------------------------------------------------------- azioni

    private void Perform(DayActivity activity)
    {
        var report = DayEngine.Perform(career, career.Today!, activity);
        if (report.Refused)
        {
            MessageBox.Show(report.Refusal, "CorsaCareer — non si può", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        save();
        Refresh_();

        // L'elenco resta volutamente pulito: il risultato viene raccontato
        // direttamente nella scena anime a tutto schermo.  Il vecchio report
        // tecnico resta solo come fallback per eventuali chiamanti legacy.
        if (onScene != null)
            onScene(report);
        else
        {
            using var result = new DriverActivityResultDialog(career, report);
            result.ShowDialog(this);
        }
        Refresh_();
    }

    /// <summary>
    /// Vero se l'esito merita una scena raccontata. Un allenamento non la
    /// merita: mostrarla ogni volta la svaluterebbe.
    /// </summary>
    private static bool DeservesScene(DayReport report) =>
        report.Activity.Actor == DayActor.Agent
        || report.Outcome.Effects.Any(x => x.Kind == DayEffectKind.Money && x.Amount > 0)
        || report.Outcome.IsSetback;

    private void GoToTomorrow()
    {
        var day = career.Today!;
        var unused = day.DriverHoursLeft + day.AgentHoursLeft;
        if (unused >= 6)
        {
            var answer = MessageBox.Show(
                $"Restano {day.DriverHoursLeft} ore a te e {day.AgentHoursLeft} a Haru. Passare a domani senza usarle?",
                "CorsaCareer — giornata non finita", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        career.Today = DayEngine.Advance(career, day);
        save();
        Refresh_();
    }

    /// <summary>
    /// Modalità rapida per chi non vuole gestire ogni singola giornata.
    /// Applica attività prudenti con le stesse regole della modalità manuale,
    /// poi passa alla notte successiva fino al prossimo test/gara. Non apre
    /// scene anime una per una: al termine resta disponibile il diario.
    /// </summary>
    private void RunAutomaticPlan()
    {
        if (career.DaysUntilNextRound <= 0) return;
        var days = career.DaysUntilNextRound;
        var answer = MessageBox.Show(
            $"Avanzare automaticamente di {days} giorni fino al prossimo appuntamento?\n\n"
            + "Il pilota userà attività conservative (riposo, corsa leggera e preparazione); "
            + "costi ed effetti seguono comunque le regole normali. Nessuna gara verrà simulata.",
            "CorsaCareer — pianificazione automatica", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        var performed = 0;
        var startDate = career.StoryDate.Date;
        var conservativeOrder = new[] { "riposo", "corsa", "palestra", "tifosi", "social", "pr", "lavoro" };

        while (career.DaysUntilNextRound > 0)
        {
            var day = DriverDay.EnsureToday(career);
            var guard = 0;
            while (day.DriverHoursLeft > 0 && guard++ < 20)
            {
                var options = DayEngine.Available(career, day, DayActor.Driver)
                    .Where(x => x.Available)
                    .OrderBy(x =>
                    {
                        var index = Array.IndexOf(conservativeOrder, x.Activity.Id);
                        return index < 0 ? 99 : index;
                    })
                    .ToList();
                if (options.Count == 0) break;
                var report = DayEngine.Perform(career, day, options[0].Activity);
                if (report.Refused) break;
                performed++;
            }
            career.Today = DayEngine.Advance(career, day);
        }

        save();
        Refresh_();
        MessageBox.Show(
            $"Pianificazione completata dal {startDate:dd/MM/yyyy} al {career.StoryDate:dd/MM/yyyy}.\n"
            + $"Giornate preparate: {days}. Attività registrate: {performed}.\n\n"
            + "Controlla il diario e prepara il prossimo test o la prossima gara.",
            "CorsaCareer — prossimo appuntamento", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

/// <summary>Scena di ritorno di un'attività: immagine, racconto e conseguenze.</summary>
internal sealed class DriverActivityResultDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly DayReport report;

    public DriverActivityResultDialog(CareerState career, DayReport report)
    {
        this.career = career;
        this.report = report;
        Text = $"CorsaCareer — {report.Activity.Name}";
        BackColor = UiTheme.Background;
        Controls.Add(BuildBody());
    }

    private Control BuildBody()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
            BackColor = UiTheme.Background, Padding = new Padding(34, 28, 34, 24)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        var artwork = SceneArtwork.ForActivity(report.Activity.Id);
        var image = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 0, 22, 0) };
        if (SceneArtwork.Exists(artwork))
        {
            try
            {
                using var source = Image.FromFile(AssetPaths.File(artwork));
                image.Image = new Bitmap(source);
            }
            catch { }
        }
        root.Controls.Add(image, 0, 0);

        var story = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, Padding = new Padding(28, 24, 28, 24) };
        story.Paint += (_, e) => { using var pen = new Pen(UiTheme.Warning, 2); e.Graphics.DrawRectangle(pen, 0, 0, story.Width - 1, story.Height - 1); };
        var text = new RichTextBox
        {
            Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = UiTheme.SurfaceRaised, ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.Body, ScrollBars = RichTextBoxScrollBars.Vertical,
            DetectUrls = false
        };
        text.Text = BuildReportText();
        story.Controls.Add(text);
        root.Controls.Add(story, 1, 0);

        var back = UiTheme.PrimaryButton("TORNA ALL'ELENCO");
        back.Dock = DockStyle.Right; back.Width = 260;
        back.Click += (_, _) => Close();
        root.Controls.Add(back, 1, 1);
        return root;
    }

    private string BuildReportText()
    {
        var day = career.Today!;
        var effects = report.Applied.Count == 0 ? "Nessuna variazione numerica." : string.Join("\n", report.Applied.Select(x => "• " + x));
        return $"ATTIVITÀ COMPLETATA\n\n{report.Activity.Name.ToUpperInvariant()}\n{report.Activity.Promise}\n\n"
             + $"{report.Outcome.Line}\n\nCONSEGUENZE\n{effects}\n\n"
             + $"Tempo impiegato: {report.Activity.Hours} ore\n"
             + $"Costo: {(report.Activity.Cost > 0 ? $"€ {report.Activity.Cost:N0}" : "nessun costo")}\n\n"
             + $"ORE ANCORA DISPONIBILI OGGI\nPilota: {day.DriverHoursLeft} ore\nHaru: {day.AgentHoursLeft} ore";
    }
}
