using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>Apertura a tavole di un capitolo: testo scritto davanti al giocatore e immagini narrative in dissolvenza.</summary>
public sealed class PhaseIntroDialog : CareerDialog
{
    private readonly CareerPhase phase;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 24 };
    private readonly System.Windows.Forms.Timer artworkTimer = new() { Interval = 100 };
    private const int FlashFrames = 22;
    private const int TitleFrames = 18;
    private const int PauseBetweenLines = 16;

    private int frame, lineIndex, letterIndex, linePause;
    private bool narrationLinked;
    private bool blocksBuilt, skipped;
    private bool soundtrackHandedBack;
    private readonly List<(Label Label, string Text)> narrative = [];
    private readonly List<string> artworkPaths = [];
    private int artworkIndex, artworkHoldTicks, artworkFadeStep;
    private Image? currentArtwork, nextArtwork;
    private readonly Panel flashBar = new();
    private readonly Label flashText = new();
    private readonly FlowLayoutPanel content = new();
    private Label titleLabel = new();
    private Label? standfirstLabel;
    private PictureBox? artworkBox;
    private Panel? artworkFrame;

    public PhaseIntroDialog(CareerPhase phase)
    {
        this.phase = phase;
        Text = $"CorsaCareer — {phase.Title}";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1000, 700); MinimumSize = new Size(760, 560);
        WindowState = FormWindowState.Maximized;
        BackColor = UiTheme.Background; ForeColor = UiTheme.TextPrimary; KeyPreview = true; DoubleBuffered = true;

        flashBar.Dock = DockStyle.Top; flashBar.Height = 48; flashBar.BackColor = UiTheme.Accent;
        flashText.Dock = DockStyle.Fill; flashText.Font = new Font(UiTheme.FamilySemibold, 12F, FontStyle.Bold);
        flashText.ForeColor = Color.White; flashText.TextAlign = ContentAlignment.MiddleLeft; flashText.Padding = new Padding(30, 0, 0, 0);
        flashBar.Controls.Add(flashText);
        content.Dock = DockStyle.Fill; content.FlowDirection = FlowDirection.TopDown; content.WrapContents = false; content.AutoScroll = true;
        content.BackColor = UiTheme.Background; content.Padding = new Padding(54, 34, 54, 24);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 76, BackColor = Color.FromArgb(16, 19, 27), Padding = new Padding(38, 12, 34, 12) };
        var start = UiTheme.PrimaryButton("COMINCIA"); start.Dock = DockStyle.Right; start.Width = 208; start.Click += (_, _) => Close();
        var hint = new Label { Dock = DockStyle.Left, Width = 500, Font = new Font(UiTheme.FamilySans, 9F), ForeColor = UiTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft, Text = "Spazio: mostra subito il testo · COMINCIA: entra nella carriera" };
        footer.Controls.Add(start); footer.Controls.Add(hint); Controls.Add(footer); Controls.Add(content); Controls.Add(flashBar);
        // Nessun tasto di conferma automatica: il prologo resta aperto finché
        // il giocatore non seleziona esplicitamente COMINCIA.
        KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Space or Keys.Down)
            {
                e.Handled = true;
                Skip();
            }
        };
        Paint += (_, e) => { using var pen = new Pen(UiTheme.Border); e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1); };
        timer.Tick += (_, _) => Advance(); artworkTimer.Tick += (_, _) => AdvanceArtwork();
        // La voce registrata ha la precedenza: se c'è, il tema del prologo non
        // parte, così le due tracce non si sovrappongono a volumi diversi.
        // Musica sempre, voce sopra se c'è.
        //
        // Prima erano alternative: con la narrazione registrata presente il
        // brano non partiva mai, e il prologo si leggeva nel silenzio. Il
        // volume del tema d'apertura è già tarato per stare sotto a chi legge
        // (IntroVolume), quindi le due cose convivono invece di escludersi.
        Shown += (_, _) => { SoundtrackService.PlayIntro(); StartNarrationAudio(); };
        FormClosed += (_, _) => HandBackSoundtrack();
        timer.Start();
    }

    private void Advance()
    {
        frame++;
        if (frame <= FlashFrames)
        {
            var count = Math.Max(1, phase.Flash.Length * frame / FlashFrames);
            flashText.Text = "●  " + phase.Flash[..Math.Min(phase.Flash.Length, count)]; return;
        }
        if (!blocksBuilt) BuildBlocks();
        if (frame <= FlashFrames + TitleFrames)
        {
            titleLabel.ForeColor = Blend(UiTheme.Background, UiTheme.TextPrimary, (frame - FlashFrames) / (double)TitleFrames);
            titleLabel.Visible = true; return;
        }
        if (standfirstLabel is not null) standfirstLabel.Visible = true;
        if (artworkFrame is not null) artworkFrame.Visible = true;
        if (lineIndex >= narrative.Count) { timer.Stop(); return; }
        if (linePause > 0) { linePause--; return; }
        var current = narrative[lineIndex]; current.Label.Visible = true;
        letterIndex = Math.Min(current.Text.Length, letterIndex + 2); current.Label.Text = current.Text[..letterIndex]; ResizeForText(current.Label);
        if (letterIndex < current.Text.Length) return;
        lineIndex++; letterIndex = 0; linePause = PauseBetweenLines; content.ScrollControlIntoView(current.Label);
    }

    /// <summary>Vero quando la narrazione registrata è partita davvero.</summary>
    private bool StartNarrationAudio()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "narration", "chapter1-intro-narration.mp3");
        narrationLinked = NarrationService.PlayAudioFile(path);
        return narrationLinked;
    }

    private void BuildBlocks()
    {
        blocksBuilt = true; var width = Math.Max(540, content.ClientSize.Width - 105);
        titleLabel = Block(phase.Title, new Font(UiTheme.FamilySemibold, 38F, FontStyle.Bold), UiTheme.TextPrimary, width, 0, 8); content.Controls.Add(titleLabel);
        standfirstLabel = Block(phase.Standfirst, new Font(UiTheme.FamilySans, 17F, FontStyle.Italic), UiTheme.TextSecondary, width, 0, 22); content.Controls.Add(standfirstLabel);
        InsertArtwork(width);
        content.Controls.Add(new Panel { Width = width, Height = 2, BackColor = UiTheme.Border, Margin = new Padding(0, 12, 0, 16) });
        foreach (var text in phase.Paragraphs) AddNarrativeLine(text, UiTheme.TextPrimary, width, 11);
        if (!string.IsNullOrWhiteSpace(phase.Objective)) { AddNarrativeLine("QUELLO CHE CONTA ADESSO", UiTheme.Positive, width, 14, true); AddNarrativeLine(phase.Objective, UiTheme.TextPrimary, width, 7); }
        if (!string.IsNullOrWhiteSpace(phase.Stake)) { AddNarrativeLine("COSA SI RISCHIA", UiTheme.Warning, width, 14, true); AddNarrativeLine(phase.Stake, UiTheme.TextSecondary, width, 7); }
        titleLabel.Visible = false; standfirstLabel.Visible = false; artworkFrame?.Hide();
    }

    private void AddNarrativeLine(string text, Color color, int width, int margin, bool kicker = false)
    {
        var font = kicker ? new Font(UiTheme.FamilySemibold, 10.5F, FontStyle.Bold) : new Font(UiTheme.FamilySans, 16F);
        var label = Block(string.Empty, font, color, width, margin, kicker ? 4 : 14); label.Visible = false; content.Controls.Add(label); narrative.Add((label, text));
    }

    private void InsertArtwork(int width)
    {
        // La tavola è il manifesto del prologo: su schermi larghi deve avere
        // una presenza reale, senza comprimersi in una miniatura centrale.
        // Le tavole sono il manifesto del capitolo: il rapporto del riquadro è
        // vicino a quello delle illustrazioni manga (3:2), così non restano
        // miniere nere ai lati quando la finestra è a tutto schermo.
        artworkFrame = new Panel { Width = width, Height = Math.Clamp((int)(width / 1.62), 420, 760), BackColor = Color.FromArgb(7, 9, 13), Margin = new Padding(0, 4, 0, 0), Padding = new Padding(3) };
        artworkFrame.Paint += (_, e) => { using var pen = new Pen(UiTheme.Border, 1); e.Graphics.DrawRectangle(pen, 0, 0, artworkFrame.Width - 1, artworkFrame.Height - 1); };
        artworkBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(7, 9, 13) }; artworkFrame.Controls.Add(artworkBox); content.Controls.Add(artworkFrame);
        foreach (var asset in ArtworkAssets())
        {
            var path = AssetPaths.File(asset);
            if (File.Exists(path) && !artworkPaths.Contains(path, StringComparer.OrdinalIgnoreCase)) artworkPaths.Add(path);
        }
        if (artworkPaths.Count == 0) return;
        currentArtwork = LoadArtwork(artworkPaths[0]); artworkBox.Image = currentArtwork; artworkTimer.Start();
    }

    // Tavole semanticamente legate al capitolo, non una galleria casuale.
    private IEnumerable<string> ArtworkAssets() => phase.Id switch
    {
        CareerPhases.Debut => [phase.ArtworkAsset, "manga-02-first-test.png", "test-day-garage.png", "calendar-rookie-rain.png", "engineering-briefing.png"],
        CareerPhases.Contract => [phase.ArtworkAsset, "manga-05-first-invitation.png", "manga-sponsor-table.png", "manga-07-teammate-duel.png"],
        CareerPhases.Climb => [phase.ArtworkAsset, "manga-07-teammate-duel.png", "manga-09-first-podium.png", "fictional-openwheel-promo.png"],
        CareerPhases.Winner => [phase.ArtworkAsset, "finish-celebration.png", "podium-trophy.png", "manga-11-promotion.png"],
        CareerPhases.Professional => [phase.ArtworkAsset, "manga-sponsor-table.png", "manga-11-promotion.png", "manga-13-champion.png"],
        _ => [phase.ArtworkAsset, "manga-13-champion.png", "podium-trophy.png", "finish-celebration.png"]
    };

    private void AdvanceArtwork()
    {
        if (artworkBox is null || artworkPaths.Count < 2) return;
        if (nextArtwork is null)
        {
            if (++artworkHoldTicks < 100) return; // 10 secondi pieni per ogni tavola
            artworkHoldTicks = 0; nextArtwork = LoadArtwork(artworkPaths[(artworkIndex + 1) % artworkPaths.Count]); artworkFadeStep = 0;
        }
        artworkFadeStep++;
        var blended = BlendArtwork(currentArtwork!, nextArtwork, artworkFadeStep / 10d); var old = artworkBox.Image; artworkBox.Image = blended;
        if (!ReferenceEquals(old, currentArtwork)) old?.Dispose();
        if (artworkFadeStep < 10) return;
        currentArtwork?.Dispose(); currentArtwork = nextArtwork; nextArtwork = null; artworkIndex = (artworkIndex + 1) % artworkPaths.Count;
    }

    private static Image LoadArtwork(string path) { using var source = Image.FromFile(path); return new Bitmap(source); }
    private static Bitmap BlendArtwork(Image from, Image to, double amount)
    {
        // Una sola tavola alla volta: prima dissolvenza al nero, poi ingresso
        // della successiva. Niente sovrapposizione di figure o ridimensionamenti.
        var showFrom = amount < .5;
        var image = showFrom ? from : to;
        // Il fotogramma segue le proporzioni della tavola mostrata. Con
        // Max(from,to) fra tavole di formato diverso il risultato non era il
        // formato di nessuna delle due, e il PictureBox in Zoom la stirava per
        // tutta la durata del passaggio: sembrava un breve ridimensionamento.
        var width = image.Width; var height = image.Height;
        var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(result); g.Clear(Color.Black);
        var alpha = showFrom ? 1 - amount * 2 : amount * 2 - 1;
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = (float)Math.Clamp(alpha, 0, 1) });
        g.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes); return result;
    }

    private static Label Block(string text, Font font, Color color, int width, int above, int below)
    {
        var label = new Label { Text = text, Font = font, ForeColor = color, AutoSize = false, Width = width, BackColor = Color.Transparent, UseMnemonic = false, Margin = new Padding(0, above, 0, below) };
        // Stessa misura usata da ResizeForText: il testo compare una lettera
        // alla volta, quindi l'altezza iniziale e quella finale devono venire
        // dalla stessa API di quella con cui la Label viene disegnata.
        var measured = TextRenderer.MeasureText(string.IsNullOrEmpty(text) ? "Ag" : text, font,
            new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        label.Height = Math.Max(font.Height + 5, measured.Height + 7); return label;
    }

    private void Skip()
    {
        // Spazio mostra subito tutto il testo, ma il prologo resta aperto: la
        // musica e la voce non vanno chiuse qui, altrimenti la schermata resta
        // visibile mentre l'audio è già passato alla Home.
        if (skipped) return; skipped = true; timer.Stop(); flashText.Text = "●  " + phase.Flash; if (!blocksBuilt) BuildBlocks();
        titleLabel.ForeColor = UiTheme.TextPrimary; titleLabel.Visible = true; if (standfirstLabel is not null) standfirstLabel.Visible = true; artworkFrame?.Show();
        foreach (var (label, text) in narrative) { label.Text = text; ResizeForText(label); label.Visible = true; }
    }

    private void HandBackSoundtrack()
    {
        if (soundtrackHandedBack) return;
        soundtrackHandedBack = true;
        // Il prologo si chiude sempre da qui, sia con COMINCIA sia con la X:
        // la voce va fermata soltanto se è questa finestra ad averla avviata.
        if (narrationLinked) NarrationService.Stop();
        // Una scelta o uno skip non deve trascinarsi il tema del prologo.
        // Prima chiudiamo quel canale, poi entra il tema della Home.
        SoundtrackService.SilenceForNavigation();
        SoundtrackService.PlayForMood("alba", SoundtrackService.BackgroundVolume);
    }

    private static Color Blend(Color from, Color to, double amount)
    {
        var t = Math.Clamp(amount, 0, 1); return Color.FromArgb((int)(from.R + (to.R - from.R) * t), (int)(from.G + (to.G - from.G) * t), (int)(from.B + (to.B - from.B) * t));
    }

    // Le Label WinForms non ricalcolano l'altezza quando il testo viene scritto
    // dopo il layout. Senza questo passaggio il paragrafo resta alto una riga e
    // il resto viene visivamente tagliato invece di andare a capo.
    private static void ResizeForText(Label label)
    {
        var measured = TextRenderer.MeasureText(label.Text.Length == 0 ? "Ag" : label.Text, label.Font,
            new Size(label.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        label.Height = Math.Max(label.Font.Height + 5, measured.Height + 7);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { timer.Dispose(); artworkTimer.Dispose(); if (artworkBox?.Image is Image image && !ReferenceEquals(image, currentArtwork)) image.Dispose(); currentArtwork?.Dispose(); nextArtwork?.Dispose(); }
        base.Dispose(disposing);
    }
}
