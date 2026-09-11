namespace CorsaCareer;

/// <summary>I momenti della carriera che hanno una scena scritta.</summary>
public enum MomentoDiCarriera
{
    /// <summary>Il primo giorno: il kart rimesso insieme e nessuno che ci crede.</summary>
    PrimoGiorno,
    /// <summary>Il primo giro cronometrato della vita.</summary>
    PrimoTest,
    /// <summary>La vigilia della prima gara.</summary>
    PrimaGara,
    /// <summary>La prima volta che si vince.</summary>
    PrimaVittoria,
    /// <summary>Il primo podio.</summary>
    PrimoPodio,
    /// <summary>Il primo ritiro, o la prima gara buttata via.</summary>
    PrimaBattuta,
    /// <summary>La firma del primo sedile.</summary>
    PrimaFirma,
    /// <summary>Si apre una stagione.</summary>
    AperturaStagione,
    /// <summary>Metà campionato, con la promozione ancora da giocarsi.</summary>
    MetaStagione,
    /// <summary>L'ultima gara, quella che decide.</summary>
    UltimaGara,
    /// <summary>Il titolo.</summary>
    TitoloVinto,
    /// <summary>Si cambia categoria: la scala si è mossa.</summary>
    CambioCategoria,
    /// <summary>Si scende di livello.</summary>
    Retrocessione,
    /// <summary>I soldi sono finiti.</summary>
    CassaVuota,
    /// <summary>Il primo sponsor della carriera.</summary>
    PrimoSponsor,
    /// <summary>Uno sponsor ha detto di no.</summary>
    SponsorRifiutato,
    /// <summary>Il rivale, quando conta.</summary>
    Rivalita,
    /// <summary>La scuola, il giorno dopo una gara.</summary>
    ScuolaDopoLaGara,
    /// <summary>La scuola quando si comincia a farsi notare.</summary>
    ScuolaSiParlaDiTe,
    /// <summary>Il vertice: la Formula 1, o quello che è il gradino più alto.</summary>
    ArrivoAlVertice
}

/// <summary>
/// Le scene scritte, quelle che raccontano i momenti che contano.
///
/// La regola di scrittura è quella che ha dato l'utente con il suo esempio:
/// niente frasi da telecronaca — «ha parlato il cronometro» — e niente
/// frammenti oscuri alla manga. Devono sembrare conversazioni fra persone che
/// si conoscono, con dentro i dati veri: il nome della squadra, quante gare
/// mancano, che pista è, quanti punti ci sono di distacco. E devono finire
/// dicendo che cosa succede adesso, perché è quello che uno vuole sapere.
///
/// Regola dura: <b>nessun numero inventato</b>. Ogni cifra citata qui esce
/// dallo stato della carriera, e ogni frase che dipende da un dato che potrebbe
/// non esserci è dentro un controllo. Se la classifica non c'è, non si parla di
/// classifica: si dice qualcos'altro. Una battuta in meno è sempre meglio di
/// una battuta che racconta una cosa mai successa.
///
/// Chi parla lo decide <see cref="CastDirector"/>: qui si dice solo di che
/// scena si tratta.
/// </summary>
public static class CapetaScenes
{
    /// <summary>
    /// I fatti che le scene possono citare. Sono solo quelli veri, letti dallo
    /// stato: raccoglierli in un posto solo evita che una scena vada a pescare
    /// un numero che non esiste.
    /// </summary>
    public sealed record Fatti(
        string Pilota, string Squadra, string Campionato, string Categoria, string Vettura,
        int Stagione, int Gare, int Vittorie, int Podi, int Punti, int Cassa,
        bool HaClassifica, int PostoInClassifica, int PuntiDelPrimo, int Iscritti,
        int GareRimaste, int Seguito, int Forma, string Sponsor, bool HaSponsor,
        int Gradino, int GradiniTotali, int Livello, int LivelliTotali, int Eta);

    public static Fatti Leggi(CareerState career, IReadOnlyList<ContentCarRecord> vetture,
        IReadOnlyList<ScheduledEvent>? agenda = null)
    {
        var gradino = CareerLadder.Current(career, vetture);
        var profilo = career.ReputationProfile ?? new ReputationProfile();

        var classifica = (career.Standings ?? [])
            .OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).ToList();
        var mio = classifica.FindIndex(x => x.Driver == career.Driver);
        var haClassifica = mio >= 0;

        var rimaste = 0;
        if (agenda != null)
            rimaste = agenda.Count(x => x.Season == career.Season && x.IsPlanned);

        var eta = career.BirthYear > 0 ? career.StoryDate.Year - career.BirthYear : 0;

        return new Fatti(
            Pilota: string.IsNullOrWhiteSpace(career.Driver) ? "il ragazzo" : career.Driver,
            Squadra: string.IsNullOrWhiteSpace(career.Team) ? "" : career.Team,
            Campionato: string.IsNullOrWhiteSpace(career.Championship) ? "" : career.Championship,
            Categoria: gradino.Name,
            Vettura: UiText.Car(career.Car ?? ""),
            Stagione: career.Season,
            Gare: career.Races,
            Vittorie: career.Wins,
            Podi: career.Podiums,
            Punti: career.Points,
            Cassa: career.Cash,
            HaClassifica: haClassifica,
            PostoInClassifica: haClassifica ? mio + 1 : 0,
            PuntiDelPrimo: classifica.Count > 0 ? classifica[0].Points : 0,
            Iscritti: classifica.Count,
            GareRimaste: rimaste,
            Seguito: profilo.PublicPopularity,
            Forma: career.Fitness,
            Sponsor: career.Sponsor ?? "",
            HaSponsor: !string.IsNullOrWhiteSpace(career.Sponsor) && !career.Sponsor.Contains("attesa", StringComparison.OrdinalIgnoreCase),
            Gradino: gradino.Step,
            GradiniTotali: CareerLadder.Steps,
            Livello: ChampionshipLadder.Clamp(career.ChampionshipLevel),
            LivelliTotali: ChampionshipLadder.Levels,
            Eta: eta);
    }

    /// <summary>La scena di un momento, già pronta da mostrare.</summary>
    public static IReadOnlyList<AnimeDialogueLine> Scena(MomentoDiCarriera momento, Fatti f) => momento switch
    {
        MomentoDiCarriera.PrimoGiorno => PrimoGiorno(f),
        MomentoDiCarriera.PrimoTest => PrimoTest(f),
        MomentoDiCarriera.PrimaGara => PrimaGara(f),
        MomentoDiCarriera.PrimaVittoria => PrimaVittoria(f),
        MomentoDiCarriera.PrimoPodio => PrimoPodio(f),
        MomentoDiCarriera.PrimaBattuta => PrimaBattuta(f),
        MomentoDiCarriera.PrimaFirma => PrimaFirma(f),
        MomentoDiCarriera.AperturaStagione => AperturaStagione(f),
        MomentoDiCarriera.MetaStagione => MetaStagione(f),
        MomentoDiCarriera.UltimaGara => UltimaGara(f),
        MomentoDiCarriera.TitoloVinto => TitoloVinto(f),
        MomentoDiCarriera.CambioCategoria => CambioCategoria(f),
        MomentoDiCarriera.Retrocessione => Retrocessione(f),
        MomentoDiCarriera.CassaVuota => CassaVuota(f),
        MomentoDiCarriera.PrimoSponsor => PrimoSponsor(f),
        MomentoDiCarriera.SponsorRifiutato => SponsorRifiutato(f),
        MomentoDiCarriera.Rivalita => Rivalita(f),
        MomentoDiCarriera.ScuolaDopoLaGara => ScuolaDopoLaGara(f),
        MomentoDiCarriera.ScuolaSiParlaDiTe => ScuolaSiParlaDiTe(f),
        _ => ArrivoAlVertice(f)
    };

    /// <summary>Il titolo con cui si apre la finestra della scena.</summary>
    public static string Titolo(MomentoDiCarriera momento) => momento switch
    {
        MomentoDiCarriera.PrimoGiorno => "CorsaCareer — il primo giorno",
        MomentoDiCarriera.PrimoTest => "CorsaCareer — il primo cronometro",
        MomentoDiCarriera.PrimaGara => "CorsaCareer — la vigilia",
        MomentoDiCarriera.PrimaVittoria => "CorsaCareer — la prima vittoria",
        MomentoDiCarriera.PrimoPodio => "CorsaCareer — il primo podio",
        MomentoDiCarriera.PrimaBattuta => "CorsaCareer — la domenica storta",
        MomentoDiCarriera.PrimaFirma => "CorsaCareer — la firma",
        MomentoDiCarriera.AperturaStagione => "CorsaCareer — si comincia",
        MomentoDiCarriera.MetaStagione => "CorsaCareer — a metà strada",
        MomentoDiCarriera.UltimaGara => "CorsaCareer — l'ultima",
        MomentoDiCarriera.TitoloVinto => "CorsaCareer — campione",
        MomentoDiCarriera.CambioCategoria => "CorsaCareer — si cambia macchina",
        MomentoDiCarriera.Retrocessione => "CorsaCareer — si torna indietro",
        MomentoDiCarriera.CassaVuota => "CorsaCareer — i conti",
        MomentoDiCarriera.PrimoSponsor => "CorsaCareer — il primo sponsor",
        MomentoDiCarriera.SponsorRifiutato => "CorsaCareer — un no",
        MomentoDiCarriera.Rivalita => "CorsaCareer — il rivale",
        MomentoDiCarriera.ScuolaDopoLaGara => "CorsaCareer — a scuola, lunedì",
        MomentoDiCarriera.ScuolaSiParlaDiTe => "CorsaCareer — a scuola",
        _ => "CorsaCareer — il vertice"
    };

    // ===================================================================
    //  Le scene. Ognuna sceglie chi parla dal regista, e dice solo cose
    //  che risultano dai dati.
    // ===================================================================

    private static List<AnimeDialogueLine> PrimoGiorno(Fatti f) =>
    [
        CastDirector.Battuta(CastDirector.Genji,
            "«Questo telaio ha più anni di te. Il motore l'ho tirato fuori da un generatore che nessuno voleva. "
            + "Non è un bel kart, ragazzo: è un kart che funziona, ed è una cosa diversa.»", "neutro"),
        CastDirector.Battuta(CastDirector.Haru,
            $"«A me sembra bellissimo. {f.Pilota}, hai capito? È nostro. "
            + $"Abbiamo € {f.Cassa:N0} in tutto e un kart che va: gli altri hanno solo i soldi.»", "felice"),
        CastDirector.Battuta(CastDirector.Genji,
            "«Non montarti la testa. Là fuori ci sono ragazzi che girano da quando avevano sei anni. "
            + "Tu parti indietro e devi saperlo. Però se impari a sentire quando il posteriore comincia ad andare, "
            + "recuperi in un anno quello che loro hanno messo insieme in cinque.»", "fiero")
    ];

    private static List<AnimeDialogueLine> PrimoTest(Fatti f) =>
    [
        CastDirector.Battuta(CastDirector.Genji,
            "«Adesso ti metto il cronometro addosso, ed è la prima volta. Non guardare me e non guardare il tempo: "
            + "guarda dove metti le ruote all'uscita dell'ultima curva. È lì che si guadagna, non in staccata.»", "neutro"),
        CastDirector.Battuta(CastDirector.Haru,
            $"«Sono qui al muretto, {f.Pilota}. Non ti dico niente perché mi hanno detto di stare zitto. "
            + "Però sto per urlare, lo sappiamo tutti e due.»", "felice"),
        CastDirector.Battuta(CastDirector.Shigeo,
            "«Un consiglio pratico: i primi tre giri non provare a fare il tempo. Scalda le gomme, prendi i riferimenti, "
            + "e solo al quarto giro comincia a spingere davvero. Chi fa il tempo al primo giro non lo fa mai.»", "neutro")
    ];

    private static List<AnimeDialogueLine> PrimaGara(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Rei,
                $"«Domani è la tua prima gara vera, {f.Pilota}"
                + (string.IsNullOrEmpty(f.Categoria) ? ".»" : $", in {f.Categoria.ToLowerInvariant()}.»")
                + " Ti dico l'unica cosa che conta: alla prima curva non devi guadagnare posizioni, "
                + "devi arrivarci. Metà dei debuttanti la gara la finisce nella ghiaia dopo trenta secondi.»", "cauta")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Haru,
            "«Io ho già preparato tutto: acqua, panini, il cronometro, il rotolo di nastro. "
            + "Ho anche il numero di un carro attrezzi, ma quello non serve, vero? Non serve.»", "preoccupato"));
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Dormi. È il consiglio migliore che ho. Chi arriva in pista stanco frena sempre un metro dopo.»", "neutro"));
        return righe;
    }

    private static List<AnimeDialogueLine> PrimaVittoria(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«HAI VINTO! {f.Pilota}, HAI VINTO! Scusa, sto urlando, non me ne importa niente. "
                + "Ti ho visto uscire dall'ultima curva e ho capito che nessuno ti riprendeva più.»", "felice")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«La prima non si scorda. Le altre sì, ne vincerai talmente tante che non te le ricorderai tutte — "
            + "e va bene così. Questa però la vedrai ancora fra vent'anni quando chiudi gli occhi.»", "festa"));
        if (f.HaClassifica)
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                $"«Goditela stasera, e domani rimettiamoci al lavoro. In campionato adesso sei {Ordinale(f.PostoInClassifica)} "
                + $"su {f.Iscritti} con {f.Punti} punti. Una vittoria cambia una domenica; è la seconda che cambia una stagione.»", "serena"));
        else
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                "«Complimenti sinceri. Adesso però la parte difficile: rifarlo. "
                + "Una vittoria può essere fortuna, due sono un pilota.»", "serena"));
        return righe;
    }

    private static List<AnimeDialogueLine> PrimoPodio(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«Sul podio! {f.Pilota} sul podio! Ho fatto ventidue foto e sono tutte mosse, "
                + "ma una la incornicio lo stesso.»", "felice")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Shigeo,
            "«Il dato che mi interessa non è il piazzamento: è che hai fatto gli ultimi cinque giri "
            + "tutti dentro il mezzo secondo. Quella è costanza, e la costanza è la cosa che i team guardano "
            + "prima di tutte le altre.»", "neutro"));
        if (f.HaClassifica && f.PuntiDelPrimo > f.Punti)
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                $"«Sei {Ordinale(f.PostoInClassifica)} con {f.Punti} punti, e il primo ne ha {f.PuntiDelPrimo}: "
                + $"sono {f.PuntiDelPrimo - f.Punti} da recuperare"
                + (f.GareRimaste > 0 ? $" in {Gare(f.GareRimaste)}.»" : ".»")
                + " Si può fare, ma non con un podio ogni tanto.»", "cauta"));
        return righe;
    }

    private static List<AnimeDialogueLine> PrimaBattuta(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Genji,
                "«Vieni qui. Guarda la macchina, non guardare me. Vedi questo? Si aggiusta. "
                + "Stanotte lavoro io, tu vai a casa e domani mattina torna che ne parliamo.»", "deluso")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Haru,
            $"«{f.Pilota}, ascoltami un secondo. Lo so che adesso non serve a niente quello che ti dico. "
            + "Ma io ero là e ti ho visto: fino a quel momento stavi andando forte davvero. "
            + "Non è che hai smesso di essere veloce alle tre del pomeriggio.»", "preoccupato"));
        righe.Add(CastDirector.Battuta(CastDirector.Rei,
            "«Una domenica storta capita a tutti e non decide niente. Quello che decide è cosa fai la settimana dopo. "
            + "Ci vediamo in pista giovedì: rivediamo i dati insieme, senza drammi.»", "preoccupata"));
        return righe;
    }

    private static List<AnimeDialogueLine> PrimaFirma(Fatti f)
    {
        var squadra = string.IsNullOrEmpty(f.Squadra) ? "la squadra" : f.Squadra;
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«Hanno firmato. {squadra} ha firmato per te. Lo sai cosa vuol dire? "
                + "Che da domani non sei più uno che si presenta al cancello: sei uno che ha un posto.»", "documenti")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Rei,
            $"«Leggi bene prima di essere contento. {squadra} ti dà"
            + (string.IsNullOrEmpty(f.Vettura) ? " la macchina" : $" la {f.Vettura}")
            + (string.IsNullOrEmpty(f.Campionato) ? "" : $" per {f.Campionato}")
            + ". In cambio si aspetta dei risultati, e se non arrivano il posto lo dà a un altro. "
            + "È giusto così: è un lavoro, adesso.»", "cauta"));
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Io continuo a venire alle gare, se mi vuoi. Non ti servo più per il motore, "
            + "ma qualcuno che ti dica la verità quando sbagli ti servirà sempre.»", "fiero"));
        return righe;
    }

    private static List<AnimeDialogueLine> AperturaStagione(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>();
        var dove = string.IsNullOrEmpty(f.Campionato) ? "il campionato" : f.Campionato;
        righe.Add(CastDirector.Battuta(CastDirector.Rei,
            $"«Stagione {f.Stagione}, {dove}"
            + (f.GareRimaste > 0 ? $", {Gare(f.GareRimaste)} in calendario" : "")
            + $". Sei in {f.Categoria.ToLowerInvariant()}, che è il gradino {f.Gradino} di {f.GradiniTotali}. "
            + $"Il livello del campionato è il {f.Livello} di {f.LivelliTotali}: "
            + "chiudere nei primi tre ti porta a quello sopra, e questo è l'obiettivo dell'anno.»", "serena"));
        righe.Add(CastDirector.Battuta(CastDirector.Shigeo,
            "«Dal punto di vista tecnico ti dico una cosa sola: le prime gare valgono come le ultime. "
            + "Ogni anno c'è qualcuno che si sveglia a metà stagione e poi passa l'inverno a chiedersi "
            + "dove sono finiti i punti di aprile.»", "neutro"));
        if (!string.IsNullOrEmpty(f.Squadra))
            righe.Add(CastDirector.Battuta(CastDirector.Haru,
                $"«E siamo con {f.Squadra}! Ho già attaccato l'adesivo sul furgone. "
                + "Andiamo a prenderceli, questi punti.»", "felice"));
        return righe;
    }

    private static List<AnimeDialogueLine> MetaStagione(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>();
        if (f.HaClassifica)
        {
            var distacco = f.PuntiDelPrimo - f.Punti;
            righe.Add(CastDirector.Battuta(CastDirector.Haru,
                $"«Facciamo il punto, {f.Pilota}. Siamo {Ordinale(f.PostoInClassifica)} su {f.Iscritti} con {f.Punti} punti"
                + (distacco > 0 ? $", a {distacco} dal primo" : ", e davanti non c'è nessuno")
                + (f.GareRimaste > 0 ? $", e restano {Gare(f.GareRimaste)}.»" : ".»")
                + " Non è finita niente, in nessuno dei due sensi.»", "preoccupato"));
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                f.PostoInClassifica <= 3
                    ? $"«Sei dentro la zona che promuove — servono i primi tre — ma ci sei dentro di poco. "
                      + "Da qui alla fine non serve vincere: serve non perdere punti stupidamente.»"
                    : $"«Per salire servono i primi tre e tu sei {Ordinale(f.PostoInClassifica)}. "
                      + "Vuol dire che una gara buona non basta più: servono piazzamenti pieni da qui in avanti.»",
                f.PostoInClassifica <= 3 ? "serena" : "severa"));
        }
        else
        {
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                $"«Siamo a metà del percorso in {f.Categoria.ToLowerInvariant()}. "
                + $"Hai {f.Gare} gare in carriera e {f.Vittorie} vittorie: quello che conta adesso è la continuità.»", "cauta"));
        }
        righe.Add(CastDirector.Battuta(CastDirector.Shigeo,
            "«E la macchina è la stessa di aprile. Se vuoi trovare il decimo che manca, "
            + "non è nel motore: è nei primi due giri dopo la partenza, dove stai perdendo più di tutti.»", "neutro"));
        return righe;
    }

    private static List<AnimeDialogueLine> UltimaGara(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>();
        if (f.HaClassifica)
        {
            var distacco = f.PuntiDelPrimo - f.Punti;
            righe.Add(CastDirector.Battuta(CastDirector.Haru,
                $"«È l'ultima. {f.Pilota}, siamo {Ordinale(f.PostoInClassifica)} con {f.Punti} punti"
                + (distacco > 0 ? $" e ne mancano {distacco} al primo" : " e comandiamo noi")
                + ". Tutto quello che abbiamo fatto da marzo finisce dentro questo pomeriggio qua.»", "preoccupato"));
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                f.PostoInClassifica <= 3
                    ? "«Sei in zona promozione e devi solo confermarla. Ti chiedo una cosa sola: niente eroismi. "
                      + "Il rischio che prendi oggi non ti fa guadagnare niente e può costarti l'anno.»"
                    : "«Non sei in zona promozione, quindi oggi non hai niente da perdere. "
                      + "È la giornata giusta per provare quella cosa che non hai mai osato.»",
                f.PostoInClassifica <= 3 ? "severa" : "cauta"));
        }
        else
        {
            righe.Add(CastDirector.Battuta(CastDirector.Haru,
                $"«Ultima gara della stagione, {f.Pilota}. Comunque vada, sappi che quest'anno "
                + $"hai corso {f.Gare} gare. L'anno scorso, di questi tempi, non ne avevi corsa nemmeno una.»", "preoccupato"));
        }
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Ho controllato tutto due volte. La macchina non ti tradisce oggi. Il resto è tuo.»", "neutro"));
        return righe;
    }

    private static List<AnimeDialogueLine> TitoloVinto(Fatti f)
    {
        var dove = string.IsNullOrEmpty(f.Campionato) ? "il campionato" : f.Campionato;
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«Campione. {f.Pilota}, sei campione di {dove}. "
                + $"{f.Punti} punti, {f.Vittorie} vittorie in carriera. "
                + "Io mi siedo un attimo perché non mi reggono le gambe.»", "felice")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Ti ricordi il primo giorno, quando ti ho detto che partivi indietro? "
            + "Ecco. Adesso te lo dico io: non parti più indietro a nessuno.»", "festa"));
        righe.Add(CastDirector.Battuta(CastDirector.Rei,
            $"«Complimenti veri. E adesso la cosa che nessuno ti dirà stasera: da domani "
            + "sei quello da battere, e si corre in un modo completamente diverso. "
            + "Ne riparliamo la settimana prossima.»", "serena"));
        return righe;
    }

    private static List<AnimeDialogueLine> CambioCategoria(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«{f.Categoria}. Gradino {f.Gradino} su {f.GradiniTotali}. "
                + "Ti rendi conto di dove eravamo due anni fa?»", "felice")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Shigeo,
            "«Ti avverto adesso così non ti spaventi dopo: le prime uscite andrai piano. "
            + "Non perché sei diventato lento, ma perché questa macchina frena molto più tardi di quella di prima "
            + "e il tuo cervello non ci crede ancora. Ci vogliono tre o quattro weekend.»", "svolta"));
        righe.Add(CastDirector.Battuta(CastDirector.Rei,
            "«E il gruppo è più forte. Quello che qui è un decimo posto, "
            + "nella categoria di prima sarebbe stato un podio. Non guardare la posizione per un po': "
            + "guarda il distacco dal tuo compagno di squadra.»", "cauta"));
        return righe;
    }

    private static List<AnimeDialogueLine> Retrocessione(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«Va bene. Va bene. Si torna giù di un livello e non è la fine del mondo, {f.Pilota}. "
                + "Ci siamo già stati, sappiamo com'è fatto, e sappiamo anche come si risale.»", "preoccupato")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Rei,
            $"«Sono chiara perché serve. Hai chiuso {(f.HaClassifica ? Ordinale(f.PostoInClassifica) + $" su {f.Iscritti}" : "in fondo")}, "
            + "e a quel punto della classifica il sedile non te lo rinnova nessuno. Per rientrare serve una prova, "
            + "e da lì nascono le proposte nuove. È lo stesso percorso dell'inizio.»", "severa"));
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Ne ho visti tanti scendere. Quelli che poi sono tornati su avevano tutti una cosa in comune: "
            + "non hanno passato l'inverno a spiegare di chi era la colpa.»", "preoccupato"));
        return righe;
    }

    private static List<AnimeDialogueLine> CassaVuota(Fatti f) =>
    [
        CastDirector.Battuta(CastDirector.Tooru,
            $"«Ho rifatto i conti tre volte. In cassa ci sono € {f.Cassa:N0}. "
            + "L'iscrizione della prossima non la copriamo. Scusa se te lo dico così, non sono capace di girarci intorno.»"),
        CastDirector.Battuta(CastDirector.Haru,
            "«Allora si trovano. Vado a bussare a tutte le porte di questa città, una per una. "
            + "Qualcuno che dice di sì c'è sempre — devo solo trovarlo prima di domenica.»", "preoccupato"),
        CastDirector.Battuta(CastDirector.Miki,
            "«O si trovano, o si salta una gara e si risparmia per la successiva. "
            + "Non è una resa: saltarne una scelta da noi è meglio che saltarne tre perché siamo a zero. "
            + "Decidete voi, ma decidete adesso.»", "preoccupata")
    ];

    private static List<AnimeDialogueLine> PrimoSponsor(Fatti f)
    {
        var chi = f.HaSponsor ? f.Sponsor : "il primo sponsor";
        return
        [
            CastDirector.Battuta(CastDirector.Haru,
                $"«HANNO DETTO DI SÌ! {chi} ha detto di sì! "
                + "Ho l'adesivo in mano, guarda, ce l'ho proprio qui in mano.»", "sollevato"),
            CastDirector.Battuta(CastDirector.Tooru,
                $"«In cassa adesso ci sono € {f.Cassa:N0}. Lo dico perché è la prima volta da quando abbiamo cominciato "
                + "che il numero sale invece di scendere. Volevo solo dirlo ad alta voce.»"),
            CastDirector.Battuta(CastDirector.Genji,
                "«Quell'adesivo mettilo dove si vede, e trattalo bene. "
                + "Il primo che ti dà dei soldi quando non sei nessuno se lo ricorda tutta la vita — e tu pure.»", "fiero")
        ];
    }

    private static List<AnimeDialogueLine> SponsorRifiutato(Fatti f) =>
    [
        CastDirector.Battuta(CastDirector.Haru,
            "«Niente. Mi ha ascoltato tutto, è stato gentile, e alla fine ha detto di no. "
            + "Il brutto è che aveva anche ragione: cosa gli porto, in cambio?»", "preoccupato"),
        CastDirector.Battuta(CastDirector.Miki,
            $"«Gli porti quello che hai: {f.Gare} gare, {f.Podi} podi, livello di seguito {f.Seguito} su 100. "
            + "Oggi non basta. Fra sei mesi, con gli stessi numeri più alti, la stessa persona ti dice di sì. "
            + "Non è un rifiuto: è un rinvio.»", "rifiuto"),
        CastDirector.Battuta(CastDirector.Tooru,
            "«Io intanto ho fatto la lista di chi non abbiamo ancora provato. Sono undici. "
            + "Cominciamo dai tre più vicini così non spendiamo in treno.»")
    ];

    private static List<AnimeDialogueLine> Rivalita(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Riku,
                $"«Bel weekend, {f.Pilota}. Davvero. Non fare quella faccia, te lo sto dicendo sul serio.» "
                + "Poi, più piano: «Però la prossima volta parto davanti io, e allora vediamo.»", "spavaldo")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Haru,
            "«Non rispondergli. Ti prego, non rispondergli.» Pausa. «Va bene, rispondigli.»", "felice"));
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Quel ragazzo lì ti fa un favore e non lo sa. Senza uno così davanti, "
            + "tu adesso staresti girando un secondo più piano e saresti pure contento.»", "neutro"));
        return righe;
    }

    // ---------------------------------------------------- la scuola

    private static List<AnimeDialogueLine> ScuolaDopoLaGara(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Sae,
                $"«Ti sei addormentato a matematica. Ti ho coperto io, ma il professore ha visto tutto.» "
                + "Poi si siede sul banco davanti al tuo. «Allora? Com'è andata domenica?»")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Tooru,
            $"«Io lo so com'è andata, ho guardato i risultati. {f.Gare} gare in carriera, {f.Podi} podi. "
            + "E prima che me lo chiedi: no, non ho fatto i compiti nemmeno io.»"));
        righe.Add(CastDirector.Battuta(CastDirector.Sae,
            "«Comunque lunedì è sempre così. Corri, torni, e il mondo qui dentro non si è accorto di niente. "
            + "All'inizio mi faceva rabbia. Adesso mi piace: è l'unico posto dove nessuno mi chiede dei tempi.»"));
        return righe;
    }

    private static List<AnimeDialogueLine> ScuolaSiParlaDiTe(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Tooru,
                $"«C'è una cosa che devi sapere. In terza B parlano di te.» "
                + $"«Livello di seguito {f.Seguito} su 100, l'ho controllato. "
                + "Non è tanto in assoluto, ma per uno di questa scuola è enorme.»")
        };
        righe.Add(CastDirector.Battuta(CastDirector.Sae,
            "«E adesso comincia la parte fastidiosa: quelli che non ti hanno mai parlato "
            + "diventano tuoi amici, e quelli che ti erano amici si mettono in imbarazzo. "
            + "Dura un paio di mesi. Poi passa e restano quelli veri.»"));
        if (f.Seguito >= 40)
            righe.Add(CastDirector.Battuta(CastDirector.Noa,
                $"«Anche fuori dalla scuola, comunque. Con {f.Seguito} di seguito cominci a essere "
                + "un nome che si può scrivere in un titolo. Quando succede, le squadre se ne accorgono "
                + "prima ancora di guardare i tuoi tempi.»", "neutro"));
        return righe;
    }

    private static List<AnimeDialogueLine> ArrivoAlVertice(Fatti f)
    {
        var righe = new List<AnimeDialogueLine>
        {
            CastDirector.Battuta(CastDirector.Haru,
                $"«{f.Categoria}. Il gradino {f.Gradino} di {f.GradiniTotali}. Non ce n'è un altro sopra, {f.Pilota}: "
                + "questo è il posto dove volevamo arrivare quando avevamo il kart tenuto insieme col nastro.»", "felice")
        };
        if (f.Eta > 0)
            righe.Add(CastDirector.Battuta(CastDirector.Rei,
                $"«Hai {f.Eta} anni e {f.Gare} gare in carriera. Sei arrivato dove arrivano venti persone al mondo. "
                + "Adesso ti dico l'ultima cosa utile che ho da dirti: qui non ti promuove più nessuno. "
                + "Da adesso in poi conta solo quello che fai la domenica.»", "serena"));
        righe.Add(CastDirector.Battuta(CastDirector.Genji,
            "«Io in televisione non ti ci vedo bene, sai? Ti vedo ancora con le mani sporche "
            + "e il motore smontato per terra.» Pausa. «Vai a vincere, ragazzo.»", "fiero"));
        return righe;
    }

    // ------------------------------------------------------------ utilità

    private static string Ordinale(int n) => n switch
    {
        1 => "primo", 2 => "secondo", 3 => "terzo", 4 => "quarto", 5 => "quinto",
        6 => "sesto", 7 => "settimo", 8 => "ottavo", 9 => "nono", 10 => "decimo",
        _ => $"{n}°"
    };

    private static string Gare(int n) => n == 1 ? "una gara" : $"{n} gare";
}
