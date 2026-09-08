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
    private readonly Button fitnessAction = new();
    private readonly Button sponsorAction = new();
    private readonly Button communityAction = new();
    private TableLayoutPanel? metricGrid;
    private Panel? tutorialOverlay;
    private Label? tutorialTitle;
    private Label? tutorialBody;
    private Button? tutorialNext;
    private int tutorialStep;
    private Control[] tutorialTargets = [];
    /// <summary>La scheda del tutorial: viene spostata accanto al riquadro spiegato.</summary>
    private Panel? tutorialCard;

    public event EventHandler? FitnessRequested;
    public event EventHandler? SponsorRequested;
    public event EventHandler? CommunityRequested;

    private readonly System.Windows.Forms.Timer pulseTimer = new() { Interval = CareerTransitions.FrameMilliseconds };
    private int pulseFrame;
    private Color pulseColor = UiTheme.Warning;
    private int? lastCash;

    /// <summary>Durata del lampeggio: abbastanza per accorgersene, non da distrarre.</summary>
    public const int PulseFrames = 34;

    public BudgetPanel()
    {
        BackColor = UiTheme.SurfaceRaised;
        Padding = new Padding(16, 8, 16, 8);
        DoubleBuffered = true;

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, BackColor = Color.Transparent,
            MinimumSize = new Size(0, 176)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
        // Una riga unica per le tre tessere evita che il TableLayoutPanel
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
        var account = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(12, 6, 12, 4), Margin = new Padding(6, 0, 6, 0) };
        account.Paint += (_, e) => { using var pen = new Pen(UiTheme.Warning, 2); e.Graphics.DrawRectangle(pen, 0, 0, account.Width - 1, account.Height - 1); using var brush = new SolidBrush(UiTheme.Warning); e.Graphics.FillRectangle(brush, 0, 0, 3, account.Height); };
        var accountLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        // 54 e non 46: la cifra e' scritta a 25 punti in grassetto e con
        // l'interlinea ne occupa quasi cinquanta. In 46 pixel la parte bassa
        // del numero finiva sotto il pulsante degli sponsor.
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        accountLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        accountLayout.Controls.Add(new Label { Text = "BUDGET DISPONIBILE", Dock = DockStyle.Fill, Font = UiTheme.Kicker, ForeColor = UiTheme.Warning, UseMnemonic = false }, 0, 0);
        accountLayout.Controls.Add(amount, 0, 1);
        ConfigureActionButton(sponsorAction, "★  SPONSORIZZAZIONI", UiTheme.Info, () => SponsorRequested?.Invoke(this, EventArgs.Empty));
        accountLayout.Controls.Add(sponsorAction, 0, 2);
        accountLayout.Controls.Add(breakdown, 0, 3);
        account.Controls.Add(accountLayout);
        grid.Controls.Add(account, 0, 0);
        metricGrid = grid;
        tutorialTargets = [account];

        grid.Controls.Add(CreateMetricCard("FORMA FISICA", fitnessValue, "⚡  ATTIVITÀ DEL PILOTA", UiTheme.Positive,
            () => FitnessRequested?.Invoke(this, EventArgs.Empty), fitnessAction), 1, 0);
        grid.Controls.Add(CreateMetricCard("LIVELLO INFLUENCER", trustValue, "★  SOCIAL · PILOTA + AMICI",
            UiTheme.Info, () => CommunityRequested?.Invoke(this, EventArgs.Empty), communityAction), 2, 0);
        tutorialTargets = [account, grid.GetControlFromPosition(1, 0)!, grid.GetControlFromPosition(2, 0)!];

        movements.Font = UiTheme.Small;
        movements.ForeColor = UiTheme.TextMuted;
        movements.Dock = DockStyle.Fill;
        movements.Margin = new Padding(6, 4, 6, 0);
        // Su due righe: l'ultimo movimento e' una frase, non un'etichetta, e
        // troncarlo a meta lo rendeva illeggibile.
        movements.AutoEllipsis = false;
        movements.UseMnemonic = false;
        grid.Controls.Add(movements, 0, 1);
        grid.SetColumnSpan(movements, 3);

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

    private static Control CreateMetricCard(string title, Label value, string actionText, Color accent, Action onAction, Button action, bool showAction = true)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(12, 6, 12, 4), Margin = new Padding(6, 0, 6, 0) };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(accent, 2);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var brush = new SolidBrush(accent);
            e.Graphics.FillRectangle(brush, 0, 0, 3, card.Height);
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = showAction ? 3 : 2, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        if (showAction) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        var caption = new Label { Text = title, Dock = DockStyle.Fill, Font = UiTheme.Kicker, ForeColor = accent, AutoEllipsis = true, UseMnemonic = false };
        value.Font = new Font(UiTheme.FamilySemibold, 22F, FontStyle.Bold);
        value.ForeColor = UiTheme.TextPrimary;
        value.Dock = DockStyle.Fill;
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.UseMnemonic = false;
        if (showAction) ConfigureActionButton(action, actionText, accent, onAction);
        layout.Controls.Add(caption, 0, 0);
        layout.Controls.Add(value, 0, 1);
        if (showAction) layout.Controls.Add(action, 0, 2);
        card.Controls.Add(layout);
        return card;
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
        amount.Text = $"€ {cash:N0}";
        amount.ForeColor = UiTheme.MoneyColor(cash);
        fitnessValue.Text = $"{Math.Clamp(career.Fitness, 0, 100)}/100";
        fitnessValue.ForeColor = career.Fitness < 30 ? UiTheme.Warning : UiTheme.Positive;
        // Il riquadro ha come azione «SOCIAL · PILOTA + AMICI» ma mostrava la
        // fiducia dei team: due grandezze diverse sotto la stessa etichetta,
        // e infatti il numero qui non corrispondeva mai alla popolarità citata
        // negli articoli. Qui va la metrica social e di pubbliche relazioni.
        var influencer = Math.Clamp(career.ReputationProfile?.PublicPopularity ?? 0, 0, 100);
        trustValue.Text = $"{influencer}/100";
        trustValue.ForeColor = influencer < 30 ? UiTheme.Warning : UiTheme.Info;

        var day = DriverDay.EnsureToday(career);
        fitnessAction.Text = $"⚡  ATTIVITÀ DEL PILOTA · {day.DriverHoursLeft}h libere";
        sponsorAction.Text = $"★  SPONSORIZZAZIONI · Haru {day.AgentHoursLeft}h libere";
        communityAction.Text = $"★  SOCIAL · PILOTA + AMICI · {day.DriverHoursLeft}h/{day.AgentHoursLeft}h";

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

    /// <summary>Mostra una volta sola il tour guidato dei tre indicatori.</summary>
    public void ShowBudgetTutorial(Action completed)
    {
        if (tutorialOverlay != null || tutorialTargets.Length < 3) return;
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
            if (tutorialStep >= 3)
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
                "È la cifra che decide cosa puoi fare: iscrizioni, trasferte, riparazioni. Il pulsante SPONSORIZZAZIONI qui sotto è il modo per farla crescere quando è troppo bassa.", "AVANTI"),
            1 => ("2 di 3  ·  QUESTA È LA FORMA FISICA",
                "Sale con allenamento e riposo, scende con le gare. Più è alta, più il pilota tiene il passo nei giri finali. Si gestisce dal pulsante ATTIVITÀ DEL PILOTA qui sotto.", "AVANTI"),
            _ => ("3 di 3  ·  QUESTO È IL LIVELLO INFLUENCER",
                "È quanto il tuo nome circola fuori dalla pista: social, interviste, presenze. Cresce con i risultati e con il lavoro di immagine del pulsante SOCIAL qui sotto. Quando è alto arrivano ingaggi promozionali e sponsor; quando è basso, nessuno ti cerca.", "INIZIA")
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
