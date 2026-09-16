using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Una visita allo sponsor deve essere una scelta rapida e leggibile: Haru
/// prepara una sola frase, il giocatore decide che tono darle e la trattativa
/// viene risolta. Il vecchio tavolo a tre domande resta nel modello dati per
/// eventuali sviluppi, ma non rallenta piu' la carriera.
/// </summary>
public sealed class SponsorNegotiationDialog : CareerDialog
{
    private readonly SponsorVisit visita;
    private readonly SponsorTemperamento chi;
    private readonly AgentSkills doti;
    private readonly int probabilitaDiPartenza;
    private int deltaScelta;

    public List<string> Trascrizione { get; } = [];
    public int ProbabilitaFinale => SponsorNegotiation.ProbabilitaFinale(probabilitaDiPartenza, 0, deltaScelta);

    public SponsorNegotiationDialog(SponsorVisit visita, CareerState carriera)
    {
        this.visita = visita;
        doti = carriera.Agent ?? new AgentSkills();
        chi = SponsorNegotiation.TemperamentoDi(visita.Trade);
        probabilitaDiPartenza = visita.Chance;

        Text = $"CorsaCareer — {visita.Target}";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;
        ClientSize = new Size(940, 510);
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(Corpo(carriera));
        Controls.Add(Testata());
    }

    private Control Testata()
    {
        var testata = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = UiTheme.HeaderBackground, Padding = new Padding(34, 16, 34, 12) };
        testata.Controls.Add(new Label
        {
            Text = "Una sola cosa da dire. Scegli il messaggio che Haru porta al tavolo.",
            Dock = DockStyle.Bottom, Height = 42, Font = UiTheme.Prose, ForeColor = UiTheme.TextSecondary
        });
        testata.Controls.Add(new Label
        {
            Text = visita.Target.ToUpperInvariant(), Dock = DockStyle.Top, Height = 34,
            Font = UiTheme.Display, ForeColor = UiTheme.TextPrimary, AutoEllipsis = true
        });
        testata.Controls.Add(new Label
        {
            Text = $"COSA DIRE · {SponsorNegotiation.NomeTemperamento(chi).ToUpperInvariant()}",
            Dock = DockStyle.Top, Height = 22, Font = UiTheme.Kicker, ForeColor = UiTheme.Accent
        });
        return testata;
    }

    private Control Corpo(CareerState carriera)
    {
        var corpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(34, 22, 34, 26), BackColor = UiTheme.Background };
        var scambio = SponsorNegotiation.Scambi(visita, carriera).First();
        var domanda = new Label
        {
            Text = scambio.Domanda, Dock = DockStyle.Top, Height = 64,
            Font = new Font(UiTheme.FamilySerif, 15F, FontStyle.Italic), ForeColor = UiTheme.TextPrimary
        };
        var scelte = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = scambio.Mosse.Count, RowCount = 1, BackColor = UiTheme.Background
        };
        for (var i = 0; i < scambio.Mosse.Count; i++)
        {
            scelte.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / scambio.Mosse.Count));
            scelte.Controls.Add(Bottone(scambio.Mosse[i]), i, 0);
        }
        corpo.Controls.Add(scelte);
        corpo.Controls.Add(domanda);
        return corpo;
    }

    private Control Bottone(SponsorMossa mossa)
    {
        var pannello = new Panel
        {
            Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Margin = new Padding(0, 0, 14, 0),
            Cursor = Cursors.Hand, Padding = new Padding(18, 16, 18, 16)
        };
        var titolo = new Label
        {
            Text = mossa.Etichetta.ToUpperInvariant(), Dock = DockStyle.Top, Height = 26,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Accent, BackColor = Color.Transparent
        };
        var testo = new Label
        {
            Text = mossa.Battuta, Dock = DockStyle.Fill, Font = new Font(UiTheme.FamilySerif, 11.5F),
            ForeColor = UiTheme.TextSecondary, BackColor = Color.Transparent
        };
        var invito = new Label
        {
            Text = "SCEGLI QUESTA FRASE", Dock = DockStyle.Bottom, Height = 22, Font = UiTheme.Kicker,
            ForeColor = UiTheme.TextMuted, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleRight
        };
        pannello.Controls.Add(testo); pannello.Controls.Add(invito); pannello.Controls.Add(titolo);
        void Evidenzia(bool acceso)
        {
            pannello.BackColor = acceso ? UiTheme.SurfaceRaised : UiTheme.Surface;
            testo.ForeColor = acceso ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            invito.ForeColor = acceso ? UiTheme.Accent : UiTheme.TextMuted;
        }
        void Scegli(object? _, EventArgs __) => Applica(mossa);
        foreach (Control c in new Control[] { pannello, titolo, testo, invito })
        {
            c.Click += Scegli;
            c.MouseEnter += (_, _) => Evidenzia(true);
            c.MouseLeave += (_, _) => Evidenzia(false);
        }
        return pannello;
    }

    private void Applica(SponsorMossa mossa)
    {
        var esito = SponsorNegotiation.Valuta(mossa, chi, doti);
        deltaScelta = esito.Delta;
        Trascrizione.Add($"Haru: {esito.Battuta}");
        Trascrizione.Add($"{visita.Target}: {esito.Reazione}");
        DialogResult = DialogResult.OK;
        Close();
    }
}
