namespace CorsaCareer;

/// <summary>Che tipo di persona hai davanti al tavolo.</summary>
public enum SponsorTemperamento
{
    /// <summary>Uno che le corse le ha viste: vuole numeri e competenza.</summary>
    Intenditore,
    /// <summary>Uno che compra visibilità: vuole sapere quanta gente ti guarda.</summary>
    Commerciante,
    /// <summary>Uno del quartiere: decide di pancia, guarda la persona.</summary>
    DiPaese,
    /// <summary>Uno abituato a trattare: rispetta chi non si fa mettere sotto.</summary>
    Duro
}

/// <summary>Una risposta possibile in una trattativa.</summary>
public sealed record SponsorMossa(
    string Etichetta,
    string Battuta,
    SponsorTemperamento Piace,
    SponsorTemperamento Stona,
    int Peso);

/// <summary>Una domanda dello sponsor con le risposte fra cui scegliere.</summary>
public sealed record SponsorScambio(string Domanda, IReadOnlyList<SponsorMossa> Mosse);

/// <summary>Come è andato un singolo passaggio della trattativa.</summary>
public sealed record SponsorEsitoMossa(string Battuta, string Reazione, int Delta);

/// <summary>
/// La trattativa, come conversazione a bivi.
///
/// Finora una sponsorizzazione era una probabilità e un tiro: si sceglieva chi
/// andare a trovare e il resto lo decideva il programma. La probabilità
/// dipendeva da cose vere — i risultati, il seguito, le doti di Haru — ma il
/// giocatore non aveva niente da fare mentre veniva calcolata.
///
/// Qui la trattativa è tre domande e tre risposte possibili ciascuna. Non c'è
/// la risposta giusta in assoluto: c'è quella giusta per la persona che hai
/// davanti. Al ricambista che le corse le conosce non interessa quanta gente ti
/// segue sui social; al distributore di bevande non interessa il tuo passo in
/// curva tre. Ed è dichiarato prima chi hai davanti, perché la scelta sia una
/// scelta e non un indovinello.
///
/// Il peso complessivo delle scelte è volutamente limitato: sposta la
/// probabilità, non la sostituisce. Chi non ha risultati non convince un
/// assicuratore nemmeno dicendo tutto giusto, ed è come deve essere.
/// </summary>
public static class SponsorNegotiation
{
    /// <summary>Quanto al massimo l'intera trattativa può spostare la probabilità.</summary>
    public const int InfluenzaMassima = 34;

    /// <summary>Che tipo è questo interlocutore, dal mestiere che fa.</summary>
    public static SponsorTemperamento TemperamentoDi(string mestiere)
    {
        var m = (mestiere ?? "").ToLowerInvariant();
        if (m.Contains("pneumatici") || m.Contains("assetti") || m.Contains("elettronica")) return SponsorTemperamento.Intenditore;
        if (m.Contains("bevande") || m.Contains("distribuzione")) return SponsorTemperamento.Commerciante;
        if (m.Contains("assicur") || m.Contains("trasporti")) return SponsorTemperamento.Duro;
        return SponsorTemperamento.DiPaese;
    }

    public static string NomeTemperamento(SponsorTemperamento t) => t switch
    {
        SponsorTemperamento.Intenditore => "un intenditore",
        SponsorTemperamento.Commerciante => "un commerciante",
        SponsorTemperamento.Duro => "uno abituato a trattare",
        _ => "gente del quartiere"
    };

    /// <summary>Come si presenta, per farsi riconoscere prima di parlare.</summary>
    public static string ComeSiPresenta(SponsorTemperamento t) => t switch
    {
        SponsorTemperamento.Intenditore =>
            "Ha una foto di un circuito appesa dietro la scrivania e un cronometro sul bancone. "
            + "Le corse le conosce: con lui i numeri contano più dell'entusiasmo.",
        SponsorTemperamento.Commerciante =>
            "Ti guarda come guarderebbe uno spazio pubblicitario. Non gli interessa come guidi: "
            + "gli interessa quanta gente ti vede guidare.",
        SponsorTemperamento.Duro =>
            "Ha trattato per tutta la vita e lo si capisce da come sta seduto. "
            + "Se ti fai mettere sotto al primo colpo, il prezzo lo fa lui.",
        _ =>
            "È del quartiere e ti ha visto crescere. Deciderà con la pancia più che con i conti, "
            + "e quello che gli interessa è che tu sia una persona seria."
    };

    /// <summary>
    /// Le tre domande della trattativa. Sono sempre le stesse tre domande —
    /// perché sei qui, cosa ci guadagno, quanto vuoi — che sono poi le tre
    /// domande di qualunque trattativa vera; a cambiare è quale risposta
    /// funziona con chi hai davanti.
    /// </summary>
    public static IReadOnlyList<SponsorScambio> Scambi(SponsorVisit visita, CareerState carriera)
    {
        var pilota = string.IsNullOrWhiteSpace(carriera.Driver) ? "il ragazzo" : carriera.Driver;
        var profilo = carriera.ReputationProfile ?? new ReputationProfile();
        var gare = carriera.Races;
        var vittorie = carriera.Wins;
        var podi = carriera.Podiums;
        var seguito = profilo.PublicPopularity;

        var risultati = vittorie > 0
            ? $"{vittorie} {(vittorie == 1 ? "vittoria" : "vittorie")} e {podi} podi in {gare} gare"
            : podi > 0
                ? $"{podi} podi in {gare} gare"
                : gare > 0
                    ? $"{gare} gare corse, nessun podio ancora"
                    : "nessuna gara ancora corsa";

        return
        [
            new SponsorScambio(
                "«Allora, ditemi. Perché dovrei mettere dei soldi su un ragazzo che corre?»",
                [
                    new SponsorMossa("I numeri",
                        $"«Perché i numeri ci sono: {risultati}. Non le sto vendendo un sogno, le sto portando dei risultati.»",
                        Piace: SponsorTemperamento.Intenditore, Stona: SponsorTemperamento.Commerciante, Peso: 7),
                    new SponsorMossa("La gente",
                        $"«Perché {pilota} lo segue della gente. Livello di seguito {seguito} su 100, e cresce a ogni gara. "
                        + "Il suo nome sul cofano lo vedono.»",
                        Piace: SponsorTemperamento.Commerciante, Stona: SponsorTemperamento.Intenditore, Peso: 7),
                    new SponsorMossa("La verità",
                        "«Perché senza qualcuno che ci creda adesso, non ci arriva. Non ho una presentazione: "
                        + "ho un ragazzo che si alza alle cinque e un kart tenuto insieme da noi.»",
                        Piace: SponsorTemperamento.DiPaese, Stona: SponsorTemperamento.Duro, Peso: 8)
                ]),

            new SponsorScambio(
                "«E io che ci guadagno? Sia chiaro, non faccio beneficenza.»",
                [
                    new SponsorMossa("Il ritorno concreto",
                        $"«L'adesivo su cofano e casco per tutta la stagione, il nome in ogni comunicato, "
                        + "e a ogni gara la sua insegna la vedono le persone che a lei servono.»",
                        Piace: SponsorTemperamento.Commerciante, Stona: SponsorTemperamento.DiPaese, Peso: 6),
                    new SponsorMossa("La competenza",
                        "«Ci guadagna che quando le chiederanno di corse, lei ne saprà più di chiunque altro in zona. "
                        + "E che il nostro tecnico le passerà i dati di ogni weekend, se le interessano.»",
                        Piace: SponsorTemperamento.Intenditore, Stona: SponsorTemperamento.Commerciante, Peso: 6),
                    new SponsorMossa("Il rispetto",
                        "«Onestamente? Poco, quest'anno. Fra tre anni però le converrà dire che c'era dall'inizio, "
                        + "e allora costerà dieci volte tanto.»",
                        Piace: SponsorTemperamento.Duro, Stona: SponsorTemperamento.Intenditore, Peso: 7)
                ]),

            new SponsorScambio(
                "«Veniamo al punto. Quanto vi serve?»",
                [
                    new SponsorMossa("La cifra intera",
                        $"«€ {visita.Amount:N0}. È quello che serve, e non ho gonfiato niente per potermelo far tagliare.»",
                        Piace: SponsorTemperamento.Duro, Stona: SponsorTemperamento.DiPaese, Peso: 8),
                    new SponsorMossa("Quello che può",
                        "«Quello che le sembra giusto. Anche poco: quello che entra oggi è la differenza fra correre "
                        + "la prossima e restare a casa.»",
                        Piace: SponsorTemperamento.DiPaese, Stona: SponsorTemperamento.Duro, Peso: 8),
                    new SponsorMossa("A risultato",
                        "«Metà adesso e metà solo se finiamo la stagione nei primi cinque. Se non ci arriviamo, "
                        + "la seconda metà non me la deve.»",
                        Piace: SponsorTemperamento.Intenditore, Stona: SponsorTemperamento.Commerciante, Peso: 9)
                ])
        ];
    }

    /// <summary>
    /// L'effetto di una risposta. Le doti di Haru contano quanto la scelta:
    /// la stessa frase detta da chi sa parlare e da chi balbetta non ottiene
    /// la stessa cosa, ed è il motivo per cui mandarlo in giro lo fa crescere.
    /// </summary>
    public static SponsorEsitoMossa Valuta(SponsorMossa mossa, SponsorTemperamento chi, AgentSkills doti)
    {
        var delta = 0;
        string reazione;

        if (mossa.Piace == chi)
        {
            delta = mossa.Peso + doti.Credibility / 25;
            reazione = chi switch
            {
                SponsorTemperamento.Intenditore => "Annuisce lentamente e per la prima volta ti guarda in faccia. «Continui.»",
                SponsorTemperamento.Commerciante => "Prende una penna e scrive qualcosa. «Questo mi interessa.»",
                SponsorTemperamento.Duro => "Un mezzo sorriso. «Almeno lei non mi racconta storie.»",
                _ => "Si appoggia allo schienale e sospira. «Eh. Vi ho visti crescere, voi due.»"
            };
        }
        else if (mossa.Stona == chi)
        {
            delta = -(mossa.Peso - doti.Negotiation / 30);
            reazione = chi switch
            {
                SponsorTemperamento.Intenditore => "Aggrotta la fronte. «Sì, ma io le ho chiesto un'altra cosa.»",
                SponsorTemperamento.Commerciante => "Guarda l'orologio. «Sarà, ma a me questo non serve.»",
                SponsorTemperamento.Duro => "Scuote la testa. «Con me la faccia da poveretto non funziona.»",
                _ => "Si irrigidisce un po'. «Mi sembra una cosa da città, questa.»"
            };
        }
        else
        {
            delta = 1 + doti.MotorsportKnowledge / 50;
            reazione = "Ascolta senza sbilanciarsi. «Va bene. Altro?»";
        }

        return new SponsorEsitoMossa(mossa.Battuta, reazione, delta);
    }

    /// <summary>
    /// La probabilità finale: quella di partenza, più com'è andato il viaggio,
    /// più com'è andata la conversazione. Resta dentro i limiti di sempre, così
    /// nessuna trattativa è impossibile e nessuna è già vinta.
    /// </summary>
    public static int ProbabilitaFinale(int probabilitaDiPartenza, int modificatoreViaggio, int sommaScelte) =>
        Math.Clamp(probabilitaDiPartenza + modificatoreViaggio + Math.Clamp(sommaScelte, -InfluenzaMassima, InfluenzaMassima), 3, 96);
}
