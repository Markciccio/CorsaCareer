using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Le opportunità aperte, con i numeri della decisione in evidenza: patrimonio,
/// costo, guadagno possibile e rischio. Nessuna proposta può essere accettata
/// senza che il quadro economico sia stato mostrato.
/// </summary>
public sealed class OpportunityDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly Func<Opportunity, bool> accept;
    private readonly Action<Opportunity> decline;
    private readonly Func<Opportunity, bool>? filter;
    private readonly ListView list = new();
    private readonly TextBox detail = new();
    private readonly Label header = new();
    private readonly Button acceptButton;
    private readonly Button declineButton;

    public OpportunityDialog(CareerState career, Func<Opportunity, bool> accept, Action<Opportunity> decline, Func<Opportunity, bool>? filter = null)
    {
        this.career = career;
        this.accept = accept;
        this.decline = decline;
        this.filter = filter;

        Text = "CorsaCareer — opportunità";
        ClientSize = new Size(1080, 700);
        MinimumSize = new Size(940, 620);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var top = new Panel { Dock = DockStyle.Top, Height = 108, BackColor = UiTheme.HeaderBackground, Padding = new Padding(28, 0, 28, 0) };
        top.Paint += (_, e) => { using var pen = new Pen(UiTheme.Accent, 4); e.Graphics.DrawLine(pen, 0, 0, top.Width, 0); };
        top.Controls.Add(new Label { Text = "OPPORTUNITÀ", Left = 28, Top = 18, AutoSize = true, Font = UiTheme.Display });
        header.Left = 30; header.Top = 60; header.Width = 1000; header.Height = 42; header.ForeColor = UiTheme.TextSecondary; header.Font = UiTheme.Small;
        top.Controls.Add(header);
        Controls.Add(top);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = UiTheme.Background, Padding = new Padding(28, 10, 28, 10) };
        var close = new Button { Text = "Chiudi", Dock = DockStyle.Right, Width = 150, FlatStyle = FlatStyle.Flat, BackColor = UiTheme.SurfaceRaised, ForeColor = UiTheme.TextPrimary, DialogResult = DialogResult.OK };
        declineButton = new Button { Text = "RIFIUTA", Dock = DockStyle.Right, Width = 170, FlatStyle = FlatStyle.Flat, BackColor = UiTheme.SurfaceRaised, ForeColor = UiTheme.TextPrimary };
        acceptButton = new Button { Text = "ACCETTA", Dock = DockStyle.Right, Width = 200, FlatStyle = FlatStyle.Flat, BackColor = UiTheme.Accent, ForeColor = Color.White, Font = UiTheme.BodyStrong };
        acceptButton.Click += (_, _) => Accept();
        declineButton.Click += (_, _) => Decline();
        bottom.Controls.Add(close);
        bottom.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 10 });
        bottom.Controls.Add(declineButton);
        bottom.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 10 });
        bottom.Controls.Add(acceptButton);
        Controls.Add(bottom);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(28, 14, 28, 8), BackColor = UiTheme.Background };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        Controls.Remove(top);
        Controls.Remove(bottom);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = UiTheme.Background, Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.Controls.Add(top, 0, 0);
        root.Controls.Add(body, 0, 1);
        root.Controls.Add(bottom, 0, 2);
        Controls.Add(root);

        list.Dock = DockStyle.Fill;
        list.View = View.Details;
        list.FullRowSelect = true;
        list.MultiSelect = false;
        list.HideSelection = false;
        list.BackColor = UiTheme.Surface;
        list.ForeColor = UiTheme.TextSecondary;
        list.BorderStyle = BorderStyle.FixedSingle;
        list.Columns.Add("Opportunità", 250);
        list.Columns.Add("Categoria", 120);
        list.Columns.Add("Costo", 100, HorizontalAlignment.Right);
        list.Columns.Add("Rischio", 110);
        list.SelectedIndexChanged += (_, _) => ShowDetail();
        body.Controls.Add(list, 0, 0);

        detail.Dock = DockStyle.Fill;
        detail.Multiline = true; detail.ReadOnly = true; detail.ScrollBars = ScrollBars.Vertical;
        detail.BorderStyle = BorderStyle.FixedSingle;
        detail.BackColor = UiTheme.Surface; detail.ForeColor = UiTheme.TextPrimary;
        detail.Font = UiTheme.Mono;
        body.Controls.Add(detail, 1, 0);

        Populate();
        AcceptButton = close;
    }

    private void Populate()
    {
        var selectedId = list.SelectedItems.Count > 0 ? list.SelectedItems[0].Tag as string : null;
        list.BeginUpdate();
        list.Items.Clear();
        foreach (var opportunity in Open())
        {
            var assessment = opportunity.Assess(career.Cash);
            var item = new ListViewItem(opportunity.Title) { Tag = opportunity.Id };
            item.SubItems.Add(opportunity.Tier);
            item.SubItems.Add(opportunity.NetCost == 0 ? "nessuno" : $"€ {opportunity.NetCost:N0}");
            item.SubItems.Add(CareerFinances.Label(assessment.Risk));
            item.ForeColor = assessment.Risk switch
            {
                FinancialRisk.Unaffordable => UiTheme.TextMuted,
                FinancialRisk.Prohibitive => UiTheme.Accent,
                FinancialRisk.High => UiTheme.Warning,
                _ => UiTheme.TextSecondary
            };
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
        ShowDetail();
    }

    private List<Opportunity> Open() => (career.Opportunities ?? [])
        .Where(x => x.IsOpen)
        .Where(x => filter?.Invoke(x) ?? true)
        .OrderBy(x => x.Date)
        .ToList();

    private void RefreshHeader()
    {
        var openPromises = (career.Promises ?? []).Count(x => x.IsOpen);
        header.Text =
            $"Patrimonio € {career.Cash:N0} · {CareerFinances.Status(career.Cash)}\n" +
            $"{career.ReputationProfile?.Describe() ?? ""}\n" +
            $"{Open().Count} proposte aperte · {openPromises} promesse in attesa di verifica";
    }

    private Opportunity? Selected() =>
        list.SelectedItems.Count == 0 ? null : Open().FirstOrDefault(x => x.Id == (list.SelectedItems[0].Tag as string));

    private void ShowDetail()
    {
        var opportunity = Selected();
        if (opportunity == null)
        {
            detail.Text = "Nessuna proposta aperta.\r\n\r\nLe opportunità nascono dai risultati: reputazione, denaro e rapporti decidono che cosa il paddock è disposto a offrire.";
            acceptButton.Enabled = false;
            declineButton.Enabled = false;
            return;
        }
        detail.Text = opportunity.Describe(career.Cash).Replace("\n", "\r\n");
        var assessment = opportunity.Assess(career.Cash);
        acceptButton.Enabled = assessment.Affordable;
        acceptButton.Text = assessment.Affordable ? "ACCETTA" : "NON SOSTENIBILE";
        declineButton.Enabled = true;
    }

    private void Accept()
    {
        var opportunity = Selected();
        if (opportunity == null) return;
        var assessment = opportunity.Assess(career.Cash);
        var confirm = MessageBox.Show(
            $"{opportunity.Title}\n\n{assessment.Describe()}\n\nConfermi?",
            "Conferma la decisione", MessageBoxButtons.YesNo,
            assessment.Risk is FinancialRisk.High or FinancialRisk.Prohibitive ? MessageBoxIcon.Warning : MessageBoxIcon.Question,
            assessment.Risk is FinancialRisk.High or FinancialRisk.Prohibitive ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1);
        if (confirm != DialogResult.Yes) return;
        if (!accept(opportunity)) return;
        // L'accettazione è una decisione di carriera, non un semplice refresh
        // dell'elenco. Restare nella finestra con la proposta rimossa faceva
        // sembrare che il pulsante non avesse prodotto nulla: il giocatore deve
        // tornare alla Home, dove «ADESSO» mostra il weekend appena generato e
        // permette di aprire briefing, analisi economica e gara.
        DialogResult = DialogResult.OK;
        Close();
    }

    private void Decline()
    {
        var opportunity = Selected();
        if (opportunity == null) return;
        var penalty = opportunity.Kind is OpportunityKind.ProfessionalSeat or OpportunityKind.SubstituteDrive or OpportunityKind.PartiallyFundedSeat or OpportunityKind.FundedTest;
        var confirm = MessageBox.Show(
            $"Rifiutare «{opportunity.Title}»?\n\n" +
            (penalty
                ? $"{opportunity.ProposedBy} registrerà il rifiuto e la fiducia dei team calerà: una proposta finanziata non si rifiuta senza conseguenze."
                : "Nessuna conseguenza sui rapporti: è una proposta a pagamento senza impegni."),
            "Rifiuta la proposta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;
        decline(opportunity);
        Populate();
    }
}
