using System.Text;

namespace CorsaCareer;

/// <summary>
/// Il collaudo dei contenuti nuovi: scene scritte, cittadina, trattative.
///
/// Serve perché il banco della carriera non li tocca. `CareerSim` sceglie le
/// attività della giornata per identificativo e non guarda mai il catalogo
/// intero, quindi aggiungere quattro attività scolastiche non cambia di una
/// virgola la simulazione — l'ho verificato, il resoconto è rimasto identico
/// riga per riga. È il comportamento giusto (il banco misura il modello di
/// carriera, non i contenuti), ma lascia le cose nuove senza nessuna verifica.
///
/// Qui si controlla quello che si può controllare senza aprire finestre: che
/// ogni scena produca battute vere e non stringhe vuote o segnaposto, che ogni
/// pianta della città sia davvero attraversabile — una mappa con il negozio
/// dietro il canale e nessun ponte renderebbe la visita impossibile — e che
/// nessuna risposta della trattativa sia inerte per tutti e quattro i tipi di
/// interlocutore.
/// </summary>
internal static class ContenutiCheck
{
    public static int Esegui(TextWriter log)
    {
        var problemi = new List<string>();

        log.WriteLine(new string('=', 78));
        log.WriteLine("COLLAUDO DEI CONTENUTI");
        log.WriteLine("");

        problemi.AddRange(ControllaScene(log));
        problemi.AddRange(ControllaCitta(log));
        problemi.AddRange(ControllaTrattative(log));
        problemi.AddRange(ControllaAttivita(log));

        log.WriteLine("");
        if (problemi.Count == 0)
        {
            log.WriteLine("ESITO: contenuti a posto.");
            return 0;
        }
        log.WriteLine($"PROBLEMI ({problemi.Count})");
        foreach (var p in problemi) log.WriteLine($"  · {p}");
        return 1;
    }

    // ------------------------------------------------------------- le scene

    private static List<string> ControllaScene(TextWriter log)
    {
        var problemi = new List<string>();

        // Due carriere agli antipodi: una all'inizio, senza niente — nessuna
        // gara, nessuna classifica, nessuno sponsor — e una matura. È la prima
        // che conta: è lì che una scena può citare un dato che non esiste.
        var carriere = new (string Nome, CareerState Stato)[]
        {
            ("appena cominciata", Vuota()),
            ("matura", Matura())
        };

        var momenti = Enum.GetValues<MomentoDiCarriera>();
        foreach (var (nome, stato) in carriere)
        {
            var fatti = CapetaScenes.Leggi(stato, [], []);
            var battuteTotali = 0;
            foreach (var momento in momenti)
            {
                var scena = CapetaScenes.Scena(momento, fatti);
                if (scena.Count == 0) { problemi.Add($"{momento} ({nome}): nessuna battuta."); continue; }
                battuteTotali += scena.Count;

                foreach (var riga in scena)
                {
                    if (string.IsNullOrWhiteSpace(riga.Speaker))
                        problemi.Add($"{momento} ({nome}): una battuta senza chi la dice.");
                    if (string.IsNullOrWhiteSpace(riga.Text))
                        problemi.Add($"{momento} ({nome}): battuta vuota di {riga.Speaker}.");
                    // I segnaposto sono il modo in cui un dato mancante finisce
                    // a schermo senza che nessuno se ne accorga.
                    foreach (var sospetto in new[] { "{", "}", "NaN", "€ 0 ", "P0", "posizione 0" })
                        if (riga.Text.Contains(sospetto, StringComparison.Ordinal))
                            problemi.Add($"{momento} ({nome}): «{sospetto}» nel testo di {riga.Speaker}.");
                }
            }
            log.WriteLine($"  scene · carriera {nome}: {momenti.Length} momenti, {battuteTotali} battute");
        }

        // Il titolo della finestra deve esistere per ogni momento.
        foreach (var momento in momenti)
            if (string.IsNullOrWhiteSpace(CapetaScenes.Titolo(momento)))
                problemi.Add($"{momento}: nessun titolo di finestra.");

        return problemi;
    }

    // ------------------------------------------------------------ la citta

    private static List<string> ControllaCitta(TextWriter log)
    {
        var problemi = new List<string>();
        var distanze = new List<int>();

        // Cento piante diverse: se anche una sola non è attraversabile, prima
        // o poi capita a qualcuno e la visita diventa impossibile.
        for (var i = 0; i < 100; i++)
        {
            var mappa = TownMap.Genera(StableHash.Of("collaudo", i), $"Negozio {i}");
            var d = mappa.DistanzaMinima();
            if (d < 0) { problemi.Add($"pianta {i}: il negozio non è raggiungibile a piedi."); continue; }
            if (d < 8) problemi.Add($"pianta {i}: il negozio è a {d} caselle, troppo vicino perché il tempo conti.");
            distanze.Add(d);

            if (!mappa.Calpestabile(mappa.Partenza.X, mappa.Partenza.Y, true))
                problemi.Add($"pianta {i}: si parte dentro un muro.");
            if (!mappa.Calpestabile(mappa.Destinazione.X, mappa.Destinazione.Y, true))
                problemi.Add($"pianta {i}: il negozio è dentro un muro.");
        }

        // La stessa pianta due volte deve essere identica: è quello che rende
        // onesto il cronometro.
        var a = TownMap.Genera(12345, "Prova");
        var b = TownMap.Genera(12345, "Prova");
        if (a.Partenza != b.Partenza || a.Destinazione != b.Destinazione || a.DistanzaMinima() != b.DistanzaMinima())
            problemi.Add("lo stesso seme produce due città diverse: il tempo concesso non sarebbe ripetibile.");

        if (distanze.Count > 0)
            log.WriteLine($"  città  · 100 piante · percorso da {distanze.Min()} a {distanze.Max()} caselle, media {distanze.Average():F0}");

        return problemi;
    }

    // -------------------------------------------------------- le trattative

    private static List<string> ControllaTrattative(TextWriter log)
    {
        var problemi = new List<string>();
        var carriera = Matura();
        var doti = new AgentSkills();

        var visita = new SponsorVisit
        {
            Id = "collaudo", Target = "Prova", Trade = "ferramenta di paese",
            Amount = 1000, Chance = 50, Pitch = "«Proviamo.»"
        };

        var scambi = SponsorNegotiation.Scambi(visita, carriera);
        if (scambi.Count < 3) problemi.Add($"la trattativa ha solo {scambi.Count} domande.");

        foreach (var tipo in Enum.GetValues<SponsorTemperamento>())
        {
            if (string.IsNullOrWhiteSpace(SponsorNegotiation.ComeSiPresenta(tipo)))
                problemi.Add($"{tipo}: non è descritto, quindi il giocatore sceglie al buio.");

            var migliore = 0; var peggiore = 0;
            foreach (var scambio in scambi)
            {
                var esiti = scambio.Mosse.Select(m => SponsorNegotiation.Valuta(m, tipo, doti).Delta).ToList();
                migliore += esiti.Max();
                peggiore += esiti.Min();
                if (esiti.All(x => x == esiti[0]))
                    problemi.Add($"{tipo}: in una domanda le tre risposte valgono tutte {esiti[0]}, quindi scegliere non serve.");
                foreach (var m in scambio.Mosse)
                {
                    if (string.IsNullOrWhiteSpace(m.Etichetta)) problemi.Add($"{tipo}: una risposta senza etichetta.");
                    if (string.IsNullOrWhiteSpace(SponsorNegotiation.Valuta(m, tipo, doti).Reazione))
                        problemi.Add($"{tipo}: una risposta senza reazione.");
                }
            }

            var alto = SponsorNegotiation.ProbabilitaFinale(visita.Chance, 6, migliore);
            var basso = SponsorNegotiation.ProbabilitaFinale(visita.Chance, -30, peggiore);
            if (alto <= basso) problemi.Add($"{tipo}: giocare bene non conviene ({alto}% contro {basso}%).");
            if (alto >= 100 || basso <= 0) problemi.Add($"{tipo}: la trattativa produce una certezza ({basso}%–{alto}%).");
            log.WriteLine($"  tavolo · {SponsorNegotiation.NomeTemperamento(tipo),-26} da {basso}% a {alto}%");
        }

        // Il mestiere deve sempre dare un tipo, anche se non lo conosciamo.
        foreach (var mestiere in new[] { "", "qualcosa di ignoto", "pneumatici e assetti", "distribuzione bevande", "agenzia assicurativa" })
            _ = SponsorNegotiation.TemperamentoDi(mestiere);

        return problemi;
    }

    // --------------------------------------------------------- le attivita

    private static List<string> ControllaAttivita(TextWriter log)
    {
        var problemi = new List<string>();
        var tutte = DayActivityCatalog.All().ToList();

        foreach (var gruppo in tutte.GroupBy(x => x.Id))
            if (gruppo.Count() > 1) problemi.Add($"attività «{gruppo.Key}» dichiarata {gruppo.Count()} volte.");

        foreach (var a in tutte)
        {
            if (a.Hours <= 0) problemi.Add($"attività «{a.Id}»: {a.Hours} ore.");
            if (string.IsNullOrWhiteSpace(a.Promise)) problemi.Add($"attività «{a.Id}»: non dichiara cosa fa.");
            if (a.Outcomes.Count == 0) problemi.Add($"attività «{a.Id}»: nessun esito.");
            foreach (var e in a.Outcomes)
                if (string.IsNullOrWhiteSpace(e.Line)) problemi.Add($"attività «{a.Id}»: un esito senza racconto.");
        }

        // L'elenco delle scolastiche e' dichiarato nel catalogo, non dedotto
        // dal prefisso: «scuola-kart» e' una giornata al kartodromo.
        var scuola = tutte.Where(x => DayActivityCatalog.AScuola(x.Id)).ToList();
        if (scuola.Count != DayActivityCatalog.Scolastiche.Count)
            problemi.Add($"{DayActivityCatalog.Scolastiche.Count} attività scolastiche dichiarate ma {scuola.Count} nel catalogo.");
        log.WriteLine($"  giorno · {tutte.Count} attività, di cui {scuola.Count} a scuola");

        return problemi;
    }

    // ------------------------------------------------------------ le carriere

    private static CareerState Vuota() => new()
    {
        Driver = "Akira Sanada",
        StoryDate = new DateTime(2005, 3, 1),
        CareerStart = new DateTime(2005, 3, 1),
        BirthYear = 1989,
        Season = 1,
        Cash = 900
    };

    private static CareerState Matura()
    {
        var c = new CareerState
        {
            Driver = "Akira Sanada",
            Team = "Orion Night Racing",
            Championship = "Campionato mondiale",
            ChampionshipLevel = 5,
            Car = "formula_1",
            Tier = "Formula / top tier",
            Sponsor = "Gomme Akatsuki",
            StoryDate = new DateTime(2015, 7, 1),
            CareerStart = new DateTime(2005, 3, 1),
            BirthYear = 1989,
            Season = 11,
            Races = 140,
            Wins = 12,
            Podiums = 48,
            Points = 180,
            Cash = 450_000,
            Fitness = 82
        };
        c.ReputationProfile = new ReputationProfile { PublicPopularity = 64 };
        c.Standings.Add(new StandingEntry { Driver = "Akira Sanada", Points = 180, Wins = 3 });
        c.Standings.Add(new StandingEntry { Driver = "Riku Hayase", Points = 205, Wins = 5 });
        c.Standings.Add(new StandingEntry { Driver = "Sota Fujimoto", Points = 150, Wins = 1 });
        return c;
    }
}
