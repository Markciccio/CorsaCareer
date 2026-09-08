using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>Una battuta nella scena anime: un personaggio, un'espressione, una voce.</summary>
public sealed record AnimeDialogueLine(string Speaker, string Text, string PortraitFile, string Expression);

/// <summary>
/// Scena narrativa a tutto schermo. Il balloon viene scritto in modo progressivo;
/// il passaggio al personaggio successivo e' sempre esplicito.
/// </summary>
public sealed class AnimeDialogueDialog : CareerDialog
{
    private readonly IReadOnlyList<AnimeDialogueLine> lines;
    private readonly Label speakerLabel = new();
    private readonly Label quoteLabel = new();
    private readonly SmoothPictureBox portrait = new();
    private readonly Button next = new();
    private readonly System.Windows.Forms.Timer typing = new() { Interval = TypingIntervalMs };

    /// <summary>
    /// La scena scorre anche da sola: quando una battuta è stata scritta per
    /// intero parte questo conto alla rovescia, e allo scadere si passa alla
    /// successiva. Sull'ultima chiude la scena. Un clic in qualunque momento ha
    /// la precedenza: chi vuole leggere con calma o accelerare lo fa lo stesso.
    /// </summary>
    private readonly System.Windows.Forms.Timer autoAdvance = new() { Interval = AutoAdvanceMs };

    /// <summary>Quanto resta a schermo una battuta già scritta, prima di proseguire.</summary>
    public const int AutoAdvanceMs = 15000;

    /// <summary>
    /// Ritmo della scrittura. A 24 ms per singola lettera una battuta di
    /// centocinquanta caratteri chiedeva quasi quattro secondi: si finiva di
    /// leggerla prima che venisse scritta, e la scena diventava un'attesa.
    /// </summary>
    public const int TypingIntervalMs = 12;

    /// <summary>Lettere scritte a ogni fotogramma.</summary>
    public const int CharactersPerTick = 4;
    private int lineIndex;
    private int characterIndex;
    private bool completed;
    private Bitmap? portraitImage;

    public AnimeDialogueDialog(string title, IReadOnlyList<AnimeDialogueLine> lines)
    {
        this.lines = lines;
        Text = title;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(5, 7, 11);
        KeyPreview = true;
        KeyDown += OnKeyDown;

        // Il titolo mostrato era un testo fisso ("Quello che succede dopo il
        // cronometro"), sempre uguale anche per le scene che precedono il
        // test: sembrava annunciare un commento a un risultato che non
        // c'era ancora. Ora riprende il titolo vero passato dal chiamante.
        var displayTitle = title.StartsWith("CorsaCareer — ", StringComparison.Ordinal) ? title["CorsaCareer — ".Length..] : title;
        var top = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = UiTheme.HeaderBackground, Padding = new Padding(44, 18, 44, 12) };
        top.Controls.Add(new Label { Text = "SCENA ANIME", Dock = DockStyle.Top, Height = 22, Font = UiTheme.Kicker, ForeColor = UiTheme.Accent });
        top.Controls.Add(new Label { Text = displayTitle, Dock = DockStyle.Bottom, Height = 28, Font = UiTheme.Title, ForeColor = UiTheme.TextPrimary });

        var body = new SmoothTableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.FromArgb(5, 7, 11), Padding = new Padding(48, 34, 48, 28) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
        portrait = new SmoothPictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(9, 12, 18), SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 0, 28, 0) };
        body.Controls.Add(portrait, 0, 0);

        var right = new SmoothTableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.Transparent, Padding = new Padding(10, 28, 14, 28) };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        speakerLabel.Dock = DockStyle.Fill; speakerLabel.Font = new Font(UiTheme.FamilySemibold, 20F, FontStyle.Bold); speakerLabel.ForeColor = UiTheme.Accent; speakerLabel.TextAlign = ContentAlignment.MiddleLeft;
        quoteLabel.Dock = DockStyle.Fill; quoteLabel.AutoSize = false; quoteLabel.Font = new Font(UiTheme.FamilySans, 22F); quoteLabel.ForeColor = Color.FromArgb(27, 30, 36); quoteLabel.BackColor = Color.FromArgb(243, 239, 226); quoteLabel.Padding = new Padding(34, 28, 34, 28); quoteLabel.UseMnemonic = false;
        next.Text = "AVANTI"; next.Dock = DockStyle.Fill; next.Font = UiTheme.BodyStrong; next.BackColor = UiTheme.Accent; next.ForeColor = Color.White; next.FlatStyle = FlatStyle.Flat; next.FlatAppearance.BorderSize = 0; next.Click += Advance;
        right.Controls.Add(speakerLabel, 0, 0); right.Controls.Add(quoteLabel, 0, 1); right.Controls.Add(next, 0, 2);
        body.Controls.Add(right, 1, 0);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = UiTheme.HeaderBackground, Padding = new Padding(44, 10, 44, 10) };
        var skip = new Button { Text = "SALTA SCENA", Dock = DockStyle.Right, Width = 150, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = UiTheme.TextSecondary, Font = UiTheme.Small };
        skip.FlatAppearance.BorderColor = UiTheme.Border; skip.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        footer.Controls.Add(skip);
        var hint = new Label
        {
            Dock = DockStyle.Left, Width = 620, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false,
            Text = "La scena prosegue da sola. Clic, INVIO o BARRA per andare avanti subito · ESC per saltare."
        };
        footer.Controls.Add(hint);
        Controls.Add(body); Controls.Add(footer); Controls.Add(top);
        typing.Tick += TypeNextCharacter;
        autoAdvance.Tick += (_, _) => { autoAdvance.Stop(); GoToNextLineOrClose(); };
        Shown += (_, _) => ShowLine(0);
    }

    private void ShowLine(int index)
    {
        if (lines.Count == 0) { DialogResult = DialogResult.OK; Close(); return; }
        autoAdvance.Stop();
        lineIndex = Math.Clamp(index, 0, lines.Count - 1);
        characterIndex = 0;
        completed = false;
        var line = lines[lineIndex];
        speakerLabel.Text = line.Speaker.ToUpperInvariant();
        quoteLabel.Text = "";
        next.Text = NextLabel();
        LoadPortrait(line.PortraitFile);
        typing.Start();
    }

    private void TypeNextCharacter(object? sender, EventArgs e)
    {
        var text = lines[lineIndex].Text;
        if (characterIndex < text.Length)
        {
            characterIndex = Math.Min(text.Length, characterIndex + CharactersPerTick);
            // Aggiorniamo solo il balloon: il pannello che contiene il ritratto
            // e' doppio-bufferizzato e non viene più ridisegnato a ogni lettera.
            quoteLabel.Text = $"“{text[..characterIndex]}”";
            return;
        }
        typing.Stop(); completed = true; next.Text = NextLabel();
        // La battuta è tutta a schermo: da qui parte il tempo di lettura.
        autoAdvance.Start();
    }

    /// <summary>
    /// Il passo successivo della scena: la battuta dopo, oppure la chiusura se
    /// questa era l'ultima. Usato sia dal clic sia dal tempo di lettura.
    /// </summary>
    private void GoToNextLineOrClose()
    {
        if (IsDisposed) return;
        if (lineIndex >= lines.Count - 1) { DialogResult = DialogResult.OK; Close(); return; }
        ShowLine(lineIndex + 1);
    }

    /// <summary>
    /// Il pulsante dichiara sempre dove porta, non cosa fa all'animazione.
    /// "Scrivi la battuta" chiedeva al giocatore di annullare un effetto: un
    /// comando che serve a evitare l'interfaccia non deve esistere.
    /// </summary>
    private string NextLabel() => lineIndex >= lines.Count - 1 ? "CHIUDI SCENA" : "CONTINUA";

    private void Advance(object? sender, EventArgs e)
    {
        if (!completed)
        {
            // Primo clic: la battuta compare tutta. Il tempo di lettura riparte
            // da adesso, altrimenti chi accelera la scrittura si vedrebbe
            // scappare la battuta dopo pochi istanti.
            var text = lines[lineIndex].Text; characterIndex = text.Length; quoteLabel.Text = $"“{text}”"; typing.Stop(); completed = true; next.Text = NextLabel();
            autoAdvance.Stop(); autoAdvance.Start();
            return;
        }
        autoAdvance.Stop();
        GoToNextLineOrClose();
    }

    private void LoadPortrait(string file)
    {
        portraitImage?.Dispose(); portraitImage = null; portrait.Image = null;
        var path = AssetPaths.File(file);
        if (!File.Exists(path)) return;
        using var source = Image.FromFile(path); portraitImage = new Bitmap(source); portrait.Image = portraitImage;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter or Keys.Space) { e.Handled = true; Advance(this, EventArgs.Empty); }
        else if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { typing.Stop(); typing.Dispose(); autoAdvance.Stop(); autoAdvance.Dispose(); portraitImage?.Dispose(); }
        base.Dispose(disposing);
    }

    private sealed class SmoothPictureBox : PictureBox
    {
        public SmoothPictureBox()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
    }

    private sealed class SmoothTableLayoutPanel : TableLayoutPanel
    {
        public SmoothTableLayoutPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
    }
}
