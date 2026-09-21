using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Design system del portale.
///
/// La UI precedente posizionava ogni controllo a coordinate assolute su una
/// finestra da 1280×820 con pannelli alti 850: su uno schermo più piccolo o con
/// scaling DPI diverso da 100% i contenuti venivano tagliati, e i "pannelli"
/// erano etichette multi-riga. Qui esistono un unico set di colori/tipografia e
/// un piccolo insieme di componenti che si impaginano da soli.
/// </summary>
public static class UiTheme
{
    // Superfici: grafite profondo con carte leggermente più chiare, come i
    // portali motorsport in tema scuro.
    public static readonly Color Background = Color.FromArgb(7, 15, 26);
    public static readonly Color Surface = Color.FromArgb(12, 27, 43);
    public static readonly Color SurfaceRaised = Color.FromArgb(19, 39, 61);
    public static readonly Color Border = Color.FromArgb(42, 67, 91);
    public static readonly Color HeaderBackground = Color.FromArgb(6, 17, 30);

    // Testo.
    public static readonly Color TextPrimary = Color.FromArgb(238, 241, 246);
    public static readonly Color TextSecondary = Color.FromArgb(158, 167, 181);
    public static readonly Color TextMuted = Color.FromArgb(112, 121, 136);

    // Accenti.
    public static readonly Color Accent = Color.FromArgb(225, 29, 56);
    public static readonly Color AccentHover = Color.FromArgb(243, 52, 79);
    public static readonly Color AccentPressed = Color.FromArgb(176, 18, 40);
    public static readonly Color Positive = Color.FromArgb(64, 200, 132);
    public static readonly Color Warning = Color.FromArgb(240, 180, 60);
    public static readonly Color Info = Color.FromArgb(88, 160, 235);

    public const string FamilySans = "Segoe UI";
    public const string FamilySemibold = "Segoe UI Semibold";
    public const string FamilyMono = "Consolas";
    // Georgia e presente su Windows. Serve per la parte che si legge: un
    // carattere da interfaccia su tre colonne di numeri fa sembrare la
    // carriera un foglio di calcolo, non una storia.
    public const string FamilySerif = "Georgia";

    public static Font Display => new(FamilySans, 18F, FontStyle.Bold);
    public static Font Title => new(FamilySemibold, 12F);
    public static Font SectionLabel => new(FamilySemibold, 8.5F, FontStyle.Bold);
    public static Font Body => new(FamilySans, 9.5F);
    public static Font BodyStrong => new(FamilySemibold, 9.5F);
    public static Font Small => new(FamilySans, 8F);
    public static Font Metric => new(FamilySemibold, 14F);
    public static Font Mono => new(FamilyMono, 9F);

    // Tipografia editoriale: titolo, sottotitolo e prosa dell'articolo.
    // Corpo piu grande del testo d'interfaccia, perche questo si legge
    // davvero invece di essere consultato.
    public static Font Headline => new(FamilySerif, 19F, FontStyle.Bold);
    public static Font HeadlineSmall => new(FamilySerif, 15F, FontStyle.Bold);
    public static Font Standfirst => new(FamilySerif, 11.5F, FontStyle.Italic);
    public static Font Prose => new(FamilySerif, 10.5F);
    public static Font ProseStrong => new(FamilySerif, 10.5F, FontStyle.Bold);
    public static Font Kicker => new(FamilySemibold, 8.5F, FontStyle.Bold);
    public static Font Byline => new(FamilySans, 8.5F, FontStyle.Italic);

    public const int Gutter = 14;
    public const int CardPadding = 16;

    /// <summary>
    /// Pannello con illustrazione di fondo e velo cromatico. Le schermate della
    /// carriera possono così avere una profondità visiva senza sacrificare la
    /// leggibilità dei testi: l'immagine resta decorativa, mentre il contenuto
    /// dei controlli figli rimane completamente invariato e cliccabile.
    /// </summary>
    public static ThemedBackdropPanel BackdropPanel(
        string assetName,
        Color? veil = null,
        int imageAlpha = 58,
        Padding? padding = null)
    {
        var panel = new ThemedBackdropPanel
        {
            BackColor = Background,
            Padding = padding ?? Padding.Empty
        };
        panel.SetBackdrop(AssetPaths.File(assetName), veil ?? Color.FromArgb(190, Background));
        panel.ImageAlpha = Math.Clamp(imageAlpha, 0, 255);
        RoundCorners(panel, 12);
        return panel;
    }

    /// <summary>Angoli morbidi applicati senza cambiare dimensioni o hit area.</summary>
    public static void RoundCorners(Control control, int radius)
    {
        void Apply()
        {
            if (control.IsDisposed || control.Width < 2 || control.Height < 2) return;
            var rect = new Rectangle(0, 0, control.Width, control.Height);
            using var path = RoundedPath(rect, Math.Min(radius, Math.Min(control.Width, control.Height) / 2));
            control.Region?.Dispose();
            control.Region = new Region(path);
        }
        control.SizeChanged += (_, _) => Apply();
        control.HandleCreated += (_, _) => Apply();
        Apply();
    }

    private static GraphicsPath RoundedPath(Rectangle rect, int radius)
    {
        var diameter = Math.Max(2, radius * 2);
        var path = new GraphicsPath();
        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawRoundedBorder(Graphics graphics, Rectangle bounds, Color color, float width = 1f, int radius = 12)
    {
        var inset = Math.Max(0.5f, width / 2f);
        var rect = RectangleF.Inflate(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), -inset, -inset);
        using var path = RoundedPath(Rectangle.Round(rect), Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2));
        using var pen = new Pen(color, width);
        graphics.DrawPath(pen, path);
    }

    /// <summary>
    /// Carta del portale: bordo sottile, filetto d'accento e titolo di sezione.
    /// Il titolo vive in una riga dedicata di un TableLayoutPanel: con i soli
    /// Dock=Top l'ordine di aggiunta lo faceva finire in fondo alla carta.
    /// </summary>
    public static Panel Card(string sectionLabel, out Panel content, Color? accent = null)
    {
        var card = new Panel { BackColor = Surface, Margin = new Padding(0, 0, Gutter, Gutter) };
        RoundCorners(card, 12);
        var accentColor = accent ?? Accent;
        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Border, 1);
            DrawRoundedBorder(e.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), Border, 1, 12);
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, 3, card.Height);
        };

        var hasLabel = !string.IsNullOrWhiteSpace(sectionLabel);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Surface,
            Padding = new Padding(CardPadding + 4, hasLabel ? CardPadding - 4 : CardPadding, CardPadding, CardPadding)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, hasLabel ? 24 : 0));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        if (hasLabel)
            layout.Controls.Add(new Label
            {
                Text = sectionLabel.ToUpperInvariant(), Dock = DockStyle.Fill,
                ForeColor = accentColor, Font = SectionLabel, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0)
            }, 0, 0);
        var inner = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0) };
        layout.Controls.Add(inner, 0, 1);
        card.Controls.Add(layout);
        content = inner;
        return card;
    }

    /// <summary>Versione illustrata di <see cref="Card"/> per le aree principali della Home.</summary>
    public static ThemedBackdropPanel BackdropCard(
        string sectionLabel,
        string assetName,
        out Panel content,
        Color? accent = null,
        int imageAlpha = 84,
        int veilAlpha = 154)
    {
        var accentColor = accent ?? Accent;
        var card = BackdropPanel(
            assetName,
            Color.FromArgb(Math.Clamp(veilAlpha, 0, 255), Surface),
            imageAlpha,
            new Padding(0));
        card.Margin = new Padding(0, 0, Gutter, Gutter);
        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawRoundedBorder(e.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), Border, 1, 12);
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, 3, card.Height);
        };

        var hasLabel = !string.IsNullOrWhiteSpace(sectionLabel);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent,
            Padding = new Padding(CardPadding + 4, hasLabel ? CardPadding - 4 : CardPadding, CardPadding, CardPadding)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, hasLabel ? 24 : 0));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        if (hasLabel)
            layout.Controls.Add(new Label
            {
                Text = sectionLabel.ToUpperInvariant(), Dock = DockStyle.Fill,
                ForeColor = accentColor, Font = SectionLabel, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0),
                BackColor = Color.Transparent
            }, 0, 0);
        var inner = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0) };
        layout.Controls.Add(inner, 0, 1);
        card.Controls.Add(layout);
        content = inner;
        return card;
    }

    /// <summary>Riquadro numerico compatto per la barra dei dati di carriera.</summary>
    public static Panel StatTile(string caption, string value, int width = 152, Color? valueColor = null)
    {
        var tile = new Panel { BackColor = SurfaceRaised, Margin = new Padding(0, 0, 8, 0), Width = width, Height = 74 };
        tile.Paint += (_, e) => { using var pen = new Pen(Border, 1); e.Graphics.DrawRectangle(pen, 0, 0, tile.Width - 1, tile.Height - 1); };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = SurfaceRaised, Padding = new Padding(12, 8, 10, 8) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = caption.ToUpperInvariant(), Dock = DockStyle.Fill, ForeColor = TextMuted, Font = Small, Margin = new Padding(0), AutoEllipsis = true }, 0, 0);
        layout.Controls.Add(new Label
        {
            Text = value, Dock = DockStyle.Fill, ForeColor = valueColor ?? TextPrimary, Font = Metric,
            TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Name = "value", Margin = new Padding(0)
        }, 0, 1);
        tile.Controls.Add(layout);
        return tile;
    }

    /// <summary>Controllo segmentato: alternativa tematizzabile al TabControl di sistema.</summary>
    public static Panel Segmented(IReadOnlyList<string> options, Action<int> onSelect, out Action<int> select)
    {
        var bar = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Surface };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Surface };
        var buttons = new List<Button>();
        for (var i = 0; i < options.Count; i++)
        {
            var index = i;
            var button = new Button
            {
                Text = options[i], FlatStyle = FlatStyle.Flat, Font = Small, Height = 26, Width = 128,
                BackColor = SurfaceRaised, ForeColor = TextSecondary, Margin = new Padding(0, 0, 4, 0), UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Border;
            button.Click += (_, _) => { onSelect(index); Highlight(buttons, index); };
            buttons.Add(button);
            flow.Controls.Add(button);
        }
        bar.Controls.Add(flow);
        select = index => { onSelect(index); Highlight(buttons, index); };
        Highlight(buttons, 0);
        return bar;
    }

    private static void Highlight(List<Button> buttons, int selected)
    {
        for (var i = 0; i < buttons.Count; i++)
        {
            buttons[i].BackColor = i == selected ? Accent : SurfaceRaised;
            buttons[i].ForeColor = i == selected ? Color.White : TextSecondary;
        }
    }

    public static void UpdateStatTile(Panel tile, string value, Color? valueColor = null)
    {
        // La ricerca deve attraversare i figli: da quando il riquadro usa un
        // TableLayoutPanel interno, l'etichetta non è più figlia diretta e con
        // searchAllChildren = false l'aggiornamento non trovava nulla — i riquadri
        // restavano fermi sui valori iniziali.
        if (tile.Controls.Find("value", true).FirstOrDefault() is not Label label) return;
        label.Text = value;
        if (valueColor.HasValue) label.ForeColor = valueColor.Value;
    }

    public static Button PrimaryButton(string text)
    {
        var button = new Button
        {
            Text = text, FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White,
            Font = BodyStrong, Height = 40, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 8),
            TextAlign = ContentAlignment.MiddleCenter, UseVisualStyleBackColor = false, AutoEllipsis = true
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = AccentHover;
        button.FlatAppearance.MouseDownBackColor = AccentPressed;
        RoundCorners(button, 8);
        return button;
    }

    public static Button SecondaryButton(string text)
    {
        var button = new Button
        {
            Text = text, FlatStyle = FlatStyle.Flat, BackColor = SurfaceRaised, ForeColor = TextPrimary,
            Font = Body, Height = 36, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 6),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 8, 0),
            UseVisualStyleBackColor = false, AutoEllipsis = true,
            // Senza questo il pannello scorrevole si sposta da solo sul pulsante
            // che riceve il fuoco, nascondendo la prima carta della colonna.
            TabStop = false
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 47, 59);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 23, 29);
        RoundCorners(button, 7);
        return button;
    }

    /// <summary>
    /// Pulsante della testata. La larghezza richiesta è un minimo, non un
    /// vincolo: le etichette venivano troncate ("IMPOSTA...") perché i valori
    /// fissi non tenevano conto del testo reale né del font di sistema.
    /// </summary>
    public static Button GhostButton(string text, int width)
    {
        var needed = TextRenderer.MeasureText(text, Body).Width + 26;
        var button = new Button
        {
            Text = text, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = TextSecondary,
            Font = Body, Width = Math.Max(width, needed), Height = 32, UseVisualStyleBackColor = false, AutoEllipsis = true
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.MouseOverBackColor = SurfaceRaised;
        RoundCorners(button, 7);
        return button;
    }

    /// <summary>Blocco di testo scorrevole: sostituisce le Label multi-riga che venivano tagliate.</summary>
    public static RichTextBox TextBlock(bool monospace = false)
    {
        var box = new RichTextBox
        {
            Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, ReadOnly = true,
            BackColor = Surface, ForeColor = TextSecondary, Font = monospace ? Mono : Body,
            ScrollBars = RichTextBoxScrollBars.Vertical, WordWrap = true, DetectUrls = false,
            TabStop = false, Margin = new Padding(0)
        };
        return box;
    }

    public static Label Caption(string text, Color? color = null) => new()
    {
        Text = text, AutoSize = false, Dock = DockStyle.Top, Height = 20,
        ForeColor = color ?? TextMuted, Font = Small, TextAlign = ContentAlignment.MiddleLeft
    };

    public static Label Heading(string text, Color? color = null) => new()
    {
        Text = text, AutoSize = false, Dock = DockStyle.Top, Height = 30,
        ForeColor = color ?? TextPrimary, Font = Title, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
    };

    /// <summary>Riga di separazione fra blocchi di una carta.</summary>
    public static Panel Divider() => new() { Dock = DockStyle.Top, Height = 1, BackColor = Border, Margin = new Padding(0, 6, 0, 6) };

    /// <summary>Colore semantico per un valore su scala 0-100.</summary>
    public static Color ScaleColor(int value) => value switch
    {
        >= 70 => Positive,
        >= 40 => TextPrimary,
        >= 20 => Warning,
        _ => Accent
    };

    public static Color MoneyColor(long value) => value < 0 ? Accent : value == 0 ? TextSecondary : Positive;
}

/// <summary>
/// Contenitore leggero per fondi illustrati: disegna l'immagine "sotto" ai
/// controlli figli e poi un velo blu/nero semitrasparente. Non usa proprietà
/// BackgroundImage di WinForms, che su immagini grandi forza una copia per ogni
/// ridisegno e rende difficile mantenere il contrasto dei testi.
/// </summary>
public sealed class ThemedBackdropPanel : Panel
{
    private Image? backdrop;
    private Color veil = Color.FromArgb(190, 7, 15, 26);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ImageAlpha { get; set; } = 58;

    public ThemedBackdropPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void SetBackdrop(string path, Color overlay)
    {
        var replacement = LoadBitmap(path);
        var previous = backdrop;
        backdrop = replacement;
        veil = overlay;
        previous?.Dispose();
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        if (backdrop != null && ClientSize.Width > 0 && ClientSize.Height > 0)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            var source = new Rectangle(0, 0, backdrop.Width, backdrop.Height);
            var destination = CoverRectangle(source.Size, ClientSize);
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            var matrix = new System.Drawing.Imaging.ColorMatrix
            {
                Matrix00 = 1f, Matrix11 = 1f, Matrix22 = 1f,
                Matrix33 = Math.Clamp(ImageAlpha, 0, 255) / 255f,
                Matrix44 = 1f
            };
            attributes.SetColorMatrix(matrix);
            e.Graphics.DrawImage(backdrop, destination, source.X, source.Y, source.Width, source.Height,
                GraphicsUnit.Pixel, attributes);
        }

        using var overlay = new SolidBrush(veil);
        e.Graphics.FillRectangle(overlay, ClientRectangle);
        // Una fascia scura in alto dà profondità e rende i titoli leggibili
        // anche quando l'immagine sottostante contiene zone molto luminose.
        if (Height > 0)
        {
            using var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, Width), Math.Min(110, Height)),
                Color.FromArgb(84, 3, 10, 20), Color.FromArgb(0, 3, 10, 20),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(gradient, 0, 0, Width, Math.Min(110, Height));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) backdrop?.Dispose();
        base.Dispose(disposing);
    }

    private static Bitmap? LoadBitmap(string path)
    {
        try
        {
            if (!System.IO.File.Exists(path)) return null;
            using var source = Image.FromFile(path);
            return new Bitmap(source);
        }
        catch (Exception error)
        {
            CareerLog.Warn("ui", $"sfondo non caricato: {error.Message}");
            return null;
        }
    }

    private static Rectangle CoverRectangle(Size source, Size destination)
    {
        if (source.Width <= 0 || source.Height <= 0) return new Rectangle(Point.Empty, destination);
        var scale = Math.Max(destination.Width / (double)source.Width, destination.Height / (double)source.Height);
        var width = (int)Math.Ceiling(source.Width * scale);
        var height = (int)Math.Ceiling(source.Height * scale);
        return new Rectangle((destination.Width - width) / 2, (destination.Height - height) / 2, width, height);
    }
}
