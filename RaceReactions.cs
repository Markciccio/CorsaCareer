namespace CorsaCareer1991;

/// <summary>
/// Che cosa si dicono al box appena rientrato.
///
/// Prima la scena era sempre la stessa: dodici battute e sette personaggi in
/// fila indiana, tutti a commentare ogni singola gara — il meccanico, il
/// direttore, l'amico, la stratega, la giornalista, l'ingegnere e il rivale,
/// nell'ordine, domenica dopo domenica. Con quel formato ogni gara sembrava
/// identica alla precedente, e nessuna delle battute poteva permettersi di dire
/// qualcosa di specifico, perché doveva funzionare in tutti i casi.
///
/// Qui parlano al massimo tre persone, scelte da quello che è successo davvero:
/// se la macchina si è rotta c'è il meccanico, se hai fatto una rimonta c'è chi
/// l'ha vista, se il campionato si è mosso c'è chi tiene i conti. E parlano
/// della gara con i numeri di quella gara: il circuito, il piazzamento, i punti
/// guadagnati, la classifica che cambia, quanto manca alla fine.
/// </summary>
public static class RaceReactions
{
    /// <summary>
    /// Tutto quello che serve per commentare una gara sapendo di che si parla.
    ///
    /// I tre «Ha…» non sono un dettaglio: dicono quali dati esistono davvero.
    /// Un personaggio non deve mai citare un numero che il programma non ha —
    /// se la classifica non c'è non si dice «siamo primi», se la posizione di
    /// partenza non è stata registrata non si parla di rimonta. Meglio una
    /// battuta più corta che una frase che racconta una cosa mai successa.
    /// </summary>
    private sealed record Contesto(
        string Pilota, string Circuito, string Vettura,
        int Posizione, int Partenti, bool HaPartenti, bool Ritiro, bool Vittoria, bool Podio,
        int Griglia, bool HaGriglia, int PosizioniGuadagnate, int PuntiGara, double Penalita,
        bool HaClassifica, int PostoCampionato, int PuntiCampionato, int DistanzaDalPrimo, int DistanzaDalTerzo,
        int GareRimaste, string Leader, string ProssimoCampionato, bool PuoSalire);

    public static IReadOnlyList<AnimeDialogueLine> Build(CareerState career, RaceHistoryEntry gara,
        IReadOnlyList<ContentCarRecord> vetture, IReadOnlyList<ContentTrackRecord> circuiti,
        IReadOnlyList<ScheduledEvent> agenda)
    {
        var c = Leggi(career, gara, vetture, circuiti, agenda);

        // Chi parla: al massimo tre, e sempre scelti da quello che è successo.
        // La rotazione usa il numero di gara, così due domeniche di fila non
        // hanno mai la stessa formazione anche a parità di risultato.
        var voci = new List<AnimeDialogueLine>();
        var rotazione = Math.Max(0, career.Races);

        // 1. Chi apre. Il meccanico se la macchina ha ceduto, altrimenti Haru,
        //    che è quello che la gara la vive insieme a te.
        voci.Add(c.Ritiro && rotazione % 2 == 0
            ? CastDirector.Battuta(CastDirector.Genji, Meccanico(c), "deluso")
            : CastDirector.Battuta(CastDirector.Haru, Amico(c), Umore(c)));

        // 2. Chi mette la gara nel campionato. È la parte che al giocatore
        //    serve davvero: dove sono adesso e che cosa manca.
        voci.Add(rotazione % 3 == 0 && !c.Ritiro
            ? CastDirector.Battuta(CastDirector.Shigeo, Ingegnere(c), "neutro")
            : CastDirector.Battuta(CastDirector.Rei, Direttrice(c), c.Podio ? "serena" : c.Ritiro ? "preoccupata" : "cauta"));

        // 3. La terza voce c'è solo se ha qualcosa da dire: il rivale quando
        //    c'è stato un confronto, la giornalista quando è successo qualcosa
        //    che si racconta. Se non c'è motivo, la scena finisce a due.
        var terza = Terza(c, rotazione);
        if (terza != null) voci.Add(terza);

        return voci;
    }

    // ------------------------------------------------------------- le voci

    private static string Amico(Contesto c)
    {
        if (c.Ritiro)
            return $"«Che sfortuna, {c.Pilota}. Ti stavo guardando e stavi andando bene davvero, poi ti ho visto rallentare "
                   + "e ho capito subito. Non è colpa tua, queste cose capitano e capitano a tutti. "
                   + (c.HaClassifica
                       ? $"Adesso però rialziamoci: in campionato siamo {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato} punti"
                         + (c.GareRimaste > 0 ? $" e ci restano {Gare(c.GareRimaste)}. Non è finita niente.»" : " e la stagione è agli sgoccioli.»")
                       : "Testa alta e si riparte dalla prossima.»");

        if (c.Vittoria)
            return $"«HAI VINTO! Hai vinto a {c.Circuito}, {c.Pilota}! Ho urlato talmente forte che quelli del box accanto si sono girati. "
                   + (c.PuntiGara > 0 ? $"E sono {c.PuntiGara} punti che pesano. " : "")
                   + (c.HaClassifica
                       ? $"Adesso in campionato siamo {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato} punti"
                         + (c.PostoCampionato > 1 && c.DistanzaDalPrimo > 0 ? $", a soli {c.DistanzaDalPrimo} dal primo" : "")
                         + (c.GareRimaste > 0 ? $" e mancano {Gare(c.GareRimaste)}. Ci siamo, ci siamo davvero!»" : ". Che stagione!»")
                       : "Questa ce la ricordiamo!»");

        // Della rimonta si parla solo se la posizione di partenza è stata
        // davvero registrata: senza, non si sa da dove sei partito.
        var apertura = c.HaGriglia && c.PosizioniGuadagnate >= 3
            ? $"«Bella gara, {c.Pilota}! Eri {Ordinale(c.Griglia)} in griglia e sei arrivato {Ordinale(c.Posizione)}: {c.PosizioniGuadagnate} posizioni recuperate, "
              + $"e a {c.Circuito} sorpassare non è mai semplice. "
            : c.Podio
                ? $"«Podio! {Maiuscola(Ordinale(c.Posizione))} posto a {c.Circuito}, e te lo sei meritato. "
                : $"«È stata una gara combattuta. {Maiuscola(Ordinale(c.Posizione))} posto a {c.Circuito}: potevamo fare meglio, ma te la sei cavata bene. ";

        var campionato = !c.HaClassifica
            ? (c.PuntiGara > 0 ? $"Sono {c.PuntiGara} punti portati a casa. " : "")
            : c.PuntiGara > 0
                ? $"Sono {c.PuntiGara} punti importanti: in campionato siamo {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato} punti"
                  + (c.PostoCampionato > 1 && c.DistanzaDalPrimo > 0 && c.Leader.Length > 0 ? $", a {c.DistanzaDalPrimo} da {c.Leader}. " : ". ")
                : $"Punti non ne abbiamo portati a casa, e in campionato restiamo {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato}. ";

        var chiusura = c.GareRimaste <= 0
            ? "La stagione finisce qui: adesso si contano i conti.»"
            : c.GareRimaste == 1 && c.PuoSalire
                ? $"Resta concentrato, manca una gara sola: se arriviamo nei primi tre passiamo a {c.ProssimoCampionato}! Forza {c.Pilota}!»"
                : c.PuoSalire
                    ? $"Restano {Gare(c.GareRimaste)} e i primi tre valgono {c.ProssimoCampionato}. Ce la possiamo fare, dai! Forza {c.Pilota}!»"
                    : $"Restano {Gare(c.GareRimaste)}: testa bassa e pedalare.»";

        return apertura + campionato + chiusura;
    }

    private static string Direttrice(Contesto c)
    {
        if (c.Ritiro)
            return "«Un ritiro costa il doppio: perdi i punti di oggi e regali quelli agli altri. "
                   + (c.HaClassifica
                       ? $"Sei {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato} punti"
                         + (c.DistanzaDalTerzo > 0 ? $" e al terzo posto ne mancano {c.DistanzaDalTerzo}" : "")
                         + (c.GareRimaste > 0 ? $", con {Gare(c.GareRimaste)} da correre. " : ". ")
                       : "")
                   + "Voglio che nelle prossime tu porti la macchina al traguardo, anche se dovesse costarti due posizioni.»";

        if (c.Vittoria)
            return "«Vittoria meritata, e lo dico raramente. "
                   + (c.HaClassifica
                       ? $"Adesso guardiamo la classifica: {Ordinale(c.PostoCampionato)} posto, {c.PuntiCampionato} punti"
                         + (c.PostoCampionato > 1 && c.DistanzaDalPrimo > 0 && c.Leader.Length > 0 ? $", {c.DistanzaDalPrimo} da {c.Leader}" : "")
                         + (c.GareRimaste > 0 ? $", {Gare(c.GareRimaste)} rimaste. " : ". ")
                       : "")
                   + "Da qui in avanti gli altri correranno contro di te: preparati a difenderti, che è più difficile che attaccare.»";

        var posizione = c.HaPartenti
            ? $"{Maiuscola(Ordinale(c.Posizione))} posto su {c.Partenti}"
            : $"{Maiuscola(Ordinale(c.Posizione))} posto";
        var penalita = c.Penalita > 0 ? $" Ci siamo presi {c.Penalita:0} secondi di penalità: sono punti buttati, e a fine anno pesano." : "";

        if (!c.HaClassifica)
            return $"«{posizione}.{penalita} Il paddock non guarda la singola domenica, guarda la costanza: "
                   + "è quella la riga su cui verrai giudicato.»";

        if (c.GareRimaste == 0)
            return $"«{posizione} nell'ultima gara dell'anno.{penalita} Chiudiamo {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato} punti. "
                   + "Adesso conta solo dove ci lascia questa classifica.»";

        return $"«{posizione}.{penalita} In campionato siamo {Ordinale(c.PostoCampionato)} con {c.PuntiCampionato} punti"
               + (c.DistanzaDalTerzo > 0
                   ? $", e per entrare nei primi tre — quelli che salgono — ne servono {c.DistanzaDalTerzo}. "
                   : ", dentro la zona che promuove. ")
               + (c.GareRimaste == 1
                   ? "Rimane una gara sola. Sai già cosa devi fare.»"
                   : $"Restano {Gare(c.GareRimaste)}: si decide lì.»");
    }

    private static string Ingegnere(Contesto c)
    {
        var passo = c.Podio
            ? "Il passo c'era per tutta la gara, e soprattutto c'era negli ultimi giri: è lì che si vede se un pilota ha finito le gomme o le ha gestite."
            : c.HaGriglia && c.PosizioniGuadagnate >= 3
                ? $"Hai recuperato {c.PosizioniGuadagnate} posizioni, e a {c.Circuito} non si sorpassa per caso: significa che stavi frenando più tardi degli altri e che la macchina ti seguiva."
                : "Il passo nella prima metà era buono, poi hai perso qualche decimo per giro. Non è la macchina: è la gestione delle gomme, e quella si allena.";
        return $"«Ho guardato i dati. {passo} "
               + (c.GareRimaste > 0
                   ? $"Per le prossime {Gare(c.GareRimaste)} lavoriamo su quello, non su altro.»"
                   : "Ce lo teniamo per l'anno prossimo.»");
    }

    private static string Meccanico(Contesto c) =>
        $"«{c.Vettura} si è fermata prima della fine. La macchina l'ho preparata io, quindi il primo che si prende la colpa sono io: "
        + "stanotte la apro e trovo il pezzo. Tu non pensarci, che di gare da correre ce ne sono ancora.»";

    private static AnimeDialogueLine? Terza(Contesto c, int rotazione)
    {
        // La giornalista arriva quando è successo qualcosa che si racconta.
        if (c.Vittoria)
            return CastDirector.Battuta(CastDirector.Noa,
                $"«Una domanda sola, poi ti lascio festeggiare. Hai vinto a {c.Circuito}"
                + (c.HaGriglia ? $" partendo {Ordinale(c.Griglia)}" : "") + ": "
                + "quando hai capito che la gara era tua? Lo chiedo perché è la frase che finisce nel titolo, e preferisco che sia tua e non mia.»",
                "entusiasta");

        if (c.HaGriglia && c.PosizioniGuadagnate >= 5)
            return CastDirector.Battuta(CastDirector.Noa,
                $"«{c.PosizioniGuadagnate} posizioni recuperate. Sono passata a chiedertelo perché una rimonta così, in una categoria come questa, "
                + "la gente se la ricorda più di una vittoria facile. Mi racconti com'è andata al via?»",
                "neutro");

        // Il rivale parla solo se c'è stato un confronto vero.
        if (c.Ritiro && rotazione % 2 == 1)
            return CastDirector.Battuta(CastDirector.Riku,
                "«Ti ho visto fermo a bordo pista. Non è divertente vincere così, te lo dico onestamente. "
                + "Rimettila a posto e ci rivediamo in griglia, che io ti voglio battere quando sei intero.»",
                "neutro");

        if (c.Podio && !c.Vittoria)
            return CastDirector.Battuta(CastDirector.Riku,
                $"«{Maiuscola(Ordinale(c.Posizione))}. Bravo, davvero.» — poi si volta mentre se ne va — «Però il podio adesso devi difenderlo, "
                + "e difendere è tutta un'altra cosa rispetto a prendersi le cose.»",
                "spavaldo");

        if (!c.Podio && !c.Ritiro && rotazione % 4 == 0)
            return CastDirector.Battuta(CastDirector.Riku,
                $"«{Maiuscola(Ordinale(c.Posizione))}{(c.HaPartenti ? $" su {c.Partenti}" : "")}. Non è un disastro e non è niente di che. "
                + "La prossima portati qualcosa in più, che così non ci divertiamo né io né te.»",
                "beffardo");

        return null;
    }

    // ------------------------------------------------------------ i numeri

    private static Contesto Leggi(CareerState career, RaceHistoryEntry gara,
        IReadOnlyList<ContentCarRecord> vetture, IReadOnlyList<ContentTrackRecord> circuiti,
        IReadOnlyList<ScheduledEvent> agenda)
    {
        var pista = circuiti.FirstOrDefault(x => x.Id.Equals(gara.Track, StringComparison.OrdinalIgnoreCase));
        var auto = vetture.FirstOrDefault(x => x.Id.Equals(gara.Car, StringComparison.OrdinalIgnoreCase));
        var classifica = (career.Standings ?? []).OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).ToList();
        var mio = classifica.FindIndex(x => x.Driver.Equals(career.Driver, StringComparison.OrdinalIgnoreCase));
        // La classifica esiste solo se il pilota ci compare davvero. Prima, non
        // trovandolo, si ripiegava su «primo posto» con i punti di carriera: una
        // posizione inventata di sana pianta, detta da un personaggio come se
        // fosse un fatto.
        var haClassifica = mio >= 0;
        var puntiMiei = haClassifica ? classifica[mio].Points : 0;
        var primo = classifica.Count > 0 ? classifica[0].Points : 0;
        var terzo = classifica.Count >= 3 ? classifica[2].Points : 0;
        var livello = ChampionshipLadder.Clamp(career.ChampionshipLevel);
        var inGriglia = gara.Classification?.Count ?? 0;

        return new Contesto(
            Pilota: string.IsNullOrWhiteSpace(career.Driver) ? "ragazzo" : career.Driver.Split(' ')[0],
            Circuito: string.IsNullOrWhiteSpace(pista?.Name) ? NarrativeEngine.Capitalize((gara.Track ?? "").Replace('_', ' ')) : pista.Name,
            Vettura: string.IsNullOrWhiteSpace(auto?.Name) ? "La macchina" : auto.Name,
            Posizione: gara.Position,
            Partenti: inGriglia,
            HaPartenti: inGriglia > 1,
            Ritiro: gara.Dnf,
            Vittoria: !gara.Dnf && gara.Position == 1,
            Podio: !gara.Dnf && gara.Position is > 0 and <= 3,
            Griglia: gara.StartingPosition,
            HaGriglia: gara.StartingPosition > 0,
            PosizioniGuadagnate: gara.Dnf || gara.StartingPosition <= 0 ? 0 : Math.Max(0, gara.StartingPosition - gara.Position),
            PuntiGara: gara.Points,
            Penalita: gara.PenaltySeconds,
            HaClassifica: haClassifica,
            PostoCampionato: haClassifica ? mio + 1 : 0,
            PuntiCampionato: puntiMiei,
            DistanzaDalPrimo: haClassifica ? Math.Max(0, primo - puntiMiei) : 0,
            DistanzaDalTerzo: haClassifica && classifica.Count >= 3 ? Math.Max(0, terzo - puntiMiei) : 0,
            GareRimaste: agenda.Count(x => x.Kind == ScheduledEventKind.ChampionshipRound
                                           && x.Season == career.Season && x.IsPlanned),
            Leader: classifica.Count > 0 ? classifica[0].Driver : "",
            ProssimoCampionato: ChampionshipLadder.IsTop(livello) ? "" : ChampionshipLadder.Name(livello + 1),
            PuoSalire: !ChampionshipLadder.IsTop(livello));
    }

    private static string Umore(Contesto c) =>
        c.Ritiro ? "preoccupato" : c.Podio ? "felice" : "";

    private static string Gare(int quante) => quante == 1 ? "una gara" : $"{quante} gare";

    private static string Maiuscola(string testo) =>
        string.IsNullOrEmpty(testo) ? testo : char.ToUpperInvariant(testo[0]) + testo[1..];

    private static string Ordinale(int posizione) => posizione switch
    {
        1 => "primo", 2 => "secondo", 3 => "terzo", 4 => "quarto", 5 => "quinto",
        6 => "sesto", 7 => "settimo", 8 => "ottavo", 9 => "nono", 10 => "decimo",
        11 => "undicesimo", 12 => "dodicesimo", 13 => "tredicesimo", 14 => "quattordicesimo",
        15 => "quindicesimo", 16 => "sedicesimo", 17 => "diciassettesimo", 18 => "diciottesimo",
        19 => "diciannovesimo", 20 => "ventesimo",
        <= 0 => "senza posizione",
        _ => $"{posizione}°"
    };
}
