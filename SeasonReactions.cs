namespace CorsaCareer1991;

/// <summary>
/// Che cosa dicono gli amici quando una stagione finisce.
///
/// Il verdetto di fine campionato arrivava come una riga di notizia, e nessuno
/// dei personaggi che accompagnano la carriera apriva bocca — mentre una
/// promozione o una retrocessione sono esattamente i momenti in cui le persone
/// intorno a te parlano.
///
/// Sul modo di scriverle vale la stessa regola dei briefing: sono conversazioni
/// fra persone che si conoscono, con frasi intere e cose concrete dette in
/// chiaro. Non battute spezzate a effetto: quelle sembrano scritte bene e non
/// dicono niente.
/// </summary>
public static class SeasonReactions
{
    // Tutti i ritratti passano dal regista: verifica che il file esista e
    // ripiega se manca, invece di lasciare il personaggio senza faccia.
    private static string ReiSerena => CastDirector.Ritratto(CastDirector.Rei, "serena");
    private static string ReiSevera => CastDirector.Ritratto(CastDirector.Rei, "severa");
    private static string ReiPreoccupata => CastDirector.Ritratto(CastDirector.Rei, "preoccupata");
    private static string ReiCauta => CastDirector.Ritratto(CastDirector.Rei, "cauta");
    private static string Shigeo => CastDirector.Ritratto(CastDirector.Shigeo, "neutro");
    private static string ShigeoSvolta => CastDirector.Ritratto(CastDirector.Shigeo, "svolta");
    private static string HaruFelice => CastDirector.Ritratto(CastDirector.Haru, "felice");
    private static string HaruInPena => CastDirector.Ritratto(CastDirector.Haru, "preoccupato");
    private static string HaruCarte => CastDirector.Ritratto(CastDirector.Haru, "documenti");
    private static string Haru => CastDirector.Ritratto(CastDirector.Haru, "");
    private static string Noa => CastDirector.Ritratto(CastDirector.Noa, "neutro");
    private static string RikuBeffardo => CastDirector.Ritratto(CastDirector.Riku, "beffardo");
    private static string RikuSpavaldo => CastDirector.Ritratto(CastDirector.Riku, "spavaldo");

    public static IReadOnlyList<AnimeDialogueLine> Build(CareerState career, SeasonVerdict verdetto,
        int posizione, int livelloPrima, int livelloDopo, bool titolo)
    {
        var pilota = string.IsNullOrWhiteSpace(career.Driver) ? "il pilota" : career.Driver;
        var campionatoPrima = ChampionshipLadder.Name(livelloPrima);
        var campionatoDopo = ChampionshipLadder.Name(livelloDopo);

        return verdetto switch
        {
            SeasonVerdict.Promosso => Promozione(pilota, posizione, campionatoDopo, livelloDopo, titolo),
            SeasonVerdict.Retrocesso => Retrocessione(pilota, posizione, campionatoPrima, campionatoDopo),
            _ => Conferma(pilota, posizione, campionatoPrima, livelloPrima)
        };
    }

    private static List<AnimeDialogueLine> Promozione(string pilota, int posizione, string campionato, int livello, bool titolo)
    {
        var scene = new List<AnimeDialogueLine>
        {
            new("Haru Senda",
                titolo
                    ? $"«Campione. Sei campione, {pilota}, l'abbiamo fatto davvero! "
                      + "Ho passato l'ultima gara a guardare il cronometro con le mani sugli occhi e non ho visto niente, "
                      + "però l'ho sentito dall'altoparlante e ho urlato come un matto. Scusami se non sono lucido, ma è una giornata che mi ricorderò.»"
                    : $"«P{posizione}! Siamo nei primi tre, sai cosa vuol dire? Vuol dire che l'anno prossimo si corre nel {campionato}. "
                      + "Ci abbiamo messo una stagione intera ma ce l'abbiamo fatta, e adesso finalmente possiamo festeggiare.»",
                HaruFelice, "esultante"),

            new("Rei Kisaragi",
                titolo
                    ? "«Complimenti, davvero, e non sono una che li fa facilmente. Te lo sei guadagnato in pista e fuori. "
                      + "Adesso però ti dico la cosa scomoda, perché è il mio mestiere: difendere un titolo è più difficile che vincerlo. "
                      + "L'anno prossimo tutti sapranno chi sei e ti studieranno, e quello che ti ha funzionato quest'anno funzionerà molto meno.»"
                    : $"«Bene, sono contenta. Adesso guardami un secondo, perché voglio che tu arrivi preparato. "
                      + $"Nel {campionato} le persone che hai battuto quest'anno non ci saranno: ci sarà gente che corre a quel livello da anni "
                      + "e per cui una gara storta è un'eccezione, non la normalità. "
                      + "Ci arriviamo pieni di fiducia, ma senza pensare di essere già arrivati.»",
                ReiSerena, "seria"),

            new("Shigeo Kanda",
                "«Io ho riguardato i dati di tutta la stagione, non solo delle gare buone, ed è lì che si vede il miglioramento vero: "
                + "hai smesso di perdere tempo in frenata a metà gara, che è la cosa su cui abbiamo lavorato di più. "
                + "Preparati però a una cosa: la macchina nuova sarà completamente diversa e per un paio di weekend non ci capirai niente. "
                + "È normale, succede a tutti, non farti prendere dal panico.»",
                ShigeoSvolta, "soddisfatto")
        };

        if (ChampionshipLadder.IsTop(livello))
            scene.Add(new AnimeDialogueLine("Noa Minazuki",
                $"«{pilota}, da domani cambia una cosa che forse non hai messo in conto: sei al vertice, e tutto quello che fai diventa una notizia. "
                + "Anche le cose che preferiresti restassero private. "
                + "Il consiglio che ti do da giornalista è di decidere tu cosa raccontare e quando, prima che lo decida qualcun altro al posto tuo.»",
                Noa, "professionale"));
        else
            scene.Add(new AnimeDialogueLine("Riku Hayase",
                $"«Complimenti, sinceri. Te lo dico una volta sola quindi ascoltala bene. "
                + $"Poi però nel {campionato} ci vediamo, e lì i primi tre non li fai: là davanti c'è gente che ti mangia. "
                + "Portati la stessa faccia che avevi oggi, mi raccomando.»",
                RikuSpavaldo, "provocatorio"));

        return scene;
    }

    private static List<AnimeDialogueLine> Retrocessione(string pilota, int posizione, string campionatoPrima, string campionatoDopo)
    {
        return
        [
            new("Haru Senda",
                $"«P{posizione}. Non so bene cosa dire, sinceramente. "
                + "Ho provato a trovarti una scusa per tutto il viaggio di ritorno e non me ne è venuta nessuna che regga. "
                + "Però ci sono, eh. Qualunque cosa decidiamo di fare adesso, io ci sono.»",
                HaruInPena, "abbattuto"),

            new("Rei Kisaragi",
                $"«Ti dico la verità perché nessun altro te la dirà, e preferisco che la senti da me. "
                + $"Nel {campionatoPrima}, quest'anno, non eri pronto: non è mancata la velocità in un paio di gare, "
                + "è mancata la costanza in tutte. "
                + $"Quindi si torna nel {campionatoDopo}, si rifà una prova per trovare un sedile e si ricomincia. "
                + "Non è una vergogna, sia chiaro: è successo a piloti che poi hanno vinto tutto. "
                + "La vergogna sarebbe usare questa stagione come scusa per smettere.»",
                ReiSevera, "severa"),

            new("Shigeo Kanda",
                "«La macchina non c'entra, e lo sai anche tu. Ti mancava mezzo secondo nei punti dove si guadagna davvero "
                + "e ti mancava lucidità negli ultimi dieci giri, quando le gomme calano e bisogna cambiare guida. "
                + "La prima cosa si allena e la sistemiamo. La seconda pure, ma ci vuole più tempo e più pazienza di quanta ne hai adesso.»",
                Shigeo, "diretto"),

            new("Riku Hayase",
                $"«{pilota} che scende di categoria. Ti confesso che non me lo sarei perso per niente al mondo.» "
                + "— poi si guarda intorno e abbassa la voce — "
                + "«Però torna su in fretta, per favore. Battere te quando sei messo così non mi diverte per niente.»",
                RikuBeffardo, "beffardo")
        ];
    }

    private static List<AnimeDialogueLine> Conferma(string pilota, int posizione, string campionato, int livello)
    {
        return
        [
            new("Haru Senda",
                $"«P{posizione}. Dai, non è andata male. Cioè, non è quello che volevamo a inizio anno, questo è vero, "
                + "però ci sono state gare in cui eri davvero forte e me le ricordo tutte.»",
                Haru, "incerto"),

            new("Rei Kisaragi",
                $"«Facciamo un altro anno nel {campionato}. Voglio essere onesta con te: non è un fallimento, "
                + "ma non è nemmeno un risultato — è una stagione che non ha deciso niente, e sono le peggiori. "
                + $"L'anno prossimo dobbiamo entrare nei primi {CareerProgression.PosizionePromozione}, "
                + "altrimenti cominciamo a farci domande diverse su dove sta andando questa carriera. "
                + "Adesso però riposati, che riposare fa parte del lavoro.»",
                ReiPreoccupata, "pensierosa"),

            new("Shigeo Kanda",
                $"«Il tuo problema quest'anno sono stati i weekend storti. Quando la macchina è a posto {pilota} è veloce quanto chiunque, "
                + "ma nelle domeniche in cui non funziona niente perdiamo troppi punti. "
                + "E le classifiche si fanno proprio lì, non nelle gare in cui sei il più forte in pista.»",
                Shigeo, "concreto")
        ];
    }

    /// <summary>
    /// La chiamata da un campionato superiore arrivata a stagione in corso: il
    /// momento in cui la carriera accelera senza aspettare la classifica.
    /// </summary>
    public static IReadOnlyList<AnimeDialogueLine> Chiamata(CareerState career, string campionato, string motivo)
    {
        var pilota = string.IsNullOrWhiteSpace(career.Driver) ? "il pilota" : career.Driver;
        return
        [
            new("Haru Senda",
                $"«Ha chiamato una squadra del {campionato}. Del {campionato}, {pilota}, hai capito bene! "
                + $"Mi hanno detto che è per {motivo}, e che non vogliono aspettare la fine del campionato: "
                + "ti vogliono adesso, e il posto lo pagano loro. Ho dovuto farmelo ripetere due volte al telefono.»",
                HaruCarte, "incredulo"),

            new("Noa Minazuki",
                "«Se ti stai chiedendo perché proprio tu, te lo spiego io che queste cose le seguo per lavoro. "
                + "Non è fortuna e non sono solo i risultati: è che il tuo nome adesso porta gente, e una squadra che ti prende "
                + "sa di comprarsi anche quello. "
                + "Il seguito che hai costruito in questi mesi è esattamente ciò che ti ha reso chiamabile. "
                + "Adesso però dovrai valere anche in pista, e subito.»",
                Noa, "professionale"),

            new("Rei Kisaragi",
                $"«Prima di dire di sì voglio che tu capisca cosa stai accettando. Vai a correre a stagione iniziata, "
                + "su una macchina che non conosci, contro gente che ci corre da anni e che ha già preso il ritmo. "
                + $"Le prime {CareerProgression.GareDiProvaDopoIlSalto} gare decidono tutto: se vanno bene il posto diventa tuo, "
                + "se vanno male ti riportano giù e ricominciamo da dove eravamo. "
                + "Detto questo: offerte così non si ripetono, e io al posto tuo direi di sì.»",
                ReiCauta, "cauta")
        ];
    }
}
