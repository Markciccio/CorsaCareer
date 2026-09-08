using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text.Json;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// Portale della carriera: barra di testata, striscia dati, colonna azioni,
/// colonna editoriale e colonna dossier. Tutto impaginato con contenitori
/// (Dock/TableLayoutPanel), senza coordinate assolute, così la finestra regge
/// ridimensionamenti e scaling DPI.
/// </summary>
public sealed partial class MainForm
{
    // Testata e stato.
    private Button narrationControl = new();
    private Label? saveStatus;
    private Label headerRound = new();
    private Label headerChampionship = new();

    /// <summary>
    /// Il centro carriera aperto, se c'è. Serve a farlo sparire mentre si apre
    /// la gestione dei salvataggi: due finestre a tutto schermo sovrapposte
    /// nascondono quale delle due sta rispondendo ai clic.
    /// </summary>
    private Form? hubToClose;

    // Striscia dati.
    // Una riga in prosa al posto di sette caselle numeriche affiancate.
    private Label situationLine = new();
    private Label todayDateLine = new();
    private BudgetPanel budgetPanel = new();

    // Colonna azioni.
    private Button continueStory = new(), launch = new(), briefing = new(), nextSeason = new(), activities = new(), opportunities = new(), simulate = new();
    /// <summary>Il comando principale della card «ADESSO», distinto da quello del pannello sezioni.</summary>
    private Button homeContinue = new();
    private Button simulateGood = new(), simulateBad = new();
    private Label pendingBanner = new();
    private Label decisionSummary = new();

    // Colonna editoriale: l'articolo per intero, non due righe.
    private Label heroKicker = new();
    private Label heroTitle = new();
    private Label heroByline = new();
    private Label heroStandfirst = new();
    private FlowLayoutPanel heroBody = new();
    private FlowLayoutPanel diary = new();
    private Label diaryFooter = new();

    // Colonna dossier.
    private Label profileName = new();
    private Label profileSeat = new();
    private PictureBox? portraitImage;
    private RichTextBox nextSession = new();
    private RichTextBox standings = new();
    private RichTextBox dossier = new();
    private RichTextBox results = new();
    private Label contentStatus = new();

    // Home di gioco: la prima schermata deve essere un capitolo da vivere,
    // non la prima pagina di un giornale. I dettagli restano nelle sezioni.
    private Label homeKicker = new(), homeTitle = new(), homeObjective = new();
    private Label homeSession = new(), homeTeam = new(), homeMoney = new(), homeMarket = new(), homeContents = new();
    /// <summary>Il passo attuale, impaginato: titolo, racconto e dati distinti.</summary>
    private FlowLayoutPanel homeStep = new();
    private FlowLayoutPanel homeCalendarTimeline = new();
    private PictureBox? homeArtwork;
    private readonly List<string> homeArtworkPaths = [];
    private System.Windows.Forms.Timer? homeArtworkTimer;
    private Image? homeArtworkCurrent;
    private Image? homeArtworkNext;
    private int homeArtworkIndex, homeArtworkHoldTicks, homeArtworkFadeStep;
    /// <summary>Firma delle tavole in uso: se cambia, la sequenza va rifatta.</summary>
    private string homeArtworkKey = "";
    private string automaticNarrationKey = "";

    /// <summary>L'avviso di articolo nuovo, in cima alla schermata.</summary>
    private ArticleNotice? articleNotice;

    private void BuildUi()
    {
        Controls.Add(BuildStatusBar());
        Controls.Add(BuildSectionBar());
        Controls.Add(BuildBody());
        Controls.Add(BuildStatStrip());
        Controls.Add(BuildHeader());
        // L'avviso di un articolo nuovo non è più un nastro ancorato in cima
        // alla finestra: vive dentro la colonna «OGGI», inserito da
        // RefreshCareerHome accanto a ciò che racconta.
    }

    /// <summary>Titolo dell'articolo da annunciare, se ce n'è uno in attesa.</summary>
    private string pendingArticleTitle = "";

    /// <summary>Cosa aprire se il giocatore clicca l'avviso.</summary>
    private Action? pendingArticleOpen;

    /// <summary>
    /// Quante volte la colonna è stata ridisegnata da quando l'avviso è
    /// comparso. Alla prima azione successiva del giocatore l'avviso sparisce:
    /// serve a segnalare, non a occupare la schermata finché non lo si chiude.
    /// </summary>
    private int pendingArticleRedraws;

    /// <summary>
    /// Le sezioni della carriera come barra di navigazione.
    ///
    /// Erano quattro riquadri nella colonna destra, e rubavano lo spazio al
    /// passo attuale: il comando principale finiva schiacciato in una striscia
    /// illeggibile. Squadra, sponsor, ingaggi e contenuti sono destinazioni, non
    /// informazioni da tenere sempre sotto gli occhi; i loro numeri vivono già
    /// nella striscia dei dati e nel dossier di ciascuna schermata.
    /// </summary>
    private Control BuildSectionBar()
    {
        var bar = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = UiTheme.Surface, Padding = new Padding(24, 7, 24, 7) };
        bar.Paint += (_, e) =>
        {
            using var line = new Pen(UiTheme.Border);
            e.Graphics.DrawLine(line, 0, bar.Height - 1, bar.Width, bar.Height - 1);
        };
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
            AutoSize = false, BackColor = Color.Transparent, Margin = new Padding(0)
        };
        void Section(string label, Action open)
        {
            var button = UiTheme.GhostButton(label, 0);
            button.Margin = new Padding(0, 0, 8, 0);
            button.Click += (_, _) => open();
            flow.Controls.Add(button);
        }
        // Una voce sola per quello che si fa fra un weekend e l'altro. Prima
        // c'erano "Agenda e sponsor" e "Sponsor": due schermate per la stessa
        // attivita, e bisognava ricordarsi in quale stava una cosa.
        // La giornata del pilota: due agende parallele — la sua e quella di
        // Haru — e un pulsante per andare a domani. Prende il posto dell'agenda
        // perche e' la stessa cosa, fatta come si deve.
        Section("ATTIVITÀ DEL PILOTA", () => OpenDriverDay());
        // Le sponsorizzazioni sono il lavoro di Haru, non un'attivita del
        // pilota: sezione separata, giornata separata.
        Section("SPONSORIZZAZIONI", OpenSponsorDay);
        Section("MERCATO E OFFERTE", OpenMarket);
        // I contenuti sono già disponibili nella barra degli strumenti in alto;
        // non duplicare il comando nella barra delle sezioni.
        bar.Controls.Add(flow);
        return bar;
    }

    // ---------------------------------------------------------------- testata

    private Control BuildHeader()
    {
        // Due righe: marchio e comandi sopra, contesto del round sotto a tutta
        // larghezza. Su una riga sola il titolo e il round si sovrapponevano.
        var header = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = UiTheme.HeaderBackground, Padding = new Padding(24, 6, 24, 8) };
        header.Paint += (_, e) =>
        {
            using var accent = new SolidBrush(UiTheme.Accent);
            e.Graphics.FillRectangle(accent, 0, 0, header.Width, 3);
            using var line = new Pen(UiTheme.Border);
            e.Graphics.DrawLine(line, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.Transparent };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        header.Controls.Add(grid);

        var brand = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, Margin = new Padding(0) };
        brand.Controls.Add(new Label { Text = "CORSA CAREER", AutoSize = true, Font = UiTheme.Display, ForeColor = UiTheme.TextPrimary, Margin = new Padding(0, 6, 14, 0) });
        brand.Controls.Add(new Label { Text = "LA TUA CARRIERA · ASSETTO CORSA", AutoSize = true, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted, Margin = new Padding(0, 18, 0, 0) });
        grid.Controls.Add(brand, 0, 0);

        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, BackColor = Color.Transparent, Margin = new Padding(0, 8, 0, 0) };
        narrationControl = UiTheme.GhostButton("▶  RUBRICA TV", 156);
        narrationControl.ForeColor = UiTheme.TextPrimary;
        narrationControl.Click += (_, _) => ToggleNarration();
        var soundtrackButton = UiTheme.GhostButton("♫  SOUNDTRACK", 138);
        soundtrackButton.Click += (_, _) => OpenSoundtrack();
        var settingsButton = UiTheme.GhostButton("⚙  IMPOSTAZIONI", 156);
        settingsButton.Click += (_, _) => OpenSettings();
        // Una voce sola per la carriera. «Centro carriera» e «Carriere» erano
        // due pulsanti indistinguibili a leggerli, e costringevano a ricordare
        // quale dei due contenesse cosa: la gestione dei salvataggi adesso è
        // una scheda dentro il centro carriera.
        var careerHub = UiTheme.GhostButton("◎  CENTRO CARRIERA", 176);
        careerHub.Click += (_, _) => OpenCareerHub();
        tools.Controls.Add(narrationControl);
        tools.Controls.Add(soundtrackButton);
        tools.Controls.Add(careerHub);
        var contentsButton = UiTheme.GhostButton("▣  CONTENUTI", 132);
        contentsButton.Click += (_, _) => RefreshContents();
        tools.Controls.Add(contentsButton);
        tools.Controls.Add(settingsButton);
        grid.Controls.Add(tools, 1, 0);

        var context = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Margin = new Padding(0) };
        context.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        context.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        headerRound = new Label { Text = "", Dock = DockStyle.Fill, Font = UiTheme.Title, ForeColor = UiTheme.TextPrimary, AutoEllipsis = true, Margin = new Padding(0) };
        headerChampionship = new Label { Text = "", Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary, AutoEllipsis = true, Margin = new Padding(0) };
        context.Controls.Add(headerRound, 0, 0);
        context.Controls.Add(headerChampionship, 0, 1);
        grid.Controls.Add(context, 0, 1);
        grid.SetColumnSpan(context, 2);

        return header;
    }

    private void OpenSoundtrack()
    {
        using var dialog = new SoundtrackDialog(career?.Events.OrderByDescending(x => x.DateUtc).FirstOrDefault()?.Type);
        dialog.ShowDialog(this);
    }

    // --------------------------------------------------------- striscia dati

    /// <summary>
    /// Dove si trova il pilota, detto in una frase.
    ///
    /// Prima qui c'erano sette caselle numeriche affiancate — punti, budget,
    /// reputazione, gare, seguito, condizione, relazioni. Erano la prima cosa
    /// che si vedeva aprendo il programma, e facevano sembrare la carriera un
    /// foglio di calcolo: sette numeri senza un filo che li leghi. Gli stessi
    /// dati ci sono ancora, nel dossier a destra, dove si consultano. Qui
    /// diventano una riga che si legge.
    /// </summary>
    private Control BuildStatStrip()
    {
        // Altezza sufficiente per i tre riquadri e per le azioni a due righe:
        // evita che il testo venga tagliato quando la finestra e' larga.
        //
        // 214 e non 196: dentro ci stanno la cifra (54), il pulsante (38), il
        // dettaglio sponsor, l'intestazione e la riga dei movimenti su due
        // righe. Con 196 l'ultima riga del riquadro — "Sponsor disponibili
        // ... premi gara ... sponsor" — veniva tagliata a meta.
        var wrapper = new TableLayoutPanel { Dock = DockStyle.Top, Height = 214, BackColor = UiTheme.Background, Padding = new Padding(24, 6, 24, 10), ColumnCount = 2, RowCount = 1 };
        // Il conto e i due indicatori decisivi devono stare a sinistra, prima
        // della frase narrativa: sono il cruscotto che guida ogni scelta.
        wrapper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        wrapper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        todayDateLine = new Label
        {
            Dock = DockStyle.Fill, Font = new Font(UiTheme.FamilySemibold, 12F, FontStyle.Bold),
            ForeColor = UiTheme.Warning, TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false, AutoEllipsis = false, Margin = new Padding(0)
        };
        situationLine = new Label
        {
            Dock = DockStyle.Fill, Font = UiTheme.Body, ForeColor = UiTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, AutoEllipsis = false,
            Padding = new Padding(0, 0, 18, 0), Margin = new Padding(0)
        };
        var narrative = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = UiTheme.Background, ColumnCount = 1, RowCount = 3,
            Margin = new Padding(8, 0, 0, 0), Padding = new Padding(0)
        };
        narrative.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        narrative.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Scorrere il calendario a mano.
        //
        // Questi pulsanti erano stati tolti perché facevano avanzare la
        // carriera senza che il pilota avesse deciso niente. Il difetto però
        // non era il pulsante: era che scavalcava gli appuntamenti. Adesso
        // AdvanceCalendar si ferma da sola sul giorno dell'impegno successivo,
        // e quando ci si arriva il programma chiede se affrontarlo — quindi
        // far passare il tempo è una scelta, non un modo per saltare le scelte.
        var comandiTempo = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
            BackColor = Color.Transparent, Margin = new Padding(0), Padding = new Padding(0, 2, 0, 0)
        };
        var domani = UiTheme.SecondaryButton("VAI A DOMANI");
        domani.Width = 150; domani.Height = 30; domani.Margin = new Padding(0, 0, 8, 0);
        domani.Click += (_, _) => AvanzaNelCalendario(1);
        var prossimo = UiTheme.SecondaryButton("VAI AL PROSSIMO IMPEGNO");
        prossimo.Width = 220; prossimo.Height = 30; prossimo.Margin = new Padding(0);
        prossimo.Click += (_, _) => AvanzaNelCalendario(0);
        comandiTempo.Controls.Add(domani);
        comandiTempo.Controls.Add(prossimo);

        narrative.Controls.Add(todayDateLine, 0, 0);
        narrative.Controls.Add(comandiTempo, 0, 1);
        narrative.Controls.Add(situationLine, 0, 2);
        // Il conto era una stringa di due righe nello stesso corpo minuscolo del
        // resto: il denaro decide ogni scelta della carriera e non si
        // distingueva da una nota a margine.
        budgetPanel = new BudgetPanel { Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0) };
        // Ogni riquadro apre le proprie attività: dal social non si deve
        // finire a scegliere la palestra.
        budgetPanel.FitnessRequested += (_, _) => OpenDriverDay(DayFocus.Fisico);
        budgetPanel.SponsorRequested += (_, _) => OpenSponsorDay();
        budgetPanel.CommunityRequested += (_, _) => OpenDriverDay(DayFocus.Immagine);
        wrapper.Controls.Add(budgetPanel, 0, 0); wrapper.Controls.Add(narrative, 1, 0);
        return wrapper;
    }

    /// <summary>
    /// Compone la riga di situazione dallo stato reale. Ogni segmento esiste solo
    /// se ha qualcosa da dire: un pilota senza contratto non legge una frase sul
    /// campionato che non ha.
    /// </summary>
    private string BuildSituationLine()
    {
        var profile = career.ReputationProfile ?? new ReputationProfile();
        var gradino = CareerLadder.Current(career, contentIndex.Cars);
        var livello = ChampionshipLadder.Clamp(career.ChampionshipLevel);

        // Le due scale, spiegate.
        //
        // In testata comparivano «CATEGORIA 1 DI 7» e «LIVELLO 1 DI 5» senza
        // dire che cosa fossero: la categoria si intuisce dal nome della
        // vettura, il livello no — «livello 1 di 5» poteva sembrare il numero
        // delle gare. Sono due scale diverse e si salgono in modo diverso, ed è
        // esattamente la cosa che il giocatore deve avere sempre sott'occhio.
        var righe = new List<string>
        {
            $"CATEGORIA  ·  {gradino.Name} — la {gradino.Step}ª delle {CareerLadder.Steps} categorie, dal kart da noleggio al vertice. "
            + "Si sale firmando per una vettura superiore, dopo aver fatto abbastanza gare su questa.",

            $"CAMPIONATO  ·  {ChampionshipLadder.Name(livello)} — il {livello}° dei {ChampionshipLadder.Levels} livelli: "
            + $"{ChampionshipLadder.Scope(livello)}. {ChampionshipLadder.PromotionRule(livello)}",

            $"SQUADRA  ·  " + (career.ContractActive && !string.IsNullOrWhiteSpace(career.Team) && career.Team != "Senza contratto"
                ? $"{career.Team}" + (string.IsNullOrWhiteSpace(career.TeamProfileId) ? "" : $" — {TeamProfile.ById(career.TeamProfileId).Nome.ToLowerInvariant()}")
                : "nessun contratto: sei in cerca di un sedile")
        };

        var parti = new List<string>();

        if (career.Races == 0) parti.Add("nessuna gara disputata");
        else if (career.Wins > 0)
            parti.Add($"{career.Races} gare, {career.Wins} {(career.Wins == 1 ? "vittoria" : "vittorie")} e {career.Podiums} {(career.Podiums == 1 ? "podio" : "podi")}");
        else if (career.Podiums > 0) parti.Add($"{career.Races} gare e {career.Podiums} {(career.Podiums == 1 ? "podio" : "podi")}, nessuna vittoria");
        else parti.Add($"{career.Races} gare, ancora nessun podio");

        parti.Add($"€ {career.Cash:N0} in cassa, {CareerFinances.Status(career.Cash).Split('\u2014')[0].Trim().ToLowerInvariant()}");

        // Una sola dimensione di reputazione: quella che in questo momento
        // racconta di piu.
        parti.Add(DescribeStanding(profile));

        if (career.SimulatedSessions > 0) parti.Add($"carriera di prova, {career.SimulatedSessions} sessioni simulate");

        righe.Add("BILANCIO  ·  " + string.Join("  ·  ", parti));
        return string.Join("\n", righe);
    }

    private static string DescribeStanding(ReputationProfile profile)
    {
        if (profile.TeamTrust >= 80) return "i team se lo giocano";
        if (profile.PressStanding >= 65) return "la stampa lo segue con attenzione";
        if (profile.SportingPrestige >= 60) return "un nome che pesa in categoria";
        if (profile.PressStanding >= 35) return "la stampa comincia a citarlo";
        if (profile.TeamTrust >= 45) return "qualche team lo tiene d'occhio";
        if (profile.SponsorAppeal >= 30) return "primi sponsor interessati";
        return "nessuno lo conosce ancora";
    }

    // ------------------------------------------------------------------ corpo

    private Control BuildBody()
    {
        return BuildCareerHome();
    }

    private Control BuildCareerHome()
    {
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Background, Padding = new Padding(24, 6, 24, 12) };
        // La tavola era piu larga di tutto il resto e la colonna di destra
        // doveva impilare ADESSO e calendario in poco spazio. Ora l'immagine
        // accompagna, e le due sezioni stanno affiancate.
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));

        var chapter = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Margin = new Padding(0, 0, UiTheme.Gutter, 0) };
        chapter.Paint += (_, e) => { using var p = new Pen(UiTheme.Border); e.Graphics.DrawRectangle(p, 0, 0, chapter.Width - 1, chapter.Height - 1); };
        homeArtwork = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = UiTheme.SurfaceRaised };
        chapter.Controls.Add(homeArtwork);
        StartHomeArtworkSequence();
        chapter.Disposed += (_, _) => StopHomeArtworkSequence();
        // La fascia con capitolo, obiettivo e rischio è stata rimossa: diceva le
        // stesse cose del pannello «ADESSO» con parole diverse, e sotto la tavola
        // sembrava una didascalia di un'altra schermata. Le etichette restano
        // come campi perché la fase serve altrove, ma non occupano più la home.
        body.Controls.Add(chapter, 0, 0);

        var command = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.Transparent };
        command.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); command.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        // Il passo attuale è la ragione per cui si guarda questa colonna, e il
        // suo comando deve avere l'aria di un comando: prende quasi due terzi.
        command.RowStyles.Add(new RowStyle(SizeType.Percent, 62)); command.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        var mission = UiTheme.Card("ADESSO", out var missionInner, UiTheme.Accent); mission.Dock = DockStyle.Fill;
        // Il passo attuale era una Label unica con un font solo: titolo, racconto
        // e dati tecnici avevano lo stesso peso, e il giocatore leggeva una
        // scheda invece di capire dove si trovava. Ora è un blocco impaginato.
        homeStep = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 2, 0, 6),
            HorizontalScroll = { Enabled = false, Visible = false }
        };
        var missionActions = new Panel { Dock = DockStyle.Fill, Height = 88, BackColor = Color.Transparent };
        // Nessun pulsante di debug in schermata: la carriera si porta avanti con
        // il pulsante rosso e basta. Quando Assetto Corsa non e installato la
        // sessione viene risolta dal programma senza chiedere niente.
        // Campo separato: il pannello «sezioni del portale» ricrea più avanti
        // `continueStory` per il proprio uso, e riassegnandolo lasciava questo
        // pulsante orfano — presente nella card ma mai aggiornato né visibile.
        homeContinue = UiTheme.PrimaryButton("CONTINUA LA STORIA"); homeContinue.Dock = DockStyle.Top; homeContinue.Height = 48; homeContinue.Click += (_, _) => ContinueStory(); missionActions.Controls.Add(homeContinue);
        // Righe esplicite invece di Dock=Fill/Bottom nello stesso pannello: un
        // FlowLayoutPanel con AutoScroll come fratello di un pannello Dock=Bottom
        // può, in certe condizioni WinForms, ignorare lo spazio riservato al
        // fratello e dipingerci sopra — il comando spariva del tutto, non solo
        // visivamente ma proprio dal disegno. Una TableLayoutPanel a righe fisse
        // non lascia margine di ambiguità: ogni riga ha uno spazio suo.
        var missionLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
        missionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        missionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        missionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        missionLayout.Controls.Add(homeStep, 0, 0);
        missionLayout.Controls.Add(missionActions, 0, 1);
        missionInner.Controls.Add(missionLayout);
        // Le righe nascono prima che il pannello abbia la larghezza definitiva:
        // senza il riadattamento il testo resterebbe misurato su un valore
        // provvisorio e le ultime righe verrebbero tagliate.
        homeStep.ClientSizeChanged += (_, _) => FitStepLines();
        // Solo due riquadri: quello che sta succedendo adesso e il calendario.
        // Squadra, sponsor, ingaggi e contenuti sono passati alla barra di
        // navigazione: erano destinazioni travestite da informazioni, e i loro
        // numeri comparivano già nella striscia dei dati sopra.
        // Due colonne verticali invece di due riquadri impilati: ADESSO e il
        // calendario si leggono insieme, e nessuno dei due resta schiacciato.
        command.RowStyles.Clear();
        command.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        command.ColumnStyles.Clear();
        command.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        command.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        mission.Margin = new Padding(0, 0, UiTheme.Gutter, 0);
        command.Controls.Add(mission, 0, 0);
        var calendarCard = BuildHomeCalendar();
        command.Controls.Add(calendarCard, 1, 0);
        body.Controls.Add(command, 1, 0);
        return body;
    }

    private void StartHomeArtworkSequence()
    {
        if (homeArtwork == null) return;
        homeArtworkPaths.Clear();
        // Quattro tavole in dissolvenza, non una sola.
        //
        // Il limite a una nasceva dal timore che una galleria contraddicesse il
        // briefing: con i nomi scritti a mano era un rischio reale. Ora le
        // tavole arrivano dal catalogo filtrate per disciplina e momento, e sono
        // gia tutte coerenti fra loro — quindi mostrarne una sola significa solo
        // tenere trecento illustrazioni chiuse in una cartella.
        const int maxHomeArtwork = 4;
        foreach (var file in HomeArtworkFiles())
        {
            if (homeArtworkPaths.Count >= maxHomeArtwork) break;
            var path = AssetPaths.File(file);
            if (File.Exists(path) && !homeArtworkPaths.Contains(path, StringComparer.OrdinalIgnoreCase)) homeArtworkPaths.Add(path);
        }
        if (homeArtworkPaths.Count == 0) return;
        // Si parte sempre dalla prima: è la tavola più pertinente al momento,
        // non una scelta a caso fra quelle disponibili.
        homeArtworkIndex = 0;
        homeArtworkCurrent = LoadArtwork(homeArtworkPaths[homeArtworkIndex]);
        homeArtwork.Image = homeArtworkCurrent;
        homeArtwork.AccessibleName = IllustrationCatalog.Context(homeArtworkPaths[homeArtworkIndex]);
        homeArtwork.Cursor = Cursors.Hand;
        homeArtworkTimer = null;
    }

    private void AdvanceHomeArtwork()
    {
        if (homeArtwork == null || homeArtworkCurrent == null || homeArtworkPaths.Count < 2) return;
        if (homeArtworkNext == null)
        {
            if (++homeArtworkHoldTicks < 100) return; // dieci secondi pieni per tavola
            homeArtworkIndex = (homeArtworkIndex + 1) % homeArtworkPaths.Count;
            homeArtworkNext = LoadArtwork(homeArtworkPaths[homeArtworkIndex]);
            if (homeArtworkNext == null) { homeArtworkHoldTicks = 0; return; }
            homeArtworkFadeStep = 1;
        }

        var frame = BlendArtwork(homeArtworkCurrent, homeArtworkNext, homeArtworkFadeStep / 10f);
        var previousFrame = homeArtwork.Image;
        homeArtwork.Image = frame;
        if (previousFrame != null && !ReferenceEquals(previousFrame, homeArtworkCurrent) && !ReferenceEquals(previousFrame, homeArtworkNext)) previousFrame.Dispose();

        if (++homeArtworkFadeStep <= 10) return;
        var finalFrame = homeArtwork.Image;
        homeArtwork.Image = homeArtworkNext;
        homeArtwork.AccessibleName = IllustrationCatalog.Context(homeArtworkPaths[homeArtworkIndex]);
        finalFrame?.Dispose();
        homeArtworkCurrent.Dispose();
        homeArtworkCurrent = homeArtworkNext;
        homeArtworkNext = null;
        homeArtworkHoldTicks = 0;
        homeArtworkFadeStep = 0;
    }

    /// <summary>
    /// Le tavole della Home: poche e coerenti con il passo che il pannello
    /// «ADESSO» sta raccontando.
    ///
    /// Prima le nove tavole generiche entravano sempre nell'elenco, quindi su
    /// tredici immagini solo quattro riguardavano il momento e la rotazione
    /// sembrava casuale: si leggeva di una firma e si guardava un kart nella
    /// pioggia. Adesso si parte da ciò che il giocatore deve fare ora — non
    /// dall'ultimo evento archiviato — e le generiche restano solo come riserva
    /// quando le tavole dedicate non sono installate.
    /// </summary>
    private IEnumerable<string> HomeArtworkFiles()
    {
        var generic = Enumerable.Range(1, 9).Select(i => Path.Combine("home-generic", $"home-generic-{i:00}.png"));
        var next = NextScheduled();
        var selection = OpenSelection;
        var chosen = new List<string>();

        if (awaitingResult)
            chosen.AddRange(["manga-02-first-test.png", "manga-night-garage.png"]);
        else if (selection != null)
            // Una selezione è un provino: box, prove, rivali che guardano.
            chosen.AddRange(["manga-gt3-first-test-japan-paddock.png", "manga-02-first-test.png", "manga-rival-grid.png"]);

        // Le offerte contano come "il momento" solo quando non c'è già un
        // prossimo appuntamento: altrimenti un test in agenda mostrava la tavola
        // della firma, perché career.Offers viene popolato in anticipo appena la
        // carriera si apre e restava diverso da zero anche prima del primo test.
        else if (next == null && career.Offers?.Count > 0 && !career.ContractActive)
            // C'è un sedile sul tavolo: è il momento della trattativa.
            chosen.AddRange(["manga-sponsor-table.png", "manga-06-first-contract.png", "manga-sponsor-meeting-kart-budget-office.png"]);
        else if (next?.Kind == ScheduledEventKind.Invitation)
            chosen.AddRange(["manga-05-first-invitation.png", "manga-rival-grid.png", "manga-03-first-rival.png"]);
        else if (next?.IsTest == true)
            chosen.AddRange(["manga-02-first-test.png", "manga-kart-first-rain-test-repaired-chassis.png", "career-library/japan-kart-dawn-garage-rookie-mechanic.png"]);
        else if (next?.Kind == ScheduledEventKind.ChampionshipRound)
            chosen.AddRange(["manga-rival-grid.png", "manga-touring-first-race-grid-rookie.png", "manga-07-teammate-duel.png"]);
        else if (career.Wins > 0)
            chosen.AddRange(["manga-10-first-victory.png", "manga-monami-kart-local-victory-celebration.png", "manga-09-first-podium.png"]);
        else if (CareerFinances.InTrouble(career.Cash))
            chosen.AddRange(["manga-08-financial-crisis.png", "manga-nobu-sponsor-rejection-kart-budget-rain.png", "manga-night-garage.png"]);
        else
            chosen.AddRange(["manga-01-rookie-dawn.png", "manga-kart-scrapyard-four-stroke-generator-first-test.png", "manga-night-garage.png"]);

        // Dal catalogo: la disciplina della carriera e il momento che si sta
        // vivendo, pescati fra tutte le tavole coerenti invece che da un elenco
        // di tre nomi. Le tavole scritte sopra restano in coda come garanzia.
        var gradinoOra = CareerLadder.Current(career, contentIndex.Cars);
        var disciplina = gradinoOra.Path switch
        {
            LadderPath.Karting => "kart",
            LadderPath.SingleSeater => gradinoOra.Tier == "Formula / top tier" ? "formula-vertice"
                : gradinoOra.Tier == "Categoria avanzata" ? "formula-alta" : "formula-minore",
            LadderPath.Endurance => gradinoOra.Tier == "Categoria regionale" ? "gt" : "endurance",
            LadderPath.Touring => "turismo",
            _ => "kart"
        };
        var momento =
            awaitingResult ? "test"
            : selection != null ? "test"
            : next?.Kind == ScheduledEventKind.Invitation ? "gara"
            : next?.IsTest == true ? "test"
            : next?.Kind == ScheduledEventKind.ChampionshipRound ? "gara"
            : career.Wins > 0 ? "vittoria"
            : CareerFinances.InTrouble(career.Cash) ? "budget"
            : (career.Offers?.Count ?? 0) > 0 && !career.ContractActive ? "contratto"
            : "";
        // Il seme lega la scelta a questa carriera e a questo momento: la
        // sequenza e stabile finche non cambia la situazione, e due carriere
        // diverse vedono tavole diverse.
        var seme = (career.Driver ?? "").Length * 31 + career.Races * 7 + career.Season;
        chosen.InsertRange(0, IllustrationCatalog.Find(disciplina, momento, seme, 5));

        // La tavola della fase narrativa in corso completa il gruppo: lega la
        // Home al capitolo che il giocatore sta vivendo.
        var phaseArtwork = CareerPhases.Current(career, contentIndex.Cars).ArtworkAsset;
        if (!string.IsNullOrWhiteSpace(phaseArtwork)) chosen.Add(phaseArtwork);

        // Nessuna tavola di categoria superiore sopra una carriera che corre nel
        // kart: un pit stop di GT3 sopra un quattro tempi da nove cavalli non
        // illustra questa carriera, ne racconta un'altra. Le tavole restano
        // disponibili per quando si arrivera davvero in quelle categorie.
        // Il filtro guarda i mezzi dichiarati dalla tavola, non solo il nome, e
        // vale per ogni disciplina: una GT sopra un kart e una monoposto sopra
        // una GT sono lo stesso errore. Le tavole neutre — officina, paddock,
        // firma — passano sempre, perché non contraddicono niente.
        var rung = CareerLadder.Current(career, contentIndex.Cars);
        chosen = IllustrationVehicles.KeepFitting(chosen, rung);

        // Le generiche solo in coda: servono se nessuna delle tavole scelte è
        // presente fra i contenuti installati.
        return chosen.Concat(generic);
    }

    private static Image? LoadArtwork(string path)
    {
        try { using var source = Image.FromFile(path); return new Bitmap(source); }
        catch (Exception error) { CareerLog.Warn("ui", $"tavola home non caricata: {error.Message}"); return null; }
    }

    /// <summary>
    /// Lato più lungo di un fotogramma di dissolvenza.
    ///
    /// I fotogrammi venivano generati alla risoluzione piena della tavola: con
    /// PNG da 1536×1024 significa oltre 6 MB per fotogramma, dieci fotogrammi
    /// per ogni transizione, tutti allocati e composti sul thread
    /// dell'interfaccia. Era la causa dello scatto periodico durante l'uso.
    /// A schermo la tavola non supera comunque questa misura.
    /// </summary>
    private const int ArtworkFrameMaxSide = 900;

    private static Bitmap BlendArtwork(Image first, Image second, float amount)
    {
        // Transizione semplice: uscita verso il nero, cambio tavola, entrata.
        // Le due immagini non vengono mai disegnate insieme, evitando il
        // doppio contorno/effetto fantasma sui personaggi.
        var showFirst = amount < .5f;
        var image = showFirst ? first : second;
        // Il fotogramma conserva le proporzioni della tavola che sta mostrando.
        // Con una misura fissa (era 1600x900) il PictureBox in modalita' Zoom
        // riceveva un 16:9 mentre le tavole sono quadrate, 3:2 o verticali:
        // per la durata della dissolvenza l'immagine veniva stirata e sembrava
        // subire un breve ridimensionamento, per poi tornare al formato vero.
        // Ridurre in scala mantiene questa proprietà e taglia il costo.
        var scala = Math.Min(1f, (float)ArtworkFrameMaxSide / Math.Max(image.Width, image.Height));
        var larghezza = Math.Max(1, (int)(image.Width * scala));
        var altezza = Math.Max(1, (int)(image.Height * scala));
        var output = new Bitmap(larghezza, altezza, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(output);
        graphics.Clear(Color.Black);
        var alpha = showFirst ? 1f - amount * 2f : amount * 2f - 1f;
        using var matrix = new ImageAttributes();
        matrix.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Clamp(alpha, 0f, 1f) });
        graphics.DrawImage(image, new Rectangle(0, 0, output.Width, output.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, matrix);
        return output;
    }

    private void StopHomeArtworkSequence()
    {
        homeArtworkTimer?.Stop();
        homeArtworkTimer?.Dispose();
        homeArtworkTimer = null;
        homeArtworkNext?.Dispose();
        homeArtworkNext = null;
        if (homeArtwork?.Image != null)
        {
            var image = homeArtwork.Image;
            homeArtwork.Image = null;
            image.Dispose();
        }
        homeArtworkCurrent = null;
    }

    private void RestartHomeArtworkSequence()
    {
        StopHomeArtworkSequence();
        StartHomeArtworkSequence();
    }

    private static Panel HomeCard(string title, out Label text, Color accent, string action, Action onClick)
    {
        var card = UiTheme.Card(title, out var inner, accent); card.Dock = DockStyle.Fill;
        text = new Label { Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary };
        var button = UiTheme.SecondaryButton(action); button.Dock = DockStyle.Bottom; button.Height = 34; button.Click += (_, _) => onClick();
        inner.Controls.Add(text); inner.Controls.Add(button); return card;
    }

    private Panel BuildHomeCalendar()
    {
        var card = UiTheme.Card("CALENDARIO DELLA CARRIERA · ARCHIVIO VIVO", out var inner, UiTheme.Warning);
        card.Dock = DockStyle.Fill;
        // I comandi di avanzamento appartengono al calendario: qui sono sempre
        // visibili, vicino agli eventi, invece di essere nascosti nella riga
        // narrativa della testata. Ogni comando aggiorna subito giorno, ore,
        // budget e prossimo appuntamento.
        // I tre comandi di avanzamento del tempo sono stati tolti su richiesta:
        // «VAI AL PROSSIMO APPUNTAMENTO» portava dove porta già il pulsante
        // rosso principale, e «AVANTI 1 GIORNO»/«+7 GIORNI» chiedevano al
        // giocatore di far scorrere il tempo a mano. Il calendario resta un
        // archivio da leggere; ad avanzare ci pensa il percorso della storia.
        var open = UiTheme.SecondaryButton("APRI ARCHIVIO COMPLETO");
        open.Dock = DockStyle.Bottom; open.Height = 32; open.Click += (_, _) => OpenCalendar();
        homeCalendarTimeline = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 6, 0)
        };
        inner.Controls.Add(homeCalendarTimeline);
        inner.Controls.Add(open);
        return card;
    }

    /// <summary>
    /// Il nome della vettura come lo direbbe una persona, con la categoria.
    ///
    /// Nel diario compariva l'identificativo della cartella — «kart_akagi» —
    /// che non dice ne' che vettura fosse ne' di che categoria: leggendo la
    /// propria carriera non si capiva se una gara fosse in kart o in formula.
    /// </summary>
    private string NomeVettura(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return "vettura non indicata";
        var auto = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (auto == null) return UiText.Car(id);
        var gradino = CareerLadder.ForCar(auto.Category, auto.PowerHp, auto.MassKg);
        var nome = string.IsNullOrWhiteSpace(auto.Name) ? UiText.Car(id) : auto.Name;
        // Il nome della vettura dice gia' la categoria quando le due cose
        // coincidono: «Kart Rental 4T (Kart a quattro tempi)» sarebbe una
        // ripetizione, e il diario deve restare leggibile.
        return nome.Contains(gradino.Name, StringComparison.OrdinalIgnoreCase)
            ? nome
            : $"{nome} · {gradino.Name.ToLowerInvariant()}";
    }

    /// <summary>Il nome del circuito per esteso, non l'identificativo della cartella.</summary>
    private string NomeCircuito(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return "circuito non indicato";
        var pista = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(pista?.Name) ? NarrativeEngine.Capitalize(id.Replace('_', ' ')) : pista.Name;
    }

    /// <summary>«terzo» invece di «P3»: il diario si legge, non si decodifica.</summary>
    private static string Ordinale(int posizione) => posizione switch
    {
        1 => "primo", 2 => "secondo", 3 => "terzo", 4 => "quarto", 5 => "quinto",
        6 => "sesto", 7 => "settimo", 8 => "ottavo", 9 => "nono", 10 => "decimo",
        11 => "undicesimo", 12 => "dodicesimo", 13 => "tredicesimo", 14 => "quattordicesimo",
        15 => "quindicesimo", 16 => "sedicesimo", 17 => "diciassettesimo", 18 => "diciottesimo",
        19 => "diciannovesimo", 20 => "ventesimo",
        <= 0 => "senza classifica",
        _ => $"{posizione}°"
    };

    private void RefreshHomeCalendar()
    {
        if (homeCalendarTimeline.IsDisposed) return;
        homeCalendarTimeline.SuspendLayout();
        foreach (Control child in homeCalendarTimeline.Controls) child.Dispose();
        homeCalendarTimeline.Controls.Clear();
        var entries = new List<(DateTime Date, string Kicker, string Title, string Detail, Color Accent, bool Next)>();
        foreach (var test in career.TestHistory)
        {
            var outcome = string.IsNullOrWhiteSpace(test.Outcome) ? "TEST ARCHIVIATO" : test.Outcome;
            var time = test.BestLapMilliseconds > 0 ? FormatLap(test.BestLapMilliseconds) : "nessun giro valido";
            var fitness = test.FitnessAfter > 0 ? test.FitnessAfter : career.Fitness;
            var trust = test.TrustAfter > 0 ? test.TrustAfter : career.TeamRelation;
            entries.Add((test.StoryDate, outcome,
                $"Prova con {NomeVettura(test.Car)} a {NomeCircuito(test.Track)}",
                $"Miglior giro {time} · forma {fitness}/100 · fiducia del team {trust}/100 · budget € {test.CashDelta:+#,##0;-#,##0;0}",
                outcome.StartsWith("SUPERATO", StringComparison.OrdinalIgnoreCase) ? UiTheme.Positive : UiTheme.Warning, false));
        }
        foreach (var race in career.RaceHistory)
        {
            var fitness = race.FitnessAfter > 0 ? race.FitnessAfter : career.Fitness;
            var trust = race.TrustAfter > 0 ? race.TrustAfter : career.TeamRelation;
            var inGriglia = race.Classification?.Count ?? 0;
            var esito = race.Dnf
                ? "RITIRATO"
                : inGriglia > 1 ? $"ARRIVATO {Ordinale(race.Position).ToUpperInvariant()} SU {inGriglia}"
                : $"ARRIVATO {Ordinale(race.Position).ToUpperInvariant()}";
            entries.Add((race.StoryDate, esito,
                $"Gara con {NomeVettura(race.Car)} a {NomeCircuito(race.Track)}",
                $"{race.Points} punti · premio € {race.Prize:N0} · forma {fitness}/100 · fiducia del team {trust}/100 · budget € {race.CashDelta:+#,##0;-#,##0;0}",
                race.Dnf ? UiTheme.Accent : UiTheme.Info, false));
        }
        var next = NextScheduled();
        if (next != null)
            entries.Add((next.Date, "PROSSIMO APPUNTAMENTO", CareerScheduler.Describe(next), next.Objective, UiTheme.Warning, true));

        var width = Math.Max(310, homeCalendarTimeline.ClientSize.Width - 24);
        // Il prossimo appuntamento in cima, poi la storia dal piu recente al
        // piu vecchio.
        //
        // Prima si vedevano solo gli ultimi tre eventi in ordine crescente, con
        // il prossimo appuntamento in fondo: la storia della carriera veniva
        // buttata via a ogni gara, e la cosa da fare adesso finiva dove si
        // guarda per ultimo. Ora nulla viene cancellato — la lista scorre.
        var selected = entries.Where(x => x.Next)
            .Concat(entries.Where(x => !x.Next).OrderByDescending(x => x.Date))
            .ToList();
        if (selected.Count == 0)
            homeCalendarTimeline.Controls.Add(new Label { Text = "Nessun evento archiviato: il primo test entrerà qui con tempo, conseguenze e prospettive.", Width = width, Height = 40, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted });
        foreach (var entry in selected)
        {
            var isToday = entry.Date.Date == career.StoryDate.Date;
            var row = new Panel { Width = width, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = entry.Next ? Color.FromArgb(38, 48, 39) : UiTheme.SurfaceRaised, Margin = new Padding(0, 0, 0, 5), Padding = new Padding(10, 5, 8, 4) };
            row.Paint += (_, e) => { using var brush = new SolidBrush(entry.Accent); e.Graphics.FillRectangle(brush, 0, 0, 3, row.Height); };
            var rowLayout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
            rowLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // La data per esteso: «03 lug» costringeva a decifrare, e in una
            // carriera lunga anni serve sapere anche di che anno si parla.
            var quando = entry.Date.ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
            rowLayout.Controls.Add(StepLine(
                $"{(isToday ? "OGGI · " : "")}{quando}  ·  {entry.Title}",
                new Font(UiTheme.FamilySemibold, isToday || entry.Next ? 9F : 8F, FontStyle.Bold),
                entry.Accent, Math.Max(220, width - 20), 0, 2), 0, 0);
            rowLayout.RowCount = 3;
            rowLayout.Controls.Add(StepLine(entry.Kicker, new Font(UiTheme.FamilySemibold, 8F, FontStyle.Bold),
                entry.Accent, Math.Max(220, width - 20), 0, 1), 0, 1);
            rowLayout.Controls.Add(StepLine(entry.Detail, UiTheme.Small, UiTheme.TextSecondary, Math.Max(220, width - 20), 0, 0), 0, 2);
            row.Controls.Add(rowLayout);
            homeCalendarTimeline.Controls.Add(row);
        }
        homeCalendarTimeline.ResumeLayout();
    }

    private Control BuildActionColumn()
    {
        var column = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, AutoScroll = true, Padding = new Padding(0, 0, UiTheme.Gutter, 0) };
        var stack = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };

        var weekendCard = UiTheme.Card("il tuo weekend", out var weekend);
        weekendCard.Dock = DockStyle.Top;
        // Tutti i comandi della giornata (storia, sessione, simulazione e
        // archivi) devono restare leggibili: con 498 px gli ultimi pulsanti
        // venivano tagliati dal pannello interno a causa del Dock=Top.
        weekendCard.Height = 680;
        decisionSummary = new Label
        {
            Dock = DockStyle.Top, Height = 102, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary,
            TextAlign = ContentAlignment.TopLeft, AutoEllipsis = false, Padding = new Padding(0, 2, 0, 6)
        };
        pendingBanner = new Label
        {
            Dock = DockStyle.Top, Height = 34, Font = UiTheme.Small, ForeColor = UiTheme.Warning,
            TextAlign = ContentAlignment.MiddleLeft, Visible = false, AutoEllipsis = false
        };
        continueStory = UiTheme.PrimaryButton("CONTINUA LA STORIA");
        continueStory.Height = 42;
        continueStory.Click += (_, _) => ContinueStory();
        launch = UiTheme.SecondaryButton("APRI IN CONTENT MANAGER");
        launch.Click += (_, _) => LaunchWeekend();
        briefing = UiTheme.SecondaryButton("PREPARA IL TEST");
        briefing.Click += (_, _) => Briefing();
        activities = UiTheme.SecondaryButton("Agenda");
        activities.Click += (_, _) => OpenActivities();
        nextSeason = UiTheme.SecondaryButton("Avanza stagione");
        nextSeason.Click += (_, _) =>
        {
            if (NextScheduled()?.Kind == ScheduledEventKind.Invitation && !awaitingResult) DeclineInvitation();
            else AdvanceSeason();
        };
        var checkResult = UiTheme.SecondaryButton("Referto di Assetto Corsa");
        checkResult.Click += (_, _) => CheckResultNow();
        // Alternativa al referto: risolve la sessione senza aprire Assetto
        // Corsa. Il risultato resta marcato come simulato.
        simulate = UiTheme.SecondaryButton("DEBUG · simula risultato");
        simulate.Click += (_, _) => SimulateResultNow();
        // Strumenti di prova: forzano un esito per verificare tutta la catena
        // delle conseguenze senza ripetere la simulazione finché il caso
        // produce il risultato che si vuole osservare.
        simulateGood = UiTheme.SecondaryButton("PROVA · esito positivo");
        simulateGood.ForeColor = UiTheme.Positive;
        simulateGood.Click += (_, _) => SimulateResultNow(SimulationBias.Positive);
        simulateBad = UiTheme.SecondaryButton("PROVA · esito negativo");
        simulateBad.ForeColor = UiTheme.Accent;
        simulateBad.Click += (_, _) => SimulateResultNow(SimulationBias.Negative);
        opportunities = UiTheme.SecondaryButton("Sponsor");
        opportunities.Click += (_, _) => OpenSponsorSearch();
        // Dock=Top impila in ordine inverso di aggiunta.
        weekend.Controls.Add(simulateBad);
        weekend.Controls.Add(simulateGood);
        weekend.Controls.Add(simulate);
        weekend.Controls.Add(checkResult);
        weekend.Controls.Add(nextSeason);
        weekend.Controls.Add(opportunities);
        weekend.Controls.Add(activities);
        weekend.Controls.Add(briefing);
        weekend.Controls.Add(launch);
        weekend.Controls.Add(pendingBanner);
        weekend.Controls.Add(continueStory);
        weekend.Controls.Add(decisionSummary);

        var sectionsCard = UiTheme.Card("sezioni del portale", out var sections, UiTheme.Info);
        sectionsCard.Dock = DockStyle.Top;
        sectionsCard.Height = 216;
        foreach (var (text, action) in new (string, Action)[]
        {
            ("Calendario e classifica", OpenCalendar),
            ("Paddock e relazioni", OpenPaddock),
            ("Mercato e scouting", OpenMarket),
            ("Giornale e archivio", OpenMedia),
            ("Dossier tecnico", OpenTechnicalDossier)
        })
        {
            var button = UiTheme.SecondaryButton(text);
            button.Click += (_, _) => action();
            sections.Controls.Add(button);
        }

        var manageCard = UiTheme.Card("gestione", out var manage, UiTheme.TextMuted);
        manageCard.Dock = DockStyle.Top;
        manageCard.Height = 292;
        foreach (var (text, action) in new (string, Action)[]
        {
            ("Aggiorna contenuti", RefreshContents),
            ("Carriere salvate", OpenCareerSlots),
            ("Nuova carriera", NewCareer),
            ("Servizio del giorno", NarrateCurrentStory),
            ("Tavole del manga", OpenComicGallery),
            // Unica voce della vecchia barra «articoli e servizi» che non era un
            // duplicato: le altre esistono già come sezioni del portale.
            ("Storia guidata del pilota", () => NarrationService.OpenCareerLaunchStories(career, replay: true))
        })
        {
            var button = UiTheme.SecondaryButton(text);
            button.Click += (_, _) => action();
            manage.Controls.Add(button);
        }

        stack.Controls.Add(manageCard);
        stack.Controls.Add(sectionsCard);
        stack.Controls.Add(weekendCard);
        column.Controls.Add(stack);
        // La colonna parte sempre dall'alto: su schermi bassi il contenuto scorre,
        // ma la prima carta non deve trovarsi già fuori vista all'apertura.
        column.HandleCreated += (_, _) => column.AutoScrollPosition = Point.Empty;
        return column;
    }

    private void OpenComicGallery()
    {
        using var gallery = new ComicGalleryDialog(career);
        gallery.ShowDialog(this);
    }

    private void OpenCareerHub()
    {
        var raceableCars = ContentAvailability.RaceableCars(contentIndex.Cars);
        var contentStatus = ContentAvailability.HomeStatus(raceableCars, contentIndex.Tracks.Count);
        using var hub = new CareerHubDialog(career, NextScheduled(), awaitingResult, contentStatus, continueStory.Text,
            ContinueStory, OpenCalendar, OpenMarket, OpenPaddock, OpenActivities, RefreshContents,
            () => { hubToClose?.Hide(); OpenCareerManager(); },
            Path.Combine(saveDir, "careers"));
        hubToClose = hub;
        hub.ShowDialog(this);
        hubToClose = null;
        RefreshUi();
    }

    private Control BuildEditorialColumn()
    {
        var column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Padding = new Padding(0, 0, UiTheme.Gutter, 0) };
        // L'articolo prende quasi due terzi della colonna: e la cosa che si
        // legge. Il diario resta sotto come cronaca di cio che c'e stato prima.
        column.RowStyles.Add(new RowStyle(SizeType.Percent, 72));
        column.RowStyles.Add(new RowStyle(SizeType.Percent, 28));

        var heroCard = UiTheme.Card("il servizio di apertura", out var hero);
        heroCard.Dock = DockStyle.Fill;
        // Tutto l'articolo scorre in un solo flusso: occhiello, titolo, firma,
        // sottotitolo, i paragrafi, il verdetto. Prima esistevano solo titolo e
        // sottotitolo e il corpo del pezzo restava chiuso in un'altra finestra.
        heroBody = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 8, 0)
        };
        hero.Controls.Add(heroBody);
        column.Controls.Add(heroCard, 0, 0);

        // Il giornale è il diario: ogni voce ha la sua data e apre il proprio
        // articolo. Prima esisteva un solo articolo e, a parte, una barra di
        // scorciatoie senza date né ordine cronologico.
        var diaryCard = UiTheme.Card("giornale della carriera · diario datato", out var diaryContent);
        diaryCard.Dock = DockStyle.Fill;
        diaryFooter = new Label { Dock = DockStyle.Bottom, Height = 20, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted };
        diary = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.Transparent
        };
        diaryContent.Controls.Add(diary);
        diaryContent.Controls.Add(diaryFooter);
        column.Controls.Add(diaryCard, 0, 1);

        return column;
    }

    /// <summary>
    /// Il tempo della carriera: cosa arriva, e cosa c'e stato.
    ///
    /// Prima qui c'erano cinque riquadri di etichette e valori — pilota,
    /// prossima sessione, classifica, dossier, ultimo weekend, stato contenuti.
    /// Erano dati da consultare: non si capiva ne dove si stava andando ne da
    /// dove si veniva. La classifica e il dettaglio del weekend restano dove si
    /// approfondiscono, in "Calendario e classifica" e "Giornale e archivio".
    /// </summary>
    private Control BuildDossierColumn()
    {
        var column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        column.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        column.RowStyles.Add(new RowStyle(SizeType.Percent, 44));
        column.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        column.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        // --- il pilota
        var driverCard = UiTheme.Card("", out var driverContent);
        driverCard.Dock = DockStyle.Fill;
        var driverGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
        driverGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
        driverGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var portraitPath = AssetPaths.File("fictional-driver-portrait.png");
        if (File.Exists(portraitPath))
        {
            try
            {
                portraitImage = new PictureBox
                {
                    Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = UiTheme.SurfaceRaised,
                    Image = Image.FromFile(portraitPath), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 10, 0)
                };
                portraitImage.Click += (_, _) => EditProfile();
                driverGrid.Controls.Add(portraitImage, 0, 0);
            }
            catch (Exception error) { CareerLog.Warn("ui", $"ritratto non caricato: {error.Message}"); }
        }
        var driverText = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
        driverText.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        driverText.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        profileName = new Label { Dock = DockStyle.Fill, Font = UiTheme.HeadlineSmall, ForeColor = UiTheme.TextPrimary, AutoEllipsis = true, Margin = new Padding(0) };
        profileSeat = new Label { Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary, Margin = new Padding(0) };
        driverText.Controls.Add(profileName, 0, 0);
        driverText.Controls.Add(profileSeat, 0, 1);
        driverGrid.Controls.Add(driverText, 1, 0);
        driverContent.Controls.Add(driverGrid);
        column.Controls.Add(driverCard, 0, 0);

        // --- il calendario: cosa arriva, e perche
        var calendarCard = UiTheme.Card("il calendario · cosa arriva", out var calendarContent, UiTheme.Warning);
        calendarCard.Dock = DockStyle.Fill;
        calendarFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.Transparent
        };
        calendarContent.Controls.Add(calendarFlow);
        column.Controls.Add(calendarCard, 0, 1);

        // --- l'ascesa: da dove viene
        var ascentCard = UiTheme.Card("l'ascesa · stagione per stagione", out var ascentContent, UiTheme.Positive);
        ascentCard.Dock = DockStyle.Fill;
        ascentFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.Transparent
        };
        ascentContent.Controls.Add(ascentFlow);
        column.Controls.Add(ascentCard, 0, 2);

        // --- stato contenuti
        var statusCard = UiTheme.Card("", out var statusContent);
        statusCard.Dock = DockStyle.Fill;
        contentStatus = new Label { Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        statusContent.Controls.Add(contentStatus);
        column.Controls.Add(statusCard, 0, 3);

        return column;
    }

    private Control BuildStatusBar()
    {
        var bar = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = UiTheme.HeaderBackground, Padding = new Padding(24, 0, 24, 0) };
        bar.Paint += (_, e) => { using var pen = new Pen(UiTheme.Border); e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0); };
        saveStatus = new Label { Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Text = "Autosave: in attesa" };
        bar.Controls.Add(saveStatus);
        return bar;
    }

    // ------------------------------------------------------------ aggiornamento

    private void RefreshUi()
    {
        SuspendLayout();
        try
        {
            RefreshHeaderAndStats();
            RefreshCareerHome();
            RefreshHomeCalendar();
            RefreshDriverCard();
            RefreshWeekendControls();
            RefreshEditorial();
            RefreshDossier();
            RefreshCalendar();
            RefreshAscent();
            // Il silenzio nei collaudi automatici è imposto dal servizio audio.
            var moodEvent = career.Events.OrderByDescending(x => x.DateUtc).FirstOrDefault();
            SoundtrackService.PlayForMood(moodEvent == null ? "alba" : SoundtrackService.SuggestedMood(moodEvent.Type));
        }
        finally { ResumeLayout(true); }

        // La presentazione arriva dopo l'aggiornamento: la schermata dietro deve
        // gia mostrare lo stato nuovo quando l'introduzione si chiude.
        AnnouncePhaseIfNew();
        RegistraStradaDalSedile();
        AnnounceSelectionIfCalled();
        NarrateHomeEssentialsIfEnabled();
    }

    private void NarrateHomeEssentialsIfEnabled()
    {
        if (!career.NarrationOnStartup || string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal)) return;
        var next = NextScheduled();
        var key = $"home|{career.StoryDate:O}|{career.Team}|{career.Cash}|{career.Headline}|{next?.TrackId}|{next?.Date:O}";
        if (key == automaticNarrationKey) return;
        automaticNarrationKey = key;
        var nextText = next == null ? "nessun appuntamento fissato" : $"il prossimo appuntamento è {next.Kind} a {next.TrackId}, il {next.Date:dd MMMM}";
        NarrationService.Speak($"Home carriera. {career.Driver}, {career.Team}, cassa disponibile {career.Cash} euro. {nextText}. {career.Headline}", "Automatica");
    }

    private void NarrateCardEssentials(string key, string text)
    {
        if (!career.NarrationOnStartup || string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal)) return;
        automaticNarrationKey = key;
        NarrationService.Speak(text, "Automatica");
    }

    private void RefreshHeaderAndStats()
    {
        var raceableCars = ContentAvailability.RaceableCars(contentIndex.Cars);
        // La disponibilità si misura sui contenuti installati, non sul calendario:
        // durante la valutazione un calendario di campionato non esiste ancora.
        var missingContent = ContentAvailability.HomeStatus(raceableCars, contentIndex.Tracks.Count);
        var evaluation = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase);
        var storyDate = career.StoryDate.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
        var scheduled = NextScheduled();

        if (ContentAvailability.IsDebugFixture(contentIndex))
            // La riga di avviso diceva al giocatore una cosa che ha scelto lui
            // e che e' gia nella fascia dei dati: occupava la seconda riga della
            // testata senza aggiungere niente.
            headerRound.Text = "";
        else if (!string.IsNullOrWhiteSpace(missingContent)) headerRound.Text = missingContent;
        else if (evaluation)
            headerRound.Text = scheduled == null
                ? $"ROOKIE EVALUATION · in attesa del prossimo appuntamento"
                : $"{(scheduled.Kind == ScheduledEventKind.ConfirmationTest ? "PROVA DI CONFERMA" : scheduled.Kind == ScheduledEventKind.Invitation ? "GARA SU INVITO" : "ROOKIE EVALUATION")} · tentativo {career.EvaluationAttempts + 1} · {(string.IsNullOrWhiteSpace(scheduled.TrackName) ? scheduled.TrackId : scheduled.TrackName)} · obiettivo {FormatLap(career.EvaluationTargetMilliseconds)}";
        else if (rounds.Count == 0) headerRound.Text = $"STAGIONE {career.Season} · calendario non ancora pubblicato";
        else if (career.Round >= rounds.Count) headerRound.Text = $"STAGIONE {career.Season} CONCLUSA · in attesa del passaggio di campionato";
        else headerRound.Text = $"STAGIONE {career.Season} · ROUND {career.Round + 1}/{rounds.Count} · {rounds[career.Round].GrandPrix} · {rounds[career.Round].Date}";
        headerRound.ForeColor = string.IsNullOrWhiteSpace(missingContent) ? UiTheme.TextPrimary : UiTheme.Accent;
        // Categoria della vettura, campionato e altezza raggiunta: prima qui
        // c'era solo il nome del campionato accanto alla categoria interna
        // («Campionato regionale · Categoria regionale»), e non si capiva né su
        // che mezzo si corresse né quanto in alto si fosse arrivati.
        // Il nome del gradino, non la categoria grezza del contenuto: «kart»
        // vale per tre gradini diversi e da solo non dice a che punto della
        // gavetta si e'. Accanto ci va il numero, perche' le altezze sono due —
        // categoria e campionato — e il quadro e' completo solo con entrambe.
        var gradinoTestata = CareerLadder.Current(career, contentIndex.Cars);
        headerChampionship.Text = ChampionshipLadder.Header(
            gradinoTestata.Name, gradinoTestata.Step, CareerLadder.Steps, career.ChampionshipLevel);

        // La data e la situazione sono il punto di riferimento della giornata:
        // devono restare visibili anche quando non c'e' una gara fissata.
        todayDateLine.Text = $"OGGI · {storyDate:dddd d MMMM yyyy}".ToUpperInvariant();
        todayDateLine.Visible = true;
        situationLine.Text = BuildSituationLine();
        situationLine.Visible = true;
        budgetPanel.Update(career);
    }

    /// <summary>
    /// Fa scorrere il calendario senza saltare un appuntamento pianificato.
    /// Il comando della settimana si ferma al giorno dell'evento; quello del
    /// prossimo appuntamento raggiunge esattamente il suo giorno.
    /// </summary>
    /// <summary>
    /// Fa scorrere il calendario e si ferma quando c'è qualcosa da decidere.
    /// </summary>
    /// <param name="giorni">Quanti giorni; zero significa «fino al prossimo impegno».</param>
    private void AvanzaNelCalendario(int giorni)
    {
        if (BlockIfPending("Far passare il tempo")) return;
        var prima = career.StoryDate.Date;
        var appuntamento = NextScheduled();

        // C'è già qualcosa oggi: il calendario non può scavalcarlo, e restare in
        // silenzio faceva sembrare i pulsanti rotti. Si dice che cosa c'è e si
        // chiede: si va, oppure si salta e se ne pagano le conseguenze.
        if (appuntamento != null && appuntamento.Date.Date <= prima && appuntamento.IsPlanned)
        {
            ChiediSeAffrontare(appuntamento);
            return;
        }

        if (appuntamento == null && giorni == 0)
        {
            CareerMessages.Show(this,
                "Non c'è nessun impegno in calendario da raggiungere.\n\n"
                + "Usa «vai a domani» per far passare i giorni: nel frattempo il paddock si muove e possono arrivare proposte.",
                "Calendario");
            return;
        }

        AdvanceCalendar(giorni <= 0 ? 3650 : giorni, giorni <= 0);
        if (career.StoryDate.Date == prima) return;

        // Arrivati al giorno dell'impegno, si chiede: è il momento in cui il
        // giocatore decide se esserci, e prima passava senza che nessuno lo
        // dicesse — l'appuntamento restava lì e la giornata scorreva.
        var oggi = NextScheduled();
        if (oggi == null || oggi.Date.Date > career.StoryDate.Date) return;
        ChiediSeAffrontare(oggi);
    }

    /// <summary>
    /// L'avviso del giorno dell'impegno: che cosa c'è, e la scelta fra esserci
    /// e saltarlo.
    ///
    /// Il calendario non può scavalcare un appuntamento, e prima non lo diceva:
    /// si premeva «vai a domani», non succedeva niente, e sembrava un pulsante
    /// rotto. Saltare deve restare possibile — capita di non avere i soldi o la
    /// condizione — ma dev'essere una scelta dichiarata, con le sue conseguenze.
    /// </summary>
    private void ChiediSeAffrontare(ScheduledEvent impegno)
    {
        if (CareerMessages.Unattended) return;
        var dove = string.IsNullOrWhiteSpace(impegno.TrackName) ? impegno.TrackId : impegno.TrackName;
        var oggi = impegno.Date.Date <= career.StoryDate.Date;
        var cosa = impegno.Kind switch
        {
            ScheduledEventKind.ChampionshipRound => $"il round {impegno.Round} di campionato",
            ScheduledEventKind.Invitation => "una gara su invito",
            ScheduledEventKind.ConfirmationTest => "la prova di conferma",
            _ => "una prova di valutazione"
        };
        var quota = impegno.EntryFee > 0 && !impegno.EntryFeePaid
            ? $"\nQuota d'iscrizione: € {impegno.EntryFee:N0} · in cassa hai € {career.Cash:N0}."
            : "";

        // Che cosa costa saltarlo, detto prima di scegliere e non dopo.
        var prezzo = RaceChoice.CostOfSkipping(career, impegno);
        var conseguenze = new List<string>();
        if (prezzo.Popularity != 0) conseguenze.Add($"seguito {prezzo.Popularity:+#;-#;0}");
        if (prezzo.SportingReputation != 0) conseguenze.Add($"prestigio sportivo {prezzo.SportingReputation:+#;-#;0}");
        if (prezzo.TeamTrust != 0) conseguenze.Add($"fiducia del team {prezzo.TeamTrust:+#;-#;0}");
        var costoSalto = conseguenze.Count > 0 ? $" ({string.Join(" · ", conseguenze)})" : "";

        var risposta = CareerMessages.Ask(this,
            (oggi ? $"Oggi c'è {cosa}, a {dove}." : $"È il giorno di {cosa}, a {dove}.")
            + $"\n\n{impegno.Objective}{quota}\n\n"
            + "Il calendario non può andare avanti finché non decidi.\n\n"
            + "SÌ = si va in pista\n"
            + $"NO = salti l'impegno{costoSalto}\n"
            + "ANNULLA = resti fermo, decidi più tardi",
            $"CorsaCareer — {NarrativeCalendar.Format(impegno.Date)}",
            MessageBoxButtons.YesNoCancel, DialogResult.Cancel);

        if (risposta == DialogResult.Yes) { ContinueStory(); return; }
        if (risposta != DialogResult.No) return;

        // Saltato per scelta: lo si registra come tale, con il suo prezzo.
        RaceChoice.ApplySkip(career, impegno);
        var titolo = $"{career.Driver} salta {cosa} a {dove}.";
        career.News.Add(titolo);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "EVENT_SKIPPED",
            Headline = titolo + (costoSalto.Length > 0 ? $" Costo:{costoSalto}" : ""),
            Track = dove, Importance = 55
        });
        CareerLog.Info("agenda", $"impegno saltato per scelta: {impegno.Id} · {dove}");
        SaveCareer();
        RefreshUi();
    }

    private void AdvanceCalendar(int requestedDays, bool untilNext)
    {
        var today = career.StoryDate.Date;
        var next = NextScheduled();
        var daysToNext = next == null ? int.MaxValue : Math.Max(0, (next.Date.Date - today).Days);
        var days = untilNext
            ? (next == null ? 1 : daysToNext)
            : next == null
                ? Math.Max(1, requestedDays)
                : Math.Min(Math.Max(1, requestedDays), daysToNext);

        if (days <= 0) return;

        for (var i = 0; i < days; i++)
        {
            var day = DriverDay.EnsureToday(career);
            career.Today = DayEngine.Advance(career, day);
        }

        // Il paddock si muove insieme al calendario.
        //
        // Prima le proposte nascevano solo dopo una gara: chi restava senza
        // impegni non riceveva piu' niente, e non avendo gare non poteva
        // generarne altre. Far passare i giorni e' il modo in cui si cercano
        // occasioni quando non si corre — e' qui che devono arrivare.
        RefreshOpportunities();
        ChiudiLaStagioneSeLAnnoSportivoEFinito();

        SaveCareer(createVersionedBackup: false);
        // Il ridisegno serve a chi guarda. Il banco di prova percorre migliaia
        // di giornate una dopo l'altra: ridisegnare il portale per ognuna
        // costava piu' del resto della simulazione messa insieme.
        if (!CareerMessages.Unattended) RefreshUi();
    }

    /// <summary>
    /// L'inverno chiude la stagione.
    ///
    /// La stagione finiva solo esaurendo il calendario dei round: un pilota
    /// cliente, che compra i weekend uno alla volta e non ha un campionato,
    /// non ne esauriva mai nessuno e restava nella «stagione 1» per anni —
    /// niente classifica finale, niente pagella, niente mercato invernale.
    /// Passato l'ultimo fine settimana utile dell'anno sportivo, con l'agenda
    /// vuota, la stagione si chiude da sé: è quello che fa il calendario vero.
    /// </summary>
    private void ChiudiLaStagioneSeLAnnoSportivoEFinito()
    {
        if (career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase)) return;
        // Serve che questa stagione sia stata corsa davvero.
        if (!career.RaceHistory.Any(x => x.Season == career.Season)) return;
        var ultimaDomenica = NarrativeCalendar.LastRaceSundayOfSeason(SeasonAnchor().Year);
        if (career.StoryDate.Date <= ultimaDomenica) return;
        // Blocca la chiusura solo quello che si deve ancora correre in
        // QUEST'anno sportivo: un appuntamento gia' fissato per la primavera
        // prossima appartiene alla stagione nuova, non tiene aperta questa.
        if ((career.Schedule ?? []).Any(x => x.IsPlanned && x.Date.Date <= ultimaDomenica)) return;
        CareerLog.Info("stagione", $"l'anno sportivo {SeasonAnchor().Year} e' finito: la stagione {career.Season} si chiude.");
        AdvanceSeason();
    }

    private void RefreshCareerHome()
    {
        var phase = CareerPhases.Current(career);
        var next = NextScheduled();
        homeKicker.Text = phase.Flash.ToUpperInvariant();
        homeTitle.Text = phase.Title;
        homeObjective.Text = phase.Objective + "\n\n" + phase.Stake;
        RenderStepBriefing(next);
        // Le tavole seguono il passo: quando questo cambia va rifatta la
        // sequenza, altrimenti la Home resta illustrata come il momento
        // precedente. Il confronto sulla firma evita di riavviare la dissolvenza
        // a ogni refresh, che spegnerebbe l'immagine a metà.
        var artworkKey = string.Join("|", HomeArtworkFiles().Take(4));
        if (!string.Equals(artworkKey, homeArtworkKey, StringComparison.Ordinal))
        {
            homeArtworkKey = artworkKey;
            RestartHomeArtworkSequence();
        }
        homeTeam.Text = $"{career.Team}\nCompagno: {career.Teammate}\nFiducia: {career.TeamRelation}/100";
        // La cifra in cassa è già nella riga dati del passo attuale: qui conta
        // come sta il bilancio, non ripetere lo stesso numero una terza volta.
        homeMoney.Text = $"CASSA PERSONALE\n€ {career.Cash:N0} · {CareerFinances.Status(career.Cash)}\nSPONSOR\n{career.Sponsor} · budget € {career.SponsorBudget:N0}\nTEAM SUPPORT\n{career.TeamSupportPercent}% dei costi coperti\nRapporto sponsor: {career.SponsorRelation}/100";
        var raceable = ContentAvailability.RaceableCars(contentIndex.Cars);
        homeContents.Text = $"{raceable} vetture da gara\n{contentIndex.Tracks.Count} circuiti\n" +
            (ContentAvailability.IsDebugFixture(contentIndex) ? "Contenuti fittizi di prova" : "Contenuti reali installati");
        var open = (career.Opportunities ?? []).Count(x => x.IsOpen);
        TeamInterestService.Refresh(career);
        var scouts = string.Join("\n", career.TeamInterests.OrderByDescending(x => x.Value).Take(2).Select(x => $"{x.Team}: {x.Value}% · {x.Status}"));
        homeMarket.Text = career.ContractActive
            ? $"Contratto attivo\n€ {career.ContractSalary:N0}/anno\n{open} proposte aperte"
            : $"Nessun contratto\n\n{scouts}";
    }

    private void SimulateDebugFromHome()
    {
        if (!ContentAvailability.IsDebugFixture(contentIndex))
        {
            CareerMessages.Show(null, "Questo comando è disponibile solo nella carriera debug con contenuti fittizi.", "CorsaCareer — debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!awaitingResult) StartDebugTestSession();
        if (awaitingResult) SimulateResultNow();
    }

    private void StartDebugTestSession()
    {
        var scheduled = NextScheduled();
        if (scheduled == null || (!scheduled.IsTest && scheduled.Kind != ScheduledEventKind.Invitation))
        {
            CareerMessages.Show(null, "Non c'è un test o una gara cliente in agenda da simulare.", "CorsaCareer — debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase) && ContentCategoryRules.IsRaceable(x));
        var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(scheduled.TrackId, StringComparison.OrdinalIgnoreCase));
        if (car == null || track == null)
        {
            CareerMessages.Show(null, "Il test debug richiede l'auto e il circuito fittizi indicati nel briefing.", "CorsaCareer — debug", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var isTest = scheduled.IsTest;
        var plan = BuildSessionPlan(track.Id, car.Id, mixedGrid: !isTest, testSession: isTest);
        pendingMode = isTest ? "test" : "invitation"; awaitingResult = true; launchTimeUtc = DateTime.UtcNow; pendingResultHash = "";
        ArchiveSessionPlan(plan, pendingMode, track.Id, car.Id);
        var pending = new { mode = pendingMode, debug = true, car = car.Id, track = track.Id, driver = career.Driver, launchUtc = launchTimeUtc, format = plan.FormatLabel };
        File.WriteAllText(Path.Combine(saveDir, "pending_weekend.json"), JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true }));
        SaveCareer(); RefreshUi();
    }

    /// <summary>
    /// Impagina il passo attuale. La gerarchia è il punto: il titolo si legge da
    /// lontano, il racconto spiega, la richiesta è l'unica riga in evidenza e i
    /// dati tecnici restano leggibili senza rubare la scena.
    /// </summary>
    private void RenderStepBriefing(ScheduledEvent? next)
    {
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
        var track = next == null
            ? null
            : contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(next.TrackId, StringComparison.OrdinalIgnoreCase));
        var briefing = StepBriefingBuilder.Build(career, next, OpenSelection, awaitingResult, car, track,
            FormatLap(career.EvaluationTargetMilliseconds));

        homeStep.SuspendLayout();
        foreach (Control existing in homeStep.Controls) existing.Dispose();
        homeStep.Controls.Clear();

        var width = Math.Max(280, homeStep.ClientSize.Width - homeStep.Padding.Horizontal - 24);

        // L'avviso dell'articolo, se ce n'è uno da leggere. Compare una volta
        // sola: al ridisegno successivo — cioè non appena il giocatore fa
        // qualcos'altro — viene lasciato cadere senza chiedere niente.
        if (pendingArticleOpen != null)
        {
            if (pendingArticleRedraws++ > 0)
            {
                pendingArticleOpen = null;
                pendingArticleTitle = "";
            }
            else
            {
                var avviso = new ArticleNotice { Width = width };
                var apri = pendingArticleOpen;
                avviso.Announce(pendingArticleTitle, () =>
                {
                    pendingArticleOpen = null;
                    pendingArticleTitle = "";
                    apri();
                });
                homeStep.Controls.Add(avviso);
                articleNotice = avviso;
            }
        }

        // La giornata sta in un riquadro solo. Erano tre cornici impilate —
        // oggi, il racconto, la richiesta — e il testo si leggeva a scatti:
        // ogni bordo faceva ricominciare la lettura da capo, come se la stessa
        // giornata fossero tre notizie diverse. Le parti restano distinte, ma
        // separate da un filetto sottile dentro un blocco unico.
        homeStep.Controls.Add(BuildTodayCard(briefing, next, width));
        homeStep.ResumeLayout(true);
        // Il FlowLayoutPanel riceve la larghezza definitiva solo dopo il layout
        // della card: ricalcoliamo subito le righe per evitare testo troncato o
        // una scrollbar orizzontale quando la finestra cambia dimensione.
        FitStepLines();
    }

    /// <summary>Larghezza della colonna dell'icona nell'area Oggi.</summary>
    private const int TodayIconColumnWidth = 78;

    /// <summary>
    /// La giornata in un riquadro solo, letta come una scheda: prima che cosa
    /// c'è oggi, poi i dati che servono per decidere — dove, con che macchina,
    /// che cosa serve ottenere, che cosa si rischia — e in fondo il racconto.
    ///
    /// Erano tre cornici impilate con lo stesso circuito e la stessa data
    /// ripetuti in ogni riga: il giocatore rileggeva quattro volte «Akagi
    /// Highland Kart · 13 marzo» prima di sapere che tempo doveva girare. Qui
    /// ogni informazione compare una volta sola, alla sua etichetta.
    /// </summary>
    private Control BuildTodayCard(StepBriefing briefing, ScheduledEvent? next, int width)
    {
        var card = new Panel
        {
            Width = width,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiTheme.SurfaceRaised,
            Margin = new Padding(0, 0, 0, 9),
            Padding = new Padding(16, 12, 16, 14)
        };
        card.Paint += (_, e) =>
        {
            using var border = new Pen(UiTheme.Warning, 2);
            using var accent = new SolidBrush(UiTheme.Warning);
            e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
            e.Graphics.FillRectangle(accent, 0, 0, 5, card.Height);
        };

        var inner = Math.Max(240, width - card.Padding.Horizontal);
        var stack = new FlowLayoutPanel
        {
            Location = new Point(card.Padding.Left, card.Padding.Top),
            Width = inner,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        // --- che giorno è e che cosa c'è ---------------------------------
        stack.Controls.Add(BuildTodayHeader(next, inner));

        // --- i dati della giornata ---------------------------------------
        stack.Controls.Add(CardRule(inner, 11, 10));
        stack.Controls.Add(BuildTodayFacts(TodayFactRows(briefing, next), inner));

        // --- la classifica del campionato -----------------------------------
        //
        // Al posto del racconto. Prima qui c'era un paragrafo narrativo — «Questa
        // volta si corre per punti» — che non aggiungeva niente a chi doveva
        // decidere: la scheda del prossimo impegno deve dire a che punto sei del
        // campionato e che cosa ti serve, non ricordarti che le gare assegnano
        // punti. La storia ha già le sue scene.
        var classifica = RigheClassifica();
        if (classifica.Count > 0)
        {
            stack.Controls.Add(CardRule(inner, 12, 8));
            stack.Controls.Add(StepLine("CLASSIFICA DEL CAMPIONATO", UiTheme.Kicker, UiTheme.Info, inner, 0, 5));
            foreach (var (riga, evidenzia) in classifica)
                stack.Controls.Add(StepLine(riga, evidenzia ? UiTheme.BodyStrong : UiTheme.Small,
                    evidenzia ? UiTheme.Warning : UiTheme.TextSecondary, inner, 0, 2));
        }

        // --- che cosa serve ottenere ----------------------------------------
        stack.Controls.Add(CardRule(inner, 12, 8));
        stack.Controls.Add(StepLine("OBIETTIVI", UiTheme.Kicker, UiTheme.Positive, inner, 0, 5));
        foreach (var obiettivo in RigheObiettivi())
            stack.Controls.Add(StepLine(obiettivo, UiTheme.Body, UiTheme.TextPrimary, inner, 0, 6));

        card.Controls.Add(stack);
        // La larghezza definitiva arriva dopo, quando il pannello della Home
        // impagina: senza questo il testo resterebbe misurato su una stima.
        card.SizeChanged += (_, _) => FitCardLines(card, stack);
        return card;
    }

    /// <summary>
    /// La classifica del campionato in corso, con la riga del pilota in
    /// evidenza. Vuota se la classifica non esiste ancora.
    /// </summary>
    private List<(string Riga, bool Io)> RigheClassifica()
    {
        var righe = new List<(string, bool)>();
        var ordinata = (career.Standings ?? [])
            .OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins)
            .ToList();
        if (ordinata.Count == 0) return righe;

        var mia = ordinata.FindIndex(x => x.Driver.Equals(career.Driver, StringComparison.OrdinalIgnoreCase));
        // Le prime cinque più la propria riga se sta più in basso: la classifica
        // intera non serve, serve sapere chi devi prendere e chi ti insegue.
        var indici = Enumerable.Range(0, Math.Min(5, ordinata.Count)).ToList();
        if (mia >= 5) { indici.Add(mia - 1); indici.Add(mia); if (mia + 1 < ordinata.Count) indici.Add(mia + 1); }
        foreach (var i in indici.Distinct().OrderBy(x => x))
        {
            var voce = ordinata[i];
            var io = i == mia;
            righe.Add(($"{i + 1,2}.  {voce.Driver,-22} {voce.Points,3} pt"
                       + (voce.Wins > 0 ? $"  ·  {voce.Wins} {(voce.Wins == 1 ? "vittoria" : "vittorie")}" : "")
                       + (io ? "   ← tu" : ""), io));
        }
        return righe;
    }

    /// <summary>
    /// Che cosa serve ottenere, detto in chiaro: la posizione che promuove, la
    /// distanza da chi ti precede, e la gavetta che manca per la categoria
    /// successiva.
    /// </summary>
    private List<string> RigheObiettivi()
    {
        var righe = new List<string>();
        var livello = ChampionshipLadder.Clamp(career.ChampionshipLevel);
        var gradino = CareerLadder.Current(career, contentIndex.Cars);

        // Durante la valutazione un campionato non esiste.
        //
        // La scheda mostrava comunque «chiudi la stagione nei primi 3» e la
        // gavetta sulla vettura, in una giornata in cui non c'è né classifica
        // né contratto: obiettivi di una cosa che non sta succedendo. Qui la
        // prova ha i suoi, che sono l'unica cosa in gioco quel giorno.
        var prossimo = NextScheduled();
        var inValutazione = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase)
                            || prossimo?.IsTest == true;
        if (inValutazione)
        {
            var obiettivo = career.EvaluationTargetMilliseconds > 0
                ? FormatLap(career.EvaluationTargetMilliseconds)
                : "il riferimento indicato";
            var migliore = career.EvaluationBestLapMilliseconds > 0
                ? FormatLap(career.EvaluationBestLapMilliseconds)
                : "";

            righe.Add($"Gira in {obiettivo} o meglio: è il tempo che il paddock ha messo come condizione per aprirti le porte.");
            righe.Add(string.IsNullOrEmpty(migliore)
                ? "Non hai ancora un tempo registrato su questa vettura."
                : $"Il tuo miglior giro finora è {migliore}."
                  + (career.EvaluationTargetMilliseconds > 0
                      ? $" Ti mancano {(career.EvaluationBestLapMilliseconds - career.EvaluationTargetMilliseconds) / 1000.0:+0.000;-0.000;0} secondi."
                      : ""));
            righe.Add("Se ci arrivi si aprono le prime offerte di sedile. Se ci vai vicino ti concedono un'altra giornata su un circuito diverso. "
                      + "Se sei lontano, il dossier resta aperto ma servirà rifarsi vedere.");
            righe.Add($"Ogni tentativo pesa sulla cassa: hai € {career.Cash:N0}. Qui non si vince niente, si compra il diritto a un secondo sguardo.");
            return righe;
        }

        righe.Add(ChampionshipLadder.IsTop(livello)
            ? $"Sei al vertice: nel {ChampionshipLadder.Name(livello)} non si sale più, si difende il posto."
            : $"Chiudi la stagione nei primi {CareerProgression.PosizionePromozione} per salire a «{ChampionshipLadder.Name(livello + 1)}», "
              + $"il livello {livello + 1} di {ChampionshipLadder.Levels}.");

        if (livello > 1)
            righe.Add($"Attenzione all'ultimo quarto della classifica: chi finisce lì retrocede a «{ChampionshipLadder.Name(livello - 1)}» e perde il sedile.");

        // La distanza dal terzo posto: è il numero che dice se l'obiettivo è
        // ancora a portata oppure no.
        var ordinata = (career.Standings ?? []).OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).ToList();
        var mia = ordinata.FindIndex(x => x.Driver.Equals(career.Driver, StringComparison.OrdinalIgnoreCase));
        if (mia >= 0)
        {
            var miei = ordinata[mia].Points;
            if (mia < CareerProgression.PosizionePromozione)
            {
                var primo = ordinata[0].Points;
                righe.Add(mia == 0
                    ? $"Sei primo con {miei} punti: il secondo è a {primo - ordinata[Math.Min(1, ordinata.Count - 1)].Points} punti da te."
                    : $"Sei {mia + 1}° con {miei} punti, dentro la zona che promuove. Il primo è a {primo - miei} punti.");
            }
            else if (ordinata.Count >= CareerProgression.PosizionePromozione)
            {
                var terzo = ordinata[CareerProgression.PosizionePromozione - 1].Points;
                righe.Add($"Sei {mia + 1}° con {miei} punti: al terzo posto mancano {Math.Max(0, terzo - miei)} punti.");
            }
        }

        // La categoria è l'altra scala, e ha un'altra condizione.
        var fatte = RacesOnCurrentStep();
        var servono = OpportunityGenerator.RacesBeforeStepUp(gradino.Step);
        var sopra = CareerLadder.NextPopulatedStep(gradino.Step, contentIndex.Cars);
        if (sopra > gradino.Step)
        {
            var nomeSopra = contentIndex.Cars
                .Where(ContentCategoryRules.IsRaceable)
                .Select(x => CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg))
                .FirstOrDefault(x => x.Step == sopra)?.Name ?? "la categoria successiva";
            righe.Add(fatte >= servono
                ? $"Categoria: la gavetta è fatta ({fatte} gare su questa vettura). Le squadre di {nomeSopra.ToLowerInvariant()} possono farsi vive."
                : $"Categoria: {fatte} gare su {servono} su questa vettura. A {servono} le squadre di {nomeSopra.ToLowerInvariant()} cominciano a guardarti.");
        }

        return righe;
    }

    /// <summary>Il filetto che separa due parti della card.</summary>
    private static Panel CardRule(int width, int above, int below) => new()
    {
        Width = width,
        Height = 1,
        BackColor = UiTheme.Border,
        Margin = new Padding(0, above, 0, below)
    };

    /// <summary>
    /// I dati della giornata, nell'ordine in cui servono per decidere: quando,
    /// dove, con che cosa, che cosa serve ottenere, che cosa si rischia e come
    /// si possono usare le ore di oggi.
    /// </summary>
    private List<(string Label, string Value, bool Highlight)> TodayFactRows(StepBriefing briefing, ScheduledEvent? next)
    {
        var righe = new List<(string, string, bool)>();
        var today = career.StoryDate.Date;

        if (next != null)
        {
            var giorni = (next.Date.Date - today).Days;
            if (giorni > 0)
                righe.Add(("QUANDO", $"{next.Date:dddd d MMMM} · {(giorni == 1 ? "domani" : $"fra {giorni} giorni")}", false));
            var dove = CareerScheduler.TrackLabel(next);
            if (!string.IsNullOrWhiteSpace(dove)) righe.Add(("DOVE", dove, false));
        }
        else if (!string.IsNullOrWhiteSpace(briefing.When))
        {
            // Nessun appuntamento in agenda ma un passo che ha comunque un
            // quando — una giornata di selezione, per esempio.
            righe.Add(("QUANDO", briefing.When.TrimEnd('.'), false));
        }

        // I dati tecnici del passo, saltando quelli che l'obiettivo o il
        // rischio dicono già a parole: lo stesso tempo e la stessa cifra
        // scritti due volte erano la ripetizione più fastidiosa della scheda.
        foreach (var (label, value) in briefing.Facts)
        {
            if (RipeteGia(value, briefing.Demand) || RipeteGia(value, briefing.Stake)) continue;
            righe.Add((label.ToUpperInvariant(), value, false));
        }

        if (!string.IsNullOrWhiteSpace(briefing.Demand))
            righe.Add(("OBIETTIVO", briefing.Demand.TrimEnd('.'), true));
        if (!string.IsNullOrWhiteSpace(briefing.Stake))
            righe.Add(("IN GIOCO", briefing.Stake, false));

        // «Ore di oggi» stava qui e non c'entrava: le ore libere sono la
        // giornata del pilota, e hanno già il loro riquadro in alto a sinistra
        // con i pulsanti per usarle. Ripeterle nella scheda del prossimo
        // impegno confondeva le due cose.
        return righe;
    }

    /// <summary>Vero se il dato è già scritto, parola per parola, nella frase.</summary>
    private static bool RipeteGia(string value, string frase) =>
        !string.IsNullOrWhiteSpace(value)
        && !string.IsNullOrWhiteSpace(frase)
        && frase.Contains(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Le righe dei dati: etichetta a sinistra, valore a destra. Una colonna
    /// sola di etichette allineate rende la scheda scorribile con l'occhio —
    /// era l'informazione che si perdeva dentro i paragrafi.
    /// </summary>
    private static TableLayoutPanel BuildTodayFacts(
        List<(string Label, string Value, bool Highlight)> righe, int width)
    {
        var labelFont = UiTheme.Kicker;
        var labelWidth = righe.Count == 0
            ? 120
            : righe.Max(x => TextRenderer.MeasureText(x.Label, labelFont).Width) + 18;
        labelWidth = Math.Min(labelWidth, Math.Max(110, width / 3));

        var table = new TableLayoutPanel
        {
            Tag = TodayFactsTag,
            Width = width,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = righe.Count,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var valueWidth = Math.Max(160, width - labelWidth);
        for (var i = 0; i < righe.Count; i++)
        {
            var (label, value, highlight) = righe[i];
            // L'etichetta scende di un pelo: allineata al centro della prima
            // riga del valore, che ha un carattere più grande.
            table.Controls.Add(StepLine(label, labelFont, UiTheme.TextMuted, labelWidth - 10, 3, 6), 0, i);
            table.Controls.Add(highlight
                ? StepLine(value, UiTheme.HeadlineSmall, UiTheme.Warning, valueWidth, 0, 6)
                : StepLine(value, UiTheme.BodyStrong, UiTheme.TextPrimary, valueWidth, 0, 6), 1, i);
        }
        return table;
    }

    /// <summary>Riconosce la tabella dei dati quando va riadattata.</summary>
    private const string TodayFactsTag = "dati-oggi";

    /// <summary>Riporta le righe della card alla larghezza reale.</summary>
    private static void FitCardLines(Panel card, FlowLayoutPanel stack)
    {
        var available = Math.Max(240, card.ClientSize.Width - card.Padding.Horizontal);
        if (available == stack.Width) return;
        stack.SuspendLayout();
        stack.Width = available;
        foreach (Control control in stack.Controls)
        {
            control.Width = available;
            if (control is TableLayoutPanel table)
            {
                FitCardTable(table, available);
                continue;
            }
            if (control is Label label) FitCardLabel(label, available);
        }
        stack.ResumeLayout(true);
    }

    /// <summary>
    /// Le due tabelle della card hanno una colonna a misura fissa — l'icona
    /// nell'intestazione, le etichette nella scheda dati: a variare è solo
    /// quello che resta.
    /// </summary>
    private static void FitCardTable(TableLayoutPanel table, int available)
    {
        var dati = (table.Tag as string) == TodayFactsTag;
        var fissa = dati ? (int)table.ColumnStyles[0].Width : TodayIconColumnWidth;
        var libera = Math.Max(160, available - fissa - (dati ? 0 : 12));
        foreach (Control child in table.Controls)
        {
            if (child is not Label label) continue;
            if (dati && table.GetColumn(child) == 0) continue;   // le etichette restano
            FitCardLabel(label, libera);
        }
    }

    private static void FitCardLabel(Label label, int width)
    {
        label.Width = width;
        var measured = TextRenderer.MeasureText(string.IsNullOrEmpty(label.Text) ? "Ag" : label.Text, label.Font,
            new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        label.Height = Math.Max(label.Font.Height + 2, measured.Height + 4);
    }

    /// <summary>
    /// L'apertura della card: il giorno della carriera e, sotto, che cosa c'è
    /// in programma. Dove e quando stanno nella scheda dati subito sotto,
    /// quindi qui non vengono ripetuti.
    /// </summary>
    private Control BuildTodayHeader(ScheduledEvent? next, int width)
    {
        var today = career.StoryDate.Date;
        var plan = DriverDay.EnsureToday(career);
        var current = next != null && next.Date.Date <= today && next.IsPlanned;

        // «In attesa: round 3 di campionato» non dice niente: il giocatore
        // vuole sapere che cos'è la prossima cosa, non in che stato si trova
        // l'agenda. E la categoria va detta qui, perché è la prima domanda —
        // con che macchina si corre.
        var vettura = next == null ? "" : NomeVettura(career.Car);
        var cosa = next == null
            ? "Giornata libera"
            : next.Kind == ScheduledEventKind.ChampionshipRound
                ? $"Gara {next.Round} di campionato · {vettura}"
                : current
                    ? $"{CareerScheduler.KindLabel(next)} · {vettura}"
                    : $"Prossimo impegno: {CareerScheduler.KindLabel(next).ToLowerInvariant()} · {vettura}";

        var layout = new TableLayoutPanel
        {
            Width = width,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        var textWidth = Math.Max(200, width - TodayIconColumnWidth - 12);
        // L'area Oggi è la prima cosa che il giocatore deve leggere per capire
        // cosa fare: i caratteri del design system pensati per le etichette
        // secondarie restavano troppo piccoli per un testo che decide la
        // giornata. Dimensioni dedicate, solo per questa intestazione.
        var dateFont = new Font(UiTheme.FamilySemibold, 13F, FontStyle.Bold);
        var whatFont = new Font(UiTheme.FamilySemibold, 15F, FontStyle.Bold);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TodayIconColumnWidth));
        layout.Controls.Add(StepLine($"OGGI · {today:dddd d MMMM yyyy}".ToUpperInvariant(), dateFont, UiTheme.Warning, textWidth, 0, 4), 0, 0);
        layout.Controls.Add(StepLine(cosa, whatFont, UiTheme.TextPrimary, textWidth, 0, 0), 0, 1);

        // L'icona riassume a colpo d'occhio il mestiere della giornata senza
        // sostituire data e briefing. Ha una colonna propria e una misura
        // fissa: il testo continua quindi ad andare a capo e non viene mai
        // coperto, anche ridimensionando la finestra.
        var iconInfo = ResolveTodayActivityIcon(next, current, plan);
        var icon = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 2, 0, 2),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Image = LoadArtwork(AssetPaths.File("ui-icons", iconInfo.FileName)),
            AccessibleName = iconInfo.AccessibleName,
            TabStop = false
        };
        icon.Disposed += (_, _) => icon.Image?.Dispose();
        layout.Controls.Add(icon, 1, 0);
        layout.SetRowSpan(icon, 2);
        return layout;
    }

    /// <summary>
    /// Sceglie l'icona dell'area Oggi dalla vera attività corrente. Gli
    /// appuntamenti hanno la precedenza; nei giorni liberi viene mostrata
    /// l'ultima attività svolta, così allenamento, social e sponsor non vengono
    /// confusi con una gara.
    /// </summary>
    private static (string FileName, string AccessibleName) ResolveTodayActivityIcon(
        ScheduledEvent? next, bool current, DayPlan plan)
    {
        if (current && next != null)
        {
            return next.Kind switch
            {
                ScheduledEventKind.EvaluationTest or ScheduledEventKind.ConfirmationTest
                    => ("oggi-test.png", "Prova in pista"),
                ScheduledEventKind.Invitation
                    => ("oggi-gara.png", "Gara su invito"),
                ScheduledEventKind.ChampionshipRound
                    => ("oggi-campionato.png", "Round di campionato"),
                _ => ("oggi-calendario.png", "Appuntamento della carriera")
            };
        }

        var lastActivity = plan.Done.LastOrDefault()?.ToLowerInvariant() ?? "";
        if (lastActivity.StartsWith("haru-", StringComparison.Ordinal)
            || lastActivity.Contains("sponsor", StringComparison.Ordinal))
            return ("oggi-sponsor.png", "Ricerca sponsor con Haru");

        if (lastActivity is "social" or "tifosi" or "pr" or "youtube"
            or "instagram-allenamento" or "domande-follower" or "intervista-radio" or "scuola-kart"
            || lastActivity.Contains("social", StringComparison.Ordinal))
            return ("oggi-social.png", "Attività social e pubbliche");

        if (lastActivity is "palestra" or "corsa" or "riposo" or "massaggio")
            return ("oggi-allenamento.png", "Allenamento e recupero del pilota");

        if (lastActivity.Contains("mercato", StringComparison.Ordinal)
            || lastActivity.Contains("contratto", StringComparison.Ordinal)
            || lastActivity.Contains("team", StringComparison.Ordinal))
            return ("oggi-mercato.png", "Mercato e trattative");

        if (lastActivity.Contains("trasf", StringComparison.Ordinal)
            || lastActivity.Contains("viaggio", StringComparison.Ordinal))
            return ("oggi-trasferimento.png", "Trasferimento verso il prossimo appuntamento");

        return ("oggi-calendario.png", "Giornata di preparazione");
    }

    /// <summary>Riporta le righe alla larghezza reale del pannello.</summary>
    private void FitStepLines()
    {
        var usable = homeStep.ClientSize.Width - homeStep.Padding.Horizontal - 8;
        if (homeStep.VerticalScroll.Visible) usable -= SystemInformation.VerticalScrollBarWidth;
        if (usable <= 0) return;
        var width = Math.Max(240, usable);
        foreach (Control control in homeStep.Controls)
        {
            control.MaximumSize = new Size(width, int.MaxValue);
            control.Width = width;
            if (control is not Label label) continue;
            var measured = TextRenderer.MeasureText(string.IsNullOrEmpty(label.Text) ? "Ag" : label.Text, label.Font,
                new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            label.Height = Math.Max(label.Font.Height + 2, measured.Height + 4);
        }
    }

    /// <summary>Una riga del passo attuale, alta quanto il testo richiede.</summary>
    private static Label StepLine(string text, Font font, Color color, int width, int above, int below)
    {
        var label = new Label
        {
            Text = text, Font = font, ForeColor = color, Width = width, AutoSize = false,
            BackColor = Color.Transparent, UseMnemonic = false, Margin = new Padding(0, above, 0, below)
        };
        var measured = TextRenderer.MeasureText(string.IsNullOrEmpty(text) ? "Ag" : text, font,
            new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        label.Height = Math.Max(font.Height + 2, measured.Height + 4);
        return label;
    }

    private string BuildSessionBrief(ScheduledEvent next)
    {
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
        var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(next.TrackId, StringComparison.OrdinalIgnoreCase));
        var carName = car?.Name ?? career.Car;
        var carSpecs = car == null ? "specifiche da definire" : string.Join(" · ", new[]
        {
            car.PowerHp > 0 ? $"{car.PowerHp} CV" : "potenza non dichiarata",
            car.MassKg > 0 ? $"{car.MassKg} kg" : "peso non dichiarato",
            string.IsNullOrWhiteSpace(car.Category) ? "categoria da definire" : car.Category.ToUpperInvariant()
        });
        var trackName = track?.Name ?? (string.IsNullOrWhiteSpace(next.TrackName) ? next.TrackId : next.TrackName);
        var target = next.IsTest
            ? $"TEMPO DA OTTENERE  {FormatLap(career.EvaluationTargetMilliseconds)} o meglio"
            : $"OBIETTIVO  {next.Objective}";
        var consequence = next.IsTest
            ? "Se lo raggiungi: secondo test, invito o prime offerte. Se no: recupero e budget sotto pressione."
            : "L'esito modifica reputazione, denaro, sponsor e mercato.";
        // Il piano endurance viene definito al lancio della sessione; prima di
        // allora mostriamo la stima standard, senza inventare costi aggiuntivi.
        var endurance = false;
        var logistics = EconomyEngine.StandardLogisticsCost(career.Tier, endurance);
        var maxRepair = EconomyEngine.MaximumDamageCost(career.Tier);
        var entry = next.Kind == ScheduledEventKind.Invitation
            ? InvitationEntryFee(next)
            : next.EntryFee > 0 ? next.EntryFee : next.IsTest ? CareerFinances.TestFee(career.Tier) : 0;
        var upfront = entry > 0 ? entry : logistics;
        var reserve = upfront + maxRepair;
        var forecastBest = next.IsTest ? 0 : EconomyEngine.ForRace(new RaceEconomyInput
        {
            Tier = career.Tier, Position = 1, FieldSize = 10, Dnf = false,
            ContractSalary = career.ContractSalary, RoundsInSeason = Math.Max(1, rounds.Count),
            SponsorBonus = career.SponsorBonus, SponsorQualified = true
        }).Income;
        var forecastWorst = -reserve;
        var budgetLine = next.IsTest
            ? $"BUDGET SESSIONE  € {career.Cash:N0} disponibili · costo test € {upfront:N0} · riserva riparazioni fino a € {maxRepair:N0} · saldo minimo stimato € {career.Cash + forecastWorst:N0}"
            : $"BUDGET GARA  € {career.Cash:N0} disponibili · iscrizione/trasferta € {upfront:N0} · riparazioni possibili fino a € {maxRepair:N0} · saldo peggiore € {career.Cash + forecastWorst:N0} · premio massimo stimato € {forecastBest:N0} (saldo migliore € {career.Cash + forecastBest - upfront:N0})";
        return $"{next.Date:dddd d MMMM} · {CareerScheduler.Describe(next)}\n" +
               $"AUTO  {carName} · {carSpecs}\n" +
               $"PISTA  {trackName}\n" +
               $"{target}\n\n{budgetLine}\n\n{consequence}";
    }

    private void RefreshDriverCard()
    {
        profileName.Text = $"{career.Driver}  #{career.RaceNumber}";
        profileSeat.Text = string.Join("\n", new[]
        {
            $"{career.Nationality} · {career.Team}",
            $"{(string.IsNullOrWhiteSpace(career.Car) ? "nessuna vettura assegnata" : career.Car)}",
            $"Livrea: {(string.IsNullOrWhiteSpace(career.Livery) ? "non dichiarata" : career.Livery)} · compagno: {career.Teammate}",
            $"Sponsor: {career.Sponsor}"
        });

        if (portraitImage == null) return;
        var avatar = !string.IsNullOrWhiteSpace(career.AvatarPath) && File.Exists(career.AvatarPath)
            ? career.AvatarPath
            : AssetPaths.File("fictional-driver-portrait.png");
        if (!File.Exists(avatar)) return;
        // Il ritratto veniva riletto e ridecodificato dal disco a OGNI refresh
        // della schermata, cioè dopo ogni clic — e le tavole del progetto sono
        // PNG da oltre un megapixel. Il file cambia solo quando si modifica il
        // profilo: se il percorso è lo stesso non c'è niente da rifare.
        if (string.Equals(loadedAvatarPath, avatar, StringComparison.OrdinalIgnoreCase) && portraitImage.Image != null) return;
        try
        {
            using var source = Image.FromFile(avatar);
            var replacement = new Bitmap(source);
            // L'immagine precedente va rilasciata: senza Dispose ogni refresh
            // perdeva un handle GDI, e RefreshUi viene chiamato molto spesso.
            var previous = portraitImage.Image;
            portraitImage.Image = replacement;
            previous?.Dispose();
            loadedAvatarPath = avatar;
        }
        catch (Exception error) { CareerLog.Warn("ui", $"avatar non aggiornato: {error.Message}"); }
    }

    /// <summary>Percorso del ritratto già caricato: evita di rileggerlo a ogni refresh.</summary>
    private string loadedAvatarPath = "";

    private void RefreshWeekendControls()
    {
        var raceableCars = ContentAvailability.RaceableCars(contentIndex.Cars);
        var missingContent = ContentAvailability.HomeStatus(raceableCars, contentIndex.Tracks.Count);
        var evaluation = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase);
        // Durante la valutazione non esiste una stagione da concludere.
        var seasonOver = !evaluation && rounds.Count > 0 && career.Round >= rounds.Count;
        var scheduled = NextScheduled();

        pendingBanner.Visible = awaitingResult;
        pendingBanner.Text = awaitingResult
            ? "● SESSIONE APERTA IN CONTENT MANAGER\nIl referto viene acquisito solo a gara conclusa."
            : "";

        decisionSummary.Text = BuildDecisionSummary(scheduled, evaluation);

        var hasOffers = career.Offers?.Count > 0;
        continueStory.Enabled = true;
        var selection = OpenSelection;
        continueStory.Text = !string.IsNullOrWhiteSpace(missingContent)
            ? "CONTINUA · SISTEMA I CONTENUTI"
            : awaitingResult
                ? "CONTINUA · CERCA IL REFERTO"
                // Una selezione aperta è la cosa più importante in agenda: il
                // pulsante deve dire a che giornata si è arrivati.
                : selection?.NextDay is { } pendingDay
                    ? $"CONTINUA · SELEZIONE, GIORNO {pendingDay.Day} DI {selection.Days.Count}"
                    : selection != null
                        ? "CONTINUA · VERDETTO DELLA SELEZIONE"
                : seasonOver
                    ? "CONTINUA · CHIUDI LA STAGIONE"
                    : evaluation && scheduled?.Kind == ScheduledEventKind.Invitation
                        ? "CONTINUA · VALUTA L'INVITO"
                        : evaluation && scheduled != null
                            ? "CONTINUA · AFFRONTA IL TEST"
                            : evaluation && hasOffers
                                ? "CONTINUA · SCEGLI IL PRIMO TEAM"
                                : evaluation
                                    ? "CONTINUA · CERCA UN'OCCASIONE"
                                    // Il pulsante deve dire che cosa fa. «Prossimo
                                    // weekend» descriveva il calendario, non l'azione:
                                    // premendolo si va in pista, e quando c'è una gara
                                    // in programma è quella la parola giusta.
                                    : scheduled?.Kind == ScheduledEventKind.ChampionshipRound
                                        ? $"PARTECIPA ALLA GARA {scheduled.Round}"
                                        : scheduled?.Kind == ScheduledEventKind.Invitation
                                            ? "PARTECIPA ALLA GARA SU INVITO"
                                            : scheduled?.IsTest == true
                                                ? "AFFRONTA LA PROVA"
                                                : "CONTINUA · CERCA UN IMPEGNO";
        // Il comando della card «ADESSO» mostra la stessa azione: va allineato
        // qui, dopo che l'etichetta è stata decisa.
        homeContinue.Text = continueStory.Text;
        homeContinue.Enabled = continueStory.Enabled;

        if (!string.IsNullOrWhiteSpace(missingContent))
        {
            launch.Enabled = false; launch.Text = "CONTENUTI MANCANTI";
            nextSeason.Enabled = false; nextSeason.Text = "Avanza di campionato";
        }
        else if (seasonOver)
        {
            launch.Enabled = false; launch.Text = "STAGIONE CONCLUSA";
            nextSeason.Enabled = true; nextSeason.Text = "Avanza di campionato";
        }
        else if (evaluation)
        {
            var isInvitation = scheduled?.Kind == ScheduledEventKind.Invitation;
            launch.Visible = awaitingResult || isInvitation;
            launch.Enabled = awaitingResult || (isInvitation && InvitationEntryFee(scheduled!) <= career.Cash);
            launch.Text = awaitingResult ? "RIAPRI LA SESSIONE" : "ACCETTA L'INVITO";
            briefing.Enabled = !awaitingResult && scheduled != null;
            briefing.Text = isInvitation ? "LEGGI LA PROPOSTA E IL REGOLAMENTO" : "AVVIA IL TEST DI VALUTAZIONE";
            activities.Text = isInvitation ? "PREPARAZIONE, SPONSOR E AGENDA" : $"AGENDA E PREPARAZIONE · {Math.Max(0, career.DaysUntilNextRound)} giorni";
            nextSeason.Visible = awaitingResult || isInvitation;
            nextSeason.Enabled = awaitingResult || isInvitation;
            nextSeason.Text = awaitingResult ? "Annulla la prova" : "Rifiuta l'invito";
        }
        else
        {
            launch.Visible = true;
            launch.Enabled = career.ContractActive && rounds.Count > 0;
            launch.Text = awaitingResult ? "RIAPRI LA SESSIONE" : rounds.Count == 0 ? "CALENDARIO IN ATTESA" : "APRI IN CONTENT MANAGER";
            nextSeason.Enabled = awaitingResult;
            nextSeason.Text = awaitingResult ? "Annulla il weekend" : "Avanza stagione";
        }
        if (!evaluation) briefing.Enabled = !seasonOver && string.IsNullOrWhiteSpace(missingContent) && scheduled != null;
        activities.Enabled = !awaitingResult;
        if (!evaluation) activities.Text = $"Agenda · {Math.Max(0, career.DaysUntilNextRound)} giorni";
        var openOffers = (career.Opportunities ?? []).Count(x => x.IsOpen);
        var openPromises = (career.Promises ?? []).Count(x => x.IsOpen);
        opportunities.Text = openOffers == 0 ? "Sponsor" : $"Sponsor · {openOffers}";
        simulate.Enabled = awaitingResult;
        simulate.Text = awaitingResult ? "DEBUG · SIMULA SENZA AC" : "DEBUG · simula risultato";
        simulateGood.Enabled = awaitingResult;
        simulateBad.Enabled = awaitingResult;
        simulate.ForeColor = awaitingResult ? UiTheme.Warning : UiTheme.TextMuted;
        opportunities.Enabled = !awaitingResult;
        opportunities.ForeColor = openOffers > 0 ? UiTheme.Warning : UiTheme.TextPrimary;
    }

    /// <summary>
    /// Un solo gesto porta al prossimo battito della storia. Non produce mai un
    /// risultato: prepara la decisione o cerca il referto. Il debug simulato
    /// resta un comando separato e riconoscibile sotto i controlli tecnici.
    /// </summary>
    private void ContinueStory()
    {
        var raceableCars = ContentAvailability.RaceableCars(contentIndex.Cars);
        if (!string.IsNullOrWhiteSpace(ContentAvailability.HomeStatus(raceableCars, contentIndex.Tracks.Count)))
        {
            RefreshContents();
            return;
        }

        if (awaitingResult)
        {
            CheckResultNow();
            return;
        }

        // Una selezione aperta viene prima di tutto: occupa giorni consecutivi e
        // decide la categoria, quindi lasciarla a metà per correre un round
        // significherebbe perdere il posto.
        if (OpenSelection != null)
        {
            OpenSelectionScreen();
            return;
        }

        var evaluation = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase);
        var scheduled = NextScheduled();
        if (!evaluation && rounds.Count > 0 && career.Round >= rounds.Count)
        {
            AdvanceSeason();
            return;
        }

        if (evaluation)
        {
            if (scheduled?.Kind == ScheduledEventKind.Invitation) LaunchWeekend();
            else if (scheduled != null)
            {
                // Il giocatore vuole affrontare subito il test: la scena del
                // Capitolo I racconta l'esito (ChapterOneBeat.Offer/Retry/…)
                // dopo il risultato, non un discorso motivazionale prima di
                // sapere nemmeno se Assetto Corsa gira su questo PC.
                if (!career.ChapterOnePreludeSeen)
                {
                    career.ChapterOnePreludeSeen = true;
                    career.Events.Add(new CareerEventRecord
                    {
                        DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CHAPTER_ONE_PRELUDE",
                        Headline = $"{career.Driver} incontra il programma rookie, {StoryCastService.Get(career, StoryCastService.Mechanic).Name} e {StoryCastService.Get(career, StoryCastService.Rival).Name}.",
                        Track = "Paddock", Importance = 72
                    });
                    SaveCareer(createVersionedBackup: false);
                }
                Briefing();
            }
            else if ((career.Offers?.Count ?? 0) > 0) OpenMarket();
            else OpenSponsorSearch();
            return;
        }

        // Un test in agenda si corre anche sotto contratto.
        //
        // I test venivano gestiti solo dentro la fase di valutazione: fuori da
        // quella il comando finiva nel ramo del weekend di campionato, che con
        // il calendario vuoto usciva in silenzio. Un pilota con quindici prove
        // in agenda premeva CONTINUA e non succedeva niente, all'infinito —
        // e le prove comprate come opportunità finiscono proprio lì.
        if (scheduled != null && scheduled.IsTest) { Briefing(); return; }

        LaunchWeekend();
    }

    private string BuildDecisionSummary(ScheduledEvent? scheduled, bool evaluation)
    {
        var latest = career.TestHistory.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        var report = latest == null
            ? "Nessun test ancora archiviato: il paddock attende il primo riferimento reale."
            : $"Ultimo test · {latest.StoryDate:dd MMMM yyyy} · {latest.Track}\nTempo registrato: {(latest.BestLapMilliseconds > 0 ? FormatLap(latest.BestLapMilliseconds) : "nessun giro valido")}.";
        if (!evaluation) return report;
        if (scheduled == null)
        {
            if (career.Offers.Count > 0)
            {
                var offer = career.Offers.OrderByDescending(x => x.Prestige).First();
                return $"VALUTAZIONE COMPLETATA\n{report}\n\nPROPOSTA DI INGAGGIO\n{offer.Team} offre € {offer.Salary:N0}/anno con {UiText.Car(offer.Car)}. Apri «Mercato e scouting» per leggere, confrontare e accettare o rifiutare le offerte.";
            }
            return $"VALUTAZIONE CONCLUSA\n{report}\n\nIl paddock sta elaborando il prossimo passo della carriera.";
        }
        if (scheduled.Kind != ScheduledEventKind.Invitation)
            return $"REFERTO E VERDETTO\n{report}\n\nPROSSIMO PASSO\n{CareerScheduler.Describe(scheduled)} · obiettivo {FormatLap(career.EvaluationTargetMilliseconds)}.\n{scheduled.Objective}.";
        var fee = InvitationEntryFee(scheduled);
        var affordable = fee <= career.Cash ? "Budget sufficiente." : "Budget insufficiente: cerca sponsor o rinuncia.";
        var proposer = string.IsNullOrWhiteSpace(scheduled.ProposedBy) ? "Paddock Club Kanto" : scheduled.ProposedBy;
        return $"PROPOSTA DI GARA\n{proposer} invita {career.Driver} a {scheduled.TrackName} il {scheduled.Date:dd MMMM yyyy}.\nCosto d'iscrizione: € {fee:N0} · budget: € {career.Cash:N0}. {affordable}\n\nAccetti la gara, oppure prepari altri test e cerchi sostegno economico?";
    }

    private void RefreshEditorial()
    {
        var latestEvent = career.Events.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        var article = NarrativeEngine.Compose(career, latestEvent, rounds);
        PhraseBank.Remember(career, article.UsedPhrases);
        RenderArticle(article);
        RefreshDiary();
    }

    /// <summary>
    /// Impagina l'articolo composto dal motore narrativo. Non aggiunge nulla al
    /// testo: decide soltanto come si legge.
    /// </summary>
    private void RenderArticle(NewsArticle article)
    {
        heroBody.SuspendLayout();
        foreach (Control existing in heroBody.Controls) existing.Dispose();
        heroBody.Controls.Clear();

        var width = Math.Max(280, heroBody.ClientSize.Width - 24);

        if (!string.IsNullOrWhiteSpace(article.Kicker))
            heroBody.Controls.Add(Righe(article.Kicker.ToUpperInvariant(), UiTheme.Kicker, UiTheme.Accent, width, 0, 4));

        // Un titolo su tre righe lascia fuori la prosa: il corpo del titolo
        // segue l'importanza della notizia, come su una pagina vera.
        var fontTitolo = article.CoverageLevel >= 4 ? UiTheme.Headline : UiTheme.HeadlineSmall;
        heroTitle = Righe(article.Title, fontTitolo, UiTheme.TextPrimary, width, 0, 8);
        heroBody.Controls.Add(heroTitle);

        var firma = string.Join("  \u00b7  ", new[] { article.Byline, article.DateLine }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!string.IsNullOrWhiteSpace(firma))
        {
            heroByline = Righe(firma, UiTheme.Byline, UiTheme.TextMuted, width, 0, 10);
            heroBody.Controls.Add(heroByline);
        }

        if (!string.IsNullOrWhiteSpace(article.Standfirst))
        {
            heroStandfirst = Righe(article.Standfirst, UiTheme.Standfirst, UiTheme.TextSecondary, width, 0, 14);
            heroBody.Controls.Add(heroStandfirst);
        }

        heroBody.Controls.Add(Filetto(width));

        // I paragrafi sono da due a sei, secondo l'importanza che il motore ha
        // attribuito alla notizia. La foto entra dopo il primo, cosi la prima
        // cosa che si incontra sotto il titolo e il testo.
        var paragrafi = article.Paragraphs.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        for (var i = 0; i < paragrafi.Count; i++)
        {
            heroBody.Controls.Add(Righe(paragrafi[i], UiTheme.Prose, UiTheme.TextPrimary, width, i == 0 ? 0 : 12, 0));
            if (i == 0) InsertArticlePhoto(width);
        }
        if (paragrafi.Count == 0) InsertArticlePhoto(width);

        if (!string.IsNullOrWhiteSpace(article.Verdict))
        {
            heroBody.Controls.Add(Filetto(width));
            heroBody.Controls.Add(Righe("IL VERDETTO" + (article.Rating > 0 ? $"  \u00b7  {article.Rating}/10" : ""),
                UiTheme.Kicker, UiTheme.Warning, width, 6, 4));
            heroBody.Controls.Add(Righe(article.Verdict, UiTheme.Prose, UiTheme.TextSecondary, width, 0, 8));
        }

        heroBody.ResumeLayout();
    }

    /// <summary>
    /// Un blocco di testo che si adatta in altezza al proprio contenuto: senza
    /// questo un paragrafo lungo veniva tagliato a meta.
    /// </summary>
    private static Label Righe(string text, Font font, Color color, int width, int spazioSopra, int spazioSotto)
    {
        var label = new Label
        {
            Text = text, Font = font, ForeColor = color, AutoSize = false,
            Width = width, BackColor = Color.Transparent, UseMnemonic = false,
            Margin = new Padding(0, spazioSopra, 0, spazioSotto)
        };
        // Misura senza creare la finestra dell'etichetta.
        //
        // CreateGraphics costringe WinForms a creare l'handle nativo di un
        // controllo che non e' ancora — e forse non sara' mai — sullo schermo.
        // Ogni ridisegno ne lasciava uno dietro: percorrendo la carriera dal
        // banco di prova il processo moriva con «errore durante la creazione
        // dell'handle della finestra» dopo qualche migliaio di aggiornamenti,
        // cioe' esauriti gli handle di sessione. TextRenderer misura con lo
        // stesso motore che poi disegna, e non crea niente.
        var misura = TextRenderer.MeasureText(string.IsNullOrEmpty(text) ? "Ag" : text, font,
            new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        label.Height = misura.Height + 2;
        return label;
    }

    /// <summary>
    /// La fotografia dell'articolo, dentro il flusso del pezzo. La didascalia
    /// dichiara sempre la provenienza: se l'immagine non documenta l'evento, il
    /// testo lo dice invece di far passare un'illustrazione per una fotografia.
    /// </summary>
    private void InsertArticlePhoto(int width)
    {
        var story = career.Events.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        var photo = ArticlePhotoService.ForEvent(career, story, Path.Combine(saveDir, "media"));
        if (photo.Exists)
        {
            var box = new PictureBox
            {
                Width = width, Height = Math.Max(96, (int)(width * 0.22)),
                SizeMode = PictureBoxSizeMode.Zoom, BackColor = UiTheme.SurfaceRaised,
                Margin = new Padding(0, 10, 0, 4)
            };
            try
            {
                using var source = Image.FromFile(photo.Path);
                box.Image = new Bitmap(source);
            }
            catch (Exception error) { CareerLog.Warn("foto", $"immagine articolo non caricata: {error.Message}"); }
            heroBody.Controls.Add(box);
        }
        if (!string.IsNullOrWhiteSpace(photo.Caption))
            heroBody.Controls.Add(Righe(photo.Caption, UiTheme.Small,
                photo.DocumentsEvent ? UiTheme.TextMuted : UiTheme.Warning, width, 0, 6));
    }

    private static Panel Filetto(int width) => new()
    {
        Width = width, Height = 1, BackColor = UiTheme.Border,
        Margin = new Padding(0, 8, 0, 10)
    };

    /// <summary>
    /// Ricostruisce il diario datato. Ogni voce è cliccabile e apre l'articolo
    /// completo del proprio evento, così la carriera si legge come una storia
    /// continua invece che come un articolo più un menu di scorciatoie.
    /// </summary>
    private void RefreshDiary()
    {
        diary.SuspendLayout();
        foreach (Control existing in diary.Controls) existing.Dispose();
        diary.Controls.Clear();

        var entries = CareerDiary.Build(career);
        var width = Math.Max(240, diary.ClientSize.Width - 26);

        // Il prossimo appuntamento apre il diario: è la parte non ancora scritta.
        var next = NextScheduled();
        if (next != null) diary.Controls.Add(BuildUpcomingEntry(next, width));

        if (entries.Count == 0)
        {
            diary.Controls.Add(new Label
            {
                Text = "Il diario è vuoto: la prima voce nascerà dal primo fatto della carriera.",
                AutoSize = false, Width = width, Height = 40, ForeColor = UiTheme.TextMuted, Font = UiTheme.Body
            });
        }
        foreach (var entry in entries)
        {
            if (entry.StartsNewDay) diary.Controls.Add(BuildDateSeparator(entry, width));
            diary.Controls.Add(BuildDiaryEntry(entry, width));
        }
        diary.ResumeLayout();
        diaryFooter.Text = $"ARCHIVIO · gare {career.RaceHistory.Count} · test {career.TestHistory.Count} · voci {career.Events.Count} · attività {career.ActivityHistory.Count}";
    }

    private Control BuildDateSeparator(DiaryEntry entry, int width)
    {
        var text = string.IsNullOrWhiteSpace(entry.Interval)
            ? entry.DateLabel.ToUpperInvariant()
            : $"{entry.DateLabel.ToUpperInvariant()}   ·   {entry.Interval}";
        var label = new Label
        {
            Text = text, AutoSize = false, Width = width, Height = 26,
            ForeColor = UiTheme.Accent, Font = UiTheme.SectionLabel,
            TextAlign = ContentAlignment.BottomLeft, Margin = new Padding(0, 10, 0, 2)
        };
        label.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawLine(pen, 0, label.Height - 1, label.Width, label.Height - 1);
        };
        return label;
    }

    private Control BuildUpcomingEntry(ScheduledEvent next, int width)
    {
        var card = new Panel { Width = width, Height = 88, BackColor = UiTheme.SurfaceRaised, Margin = new Padding(0, 0, 0, 8), Padding = new Padding(10, 6, 8, 6) };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Warning);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        card.Controls.Add(new Label { Text = next.GeneratedBy, Dock = DockStyle.Bottom, Height = 20, ForeColor = UiTheme.TextSecondary, Font = UiTheme.Small, AutoEllipsis = true });
        card.Controls.Add(new Label { Text = $"{CareerDiary.FormatDate(next.Date).ToUpperInvariant()}   ·   IN PROGRAMMA", Dock = DockStyle.Top, Height = 20, ForeColor = UiTheme.Warning, Font = UiTheme.Small });
        card.Controls.Add(new Label { Text = CareerScheduler.Describe(next), Dock = DockStyle.Top, Height = 24, ForeColor = UiTheme.TextPrimary, Font = UiTheme.BodyStrong, AutoEllipsis = true });
        return card;
    }

    private Control BuildDiaryEntry(DiaryEntry entry, int width)
    {
        var hasSummary = !string.IsNullOrWhiteSpace(entry.Summary);
        var card = new Panel
        {
            Width = width, Height = hasSummary ? 104 : 84, BackColor = UiTheme.Surface,
            Margin = new Padding(0, 0, 0, 6), Padding = new Padding(10, 4, 8, 4), Cursor = Cursors.Hand
        };
        var accent = entry.Importance >= 85 ? UiTheme.Accent : entry.Importance >= 60 ? UiTheme.Info : UiTheme.Border;
        card.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(accent);
            e.Graphics.FillRectangle(brush, 0, 0, 2, card.Height);
        };

        var kicker = new Label { Text = entry.Kicker, Dock = DockStyle.Top, Height = 20, ForeColor = accent == UiTheme.Border ? UiTheme.TextMuted : accent, Font = UiTheme.Small };
        var title = new Label { Text = entry.Title, Dock = DockStyle.Top, Height = hasSummary ? 54 : 56, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body };
        card.Controls.Add(title);
        card.Controls.Add(kicker);
        if (hasSummary)
            card.Controls.Add(new Label { Text = entry.Summary, Dock = DockStyle.Bottom, Height = 18, ForeColor = UiTheme.TextMuted, Font = UiTheme.Small, AutoEllipsis = true });

        void Open() => OpenDiaryArticle(entry);
        card.Click += (_, _) => Open();
        foreach (Control child in card.Controls) { child.Cursor = Cursors.Hand; child.Click += (_, _) => Open(); }
        return card;
    }

    /// <summary>Apre l'articolo completo della voce di diario selezionata.</summary>
    private void OpenDiaryArticle(DiaryEntry entry)
    {
        var events = career.Events ?? [];
        if (entry.EventIndex < 0 || entry.EventIndex >= events.Count) return;
        NarrationService.OpenBrowserPortal(CareerArticleBuilder.Build(career, events[entry.EventIndex], rounds));
        SaveCareer(createVersionedBackup: false);
    }


    private void RefreshDossier()
    {
        nextSession.Text = UpcomingSessionSummary();

        standings.Text = career.Standings.Count == 0
            ? "La classifica nascerà dal primo referto reale importato."
            : string.Join("\n", career.Standings.OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins)
                .Take(10).Select((x, i) => $"{i + 1,2}. {Truncate(x.Driver, 22),-22} {x.Points,4} pt"));

        var latestRace = career.RaceHistory.LastOrDefault();
        var latestTest = career.TestHistory.LastOrDefault();
        var lines = new List<string>();
        if (career.Results.Count == 0) lines.Add("Nessuna gara disputata: il percorso comincia dai test.");
        else lines.AddRange(career.Results.TakeLast(5));
        if (latestRace != null)
        {
            lines.Add("");
            lines.Add("DATI TECNICI");
            lines.Add($"Partenza P{latestRace.StartingPosition} · qualifica {(latestRace.QualificationPosition > 0 ? $"P{latestRace.QualificationPosition}" : "n/d")}");
            lines.Add($"Giri {latestRace.Laps}{(latestRace.PlannedLaps > 0 ? $"/{latestRace.PlannedLaps}" : "")} · miglior giro {(latestRace.BestLapMilliseconds > 0 ? FormatLap(latestRace.BestLapMilliseconds) : "n/d")}");
            lines.Add($"Pit stop {(latestRace.PitStops > 0 ? latestRace.PitStops.ToString() : "n/d")} · danno {latestRace.Damage:0.##} · penalità {latestRace.PenaltySeconds:0.#} s");
            if (!string.IsNullOrWhiteSpace(latestRace.WeatherLabel))
                lines.Add($"Condizioni: {latestRace.FormatLabel} · {latestRace.WeatherLabel} · {latestRace.TemperatureC:0.#} °C · ore {TimeSpan.FromSeconds(latestRace.TimeOfDaySeconds):hh\\:mm} · AI {latestRace.AiLevel:0}%");
            lines.Add(latestRace.TeammatePosition <= 0
                ? "Compagno: n/d"
                : $"Compagno {latestRace.TeammateName}: P{latestRace.TeammatePosition} ({(latestRace.Position < latestRace.TeammatePosition ? "davanti" : latestRace.Position > latestRace.TeammatePosition ? "dietro" : "pari")})");
        }
        lines.Add("");
        lines.Add("TEST IN PISTA");
        lines.Add(latestTest == null
            ? "Nessun test reale archiviato."
            : $"{latestTest.Track} · {UiText.Car(latestTest.Car)} · giri {latestTest.Laps} · miglior giro {(latestTest.BestLapMilliseconds > 0 ? FormatLap(latestTest.BestLapMilliseconds) : "n/d")} · archiviati {career.TestHistory.Count}");
        results.Text = string.Join("\n", lines);

        var topRival = career.Rivalries.OrderByDescending(x => x.Level).FirstOrDefault();
        var openOffers = (career.Opportunities ?? []).Count(x => x.IsOpen);
        var openPromises = (career.Promises ?? []).Count(x => x.IsOpen);
        var activeArc = career.StoryArcs.OrderByDescending(x => x.Importance).FirstOrDefault(x => x.Status == "In corso");
        var aiDrivers = career.Drivers.Count(x => !x.Name.Equals(career.Driver, StringComparison.OrdinalIgnoreCase));
        var rules = ChampionshipRulesForCareer();
        dossier.Text = string.Join("\n", new[]
        {
            "ARCO NARRATIVO",
            activeArc == null ? "In attesa del primo verdetto reale." : $"{activeArc.Title}: {activeArc.Summary}",
            "",
            "REGOLAMENTO",
            ChampionshipRegulations.Describe(rules),
            "",
            "CONTRATTO E SPONSOR",
            career.ContractActive ? $"{career.Team} · {career.ContractYears} anno/i · € {career.ContractSalary:N0} · prestigio sedile {career.SeatPrestige}/100" : "Nessun contratto attivo",
            $"Obiettivo: {career.ContractObjective} ({career.ContractObjectiveStatus})",
            $"Sponsor: {career.Sponsor} · {career.SponsorObjectiveStatus} · rapporto {OffTrackActivities.RelationLabel(career.SponsorRelation)}",
            "",
            "RELAZIONI",
            topRival == null ? "Nessuna rivalità registrata." : $"Rivale principale: {topRival.Rival} · intensità {topRival.Level}/100",
            $"Rapporto col team: {OffTrackActivities.RelationLabel(career.TeamRelation)} ({career.TeamRelation}/100)",
            $"Confronto col compagno in stagione: {career.TeammateRacesWon}-{career.TeammateRacesLost}",
            $"Roster persistente: {aiDrivers} piloti AI",
            "",
            // Una carriera che contiene risultati simulati lo dichiara qui,
            // in cima al dossier: non deve poter essere confusa con una
            // carriera giocata davvero.
            career.SimulatedSessions > 0
                ? $"CARRIERA DI PROVA — {career.SimulatedSessions} sessione/i risolte dal programma, non disputate\n"
                : "",
            "REPUTAZIONE",
            career.ReputationProfile?.Describe() ?? "non disponibile",
            "",
            "OPPORTUNITÀ E PROMESSE",
            openOffers == 0 ? "Nessuna proposta aperta." : $"{openOffers} proposta/e aperta/e — apri «Opportunità» per decidere.",
            openPromises == 0 ? "Nessuna promessa in attesa." : string.Join("\n", (career.Promises ?? []).Where(x => x.IsOpen).Take(3)
                .Select(x => $"• {x.PromisedBy}: {x.Description} — serve {x.ConditionText} entro il {NarrativeCalendar.Format(x.Deadline)}")),
            "",
            "ECONOMIA",
            $"Budget € {career.Cash:N0} · {CareerFinances.Status(career.Cash)}",
            $"Quote d'iscrizione pagate € {career.EntryFeesPaid:N0}{(career.SeasonCoveragePercent > 0 ? $" · copertura stagione {career.SeasonCoveragePercent}%" : "")}",
            $"Premi € {career.PrizeMoney:N0} · sponsor € {career.SponsorMoney:N0} · stipendio € {career.SalaryPaid:N0}",
            $"Riparazioni € -{career.RepairCosts:N0} · trasferte € -{career.LogisticsCosts:N0}",
            "",
            "CONDIZIONE",
            $"{OffTrackActivities.FatigueLabel(career.Fatigue)} ({career.Fatigue}/100) · seguito {career.Fanbase} · {Math.Max(0, career.DaysUntilNextRound)} giorni liberi prima del prossimo round",
            "",
            "SESSIONI NON CONCLUSE",
            // Il conto resta pubblico: l'app non può distinguere un problema
            // tecnico da una scappatoia, quindi lo dichiara invece di nasconderlo.
            $"Abbandoni a referto: {career.AbandonedSessions} · annullamenti tecnici dichiarati: {career.TechnicalAnnulments}",
            "",
            "ROOKIE EVALUATION",
            // Lo stato a parole, non un voto: i valori che contano sono forma,
            // budget e fiducia, e stanno nella fascia in alto.
            career.RookieEvaluationStatus,
            string.IsNullOrWhiteSpace(career.EvaluationTargetBasis) ? "" : $"Riferimento: {career.EvaluationTargetBasis}"
        });

        // La disponibilità dei contenuti non dipende dal calendario: durante la
        // valutazione i round non esistono, ma i circuiti installati sì.
        var availability = ContentAvailability.HomeStatus(contentIndex.Cars, contentIndex.Tracks.Count);
        var metadataNotes = contentIndex.ScanWarnings.Count(x => x.Contains("JSON non standard", StringComparison.OrdinalIgnoreCase));
        var blockingWarnings = contentIndex.ScanWarnings.Count - metadataNotes;
        var status = new List<string>
        {
            string.IsNullOrWhiteSpace(availability)
                ? $"CONTENUTI PRONTI · {contentIndex.Cars.Count} auto · {contentIndex.Tracks.Count} circuiti · {contentIndex.Weathers.Count} preset meteo"
                : availability
        };
        if (blockingWarnings > 0) status.Add($"{blockingWarnings} percorso/i da controllare in Revisione contenuti");
        if (metadataNotes > 0) status.Add($"{metadataNotes} JSON multilinea gestiti con fallback");
        contentStatus.Text = string.Join("\n", status);
        contentStatus.ForeColor = string.IsNullOrWhiteSpace(availability) ? UiTheme.TextSecondary : UiTheme.Accent;
    }

    /// <summary>
    /// Anteprima del formato del prossimo weekend, calcolata dagli stessi dati che
    /// finiranno nel preset: la home non promette condizioni diverse da quelle che
    /// Content Manager riceverà davvero.
    /// </summary>
    private string UpcomingSessionSummary()
    {
        var scheduled = NextScheduled();
        if (scheduled == null || contentIndex.Tracks.Count == 0) return "Nessun appuntamento in agenda: il prossimo nascerà dal risultato successivo.";
        var raceable = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).Select(x => x.Id).ToArray();
        if (raceable.Length == 0) return "Serve almeno un'auto da competizione installata.";
        var car = raceable.Contains(career.Car, StringComparer.OrdinalIgnoreCase) ? career.Car : raceable[0];
        var track = contentIndex.Tracks.Any(x => x.Id.Equals(scheduled.TrackId, StringComparison.OrdinalIgnoreCase)) ? scheduled.TrackId : contentIndex.Tracks[0].Id;
        var evaluation = scheduled.IsTest;
        try
        {
            var plan = BuildSessionPlan(track, car, mixedGrid: false, testSession: evaluation);
            var calibration = career.AutoCalibrateAi ? AiCalibration.Describe(career.AiCalibrationOffset) : "Calibrazione automatica disattivata.";
            var notes = plan.Notes.Count == 0 ? "" : "\n" + string.Join("\n", plan.Notes.Select(x => "• " + x));
            // L'appuntamento dichiara anche perché esiste: è la catena causale.
            return $"{CareerScheduler.Describe(scheduled)}\n{scheduled.GeneratedBy}.\nObiettivo: {scheduled.Objective}\n\n{plan.Describe()}\nProfilo: difficoltà {career.DifficultyProfile} · distanza {career.DistanceProfile}\n{calibration}{notes}";
        }
        catch (Exception error)
        {
            CareerLog.Warn("ui", $"anteprima sessione non calcolabile: {error.Message}");
            return "Formato non calcolabile con i contenuti attuali.";
        }
    }

    private static string Truncate(string value, int length) =>
        string.IsNullOrEmpty(value) ? "" : value.Length <= length ? value : value[..(length - 1)] + "…";
}
