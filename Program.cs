using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Cryptography;

namespace CorsaCareer;

public sealed class CareerState
{
    public int SchemaVersion { get; set; } = 12;
    // Una carriera nuova non deve avere un'identità fittizia. Il profilo viene
    // richiesto prima di mostrare Home/prologo; questi valori vuoti permettono
    // al launcher di riconoscere anche i salvataggi creati da versioni vecchie.
    public string Driver { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Nationality { get; set; } = "Giappone";
    /// <summary>
    /// La citta da cui parte la carriera. Le distanze verso gli sponsor si
    /// misurano da qui: e' il motivo per cui alcuni contatti costano un viaggio
    /// e altri sono dietro l'angolo.
    /// </summary>
    public string HomeCity { get; set; } = "Suzuka";
    public int RaceNumber { get; set; } = 27;
    public string Nickname { get; set; } = "";
    public string AvatarPath { get; set; } = "";
    public string CareerMode { get; set; } = "Realistica";
    public string CareerPhase { get; set; } = "Evaluation";
    public int EvaluationAttempts { get; set; }
    public int EvaluationBestLapMilliseconds { get; set; }
    public int EvaluationTargetMilliseconds { get; set; } = RookieTargetEngine.FallbackTargetMilliseconds;
    /// <summary>Come è stato ottenuto l'obiettivo: stima dai metadati o tempo reale.</summary>
    public string EvaluationTargetBasis { get; set; } = "";
    public string EvaluationTargetTrack { get; set; } = "";
    public bool LaunchStoriesOpened { get; set; }
    public int Season { get; set; } = 1;
    public string Tier { get; set; } = "Rookie";
    public string Championship { get; set; } = "Campionato locale";

    /// <summary>
    /// Altezza del campionato che si sta correndo, da 1 (zona) a
    /// <see cref="ChampionshipLadder.Levels"/> (mondiale). Sale di un gradino
    /// quando la stagione si chiude nei primi tre; non scende mai.
    /// </summary>
    public int ChampionshipLevel { get; set; } = 1;

    /// <summary>
    /// Le doti innate del pilota: non si allenano, si scoprono correndo.
    /// Restano fisse per tutta la carriera e rendono ogni pilota diverso a
    /// parità di scelte.
    /// </summary>
    public DriverTalent Talent { get; set; } = new();

    /// <summary>
    /// Numero di gare corse quando si è saliti di campionato l'ultima volta.
    /// Serve a distanziare le promozioni per conoscenza.
    /// </summary>
    public int RacesAtLastLevelUp { get; set; }
    public DateTime StoryDate { get; set; } = NarrativeCalendar.DefaultSeasonStart;

    /// <summary>
    /// L'anno di nascita del pilota. Zero nelle carriere salvate prima che
    /// l'età esistesse: in quel caso viene ricavato dalla data d'inizio.
    /// </summary>
    public int BirthYear { get; set; }

    /// <summary>
    /// Il giorno in cui la carriera è cominciata. Serve a sapere da quanto si
    /// corre, che è una cosa diversa da quante gare si sono fatte.
    /// </summary>
    public DateTime CareerStart { get; set; }

    /// <summary>
    /// I totali economici di tutta la carriera.
    ///
    /// Le voci PrizeMoney, SponsorMoney e simili si azzerano a ogni cambio di
    /// stagione, perche' servono al bilancio dell'annata. Il risultato era che
    /// il consuntivo di una carriera di trent'anni mostrava gli incassi degli
    /// ultimi dodici mesi accanto a una cassa da milioni, e non tornava niente.
    /// Questi invece non si azzerano mai: sono quello che il pilota ha
    /// incassato e speso da quando ha cominciato.
    /// </summary>
    public long LifetimePrizeMoney { get; set; }
    public long LifetimeSponsorMoney { get; set; }
    public long LifetimeSalary { get; set; }
    public long LifetimeRepairCosts { get; set; }
    public long LifetimeLogisticsCosts { get; set; }

    /// <summary>
    /// Le prime volte: il primo test, la prima gara, la prima vittoria, il
    /// primo contratto.
    ///
    /// La classe esisteva gia' e non la usava nessuno: era stata scritta e mai
    /// collegata, quindi la prima vittoria di una carriera passava esattamente
    /// come la quindicesima. Adesso il registro vive nel salvataggio ed e' cio'
    /// che permette di raccontare un momento una volta sola.
    /// </summary>
    public CareerFirsts Firsts { get; set; } = new();

    /// <summary>Vero se il pilota ha appeso il casco al chiodo.</summary>
    public bool Retired { get; set; }

    /// <summary>Il giorno del ritiro, e il motivo con cui è stato raccontato.</summary>
    public DateTime RetiredOn { get; set; }
    public string RetirementReason { get; set; } = "";
    // Ancora fissa del calendario di campionato: cambia solo al passaggio di
    // stagione, così le date dei round non si spostano dopo ogni gara.
    public DateTime SeasonStartDate { get; set; }
    public int TeammateRacesWon { get; set; }
    public int TeammateRacesLost { get; set; }
    public string NarratorStyle { get; set; } = "Radiocronaca italiana classica — narratore originale";
    public string PreferredVoice { get; set; } = NarrationVoiceCatalog.Browser;
    /// <summary>
    /// La voce sintetica che legge lo stato della schermata.
    ///
    /// Spenta per difetto: leggeva ad alta voce cose gia scritte sullo schermo,
    /// con la voce robotica di Windows, e partiva da sola a ogni aggiornamento.
    /// Chi la vuole la accende dalle impostazioni.
    /// </summary>
    public bool NarrationOnStartup { get; set; }
    public string RookieEvaluationStatus { get; set; } = "In attesa della prima gara reale";
    public int RookieEvaluationScore { get; set; }
    // Una carriera nasce senza squadra: il nome di un team fra i predefiniti
    // faceva risultare ingaggiato un pilota che non aveva ancora corso.
    public string Team { get; set; } = "Senza contratto";
    public string Car { get; set; } = "";
    public string Livery { get; set; } = "";
    // Una carriera nasce senza contratto e senza stipendio. I predefiniti
    // precedenti (un anno, 25.000 euro) venivano dalla versione in cui il pilota
    // partiva gia ingaggiato, e facevano leggere una carriera appena creata come
    // se avesse un contratto professionistico.
    public int ContractYears { get; set; }
    public int ContractSalary { get; set; }
    /// <summary>Percentuale dei costi del sedile coperta direttamente dal team.</summary>
    public int TeamSupportPercent { get; set; }
    /// <summary>Somma di sponsor destinata alle prossime quote, separata dalla cassa personale.</summary>
    public int SponsorBudget { get; set; }
    /// <summary>Indica che il pilota sta pagando per correre, non che è assunto.</summary>
    public bool IsClientDriver { get; set; } = true;
    public string ContractObjective { get; set; } = "Completa la stagione e batti almeno un avversario";
    public string ContractObjectiveStatus { get; set; } = "In corso";
    public bool ContractActive { get; set; }
    public string Teammate { get; set; } = "Kenta Ogawa";
    public List<TeamOffer> Offers { get; set; } = [];
    public string Sponsor { get; set; } = "In attesa di sponsor";
    /// <summary>
    /// L'interesse dello sponsor al momento della firma. Serve a riaprire il
    /// mercato solo per un salto reale: senza questo dato le sponsorizzazioni si
    /// potevano incassare una dopo l'altra.
    /// </summary>
    public int SponsorSignedAppeal { get; set; }
    /// <summary>
    /// Quante sessioni sono state simulate invece di essere disputate. Serve a
    /// tenere una carriera di prova riconoscibile: se questo valore e maggiore
    /// di zero il portale lo dichiara, e non si puo confondere con una carriera
    /// giocata davvero.
    /// </summary>
    public int SimulatedSessions { get; set; }
    /// <summary>
    /// L'ultima fase di carriera presentata al giocatore. Serve a non riproporre
    /// la stessa introduzione, e a non riproporre quelle gia superate se la
    /// carriera arretra.
    /// </summary>
    public string AnnouncedPhase { get; set; } = "";
    /// <summary>
    /// La disciplina scelta dal pilota: monoposto, durata, turismo, rally,
    /// autocross. Vuota finche la scelta non e stata fatta. Non la decide il
    /// programma, e non e fissa: dipende da cosa e installato e da cosa vuole
    /// il pilota.
    /// </summary>
    public string ChosenPath { get; set; } = "";
    public int Round { get; set; }
    public int Points { get; set; }
    public int Reputation { get; set; } = 20;
    // Il capitale iniziale deve essere scarso: con 250.000 e nessuna quota
    // d'iscrizione non era possibile andare in difficolta economica, e questo
    // togliva il rischio da ogni decisione.
    public int Cash { get; set; } = CareerFinances.StartingCash;
    public int Races { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int SponsorTarget { get; set; } = 6;
    public int SponsorBonus { get; set; } = 30000;
    public int SponsorQualifyingResults { get; set; }
    public string SponsorObjectiveStatus { get; set; } = "In corso";
    public int SalaryPaid { get; set; }
    public int PrizeMoney { get; set; }
    public int SponsorMoney { get; set; }
    public int RepairCosts { get; set; }
    public int LogisticsCosts { get; set; }
    // Vita fuori dalla pista: queste grandezze non toccano mai un risultato di
    // gara, ma governano mercato, contratti, sponsor ed economia.
    public int Fanbase { get; set; }
    public int Fatigue { get; set; }

    /// <summary>
    /// Che tipo di squadra è quella per cui si corre. Decide quanto vale un
    /// risultato in visibilità, quanto costa un weekend e quanto rendono i
    /// premi: la stessa vittoria non vale uguale dappertutto.
    /// </summary>
    public string TeamProfileId { get; set; } = "";

    /// <summary>
    /// Il numero di gare al momento di un salto di campionato a stagione in
    /// corso. Zero se non c'è nessuna prova in corso: chi sale così non ha una
    /// classifica da difendere ma una squadra che si è esposta, e le prime gare
    /// dicono se la scommessa ha funzionato.
    /// </summary>
    public int SaltoAllaGara { get; set; }

    /// <summary>Il livello da cui si è saltati: è lì che si torna se va male.</summary>
    public int LivelloPrimaDelSalto { get; set; }

    /// <summary>La squadra lasciata saltando: la prima a cui si ripresenta il conto.</summary>
    public string SquadraPrimaDelSalto { get; set; } = "";

    /// <summary>
    /// Le spiegazioni dell'allenatore già date. Una regola spiegata due volte è
    /// una scena che si salta senza leggere.
    /// </summary>
    public List<string> BriefingFatti { get; set; } = [];

    /// <summary>
    /// Forma fisica del pilota, 0-100. Cresce allenandosi e con i test in pista,
    /// e serve a tenere il ritmo nei long run: senza, la palestra sarebbe una
    /// barra decorativa.
    /// </summary>
    public int Fitness { get; set; } = 30;

    /// <summary>
    /// La giornata in corso: ore rimaste al pilota e a Haru, e cosa è già stato
    /// fatto. Prima fra un impegno e l'altro non c'era niente da fare, e per
    /// questo non si poteva nemmeno indicare un'alternativa quando i soldi
    /// mancavano.
    /// </summary>
    public DayPlan? Today { get; set; }

    /// <summary>
    /// Le capacita di Haru Senda. Migliorano lavorando, e cambiano l'esito di
    /// una trattativa quanto i risultati del pilota: senza, mandarlo da
    /// un'azienda o da un'altra sarebbe la stessa cosa.
    /// </summary>
    public AgentSkills? Agent { get; set; }
    public int TeamRelation { get; set; } = 40;
    public int SponsorRelation { get; set; } = 40;
    public int DaysUntilNextRound { get; set; } = -1;
    public List<ActivityRecord> ActivityHistory { get; set; } = [];
    /// <summary>
    /// Annullamenti dichiarati per problema tecnico. Non consumano il round, ma
    /// restano contati e visibili: l'app non può distinguere un crash reale da
    /// una scappatoia, quindi la scelta resta al giocatore e il conto è pubblico.
    /// </summary>
    public int TechnicalAnnulments { get; set; }
    public int AbandonedSessions { get; set; }
    /// <summary>Prestigio del sedile attuale: definisce la posizione attesa dal contratto.</summary>
    public int SeatPrestige { get; set; } = 25;
    public List<string> Results { get; set; } = [];
    public List<string> News { get; set; } = [];
    public List<RaceHistoryEntry> RaceHistory { get; set; } = [];
    public List<TestSessionRecord> TestHistory { get; set; } = [];
    public List<CareerEventRecord> Events { get; set; } = [];
    public List<PersistentDriver> Drivers { get; set; } = [];
    public List<StandingEntry> Standings { get; set; } = [];
    public List<RivalryRecord> Rivalries { get; set; } = [];
    public List<StoryArcRecord> StoryArcs { get; set; } = [];
    /// <summary>Le persone ricorrenti che danno continuita al racconto, non ai risultati.</summary>
    public List<StoryCharacter> StoryCast { get; set; } = [];
    public List<TeamInterest> TeamInterests { get; set; } = [];
    /// <summary>Il briefing del primo capitolo e stato visto almeno una volta.</summary>
    public bool ChapterOnePreludeSeen { get; set; }
    /// <summary>Il tutorial dei tre indicatori della Home è stato completato.</summary>
    public bool BudgetTutorialSeen { get; set; }
    public JournalistProfile Journalist { get; set; } = new();
    public List<TeamHistoryEntry> TeamHistory { get; set; } = [];
    public List<SeasonSummary> SeasonArchive { get; set; } = [];
    public string DifficultyProfile { get; set; } = SessionProfiles.DifficultyRealistic;
    public string DistanceProfile { get; set; } = SessionProfiles.DistanceRealistic;
    public bool AutoCalibrateAi { get; set; } = true;
    public int AiCalibrationOffset { get; set; }
    public List<SessionPlanRecord> SessionPlans { get; set; } = [];
    /// <summary>
    /// Agenda della carriera: esistono solo gli appuntamenti realmente generati da
    /// un fatto precedente. Sostituisce il calendario derivato dalla cartella dei
    /// contenuti, che esisteva anche quando il pilota non aveva un sedile.
    /// </summary>
    public List<ScheduledEvent> Schedule { get; set; } = [];
    /// <summary>
    /// Le selezioni a piu giornate affrontate dal pilota, compresa quella in
    /// corso. Una lista vuota e lo stato corretto di una carriera che non ne ha
    /// ancora incontrata nessuna: i salvataggi precedenti restano validi.
    /// </summary>
    public List<SelectionTrial> Selections { get; set; } = [];
    /// <summary>
    /// Reputazione su piu dimensioni indipendenti. Il vecchio campo Reputation
    /// resta come indice sintetico, allineato dal motore delle conseguenze.
    /// </summary>
    public ReputationProfile ReputationProfile { get; set; } = new();
    /// <summary>Opportunita proposte al pilota, con stato e scadenza.</summary>
    public List<Opportunity> Opportunities { get; set; } = [];
    /// <summary>Contatti sponsor disponibili per la settimana corrente.</summary>
    public List<SponsorProspect> SponsorProspects { get; set; } = [];
    public int SponsorActionsRemaining { get; set; } = 2;
    /// <summary>Promesse condizionali aperte: verificate dopo ogni risultato reale.</summary>
    public List<ConditionalPromise> Promises { get; set; } = [];
    /// <summary>Quote d'iscrizione realmente pagate: il denaro speso per correre.</summary>
    public int EntryFeesPaid { get; set; }
    /// <summary>Copertura della stagione ottenuta da promesse mantenute, in percentuale.</summary>
    public int SeasonCoveragePercent { get; set; }
    /// <summary>
    /// Formulazioni editoriali usate di recente: impediscono alla redazione di
    /// ripetere gli stessi giri di frase articolo dopo articolo.
    /// </summary>
    public List<string> UsedPhrases { get; set; } = [];
    public string MediaSequenceMode { get; set; } = "Rassegna consigliata";
    public string MediaFilter { get; set; } = "Tutti i media";
    public int MediaSequencePosition { get; set; }
    public string Headline { get; set; } = "Un nuovo pilota entra nel paddock: il progetto attende il primo referto reale.";
}

public sealed record Round(string GrandPrix, string Country, string Date, string Track);

public sealed partial class MainForm : Form
{
    /// <summary>
    /// Cartella dei salvataggi. Normalmente sotto Documenti, ma CORSACAREER_HOME
    /// permette di puntarla altrove: serve a rendere il portale su carriere di
    /// prova senza mai toccare i salvataggi reali del giocatore.
    /// </summary>
    private readonly string saveDir = Environment.GetEnvironmentVariable("CORSACAREER_HOME") is { Length: > 0 } home
        ? home
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "CorsaCareer");
    private List<Round> rounds = new();
    private ContentIndexRecord contentIndex = new();
    private CareerState career = new();
    private bool awaitingResult;
    /// <summary>Il dossier del pilota è appena stato aperto nel browser in
    /// questa stessa sessione: al primo Shown va ripreso il fuoco, altrimenti
    /// il portale — attivandosi per la prima volta — glielo ruba subito dopo.</summary>
    private bool storiesJustOpened;
    private string pendingMode = "race";
    private DateTime launchTimeUtc = DateTime.MinValue;
    private string pendingResultHash = "";
    private string lastRejectedResultSignature = "";
    private string lastImportProbe = "";
    private readonly System.Windows.Forms.Timer resultTimer = new() { Interval = 2000 };
    private readonly System.Windows.Forms.Timer autosaveTimer = new() { Interval = 60000 };
    private readonly System.Windows.Forms.Timer narrationTimer = new() { Interval = 500 };
    private PortalFocusServer? focusServer;

    /// <summary>
    /// Riporta in primo piano la finestra su richiesta di una pagina del browser.
    /// La chiamata arriva da un thread di rete, quindi va marshallata sulla UI.
    /// </summary>
    private void FocusFromBrowser()
    {
        try
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() =>
            {
                if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
                Show();
                // TopMost momentaneo: senza questo Windows può ignorare la
                // richiesta di fuoco proveniente da un processo diverso.
                var wasTopMost = TopMost;
                TopMost = true;
                Activate();
                BringToFront();
                TopMost = wasTopMost;
                Focus();
            });
        }
        catch (Exception error) { CareerLog.Warn("portale", $"ritorno al portale non eseguito: {error.Message}"); }
    }
    public MainForm()
    {
        Text = "CorsaCareer — portale della carriera";
        ClientSize = new Size(1400, 900);
        // Il minimo è ora una finestra realmente utilizzabile su 1366×768: il
        // layout si impagina da solo invece di tagliare i pannelli.
        MinimumSize = new Size(1120, 700);
        StartPosition = FormStartPosition.CenterScreen;
        // A schermo intero: il portale ha tre colonne e una finestra piccola
        // le comprime al punto da tagliare le etichette.
        WindowState = FormWindowState.Maximized;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = UiTheme.Background; ForeColor = UiTheme.TextPrimary; Font = UiTheme.Body;
        CareerLog.Initialize(saveDir);
        // Le pagine aperte nel browser possono richiamare la finestra principale
        // attraverso questo endpoint locale, così non serve cercarla fra le altre.
        // Il server di focus serve solo all'uso interattivo del portale. Nei
        // test UI automatici non avviarlo: evita un listener TCP e un task
        // persistente che possono tenere vivo il processo dopo la chiusura
        // della finestra.
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1")
            focusServer = PortalFocusServer.TryStart(FocusFromBrowser);
        NarrationService.PortalFocusUrl = focusServer?.Url ?? "";
        rounds = BuildRounds();
        BuildUi();
        LoadCareer();
        RestartHomeArtworkSequence(); LoadPending();
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1")
            MagazineExporter.Export(career, Path.Combine(saveDir, "media", "magazine"));
        // Non basta controllare l'esistenza del json: le versioni precedenti
        // potevano creare un salvataggio con il nome generico "Pilota
        // esordiente". In quel caso la prima schermata deve essere il profilo,
        // non la Home anonima.
        if (!File.Exists(SaveFile))
        {
            // I renderer di collaudo deve poter aprire una carriera demo senza
            // fermarsi sulla finestra modale che chiede nome e cognome. Il
            // portale usato dall'utente continua a mostrare il profilo come
            // primo passo obbligatorio.
            if (string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal))
            {
                career = new CareerState { NarrationOnStartup = false };
                // Il nome del pilota di collaudo è configurabile: i motori sono
                // deterministici e partono dal nome, quindi con un nome fisso
                // ogni esecuzione produce la stessa identica carriera. Per
                // misurare la forma di una carriera servono campioni diversi.
                var demoName = Environment.GetEnvironmentVariable("CORSACAREER_DEMO_DRIVER");
                var demoFirst = string.IsNullOrWhiteSpace(demoName) ? "Demo" : demoName.Trim();
                ApplyProfile(new DriverProfile
                {
                    FirstName = demoFirst, LastName = "Driver", Nationality = "Giappone",
                    RaceNumber = 27, Nickname = demoFirst, AvatarPath = "", CareerMode = "Realistica"
                });
                StartEvaluation();
                EnsureSponsorProspects();
                EnsurePaddockRoster();
                // Nessuna offerta in partenza: la carriera comincia in
                // valutazione, e un sedile va guadagnato. Prima qui si
                // firmava un contratto prima ancora della prima prova.
                SaveCareer();
            }
            else CreateProfile();
        }
        else if (NeedsDriverIdentity())
        {
            if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") == "1")
            {
                // Una migrazione può aver creato career.json prima che il
                // profilo fosse inizializzato. Nei test non aprire il dialogo
                // modale del nome: completa il profilo demo come al primo
                // avvio e lascia all'utente reale la richiesta interattiva.
                career = new CareerState { NarrationOnStartup = false };
                // Il nome del pilota di collaudo è configurabile: i motori sono
                // deterministici e partono dal nome, quindi con un nome fisso
                // ogni esecuzione produce la stessa identica carriera. Per
                // misurare la forma di una carriera servono campioni diversi.
                var demoName = Environment.GetEnvironmentVariable("CORSACAREER_DEMO_DRIVER");
                var demoFirst = string.IsNullOrWhiteSpace(demoName) ? "Demo" : demoName.Trim();
                ApplyProfile(new DriverProfile
                {
                    FirstName = demoFirst, LastName = "Driver", Nationality = "Giappone",
                    RaceNumber = 27, Nickname = demoFirst, AvatarPath = "", CareerMode = "Realistica"
                });
                StartEvaluation(); EnsureSponsorProspects(); EnsurePaddockRoster(); SaveCareer();
            }
            else CreateProfile();
        }
        RefreshUi();
        // L'apertura del capitolo e il bivio sono finestre modali: all'avvio la
        // finestra principale non e ancora visibile, quindi la presentazione
        // veniva scartata e non tornava piu. Va mostrata quando la finestra c'e
        // davvero.
        Shown += (_, _) =>
        {
            // Il renderer UI automatico deve poter catturare la finestra senza
            // aprire dialoghi modali o avviare la presentazione narrativa:
            // questi callback richiedono interazione e lasciavano il processo
            // appeso durante DrawToBitmap. Nel portale reale il comportamento
            // resta invariato.
            if (string.Equals(Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION"), "1", StringComparison.Ordinal))
                return;
            // Il tema d'avvio lo decide chi apre la scena: se c'è una fase da
            // presentare è il prologo a scegliere musica o voce e a restituire
            // il tema della Home alla chiusura. Suonare qui l'intro significava
            // avviare un terzo canale contro quello del prologo e quello che
            // RefreshUi ha già impostato, con esito indeterminato.
            AnnouncePhaseIfNew(); RegistraStradaDalSedile();
            if (storiesJustOpened) { storiesJustOpened = false; NarrationService.RefocusLastGuidedStory(); }
            if (!SoundtrackService.IsLoaded) SoundtrackService.PlayIntro();
            // Primo accesso: i tre indicatori li spiegano i personaggi.
            //
            // Prima c'era un tour guidato — velo scuro, riquadro illuminato,
            // freccia e «avanti» tre volte. Meccanico da attraversare e
            // dimenticabile: si premeva avanti e non restava niente. Le stesse
            // tre cose dette da chi se ne occupa davvero si ricordano.
            if (!career.BudgetTutorialSeen && !CareerMessages.Unattended)
            {
                BeginInvoke(new Action(() =>
                {
                    using var scena = new AnimeDialogueDialog(
                        "CorsaCareer — i tre numeri che contano", CoachBriefing.Indicatori(career));
                    scena.ShowDialog(this);
                    career.BudgetTutorialSeen = true;
                    SaveCareer(createVersionedBackup: false);
                }));
            }
        };
        resultTimer.Tick += (_, _) => TryImportRaceResult(); resultTimer.Start();
        autosaveTimer.Tick += (_, _) => SaveCareer(createVersionedBackup: false); autosaveTimer.Start();
        narrationTimer.Tick += (_, _) => { if (!NarrationService.IsSpeaking && narrationControl.Text.StartsWith("❚❚", StringComparison.Ordinal)) narrationControl.Text = "▶  AVVIA RUBRICA TV"; }; narrationTimer.Start();
        // Il referto può essere scritto da Assetto Corsa mentre la finestra è
        // sullo sfondo. Oltre al polling, lo rileggiamo immediatamente quando
        // il portale torna visibile e subito dopo il caricamento della UI:
        // una Practice chiusa senza giro valido deve comunque chiudere il
        // tentativo e far avanzare la carriera.
        Shown += (_, _) => BeginInvoke(new Action(TryImportRaceResult));
        Activated += (_, _) => TryImportRaceResult();
        FormClosed += (_, _) => { autosaveTimer.Stop(); resultTimer.Stop(); narrationTimer.Stop(); NarrationService.Stop(); SoundtrackService.Stop(); focusServer?.Dispose(); };
    }

    private string SaveFile => Path.Combine(saveDir, "career.json");

    private bool NeedsDriverIdentity()
    {
        static bool Placeholder(string value) => string.IsNullOrWhiteSpace(value)
            || value.Equals("Pilota", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Esordiente", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Pilota esordiente", StringComparison.OrdinalIgnoreCase);
        return Placeholder(career.FirstName) || Placeholder(career.LastName) || Placeholder(career.Driver);
    }
    private void LoadCareer()
    {
        var temporarySave = SaveFile + ".tmp";
        try
        {
            if (!File.Exists(SaveFile) && File.Exists(temporarySave)) File.Move(temporarySave, SaveFile, true);
            if (File.Exists(SaveFile)) career = JsonSerializer.Deserialize<CareerState>(File.ReadAllText(SaveFile)) ?? new();
        }
        catch
        {
            try
            {
                career = File.Exists(temporarySave) ? JsonSerializer.Deserialize<CareerState>(File.ReadAllText(temporarySave)) ?? new() : new();
                if (File.Exists(temporarySave)) File.Move(temporarySave, SaveFile, true);
            }
            catch { career = new(); }
        }
        var migrated = career.SchemaVersion < 11;
        var previousSchema = career.SchemaVersion;
        career.SchemaVersion = Math.Max(career.SchemaVersion, 11);
        // Normalizza i salvataggi creati da versioni precedenti: il budget è
        // un vincolo reale e non può mai risultare negativo nella UI o nelle
        // decisioni successive. Gli indici restano sempre nel loro intervallo.
        var normalizedCash = Math.Max(0, career.Cash);
        var normalizedFitness = Math.Clamp(career.Fitness, 0, 100);
        var normalizedTrust = Math.Clamp(career.TeamRelation, 0, 100);
        var normalizedSponsor = Math.Clamp(career.SponsorRelation, 0, 100);
        var normalizedFatigue = Math.Clamp(career.Fatigue, 0, 100);
        migrated |= normalizedCash != career.Cash
                    || normalizedFitness != career.Fitness
                    || normalizedTrust != career.TeamRelation
                    || normalizedSponsor != career.SponsorRelation
                    || normalizedFatigue != career.Fatigue;
        career.Cash = normalizedCash;
        career.Fitness = normalizedFitness;
        career.TeamRelation = normalizedTrust;
        career.SponsorRelation = normalizedSponsor;
        career.Fatigue = normalizedFatigue;
        career.Opportunities ??= new List<Opportunity>();
        career.Promises ??= new List<ConditionalPromise>();
        // Le carriere precedenti avevano un solo valore di reputazione: viene
        // proiettato sulle sei dimensioni senza perdere il percorso gia fatto.
        if (career.ReputationProfile == null || previousSchema < 11)
        {
            // Una carriera senza gare non ha un passato da proiettare: i vecchi
            // campi valgono i loro predefiniti (40 e 40) e facevano partire un
            // esordiente con la fiducia di chi ha gia corso una stagione.
            career.ReputationProfile = career.Races == 0 && career.RaceHistory.Count == 0
                ? new ReputationProfile()
                : new ReputationProfile
            {
                SportingPrestige = Math.Clamp(career.Reputation, 0, 100),
                TeamTrust = Math.Clamp(career.TeamRelation, 0, 100),
                SponsorAppeal = Math.Clamp(career.SponsorRelation, 0, 100),
                PublicPopularity = Math.Clamp(career.Fanbase, 0, 100),
                PressStanding = Math.Clamp((career.Reputation + career.Fanbase) / 2, 0, 100),
                Professionalism = Math.Clamp(60 - career.AbandonedSessions * 6, 0, 100)
            };
            migrated = true;
        }
        if (career.SeasonStartDate == default)
        {
            // Carriere precedenti: l'ancora viene dedotta dalle gare già archiviate
            // della stagione corrente, senza spostare nessun risultato.
            career.SeasonStartDate = NarrativeCalendar.InferSeasonStart(
                (career.RaceHistory ?? new List<RaceHistoryEntry>()).Where(x => x.Season == career.Season).Select(x => x.StoryDate),
                career.StoryDate);
            migrated = true;
        }
        if (!SessionProfiles.Difficulties.Contains(career.DifficultyProfile)) { career.DifficultyProfile = SessionProfiles.DifficultyRealistic; migrated = true; }
        if (!SessionProfiles.Distances.Contains(career.DistanceProfile)) { career.DistanceProfile = SessionProfiles.DistanceRealistic; migrated = true; }
        career.SessionPlans ??= new List<SessionPlanRecord>();
        var clampedCalibration = Math.Clamp(career.AiCalibrationOffset, AiCalibration.MinimumOffset, AiCalibration.MaximumOffset);
        if (clampedCalibration != career.AiCalibrationOffset) { career.AiCalibrationOffset = clampedCalibration; migrated = true; }
        if (career.Season <= 0) { career.Season = 1; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.Tier)) { career.Tier = "Rookie"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.CareerPhase)) { career.CareerPhase = career.Races > 0 ? "Active" : "Evaluation"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.Championship)) { career.Championship = "Campionato locale"; migrated = true; }
        if (career.StoryDate == default) { career.StoryDate = NarrativeCalendar.DefaultSeasonStart; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.NarratorStyle)) { career.NarratorStyle = "Radiocronaca italiana classica — narratore originale"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.PreferredVoice) || career.PreferredVoice.Equals("Automatica", StringComparison.OrdinalIgnoreCase) || career.PreferredVoice.StartsWith("Automatica ", StringComparison.OrdinalIgnoreCase)) { career.PreferredVoice = NarrationVoiceCatalog.Browser; migrated = true; }
        // L'etichetta della modalità browser è cambiata quando Google italiano è
        // diventato il servizio predefinito: i salvataggi precedenti vanno
        // normalizzati, altrimenti la voce non risulta selezionata in Impostazioni.
        if (career.PreferredVoice.StartsWith("Browser", StringComparison.OrdinalIgnoreCase) && !career.PreferredVoice.Equals(NarrationVoiceCatalog.Browser, StringComparison.Ordinal)) { career.PreferredVoice = NarrationVoiceCatalog.Browser; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.RookieEvaluationStatus)) { career.RookieEvaluationStatus = "In attesa della prima gara reale"; migrated = true; }
        // La prima auto in ordine di scansione non e la piu adatta a un
        // esordiente: una carriera nuova riceveva cosi una vettura da turismo da
        // 260 cavalli invece del gradino piu basso della scala.
        if (string.IsNullOrWhiteSpace(career.Car))
        {
            var entryCar = EntryLevelCar();
            if (entryCar != null) { career.Car = entryCar.Id; migrated = true; }
        }
        career.Livery ??= "";
        if (string.IsNullOrWhiteSpace(career.Livery))
        {
            var installedCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
            if (installedCar != null && installedCar.Skins.Count > 0) { career.Livery = installedCar.Skins[0]; migrated = true; }
        }
        if ((career.RaceHistory?.Count ?? 0) == 0 && career.StoryDate.Date == NarrativeCalendar.DefaultSeasonStart.Date && !string.IsNullOrWhiteSpace(career.Car))
        {
            // L'epoca narrativa segue l'auto d'ingresso quando questa dichiara un
            // anno: una vettura storica apre una carriera storica, una moderna no.
            //
            // Qui si guardava solo il NOME del file, cercandoci dentro quattro
            // cifre. Un kart chiamato «kart_rental_outdoor» non ne ha, quindi
            // finiva sempre nell'anno di ripiego: la carriera partiva nel 2005
            // anche con contenuti degli anni Novanta. L'anno dichiarato nei
            // metadati c'era gia' ed e' il dato giusto — e' quello che fa
            // partire la carriera nell'anno in cui quel campionato esisteva
            // davvero, che e' l'unica cosa che serve perche' tutte le vetture
            // che si incontreranno siano vetture vere di quell'epoca.
            var derived = StoryStartDateForContent(
                contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase)));
            if (derived.Year != career.StoryDate.Year) { career.StoryDate = derived; migrated = true; }
        }
        if (string.IsNullOrWhiteSpace(career.ContractObjectiveStatus)) { career.ContractObjectiveStatus = "In corso"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.SponsorObjectiveStatus)) { career.SponsorObjectiveStatus = "In corso"; migrated = true; }
        if (career.ContractYears <= 0 && career.ContractActive) { career.ContractActive = false; migrated = true; }
        career.Results ??= new List<string>();
        career.News ??= new List<string>();
        career.Offers ??= new List<TeamOffer>();
        career.RaceHistory ??= new List<RaceHistoryEntry>();
        foreach (var race in career.RaceHistory) { race.Classification ??= new List<RaceParticipantSnapshot>(); race.ResultSha256 ??= ""; }
        career.TestHistory ??= new List<TestSessionRecord>();
        foreach (var test in career.TestHistory) test.ResultSha256 ??= "";
        career.Events ??= new List<CareerEventRecord>();
        career.Drivers ??= new List<PersistentDriver>();
        career.Standings ??= new List<StandingEntry>();
        career.Rivalries ??= new List<RivalryRecord>();
        career.StoryArcs ??= new List<StoryArcRecord>();
        career.Journalist ??= new JournalistProfile();
        if (string.Equals(career.Journalist.Name, "Alessandro Ferretti", StringComparison.OrdinalIgnoreCase))
        {
            career.Journalist.Name = "Noa Minazuki";
            career.Journalist.Publication = "Grand Prix CorsaCareer";
            career.Journalist.Tone = "cronaca sportiva originale";
            migrated = true;
        }
        // Le firme salvate erano alias riconoscibili di persone reali. Vanno
        // sostituite anche nelle carriere già esistenti: un articolo generato non
        // deve continuare a essere attribuito a qualcuno che esiste davvero.
        foreach (var alias in new[] { "Mario Poltroni", "Ezio Zermignani" })
            if (string.Equals(career.Journalist.Name, alias, StringComparison.OrdinalIgnoreCase))
            {
                career.Journalist.Name = alias == "Mario Poltroni" ? "Noa Minazuki" : "Aoi Serizawa";
                migrated = true;
            }
        if (string.Equals(career.Journalist.ExpertCommentator, "Cally Ragazzoni", StringComparison.OrdinalIgnoreCase))
        {
            career.Journalist.ExpertCommentator = "Vittorio Sanna";
            career.Journalist.ExpertCommentatorBio = "ex pilota di formule minori, oggi commentatore tecnico — personaggio inventato";
            migrated = true;
        }
        if (string.IsNullOrWhiteSpace(career.Journalist.ExpertCommentator)) { career.Journalist.ExpertCommentator = "Vittorio Sanna"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.Journalist.NarrativeStyle)) { career.Journalist.NarrativeStyle = "Cronaca classica analitica"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.Journalist.ExpertCommentatorBio)) { career.Journalist.ExpertCommentatorBio = "ex pilota di formule minori, oggi commentatore tecnico — personaggio inventato"; migrated = true; }
        career.TeamHistory ??= new List<TeamHistoryEntry>();
        career.SeasonArchive ??= new List<SeasonSummary>();
        if (string.IsNullOrWhiteSpace(career.MediaSequenceMode)) { career.MediaSequenceMode = "Rassegna consigliata"; migrated = true; }
        if (string.IsNullOrWhiteSpace(career.MediaFilter)) { career.MediaFilter = "Tutti i media"; migrated = true; }
        if (career.MediaSequencePosition < 0) { career.MediaSequencePosition = 0; migrated = true; }
        if (!string.IsNullOrWhiteSpace(career.AvatarPath) && File.Exists(career.AvatarPath))
        {
            var archivedAvatar = ArchiveDriverPortrait(career.AvatarPath);
            if (!string.Equals(archivedAvatar, career.AvatarPath, StringComparison.OrdinalIgnoreCase)) { career.AvatarPath = archivedAvatar; migrated = true; }
        }
        if (career.RaceHistory.Count == 0)
            foreach (var item in career.Events.Where(x => x.StoryDate == default || x.StoryDate.Year != career.StoryDate.Year))
                item.StoryDate = career.StoryDate;
        var timelineDate = career.StoryDate;
        foreach (var item in career.Events.Where(x => x.StoryDate == default).OrderBy(x => x.DateUtc)) { item.StoryDate = timelineDate; timelineDate = timelineDate.AddDays(7); migrated = true; }
        // Solo senza contratto: sotto contratto una lista vuota e la
        // situazione corretta, non un mercato da riempire.
        if (career.Offers.Count == 0 && !career.ContractActive) { career.Offers = BuildOffers(); SaveCareer(); }
        else if (migrated) SaveCareer();
        career.EvaluationTargetBasis ??= "";
        career.EvaluationTargetTrack ??= "";
        career.Schedule ??= new List<ScheduledEvent>();
        // Le selezioni sono arrivate dopo: una carriera salvata prima non ne ha,
        // e la lista vuota e lo stato giusto, non un dato mancante da inventare.
        career.Selections ??= new List<SelectionTrial>();
        foreach (var trial in career.Selections) trial.Days ??= [];
        career.ActivityHistory ??= new List<ActivityRecord>();
        // Salvataggi creati dalla precedente regola non devono conservare un
        // invito regalato dopo prove insufficienti. L'appuntamento diventa un
        // test di recupero, senza cancellare la cronologia già registrata.
        foreach (var invite in career.Schedule.Where(x => x.IsPlanned && x.Kind == ScheduledEventKind.Invitation && career.RookieEvaluationScore < CareerScheduler.MinimumInvitationScore))
        {
            invite.Kind = ScheduledEventKind.EvaluationTest;
            invite.GeneratedBy = "Invito non concesso: le prove non hanno ancora convinto il paddock.";
            invite.Objective = "Dimostrare continuità prima di ricevere una gara su invito";
            migrated = true;
        }
        if (career.DaysUntilNextRound < 0) { career.DaysUntilNextRound = OffTrackActivities.DaysBetweenRounds(); migrated = true; }
        career.Fatigue = Math.Clamp(career.Fatigue, 0, OffTrackActivities.MaxFatigue);
        career.TeamRelation = Math.Clamp(career.TeamRelation, 0, 100);
        career.SponsorRelation = Math.Clamp(career.SponsorRelation, 0, 100);
        career.Fanbase = Math.Max(0, career.Fanbase);
        EnsureEntryLevelCareer();
        EnsureSponsorProspects();
        EnsurePaddockRoster();
        // L'anno e il mese dei round dipendono dalla data narrativa scelta
        // per la vettura/categoria, non da una stagione storica fissa.
        rounds = BuildRounds();
        // Carriere salvate prima dell'agenda: gli appuntamenti già disputati
        // vengono ricostruiti dallo storico, così nessun risultato si sposta.
        MigrateScheduleFromHistory();
        EnsureSchedule();
        // Riparazione delle carriere rimaste in uno stato senza uscita: un
        // contratto attivo con la fase ancora in valutazione e nessun round in
        // calendario. Nasceva dall'assegnazione della categoria d'ingresso, che
        // attivava il contratto senza aprire il campionato: la home mostrava
        // squadra e sponsor ma dopo la scelta del team non accadeva nulla.
        if (career.ContractActive && contentIndex.Tracks.Count > 0
            && !career.Schedule.Any(x => x.Kind == ScheduledEventKind.ChampionshipRound))
        {
            if (career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase))
            {
                career.CareerPhase = "Active";
                if (string.IsNullOrWhiteSpace(career.ContractObjectiveStatus)) career.ContractObjectiveStatus = "In corso";
            }
            if (career.SeasonStartDate == default) career.SeasonStartDate = career.StoryDate;
            GenerateSeasonSchedule("Riparazione: contratto attivo senza calendario");
            CareerLog.Info("agenda", "carriera riparata: il contratto era attivo ma il campionato non era mai stato aperto");
            migrated = true;
        }
        if (migrated) SaveCareer(createVersionedBackup: false);
        // Le carriere salvate con il vecchio obiettivo fisso vengono riallineate
        // alla combinazione pista/vettura realmente installata.
        if (career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase) && contentIndex.Tracks.Count > 0)
        {
            var previousTarget = career.EvaluationTargetMilliseconds;
            RefreshEvaluationTarget();
            if (career.EvaluationTargetMilliseconds != previousTarget) SaveCareer(createVersionedBackup: false);
        }
    }

    private void EnsurePaddockRoster()
    {
        StoryCastService.Ensure(career);
        TeamInterestService.Refresh(career);
        career.Drivers ??= new List<PersistentDriver>();
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase) && ContentCategoryRules.IsRaceable(x))
            ?? contentIndex.Cars.FirstOrDefault(ContentCategoryRules.IsRaceable);
        if (car == null) return;
        var categoryCars = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable)
            .Where(x => x.Category.Equals(car.Category, StringComparison.OrdinalIgnoreCase)).ToList();
        if (categoryCars.Count == 0) categoryCars.Add(car);
        var before = career.Drivers.Count;
        var rosterEventMissing = !career.Events.Any(x => x.Type.Equals("PADDOCK_ROSTER", StringComparison.OrdinalIgnoreCase));
        AddRosterDriver(career.Driver, career.Nationality, career.Team, car.Id, car.Category, "pilota", 70, "all-rounder");
        AddRosterDriver(career.Teammate, "Italia", career.Team, categoryCars[0].Id, car.Category, "compagno di squadra", 72, "gara");
        var names = new[] { "Sota Fujimoto", "Mei Kanzaki", "Jonas Weber", "Claire Moreau", "Diego Serrano" };
        for (var i = 0; i < names.Length; i++)
        {
            var rivalCar = categoryCars[i % categoryCars.Count];
            var rivalTeam = AiDriverIdentity.Team(names[i], rivalCar.Id);
            AddRosterDriver(names[i], AiDriverIdentity.Nationality(names[i]), rivalTeam, rivalCar.Id, rivalCar.Category, "avversario", 60 + (i * 3 % 18), i % 2 == 0 ? "qualifica" : "all-rounder");
        }
        SyncCurrentLineup(car);
        if ((career.Drivers.Count > before || (rosterEventMissing && career.Drivers.Count >= 2)) && rosterEventMissing)
        {
            // Un esordiente non ha un progetto: nominarne uno faceva comparire
            // nel primo articolo una squadra che il pilota non ha.
            var headline = CareerPhases.IsUncontracted(career.Team)
                ? $"Il paddock apre le porte a {career.Driver}: {career.Drivers.Count} identità registrate, nessuna delle quali lo aspetta."
                : $"Il paddock apre le porte a {career.Driver}: {career.Drivers.Count} identità sono state registrate per il progetto {career.Team}.";
            career.News.Add(headline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "PADDOCK_ROSTER", Headline = headline, Track = "Paddock", Importance = 55 });
        }
        SaveCareer();
    }

    private void SyncCurrentLineup(ContentCarRecord activeCar)
    {
        var player = career.Drivers.FirstOrDefault(x => AiDriverIdentity.NamesEqual(x.Name, career.Driver));
        if (player != null)
        {
            player.Team = career.Team; player.Car = activeCar.Id; player.Category = activeCar.Category; player.Relationship = "pilota";
        }
        var teammate = career.Drivers.FirstOrDefault(x => AiDriverIdentity.NamesEqual(x.Name, career.Teammate));
        if (teammate != null)
        {
            teammate.Team = career.Team; teammate.Car = activeCar.Id; teammate.Category = activeCar.Category; teammate.Relationship = "compagno di squadra";
        }
    }

    private void AddRosterDriver(string name, string nationality, string team, string car, string category, string relationship, int skill, string specialization)
    {
        if (string.IsNullOrWhiteSpace(name) || career.Drivers.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return;
        var seed = Math.Abs(name.Aggregate(17L, (hash, character) => hash * 31 + character));
        career.Drivers.Add(new PersistentDriver
        {
            Name = name, Nationality = nationality, Team = team, Car = car, Category = category,
            Relationship = relationship, Skill = skill, Speed = Math.Clamp(skill + (int)(seed % 11) - 5, 1, 100),
            Consistency = Math.Clamp(skill + (int)(seed % 17) - 8, 1, 100), Aggression = 30 + (int)(seed % 51),
            Age = 20 + (int)(seed % 21), Specialization = specialization, Reputation = skill / 2,
            Races = 0, Wins = 0, Points = 0, LastPosition = 0, LastTrack = ""
        });
    }

    private void EnsureEntryLevelCareer()
    {
        if (career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase)) return;
        // Una carriera senza gare parte sempre dalla categoria minore disponibile.
        if (contentIndex.Cars.Count == 0) return;
        if (career.Races > 0 || career.Round > 0)
        {
            var activeCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase) && ContentCategoryRules.IsRaceable(x));
            if (activeCar != null)
            {
                var activeTier = TierForCategory(activeCar.Category);
                // Il nome del campionato non dipende più dalla categoria della
                // vettura ma dall'altezza raggiunta: qui si riallinea solo la
                // categoria, e il campionato prende il nome del proprio livello.
                if (!career.Tier.Equals(activeTier, StringComparison.OrdinalIgnoreCase))
                {
                    career.Tier = activeTier; career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel);
                    career.News.Add($"Categoria riallineata dai contenuti installati: {activeCar.Name} · {activeTier}.");
                    SaveCareer();
                }
            }
            return;
        }
        var raceable = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).ToList();
        // Una carriera reale non può iniziare con una vettura stradale o con
        // un placeholder: senza un'auto da gara si resta in attesa dei contenuti.
        if (raceable.Count == 0) return;
        var entry = EntryLevelCar();
        if (entry == null) return;
        var expectedTier = TierForCategory(entry.Category);
        var sameCar = entry.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase);
        if (sameCar && career.Tier.Equals(expectedTier, StringComparison.OrdinalIgnoreCase)) return;

        // Una carriera gia avviata non va riportata al punto di partenza.
        //
        // Firmare con un team significa cambiare vettura, ed e' proprio quello
        // che questa funzione interpretava come "carriera da inizializzare":
        // riazzerava squadra, fase e calendario a ogni avvio. Si firmava, si
        // riapriva il programma, e la firma era sparita.
        var alreadyStarted = CareerPhases.IsUncontracted(career.Team) == false
            || career.Races > 0
            || (career.Schedule ?? []).Any(x => x.IsPlanned)
            || (career.RaceHistory ?? []).Count > 0;
        if (alreadyStarted)
        {
            CareerLog.Info("contenuti",
                $"vettura d'ingresso diversa da quella in uso ({career.Car}), ma la carriera e' gia avviata: nessun ripristino.");
            return;
        }
        career.Car = entry.Id; career.Livery = entry.Skins.FirstOrDefault() ?? ""; career.Tier = expectedTier; career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel);
        if (!sameCar)
        {
            // L'ingresso di categoria assegna solo il mezzo di riferimento. Il
            // pilota non viene assunto automaticamente: al primo gradino è un
            // cliente che deve acquistare la propria giornata dal paddock.
            career.Team = "Senza contratto"; career.Teammate = "Kenta Ogawa"; career.Sponsor = "In attesa di sponsor"; career.ContractSalary = 0;
            career.ContractYears = 0; career.ContractActive = false; career.IsClientDriver = true; career.TeamSupportPercent = 0;
            career.ContractObjective = "Trovare una gara cliente e restare in corsa";
            career.ContractObjectiveStatus = "In attesa";
            career.StoryDate = StoryStartDateForContent(entry);
            career.CareerPhase = "Evaluation";
            career.Round = 0;
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "ENTRY_LEVEL_ASSIGNED",
                Headline = $"{career.Driver} trova una vettura d'ingresso: il prossimo passo sarà pagare una gara cliente.",
                Track = "Paddock", Importance = 55
            });
            // Una carriera appena creata può non avere ancora l'agenda.
            career.Schedule ??= [];
            career.Schedule.RemoveAll(x => x.Kind == ScheduledEventKind.ChampionshipRound && x.IsPlanned);
        }
        career.Headline = $"{career.Driver} viene riallineato alla categoria {entry.Category}: {entry.Name}.";
        if (!career.News.Contains(career.Headline)) career.News.Add(career.Headline);
        if (!career.Events.Any(x => x.Type == "ENTRY_LEVEL_ASSIGNED")) career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "ENTRY_LEVEL_ASSIGNED", Headline = career.Headline, Track = "Paddock", Importance = 70 });
        SaveCareer();
    }

    private void EnsureSponsorProspects()
    {
        career.SponsorProspects ??= new List<SponsorProspect>();
        if (career.SponsorProspects.Count > 0) return;
        // Nomi inventati: nessuno corrisponde a un'azienda reale. Le distanze
        // partono dalla citta del pilota e decidono il costo del viaggio.
        career.SponsorProspects.AddRange(SponsorHunt.SeedFor(career.HomeCity));
        career.SponsorActionsRemaining = SponsorHunt.MaxVisitsPerWeek;
    }
    private string lastSavedCareerHash = "";

    private void SaveCareer(bool createVersionedBackup = true)
    {
        Directory.CreateDirectory(saveDir);
        var payload = JsonSerializer.Serialize(career, new JsonSerializerOptions { WriteIndented = true });
        var payloadHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload)));
        // La guardia deve partire dal contenuto già su disco, non solo
        // dall'ultimo salvataggio di questo processo: altrimenti il primo
        // SaveCareer di ogni avvio riscriveva e creava un backup anche quando la
        // carriera era identica, ed era proprio l'avvio a consumare l'anello.
        if (string.IsNullOrEmpty(lastSavedCareerHash) && File.Exists(SaveFile))
        {
            try { lastSavedCareerHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(SaveFile))); }
            catch (Exception error) { CareerLog.Warn("salvataggio", $"impronta del salvataggio su disco non leggibile: {error.Message}"); }
        }
        // Guardia contro i salvataggi a vuoto. L'allineamento all'avvio chiamava
        // SaveCareer anche senza modifiche: ogni lancio bruciava due copie
        // dell'anello di venti backup, cancellando la storia reale in dieci avvii.
        if (payloadHash.Equals(lastSavedCareerHash, StringComparison.Ordinal) && File.Exists(SaveFile))
        {
            if (!createVersionedBackup && saveStatus != null) saveStatus.Text = $"Autosave: {DateTime.Now:HH:mm:ss}  ·  nessuna modifica da salvare.";
            return;
        }
        // Nessuno sta giocando: non serve conservare venti versioni.
        //
        // L'anello di backup protegge la carriera di chi gioca da un errore o
        // da un salvataggio corrotto. Il banco di prova percorre una carriera
        // intera in due minuti e ne produceva venti copie da sei megabyte
        // ciascuna: centoventisei megabyte per esecuzione, e nove gigabyte di
        // cartelle temporanee dopo una sessione di lavoro. Lì il salvataggio
        // serve solo a poter riaprire il risultato nel portale.
        if (createVersionedBackup && !CareerMessages.Unattended && File.Exists(SaveFile))
        {
            File.Copy(SaveFile, SaveFile + ".bak", true);
            var backupDir = Path.Combine(saveDir, "backups"); Directory.CreateDirectory(backupDir);
            var versioned = Path.Combine(backupDir, $"career-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json");
            File.Copy(SaveFile, versioned, true);
            foreach (var old in Directory.GetFiles(backupDir, "career-*.json").OrderByDescending(File.GetLastWriteTimeUtc).Skip(20)) File.Delete(old);
        }
        var temporary = SaveFile + ".tmp";
        File.WriteAllText(temporary, payload);
        // Il salvataggio non deve poter buttare giu' il programma.
        //
        // La sostituzione del file puo' fallire per ragioni che non dipendono
        // dalla carriera: un antivirus che tiene aperto il file appena scritto,
        // una sincronizzazione su cartella condivisa, un indicizzatore. Prima
        // l'eccezione usciva da qui e chiudeva il gioco — con la carriera
        // ancora buona sul disco e il giocatore convinto di averla persa.
        // Si riprova qualche volta e, se proprio non si puo', si continua: il
        // file temporaneo resta accanto al salvataggio e il registro lo dice.
        var sostituito = false;
        for (var tentativo = 1; tentativo <= 4 && !sostituito; tentativo++)
        {
            try
            {
                File.Move(temporary, SaveFile, true);
                sostituito = true;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                if (tentativo == 4)
                {
                    CareerLog.Error("salvataggio", $"il file «{SaveFile}» non ha potuto essere sostituito dopo {tentativo} tentativi. La copia nuova resta in «{temporary}».", error);
                    if (saveStatus != null) saveStatus.Text = "Salvataggio non riuscito: il file e' occupato da un altro programma. La carriera precedente resta intatta.";
                    return;
                }
                Thread.Sleep(60 * tentativo);
            }
        }
        lastSavedCareerHash = payloadHash;
        if (!createVersionedBackup && saveStatus != null) saveStatus.Text = $"Autosave: {DateTime.Now:HH:mm:ss}  ·  Il risultato viene acquisito solo dopo una gara realmente conclusa in Assetto Corsa.";
        // Il magazine viene rigenerato solo quando la carriera cambia davvero:
        // prima girava a ogni autosave, cioè ogni sessanta secondi per sempre.
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1")
            MagazineExporter.Export(career, Path.Combine(saveDir, "media", "magazine"));
    }
    private void LoadPending()
    {
        try
        {
            var pendingFile = Path.Combine(saveDir, "pending_weekend.json");
            if (!File.Exists(pendingFile)) return;
            using var doc = JsonDocument.Parse(File.ReadAllText(pendingFile));
            pendingMode = doc.RootElement.TryGetProperty("mode", out var pendingType) ? pendingType.GetString() ?? "race" : "race";
            pendingResultHash = doc.RootElement.TryGetProperty("resultHashBeforeLaunch", out var resultHash) ? resultHash.GetString() ?? "" : "";
            if (doc.RootElement.TryGetProperty("launchUtc", out var launch))
            {
                launchTimeUtc = launch.GetDateTime();
                if (DateTime.UtcNow - launchTimeUtc > TimeSpan.FromHours(6))
                {
                    awaitingResult = false; pendingResultHash = "";
                    var stale = pendingFile + ".stale-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                    File.Move(pendingFile, stale, true);
                    CareerMessages.Show(null, "È stato trovato un weekend pendente da oltre 6 ore. È stato archiviato come non concluso: nessun risultato è stato registrato.", "CorsaCareer - weekend pendente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else awaitingResult = true;
            }
        }
        catch { awaitingResult = false; pendingResultHash = ""; }
    }
    private void TryImportRaceResult()
    {
        if (!awaitingResult) return;
        var resultFile = AssettoCorsaResultLocator.FindLatestExisting();
        var probe = $"{pendingMode}|{launchTimeUtc:O}|{resultFile}|{(File.Exists(resultFile) ? File.GetLastWriteTimeUtc(resultFile).ToString("O") : "missing")}";
        if (!probe.Equals(lastImportProbe, StringComparison.Ordinal))
        {
            lastImportProbe = probe;
            CareerLog.Info("import", $"controllo referto: modalità={pendingMode}, avvio={launchTimeUtc:O}, file={resultFile}, aggiornato={(File.Exists(resultFile) ? File.GetLastWriteTimeUtc(resultFile).ToString("O") : "assente")}");
        }
        if (!File.Exists(resultFile) || File.GetLastWriteTimeUtc(resultFile) <= launchTimeUtc) return;
        try
        {
            var json = File.ReadAllText(resultFile);
            var currentResultHash = HashFile(resultFile);
            if (!string.IsNullOrWhiteSpace(pendingResultHash) && currentResultHash.Equals(pendingResultHash, StringComparison.OrdinalIgnoreCase))
            {
                var unchangedSignature = $"unchanged:{currentResultHash}";
                if (lastRejectedResultSignature != unchangedSignature)
                {
                    lastRejectedResultSignature = unchangedSignature;
                    CareerLog.Warn("import", "referto invariato rispetto all'avvio della sessione");
                    CareerMessages.Show(null, "Il race_out.json è identico a quello presente prima dell’avvio del weekend. Completa una nuova sessione reale in Assetto Corsa.", "CorsaCareer - referto precedente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            var pendingPath = Path.Combine(saveDir, "pending_weekend.json");
            if (File.Exists(pendingPath))
            {
                try { using var pendingDoc = JsonDocument.Parse(File.ReadAllText(pendingPath)); pendingMode = pendingDoc.RootElement.TryGetProperty("mode", out var mode) ? mode.GetString() ?? "race" : "race"; } catch { pendingMode = "race"; }
            }
            var sessionType = pendingMode.Equals("test", StringComparison.OrdinalIgnoreCase) ? 1 : 3;
            if (!RaceResultParser.TryParse(json, out var imported, sessionType, career.Driver))
            {
                var invalidSignature = $"invalid:{File.GetLastWriteTimeUtc(resultFile):O}";
                if (lastRejectedResultSignature != invalidSignature) { lastRejectedResultSignature = invalidSignature; CareerMessages.Show(null, "È stato trovato un race_out.json aggiornato, ma non contiene una classifica di gara leggibile. Completa una gara in Assetto Corsa e riprova.", "CorsaCareer - risultato non leggibile", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                return;
            }
            // Una rookie evaluation precede il campionato: non esistono ancora
            // round da indicizzare. In quel caso l'identità della sessione è
            // quella salvata nel weekend pendente, non una classifica futura.
            var scheduledTrack = NextScheduled()?.TrackId ?? "";
            var expected = !string.IsNullOrWhiteSpace(scheduledTrack)
                ? scheduledTrack
                : rounds.Count > 0 ? rounds[Math.Min(career.Round, rounds.Count - 1)].Track : "";
            var expectedCar = "";
            var pendingPreset = "";
            var pendingFound = false;
            var pendingPathForTrack = Path.Combine(saveDir, "pending_weekend.json");
            try
            {
                if (File.Exists(pendingPathForTrack))
                {
                    pendingFound = true;
                    using var pendingDoc = JsonDocument.Parse(File.ReadAllText(pendingPathForTrack));
                    if (pendingDoc.RootElement.TryGetProperty("preset", out var pendingPresetValue)) pendingPreset = pendingPresetValue.GetString() ?? "";
                    if (pendingDoc.RootElement.TryGetProperty("track", out var pendingTrack) && !string.IsNullOrWhiteSpace(pendingTrack.GetString())) expected = pendingTrack.GetString()!;
                    if (pendingDoc.RootElement.TryGetProperty("car", out var pendingCar) && !string.IsNullOrWhiteSpace(pendingCar.GetString())) expectedCar = pendingCar.GetString()!;
                }
            }
            catch { }
            if (!pendingFound)
            {
                var missingPendingSignature = $"pending-missing:{File.GetLastWriteTimeUtc(resultFile):O}";
                if (lastRejectedResultSignature != missingPendingSignature) { lastRejectedResultSignature = missingPendingSignature; CareerMessages.Show(null, "Risultato reale trovato, ma non esiste un weekend CorsaCareer associato. Nessun dato è stato registrato.", "CorsaCareer - sessione non associata", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                return;
            }
            if (string.IsNullOrWhiteSpace(expected))
            {
                const string noIdentitySignature = "pending-without-track";
                if (lastRejectedResultSignature != noIdentitySignature)
                {
                    lastRejectedResultSignature = noIdentitySignature;
                    CareerLog.Warn("import", "weekend pendente senza circuito associato");
                    CareerMessages.Show(null, "Il weekend pendente non dichiara il circuito della sessione. Nessun risultato è stato registrato.", "CorsaCareer - sessione non associata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }
            if (!ContentManagerPresetValidator.TryValidate(pendingPreset, out var pendingPresetError) || !ContentManagerPresetValidator.TryReadIdentity(pendingPreset, out var presetCar, out var presetTrack, out _)
                || !string.Equals(expected, presetTrack, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(expectedCar) && !string.Equals(expectedCar, presetCar, StringComparison.OrdinalIgnoreCase)))
            {
                var presetSignature = $"preset-mismatch:{File.GetLastWriteTimeUtc(resultFile):O}";
                if (lastRejectedResultSignature != presetSignature) { lastRejectedResultSignature = presetSignature; CareerMessages.Show(null, $"Weekend pendente incoerente: il preset Content Manager non corrisponde ai dati salvati ({pendingPresetError}). Nessun risultato è stato registrato.", "CorsaCareer - identità weekend", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                return;
            }
            var identityMismatch = RaceImportIdentity.Mismatch(expected, imported.Track, expectedCar, imported.Car);
            if (identityMismatch != null)
            {
                var mismatchSignature = $"identity:{File.GetLastWriteTimeUtc(resultFile):O}:{imported.Track}:{imported.Car}";
                if (lastRejectedResultSignature != mismatchSignature) { lastRejectedResultSignature = mismatchSignature; CareerMessages.Show(null, $"Risultato reale rifiutato: {identityMismatch}. Nessun dato è stato registrato.", "CorsaCareer - sessione diversa", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                return;
            }
            var resultWrittenUtc = File.GetLastWriteTimeUtc(resultFile);
            var photo = ArchiveLatestScreenshot(launchTimeUtc, career.Round + 1, imported.Track, resultWrittenUtc);
            var archivedResultFile = ArchiveResultSnapshot(resultFile, career.Round + 1, imported.Track, pendingMode);
            if (string.IsNullOrWhiteSpace(archivedResultFile)) archivedResultFile = resultFile;
            var importedHash = HashFile(archivedResultFile);
            // Una giornata di selezione e' una sessione di prove come il test, ma
            // il referto va archiviato nella giornata giusta della selezione.
            if (pendingMode.Equals("selection", StringComparison.OrdinalIgnoreCase))
            {
                var trial = career.Selections.FirstOrDefault(x => x.IsOpen);
                var day = trial?.NextDay;
                if (trial == null || day == null) { CompletePendingWeekend(); return; }
                if (!string.IsNullOrWhiteSpace(importedHash)
                    && trial.Days.Any(x => x.Completed && x.ResultSha256.Equals(importedHash, StringComparison.OrdinalIgnoreCase)))
                { CompletePendingWeekend(); return; }
                CompletePendingWeekend();
                RecordSelectionDay(trial, day, imported, archivedResultFile, importedHash, simulated: false);
                return;
            }
            var alreadyRecorded = pendingMode.Equals("test", StringComparison.OrdinalIgnoreCase)
                ? career.TestHistory.Any(x => !string.IsNullOrWhiteSpace(importedHash) && x.ResultSha256.Equals(importedHash, StringComparison.OrdinalIgnoreCase))
                : career.RaceHistory.Any(x => x.Season == career.Season && x.Round == career.Round + 1 && x.Track.Equals(imported.Track, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(importedHash) && x.ResultSha256.Equals(importedHash, StringComparison.OrdinalIgnoreCase));
            if (alreadyRecorded) { CompletePendingWeekend(); return; }
            if (pendingMode.Equals("test", StringComparison.OrdinalIgnoreCase))
            {
                // Una sessione chiusa senza alcun giro cronometrato non produce
                // un tempo, ma resta un tentativo speso: senza questo si potrebbe
                // entrare e uscire all'infinito fino a ottenere il giro giusto.
                if (imported.BestLapMilliseconds <= 0)
                {
                    // Chiudiamo l'associazione PRIMA del dialogo di esito.
                    // Un MessageBox fa riattivare la finestra e può quindi
                    // provocare un secondo Tick/Activated: lo stesso referto
                    // deve poter consumare un solo tentativo.
                    CompletePendingWeekend();
                    RegisterWithdrawnTest(WithdrawalKind.NoTimedLap, imported.Track);
                    CareerLog.Info("import", $"test chiuso senza giro cronometrato: {imported.Track} · referto {Path.GetFileName(resultFile)}");
                }
                else
                {
                    CompletePendingWeekend();
                    RecordTest(imported, photo, archivedResultFile);
                    CareerLog.Info("import", $"test importato: {imported.Track} · miglior giro {imported.BestLapMilliseconds} ms");
                    CareerMessages.Show(null, $"Dati del test importati: miglior giro {FormatLap(imported.BestLapMilliseconds)}.\nNessun punto o risultato di campionato è stato assegnato.", "CorsaCareer — test reale", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (pendingMode.Equals("invitation", StringComparison.OrdinalIgnoreCase))
            {
                RecordInvitation(imported, photo, archivedResultFile);
                CompletePendingWeekend();
                CareerMessages.Show(null, $"Gara su invito importata: P{imported.Position} su {imported.Classification.Count} partecipanti.\n\nIl risultato entra nella storia del pilota, ma non assegna punti di campionato.", "CorsaCareer — gara su invito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // La reazione del box e l'articolo li apre già RecordInvitation:
                // qui aprirli di nuovo mostrava lo stesso articolo due volte.
            }
            else
            {
                ApplyClassification(imported.Classification, imported.Track, imported.QualificationPosition); Record(imported, photo, archivedResultFile);
                CompletePendingWeekend();
                // A stagione chiusa il bilancio lo ha gia' mostrato la
                // chiusura stessa, insieme al verdetto: qui si aprirebbe due
                // volte lo stesso dossier.
                if (career.Round < rounds.Count || career.SeasonArchive.Count == 0)
                {
                    using var report = new RaceReportDialog(career, imported, photo, NarrateCurrentStory, OpenMedia);
                    report.ShowDialog(this);
                }
            }
        }
        catch (Exception error) { CareerLog.Error("import", "importazione del referto interrotta", error); acLog(error.Message); }
    }
    private void CheckResultNow()
    {
        if (!awaitingResult)
        {
            CareerMessages.Show(null, "Non c’è un weekend CorsaCareer pendente. Avvia prima una gara o un test tramite Content Manager.", "Risultato Assetto Corsa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var resultFile = AssettoCorsaResultLocator.FindLatestExisting();
        if (!File.Exists(resultFile))
        {
            CareerMessages.Show(null, "Non è ancora presente race_out.json. Completa una sessione reale in Assetto Corsa e poi riprova.", "Risultato Assetto Corsa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (File.GetLastWriteTimeUtc(resultFile) <= launchTimeUtc)
        {
            CareerMessages.Show(null, "Il race_out.json presente è precedente all’avvio di questo weekend. Completa una nuova sessione reale in Assetto Corsa.", "Risultato Assetto Corsa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var before = awaitingResult;
        TryImportRaceResult();
        if (before && awaitingResult && string.IsNullOrWhiteSpace(lastRejectedResultSignature)) CareerMessages.Show(null, "Il referto esiste, ma non è ancora importabile per questo weekend: verifica circuito, auto e tipo di sessione.", "Risultato Assetto Corsa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    private static void acLog(string message) { System.Diagnostics.Debug.WriteLine(message); CareerLog.Warn("import", message); }
    private static string HashFile(string path)
    {
        try { return File.Exists(path) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) : ""; }
        catch { return ""; }
    }
    private void ApplyClassification(IReadOnlyList<ImportedDriverResult> classification, string actualTrack, int playerQualifyingPosition)
    {
        // Lo stesso regolamento del giocatore, altrimenti la classifica mescola
        // due sistemi di punteggio diversi e non è confrontabile.
        var rules = ChampionshipRulesForCareer();
        var fastestLapDriver = ChampionshipRegulations.FastestLapDriver(classification);
        foreach (var participant in classification)
        {
            var displayName = participant.IsPlayer ? career.Driver : participant.Name;
            var driver = career.Drivers.FirstOrDefault(x => AiDriverIdentity.NamesEqual(x.Name, displayName));
            var contentCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(participant.Car, StringComparison.OrdinalIgnoreCase));
            if (driver == null)
            {
                var stableSeed = (int)(Math.Abs(displayName.Aggregate(17L, (hash, character) => hash * 31 + character)) % 100000);
                driver = new PersistentDriver { Name = displayName, Nationality = displayName == career.Driver ? career.Nationality : AiDriverIdentity.Nationality(displayName), Car = participant.Car, Team = displayName == career.Driver ? career.Team : AiDriverIdentity.Team(displayName, participant.Car), Skill = displayName == career.Driver ? 70 : Math.Max(50, 96 - participant.Position), Age = 20 + stableSeed % 21, Speed = 55 + stableSeed % 41, Consistency = 45 + (stableSeed / 7) % 51, Aggression = 25 + (stableSeed / 13) % 71, Specialization = stableSeed % 3 == 0 ? "qualifica" : stableSeed % 3 == 1 ? "gara" : "all-rounder", Category = contentCar?.Category ?? "" };
                career.Drivers.Add(driver);
            }
            if (!participant.IsPlayer)
            {
                driver.Car = participant.Car;
                driver.Team = AiDriverIdentity.Team(displayName, participant.Car);
            }
            driver.Races++;
            var observedSkill = classification.Count <= 1 ? 70 : Math.Clamp(100 - ((participant.Position - 1) * 70 / (classification.Count - 1)), 30, 100);
            driver.Skill = driver.Races == 1 ? observedSkill : (driver.Skill * 3 + observedSkill) / 4;
            driver.Speed = Math.Clamp((driver.Speed * 3 + observedSkill) / 4, 1, 100);
            driver.Experience++;
            if (string.IsNullOrWhiteSpace(driver.Category)) driver.Category = contentCar?.Category ?? "";
            driver.LastPosition = participant.Position; driver.LastTrack = string.IsNullOrWhiteSpace(actualTrack) ? "" : actualTrack;
            var hasFastestLap = !string.IsNullOrWhiteSpace(fastestLapDriver) && AiDriverIdentity.NamesEqual(fastestLapDriver, participant.Name);
            // La pole dei piloti AI non è nel referto di gara: il bonus pole viene
            // assegnato solo al giocatore, dove la qualifica è realmente nota.
            var points = ChampionshipRegulations.PointsForPosition(rules, participant.Position, false)
                + (rules.FastestLapPoint && hasFastestLap && participant.Position <= rules.ScoringPositions ? 1 : 0)
                + (participant.IsPlayer && rules.PolePoint && playerQualifyingPosition == 1 ? 1 : 0);
            driver.Points += points; if (participant.Position == 1) driver.Wins++;
            var standing = career.Standings.FirstOrDefault(x => x.Driver == displayName);
            if (standing == null) { standing = new StandingEntry { Driver = displayName }; career.Standings.Add(standing); }
            standing.Races++; standing.Points += points; if (participant.Position == 1) standing.Wins++;
        }
        var player = classification.FirstOrDefault(x => x.IsPlayer);
        if (player != null)
        {
            var rival = classification.Where(x => !x.IsPlayer).OrderBy(x => Math.Abs(x.Position - player.Position)).FirstOrDefault();
            if (rival != null)
            {
                var item = career.Rivalries.FirstOrDefault(x => x.Rival == rival.Name);
                if (item == null) { item = new RivalryRecord { Rival = rival.Name }; career.Rivalries.Add(item); }
                var levelBefore = item.Level;
                item.Duels++;
                if (player.Position < rival.Position) item.PlayerWins++; else if (rival.Position < player.Position) item.RivalWins++;
                item.Level = Math.Clamp(item.Level + (Math.Abs(player.Position - rival.Position) <= 2 ? 8 : 3), 0, 100);
                var grandPrix = rounds[Math.Min(career.Round, rounds.Count - 1)].GrandPrix;
                item.Story = $"Duello a {grandPrix}: {career.Driver} contro {rival.Name}.";
                var duelHeadline = item.PlayerWins > item.RivalWins
                    ? $"Duello reale a {grandPrix}: {career.Driver} precede {rival.Name} nel referto Assetto Corsa."
                    : item.RivalWins > item.PlayerWins
                        ? $"Duello reale a {grandPrix}: {rival.Name} precede {career.Driver}; la rivalità resta aperta."
                        : $"Duello reale a {grandPrix}: {career.Driver} e {rival.Name} restano appaiati nel confronto del paddock.";
                career.News.Add(duelHeadline);
                career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "RIVALRY_DUEL", Headline = duelHeadline, Track = actualTrack, Importance = Math.Abs(player.Position - rival.Position) <= 2 ? 68 : 42 });
                if (levelBefore < 30 && item.Level >= 30)
                {
                    var escalation = $"La sfida con {rival.Name} diventa una rivalità: {item.Duels} duelli reali e livello {item.Level}/100.";
                    career.News.Add(escalation);
                    career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "RIVALRY_ESCALATION", Headline = escalation, Track = actualTrack, Importance = 82 });
                }
                var rivalDriver = career.Drivers.FirstOrDefault(x => x.Name == rival.Name);
                if (rivalDriver != null) rivalDriver.Relationship = item.Level >= 60 ? "forte antagonismo" : item.Level >= 30 ? "competitivo" : "neutrale";
            }
            var teammate = classification.FirstOrDefault(x => AiDriverIdentity.NamesEqual(x.Name, career.Teammate));
            if (teammate != null)
            {
                var teammateHeadline = player.Position < teammate.Position
                    ? $"Confronto interno a {rounds[Math.Min(career.Round, rounds.Count - 1)].GrandPrix}: {career.Driver} precede il compagno {teammate.Name}."
                    : player.Position > teammate.Position
                        ? $"Confronto interno a {rounds[Math.Min(career.Round, rounds.Count - 1)].GrandPrix}: il compagno {teammate.Name} precede {career.Driver}."
                        : $"Confronto interno a {rounds[Math.Min(career.Round, rounds.Count - 1)].GrandPrix}: {career.Driver} e {teammate.Name} chiudono appaiati.";
                career.News.Add(teammateHeadline);
                career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "TEAMMATE_DUEL", Headline = teammateHeadline, Track = actualTrack, Importance = Math.Abs(player.Position - teammate.Position) <= 1 ? 62 : 38 });
            }
        }
    }
    private string ArchiveLatestScreenshot(DateTime sinceUtc, int round, string track, DateTime? untilUtc = null)
    {
        try
        {
            var source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "screens");
            if (!Directory.Exists(source)) return "";
            var upperBound = untilUtc ?? DateTime.UtcNow.AddSeconds(2);
            var shot = Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories)
                .Where(x => new[] { ".png", ".jpg", ".jpeg", ".bmp" }.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
                .Select(x => new FileInfo(x))
                .Where(x => x.LastWriteTimeUtc >= sinceUtc.AddSeconds(-2) && x.LastWriteTimeUtc <= upperBound.AddSeconds(2))
                .OrderByDescending(x => x.LastWriteTimeUtc).FirstOrDefault();
            if (shot == null) return "";
            var destinationDir = Path.Combine(saveDir, "media", $"round-{round:00}-{track}"); Directory.CreateDirectory(destinationDir);
            var destination = Path.Combine(destinationDir, shot.Name); File.Copy(shot.FullName, destination, true);
            using var imageStream = File.OpenRead(destination);
            var sha256 = Convert.ToHexString(SHA256.HashData(imageStream));
            var metadata = new { source = shot.FullName, copiedUtc = DateTime.UtcNow, track, round, windowStartUtc = sinceUtc, windowEndUtc = upperBound, view = "visuale non dichiarata", sha256, kind = "ASSETTO_CORSA_SCREENSHOT" };
            File.WriteAllText(destination + ".meta.json", JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
            return destination;
        }
        catch { return ""; }
    }
    private string ArchiveResultSnapshot(string sourcePath, int round, string track, string mode)
    {
        try
        {
            if (!File.Exists(sourcePath)) return "";
            var directory = Path.Combine(saveDir, "media", "results"); Directory.CreateDirectory(directory);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var destination = Path.Combine(directory, $"round-{round:00}-{track}-{stamp}-race_out.json");
            File.Copy(sourcePath, destination, false);
            using var stream = File.OpenRead(destination);
            var hash = Convert.ToHexString(SHA256.HashData(stream));
            var metadata = new { source = sourcePath, archived = destination, copiedUtc = DateTime.UtcNow, sha256 = hash, mode, kind = "ASSETTO_CORSA_RESULT_SNAPSHOT" };
            File.WriteAllText(destination + ".meta.json", JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
            return destination;
        }
        catch { return ""; }
    }
    private static string FormatLap(int milliseconds) => $"{milliseconds / 60000:00}:{milliseconds / 1000 % 60:00}.{milliseconds % 1000:000}";
    /// <summary>
    /// La vettura da cui parte una carriera: il gradino piu basso della scala fra
    /// i contenuti installati, e a parita di gradino la meno potente. Serve a
    /// cominciare davvero dal fondo invece che dalla prima auto trovata.
    /// </summary>
    private ContentCarRecord? EntryLevelCar() =>
        contentIndex.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .OrderBy(x => CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step)
            .ThenBy(x => x.PowerHp == 0 ? int.MaxValue : x.PowerHp)
            .ThenBy(x => x.Name)
            .FirstOrDefault();

    private string[] InstalledCars()
    {
        return contentIndex.Cars.Count > 0 ? contentIndex.Cars.Select(x => x.Id).ToArray() : Array.Empty<string>();
    }
    private List<Round> BuildRounds()
    {
        var indexPath = Path.Combine(saveDir, "content-index.json");
        Dictionary<string, string>? overrides = null;
        var storedRoot = "";
        try
        {
            if (File.Exists(indexPath))
            {
                var previous = JsonSerializer.Deserialize<ContentIndexRecord>(File.ReadAllText(indexPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                overrides = previous?.CategoryOverrides;
                storedRoot = previous?.AssettoCorsaRoot ?? "";
            }
        }
        catch (Exception error) { CareerLog.Warn("contenuti", $"indice precedente non leggibile: {error.Message}"); }
        // CORSACAREER_AC_ROOT ha la precedenza su tutto: serve alle installazioni
        // fuori dai percorsi Steam standard e a poter provare il programma su una
        // libreria di contenuti separata.
        var forcedRoot = Environment.GetEnvironmentVariable("CORSACAREER_AC_ROOT") ?? "";
        var root = Directory.Exists(Path.Combine(forcedRoot, "content")) ? forcedRoot
            : Directory.Exists(Path.Combine(storedRoot, "content")) ? storedRoot
            : AssettoCorsaRoot();
        contentIndex = ContentScanner.Scan(root, overrides);
        try
        {
            Directory.CreateDirectory(saveDir);
            SaveContentIndex();
        }
        // Questi errori venivano inghiottiti in silenzio: una scansione non
        // salvata rendeva impossibile capire perché i contenuti "sparivano".
        catch (Exception error) { CareerLog.Error("contenuti", $"indice non salvato in {saveDir}", error); }
        CareerLog.Info("contenuti", $"scansione di {root}: {contentIndex.Cars.Count} auto, {contentIndex.Tracks.Count} circuiti, {contentIndex.Weathers.Count} meteo");
        // Il calendario non è più una proiezione della cartella dei contenuti: è
        // la vista dei round realmente presenti in agenda per questa stagione.
        // Durante la valutazione l'agenda non contiene round, e questo è corretto:
        // senza contratto non esiste un campionato.
        return ChampionshipRoundsView();
    }

    /// <summary>
    /// Ricostruisce l'agenda per una carriera salvata prima della sua
    /// introduzione. I round già disputati vengono marcati come conclusi con la
    /// data che hanno nello storico: la migrazione non sposta nessun risultato.
    /// </summary>
    private void MigrateScheduleFromHistory()
    {
        career.Schedule ??= new List<ScheduledEvent>();
        if (career.Schedule.Count > 0) return;
        if (career.RaceHistory.Count == 0 && career.TestHistory.Count == 0) return;

        foreach (var race in career.RaceHistory.OrderBy(x => x.Season).ThenBy(x => x.Round))
        {
            var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(race.Track, StringComparison.OrdinalIgnoreCase));
            CareerScheduler.Append(career.Schedule, new ScheduledEvent
            {
                Id = $"round-s{race.Season:00}-{race.Round:00}",
                Kind = ScheduledEventKind.ChampionshipRound,
                Date = race.StoryDate == default ? career.StoryDate : race.StoryDate,
                TrackId = race.Track,
                TrackName = track?.Name ?? race.Track,
                Country = track?.Country ?? career.Championship,
                Season = race.Season,
                Round = race.Round,
                Status = CareerScheduler.StatusDone,
                GeneratedBy = "Ricostruito dallo storico gare di una carriera precedente",
                Objective = "Round di campionato"
            });
        }
        foreach (var test in career.TestHistory.Select((x, index) => (x, index)))
        {
            var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(test.x.Track, StringComparison.OrdinalIgnoreCase));
            CareerScheduler.Append(career.Schedule, new ScheduledEvent
            {
                Id = $"eval-migrated-{test.index + 1}",
                Kind = ScheduledEventKind.EvaluationTest,
                Date = test.x.StoryDate == default ? career.StoryDate : test.x.StoryDate,
                TrackId = test.x.Track,
                TrackName = track?.Name ?? test.x.Track,
                Country = track?.Country ?? "",
                Season = career.Season,
                Status = CareerScheduler.StatusDone,
                GeneratedBy = "Ricostruito dallo storico test di una carriera precedente",
                Objective = "Prova di valutazione"
            });
        }
        CareerLog.Info("agenda", $"agenda ricostruita da {career.RaceHistory.Count} gare e {career.TestHistory.Count} test archiviati");
    }

    /// <summary>Round di campionato della stagione corrente, letti dall'agenda.</summary>
    private List<Round> ChampionshipRoundsView() =>
        CareerScheduler.ChampionshipRounds(career.Schedule ?? [], career.Season)
            .Select(x => new Round(
                string.IsNullOrWhiteSpace(x.TrackName) ? x.TrackId : x.TrackName,
                string.IsNullOrWhiteSpace(x.Country) ? career.Championship : x.Country,
                NarrativeCalendar.Format(x.Date),
                x.TrackId))
            .ToList();

    /// <summary>
    /// Prossimo appuntamento in agenda, qualunque sia il suo tipo. È la fonte per
    /// la testata e per i comandi: durante la valutazione è una prova, dopo la
    /// firma è un round di campionato.
    /// </summary>
    private ScheduledEvent? NextScheduled() => CareerScheduler.NextPlanned(career.Schedule ?? []);

    /// <summary>
    /// Vero se in questa stagione si è già corso almeno un round.
    ///
    /// È la condizione che distingue una firma «da subito» da una firma per
    /// l'anno prossimo. Una stagione cominciata non si azzera: il contatore
    /// dei round e il calendario restano dove sono, altrimenti ogni firma
    /// fa ripartire il campionato dal round 1 e la stagione non finisce mai —
    /// nel banco di prova otto anni di storia restavano «stagione 1», senza
    /// una classifica finale né un titolo.
    /// </summary>
    private bool StagioneGiaCominciata() => (career.Schedule ?? [])
        .Any(x => x.Kind == ScheduledEventKind.ChampionshipRound
                  && x.Season == career.Season && !x.IsPlanned);

    /// <summary>
    /// Circuito del prossimo appuntamento, con ripiego sul primo installato: serve
    /// dove il codice ha bisogno di una pista anche prima che l'agenda esista.
    /// </summary>
    private string NextTrackId()
    {
        var next = NextScheduled();
        if (next != null && !string.IsNullOrWhiteSpace(next.TrackId)) return next.TrackId;
        if (career.Round < rounds.Count) return rounds[career.Round].Track;
        return contentIndex.Tracks.FirstOrDefault()?.Id ?? "";
    }

    /// <summary>
    /// Garantisce che l'agenda contenga almeno il prossimo passo coerente con la
    /// fase di carriera. Non inventa un campionato: se non c'è un contratto,
    /// programma una prova.
    /// </summary>
    private void EnsureSchedule()
    {
        career.Schedule ??= new List<ScheduledEvent>();
        if (contentIndex.Tracks.Count == 0) return;
        var evaluation = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase);

        if (evaluation)
        {
            if (CareerScheduler.NextPlanned(career.Schedule) == null)
            {
                var start = career.StoryDate == default ? NarrativeCalendar.DefaultSeasonStart : career.StoryDate;
                if (CareerScheduler.Append(career.Schedule, CareerScheduler.EvaluationStart(contentIndex.Tracks, start, career.Season)))
                {
                    CareerLog.Info("agenda", "programmata la prima prova di valutazione");
                    SaveCareer(createVersionedBackup: false);
                }
            }
            rounds = ChampionshipRoundsView();
            return;
        }

        // Carriera attiva senza calendario: si genera quello della stagione.
        if (CareerScheduler.ChampionshipRounds(career.Schedule, career.Season).Count == 0)
            GenerateSeasonSchedule("Allineamento della carriera attiva");
        rounds = ChampionshipRoundsView();
    }

    /// <summary>Genera il calendario della stagione corrente e lo archivia in agenda.</summary>
    private void GenerateSeasonSchedule(string reason)
    {
        // Chi si e' ritirato non corre piu': niente calendari, niente offerte,
        // niente stagioni nuove. Senza questo la carriera ripartiva da sola il
        // giorno dopo il ritiro.
        if (career.Retired) return;
        career.Schedule ??= new List<ScheduledEvent>();
        if (contentIndex.Tracks.Count == 0) return;
        if (career.SeasonStartDate == default) career.SeasonStartDate = career.StoryDate == default ? NarrativeCalendar.DefaultSeasonStart : career.StoryDate;
        // Una firma può arrivare quando un calendario esiste già — per esempio
        // generato da una riparazione, o da un contratto precedente nella stessa
        // stagione. Gli ID dei round non cambiano, quindi Append li scartava
        // tutti e la firma non produceva NIENTE: nessuna gara nuova, nessun
        // annuncio. I round ancora da disputare della stagione corrente vengono
        // quindi rimossi e ricostruiti sulla categoria appena firmata; quelli già
        // corsi restano, perché sono storia.
        // Un calendario gia' in agenda per QUESTO campionato non va rifatto.
        //
        // Qui si cancellavano e ricreavano tutti i round non ancora disputati a
        // ogni firma, e la firma capita spesso. Ogni ricostruzione li rimetteva
        // in calendario partendo dalla data di inizio stagione aggiornata,
        // cioe' li spingeva piu' avanti nel tempo: nel collaudo i round di una
        // stagione finivano a cinquantasei giorni l'uno dall'altro invece di
        // ventuno, il pilota riempiva i buchi con gare open comprate a parte, e
        // la stagione non si chiudeva mai — tre anni fermi al primo campionato.
        // Si ricostruisce solo quando il campionato e' davvero cambiato.
        var siglaNuova = CareerScheduler.SeasonSignature(career.Championship, CategoryForCareer(career));
        var pianificati = career.Schedule
            .Where(x => x.Kind == ScheduledEventKind.ChampionshipRound
                && x.Season == career.Season && x.IsPlanned)
            .ToList();
        if (pianificati.Count > 0 && pianificati.All(x => x.Id.Contains(siglaNuova, StringComparison.OrdinalIgnoreCase)))
        {
            rounds = ChampionshipRoundsView();
            return;
        }
        // Una stagione gia' cominciata non cambia campionato a meta'.
        //
        // Firmando per una categoria diversa i round non ancora corsi venivano
        // sostituiti con quelli del campionato nuovo, ma dentro la STESSA
        // stagione: nel collaudo la stagione 2 contava ventotto gare corse fra
        // kart due tempi, kart 125 e Formula Vee, e si chiudeva con un titolo
        // «regionale» vinto cambiando vettura tre volte. Un campionato lo si
        // vince con una vettura sola.
        //
        // Il sedile nuovo resta firmato: comincia a correre dalla stagione
        // successiva, che e' come funziona davvero — di categoria si cambia
        // nella pausa invernale, non a luglio.
        var giaCorsiQuestaStagione = career.Schedule
            .Any(x => x.Kind == ScheduledEventKind.ChampionshipRound
                      && x.Season == career.Season && !x.IsPlanned);
        // Basta che questa stagione sia stata corsa: non serve che ne restino
        // ancora da correre.
        //
        // La condizione chiedeva anche round ancora in programma. Finito il
        // calendario — cioe' proprio nel momento in cui la stagione dovrebbe
        // chiudersi — la guardia non scattava piu': un cambio di campionato
        // generava un secondo calendario dentro la stessa annata, la stagione
        // non arrivava mai alla propria classifica e nel banco di prova la
        // «stagione 1» copriva tre anni di storia.
        if (giaCorsiQuestaStagione)
        {
            // Non basta rifiutarsi di generare: così il pilota resterebbe senza
            // calendario per sempre, perché una stagione poteva chiudersi solo
            // finendo una gara e gare in programma non ce n'erano più. Adesso
            // la chiusura è un passo richiamabile, quindi si chiude davvero
            // l'anno in corso — classifica, premio, verdetto, promozione o
            // retrocessione — e il campionato nuovo comincia dall'anno dopo.
            foreach (var rimasto in pianificati)
                CareerScheduler.Close(career.Schedule, rimasto.Id, cancelled: true);

            if (ChiudiStagioneSeCompleta())
            {
                career.Season++;
                career.Round = 0; career.Points = 0; career.Standings.Clear();
                career.SeasonStartDate = NarrativeCalendar.NextSeasonStart(
                    career.SeasonStartDate == default ? career.StoryDate : career.SeasonStartDate,
                    Math.Max(1, pianificati.Count));
                CareerLog.Info("agenda",
                    $"cambio di campionato: chiusa la stagione precedente, il nuovo calendario parte dalla stagione {career.Season}.");
                // Si prosegue: il calendario della stagione nuova va generato
                // adesso, con la categoria appena firmata.
            }
            else
            {
                // Non chiudibile — nessun round corso, o annata già archiviata.
                // Si rimettono in programma i round annullati invece di
                // lasciare l'agenda vuota.
                foreach (var rimasto in pianificati) rimasto.Status = CareerScheduler.StatusPlanned;
                CareerLog.Info("agenda",
                    $"{career.Championship}: stagione {career.Season} non chiudibile, calendario invariato.");
                rounds = ChampionshipRoundsView();
                return;
            }
        }
        // Un calendario non nasce nel passato.
        //
        // L'ancora della stagione restava quella della prima firma: un
        // campionato generato mesi dopo si ritrovava i primi round con date
        // gia' scadute — che il portale mostrava tutte insieme come «da
        // correre oggi» — e gli ultimi spinti in pieno inverno. Si sposta qui,
        // dove il calendario viene davvero costruito: farlo prima delle
        // guardie muoveva l'ancora anche quando non si generava niente, e la
        // stagione in corso si ritrovava spostata di un anno.
        if (career.SeasonStartDate.Date < career.StoryDate.Date)
            career.SeasonStartDate = NarrativeCalendar.SeasonStartAfterSigning(career.StoryDate);

        var stale = pianificati;
        var rebuilding = stale.Count > 0;
        foreach (var item in stale) career.Schedule.Remove(item);

        var generated = CareerScheduler.BuildSeason(contentIndex.Tracks, career.Tier, career.Season, career.SeasonStartDate, career.Championship, CategoryForCareer(career));
        var added = generated.Count(x => CareerScheduler.Append(career.Schedule, x));
        if (added == 0)
        {
            // Ripristina quello che c'era: meglio il calendario vecchio che nessuno.
            foreach (var item in stale) CareerScheduler.Append(career.Schedule, item);
            return;
        }
        if (rebuilding) CareerLog.Info("agenda", $"{stale.Count} round non disputati sostituiti con il calendario di {career.Championship}");
        rounds = ChampionshipRoundsView();
        SaveCareer(createVersionedBackup: false);
        var headline = $"Calendario {career.Championship} {career.Season}: {added} appuntamenti confermati.";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.SeasonStartDate, Type = "CALENDAR_PUBLISHED",
            Headline = headline, Track = career.Championship, Importance = 58
        });
        CareerLog.Info("agenda", $"{reason}: {added} round generati per la stagione {career.Season}");
    }
    /// <summary>
    /// Il riposo dei giorni realmente trascorsi.
    ///
    /// La stanchezza scendeva solo quando il giocatore contava i giorni a mano
    /// dall'agenda: un weekend che sposta il calendario di tre settimane non
    /// faceva recuperare niente, e il pilota correva sempre esausto — ottanta
    /// gare senza mai un podio. Qui si guarda quanti giorni sono passati
    /// davvero e si applica il recupero di quelle notti.
    /// </summary>
    /// <summary>
    /// Quello che un weekend lascia al pilota, comunque sia andato.
    ///
    /// Correre allena: prove, qualifica e gara mettono in macchina ore che
    /// contano. Prima la forma cambiava soltanto con le attivita scelte a mano,
    /// e chi correva e basta restava per sempre al valore di partenza: ottanta
    /// gare con la forma del primo giorno, un'abilita sempre appena sotto quella
    /// degli avversari, e nessuna vittoria possibile.
    ///
    /// Il guadagno e' piccolo e non sostituisce l'allenamento: serve a
    /// impedire che una carriera si fossilizzi.
    /// </summary>
    private void GrowFromRacing(bool finished, int position, int fieldSize)
    {
        var prima = career.Fitness;
        // Il mestiere si accumula comunque; arrivare in fondo vale di piu di
        // un ritiro.
        career.Fitness = Math.Clamp(career.Fitness + (finished ? 2 : 1), 0, 100);

        career.ReputationProfile ??= new ReputationProfile();
        var profilo = career.ReputationProfile;
        // Un weekend concluso e' professionalita dimostrata: la squadra impara
        // a fidarsi di chi porta a casa la macchina.
        if (finished)
        {
            profilo.TeamTrust = Math.Clamp(profilo.TeamTrust + 1, 0, 100);
            // La prima meta dello schieramento comincia a farsi notare.
            if (fieldSize > 1 && position > 0 && position <= fieldSize / 2)
                profilo.SportingPrestige = Math.Clamp(profilo.SportingPrestige + 1, 0, 100);
        }
        if (career.Fitness != prima)
            CareerLog.Info("pilota", $"weekend in macchina: forma da {prima} a {career.Fitness}");
    }

    private void RecoverForElapsedDays(DateTime primaDi)
    {
        if (primaDi == default || career.StoryDate <= primaDi) return;
        var notti = (career.StoryDate.Date - primaDi.Date).Days;
        if (notti <= 0) return;
        // Il recupero ha un tetto: due settimane di pausa rimettono in sesto,
        // sei mesi non danno un vantaggio.
        // E rallenta con gli anni: a quarant'anni un weekend si smaltisce in
        // quasi il doppio del tempo che serviva a venti. È il modo in cui l'età
        // si sente prima ancora che nel cronometro.
        var recupero = (int)Math.Round(Math.Min(notti, 21) * DayEngine.NightRecovery * DriverAge.FattoreRecupero(EtaPilota()));
        var prima = career.Fatigue;
        career.Fatigue = Math.Clamp(career.Fatigue - recupero, 0, OffTrackActivities.MaxFatigue);
        if (career.Fatigue != prima)
            CareerLog.Info("pilota", $"{notti} giorni di pausa: stanchezza da {prima} a {career.Fatigue}");
    }

    private DateTime SeasonAnchor() => career.SeasonStartDate != default
        ? career.SeasonStartDate
        : career.StoryDate != default ? career.StoryDate : NarrativeCalendar.DefaultSeasonStart;

    private void SaveContentIndex()
    {
        Directory.CreateDirectory(saveDir);
        File.WriteAllText(Path.Combine(saveDir, "content-index.json"), JsonSerializer.Serialize(contentIndex, new JsonSerializerOptions { WriteIndented = true }));
    }
    private string AssettoCorsaRoot()
    {
        var candidates = new List<string>
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "assettocorsa")
        };
        try
        {
            var vdf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf))
                foreach (Match match in Regex.Matches(File.ReadAllText(vdf), "\\\"path\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase))
                    candidates.Add(Path.Combine(match.Groups[1].Value.Replace("\\\\", "\\"), "steamapps", "common", "assettocorsa"));
        }
        catch { }
        return candidates.FirstOrDefault(root => Directory.Exists(Path.Combine(root, "content", "cars")) || Directory.Exists(Path.Combine(root, "content", "tracks"))) ?? candidates[0];
    }
    private static int CategoryRank(string category)
    {
        return (category ?? "").Trim().ToLowerInvariant() switch
        {
            "kart" => 1,
            "rookie" => 2,
            "cup" => 3,
            "touring" or "tcr" => 4,
            "gt4" or "formula 4" or "formula junior" => 5,
            "gt3" or "formula 3" or "gt2" or "prototype" => 6,
            "lmp" or "gt1" or "formula 2" => 7,
            "hypercar" or "formula" => 8,
            _ => 20
        };
    }
    private List<TeamOffer> BuildOffers()
    {
        // Le offerte partono dal gradino raggiunto, non dal fondo della scala.
        //
        // Qui si prendeva sempre la categoria d'ingresso: un pilota già in
        // Formula 3 riceveva offerte per il kart, le firmava e tornava
        // indietro. Insieme alle altre due strade era la causa per cui la
        // carriera rimbalzava fra due categorie per anni invece di salire.
        var gradinoOfferte = CareerLadder.Current(career, contentIndex.Cars).Step;
        var offerPool = contentIndex.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .Where(x =>
            {
                var passo = CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step;
                if (!CareerLadder.WithinReach(passo, gradinoOfferte, contentIndex.Cars)) return false;
                // La gavetta vale anche qui, ed era l'ultima strada che la
                // aggirava: al primo avvio le offerte comprendevano gia' il
                // gradino sopra e la scelta cadeva sul nome alfabeticamente
                // primo, cosi' il kart da noleggio non veniva mai corso e la
                // carriera cominciava direttamente sul due tempi.
                return passo <= gradinoOfferte
                       || RacesOnCurrentStep() >= OpportunityGenerator.RacesBeforeStepUp(gradinoOfferte);
            })
            .ToList();
        // Nessuna vettura entro un gradino: si resta su quelle del proprio, e
        // se non ce ne sono non si inventa un'offerta.
        if (offerPool.Count == 0)
            offerPool = contentIndex.Cars
                .Where(ContentCategoryRules.IsRaceable)
                .Where(x => CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step >= gradinoOfferte)
                .ToList();
        // E si resta sulla strada scelta al bivio.
        //
        // Le offerte del mercato la ignoravano: dopo il kart il pilota che
        // aveva scelto le monoposto riceveva un trofeo turismo, perché la
        // scelta cadeva sulla categoria di rango più basso fra le installate e
        // «cup» viene prima di «formula 4». Firmava, e la carriera cambiava
        // disciplina senza che nessuno l'avesse deciso.
        offerPool = CareerLadder.OnPath(offerPool, career.ChosenPath);
        if (offerPool.Count == 0) return new List<TeamOffer>();
        var entryCategory = offerPool.OrderBy(x => CategoryRank(x.Category)).ThenBy(x => x.Name).Select(x => x.Category).FirstOrDefault() ?? "special";
        var selected = offerPool.Where(x => x.Category.Equals(entryCategory, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Name).Select(x => x.Id).Take(3).ToArray();
        if (selected.Length == 0) return new List<TeamOffer>();
        var offers = new List<TeamOffer>();
        // Le squadre hanno un nome e un simbolo propri, presi dal catalogo del
        // paddock: prima erano tre nomi hardcoded senza identità visiva.
        var rung = CareerLadder.Current(career, contentIndex.Cars);
        var identities = TeamLogoCatalog.Pick(rung, selected.Length,
            StableHash.Of(career.Driver ?? "", career.Season));
        var teammates = new[] { "Kenta Ogawa", "Sota Fujimoto", "Mei Kanzaki" };
        // Nel kart si corre pagando la singola gara; piu in alto si firma per
        // una stagione. E' la differenza fra comprarsi un weekend e avere un
        // sedile.
        var clientLevel = rung.Path == LadderPath.Karting;
        // Tre proposte, tre caratteri diversi.
        //
        // Prima differivano soltanto per la quota, e la scelta era aritmetica:
        // vinceva sempre la piu' economica. Adesso una costa di piu' ma ti mette
        // in vetrina — e una vittoria li' vale il doppio in visibilita', quindi
        // in sponsor; un'altra costa poco e ha i meccanici bravi ma non ti fa
        // vedere da nessuno. Non esiste la squadra giusta: esiste quella giusta
        // per quello che ti serve adesso.
        var profili = TeamProfile.Assortimento(selected.Length,
            StableHash.Of(career.Driver ?? "", career.Season, rung.Step));
        for (var i = 0; i < selected.Length; i++)
        {
            var profilo = profili.Count > 0 ? profili[i % profili.Count] : TeamProfile.Neutro;
            var level = 20 + i * 12;
            var fee = (int)Math.Round((220 + i * 40) * profilo.CostoWeekend);
            var contentCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(selected[i], StringComparison.OrdinalIgnoreCase));
            var identity = identities.Count > 0 ? identities[i % identities.Count] : null;
            offers.Add(new TeamOffer
            {
                ProfileId = profilo.Id,
                Team = identity?.Name ?? (i == 0 ? "Shinjin Racing" : i == 1 ? "Kaido Motorsport" : "Minato Apex Racing"),
                Car = selected[i], Category = contentCar?.Category ?? "special",
                Sponsor = "In attesa di sponsor", Teammate = teammates[i % teammates.Length],
                // Un sedile cliente si paga a gara ed e' la condizione del kart.
                // Dalla prima categoria vera in su un contratto apre una
                // stagione: cablare IsClientSeat = true rendeva irraggiungibile
                // il ramo che genera il campionato, e nessuna firma faceva mai
                // nascere un calendario.
                Salary = clientLevel ? 0 : (int)Math.Round((4000 + i * 2000) * profilo.Premi),
                Years = clientLevel ? 0 : 1,
                Prestige = level,
                IsClientSeat = clientLevel,
                RaceFee = clientLevel ? fee : 0,
                // Il programma giovani e' l'unico che mette del suo: e' quello
                // che lo rende l'occasione migliore, e anche la piu' esigente.
                TeamSupportPercent = profilo.Id == "giovani" ? 45 : 0,
                PrizeP1 = (int)Math.Round(300 * profilo.Premi),
                PrizeP2 = (int)Math.Round(180 * profilo.Premi),
                PrizeP3 = (int)Math.Round(100 * profilo.Premi),
                PrizeP4P5 = (int)Math.Round(50 * profilo.Premi),
                Objective = i == 0
                    ? "Gara kart club: chiudere davanti almeno a metà gruppo"
                    : i == 1 ? "Gara kart regionale: dimostrare continuità"
                    : "Gara kart open: battere almeno un avversario diretto",
                Origin = $"{profilo.Nome.ToUpperInvariant()} · {profilo.Descrizione}\n{profilo.Compromesso}"
                         + (identity != null ? $"\n{identity.Motto}" : ""),
                Livery = contentCar?.Skins.FirstOrDefault() ?? ""
            });
        }
        return offers;
    }
    private void CreateProfile()
    {
        using var dialog = new ProfileDialog();
        dialog.ShowDialog(this);
        if (!dialog.WasSubmitted) return;
        career = new CareerState(); ApplyProfile(dialog.Profile); StartEvaluation(); EnsureSponsorProspects(); EnsurePaddockRoster(); SaveCareer();
        AnnounceDebutPhase();
        NarrationService.OpenCareerLaunchStories(career); career.LaunchStoriesOpened = true; SaveCareer();
        storiesJustOpened = true;
    }

    /// <summary>
    /// Mostra subito "CAPITOLO I · SI COMINCIA DA QUI" quando la carriera nasce.
    ///
    /// Prima la stessa scena arrivava solo al primo refresh della finestra
    /// principale (AnnouncePhaseIfNew in MainFormTimeline), cioè DOPO che il
    /// dossier del pilota era già stato aperto nel browser: una prefazione
    /// generica che spunta a metà di quella personale, invece di precederla.
    /// Qui l'ordine voluto è esplicito: prima l'ambientazione generale, poi il
    /// dossier del pilota, poi il portale.
    /// </summary>
    private void AnnounceDebutPhase()
    {
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") == "1") return;
        var phase = CareerPhases.Current(career);
        career.AnnouncedPhase = phase.Id;
        using var intro = new PhaseIntroDialog(phase);
        intro.ShowDialog(this);
        CareerLog.Info("fase", $"presentata la fase «{phase.Title}» ({phase.Id})");
        // Subito dopo l'ambientazione, le persone: i personaggi comparivano
        // man mano dicendo la loro senza che nessuno avesse spiegato chi
        // fossero, e la prima volta che Rei ti spiega una regola per il
        // giocatore e' una sconosciuta che da' ordini. Le stesse parole dette
        // da qualcuno che sai chi e' valgono un'altra cosa.
        using var presentazione = new CastIntroDialog(career.Driver ?? "pilota");
        presentazione.ShowDialog(this);
        CareerLog.Info("fase", $"presentata la compagnia: {CastDirector.Compagnia.Count} personaggi");
        // Conosciute le persone, la prima scena: il kart rimesso insieme e
        // nessuno che ci creda. E' l'inizio da cui tutto il resto si misura.
        RaccontaMomento(MomentoDiCarriera.PrimoGiorno, "primo-giorno");
    }
    private void EditProfile()
    {
        if (BlockIfPending("Profilo pilota")) return;
        using var dialog = new ProfileDialog(new DriverProfile
        {
            FirstName = career.FirstName,
            LastName = career.LastName,
            Nationality = career.Nationality,
            RaceNumber = career.RaceNumber,
            Nickname = career.Nickname,
            AvatarPath = career.AvatarPath,
            CareerMode = career.CareerMode
        });
        dialog.ShowDialog(this);
        if (!dialog.WasSubmitted) return;
        ApplyProfile(dialog.Profile);
        career.Headline = $"Profilo aggiornato: {career.Driver} conferma il proprio percorso nel paddock.";
        career.News.Add(career.Headline);
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "PROFILE_UPDATED", Headline = career.Headline, Track = "Paddock", Importance = 25 });
        SaveCareer(); RefreshUi();
    }
    private void ApplyProfile(DriverProfile profile)
    {
        career.FirstName = profile.FirstName; career.LastName = profile.LastName; career.Driver = profile.FullName; career.Nationality = profile.Nationality; career.RaceNumber = profile.RaceNumber; career.Nickname = profile.Nickname; career.CareerMode = profile.CareerMode;
        career.CareerPhase = "Evaluation"; career.EvaluationAttempts = 0; career.EvaluationBestLapMilliseconds = 0; career.EvaluationTargetMilliseconds = 125000; career.Team = "Senza contratto"; career.Sponsor = "In attesa di sponsor"; career.ContractActive = false; career.ContractYears = 0; career.ContractSalary = 0;
        career.AvatarPath = ArchiveDriverPortrait(profile.AvatarPath);
        // Le doti nascono col pilota e non cambiano più: due carriere condotte
        // allo stesso modo non danno più lo stesso risultato.
        career.Talent = DriverTalent.For(career.Driver);
        career.Headline = $"{career.Driver} entra nel paddock senza contratto e si prepara alla rookie evaluation.";
        career.News.Add($"{career.Driver} — {career.Talent.Archetype}. {career.Talent.Description}");
    }
    private void StartEvaluation()
    {
        var raceable = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).ToList();
        if (raceable.Count == 0)
        {
            career.Headline = $"{career.Driver} entra nel paddock senza contratto: in attesa di una vettura da test installata.";
            career.News.Add(career.Headline);
            return;
        }
        var first = EntryLevelCar() ?? raceable[0];
        career.Car = first.Id; career.Livery = first.Skins.FirstOrDefault() ?? ""; career.Tier = TierForCategory(first.Category); career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel);
        career.Team = "Senza contratto"; career.Sponsor = "In attesa di sponsor"; career.ContractActive = false; career.ContractObjective = "Supera la valutazione del paddock"; career.ContractObjectiveStatus = "In attesa";
        career.CareerPhase = "Evaluation";
        career.RookieEvaluationStatus = "Test iniziale da completare";
        RefreshEvaluationTarget();
        career.Headline = $"{career.Driver} comincia dalla rookie evaluation: primo test su {first.Name}, obiettivo {FormatLap(career.EvaluationTargetMilliseconds)}.";
        career.News.Add(career.Headline);
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "EVALUATION_STARTED", Headline = career.Headline, Track = "Paddock", Importance = 80 });

        // L'appuntamento nasce qui, non alla riapertura del programma.
        //
        // EnsureSchedule veniva chiamata solo da LoadCareer: chi creava un
        // profilo nuovo si trovava un'agenda vuota — nessuna prova, nessuna
        // gara, nessuna azione possibile — e la carriera era bloccata prima
        // ancora di cominciare.
        EnsureSchedule();
    }
    private string ArchiveDriverPortrait(string sourcePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) return sourcePath ?? "";
            var directory = Path.Combine(saveDir, "media", "profile"); Directory.CreateDirectory(directory);
            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || extension.Length > 5) extension = ".png";
            var destination = Path.Combine(directory, "driver-portrait" + extension);
            if (!Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) File.Copy(sourcePath, destination, true);
            var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(destination)));
            File.WriteAllText(destination + ".meta.json", JsonSerializer.Serialize(new { source = sourcePath, copiedUtc = DateTime.UtcNow, sha256 = hash, kind = "DRIVER_PORTRAIT_IMPORT" }, new JsonSerializerOptions { WriteIndented = true }));
            return destination;
        }
        catch { return sourcePath ?? ""; }
    }
    private void ChooseOffer()
    {
        career.Offers = BuildOffers();
        using var dialog = new OfferDialog(career.Offers);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedOffer == null) return;
        var offer = dialog.SelectedOffer;
        var initialTeam = career.Team;
        career.Team = offer.Team; career.Car = offer.Car; career.Livery = offer.Livery ?? ""; career.Tier = TierForCategory(offer.Category); career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel); career.Sponsor = offer.Sponsor; career.Teammate = offer.Teammate; career.ContractYears = offer.Years; career.ContractSalary = offer.Salary; career.ContractObjective = offer.Objective; career.ContractObjectiveStatus = "In corso"; career.ContractActive = true; career.CareerPhase = "Active"; career.SeatPrestige = offer.Prestige;
        // L'impegno chiesto deve parlare della categoria che si corre.
        AggiornaObiettivoDiContratto();
        // Il calendario deve essere costruito usando la categoria appena scelta.
        // Prima veniva generato mentre la carriera aveva ancora il vecchio Tier:
        // la firma andava a buon fine, ma in home non compariva nessun weekend.
        career.StoryDate = StoryStartDateForContent(contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(offer.Car, StringComparison.OrdinalIgnoreCase)));
        career.SeasonStartDate = NarrativeCalendar.SeasonStartAfterSigning(career.StoryDate);
        career.Round = 0;
        // Le prove/inviti della fase rookie non devono restare davanti al primo
        // round del nuovo contratto: altrimenti NextScheduled() continua a
        // restituire il vecchio test e la firma sembra non aver aperto il campionato.
        career.Schedule.RemoveAll(x => x.IsPlanned);
        GenerateSeasonSchedule("Firma del contratto");
        RegisterTeamChange(initialTeam, offer.Team, offer.Car, offer.Category, "Firma del primo contratto");
        career.Headline = $"{career.Driver} firma con {career.Team}. Il compagno sarà {career.Teammate}.";
        career.News.Add(career.Headline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CONTRACT_SIGNING", Headline = career.Headline, Track = "Paddock", Importance = 75 });
        // Il tema entra subito, sulla firma: aspettare il refresh avrebbe fatto
        // arrivare la musica dopo il momento che deve accompagnare.
        SoundtrackService.PlayForMood("firma");
        // Accettato un sedile, le altre proposte decadono. Prima restavano sul
        // tavolo e si potevano firmare una dopo l'altra: ogni firma cambiava
        // squadra e rigenerava il calendario, e la decisione non era mai
        // definitiva.
        career.Offers.Clear();
        SaveCareer();
        RestartHomeArtworkSequence();
        RefreshUi();
        AnnounceContractSigned(offer, offer.Category);
    }
    /// <summary>
    /// La gestione delle carriere: cominciarne una, riprenderne una, archiviare,
    /// cancellare.
    ///
    /// Le funzioni c'erano tutte ma erano sei pulsanti piccoli dentro un
    /// pannello secondario, in mezzo alle tavole del manga: per chi gioca non
    /// esistevano. Adesso hanno una schermata loro, raggiungibile dalla barra.
    /// </summary>
    private void OpenCareerManager()
    {
        if (BlockIfPending("La gestione delle carriere")) return;
        using var dialog = new CareerManagerDialog(Path.Combine(saveDir, "careers"), career);
        if (dialog.ShowDialog(this) != DialogResult.OK) { RefreshUi(); return; }

        switch (dialog.Azione)
        {
            case CareerManagerAction.Nuova:
                // La copia di sicurezza si fa sempre: da qui il giocatore ha
                // gia' confermato, e perdere una carriera per un clic sarebbe
                // il danno peggiore che questa schermata possa fare.
                if (career.Races > 0 || (career.RaceHistory?.Count ?? 0) > 0) SaveCareerSlotCopy();
                CreateProfileAndStart();
                break;

            case CareerManagerAction.Riprendi when dialog.Scelta != null:
                if (career.Races > 0 || (career.RaceHistory?.Count ?? 0) > 0) SaveCareerSlotCopy();
                AdottaCarriera(dialog.Scelta);
                break;

            case CareerManagerAction.Archivia:
                SaveCareerSlotCopy(dialog.NomeArchivio);
                CareerMessages.Show(this,
                    $"Copia archiviata come «{dialog.NomeArchivio}». La carriera in corso continua da dove eravamo.",
                    "Gestione carriere");
                break;

            case CareerManagerAction.EliminaCorrente:
                // La copia, se il giocatore l'ha chiesta, si fa PRIMA di
                // toccare il salvataggio: se qualcosa va storto nell'archivio
                // meglio ritrovarsi la carriera ancora al suo posto.
                if (!string.IsNullOrWhiteSpace(dialog.NomeArchivio) && !SaveCareerSlotCopy(dialog.NomeArchivio))
                {
                    CareerMessages.Show(this,
                        "La copia non è riuscita, quindi la carriera non è stata eliminata.",
                        "Gestione carriere", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }
                EliminaCarrieraCorrente();
                CreateProfileAndStart();
                break;
        }
        RefreshUi();
    }

    /// <summary>
    /// Rende corrente una carriera archiviata, riallineando quello che dipende
    /// dai contenuti installati: una carriera ripresa deve trovare le sue
    /// vetture e il suo calendario, non ripartire da un mondo diverso.
    /// </summary>
    private void AdottaCarriera(CareerState stato)
    {
        career = stato;
        career.Results ??= []; career.News ??= []; career.Events ??= []; career.RaceHistory ??= [];
        career.Drivers ??= []; career.Standings ??= []; career.Rivalries ??= []; career.SeasonArchive ??= [];
        career.Offers ??= []; career.StoryArcs ??= []; career.TeamHistory ??= []; career.Schedule ??= [];
        career.BriefingFatti ??= [];
        career.Journalist ??= new JournalistProfile();
        career.Livery ??= "";
        EnsureSponsorProspects();
        EnsurePaddockRoster();
        rounds = ChampionshipRoundsView();
        SaveCareer();
        CareerLog.Info("carriera", $"ripresa la carriera di {career.Driver}: {career.Races} gare, stagione {career.Season}");
        CareerMessages.Show(this,
            $"Bentornato: carriera di {career.Driver} ripresa a {career.Races} gare, stagione {career.Season}, {career.Championship}.",
            "Gestione carriere");
    }

    /// <summary>
    /// Cancella il salvataggio della carriera in corso e riporta lo stato a
    /// vuoto. Non tocca l'archivio: quelle messe da parte restano.
    /// </summary>
    private void EliminaCarrieraCorrente()
    {
        var chi = career.Driver;
        var gare = career.Races;
        foreach (var file in new[] { "career.json", "career.json.bak", "career.json.tmp", "pending_weekend.json" })
        {
            var percorso = Path.Combine(saveDir, file);
            try { if (File.Exists(percorso)) File.Delete(percorso); }
            catch (Exception errore) { CareerLog.Warn("carriera", $"{file} non cancellato: {errore.Message}"); }
        }
        career = new CareerState();
        rounds = [];
        awaitingResult = false;
        CareerLog.Info("carriera", $"eliminata la carriera di {chi} ({gare} gare)");
    }

    /// <summary>Il percorso di creazione, condiviso fra il menu e la gestione carriere.</summary>
    private void CreateProfileAndStart()
    {
        using var dialog = new ProfileDialog();
        dialog.ShowDialog(this);
        if (!dialog.WasSubmitted) return;
        career = new CareerState(); ApplyProfile(dialog.Profile); StartEvaluation();
        EnsureSponsorProspects(); EnsurePaddockRoster(); SaveCareer();
        AnnounceDebutPhase();
        NarrationService.OpenCareerLaunchStories(career); career.LaunchStoriesOpened = true; SaveCareer();
        storiesJustOpened = true; RefreshUi();
    }

    private void NewCareer()
    {
        if (BlockIfPending("Nuova carriera")) return;
        if (career.Races > 0 || career.Events.Count > 0 || career.SeasonArchive.Count > 0)
        {
            var answer = CareerMessages.Ask(null, "La carriera corrente contiene dati. Vuoi salvarne una copia prima di crearne una nuova?\n\nSì = salva e continua\nNo = continua senza copia\nAnnulla = resta nella carriera corrente", "Nuova carriera", MessageBoxButtons.YesNoCancel, DialogResult.Cancel);
            if (answer == DialogResult.Cancel) return;
            if (answer == DialogResult.Yes && !SaveCareerSlotCopy()) return;
        }
        using var dialog = new ProfileDialog();
        dialog.ShowDialog(this);
        if (!dialog.WasSubmitted) return;
        career = new CareerState(); ApplyProfile(dialog.Profile); StartEvaluation(); EnsureSponsorProspects(); EnsurePaddockRoster(); SaveCareer();
        AnnounceDebutPhase();
        NarrationService.OpenCareerLaunchStories(career); career.LaunchStoriesOpened = true; SaveCareer();
        storiesJustOpened = true; RefreshUi();
    }
    /// <param name="nome">
    /// Nome scelto dal giocatore. Vuoto: se ne costruisce uno con il pilota e
    /// la data, che e' il comportamento di quando si archivia in automatico.
    /// </param>
    private bool SaveCareerSlotCopy(string nome = "")
    {
        try
        {
            var directory = Path.Combine(saveDir, "careers"); Directory.CreateDirectory(directory);
            var safeDriver = string.Join("_", career.Driver.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrWhiteSpace(safeDriver)) safeDriver = "carriera";
            var etichetta = string.IsNullOrWhiteSpace(nome)
                ? $"{safeDriver}_{career.Races}gare_{DateTime.Now:yyyyMMdd_HHmmss}"
                : string.Join("_", nome.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            var path = Path.Combine(directory, $"{etichetta}.json");
            File.WriteAllText(path, JsonSerializer.Serialize(career, new JsonSerializerOptions { WriteIndented = true }));
            CareerLog.Info("carriera", $"copia archiviata: {Path.GetFileName(path)}");
            return true;
        }
        catch (Exception error)
        {
            CareerMessages.Show(null, $"Impossibile salvare la copia: {error.Message}", "Gestione carriere", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
    private void Briefing()
    {
        // La sede della prova viene dall'agenda, non dal calendario: durante la
        // valutazione un calendario di campionato non esiste.
        var scheduled = NextScheduled();
        if (scheduled?.Kind == ScheduledEventKind.Invitation)
        {
            var fee = InvitationEntryFee(scheduled);
            var proposer = string.IsNullOrWhiteSpace(scheduled.ProposedBy) ? "Paddock Club Kanto" : scheduled.ProposedBy;
            CareerMessages.Show(null, $"PROPOSTA DI GARA\n\n{proposer} propone a {career.Driver} una gara su invito a {scheduled.TrackName}. Non è un test: ci sono griglia, avversari, qualifica e un risultato reale.\n\nCosto d'iscrizione: € {fee:N0}\nBudget disponibile: € {career.Cash:N0}\nObiettivo: {scheduled.Objective}.\n\nPuoi accettare dal pulsante rosso, oppure rifiutare e usare agenda e mercato per prepararti.", "CorsaCareer — proposta del paddock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var trackId = NextTrackId();
        if (string.IsNullOrWhiteSpace(trackId))
        {
            CareerMessages.Show(null, "Non c'è nessun appuntamento in agenda e non risultano circuiti installati.", "CorsaCareer — agenda vuota", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var round = new Round(
            scheduled != null && !string.IsNullOrWhiteSpace(scheduled.TrackName) ? scheduled.TrackName : trackId,
            scheduled?.Country ?? career.Championship,
            scheduled != null ? NarrativeCalendar.Format(scheduled.Date) : NarrativeCalendar.Format(career.StoryDate),
            trackId);
        // L'obiettivo dipende dal circuito di questo round: va ricalcolato prima
        // di annunciarlo, non letto da una costante.
        var evaluationTarget = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase) ? RefreshEvaluationTarget() : null;
        if (!career.Events.Any(x => x.Type == "TEST_BRIEFING" && x.Track.Equals(round.Track, StringComparison.OrdinalIgnoreCase)))
        {
            career.Headline = career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase)
                ? $"Rookie evaluation a {round.GrandPrix}: {career.Driver} deve avvicinarsi a {FormatLap(career.EvaluationTargetMilliseconds)} per convincere il paddock."
                : $"Briefing {round.GrandPrix}: il team chiede una gara pulita. Tre run di test disponibili prima della qualifica.";
            career.News.Add(career.Headline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "TEST_BRIEFING", Headline = career.Headline, Track = round.Track, Importance = 35 });
            SaveCareer(); RefreshUi();
        }
        var evaluationBriefing = evaluationTarget == null
            ? "Il test non assegna punti e non avanza il campionato."
            : $"Obiettivo rookie evaluation: {FormatLap(career.EvaluationTargetMilliseconds)} o meglio.\nRiferimento: {evaluationTarget.Basis}.\n{string.Join("\n", evaluationTarget.Notes.Select(x => "• " + x))}\nIl risultato decide se arriveranno altre prove o un primo contratto.";
        // Il test parte: il giocatore ha gia deciso premendo il pulsante.
        //
        // Prima qui si apriva una seconda domanda, e rispondendo no la
        // carriera restava ferma senza che nulla lo spiegasse. Il briefing
        // sopra racconta obiettivo e posta in gioco: dopo averlo letto si
        // scende in pista.
        if (!awaitingResult) LaunchTestSession();
    }

    private int InvitationEntryFee(ScheduledEvent invitation)
    {
        // La quota fissata nell'appuntamento e' gia il totale a carico del
        // pilota: chi programma l'invito sa in che momento della carriera
        // arriva e sceglie una cifra sostenibile — il debutto costa 300 euro
        // proprio perche' si parte con 900.
        //
        // Prima qui si sommava la logistica di EconomyEngine, e la prima gara
        // veniva a costare 2800 euro: impossibile da pagare, con nessun altro
        // modo di guadagnare in agenda. La carriera si fermava al debutto.
        if (invitation.EntryFee > 0) return invitation.EntryFee + TravelCost(invitation);

        // Senza una quota dichiarata si stima dal costo reale del weekend.
        var logistics = EconomyEngine.ForRace(new RaceEconomyInput
        {
            Tier = career.Tier, LadderStep = CareerLadder.Current(career, contentIndex.Cars).Step, EmployedDriver = career.ContractActive && !career.IsClientDriver, Position = 0, FieldSize = 1, Dnf = true,
            RoundsInSeason = 1, Endurance = false
        }).LogisticsCost;
        return logistics;
    }

    /// <summary>
    /// Quanto costa arrivare fino al circuito.
    ///
    /// Il preventivo mostrava sempre «trasferta e organizzazione: € 0», perché
    /// la quota dichiarata nell'appuntamento era già il totale e non restava
    /// niente da attribuire al viaggio. Andare a correre da un'altra parte del
    /// paese costa qualcosa, e deve pesare sulla decisione: qui la trasferta è
    /// una voce vera, proporzionata alla quota — quindi cresce con la
    /// categoria — ma abbastanza contenuta da non rendere impagabile il
    /// debutto, che resta il vincolo da cui è nata la quota unica.
    /// </summary>
    private static int TravelCost(ScheduledEvent invitation)
    {
        if (invitation.EntryFee <= 0) return 0;
        var stimato = (int)Math.Round(invitation.EntryFee * 0.35 / 10) * 10;
        return Math.Max(30, stimato);
    }

    private void DeclineInvitation()
    {
        var invitation = NextScheduled();
        if (invitation?.Kind != ScheduledEventKind.Invitation) return;
        var answer = CareerMessages.Ask(null, $"Rinunciare alla gara su invito di {invitation.TrackName}?\n\nNon verrà addebitato alcun costo. L'invito sarà archiviato e il paddock proporrà un test di recupero; potrai comunque lavorare con agenda, sponsor e mercato.", "CorsaCareer — rinuncia all'invito", MessageBoxButtons.YesNo, DialogResult.No);
        if (answer != DialogResult.Yes) return;
        CareerScheduler.Close(career.Schedule, invitation.Id, cancelled: true);
        var recovery = new ScheduledEvent
        {
            Id = $"declined-recovery-s{career.Season:00}-{career.EvaluationAttempts + 1}-{DateTime.UtcNow:HHmmss}",
            Kind = ScheduledEventKind.EvaluationTest, Date = invitation.Date.AddDays(12),
            TrackId = invitation.TrackId, TrackName = invitation.TrackName, Country = invitation.Country, Season = career.Season,
            GeneratedBy = "Invito rifiutato: il pilota privilegia preparazione, sponsor e un nuovo test",
            Objective = "Costruire un riferimento più solido prima della prossima proposta"
        };
        CareerScheduler.Append(career.Schedule, recovery);
        career.StoryDate = invitation.Date;
        career.RookieEvaluationStatus = "Invito rifiutato — ritorno al lavoro";
        career.Headline = $"{career.Driver} rinuncia alla gara su invito di {invitation.TrackName}: prima di rischiare il budget, sceglie test e preparazione.";
        career.News.Add(career.Headline);
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "INVITATION_DECLINED", Headline = career.Headline, Track = invitation.TrackId, Importance = 58 });
        SaveCareer(); RefreshUi();
        OpenCareerArticle(career.Events.LastOrDefault(x => x.Type == "INVITATION_DECLINED"));
    }
    private void LaunchTestSession()
    {
        var uiAutomation = Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") == "1";
        if (awaitingResult) return;
        var raceableCars = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).ToList();
        if (raceableCars.Count == 0)
        {
            CareerMessages.Show(null, "Nessuna auto da competizione installata per il test. Le auto stradali non possono essere usate come sessione di carriera.", "CorsaCareer — contenuti mancanti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var scheduled = NextScheduled();
        var round = new Round(
            scheduled != null && !string.IsNullOrWhiteSpace(scheduled.TrackName) ? scheduled.TrackName : NextTrackId(),
            scheduled?.Country ?? career.Championship,
            scheduled != null ? NarrativeCalendar.Format(scheduled.Date) : NarrativeCalendar.Format(career.StoryDate),
            NextTrackId());
        var track = contentIndex.Tracks.Any(x => x.Id.Equals(round.Track, StringComparison.OrdinalIgnoreCase)) ? round.Track : contentIndex.Tracks.FirstOrDefault()?.Id ?? "";
        if (track.Length == 0) { CareerMessages.Show(null, "Nessun circuito installato per il test.", "CorsaCareer — test", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var testCar = raceableCars.Any(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase)) ? career.Car : raceableCars[0].Id;
        // Il test usa lo stesso pianificatore della gara: cambia solo il formato
        // (prove libere, nessuna qualifica, nessun avversario obbligatorio).
        var plan = BuildSessionPlan(track, testCar, mixedGrid: false, testSession: true);
        var presetJson = ContentManagerPresetBuilder.BuildTest(testCar, track, plan);
        var presetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Presets", "Quick Drive"); Directory.CreateDirectory(presetDir);
        var presetPath = Path.Combine(presetDir, $"CorsaCareer - Test S{career.Season:00} GP {career.Round + 1:00} {round.GrandPrix}.cmpreset");
        File.WriteAllText(presetPath, presetJson);
        if (!ContentManagerPresetValidator.TryValidate(presetPath, out var testPresetError))
        {
            CareerMessages.Show(null, $"Il preset del test non ha superato il controllo: {testPresetError}.\n\nNessuna sessione è stata registrata.", "CorsaCareer - preset non valido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        pendingMode = "test"; awaitingResult = true; launchTimeUtc = DateTime.UtcNow;
        var resultFile = AssettoCorsaResultLocator.FindLatestExisting();
        var resultHashBeforeLaunch = HashFile(resultFile); pendingResultHash = resultHashBeforeLaunch;
        ArchiveSessionPlan(plan, "test", track, testCar);
        var pending = new { mode = "test", round = career.Round + 1, grandPrix = round.GrandPrix, country = round.Country, date = round.Date, car = testCar, track, preset = presetPath, launchUtc = launchTimeUtc, resultHashBeforeLaunch, driver = career.Driver, team = career.Team, format = plan.FormatLabel, weather = plan.WeatherId, temperature = plan.TemperatureC, timeOfDay = plan.TimeOfDaySeconds };
        File.WriteAllText(Path.Combine(saveDir, "pending_weekend.json"), JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true }));
        // Il banco UI automatico verifica la preparazione e la persistenza,
        // ma non deve scandire collegamenti Windows o aprire Content Manager.
        // Nel gioco normale il percorso reale viene comunque cercato subito dopo.
        if (uiAutomation) return;
        var cmPath = LocateContentManager();
        if (string.IsNullOrWhiteSpace(cmPath))
        {
            // Assetto Corsa non c e su questo computer: la sessione viene
            // risolta dal programma e la carriera prosegue. Prima ci si
            // fermava qui con un avviso, e serviva un secondo pulsante per
            // sbloccare la situazione.
            CareerLog.Info("sessione", "Content Manager non installato: la sessione viene risolta dal programma.");
            ResolvePendingSessionWithoutAssettoCorsa();
            return;
        }
        try
        {
            OpenContentManagerPreset(cmPath, presetPath);
            // La conferma modale restava dietro a Content Manager e poteva
            // riapparire quando il test era già finito. Lo stato della
            // sessione vive invece nel banner del portale e si aggiorna da sé.
            RefreshUi();
            if (saveStatus != null) saveStatus.Text = "Sessione di test aperta in Content Manager · in attesa del referto Assetto Corsa.";
        }
        catch (Exception error) { CareerMessages.Show(null, $"Impossibile aprire il test: {error.Message}", "CorsaCareer — test", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private void OpenMarket()
    {
        if (BlockIfPending("Mercato piloti")) return;
        NarrateCardEssentials("market|" + career.StoryDate.ToString("O"), $"Mercato piloti. {career.Offers?.Count ?? 0} offerte disponibili. La tua reputazione è {career.Reputation} su 100 e la cassa è di {career.Cash} euro. Seleziona un sedile per confrontare auto, stipendio, durata e obiettivi.");
        using var dialog = new MarketDialog(career);
        var marketResult = dialog.ShowDialog(this);
        // «Firmo e non succede niente» e' il difetto piu' segnalato di tutto il
        // programma, e ogni volta e' stata una causa diversa. Senza una riga di
        // registro qui non si puo' sapere quale: il mercato usciva in silenzio
        // in tre punti distinti.
        CareerLog.Info("mercato",
            $"chiusura mercato: esito={marketResult}, sedile={(dialog.SelectedOffer == null ? "nessuno" : dialog.SelectedOffer.Team + " · " + dialog.SelectedOffer.Car)}, sponsor cambiato={dialog.SponsorChanged}");
        if (dialog.SponsorChanged)
        {
            SaveCareer(); RefreshUi();
            if (marketResult != DialogResult.OK || dialog.SelectedOffer == null) return;
        }
        if (marketResult != DialogResult.OK || dialog.SelectedOffer == null) return;
        SignOffer(dialog.SelectedOffer);
    }

    /// <summary>
    /// Applica la firma di un sedile.
    ///
    /// Era dentro OpenMarket, dopo l'apertura del dialogo: non si poteva
    /// eseguire senza interazione, e quindi non si poteva verificare che
    /// producesse davvero un calendario. Il difetto piu grave del programma —
    /// "firmo e non parte nessuna gara" — viveva proprio qui, al riparo da ogni
    /// collaudo.
    /// </summary>
    private void SignOffer(TeamOffer offer)
    {
        CareerLog.Info("mercato", $"firma richiesta: {offer.Team} · {offer.Car} · prestigio {offer.Prestige} · cliente {offer.IsClientSeat}");
        if (offer.Prestige > career.Reputation + 25)
        {
            CareerLog.Info("mercato", $"firma rifiutata: prestigio {offer.Prestige} oltre la reputazione {career.Reputation}+25.");
            CareerMessages.Show(this, $"{offer.Team} non è ancora convinta. Reputazione richiesta: {Math.Max(0, offer.Prestige - 25)}.", "Mercato piloti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var installed = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(offer.Car, StringComparison.OrdinalIgnoreCase));
        if (installed == null || !ContentCategoryRules.IsRaceable(installed))
        {
            CareerLog.Info("mercato", $"firma rifiutata: l'auto {offer.Car} non e' fra i contenuti da gara.");
            CareerMessages.Show(this, $"L'offerta di {offer.Team} non è più valida: l'auto {UiText.Car(offer.Car)} non risulta installata come contenuto da gara. Aggiorna i contenuti e riprova.", "Mercato piloti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var previousTeam = career.Team;
        var offerCategory = string.IsNullOrWhiteSpace(offer.Category) ? installed.Category : offer.Category;
        offer.Category = offerCategory;
        career.Team = offer.Team; career.Car = offer.Car; career.Livery = offer.Livery ?? ""; career.Tier = TierForCategory(offerCategory); career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel); career.Teammate = offer.Teammate; career.SeatPrestige = offer.Prestige;
        // Il carattere della squadra segue il sedile: da qui in poi decide
        // quanto vale una vittoria in visibilita' e quanto rendono i premi.
        career.TeamProfileId = offer.ProfileId;
        if (offer.IsClientSeat)
        {
            career.ContractYears = 0; career.ContractSalary = 0; career.ContractActive = false; career.IsClientDriver = true;
            career.TeamSupportPercent = offer.TeamSupportPercent; career.Sponsor = "In attesa di sponsor";
            career.ContractObjective = offer.Objective; career.ContractObjectiveStatus = "In corso"; career.CareerPhase = "Active";
            // Il sedile cliente non compra una stagione né una gara invisibile:
            // apre una rosa di weekend coerenti con vettura e categoria. La quota
            // viene pagata soltanto scegliendo UNA gara nella schermata Opportunità.
            // Si azzerano solo gli appuntamenti della vecchia condizione, non
            // il calendario intero: le gare appena aperte devono restare.
            //
            // E nemmeno gli inviti gia ricevuti: IsPlanned prendeva tutto,
            // compresa una gara che il paddock aveva offerto e che il pilota
            // poteva ancora voler correre. Un sedile cliente aggiunge occasioni,
            // non le cancella. Spariscono solo le prove di valutazione, che con
            // un sedile in mano non hanno piu senso.
            career.Schedule.RemoveAll(x => x.IsPlanned && x.IsTest);
            var clientChoices = CreateClientRaceChoices(offer, offerCategory);
            CareerLog.Info("mercato", $"sedile cliente firmato con {offer.Team}: {clientChoices} gare aperte alla scelta.");
            // Le gare aperte da un sedile cliente devono comparire nel
            // calendario, non soltanto fra le opportunita: e' li che il
            // giocatore le cerca, e non trovandole la carriera sembra bloccata.
            PublishClientRacesToCalendar(offer);
            career.Headline = $"{career.Driver} entra da cliente in {career.Team}: {clientChoices} gare di {offerCategory} attendono una scelta.";
            career.News.Add(career.Headline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CLIENT_SEAT_SELECTED", Headline = career.Headline, Track = career.Team, Importance = 72 });
            career.Offers ??= [];
            // Accettato un sedile, le altre proposte decadono: prima restavano
            // e si poteva firmare con una squadra dopo l'altra.
            career.Offers.Clear(); SaveCareer(); RestartHomeArtworkSequence();
            // Nei banchi UI la finestra è nascosta fuori schermo: il refresh
            // completo avvierebbe timer/audio e non aggiunge alcuna verifica
            // allo stato persistito. L'app reale aggiorna normalmente la home.
            if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") RefreshUi();
            if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") SoundtrackService.PlayForMood("firma");
            // Il benvenuto viene prima della scelta del weekend: e' il momento
            // della firma, e senza di esso il sedile cliente — cioe' il primo
            // di tutta la carriera — non diceva assolutamente niente.
            AnnounceSeatSigned(offer, offerCategory, clientChoices);
            BriefingDiApertura();
            OpenClientRaceChoices();
            return;
        }
        career.Sponsor = offer.Sponsor; career.ContractYears = offer.Years; career.ContractSalary = offer.Salary; career.ContractObjective = offer.Objective; career.ContractObjectiveStatus = "In corso"; career.ContractActive = true; career.IsClientDriver = false; career.TeamSupportPercent = 100; career.CareerPhase = "Active";
        // L'impegno chiesto deve parlare della categoria che si corre.
        AggiornaObiettivoDiContratto();
        // Il calendario del campionato nasce qui: prima della firma non esiste.
        // Aggiorniamo anche l'ancora temporale prima di generare gli eventi:
        // così il primo round è coerente con il nuovo campionato e non con la
        // vecchia fase di valutazione.
        if (career.StoryDate == default) career.StoryDate = StoryStartDateForContent(installed);
        if (StagioneGiaCominciata())
        {
            // Si firma a stagione in corso: il campionato che si sta correndo
            // resta quello, il sedile nuovo vale dall'anno prossimo. È come
            // funziona davvero, ed è l'unico modo perché una stagione arrivi
            // alla propria classifica finale.
            var rinvio = $"{career.Driver} firma con {career.Team}: si comincia dalla stagione {career.Season + 1}. La stagione in corso si chiude con il sedile attuale.";
            career.News.Add(rinvio);
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SEAT_SIGNED_NEXT_SEASON",
                Headline = rinvio, Track = career.Team, Importance = 76
            });
            CareerLog.Info("mercato", $"contratto firmato a stagione in corso: parte dalla stagione {career.Season + 1}.");
        }
        else
        {
            career.SeasonStartDate = NarrativeCalendar.SeasonStartAfterSigning(career.StoryDate);
            career.Round = 0;
            GenerateSeasonSchedule("Firma del contratto");
        }
        var player = career.Drivers.FirstOrDefault(x => x.Name.Equals(career.Driver, StringComparison.OrdinalIgnoreCase));
        if (player != null) { player.Team = career.Team; player.Car = career.Car; player.Category = offerCategory; player.Relationship = "pilota"; }
        foreach (var formerTeammate in career.Drivers.Where(x => x.Relationship.Equals("compagno di squadra", StringComparison.OrdinalIgnoreCase) && !x.Name.Equals(career.Teammate, StringComparison.OrdinalIgnoreCase)))
        {
            formerTeammate.Relationship = "ex compagno";
            formerTeammate.Team = previousTeam;
        }
        var teammate = career.Drivers.FirstOrDefault(x => x.Name.Equals(career.Teammate, StringComparison.OrdinalIgnoreCase));
        if (teammate != null) { teammate.Team = career.Team; teammate.Car = career.Car; teammate.Category = offerCategory; teammate.Relationship = "compagno di squadra"; }
        else AddRosterDriver(career.Teammate, "Italia", career.Team, career.Car, offerCategory, "compagno di squadra", 72, "gara");
        career.Offers ??= [];
        career.Offers.Remove(offer);
        RegisterTeamChange(previousTeam, offer.Team, offer.Car, offerCategory, "Scelta dal mercato piloti");
        career.Headline = $"{career.Driver} cambia squadra: nuova sfida con {career.Team}.";
        career.News ??= new List<string>(); career.News.Add(career.Headline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "TEAM_CHANGE", Headline = career.Headline, Track = "Mercato piloti", Importance = 70 });
        // Anche il cambio squadra e' una firma: il tema entra sul momento.
        SoundtrackService.PlayForMood("firma");
        SaveCareer();
        // La tavola della home segue lo stato: dopo una firma la sequenza va
        // ricalcolata, altrimenti resta quella del pilota senza contratto.
        RestartHomeArtworkSequence();
        RefreshUi();
        AnnounceContractSigned(offer, offerCategory);
    }

    /// <summary>
    /// Il riconoscimento della firma. Non è una cortesia: è il punto in cui il
    /// giocatore deve vedere che cosa è cambiato davvero — calendario, cassa,
    /// obiettivo — perché prima la firma non produceva alcun segnale visibile e
    /// sembrava che il pulsante non funzionasse.
    /// </summary>
    /// <summary>
    /// Un team cliente non attiva un campionato fantasma e non addebita una
    /// quota prima che il pilota abbia scelto dove correre. Produce invece una
    /// rosa corta di gare della stessa categoria e con la stessa vettura.
    /// </summary>
    private int CreateClientRaceChoices(TeamOffer offer, string category)
    {
        career.Opportunities ??= [];
        var prefix = $"client-race-{career.Season}-{offer.Team}";
        career.Opportunities.RemoveAll(x => x.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && x.IsOpen);

        var homeCountry = career.Nationality;
        // La sede deve raccontare la stessa disciplina del sedile. Un kart non
        // viene mandato su una pista stradale scelta solo perché è la prima in
        // ordine alfabetico; se esiste un kartodromo installato, quello viene
        // sempre prima. Per formule/touring si preferiscono invece i circuiti
        // permanenti, mantenendo comunque un ripiego dichiarato se i contenuti
        // disponibili sono pochi.
        var compatible = contentIndex.Tracks
            .Where(x => TrackMatchesCategory(x, category))
            .ToList();
        var pool = compatible.Count > 0 ? compatible : contentIndex.Tracks.ToList();
        var tracks = pool
            .OrderByDescending(x => !string.IsNullOrWhiteSpace(homeCountry) && x.Country.Equals(homeCountry, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => TrackAffinityForCategory(x, category))
            .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(3)
            .ToList();
        if (tracks.Count == 0) return 0;

        for (var i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            var travel = i * 30;
            var grossCost = offer.RaceFee + travel;
            // Anche il weekend comprato cade dentro l'anno sportivo: senza
            // questo, un sedile cliente firmato a novembre proponeva gare a
            // gennaio.
            var grezza = career.StoryDate.AddDays(7 + i * 14);
            var ultimaUtile = NarrativeCalendar.LastRaceSundayOfSeason(grezza.Year);
            var date = grezza.Month >= NarrativeCalendar.SeasonFirstMonth && grezza <= ultimaUtile
                ? grezza
                : new DateTime(grezza.Month >= NarrativeCalendar.SeasonFirstMonth ? grezza.Year + 1 : grezza.Year,
                    NarrativeCalendar.SeasonFirstMonth, 12).AddDays(i * 14);
            career.Opportunities.Add(new Opportunity
            {
                Id = $"{prefix}-{track.Id}",
                Kind = i == 0 ? OpportunityKind.EntryRace : OpportunityKind.InvitationRace,
                Title = $"{(i == 0 ? "Prima gara" : "Weekend kart")} · {track.Name}",
                ProposedBy = offer.Team,
                Justification = $"{offer.Team} ti mette a disposizione {UiText.Car(offer.Car)} per una gara di {category}. " +
                    $"La quota comprende il sedile e la preparazione di base; il viaggio verso {track.Name} aggiunge € {travel:N0}. " +
                    "Scegli un solo weekend: un pilota cliente non può bloccare più vetture nello stesso periodo.",
                Tier = career.Tier,
                Category = category,
                CarId = offer.Car,
                TrackId = track.Id,
                TrackName = track.Name,
                Date = date,
                Deadline = date.AddDays(-3),
                Cost = grossCost,
                CoveredPercent = offer.TeamSupportPercent,
                BestCaseReturn = offer.PrizeP1 + i * 40,
                LikelyReturn = offer.PrizeP4P5,
                WorstCaseReturn = 0,
                Objective = offer.Objective,
                SeasonRounds = 1
            });
        }
        return tracks.Count;
    }

    private static bool TrackMatchesCategory(ContentTrackRecord track, string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return true;
        var text = $"{track.Id} {track.Name} {track.Category}".ToLowerInvariant();
        if (category.Equals("kart", StringComparison.OrdinalIgnoreCase))
            return text.Contains("kart", StringComparison.OrdinalIgnoreCase) || track.Category.Equals("kartodromo", StringComparison.OrdinalIgnoreCase);
        // Tutte le categorie con vetture a ruote coperte o formule usano un
        // circuito permanente/cittadino; non sono sedi compatibili le sole
        // varianti hillclimb o i tracciati speciali.
        return !track.Category.Equals("hillclimb", StringComparison.OrdinalIgnoreCase)
            && !track.Category.Equals("special", StringComparison.OrdinalIgnoreCase);
    }

    private static int TrackAffinityForCategory(ContentTrackRecord track, string category)
    {
        if (category.Equals("kart", StringComparison.OrdinalIgnoreCase))
            return track.Category.Equals("kartodromo", StringComparison.OrdinalIgnoreCase) || track.Name.Contains("kart", StringComparison.OrdinalIgnoreCase) ? 100 : 0;
        return track.Category.Equals("permanent", StringComparison.OrdinalIgnoreCase) ? 60 : 20;
    }

    private void OpenClientRaceChoices()
    {
        if (BlockIfPending("le gare disponibili")) return;
        // Il banco di collaudo verifica lo stato prodotto dalla firma senza
        // aprire una finestra modale (che resterebbe invisibile fuori schermo).
        // Nel gioco normale la rosa delle gare si apre subito dopo la firma.
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") == "1") return;
        using var dialog = new OpportunityDialog(
            career,
            AcceptOpportunity,
            DeclineOpportunity,
            x => x.IsRace && x.ProposedBy.Equals(career.Team, StringComparison.OrdinalIgnoreCase)
                && x.CarId.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
        dialog.ShowDialog(this);
        SaveCareer();
        RefreshUi();
    }

    /// <summary>
    /// La scena della firma: simbolo della squadra, tavola, e che cosa comporta.
    ///
    /// Vale per tutti e due i tipi di sedile. Prima il professionista chiudeva
    /// con una finestrella di testo e il cliente con niente del tutto: si
    /// premeva «firma» e lo schermo restava identico.
    /// </summary>
    /// <summary>
    /// La prova che riapre la porta dopo una retrocessione.
    ///
    /// Retrocedere non e' solo scendere di livello: e' perdere il sedile. Per
    /// tornare in griglia bisogna rifarsi vedere, e da quella prova nascono le
    /// proposte nuove — esattamente come all'inizio della carriera.
    /// </summary>
    /// <summary>
    /// La scena di fine campionato: la notifica grande, poi gli amici.
    ///
    /// Promozione, permanenza e retrocessione passavano tutte e tre in una riga
    /// di notizia in fondo alla schermata: si vinceva un campionato e non se ne
    /// accorgeva nessuno. Sono i tre momenti che danno forma a una carriera e
    /// meritano lo schermo intero.
    /// </summary>
    private void MostraEsitoStagione(SeasonVerdict verdetto, int posizione,
        int livelloPrima, int livelloDopo, bool titolo, int premio)
    {
        if (CareerMessages.Unattended) return;

        // Prima il bilancio dell'annata, poi che cosa comporta. Le statistiche
        // di fine anno stavano solo sul ramo del referto reale: su un computer
        // senza Assetto Corsa una stagione finiva senza che nessuno dicesse
        // quante pole, quanti ritiri, come era andata col compagno di squadra.
        if (career.SeasonArchive.Count > 0)
        {
            using var bilancio = new SeasonReportDialog(career, career.SeasonArchive[^1], NarrateCurrentStory, OpenMedia);
            bilancio.ShowDialog(this);
        }

        var gradino = CareerLadder.Current(career, contentIndex.Cars);
        var disciplina = gradino.Path switch
        {
            LadderPath.Karting => "kart",
            LadderPath.SingleSeater => gradino.Tier == "Formula / top tier" ? "formula-vertice"
                : gradino.Tier == "Categoria avanzata" ? "formula-alta" : "formula-minore",
            LadderPath.Endurance => gradino.Tier == "Categoria regionale" ? "gt" : "endurance",
            LadderPath.Touring => "turismo",
            _ => "kart"
        };
        var momento = verdetto switch
        {
            SeasonVerdict.Promosso => titolo ? "vittoria" : "campionato",
            SeasonVerdict.Retrocesso => "sconfitta",
            _ => "campionato"
        };
        var seme = (career.Driver ?? "").Length * 23 + career.Season * 7;
        var tavole = IllustrationCatalog.Find(disciplina, momento, seme, 1);
        var immagine = tavole.Count > 0 ? tavole[0] : "";

        var (kicker, intestazione, tono, pulsante) = verdetto switch
        {
            SeasonVerdict.Promosso when titolo =>
                ("complimenti, sei campione", $"HAI VINTO IL {career.Championship.ToUpperInvariant()}", MilestoneTone.Trionfo, "E ADESSO?"),
            SeasonVerdict.Promosso =>
                ("complimenti, sei promosso", $"P{posizione}: SI SALE A {ChampionshipLadder.Name(livelloDopo).ToUpperInvariant()}", MilestoneTone.Trionfo, "E ADESSO?"),
            SeasonVerdict.Retrocesso =>
                ("stagione chiusa in fondo", $"P{posizione}: SI TORNA IN {ChampionshipLadder.Name(livelloDopo).ToUpperInvariant()}", MilestoneTone.Caduta, "ACCETTARE"),
            _ =>
                ("stagione conclusa", $"P{posizione}: UN ALTRO ANNO IN {ChampionshipLadder.Name(livelloDopo).ToUpperInvariant()}", MilestoneTone.Passaggio, "AVANTI")
        };

        var conseguenze = new List<string>
        {
            CareerProgression.Racconto(verdetto, posizione, livelloPrima, livelloDopo),
            $"PREMIO DI FINE STAGIONE\n€ {premio:N0} · cassa dopo l'accredito: € {career.Cash:N0}"
        };
        conseguenze.Add(verdetto switch
        {
            SeasonVerdict.Promosso =>
                "LE PROPOSTE\nCon il livello nuovo arrivano squadre nuove, e non sono tutte uguali: "
                + "una costa di più e ti mette in vetrina, un'altra costa poco e ha i meccanici bravi. "
                + "Guarda i profili prima di scegliere il prezzo.",
            SeasonVerdict.Retrocesso =>
                "PER RIENTRARE\nHai perso il sedile. Fra tre settimane c'è una prova: da lì nascono le proposte nuove. "
                + "È lo stesso percorso dell'inizio, con la differenza che adesso sai quanto costa sbagliare una stagione.",
            _ =>
                "L'ANNO PROSSIMO\nSi resta allo stesso livello, con altre proposte. "
                + "Una stagione a metà classifica non decide niente: decide quella dopo."
        });

        using var notifica = new MilestoneDialog(kicker, intestazione,
            $"{career.Driver} · stagione {career.Season} · {career.Championship}",
            conseguenze, immagine, tono, pulsante);
        notifica.ShowDialog(this);

        // Quando esiste una scena scritta per questo esito, e' quella che si
        // vede: prima si aprivano tutte e due — la scena generica e poi quella
        // scritta — e il giocatore si trovava tre finestre di dialogo di fila
        // sullo stesso fatto.
        var fatti = CapetaScenes.Leggi(career, contentIndex.Cars, career.Schedule ?? []);
        var scritta = titolo ? CapetaScenes.Scena(MomentoDiCarriera.TitoloVinto, fatti)
            : verdetto == SeasonVerdict.Retrocesso ? CapetaScenes.Scena(MomentoDiCarriera.Retrocessione, fatti)
            : [];
        var battute = scritta.Count > 0
            ? scritta
            : SeasonReactions.Build(career, verdetto, posizione, livelloPrima, livelloDopo, titolo);
        if (battute.Count == 0) return;
        using var scena = new AnimeDialogueDialog($"CorsaCareer — fine stagione {career.Season}", battute);
        scena.ShowDialog(this);
    }

    /// <summary>
    /// La tavola giusta per un momento della carriera, nella disciplina in cui
    /// si corre davvero. Nessuna immagine e' meglio di una sbagliata: una GT
    /// sopra una carriera di kart racconta un'altra storia.
    /// </summary>
    private string TavolaPerMomento(string momento)
    {
        var gradino = CareerLadder.Current(career, contentIndex.Cars);
        var disciplina = gradino.Path switch
        {
            LadderPath.Karting => "kart",
            LadderPath.SingleSeater => gradino.Tier == "Formula / top tier" ? "formula-vertice"
                : gradino.Tier == "Categoria avanzata" ? "formula-alta" : "formula-minore",
            LadderPath.Endurance => gradino.Tier == "Categoria regionale" ? "gt" : "endurance",
            LadderPath.Touring => "turismo",
            _ => "kart"
        };
        var seme = (career.Driver ?? "").Length * 29 + career.Races * 5 + career.Season;
        var scelte = IllustrationCatalog.Find(disciplina, momento, seme, 1);
        return scelte.Count > 0 ? scelte[0] : "";
    }

    /// <summary>
    /// Le spiegazioni dell'allenatore, al momento in cui servono.
    ///
    /// Le regole della carriera esistevano tutte e non le diceva nessuno: le
    /// offerte sembravano piovute dal cielo, e quelle che non arrivavano non si
    /// sapeva perche' non arrivassero. Rei le spiega guardando i numeri veri del
    /// pilota — «ti manca questo, e serve a questo» — cosi' ogni occasione
    /// diventa una cosa che ci si e' guadagnati.
    /// </summary>
    private void BriefingDiApertura()
    {
        if (CareerMessages.Unattended) return;
        career.BriefingFatti ??= [];
        if (career.BriefingFatti.Contains(CoachBriefing.DettoInizioCarriera)) return;
        career.BriefingFatti.Add(CoachBriefing.DettoInizioCarriera);
        var calendario = CareerScheduler.ChampionshipRounds(career.Schedule ?? [], career.Season)
            .OrderBy(x => x.Date).ToList();
        var battute = CoachBriefing.Apertura(career, CareerLadder.Current(career, contentIndex.Cars), calendario);
        using var scena = new AnimeDialogueDialog("CorsaCareer — come funziona da qui in avanti", battute);
        scena.ShowDialog(this);
        SaveCareer();
    }

    /// <summary>
    /// L'avviso quando una soglia si avvicina: il momento in cui una regola
    /// smette di essere teoria e diventa una cosa da fare questa settimana.
    /// </summary>
    private void BriefingDopoLaGara()
    {
        if (CareerMessages.Unattended) return;
        career.BriefingFatti ??= [];
        var vittorieDiFila = career.RaceHistory.AsEnumerable().Reverse()
            .TakeWhile(x => !x.Dnf && x.Position == 1).Count();
        var podiDiFila = career.RaceHistory.AsEnumerable().Reverse()
            .TakeWhile(x => !x.Dnf && x.Position is > 0 and <= 3).Count();
        var battute = CoachBriefing.Soglia(career, CareerLadder.Current(career, contentIndex.Cars),
            RacesOnCurrentStep(), vittorieDiFila, podiDiFila, out var chiave);
        if (battute.Count == 0 || string.IsNullOrEmpty(chiave)) return;
        // Ogni spiegazione si dà una volta sola: ripetuta a ogni gara
        // diventerebbe la cosa che si salta senza leggere.
        if (career.BriefingFatti.Contains(chiave)) return;
        career.BriefingFatti.Add(chiave);
        using var scena = new AnimeDialogueDialog("CorsaCareer — due parole con Rei", battute);
        scena.ShowDialog(this);
        SaveCareer();
    }

    /// <summary>
    /// <summary>
    /// Chiude la stagione se i suoi round sono tutti corsi: classifica finale,
    /// premio, verdetto, promozione o retrocessione, scena e archivio.
    ///
    /// Era un blocco di centotrenta righe dentro la registrazione di una gara,
    /// e questo era il difetto: una stagione poteva finire soltanto come
    /// effetto collaterale di una gara appena conclusa. Firmando per una
    /// categoria diversa non c'era modo di chiudere quella in corso, quindi i
    /// round nuovi finivano nella stessa stagione — al banco se ne sono viste
    /// da ventotto gare corse fra kart e monoposto, con un titolo «regionale»
    /// vinto cambiando vettura tre volte.
    ///
    /// Adesso è un passo che si può chiamare anche da fuori. Restituisce vero
    /// se la stagione è stata davvero chiusa.
    /// </summary>
    private bool ChiudiStagioneSeCompleta()
    {
        var roundStagione = CareerScheduler.ChampionshipRounds(career.Schedule ?? [], career.Season);
        var stagioneFinita = roundStagione.Count > 0
            && roundStagione.All(x => !x.IsPlanned)
            && !career.SeasonArchive.Any(x => x.Season == career.Season);
        if (stagioneFinita)
        {
            if (career.SponsorObjectiveStatus == "In corso")
            {
                career.SponsorObjectiveStatus = "Non raggiunto";
                var sponsorHeadline = $"A fine campionato {career.Driver} chiude con {career.SponsorQualifyingResults}/{SponsorRequiredResults()} risultati Top {career.SponsorTarget}: {career.Sponsor} valuterà il rinnovo.";
                career.News.Add(sponsorHeadline);
                career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SPONSOR_REVIEW", Headline = sponsorHeadline, Track = "Campionato", Importance = 65 });
            }
            var finalPosition = career.Standings.OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).Select((x, index) => new { x.Driver, Position = index + 1 }).FirstOrDefault(x => x.Driver == career.Driver)?.Position ?? 0;
            // Il premio finale dipende dalla posizione in classifica e dalla
            // categoria, non da una soglia di punti che ignora il calendario.
            var award = EconomyEngine.SeasonAward(career.Tier, finalPosition, Math.Max(1, career.Standings.Count),
                CareerLadder.Current(career, contentIndex.Cars).Step);
            // Come per il premio di gara: a un pilota ingaggiato ne resta una
            // quota, il resto è della squadra che ha pagato la stagione.
            if (career.ContractActive && !career.IsClientDriver) award = (int)Math.Round(award * 0.30 / 100.0) * 100;
            career.Cash += award; career.PrizeMoney += award;
            // Il titolo: chiudere una stagione al primo posto.
            //
            // E il mondiale, quando quel titolo arriva sul gradino piu alto
            // della strada scelta — Formula 1, hypercar, turismo mondiale.
            // Prima non esisteva nessuno dei due: si saliva la scala e poi si
            // correvano stagioni identiche senza un traguardo.
            var titolo = finalPosition == 1;
            var gradino = CareerLadder.Current(career, contentIndex.Cars);
            var vetta = CareerLadder.SummitFor(gradino, contentIndex.Cars);
            // Il mondiale è il titolo vinto in cima alla scala dei campionati,
            // non semplicemente sulla vettura più veloce installata.
            var mondiale = titolo && ChampionshipLadder.IsTop(career.ChampionshipLevel);

            // Tre esiti, non due: primi tre si sale, in fondo si scende, in
            // mezzo si resta.
            //
            // Prima la retrocessione non esisteva e una stagione andata male non
            // costava niente: si restava dov'era all'infinito, e questo toglieva
            // senso anche alla promozione. Il terzo esito e' quello che rende
            // interessante il secondo.
            var livelloPrima = ChampionshipLadder.Clamp(career.ChampionshipLevel);
            var inClassifica = Math.Max(career.Standings?.Count ?? 0, 12);
            var verdetto = CareerProgression.Verdetto(finalPosition, inClassifica, livelloPrima);
            var livelloDopo = CareerProgression.LivelloDopo(livelloPrima, verdetto);
            // Il campionato non può salire più in alto di quanto regga la
            // categoria: per correre il mondiale serve prima la vettura da
            // mondiale. È il legame che rende faticoso — e quindi sensato — il
            // salto di categoria.
            livelloDopo = Math.Min(livelloDopo, ChampionshipLadder.MaxLevelForStep(gradino.Step));
            if (livelloDopo > livelloPrima)
            {
                career.ChampionshipLevel = livelloDopo;
                career.RacesAtLastLevelUp = career.Races;
                career.Championship = ChampionshipLadder.Name(livelloDopo);
                var salita = $"{career.Driver} chiude P{finalPosition} e sale a «{ChampionshipLadder.Name(livelloDopo)}» (livello {livelloDopo} di {ChampionshipLadder.Levels}): {ChampionshipLadder.Scope(livelloDopo)}.";
                career.News.Add(salita);
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CHAMPIONSHIP_PROMOTION",
                    Headline = salita, Track = career.Championship, Importance = 88
                });
                CareerLog.Info("carriera", $"promozione di campionato: livello {livelloPrima} → {livelloDopo}");
            }
            else if (livelloDopo < livelloPrima)
            {
                // Si retrocede. Per rientrare serve una prova: nessuno firma un
                // pilota che arriva da una stagione cosi' senza rivederlo girare.
                career.ChampionshipLevel = livelloDopo;
                career.Championship = ChampionshipLadder.Name(livelloDopo);
                career.CareerPhase = "Evaluation";
                career.RookieEvaluationStatus = "Rientro da valutare dopo una stagione negativa";
                career.ContractActive = false; career.ContractYears = 0;
                career.Offers?.Clear();
                var discesa = CareerProgression.Racconto(verdetto, finalPosition, livelloPrima, livelloDopo);
                career.News.Add($"{career.Driver}: {discesa}");
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CHAMPIONSHIP_RELEGATION",
                    Headline = $"{career.Driver} retrocede a «{ChampionshipLadder.Name(livelloDopo)}»: {discesa}",
                    Track = career.Championship, Importance = 90
                });
                ProgrammaProvaDiRientro();
                CareerLog.Info("carriera", $"retrocessione: livello {livelloPrima} → {livelloDopo}, prova di rientro programmata");
            }
            else
            {
                var resta = $"{career.Driver} chiude P{finalPosition}: resta in «{ChampionshipLadder.Name(livelloPrima)}». {ChampionshipLadder.PromotionRule(livelloPrima)}";
                career.News.Add(resta);
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CHAMPIONSHIP_STAY",
                    Headline = resta, Track = career.Championship, Importance = 62
                });
            }

            // Le vittorie dell'annata, non quelle di sempre: qui finiva il
            // totale di carriera, e l'archivio raccontava una stagione da
            // ventidue vittorie in un campionato di diciotto gare.
            var vittorieStagione = career.RaceHistory
                .Count(x => x.Season == career.Season && !x.Dnf && x.Position == 1);
            career.SeasonArchive.Add(new SeasonSummary { Season = career.Season, Championship = career.Championship, Tier = career.Tier, CompletedUtc = DateTime.UtcNow, Points = career.Points, Wins = vittorieStagione, FinalPosition = finalPosition, Award = award, TitleWon = titolo, WorldTitle = mondiale });
            // La scena del verdetto: la notifica grande e poi gli amici. Viene
            // dopo l'archiviazione perche' il bilancio deve poter leggere la
            // stagione appena chiusa.
            MostraEsitoStagione(verdetto, finalPosition, livelloPrima, livelloDopo, titolo, award);
            // La scena scritta del titolo e della retrocessione la mostra
            // MostraEsitoStagione al posto di quella generica: aprirla anche
            // qui significherebbe raccontare due volte lo stesso momento.
            stagioneAppenaChiusa = true;
            // Fine stagione è il momento in cui un pilota decide se continuare:
            // non lo si chiede dopo una gara storta, lo si chiede guardando
            // l'anno appena finito.
            ChiediSeRitirarsi();

            if (mondiale)
            {
                var mondialeTitolo = $"{career.Driver} è campione del mondo. {career.Championship}, stagione {career.Season}: {career.Points} punti, {career.Wins} vittorie.";
                career.Headline = mondialeTitolo; career.News.Add(mondialeTitolo);
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "WORLD_TITLE",
                    Headline = mondialeTitolo, Track = career.Championship, Importance = 100
                });
                CareerLog.Info("carriera", $"MONDIALE VINTO: {career.Championship} stagione {career.Season}");
            }
            else if (titolo)
            {
                var titoloTesto = $"{career.Driver} vince il campionato {career.Championship} con {career.Points} punti e {career.Wins} vittorie.";
                career.Headline = titoloTesto; career.News.Add(titoloTesto);
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CHAMPIONSHIP_TITLE",
                    Headline = titoloTesto, Track = career.Championship, Importance = 98
                });
                CareerLog.Info("carriera", $"titolo vinto: {career.Championship} stagione {career.Season}");
            }
            var seasonHeadline = $"Fine campionato: {career.Driver} chiude con {career.Points} punti{(finalPosition > 0 ? $" e la posizione P{finalPosition}" : "")}. Premio classifica € {award:N0}.";
            career.Headline = seasonHeadline; career.News.Add(seasonHeadline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SEASON_AWARD", Headline = seasonHeadline, Track = "Campionato", Importance = 90 });
        }
        return stagioneFinita;
    }

    /// A fine stagione, se è il caso, si chiede al pilota se smettere.
    ///
    /// La domanda non arriva per età e basta: arriva quando l'età si somma a
    /// una ragione — un sedile che non c'è più, stagioni senza vittorie, o
    /// semplicemente troppi anni per quel livello. La decisione resta del
    /// giocatore: si può continuare a correre finché si vuole, ma da un certo
    /// punto in poi si corre sapendo che si sta correndo di troppo.
    /// </summary>
    private void ChiediSeRitirarsi()
    {
        if (career.Retired) return;
        var eta = EtaPilota();
        var stagioni = career.SeasonArchive ?? [];
        var senzaVittorie = 0;
        foreach (var s in stagioni.OrderByDescending(x => x.Season))
        {
            if (s.Wins > 0) break;
            senzaVittorie++;
        }
        var motivo = DriverAge.MotivoDelRitiro(
            eta, senzaVittorie,
            senzaSedile: !career.ContractActive && (career.Offers?.Count ?? 0) == 0,
            alVertice: ChampionshipLadder.IsTop(career.ChampionshipLevel));
        if (motivo == null) return;
        // Senza nessuno a cui chiedere la carriera non puo' restare aperta per
        // sempre: al banco si vedeva un pilota correre a quarantasei anni
        // perche' la domanda non veniva mai posta. Se non c'e' un giocatore, la
        // risposta la da' l'eta'.
        if (CareerMessages.Unattended) { Ritirati(motivo); return; }

        var risposta = CareerMessages.Ask(this,
            $"{motivo}\n\n"
            + $"In carriera: {career.Races} gare, {career.Wins} vittorie, "
            + $"{stagioni.Count(x => x.TitleWon)} titoli in {stagioni.Count} stagioni.\n\n"
            + "Vuoi smettere adesso?\n\n"
            + "SÌ = appendi il casco al chiodo e chiudi la carriera\n"
            + "NO = si continua, finché il corpo tiene",
            "CorsaCareer — è ora di smettere?", MessageBoxButtons.YesNo, DialogResult.No);
        if (risposta != DialogResult.Yes) return;

        Ritirati(motivo);
    }

    /// <summary>
    /// Travasa il bilancio dell'annata nei totali di carriera, prima che le
    /// voci di stagione vengano azzerate. Va chiamato una volta sola per
    /// stagione, subito prima dell'azzeramento.
    /// </summary>
    private void AccumulaTotaliDiCarriera()
    {
        career.LifetimePrizeMoney += career.PrizeMoney;
        career.LifetimeSponsorMoney += career.SponsorMoney;
        career.LifetimeSalary += career.SalaryPaid;
        career.LifetimeRepairCosts += career.RepairCosts;
        career.LifetimeLogisticsCosts += career.LogisticsCosts;
    }

    /// <summary>Chiude la carriera e mostra il bilancio di tutti gli anni.</summary>
    private void Ritirati(string motivo)
    {
        career.Retired = true;
        career.RetiredOn = career.StoryDate;
        career.RetirementReason = motivo;
        career.ContractActive = false;
        career.Offers?.Clear();
        (career.Schedule ?? []).RemoveAll(x => x.IsPlanned);

        var bilancio = CareerEpilogue.Compute(career, contentIndex.Cars);
        var titolo = $"{career.Driver} si ritira dalle corse a {bilancio.EtaAlRitiro} anni: "
                     + $"{bilancio.Gare} gare, {bilancio.Vittorie} vittorie, {bilancio.Titoli} titoli.";
        career.Headline = titolo;
        career.News.Add(titolo);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CAREER_END",
            Headline = titolo + " " + bilancio.Giudizio(), Track = "Fine della carriera", Importance = 100
        });
        CareerLog.Info("carriera", $"RITIRO: {bilancio.Gare} gare, {bilancio.Vittorie} vittorie, {bilancio.Mondiali} mondiali");
        SaveCareer();

        if (CareerMessages.Unattended) return;
        using var epilogo = new CareerEpilogueDialog(bilancio, motivo, TavolaPerMomento("vittoria"));
        epilogo.ShowDialog(this);
        RefreshUi();
    }

    /// <summary>
    /// Racconta un momento della carriera, una volta sola.
    ///
    /// Le scene scritte esistevano ma non le apriva nessuno, come il registro
    /// delle prime volte e come il regista del cast: tre sistemi scritti e mai
    /// collegati fra loro. Questo e' il filo che li unisce — il registro dice
    /// se il momento e' gia' successo, il catalogo fornisce le battute, il
    /// regista sceglie chi le dice.
    /// </summary>
    private void RaccontaMomento(MomentoDiCarriera momento, string chiave = "")
    {
        if (CareerMessages.Unattended || !Visible || IsDisposed) return;

        career.Firsts ??= new CareerFirsts();
        // Una chiave vuota significa «si puo' ripetere»: l'apertura di stagione
        // succede ogni anno, la prima vittoria una volta sola.
        if (!string.IsNullOrEmpty(chiave))
        {
            if (career.Firsts.Has(chiave)) return;
            career.Firsts.Register(chiave, career.StoryDate,
                career.RaceHistory?.LastOrDefault()?.Track ?? "", career.Tier ?? "", career.Season);
        }

        var fatti = CapetaScenes.Leggi(career, contentIndex.Cars, career.Schedule ?? []);
        var battute = CapetaScenes.Scena(momento, fatti);
        if (battute.Count == 0) return;

        using var scena = new AnimeDialogueDialog(CapetaScenes.Titolo(momento), battute);
        scena.ShowDialog(this);
        RefreshUi();
    }

    /// <summary>
    /// Dopo una gara: se e' appena successa una prima volta, la si racconta.
    ///
    /// Una scena sola per gara, sempre la piu' importante fra quelle appena
    /// avvenute. Mostrarne tre di fila dopo la stessa domenica trasformerebbe
    /// un momento in una coda di finestre.
    /// </summary>
    /// <summary>
    /// Vero se la gara appena registrata ha anche chiuso la stagione.
    ///
    /// Serve a non impilare finestre: una chiusura di stagione ne apre gia'
    /// quattro o cinque — il bilancio statistico, la notifica del verdetto, la
    /// scena, la domanda sul ritiro — e aggiungerci sopra le reazioni di gara,
    /// la scena della prima volta e l'articolo faceva otto finestre modali in
    /// fila per un singolo pomeriggio.
    /// </summary>
    private bool stagioneAppenaChiusa;

    private void ControllaPrimeVolte(int posizione, bool ritiro)
    {
        career.Firsts ??= new CareerFirsts();

        if (!ritiro && posizione == 1 && !career.Firsts.Has(CareerFirsts.Win))
        { RaccontaMomento(MomentoDiCarriera.PrimaVittoria, CareerFirsts.Win); return; }

        if (!ritiro && posizione is > 0 and <= 3 && !career.Firsts.Has(CareerFirsts.Podium))
        { RaccontaMomento(MomentoDiCarriera.PrimoPodio, CareerFirsts.Podium); return; }

        if (ritiro && !career.Firsts.Has(CareerFirsts.Dnf))
        { RaccontaMomento(MomentoDiCarriera.PrimaBattuta, CareerFirsts.Dnf); return; }

        if (!career.Firsts.Has(CareerFirsts.Race))
        { RaccontaMomento(MomentoDiCarriera.PrimaGara, CareerFirsts.Race); return; }

        // Niente prime volte: restano i momenti del campionato, che si possono
        // ripetere ma non nella stessa stagione.
        var rimaste = (career.Schedule ?? []).Count(x => x.Season == career.Season && x.IsPlanned);
        var corse = (career.Schedule ?? []).Count(x => x.Season == career.Season && !x.IsPlanned);
        if (rimaste == 1)
            RaccontaMomento(MomentoDiCarriera.UltimaGara, $"ultima-s{career.Season:00}");
        else if (rimaste > 0 && corse > 0 && Math.Abs(corse - rimaste) <= 1)
            RaccontaMomento(MomentoDiCarriera.MetaStagione, $"meta-s{career.Season:00}");
        else if (career.Firsts.Has(CareerFirsts.Win) && posizione == 1 && !ritiro)
            RaccontaMomento(MomentoDiCarriera.Rivalita, $"rivale-s{career.Season:00}");
    }

    /// <summary>La scena della chiamata da un campionato superiore, con gli amici.</summary>
    private void MostraChiamata(Opportunity opportunity, int livelloPrima)
    {
        if (CareerMessages.Unattended) return;
        var motivo = CareerProgression.MotivoDellaChiamata(
            career.RaceHistory.AsEnumerable().Reverse().TakeWhile(x => !x.Dnf && x.Position == 1).Count(),
            career.RaceHistory.AsEnumerable().Reverse().TakeWhile(x => !x.Dnf && x.Position is > 0 and <= 3).Count(),
            career.RaceHistory.TakeLast(6).Where(x => !x.Dnf && x.Position > 0 && x.StartingPosition > 0)
                .Select(x => x.StartingPosition - x.Position).DefaultIfEmpty(0).Max());

        var conseguenze = new List<string>
        {
            $"{opportunity.ProposedBy} non ha aspettato la fine del campionato: ti vuole adesso, e il posto lo paga la squadra.",
            $"DA DOVE A DOVE\n{ChampionshipLadder.Name(livelloPrima)} → {career.Championship} "
            + $"(livello {career.ChampionshipLevel} di {ChampionshipLadder.Levels}): {ChampionshipLadder.Scope(career.ChampionshipLevel)}",
            $"LA PROVA\nLe prime {CareerProgression.GareDiProvaDopoIlSalto} gare decidono tutto. "
            + "Due podi o una media nella prima parte del gruppo e il posto è tuo per la stagione dopo; "
            + $"un rendimento da fondo classifica e si torna in {ChampionshipLadder.Name(livelloPrima).ToLowerInvariant()}, "
            + "con la vecchia squadra o con un'altra.",
            "PERCHÉ PROPRIO TE\nNon sono solo i risultati: è il seguito che hai costruito e la condizione in cui sei. "
            + "Senza quelli una squadra non rischia su un pilota a stagione in corso."
        };
        using var notifica = new MilestoneDialog(
            "hanno chiamato per te", $"{career.Championship.ToUpperInvariant()}: SI SALE SUBITO",
            $"{career.Driver} · {motivo}", conseguenze,
            TavolaPerMomento("trattativa"), MilestoneTone.Trionfo, "ACCETTO");
        notifica.ShowDialog(this);

        var battute = SeasonReactions.Chiamata(career, career.Championship, motivo);
        using var scena = new AnimeDialogueDialog($"CorsaCareer — la chiamata", battute);
        scena.ShowDialog(this);
    }

    /// <summary>
    /// Il verdetto della scommessa, dopo le prime gare al livello nuovo.
    ///
    /// Chi sale a stagione in corso non ha una classifica da difendere: ha una
    /// squadra che si e' esposta. Il giudizio arriva presto e guarda il
    /// rendimento immediato, come nella realta'.
    /// </summary>
    private void ValutaLaProvaDopoIlSalto()
    {
        if (career.SaltoAllaGara <= 0) return;
        var dopoIlSalto = career.RaceHistory.Skip(career.SaltoAllaGara).ToList();
        if (dopoIlSalto.Count < CareerProgression.GareDiProvaDopoIlSalto) return;

        var griglia = dopoIlSalto.LastOrDefault()?.Classification.Count ?? 12;
        var verdetto = CareerProgression.VerdettoDelSalto(
            dopoIlSalto.Select(x => x.Dnf ? 0 : x.Position).ToList(), griglia);
        var livelloPrima = ChampionshipLadder.Clamp(career.ChampionshipLevel);
        career.SaltoAllaGara = 0;

        if (verdetto == SeasonVerdict.Retrocesso)
        {
            var tornaA = ChampionshipLadder.Clamp(career.LivelloPrimaDelSalto <= 0 ? livelloPrima - 1 : career.LivelloPrimaDelSalto);
            career.ChampionshipLevel = tornaA;
            career.Championship = ChampionshipLadder.Name(tornaA);
            career.ContractActive = false; career.ContractYears = 0;
            career.Offers = BuildOffers();
            var testo = $"La scommessa non ha funzionato: dopo {dopoIlSalto.Count} gare {career.Driver} torna in «{career.Championship}».";
            career.News.Add(testo);
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "STEPUP_FAILED",
                Headline = testo, Track = career.Championship, Importance = 84
            });
            CareerLog.Info("carriera", $"prova dopo il salto fallita: si torna al livello {tornaA}");
            if (!CareerMessages.Unattended)
            {
                var conseguenze = new List<string>
                {
                    $"In {dopoIlSalto.Count} gare non è arrivato quello che serviva, e la squadra che si era esposta ha smesso di esporsi.",
                    $"DOVE SI TORNA\n{career.Championship} · livello {tornaA} di {ChampionshipLadder.Levels}."
                    + (string.IsNullOrWhiteSpace(career.SquadraPrimaDelSalto) ? "" : $" {career.SquadraPrimaDelSalto} è la prima porta a cui bussare."),
                    "COSA RESTA\nL'esperienza fatta lassù non si perde: hai corso contro gente più forte e adesso lo sai fare. "
                    + "La prossima chiamata dovrà trovarti pronto."
                };
                using var notifica = new MilestoneDialog(
                    "la scommessa è finita", $"SI TORNA IN {career.Championship.ToUpperInvariant()}",
                    $"{career.Driver} · {dopoIlSalto.Count} gare al livello superiore", conseguenze,
                    "", MilestoneTone.Caduta, "RICOMINCIARE");
                notifica.ShowDialog(this);
            }
        }
        else if (verdetto == SeasonVerdict.Promosso && !CareerMessages.Unattended)
        {
            var podi = dopoIlSalto.Count(x => !x.Dnf && x.Position is > 0 and <= 3);
            var conseguenze = new List<string>
            {
                $"{podi} podi nelle prime {dopoIlSalto.Count} gare al livello nuovo: la squadra che ha rischiato su di te ha avuto ragione.",
                $"IL POSTO È TUO\nResti in «{career.Championship}» (livello {livelloPrima} di {ChampionshipLadder.Levels}). "
                + "Il sedile non è più in prova.",
                "E ADESSO\nChi va così forte appena salito viene guardato da chi sta ancora più in alto. "
                + "Continua e la prossima chiamata arriverà da lì."
            };
            using var notifica = new MilestoneDialog(
                "scommessa vinta", "IL SEDILE È TUO",
                $"{career.Driver} · {career.Championship}", conseguenze,
                "", MilestoneTone.Trionfo, "AVANTI");
            notifica.ShowDialog(this);
        }
    }

    private void ProgrammaProvaDiRientro()
    {
        career.Schedule ??= [];
        // Via gli appuntamenti della stagione appena chiusa: quel campionato
        // non esiste piu' per questo pilota.
        career.Schedule.RemoveAll(x => x.IsPlanned && x.Kind == ScheduledEventKind.ChampionshipRound);
        var pista = contentIndex.Tracks.FirstOrDefault(x => TrackMatchesCategory(x, CategoryForCareer(career)))
                    ?? contentIndex.Tracks.FirstOrDefault();
        if (pista == null) return;
        CareerScheduler.Append(career.Schedule, new ScheduledEvent
        {
            Id = $"rientro-s{career.Season:00}-{career.Races}",
            Kind = ScheduledEventKind.EvaluationTest,
            Date = NarrativeCalendar.RaceWeekend(career.StoryDate.AddDays(21)),
            TrackId = pista.Id,
            TrackName = string.IsNullOrWhiteSpace(pista.Name) ? pista.Id : pista.Name,
            Country = pista.Country,
            Season = career.Season,
            GeneratedBy = "Retrocessione: per rientrare in griglia serve rifarsi vedere",
            Objective = "Convincere il paddock che la stagione storta era un incidente"
        });
    }

    private void AnnounceSeatSigned(TeamOffer offer, string category, int gareDaScegliere)
    {
        if (CareerMessages.Unattended) return;
        var calendario = CareerScheduler.ChampionshipRounds(career.Schedule, career.Season)
            .Where(x => x.IsPlanned).OrderBy(x => x.Date).ToList();
        using var dialog = new ContractSignedDialog(
            career, offer, category,
            CareerLadder.Current(career, contentIndex.Cars),
            gareDaScegliere, calendario);
        dialog.ShowDialog(this);
        DopoLaFirma();
    }

    /// <summary>
    /// Le due scene che seguono una firma: la prima volta in assoluto, e
    /// l'apertura del campionato che quella firma ha aperto.
    ///
    /// Sta in un metodo suo perche' i sedili si firmano da due strade diverse
    /// — il sedile cliente e il contratto vero — e la scena deve arrivare da
    /// entrambe.
    /// </summary>
    private void DopoLaFirma()
    {
        career.Firsts ??= new CareerFirsts();
        if (!career.Firsts.Has(CareerFirsts.Contract))
        {
            RaccontaMomento(MomentoDiCarriera.PrimaFirma, CareerFirsts.Contract);
            return;
        }
        // Una volta per stagione: si apre un campionato all'anno.
        RaccontaMomento(MomentoDiCarriera.AperturaStagione, $"apertura-s{career.Season:00}");

        // E se questa firma ha portato al gradino piu' alto che i contenuti
        // installati permettono, e' il momento che tutta la carriera aspettava.
        var gradino = CareerLadder.Current(career, contentIndex.Cars).Step;
        var vetta = CareerLadder.PopulatedSteps(contentIndex.Cars).LastOrDefault();
        if (vetta > 0 && gradino >= vetta)
            RaccontaMomento(MomentoDiCarriera.ArrivoAlVertice, "vertice");
        else if (gradino > 1)
            RaccontaMomento(MomentoDiCarriera.CambioCategoria, $"categoria-{gradino}");
    }

    private void AnnounceContractSigned(TeamOffer offer, string category)
    {
        var rounds = CareerScheduler.ChampionshipRounds(career.Schedule, career.Season)
            .Where(x => x.IsPlanned).OrderBy(x => x.Date).ToList();
        var first = rounds.FirstOrDefault();
        var lines = new List<string>
        {
            $"{career.Driver} firma con {career.Team}.",
            "",
            // Le due altezze, insieme: su cosa si corre e contro chi. Firmando
            // si cambia spesso solo una delle due, e senza entrambe non si
            // capisce se il contratto e' una promozione o un passo di lato.
            $"CATEGORIA   {category} · gradino {CareerLadder.Current(career, contentIndex.Cars).Step} di {CareerLadder.Steps}",
            $"CAMPIONATO  {career.Championship} · livello {ChampionshipLadder.Clamp(career.ChampionshipLevel)} di {ChampionshipLadder.Levels}",
            $"VETTURA     {UiText.Car(offer.Car)}",
            $"COMPAGNO    {career.Teammate}",
            $"CONTRATTO   {offer.Years} anno/i · " + (offer.Salary > 0
                ? $"€ {offer.Salary:N0}/anno"
                : "nessuno stipendio: si corre per farsi vedere"),
            $"OBIETTIVO   {career.ContractObjective}",
            ""
        };
        if (rounds.Count > 0)
        {
            lines.Add($"CALENDARIO  {rounds.Count} round confermati");
            if (first != null)
                lines.Add($"            si comincia a {(string.IsNullOrWhiteSpace(first.TrackName) ? first.TrackId : first.TrackName)}, " +
                          $"{NarrativeCalendar.Format(first.Date)}");
        }
        else lines.Add("CALENDARIO  nessun round generabile con i circuiti installati");
        lines.Add("");
        lines.Add($"CASSA       € {career.Cash:N0}");
        lines.Add("");
        lines.Add("Da qui in poi ogni gara assegna punti e muove la classifica.");
        // I collaudi UI invocano la firma direttamente per verificare che il
        // calendario venga realmente creato. Non devono restare bloccati da
        // finestre modali invisibili: nel gioco normale il riepilogo resta
        // invece parte della narrazione subito dopo la firma.
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1")
        {
            // La scena della firma al posto della finestrella di testo. Il
            // riepilogo qui sopra resta: finisce nel registro della carriera,
            // dove serve a ricostruire cosa e' stato firmato e quando.
            CareerLog.Info("mercato", string.Join(" · ", lines.Where(x => x.Length > 0)));
            AnnounceSeatSigned(offer, category, 0);
            OpenCareerArticle(career.Events.LastOrDefault(x => x.Type is "TEAM_CHANGE" or "CONTRACT_SIGNING"));
        }
    }
    private void OpenSettings()
    {
        if (awaitingResult) { CareerMessages.Show(null, "Le impostazioni restano disponibili, ma il weekend pendente deve mantenere la propria identità fino all’importazione del risultato.", "CorsaCareer — weekend in corso", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        using var dialog = new SettingsDialog(career, () => { SaveCareer(); RefreshUi(); });
        dialog.ShowDialog(this);
        RefreshUi();
    }

    private void OpenMedia()
    {
        using var dialog = new MediaDialog(career, () => SaveCareer()); dialog.ShowDialog(this); RefreshUi();
    }
    private bool BlockIfPending(string operation)
    {
        if (!awaitingResult) return false;
        CareerMessages.Show(null, $"{operation} bloccato: c’è un weekend reale ancora pendente. Completa la sessione in Assetto Corsa oppure annullala dal pulsante del weekend prima di modificare la carriera.", "CorsaCareer — sessione reale pendente", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return true;
    }
    private void OpenCareerSlots()
    {
        if (awaitingResult) { CareerMessages.Show(null, "Completa prima il weekend reale già aperto in Content Manager.", "Carriere salvate", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var dialog = new CareerSlotsDialog(Path.Combine(saveDir, "careers"), career, state =>
        {
            career = state;
            career.Results ??= []; career.News ??= []; career.Events ??= []; career.RaceHistory ??= []; career.Drivers ??= [];
            career.Standings ??= []; career.Rivalries ??= []; career.SeasonArchive ??= []; career.Offers ??= []; career.StoryArcs ??= [];
            career.TeamHistory ??= []; career.Journalist ??= new JournalistProfile();
            career.Livery ??= "";
            if (string.IsNullOrWhiteSpace(career.Livery))
            {
                var savedCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
                if (savedCar != null) career.Livery = savedCar.Skins.FirstOrDefault() ?? "";
            }
            EnsureEntryLevelCareer(); EnsurePaddockRoster(); rounds = BuildRounds(); SaveCareer(); RefreshUi();
        }, Path.Combine(saveDir, "backups")); dialog.ShowDialog(this);
    }
    private void OpenPaddock()
    {
        NarrateCardEssentials("paddock|" + career.StoryDate.ToString("O"), $"Paddock e persone. Team attuale: {career.Team}. Compagno: {career.Teammate}. La fiducia della squadra è {career.TeamRelation} su 100. Qui puoi leggere relazioni, rivali e stato del garage.");
        using var dialog = new PaddockDialog(career); dialog.ShowDialog(this);
    }
    private void OpenCalendar()
    {
        var next = NextScheduled();
        NarrateCardEssentials("calendar|" + (next?.Id ?? career.StoryDate.ToString("O")), next == null
            ? "Calendario della carriera. Non ci sono ancora appuntamenti programmati."
            : $"Calendario della carriera. Il prossimo appuntamento è {next.Kind} a {next.TrackName}, il {next.Date:dd MMMM yyyy}. Obiettivo: {next.Objective}.");
        using var dialog = new CalendarDialog(career, rounds, contentIndex.Tracks); dialog.ShowDialog(this);
    }
    private void ToggleNarration()
    {
        if (NarrationService.IsSpeaking)
        {
            NarrationService.Stop();
            narrationControl.Text = "▶  AVVIA RUBRICA TV";
            return;
        }
        NarrateCurrentStory();
        narrationControl.Text = "❚❚  PAUSA / FERMA RUBRICA";
    }

    private void NarrateCurrentStory()
    {
        var latestEvent = career.Events.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        if (career.PreferredVoice.StartsWith("Browser", StringComparison.OrdinalIgnoreCase))
        {
            NarrationService.OpenBrowserPortal(CareerArticleBuilder.Build(career, latestEvent));
            return;
        }
        if (latestEvent != null)
        {
            NarrationService.Speak(AudioCommentaryBuilder.ForEvent(career, latestEvent).Text, career.PreferredVoice);
            return;
        }
        var date = career.StoryDate.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
        var roundText = career.Round < rounds.Count ? $" Siamo alla vigilia di {rounds[career.Round].GrandPrix}." : " Il campionato è al momento concluso.";
        var script = $"Rubrica Corsa Career. È {date}. Dal paddock, {career.Driver} corre con {career.Team}.{roundText} {career.Headline} La redazione seguirà ogni sviluppo con dati e risultati realmente registrati in Assetto Corsa.";
        NarrationService.Speak(script, career.PreferredVoice);
    }
    private DateTime StoryStartDateForCar(string car)
    {
        var normalized = car.ToLowerInvariant();
        var known = normalized switch
        {
            var x when x.Contains("787b") => 1991,
            var x when x.Contains("312t") => 1975,
            var x when x.Contains("98t") => 1986,
            var x when x.Contains("f2004") => 2004,
            var x when x.Contains("458") || x.Contains("599") => 2009,
            var x when x.Contains("sf70") => 2017,
            var x when x.Contains("sf90") => 2019,
            _ => 0
        };
        if (known == 0)
        {
            var match = Regex.Match(normalized, "(?<!\\d)(19|20)\\d{2}(?!\\d)");
            if (match.Success && int.TryParse(match.Value, out var parsed)) known = parsed;
        }
        // Nessun anno ricavabile: si usa il default dichiarato, non l'anno di
        // sistema, così un salvataggio non dipende dall'orologio della macchina.
        if (known == 0) known = NarrativeCalendar.DefaultStartYear;
        return new DateTime(Math.Clamp(known, 1970, 2100), 1, 1);
    }
    private DateTime StoryStartDateForContent(ContentCarRecord? content)
    {
        if (content?.ModelYear is >= 1970 and <= 2100) return new DateTime(content.ModelYear, 1, 1);
        return StoryStartDateForCar(content?.Id ?? "");
    }
    private void RefreshContents()
    {
        ReloadContentAndAlignCareer();
        if (career.Offers.Count == 0 && !career.ContractActive) career.Offers = BuildOffers();
        SaveCareer(); RefreshUi();
        var categories = string.Join(", ", contentIndex.Cars.GroupBy(x => x.Category).OrderByDescending(x => x.Count()).Take(5).Select(x => $"{x.Key} {x.Count()}"));
        var raceableCount = ContentAvailability.RaceableCars(contentIndex.Cars);
        var availability = ContentAvailability.HomeStatus(contentIndex.Cars, rounds.Count);
        var missingHint = string.IsNullOrWhiteSpace(availability)
            ? "\nContenuti sufficienti per preparare un weekend reale."
            : $"\nATTENZIONE: {availability}.\nInstalla o abilita il contenuto in Content Manager, poi premi nuovamente Aggiorna contenuti. Nessun download automatico viene eseguito senza una fonte verificata.\n";
        var metadataNotes = contentIndex.ScanWarnings.Count(x => x.Contains("JSON non standard", StringComparison.OrdinalIgnoreCase));
        var blockingWarnings = contentIndex.ScanWarnings.Count - metadataNotes;
        var scanWarnings = contentIndex.ScanWarnings.Count == 0 ? "Nessun avviso di lettura." : "NOTE/AVVISI DI SCANSIONE:\n" + string.Join("\n", contentIndex.ScanWarnings.Take(8)) + (contentIndex.ScanWarnings.Count > 8 ? "\n…" : "");
        var scanIcon = string.IsNullOrWhiteSpace(availability) && blockingWarnings == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
        CareerMessages.Show(null, $"Scansione completata.\n\nCircuiti/layout: {rounds.Count}\nAuto installate: {contentIndex.Cars.Count}\nAuto da gara: {raceableCount}\nCategorie rilevate: {(categories.Length == 0 ? "nessuna" : categories)}\n{missingHint}\n{scanWarnings}\n\nLa carriera esistente è stata conservata.", "Contenuti Assetto Corsa", MessageBoxButtons.OK, scanIcon);
        using var review = new ContentReviewDialog(contentIndex, SaveContentIndex, ReloadContentAndAlignCareer, OpenContentManagerHome, ChooseAssettoCorsaRoot);
        review.ShowDialog(this);
        RefreshUi();
    }
    private void ReloadContentAndAlignCareer()
    {
        // Ogni nuova scansione può cambiare la categoria d'ingresso, la griglia
        // e il calendario. Riallineiamo prima la carriera, poi rigeneriamo i
        // round usando l'eventuale vettura appena installata.
        rounds = BuildRounds();
        EnsureEntryLevelCareer();
        EnsurePaddockRoster();
        career.Offers ??= new List<TeamOffer>();
        career.Offers.RemoveAll(offer => !contentIndex.Cars.Any(car => car.Id.Equals(offer.Car, StringComparison.OrdinalIgnoreCase) && ContentCategoryRules.IsRaceable(car)));
        if (career.Offers.Count == 0 && !career.ContractActive) career.Offers = BuildOffers();
        rounds = BuildRounds();
        SaveCareer();
        RefreshUi();
    }
    private void ChooseAssettoCorsaRoot()
    {
        using var picker = new FolderBrowserDialog { Description = "Seleziona la cartella principale di Assetto Corsa (deve contenere content\\cars o content\\tracks)." };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        var root = picker.SelectedPath;
        if (!Directory.Exists(Path.Combine(root, "content", "cars")) && !Directory.Exists(Path.Combine(root, "content", "tracks")))
        {
            CareerMessages.Show(null, "La cartella selezionata non contiene content\\cars o content\\tracks.", "Radice Assetto Corsa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        contentIndex.AssettoCorsaRoot = root;
        SaveContentIndex(); ReloadContentAndAlignCareer();
        CareerMessages.Show(null, $"Radice salvata e scansione aggiornata.\n\n{root}\n\nCircuiti: {contentIndex.Tracks.Count}\nAuto: {contentIndex.Cars.Count}", "Radice Assetto Corsa", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void OpenContentManagerHome()
    {
        var path = LocateContentManager();
        if (string.IsNullOrWhiteSpace(path))
        {
            CareerMessages.Show(null, "Content Manager non è stato trovato. Installa o collega Content Manager, poi riprova.", "CorsaCareer — Content Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch (Exception error) { CareerMessages.Show(null, $"Impossibile aprire Content Manager: {error.Message}", "CorsaCareer — Content Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    /// <summary>
    /// L'impegno che il team chiede, detto nei termini della categoria in cui
    /// si corre davvero.
    ///
    /// L'obiettivo veniva copiato dalla proposta accettata e non cambiava mai
    /// piu': in Formula internazionale il portale chiedeva ancora «gara kart
    /// club: chiudere davanti almeno a meta' gruppo», cioe' l'impegno preso
    /// anni prima su un kart a noleggio.
    /// </summary>
    private void AggiornaObiettivoDiContratto()
    {
        if (!career.ContractActive) return;
        var gradino = CareerLadder.Current(career, contentIndex.Cars);
        var obiettivo = gradino.Step switch
        {
            <= 2 => "Chiudere davanti a meta' gruppo e portare a casa ogni gara",
            3 => "Salire sul podio almeno una volta nella stagione",
            4 => "Entrare stabilmente nei primi cinque",
            5 => "Giocarsi il campionato fino all'ultimo round",
            6 => "Vincere gare e chiudere nei primi tre in classifica",
            _ => "Il titolo, niente di meno"
        };
        if (string.Equals(career.ContractObjective, obiettivo, StringComparison.Ordinal)) return;
        career.ContractObjective = obiettivo;
        career.ContractObjectiveStatus = "In corso";
    }

    private void AdvanceSeason()
    {
        // Chi si e' ritirato non corre piu': niente calendari, niente offerte,
        // niente stagioni nuove. Senza questo la carriera ripartiva da sola il
        // giorno dopo il ritiro.
        if (career.Retired) return;
        if (awaitingResult) { CancelPendingWeekend(); return; }
        // Restano gare da correre in questa stagione? Allora non si avanza.
        //
        // Prima si confrontava il contatore dei round con la lunghezza della
        // lista, ma il contatore viene azzerato a ogni sedile mentre la lista
        // conserva i round gia corsi: la condizione era sempre vera e il
        // passaggio di stagione non avveniva mai.
        var roundQuestaStagione = CareerScheduler.ChampionshipRounds(career.Schedule ?? [], career.Season);
        if (roundQuestaStagione.Any(x => x.IsPlanned)) return;
        // Conta l'archivio, non la lista dei round: una lista vuota a fine
        // stagione e' normale — le gare corse vengono chiuse e tolte — e
        // pretendere che ci siano ancora round bloccava il passaggio proprio
        // quando la stagione era regolarmente finita.
        if (!career.SeasonArchive.Any(x => x.Season == career.Season))
        {
            // Anche un anno senza campionato e' una stagione.
            //
            // L'archivio nasceva solo dal premio finale di un campionato: un
            // pilota cliente, che compra i weekend uno alla volta, non ne
            // chiudeva mai uno e restava per sempre nella stessa stagione. Nel
            // banco di prova la «stagione 1» arrivava a sessantanove gare e
            // copriva tre anni, senza classifiche e senza mercato invernale.
            // Se l'anno sportivo e' finito e si e' corso, la stagione si
            // archivia per quello che e' stata: gare e vittorie, nessuna
            // classifica finale.
            var gareDellAnno = career.RaceHistory.Where(x => x.Season == career.Season).ToList();
            var annoFinito = career.StoryDate.Date > NarrativeCalendar.LastRaceSundayOfSeason(SeasonAnchor().Year);
            if (gareDellAnno.Count > 0 && annoFinito)
            {
                career.SeasonArchive.Add(new SeasonSummary
                {
                    Season = career.Season, Championship = career.Championship, Tier = career.Tier,
                    CompletedUtc = DateTime.UtcNow, Points = career.Points,
                    Wins = gareDellAnno.Count(x => !x.Dnf && x.Position == 1),
                    FinalPosition = 0, Award = 0, TitleWon = false, WorldTitle = false
                });
                CareerLog.Info("stagione", $"stagione {career.Season} archiviata senza campionato: {gareDellAnno.Count} gare comprate.");
            }
            else
            {
                CareerMessages.Show(null, "Il passaggio di stagione è bloccato: manca il premio finale generato dall’ultima gara realmente importata. Completa e importa l’ultimo weekend in Assetto Corsa prima di avanzare.", "CorsaCareer - stagione non conclusa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        var oldTier = career.Tier;
        var lastSeason = career.SeasonArchive.Last(x => x.Season == career.Season);
        // La promozione non dipende più da una sola soglia di reputazione: viene
        // valutata su reputazione, classifica, risultati reali, confronto col
        // compagno, obiettivi rispettati e budget, e la pagella resta consultabile.
        var assessment = ProgressionEngine.Assess(new ProgressionInput
        {
            Reputation = career.Reputation,
            FinalChampionshipPosition = lastSeason.FinalPosition,
            ChampionshipFieldSize = Math.Max(1, career.Standings.Count),
            Wins = career.RaceHistory.Count(x => x.Season == career.Season && !x.Dnf && x.Position == 1),
            Podiums = career.RaceHistory.Count(x => x.Season == career.Season && !x.Dnf && x.Position <= 3),
            SeasonRaces = career.RaceHistory.Count(x => x.Season == career.Season),
            TeammateRacesWon = career.TeammateRacesWon,
            TeammateRacesLost = career.TeammateRacesLost,
            SponsorObjectiveMet = career.SponsorObjectiveStatus == "Raggiunto",
            ContractObjectiveMet = career.ContractObjectiveStatus == "Raggiunto",
            Cash = career.Cash,
            CurrentTier = oldTier
        });
        var installedTiers = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).Select(x => TierForCategory(x.Category)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var nextAvailableTier = ProgressionEngine.NextAvailableTier(oldTier, installedTiers);
        var choices = ProgressionEngine.BuildChoices(oldTier, career.Team, nextAvailableTier, career.Reputation, career.ContractSalary, assessment.Ready);
        TierChoice? decision = null;
        if (choices.Count > 1 && !CareerMessages.Unattended)
        {
            using var dialog = new PromotionDialog(career, assessment, choices);
            decision = dialog.ShowDialog(this) == DialogResult.OK ? dialog.Selected : choices[0];
        }
        else
        {
            decision = choices[0];
            CareerMessages.Show(null, $"{assessment.Verdict}\n\n{assessment.Explain()}\n\n{(string.IsNullOrWhiteSpace(nextAvailableTier) ? "Non esiste una categoria superiore fra i contenuti installati: la promozione non viene promessa." : "La categoria superiore resta chiusa per questa stagione.")}", "CorsaCareer — pagella di fine stagione", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        decision ??= choices[0];
        career.Season++;

        // Il conto della stagione che comincia.
        //
        // Correre costa ogni anno: iscrizione al campionato, gomme, meccanici,
        // trasferte. Il programma conosceva questa cifra e la usava solo per
        // PREZZARE i sedili in vendita: chi un sedile ce l'aveva già non pagava
        // più niente, e i premi si accumulavano senza freno — nel banco di
        // prova cinque milioni in cassa a metà scala, con nessuna decisione
        // economica che contasse più qualcosa. Quello che il team copre
        // (TeamSupportPercent) resta coperto: un sedile professionistico non
        // costa nulla al pilota, ed è la ricompensa di esserci arrivato.
        var gradinoStagione = CareerLadder.Current(career, contentIndex.Cars).Step;
        var quotaStagione = CareerFinances.SeasonEntryFeeForStep(gradinoStagione);
        var aCaricoDelPilota = (int)Math.Round(quotaStagione * (100 - Math.Clamp(career.TeamSupportPercent, 0, 100)) / 100.0);
        if (aCaricoDelPilota > 0)
        {
            var pagamento = CareerWallet.TryPay(career, aCaricoDelPilota, $"Iscrizione e gestione stagione {career.Season}");
            var rigaCosti = pagamento.Paid
                ? $"Stagione {career.Season}: pagati € {aCaricoDelPilota:N0} fra iscrizione, gomme e gestione. In cassa restano € {career.Cash:N0}."
                : $"Stagione {career.Season}: servono € {aCaricoDelPilota:N0} per iscrizione e gestione e in cassa ce ne sono € {career.Cash:N0}. Il debito resta aperto con la squadra.";
            career.News.Add(rigaCosti);
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SEASON_COSTS",
                Headline = rigaCosti, Track = career.Team, Importance = 58
            });
            CareerLog.Info("economia", $"quota di stagione {career.Season}: € {aCaricoDelPilota:N0} (copertura team {career.TeamSupportPercent}%) · pagata: {pagamento.Paid}");
        }
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "PROGRESSION_REVIEW", Headline = $"Fine stagione: {assessment.Verdict}", Track = "Paddock", Importance = 70 });

        var promoted = !decision.Tier.Equals(oldTier, StringComparison.OrdinalIgnoreCase);
        career.Tier = decision.Tier;
        career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel);
        career.SeatPrestige = decision.Prestige;
        career.ContractObjective = decision.Objective;
        career.ContractObjectiveStatus = "In corso";
        if (promoted)
        {
            // Anche la promozione di fine stagione rispetta la scala.
            //
            // Qui si prendeva la prima vettura del tier in ordine alfabetico,
            // senza guardare il gradino: era la quarta strada per cui una
            // carriera poteva saltare due categorie in un colpo, o tornare
            // indietro, o passare in monoposto senza aver fatto la gavetta.
            var gradinoPrima = CareerLadder.Current(career, contentIndex.Cars).Step;
            var gavettaFatta = RacesOnCurrentStep() >= OpportunityGenerator.RacesBeforeStepUp(gradinoPrima);
            var promotedCar = contentIndex.Cars
                .Where(ContentCategoryRules.IsRaceable)
                .Where(x =>
                {
                    var passo = CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step;
                    if (!CareerLadder.WithinReach(passo, gradinoPrima, contentIndex.Cars)) return false;
                    return passo <= gradinoPrima || gavettaFatta;
                })
                .OrderBy(x => x.Name)
                .FirstOrDefault(x => TierForCategory(x.Category).Equals(career.Tier, StringComparison.OrdinalIgnoreCase));
            if (promotedCar != null)
            {
                var fromTeam = career.Team;
                career.Car = promotedCar.Id; career.Livery = promotedCar.Skins.FirstOrDefault() ?? "";
                career.Team = career.Tier == "Formula / top tier" ? "Minato Apex Racing" : "Kaido Motorsport";
                if (!string.IsNullOrWhiteSpace(career.Teammate)) career.TeamHistory.Add(new TeamHistoryEntry { StoryDate = career.StoryDate, FromTeam = fromTeam, ToTeam = career.Team, Car = promotedCar.Id, Category = promotedCar.Category, Reason = $"Ex compagno: {career.Teammate}" });
                career.Teammate = career.Tier == "Formula / top tier" ? "Mei Kanzaki" : "Sota Fujimoto";
                career.ContractSalary = decision.Salary;
                RegisterTeamChange(fromTeam, career.Team, career.Car, promotedCar.Category, "Promozione scelta dal pilota a fine stagione");
            }
        }
        else career.ContractSalary = decision.Salary;
        career.TeammateRacesWon = 0; career.TeammateRacesLost = 0;
        career.DaysUntilNextRound = OffTrackActivities.DaysBetweenRounds();
        // La pausa fra due stagioni permette un recupero reale della condizione.
        career.Fatigue = Math.Max(0, career.Fatigue - 40);
        AggiornaObiettivoDiContratto();
        AccumulaTotaliDiCarriera();
        career.Round = 0; career.Points = 0; career.Standings.Clear(); career.SalaryPaid = 0; career.PrizeMoney = 0; career.SponsorMoney = 0; career.RepairCosts = 0; career.LogisticsCosts = 0; career.SponsorQualifyingResults = 0; career.SponsorObjectiveStatus = "In corso";
        career.ContractYears = Math.Max(0, career.ContractYears - 1); career.ContractActive = career.ContractYears > 0;
        if (!career.ContractActive)
        {
            career.Offers = BuildOffers();
            var marketHeadline = $"Il contratto con {career.Team} è terminato: il mercato apre nuove trattative per {career.Driver}.";
            career.News.Add(marketHeadline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CONTRACT_EXPIRED", Headline = marketHeadline, Track = "Mercato piloti", Importance = 80 });
        }
        var promotion = promoted ? $" Promozione scelta: {career.Tier}." : $" Il pilota resta in {career.Tier} da prima guida.";
        career.Headline = $"Inizia la stagione {career.Season} per {career.Driver}.{promotion}"; career.News.Add(career.Headline);
        // Nuova ancora di calendario: le date dei round della stagione entrante
        // partono da qui e non si spostano più a ogni risultato importato.
        career.SeasonStartDate = NarrativeCalendar.NextSeasonStart(SeasonAnchor(), rounds.Count);
        career.StoryDate = career.SeasonStartDate.AddDays(-NarrativeCalendar.DaysBetweenRounds);
        // Calendario della stagione entrante: circuiti ruotati, quindi diverso da
        // quello appena concluso.
        GenerateSeasonSchedule($"Apertura della stagione {career.Season}");
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = oldTier != career.Tier ? "PROMOTION" : "NEW_SEASON", Headline = career.Headline, Track = career.Championship, Importance = oldTier != career.Tier ? 95 : 60 });
        SaveCareer(); RefreshUi();
    }
    private string TierForReputation(int reputation)
    {
        var available = contentIndex.Cars.Select(x => TierForCategory(x.Category)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (available.Count == 0) return "Rookie";
        var allowedRank = reputation >= 70 ? 3 : reputation >= 45 ? 2 : reputation >= 25 ? 1 : 0;
        var selected = available.Select(tier => (tier, rank: tier switch { "Rookie" => 0, "Categoria regionale" => 1, "Categoria avanzata" => 2, "Formula / top tier" => 3, _ => 0 }))
            .Where(x => x.rank <= allowedRank).OrderByDescending(x => x.rank).FirstOrDefault();
        if (selected == default) return available.OrderBy(tier => tier switch { "Rookie" => 0, "Categoria regionale" => 1, "Categoria avanzata" => 2, _ => 3 }).First();
        return selected.tier;
    }
    public static string TierForCategory(string category)
    {
        if (new[] { "formula", "GT1", "hypercar", "LMP" }.Contains(category, StringComparer.OrdinalIgnoreCase)) return "Formula / top tier";
        if (new[] { "formula 2", "formula 3", "GT3", "GT2", "prototype" }.Contains(category, StringComparer.OrdinalIgnoreCase)) return "Categoria avanzata";
        if (new[] { "formula 4", "formula junior", "GT4", "TCR", "touring", "cup" }.Contains(category, StringComparer.OrdinalIgnoreCase)) return "Categoria regionale";
        return "Rookie";
    }
    private string CategoryForCareer(CareerState state)
    {
        if (!string.IsNullOrWhiteSpace(state.Car))
            return contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(state.Car, StringComparison.OrdinalIgnoreCase))?.Category ?? "";
        return "";
    }
    // ChampionshipForTier è stato rimosso: il nome del campionato non nasce
    // più dalla categoria della vettura ma dall'altezza raggiunta nella scala
    // (ChampionshipLadder). Si può correre in kart a livello di zona o
    // nazionale, ed erano due cose che il vecchio nome non distingueva.
    /// <summary>
    /// Rilegge la gara appena archiviata cercando i momenti che valgono una
    /// storia, e ne applica le conseguenze su forma e seguito.
    /// </summary>
    private void ApplyRaceHighlights(RaceHistoryEntry gara, bool decisivaPerIlTitolo)
    {
        // Prima gara con questa vettura: l'esordio in una categoria è uno dei
        // momenti che pesano di più.
        var primaInCategoria = (career.RaceHistory ?? [])
            .Count(x => !string.IsNullOrWhiteSpace(x.Car)
                        && x.Car.Equals(gara.Car, StringComparison.OrdinalIgnoreCase)) <= 1;
        var momenti = RaceHighlights.For(gara, primaInCategoria, decisivaPerIlTitolo);
        if (momenti.Count > 0) RaceHighlights.Apply(career, momenti);
    }

    /// <summary>
    /// Vero se il campionato è ancora aperto: servono round da correre e una
    /// posizione che permetta ancora di vincerlo. Un ritiro qui costa il
    /// titolo, e la carriera se ne ricorda.
    /// </summary>
    private bool TitoloAncoraInGioco()
    {
        if (rounds.Count == 0 || career.Round >= rounds.Count) return false;
        var restanti = rounds.Count - career.Round;
        if (restanti > 3) return false;
        var classifica = career.Standings?.OrderByDescending(x => x.Points).ToList() ?? [];
        if (classifica.Count == 0) return false;
        var mio = classifica.FindIndex(x => AiDriverIdentity.NamesEqual(x.Driver, career.Driver));
        return mio >= 0 && mio <= 2;
    }

    private void RegisterTeamChange(string fromTeam, string toTeam, string car, string category, string reason)
    {
        career.TeamHistory ??= new List<TeamHistoryEntry>();
        career.TeamHistory.Add(new TeamHistoryEntry { StoryDate = career.StoryDate, FromTeam = fromTeam, ToTeam = toTeam, Car = car, Category = category, Reason = reason });
    }
    /// <summary>
    /// Chiude un weekend pendente. Il giocatore deve dichiarare se si sta
    /// ritirando — e allora il round o il tentativo viene consumato — oppure se
    /// la sessione non è mai partita per un problema tecnico. L'applicazione non
    /// può distinguere i due casi da sola, quindi la scelta è esplicita e
    /// l'annullamento tecnico resta contato e visibile nel dossier.
    /// </summary>
    private void CancelPendingWeekend()
    {
        var pendingPath = Path.Combine(saveDir, "pending_weekend.json");
        var isTest = pendingMode.Equals("test", StringComparison.OrdinalIgnoreCase);
        var grandPrix = career.Round < rounds.Count ? rounds[career.Round].GrandPrix : career.Championship;
        if (!File.Exists(pendingPath)) { awaitingResult = false; pendingResultHash = ""; RefreshUi(); return; }

        var answer = CareerMessages.Ask(null, 
            WithdrawalRules.ConfirmationText(isTest, grandPrix, career.Tier) +
            "\n\nSÌ = ritiro (conta)\nNO = annullamento tecnico (non conta, resta registrato)\nANNULLA = torna indietro",
            isTest ? "CorsaCareer — abbandono della prova" : "CorsaCareer — ritiro dal weekend",
            MessageBoxButtons.YesNoCancel, DialogResult.Cancel);
        if (answer == DialogResult.Cancel) return;
        var kind = answer == DialogResult.Yes ? WithdrawalKind.DriverWithdrawal : WithdrawalKind.TechnicalAnnulment;

        var archive = Path.Combine(saveDir, $"cancelled_weekend-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        File.Move(pendingPath, archive, true);
        awaitingResult = false; launchTimeUtc = DateTime.MinValue; pendingResultHash = "";
        if (isTest) RegisterWithdrawnTest(kind, grandPrix);
        else RegisterWithdrawnRace(kind, grandPrix);
        SaveCareer(); RefreshUi();
    }

    /// <summary>
    /// Registra una prova non completata. Non viene inventato nessun tempo: la
    /// valutazione resta aperta, ma il tentativo è speso.
    /// </summary>
    private void RegisterWithdrawnTest(WithdrawalKind kind, string track)
    {
        var attempt = career.EvaluationAttempts + 1;
        var outcome = WithdrawalRules.ForTest(kind, attempt);
        if (outcome.ConsumesAttempt)
        {
            career.EvaluationAttempts++;
            career.AbandonedSessions++;
            career.Reputation = Math.Clamp(career.Reputation + outcome.Reputation, 0, 100);
            career.TeamRelation = Math.Clamp(career.TeamRelation + outcome.TeamRelation, 0, 100);
            if (career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase))
                career.RookieEvaluationStatus = "In corso — prova non completata";
        }
        else career.TechnicalAnnulments++;

        career.Headline = outcome.Headline;
        career.News.Add(outcome.Headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = outcome.EventType,
            Headline = outcome.Headline, Track = string.IsNullOrWhiteSpace(track) ? "Paddock" : track, Importance = outcome.Importance
        });
        // Anche una sessione senza tempo utile chiude l'appuntamento corrente:
        // il risultato è negativo, ma il percorso non resta bloccato sullo
        // stesso test. L'agenda propone il passo successivo coerente con il
        // tentativo realmente registrato.
        if (outcome.ConsumesAttempt && career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase))
        {
            var target = career.EvaluationTargetMilliseconds <= 0 ? RookieTargetEngine.FallbackTargetMilliseconds : career.EvaluationTargetMilliseconds;
            AdvanceScheduleAfterEvaluation(RookieTargetEngine.Evaluate(0, target), track);
        }
        CareerLog.Info("sessione", $"prova non completata ({kind}): {outcome.Explain()}");
        SaveCareer();
        RefreshUi();
        CareerMessages.Show(null, $"{outcome.Headline}\n\n{outcome.Explain()}", "CorsaCareer — prova non completata", MessageBoxButtons.OK, MessageBoxIcon.Information);
        OpenCareerArticle(career.Events.LastOrDefault(x => x.Type == outcome.EventType));
    }

    /// <summary>
    /// Registra un weekend di gara non concluso. Il round viene consumato e va a
    /// referto come ritiro, senza posizione d'arrivo: non esiste un risultato da
    /// attribuire, ma esiste il fatto che il weekend è stato disputato.
    /// </summary>
    private void RegisterWithdrawnRace(WithdrawalKind kind, string grandPrix)
    {
        var outcome = WithdrawalRules.ForRace(kind, career.Tier, grandPrix, career.Round + 1);
        if (!outcome.ConsumesAttempt)
        {
            career.TechnicalAnnulments++;
            career.Headline = outcome.Headline;
            career.News.Add(outcome.Headline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = outcome.EventType, Headline = outcome.Headline, Track = "Paddock", Importance = outcome.Importance });
            CareerLog.Info("sessione", $"weekend annullato per problema tecnico: {grandPrix}");
            CareerMessages.Show(null, $"{outcome.Headline}\n\n{outcome.Explain()}", "CorsaCareer — annullamento tecnico", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        career.AbandonedSessions++;
        career.Reputation = Math.Clamp(career.Reputation + outcome.Reputation, 0, 100);
        career.TeamRelation = Math.Clamp(career.TeamRelation + outcome.TeamRelation, 0, 100);
        career.SponsorRelation = Math.Clamp(career.SponsorRelation + outcome.SponsorRelation, 0, 100);
        // Anche una spesa subita non puo portare la cassa sotto zero: si paga
        // quello che c'e, il resto resta un costo che non si e potuto sostenere.
        var logistics = Math.Min(outcome.LogisticsCost, Math.Max(0, career.Cash));
        career.Cash -= logistics;
        career.LogisticsCosts += logistics;

        var track = career.Round < rounds.Count ? rounds[career.Round].Track : "";
        var raceDate = NarrativeCalendar.RoundDate(career.Round, SeasonAnchor(), Math.Max(1, rounds.Count));
        career.StoryDate = raceDate;
        var plan = CurrentSessionPlan("race");
        // Posizione 0 e Abandoned: nessuna posizione d'arrivo viene attribuita.
        career.RaceHistory.Add(new RaceHistoryEntry
        {
            Season = career.Season, Round = career.Round + 1, DateUtc = DateTime.UtcNow, StoryDate = raceDate,
            Track = track, Car = career.Car, Position = 0, Dnf = true, Abandoned = true,
            Points = 0, Prize = 0, SponsorBonus = 0, Laps = 0,
            PlannedLaps = plan?.RaceLaps ?? 0, FormatLabel = plan?.FormatLabel ?? "",
            WeatherId = plan?.WeatherId ?? "", WeatherLabel = plan?.WeatherLabel ?? "",
            TemperatureC = plan?.TemperatureC ?? 0, TimeOfDaySeconds = plan?.TimeOfDaySeconds ?? 0,
            AiLevel = plan?.AiLevel ?? 0, SessionName = "Weekend non concluso",
            ImportedUtc = DateTime.UtcNow, SourceKind = "CORSACAREER_WITHDRAWAL",
            TeammateName = career.Teammate
        });
        career.Races++;
        career.Round++;
        var primaDelWeekend = career.StoryDate;
        career.StoryDate = NarrativeCalendar.DayAfterRound(career.Round - 1, SeasonAnchor());
        // Le settimane fra un weekend e l altro fanno recuperare il pilota.
        RecoverForElapsedDays(primaDelWeekend);
        career.DaysUntilNextRound = OffTrackActivities.DaysBetweenRounds();

        career.Headline = outcome.Headline;
        career.News.Add(outcome.Headline);
        career.Results.Add($"{grandPrix}: ritiro dal weekend (0 pt, nessun premio, trasferta € -{outcome.LogisticsCost:N0})");
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = raceDate, Type = outcome.EventType,
            Headline = outcome.Headline, Track = string.IsNullOrWhiteSpace(track) ? "Paddock" : track, Importance = outcome.Importance
        });
        UpdateStoryArcs();
        CareerLog.Info("sessione", $"ritiro dal weekend {grandPrix}: {outcome.Explain()}");
        CareerMessages.Show(null, $"{outcome.Headline}\n\n{outcome.Explain()}", "CorsaCareer — ritiro dal weekend", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    private void CompletePendingWeekend()
    {
        var pendingPath = Path.Combine(saveDir, "pending_weekend.json");
        if (File.Exists(pendingPath)) File.Move(pendingPath, Path.Combine(saveDir, $"completed_weekend-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json"), true);
        awaitingResult = false; launchTimeUtc = DateTime.MinValue; pendingResultHash = ""; lastRejectedResultSignature = "";
    }
    private void Record(ImportedRaceResult imported, string photoPath, string resultFile)
    {
        if (career.Round >= rounds.Count) return;
        EvaluateContractObjective(imported);
        var teammate = imported.Classification.FirstOrDefault(x => AiDriverIdentity.NamesEqual(x.Name, career.Teammate));
        RecordInternal(imported.Dnf ? 99 : imported.Position, imported.Track, imported.Car, imported.StartingPosition, imported.QualificationPosition, imported.SessionName, imported.Laps, imported.BestLapMilliseconds, imported.GapMilliseconds, imported.PitStops, imported.PenaltySeconds, imported.Damage, teammate?.Position ?? 0, teammate?.Name ?? career.Teammate, imported.Dnf, photoPath, resultFile, imported.Classification);
    }

    /// <summary>
    /// Registra le uscite del weekend attraverso il portafoglio unico. I ricavi
    /// vengono accreditati prima dei costi (sono il premio maturato dal referto),
    /// poi ogni costo viene pagato solo se sostenibile: il saldo non può mai
    /// diventare negativo e lo storico distingue il pagato dal non pagato.
    /// </summary>
    private (int LogisticsPaid, int DamagePaid, int Unpaid) SettleRaceCosts(int logisticsCost, int damageCost, string context)
    {
        var logistics = CareerWallet.TryPay(career, logisticsCost, $"{context} · trasferta");
        var damage = CareerWallet.TryPay(career, damageCost, $"{context} · riparazioni");
        var logisticsPaid = logistics.Paid ? logistics.Amount : 0;
        var damagePaid = damage.Paid ? damage.Amount : 0;
        var unpaid = (logistics.Paid ? 0 : Math.Max(0, logisticsCost)) + (damage.Paid ? 0 : Math.Max(0, damageCost));
        career.LogisticsCosts += logisticsPaid;
        career.RepairCosts += damagePaid;
        if (unpaid > 0)
        {
            career.News ??= new List<string>();
            career.News.Add($"{context}: costi non sostenibili per € {unpaid:N0}; la cassa resta a € {career.Cash:N0}.");
        }
        return (logisticsPaid, damagePaid, unpaid);
    }

    private void RecordInvitation(ImportedRaceResult imported, string photoPath, string resultFile)
    {
        var invitation = NextScheduled();
        if (invitation?.Kind != ScheduledEventKind.Invitation) return;
        var position = imported.Dnf ? 99 : Math.Max(1, imported.Position);
        var fieldSize = Math.Max(1, imported.Classification.Count);
        var plan = CurrentSessionPlan("invitation");
        var economy = EconomyEngine.ForRace(new RaceEconomyInput
        {
            Tier = career.Tier, LadderStep = CareerLadder.Current(career, contentIndex.Cars).Step, EmployedDriver = career.ContractActive && !career.IsClientDriver, Position = imported.Dnf ? 0 : position, FieldSize = fieldSize, Dnf = imported.Dnf,
            Damage = imported.Damage, ContractSalary = 0, RoundsInSeason = 1, SponsorBonus = 0,
            SponsorQualified = false, Endurance = plan?.Endurance ?? false
        });
        // La trasferta viene già pagata nella quota mostrata prima della
        // partenza: non va sottratta una seconda volta all'importazione.
        economy.LogisticsCost = 0;
        var reputation = ReputationEngine.Evaluate(new ReputationInput
        {
            Position = imported.Dnf ? 0 : position, FieldSize = fieldSize, Dnf = imported.Dnf,
            QualifyingPosition = imported.QualificationPosition, TeammatePosition = 0,
            CurrentReputation = career.Reputation, Tier = career.Tier,
            ExpectedPosition = Math.Max(3, (fieldSize + 1) / 2)
        });
        var cashBefore = career.Cash;
        var fitnessBefore = career.Fitness;
        var trustBefore = career.TeamRelation;
        // Mai indietro: se la carriera e' gia oltre la data dell'invito — un
        // contratto firmato, un round corso — quella data e' passata, e
        // riscriverla faceva tornare il diario nel passato.
        career.StoryDate = invitation.Date > career.StoryDate ? invitation.Date : career.StoryDate;
        career.Cash += economy.Prize;
        career.PrizeMoney += economy.Prize;
        var paid = SettleRaceCosts(economy.LogisticsCost, economy.DamageCost, $"Gara su invito a {invitation.TrackName}");
        career.Reputation = reputation.Reputation; career.Races++;
        if (!imported.Dnf && position == 1) career.Wins++;
        if (!imported.Dnf && position <= 3) career.Podiums++;
        career.Results.Add($"Gara su invito {invitation.TrackName}: {(imported.Dnf ? "ritiro" : $"P{position}")} (nessun punto campionato, premio € {economy.Prize:N0})");
        career.RaceHistory.Add(new RaceHistoryEntry
        {
            Season = career.Season, Round = 0, DateUtc = DateTime.UtcNow, StoryDate = invitation.Date,
            Track = imported.Track, Car = imported.Car, Position = position, StartingPosition = imported.StartingPosition,
            QualificationPosition = imported.QualificationPosition, SessionName = "Gara su invito", Laps = imported.Laps,
            BestLapMilliseconds = imported.BestLapMilliseconds, GapMilliseconds = imported.GapMilliseconds,
            PitStops = imported.PitStops, PenaltySeconds = imported.PenaltySeconds, Damage = imported.Damage,
            Dnf = imported.Dnf, Points = 0, Prize = economy.Prize, PhotoPath = photoPath, ResultFile = resultFile,
            ResultSha256 = HashFile(resultFile), ImportedUtc = DateTime.UtcNow,
            // La provenienza va letta, non decisa a priori: qui era scritta a
            // mano come referto reale, e una gara risolta dal programma veniva
            // archiviata — e mostrata — come se fosse stata guidata davvero.
            SourceKind = pendingSourceKind == ResultProvenance.Simulated
                ? ResultProvenance.Simulated
                : ResultProvenance.AssettoCorsaInvitation,
            FormatLabel = plan?.FormatLabel ?? "Gara su invito", WeatherId = plan?.WeatherId ?? "",
            WeatherLabel = plan?.WeatherLabel ?? "", TemperatureC = plan?.TemperatureC ?? 0,
            TimeOfDaySeconds = plan?.TimeOfDaySeconds ?? 0, PlannedLaps = plan?.RaceLaps ?? 0, AiLevel = plan?.AiLevel ?? 0,
            CashDelta = career.Cash - cashBefore, CashAfter = career.Cash,
            FitnessDelta = career.Fitness - fitnessBefore, FitnessAfter = career.Fitness,
            TrustDelta = career.TeamRelation - trustBefore, TrustAfter = career.TeamRelation,
            LogisticsPaid = paid.LogisticsPaid, DamagePaid = paid.DamagePaid, UnpaidCosts = paid.Unpaid,
            Classification = imported.Classification.Select(x => new RaceParticipantSnapshot { Name = x.Name, Car = x.Car, Position = x.Position, IsPlayer = x.IsPlayer }).ToList()
        });
        // Una gara su invito e una gara vera, e deve avere le stesse
        // conseguenze di un round di campionato.
        //
        // Prima aggiornava solo la reputazione grezza: popolarita, prestigio
        // sportivo e appeal presso gli sponsor restavano fermi. Il banco ha
        // trovato un pilota con nove vittorie, reputazione 100 e popolarita
        // ancora a 10 — quindi nessun ingaggio promozionale, nessuno sponsor
        // e nessuna promozione di categoria. Tutte le gare open passano da
        // qui, quindi era la strada normale a non produrre conseguenze.
        var invitationCar = string.IsNullOrWhiteSpace(imported.Car) ? career.Car : imported.Car;
        ConsequenceEngine.ApplyRaceResult(career, career.RaceHistory[^1], CarCompetitiveness(invitationCar));
        // Quello che la gara lascia oltre al piazzamento: rimonte, esordi,
        // giornate di pioggia. Muove forma e seguito, non solo la cassa.
        ApplyRaceHighlights(career.RaceHistory[^1], decisivaPerIlTitolo: false);
        RefreshOpportunities();
        CareerScheduler.Close(career.Schedule, invitation.Id);
        var strongResult = !imported.Dnf && position <= Math.Max(3, (int)Math.Ceiling(fieldSize / 3.0));
        if (strongResult)
        {
            career.RookieEvaluationStatus = "Invito sfruttato — mercato interessato";
            if (!career.ContractActive) career.Offers = BuildOffers();
            career.Headline = $"Gara su invito a {invitation.TrackName}: {career.Driver} chiude P{position} e rimette il proprio nome sul taccuino del mercato.";
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "INVITATION_BREAKTHROUGH", Headline = career.Headline, Track = imported.Track, Importance = 82, PhotoPath = photoPath, PhotoView = string.IsNullOrWhiteSpace(photoPath) ? "" : PhotoSource.View(photoPath) });
            // Anche il risultato buono deve lasciare qualcosa in agenda.
            //
            // Prima questo ramo si fermava alle offerte, e le offerte non sono
            // un appuntamento: chi non firmava restava senza niente da fare.
            // Era l'unico dei tre esiti a non programmare il seguito — cioe'
            // andare forte bloccava la carriera, andare piano no.
            var strongTarget = career.EvaluationTargetMilliseconds <= 0 ? RookieTargetEngine.FallbackTargetMilliseconds : career.EvaluationTargetMilliseconds;
            AdvanceScheduleAfterEvaluation(RookieTargetEngine.Evaluate(imported.BestLapMilliseconds, strongTarget), imported.Track);
        }
        else if (!imported.Dnf && position <= Math.Ceiling(fieldSize * 0.7))
        {
            // Il debutto onesto: hai finito, hai battuto qualcuno, nessuno ti
            // offre un sedile ma nessuno ti rimprovera. Prima questo caso non
            // esisteva e un P7 su 12 alla prima gara veniva archiviato come
            // "risultato insufficiente".
            career.RookieEvaluationStatus = "Prima gara chiusa — il paddock aspetta la conferma";
            career.Headline = $"Gara su invito a {invitation.TrackName}: {career.Driver} chiude P{position} su {fieldSize} al debutto. Nessun clamore, ma la gara è finita e qualcuno è rimasto dietro.";
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "INVITATION_DEBUT", Headline = career.Headline, Track = imported.Track, Importance = 70, PhotoPath = photoPath, PhotoView = string.IsNullOrWhiteSpace(photoPath) ? "" : PhotoSource.View(photoPath) });
            var debutTarget = career.EvaluationTargetMilliseconds <= 0 ? RookieTargetEngine.FallbackTargetMilliseconds : career.EvaluationTargetMilliseconds;
            AdvanceScheduleAfterEvaluation(RookieTargetEngine.Evaluate(imported.BestLapMilliseconds, debutTarget), imported.Track);
        }
        else
        {
            career.RookieEvaluationStatus = "Invito non sfruttato — serve un test di recupero";
            career.Headline = $"Gara su invito a {invitation.TrackName}: risultato insufficiente, il paddock rimanda {career.Driver} al lavoro di sviluppo.";
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "INVITATION_SETBACK", Headline = career.Headline, Track = imported.Track, Importance = 64, PhotoPath = photoPath, PhotoView = string.IsNullOrWhiteSpace(photoPath) ? "" : PhotoSource.View(photoPath) });
            var target = career.EvaluationTargetMilliseconds <= 0 ? RookieTargetEngine.FallbackTargetMilliseconds : career.EvaluationTargetMilliseconds;
            AdvanceScheduleAfterEvaluation(RookieTargetEngine.Evaluate(imported.BestLapMilliseconds, target), imported.Track);
        }
        career.News.Add(career.Headline);
        SaveCareer(); RefreshUi();

        // Come le prove e le gare di campionato: una gara su invito è un
        // evento della storia, non solo una riga nello stato. Prima finiva
        // qui — nessuna reazione del box, nessun articolo — e chi corriva
        // vedeva solo la cassa cambiare in silenzio.
        if (!CareerMessages.Unattended && Visible && !IsDisposed)
        {
            ChapterOneDialog.ShowRaceReactions(career, career.RaceHistory[^1], this, contentIndex.Cars, contentIndex.Tracks, career.Schedule ?? []);
            // Chiusa la scena, la Home riprende il proprio tema: senza questo
            // il brano della gara resterebbe addosso al portale.
            RefreshUi();
            OpenCareerArticle(career.Events.LastOrDefault(x => x.Type is "INVITATION_BREAKTHROUGH" or "INVITATION_DEBUT" or "INVITATION_SETBACK"));
        }
    }
    private void RecordTest(ImportedRaceResult imported, string photoPath, string resultFile)
    {
        career.TestHistory ??= new List<TestSessionRecord>();
        var testPlan = CurrentSessionPlan("test");
        career.TestHistory.Add(new TestSessionRecord { WeatherLabel = testPlan?.WeatherLabel ?? "", TemperatureC = testPlan?.TemperatureC ?? 0, TimeOfDaySeconds = testPlan?.TimeOfDaySeconds ?? 0, DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Track = imported.Track, Car = imported.Car, SessionName = imported.SessionName, Laps = imported.Laps, BestLapMilliseconds = imported.BestLapMilliseconds, PhotoPath = photoPath, ResultFile = resultFile, ResultSha256 = HashFile(resultFile), ImportedUtc = DateTime.UtcNow, SourceKind = pendingSourceKind });
        StoryCastService.Remember(career, StoryCastService.Mechanic, $"Ha ascoltato il test di {imported.Track}: {FormatLap(imported.BestLapMilliseconds)} nel miglior giro.", imported.BestLapMilliseconds > 0 ? 2 : -2);
        StoryCastService.Remember(career, StoryCastService.Manager, $"Ha ricevuto il dossier del test a {imported.Track}: ora deve trasformare i dati in un'opportunità.", imported.BestLapMilliseconds > 0 ? 1 : -1);
        var headline = imported.BestLapMilliseconds > 0 ? $"Test a {imported.Track}: {career.Driver} segna {FormatLap(imported.BestLapMilliseconds)}." : $"Test a {imported.Track}: sessione reale archiviata senza miglior giro disponibile.";
        career.Headline = headline; career.News.Add(headline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "TRACK_TEST", Headline = headline, Track = imported.Track, Importance = 45, PhotoPath = photoPath, PhotoView = string.IsNullOrWhiteSpace(photoPath) ? "" : PhotoSource.View(photoPath) }); SaveCareer(); if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") RefreshUi();
        // Il primo giro cronometrato della vita si racconta una volta sola.
        RaccontaMomento(MomentoDiCarriera.PrimoTest, CareerFirsts.Test);
        if (career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase))
        {
            // Nella valutazione il servizio racconta il verdetto, non due volte
            // la stessa prova: dati del test, giudizio e prossimo appuntamento
            // restano nello stesso articolo di valutazione.
            EvaluateRookieTest(imported);
        }
        else
        {
            // Fuori dalla valutazione la prova va chiusa qui.
            //
            // Il verdetto rookie chiudeva l'appuntamento e ne programmava il
            // seguito; sotto contratto quel percorso non passa, e nessuno
            // toglieva la prova dall'agenda: la stessa restava prima in
            // elenco e veniva rifatta all'infinito — nel banco ventitré volte
            // prima che la carriera si spegnesse.
            var provaSvolta = CareerScheduler.NextPlanned(career.Schedule ?? []);
            if (provaSvolta != null && provaSvolta.IsTest)
            {
                CareerScheduler.Close(career.Schedule ?? [], provaSvolta.Id);
                SaveCareer();
            }
            OpenCareerArticle(career.Events.LastOrDefault(x => x.Type == "TRACK_TEST"));
        }
    }
    /// <summary>
    /// Ricalcola il tempo-obiettivo della valutazione dai contenuti realmente
    /// installati. Prima il valore restava la costante 125000 per qualsiasi
    /// combinazione di pista e vettura.
    /// </summary>
    /// <summary>
    /// Ricalcola la soglia della valutazione per una combinazione precisa.
    ///
    /// Il circuito va indicato quando non è quello del prossimo appuntamento —
    /// per esempio subito dopo una prova, che va giudicata sulla pista dove si
    /// è svolta e non su quella dove si andrà la volta successiva. Era il
    /// difetto per cui il tempo segnato su un kartodromo veniva confrontato con
    /// la soglia di un altro tracciato.
    /// </summary>
    private RookieTarget RefreshEvaluationTarget(string? trackIdRichiesto = null, string? carIdRichiesto = null)
    {
        var trackId = string.IsNullOrWhiteSpace(trackIdRichiesto) ? NextTrackId() : trackIdRichiesto;
        var carId = string.IsNullOrWhiteSpace(carIdRichiesto) ? career.Car : carIdRichiesto;
        var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(trackId, StringComparison.OrdinalIgnoreCase));
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(carId, StringComparison.OrdinalIgnoreCase));
        // Un tempo già girato su questa esatta combinazione batte la stima.
        var measured = career.TestHistory
            .Where(x => x.BestLapMilliseconds > 0
                && RaceImportIdentity.TracksMatch(trackId, x.Track)
                && x.Car.Equals(carId, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.BestLapMilliseconds)
            .DefaultIfEmpty(0)
            .Min();
        var target = RookieTargetEngine.For(track?.LengthMeters ?? 0, car?.Category ?? "", car?.PowerHp ?? 0, car?.MassKg ?? 0, measured, track?.Category ?? "permanent");
        career.EvaluationTargetMilliseconds = target.TargetMilliseconds;
        career.EvaluationTargetBasis = target.Basis;
        career.EvaluationTargetTrack = trackId;
        return target;
    }

    /// <summary>
    /// Chiude l'appuntamento appena svolto e programma il successivo in base
    /// all'esito reale. È il punto in cui l'agenda diventa causale: non esiste un
    /// calendario deciso in anticipo, esiste la conseguenza di quello che è
    /// accaduto in pista.
    /// </summary>
    private void AdvanceScheduleAfterEvaluation(RookieVerdict verdict, string track)
    {
        career.Schedule ??= new List<ScheduledEvent>();
        // La valutazione serve a guadagnarsi un sedile: con un sedile in mano
        // non c'e' piu' niente da valutare. Prima si continuavano a programmare
        // prove rookie anche sotto contratto, e finivano in agenda PRIMA dei
        // round del campionato: il prossimo appuntamento restava per sempre una
        // prova e le gare vere non arrivavano mai.
        // Ha davvero un sedile?
        //
        // Prima qui c'era "career.IsClientDriver", che pero' e' true dalla
        // creazione della carriera: la guardia scattava sempre e la valutazione
        // non produceva mai un appuntamento nuovo — tutta la carriera restava
        // ferma al primo giorno.
        //
        // Un sedile esiste quando c'e' un contratto attivo, oppure quando una
        // squadra ha preso il pilota come cliente. Senza squadra, "cliente" non
        // vuol dire niente.
        var haUnSedile = career.ContractActive
            || (career.IsClientDriver && !string.IsNullOrWhiteSpace(career.Team)
                && !career.Team.StartsWith("Senza contratto", StringComparison.OrdinalIgnoreCase));
        if (haUnSedile) return;
        var current = CareerScheduler.NextPlanned(career.Schedule);
        if (current != null) CareerScheduler.Close(career.Schedule, current.Id);
        var lastDate = current?.Date ?? career.StoryDate;

        // Il tempo passa anche con le prove.
        //
        // Una prova si svolge nel giorno in cui era fissata: se il diario resta
        // fermo, tre prove, un verdetto e un invito risultano accaduti tutti
        // nello stesso giorno. E' il difetto che si vedeva come "succede tutto
        // il 12 marzo": le gare avanzavano la data, le prove no, e la
        // valutazione e' fatta solo di prove.
        if (lastDate > career.StoryDate) career.StoryDate = lastDate;
        var next = CareerScheduler.AfterEvaluation(verdict, career.EvaluationAttempts, contentIndex.Tracks, lastDate, career.Season, career.Cash, career.Races);
        if (next == null) return;
        if (!CareerScheduler.Append(career.Schedule, next))
        {
            // Un appuntamento rifiutato lasciava l'agenda vuota senza che
            // nessuno lo sapesse: e' esattamente il modo in cui la carriera si
            // fermava in silenzio.
            CareerLog.Info("agenda", $"appuntamento «{next.Id}» rifiutato: identificativo gia presente. L'agenda resta com'era.");
            return;
        }
        var headline = $"Prossimo appuntamento: {CareerScheduler.Describe(next)}. {next.GeneratedBy}.";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = next.Date, Type = "SCHEDULE_UPDATED",
            Headline = headline, Track = string.IsNullOrWhiteSpace(track) ? next.TrackId : track, Importance = 48
        });
        // Il diario si porta al giorno dell'appuntamento appena fissato: e' li
        // che il pilota sta guardando, ed e' il modo in cui la carriera respira
        // fra un impegno e l'altro invece di restare in un giorno unico.
        if (next.Date > career.StoryDate)
        {
            var primaDellaPausa = career.StoryDate;
            career.StoryDate = next.Date;
            RecoverForElapsedDays(primaDellaPausa);
        }
        CareerLog.Info("agenda", $"programmato {next.Kind} dopo la prova {career.EvaluationAttempts} · diario al {career.StoryDate:dd/MM/yyyy}");
    }

    private void EvaluateRookieTest(ImportedRaceResult imported)
    {
        var cashBefore = career.Cash;
        var fitnessBefore = career.Fitness;
        var trustBefore = career.TeamRelation;
        var reputationBefore = career.Reputation;
        career.EvaluationAttempts++;
        if (imported.BestLapMilliseconds > 0 && (career.EvaluationBestLapMilliseconds == 0 || imported.BestLapMilliseconds < career.EvaluationBestLapMilliseconds)) career.EvaluationBestLapMilliseconds = imported.BestLapMilliseconds;
        // Si giudica il giro di questa prova, su questa pista.
        //
        // Prima si usava il miglior tempo di sempre della carriera: un giro
        // segnato su un kartodromo corto restava il metro di paragone anche
        // quando la prova successiva si correva su un tracciato completamente
        // diverso, e la valutazione diceva «superata» senza che il pilota
        // avesse fatto niente in quella giornata.
        var best = imported.BestLapMilliseconds > 0 ? imported.BestLapMilliseconds : career.EvaluationBestLapMilliseconds;
        // L'obiettivo appartiene alla combinazione che si è appena corsa: se la
        // prova si è svolta altrove va ricalcolato per quella pista, non per la
        // prossima in agenda.
        if (!RaceImportIdentity.TracksMatch(career.EvaluationTargetTrack, imported.Track))
            RefreshEvaluationTarget(imported.Track, imported.Car);
        var target = career.EvaluationTargetMilliseconds <= 0 ? RookieTargetEngine.FallbackTargetMilliseconds : career.EvaluationTargetMilliseconds;
        var verdict = RookieTargetEngine.Evaluate(best, target);
        var consequence = ConsequenceEngine.ApplyTestResult(career, verdict.Passed, verdict.Close, best <= 0);
        career.RookieEvaluationScore = verdict.Score;
        if (verdict.Passed)
        {
            career.RookieEvaluationStatus = "Superata — offerte in arrivo";
            // Il punteggio interno resta al motore (RookieEvaluationScore),
            // ma non compare più nel titolo: qui contano le conseguenze che
            // il giocatore vede altrove nel portale — forma, popolarità, cassa.
            career.Headline = $"Rookie evaluation superata a {imported.Track}: {career.Driver} firma {FormatLap(best)}, {FormatLap(target - best)} sotto il riferimento di {FormatLap(target)}. Forma {career.Fitness}/100, livello influencer {career.ReputationProfile?.PublicPopularity ?? 0}/100, cassa € {career.Cash:N0}: il paddock apre il mercato.";
            career.News.Add(career.Headline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "EVALUATION_PASSED", Headline = career.Headline, Track = imported.Track, Importance = 95 });
            AdvanceScheduleAfterEvaluation(verdict, imported.Track);
            SaveCareer(); if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") RefreshUi();
            // La proposta non forza più una firma in una finestra modale: resta
            // nell'area Mercato, dove il pilota può confrontarla, rinviarla o
            // scegliere di continuare con sponsor e attività.
            //
            // Solo se il pilota e libero: sotto contratto una prova non fa
            // arrivare tre sedili nuovi, e prima invece succedeva a ogni test.
            if (!career.ContractActive) career.Offers = BuildOffers();
            if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1")
            {
                ShowChapterOneOutcome(ChapterOneBeat.Offer);
                OpenCareerArticle(career.Events.LastOrDefault(x => x.Type == "EVALUATION_PASSED"));
            }
            EnsurePaddockRoster(); SaveCareer(); if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") RefreshUi();
        }
        else
        {
            career.RookieEvaluationStatus = verdict.Close ? "In corso — vicino al riferimento" : "In corso — serve un altro test";
            // Come sopra: il numero della valutazione resta al motore, non al
            // titolo. Il lettore vede le conseguenze concrete, non un voto.
            var standing = $"Forma {career.Fitness}/100, livello influencer {career.ReputationProfile?.PublicPopularity ?? 0}/100, cassa € {career.Cash:N0}";
            career.Headline = verdict.Close
                ? $"Rookie evaluation a {imported.Track}: {career.Driver} chiude in {FormatLap(best)}, a {FormatLap(best - target)} dal riferimento di {FormatLap(target)}. {standing}: il team concede un test di conferma."
                : $"Rookie evaluation a {imported.Track}: {career.Driver} registra {(best > 0 ? FormatLap(best) : "nessun giro valido")} contro l'obiettivo {FormatLap(target)}. {standing}: il paddock chiede una prova di recupero.";
            career.News.Add(career.Headline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "EVALUATION_REVIEW", Headline = career.Headline, Track = imported.Track, Importance = 55 });
            AdvanceScheduleAfterEvaluation(verdict, imported.Track);
            SaveCareer(); if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1") RefreshUi();
            var next = CareerScheduler.NextPlanned(career.Schedule);
            if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1")
            {
                ShowChapterOneOutcome(next?.Kind == ScheduledEventKind.Invitation
                    ? ChapterOneBeat.Invitation
                    : verdict.Close ? ChapterOneBeat.ConfirmationTest : ChapterOneBeat.Retry);
                OpenCareerArticle(career.Events.LastOrDefault(x => x.Type == "EVALUATION_REVIEW"));
            }
        }
        var archived = career.TestHistory.LastOrDefault();
        if (archived != null)
        {
            archived.TargetLapMilliseconds = target;
            archived.EvaluationScore = verdict.Score;
            archived.CashDelta = career.Cash - cashBefore;
            archived.CashAfter = career.Cash;
            archived.FitnessDelta = career.Fitness - fitnessBefore;
            archived.FitnessAfter = career.Fitness;
            archived.TrustDelta = career.TeamRelation - trustBefore;
            archived.TrustAfter = career.TeamRelation;
            archived.ReputationDelta = career.Reputation - reputationBefore;
            archived.Outcome = verdict.Passed ? "SUPERATO · offerte in arrivo" : verdict.Close ? "VICINO · test di conferma" : "DA RIPETERE · test di recupero";
            archived.ConsequenceSummary = consequence.Explain();
        }
        SaveCareer();
    }

    private void ShowChapterOneOutcome(ChapterOneBeat beat)
    {
        if (!Visible || IsDisposed) return;
        // Il risultato non passa più da una pagina tecnica intermedia: dati e
        // archivio vivono nel portale, mentre il dopo-gara apre subito la scena
        // anime con i personaggi che li interpretano.
        ChapterOneDialog.ShowOutcomeAnime(career, beat, this);
    }

    /// <summary>
    /// Ogni snodo reale della carriera apre il suo servizio nel portale: il
    /// diario resta archivio permanente, ma il giocatore viene accompagnato
    /// nell'ordine in cui accadono i fatti.
    /// </summary>
    /// <summary>
    /// Annuncia un articolo nuovo. Non apre niente: mette un avviso in cima
    /// alla schermata, e il browser si apre solo se il giocatore lo clicca.
    ///
    /// Prima da qui partiva direttamente una scheda del browser, per ogni
    /// evento della carriera. Dopo qualche prova e qualche gara il browser era
    /// pieno di schede che nessuno aveva chiesto.
    /// </summary>
    private void OpenCareerArticle(CareerEventRecord? story)
    {
        if (story == null || IsDisposed || CareerMessages.Unattended) return;
        BeginInvoke(new Action(() =>
        {
            if (IsDisposed) return;
            // L'articolo viene messo in attesa: lo mostra la colonna «OGGI» al
            // prossimo ridisegno, e sparisce da solo a quello dopo.
            pendingArticleTitle = string.IsNullOrWhiteSpace(story.Headline) ? "Nuovo articolo dal paddock" : story.Headline;
            pendingArticleRedraws = 0;
            pendingArticleOpen = () =>
            {
                if (!IsDisposed) NarrationService.OpenBrowserPortal(CareerArticleBuilder.Build(career, story, rounds));
            };
            RefreshCareerHome();
        }));
    }
    private void EvaluateContractObjective(ImportedRaceResult imported)
    {
        if (career.ContractObjectiveStatus == "Raggiunto") return;
        var objective = career.ContractObjective.ToLowerInvariant();
        var met = objective.Contains("podio") && !imported.Dnf && imported.Position <= 3;
        met |= objective.Contains("stagione") && career.Round + 1 >= rounds.Count;
        if (objective.Contains("compagno"))
        {
            var teammate = imported.Classification.FirstOrDefault(x => AiDriverIdentity.NamesEqual(x.Name, career.Teammate));
            met |= teammate != null && !imported.Dnf && imported.Position < teammate.Position;
        }
        if (!met) return;
        career.ContractObjectiveStatus = "Raggiunto";
        var headline = $"Obiettivo contrattuale raggiunto: {career.ContractObjective}.";
        career.News.Add(headline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CONTRACT_OBJECTIVE", Headline = headline, Track = imported.Track, Importance = 85 });
    }
    private int SponsorRequiredResults() => SponsorRules.RequiredResults(rounds.Count);

    /// <summary>
    /// Regolamento punti del campionato in corso, dedotto dalla categoria della
    /// vettura realmente installata. Player e piloti AI devono usare lo stesso
    /// regolamento, altrimenti la classifica non è confrontabile.
    /// </summary>
    private ChampionshipRules ChampionshipRulesForCareer()
    {
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
        return ChampionshipRegulations.ForCategory(car?.Category ?? "", career.Tier);
    }
    private void RecordInternal(int position, string track, string car, int startingPosition, int qualificationPosition, string sessionName, int laps, int bestLapMilliseconds, int gapMilliseconds, int pitStops, double penaltySeconds, double damage, int teammatePosition, string teammateName, bool dnf, string photoPath, string resultFile, IReadOnlyList<ImportedDriverResult> classification)
    {
        // Si riparte da zero a ogni gara: la bandiera dice se e' QUESTA gara ad
        // aver chiuso la stagione, non se ne e' mai stata chiusa una.
        stagioneAppenaChiusa = false;
        // La data narrativa del referto è il giorno del round: senza questo la
        // cronaca datava la gara al giorno in cui il diario si trovava per caso.
        var sessionPlanForRound = CurrentSessionPlan("race");
        var raceDate = NarrativeCalendar.RoundDate(career.Round, SeasonAnchor(), Math.Max(1, rounds.Count));
        career.StoryDate = raceDate;
        var r = rounds[career.Round];
        var fieldSize = Math.Max(1, classification.Count);
        var rules = ChampionshipRulesForCareer();
        var fastestLapDriver = ChampionshipRegulations.FastestLapDriver(classification);
        var playerHasFastestLap = !string.IsNullOrWhiteSpace(fastestLapDriver) && AiDriverIdentity.NamesEqual(fastestLapDriver, career.Driver);
        var scoringPosition = dnf ? 0 : position;
        var pts = ChampionshipRegulations.TotalPoints(rules, scoringPosition, qualificationPosition, playerHasFastestLap, dnf);
        var racesBefore = career.Races; var winsBefore = career.Wins; var podiumsBefore = career.Podiums;
        var cashBefore = career.Cash;
        var fitnessBefore = career.Fitness;
        var trustBefore = career.TeamRelation;
        var sponsorQualifiedForBonus = SponsorRules.Qualifies(position, dnf, career.SponsorTarget);
        var economy = EconomyEngine.ForRace(new RaceEconomyInput
        {
            Tier = career.Tier, LadderStep = CareerLadder.Current(career, contentIndex.Cars).Step, EmployedDriver = career.ContractActive && !career.IsClientDriver, Position = scoringPosition, FieldSize = fieldSize, Dnf = dnf, Damage = damage,
            ContractSalary = career.ContractSalary, RoundsInSeason = Math.Max(1, rounds.Count),
            SponsorBonus = career.SponsorBonus, SponsorQualified = sponsorQualifiedForBonus,
            Endurance = sessionPlanForRound?.Endurance ?? false
        });
        var prize = economy.Prize; var salary = economy.Salary;
        var reputationChange = ReputationEngine.Evaluate(new ReputationInput
        {
            Position = scoringPosition, FieldSize = fieldSize, Dnf = dnf, QualifyingPosition = qualificationPosition,
            TeammatePosition = teammatePosition, CurrentReputation = career.Reputation, Tier = career.Tier,
            ExpectedPosition = ReputationEngine.ExpectedPosition(career.SeatPrestige, fieldSize)
        });
        career.Points += pts;
        career.PrizeMoney += prize; career.SalaryPaid += salary;
        // Premio, stipendio e bonus entrano prima del regolamento costi; questo
        // rende esplicito il bilancio del weekend e impedisce saldi impossibili.
        career.Cash += economy.Prize + economy.Salary;
        career.Reputation = reputationChange.Reputation;
        career.Races++;
        if (!dnf && position == 1) career.Wins++;
        if (!dnf && position <= 3) career.Podiums++;
        if (teammatePosition > 0 && !dnf)
        {
            if (position < teammatePosition) career.TeammateRacesWon++; else if (position > teammatePosition) career.TeammateRacesLost++;
        }
        else if (dnf && teammatePosition > 0) career.TeammateRacesLost++;
        if (playerHasFastestLap && rules.FastestLapPoint && !dnf)
        {
            var fastestHeadline = $"Giro veloce a {r.GrandPrix} per {career.Driver}: {FormatLap(bestLapMilliseconds)} dal referto Assetto Corsa.";
            career.News.Add(fastestHeadline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "FASTEST_LAP", Headline = fastestHeadline, Track = track, Importance = 48 });
        }
        if (reputationChange.Delta != 0)
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "REPUTATION_CHANGE", Headline = $"Reputazione {reputationChange.Delta:+#;-#;0} → {reputationChange.Reputation}/100 · {reputationChange.Explain()}", Track = track, Importance = 25 });
        if (career.Races == 1)
        {
            var evaluation = RookieEvaluationEngine.Evaluate(position, dnf, classification.Count, qualificationPosition, teammatePosition, laps);
            career.RookieEvaluationScore = evaluation.Score;
            career.RookieEvaluationStatus = evaluation.Status;
            var rookieHeadline = $"Rookie evaluation di {career.Driver}: {career.RookieEvaluationStatus}, calcolata dal debutto reale. Forma {career.Fitness}/100, livello influencer {career.ReputationProfile?.PublicPopularity ?? 0}/100, cassa € {career.Cash:N0}.";
            career.News.Add(rookieHeadline); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "ROOKIE_EVALUATION", Headline = rookieHeadline, Track = track, Importance = 80 });
        }
        if (sponsorQualifiedForBonus) career.SponsorQualifyingResults++;
        var sponsorAward = economy.SponsorAward;
        if (career.SponsorObjectiveStatus == "In corso" && career.SponsorQualifyingResults >= SponsorRequiredResults())
        {
            career.SponsorObjectiveStatus = "Raggiunto";
            var sponsorHeadline = $"Obiettivo sponsor raggiunto: {career.Driver} ha centrato {career.SponsorQualifyingResults} risultati Top {career.SponsorTarget} per {career.Sponsor}.";
            career.News.Add(sponsorHeadline);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SPONSOR_OBJECTIVE", Headline = sponsorHeadline, Track = track, Importance = 75 });
        }
        career.Cash += sponsorAward; career.SponsorMoney += sponsorAward;
        var paid = SettleRaceCosts(economy.LogisticsCost, economy.DamageCost, $"Gara a {r.GrandPrix}");
        var outcome = position == 99 || dnf ? "Ritiro" : $"P{position}";
        var paidCosts = paid.LogisticsPaid + paid.DamagePaid;
        var costSummary = economy.Costs > 0 ? $", costi pagati € -{paidCosts:N0}" + (paid.Unpaid > 0 ? $" · non sostenibili € {paid.Unpaid:N0}" : "") : "";
        career.Results.Add($"{r.GrandPrix}: {outcome} (+{pts} pt, premio € {prize:N0}, stipendio € {salary:N0}{(sponsorAward > 0 ? $", sponsor +€ {sponsorAward:N0}" : "")}{costSummary})");
        career.Headline = position == 99 ? $"Ritiro a {r.GrandPrix}: il box analizza i dati e {career.Sponsor} chiede una risposta." : $"A {r.GrandPrix}, {career.Driver} chiude {outcome}: il paddock parla di una prestazione che cambia il fine settimana.";
        career.News ??= new List<string>();
        career.News.Add(position == 1 ? $"{career.Driver} vince a {r.GrandPrix}: {career.Sponsor} versa il bonus di risultato." : position <= 3 ? $"Primo podio della stagione per {career.Driver} a {r.GrandPrix}." : position == 99 ? $"Weekend da dimenticare a {r.GrandPrix}: la squadra dovrà reagire." : $"{career.Driver} chiude {outcome} a {r.GrandPrix}, rispettando l'obiettivo di {career.Sponsor}.");
        career.RaceHistory ??= new List<RaceHistoryEntry>();
        var sessionPlan = sessionPlanForRound;
        career.RaceHistory.Add(new RaceHistoryEntry { SourceKind = pendingSourceKind, FormatLabel = sessionPlan?.FormatLabel ?? "", WeatherId = sessionPlan?.WeatherId ?? "", WeatherLabel = sessionPlan?.WeatherLabel ?? "", TemperatureC = sessionPlan?.TemperatureC ?? 0, TimeOfDaySeconds = sessionPlan?.TimeOfDaySeconds ?? 0, PlannedLaps = sessionPlan?.RaceLaps ?? 0, AiLevel = sessionPlan?.AiLevel ?? 0, Season = career.Season, Round = career.Round + 1, DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Track = track, Car = car, Position = position, TeammatePosition = teammatePosition, TeammateName = teammateName, StartingPosition = startingPosition, QualificationPosition = qualificationPosition, SessionName = sessionName, Laps = laps, BestLapMilliseconds = bestLapMilliseconds, GapMilliseconds = gapMilliseconds, PitStops = pitStops, PenaltySeconds = penaltySeconds, Damage = damage, Dnf = dnf, Points = pts, Prize = prize, SponsorBonus = sponsorAward, CashDelta = career.Cash - cashBefore, CashAfter = career.Cash, FitnessDelta = career.Fitness - fitnessBefore, FitnessAfter = career.Fitness, TrustDelta = career.TeamRelation - trustBefore, TrustAfter = career.TeamRelation, LogisticsPaid = paid.LogisticsPaid, DamagePaid = paid.DamagePaid, UnpaidCosts = paid.Unpaid, PhotoPath = photoPath, ResultFile = resultFile, ResultSha256 = HashFile(resultFile), ImportedUtc = DateTime.UtcNow, Classification = classification.Select(x => new RaceParticipantSnapshot { Name = x.Name, Car = x.Car, Position = x.Position, IsPlayer = x.IsPlayer }).ToList() });
        StoryCastService.Remember(career, StoryCastService.Mechanic, $"Ha smontato i dati di {track}: {outcome}, {pts} punti e miglior giro {FormatLap(bestLapMilliseconds)}.", position <= 3 ? 4 : dnf ? -4 : 1);
        StoryCastService.Remember(career, StoryCastService.Manager, $"Ha aggiornato il mercato dopo {track}: il risultato {outcome} vale {pts} punti e € {prize:N0}.", position <= 3 ? 5 : dnf ? -5 : 1);
        StoryCastService.Remember(career, StoryCastService.Rival, $"Ha visto {career.Driver} chiudere {outcome} a {track}: il duello resta aperto.", position <= 3 ? 3 : 0);
        career.Events ??= new List<CareerEventRecord>();
        var raceType = position == 1 ? (winsBefore == 0 ? "FIRST_VICTORY" : "VICTORY") : position <= 3 ? (podiumsBefore == 0 ? "FIRST_PODIUM" : "PODIUM") : dnf ? "RETIREMENT" : "RACE_FINISHED";
        var eventHeadline = raceType switch
        {
            "FIRST_VICTORY" => $"Prima vittoria di {career.Driver}: successo reale a {r.GrandPrix}.",
            "FIRST_PODIUM" => $"Primo podio di {career.Driver}: il risultato reale a {r.GrandPrix} entra nella storia.",
            "MILESTONE_RACE" => career.Headline,
            _ => career.Headline
        };
        if (raceType is "FIRST_VICTORY" or "FIRST_PODIUM") { career.Headline = eventHeadline; career.News.Add(eventHeadline); }
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = raceType, Headline = eventHeadline, Track = track, Importance = position == 1 ? 100 : position <= 3 ? 70 : dnf ? 55 : 20, PhotoPath = photoPath, PhotoView = string.IsNullOrWhiteSpace(photoPath) ? "" : PhotoSource.View(photoPath) });
        if (career.Races == 50 || career.Races == 100)
        {
            var milestone = $"Traguardo carriera: {career.Driver} raggiunge la gara {career.Races} a {track}.";
            career.News.Add(milestone);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "MILESTONE_RACE", Headline = milestone, Track = track, Importance = 92, PhotoPath = photoPath, PhotoView = string.IsNullOrWhiteSpace(photoPath) ? "" : PhotoSource.View(photoPath) });
        }
        // Il recap mensile confronta il mese di questa gara con quello della gara
        // precedente: prima dipendeva da un avanzamento fisso di sette giorni che
        // non corrispondeva alla spaziatura reale del calendario.
        var previousRaceDate = career.RaceHistory.Count > 1 ? career.RaceHistory[^2].StoryDate : default;
        // L'appuntamento appena corso va chiuso qualunque nome abbia.
        //
        // Prima si cercava solo "round-sNN-NN": le gare di un sedile cliente si
        // chiamano "cal-client-race-..." e restavano in programma per sempre,
        // mentre Round avanzava oltre la lista e il pulsante della gara
        // smetteva di rispondere.
        var disputato = CareerScheduler.NextPlanned(career.Schedule ?? []);
        if (disputato != null && disputato.Kind == ScheduledEventKind.ChampionshipRound)
            CareerScheduler.Close(career.Schedule ?? [], disputato.Id);
        else
            CareerScheduler.Close(career.Schedule ?? [], $"round-s{career.Season:00}-{career.Round + 1:00}");
        career.Round++;
        // La lista dei round va riletta dall'agenda: chiudendo l'appuntamento
        // e cambiata, e i conti di fine stagione si fanno su questa.
        rounds = ChampionshipRoundsView();
        // Una gara e una scena nuova: la musica non resta quella di prima.
        SoundtrackService.MarkNewScene();
        // Quello che il weekend lascia al pilota, comunque sia andato.
        GrowFromRacing(!dnf, position, fieldSize);
        var primaDelWeekend = career.StoryDate;
        career.StoryDate = NarrativeCalendar.DayAfterRound(career.Round - 1, SeasonAnchor());
        // Le settimane fra un weekend e l altro fanno recuperare il pilota.
        RecoverForElapsedDays(primaDelWeekend);
        // Nuova finestra fra i weekend: l'agenda del pilota si ricarica.
        career.DaysUntilNextRound = OffTrackActivities.DaysBetweenRounds();
        // La stanchezza del weekend non si azzera da sola: va gestita con l'agenda.
        career.Fatigue = Math.Clamp(career.Fatigue + (sessionPlanForRound?.Endurance == true ? 14 : 8), 0, OffTrackActivities.MaxFatigue);
        if (previousRaceDate != default && (previousRaceDate.Year != raceDate.Year || previousRaceDate.Month != raceDate.Month))
        {
            var monthName = raceDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
            var recap = $"Riepilogo di {monthName}: {career.Driver} ha disputato {career.Races} gare, conquistato {career.Points} punti, con budget € {career.Cash:N0} e reputazione {career.Reputation}/100.";
            career.News.Add(recap); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "MONTHLY_RECAP", Headline = recap, Track = "Diario della carriera", Importance = 45 });
        }
        // Il campionato finisce quando finiscono i suoi round. Un pilota
        // cliente non ha un campionato: compra le gare una alla volta, e la
        // sua lista si svuota a ogni weekend — cosi la stagione risultava
        // conclusa gia dalla prima gara, con premio finale e pagella, e non
        // poteva piu esserlo dopo.
        // Ci sono round di campionato in questa stagione, e sono tutti corsi?
        //
        // Prima si confrontava un contatore con la lunghezza di una lista, ma
        // la lista viene ricostruita a ogni sedile e il contatore azzerato: due
        // grandezze che non parlavano piu della stessa cosa. E l'esclusione dei
        // piloti cliente guardava IsClientDriver, che resta true anche dopo un
        // contratto vero — cosi la stagione non finiva mai per nessuno, e senza
        // stagioni concluse non esistono titoli ne mondiali.
        // La stagione si chiude da sé quando i suoi round sono finiti.
        ChiudiStagioneSeCompleta();
        // Conseguenze del risultato: reputazione su tutte le dimensioni, promesse
        // condizionali verificate, opportunita scadute, stato economico.
        var consequences = ConsequenceEngine.ApplyRaceResult(career, career.RaceHistory[^1], CarCompetitiveness(car));
        // Come per gli inviti: i fatti che rendono una gara una storia. Qui in
        // più si sa se il campionato era ancora in gioco, e un ritiro in quel
        // momento pesa molto di più.
        ApplyRaceHighlights(career.RaceHistory[^1], decisivaPerIlTitolo: TitoloAncoraInGioco());
        foreach (var honoured in consequences.Honoured)
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "PROMISE_HONOURED",
                Headline = $"{honoured.PromisedBy} mantiene la promessa: {honoured.RewardDescription}.",
                Track = track, Importance = 78
            });
        foreach (var failed in consequences.Failed)
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "PROMISE_FAILED",
                Headline = $"Scaduta la promessa di {failed.PromisedBy}: {failed.ConditionText} non raggiunto.",
                Track = track, Importance = 64
            });
        foreach (var consequence in consequences.Applied) career.News.Add(consequence.Text);
        if (consequences.EnteredFinancialTrouble)
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "FINANCIAL_TROUBLE",
                Headline = $"Patrimonio sotto la soglia di sopravvivenza: € {career.Cash:N0}. La carriera va ricostruita dal basso.",
                Track = "Bilancio", Importance = 80
            });
        UpdateMarketAfterRace();
        UpdateStoryArcs();
        UpdateAiCalibration();
        // Chi e' salito a stagione in corso e' in prova: qui si vede se la
        // squadra che si e' esposta ha avuto ragione.
        ValutaLaProvaDopoIlSalto();
        // E qui l'allenatore spiega quello che sta per diventare possibile:
        // un'occasione annunciata e' una cosa che ci si e' guadagnati, una
        // occasione muta sembra piovuta dal cielo.
        BriefingDopoLaGara();
        // Dopo una gara ci deve essere la prossima.
        //
        // Era il difetto piu' segnalato dal collaudo: quarantanove volte su
        // sessantuno gare l'agenda restava vuota, e l'unico modo di andare
        // avanti era comprare un weekend fuori campionato. Da li' venivano
        // trentacinque sostituzioni e dieci gare open su sessantuno gare
        // totali, la stagione che non si chiudeva mai e i round veri spinti
        // sempre piu' in la'. Se il campionato non ha piu' appuntamenti in
        // programma, se ne pubblica uno: e' il campionato l'ossatura della
        // carriera, non le occasioni che capitano.
        if (!(career.Schedule ?? []).Any(x => x.Kind == ScheduledEventKind.ChampionshipRound && x.IsPlanned)
            && !career.CareerPhase.Equals("Evaluation", StringComparison.OrdinalIgnoreCase))
            GenerateSeasonSchedule("La stagione continua: il campionato ha bisogno del prossimo round");
        // Nuove proposte dallo stato aggiornato: e il punto in cui il risultato
        // appena ottenuto cambia cio che il paddock offre.
        RefreshOpportunities();
        SaveCareer(); RefreshUi();

        // Le voci del box arrivano anche dopo una gara.
        //
        // Prove e inviti avevano la loro scena commentata; le gare di
        // campionato no. Si correva, il risultato entrava nello storico, e
        // nessuno intorno al pilota diceva niente — proprio nel momento in cui
        // ci sarebbe piu da dire.
        // Le battute erano quelle del capitolo sul TEST: dopo ogni gara di
        // campionato il box commentava cronometro, riferimento e scarto di una
        // prova che poteva essere di mesi prima. Sembrava una scena che
        // ricompariva a caso, ed era proprio così. Ora si parla della gara
        // appena corsa, con i dati di quella gara.
        var ultimaGara = career.RaceHistory.LastOrDefault();
        // Se la stagione si e' appena chiusa, il racconto lo ha gia' fatto la
        // chiusura: qui si tace e si lascia solo l'articolo.
        if (ultimaGara != null && !stagioneAppenaChiusa && !CareerMessages.Unattended && Visible && !IsDisposed)
        {
            ChapterOneDialog.ShowRaceReactions(career, ultimaGara, this, contentIndex.Cars, contentIndex.Tracks, career.Schedule ?? []);
            // Come sopra: il portale torna al proprio tema quando la scena finisce.
            RefreshUi();
            // E se questa domenica e' stata una prima volta, la si racconta:
            // viene dopo le reazioni perche' e' il momento piu' grande dei due
            // e chiudere su quello lascia la sensazione giusta.
            ControllaPrimeVolte(ultimaGara.Dnf ? 0 : ultimaGara.Position, ultimaGara.Dnf);
        }

        // Il giornale racconta anche le gare, non solo le prove.
        //
        // Prove, inviti e contratti avevano il loro articolo; le gare di
        // campionato — il cuore della carriera — no: si correva e il giornale
        // taceva. L'evento piu importante fra quelli appena scritti diventa
        // l'articolo, e l'avviso in cima alla schermata lo annuncia.
        var raccontabile = career.Events
            .Where(x => x.Type is "MILESTONE_RACE" or "SEASON_AWARD" or "CONTRACT_OBJECTIVE"
                or "MONTHLY_RECAP" or "PROGRESSION_REVIEW")
            .LastOrDefault();
        OpenCareerArticle(raccontabile ?? career.Events.LastOrDefault());
    }

    /// <summary>
    /// Ricalcola il livello AI proposto per le sessioni successive usando solo i
    /// referti reali già archiviati. Non modifica punti, posizioni o storico: il
    /// risultato appena importato resta esattamente quello prodotto da Assetto Corsa.
    /// </summary>
    private void UpdateAiCalibration()
    {
        if (!career.AutoCalibrateAi) return;
        // Solo i referti veri di Assetto Corsa.
        //
        // La correzione esiste per riavvicinare il livello IA del preset a come
        // il giocatore va davvero in pista: sulle gare risolte dal programma
        // non ha niente da correggere, perche' la difficolta' e' gia' quella
        // del gradino. Applicandola anche li' diventava una seconda spinta
        // sopra la prima, e la carriera si spaccava in due: al banco tre piloti
        // vincevano cento gare su centoquaranta e due non ne vincevano mai una.
        var recent = career.RaceHistory
            .Where(x => x.Classification.Count > 1)
            .Where(x => x.SourceKind != ResultProvenance.Simulated
                        && x.SourceKind != ResultProvenance.Withdrawal)
            .Select(x => (Position: x.Position, FieldSize: x.Classification.Count, Dnf: x.Dnf));
        var before = career.AiCalibrationOffset;
        career.AiCalibrationOffset = AiCalibration.NextOffset(before, recent);
        if (career.AiCalibrationOffset == before) return;
        var headline = $"Calibrazione difficoltà: il livello AI proposto passa da {before:+#;-#;0} a {career.AiCalibrationOffset:+#;-#;0}. {AiCalibration.Describe(career.AiCalibrationOffset)}";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "AI_CALIBRATION", Headline = headline, Track = "Box tecnico", Importance = 30 });
    }

    private void UpdateMarketAfterRace()
    {
        if (career.Races == 0 || career.Races % 2 != 0 || contentIndex.Cars.Count == 0) return;
        // L'offerta di mercato non può scavalcare la scala di carriera.
        //
        // Qui si sceglieva fra TUTTE le vetture installate, ordinandole solo
        // per reputazione: dopo due gare di kart poteva arrivare l'offerta per
        // una monoposto di vertice, e la carriera saltava cinque gradini in un
        // colpo restando poi ferma lì per quattro anni. Un'offerta può portare
        // al massimo un gradino sopra quello attuale.
        // Un gradino sopra al massimo, e mai uno sotto: un'offerta non deve
        // poter riportare indietro chi è già salito.
        var gradinoAttuale = CareerLadder.Current(career, contentIndex.Cars).Step;
        // Anche il mercato spontaneo resta sulla strada scelta al bivio.
        //
        // Era la terza porta da cui rientrava la stessa incoerenza: il pilota
        // sceglieva le monoposto, e dopo due gare arrivava l'interesse di una
        // squadra per un trofeo turismo — la scelta cadeva sulla vettura con
        // il punteggio di riconoscimento più vicino alla reputazione, senza
        // guardare la disciplina. Firmata quella, la carriera cambiava
        // mestiere e la scelta del bivio non aveva significato.
        var candidate = CareerLadder.OnPath(contentIndex.Cars.Where(ContentCategoryRules.IsRaceable), career.ChosenPath)
            .Where(x =>
            {
                var passo = CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step;
                if (!CareerLadder.WithinReach(passo, gradinoAttuale, contentIndex.Cars)) return false;
                // Anche il mercato rispetta la gavetta: un'offerta per la
                // categoria superiore arriva dopo un numero di gare fatte su
                // quella attuale, non dopo la prima buona domenica.
                return passo <= gradinoAttuale
                       || RacesOnCurrentStep() >= OpportunityGenerator.RacesBeforeStepUp(gradinoAttuale);
            })
            .OrderBy(x => Math.Abs((x.Confidence >= 70 ? 1 : 2) * 20 + career.Reputation - 35))
            .FirstOrDefault(x => !x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
        if (candidate == null || career.Offers.Any(x => x.Car.Equals(candidate.Id, StringComparison.OrdinalIgnoreCase))) return;
        // Il seguito e i rapporti costruiti fuori dalla pista contano nel mercato:
        // è l'unico canale attraverso cui l'agenda del pilota incide sulla carriera.
        var marketability = Math.Clamp(career.Fanbase / 4 + career.SponsorRelation / 8, 0, 25);
        var prestige = Math.Clamp(career.Reputation + 10 + marketability / 2, 25, 95);
        var team = prestige >= 65 ? "Rinkai Racing Team" : "Kaido Motorsport";
        var offer = new TeamOffer { Team = team, Car = candidate.Id, Category = candidate.Category, Sponsor = prestige >= 65 ? "Rinkai Energy" : "Kanda Tools", Teammate = prestige >= 65 ? "Mei Kanzaki" : "Sota Fujimoto", Salary = 25000 + career.Reputation * 900 + marketability * 1200, Years = 1, Prestige = prestige, Objective = career.Podiums > 0 ? "Conquista un podio" : "Batti il compagno", Origin = "Scuderia fittizia generata dal manager da contenuti installati", Livery = candidate.Skins.FirstOrDefault() ?? "" };
        career.Offers.Add(offer);
        var headline = $"{team} osserva {career.Driver}: arriva un interesse concreto per la categoria {candidate.Category} dopo {career.Races} gare reali.";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "MARKET_INTEREST", Headline = headline, Track = "Mercato piloti", Importance = 65 });
    }

    private void UpdateStoryArcs()
    {
        career.StoryArcs ??= new List<StoryArcRecord>();
        void Upsert(string id, string title, string type, int importance, string summary, string status = "In corso")
        {
            var arc = career.StoryArcs.FirstOrDefault(x => x.Id == id);
            if (arc == null)
            {
                arc = new StoryArcRecord { Id = id, Title = title, Type = type, StartedStoryDate = career.StoryDate, Importance = importance };
                career.StoryArcs.Add(arc);
            }
            arc.UpdatedStoryDate = career.StoryDate; arc.Status = status; arc.Summary = summary;
            arc.EventTypes = career.Events.TakeLast(12).Select(x => x.Type).Distinct().ToList();
        }
        if (career.Races > 0)
            Upsert("rookie-season", "La prima stagione", "CAREER_BEGINNING", 70, $"{career.Driver} ha già affrontato {career.Races} weekend reali nel suo primo campionato.", career.Round >= rounds.Count ? "Concluso" : "In corso");
        if (career.Wins == 0 && career.Races >= 3)
            Upsert("chasing-first-win", "La ricerca della prima vittoria", "FIRST_WIN_CHASE", 75, $"Dopo {career.Races} gare, il primo successo di {career.Driver} resta l'obiettivo del paddock.");
        if (career.Wins > 0)
            Upsert("breakthrough", "Il salto di qualità", "BREAKTHROUGH", 90, $"Le vittorie reali di {career.Driver} hanno cambiato le aspettative del team.", career.Round >= rounds.Count ? "In pausa" : "In corso");
        foreach (var rivalry in career.Rivalries.Where(x => x.Level >= 30))
            Upsert($"rivalry-{rivalry.Rival}", $"La rivalità con {rivalry.Rival}", "RIVALRY", 80, rivalry.Story);
        if (career.SeasonArchive.Count > 0)
            Upsert("season-legacy", "Costruire una carriera", "LEGACY", 85, $"La carriera conserva {career.SeasonArchive.Count} stagione/i archiviate e una memoria consultabile.");
    }
    private void LaunchWeekend()
    {
        if (awaitingResult) { ReopenPendingWeekend(); return; }
        if (NextScheduled()?.Kind == ScheduledEventKind.Invitation) { LaunchInvitation(); return; }
        // Il pilota cliente non ha un contratto perche' non deve averlo: paga
        // la quota gara per gara. Prima finiva anche lui in questo ramo e si
        // sentiva dire che il contratto era scaduto, con l'invito a firmare
        // qualcosa che aveva gia. Nel kart significava non correre mai.
        if (!career.ContractActive && !career.IsClientDriver)
        {
            CareerMessages.Show(null, "Il contratto è scaduto. Scegli una nuova offerta dal Mercato e scouting prima di avviare il weekend.", "CorsaCareer - contratto scaduto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        // Il calendario e il contatore dei round devono parlare la stessa lingua.
        //
        // Qui si usciva in silenzio: il portale mostrava «Round 1 di
        // campionato» in agenda, il pulsante rosso non faceva niente e non
        // veniva detto perche'. Succede quando la vista dei round non e'
        // allineata all'agenda — per esempio dopo una firma che ha
        // ricostruito il calendario. Prima di arrendersi si riallinea; se
        // dopo il riallineamento il round non c'e' davvero, lo si dice.
        if (career.Round >= rounds.Count) rounds = ChampionshipRoundsView();
        if (career.Round >= rounds.Count)
        {
            var inAgenda = CareerScheduler.ChampionshipRounds(career.Schedule ?? [], career.Season).Count;
            CareerLog.Warn("weekend", $"nessun round da avviare: contatore {career.Round}, calendario {rounds.Count} round, agenda {inAgenda} round per la stagione {career.Season}.");
            CareerMessages.Show(null,
                $"Il calendario di {career.Championship} non ha un round da avviare.\n\n" +
                $"Round corsi: {career.Round} · round in calendario: {rounds.Count}.\n\n" +
                "Se la stagione e' finita, chiudila dalla Home per aprire quella nuova.",
                "CorsaCareer - nessun round da avviare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var r = rounds[career.Round]; Directory.CreateDirectory(saveDir);
        // Quick Drive preset: il formato viene scritto direttamente nella cartella letta da CM.
        // Il contenuto storico viene usato solo se realmente installato; altrimenti si usa un fallback verificato.
        var installedCars = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).Select(x => x.Id).ToArray();
        var car = installedCars.Contains(career.Car, StringComparer.OrdinalIgnoreCase) ? career.Car : installedCars.FirstOrDefault(x => x == "ks_ferrari_f2004") ?? installedCars.FirstOrDefault() ?? "";
        var selectedCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(car, StringComparison.OrdinalIgnoreCase));
        var grid = RaceGridSelector.Select(car, selectedCar?.Category ?? "", contentIndex.Cars);
        var candidates = grid.Candidates;
        var mixedGridFallback = grid.UsedMixedFallback;
        if (candidates.Length == 0) candidates = installedCars;
        var opponentCount = Math.Max(0, Math.Min(7, candidates.Length - 1));
        if (candidates.Length < 2)
        {
            CareerMessages.Show(null, $"Il weekend {r.GrandPrix} non può essere preparato come gara reale: è installata una sola auto da competizione compatibile ({car}) e non esiste alcun avversario da inserire in griglia.\n\nInstalla almeno una seconda auto da gara compatibile, poi aggiorna i contenuti. Nessun risultato verrà simulato.", "CorsaCareer - griglia insufficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var installedTracks = contentIndex.Tracks.Select(x => x.Id).ToArray();
        var requestedTrackFound = installedTracks.Contains(r.Track, StringComparer.OrdinalIgnoreCase);
        var track = requestedTrackFound ? r.Track : (installedTracks.Contains("monza", StringComparer.OrdinalIgnoreCase) ? "monza" : installedTracks.FirstOrDefault() ?? "");
        if (installedCars.Length == 0 || candidates.Length == 0 || track.Length == 0)
        {
            CareerMessages.Show(null, $"Non ci sono contenuti sufficienti per avviare il weekend {r.GrandPrix}.\n\nAuto trovate: {installedCars.Length}\nCircuiti trovati: {installedTracks.Length}\n\nInstalla almeno un'auto e un circuito compatibili, poi riprova.", "CorsaCareer - contenuti mancanti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!requestedTrackFound || mixedGridFallback)
        {
            var missing = new List<string>();
            if (!requestedTrackFound) missing.Add($"circuito {r.GrandPrix}");
            var gridNote = mixedGridFallback ? "La categoria scelta non ha abbastanza auto: verrà usata una griglia mista con altre auto da competizione installate." : "";
            var missingNote = missing.Count == 0 ? "" : $"Contenuti storici non trovati: {string.Join(", ", missing)}.\n\n";
            var answer = CareerMessages.Ask(null, $"{missingNote}{gridNote}\nPosso fare il possibile con ciò che è installato:\nAuto: {car}\nCircuito: {track}\nAvversari: {opponentCount}\n\nVuoi continuare con questo fallback?", "CorsaCareer - fallback contenuti", MessageBoxButtons.YesNo, DialogResult.Yes);
            if (answer != DialogResult.Yes) return;
        }
        // Il formato del weekend viene calcolato dai contenuti reali: distanza dalla
        // lunghezza del circuito, meteo fra quelli installati, orario e AI dal livello
        // e dai referti precedenti. Non esiste più una gara fissa da cinque giri.
        var plan = BuildSessionPlan(track, car, mixedGridFallback, testSession: false);
        var presetJson = ContentManagerPresetBuilder.Build(car, track, plan, candidates, opponentCount);
        var presetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Presets", "Quick Drive");
        Directory.CreateDirectory(presetDir);
        var presetPath = Path.Combine(presetDir, $"CorsaCareer - S{career.Season:00} GP {career.Round + 1:00} {r.GrandPrix}.cmpreset");
        File.WriteAllText(presetPath, presetJson);
        if (!ContentManagerPresetValidator.TryValidate(presetPath, out var presetError))
        {
            CareerMessages.Show(null, $"Il preset di Content Manager non ha superato il controllo: {presetError}.\n\nNessun weekend è stato registrato.", "CorsaCareer - preset non valido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (Environment.GetEnvironmentVariable("CORSACAREER_UI_AUTOMATION") != "1" && !ConfirmSessionPlan(plan, $"{r.GrandPrix} · round {career.Round + 1}/{rounds.Count}", car, track, opponentCount))
        {
            // Nessun weekend registrato: il preset rifiutato non deve restare
            // nella cartella di Content Manager come sessione avviabile.
            try { File.Delete(presetPath); } catch { }
            return;
        }
        // La modalita va dichiarata qui, non ereditata.
        //
        // Era l'unico punto che armava una sessione senza dirne il tipo: il
        // weekend si prendeva il pendingMode rimasto dal briefing precedente —
        // "test" — e la gara finiva archiviata fra le prove, senza posizione,
        // senza punti e senza premio.
        pendingMode = "race"; awaitingResult = true; launchTimeUtc = DateTime.UtcNow;
        var resultFile = AssettoCorsaResultLocator.FindLatestExisting();
        var resultHashBeforeLaunch = HashFile(resultFile); pendingResultHash = resultHashBeforeLaunch;
        ArchiveSessionPlan(plan, "race", track, car);
        var pending = new { mode = "dynamic", round = career.Round + 1, grandPrix = r.GrandPrix, country = r.Country, date = r.Date, car, track, candidates, opponents = opponentCount, preset = presetPath, launchUtc = launchTimeUtc, resultHashBeforeLaunch, driver = career.Driver, team = career.Team, format = plan.FormatLabel, laps = plan.RaceLaps, weather = plan.WeatherId, temperature = plan.TemperatureC, timeOfDay = plan.TimeOfDaySeconds, aiLevel = plan.AiLevel };
        File.WriteAllText(Path.Combine(saveDir, "pending_weekend.json"), JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true }));
        var cmPath = LocateContentManager();
        if (!string.IsNullOrWhiteSpace(cmPath))
        {
            try
            {
                // La URI race/quick esegue direttamente il preset in CM senza
                // fermarsi sulla scelta "Apply preset" / "Just Go".
                OpenContentManagerPreset(cmPath, presetPath);
                RefreshUi();
                if (saveStatus != null) saveStatus.Text = $"Weekend {r.GrandPrix} aperto in Content Manager · in attesa del referto Assetto Corsa.";
            }
            catch (Exception error)
            {
                CareerMessages.Show(null, $"Il preset è stato creato, ma Content Manager non si è aperto.\n\n{error.Message}\n\nPuoi aprire manualmente:\n{presetPath}", "CorsaCareer — avvio CM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            // Assetto Corsa non c e su questo computer: la sessione viene
            // risolta dal programma e la carriera prosegue. Prima ci si
            // fermava qui con un avviso, e serviva un secondo pulsante per
            // sbloccare la situazione.
            CareerLog.Info("sessione", "Content Manager non installato: la sessione viene risolta dal programma.");
            ResolvePendingSessionWithoutAssettoCorsa();
        }
    }

    /// <summary>Una gara su invito è una gara reale isolata: griglia, qualifica e risultato, ma senza punti campionato.</summary>
    private void LaunchInvitation()
    {
        if (awaitingResult) return;
        var invitation = NextScheduled();
        if (invitation?.Kind != ScheduledEventKind.Invitation) return;
        var entryFee = InvitationEntryFee(invitation);

        // Il giorno della gara e' una decisione, non un automatismo: avere i
        // soldi non rende ovvia la scelta, e non averli non deve nascondere le
        // alternative. La schermata dice cosa resta in cassa dopo, quali
        // impegni gia fissati diventerebbero insostenibili, e cosa costa
        // saltare.
        if (!invitation.EntryFeePaid)
        {
            // Senza nessuno davanti la scelta non puo' essere posta: si corre,
            // che e' l'unico esito che permette di osservare cosa succede dopo.
            var decision = RaceDecision.Race;
            if (!CareerMessages.Unattended)
            {
                using var choice = new RaceChoiceDialog(career, invitation, entryFee);
                choice.ShowDialog(this);
                decision = choice.Decision;
            }
            switch (decision)
            {
                case RaceDecision.Skip:
                    RaceChoice.ApplySkip(career, invitation);
                    career.Headline = $"{career.Driver} rinuncia a {invitation.TrackName}.";
                    career.News.Add(career.Headline);
                    career.Events.Add(new CareerEventRecord
                    {
                        DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "RACE_SKIPPED",
                        Headline = career.Headline, Track = invitation.TrackName, Importance = 45
                    });
                    CareerLog.Info("gara", $"gara saltata: {invitation.TrackName}");
                    SaveCareer(); RefreshUi();
                    return;
                case RaceDecision.Race:
                    break;
                default:
                    return;
            }
        }
        var installedCars = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).Select(x => x.Id).ToArray();
        var car = installedCars.Contains(career.Car, StringComparer.OrdinalIgnoreCase) ? career.Car : installedCars.FirstOrDefault() ?? "";
        var selectedCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(car, StringComparison.OrdinalIgnoreCase));
        // Rete di sicurezza: la schermata non offre "corri" senza i soldi, ma
        // questo percorso puo essere raggiunto anche da altrove.
        if (!invitation.EntryFeePaid && entryFee > career.Cash)
        {
            CareerMessages.Show(null, 
                $"L'iscrizione costa € {entryFee:N0} e in cassa ci sono € {career.Cash:N0}.",
                "CorsaCareer — cassa insufficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var grid = RaceGridSelector.Select(car, selectedCar?.Category ?? "", contentIndex.Cars);
        if (grid.Candidates.Length < 2)
        {
            CareerMessages.Show(null, "La gara su invito richiede almeno un avversario reale. Installa o abilita una seconda auto da competizione compatibile, poi riprova.", "CorsaCareer — griglia insufficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var track = contentIndex.Tracks.Any(x => x.Id.Equals(invitation.TrackId, StringComparison.OrdinalIgnoreCase)) ? invitation.TrackId : contentIndex.Tracks.FirstOrDefault()?.Id ?? "";
        if (string.IsNullOrWhiteSpace(track)) { CareerMessages.Show(null, "Nessun circuito installato per la gara su invito.", "CorsaCareer — contenuti mancanti", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var plan = BuildSessionPlan(track, car, mixedGrid: grid.UsedMixedFallback, testSession: false);
        var presetJson = ContentManagerPresetBuilder.Build(car, track, plan, grid.Candidates, grid.OpponentCount);
        var presetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Presets", "Quick Drive");
        Directory.CreateDirectory(presetDir);
        var presetPath = Path.Combine(presetDir, $"CorsaCareer - Invito S{career.Season:00} {invitation.TrackName}.cmpreset");
        File.WriteAllText(presetPath, presetJson);
        if (!ContentManagerPresetValidator.TryValidate(presetPath, out var presetError, installedCars, contentIndex.Tracks.Select(x => x.Id)))
        {
            CareerMessages.Show(null, $"Il preset della gara su invito non è valido: {presetError}.", "CorsaCareer — preset non valido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        // La quota si paga qui: accettare l'invito e' la decisione che costa,
        // e deve costare anche se Content Manager non parte. Prima stava dentro
        // il blocco che apre CM, cioe' si correva gratis ogni volta che CM non
        // c'era.
        if (!invitation.EntryFeePaid)
        {
            var payment = CareerWallet.TryPay(career, entryFee, $"iscriverti a {invitation.TrackName}");
            if (!payment.Paid)
            {
                CareerMessages.Show(null,
                    payment.Refusal + "\n\nCerca uno sponsor o guadagna qualcosa con le attività fra i weekend.",
                    "CorsaCareer — cassa insufficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            invitation.EntryFeePaid = true;
            invitation.EntryFee = entryFee;
            career.LogisticsCosts += entryFee;
            career.Results.Add($"Iscrizione gara su invito {invitation.TrackName}: € -{entryFee:N0}");
            career.News.Add($"{career.Driver} accetta l'invito di {(string.IsNullOrWhiteSpace(invitation.ProposedBy) ? "un team ospitante" : invitation.ProposedBy)} a {invitation.TrackName}: iscrizione € {entryFee:N0}.");
        }
        pendingMode = "invitation"; awaitingResult = true; launchTimeUtc = DateTime.UtcNow;
        var resultFile = AssettoCorsaResultLocator.FindLatestExisting();
        var resultHashBeforeLaunch = HashFile(resultFile); pendingResultHash = resultHashBeforeLaunch;
        ArchiveSessionPlan(plan, "invitation", track, car);
        var pending = new { mode = "invitation", round = 0, grandPrix = invitation.TrackName, country = invitation.Country, date = NarrativeCalendar.Format(invitation.Date), car, track, candidates = grid.Candidates, opponents = grid.OpponentCount, preset = presetPath, launchUtc = launchTimeUtc, resultHashBeforeLaunch, driver = career.Driver, team = career.Team, format = plan.FormatLabel, laps = plan.RaceLaps, weather = plan.WeatherId, temperature = plan.TemperatureC, timeOfDay = plan.TimeOfDaySeconds, aiLevel = plan.AiLevel };
        File.WriteAllText(Path.Combine(saveDir, "pending_weekend.json"), JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true }));
        SaveCareer();
        // Come per la prova: il banco automatico verifica la preparazione, ma
        // non va a cercare Content Manager ne apre niente.
        if (CareerMessages.Unattended) { RefreshUi(); return; }
        var cmPath = LocateContentManager();
        if (string.IsNullOrWhiteSpace(cmPath))
        {
            // Assetto Corsa non c'è su questo computer: la sessione viene
            // risolta dal programma e la carriera prosegue. Prima ci si
            // fermava qui con un avviso, e serviva un secondo pulsante per
            // sbloccare la situazione.
            CareerLog.Info("sessione", "Content Manager non installato: la sessione viene risolta dal programma.");
            ResolvePendingSessionWithoutAssettoCorsa();
            return;
        }
        try
        {
            OpenContentManagerPreset(cmPath, presetPath);
            RefreshUi();
            if (saveStatus != null) saveStatus.Text = "Gara su invito aperta in Content Manager · in attesa del referto Assetto Corsa.";
        }
        catch (Exception error) { CareerMessages.Show(null, $"Impossibile aprire la gara su invito: {error.Message}", "CorsaCareer — avvio CM", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private SessionPlan BuildSessionPlan(string trackId, string carId, bool mixedGrid, bool testSession)
    {
        var track = contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(trackId, StringComparison.OrdinalIgnoreCase));
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(carId, StringComparison.OrdinalIgnoreCase));
        var calendarTracks = rounds
            .Select(round => contentIndex.Tracks.FirstOrDefault(x => x.Id.Equals(round.Track, StringComparison.OrdinalIgnoreCase)) ?? new ContentTrackRecord { Id = round.Track })
            .ToList();
        var enduranceIndex = testSession ? -1 : SessionPlanner.EnduranceRoundIndex(calendarTracks, car?.Category ?? "");
        return SessionPlanner.Plan(new SessionRequest
        {
            TrackId = trackId,
            TrackName = track?.Name ?? trackId,
            TrackLengthMeters = track?.LengthMeters ?? 0,
            TrackCategory = track?.Category ?? "permanent",
            CarCategory = car?.Category ?? "special",
            Tier = career.Tier,
            ChampionshipLevel = career.ChampionshipLevel,
            // Il gradino reale della vettura decide la forza del gruppo, e
            // finisce nel preset: vale quindi anche per la gara vera aperta in
            // Content Manager, non solo per la simulazione interna.
            LadderStep = car == null ? 0 : CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Step,
            Season = career.Season,
            RoundIndex = career.Round,
            RoundCount = Math.Max(1, rounds.Count),
            StoryDate = career.StoryDate,
            DifficultyProfile = career.DifficultyProfile,
            DistanceProfile = career.DistanceProfile,
            AiCalibrationOffset = career.AutoCalibrateAi ? career.AiCalibrationOffset : 0,
            InstalledWeatherIds = contentIndex.Weathers.Select(x => x.Id).ToList(),
            EnduranceRound = enduranceIndex >= 0 && enduranceIndex == career.Round,
            MixedGrid = mixedGrid,
            TestSession = testSession
        });
    }

    /// <summary>
    /// Il foglio del weekend, prima di scendere in pista.
    ///
    /// Era un MessageBox di sistema con dentro un blocco di testo e gli
    /// identificativi delle cartelle — «kart_60_mini», «kart_narashino»: una
    /// scatola di Windows in mezzo a una schermata di gioco, che comunicava
    /// tutto e non si leggeva. Adesso e' una scheda da paddock con i nomi veri.
    /// </summary>
    private bool ConfirmSessionPlan(SessionPlan plan, string header, string car, string track, int opponents)
    {
        if (CareerMessages.Unattended) return true;
        var vettura = NomeVettura(car);
        var circuito = NomeCircuito(track);
        var gara = !plan.FormatLabel.Contains("prova", StringComparison.OrdinalIgnoreCase)
                   && !plan.FormatLabel.Contains("test", StringComparison.OrdinalIgnoreCase)
                   && plan.RaceLaps > 0;
        using var scheda = new SessionBriefDialog(
            header, $"{career.Driver} · {career.Championship} · {career.Team}",
            plan, vettura, circuito, opponents,
            TavolaPerMomento(gara ? "gara" : "test"),
            gara ? "PARTECIPA ALLA GARA" : "SCENDI IN PISTA");
        return scheda.ShowDialog(this) == DialogResult.Yes;
    }

    private void ArchiveSessionPlan(SessionPlan plan, string mode, string track, string car)
    {
        career.SessionPlans ??= new List<SessionPlanRecord>();
        career.SessionPlans.RemoveAll(x => x.Season == career.Season && x.Round == career.Round + 1 && x.Mode.Equals(mode, StringComparison.OrdinalIgnoreCase));
        career.SessionPlans.Add(SessionPlanRecord.From(plan, career.Season, career.Round + 1, mode, track, car, career.StoryDate));
        // L'archivio serve al racconto: conserva solo le stagioni recenti.
        if (career.SessionPlans.Count > 200) career.SessionPlans.RemoveRange(0, career.SessionPlans.Count - 200);
        SaveCareer(createVersionedBackup: false);
    }

    /// <summary>
    /// Apre l'agenda fra i weekend. I giorni disponibili si azzerano a ogni round
    /// e vengono ricaricati quando il calendario avanza.
    /// </summary>
    /// <summary>
    /// La giornata di Haru Senda: le trattative, raccontate come scene.
    /// </summary>
    private void OpenSponsorDay()
    {
        if (BlockIfPending("le sponsorizzazioni")) return;
        career.Agent ??= new AgentSkills();
        using var dialog = new SponsorDayDialog(career, () => SaveCareer(createVersionedBackup: false), PlaySponsorScene);
        dialog.ShowDialog(this);
        SaveCareer(); RefreshUi();
    }

    /// <summary>
    /// La trattativa raccontata: Haru entra, parla, e si scopre com'e andata.
    /// La decisione e' gia stata presa prima — qui si vede il seguito.
    /// </summary>
    private bool PlaySponsorScene(SponsorVisit visit, SponsorReply reply)
    {
        // La trattativa e' gia' stata giocata: qui si vede solo come e' finita.
        // Ripetere il discorso d'apertura di Haru farebbe sembrare che la
        // conversazione appena avuta non sia mai avvenuta.
        var lines = new List<AnimeDialogueLine>
        {
            // L'interlocutore non ha un ritratto proprio: il fruttivendolo e
            // l'assicuratore non sono il meccanico. Si mostra il luogo della
            // trattativa, e se quella tavola non c'è ancora si ripiega
            // sull'esito — un accordo e un rifiuto si raccontano diversamente.
            new(visit.Target, reply.Line,
                SceneArtwork.ForSponsorVisit(visit.Trade, reply.Accepted),
                reply.Accepted ? "convinto" : "dispiaciuto"),
            new("Haru Senda",
                reply.Accepted
                    ? $"«Sono € {reply.Amount:N0}. Non cambiano la stagione, ma cambiano la prossima iscrizione.»"
                    : "«Niente. Ci riprovo, magari quando avremo qualcosa di piu da mostrare.»",
                "character-haru-senda.png",
                reply.Accepted ? "soddisfatto" : "paziente")
        };
        using var scene = new AnimeDialogueDialog($"CorsaCareer — {visit.Target}", lines);
        scene.ShowDialog(this);

        // Le scene scritte del denaro: il primo sponsor della carriera e il
        // primo no si raccontano una volta sola, perche' la prima volta che
        // qualcuno crede in te — o non ci crede — non e' come la decima.
        if (reply.Accepted) RaccontaMomento(MomentoDiCarriera.PrimoSponsor, CareerFirsts.Sponsor);
        else RaccontaMomento(MomentoDiCarriera.SponsorRifiutato, "primo-no");

        // E se dopo tutto questo la cassa non copre nemmeno un'iscrizione, la
        // cosa va detta da chi tiene i conti, non lasciata a un numero rosso.
        if (career.Cash < CareerFinances.SurvivalFloor)
            RaccontaMomento(MomentoDiCarriera.CassaVuota, $"cassa-s{career.Season:00}");

        return reply.Accepted;
    }

    /// <summary>
    /// La giornata del pilota. Prima fra un impegno e l'altro non c'era niente
    /// da fare, e per questo non si poteva nemmeno indicare un'alternativa
    /// quando i soldi mancavano.
    /// </summary>
    private void OpenDriverDay(DayFocus? focus = null)
    {
        if (BlockIfPending("le attività del pilota")) return;
        using var dialog = new DriverDayDialog(career, () => SaveCareer(createVersionedBackup: false), ShowDayScene, OpenSponsorDay, focus);
        dialog.ShowDialog(this);
        SaveCareer(); RefreshUi();
    }

    /// <summary>
    /// Mostra come scena quello che vale la pena vedere: un accordo, un
    /// rifiuto, un incontro. Non ogni ora di palestra.
    /// </summary>
    private void ShowDayScene(DayReport report)
    {
        var speaker = report.Activity.Actor == DayActor.Agent ? "Haru Senda" : career.Driver;

        // La tavola dell'attività, se esiste: una palestra e una trattativa non
        // sono la stessa scena. Finché il file non c'è si ripiega sul ritratto
        // di chi parla, e la scena funziona lo stesso.
        var artwork = SceneArtwork.ForActivity(report.Activity.Id);
        if (!SceneArtwork.Exists(artwork))
            artwork = report.Activity.Actor == DayActor.Agent
                ? "character-haru-senda.png"
                : RitrattoDelPilota();

        var lines = new List<AnimeDialogueLine>
        {
            new(speaker, report.Outcome.Line, artwork,
                report.Outcome.IsSetback ? "deluso" : "soddisfatto")
        };
        if (report.Applied.Count > 0)
            lines.Add(new(speaker, string.Join("  ·  ", report.Applied), artwork, "riepilogo"));

        using var scene = new AnimeDialogueDialog($"CorsaCareer — {report.Activity.Name}", lines);
        scene.ShowDialog(this);

        // Le scene scritte della scuola. La prima volta che ci si va dopo aver
        // corso, Sae e Tooru hanno qualcosa da dire; e quando il seguito
        // comincia a farsi sentire, la scuola se ne accorge prima del paddock.
        if (DayActivityCatalog.AScuola(report.Activity.Id))
        {
            var seguito = (career.ReputationProfile ?? new ReputationProfile()).PublicPopularity;
            if (seguito >= 30 && career.Races > 0)
                RaccontaMomento(MomentoDiCarriera.ScuolaSiParlaDiTe, "scuola-fama");
            else if (career.Races > 0)
                RaccontaMomento(MomentoDiCarriera.ScuolaDopoLaGara, "scuola-lunedi");
        }
    }

    /// <summary>
    /// La faccia del pilota quando e' lui a parlare.
    ///
    /// Finora non ne aveva una: quando il pilota diceva qualcosa in una scena
    /// della giornata, la finestra ripiegava sul ritratto di Genji — cioe' il
    /// protagonista parlava con la faccia del suo meccanico, e nessuno se ne
    /// era accorto perche' la scena funzionava lo stesso.
    ///
    /// Il ritratto importato dal giocatore (<c>AvatarPath</c>) esisteva gia' ed
    /// era completamente inutilizzato. Viene prima di tutto; poi un ritratto
    /// generico se c'e'; e solo alla fine il ripiego di prima.
    /// </summary>
    private string RitrattoDelPilota()
    {
        if (!string.IsNullOrWhiteSpace(career.AvatarPath) && File.Exists(career.AvatarPath))
            return career.AvatarPath;
        foreach (var candidato in new[] { "character-pilota.png", "character-pilota-casco.png" })
            if (AssetPaths.Exists(candidato)) return candidato;
        return "character-genji-arakawa.png";
    }

    private void OpenActivities()
    {
        if (BlockIfPending("l'agenda del pilota")) return;
        NarrateCardEssentials("activities|" + career.StoryDate.ToString("O"), $"Agenda e sponsor. Hai {career.DaysUntilNextRound} giorni prima del prossimo appuntamento e {career.Cash} euro disponibili. Le attività possono aumentare preparazione, reputazione o interesse degli sponsor.");
        if (career.DaysUntilNextRound < 0) career.DaysUntilNextRound = OffTrackActivities.DaysBetweenRounds();
        using var dialog = new ActivitiesDialog(career, PerformActivity, OpenActivityArticle, OpenSponsorSearch, OpenActivityAnimeScene);
        dialog.ShowDialog(this);
        SaveCareer(); RefreshUi();
    }

    /// <summary>
    /// Restituisce all'attività il suo momento narrativo: una sola tavola,
    /// il personaggio coerente con l'area e un balloon scritto progressivamente.
    /// Alla chiusura della scena ActivitiesDialog ripopola l'elenco.
    /// </summary>
    private void OpenActivityAnimeScene(ActivityRecord record)
    {
        var isHaru = record.ActivityId.StartsWith("haru-", StringComparison.OrdinalIgnoreCase);
        var artwork = SceneArtwork.ForActivity(record.ActivityId);
        if (!SceneArtwork.Exists(artwork))
            artwork = isHaru ? "character-haru-senda.png" : "character-genji-arakawa.png";

        var speaker = isHaru ? "Haru Senda" : string.IsNullOrWhiteSpace(career.Driver) ? "Il pilota" : career.Driver;
        var portrait = isHaru ? SceneArtwork.PortraitFor("Haru Senda") : SceneArtwork.PortraitFor(career.Driver);
        if (string.IsNullOrWhiteSpace(portrait) || !SceneArtwork.Exists(portrait))
            portrait = artwork;

        var effects = new List<string>();
        if (record.Reputation != 0) effects.Add($"reputazione {record.Reputation:+#;-#;0}");
        if (record.Fanbase != 0) effects.Add($"seguito {record.Fanbase:+#;-#;0}");
        if (record.TeamRelation != 0) effects.Add($"fiducia team {record.TeamRelation:+#;-#;0}");
        if (record.SponsorRelation != 0) effects.Add($"appeal sponsor {record.SponsorRelation:+#;-#;0}");
        if (record.Fatigue != 0) effects.Add($"stanchezza {record.Fatigue:+#;-#;0}");
        if (record.Money != 0) effects.Add($"budget {record.Money:+€ #,##0;-€ #,##0;€ 0}");
        var summary = effects.Count == 0 ? "Nessuna variazione numerica." : "Conseguenze: " + string.Join(" · ", effects) + ".";
        if (!string.IsNullOrWhiteSpace(record.ChoiceLabel))
            summary = $"Scelta: {record.ChoiceLabel}. " + summary;

        var lines = new List<AnimeDialogueLine>
        {
            new(speaker, record.Story, portrait, record.Success ? "soddisfatto" : "deluso"),
            new("RIEPILOGO", summary, artwork, "riepilogo")
        };
        using var scene = new AnimeDialogueDialog($"CorsaCareer — {record.Name}", lines);
        scene.ShowDialog(this);
    }

    /// <summary>
    /// Apre il servizio dell'attività nel portale browser: una voce dell'agenda
    /// produce una pagina come qualunque altro fatto della carriera.
    /// </summary>
    private void OpenActivityArticle(ActivityRecord record)
    {
        var story = career.Events
            .Where(x => x.Type is "ACTIVITY_DONE" or "ACTIVITY_SETBACK")
            .OrderBy(x => Math.Abs((x.StoryDate - record.StoryDate).Ticks))
            .FirstOrDefault()
            ?? new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = record.StoryDate,
                Type = record.Success ? "ACTIVITY_DONE" : "ACTIVITY_SETBACK",
                Headline = record.Story, Track = record.Name, Importance = 40
            };
        NarrationService.OpenBrowserPortal(CareerArticleBuilder.Build(career, story, rounds));
    }

    private ActivityOutcome PerformActivity(ActivityDefinition definition, ActivityOption? choice)
    {
        var context = new ActivityContext
        {
            Tier = career.Tier, Season = career.Season, Round = career.Round,
            Fatigue = career.Fatigue, Reputation = career.Reputation, Cash = career.Cash,
            DaysAvailable = career.DaysUntilNextRound,
            TimesDoneThisSeason = career.ActivityHistory.Count(x => x.Season == career.Season && x.ActivityId == definition.Id)
        };
        var outcome = OffTrackActivities.Resolve(definition, context, choice?.Id);
        if (!outcome.Performed) return outcome;

        career.DaysUntilNextRound = Math.Max(0, career.DaysUntilNextRound - definition.DurationDays);
        career.Reputation = Math.Clamp(career.Reputation + outcome.Effect.Reputation, 0, 100);
        career.Fanbase = Math.Max(0, career.Fanbase + outcome.Effect.Fanbase);
        career.TeamRelation = Math.Clamp(career.TeamRelation + outcome.Effect.TeamRelation, 0, 100);
        career.SponsorRelation = Math.Clamp(career.SponsorRelation + outcome.Effect.SponsorRelation, 0, 100);
        career.Fatigue = Math.Clamp(career.Fatigue + outcome.Effect.Fatigue, 0, OffTrackActivities.MaxFatigue);
        // Un'attività può avere una penalità economica (esito negativo), ma il
        // budget è un vincolo duro: anche in questo caso non può mai scendere
        // sotto zero. Le spese dichiarate sono già incluse nell'effetto
        // restituito da OffTrackActivities.Resolve.
        career.Cash = Math.Max(0, career.Cash + outcome.Effect.Money);
        // Il pannello storico usa gli effetti legacy, ma il resto del portale
        // usa ReputationProfile: applica gli stessi delta anche lì e riallinea
        // i campi aggregati, evitando due carriere diverse a seconda della
        // schermata aperta.
        var profile = career.ReputationProfile ??= new ReputationProfile();
        profile.SportingPrestige = Math.Clamp(profile.SportingPrestige + outcome.Effect.Reputation, 0, 100);
        profile.PublicPopularity = Math.Clamp(profile.PublicPopularity + outcome.Effect.Fanbase, 0, 100);
        profile.TeamTrust = Math.Clamp(profile.TeamTrust + outcome.Effect.TeamRelation, 0, 100);
        profile.SponsorAppeal = Math.Clamp(profile.SponsorAppeal + outcome.Effect.SponsorRelation, 0, 100);
        profile.SyncLegacyFields(career);
        career.StoryDate = career.StoryDate.AddDays(definition.DurationDays);
        // Le attività fuori pista possono far avanzare più giorni: il piano
        // precedente non deve sopravvivere alla nuova data.
        DriverDay.EnsureToday(career);

        career.ActivityHistory.Add(new ActivityRecord
        {
            Season = career.Season, Round = career.Round + 1, StoryDate = career.StoryDate,
            ActivityId = definition.Id, Name = definition.Name, Success = outcome.Success, Story = outcome.Story,
            ChoiceId = choice?.Id ?? "", ChoiceLabel = choice?.Label ?? "", ChoiceAngle = choice?.Angle ?? "",
            EffectiveRisk = outcome.EffectiveRisk,
            Days = definition.DurationDays, Reputation = outcome.Effect.Reputation, Fanbase = outcome.Effect.Fanbase,
            TeamRelation = outcome.Effect.TeamRelation, SponsorRelation = outcome.Effect.SponsorRelation,
            Fatigue = outcome.Effect.Fatigue, Money = outcome.Effect.Money
        });
        var headline = choice == null
            ? $"{definition.Name}: {outcome.Story}"
            : $"{definition.Name} · {choice.Label}: {outcome.Story}";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate,
            Type = outcome.Success ? "ACTIVITY_DONE" : "ACTIVITY_SETBACK",
            Headline = headline, Track = definition.Category,
            Importance = outcome.Success ? 40 : 52
        });
        if (career.SponsorRelation <= 15)
        {
            var warning = $"{career.Sponsor} mette il contratto in revisione: il rapporto è scivolato a {career.SponsorRelation}/100.";
            career.News.Add(warning);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SPONSOR_WARNING", Headline = warning, Track = "Sponsor", Importance = 72 });
        }
        if (career.TeamRelation <= 15)
        {
            var warning = $"Il rapporto con {career.Team} è critico ({career.TeamRelation}/100): il rinnovo è a rischio.";
            career.News.Add(warning);
            career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "TEAM_WARNING", Headline = warning, Track = "Paddock", Importance = 72 });
        }
        SaveCareer(createVersionedBackup: false);
        return outcome;
    }

    // ------------------------------------------------------------ opportunita

    /// <summary>
    /// Competitivita della vettura fra 0 e 100, dal rapporto peso/potenza
    /// confrontato con le altre auto installate della stessa categoria. Serve a
    /// far capire alla stampa quanto vale un risultato: lo stesso piazzamento con
    /// una vettura debole e una notizia diversa.
    /// </summary>
    private int CarCompetitiveness(string carId)
    {
        var car = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(carId, StringComparison.OrdinalIgnoreCase));
        if (car == null || car.PowerHp <= 0 || car.MassKg <= 0) return 50;
        var peers = contentIndex.Cars
            .Where(x => x.Category.Equals(car.Category, StringComparison.OrdinalIgnoreCase) && x.PowerHp > 0 && x.MassKg > 0)
            .Select(x => x.PowerHp / (x.MassKg / 1000.0))
            .OrderBy(x => x).ToList();
        if (peers.Count < 2) return 50;
        var own = car.PowerHp / (car.MassKg / 1000.0);
        var below = peers.Count(x => x < own);
        return Math.Clamp((int)Math.Round(below / (double)(peers.Count - 1) * 100), 0, 100);
    }

    /// <summary>
    /// Quante gare il pilota ha corso sul gradino dove si trova adesso.
    ///
    /// Conta il gradino e non la singola vettura: passare dalla Formula Vee
    /// alla Tatuus FA01 non azzera la gavetta, sono la stessa categoria.
    /// </summary>
    /// <summary>
    /// L'età del pilota oggi, nel tempo della storia.
    ///
    /// Le carriere salvate prima che l'età esistesse non hanno un anno di
    /// nascita: glielo si assegna una volta sola, all'indietro, in modo che un
    /// pilota che ha già corso cinque stagioni non risulti sedicenne.
    /// </summary>
    private int EtaPilota()
    {
        if (career.CareerStart == default)
            career.CareerStart = career.RaceHistory?.FirstOrDefault()?.StoryDate is { } prima && prima != default
                ? prima
                : career.StoryDate;
        if (career.BirthYear <= 0)
            career.BirthYear = career.CareerStart.Year - DriverAge.EtaIniziale;
        var eta = career.StoryDate.Year - career.BirthYear;
        return Math.Clamp(eta, 10, 80);
    }

    private int RacesOnCurrentStep()
    {
        var passo = CareerLadder.Current(career, contentIndex.Cars).Step;
        var conto = 0;
        foreach (var gara in career.RaceHistory ?? [])
        {
            var auto = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(gara.Car, StringComparison.OrdinalIgnoreCase));
            if (auto == null) continue;
            if (CareerLadder.ForCar(auto.Category, auto.PowerHp, auto.MassKg).Step == passo) conto++;
        }
        return conto;
    }

    private OpportunityContext BuildOpportunityContext()
    {
        var seasonRaces = career.RaceHistory.Where(x => x.Season == career.Season).ToList();
        var last = career.RaceHistory.LastOrDefault();
        return new OpportunityContext
        {
            Driver = career.Driver,
            Cash = career.Cash,
            Reputation = career.ReputationProfile ?? new ReputationProfile(),
            Tier = career.Tier,
            Championship = career.Championship,
            HasActiveContract = career.ContractActive,
            CurrentTeam = career.Team,
            Season = career.Season,
            Today = career.StoryDate,
            Races = career.Races,
            Wins = career.Wins,
            Podiums = career.Podiums,
            RecentPositions = seasonRaces.TakeLast(5).Reverse().Select(x => x.Dnf ? 0 : x.Position).ToList(),
            RecentFieldSize = last?.Classification.Count ?? 12,
            LastTrackName = last == null ? "" : NarrativeEngine.Capitalize(last.Track),
            LastWeather = last?.WeatherLabel ?? "",
            LastPosition = last == null || last.Dnf ? 0 : last.Position,
            // AsEnumerable è necessario: List<T>.Reverse() inverte in posto e
            // restituisce void, quindi ordinerebbe davvero lo storico gare.
            PointlessStreak = career.RaceHistory.AsEnumerable().Reverse().TakeWhile(x => x.Dnf || x.Points == 0).Count(),
            TeamRelation = career.TeamRelation,
            Fitness = career.Fitness,
            ChampionshipLevel = career.ChampionshipLevel,
            LadderStep = CareerLadder.Current(career, contentIndex.Cars).Step,
            RacesAtStep = RacesOnCurrentStep(),
            CurrentCarId = career.Car ?? "",
            VittorieDiFila = career.RaceHistory.AsEnumerable().Reverse()
                .TakeWhile(x => !x.Dnf && x.Position == 1).Count(),
            PodiDiFila = career.RaceHistory.AsEnumerable().Reverse()
                .TakeWhile(x => !x.Dnf && x.Position is > 0 and <= 3).Count(),
            MigliorRimontaRecente = career.RaceHistory
                .TakeLast(6)
                .Where(x => !x.Dnf && x.Position > 0 && x.StartingPosition > 0)
                .Select(x => x.StartingPosition - x.Position)
                .DefaultIfEmpty(0)
                .Max(),
            RacesSinceLevelUp = career.RacesAtLastLevelUp <= 0
                ? 999
                : Math.Max(0, career.Races - career.RacesAtLastLevelUp),
            SponsorRelation = career.SponsorRelation,
            CurrentSponsor = career.Sponsor ?? "",
            CurrentSponsorAppeal = career.SponsorSignedAppeal,
            ChosenPath = career.ChosenPath ?? "",
            TestsWithCurrentCar = (career.TestHistory ?? [])
                .Count(x => !string.IsNullOrWhiteSpace(career.Car)
                            && x.Car.Equals(career.Car, StringComparison.OrdinalIgnoreCase)),
            AvailableTiers = contentIndex.Cars.Where(ContentCategoryRules.IsRaceable)
                .Select(x => TierForCategory(x.Category)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            Tracks = contentIndex.Tracks,
            Cars = contentIndex.Cars,
            OpenOpportunities = (career.Opportunities ?? []).Count(x => x.IsOpen),
            // Se una gara da correre c'e gia, non serve proporne un'altra.
            HasOpenRace = (career.Opportunities ?? []).Any(x => x.IsOpen && x.IsRace),
            HasPlannedRound = (career.Schedule ?? [])
                .Any(x => x.Kind == ScheduledEventKind.ChampionshipRound && x.IsPlanned)
        };
    }

    /// <summary>
    /// Rigenera le opportunita aperte dallo stato reale. Le proposte non arrivano
    /// a caso: ogni voce dichiara il motivo per cui esiste.
    /// </summary>
    private void RefreshOpportunities(bool announce = true)
    {
        // Chi si e' ritirato non corre piu': niente calendari, niente offerte,
        // niente stagioni nuove. Senza questo la carriera ripartiva da sola il
        // giorno dopo il ritiro.
        if (career.Retired) return;
        career.Opportunities ??= new List<Opportunity>();
        if (contentIndex.Tracks.Count == 0) return;
        var generated = OpportunityGenerator.Generate(BuildOpportunityContext());
        var added = 0;
        foreach (var opportunity in generated)
        {
            // Solo fra le occasioni ancora aperte: una proposta scaduta ha
            // esaurito il suo compito, e continuare a confrontarsi con essa
            // impediva che ne nascesse un'altra dello stesso tipo. Un pilota
            // si ritrovava con i soldi in tasca e niente da comprare.
            if (career.Opportunities.Any(x => x.IsOpen && x.Id.Equals(opportunity.Id, StringComparison.OrdinalIgnoreCase))) continue;
            // Una proposta per tipo alla volta.
            //
            // L'identificativo generato contiene la data, quindi ogni mattina
            // era diverso e il controllo qui sopra non fermava niente: il
            // portale si riempiva di copie della stessa offerta, una al
            // giorno, e bastava accettarle tutte. Con due sedili aperti nello
            // stesso pomeriggio si potevano firmare due stagioni diverse.
            if (career.Opportunities.Any(x => x.IsOpen && x.Kind == opportunity.Kind)) continue;
            if (opportunity.IsSeat && career.Opportunities.Any(x => x.IsOpen && x.IsSeat)) continue;
            // E il paddock non ritorna il giorno dopo con la stessa cosa che si
            // è appena accettata, rifiutata o lasciata scadere.
            var attesa = Opportunity.CooldownDays(opportunity.Kind);
            if (career.Opportunities.Any(x => !x.IsOpen && x.Kind == opportunity.Kind
                    && x.ClosedStoryDate != default
                    && (career.StoryDate - x.ClosedStoryDate).TotalDays < attesa))
                continue;
            // L'identificativo pero' deve restare unico: se ne esiste gia uno
            // uguale ma chiuso, quello nuovo prende un suffisso.
            if (career.Opportunities.Any(x => x.Id.Equals(opportunity.Id, StringComparison.OrdinalIgnoreCase)))
                opportunity.Id = $"{opportunity.Id}-r{career.Opportunities.Count}";
            career.Opportunities.Add(opportunity);
            added++;
            if (!announce) continue;
            var headline = $"{opportunity.Title}: {opportunity.Justification}";
            career.News.Add(headline);
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "OPPORTUNITY_OFFERED",
                Headline = headline, Track = string.IsNullOrWhiteSpace(opportunity.TrackName) ? opportunity.ProposedBy : opportunity.TrackName,
                Importance = opportunity.IsSeat ? 74 : 52
            });
        }
        // L'archivio non deve crescere senza fine: le proposte chiuse restano
        // solo finche servono al racconto.
        if (career.Opportunities.Count > 40)
            career.Opportunities.RemoveRange(0, career.Opportunities.Count - 40);
        if (added > 0) CareerLog.Info("opportunita", $"{added} proposte generate dallo stato della carriera");
    }

    /// <summary>
    /// La ricerca di sponsor. Prende il posto della schermata «Opportunita», che
    /// elencava insieme gare, test e proposte commerciali: le gare esistevano
    /// cosi in due posti — qui e nel calendario — e il giocatore ne vedeva uno
    /// solo. Le gare vivono nel calendario; qui restano soltanto i soldi.
    /// </summary>
    private void OpenSponsorSearch()
    {
        if (BlockIfPending("la ricerca sponsor")) return;
        RefreshOpportunities();
        using var dialog = new SponsorSearchDialog(career, AcceptOpportunity, DeclineOpportunity);
        dialog.ShowDialog(this);
        SaveCareer(); RefreshUi();
    }

    /// <summary>
    /// Accetta una proposta: paga il costo, applica le conseguenze e la traduce in
    /// un appuntamento reale in agenda o in un contratto.
    /// </summary>
    private bool AcceptOpportunity(Opportunity opportunity)
    {
        var report = ConsequenceEngine.AcceptOpportunity(career, opportunity);
        if (report == null)
        {
            CareerMessages.Show(null, $"Patrimonio insufficiente: servono € {opportunity.NetCost:N0} e ne hai € {career.Cash:N0}.",
                "Opportunita non sostenibile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        career.Schedule ??= new List<ScheduledEvent>();
        if (opportunity.IsRace || opportunity.IsTest)
        {
            var kind = opportunity.IsTest ? ScheduledEventKind.EvaluationTest : ScheduledEventKind.Invitation;
            // Una gara cade dentro l'anno sportivo, sempre.
            //
            // È il punto in cui ogni proposta diventa un appuntamento vero: i
            // generatori sono molti e ognuno calcolava la propria data a modo
            // suo, quindi qualcuna finiva a gennaio o febbraio. Le prove no:
            // quelle si possono fare anche d'inverno, ed è quando si prova.
            if (kind == ScheduledEventKind.Invitation)
                opportunity.Date = CareerScheduler.DentroLaStagione(opportunity.Date);
            // La gara puo essere gia in calendario: le gare di un sedile cliente
            // vengono pubblicate alla firma, e accettarne una significa
            // confermare quel weekend, non aggiungerne un secondo identico.
            var giaInCalendario = career.Schedule.FirstOrDefault(x =>
                x.IsPlanned
                && !string.IsNullOrWhiteSpace(x.TrackId)
                && x.TrackId.Equals(opportunity.TrackId, StringComparison.OrdinalIgnoreCase)
                && x.Date.Date == opportunity.Date.Date);
            if (giaInCalendario != null)
            {
                giaInCalendario.EntryFee = opportunity.NetCost;
                giaInCalendario.EntryFeePaid = true;
                giaInCalendario.GeneratedBy = $"Weekend confermato con {opportunity.ProposedBy}.";
                rounds = ChampionshipRoundsView();
            }
            else
            CareerScheduler.Append(career.Schedule, new ScheduledEvent
            {
                Id = $"opp-{opportunity.Id}",
                Kind = kind,
                Date = opportunity.Date,
                TrackId = opportunity.TrackId,
                TrackName = opportunity.TrackName,
                Country = opportunity.Tier,
                Season = career.Season,
                GeneratedBy = $"Opportunita accettata: {opportunity.Justification}",
                Objective = opportunity.Objective,
                EntryFee = opportunity.NetCost,
                EntryFeePaid = true,
                ProposedBy = opportunity.ProposedBy
            });
            rounds = ChampionshipRoundsView();
        }
        else if (opportunity.IsSeat)
        {
            // Un sedile accettato diventa un contratto e fa nascere il calendario.
            var previousTeam = career.Team;
            career.Team = opportunity.ProposedBy;
            career.Tier = opportunity.Tier;
            // Un salto di campionato porta con sé il proprio livello: è il
            // punto in cui immagine, forma e conti si trasformano davvero in
            // avanzamento, senza passare dalla classifica.
            var gradinoSedile = string.IsNullOrWhiteSpace(opportunity.CarId)
                ? CareerLadder.Current(career, contentIndex.Cars).Step
                : CareerLadder.ForCar(
                    contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(opportunity.CarId, StringComparison.OrdinalIgnoreCase))?.Category ?? "",
                    contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(opportunity.CarId, StringComparison.OrdinalIgnoreCase))?.PowerHp ?? 0,
                    contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(opportunity.CarId, StringComparison.OrdinalIgnoreCase))?.MassKg ?? 0).Step;
            if (opportunity.GrantsChampionshipLevel > 0
                && opportunity.GrantsChampionshipLevel > career.ChampionshipLevel)
            {
                var da = career.ChampionshipLevel;
                // Anche la chiamata inattesa resta legata alla vettura che
                // porta con sé: un kart non entra in un mondiale.
                career.ChampionshipLevel = ChampionshipLadder.Clamp(Math.Min(
                    opportunity.GrantsChampionshipLevel,
                    ChampionshipLadder.MaxLevelForStep(gradinoSedile)));
                career.RacesAtLastLevelUp = career.Races;
                var salto = $"{career.Driver} passa a «{ChampionshipLadder.Name(career.ChampionshipLevel)}» (livello {career.ChampionshipLevel} di {ChampionshipLadder.Levels}) su chiamata di {opportunity.ProposedBy}, senza passare dalla classifica.";
                career.News.Add(salto);
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "CHAMPIONSHIP_PROMOTION",
                    Headline = salto, Track = ChampionshipLadder.Name(career.ChampionshipLevel), Importance = 90
                });
                CareerLog.Info("carriera", $"salto di campionato per chiamata: livello {da} → {career.ChampionshipLevel}");
                // Chi sale a stagione in corso e' in prova. Non ha una
                // classifica da difendere: ha una squadra che si e' esposta, e
                // le prossime gare dicono se la scommessa ha funzionato.
                if (career.ChampionshipLevel > da)
                {
                    career.SaltoAllaGara = career.Races;
                    career.LivelloPrimaDelSalto = da;
                    career.SquadraPrimaDelSalto = career.Team ?? "";
                    MostraChiamata(opportunity, da);
                }
            }
            career.Championship = ChampionshipLadder.Name(career.ChampionshipLevel);
            if (!string.IsNullOrWhiteSpace(opportunity.CarId)) career.Car = opportunity.CarId;
            career.ContractActive = true;
            career.ContractYears = Math.Max(1, career.ContractYears);
            career.ContractSalary = opportunity.Salary;
            career.ContractObjective = opportunity.Objective;
            career.ContractObjectiveStatus = "In corso";
            career.SeatPrestige = opportunity.Kind switch
            {
                OpportunityKind.ProfessionalSeat => 75,
                OpportunityKind.PartiallyFundedSeat => 50,
                _ => 30
            };
            career.CareerPhase = "Active";
            if (career.StoryDate == default)
            {
                var acceptedCar = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase));
                career.StoryDate = StoryStartDateForContent(acceptedCar);
            }
            RegisterTeamChange(previousTeam, career.Team ?? "", career.Car, opportunity.Category, $"Sedile accettato: {Opportunity.KindLabel(opportunity.Kind)}");
            // Una stagione già cominciata non si azzera.
            //
            // Qui si buttava via tutta l'agenda e si rimetteva il contatore dei
            // round a zero: firmando a metà campionato il pilota ricominciava
            // dal round 1, e siccome i sedili arrivavano di continuo la
            // stagione non arrivava mai in fondo — otto anni di storia con il
            // contatore fermo a «stagione 1», nessuna classifica finale,
            // nessun titolo. E spariva anche quello che il pilota aveva già
            // pagato: test e gare su invito comprati con i propri soldi.
            if (StagioneGiaCominciata())
            {
                var rinvio = $"{career.Driver} firma con {career.Team}: il sedile vale dalla stagione {career.Season + 1}, la stagione in corso si chiude dov'era.";
                career.News.Add(rinvio);
                career.Events.Add(new CareerEventRecord
                {
                    DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SEAT_SIGNED_NEXT_SEASON",
                    Headline = rinvio, Track = career.Team ?? "", Importance = 76
                });
                CareerLog.Info("mercato", $"sedile firmato a stagione in corso: parte dalla stagione {career.Season + 1}.");
            }
            else
            {
                AggiornaObiettivoDiContratto();
                career.SeasonStartDate = NarrativeCalendar.SeasonStartAfterSigning(career.StoryDate);
                career.Round = 0;
                // Solo i round: un test o una gara su invito già pagati restano
                // in agenda, sono soldi del pilota.
                career.Schedule.RemoveAll(x => x.IsPlanned && x.Kind == ScheduledEventKind.ChampionshipRound);
                GenerateSeasonSchedule($"Accettazione del sedile con {career.Team}");
            }
        }

        var headline = $"{career.Driver} accetta: {opportunity.Title}.";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "OPPORTUNITY_ACCEPTED",
            Headline = headline, Track = string.IsNullOrWhiteSpace(opportunity.TrackName) ? opportunity.ProposedBy : opportunity.TrackName,
            Importance = opportunity.IsSeat ? 84 : 60
        });
        CareerLog.Info("opportunita", $"accettata «{opportunity.Title}» · {report.Explain()}");
        CareerMessages.Show(null, $"{opportunity.Title}\n\n{report.Explain()}\n\nPatrimonio: € {career.Cash:N0}",
            "Opportunità accettata", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SaveCareer(); RefreshUi();
        return true;
    }

    private void DeclineOpportunity(Opportunity opportunity)
    {
        var report = ConsequenceEngine.DeclineOpportunity(career, opportunity);
        var headline = $"{career.Driver} rifiuta la proposta di {opportunity.ProposedBy}: {opportunity.Title}.";
        career.News.Add(headline);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "OPPORTUNITY_DECLINED",
            Headline = headline, Track = opportunity.ProposedBy, Importance = 48
        });
        CareerLog.Info("opportunita", $"rifiutata «{opportunity.Title}» · {report.Explain()}");
        CareerMessages.Show(null, report.Explain(), "Proposta rifiutata", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SaveCareer(); RefreshUi();
    }

    private SessionPlanRecord? CurrentSessionPlan(string mode) =>
        career.SessionPlans?.LastOrDefault(x => x.Season == career.Season && x.Round == career.Round + 1 && x.Mode.Equals(mode, StringComparison.OrdinalIgnoreCase));

    private string LocateContentManager()
    {
        var candidates = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Content Manager.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Content Manager.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Content Manager", "Content Manager.exe")
        };
        var direct = candidates.FirstOrDefault(File.Exists);
        if (!string.IsNullOrWhiteSpace(direct)) return direct;
        try
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")!);
            var shortcutType = shell?.GetType();
            foreach (var link in Directory.EnumerateFiles(desktop, "*.lnk", SearchOption.TopDirectoryOnly).Where(x => Path.GetFileNameWithoutExtension(x).Contains("content manager", StringComparison.OrdinalIgnoreCase)))
            {
                var shortcut = shortcutType?.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { link });
                var target = shortcut?.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.GetProperty, null, shortcut, null) as string;
                if (!string.IsNullOrWhiteSpace(target) && File.Exists(target)) return target;
            }
        }
        catch { }
        return "";
    }
    private void ReopenPendingWeekend()
    {
        var pendingPath = Path.Combine(saveDir, "pending_weekend.json");
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(pendingPath));
            var preset = doc.RootElement.TryGetProperty("preset", out var p) ? p.GetString() ?? "" : "";
            var cm = LocateContentManager();
            if (string.IsNullOrWhiteSpace(preset) || !File.Exists(preset)) { CareerMessages.Show(null, "Il preset del weekend pendente non è più disponibile. Nessun risultato è stato registrato.", "CorsaCareer - preset mancante", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!ContentManagerPresetValidator.TryValidate(preset, out var presetError, contentIndex.Cars.Where(ContentCategoryRules.IsRaceable).Select(x => x.Id), contentIndex.Tracks.Select(x => x.Id)))
            {
                CareerMessages.Show(null, $"Il weekend pendente non può essere riaperto: {presetError}. Aggiorna i contenuti e prepara un nuovo weekend. Nessun risultato è stato registrato.", "CorsaCareer - contenuto cambiato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(cm)) { CareerMessages.Show(null, "Content Manager non è stato trovato. Il preset resta disponibile nella cartella dei preset.", "CorsaCareer - CM mancante", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            OpenContentManagerPreset(cm, preset);
        }
        catch (Exception error) { CareerMessages.Show(null, $"Impossibile riaprire il weekend pendente: {error.Message}", "CorsaCareer", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private static void OpenContentManagerPreset(string contentManagerPath, string presetPath)
    {
        Process.Start(ContentManagerLaunch.Build(contentManagerPath, presetPath));
    }
}

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // La musica va fermata qui, non soltanto alla chiusura della finestra.
        // Il canale MCI vive nel processo e non in un programma esterno: se la
        // finestra si chiude per una via che non scatena FormClosed — un errore
        // non gestito, una chiusura forzata, una dissolvenza d'uscita che
        // annulla l'evento — il brano continuava a suonare senza piu nessuna
        // finestra da cui fermarlo.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => SilenceAudio();
        Application.ApplicationExit += (_, _) => SilenceAudio();
        AppDomain.CurrentDomain.UnhandledException += (_, _) => SilenceAudio();

        try { Application.Run(new MainForm()); }
        finally { SilenceAudio(); }
    }

    /// <summary>
    /// Spegne ogni canale audio. Deve poter essere chiamata più volte senza
    /// conseguenze: viene invocata da più vie d'uscita, e nessuna sa se un'altra
    /// l'ha già fatto.
    /// </summary>
    private static void SilenceAudio()
    {
        try { SoundtrackService.Stop(); } catch { }
        try { NarrationService.Stop(); } catch { }
    }
}
