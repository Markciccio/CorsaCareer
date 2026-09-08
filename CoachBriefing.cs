using System.Globalization;

namespace CorsaCareer1991;

/// <summary>
/// Quello che l'allenatore ti spiega prima che succeda.
///
/// La carriera ha regole precise — tre vittorie di fila e qualcuno chiama, un
/// seguito sotto cinquanta e non chiama nessuno, dodici gare su un gradino
/// prima che ti offrano quello sopra — e finora non le diceva a nessuno. Il
/// giocatore vedeva arrivare un'offerta e sembrava piovuta dal cielo; oppure
/// non la vedeva arrivare e non sapeva perché.
///
/// Sul modo di scriverle: sono conversazioni fra persone che si conoscono, non
/// battute a effetto. Frasi intere, discorso disteso, e soprattutto i dati
/// veri — il nome della squadra, quante gare sono, su quali circuiti, quanti
/// punti di forma mancano. Una spiegazione ermetica non spiega niente: se dopo
/// averla letta il giocatore non sa cosa fare domani, la scena è inutile.
/// </summary>
public static class CoachBriefing
{
    /// <summary>Le spiegazioni già date, per non ripeterle a ogni gara.</summary>
    public const string DettoInizioCarriera = "briefing-inizio";
    public const string DettoSalto = "briefing-salto";
    public const string DettoGavetta = "briefing-gavetta";
    public const string DettoParametri = "briefing-parametri";

    // I ritratti passano tutti dal regista, che verifica che il file esista e
    // altrimenti ripiega. Scritti a mano, bastava un nome sbagliato perché un
    // personaggio comparisse senza faccia — ed è successo davvero.
    private static string ReiSerena => CastDirector.Ritratto(CastDirector.Rei, "serena");
    private static string ReiSevera => CastDirector.Ritratto(CastDirector.Rei, "severa");
    private static string ReiPreoccupata => CastDirector.Ritratto(CastDirector.Rei, "preoccupata");
    private static string ReiCauta => CastDirector.Ritratto(CastDirector.Rei, "cauta");
    private static string Shigeo => CastDirector.Ritratto(CastDirector.Shigeo, "neutro");
    private static string HaruContento => CastDirector.Ritratto(CastDirector.Haru, "felice");
    private static string Haru => CastDirector.Ritratto(CastDirector.Haru, "");

    /// <summary>Chiave della spiegazione dei tre indicatori.</summary>
    public const string DettoParametriBase = "briefing-indicatori";

    /// <summary>
    /// Che cosa sono i tre numeri in alto e come si muovono.
    ///
    /// Prima era un tour guidato: un velo scuro sulla schermata, un riquadro
    /// illuminato per volta, una freccia e il pulsante «avanti» — tre passaggi
    /// meccanici per dire tre cose. Funzionava come funziona un manuale, cioè
    /// male: si preme avanti tre volte e non resta niente.
    ///
    /// Adesso lo spiegano le persone, e ognuna parla di quello che è affar suo:
    /// i soldi e la visibilità sono il mestiere di Haru, la condizione fisica è
    /// del preparatore, e Rei chiude dicendo come i tre numeri si tengono
    /// insieme. La stessa informazione detta da qualcuno che ha un interesse a
    /// dirtela si ricorda.
    /// </summary>
    public static IReadOnlyList<AnimeDialogueLine> Indicatori(CareerState career)
    {
        var profilo = career.ReputationProfile ?? new ReputationProfile();
        var pilota = string.IsNullOrWhiteSpace(career.Driver) ? "ragazzo" : career.Driver.Split(' ')[0];

        return
        [
            CastDirector.Battuta(CastDirector.Haru,
                $"«Prima che cominciamo ti spiego i tre numeri che vedi lì in alto, perché sono la tua carriera in tre cifre. "
                + $"Il primo è il budget: adesso hai {career.Cash:N0} euro. Con quelli si pagano le iscrizioni, le trasferte e i pezzi rotti, "
                + "e quando finiscono non si corre, punto. Aumentarlo è il mio lavoro: dal pulsante SPONSORIZZAZIONI mi mandi in giro "
                + "a bussare alle porte, e ogni tanto qualcuno apre.»",
                "documenti"),

            CastDirector.Battuta(CastDirector.Haru,
                $"«Il terzo numero invece è il livello influencer, che adesso sta a {profilo.PublicPopularity} su 100. "
                + "È quanto il tuo nome gira fuori dalla pista: social, interviste, gente che si ferma a guardarti. "
                + "Lo fai crescere con i risultati ma anche con il lavoro d'immagine, dal pulsante SOCIAL. "
                + "E non è vanità, {0}: quando quel numero è alto arrivano gli sponsor e arrivano le chiamate. Quando è basso non ti cerca nessuno.»"
                    .Replace("{0}", pilota),
                "felice"),

            CastDirector.Battuta(CastDirector.Shigeo,
                $"«Il numero in mezzo è la condizione fisica, {career.Fitness} su 100, ed è quello che i ragazzi sottovalutano sempre. "
                + "Sale con l'allenamento e con il riposo, scende con le gare e con i viaggi. "
                + "Serve negli ultimi giri, quando le braccia si irrigidiscono e si comincia a frenare un metro prima senza accorgersene: "
                + "è lì che si perdono i decimi, non al primo giro. Si gestisce dal pulsante ATTIVITÀ DEL PILOTA.»",
                "neutro"),

            CastDirector.Battuta(CastDirector.Rei,
                "«E adesso la cosa che conta più delle tre messe insieme: nessuno dei tre numeri basta da solo. "
                + "Con i soldi e senza forma non arrivi in fondo alle gare; con la forma e senza nome non ti chiama nessuno; "
                + "con il nome e senza soldi non ti iscrivi. "
                + "Ogni giornata libera ti dà delle ore: puoi darle all'allenamento, all'immagine o a Haru per gli sponsor. "
                + "Come le spendi è la vera decisione di questa carriera — molto più di come guidi.»",
                "seria")
        ];
    }

    /// <summary>
    /// Il discorso di apertura: che campionato è, quante gare, su quali piste,
    /// che cosa serve per salire e che cosa succede se va male.
    /// </summary>
    public static IReadOnlyList<AnimeDialogueLine> Apertura(CareerState career, LadderRung gradino,
        IReadOnlyList<ScheduledEvent> calendario)
    {
        var livello = ChampionshipLadder.Clamp(career.ChampionshipLevel);
        var squadra = string.IsNullOrWhiteSpace(career.Team) ? "la squadra" : career.Team;
        var campionato = ChampionshipLadder.Name(livello);
        var prossimo = ChampionshipLadder.IsTop(livello) ? "" : ChampionshipLadder.Name(livello + 1);

        var apertura =
            $"«Finalmente ci siamo! Ti è arrivato l'ingaggio da {squadra} e sono contento che ti abbiano dato fiducia, "
            + "perché di ragazzi che chiedevano quel posto ce n'erano altri. "
            + $"Corriamo nel {campionato}, che è il livello {livello} di {ChampionshipLadder.Levels}, con una {gradino.Name.ToLowerInvariant()}: "
            + $"come categoria siamo al gradino {gradino.Step} su {CareerLadder.Steps}, quindi c'è ancora tutta la strada davanti.»";

        var calendarioTesto = DescriviCalendario(calendario);

        var obiettivo =
            $"«Dalla riunione con il team è uscita una cosa sola, ed è chiarissima: dobbiamo arrivare almeno terzi in campionato. "
            + $"Se ci riusciamo saliamo{(string.IsNullOrEmpty(prossimo) ? "" : $" nel {prossimo}")} e ci arrivano proposte da squadre nuove, "
            + "che è esattamente quello per cui stiamo lavorando. "
            + (livello > 1
                ? "Se invece finiamo nelle ultime posizioni, però, ci tocca ricominciare da capo: perdiamo il sedile, scendiamo di livello "
                  + "e per rientrare bisogna rifare una prova davanti a tutti. Dobbiamo assolutamente evitarlo."
                : "Peggio di così non possiamo andare, perché sotto questo campionato non c'è niente: da qui si può solo salire, "
                  + "ma bisogna volerlo.")
            + "»";

        var scorciatoia = string.IsNullOrEmpty(prossimo)
            ? $"«E tieni conto di una cosa: sei già in cima alla scala dei campionati. Qui non si sale più, si difende — "
              + "ed è più difficile che arrivarci.»"
            : $"«Poi tieni conto di un'altra cosa, che quasi nessuno sa e che invece a noi può cambiare la stagione. "
              + $"Se durante l'anno fai davvero vedere di essere in forma — parlo di almeno {CareerProgression.FitnessPerIlSalto} di condizione fisica — "
              + $"e insieme tieni il livello influencer sopra {CareerProgression.InfluencerPerIlSalto}, allora ho sentito che c'è un team del {prossimo} "
              + "che potrebbe farti fare il salto direttamente, anche a stagione in corso, senza aspettare la classifica finale. "
              + $"Non ti chiamano a caso, però: succede se metti insieme {CareerProgression.VittorieDiFilaPerLaChiamata} vittorie di fila, "
              + $"oppure {CareerProgression.PodiDiFilaPerLaChiamata} podi consecutivi, oppure una rimonta da {CareerProgression.RimontaCheFaNotizia} posizioni "
              + "in una gara sola. Sono le cose di cui in paddock si parla per settimane.»";

        var chiusura =
            "«Quindi abbiamo tutte le possibilità di fare bene, ma devi lavorare su tre fronti insieme: allenarti sul serio, "
            + "farti seguire dai tifosi e dagli sponsor, e portare a casa i risultati. "
            + "Nessuno dei tre da solo basta. Io so che ce la puoi fare.»";

        var meccanico =
            "«E un'ultima cosa da parte mia, che di squadre ne ho viste passare tante. "
            + "Non sono tutte uguali, e la differenza non è solo il prezzo. "
            + "Quelle che costano di più di solito ti mettono in vetrina: ci sono i fotografi, gli sponsor importanti, "
            + "e una vittoria con loro ti vale il doppio in visibilità. Quelle piccole costano la metà e ti preparano meglio la macchina, "
            + "ma puoi vincere tutto senza che se ne accorga nessuno. "
            + "Quando arriva il momento di scegliere, chiediti cosa ti manca davvero: i soldi, la macchina o il nome.»";

        return
        [
            CastDirector.Battuta(CastDirector.Rei, apertura, "serena"),
            CastDirector.Battuta(CastDirector.Rei, calendarioTesto, "cauta"),
            CastDirector.Battuta(CastDirector.Rei, obiettivo, "severa"),
            CastDirector.Battuta(CastDirector.Rei, scorciatoia, "cauta"),
            CastDirector.Battuta(CastDirector.Shigeo, meccanico, "neutro"),
            CastDirector.Battuta(CastDirector.Rei, chiusura, "serena")
        ];
    }

    /// <summary>Il calendario detto come lo direbbe una persona, non una tabella.</summary>
    private static string DescriviCalendario(IReadOnlyList<ScheduledEvent> calendario)
    {
        var gare = calendario
            .Where(x => x.Kind == ScheduledEventKind.ChampionshipRound)
            .OrderBy(x => x.Date)
            .ToList();

        if (gare.Count == 0)
            return "«Il calendario non è ancora stato pubblicato: appena il team ce lo manda ti dico dove andiamo e quando. "
                   + "Intanto non stare fermo, che la condizione si perde più in fretta di quanto si costruisca.»";

        var italiano = CultureInfo.GetCultureInfo("it-IT");
        var elenco = new List<string>();
        for (var i = 0; i < gare.Count && i < 8; i++)
        {
            var nome = string.IsNullOrWhiteSpace(gare[i].TrackName) ? gare[i].TrackId : gare[i].TrackName;
            var quando = gare[i].Date.ToString("d MMMM", italiano);
            elenco.Add(i switch
            {
                0 => $"la prima a {nome}, il {quando}",
                1 => $"la seconda a {nome}, il {quando}",
                2 => $"la terza a {nome}, il {quando}",
                _ => $"poi {nome} il {quando}"
            });
        }
        var coda = gare.Count > 8 ? $", e altre {gare.Count - 8} fino a fine stagione" : "";

        return $"«Il campionato sono {gare.Count} gare: {string.Join(", ", elenco)}{coda}. "
               + $"L'ultima è {(string.IsNullOrWhiteSpace(gare[^1].TrackName) ? gare[^1].TrackId : gare[^1].TrackName)}, "
               + $"il {gare[^1].Date.ToString("d MMMM yyyy", italiano)}: quel giorno sapremo com'è andata. "
               + "Segnatele, perché fra una e l'altra c'è tempo per allenarsi e per farsi vedere, e quel tempo vale quanto le gare.»";
    }

    /// <summary>
    /// L'avviso quando una soglia è vicina: il momento in cui una regola smette
    /// di essere teoria e diventa una cosa da fare questa settimana. Lista vuota
    /// se adesso non c'è niente da dire.
    /// </summary>
    public static IReadOnlyList<AnimeDialogueLine> Soglia(CareerState career, LadderRung gradino,
        int gareSulGradino, int vittorieDiFila, int podiDiFila, out string chiave)
    {
        chiave = "";
        var profilo = career.ReputationProfile ?? new ReputationProfile();
        var influencer = profilo.PublicPopularity;
        var forma = career.Fitness;
        var livello = ChampionshipLadder.Clamp(career.ChampionshipLevel);
        var prossimo = ChampionshipLadder.IsTop(livello) ? "" : ChampionshipLadder.Name(livello + 1);

        // 1. Manca una vittoria alla chiamata, e i parametri ci sono già.
        if (vittorieDiFila == CareerProgression.VittorieDiFilaPerLaChiamata - 1
            && influencer >= CareerProgression.InfluencerPerIlSalto
            && forma >= CareerProgression.FitnessPerIlSalto
            && !string.IsNullOrEmpty(prossimo))
        {
            chiave = DettoSalto;
            return
            [
                new("Rei Kisaragi",
                    $"«Ascoltami bene, perché siamo in un momento importante. Hai vinto le ultime {vittorieDiFila} gare di fila, "
                    + $"e in questo momento hai {forma} di condizione fisica e {influencer} di livello influencer: "
                    + "sono tutti e due sopra la soglia che serve. "
                    + $"Vuol dire che se vinci anche la prossima — una sola, la prossima — quel team del {prossimo} di cui ti parlavo "
                    + "può chiamarti davvero, e non aspetterebbe la fine del campionato. Ti prenderebbero subito.»",
                    ReiCauta, "tesa"),
                new("Haru Senda",
                    "«Una gara. Ti rendi conto? Una gara e ci saltiamo un campionato intero. "
                    + "Non voglio metterti pressione, ma questa cosa non capita spesso e io stanotte non dormo.»",
                    HaruContento, "elettrizzato")
            ];
        }

        // 2. I risultati ci sono, i parametri no: è l'informazione che
        //    trasforma la frustrazione in un piano concreto.
        if ((vittorieDiFila >= 2 || podiDiFila >= 3)
            && (influencer < CareerProgression.InfluencerPerIlSalto || forma < CareerProgression.FitnessPerIlSalto)
            && !string.IsNullOrEmpty(prossimo))
        {
            chiave = DettoParametri;
            var mancante = influencer < CareerProgression.InfluencerPerIlSalto
                ? $"il livello influencer: sei a {influencer} e ne servono {CareerProgression.InfluencerPerIlSalto}"
                : $"la condizione fisica: sei a {forma} e ne servono {CareerProgression.FitnessPerIlSalto}";
            var rimedio = influencer < CareerProgression.InfluencerPerIlSalto
                ? "«Allora da domani ci muoviamo su quello. Facciamo dei video negli allenamenti, pubblichiamo le foto del weekend, "
                  + "e io comincio a girare fra gli sponsor con i tuoi risultati in mano. "
                  + "In un paio di mesi quel numero lo tiriamo su, te lo garantisco.»"
                : "«E allora tu in palestra, punto. Lo so che dopo un weekend l'ultima cosa che vuoi è allenarti, "
                  + "ma è letteralmente la cosa che ci separa da un campionato più in alto.»";

            return
            [
                new("Rei Kisaragi",
                    $"«Stai andando forte e ti chiederai perché non ti chiama nessuno. Te lo spiego, perché non è sfortuna. "
                    + $"Per farti salire a stagione in corso una squadra del {prossimo} deve rischiare su di te, "
                    + "e prima di rischiare guarda due numeri oltre ai risultati. "
                    + $"A te manca {mancante}. "
                    + "Non è una formalità: una squadra vuole un pilota che porti pubblico e che regga la stagione fisicamente, "
                    + "altrimenti prende quello che ce l'ha già.»",
                    ReiPreoccupata, "franca"),
                CastDirector.Battuta(CastDirector.Haru, rimedio, "")
            ];
        }

        // 3. La gavetta sul gradino: perché le squadre di sopra non ti guardano
        //    ancora, e quando cominceranno.
        var servono = OpportunityGenerator.RacesBeforeStepUp(gradino.Step);
        if (gareSulGradino == servono - 2)
        {
            chiave = DettoGavetta;
            return
            [
                new("Shigeo Kanda",
                    $"«Hai fatto {gareSulGradino} gare su questa vettura. Te lo dico perché è una cosa che nessuno spiega mai ai ragazzi: "
                    + $"le squadre della categoria superiore cominciano a guardarti seriamente intorno alle {servono} gare, non prima. "
                    + "Non è che siano cattivi o che non vedano il tuo talento. È che una categoria si impara con i chilometri, "
                    + "e uno che ne ha fatti pochi lo sanno che in una macchina nuova va in difficoltà.»",
                    Shigeo, "paziente"),
                new("Rei Kisaragi",
                    "«Quindi le prossime due gare non buttiamole via. Non serve nemmeno vincerle: serve arrivare in fondo "
                    + "facendo vedere che sai dove stai andando, che gestisci le gomme e che non fai errori sotto pressione. "
                    + "È quello che guardano.»",
                    ReiSerena, "pratica")
            ];
        }

        return [];
    }

    /// <summary>
    /// L'avvertimento quando la classifica scivola verso la zona di
    /// retrocessione. Meglio saperlo adesso che all'ultima gara.
    /// </summary>
    public static IReadOnlyList<AnimeDialogueLine> Pericolo(CareerState career, int posizione, int inClassifica)
    {
        var sogliaRossa = (int)Math.Ceiling(Math.Max(4, inClassifica) * CareerProgression.FrazioneRetrocessione);
        return
        [
            new("Rei Kisaragi",
                $"«Dobbiamo parlare della classifica, perché la situazione non mi piace. Sei P{posizione} su {inClassifica}, "
                + $"e da P{sogliaRossa + 1} in giù si retrocede. Non è una minaccia che ti faccio io: è la regola del campionato. "
                + "Se finiamo lì perdi il sedile, scendiamo di livello e per rientrare devi rifare una prova come il primo giorno. "
                + "Abbiamo ancora tempo, ma dobbiamo cambiare qualcosa adesso.»",
                ReiSevera, "grave"),
            new("Shigeo Kanda",
                "«Il problema non è la velocità, è che stai inseguendo il giro veloce in gare dove bisognava solo portare a casa i punti. "
                + "Nelle prossime, quando la macchina non va, accontentati: un settimo posto vale più di un ritiro mentre provavi il sorpasso impossibile. "
                + "Le classifiche le fanno le domeniche brutte, non quelle belle.»",
                Shigeo, "diretto")
        ];
    }
}
