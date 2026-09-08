using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// La ricerca di sponsor.
///
/// Prende il posto della schermata «Opportunità», che metteva nello stesso
/// elenco gare, test e proposte commerciali. Era la causa di un difetto grave:
/// le gare esistevano in due posti — qui e nel calendario — e il giocatore ne
/// vedeva uno solo, con la carriera che sembrava bloccata.
///
/// Le gare vivono nel calendario, perché la carriera è una storia sola. Qui
/// resta l'unica cosa che è davvero parallela al correre: trovare chi paga.
/// </summary>
public sealed class SponsorSearchDialog : CareerDialog
{
    private readonly FlowLayoutPanel list = new();
    private readonly CareerState career;
    private readonly Func<Opportunity, bool> accept;
    private readonly Action<Opportunity> decline;

    public SponsorSearchDialog(CareerState career, Func<Opportunity, bool> accept, Action<Opportunity> decline)
    {
        this.career = career;
        this.accept = accept;
        this.decline = decline;

        Text = "CorsaCareer — ricerca sponsor";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 16, 40, 12) };
        header.Controls.Add(new Label
        {
            Text = "RICERCA SPONSOR", Dock = DockStyle.Top, Height = 20,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Warning, UseMnemonic = false
        });
        header.Controls.Add(new Label
        {
            Text = "Chi è disposto a metterci dei soldi", Dock = DockStyle.Top, Height = 30,
            Font = UiTheme.HeadlineSmall, ForeColor = UiTheme.TextPrimary, UseMnemonic = false
        });
        header.Controls.Add(new Label
        {
            Text = "Le gare stanno nel calendario. Qui si cercano soltanto i soldi per correrle.",
            Dock = DockStyle.Top, Height = 22, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted, UseMnemonic = false
        });

        list.Dock = DockStyle.Fill;
        list.FlowDirection = FlowDirection.TopDown;
        list.WrapContents = false;
        list.AutoScroll = true;
        list.BackColor = UiTheme.Background;
        list.Padding = new Padding(40, 20, 30, 20);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 14, 40, 16) };
        var close = UiTheme.SecondaryButton("CHIUDI");
        close.Dock = DockStyle.Right;
        close.Width = 220;
        close.Click += (_, _) => Close();
        footer.Controls.Add(close);
        CancelButton = close;

        Controls.Add(footer);
        Controls.Add(list);
        Controls.Add(header);

        Rebuild();
    }

    /// <summary>
    /// Ricostruisce l'elenco. Compaiono solo le proposte commerciali: una gara
    /// in questo elenco sarebbe di nuovo il difetto di prima.
    /// </summary>
    private void Rebuild()
    {
        list.SuspendLayout();
        foreach (Control existing in list.Controls) existing.Dispose();
        list.Controls.Clear();

        var width = Math.Max(520, list.ClientSize.Width - 90);
        var commercial = (career.Opportunities ?? [])
            .Where(x => x.IsOpen && IsCommercial(x.Kind))
            .OrderByDescending(x => x.LikelyReturn)
            .ToList();

        if (commercial.Count == 0)
        {
            list.Controls.Add(Riga(
                "Nessuno, per adesso.",
                UiTheme.HeadlineSmall, UiTheme.TextSecondary, width, 0, 8));
            list.Controls.Add(Riga(
                "Gli sponsor arrivano quando c'è qualcosa da mostrare: risultati, continuità, un nome che "
                + "qualcuno ha già sentito. Corri, e questa pagina si riempie da sola.",
                UiTheme.Prose, UiTheme.TextMuted, width, 0, 0));
            list.ResumeLayout();
            return;
        }

        foreach (var offer in commercial) list.Controls.Add(BuildCard(offer, width));
        list.ResumeLayout();
    }

    /// <summary>
    /// Vero per le proposte che portano denaro senza essere una sessione in
    /// pista. Sono le sole che hanno senso qui.
    /// </summary>
    public static bool IsCommercial(OpportunityKind kind) =>
        kind is OpportunityKind.SponsorDeal or OpportunityKind.PromotionalEvent;

    private Control BuildCard(Opportunity offer, int width)
    {
        var card = new Panel
        {
            Width = width, BackColor = UiTheme.SurfaceRaised,
            Padding = new Padding(20, 16, 20, 16), Margin = new Padding(0, 0, 0, 16)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var accent = new SolidBrush(UiTheme.Positive);
            e.Graphics.FillRectangle(accent, 0, 0, 3, card.Height);
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent
        };
        var inner = width - 48;

        flow.Controls.Add(Riga(Opportunity.KindLabel(offer.Kind).ToUpperInvariant(),
            UiTheme.Kicker, UiTheme.Positive, inner, 0, 4));
        flow.Controls.Add(Riga(offer.Title, UiTheme.HeadlineSmall, UiTheme.TextPrimary, inner, 0, 6));
        flow.Controls.Add(Riga(offer.Justification, UiTheme.Prose, UiTheme.TextSecondary, inner, 0, 8));

        // Quanto porta, e quanto costa. Una proposta commerciale senza cifre non
        // è una decisione.
        var money = offer.NetCost > 0
            ? $"Porta € {offer.LikelyReturn:N0}  ·  costa € {offer.NetCost:N0}"
            : $"Porta € {offer.LikelyReturn:N0}  ·  nessun costo";
        flow.Controls.Add(Riga(money, UiTheme.BodyStrong, UiTheme.TextPrimary, inner, 0, 4));
        flow.Controls.Add(Riga($"Scade il {NarrativeCalendar.Format(offer.Deadline)}",
            UiTheme.Small, UiTheme.TextMuted, inner, 0, 10));

        var buttons = new Panel { Width = inner, Height = 40, BackColor = Color.Transparent };
        var sign = UiTheme.PrimaryButton("ACCETTA");
        sign.Width = 220;
        sign.Left = 0;
        sign.Click += (_, _) => { if (accept(offer)) Rebuild(); };
        var refuse = UiTheme.SecondaryButton("RIFIUTA");
        refuse.Width = 180;
        refuse.Left = 234;
        refuse.Click += (_, _) => { decline(offer); Rebuild(); };
        buttons.Controls.Add(sign);
        buttons.Controls.Add(refuse);
        flow.Controls.Add(buttons);

        card.Controls.Add(flow);
        card.Height = flow.PreferredSize.Height + card.Padding.Vertical;
        return card;
    }

    private static Label Riga(string text, Font font, Color color, int width, int sopra, int sotto)
    {
        var label = new Label
        {
            Text = text, Font = font, ForeColor = color, AutoSize = false, Width = width,
            BackColor = Color.Transparent, UseMnemonic = false, Margin = new Padding(0, sopra, 0, sotto)
        };
        using var g = label.CreateGraphics();
        label.Height = (int)Math.Ceiling(g.MeasureString(text, font, width).Height) + 2;
        return label;
    }
}
