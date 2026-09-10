using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// L'ultima schermata: quello che resta di una carriera intera.
///
/// È il momento che mancava. Una carriera che non finisce non ha un peso: si
/// arriva in cima e si continua a correre stagioni identiche finché ci si
/// annoia. Qui invece si conta tutto — anni, gare, vittorie, titoli, il gradino
/// più alto toccato — e si legge una frase che dice come verrà ricordata.
/// </summary>
public sealed class CareerEpilogueDialog : CareerDialog
{
    private Bitmap? tavola;

    public CareerEpilogueDialog(CareerEpilogue bilancio, string motivo, string immagine)
    {
        Text = $"CorsaCareer — la carriera di {bilancio.Pilota}";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var trionfale = bilancio.Mondiali > 0 || bilancio.Titoli > 0;
        var colore = trionfale ? UiTheme.Warning : UiTheme.Info;

        var header = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 18, 40, 14) };
        header.Controls.Add(new Label
        {
            Text = "FINE DELLA CARRIERA", Dock = DockStyle.Top, Height = 24,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Accent
        });
        header.Controls.Add(new Label
        {
            Text = bilancio.Pilota.ToUpperInvariant(), Dock = DockStyle.Top, Height = 48,
            Font = UiTheme.Headline, ForeColor = colore, AutoEllipsis = true
        });
        header.Controls.Add(new Label
        {
            Text = bilancio.AnniDiCarriera > 0
                ? $"{bilancio.AnniDiCarriera} anni di corse · {bilancio.Gare} gare · si ritira a {bilancio.EtaAlRitiro} anni"
                : $"{bilancio.Gare} gare",
            Dock = DockStyle.Bottom, Height = 32, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary
        });

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
            Padding = new Padding(34, 20, 34, 16), BackColor = UiTheme.Background
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));

        var fotoCard = UiTheme.Card("L'ULTIMO GIORNO", out var fotoInner, colore);
        fotoCard.Dock = DockStyle.Fill;
        var foto = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Zoom };
        tavola = Carica(immagine);
        foto.Image = tavola;
        fotoInner.Controls.Add(foto);
        grid.Controls.Add(fotoCard, 0, 0);

        var numeriCard = UiTheme.Card("QUELLO CHE RESTA", out var numeriInner, colore);
        numeriCard.Dock = DockStyle.Fill;
        var testo = new RichTextBox
        {
            Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, Font = new Font(UiTheme.FamilyMono, 9.5f)
        };
        testo.Text = Annuario(bilancio, motivo);
        numeriInner.Controls.Add(testo);
        grid.Controls.Add(numeriCard, 1, 0);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 84, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 16, 40, 16) };
        var chiudi = UiTheme.PrimaryButton("CHIUDI LA CARRIERA");
        chiudi.Dock = DockStyle.Right; chiudi.Width = 300; chiudi.DialogResult = DialogResult.OK;
        footer.Controls.Add(chiudi);
        footer.Controls.Add(new Label
        {
            Text = bilancio.Giudizio(),
            Dock = DockStyle.Fill, Font = UiTheme.Standfirst,
            ForeColor = trionfale ? UiTheme.Warning : UiTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        });
        AcceptButton = chiudi;

        Controls.Add(grid); Controls.Add(footer); Controls.Add(header);
        Disposed += (_, _) => tavola?.Dispose();
    }

    private static string Annuario(CareerEpilogue b, string motivo)
    {
        var righe = new List<string>();
        if (!string.IsNullOrWhiteSpace(motivo)) { righe.Add(motivo); righe.Add(""); }

        righe.Add("IN PISTA");
        righe.Add($"  Gare disputate      {b.Gare}");
        righe.Add($"  Vittorie            {b.Vittorie}   ({b.PercentualeVittorie}%)");
        righe.Add($"  Podi                {b.Podi}   ({b.PercentualePodi}%)");
        righe.Add($"  Pole position       {b.Pole}");
        righe.Add($"  Ritiri              {b.Ritiri}");
        righe.Add($"  Punti in carriera   {b.Punti}");
        righe.Add($"  Giri percorsi       {b.GiriPercorsi}");
        righe.Add("");
        righe.Add("CAMPIONATI");
        righe.Add($"  Stagioni concluse   {b.Stagioni}");
        righe.Add($"  Titoli              {b.Titoli}");
        righe.Add($"  Mondiali            {b.Mondiali}");
        righe.Add("");
        righe.Add("LA SCALATA");
        righe.Add($"  Categoria più alta  {(string.IsNullOrWhiteSpace(b.CategoriaMassima) ? "—" : b.CategoriaMassima)}"
                  + (b.GradinoMassimo > 0 ? $"  ({b.GradinoMassimo} di {CareerLadder.Steps})" : ""));
        righe.Add($"  Vetture guidate     {b.CategorieAttraversate}");
        if (!string.IsNullOrWhiteSpace(b.PrimaVittoria)) righe.Add($"  Prima vittoria      {b.PrimaVittoria}");
        if (!string.IsNullOrWhiteSpace(b.UltimaGara)) righe.Add($"  Ultima gara         {b.UltimaGara}");

        return string.Join("\n", righe);
    }

    private static Bitmap? Carica(string immagine)
    {
        if (string.IsNullOrWhiteSpace(immagine)) return null;
        var percorso = AssetPaths.File(immagine);
        if (!File.Exists(percorso)) return null;
        try { using var source = Image.FromFile(percorso); return new Bitmap(source); }
        catch { return null; }
    }
}
