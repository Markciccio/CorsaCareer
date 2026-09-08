namespace CorsaCareer;

/// <summary>
/// Una fase della carriera: il momento in cui cambia la posta in gioco.
///
/// Non e un livello e non e una categoria: e il punto in cui il pilota diventa
/// un'altra cosa. Passare da "nessuno ti conosce" a "un team ha scommesso su di
/// te" cambia il significato di ogni gara successiva, e questo va annunciato.
/// </summary>
public sealed class CareerPhase
{
    /// <summary>Identificativo stabile: la fase annunciata viene salvata con questo.</summary>
    public string Id { get; init; } = "";
    /// <summary>Fascia d'apertura, in stile ultim'ora.</summary>
    public string Flash { get; init; } = "";
    /// <summary>Il nome della fase, quello grande.</summary>
    public string Title { get; init; } = "";
    /// <summary>Una riga che dice cosa e cambiato.</summary>
    public string Standfirst { get; init; } = "";
    /// <summary>Il racconto della fase: cosa comincia, e perche conta.</summary>
    public List<string> Paragraphs { get; init; } = [];
    /// <summary>Cosa serve fare adesso, in concreto.</summary>
    public string Objective { get; init; } = "";
    /// <summary>Cosa si rischia. Una fase senza rischio non e una fase.</summary>
    public string Stake { get; init; } = "";
    /// <summary>Tavola illustrata inclusa nel pacchetto per l'apertura del capitolo.</summary>
    public string ArtworkAsset { get; init; } = "";
    /// <summary>Didascalia: l'illustrazione crea atmosfera, non documenta un risultato.</summary>
    public string ArtworkCaption { get; init; } = "";
    /// <summary>Ordine: le fasi non tornano indietro.</summary>
    public int Rank { get; init; }
}

/// <summary>
/// Riconosce la fase della carriera dallo stato salvato e ne compone la
/// presentazione.
///
/// Serve a un problema reale: la carriera cambiava natura — primo contratto,
/// promozione, prima vittoria, primo stipendio — e sullo schermo cambiavano solo
/// dei numeri. Il giocatore non percepiva di essere entrato in una fase nuova.
///
/// Le fasi non tornano indietro: una volta annunciata la vittoria, retrocedere
/// non ripropone l'annuncio della fase precedente. Il rango salvato lo impedisce.
/// </summary>
public static class CareerPhases
{
    public const string Debut = "esordio";
    public const string Contract = "primo-contratto";
    public const string Climb = "categoria-superiore";
    public const string Winner = "prima-vittoria";
    public const string Professional = "professionista";
    public const string Reference = "punto-di-riferimento";

    /// <summary>Ordine delle fasi. Una fase con rango inferiore non viene mai riannunciata.</summary>
    public static int RankOf(string? id) => id switch
    {
        Debut => 1,
        Contract => 2,
        Climb => 3,
        Winner => 4,
        Professional => 5,
        Reference => 6,
        _ => 0
    };

    /// <summary>
    /// La fase in cui si trova la carriera adesso. Guarda solo lo stato salvato.
    /// </summary>
    public static CareerPhase Current(CareerState career, IReadOnlyList<ContentCarRecord>? cars = null)
    {
        var profile = career.ReputationProfile ?? new ReputationProfile();
        var rung = CareerLadder.Current(career, cars ?? []);
        // Uno stipendio senza contratto attivo non significa niente: va letto
        // insieme al sedile, altrimenti un valore residuo nello stato fa
        // sembrare professionista un pilota che non ha ancora corso.
        var sottoContratto = career.ContractActive && !IsUncontracted(career.Team);
        var professionista = sottoContratto && (career.ContractSalary > 0 || profile.TeamTrust >= 85);
        var riferimento = career.Wins >= 5 && profile.SportingPrestige >= 75 && career.ContractActive;

        if (riferimento) return Build(Reference, career, rung, cars ?? []);
        if (professionista) return Build(Professional, career, rung, cars ?? []);
        if (career.Wins > 0) return Build(Winner, career, rung, cars ?? []);
        if (sottoContratto)
            // Salito di categoria almeno una volta: la prima stagione con un
            // contratto e "primo contratto", le successive sono la scalata.
            return Build((career.SeasonArchive?.Count ?? 0) > 0 ? Climb : Contract, career, rung, cars ?? []);
        return Build(Debut, career, rung, cars ?? []);
    }

    public static bool IsUncontracted(string? team) =>
        string.IsNullOrWhiteSpace(team) || team.StartsWith("Senza contratto", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Vero se la fase attuale non e ancora stata annunciata. Confronta i ranghi,
    /// non le stringhe: una carriera che retrocede non riceve l'annuncio della
    /// fase che aveva gia superato.
    /// </summary>
    public static bool NeedsAnnouncement(CareerState career, out CareerPhase phase,
        IReadOnlyList<ContentCarRecord>? cars = null)
    {
        phase = Current(career, cars);
        return phase.Rank > RankOf(career.AnnouncedPhase);
    }

    // -------------------------------------------------------- le presentazioni

    /// <summary>
    /// Il titolo del primo capitolo dipende da dove si comincia. Con solo GT
    /// installate non si parte dal fondo di niente: si parte gia in mezzo.
    /// </summary>
    private static string DebutTitle(LadderRung rung) => rung.Step switch
    {
        1 => "Nessuno ti aspetta",
        2 or 3 => "Si comincia dal kart",
        _ => "Si comincia da qui"
    };

    /// <summary>
    /// L'attacco del racconto, adattato al gradino reale. Un circuito di paese
    /// con i kart e un paddock di GT non sono lo stesso posto, e descriverli
    /// allo stesso modo renderebbe falsa la meta delle carriere.
    /// </summary>
    private static string DebutOpening(string driver, LadderRung rung) => rung.Path == LadderPath.Karting
        ? $"Cominciamo da dove comincia tutto, che non è mai un gran premio. È un circuito di provincia, un box che " +
          $"è una tettoia, e {driver} che si allaccia il casco senza che nessuno lo stia guardando. {rung.Description}"
        : $"Non tutte le carriere cominciano dal kart: si comincia con quello che c'è, e qui il gradino più basso " +
          $"disponibile è {rung.Name.ToLowerInvariant()}. {rung.Description} {driver} parte da lì, il che vuol dire " +
          "saltare la gavetta più lunga e trovarsi subito davanti gente che quella gavetta l'ha fatta.";

    private static CareerPhase Build(string id, CareerState career, LadderRung rung, IReadOnlyList<ContentCarRecord> cars)
    {
        var driver = string.IsNullOrWhiteSpace(career.Driver) ? "il pilota" : career.Driver;
        var team = IsUncontracted(career.Team) ? "" : career.Team;
        var profile = career.ReputationProfile ?? new ReputationProfile();

        return id switch
        {
            Debut => new CareerPhase
            {
                Id = Debut, Rank = 1,
                ArtworkAsset = "manga-01-rookie-dawn.png",
                ArtworkCaption = "Tavola I · Prima dell'alba — illustrazione narrativa originale, non fotografia di un evento reale",
                Flash = "SI COMINCIA DA QUI",
                Title = DebutTitle(rung),
                Standfirst = "Nessun contratto. Nessun aiuto vero. Solo un motore piccolo, pochi soldi e la voglia di correre più forte di tutti.",
                Paragraphs =
                [
                    "Non si comincia da una squadra importante. Non si comincia da un paddock perfetto. Non si comincia con gente che fa il tifo per te.",
                    $"Si comincia presto, quando fa ancora freddo, in un circuito di provincia dove l'asfalto è bagnato e i box sembrano tettoie tirate su in fretta. Si comincia con {rung.Name.ToLowerInvariant()}, mezzi modesti, pezzi usati, gomme non proprio nuove e conti fatti al centesimo.",
                    $"{driver} non è uno che il mondo del motorsport stava aspettando. Non ha un manager. Non ha sponsor. Non ha soldi da buttare. Ha solo abbastanza per provarci: € {career.Cash:N0} e una prima sessione da pagarsi da solo.",
                    "Ogni iscrizione pesa. Ogni trasferta costa. Ogni errore si paga due volte: in tempo e in denaro.",
                    "Intorno a lui ci sono ragazzi come lui, famiglie che fanno sacrifici, piccoli team che tirano avanti, sogni troppo grandi per i mezzi che hanno. Tutti vogliono emergere. Quasi nessuno ce la farà.",
                    "Ed è proprio per questo che ogni gara conta. Non perché sia già importante per il mondo, ma perché potrebbe diventarlo per te.",
                    "Un buon risultato può farti guadagnare fiducia. Può attirare uno sponsor. Può convincere qualcuno a darti un'altra occasione. Un risultato brutto, invece, può lasciarti fermo prima ancora di aver cominciato davvero.",
                    "Questa carriera parte da qui: da chi non ha quasi niente e deve costruirsi tutto. Nome. Credibilità. Occasioni. Soldi per andare avanti.",
                    "Nel portale, ogni sessione viene preparata con i parametri giusti. Tu scendi in pista, fai la tua gara, e al ritorno il sistema legge il risultato e lo trasforma in conseguenze reali: premi, spese, reputazione, articoli, offerte, delusioni, nuove possibilità."
                ],
                Objective = "Restare in corsa. Fare vedere che vali senza finire i soldi subito. Ottenere abbastanza risultati da convincere qualcuno a puntare su di te.",
                Stake = "Se sprechi le prime occasioni, il capitale finisce in fretta. E quando i soldi finiscono, il sogno non si interrompe con un grande finale: semplicemente ti ritrovi fuori, mentre gli altri continuano."
            },

            Contract => new CareerPhase
            {
                Id = Contract, Rank = 2,
                ArtworkAsset = "manga-06-first-contract.png",
                ArtworkCaption = "Tavola VI · La prima firma — illustrazione narrativa originale",
                Flash = "QUALCUNO CI CREDE",
                Title = "Qualcuno ha scommesso su di te",
                Standfirst = (team.Length > 0
                    ? $"{team} mette {driver} su una vettura per una stagione intera."
                    : $"{driver} ha un sedile per una stagione intera.")
                    + $" {rung.Name}: {CareerLadder.DistanceToTop(rung, cars)}",
                Paragraphs =
                [
                    $"Da adesso le gare non le scegli più una alla volta: c'è un calendario, e c'è un campionato con una " +
                    $"classifica in cui il tuo nome comparirà settimana dopo settimana. {(team.Length > 0 ? team + " ha" : "Il team ha")} " +
                    "messo qualcosa di proprio in questa scommessa, e questo cambia il tono di ogni conversazione nel box.",

                    "Cambia anche cosa viene giudicato. Prima contava solo arrivare; ora conta arrivare rispetto alle " +
                    "attese del sedile che occupi e rispetto al tuo compagno di squadra. Un buon piazzamento su una " +
                    "vettura scarsa vale più di un piazzamento anonimo su una vettura competitiva, e la stampa lo nota.",

                    "La fiducia si costruisce e si perde in fretta. Ritirarsi da una sessione, non presentarsi, chiudere " +
                    "un weekend senza un risultato: sono cose che il team registra, e che pesano al momento di " +
                    "rinnovare o di lasciarti andare."
                ],
                Objective = string.IsNullOrWhiteSpace(career.ContractObjective)
                    ? "Onorare il contratto e battere le attese del sedile."
                    : career.ContractObjective,
                Stake = "Deludere chi ti ha dato la prima possibilità chiude più porte di quante ne apra un buon risultato."
            },

            Climb => new CareerPhase
            {
                Id = Climb, Rank = 3,
                ArtworkAsset = "manga-11-promotion.png",
                ArtworkCaption = "Tavola XI · Si ricomincia più in alto — illustrazione narrativa originale",
                Flash = "SI SALE",
                Title = $"Si alza l'asticella: {career.Tier}",
                Standfirst = $"{rung.Name}. {CareerLadder.DistanceToTop(rung, cars)} Qui gli avversari non sbagliano.",
                Paragraphs =
                [
                    "La differenza non è la velocità: è il margine. Nella categoria da cui vieni un errore lo recuperavi " +
                    "in tre giri; qui un errore è il weekend. Gli avversari hanno tutti fatto il tuo percorso, e nessuno " +
                    "regala una posizione.",

                    "Cambiano anche i costi. Iscrizioni, trasferte, riparazioni: tutto pesa di più, e una stagione non " +
                    "finanziata in questa categoria può svuotare la cassa costruita in due anni. Le coperture dei team e " +
                    "gli sponsor non sono un dettaglio contabile: sono la condizione per esserci.",

                    "In compenso qui si viene visti. Un risultato in questa categoria arriva a orecchie che nella " +
                    "precedente non ti ascoltavano, e le promesse condizionate — una stagione coperta, un test pagato — " +
                    "cominciano a valere cifre serie."
                ],
                Objective = "Restare nel gruppo di testa senza compromettere la sostenibilità della stagione.",
                Stake = "Una stagione anonima in categoria superiore costa più di una stagione vinta in quella inferiore."
            },

            Winner => new CareerPhase
            {
                Id = Winner, Rank = 4,
                ArtworkAsset = "manga-10-first-victory.png",
                ArtworkCaption = "Tavola X · La prima vittoria — illustrazione narrativa originale",
                Flash = "LA PRIMA VOLTA",
                Title = "Da promessa a vincente",
                Standfirst = $"{driver} ha vinto in {rung.Name.ToLowerInvariant()}. {CareerLadder.DistanceToTop(rung, cars)}",
                Paragraphs =
                [
                    "Una vittoria cambia il modo in cui vieni letto. Prima ogni buon risultato era una sorpresa da " +
                    "confermare; adesso ogni risultato mediocre è una domanda. È il prezzo di essere entrato nell'elenco " +
                    "di quelli che possono farcela.",

                    "Il mercato si muove. I team che ti ignoravano chiedono informazioni, quelli che ti hanno fatto " +
                    "crescere cominciano a temere di perderti, e prima o poi arriverà l'offerta che ti costringe a " +
                    "scegliere fra chi ha creduto in te per primo e chi può portarti più in alto. Non è una scelta " +
                    "gratuita in nessuna delle due direzioni.",

                    "Gli sponsor smettono di valutare il potenziale e cominciano a valutare la visibilità. È il momento " +
                    "in cui il denaro può passare dall'essere un vincolo a essere uno strumento."
                ],
                Objective = "Confermare: una vittoria è un episodio, due sono una tendenza.",
                Stake = "Restare il pilota di una vittoria sola è una definizione da cui si esce difficilmente."
            },

            Professional => new CareerPhase
            {
                Id = Professional, Rank = 5,
                ArtworkAsset = "manga-12-professional.png",
                ArtworkCaption = "Tavola XII · Ora è un mestiere — illustrazione narrativa originale",
                Flash = "ORA È UN MESTIERE",
                Title = "Non paghi più per correre",
                Standfirst = career.ContractSalary > 0
                    ? $"{driver} firma per € {career.ContractSalary:N0}: da adesso è il team a pagare."
                    : $"{driver} è un pilota che i team vogliono, non uno che deve comprarsi il posto.",
                Paragraphs =
                [
                    "È il rovesciamento completo del punto di partenza. Nelle prime gare pagavi di tasca tua per esserci; " +
                    "adesso guidare è il tuo lavoro, e chi ti paga si aspetta un rendimento. La libertà di sbagliare che " +
                    "avevi nelle categorie minori non esiste più.",

                    $"La fiducia dei team è a {profile.TeamTrust}/100 e questo apre porte che prima non si vedevano: " +
                    "sostituzioni all'ultimo momento in categorie superiori, test su vetture che non avresti potuto " +
                    "affittare, contratti con clausole al posto di quote d'iscrizione.",

                    "Da qui la carriera non sale più per gradini: sale per occasioni. Riconoscere quella giusta, e avere " +
                    "il capitale e la reputazione per prenderla quando passa, è tutto il gioco che resta."
                ],
                Objective = "Trasformare lo stipendio in posizione: vincere dove si viene guardati.",
                Stake = "Un professionista che non rende viene sostituito, e il posto non torna libero."
            },

            _ => new CareerPhase
            {
                Id = Reference, Rank = 6,
                ArtworkAsset = "manga-13-champion.png",
                ArtworkCaption = "Tavola XIII · Il nome da battere — illustrazione narrativa originale",
                Flash = "IL NOME DA BATTERE",
                Title = "Sei tu quello da battere",
                Standfirst = $"{career.Wins} vittorie e un prestigio di {profile.SportingPrestige}/100: {driver} è il riferimento della categoria.",
                Paragraphs =
                [
                    "Non corri più per farti notare: corri per difendere una posizione. Ogni avversario che ti supera " +
                    "costruisce la propria carriera su quel sorpasso, e ogni tua giornata mediocre diventa una notizia " +
                    "più grande di quanto sarebbe stata tre anni fa.",

                    "Le decisioni pesanti non sono più sportive ma di struttura: quale progetto sposare, quanto capitale " +
                    "immobilizzare, se restare il pilota di punta di qualcuno o cominciare a costruire qualcosa di tuo.",

                    "Il tempo, adesso, lavora contro. Ogni stagione spesa in una categoria che non porta più niente è " +
                    "una stagione che non torna."
                ],
                Objective = "Scegliere dove finisce questa carriera, invece di lasciarlo decidere ai risultati.",
                Stake = "Restare troppo a lungo dove hai già vinto tutto è il modo più comune di finire."
            }
        };
    }
}
