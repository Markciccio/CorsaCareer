using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Le anteprime delle schermate, salvate come immagini.
///
/// Ricostruisce quello che faceva `tests/UiRender`, le cui fonti sono andate
/// perse nella pulizia della cartella e non erano mai state committate.
///
/// Serve per una ragione precisa: in WinForms i difetti veri non sono di
/// logica ma di impaginazione — testo troncato, pulsanti tagliati, pannelli
/// nell'ordine sbagliato, etichette che si sovrappongono — e nessun collaudo
/// che non guardi i pixel li puo' trovare. Una singola scheda di questo
/// progetto ne aveva sei, tutti emersi solo aprendola.
///
/// Ogni finestra viene aperta fuori dallo schermo, disegnata su una bitmap e
/// chiusa subito. Non serve nessuno davanti al computer.
/// </summary>
internal static class Anteprime
{
    public static int Esegui(string cartella, TextWriter log)
    {
        Directory.CreateDirectory(cartella);
        var fatte = 0;
        var problemi = new List<string>();

        // Le animazioni d'ingresso vanno spente PRIMA di aprire qualsiasi
        // finestra.
        //
        // CareerDialog fa entrare le schermate con una dissolvenza, e due di
        // esse con un sipario o un lampo: due bande che si ritirano lasciando
        // un filo rosso sul bordo. L'anteprima scattava a meta' transizione, e
        // il risultato era una fotografia con due righe rosse orizzontali in
        // mezzo alla schermata e l'opacita' a meta'. Cercandole nel codice
        // della schermata quelle righe non esistono: sono il sipario colto a
        // meta' strada. Uno strumento di verifica che mostra artefatti propri
        // fa perdere piu' tempo di quanto ne faccia risparmiare.
        Environment.SetEnvironmentVariable("CORSACAREER_NO_ANIM", "1");

        log.WriteLine(new string('=', 78));
        log.WriteLine("ANTEPRIME DELLE SCHERMATE");
        log.WriteLine($"cartella: {cartella}");
        log.WriteLine("");

        void Scatta(string nome, Func<Form> costruisci)
        {
            try
            {
                using var form = costruisci();
                // Fuori dallo schermo, e soprattutto NON massimizzata.
                //
                // CareerDialog massimizza ogni finestra, e una finestra
                // massimizzata ignora la posizione: le anteprime si aprivano a
                // tutto schermo sul monitor di chi stava lavorando, una dopo
                // l'altra, ventisette volte. Si riporta a dimensione normale e
                // le si da' a mano la misura che avrebbe da massimizzata, cosi'
                // l'impaginazione e' quella vera ma non la vede nessuno.
                var schermo = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1040);
                form.WindowState = FormWindowState.Normal;
                form.StartPosition = FormStartPosition.Manual;
                form.MinimumSize = new Size(200, 200);
                form.Size = new Size(schermo.Width, schermo.Height);
                form.Location = new Point(-schermo.Width - 200, -schermo.Height - 200);
                form.ShowInTaskbar = false;
                form.Show();
                Application.DoEvents();
                form.PerformLayout();
                Application.DoEvents();

                using var bitmap = new Bitmap(form.Width, form.Height);
                form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
                var percorso = Path.Combine(cartella, nome + ".png");
                bitmap.Save(percorso, ImageFormat.Png);
                form.Hide();
                form.Close();
                Application.DoEvents();

                fatte++;
                log.WriteLine($"  {nome,-34} {form.Width}x{form.Height}");
            }
            catch (Exception errore)
            {
                problemi.Add($"{nome}: {errore.GetType().Name} — {errore.Message}");
                log.WriteLine($"  {nome,-34} NON RESA: {errore.Message}");
            }
        }

        var carriera = CarrieraDiProva();
        var fatti = CapetaScenes.Leggi(carriera, [], []);

        // 1. La camminata: due piante diverse, perche' e' generata e va vista
        //    piu' di una volta.
        Scatta("citta-1", () => new TownWalkDialog(TownMap.Genera(1, "Officina Doppino"), "Haru", 70));
        Scatta("citta-2", () => new TownWalkDialog(TownMap.Genera(77, "Gomme Akatsuki"), "Haru", 70));

        // 2. La trattativa, per ognuno dei quattro tipi di interlocutore: i
        //    testi hanno lunghezze molto diverse ed e' li' che si tronca.
        foreach (var (nome, mestiere) in new[]
                 {
                     ("trattativa-intenditore", "pneumatici e assetti"),
                     ("trattativa-commerciante", "distribuzione bevande"),
                     ("trattativa-paese", "ferramenta di paese"),
                     ("trattativa-duro", "agenzia assicurativa")
                 })
        {
            var visita = new SponsorVisit
            {
                Id = "anteprima", Target = "Assicurazioni Tomoshibi", Trade = mestiere,
                Amount = 4600, Chance = 42, Pitch = "«Proviamo.»"
            };
            Scatta(nome, () => new SponsorNegotiationDialog(visita, carriera));
        }

        // 3. Le scene scritte. Tutte e venti: sono la parte con piu' testo e
        //    quella dove un balloon puo' non starci.
        foreach (var momento in Enum.GetValues<MomentoDiCarriera>())
        {
            var battute = CapetaScenes.Scena(momento, fatti);
            if (battute.Count == 0) continue;
            Scatta("scena-" + momento.ToString().ToLowerInvariant(),
                () => new AnimeDialogueDialog(CapetaScenes.Titolo(momento), battute));
        }

        // 4. Le schermate arrivate con l'aggiornamento di Codex: la mappa
        //    della carriera sui contenuti installati, la scelta dell'auto
        //    equivalente e lo stacco fra una scena e il portale. Nessuna di
        //    queste e' mai stata vista da nessuno.
        var indice = new ContentIndexRecord
        {
            AssettoCorsaRoot = AssetPaths.Root,
            Cars =
            [
                new() { Id = "kart_4t", Name = "Kart Rental 4T", Category = "kart", PowerHp = 9, MassKg = 140 },
                new() { Id = "kart_125", Name = "Kart KZ 125", Category = "kart", PowerHp = 48, MassKg = 165 },
                new() { Id = "formula_4", Name = "Formula 4", Category = "formula 4", PowerHp = 160, MassKg = 570 },
                new() { Id = "formula_1", Name = "Formula 1", Category = "formula 1", PowerHp = 1000, MassKg = 798 },
                new() { Id = "civic", Name = "Civic Type R", Category = "touring", PowerHp = 320, MassKg = 1380 }
            ]
        };
        Scatta("mappa-contenuti", () => new InstalledCareerAnalysisDialog(indice, indice.Cars[0]));
        Scatta("mappa-contenuti-sola-lettura", () => new InstalledCareerAnalysisDialog(indice, indice.Cars[0], "SingleSeater", soloLettura: true));

        // 5. La giornata del pilota, il pannello che cambia piu' spesso di
        //    tutti: un ragazzo di dodici anni, un giorno feriale con scuola,
        //    con un impegno gia' fissato e delle ore ancora libere.
        var studente = CarrieraStudente();
        Scatta("giornata-feriale", () => new DailyAgendaDialog(studente, indice, () => { }, () => { }));
        var ripetente = CarrieraStudente();
        ripetente.RepeatingYear = true;
        ripetente.SchoolPerformance = 18;
        Scatta("giornata-ripetente-a-rischio", () => new DailyAgendaDialog(ripetente, indice, () => { }, () => { }));
        Scatta("auto-equivalente", () => new EquivalentCarPickerDialog(
            indice.Cars, "Formula nazionale", CareerLadder.Rungs.First(x => x.Step == 5)));
        Scatta("stacco", () => new PortalTransitionOverlay(
            "Campionato nazionale", "Stagione 4 · dodici gare in calendario"));

        // 5. L'epilogo della carriera.
        // L'epilogo va costruito con i contenuti veri e con un motivo di
        // ritiro coerente con il pilota del fixture.
        //
        // Prima riceveva una lista auto VUOTA — quindi «categoria piu' alta»
        // usciva sempre «—», che sembrava un difetto della schermata — e un
        // motivo di ritiro scritto a mano che diceva «hai 39 anni» mentre il
        // sommario sopra, calcolato dai dati veri, diceva «si ritira a 26
        // anni». Tre incoerenze in una fotografia sola, tutte e tre
        // dell'anteprima e nessuna del gioco: esattamente il tipo di falso
        // allarme che manda a cercare bug dove non ci sono.
        var epilogo = CareerEpilogue.Compute(carriera, indice.Cars);
        Scatta("epilogo", () => new CareerEpilogueDialog(
            epilogo,
            $"Hai {epilogo.EtaAlRitiro} anni e {epilogo.Gare} gare in carriera. "
            + "Puoi continuare, ma la domanda è se ha ancora senso.",
            ""));

        log.WriteLine("");
        log.WriteLine($"{fatte} anteprime salvate.");
        if (problemi.Count > 0)
        {
            log.WriteLine($"PROBLEMI ({problemi.Count})");
            foreach (var p in problemi) log.WriteLine($"  · {p}");
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Un pilota di dodici anni, in un giorno feriale qualunque: e' lo stato
    /// su cui vive il pannello della giornata, che e' cambiato piu' di ogni
    /// altra schermata in questa fase e non era mai stato visto in anteprima.
    /// </summary>
    private static CareerState CarrieraStudente()
    {
        var c = new CareerState
        {
            Driver = "Akira Sanada",
            Team = "",
            Championship = "Campionato di zona",
            ChampionshipLevel = 1,
            Car = "kart_4t",
            Tier = "Rookie",
            Sponsor = "",
            StoryDate = new DateTime(2003, 5, 6), // martedi'
            CareerStart = new DateTime(2003, 1, 11),
            BirthYear = 1991,
            Season = 1,
            Races = 8,
            Cash = 4200,
            Fitness = 55,
            Fatigue = 30,
            SchoolPerformance = 62
        };
        c.ReputationProfile = new ReputationProfile { PublicPopularity = 12 };
        return c;
    }

    private static CareerState CarrieraDiProva()
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
        c.RaceHistory.Add(new RaceHistoryEntry
        {
            Season = 11, Round = 1, Track = "suzuka", Car = "formula_1",
            Position = 2, Points = 18, Laps = 53, StoryDate = new DateTime(2015, 4, 12)
        });
        return c;
    }
}
