namespace CorsaCareer;

public enum ScheduledEventKind
{
    /// <summary>Prova di valutazione: il pilota non ha ancora un sedile.</summary>
    EvaluationTest,
    /// <summary>Prova di conferma, concessa a chi è andato vicino al riferimento.</summary>
    ConfirmationTest,
    /// <summary>Gara singola su invito: percorso alternativo quando la valutazione si arena.</summary>
    Invitation,
    /// <summary>Round di un campionato realmente firmato.</summary>
    ChampionshipRound
}

public sealed class ScheduledEvent
{
    public string Id { get; set; } = "";
    public ScheduledEventKind Kind { get; set; }
    public DateTime Date { get; set; }
    public string TrackId { get; set; } = "";
    public string TrackName { get; set; } = "";
    public string Country { get; set; } = "";
    public int Season { get; set; }
    /// <summary>Numero di round: valorizzato solo per i round di campionato.</summary>
    public int Round { get; set; }
    public string Status { get; set; } = CareerScheduler.StatusPlanned;
    /// <summary>Perché questo appuntamento esiste: è la catena causale dell'agenda.</summary>
    public string GeneratedBy { get; set; } = "";
    public string Objective { get; set; } = "";
    /// <summary>Costo d'iscrizione concordato per un invito; zero per test e campionato.</summary>
    public int EntryFee { get; set; }
    public bool EntryFeePaid { get; set; }
    /// <summary>Scuderia/organizzatore che ha proposto l'invito.</summary>
    public string ProposedBy { get; set; } = "";

    public bool IsTest => Kind is ScheduledEventKind.EvaluationTest or ScheduledEventKind.ConfirmationTest;
    public bool IsPlanned => Status.Equals(CareerScheduler.StatusPlanned, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Agenda della carriera.
///
/// Prima il calendario era una proiezione statica della cartella dei contenuti:
/// <c>BuildRounds()</c> trasformava ogni circuito installato in un round e lo
/// faceva a ogni avvio, indipendentemente dalla fase di carriera. Il risultato
/// era incoerente: durante la rookie evaluation — senza contratto e senza
/// campionato — esisteva già un calendario completo, e nessun esito poteva
/// cambiare quello che veniva dopo.
///
/// Qui l'agenda è persistente e causale: esiste solo ciò che è stato davvero
/// generato da un fatto precedente. Una prova produce la prova successiva, o un
/// invito, o — dopo la firma — il calendario del campionato. Ogni voce dichiara
/// da cosa è nata.
/// </summary>
public static class CareerScheduler
{
    public const string StatusPlanned = "In programma";
    public const string StatusDone = "Concluso";
    public const string StatusCancelled = "Annullato";

    /// <summary>
    /// Saltata dal pilota. Diverso da «annullato»: l'appuntamento c'era e la
    /// scelta di non esserci è stata sua, e resta nella storia della carriera.
    /// </summary>
    public const string StatusSkipped = "Saltata";

    /// <summary>Tentativi di valutazione dopo i quali si apre un percorso alternativo.</summary>
    public const int AttemptsBeforeAlternativePath = 3;
    /// <summary>Un invito è una possibilità guadagnata, non il premio per una serie di test negativi.</summary>
    public const int MinimumInvitationScore = 65;

    /// <summary>Primo appuntamento di una carriera: una prova, non un campionato.</summary>
    public static ScheduledEvent EvaluationStart(IReadOnlyList<ContentTrackRecord> tracks, DateTime date, int season = 1)
    {
        var track = PickTrack(tracks, 0);
        return new ScheduledEvent
        {
            Id = $"eval-s{season:00}-1",
            Kind = ScheduledEventKind.EvaluationTest,
            Date = date,
            TrackId = track?.Id ?? "",
            TrackName = track?.Name ?? "",
            Country = track?.Country ?? "",
            Season = season,
            GeneratedBy = "Apertura della carriera: il paddock concede una prova di valutazione",
            Objective = "Avvicinare il tempo di riferimento per ottenere un sedile"
        };
    }

    /// <summary>
    /// Cosa viene dopo una prova di valutazione. È il punto in cui l'agenda
    /// diventa causale: l'esito reale decide il prossimo appuntamento.
    /// </summary>
    /// <summary>
    /// Quota della prima gara offerta dopo la prova superata. Deve essere
    /// pagabile con il capitale iniziale ma pesare: e' la prima decisione
    /// economica vera della carriera.
    /// </summary>
    public const int FirstRaceFee = 300;

    /// <param name="cash">
    /// Quanto ha in cassa il pilota. Serve a non proporre gare fuori dalla sua
    /// portata: un invito che non si puo' pagare blocca la carriera invece di
    /// farla avanzare, perche' occupa l'unica voce dell'agenda.
    /// </param>
    public static ScheduledEvent? AfterEvaluation(RookieVerdict verdict, int attempts, IReadOnlyList<ContentTrackRecord> tracks, DateTime lastDate, int season = 1, int cash = int.MaxValue, int races = 0)
    {
        // Il numero di prove non basta a distinguere un appuntamento: dopo una
        // gara resta lo stesso, e l'identificativo generato collideva con uno
        // gia' in agenda. Append lo rifiutava e la carriera si fermava senza
        // dire niente.
        var tag = races > 0 ? $"{attempts + 1}g{races}" : $"{attempts + 1}";
        // La rotazione dei circuiti deve avanzare a ogni appuntamento, non solo
        // a ogni prova: contando solo le prove restava ferma sulla stessa
        // casella e la carriera correva sempre sullo stesso tracciato.
        var turn = attempts + races;
        // Superata: un team si fa vivo e mette sul tavolo un weekend.
        //
        // Prima qui non si programmava niente e il calendario doveva nascere
        // dalla firma di un contratto: un percorso lungo — dialogo, scelta,
        // ramo cliente, generazione del calendario — in cui bastava un passaggio
        // rotto perche la carriera si fermasse, e si fermava.
        //
        // Un invito e' la stessa cosa di una prova, con in piu una quota da
        // pagare: e' la strada che funziona gia, e non richiede nessuna firma.
        if (verdict.Passed)
        {
            var opening = PickTrack(tracks, turn + 2);
            return new ScheduledEvent
            {
                Id = $"debut-s{season:00}-{tag}",
                Kind = ScheduledEventKind.Invitation,
                Date = DentroLaStagione(lastDate.AddDays(TestGapDays)),
                TrackId = opening?.Id ?? "",
                TrackName = opening?.Name ?? "",
                Country = opening?.Country ?? "",
                Season = season,
                GeneratedBy = "Prova superata: un team ti offre un weekend, a tue spese",
                Objective = "La prima gara vera: arrivare in fondo e battere qualcuno",
                EntryFee = FirstRaceFee,
                ProposedBy = attempts % 2 == 0 ? "Minato Apex Kart" : "Hoshi Kart Works"
            };
        }

        var next = lastDate.AddDays(TestGapDays);
        if (verdict.Close)
        {
            var track = PickTrack(tracks, turn);
            return new ScheduledEvent
            {
                Id = $"confirm-s{season:00}-{tag}",
                Kind = ScheduledEventKind.ConfirmationTest,
                Date = next,
                TrackId = track?.Id ?? "",
                TrackName = track?.Name ?? "",
                Country = track?.Country ?? "",
                Season = season,
                GeneratedBy = $"Prova {attempts} chiusa vicino al riferimento: il team concede una prova di conferma",
                Objective = "Confermare il passo su un circuito diverso"
            };
        }

        // La quota richiesta e quella sostenibile: sotto quella soglia non si
        // propone una gara, si propone un test. Meglio un appuntamento umile
        // che uno impossibile.
        var lateralFee = Math.Min(8000 + attempts * 2000, Math.Max(FirstRaceFee, (int)(cash * 0.6)));
        if (attempts >= AttemptsBeforeAlternativePath && verdict.Score >= MinimumInvitationScore
            && cash >= FirstRaceFee)
        {
            // La valutazione si è arenata: si apre una strada laterale invece di
            // riproporre la stessa prova all'infinito.
            var track = PickTrack(tracks, turn + 1);
            return new ScheduledEvent
            {
                Id = $"invite-s{season:00}-{tag}",
                Kind = ScheduledEventKind.Invitation,
                Date = DentroLaStagione(next),
                TrackId = track?.Id ?? "",
                TrackName = track?.Name ?? "",
                Country = track?.Country ?? "",
                Season = season,
                GeneratedBy = $"{attempts} prove senza il riferimento richiesto: arriva un invito a una gara minore",
                Objective = "Farsi notare in gara, dove il cronometro non è l'unico criterio",
                EntryFee = lateralFee,
                ProposedBy = attempts % 2 == 0 ? "Kaido Motorsport" : "Minato Apex Racing"
            };
        }

        if (attempts >= AttemptsBeforeAlternativePath)
        {
            var recovery = PickTrack(tracks, turn);
            return new ScheduledEvent
            {
                Id = $"recovery-s{season:00}-{tag}",
                Kind = ScheduledEventKind.EvaluationTest,
                Date = next,
                TrackId = recovery?.Id ?? "",
                TrackName = recovery?.Name ?? "",
                Country = recovery?.Country ?? "",
                Season = season,
                GeneratedBy = $"{attempts} prove sotto la soglia: il paddock non concede un invito e richiede un test di recupero",
                Objective = "Dimostrare continuità prima di tornare a chiedere una gara"
            };
        }

        var retry = PickTrack(tracks, turn);
        return new ScheduledEvent
        {
            Id = $"eval-s{season:00}-{tag}",
            Kind = ScheduledEventKind.EvaluationTest,
            Date = next,
            TrackId = retry?.Id ?? "",
            TrackName = retry?.Name ?? "",
            Country = retry?.Country ?? "",
            Season = season,
            GeneratedBy = $"Prova {attempts} lontana dal riferimento: il paddock concede un altro tentativo",
            Objective = "Avvicinare il tempo di riferimento"
        };
    }

    public const int TestGapDays = 10;

    /// <summary>
    /// La data di un appuntamento, riportata dentro l'anno sportivo.
    ///
    /// Gli appuntamenti della valutazione si fissavano a «dieci giorni dopo
    /// l'ultimo»: una catena di prove iniziata in autunno finiva per produrre
    /// gare a gennaio e febbraio. Le prove private restano dove cadono — si
    /// possono fare tutto l'anno — ma una gara no.
    /// </summary>
    public static DateTime DentroLaStagione(DateTime data)
    {
        var ultima = NarrativeCalendar.LastRaceSundayOfSeason(data.Year);
        if (data.Month >= NarrativeCalendar.SeasonFirstMonth && data <= ultima) return data;
        var anno = data.Month >= NarrativeCalendar.SeasonFirstMonth ? data.Year + 1 : data.Year;
        return new DateTime(anno, NarrativeCalendar.SeasonFirstMonth, 12);
    }

    /// <summary>
    /// Calendario di un campionato realmente firmato. Il numero di round dipende
    /// dalla categoria e dai circuiti disponibili: non è più "tutti i circuiti
    /// installati".
    /// </summary>
    /// <summary>
    /// Una sigla breve e stabile per un campionato: entra negli identificativi
    /// dei round, cosi due campionati diversi nella stessa stagione non si
    /// sovrappongono.
    /// </summary>
    /// <summary>
    /// La sigla del calendario di un campionato, quella che compare negli
    /// identificativi dei round. Serve a riconoscere se un calendario gia' in
    /// agenda e' dello stesso campionato che si sta per generare.
    /// </summary>
    public static string SeasonSignature(string championship, string category) => Sigla(championship, category);

    private static string Sigla(string championship, string category)
    {
        var testo = ((championship ?? "") + "|" + (category ?? "")).ToLowerInvariant();
        if (testo.Length == 0) return "";
        // Somma stabile: nessun caso, e lo stesso campionato da sempre la
        // stessa sigla anche fra un avvio e l'altro.
        var somma = 0;
        foreach (var c in testo) somma = (somma * 31 + c) % 9999;
        return $"c{somma:0000}";
    }

    public static List<ScheduledEvent> BuildSeason(IReadOnlyList<ContentTrackRecord> tracks, string tier, int season, DateTime seasonStart, string championship, string category = "")
    {
        var events = new List<ScheduledEvent>();
        if (tracks.Count == 0) return events;
        var target = RoundsForTier(tier);
        // Un contratto genera un calendario coerente con la vettura firmata:
        // un team kart non può spedire il pilota su un circuito GT e viceversa.
        // Se il catalogo è troppo povero manteniamo il fallback completo, così
        // i cataloghi di test minimali continuano a produrre una stagione.
        var compatible = TracksForCategory(tracks, category);
        var selected = SelectSeasonTracks(compatible.Count > 0 ? compatible : tracks, target, season);

        // La firma di un campionato diverso nella stessa stagione deve produrre
        // un calendario suo.
        //
        // Prima gli identificativi erano "round-sNN-01": firmando un secondo
        // sedile nello stesso anno esistevano gia — conclusi — e venivano
        // scartati tutti. Il calendario nuovo nasceva vuoto e la carriera non
        // chiudeva mai una stagione, quindi nessun titolo e nessun mondiale.
        var firma = Sigla(championship, category);
        for (var i = 0; i < selected.Count; i++)
        {
            var track = selected[i];
            events.Add(new ScheduledEvent
            {
                Id = $"round-s{season:00}{firma}-{i + 1:00}",
                Kind = ScheduledEventKind.ChampionshipRound,
                Date = NarrativeCalendar.RoundDate(i, seasonStart, selected.Count),
                TrackId = track.Id,
                TrackName = string.IsNullOrWhiteSpace(track.Name) ? track.Id : track.Name,
                Country = string.IsNullOrWhiteSpace(track.Country) ? championship : track.Country,
                Season = season,
                Round = i + 1,
                GeneratedBy = $"Calendario del {championship} generato alla firma del contratto",
                Objective = i == selected.Count - 1 ? "Ultimo round: chiude la stagione" : "Round di campionato"
            });
        }
        return events;
    }

    private static List<ContentTrackRecord> TracksForCategory(IReadOnlyList<ContentTrackRecord> tracks, string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return tracks.ToList();
        var kart = category.Contains("kart", StringComparison.OrdinalIgnoreCase);
        return tracks.Where(track =>
        {
            var text = $"{track.Id} {track.Name} {track.Category}";
            var isKartTrack = text.Contains("kart", StringComparison.OrdinalIgnoreCase)
                || track.Category.Equals("kartodromo", StringComparison.OrdinalIgnoreCase);
            if (kart) return isKartTrack;
            // Le categorie a ruote coperte/formula usano tracciati permanenti
            // o cittadini, non layout kart dedicati.
            return !isKartTrack
                && !track.Category.Equals("hillclimb", StringComparison.OrdinalIgnoreCase)
                && !track.Category.Equals("special", StringComparison.OrdinalIgnoreCase);
        }).ToList();
    }

    /// <summary>Numero di appuntamenti credibile per la categoria.</summary>
    public static int RoundsForTier(string tier) => tier switch
    {
        "Formula / top tier" => 12,
        "Categoria avanzata" => 10,
        "Categoria regionale" => 8,
        _ => 6
    };

    /// <summary>
    /// Circuiti della stagione. La rotazione dipende dalla stagione, così due
    /// campionati consecutivi non hanno lo stesso calendario, e resta
    /// deterministica: la stessa carriera produce sempre lo stesso elenco.
    /// </summary>
    public static List<ContentTrackRecord> SelectSeasonTracks(IReadOnlyList<ContentTrackRecord> tracks, int count, int season)
    {
        if (tracks.Count == 0) return [];
        var ordered = tracks.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToList();
        // Meno circuiti che round: si torna sugli stessi, come fa qualunque
        // campionato minore vero.
        //
        // Il numero di round era limitato al numero di circuiti adatti alla
        // categoria. Nel kart, dove i tracciati compatibili sono due o tre, la
        // stagione durava due gare: finiva subito, portava una promozione di
        // livello, e nel collaudo il pilota arrivava al campionato mondiale
        // alla settima stagione con cinquantanove gare in tutto e ventidue
        // titoli. Una stagione corta non e' una stagione.
        if (ordered.Count < count)
        {
            var ripetuti = new List<ContentTrackRecord>();
            for (var i = 0; i < count; i++) ripetuti.Add(ordered[i % ordered.Count]);
            return ripetuti;
        }
        var wanted = Math.Clamp(count, 1, ordered.Count);
        var offset = ordered.Count == 0 ? 0 : Math.Abs(season - 1) * 3 % ordered.Count;
        var selected = new List<ContentTrackRecord>();
        for (var i = 0; i < wanted; i++) selected.Add(ordered[(offset + i) % ordered.Count]);
        return selected;
    }

    /// <summary>Prossimo appuntamento in programma, o null se l'agenda è vuota.</summary>
    public static ScheduledEvent? NextPlanned(IEnumerable<ScheduledEvent> schedule) =>
        schedule.Where(x => x.IsPlanned).OrderBy(x => x.Date).ThenBy(x => x.Round).FirstOrDefault();

    /// <summary>Round di campionato della stagione indicata, in ordine.</summary>
    public static List<ScheduledEvent> ChampionshipRounds(IEnumerable<ScheduledEvent> schedule, int season) =>
        schedule.Where(x => x.Kind == ScheduledEventKind.ChampionshipRound && x.Season == season)
            .OrderBy(x => x.Round).ToList();

    /// <summary>Aggiunge un appuntamento evitando i duplicati per identificativo.</summary>
    public static bool Append(List<ScheduledEvent> schedule, ScheduledEvent? item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Id)) return false;
        if (schedule.Any(x => x.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase))) return false;
        // Si corre nel fine settimana. Ogni generatore fissava la propria data
        // contando i giorni da oggi (+7, +14, +21…), quindi le gare cadevano di
        // martedì o di giovedì a seconda di quando il giocatore aveva premuto un
        // pulsante. Le prove restano invece infrasettimanali, come nella realtà:
        // un test privato non occupa un weekend di gara.
        if (item.Kind is ScheduledEventKind.Invitation or ScheduledEventKind.ChampionshipRound)
            item.Date = NarrativeCalendar.RaceWeekend(item.Date);
        schedule.Add(item);
        return true;
    }

    public static void Close(List<ScheduledEvent> schedule, string id, bool cancelled = false)
    {
        var item = schedule.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (item != null) item.Status = cancelled ? StatusCancelled : StatusDone;
    }

    public static string Describe(ScheduledEvent item)
        => $"{KindLabel(item)} · {TrackLabel(item)} · {NarrativeCalendar.Format(item.Date)}";

    /// <summary>
    /// Che cosa si corre, senza dove e senza quando.
    ///
    /// Serve dove circuito e data sono già scritti accanto — nell'area Oggi,
    /// per esempio, che li elenca come dati: ripeterli nella stessa riga
    /// faceva leggere tre volte la stessa informazione.
    /// </summary>
    public static string KindLabel(ScheduledEvent item) => item.Kind switch
    {
        ScheduledEventKind.EvaluationTest => "Prova di valutazione",
        ScheduledEventKind.ConfirmationTest => "Prova di conferma",
        ScheduledEventKind.Invitation => "Gara su invito",
        _ => $"Round {item.Round} di campionato"
    };

    /// <summary>Il circuito dell'appuntamento, con il codice come ripiego.</summary>
    public static string TrackLabel(ScheduledEvent item) =>
        string.IsNullOrWhiteSpace(item.TrackName) ? item.TrackId : item.TrackName;

    private static ContentTrackRecord? PickTrack(IReadOnlyList<ContentTrackRecord> tracks, int index)
    {
        if (tracks.Count == 0) return null;
        var ordered = tracks.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToList();
        return ordered[Math.Abs(index) % ordered.Count];
    }
}
