namespace CorsaCareer1991;

/// <summary>Come il pilota si presenta a un possibile sponsor.</summary>
public enum SponsorApproach
{
    /// <summary>I numeri: tempi, piazzamenti, quello che c'è a referto.</summary>
    Results,
    /// <summary>La visibilità: dove finirebbe il loro nome.</summary>
    Visibility,
    /// <summary>La persona: perché corre, dove vuole arrivare.</summary>
    Personal
}

/// <summary>Esito di una visita, con le parole di chi risponde.</summary>
public sealed class SponsorReply
{
    public bool Accepted { get; set; }
    public int Amount { get; set; }
    /// <summary>Cosa dice, con le sue parole.</summary>
    public string Line { get; set; } = "";
    /// <summary>Perché ha risposto così: il giocatore deve poter imparare.</summary>
    public string Reason { get; set; } = "";
}

/// <summary>
/// La caccia agli sponsor.
///
/// Non è un elenco da cui pescare: è un'attività che costa tempo e denaro e che
/// si può fare poche volte a settimana. Più lontano si va, più costa il viaggio
/// e meno visite entrano nella settimana — ma le aziende lontane sono anche
/// quelle con i budget seri.
///
/// La città di partenza è quella del pilota e le distanze si misurano da lì.
/// I nomi sono inventati: un programma che facesse dire a un'azienda reale di
/// sponsorizzare qualcuno starebbe inventando fatti su un soggetto vero.
/// </summary>
public static class SponsorHunt
{
    /// <summary>Quante visite si possono fare in una settimana, al massimo.</summary>
    public const int MaxVisitsPerWeek = 2;

    /// <summary>Oltre questa distanza il viaggio consuma l'intera settimana.</summary>
    public const int LongTripKm = 400;

    /// <summary>
    /// Le città, ordinate per lontananza dal punto di partenza tipico. La prima
    /// è la provincia da cui si comincia, non la capitale.
    /// </summary>
    public static readonly IReadOnlyList<string> Cities =
        ["Suzuka", "Nagoya", "Hamamatsu", "Shizuoka", "Yokohama", "Tokyo", "Sendai", "Sapporo"];

    /// <summary>
    /// Distanza di gioco in chilometri: serve a rendere il viaggio una
    /// decisione, non a riprodurre una mappa.
    /// </summary>
    public static int Distance(string from, string to)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return 0;
        return Math.Abs(IndexOf(from) - IndexOf(to)) * 120 + 40;
    }

    private static int IndexOf(string city)
    {
        for (var i = 0; i < Cities.Count; i++)
            if (string.Equals(Cities[i], city, StringComparison.OrdinalIgnoreCase)) return i;
        return 0;
    }

    /// <summary>Vero se il viaggio consuma l'intera disponibilità settimanale.</summary>
    public static bool IsLongTrip(int distanceKm) => distanceKm >= LongTripKm;

    /// <summary>
    /// Quante visite consuma un contatto: una in zona, due se è lontano. È il
    /// costo in tempo, distinto da quello in denaro.
    /// </summary>
    public static int VisitCost(int distanceKm) => IsLongTrip(distanceKm) ? 2 : 1;

    /// <summary>Costo del viaggio: cresce con la distanza.</summary>
    public static int TravelCostFor(int distanceKm) => distanceKm == 0 ? 0 : 15 + distanceKm / 8;

    // ------------------------------------------------------- i finanziatori

    private static readonly (string Name, string Sector, string Temperament, int Interest, int Contribution)[] Catalogue =
    [
        ("Ramen Kizuna", "trattoria di quartiere", "diffidente ma leale", 62, 90),
        ("Officina Tsubame", "riparazioni e saldature", "parla solo di lavoro", 55, 160),
        ("Cartoleria Hinode", "cartoleria e stampe", "curiosa, fa molte domande", 58, 110),
        ("Ferramenta Kuroda", "ferramenta di paese", "misura ogni parola", 48, 150),
        ("Panificio Asagiri", "panetteria mattutina", "generoso se si fida", 66, 100),
        ("Gomme Akatsuki", "pneumatici e assetti", "conosce le corse", 34, 320),
        ("Bevande Mikazuki", "distribuzione bevande", "cerca visibilità", 30, 380),
        ("Elettronica Habataki", "riparazione elettronica", "vuole capire il ritorno", 38, 260),
        ("Trasporti Nagareboshi", "piccoli trasporti", "abituato a trattare", 32, 340),
        ("Tintoria Yuunagi", "tintoria e lavanderia", "attenta all'immagine", 52, 130),
        ("Assicurazioni Tomoshibi", "agenzia assicurativa", "prudente per mestiere", 24, 460),
        ("Utensili Seiryuu", "utensili da lavoro", "diretto, poche parole", 44, 190),
        ("Fotografia Kagerou", "studio fotografico", "sensibile alle storie", 60, 120)
    ];

    /// <summary>
    /// I contatti iniziali per una carriera che parte dalla città indicata. I
    /// primi sono in zona e piccoli; i budget seri stanno lontano.
    /// </summary>
    public static List<SponsorProspect> SeedFor(string homeCity)
    {
        var home = string.IsNullOrWhiteSpace(homeCity) ? Cities[0] : homeCity;
        var prospects = new List<SponsorProspect>();

        // Cinque contatti: tre vicini, due che richiedono un viaggio. È la
        // proporzione che rende il viaggio una scelta e non un obbligo.
        var plan = new (int CatalogueIndex, int CityOffset)[]
        {
            (0, 0), (1, 0), (4, 1), (5, 3), (10, 5)
        };

        var homeIndex = IndexOf(home);
        for (var i = 0; i < plan.Length; i++)
        {
            var (index, offset) = plan[i];
            var entry = Catalogue[index % Catalogue.Length];
            var city = Cities[Math.Min(Cities.Count - 1, homeIndex + offset)];
            var distance = Distance(home, city);
            prospects.Add(new SponsorProspect
            {
                Name = entry.Name,
                Sector = entry.Sector,
                Temperament = entry.Temperament,
                City = city,
                DistanceKm = distance,
                TravelCost = TravelCostFor(distance),
                BaseInterest = entry.Interest,
                // Più lontano, più grosso: è il motivo per cui vale il viaggio.
                Contribution = entry.Contribution + distance * 2,
                DurationRounds = distance >= LongTripKm ? 3 : 1,
                ContactLevel = distance >= LongTripKm ? "motorsport" : "locale"
            });
        }
        return prospects;
    }

    // ------------------------------------------------------------- la visita

    /// <summary>
    /// L'esito di una visita. Non è un lancio di dadi: pesa quanto il pilota
    /// vale, se l'approccio scelto parla a quel carattere, e cosa ha davvero
    /// ottenuto in pista.
    /// </summary>
    public static SponsorReply Visit(SponsorProspect prospect, SponsorApproach approach, CareerState career)
    {
        var profile = career.ReputationProfile ?? new ReputationProfile();

        // L'interesse di base è quanto quell'attività è predisposta; l'appeal è
        // quanto vale il pilota. Servono entrambi.
        var score = prospect.BaseInterest / 2 + profile.SponsorAppeal / 2 - 30;

        // L'approccio giusto per quel carattere vale più di qualunque numero.
        var fitting = BestApproachFor(prospect.Temperament);
        score += approach == fitting ? 20 : -8;

        // Mostrare risultati che non esistono non funziona mai, per quanto
        // ben disposto sia l'interlocutore: non e' una penalita da compensare,
        // e' una risposta che deve restare no finche non si e' corso.
        if (approach == SponsorApproach.Results && career.Races == 0)
            return new SponsorReply
            {
                Accepted = false,
                Amount = 0,
                Line = "«Quali risultati? Non hai ancora corso. Torna quando hai qualcosa da farmi vedere.»",
                Reason = "Non c'e nessun risultato da mostrare: serve almeno una gara a referto."
            };
        if (approach == SponsorApproach.Results)
            score += career.Wins > 0 ? 14 : career.Podiums > 0 ? 7 : 0;
        if (approach == SponsorApproach.Visibility) score += profile.PublicPopularity / 4;
        if (approach == SponsorApproach.Personal) score += profile.Professionalism / 6;

        var accepted = score >= 8;
        return new SponsorReply
        {
            Accepted = accepted,
            Amount = accepted
                ? (int)Math.Round(prospect.Contribution * Math.Clamp(0.6 + score / 100.0, 0.6, 1.3))
                : 0,
            Line = accepted ? AcceptLine(prospect, approach) : RefuseLine(prospect, approach),
            Reason = accepted
                ? $"L'approccio era quello giusto per chi {prospect.Temperament}."
                : approach == fitting
                    ? $"L'approccio era giusto, ma serve più credito: interesse {prospect.BaseInterest}/100 e appeal {profile.SponsorAppeal}/100 non bastano."
                    : $"Con chi {prospect.Temperament}, «{Label(approach).ToLowerInvariant()}» non funziona."
        };
    }

    /// <summary>Quale approccio funziona con un certo carattere.</summary>
    public static SponsorApproach BestApproachFor(string temperament)
    {
        var t = (temperament ?? "").ToLowerInvariant();
        if (t.Contains("lavoro") || t.Contains("ritorno") || t.Contains("corse") || t.Contains("poche parole"))
            return SponsorApproach.Results;
        if (t.Contains("visibilità") || t.Contains("immagine") || t.Contains("tratta"))
            return SponsorApproach.Visibility;
        return SponsorApproach.Personal;
    }

    public static string Label(SponsorApproach approach) => approach switch
    {
        SponsorApproach.Results => "Mostra i risultati",
        SponsorApproach.Visibility => "Parla di visibilità",
        _ => "Racconta perché corri"
    };

    public static string Describe(SponsorApproach approach) => approach switch
    {
        SponsorApproach.Results => "Tempi, piazzamenti, quello che c'è a referto. Funziona con chi guarda i fatti.",
        SponsorApproach.Visibility => "Dove finirebbe il loro nome, e chi lo vedrebbe. Funziona con chi cerca clienti.",
        _ => "Perché corri e dove vuoi arrivare. Funziona con chi decide di pancia."
    };

    private static string AcceptLine(SponsorProspect prospect, SponsorApproach approach) => approach switch
    {
        SponsorApproach.Results =>
            "«I numeri li capisco. Non prometto molto, ma quello che metto lo metto davvero. Mandami una foto della vettura col nostro nome sopra.»",
        SponsorApproach.Visibility =>
            $"«{prospect.City} è piccola, la gente parla. Se ti vedono correre con noi sopra, qualcosa torna. Ci sto.»",
        _ =>
            "«Ho fatto anch'io qualcosa del genere, tanti anni fa. Non ci ho campato, ma non me ne sono pentito. Vai, ti diamo una mano.»"
    };

    private static string RefuseLine(SponsorProspect prospect, SponsorApproach approach) => approach switch
    {
        SponsorApproach.Results =>
            "«Con questi risultati non mi convinci. Torna quando c'è qualcosa scritto, e ne riparliamo.»",
        SponsorApproach.Visibility =>
            "«Visibilità dove? Sono quattro persone su una pista di provincia. Non è pubblicità, è beneficenza.»",
        _ =>
            $"«Bella storia. Ma qui si tira avanti con {prospect.Sector}, non coi sogni. Mi dispiace davvero.»"
    };
}
