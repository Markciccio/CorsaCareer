using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Il tavolo della trattativa.
///
/// Tre domande, tre risposte possibili ciascuna, e un indicatore che dice come
/// sta andando mentre va. L'indicatore è la parte importante: senza, il
/// giocatore sceglierebbe al buio e scoprirebbe solo alla fine di aver detto la
/// cosa sbagliata, che è esattamente il difetto che questa schermata esiste per
/// togliere.
///
/// Chi hai davanti è dichiarato prima di cominciare. Non è un indovinello:
/// è una lettura della persona, e la lettura è metà del mestiere di Haru.
/// </summary>
public sealed class SponsorNegotiationDialog : CareerDialog
{
    private readonly SponsorVisit visita;
    private readonly SponsorTemperamento chi;
    private readonly AgentSkills doti;
    private readonly IReadOnlyList<SponsorScambio> scambi;
    private readonly int probabilitaDiPartenza;
    private readonly int modificatoreViaggio;

    private readonly Panel corpo = new() { Dock = DockStyle.Fill, Padding = new Padding(34, 18, 34, 12), BackColor = UiTheme.Background };
    private readonly Label barra = new();
    private readonly Label statoTesto = new();

    private int indice;
    private int somma;

    /// <summary>Le battute scambiate, per il resoconto finale.</summary>
    public List<string> Trascrizione { get; } = [];

    /// <summary>La probabilità dopo la trattativa.</summary>
    public int ProbabilitaFinale => SponsorNegotiation.ProbabilitaFinale(probabilitaDiPartenza, modificatoreViaggio, somma);

    public SponsorNegotiationDialog(SponsorVisit visita, CareerState carriera, int modificatoreViaggio)
    {
        this.visita = visita;
        this.modificatoreViaggio = modificatoreViaggio;
        doti = carriera.Agent ?? new AgentSkills();
        chi = SponsorNegotiation.TemperamentoDi(visita.Trade);
        scambi = SponsorNegotiation.Scambi(visita, carriera);
        probabilitaDiPartenza = visita.Chance;

        Text = $"CorsaCareer — trattativa con {visita.Target}";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;
        ClientSize = new Size(940, 640);
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(corpo);
        Controls.Add(Piede());
        Controls.Add(Testata());

        MostraScambio();
    }

    private Panel Testata()
    {
        var testata = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = UiTheme.HeaderBackground, Padding = new Padding(34, 16, 34, 12) };
        testata.Controls.Add(new Label
        {
            Text = SponsorNegotiation.ComeSiPresenta(chi),
            Dock = DockStyle.Bottom, Height = 54, Font = UiTheme.Prose, ForeColor = UiTheme.TextSecondary
        });
        testata.Controls.Add(new Label
        {
            Text = $"{visita.Target.ToUpperInvariant()}  ·  {visita.Trade}",
            Dock = DockStyle.Top, Height = 34, Font = UiTheme.Display, ForeColor = UiTheme.TextPrimary, AutoEllipsis = true
        });
        testata.Controls.Add(new Label
        {
            Text = $"TRATTATIVA  ·  HAI DAVANTI {SponsorNegotiation.NomeTemperamento(chi).ToUpperInvariant()}",
            Dock = DockStyle.Top, Height = 22, Font = UiTheme.Kicker, ForeColor = UiTheme.Accent
        });
        return testata;
    }

    private Panel Piede()
    {
        var piede = new Panel { Dock = DockStyle.Bottom, Height = 82, BackColor = UiTheme.HeaderBackground, Padding = new Padding(34, 14, 34, 14) };

        barra.Dock = DockStyle.Top;
        barra.Height = 20;
        barra.Font = new Font(UiTheme.FamilyMono, 11F, FontStyle.Bold);
        barra.ForeColor = UiTheme.Positive;

        statoTesto.Dock = DockStyle.Bottom;
        statoTesto.Height = 34;
        statoTesto.Font = UiTheme.Body;
        statoTesto.ForeColor = UiTheme.TextSecondary;

        piede.Controls.Add(statoTesto);
        piede.Controls.Add(barra);
        AggiornaIndicatore();
        return piede;
    }

    /// <summary>
    /// L'indicatore: una barra a blocchi invece di una percentuale nuda,
    /// perché durante una conversazione non si conoscono le probabilità
    /// esatte — si capisce se sta andando bene o male.
    /// </summary>
    private void AggiornaIndicatore()
    {
        var p = ProbabilitaFinale;
        var pieni = Math.Clamp(p / 5, 0, 20);
        barra.Text = new string('█', pieni) + new string('·', 20 - pieni) + $"   {p}%";
        barra.ForeColor = p >= 60 ? UiTheme.Positive : p >= 32 ? UiTheme.Warning : UiTheme.Accent;

        var viaggio = modificatoreViaggio switch
        {
            > 0 => "siete arrivati puntuali",
            0 => "il viaggio non ha inciso",
            > -12 => "siete arrivati un po' tardi",
            _ => "siete arrivati parecchio in ritardo"
        };
        statoTesto.Text = $"Probabilità di chiudere: {p}%.  Di partenza era {probabilitaDiPartenza}%, e {viaggio}.";
    }

    private void MostraScambio()
    {
        corpo.Controls.Clear();

        if (indice >= scambi.Count)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        var scambio = scambi[indice];

        var contenitore = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, BackColor = UiTheme.Background
        };

        contenitore.Controls.Add(new Label
        {
            Text = $"DOMANDA {indice + 1} DI {scambi.Count}",
            Font = UiTheme.Kicker, ForeColor = UiTheme.TextMuted, AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        });

        contenitore.Controls.Add(new Label
        {
            Text = scambio.Domanda,
            Font = new Font(UiTheme.FamilySerif, 14F, FontStyle.Italic),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = false, Width = 850, Height = 62,
            Margin = new Padding(0, 0, 0, 14)
        });

        foreach (var mossa in scambio.Mosse)
            contenitore.Controls.Add(Bottone(mossa));

        corpo.Controls.Add(contenitore);
    }

    private Control Bottone(SponsorMossa mossa)
    {
        var pannello = new Panel
        {
            Width = 850, Height = 92, BackColor = UiTheme.Surface,
            Margin = new Padding(0, 0, 0, 10), Cursor = Cursors.Hand,
            Padding = new Padding(16, 10, 16, 10)
        };

        var titolo = new Label
        {
            Text = mossa.Etichetta.ToUpperInvariant(), Dock = DockStyle.Top, Height = 20,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Accent, BackColor = Color.Transparent
        };
        var testo = new Label
        {
            Text = mossa.Battuta, Dock = DockStyle.Fill,
            Font = UiTheme.Prose, ForeColor = UiTheme.TextSecondary, BackColor = Color.Transparent
        };

        pannello.Controls.Add(testo);
        pannello.Controls.Add(titolo);

        void Evidenzia(bool acceso)
        {
            pannello.BackColor = acceso ? UiTheme.SurfaceRaised : UiTheme.Surface;
            testo.ForeColor = acceso ? UiTheme.TextPrimary : UiTheme.TextSecondary;
        }

        void Scegli(object? s, EventArgs e) => Applica(mossa);

        foreach (Control c in new Control[] { pannello, titolo, testo })
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
        somma += esito.Delta;
        Trascrizione.Add($"Haru: {esito.Battuta}");
        Trascrizione.Add($"{visita.Target}: {esito.Reazione}");

        AggiornaIndicatore();

        // La reazione va mostrata prima della domanda successiva, altrimenti
        // il giocatore vede solo la barra muoversi e non sa perché.
        corpo.Controls.Clear();
        var reazione = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = UiTheme.Background
        };
        reazione.Controls.Add(new Label
        {
            Text = "HAI RISPOSTO", Font = UiTheme.Kicker, ForeColor = UiTheme.TextMuted,
            AutoSize = true, Margin = new Padding(0, 0, 0, 6)
        });
        reazione.Controls.Add(new Label
        {
            Text = esito.Battuta, Font = UiTheme.Prose, ForeColor = UiTheme.TextSecondary,
            AutoSize = false, Width = 850, Height = 54, Margin = new Padding(0, 0, 0, 16)
        });
        reazione.Controls.Add(new Label
        {
            Text = esito.Delta > 0 ? "GLI È PIACIUTA" : esito.Delta < 0 ? "NON GLI È PIACIUTA" : "L'HA PRESA COM'È",
            Font = UiTheme.Kicker,
            ForeColor = esito.Delta > 0 ? UiTheme.Positive : esito.Delta < 0 ? UiTheme.Accent : UiTheme.TextMuted,
            AutoSize = true, Margin = new Padding(0, 0, 0, 6)
        });
        reazione.Controls.Add(new Label
        {
            Text = esito.Reazione, Font = new Font(UiTheme.FamilySerif, 13F, FontStyle.Italic),
            ForeColor = UiTheme.TextPrimary, AutoSize = false, Width = 850, Height = 58,
            Margin = new Padding(0, 0, 0, 18)
        });

        var avanti = UiTheme.PrimaryButton(indice + 1 >= scambi.Count ? "CHIUDIAMO" : "E POI?");
        avanti.Width = 260;
        avanti.Click += (_, _) => { indice++; MostraScambio(); };
        reazione.Controls.Add(avanti);

        corpo.Controls.Add(reazione);
        AcceptButton = avanti;
    }
}
