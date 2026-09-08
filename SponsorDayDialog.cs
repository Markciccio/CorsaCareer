using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// La giornata di Haru Senda.
///
/// Sezione separata da quella del pilota, perché è un'attività diversa svolta da
/// un'altra persona: mentre il pilota si allena, Haru gira a cercare chi paga.
///
/// Ogni visita è una decisione dichiarata prima: si vede dove vorrebbe andare,
/// quanto porterebbe, quanto è probabile e <em>perché</em>. Solo dopo aver detto
/// di sì si scopre com'è andata. Senza la probabilità in chiaro sarebbe un
/// pulsante da premere a caso.
/// </summary>
public sealed class SponsorDayDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly Action save;
    private readonly Func<SponsorVisit, SponsorReply, bool> playScene;

    private readonly FlowLayoutPanel list = new();
    private readonly Label header = new();
    private readonly Label skills = new();
    private readonly Label hours = new();

    public SponsorDayDialog(CareerState career, Action save, Func<SponsorVisit, SponsorReply, bool> playScene)
    {
        this.career = career;
        this.save = save;
        this.playScene = playScene;

        Text = "CorsaCareer — sponsorizzazioni";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        DriverDay.EnsureToday(career);
        career.Agent ??= new AgentSkills();

        Controls.Add(BuildBody());
        Controls.Add(BuildFooter());
        Controls.Add(BuildHeader());
        Refresh_();
    }

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 14, 40, 12) };
        panel.Controls.Add(new Label
        {
            Text = "SPONSORIZZAZIONI · HARU SENDA", Dock = DockStyle.Top, Height = 20,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Info, UseMnemonic = false
        });
        header.Dock = DockStyle.Top; header.Height = 30; header.Font = UiTheme.HeadlineSmall;
        header.ForeColor = UiTheme.TextPrimary; header.UseMnemonic = false;
        skills.Dock = DockStyle.Top; skills.Height = 22; skills.Font = UiTheme.Small;
        skills.ForeColor = UiTheme.TextSecondary; skills.UseMnemonic = false;
        hours.Dock = DockStyle.Top; hours.Height = 42; hours.Font = new Font(UiTheme.FamilySemibold, 14F, FontStyle.Bold);
        hours.ForeColor = UiTheme.Positive; hours.BackColor = Color.FromArgb(34, 40, 52);
        hours.Padding = new Padding(10, 6, 10, 6); hours.UseMnemonic = false;
        var note = new Label
        {
            Dock = DockStyle.Top, Height = 20, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted,
            UseMnemonic = false,
            Text = "Haru migliora lavorando: anche un rifiuto gli insegna qualcosa."
        };
        panel.Controls.Add(note);
        panel.Controls.Add(hours);
        panel.Controls.Add(skills);
        panel.Controls.Add(header);
        panel.Controls.SetChildIndex(note, 0);
        panel.Controls.SetChildIndex(hours, 1);
        panel.Controls.SetChildIndex(skills, 2);
        panel.Controls.SetChildIndex(header, 3);
        return panel;
    }

    private Control BuildBody()
    {
        list.Dock = DockStyle.Fill;
        list.FlowDirection = FlowDirection.TopDown;
        list.WrapContents = false;
        list.AutoScroll = true;
        list.BackColor = UiTheme.Background;
        list.Padding = new Padding(40, 18, 30, 16);
        return list;
    }

    private Control BuildFooter()
    {
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 14, 40, 16) };
        var close = UiTheme.SecondaryButton("CHIUDI");
        close.Dock = DockStyle.Right;
        close.Width = 200;
        close.Click += (_, _) => Close();
        panel.Controls.Add(close);
        CancelButton = close;
        return panel;
    }

    private void Refresh_()
    {
        var day = career.Today!;
        header.Text = NarrativeCalendar.Format(day.Date);
        hours.Text = $"◉  HARU SENDA   {day.AgentHoursLeft} / {DriverDay.AgentHours} ORE LIBERE    ·    BUDGET € {career.Cash:N0}";
        skills.Text = career.Agent!.Describe();

        list.SuspendLayout();
        foreach (Control existing in list.Controls) existing.Dispose();
        list.Controls.Clear();

        var width = Math.Max(520, list.ClientSize.Width - 90);
        var visits = SponsorVisits.ForDay(career)
            .Where(x => !day.Done.Contains(x.Id, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (visits.Count == 0)
        {
            list.Controls.Add(Line("Per oggi Haru ha finito.", UiTheme.HeadlineSmall, UiTheme.TextSecondary, width, 0, 8));
            list.Controls.Add(Line(
                "Le porte che restano aperte le riprova domani. Nel frattempo, quello che conta lo decide la pista.",
                UiTheme.Prose, UiTheme.TextMuted, width, 0, 0));
        }
        else foreach (var visit in visits) list.Controls.Add(BuildCard(visit, width, day));

        list.ResumeLayout();
    }

    private Control BuildCard(SponsorVisit visit, int width, DayPlan day)
    {
        var affordableHours = visit.Hours <= day.AgentHoursLeft;
        var card = new Panel
        {
            Width = width, BackColor = UiTheme.SurfaceRaised,
            Padding = new Padding(20, 16, 20, 16), Margin = new Padding(0, 0, 0, 14)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var accent = new SolidBrush(ChanceColour(visit.Chance));
            e.Graphics.FillRectangle(accent, 0, 0, 3, card.Height);
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent
        };
        var inner = width - 48;

        flow.Controls.Add(Line(visit.Target.ToUpperInvariant(), UiTheme.Kicker, UiTheme.Info, inner, 0, 4));
        flow.Controls.Add(Line(visit.Pitch, UiTheme.Standfirst, UiTheme.TextPrimary, inner, 0, 8));
        flow.Controls.Add(Line(
            $"Porterebbe € {visit.Amount:N0}   ·   {visit.Hours} ore di Haru   ·   probabilità {visit.Chance}%",
            UiTheme.BodyStrong, ChanceColour(visit.Chance), inner, 0, 4));

        // Da cosa dipende: senza questo la probabilità è un numero calato dall'alto.
        if (visit.Reasons.Count > 0)
            flow.Controls.Add(Line("Dipende da: " + string.Join("  ·  ", visit.Reasons),
                UiTheme.Small, UiTheme.TextMuted, inner, 0, 10));

        if (affordableHours)
        {
            var go = UiTheme.PrimaryButton($"MANDA HARU DA {visit.Target.ToUpperInvariant()}");
            go.Width = Math.Min(inner, 460);
            go.Click += (_, _) => Send(visit);
            flow.Controls.Add(go);
        }
        else
            flow.Controls.Add(Line($"Servono {visit.Hours} ore e ne restano {day.AgentHoursLeft}.",
                UiTheme.Small, UiTheme.Accent, inner, 0, 0));

        card.Controls.Add(flow);
        card.Height = flow.PreferredSize.Height + card.Padding.Vertical;
        return card;
    }

    private static Color ChanceColour(int chance) =>
        chance >= 60 ? UiTheme.Positive : chance >= 35 ? UiTheme.Warning : UiTheme.Accent;

    private void Send(SponsorVisit visit)
    {
        var day = career.Today!;
        if (visit.Hours > day.AgentHoursLeft)
        {
            MessageBox.Show($"Servono {visit.Hours} ore e ne restano {day.AgentHoursLeft}.",
                "CorsaCareer — giornata finita", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var reply = SponsorVisits.Resolve(visit, career);

        // Le ore si consumano comunque: anche una trattativa fallita è un
        // pomeriggio speso.
        day.AgentHoursLeft -= visit.Hours;
        day.Done.Add(visit.Id);

        if (reply.Accepted)
        {
            career.Cash += reply.Amount;
            career.SponsorMoney += reply.Amount;
            career.Results.Add($"Sponsorizzazione da {visit.Target}: € +{reply.Amount:N0}");
            var profile = career.ReputationProfile ??= new ReputationProfile();
            profile.SponsorAppeal = Math.Clamp(profile.SponsorAppeal + 2, 0, 100);
            profile.SyncLegacyFields(career);
        }

        SponsorVisits.Learn(career.Agent!, reply.Accepted);
        save();

        playScene(visit, reply);
        Refresh_();
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
}
