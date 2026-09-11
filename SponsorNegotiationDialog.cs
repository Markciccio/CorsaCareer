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

    /// <summary>
    /// La larghezza della colonna di lettura.
    ///
    /// La finestra viene massimizzata dalla classe base, e i pannelli erano a
    /// larghezza fissa: su un monitor grande le tre risposte occupavano meno
    /// di meta' schermo con un vuoto enorme a destra. Qui la colonna si prende
    /// lo spazio ma non piu' di quanto se ne possa leggere comodamente.
    /// </summary>
    private int LarghezzaColonna => Math.Min(1560, Math.Max(700, ClientSize.Width - 160));

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

        // Le schede hanno un'altezza propria e non si stirano fino in fondo:
        // riempite dell'intera colonna restavano mezze vuote, con il testo
        // rannicchiato in alto e trecento pixel di niente sotto.
        var colonna = new TableLayoutPanel
        {
            ColumnCount = 1, RowCount = 4, BackColor = UiTheme.Background
        };
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));
        colonna.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        colonna.Controls.Add(new Label
        {
            Text = $"DOMANDA {indice + 1} DI {scambi.Count}", Dock = DockStyle.Fill,
            Font = UiTheme.Kicker, ForeColor = UiTheme.TextMuted
        }, 0, 0);

        colonna.Controls.Add(new Label
        {
            Text = scambio.Domanda, Dock = DockStyle.Fill,
            Font = new Font(UiTheme.FamilySerif, 15F, FontStyle.Italic),
            ForeColor = UiTheme.TextPrimary
        }, 0, 1);

        // Le tre risposte affiancate invece che in pila: sono alternative fra
        // loro, e una accanto all'altra si confrontano con un colpo d'occhio.
        var scelte = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = scambio.Mosse.Count, RowCount = 1,
            BackColor = UiTheme.Background
        };
        for (var i = 0; i < scambio.Mosse.Count; i++)
            scelte.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / scambio.Mosse.Count));
        for (var i = 0; i < scambio.Mosse.Count; i++)
            scelte.Controls.Add(Bottone(scambio.Mosse[i]), i, 0);
        colonna.Controls.Add(scelte, 0, 2);
        colonna.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background }, 0, 3);

        corpo.Controls.Add(Centrato(colonna));
    }

    /// <summary>
    /// Tiene un contenuto al centro della finestra, con la larghezza della
    /// colonna di lettura. Serve a tutte e due le schermate della trattativa.
    /// </summary>
    private Control Centrato(Control contenuto)
    {
        var centratore = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background };
        centratore.Controls.Add(contenuto);
        void Centra()
        {
            contenuto.Width = LarghezzaColonna;
            contenuto.Height = Math.Max(300, centratore.ClientSize.Height - 60);
            contenuto.Left = Math.Max(0, (centratore.ClientSize.Width - contenuto.Width) / 2);
            contenuto.Top = Math.Max(0, (centratore.ClientSize.Height - contenuto.Height) / 2);
        }
        centratore.Resize += (_, _) => Centra();
        centratore.HandleCreated += (_, _) => Centra();
        Centra();
        return centratore;
    }

    private Control Bottone(SponsorMossa mossa)
    {
        var pannello = new Panel
        {
            Dock = DockStyle.Fill, BackColor = UiTheme.Surface,
            Margin = new Padding(0, 0, 14, 0), Cursor = Cursors.Hand,
            Padding = new Padding(18, 14, 18, 14)
        };

        var titolo = new Label
        {
            Text = mossa.Etichetta.ToUpperInvariant(), Dock = DockStyle.Top, Height = 24,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Accent, BackColor = Color.Transparent
        };
        var testo = new Label
        {
            Text = mossa.Battuta, Dock = DockStyle.Fill,
            Font = new Font(UiTheme.FamilySerif, 11.5F), ForeColor = UiTheme.TextSecondary,
            BackColor = Color.Transparent
        };
        var invito = new Label
        {
            Text = "SCEGLI QUESTA", Dock = DockStyle.Bottom, Height = 20,
            Font = UiTheme.Kicker, ForeColor = UiTheme.TextMuted, BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleRight
        };

        pannello.Controls.Add(testo);
        pannello.Controls.Add(invito);
        pannello.Controls.Add(titolo);

        void Evidenzia(bool acceso)
        {
            pannello.BackColor = acceso ? UiTheme.SurfaceRaised : UiTheme.Surface;
            testo.ForeColor = acceso ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            invito.ForeColor = acceso ? UiTheme.Accent : UiTheme.TextMuted;
        }

        void Scegli(object? s, EventArgs e) => Applica(mossa);

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
        somma += esito.Delta;
        Trascrizione.Add($"Haru: {esito.Battuta}");
        Trascrizione.Add($"{visita.Target}: {esito.Reazione}");

        AggiornaIndicatore();

        // La reazione va mostrata prima della domanda successiva, altrimenti
        // il giocatore vede solo la barra muoversi e non sa perché.
        corpo.Controls.Clear();

        var colonna = new TableLayoutPanel
        {
            ColumnCount = 1, RowCount = 6, BackColor = UiTheme.Background
        };
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        colonna.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        colonna.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        colonna.Controls.Add(new Label
        {
            Text = "HAI RISPOSTO", Dock = DockStyle.Fill,
            Font = UiTheme.Kicker, ForeColor = UiTheme.TextMuted
        }, 0, 0);
        colonna.Controls.Add(new Label
        {
            Text = esito.Battuta, Dock = DockStyle.Fill,
            Font = new Font(UiTheme.FamilySerif, 12F), ForeColor = UiTheme.TextSecondary
        }, 0, 1);
        colonna.Controls.Add(new Label
        {
            Text = esito.Delta > 0 ? "GLI È PIACIUTA" : esito.Delta < 0 ? "NON GLI È PIACIUTA" : "L'HA PRESA COM'È",
            Dock = DockStyle.Fill, Font = UiTheme.Kicker,
            ForeColor = esito.Delta > 0 ? UiTheme.Positive : esito.Delta < 0 ? UiTheme.Accent : UiTheme.TextMuted
        }, 0, 2);
        colonna.Controls.Add(new Label
        {
            Text = esito.Reazione, Dock = DockStyle.Fill,
            Font = new Font(UiTheme.FamilySerif, 15F, FontStyle.Italic), ForeColor = UiTheme.TextPrimary
        }, 0, 3);
        colonna.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background }, 0, 4);

        var avanti = UiTheme.PrimaryButton(indice + 1 >= scambi.Count ? "CHIUDIAMO" : "E POI?");
        avanti.Dock = DockStyle.Right;
        avanti.Width = 280;
        avanti.Click += (_, _) => { indice++; MostraScambio(); };
        var riga = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background };
        riga.Controls.Add(avanti);
        colonna.Controls.Add(riga, 0, 5);

        corpo.Controls.Add(Centrato(colonna));
        AcceptButton = avanti;
    }
}
