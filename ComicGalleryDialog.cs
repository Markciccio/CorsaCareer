using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// L'atlante illustrato della carriera. Le tavole non fingono di essere foto
/// delle gare: accompagnano i fatti come farebbe un capitolo di un fumetto.
/// Tutte sono consultabili come anteprima; quelle gia vissute vengono marcate
/// come parte della storia del pilota.
/// </summary>
public sealed class ComicGalleryDialog : CareerDialog
{
    private sealed record Plate(string File, string Chapter, string Title, string Caption);

    private static readonly Plate[] Plates =
    [
        new("manga-01-rookie-dawn.png", "TAVOLA I", "Prima dell'alba", "Nessun contratto, nessun pubblico: soltanto il primo casco e una possibilità."),
        new("manga-02-first-test.png", "TAVOLA II", "Il primo riferimento", "Il cronometro decide se il paddock continuerà a guardare."),
        new("manga-03-first-rival.png", "TAVOLA III", "Quello che non distoglie lo sguardo", "Un rivale non nasce da una frase: nasce quando due percorsi cominciano a misurarsi."),
        new("manga-04-first-setback.png", "TAVOLA IV", "La prima porta chiusa", "Fallire un test non chiude la carriera. Decide che cosa sei disposto a fare per riaprirla."),
        new("manga-05-first-invitation.png", "TAVOLA V", "Una griglia vera", "La prima gara su invito: mezzi inferiori, avversari veri e nessun posto dove nascondersi."),
        new("manga-06-first-contract.png", "TAVOLA VI", "La prima firma", "Qualcuno mette il proprio nome accanto al tuo. Da questo momento deludi anche gli altri."),
        new("manga-07-teammate-duel.png", "TAVOLA VII", "Dall'altra parte del box", "Stessa vettura, stessi dati: il compagno diventa il confronto che nessuna scusa può cancellare."),
        new("manga-08-financial-crisis.png", "TAVOLA VIII", "Quanto costa continuare", "A volte la carriera non si ferma per mancanza di velocità, ma per un ricambio che non puoi pagare."),
        new("manga-09-first-podium.png", "TAVOLA IX", "Tre dita", "Il primo podio appartiene anche a chi ha continuato a spingere il kart quando nessuno guardava."),
        new("manga-10-first-victory.png", "TAVOLA X", "La prima volta", "Un solo traguardo trasforma una promessa in un pilota che gli altri devono battere."),
        new("manga-11-promotion.png", "TAVOLA XI", "Si ricomincia più in alto", "La categoria superiore assomiglia al primo giorno: tutto torna enorme e sconosciuto."),
        new("manga-12-professional.png", "TAVOLA XII", "Ora è un mestiere", "Non paghi più per correre. Da oggi qualcuno paga perché tu produca risultati."),
        new("manga-13-champion.png", "TAVOLA XIII", "Dopo il rumore", "Il titolo resta. Ma all'alba, sul rettilineo vuoto, restano soprattutto le persone del viaggio.")
    ];

    private readonly CareerState career;
    private readonly ListBox index = new();
    private readonly PictureBox artwork = new();
    private readonly Label chapter = new();
    private readonly Label title = new();
    private readonly Label caption = new();
    private readonly Label provenance = new();

    public ComicGalleryDialog(CareerState career)
    {
        this.career = career;
        Text = "CorsaCareer — tavole della carriera";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 820);
        MinimumSize = new Size(920, 640);
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var header = new Panel { Dock = DockStyle.Top, Height = 86, Padding = new Padding(26, 14, 26, 8), BackColor = UiTheme.HeaderBackground };
        header.Controls.Add(new Label
        {
            Text = "ATLANTE ILLUSTRATO · 13 TAVOLE",
            Dock = DockStyle.Top, Height = 31, Font = UiTheme.Title, ForeColor = UiTheme.TextPrimary
        });
        header.Controls.Add(new Label
        {
            Text = "Il fumetto interpreta la carriera; screenshot e referti restano le sole prove degli eventi realmente disputati.",
            Dock = DockStyle.Bottom, Height = 25, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary
        });

        var body = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 300, BackColor = UiTheme.Border };
        body.Panel1MinSize = 240;
        body.Panel1.BackColor = UiTheme.Surface;
        body.Panel1.Padding = new Padding(14);
        body.Panel2.BackColor = UiTheme.Background;
        body.Panel2.Padding = new Padding(22, 16, 22, 14);

        index.Dock = DockStyle.Fill;
        index.BackColor = UiTheme.Surface;
        index.ForeColor = UiTheme.TextPrimary;
        index.BorderStyle = BorderStyle.None;
        index.Font = UiTheme.Body;
        index.ItemHeight = 29;
        index.IntegralHeight = false;
        foreach (var plate in Plates) index.Items.Add($"{plate.Chapter.Replace("TAVOLA ", "")}  ·  {plate.Title}");
        index.SelectedIndexChanged += (_, _) => ShowSelected();
        body.Panel1.Controls.Add(index);

        var text = new Panel { Dock = DockStyle.Bottom, Height = 170, Padding = new Padding(4, 10, 4, 0), BackColor = Color.Transparent };
        provenance.Dock = DockStyle.Bottom;
        provenance.Height = 28;
        provenance.Font = UiTheme.Small;
        provenance.ForeColor = UiTheme.Warning;
        caption.Dock = DockStyle.Fill;
        caption.Font = UiTheme.Standfirst;
        caption.ForeColor = UiTheme.TextSecondary;
        caption.Padding = new Padding(0, 8, 0, 0);
        title.Dock = DockStyle.Top;
        title.Height = 44;
        title.Font = UiTheme.HeadlineSmall;
        title.ForeColor = UiTheme.TextPrimary;
        chapter.Dock = DockStyle.Top;
        chapter.Height = 24;
        chapter.Font = UiTheme.Kicker;
        chapter.ForeColor = UiTheme.Accent;
        text.Controls.Add(caption);
        text.Controls.Add(provenance);
        text.Controls.Add(title);
        text.Controls.Add(chapter);

        artwork.Dock = DockStyle.Fill;
        artwork.SizeMode = PictureBoxSizeMode.Zoom;
        artwork.BackColor = Color.FromArgb(7, 9, 12);
        body.Panel2.Controls.Add(artwork);
        body.Panel2.Controls.Add(text);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 62, Padding = new Padding(22, 10, 22, 10), BackColor = UiTheme.HeaderBackground };
        var close = UiTheme.PrimaryButton("CHIUDI L'ATLANTE");
        close.Dock = DockStyle.Right;
        close.Width = 220;
        close.Click += (_, _) => Close();
        footer.Controls.Add(close);

        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
        Shown += (_, _) => FitIndex(body);
        Resize += (_, _) => { if (IsHandleCreated) FitIndex(body); };
        AcceptButton = close;
        CancelButton = close;
        index.SelectedIndex = 0;
    }

    private static void FitIndex(SplitContainer body)
    {
        // AutoScale DPI puo applicare la distanza prima che la finestra abbia
        // le dimensioni definitive. La ricalcoliamo quando e davvero visibile.
        var desired = Math.Clamp(body.ClientSize.Width / 3, 320, 420);
        if (body.ClientSize.Width > desired + body.Panel2MinSize + body.SplitterWidth)
            body.SplitterDistance = desired;
    }

    private void ShowSelected()
    {
        if (index.SelectedIndex < 0 || index.SelectedIndex >= Plates.Length) return;
        var plate = Plates[index.SelectedIndex];
        chapter.Text = plate.Chapter + (IsLived(index.SelectedIndex) ? "  ·  PARTE DELLA TUA STORIA" : "  ·  ANTEPRIMA DEL PERCORSO");
        title.Text = plate.Title;
        caption.Text = plate.Caption;
        provenance.Text = "ILLUSTRAZIONE NARRATIVA GENERATA CON AI · NON È UNA FOTOGRAFIA DELL'EVENTO";

        var previous = artwork.Image;
        artwork.Image = null;
        previous?.Dispose();
        var path = AssetPaths.File(plate.File);
        if (!File.Exists(path)) return;
        try
        {
            using var source = Image.FromFile(path);
            artwork.Image = new Bitmap(source);
        }
        catch (Exception error) { CareerLog.Warn("fumetto", $"galleria: {plate.File} non caricata: {error.Message}"); }
    }

    private bool IsLived(int plateIndex) => plateIndex switch
    {
        0 => true,
        1 => career.TestHistory.Count > 0,
        2 => career.Events.Any(x => x.Type is "EVALUATION_PASSED" or "RIVALRY_DUEL" or "TEAMMATE_DUEL"),
        3 => career.Events.Any(x => x.Type is "EVALUATION_REVIEW" or "INVITATION_SETBACK" or "RETIREMENT"),
        4 => career.Events.Any(x => x.Type is "INVITATION_BREAKTHROUGH" or "INVITATION_SETBACK"),
        5 => career.ContractActive || career.Events.Any(x => x.Type == "CONTRACT_SIGNING"),
        6 => career.TeammateRacesWon + career.TeammateRacesLost > 0,
        7 => career.Events.Any(x => x.Type == "FINANCIAL_TROUBLE"),
        8 => career.Podiums > 0,
        9 => career.Wins > 0,
        10 => (career.SeasonArchive?.Count ?? 0) > 0,
        11 => career.ContractSalary > 0,
        12 => career.Events.Any(x => x.Type is "SEASON_AWARD" or "CHAMPIONSHIP_WON"),
        _ => false
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing) artwork.Image?.Dispose();
        base.Dispose(disposing);
    }
}
