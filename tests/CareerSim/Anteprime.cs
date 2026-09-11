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

        log.WriteLine(new string('=', 78));
        log.WriteLine("ANTEPRIME DELLE SCHERMATE");
        log.WriteLine($"cartella: {cartella}");
        log.WriteLine("");

        void Scatta(string nome, Func<Form> costruisci)
        {
            try
            {
                using var form = costruisci();
                // Fuori dallo schermo: si apre davvero — serve perche' i
                // pannelli si dispongono solo quando la finestra ha un handle —
                // ma non compare a nessuno.
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-4000, -4000);
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
            Scatta(nome, () => new SponsorNegotiationDialog(visita, carriera, 6));
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

        // 4. L'epilogo della carriera.
        Scatta("epilogo", () => new CareerEpilogueDialog(
            CareerEpilogue.Compute(carriera, []),
            "Hai 39 anni e sono 4 stagioni che non vinci. Puoi continuare, ma la domanda è se ha ancora senso.",
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
