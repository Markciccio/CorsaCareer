using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// Chi c'è intorno a te, presentato all'inizio della carriera.
///
/// I personaggi comparivano man mano, dicendo la loro senza che nessuno avesse
/// spiegato chi fossero: la prima volta che Rei Kisaragi ti spiega una regola,
/// per il giocatore è una sconosciuta che dà ordini, e la scena perde tutto il
/// peso che dovrebbe avere. Le stesse parole dette da qualcuno che sai chi è
/// valgono un'altra cosa.
///
/// Qui ci sono tutti insieme, una volta sola, all'inizio: faccia, nome, che
/// ruolo hanno nella tua carriera, che carattere hanno e in che momenti li
/// sentirai parlare. Le informazioni vengono dal regista del cast, quindi non
/// possono divergere da come i personaggi si comportano davvero.
/// </summary>
public sealed class CastIntroDialog : CareerDialog
{
    private readonly List<Bitmap> immagini = [];

    public CastIntroDialog(string pilota)
    {
        Text = "CorsaCareer — le persone intorno a te";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var header = new Panel { Dock = DockStyle.Top, Height = 124, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 18, 40, 14) };
        header.Controls.Add(new Label
        {
            Text = "PRIMA DI COMINCIARE", Dock = DockStyle.Top, Height = 24,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Positive
        });
        header.Controls.Add(new Label
        {
            Text = "LE PERSONE INTORNO A TE", Dock = DockStyle.Top, Height = 46,
            Font = UiTheme.Headline, ForeColor = UiTheme.Warning
        });
        header.Controls.Add(new Label
        {
            Text = $"Nessuno arriva da solo dove vuoi arrivare tu, {pilota}. "
                   + "Questi sono quelli che ti accompagneranno: alcuni ti aiuteranno, uno proverà a batterti, "
                   + "e tutti diranno la loro dopo ogni gara.",
            Dock = DockStyle.Bottom, Height = 34, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary
        });

        var scorrimento = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true,
            Padding = new Padding(28, 20, 28, 20), BackColor = UiTheme.Background
        };
        foreach (var persona in CastDirector.Compagnia) scorrimento.Controls.Add(Scheda(persona));

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 76, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 15, 40, 15) };
        var avanti = UiTheme.PrimaryButton("COMINCIAMO");
        avanti.Dock = DockStyle.Right; avanti.Width = 280; avanti.DialogResult = DialogResult.OK;
        footer.Controls.Add(avanti);
        footer.Controls.Add(new Label
        {
            Text = "Li ritroverai dopo ogni gara, a fine stagione e ogni volta che ci sarà una decisione da prendere.",
            Dock = DockStyle.Fill, Font = UiTheme.Body, ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        });
        AcceptButton = avanti;

        Controls.Add(scorrimento); Controls.Add(footer); Controls.Add(header);
        Disposed += (_, _) => { foreach (var b in immagini) b.Dispose(); };
    }

    private Control Scheda(CastMember persona)
    {
        var carta = new Panel
        {
            Width = 420, Height = 268, Margin = new Padding(10),
            BackColor = UiTheme.Surface, Padding = new Padding(1)
        };
        var bordo = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(14) };

        var ritratto = new PictureBox
        {
            Dock = DockStyle.Left, Width = 128, BackColor = UiTheme.SurfaceRaised,
            SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 0, 12, 0)
        };
        var file = CastDirector.Ritratto(persona.Id);
        if (!string.IsNullOrWhiteSpace(file))
        {
            var percorso = AssetPaths.File(file);
            if (File.Exists(percorso))
            {
                try
                {
                    using var source = Image.FromFile(percorso);
                    var copia = new Bitmap(source);
                    immagini.Add(copia);
                    ritratto.Image = copia;
                }
                catch { }
            }
        }

        var testi = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 0, 0, 0), BackColor = UiTheme.Surface };
        testi.Controls.Add(new Label
        {
            Text = persona.Personalita + "\n\nCOME PARLA\n" + persona.ComeParla,
            Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary
        });
        testi.Controls.Add(new Label
        {
            Text = persona.Ruolo, Dock = DockStyle.Top, Height = 34,
            Font = UiTheme.Small, ForeColor = UiTheme.Info
        });
        testi.Controls.Add(new Label
        {
            Text = persona.Nome.ToUpperInvariant(), Dock = DockStyle.Top, Height = 26,
            Font = UiTheme.BodyStrong, ForeColor = UiTheme.TextPrimary
        });

        bordo.Controls.Add(testi);
        bordo.Controls.Add(ritratto);
        carta.Controls.Add(bordo);
        return carta;
    }
}
