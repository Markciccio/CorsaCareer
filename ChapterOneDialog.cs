using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

public enum ChapterOneBeat { Prelude, Offer, ConfirmationTest, Invitation, Retry }

/// <summary>
/// Scene brevi e concrete del primo capitolo. Il dialogo non decide mai un
/// risultato: rende leggibile il verdetto che arriva dal referto o dal debug.
/// </summary>
public sealed class ChapterOneDialog : CareerDialog
{
    /// <summary>
    /// La scena delle voci del box, in attesa di aprirsi da se.
    ///
    /// Viene svuotata alla prima apertura: la storia scorre una volta, e non
    /// si riavvia a ogni ridisegno della finestra.
    /// </summary>
    private Action? scenaDaAprire;

    /// <summary>
    /// Dopo un risultato non serve una pagina di attesa: il giocatore entra
    /// direttamente nella scena anime. Il Prelude mantiene invece la sua
    /// pagina introduttiva, perché precede la prima sessione.
    /// </summary>
    public static void ShowOutcomeAnime(CareerState career, ChapterOneBeat beat, IWin32Window owner)
    {
        StoryCastService.Ensure(career);
        var manager = StoryCastService.Get(career, StoryCastService.Manager);
        var mechanic = StoryCastService.Get(career, StoryCastService.Mechanic);
        var rival = StoryCastService.Get(career, StoryCastService.Rival);
        var scene = Scene(career, beat, manager, mechanic, rival);
        var speakers = new[] { manager, mechanic, rival };
        var lines = scene.Dialogues.Select((line, i) =>
        {
            var speaker = speakers.FirstOrDefault(x => x.Name.Equals(line.Speaker, StringComparison.OrdinalIgnoreCase));
            var id = speaker?.Id ?? (i == 0 ? StoryCastService.Manager : i == 1 ? StoryCastService.Mechanic : StoryCastService.Rival);
            var expression = ExpressionFor(id, beat);
            return new AnimeDialogueLine(line.Speaker, line.Text, PortraitFile(id, expression), expression);
        }).ToList();
        // Una scena nuova merita un brano nuovo, anche nella stessa atmosfera.
        SoundtrackService.MarkNewScene();
        using var dialog = new AnimeDialogueDialog($"CorsaCareer — {scene.Title}", lines);
        dialog.ShowDialog(owner);
    }

    /// <summary>
    /// Le reazioni del gruppo dopo una gara.
    ///
    /// Le scene del capitolo raccontano tutte la <em>prova</em>: parlano di
    /// cronometro, riferimento e scarto perché nascono da
    /// <see cref="LatestTestFacts"/>. Dopo una gara quei numeri non c'entrano —
    /// contano la posizione, chi ha vinto, il distacco e i punti. Qui parlano
    /// degli stessi fatti che il giocatore ha appena letto nella classifica, e
    /// c'è anche Haru, che nelle scene del test non compare mai.
    /// </summary>
    /// <summary>
    /// Le reazioni al rientro ai box.
    /// </summary>
    /// <param name="vetture">Contenuti installati: servono i nomi veri, non gli identificativi.</param>
    /// <param name="circuiti">Come sopra, per il circuito.</param>
    /// <param name="agenda">L'agenda della stagione: da qui si sa quante gare restano.</param>
    public static void ShowRaceReactions(CareerState career, RaceHistoryEntry race, IWin32Window owner,
        IReadOnlyList<ContentCarRecord> vetture, IReadOnlyList<ContentTrackRecord> circuiti,
        IReadOnlyList<ScheduledEvent> agenda)
    {
        if (race == null) return;
        StoryCastService.Ensure(career);

        // Due o tre persone, scelte da quello che è successo.
        //
        // Prima parlavano tutti e sette, in fila, dopo ogni singola gara: dodici
        // battute che dovevano funzionare in qualunque situazione e quindi non
        // dicevano niente di specifico. Ogni domenica sembrava la precedente.
        var contestuali = RaceReactions.Build(career, race, vetture, circuiti, agenda);
        if (contestuali.Count > 0)
        {
            var titoloScena = race.Dnf ? "ritiro" : race.Position == 1 ? "vittoria" : race.Position <= 3 ? "podio" : "rientro ai box";
            SoundtrackService.MarkNewScene();
            SoundtrackService.PlayForMood(race.Position == 1 || race.Position <= 3 ? "vittoria" : race.Dnf ? "crisi" : "griglia");
            using var scena = new AnimeDialogueDialog($"CorsaCareer — {titoloScena}", contestuali);
            scena.ShowDialog(owner);
            return;
        }
        var manager = StoryCastService.Get(career, StoryCastService.Manager);
        var mechanic = StoryCastService.Get(career, StoryCastService.Mechanic);
        var rival = StoryCastService.Get(career, StoryCastService.Rival);
        var friend = StoryCastService.Get(career, StoryCastService.Friend);

        var partenti = Math.Max(1, race.Classification.Count);
        var vincitore = race.Classification.OrderBy(x => x.Position).FirstOrDefault();
        var haVinto = !race.Dnf && race.Position == 1;
        var podio = !race.Dnf && race.Position <= 3;
        var buono = !race.Dnf && race.Position <= Math.Ceiling(partenti / 2d);
        var guadagnate = race.StartingPosition > 0 ? race.StartingPosition - race.Position : 0;

        var esito = race.Dnf
            ? "ritiro"
            : haVinto ? "vittoria"
            : podio ? "podio"
            : buono ? "buon piazzamento"
            : "gara in salita";

        var titolo = race.Dnf ? "Non siamo arrivati in fondo"
            : haVinto ? "Hai vinto"
            : podio ? "Sul podio"
            : buono ? "Dentro il gruppo che conta"
            : "C'è da lavorare";

        // Ogni battuta cita solo dati che la gara ha davvero prodotto.
        var reiTesto = race.Dnf
            ? $"A {race.Track} non siamo arrivati in fondo. Un ritiro non si commenta, si spiega: voglio sapere cosa hai sentito prima di fermarti."
            : haVinto
                ? $"Primo su {partenti} a {race.Track}. Questa la scrivo io stessa nel rapporto: {race.Points} punti e € {race.Prize:N0} di premio. Adesso il difficile è ripeterlo."
                : $"P{race.Position} su {partenti} a {race.Track}"
                  + (guadagnate > 0 ? $", {guadagnate} posizioni guadagnate dalla partenza" : guadagnate < 0 ? $", {Math.Abs(guadagnate)} perse dalla partenza" : "")
                  + $". {race.Points} punti, € {race.Prize:N0}. Non è un verdetto: è un dato su cui costruire.";

        var genjiTesto = race.Dnf
            ? $"La macchina l'ho preparata io, quindi il primo che controlla i pezzi sono io. Dopo {race.Laps} giri qualcosa ha ceduto: prima di rifarla voglio capire cosa."
            : $"{race.Laps} giri, miglior giro {(race.BestLapMilliseconds > 0 ? Lap(race.BestLapMilliseconds) : "non registrato")}"
              + (race.PenaltySeconds > 0 ? $", e {race.PenaltySeconds:0} secondi di penalità che ci siamo presi da soli" : ", pulito")
              + $". {(podio ? "Il passo c'era. Teniamocelo." : "Il passo si può migliorare, e si migliora al banco, non lamentandosi.")}";

        var rikuTesto = haVinto
            ? "Hai vinto. Una volta. Io ti aspetto alla prossima, quando non sarà una sorpresa per nessuno."
            : podio
                ? $"P{race.Position}. Bene. Adesso però il podio devi difenderlo, e difenderlo è un'altra cosa."
                : race.Dnf
                    ? "Chi si ritira non ha una posizione da difendere. Comodo, per oggi."
                    : $"P{race.Position}{(vincitore != null && !vincitore.IsPlayer ? $", mentre davanti c'era {vincitore.Name}" : "")}. La classifica non discute: dice solo dove sei.";

        var haruTesto = race.Dnf
            ? $"Ho tenuto i conti anche oggi: in cassa restano € {career.Cash:N0}. Un ritiro pesa più sul morale che sul bilancio, ma il bilancio lo guardo lo stesso."
            : $"Ho seguito tutto dal muretto. Il tuo nome è girato: livello influencer {career.ReputationProfile?.PublicPopularity ?? 0}/100, cassa € {career.Cash:N0}. "
              + (podio ? "Con un risultato così qualche sponsor si fa vivo da solo." : "Non basta per farli chiamare, ma serve per non farsi dimenticare.");

        var strategist = StoryCastService.Get(career, StoryCastService.Strategist);
        var journalist = StoryCastService.Get(career, StoryCastService.Journalist);
        var engineer = StoryCastService.Get(career, StoryCastService.Engineer);

        var luogo = string.IsNullOrWhiteSpace(race.Track) ? "il circuito" : race.Track;
        var distacco = race.GapMilliseconds > 0 ? $"{race.GapMilliseconds / 1000d:0.000} s" : "";
        var nomeVincitore = vincitore != null && !vincitore.IsPlayer ? vincitore.Name : "";

        // La scena segue l'ordine in cui il pilota incontra davvero le persone
        // rientrando ai box: prima chi ha in mano la macchina, poi chi decide,
        // poi chi gli sta accanto, poi chi guarda da fuori, e per ultimo chi
        // corre contro di lui.
        var mecc = race.Dnf ? "frustrated" : haVinto ? "celebrating" : podio ? "proud" : "neutral";
        var dir = race.Dnf ? "concerned" : podio ? "relieved" : "neutral";
        var ami = race.Dnf ? "worried" : podio ? "proud" : "relieved";
        var stra = race.Dnf ? "worried" : podio ? "relieved" : "concerned";
        var giorn = podio ? "proud" : "neutral";
        var riv = haVinto || podio ? "defeated" : "taunting";

        var lines = new List<AnimeDialogueLine>
        {
            // --- il box, appena rientrato
            Battuta(mechanic, StoryCastService.Mechanic,
                race.Dnf
                    ? $"Fermo lì, non toccare niente. Da qui la macchina la guardo io: se si è rotta a {luogo} voglio sapere se è colpa sua o nostra."
                    : $"Casco giù, respira. {race.Laps} giri a {luogo}: la macchina è tornata intera, ed è già più di quanto capiti a molti oggi.",
                mecc),
            Battuta(mechanic, StoryCastService.Mechanic, genjiTesto, mecc),

            // --- chi decide
            Battuta(manager, StoryCastService.Manager, reiTesto, dir),
            Battuta(manager, StoryCastService.Manager,
                race.Dnf
                    ? "Non ti sto rimproverando. Ti sto dicendo che un ritiro va spiegato a chi mette i soldi, e quella spiegazione la scriviamo insieme."
                    : haVinto
                        ? "E adesso la parte difficile: da domani tutti si aspetteranno questo. Vincere una volta apre una porta; restarci dentro è un altro mestiere."
                        : $"Il paddock non guarda la singola domenica, guarda la riga sotto: costanza. {(buono ? "Oggi quella riga è migliorata." : "Oggi quella riga non si è mossa.")}",
                dir),

            // --- chi ti sta accanto
            Battuta(friend, StoryCastService.Friend,
                race.Dnf
                    ? "Io ti ho visto entrare in curva bene, sai. Poi si è fermato tutto e non è dipeso da te. Non buttare via la giornata solo perché è finita male."
                    : haVinto
                        ? "Ho urlato così tanto che mi sono perso l'ultimo giro. Scusami. Ma l'ho visto, il momento in cui sei passato davanti."
                        : $"Ti ho cronometrato da bordo pista{(distacco.Length > 0 ? $": {distacco} dal primo" : "")}. Non è una condanna, è una misura. Le misure si migliorano.",
                ami),
            Battuta(friend, StoryCastService.Friend, haruTesto, ami),

            // --- chi legge i numeri e gli sponsor
            Battuta(strategist, StoryCastService.Strategist,
                race.Dnf
                    ? $"Metto giù i conti senza girarci intorno: la trasferta è stata pagata, il premio no. In cassa restano € {career.Cash:N0} e la prossima quota arriverà comunque."
                    : podio
                        ? $"Un piazzamento così cambia il tono delle telefonate. Con € {race.Prize:N0} di premio e un risultato da mostrare posso bussare a porte che finora restavano chiuse."
                        : $"Nessuno firma per un P{race.Position}, ma nessuno chiude la porta. Tengo aperti i contatti e aspetto un risultato da mettere sul tavolo.",
                stra),

            // --- chi racconta da fuori
            Battuta(journalist, StoryCastService.Journalist,
                race.Dnf
                    ? "Una domanda sola e poi ti lascio: quando hai capito che era finita? Lo chiedo perché la gente legge i ritiri come resa, e quasi mai lo sono."
                    : haVinto
                        ? $"Prima vittoria a {luogo}. Il titolo lo scrivo stasera, ma la frase che userò voglio sentirla da te: che cosa è cambiato oggi rispetto alle altre volte?"
                        : $"P{race.Position} su {partenti}{(nomeVincitore.Length > 0 ? $", vince {nomeVincitore}" : "")}. Lo scrivo così com'è. Poi però mi interessa il tuo passo negli ultimi giri: quello dice più della posizione.",
                giorn),

            // --- il tecnico
            Battuta(engineer, StoryCastService.Engineer,
                race.BestLapMilliseconds > 0
                    ? $"Ho i dati: miglior giro {Lap(race.BestLapMilliseconds)}. Non è il tempo che mi interessa, è dove lo hai fatto — {(podio ? "in mezzo al traffico, ed è questo che conta." : "da solo, in pista libera. In gara serve farlo con qualcuno davanti.")}"
                    : "Senza un giro pulito registrato non ho niente da analizzare. La prossima volta portami dei dati, anche brutti: sui dati si lavora, sulle impressioni no.",
                "neutral"),

            // --- il rivale, per ultimo
            Battuta(rival, StoryCastService.Rival, rikuTesto, riv),
            Battuta(rival, StoryCastService.Rival,
                haVinto
                    ? "Non ti faccio i complimenti. Te li farei se lo rifacessi la prossima volta, con me dietro che ci provo davvero."
                    : race.Dnf
                        ? "Un consiglio, gratis: la gente ricorda chi arriva, non chi spiega perché non è arrivato."
                        : "Ci rivediamo in griglia. Porta qualcosa di più della voglia di esserci.",
                riv)
        };

        // La transizione audio la fa la scena, non un refresh che passa di lì.
        //
        // Dopo un test il brano cambiava perché al termine c'era comunque un
        // RefreshUi, ed era quello a chiamare PlayForMood. Dopo una gara la
        // scena arriva invece dopo l'ultimo refresh: restava MarkNewScene
        // senza nessuno che lo raccogliesse, e i risultati entravano in
        // silenzio. Qui il tema viene scelto dall'esito e parte subito.
        SoundtrackService.MarkNewScene();
        SoundtrackService.PlayForMood(haVinto || podio ? "vittoria" : race.Dnf ? "crisi" : "griglia");
        using var dialog = new AnimeDialogueDialog($"CorsaCareer — {titolo} · {luogo} ({esito})", lines);
        dialog.ShowDialog(owner);
    }

    private static AnimeDialogueLine Battuta(StoryCharacter? speaker, string fallbackId, string text, string expression)
    {
        var id = speaker?.Id ?? fallbackId;
        var name = string.IsNullOrWhiteSpace(speaker?.Name) ? fallbackId : speaker!.Name;
        return new AnimeDialogueLine(name, text, PortraitFile(id, expression), expression);
    }

    public ChapterOneDialog(CareerState career, ChapterOneBeat beat)
    {
        StoryCastService.Ensure(career);
        var manager = StoryCastService.Get(career, StoryCastService.Manager);
        var mechanic = StoryCastService.Get(career, StoryCastService.Mechanic);
        var rival = StoryCastService.Get(career, StoryCastService.Rival);
        var scene = Scene(career, beat, manager, mechanic, rival);
        SoundtrackService.PlayForMood(beat switch
        {
            ChapterOneBeat.Offer => "sponsor",
            ChapterOneBeat.Invitation => "griglia",
            ChapterOneBeat.ConfirmationTest or ChapterOneBeat.Prelude => "concentrazione",
            _ => "crisi"
        });

        Text = "CorsaCareer — la prima prova";
        Size = new Size(1400, 900);
        MinimumSize = new Size(820, 560);
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = UiTheme.HeaderBackground, Padding = new Padding(42, 20, 42, 10) };
        header.Controls.Add(new Label { Text = scene.Kicker, Dock = DockStyle.Top, Height = 22, Font = UiTheme.Kicker, ForeColor = UiTheme.Accent });
        header.Controls.Add(new Label { Text = scene.Title, Dock = DockStyle.Bottom, Height = 34, Font = UiTheme.Title, ForeColor = UiTheme.TextPrimary });

        var body = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 620, BackColor = UiTheme.Border };
        body.Panel1MinSize = 360;
        body.Panel1.BackColor = Color.FromArgb(7, 9, 12);
        body.Panel1.Padding = new Padding(32);
        var art = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(7, 9, 12) };
        var asset = AssetPaths.File(scene.Artwork);
        if (File.Exists(asset))
        {
            using var source = Image.FromFile(asset);
            art.Image = new Bitmap(source);
            art.Disposed += (_, _) => art.Image?.Dispose();
        }
        body.Panel1.Controls.Add(art);

        var words = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = UiTheme.Background, Padding = new Padding(42, 36, 42, 20) };
        var width = Math.Max(680, Math.Min(1040, body.Panel2.ClientSize.Width - 96));
        words.Controls.Add(Block(scene.Standfirst, UiTheme.Standfirst, UiTheme.TextSecondary, width, 0, 18));
        var speakers = new[] { manager, mechanic, rival };
        Action openAnime = () =>
        {
            var lines = scene.Dialogues.Select((line, i) =>
            {
                var speaker = speakers.FirstOrDefault(x => x.Name.Equals(line.Speaker, StringComparison.OrdinalIgnoreCase));
                var id = speaker?.Id ?? (i == 0 ? StoryCastService.Manager : i == 1 ? StoryCastService.Mechanic : StoryCastService.Rival);
                var expression = ExpressionFor(id, beat);
                return new AnimeDialogueLine(line.Speaker, line.Text, PortraitFile(id, expression), expression);
            }).ToList();
            using var sceneDialog = new AnimeDialogueDialog($"CorsaCareer — {scene.Title}", lines);
            sceneDialog.ShowDialog(this);
        };
        // Referto, bilancio e archivio restano sulla home. Qui non duplico
        // numeri: questa e' soltanto la soglia tra il risultato e le persone
        // che gli danno un significato.
        words.Controls.Add(Block("IL CRONOMETRO HA SMESSO DI PARLARE. ADESSO PARLA IL BOX.", UiTheme.Kicker, UiTheme.Warning, width, 12, 6));
        // La scena arriva da sola: la storia scorre, non si sceglie da un menu.
        //
        // Prima c'era un pulsante da premere per vedere le voci del box — la
        // parte migliore del capitolo nascosta dietro un clic. Le persone che
        // danno un significato al cronometro parlano subito dopo il verdetto,
        // senza chiedere il permesso.
        scenaDaAprire = openAnime;
        words.Controls.Add(Block("COSA CAMBIA ADESSO", UiTheme.Kicker, UiTheme.Positive, width, 14, 3));
        words.Controls.Add(Block(scene.Consequence, UiTheme.ProseStrong, UiTheme.TextPrimary, width, 0, 6));
        body.Panel2.Controls.Add(words);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 82, BackColor = UiTheme.HeaderBackground, Padding = new Padding(42, 16, 42, 16) };
        footer.Controls.Add(new Label { Text = "Le prestazioni restano esclusivamente quelle del referto di Assetto Corsa o della modalità debug dichiarata.", Dock = DockStyle.Left, Width = 760, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft });
        var proceed = UiTheme.PrimaryButton(beat == ChapterOneBeat.Prelude ? "PREPARA IL TEST" : "CONTINUA LA STORIA");
        proceed.Dock = DockStyle.Right; proceed.Width = 245; proceed.DialogResult = DialogResult.OK;
        footer.Controls.Add(proceed);

        Controls.Add(body); Controls.Add(footer); Controls.Add(header);
        // La distanza va imposta anche subito: su handle già creato il primo
        // frame mostrava altrimenti la ripartizione iniziale del costruttore.
        FitArtwork(body);
        Shown += (_, _) =>
        {
            FitArtwork(body); RefitText(words);
            // Una volta sola: la scena scorre appena il capitolo e leggibile,
            // e non si riapre a ogni ridisegno della finestra.
            if (scenaDaAprire == null) return;
            var apri = scenaDaAprire; scenaDaAprire = null;
            BeginInvoke(new Action(() => { if (!IsDisposed) apri(); }));
        };
        Resize += (_, _) => { if (IsHandleCreated) { FitArtwork(body); RefitText(words); } };
        // Esc non deve valere come conferma: il prelude viene letto dal
        // chiamante come «procedi», e con CancelButton = proceed una fuga con
        // Esc faceva avanzare la storia invece di annullare la scena.
        AcceptButton = proceed;
    }

    /// <summary>
    /// Riadatta larghezze e altezze quando la larghezza reale del pannello e'
    /// nota. Nel costruttore lo SplitContainer non e' ancora stato disposto,
    /// quindi ogni testo era misurato su una larghezza provvisoria: le battute
    /// piu' lunghe andavano a capo piu' volte del previsto e le ultime righe
    /// restavano fuori dalla card, tagliate a meta' frase.
    /// </summary>
    private static void RefitText(FlowLayoutPanel words)
    {
        var available = words.ClientSize.Width - words.Padding.Horizontal;
        if (available <= 0) return;
        // La barra di scorrimento verticale, quando compare, sottrae larghezza:
        // senza tenerne conto il testo verrebbe misurato piu' largo del reale.
        if (words.VerticalScroll.Visible) available -= SystemInformation.VerticalScrollBarWidth;
        var width = Math.Max(360, available);
        foreach (Control control in words.Controls)
        {
            control.Width = width;
            if (control is Panel card && card.Tag is Line line)
            {
                var quote = card.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?
                    .Controls.OfType<Panel>().FirstOrDefault()?
                    .Controls.OfType<Label>().FirstOrDefault(x => x.Dock == DockStyle.Fill);
                card.Height = QuoteCardHeight(line, quote?.Font ?? card.Font, width);
            }
            else if (control is Label label)
            {
                var measured = TextRenderer.MeasureText(string.IsNullOrEmpty(label.Text) ? "Ag" : label.Text, label.Font,
                    new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                label.Height = Math.Max(label.Font.Height + 4, measured.Height + 6);
            }
        }
    }

    private static void FitArtwork(SplitContainer body)
    {
        // La distanza iniziale viene ridimensionata prima del layout definitivo
        // su alcuni monitor ad alto DPI. Il pannello sinistro deve restare una
        // vignetta leggibile, non una striscia decorativa.
        if (body.ClientSize.Width <= 0) return;
        var desired = Math.Clamp((int)(body.ClientSize.Width * .45), 520, 760);
        if (body.ClientSize.Width > desired + body.Panel2MinSize + body.SplitterWidth)
            body.SplitterDistance = desired;
    }

    private static Label Block(string text, Font font, Color color, int width, int top, int bottom)
    {
        var label = new Label { Text = text, Font = font, ForeColor = color, Width = width, AutoSize = false, Margin = new Padding(0, top, 0, bottom), UseMnemonic = false };
        // Le Label WinForms disegnano con TextRenderer (GDI): misurare con
        // MeasureString (GDI+) dava un'altezza diversa da quella reale e le
        // ultime righe dei paragrafi lunghi restavano tagliate.
        var measured = TextRenderer.MeasureText(string.IsNullOrEmpty(text) ? "Ag" : text, font,
            new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        label.Height = Math.Max(font.Height + 4, measured.Height + 6);
        return label;
    }

    // Larghezza della colonna del ritratto e spazio perso in bordi, margini e
    // padding del balloon. Erano numeri diversi fra il calcolo dell'altezza e
    // il layout reale (196 contro 220): la battuta veniva misurata su una
    // colonna piu' larga di quella vera, andava a capo piu' volte del previsto
    // e le ultime righe finivano fuori dalla card.
    private const int PortraitColumnWidth = 220;
    private const int BubbleChrome = 74;

    /// <summary>Altezza che la card deve avere perche' la battuta entri tutta.</summary>
    private static int QuoteCardHeight(Line line, Font quoteFont, int width)
    {
        var usable = Math.Max(240, width - PortraitColumnWidth - BubbleChrome);
        var quote = TextRenderer.MeasureText($"“{line.Text}”", quoteFont,
            new Size(usable, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        // 28 per la riga del nome, il resto per padding del balloon e margini.
        return Math.Max(276, quote.Height + 28 + 62);
    }

    private static Panel DialogueCard(Line line, StoryCharacter? speaker, int width, int index, ChapterOneBeat beat)
    {
        // Ogni balloon ha una battuta diversa: un'altezza fissa tronca il
        // testo proprio quando il commento tecnico diventa interessante.
        var quoteFont = new Font(UiTheme.FamilySans, 15F);
        var card = new Panel { Width = width, Height = QuoteCardHeight(line, quoteFont, width), Margin = new Padding(0, 12, 0, 4), BackColor = Color.FromArgb(12, 14, 19), Padding = new Padding(8) };
        card.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(60, UiTheme.Accent)); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.Transparent };
        // Quando il ritratto e a destra anche la sua colonna deve restare
        // stretta: prima il balloon finiva nella colonna fissa da 156px.
        if (index % 2 == 0)
        {
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        }
        else
        {
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        }
        var portrait = speaker == null ? null : Portrait(speaker.Id, ExpressionFor(speaker.Id, beat));
        var image = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(5, 7, 10), Image = portrait, Margin = new Padding(0) };
        // Ogni ritratto è una Bitmap caricata da file: senza questo rilascio le
        // handle GDI restavano allocate a ogni apertura del capitolo.
        if (portrait != null) image.Disposed += (_, _) => portrait.Dispose();
        var bubble = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(238, 235, 223), Margin = new Padding(12, 12, 0, 12), Padding = new Padding(18, 12, 14, 10) };
        var name = new Label { Dock = DockStyle.Top, Height = 28, Text = line.Speaker.ToUpperInvariant(), Font = new Font(UiTheme.FamilySemibold, 10F, FontStyle.Bold), ForeColor = Color.FromArgb(180, 20, 52) };
        var quote = new Label { Dock = DockStyle.Fill, Text = $"“{line.Text}”", Font = quoteFont, ForeColor = Color.FromArgb(25, 28, 34), AutoSize = false, UseMnemonic = false, AutoEllipsis = false };
        bubble.Controls.Add(quote); bubble.Controls.Add(name);
        if (index % 2 == 0) { table.Controls.Add(image, 0, 0); table.Controls.Add(bubble, 1, 0); }
        else
        {
            bubble.Margin = new Padding(0, 12, 12, 12);
            table.Controls.Add(bubble, 0, 0); table.Controls.Add(image, 1, 0);
        }
        card.Controls.Add(table);
        return card;
    }

    private static string ExpressionFor(string id, ChapterOneBeat beat) => (id, beat) switch
    {
        (StoryCastService.Manager, ChapterOneBeat.Retry or ChapterOneBeat.ConfirmationTest) => "concerned",
        (StoryCastService.Manager, ChapterOneBeat.Offer) => "relieved",
        (StoryCastService.Mechanic, ChapterOneBeat.Retry or ChapterOneBeat.ConfirmationTest) => "worried",
        (StoryCastService.Mechanic, ChapterOneBeat.Offer) => "proud",
        (StoryCastService.Rival, ChapterOneBeat.Offer) => "angry",
        (StoryCastService.Rival, ChapterOneBeat.Retry or ChapterOneBeat.ConfirmationTest) => "smug",
        _ => "neutral"
    };

    private static Image? Portrait(string id, string expression = "neutral")
    {
        var file = PortraitFile(id, expression);
        if (string.IsNullOrWhiteSpace(file)) return null;
        var path = AssetPaths.File(file);
        if (!File.Exists(path)) return null;
        using var source = Image.FromFile(path);
        return new Bitmap(source);
    }

    private static string PortraitFile(string id, string expression = "neutral")
    {
        var expressive = (id, expression) switch
        {
            (StoryCastService.Manager, "concerned") => "character-rei-kisaragi-concerned.png",
            (StoryCastService.Manager, "relieved") => "character-rei-kisaragi-relieved.png",
            (StoryCastService.Mechanic, "worried") => "character-genji-arakawa-worried.png",
            (StoryCastService.Mechanic, "proud") => "character-genji-arakawa-proud.png",
            (StoryCastService.Rival, "angry") => "character-riku-hayase-angry.png",
            (StoryCastService.Rival, "smug") => "character-riku-hayase-smug.png",
            (StoryCastService.Rival, "defeated") => "character-riku-hayase-defeated.png",
            (StoryCastService.Rival, "taunting") => "character-riku-hayase-taunting.png",
            (StoryCastService.Mechanic, "celebrating") => "character-genji-arakawa-celebrating.png",
            (StoryCastService.Mechanic, "frustrated") => "character-genji-arakawa-frustrated.png",
            // Il cast oltre i tre del test aveva un solo ritratto neutro: in una
            // scena lunga ricompariva sempre con la stessa faccia qualunque cosa
            // fosse successa in pista.
            (StoryCastService.Friend, "proud") => "character-haru-senda-podium-elated.png",
            (StoryCastService.Friend, "worried") => "character-haru-senda-anxious-data.png",
            (StoryCastService.Friend, "relieved") => "character-haru-senda-relieved-small-sponsor-kart-paddock.png",
            (StoryCastService.Strategist, "relieved") => "character-miki-arisawa-sponsor-relieved.png",
            (StoryCastService.Strategist, "worried") => "character-miki-arisawa-sponsor-worried.png",
            (StoryCastService.Strategist, "concerned") => "character-miki-arisawa-rejection-composed.png",
            (StoryCastService.Journalist, "proud") => "character-noa-minazuki-gt-lead-excited.png",
            _ => ""
        };
        // Gli id dei ruoli sono rimasti agli identificativi storici (Marta,
        // Gianni, Nico, Nori, Mina...) da quando i personaggi avevano quei
        // nomi. I nomi mostrati in carriera sono cambiati da tempo (Rei
        // Kisaragi, Genji Arakawa, Riku Hayase, Haru Senda, Miki Arisawa...)
        // e le tavole sono state rigenerate con i nomi nuovi: il vecchio file
        // non esiste più, quindi il ritratto restava vuoto.
        return !string.IsNullOrWhiteSpace(expressive) ? expressive : id switch
        {
            StoryCastService.Manager => "character-rei-kisaragi-stern.png",
            StoryCastService.Mechanic => "character-genji-arakawa.png",
            StoryCastService.Rival => "character-riku-hayase.png",
            StoryCastService.Friend => "character-haru-senda.png",
            StoryCastService.Strategist => "character-miki-arisawa.png",
            StoryCastService.Engineer => "character-shigeo-kanda.png",
            StoryCastService.Coordinator => "character-minato-nakahara-touring-penalty.png",
            StoryCastService.Journalist => "character-noa-minazuki.png",
            _ => ""
        };
    }

    private static string ResultDescription(CareerState career)
    {
        var test = career.TestHistory.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        if (test == null) return "Nessun giro archiviato: il verdetto nasce da una sessione senza riferimento valido.";
        var target = career.EvaluationTargetMilliseconds;
        var lap = test.BestLapMilliseconds > 0 ? Lap(test.BestLapMilliseconds) : "nessun giro valido";
        var gap = test.BestLapMilliseconds > 0 && target > 0 ? test.BestLapMilliseconds - target : 0;
        var gapText = test.BestLapMilliseconds > 0 && target > 0 ? $"{(gap >= 0 ? "+" : "")}{gap / 1000.0:0.000} s" : "n/d";
        return $"{test.Track} · {UiText.Car(test.Car)}\nMiglior giro: {lap} · riferimento: {(target > 0 ? Lap(target) : "n/d")} · scarto: {gapText}\nEsito: {career.RookieEvaluationStatus}\nCosa cambia: forma {career.Fitness}/100 · fiducia nel paddock {career.TeamRelation}/100 · budget € {career.Cash:N0}.";
    }

    private static string Lap(int milliseconds)
    {
        var span = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)span.TotalMinutes}:{span.Seconds:00}.{span.Milliseconds:000}";
    }

    private sealed record Line(string Speaker, string Text);
    private sealed record SceneData(string Kicker, string Title, string Standfirst, string Artwork, List<Line> Dialogues, string Consequence);

    private static SceneData Scene(CareerState career, ChapterOneBeat beat, StoryCharacter manager, StoryCharacter mechanic, StoryCharacter rival)
    {
        var facts = LatestTestFacts(career);
        return beat switch
        {
        ChapterOneBeat.Prelude => new(
            "IL PRIMO CRONOMETRO", "Nessuno ti aspetta", $"{career.Driver} ha una sola sessione per trasformare un casco, una vettura modesta e un obiettivo cronometrico in una possibilità.", "manga-02-first-test.png",
            [new(manager.Name, "Non mi serve un giro eroico. Mi serve capire se sai tornare al box con qualcosa da spiegare."), new(mechanic.Name, "La macchina è quella che è. Tu pensa a farle fare un giro pulito."), new(rival.Name, "Io un riferimento l'ho già lasciato. Vediamo se oggi ne lasci uno anche tu.")],
            "Obiettivo immediato: chiudere un giro valido vicino al riferimento. Non stai ancora cercando un contratto: stai comprando il diritto a un secondo sguardo."),
        ChapterOneBeat.Offer => new(
            "IL VERDETTO", "Hai aperto una porta", $"Il tempo di {career.Driver} ha convinto abbastanza persone da trasformare il test in una trattativa.", "manga-sponsor-table.png",
            [new(manager.Name, $"«Ce l'hai fatta. A {facts.Track} hai fermato il cronometro a {facts.Lap}, e tanto è bastato: "
                + "sul tavolo ci sono delle proposte. Non ti dico quale scegliere, perché è una decisione tua e sarà la prima vera della tua carriera. "
                + "Ti dico solo di guardare che cosa ti danno oltre alla macchina — c'è chi ti fa vedere e chi ti fa crescere, e non sono la stessa cosa.»"),
             new(mechanic.Name, $"«{facts.Laps} giri e nemmeno un errore grosso: il tempo che hanno visto i team è tuo, non è stato un colpo di fortuna. "
                + "Adesso però cambia tutto. In gara ci sono le gomme che calano, la benzina che pesa e gente davanti che non si sposta. "
                + "È lì che si vede se il passo è davvero tuo, e io sono curioso quanto te.»"),
             new(rival.Name, "«Un test lo fanno in tanti. Poi arriva la prima partenza e la metà di quelli sparisce.» — sorride — "
                + "«Ci vediamo in griglia. Non ti auguro buona fortuna, però sono contento che tu ci sia.»")],
            "Si sono aperte le prime offerte. Puoi firmare un sedile, aspettare una proposta migliore o cercare un invito: la scelta è tua e resta nella storia."),
        ChapterOneBeat.ConfirmationTest => new(
            "QUASI", "Sei abbastanza vicino da meritare un altro giorno", $"Il riferimento non è caduto, ma il paddock ha visto abbastanza per non chiudere il dossier di {career.Driver}.", "manga-04-first-setback.png",
            [new(manager.Name, $"«Allora, ti dico com'è andata. A {facts.Track} hai girato in {facts.Lap} e il riferimento era {facts.Target}: "
                + $"ti sono mancati {facts.Gap}. Non è abbastanza per darti un sedile, questo è chiaro. "
                + "Ma è troppo poco perché io chiuda il tuo dossier e passi al prossimo ragazzo. "
                + "Quindi ti do un'altra giornata: non capita spesso, quindi prendila per quello che è.»"),
             new(mechanic.Name, $"«Il {UiText.Car(facts.Car)} l'hai portato a casa intero e hai fatto {facts.Laps} giri: già questo dice qualcosa. "
                + "Il secondo test però sarà su un'altra pista, ed è lì che si capisce tutto. "
                + "Su un circuito che conosci puoi anche azzeccare un giro per caso; su uno nuovo no. "
                + "Vieni al banco domani mattina, guardiamo insieme dove hai perso il tempo.»"),
             new(rival.Name, $"«{facts.Gap}. Lo so che sul tabellone sembrano niente.» — scrolla le spalle — "
                + "«In una categoria vera sono due curve intere. Comunque bravo, sei ancora in gioco. "
                + "Fammi vedere un giro pulito la prossima volta e poi ne riparliamo seriamente.»")],
            "Conseguenza: prova di conferma. Un secondo test su un circuito diverso decide se il passo è ripetibile oppure soltanto un lampo."),
        ChapterOneBeat.Invitation => new(
            "UNA STRADA LATERALE", "Il cronometro non è l'unico giudice", $"Dopo più tentativi, il paddock offre a {career.Driver} una gara minore per dimostrare ciò che il test non ha chiarito.", "manga-rival-grid.png",
            [new(manager.Name, $"«Il tuo {facts.Lap} a {facts.Track} non ha raggiunto il riferimento, e con i test ci fermiamo qui. "
                + "Però c'è un'altra strada e te la propongo: una gara su invito. Non assegna punti di campionato e te la paghi tu, "
                + "quindi pensaci bene. Quello che ci guadagni è che in griglia si vedono cose che un giro solo non dice mai — "
                + "come parti, come stai nel traffico, se tieni la testa a posto quando qualcuno ti tocca.»"),
             new(mechanic.Name, $"«Se portiamo il {UiText.Car(facts.Car)} in griglia io ti chiedo una cosa sola: arrivare in fondo. "
                + "Non serve l'eroismo al primo giro, serve un arrivo pulito e dei dati su cui lavorare. "
                + "L'iscrizione pesa sulla cassa e non voglio che sia buttata via in curva 1.»"),
             new(rival.Name, "«Finalmente una griglia, era ora. Lì non ti puoi nascondere dietro un giro fortunato o una pista che ti piace.» — "
                + "«Dopo la bandiera guardiamo la classifica, e quella non si discute.»")],
            "Conseguenza: gara su invito. Costa denaro e non assegna punti di campionato, ma un buon risultato può riaprire il mercato."),
        _ => new(
            "NON È FINITA", "Il paddock non ha ancora detto sì", $"Il test non ha raggiunto la soglia richiesta. La carriera di {career.Driver} resta aperta, ma non gratuita.", "manga-night-garage.png",
            [new(manager.Name, $"«Ti parlo chiaro perché è quello che ti serve. Il riferimento era {facts.Target}, tu hai fatto {facts.Lap}: "
                + $"{facts.Gap}, e non è poco. Un sedile oggi non te lo posso dare, e se te lo dessi ti farei un danno. "
                + "Però il dossier non lo chiudo. Torna con un piano — non con la voglia di riprovare, con un piano — e ti trovo un'altra giornata.»"),
             new(mechanic.Name, $"«{facts.Laps} giri, e ho guardato tutto. La frenata è pulita, quello sì. "
                + "Il problema è che lasci un paio di metri all'apice e riapri il gas mezzo secondo dopo del necessario: "
                + "moltiplicato per le curve del giro, sono esattamente i decimi che ti mancano. "
                + "Non è una cosa da rivoluzionare, è una cosa da allenare. Ci lavoriamo.»"),
             new(rival.Name, $"«{facts.Gap} dal riferimento. Sul rettilineo puoi anche prendertela con il motore, ma nelle curve lente il tempo l'hai lasciato lì tu, "
                + "e lo sappiamo tutti e due.» — poi, mentre se ne va — «Smettila di dire che ci sei quasi e portami un giro intero senza alzare il piede.»")],
            "Conseguenza: nuovo test di recupero. Restano aperte attività, sponsor e preparazione; nessun contratto verrà inventato.")
        };
    }

    private sealed record TestFacts(string Track, string Car, int Laps, string Lap, string Target, string Gap, int Score, string Status, int Reputation, int TeamRelation, long Cash);

    private static TestFacts LatestTestFacts(CareerState career)
    {
        var test = career.TestHistory.OrderByDescending(x => x.DateUtc).FirstOrDefault();
        var target = test?.TargetLapMilliseconds > 0 ? test.TargetLapMilliseconds : career.EvaluationTargetMilliseconds;
        var lapMilliseconds = test?.BestLapMilliseconds ?? 0;
        var gap = target > 0 && lapMilliseconds > 0 ? lapMilliseconds - target : 0;
        return new TestFacts(
            // Il nome vero del circuito, non l'identificativo della cartella:
            // «kart_akagi» in bocca a un personaggio faceva sembrare la scena
            // un messaggio di sistema travestito da dialogo.
            string.IsNullOrWhiteSpace(test?.Track)
                ? "la pista del test"
                : NarrativeEngine.Capitalize(test.Track.Replace('_', ' ')),
            string.IsNullOrWhiteSpace(test?.Car) ? "kart assegnato" : test.Car,
            test?.Laps ?? 0,
            lapMilliseconds > 0 ? Lap(lapMilliseconds) : "nessun giro valido",
            target > 0 ? Lap(target) : "il riferimento richiesto",
            target > 0 && lapMilliseconds > 0 ? $"{(gap >= 0 ? "+" : "")}{gap / 1000d:0.000} s" : "uno scarto non calcolabile",
            test?.EvaluationScore > 0 ? test.EvaluationScore : career.RookieEvaluationScore,
            string.IsNullOrWhiteSpace(career.RookieEvaluationStatus) ? "in valutazione" : career.RookieEvaluationStatus.ToLowerInvariant(),
            career.Reputation, career.TeamRelation, career.Cash);
    }
}
