using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Il conto della carriera.
///
/// Prima era una stringa di due righe nello stesso corpo minuscolo del resto:
/// il denaro è il vincolo su cui si regge ogni decisione, e non si distingueva
/// da una nota a margine. Qui la cifra è la cosa più grande della fascia, le
/// voci hanno ognuna il suo spazio, e una variazione si annuncia da sola con un
/// lampeggio: il giocatore deve accorgersi che ha speso o incassato anche se in
/// quel momento stava guardando altro.
/// </summary>
public sealed class BudgetPanel : Panel
{
    private readonly Label amount = new();
    private readonly Label status = new();
    private readonly Label breakdown = new();
    private readonly Label movements = new();
    private readonly Label fitnessValue = new();
    private readonly Label trustValue = new();
    private readonly Label schoolValue = new();
    private readonly Panel fitnessProgress = ProgressTrack();
    private readonly Panel influencerProgress = ProgressTrack();
    private readonly Panel schoolProgress = ProgressTrack();
    private readonly Label pilotName = new();
    private readonly Label pilotDetails = new();
    private readonly Label pilotSchool = new();
    private ThemedBackdropPanel? pilotCard;
    private TableLayoutPanel? metricGrid;
    private Panel? tutorialOverlay;
    private Label? tutorialTitle;
    private Label? tutorialBody;
    private Button? tutorialNext;
    private int tutorialStep;
    private Control[] tutorialTargets = [];
    /// <summary>La scheda del tutorial: viene spostata accanto al riquadro spiegato.</summary>
    private Panel? tutorialCard;

    // FitnessRequested, SponsorRequested e CommunityRequested sono spariti da
    // qui: erano gli eventi dei tre pulsanti che i riquadri avevano prima di
    // diventare puri numeri. Nessuno li sottoscriveva piu' da nessuna parte —
    // i tre pulsanti a cui erano legati (fitnessAction, sponsorAction,
    // communityAction) non venivano piu' aggiunti a nessun layout — quindi
    // erano diventati un click che non poteva mai arrivare a nessuno.

    private readonly System.Windows.Forms.Timer pulseTimer = new() { Interval = CareerTransitions.FrameMilliseconds };
    private int pulseFrame;
    private Color pulseColor = UiTheme.Warning;
    private int? lastCash;

    /// <summary>Durata del lampeggio: abbastanza per accorgersene, non da distrarre.</summary>
    public const int PulseFrames = 34;

    public BudgetPanel(bool includePilot = true)
    {
        BackColor = UiTheme.SurfaceRaised;
        Padding = new Padding(16, 8, 16, 8);
        DoubleBuffered = true;

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = includePilot ? 5 : 4, RowCount = 2, BackColor = Color.Transparent,
            MinimumSize = new Size(0, 176)
        };
        // I quattro indicatori hanno lo stesso peso visivo: il livello scuola
        // non e' un dettaglio dell'influencer, ma un parametro autonomo che il
        // pilota deve tenere d'occhio ogni giorno.
        if (includePilot) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        // Una riga unica per le quattro tessere evita che il TableLayoutPanel
        // distribuisca lo spazio su una riga fantasma e tagli i pulsanti.
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        // 40 e non 24: la riga dei movimenti va a capo — e' una frase, non
        // un'etichetta — e in 24 pixel la seconda riga finiva dentro la
        // cornice del budget, sopra il dettaglio degli sponsor.
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        // La cifra: è il numero che decide se una gara si può fare o no.
        amount.Font = new Font(UiTheme.FamilySemibold, 25F, FontStyle.Bold);
        amount.ForeColor = UiTheme.TextPrimary;
        amount.Dock = DockStyle.Fill;
        // In alto nella propria riga: centrata, una cifra alta sconfinava
        // verso il basso sulla riga successiva.
        amount.TextAlign = ContentAlignment.TopLeft;
        amount.AutoEllipsis = true;
        amount.TextAlign = ContentAlignment.MiddleLeft;
        amount.AutoSize = false;
        amount.Width = 168;
        amount.UseMnemonic = false;
        status.Font = UiTheme.BodyStrong;
        status.ForeColor = UiTheme.TextSecondary;
        status.Dock = DockStyle.Fill;
        // Allineato in alto: quando diventa un avviso occupa due righe, e
        // ancorandolo in basso la seconda finiva fuori dal riquadro.
        status.TextAlign = ContentAlignment.TopLeft;
        status.UseMnemonic = false;
        breakdown.Font = UiTheme.Small;
        breakdown.ForeColor = UiTheme.TextMuted;
        breakdown.Dock = DockStyle.Fill;
        breakdown.TextAlign = ContentAlignment.TopLeft;
        breakdown.UseMnemonic = false;
        var account = UiTheme.BackdropPanel(
            "ui-backgrounds/metric-budget-garage-v1.png",
            Color.FromArgb(104, UiTheme.Surface),
            imageAlpha: 176,
            padding: new Padding(12, 6, 12, 4));
        account.Dock = DockStyle.Fill;
        account.Margin = new Padding(6, 0, 6, 0);
        account.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Warning, 2);
            UiTheme.DrawRoundedBorder(e.Graphics, new Rectangle(0, 0, account.Width - 1, account.Height - 1), UiTheme.Warning, 2, 12);
            using var brush = new SolidBrush(UiTheme.Warning);
            e.Graphics.FillRectangle(brush, 0, 0, 3, account.Height);
            using var rule = new Pen(Color.FromArgb(110, UiTheme.Warning), 1);
            e.Graphics.DrawLine(rule, 14, 60, account.Width - 14, 60);
        };
        var accountLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        // 54 e non 46: la cifra e' scritta a 25 punti in grassetto e con
        // l'interlinea ne occupa quasi cinquanta. In 46 pixel la parte bassa
        // del numero finiva sotto il pulsante degli sponsor.
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        accountLayout.Controls.Add(CreateMetricHeader("BUDGET DISPONIBILE", UiTheme.Warning, "", null), 0, 0);
        accountLayout.Controls.Add(amount, 0, 1);
        // I tre riquadri non aprono piu' niente.
        //
        // Ognuno portava a una schermata diversa — sponsor, attivita' del
        // pilota, social — e ognuna mostrava un pezzo del bilancio delle ore
        // senza vedere gli altri due: dal social non si poteva sapere quante
        // ore restavano dopo la palestra, perche' la palestra stava altrove.
        // Adesso la giornata si decide in un posto solo, il sottomenu' OGGI,
        // e questi riquadri fanno quello che il loro titolo promette: mostrano
        // un numero.
        accountLayout.Controls.Add(status, 0, 2);
        accountLayout.Controls.Add(breakdown, 0, 3);
        account.Controls.Add(accountLayout);
        var firstMetricColumn = includePilot ? 1 : 0;
        if (includePilot) grid.Controls.Add(BuildPilotCard(), 0, 0);
        grid.Controls.Add(account, firstMetricColumn, 0);
        metricGrid = grid;
        tutorialTargets = [account];

        grid.Controls.Add(CreateMetricCard("FORMA FISICA", fitnessValue, "", UiTheme.Positive,
            () => { }, new Button(), showAction: false,
            iconAsset: "metric-fitness-manga-v1.png", progressBar: fitnessProgress), firstMetricColumn + 1, 0);
        grid.Controls.Add(CreateMetricCard("LIVELLO INFLUENCER", trustValue, "", UiTheme.Info,
            () => { }, new Button(), showAction: false, iconAsset: "metric-influencer-manga-v1.png", progressBar: influencerProgress), firstMetricColumn + 2, 0);
        grid.Controls.Add(CreateMetricCard("LIVELLO SCUOLA", schoolValue, "", UiTheme.Warning,
            () => { }, new Button(), showAction: false, iconAsset: "metric-school-manga-v1.png", progressBar: schoolProgress), firstMetricColumn + 3, 0);
        tutorialTargets = [account, grid.GetControlFromPosition(firstMetricColumn + 1, 0)!, grid.GetControlFromPosition(firstMetricColumn + 2, 0)!, grid.GetControlFromPosition(firstMetricColumn + 3, 0)!];

        movements.Font = UiTheme.Small;
        movements.ForeColor = UiTheme.TextMuted;
        movements.Dock = DockStyle.Fill;
        movements.Margin = new Padding(6, 4, 6, 0);
        // Su due righe: l'ultimo movimento e' una frase, non un'etichetta, e
        // troncarlo a meta lo rendeva illeggibile.
        movements.AutoEllipsis = false;
        movements.UseMnemonic = false;
        grid.Controls.Add(movements, 0, 1);
        grid.SetColumnSpan(movements, includePilot ? 5 : 4);

        Controls.Add(grid);

        pulseTimer.Tick += (_, _) =>
        {
            pulseFrame++;
            if (pulseFrame >= PulseFrames)
            {
                pulseTimer.Stop();
                pulseFrame = 0;
            }
            Invalidate();
        };
    }

    /// <summary>
    /// L'intestazione di un riquadro: l'icona e il titolo, sulla stessa riga.
    ///
    /// I tre riquadri erano tre scritte in maiuscolo dello stesso colore e
    /// della stessa misura: per capire quale fosse quale bisognava leggerle.
    /// Un simbolo si riconosce prima di leggere, ed e' tutto quello che serve
    /// a un cruscotto.
    /// </summary>
    /// <summary>
    /// Un riquadro del cruscotto: titolo e numero.
    ///
    /// Ci ho provato a metterci un'icona e il risultato era peggio del
    /// problema: le tavole in <c>assets/ui-icons</c> sono disegni grandi, e
    /// ridotte a sedici pixel diventano un grumo. Per giunta un'etichetta
    /// disegna immagine e testo nello stesso rettangolo, quindi il grumo
    /// finiva SOPRA le prime due lettere: «BUDGET DISPONIBILE» si leggeva
    /// «DGET DISPONIBILE».
    ///
    /// Un'icona qui serve — il titolo si riconoscerebbe prima di leggerlo — ma
    /// serve disegnata per questa misura. Finche' non c'e', il titolo da solo
    /// e' meglio di un titolo mangiato.
    /// </summary>
    private static Control CreateMetricCard(string title, Label value, string actionText, Color accent, Action onAction, Button action, bool showAction = true, string? iconText = null, string? iconAsset = null, Panel? progressBar = null)
    {
        var card = UiTheme.BackdropPanel(
            MetricBackdrop(title),
            Color.FromArgb(104, UiTheme.Surface),
            imageAlpha: 176,
            padding: new Padding(12, 6, 12, 4));
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(6, 0, 6, 0);
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(accent, 2);
            UiTheme.DrawRoundedBorder(e.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), accent, 2, 12);
            using var brush = new SolidBrush(accent);
            e.Graphics.FillRectangle(brush, 0, 0, 3, card.Height);
            using var rule = new Pen(Color.FromArgb(112, accent), 1);
            e.Graphics.DrawLine(rule, 14, 60, card.Width - 14, 60);
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
        if (progressBar != null && !showAction) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
        if (showAction) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        // I quattro box usano ora illustrazioni di fondo dedicate: l'icona
        // piccola sopra il titolo diventava rumore e copriva le prime lettere.
        var caption = CreateMetricHeader(title, accent, "", null);
        value.Font = new Font(UiTheme.FamilySemibold, 22F, FontStyle.Bold);
        value.ForeColor = UiTheme.TextPrimary;
        value.Dock = DockStyle.Fill;
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.UseMnemonic = false;
        var descriptor = new Label
        {
            Text = MetricDescriptor(title), Dock = DockStyle.Fill, AutoEllipsis = true,
            Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false,
            Padding = new Padding(0, 0, 2, 0)
        };
        if (showAction) ConfigureActionButton(action, actionText, accent, onAction);
        layout.Controls.Add(caption, 0, 0);
        layout.Controls.Add(value, 0, 1);
        layout.Controls.Add(descriptor, 0, 2);
        if (progressBar != null && !showAction) layout.Controls.Add(progressBar, 0, 3);
        if (showAction) layout.Controls.Add(action, 0, 3);
        card.Controls.Add(layout);
        return card;
    }

    /// <summary>Restituisce la carta verticale del pilota per la colonna Home.</summary>
    public Control CreatePilotCardForHome() => BuildPilotCard();

    private static string MetricDescriptor(string title) => title switch
    {
        var value when value.StartsWith("FORMA", StringComparison.OrdinalIgnoreCase)
            => "Energia, resistenza e recupero in pista.",
        var value when value.StartsWith("LIVELLO INFLUENCER", StringComparison.OrdinalIgnoreCase)
            => "Presenza, pubblico e attenzione dei media.",
        var value when value.StartsWith("LIVELLO SCUOLA", StringComparison.OrdinalIgnoreCase)
            => "Rendimento scolastico: la carriera parte da qui.",
        _ => "Risorse disponibili per il prossimo passo."
    };

    private static Panel ProgressTrack()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(41, 49, 61), Margin = new Padding(0, 1, 0, 2), Tag = 0 };
        panel.Paint += PaintProgressTrack;
        return panel;
    }

    private static string MetricBackdrop(string title) => title switch
    {
        var value when value.StartsWith("FORMA", StringComparison.OrdinalIgnoreCase)
            => "ui-backgrounds/metric-fitness-paddock-v1.png",
        var value when value.StartsWith("LIVELLO INFLUENCER", StringComparison.OrdinalIgnoreCase)
            => "ui-backgrounds/metric-influencer-media-v1.png",
        var value when value.StartsWith("LIVELLO SCUOLA", StringComparison.OrdinalIgnoreCase)
            => "ui-backgrounds/metric-school-study-v1.png",
        _ => "ui-backgrounds/metric-budget-garage-v1.png"
    };

    private static void SetProgress(Panel panel, int value, Color accent)
    {
        panel.Tag = Math.Clamp(value, 0, 100);
        panel.BackColor = Color.FromArgb(41, 49, 61);
        panel.ForeColor = accent;
        panel.AccessibleName = $"{value}/100";
        panel.Invalidate();
    }

    private static void PaintProgressTrack(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel panel) return;
        var value = panel.Tag is int number ? number : 0;
        var width = Math.Max(0, (int)Math.Round((panel.Width - 2) * (value / 100d)));
        using var brush = new SolidBrush(panel.ForeColor == Color.Empty ? UiTheme.Positive : panel.ForeColor);
        e.Graphics.FillRectangle(brush, 1, 1, width, Math.Max(1, panel.Height - 2));
    }

    /// <summary>
    /// Intestazione riconoscibile a colpo d'occhio: il simbolo resta separato
    /// dal testo, così anche con lo scaling di Windows non mangia le prime
    /// lettere del titolo.
    /// </summary>
    private static Control CreateMetricHeader(string title, Color accent, string iconText, string? iconAsset = null)
    {
        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, BackColor = Color.Transparent, Margin = new Padding(0),
            Padding = new Padding(0), AutoSize = false
        };
        var imagePath = string.IsNullOrWhiteSpace(iconAsset) ? "" : AssetPaths.File("ui-icons", iconAsset);
        var image = LoadMetricIcon(imagePath);
        if (image != null)
        {
            header.Controls.Add(new PictureBox
            {
                Image = image, Width = 60, Height = 58, SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent, Margin = new Padding(0, 0, 4, 0), TabStop = false,
                AccessibleName = title
            });
        }
        else
        {
            header.Controls.Add(new Label
            {
                Text = iconText, Width = 42, Height = 58, AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Symbol", 17F, FontStyle.Bold),
                ForeColor = accent, UseMnemonic = false, Margin = new Padding(0, -1, 4, 0)
            });
        }
        header.Controls.Add(new Label
        {
            Text = title, AutoSize = false, Width = 110, Height = 58,
            Font = UiTheme.Kicker, ForeColor = accent,
            AutoEllipsis = true, UseMnemonic = false, Margin = new Padding(0, 1, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft
        });
        return header;
    }

    private Control BuildPilotCard()
    {
        var card = UiTheme.BackdropPanel(
            "ui-backgrounds/pilot-card-12-manga-v1.png",
            Color.FromArgb(92, UiTheme.Background),
            imageAlpha: 232,
            padding: new Padding(12, 8, 12, 8));
        pilotCard = card;
        card.Tag = AssetPaths.File(DriverFigurinaAsset(12));
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(6, 0, 6, 0);
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Warning, 2);
            using var brush = new SolidBrush(UiTheme.Warning);
            UiTheme.DrawRoundedBorder(e.Graphics, new Rectangle(0, 0, card.Width - 1, card.Height - 1), UiTheme.Warning, 2, 12);
            e.Graphics.FillRectangle(brush, 0, 0, 3, card.Height);
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pilotName.Dock = DockStyle.Fill; pilotName.Font = new Font(UiTheme.FamilySemibold, 16F, FontStyle.Bold);
        pilotName.ForeColor = UiTheme.TextPrimary; pilotName.AutoEllipsis = true; pilotName.UseMnemonic = false;
        pilotName.TextAlign = ContentAlignment.MiddleLeft;
        pilotDetails.Dock = DockStyle.Fill; pilotDetails.Font = new Font(UiTheme.FamilySans, 9F, FontStyle.Regular); pilotDetails.ForeColor = UiTheme.TextPrimary;
        pilotDetails.AutoEllipsis = true; pilotDetails.UseMnemonic = false; pilotDetails.TextAlign = ContentAlignment.MiddleLeft;
        pilotSchool.Dock = DockStyle.Fill; pilotSchool.Font = new Font(UiTheme.FamilySemibold, 9F, FontStyle.Bold); pilotSchool.ForeColor = UiTheme.Positive;
        pilotSchool.AutoEllipsis = true; pilotSchool.UseMnemonic = false; pilotSchool.TextAlign = ContentAlignment.MiddleLeft;
        var quote = new Label
        {
            Text = "« PICCOLI PASSI\n   GRANDI TRAGUARDI »", Dock = DockStyle.Fill,
            Font = new Font(UiTheme.FamilySerif, 9F, FontStyle.Italic | FontStyle.Bold),
            ForeColor = Color.FromArgb(238, 241, 246), UseMnemonic = false,
            TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(0, 0, 0, 2)
        };
        layout.Controls.Add(pilotName, 0, 0);
        layout.Controls.Add(pilotDetails, 0, 1);
        layout.Controls.Add(pilotSchool, 0, 2);
        layout.Controls.Add(quote, 0, 3);
        card.Controls.Add(layout);
        return card;
    }

    private static string DriverFigurinaAsset(int eta) => eta switch
    {
        <= 13 => "ui-backgrounds/pilot-card-12-manga-v1.png",
        <= 16 => "ui-backgrounds/pilot-card-15-manga-v1.png",
        _ => "ui-backgrounds/pilot-card-18-manga-v1.png"
    };

    private static Image? LoadMetricIcon(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            using var source = Image.FromFile(path);
            return new Bitmap(source);
        }
        catch (Exception error)
        {
            CareerLog.Warn("ui", $"icona metrica non caricata: {error.Message}");
            return null;
        }
    }

    private static void ConfigureActionButton(Button action, string text, Color accent, Action onAction)
    {
        action.Text = text;
        action.Dock = DockStyle.Fill;
        action.Height = 36;
        action.AutoEllipsis = false;
        action.Font = new Font(UiTheme.FamilySans, 8F, FontStyle.Bold);
        action.ForeColor = accent;
        action.BackColor = UiTheme.SurfaceRaised;
        action.FlatStyle = FlatStyle.Flat;
        action.TextAlign = ContentAlignment.MiddleLeft;
        action.Padding = new Padding(8, 2, 4, 2);
        action.UseMnemonic = false;
        action.Cursor = Cursors.Hand;
        action.FlatAppearance.BorderColor = accent;
        action.FlatAppearance.BorderSize = 1;
        action.Click += (_, _) => onAction();
    }

    /// <summary>
    /// Aggiorna il conto dallo stato della carriera. La prima chiamata non
    /// lampeggia: aprire il programma non è un movimento di denaro.
    /// </summary>
    public void Update(CareerState career)
    {
        var cash = career.Cash;
        var eta = career.BirthYear <= 0 ? 12 : Math.Max(10, career.StoryDate.Year - career.BirthYear);
        var figurePath = AssetPaths.File(DriverFigurinaAsset(eta));
        if (pilotCard != null
            && !string.Equals(pilotCard.Tag as string, figurePath, StringComparison.OrdinalIgnoreCase)
            && File.Exists(figurePath))
        {
            pilotCard.SetBackdrop(figurePath, Color.FromArgb(92, UiTheme.Background));
            pilotCard.Tag = figurePath;
        }
        var school = Scuola.Livello(career);
        pilotName.Text = string.IsNullOrWhiteSpace(career.Driver) ? "Pilota" : career.Driver;
        pilotDetails.Text = $"{eta} ANNI\n{Scuola.Classe(career).ToUpperInvariant()}";
        pilotSchool.Text = $"SCUOLA · {school}/100 · {GiudizioScuola(school)}";
        pilotSchool.ForeColor = school < Scuola.SogliaDiDivieto ? UiTheme.Accent
            : school < Scuola.SogliaDiPromozione ? UiTheme.Warning : UiTheme.Positive;
        amount.Text = $"€ {cash:N0}";
        amount.ForeColor = UiTheme.MoneyColor(cash);
        var fitness = Math.Clamp(career.Fitness, 0, 100);
        fitnessValue.Text = $"{fitness}/100";
        fitnessValue.ForeColor = fitness < 30 ? UiTheme.Warning : UiTheme.Positive;
        SetProgress(fitnessProgress, fitness, fitnessValue.ForeColor);
        // Il riquadro ha come azione «SOCIAL · PILOTA + AMICI» ma mostrava la
        // fiducia dei team: due grandezze diverse sotto la stessa etichetta,
        // e infatti il numero qui non corrispondeva mai alla popolarità citata
        // negli articoli. Qui va la metrica social e di pubbliche relazioni.
        var influencer = Math.Clamp(career.ReputationProfile?.PublicPopularity ?? 0, 0, 100);
        trustValue.Text = $"{influencer}/100";
        trustValue.ForeColor = influencer < 30 ? UiTheme.Warning : UiTheme.Info;
        SetProgress(influencerProgress, influencer, trustValue.ForeColor);
        schoolValue.Text = $"{school}/100";
        schoolValue.ForeColor = Scuola.ARischio(career) ? UiTheme.Accent : UiTheme.Warning;
        SetProgress(schoolProgress, school, schoolValue.ForeColor);

        // Lo stato economico in parole: dice se quella cifra basta o no.
        // Quando la cassa è al limite l'avviso prende il posto dello stato: dire
        // «margine minimo» non aiuta, dire cosa fare sì.
        var warning = CareerWallet.Warning(career);
        if (warning.Length > 0)
        {
            status.Text = warning;
            status.ForeColor = cash < CareerFinances.SurvivalFloor ? UiTheme.Accent : UiTheme.Warning;
        }
        else
        {
            // Nessuna formula motivazionale nella carta: qui deve restare solo
            // il dato economico e, quando serve, l'avviso operativo.
            status.Text = "";
            status.ForeColor = UiTheme.TextSecondary;
        }

        var sponsorBudget = career.SponsorBudget;
        breakdown.Text = $"Sponsor disponibili € {sponsorBudget:N0}   ·   "
                         + $"premi gara € {career.PrizeMoney:N0}   ·   sponsor incassati € {career.SponsorMoney:N0}";

        var recent = (career.Results ?? [])
            .TakeLast(2)
            .Select(x => x.Length > 60 ? x[..60] + "…" : x)
            .ToList();
        movements.Text = recent.Count == 0
            ? "ULTIMI MOVIMENTI: nessuno"
            : "ULTIMI MOVIMENTI: " + string.Join("  ·  ", recent);

        if (lastCash.HasValue && lastCash.Value != cash)
        {
            // Verde se è entrato denaro, rosso se è uscito: il colore dice il
            // verso prima che si legga la cifra.
            pulseColor = cash > lastCash.Value ? UiTheme.Positive : UiTheme.Accent;
            StartPulse();
        }
        lastCash = cash;
    }

    private static string GiudizioScuola(int livello) => livello switch
    {
        >= 80 => "OTTIMO",
        >= 60 => "BUONO",
        >= 40 => "SUFFICIENTE",
        _ => "DA RECUPERARE"
    };

    private void StartPulse()
    {
        if (!CareerTransitions.PulsesEnabled) return;
        pulseFrame = 0;
        pulseTimer.Stop();
        pulseTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // Cornice: normalmente ambra, durante il lampeggio del colore del
        // movimento e piu spessa.
        var glow = pulseTimer.Enabled ? CareerTransitions.PulseIntensity(pulseFrame, PulseFrames) : 0.0;
        if (glow > 0.01)
        {
            using var fill = new SolidBrush(Color.FromArgb((int)(glow * 46), pulseColor));
            e.Graphics.FillRectangle(fill, ClientRectangle);
            using var thick = new Pen(pulseColor, 1 + (float)(glow * 2.5));
            e.Graphics.DrawRectangle(thick, rect);
        }
        else
        {
            using var pen = new Pen(UiTheme.Warning);
            e.Graphics.DrawRectangle(pen, rect);
        }

        // Filetto verticale a sinistra: ancora la cifra al bordo della carta.
        using var accent = new SolidBrush(glow > 0.01 ? pulseColor : UiTheme.Warning);
        e.Graphics.FillRectangle(accent, 0, 0, 3, Height);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { pulseTimer.Dispose(); tutorialOverlay?.Dispose(); }
        base.Dispose(disposing);
    }

    /// <summary>Mostra una volta sola il tour guidato dei quattro indicatori.</summary>
    public void ShowBudgetTutorial(Action completed)
    {
        if (tutorialOverlay != null || tutorialTargets.Length < 4) return;
        var host = FindForm();
        if (host == null) return;
        tutorialStep = 0;
        // Il tour appartiene alla finestra, non alla sola fascia dei valori:
        // ancorarlo a BudgetPanel faceva finire la scheda esplicativa sopra le
        // tessere e tagliava pulsanti e cifre. A livello form la scheda vive in
        // una fascia indipendente in fondo, lasciando il budget sempre leggibile.
        tutorialOverlay = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(205, 8, 10, 16), Padding = new Padding(28), Cursor = Cursors.Default };
        tutorialOverlay.Paint += (_, e) => PaintTutorial(e.Graphics);
        // La scheda si posiziona a mano, sotto il riquadro che spiega: agganciata
        // in fondo alla finestra finiva a centinaia di pixel di distanza da
        // cio' di cui parlava, e nessuna freccia poteva rimediare.
        tutorialCard = new Panel { BackColor = UiTheme.SurfaceRaised, Padding = new Padding(16, 12, 16, 12) };
        var card = tutorialCard;
        tutorialTitle = new Label { Dock = DockStyle.Top, Height = 24, Font = UiTheme.BodyStrong, ForeColor = UiTheme.Warning, UseMnemonic = false };
        tutorialBody = new Label { Dock = DockStyle.Fill, Font = UiTheme.Body, ForeColor = UiTheme.TextPrimary, UseMnemonic = false };
        tutorialNext = new Button { Text = "AVANTI", Dock = DockStyle.Bottom, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = UiTheme.Accent, ForeColor = Color.White, Font = UiTheme.BodyStrong, UseVisualStyleBackColor = false };
        tutorialNext.FlatAppearance.BorderSize = 0;
        tutorialNext.Click += (_, _) =>
        {
            tutorialStep++;
            if (tutorialStep >= 4)
            {
                tutorialOverlay?.Dispose(); tutorialOverlay = null;
                completed(); Invalidate(); return;
            }
            UpdateTutorialText(); PlaceTutorialCard(); tutorialOverlay?.Invalidate();
        };
        card.Controls.Add(tutorialBody); card.Controls.Add(tutorialTitle); card.Controls.Add(tutorialNext);
        tutorialOverlay.Controls.Add(card); host.Controls.Add(tutorialOverlay); tutorialOverlay.BringToFront();
        UpdateTutorialText(); PlaceTutorialCard();
    }

    private void UpdateTutorialText()
    {
        if (tutorialTitle == null || tutorialBody == null || tutorialNext == null) return;
        (tutorialTitle.Text, tutorialBody.Text, tutorialNext.Text) = tutorialStep switch
        {
            // Il testo nomina il riquadro indicato dalla freccia e dice cosa
            // farci: prima parlava in generale e non si capiva a quale dei tre
            // valori si riferisse.
            0 => ("1 di 3  ·  QUESTO È IL BUDGET",
                "È la cifra che decide cosa puoi fare: iscrizioni, trasferte, riparazioni. Per farla crescere si manda Haru a cercare sponsor, dal pannello OGGI in alto a destra.", "AVANTI"),
            1 => ("2 di 4  ·  QUESTA È LA FORMA FISICA",
                "Sale con allenamento e riposo, scende con le gare. Più è alta, più il pilota tiene il passo nei giri finali. Palestra, corsa e riposo stanno nel pannello OGGI, in alto a destra.", "AVANTI"),
            2 => ("3 di 4  ·  QUESTO È IL LIVELLO INFLUENCER",
                "È quanto il tuo nome circola fuori dalla pista: social, interviste, presenze. Cresce con i risultati e con il lavoro d'immagine, che si sceglie dal pannello OGGI. Quando è alto arrivano ingaggi promozionali e sponsor; quando è basso, nessuno ti cerca.", "AVANTI"),
            _ => ("4 di 4  ·  QUESTO È IL LIVELLO SCUOLA",
                "Misura il rendimento scolastico. È separato dall'influencer: frequentare e studiare lo tengono alto, saltare la scuola lo abbassa e può bloccare temporaneamente gli appuntamenti in pista.", "INIZIA")
        };
    }

    /// <summary>
    /// Il rettangolo del riquadro che si sta spiegando, nelle coordinate del
    /// velo che copre la finestra.
    /// </summary>
    private Rectangle TutorialTargetBounds()
    {
        if (tutorialStep >= tutorialTargets.Length) return Rectangle.Empty;
        var target = tutorialTargets[tutorialStep];
        // Le coordinate vanno riferite al VELO, non alla finestra: il velo
        // riempie l'area cliente e comincia sotto la barra del titolo, quindi
        // usare la finestra spostava cornice e freccia piu in basso del
        // riquadro, su una zona vuota della schermata.
        var riferimento = (Control?)tutorialOverlay ?? this;
        if (target.Parent == null) return Rectangle.Empty;
        var origine = riferimento.PointToClient(target.Parent.PointToScreen(target.Location));
        var r = new Rectangle(origine, target.Size);
        r.Inflate(6, 6);
        return r;
    }

    /// <summary>
    /// Mette la scheda subito sotto il riquadro spiegato, allineata a lui.
    ///
    /// Prima era agganciata in fondo alla finestra: il riquadro si accendeva in
    /// alto e la spiegazione compariva in basso, senza nessun rapporto visibile
    /// fra le due cose. Ora stanno vicine e la freccia le unisce.
    /// </summary>
    private void PlaceTutorialCard()
    {
        if (tutorialCard == null || tutorialOverlay == null) return;
        var bersaglio = TutorialTargetBounds();
        if (bersaglio.IsEmpty) return;

        var velo = tutorialOverlay.ClientSize;
        // Larga come il riquadro, con un minimo che tenga il testo leggibile.
        var largh = Math.Min(velo.Width - 40, Math.Max(430, bersaglio.Width));
        // 190 e non 150: il testo di tre righe con titolo e pulsante non ci
        // stava, e la frase si interrompeva a meta — nella schermata si leggeva
        // "Il pulsante" e poi il vuoto.
        var alt = 190;

        // Sotto il riquadro, staccata di quanto basta a far vedere la freccia.
        var y = bersaglio.Bottom + 34;
        // Se sotto non ci sta, va sopra: succede quando il riquadro e in fondo.
        if (y + alt > velo.Height - 20) y = Math.Max(20, bersaglio.Top - alt - 34);

        // Allineata al bordo sinistro del riquadro, senza uscire dal velo.
        var x = Math.Min(Math.Max(20, bersaglio.Left), Math.Max(20, velo.Width - largh - 20));

        tutorialCard.Bounds = new Rectangle(x, y, largh, alt);
        tutorialCard.BringToFront();
    }

    private void PaintTutorial(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var velo = tutorialOverlay?.ClientRectangle ?? ClientRectangle;
        var r = TutorialTargetBounds();
        if (r.IsEmpty) { using var pieno = new SolidBrush(Color.FromArgb(185, 8, 10, 16)); g.FillRectangle(pieno, velo); return; }

        // Il velo lascia scoperto il riquadro spiegato: quello che conta resta
        // in chiaro, tutto il resto si spegne. Prima il velo copriva anche il
        // riquadro e lo si distingueva solo dalla cornice.
        using (var dim = new SolidBrush(Color.FromArgb(190, 8, 10, 16)))
        {
            g.FillRectangle(dim, new Rectangle(velo.Left, velo.Top, velo.Width, Math.Max(0, r.Top - velo.Top)));
            g.FillRectangle(dim, new Rectangle(velo.Left, r.Bottom, velo.Width, Math.Max(0, velo.Bottom - r.Bottom)));
            g.FillRectangle(dim, new Rectangle(velo.Left, r.Top, Math.Max(0, r.Left - velo.Left), r.Height));
            g.FillRectangle(dim, new Rectangle(r.Right, r.Top, Math.Max(0, velo.Right - r.Right), r.Height));
        }

        var colore = tutorialStep == 1 ? UiTheme.Positive : tutorialStep == 2 ? UiTheme.Info : UiTheme.Warning;
        using var pen = new Pen(colore, 3);
        g.DrawRectangle(pen, r);

        // La freccia parte dalla scheda e arriva al riquadro: e il collegamento
        // fra la spiegazione e cio' che spiega. Prima puntava verso l'angolo in
        // alto a sinistra dello schermo, cioe' verso niente.
        if (tutorialCard == null) return;
        var scheda = tutorialCard.Bounds;
        var daX = Math.Max(scheda.Left + 24, Math.Min(scheda.Left + scheda.Width / 3, r.Left + r.Width / 2));
        var sopra = scheda.Top > r.Bottom;
        var da = new Point(daX, sopra ? scheda.Top - 4 : scheda.Bottom + 4);
        var a2 = new Point(daX, sopra ? r.Bottom + 4 : r.Top - 4);
        using var arrow = new Pen(colore, 3) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
        g.DrawLine(arrow, da, a2);
    }
}
