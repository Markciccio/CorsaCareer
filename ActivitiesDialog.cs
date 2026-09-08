using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Agenda fra due weekend: ogni voce dichiara costo, giorni, rischio e
/// conseguenze prima della scelta. Nessuna di queste attività può cambiare un
/// risultato di gara, e la finestra lo dichiara esplicitamente.
/// </summary>
public sealed class ActivitiesDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly Func<ActivityDefinition, ActivityOption?, ActivityOutcome> perform;
    private readonly Action<ActivityRecord> openArticle;
    private readonly Action? openSponsorSearch;
    private readonly Action<ActivityRecord>? openAnime;
    private readonly ListBox choices = new();
    private readonly ListView list = new();
    private readonly Label header = new();
    private readonly TextBox detail = new();
    private readonly TextBox journal = new();
    private readonly Label question = new();

    public ActivitiesDialog(CareerState career, Func<ActivityDefinition, ActivityOption?, ActivityOutcome> perform,
        Action<ActivityRecord> openArticle, Action? openSponsorSearch = null,
        Action<ActivityRecord>? openAnime = null)
    {
        this.career = career;
        this.perform = perform;
        this.openArticle = openArticle;
        this.openSponsorSearch = openSponsorSearch;
        this.openAnime = openAnime;
        Text = "CorsaCareer — agenda fra i weekend";
        ClientSize = new Size(1040, 700);
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(18, 21, 28);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        var top = new Panel { Dock = DockStyle.Top, Height = 118, BackColor = Color.FromArgb(27, 31, 41), Padding = new Padding(30, 0, 30, 0) };
        top.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(220, 25, 45), 4); e.Graphics.DrawLine(pen, 0, 0, top.Width, 0); };
        top.Controls.Add(new Label { Text = "AGENDA DEL PILOTA", Left = 30, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 22, FontStyle.Bold) });
        header.Left = 33; header.Top = 66; header.Width = 960; header.Height = 44; header.ForeColor = Color.FromArgb(190, 196, 207);
        top.Controls.Add(header);
        Controls.Add(top);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(18, 21, 28), Padding = new Padding(30, 10, 30, 10) };
        // Cercare finanziatori e' un'attivita fra i weekend come le altre, e
        // consuma gli stessi giorni: sta qui, non in una voce di menu a parte.
        if (openSponsorSearch != null)
        {
            var sponsorButton = UiTheme.SecondaryButton("CERCA SPONSOR");
            sponsorButton.Dock = DockStyle.Left;
            sponsorButton.Width = 260;
            sponsorButton.Click += (_, _) => { openSponsorSearch(); Close(); };
            bottom.Controls.Add(sponsorButton);
        }
        var close = new Button { Text = "Torna al portale", Dock = DockStyle.Right, Width = 190, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, DialogResult = DialogResult.OK };
        var run = new Button { Text = "SVOLGI L'ATTIVITÀ", Dock = DockStyle.Right, Width = 220, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(190, 22, 55), ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 10), Margin = new Padding(0, 0, 12, 0) };
        run.Click += (_, _) => Perform();
        bottom.Controls.Add(close); bottom.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 12 }); bottom.Controls.Add(run);
        Controls.Add(bottom);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(30, 16, 30, 8), BackColor = Color.FromArgb(18, 21, 28) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        Controls.Remove(top);
        Controls.Remove(bottom);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.FromArgb(18, 21, 28), Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.Controls.Add(top, 0, 0);
        root.Controls.Add(body, 0, 1);
        root.Controls.Add(bottom, 0, 2);
        Controls.Add(root);

        list.Dock = DockStyle.Fill;
        list.View = View.Details;
        list.FullRowSelect = true;
        list.MultiSelect = false;
        list.HideSelection = false;
        list.BackColor = Color.FromArgb(27, 31, 41);
        list.ForeColor = Color.Gainsboro;
        list.BorderStyle = BorderStyle.FixedSingle;
        list.Columns.Add("Attività", 250);
        list.Columns.Add("Area", 110);
        list.Columns.Add("Giorni", 60, HorizontalAlignment.Right);
        list.Columns.Add("Costo", 110, HorizontalAlignment.Right);
        list.Columns.Add("Rischio", 70, HorizontalAlignment.Right);
        list.SelectedIndexChanged += (_, _) => ShowDetail();
        list.DoubleClick += (_, _) => Perform();
        body.Controls.Add(list, 0, 0);

        // Colonna destra: la scheda dell'attività sopra, la decisione sotto.
        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.FromArgb(18, 21, 28) };
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        detail.Dock = DockStyle.Fill;
        detail.Multiline = true; detail.ReadOnly = true; detail.ScrollBars = ScrollBars.Vertical;
        detail.BorderStyle = BorderStyle.FixedSingle;
        detail.BackColor = Color.FromArgb(27, 31, 41); detail.ForeColor = Color.Gainsboro;
        right.Controls.Add(detail, 0, 0);
        question.Dock = DockStyle.Fill;
        question.ForeColor = Color.FromArgb(245, 190, 65);
        question.Font = new Font("Segoe UI Semibold", 9);
        right.Controls.Add(question, 0, 1);
        choices.Dock = DockStyle.Fill;
        choices.BackColor = Color.FromArgb(35, 40, 52);
        choices.ForeColor = Color.White;
        choices.BorderStyle = BorderStyle.FixedSingle;
        choices.SelectedIndexChanged += (_, _) => ShowChoiceDescription();
        choices.DoubleClick += (_, _) => Perform();
        right.Controls.Add(choices, 0, 2);
        body.Controls.Add(right, 1, 0);

        journal.Dock = DockStyle.Fill;
        journal.Multiline = true; journal.ReadOnly = true; journal.ScrollBars = ScrollBars.Vertical;
        journal.BorderStyle = BorderStyle.FixedSingle;
        journal.BackColor = Color.FromArgb(22, 25, 33); journal.ForeColor = Color.FromArgb(170, 178, 190);
        journal.Cursor = Cursors.Hand;
        // Doppio clic su una riga del diario riapre il servizio corrispondente.
        journal.DoubleClick += (_, _) => OpenJournalArticle();
        body.SetColumnSpan(journal, 2);
        body.Controls.Add(journal, 0, 1);

        Populate();
        AcceptButton = close;
    }

    private void Populate()
    {
        var selectedId = list.SelectedItems.Count > 0 ? list.SelectedItems[0].Tag as string : null;
        list.BeginUpdate();
        list.Items.Clear();
        foreach (var activity in OffTrackActivities.Catalog(career.Tier))
        {
            var item = new ListViewItem(activity.Name) { Tag = activity.Id };
            item.SubItems.Add(activity.Category);
            item.SubItems.Add(activity.DurationDays.ToString());
            item.SubItems.Add(activity.CostLabel);
            item.SubItems.Add($"{activity.Risk}%");
            var affordable = activity.Cost <= 0 || career.Cash >= activity.Cost;
            var enoughDays = career.DaysUntilNextRound >= activity.DurationDays;
            if (!affordable || !enoughDays) item.ForeColor = Color.FromArgb(120, 126, 138);
            list.Items.Add(item);
        }
        list.EndUpdate();
        if (list.Items.Count > 0)
        {
            var restored = list.Items.Cast<ListViewItem>().FirstOrDefault(x => (x.Tag as string) == selectedId);
            (restored ?? list.Items[0]).Selected = true;
            list.Select();
        }
        RefreshHeader();
        RefreshJournal();
        ShowDetail();
    }

    private void RefreshHeader() => header.Text =
        $"Stagione {career.Season} · verso il round {Math.Min(career.Round + 1, 99)} · {career.DaysUntilNextRound} giorni liberi\n" +
        $"Budget € {career.Cash:N0} · reputazione {career.Reputation}/100 · seguito {career.Fanbase} · condizione: {OffTrackActivities.FatigueLabel(career.Fatigue)} ({career.Fatigue}/100) · " +
        $"team {OffTrackActivities.RelationLabel(career.TeamRelation)} ({career.TeamRelation}) · sponsor {OffTrackActivities.RelationLabel(career.SponsorRelation)} ({career.SponsorRelation})";

    private void RefreshJournal()
    {
        var recent = career.ActivityHistory.TakeLast(12).Reverse().ToList();
        journal.Text = recent.Count == 0
            ? "DIARIO DELLE ATTIVITÀ\r\nNessuna attività svolta: l'agenda fra i weekend è ancora vuota."
            : "DIARIO DELLE ATTIVITÀ\r\n" + string.Join("\r\n", recent.Select(x =>
                $"S{x.Season:00} R{x.Round:00} · {x.Name} · {(x.Success ? "riuscita" : "non riuscita")} · {x.Story} " +
                $"[rep {x.Reputation:+#;-#;0} · seguito {x.Fanbase:+#;-#;0} · team {x.TeamRelation:+#;-#;0} · sponsor {x.SponsorRelation:+#;-#;0} · condizione {x.Fatigue:+#;-#;0} · € {x.Money:N0}]"));
    }

    /// <summary>Riapre il servizio della riga del diario su cui si fa doppio clic.</summary>
    private void OpenJournalArticle()
    {
        var recent = career.ActivityHistory.TakeLast(12).Reverse().ToList();
        if (recent.Count == 0) return;
        var line = journal.GetLineFromCharIndex(journal.SelectionStart) - 1;
        if (line < 0 || line >= recent.Count) return;
        openArticle(recent[line]);
    }

    private ActivityDefinition? SelectedActivity() =>
        list.SelectedItems.Count == 0 ? null : OffTrackActivities.Find(list.SelectedItems[0].Tag as string ?? "");

    private void ShowDetail()
    {
        var activity = SelectedActivity();
        if (activity == null) { detail.Text = ""; return; }
        var timesDone = career.ActivityHistory.Count(x => x.Season == career.Season && x.ActivityId == activity.Id);
        detail.Text = string.Join("\r\n", new[]
        {
            activity.Name.ToUpperInvariant(),
            activity.Category,
            "",
            activity.Description,
            "",
            $"Durata: {activity.DurationDays} giorni sui {career.DaysUntilNextRound} disponibili",
            $"Costo: {activity.CostLabel}",
            $"Rischio dichiarato: {activity.Risk}%",
            timesDone > 0 ? $"Già svolta {timesDone} volta/e in questa stagione: il ritorno sarà minore." : "Non ancora svolta in questa stagione.",
            "",
            "SE RIESCE",
            Describe(activity.Success),
            activity.SuccessStory,
            "",
            "SE NON RIESCE",
            Describe(activity.Setback),
            activity.SetbackStory,
            "",
            "Nessuna attività può modificare un risultato di gara: i risultati arrivano soltanto da Assetto Corsa."
        });
        RefreshChoices(activity);
    }

    /// <summary>
    /// Popola la decisione dell'attività. Le attività senza scelte restano
    /// eseguibili direttamente; quelle con scelte richiedono di sceglierne una.
    /// </summary>
    private void RefreshChoices(ActivityDefinition activity)
    {
        choices.BeginUpdate();
        choices.Items.Clear();
        foreach (var option in activity.Choices)
            choices.Items.Add($"{option.Label}   ·   rischio {(option.RiskDelta >= 0 ? "+" : "")}{option.RiskDelta}");
        choices.EndUpdate();
        choices.Enabled = activity.RequiresDecision;
        question.Text = activity.RequiresDecision
            ? activity.Question
            : "Questa attività non richiede una decisione.";
        if (choices.Items.Count > 0) choices.SelectedIndex = 0;
    }

    private ActivityOption? SelectedChoice()
    {
        var activity = SelectedActivity();
        if (activity == null || !activity.RequiresDecision) return null;
        var index = choices.SelectedIndex;
        return index >= 0 && index < activity.Choices.Count ? activity.Choices[index] : null;
    }

    private void ShowChoiceDescription()
    {
        var option = SelectedChoice();
        if (option == null) return;
        question.Text = $"{option.Label}: {option.Description}";
    }

    private static string Describe(ActivityEffect effect)
    {
        var parts = new List<string>();
        if (effect.Reputation != 0) parts.Add($"reputazione {effect.Reputation:+#;-#;0}");
        if (effect.Fanbase != 0) parts.Add($"seguito {effect.Fanbase:+#;-#;0}");
        if (effect.TeamRelation != 0) parts.Add($"rapporto col team {effect.TeamRelation:+#;-#;0}");
        if (effect.SponsorRelation != 0) parts.Add($"rapporto sponsor {effect.SponsorRelation:+#;-#;0}");
        if (effect.Fatigue != 0) parts.Add($"stanchezza {effect.Fatigue:+#;-#;0}");
        if (effect.Money != 0) parts.Add($"compenso € {effect.Money:N0}");
        return parts.Count == 0 ? "nessun effetto" : string.Join(" · ", parts);
    }

    private void Perform()
    {
        var activity = SelectedActivity();
        if (activity == null) return;
        var choice = SelectedChoice();
        if (activity.RequiresDecision && choice == null)
        {
            MessageBox.Show($"{activity.Name} richiede una decisione.\n\n{activity.Question}", "Agenda del pilota", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var outcome = perform(activity, choice);
        if (!outcome.Performed)
        {
            MessageBox.Show(outcome.Reason, "Agenda non disponibile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var record = career.ActivityHistory.LastOrDefault();
        // L'attività è un momento narrativo, non una conferma tecnica: si apre
        // subito la scena anime contestuale e, alla chiusura, si torna all'elenco
        // aggiornato. Il servizio resta consultabile dal diario con doppio clic.
        if (record != null && openAnime != null &&
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CORSACAREER_NO_ANIM")))
            openAnime(record);
        Populate();
    }
}
