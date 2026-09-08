using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>Il tono di un passaggio di carriera: decide colori e parole.</summary>
public enum MilestoneTone
{
    /// <summary>Una cosa bella: promozione, titolo, chiamata da sopra.</summary>
    Trionfo,
    /// <summary>Una cosa neutra: si resta, si riparte, si conferma.</summary>
    Passaggio,
    /// <summary>Una cosa brutta: retrocessione, sedile perso, scommessa fallita.</summary>
    Caduta
}

/// <summary>
/// La notifica grande dei passaggi di carriera.
///
/// Salire di livello, restarci o retrocedere sono i tre momenti che danno forma
/// a una carriera, e passavano tutti e tre in una riga di notizia in fondo alla
/// schermata: si vinceva un campionato e non se ne accorgeva nessuno. Qui
/// occupano lo schermo intero, con una fotografia, il verdetto scritto grande e
/// le conseguenze dette in chiaro — che cosa cambia adesso.
///
/// I commenti degli amici non stanno qui: arrivano subito dopo come scena
/// anime, che e' il posto dove questa carriera racconta le reazioni.
/// </summary>
public sealed class MilestoneDialog : CareerDialog
{
    private Bitmap? tavola;

    public MilestoneDialog(string kicker, string titolo, string sommario,
        IReadOnlyList<string> conseguenze, string immagine, MilestoneTone tono, string pulsante = "AVANTI")
    {
        Text = $"CorsaCareer — {titolo}";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var colore = tono switch
        {
            MilestoneTone.Trionfo => UiTheme.Warning,
            MilestoneTone.Caduta => UiTheme.Accent,
            _ => UiTheme.Info
        };

        var header = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 18, 40, 14) };
        header.Controls.Add(new Label
        {
            Text = kicker.ToUpperInvariant(), Dock = DockStyle.Top, Height = 26,
            Font = UiTheme.Kicker,
            ForeColor = tono switch { MilestoneTone.Trionfo => UiTheme.Positive, MilestoneTone.Caduta => UiTheme.Accent, _ => UiTheme.Info }
        });
        header.Controls.Add(new Label
        {
            Text = titolo.ToUpperInvariant(), Dock = DockStyle.Top, Height = 52,
            Font = UiTheme.Headline, ForeColor = colore, AutoEllipsis = true
        });
        header.Controls.Add(new Label
        {
            Text = sommario, Dock = DockStyle.Bottom, Height = 36,
            Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary
        });

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
            Padding = new Padding(34, 22, 34, 18), BackColor = UiTheme.Background
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

        var fotoCard = UiTheme.Card("LA SCENA", out var fotoInner, colore);
        fotoCard.Dock = DockStyle.Fill;
        var foto = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Zoom };
        tavola = Carica(immagine);
        foto.Image = tavola;
        fotoInner.Controls.Add(foto);
        grid.Controls.Add(fotoCard, 0, 0);

        var testoCard = UiTheme.Card("COSA CAMBIA ADESSO", out var testoInner, colore);
        testoCard.Dock = DockStyle.Fill;
        var testo = new RichTextBox
        {
            Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Prose
        };
        testo.Text = string.Join("\n\n", conseguenze.Where(x => !string.IsNullOrWhiteSpace(x)));
        testoInner.Controls.Add(testo);
        grid.Controls.Add(testoCard, 1, 0);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 78, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 15, 40, 15) };
        var avanti = UiTheme.PrimaryButton(pulsante);
        avanti.Dock = DockStyle.Right; avanti.Width = 300; avanti.DialogResult = DialogResult.OK;
        footer.Controls.Add(avanti);
        footer.Controls.Add(new Label
        {
            Text = tono == MilestoneTone.Caduta
                ? "Nessuna carriera è fatta solo di stagioni buone."
                : "Ogni stagione parte da quello che hai costruito nella precedente.",
            Dock = DockStyle.Fill, Font = UiTheme.Body, ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        });
        AcceptButton = avanti;

        Controls.Add(grid); Controls.Add(footer); Controls.Add(header);
        Disposed += (_, _) => tavola?.Dispose();
    }

    private static Bitmap? Carica(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var pieno = Path.IsPathRooted(path) ? path : AssetPaths.File(path);
        if (!File.Exists(pieno)) return null;
        try { using var source = Image.FromFile(pieno); return new Bitmap(source); }
        catch { return null; }
    }
}
