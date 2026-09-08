using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// La schermata di una selezione a più giornate.
///
/// Mostra tutte le prove insieme — quelle fatte con il loro giudizio, quella di
/// domani con cosa il team ci guarderà — perché il senso di una selezione sta
/// nell'arco: si può recuperare un brutto primo giorno, e saperlo cambia come si
/// affronta il secondo. Il verdetto finale arriva solo quando le prove sono
/// finite, e porta con sé il calcolo che lo ha prodotto.
/// </summary>
public sealed class SelectionDialog : CareerDialog
{
    /// <summary>La prova che il giocatore ha scelto di affrontare adesso.</summary>
    public SelectionDayResult? ChosenDay { get; private set; }
    /// <summary>Vero se il giocatore chiede di risolvere la prova in debug.</summary>
    public bool UseDebug { get; private set; }

    public SelectionDialog(SelectionTrial trial, int cash, bool debugAvailable)
    {
        var report = SelectionEngine.Assess(trial);
        var next = trial.NextDay;

        Text = $"CorsaCareer — {trial.Title}";
        Size = new Size(1400, 900);
        MinimumSize = new Size(1000, 680);
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        // Dock=Top impila in ordine inverso di inserimento: il kicker va aggiunto
        // per ultimo per comparire in cima, sopra il titolo.
        var header = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 14, 38, 10) };
        header.Controls.Add(new Label
        {
            Text = $"{trial.TrackName} · dal {NarrativeCalendar.Format(trial.StartDate)} · {CareerLadder.ById(trial.TargetRungId).Name}",
            Dock = DockStyle.Bottom, Height = 22, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary
        });
        header.Controls.Add(new Label
        {
            Text = trial.Title, Dock = DockStyle.Top, Height = 38, Font = UiTheme.Title, ForeColor = UiTheme.TextPrimary
        });
        header.Controls.Add(new Label
        {
            Text = $"SELEZIONE · {trial.OrganisedBy.ToUpperInvariant()} · {trial.Candidates} CANDIDATI PER {trial.Seats} POSTI",
            Dock = DockStyle.Top, Height = 20, Font = UiTheme.Kicker, ForeColor = UiTheme.Accent
        });

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Background, Padding = new Padding(30, 20, 30, 14) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));

        // --- le giornate
        var daysCard = UiTheme.Card("LE PROVE", out var daysInner, UiTheme.Warning);
        daysCard.Dock = DockStyle.Fill;
        var daysList = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = UiTheme.Surface
        };
        foreach (var day in trial.Days) daysList.Controls.Add(DayRow(day, next));
        daysInner.Controls.Add(daysList);
        // Le righe vengono create prima che il pannello abbia la sua larghezza
        // reale: senza questo riadattamento restavano strette come il valore
        // provvisorio e il verdetto veniva tagliato a metà frase.
        void FitRows()
        {
            var usable = daysList.ClientSize.Width - daysList.Padding.Horizontal;
            if (daysList.VerticalScroll.Visible) usable -= SystemInformation.VerticalScrollBarWidth;
            if (usable <= 0) return;
            foreach (Control row in daysList.Controls)
            {
                row.Width = Math.Max(320, usable - row.Margin.Horizontal);
                if (row.Tag is SelectionDayResult rowDay) row.Height = RowHeight(rowDay, next, row.Width);
            }
        }
        daysList.ClientSizeChanged += (_, _) => FitRows();
        Shown += (_, _) => FitRows();
        body.Controls.Add(daysCard, 0, 0);

        // --- il dossier
        var reportCard = UiTheme.Card("DOSSIER DEL TEAM", out var reportInner, UiTheme.Accent);
        reportCard.Dock = DockStyle.Fill;
        var reportText = new RichTextBox
        {
            Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = UiTheme.SurfaceRaised, ForeColor = UiTheme.TextSecondary,
            // Il dossier contiene frasi intere, non solo colonne di numeri:
            // senza il ritorno a capo le righe lunghe finivano fuori dal
            // pannello e servivano barre orizzontali per leggere una spiegazione.
            Font = new Font(UiTheme.FamilyMono, 9.5F), WordWrap = true, ScrollBars = RichTextBoxScrollBars.Vertical
        };
        reportText.Text = BuildDossier(trial, report, next, cash);
        reportInner.Controls.Add(reportText);
        body.Controls.Add(reportCard, 1, 0);

        // --- la barra delle azioni
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 86, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 16, 38, 16) };
        var close = UiTheme.SecondaryButton(trial.AllDaysDone ? "CHIUDI IL DOSSIER" : "TORNA ALLA CARRIERA");
        close.Dock = DockStyle.Right;
        close.Width = TextRenderer.MeasureText(close.Text, close.Font).Width + 44;
        close.DialogResult = DialogResult.Cancel;
        footer.Controls.Add(close);

        if (next != null)
        {
            var assessment = CareerFinances.Assess(cash, next.Day == 1 ? trial.NetCost : 0, 0, 0, 0);
            // L'etichetta contiene il nome della prova, che è lungo: la larghezza
            // viene misurata sul testo invece di essere fissata, altrimenti
            // "GARA DI VALUTAZIONE" veniva troncato con i puntini.
            var runLabel = $"AFFRONTA IL GIORNO {next.Day} · {SelectionDayResult.KindLabel(next.Kind).ToUpperInvariant()}";
            var run = UiTheme.PrimaryButton(runLabel);
            run.Dock = DockStyle.Right;
            run.Width = Math.Min(560, TextRenderer.MeasureText(runLabel, run.Font).Width + 52);
            run.Enabled = assessment.Affordable;
            run.Click += (_, _) => { ChosenDay = next; DialogResult = DialogResult.OK; };
            footer.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 14, BackColor = Color.Transparent });
            footer.Controls.Add(run);
            if (debugAvailable)
            {
                var simulate = UiTheme.SecondaryButton("DEBUG · SIMULA LA PROVA");
                simulate.Dock = DockStyle.Right;
                simulate.Width = TextRenderer.MeasureText(simulate.Text, simulate.Font).Width + 44;
                simulate.ForeColor = UiTheme.Warning;
                simulate.Click += (_, _) => { ChosenDay = next; UseDebug = true; DialogResult = DialogResult.OK; };
                footer.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 14, BackColor = Color.Transparent });
                footer.Controls.Add(simulate);
            }
            if (!assessment.Affordable)
                footer.Controls.Add(new Label
                {
                    Text = $"Non basta la cassa: {assessment.RiskReason}.",
                    Dock = DockStyle.Left, Width = 380, Font = UiTheme.Small,
                    ForeColor = UiTheme.Accent, TextAlign = ContentAlignment.MiddleLeft
                });
        }
        else
        {
            footer.Controls.Add(new Label
            {
                Text = report.Simulated
                    ? "Prove concluse. Il verdetto include prove simulate in debug ed è dichiarato come tale."
                    : "Prove concluse. Il verdetto nasce esclusivamente dai referti reali di Assetto Corsa.",
                Dock = DockStyle.Left, Width = 700, Font = UiTheme.Small,
                ForeColor = report.Simulated ? UiTheme.Warning : UiTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleLeft
            });
        }

        Controls.Add(body); Controls.Add(footer); Controls.Add(header);
        AcceptButton = close;
    }

    /// <summary>
    /// Altezza necessaria a una riga: il verdetto e il briefing sono frasi di
    /// lunghezza variabile, quindi l'altezza va misurata, non fissata.
    /// </summary>
    private static int RowHeight(SelectionDayResult day, SelectionDayResult? next, int width)
    {
        var isNext = next != null && ReferenceEquals(day, next);
        var text = day.Completed ? $"{day.Score}/100 — {day.Verdict}"
            : isNext ? SelectionDayResult.KindBrief(day.Kind) : "";
        // Una prova non ancora svolta ha comunque due righe — intestazione e
        // data — più il padding: con 62 px la seconda restava tagliata a metà.
        if (text.Length == 0) return 78;
        var font = day.Completed ? UiTheme.BodyStrong : UiTheme.Small;
        var measured = TextRenderer.MeasureText(text, font,
            new Size(Math.Max(160, width - 40), int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        // Intestazione, riga di dettaglio e margini interni della riga.
        return Math.Max(day.Completed ? 96 : 104, measured.Height + (day.Completed ? 76 : 62));
    }

    /// <summary>Una riga per giornata: esito se svolta, briefing se è la prossima.</summary>
    private static Panel DayRow(SelectionDayResult day, SelectionDayResult? next)
    {
        var isNext = next != null && ReferenceEquals(day, next);
        var accent = day.Completed
            ? day.Score >= 70 ? UiTheme.Positive : day.Score >= 50 ? UiTheme.Warning : UiTheme.Accent
            : isNext ? UiTheme.Accent : UiTheme.Border;

        var row = new Panel
        {
            // La larghezza reale arriva da FitRows: qui serve solo un valore di
            // partenza credibile per la prima misura.
            Width = 620, Height = RowHeight(day, next, 620), Tag = day,
            Margin = new Padding(0, 0, 0, 10), BackColor = UiTheme.SurfaceRaised, Padding = new Padding(14, 10, 14, 10)
        };
        row.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(accent);
            e.Graphics.FillRectangle(brush, 0, 0, 4, row.Height);
        };

        // L'intestazione va aggiunta per ultima: con Dock=Top l'ordine di
        // inserimento è invertito, e inserendola prima finiva sotto al blocco
        // Fill del verdetto, che la copriva.
        var title = new Label
        {
            Text = $"GIORNO {day.Day} · {SelectionDayResult.KindLabel(day.Kind).ToUpperInvariant()}"
                   + (day.Simulated ? "   [SIMULATO]" : ""),
            Dock = DockStyle.Top, Height = 22, Font = UiTheme.Kicker,
            ForeColor = day.Simulated ? UiTheme.Warning : accent
        };

        if (day.Completed)
        {
            // Inserito prima perché Dock=Top impila al contrario: il dettaglio
            // tecnico resta sotto al verdetto.
            var detail = new List<string>();
            if (day.Laps > 0) detail.Add($"{day.Laps} giri");
            if (day.BestLapMilliseconds > 0) detail.Add($"miglior giro {Lap(day.BestLapMilliseconds)}");
            if (day.Position > 0) detail.Add($"{day.Position}° su {day.FieldSize}");
            if (day.PenaltySeconds > 0) detail.Add($"{day.PenaltySeconds:0.#}s di penalità");
            row.Controls.Add(new Label
            {
                Text = string.Join(" · ", detail), Dock = DockStyle.Bottom, Height = 22,
                Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary, AutoEllipsis = true
            });
            // Il verdetto è una frase: Fill più il ritorno a capo, così non
            // viene tagliato quando spiega perché il punteggio è quello.
            row.Controls.Add(new Label
            {
                Text = $"{day.Score}/100 — {day.Verdict}", Dock = DockStyle.Fill,
                Font = UiTheme.BodyStrong, ForeColor = UiTheme.TextPrimary, UseMnemonic = false
            });
        }
        else if (isNext)
        {
            // Il briefing riempie lo spazio: va inserito prima della data, che
            // deve restare ancorata in alto sotto l'intestazione.
            row.Controls.Add(new Label
            {
                Text = SelectionDayResult.KindBrief(day.Kind), Dock = DockStyle.Fill,
                Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary, UseMnemonic = false
            });
            row.Controls.Add(new Label
            {
                Text = $"{NarrativeCalendar.Format(day.StoryDate)} — la prossima prova",
                Dock = DockStyle.Top, Height = 22, Font = UiTheme.BodyStrong, ForeColor = UiTheme.TextPrimary
            });
        }
        else
        {
            row.Controls.Add(new Label
            {
                Text = $"{NarrativeCalendar.Format(day.StoryDate)} — ancora da svolgere",
                Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted
            });
        }
        row.Controls.Add(title);
        return row;
    }

    private static string BuildDossier(SelectionTrial trial, SelectionReport report, SelectionDayResult? next, int cash)
    {
        var lines = new List<string>();
        if (trial.TrackSubstituted)
        {
            lines.Add("SEDE");
            lines.Add(trial.TrackSubstitutionNote);
            lines.Add("");
        }
        lines.Add("COSA METTE IN GIOCO");
        lines.Add($"Gradino: {CareerLadder.ById(trial.TargetRungId).Name}");
        lines.Add($"Posti disponibili: {trial.Seats} su {trial.Candidates} candidati");
        if (trial.Attempt > 1) lines.Add($"Tentativo numero {trial.Attempt}: la porta era rimasta aperta.");
        lines.Add("");
        lines.Add("ECONOMIA");
        lines.Add(CareerFinances.Assess(cash, next?.Day == 1 ? trial.NetCost : 0, 0, 0, 0).Describe());
        if (trial.CoveredPercent > 0)
            lines.Add($"Copertura di {trial.OrganisedBy}: {trial.CoveredPercent}% (€ {trial.Cost - trial.NetCost:N0})");
        lines.Add("");
        lines.Add(report.Describe());
        if (trial.AllDaysDone)
        {
            lines.Add("");
            lines.Add("VERDETTO");
            lines.Add(report.TeamVerdict);
        }
        return string.Join("\n", lines);
    }

    private static string Lap(int milliseconds)
    {
        var span = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)span.TotalMinutes}:{span.Seconds:00}.{span.Milliseconds:000}";
    }
}
