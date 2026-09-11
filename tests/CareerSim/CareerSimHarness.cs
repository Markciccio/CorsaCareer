using System.Text;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Il punto d'ingresso del banco di prova.
///
/// Sta in un progetto suo perché deve poter aprire il portale senza mostrarlo,
/// ma compila gli stessi sorgenti del gioco: quello che qui va a buon fine è
/// andato a buon fine nel programma vero, non in una copia semplificata.
/// </summary>
internal static class CareerSimEntry
{
    [STAThread]
    private static int Main(string[] args)
    {
        // Nessuno può rispondere a una finestra modale: la guardia globale
        // trasforma gli avvisi in righe di registro.
        Environment.SetEnvironmentVariable("CORSACAREER_UI_AUTOMATION", "1");
        ApplicationConfiguration.Initialize();

        // Il collaudo dei contenuti non simula niente: controlla le scene, le
        // piante della citta' e le trattative. Sta qui perche' e' l'unico
        // progetto di prova rimasto nel repository.
        if (args.Any(x => x.Equals("--contenuti", StringComparison.OrdinalIgnoreCase)))
        {
            var problemi = ContenutiCheck.Esegui(Console.Out);
            Console.Out.Flush();
            return problemi;
        }

        // Le anteprime: apre le schermate fuori campo e le salva come PNG.
        // E' l'unico modo di accorgersi di un testo troncato senza che ci sia
        // una persona davanti allo schermo.
        if (args.Any(x => x.Equals("--anteprime", StringComparison.OrdinalIgnoreCase)))
        {
            var dove = ArgText(args, "--anteprime", "");
            if (string.IsNullOrWhiteSpace(dove) || dove.StartsWith("--"))
                dove = Path.Combine(Path.GetTempPath(), "CorsaCareerAnteprime");
            var esitoAnteprime = Anteprime.Esegui(dove, Console.Out);
            Console.Out.Flush();
            return esitoAnteprime;
        }

        var stagioni = ArgInt(args, "--stagioni", 8);
        var pilota = ArgText(args, "--pilota", "Sim");
        Environment.SetEnvironmentVariable("CORSACAREER_DEMO_DRIVER", pilota);

        // Ogni esecuzione parte da una carriera vuota: un residuo del giro
        // precedente falserebbe tutto quello che viene misurato dopo.
        // Una cartella indicata da fuori vince: serve a poter aprire poi il
        // portale sulla carriera appena simulata, invece di cercarla fra le
        // cartelle temporanee.
        var casa = ArgText(args, "--casa", "");
        if (string.IsNullOrWhiteSpace(casa))
            casa = Path.Combine(Path.GetTempPath(), "CorsaCareerSim", $"{pilota}-{DateTime.Now:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(casa);
        Environment.SetEnvironmentVariable("CORSACAREER_HOME", casa);
        Console.WriteLine($"carriera salvata in: {casa}");
        if (Environment.GetEnvironmentVariable("CORSACAREER_AC_ROOT") is not { Length: > 0 })
        {
            var finto = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "ac-finto"));
            if (Directory.Exists(finto)) Environment.SetEnvironmentVariable("CORSACAREER_AC_ROOT", finto);
        }

        using var portale = new MainForm();
        var esito = portale.RunCareerSimulation(stagioni, Console.Out);
        Console.Out.Flush();
        return esito;
    }

    private static int ArgInt(string[] args, string nome, int predefinito)
    {
        var testo = ArgText(args, nome, "");
        return int.TryParse(testo, out var valore) ? valore : predefinito;
    }

    private static string ArgText(string[] args, string nome, string predefinito)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i].Equals(nome, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return predefinito;
    }
}

/// <summary>
/// La carriera percorsa da sola.
///
/// È un altro file della stessa classe del portale, quindi chiama i metodi
/// veri — <c>LaunchTestSession</c>, <c>SignOffer</c>, <c>AcceptOpportunity</c>,
/// <c>AdvanceSeason</c> — invece di riscriverne una versione semplificata. Una
/// carriera che qui arriva in Formula 3 ci arriva anche giocando; una che qui
/// si blocca è bloccata anche per chi gioca.
///
/// Il giocatore simulato non bara: sceglie fra quello che il programma gli
/// mette davanti, con i soldi che ha, e usa le ore libere come le userebbe
/// qualcuno che vuole salire — allenamento quando è stanco, immagine quando
/// serve farsi notare, Haru sugli sponsor quando la cassa è bassa.
/// </summary>
public sealed partial class MainForm
{
    /// <summary>Quanti giri di decisione senza che la data avanzi prima di dichiarare un blocco.</summary>
    private const int MaxDecisioniPerGiorno = 40;

    private TextWriter simLog = TextWriter.Null;
    private readonly List<string> simAnomalie = [];
    private int simUltimoGradino;
    private string simUltimaCategoria = "";
    private int simAvvisiLetti;
    private int simCassaPrecedente = int.MinValue;

    /// <summary>
    /// Percorre la carriera per il numero di stagioni indicato e racconta cosa
    /// succede. Restituisce 0 se la carriera è arrivata almeno alla formula
    /// nazionale senza bloccarsi, 1 altrimenti.
    /// </summary>
    public int RunCareerSimulation(int stagioni, TextWriter log)
    {
        simLog = log;
        var limite = career.StoryDate.AddYears(stagioni);
        var giorniSenzaGara = 0;
        var decisioni = 0;
        var ultimaData = career.StoryDate;

        Intestazione();

        while (career.StoryDate < limite)
        {
            // Una carriera finisce: quando il pilota appende il casco al chiodo
            // il banco si ferma li', altrimenti misurerebbe stagioni che nella
            // realta' nessuno correrebbe.
            if (career.Retired) break;
            if (career.StoryDate != ultimaData) { decisioni = 0; ultimaData = career.StoryDate; }
            if (++decisioni > MaxDecisioniPerGiorno)
            {
                Anomalia($"BLOCCO: {MaxDecisioniPerGiorno} decisioni nello stesso giorno ({career.StoryDate:d MMM yyyy}) senza che il calendario avanzi.");
                break;
            }

            // 1. una sessione aperta viene prima di tutto: finché il referto non
            //    entra la carriera è ferma, esattamente come nel portale.
            if (awaitingResult)
            {
                var prima = career.Races;
                ExecuteSimulatedResult(SimulationBias.Natural, out _);
                if (awaitingResult)
                {
                    Anomalia("BLOCCO: la sessione preparata non produce nessun referto.");
                    break;
                }
                if (career.Races > prima) giorniSenzaGara = 0;
                RaccontaSessione();
                continue;
            }

            // 2. una selezione occupa giorni consecutivi e decide la categoria.
            if (OpenSelection is { } selezione)
            {
                if (selezione.NextDay is { } giornata)
                {
                    Riga($"selezione · {selezione.OrganisedBy} · giorno {giornata.Day} di {selezione.Days.Count}");
                    SimulateSelectionDay(selezione, giornata);
                }
                else CloseSelection(selezione);
                continue;
            }

            // 2b. il bivio delle discipline. Il portale lo chiede con una
            //     finestra, che qui non può aprirsi: il pilota simulato sceglie
            //     le monoposto e da lì non cambia più mestiere.
            ScegliStradaSeDovuto();

            // 3. il mercato prima di tutto: un contratto sul tavolo si firma
            //    subito, altrimenti si continua a correre da clienti paganti
            //    con un sedile che aspetta.
            if ((career.Offers?.Count ?? 0) > 0)
            {
                var migliore = career.Offers!.OrderByDescending(x => x.Prestige).ThenByDescending(x => x.Salary).First();
                var elenco = string.Join(" | ", career.Offers!.Select(x =>
                {
                    var g = contentIndex.Cars.FirstOrDefault(c => c.Id.Equals(x.Car, StringComparison.OrdinalIgnoreCase));
                    var r = g == null ? null : CareerLadder.ForCar(g.Category, g.PowerHp, g.MassKg);
                    return $"{UiText.Car(x.Car)}({x.Category}/{r?.Path.ToString() ?? "?"}/g{r?.Step ?? 0})";
                }));
                Riga($"firma · {migliore.Team} · {UiText.Car(migliore.Car)} · prestigio {migliore.Prestige} " +
                     $"[strada {(string.IsNullOrWhiteSpace(career.ChosenPath) ? "non scelta" : career.ChosenPath)} · sul tavolo: {elenco}]");
                SignOffer(migliore);
                continue;
            }

            // 4. l'appuntamento di oggi.
            var appuntamento = NextScheduled();
            if (appuntamento != null && appuntamento.Date.Date <= career.StoryDate.Date)
            {
                // Un lancio può finire in due modi diversi, entrambi legittimi:
                // la sessione resta aperta in attesa del referto (è quello che
                // succede con Content Manager installato, e nel giro dopo il
                // banco la risolve), oppure — senza Assetto Corsa — il
                // programma la risolve da sé prima di restituire il controllo.
                // Guardare solo «awaitingResult» faceva scambiare il secondo
                // caso per una carriera bloccata.
                var sessioniPrima = career.RaceHistory.Count + career.TestHistory.Count;
                var idAppuntamento = appuntamento.Id;
                LanciaAppuntamento(appuntamento);
                var risolta = career.RaceHistory.Count + career.TestHistory.Count > sessioniPrima;
                var chiuso = (career.Schedule ?? []).FirstOrDefault(x => x.Id == idAppuntamento)?.IsPlanned != true;
                if (!awaitingResult && !risolta && !chiuso)
                {
                    Anomalia($"BLOCCO: l'appuntamento «{CareerScheduler.Describe(appuntamento)}» non apre nessuna sessione e resta in agenda.");
                    // Toglierlo evita di ripetere all'infinito lo stesso passo:
                    // il blocco è già registrato.
                    appuntamento.Status = CareerScheduler.StatusCancelled;
                }
                else if (risolta) RaccontaSessione();
                continue;
            }

            // 5. le proposte del paddock.
            if (ScegliOpportunita() is { } proposta)
            {
                var costo = proposta.NetCost;
                if (AcceptOpportunity(proposta))
                    Riga($"accetta · {Opportunity.KindLabel(proposta.Kind)} · {proposta.Title} · costo € {costo:N0} · cassa € {career.Cash:N0}");
                else
                    proposta.Status = Opportunity.StatusDeclined;
                continue;
            }

            // 6. la stagione è finita.
            if (rounds.Count > 0 && career.Round >= rounds.Count)
            {
                var stagionePrima = career.Season;
                ChiudiStagione();
                if (career.Season == stagionePrima)
                {
                    Anomalia("BLOCCO: la stagione è finita ma non se ne apre un'altra.");
                    break;
                }
                continue;
            }

            // 7. giornata libera: si lavora su sé stessi.
            GiornataLibera();
            // La pausa invernale non è un blocco: fra novembre e marzo non si
            // corre, ed è giusto così. Una stagione che finisce presto e una che
            // comincia tardi possono lasciare otto mesi vuoti senza che niente
            // sia rotto: si segnala solo un fermo più lungo di così.
            if (++giorniSenzaGara == 270)
                Anomalia($"FERMO: 270 giorni senza correre (dal {career.StoryDate.AddDays(-270):d MMM yyyy}). " +
                         $"Cassa € {career.Cash:N0} · reputazione {career.Reputation} · fase {career.CareerPhase}.");
            AdvanceCalendar(1, untilNext: false);
        }

        return Resoconto();
    }

    /// <summary>
    /// La scelta del bivio, quella che nel portale è una finestra. Va fatta
    /// quando il pilota è ancora nel kart: è lì che una carriera decide se
    /// diventerà una carriera da monoposto o da vetture chiuse.
    /// </summary>
    private void ScegliStradaSeDovuto()
    {
        if (!string.IsNullOrWhiteSpace(career.ChosenPath)) return;
        if (career.Races < 3) return;
        if (CareerLadder.Current(career, contentIndex.Cars).Path != LadderPath.Karting) return;
        career.ChosenPath = LadderPath.SingleSeater.ToString();
        Riga($"** strada scelta: {CareerLadder.PathName(LadderPath.SingleSeater)} **");
    }

    // ------------------------------------------------------------------ passi

    private void LanciaAppuntamento(ScheduledEvent appuntamento)
    {
        var dove = CareerScheduler.TrackLabel(appuntamento);
        if (appuntamento.IsTest)
        {
            Riga($"test · {dove} · obiettivo {FormatLap(career.EvaluationTargetMilliseconds)}");
            LaunchTestSession();
        }
        else if (appuntamento.Kind == ScheduledEventKind.Invitation)
        {
            Riga($"gara su invito · {dove}");
            LaunchInvitation();
        }
        else
        {
            var inAgenda = CareerScheduler.ChampionshipRounds(career.Schedule ?? [], career.Season).Count;
            Riga($"round {appuntamento.Round} · {dove} · {career.Championship} " +
                 $"[contratto {(career.ContractActive ? "sì" : "no")} · cliente {(career.IsClientDriver ? "sì" : "no")} · " +
                 $"contatore {career.Round} · calendario {rounds.Count} · agenda {inAgenda} · stagione {career.Season} · " +
                 $"evento stagione {appuntamento.Season}]");
            LaunchWeekend();
        }
    }

    /// <summary>
    /// La proposta che un pilota con la testa sulle spalle accetterebbe: prima
    /// i sedili, poi i test, poi le gare, e mai una spesa che lascia la cassa
    /// senza margine per la quota successiva.
    /// </summary>
    private Opportunity? ScegliOpportunita()
    {
        var aperte = (career.Opportunities ?? []).Where(x => x.IsOpen && x.Date.Date >= career.StoryDate.Date).ToList();
        if (aperte.Count == 0) return null;

        // Uno sponsor non costa niente e paga: si prende sempre.
        var sponsor = aperte.FirstOrDefault(x => x.Kind == OpportunityKind.SponsorDeal);
        if (sponsor != null) return sponsor;

        var riserva = CareerFinances.SurvivalFloor;
        bool Sostenibile(Opportunity x) => x.NetCost <= Math.Max(0, career.Cash - riserva) || x.NetCost <= 0;

        // Il cambio di disciplina non è una promozione: il pilota simulato
        // resta sulla strada delle monoposto, altrimenti la carriera oscilla
        // e non si misura più niente.
        var candidate = aperte.Where(x => x.Kind != OpportunityKind.DisciplineSwitch && Sostenibile(x)).ToList();
        if (candidate.Count == 0) return null;

        int Priorita(Opportunity x) => x.Kind switch
        {
            OpportunityKind.ProfessionalSeat => 100,
            OpportunityKind.ChampionshipStepUp => 95,
            OpportunityKind.PartiallyFundedSeat => 90,
            OpportunityKind.PayDriverSeat => 80,
            OpportunityKind.FundedTest => 70,
            OpportunityKind.SubstituteDrive => 65,
            OpportunityKind.PaidTest => 60,
            OpportunityKind.EntryRace => 55,
            OpportunityKind.InvitationRace => 50,
            OpportunityKind.PromotionalEvent => 40,
            _ => 10
        };

        return candidate
            .OrderByDescending(Priorita)
            .ThenBy(x => x.NetCost)
            .ThenBy(x => x.Date)
            .First();
    }

    private void ChiudiStagione()
    {
        var stagione = career.Season;
        var vittorie = career.Wins;
        var podi = career.Podiums;
        var punti = career.Points;
        AdvanceSeason();
        Riga($"── fine stagione {stagione} · {punti} punti · {vittorie} vittorie · {podi} podi · " +
             $"cassa € {career.Cash:N0} · reputazione {career.Reputation} · {career.Championship} (livello {career.ChampionshipLevel})");
    }

    /// <summary>
    /// Le ore di una giornata senza gare. Non è un riempitivo: forma, immagine
    /// e sponsor sono i tre valori che decidono se il paddock si accorge del
    /// pilota, ed è qui che si costruiscono.
    /// </summary>
    private void GiornataLibera()
    {
        var giornata = DriverDay.EnsureToday(career);
        var driver = DayActivityCatalog.ForDriver().ToList();
        var agente = DayActivityCatalog.ForAgent().ToList();

        while (giornata.DriverHoursLeft > 0)
        {
            var scelta = ScegliAttivitaPilota(driver, giornata);
            if (scelta == null) break;
            var esito = DayEngine.Perform(career, giornata, scelta);
            if (esito.Refused) break;
        }

        while (giornata.AgentHoursLeft > 0)
        {
            // Haru cerca sponsor: quale porta bussare dipende da quanto pesa
            // il nome del pilota, non dal caso.
            var mira = career.Reputation >= 55 ? "haru-gomme" : career.Reputation >= 35 ? "haru-ricambi" : "haru-officina";
            var scelta = agente.FirstOrDefault(x => x.Id == mira && x.Hours <= giornata.AgentHoursLeft)
                         ?? agente.Where(x => x.Hours <= giornata.AgentHoursLeft).OrderBy(x => x.Hours).FirstOrDefault();
            if (scelta == null) break;
            var esito = DayEngine.Perform(career, giornata, scelta);
            if (esito.Refused) break;
        }
    }

    private DayActivity? ScegliAttivitaPilota(List<DayActivity> catalogo, DayPlan giornata)
    {
        DayActivity? PerId(string id) => catalogo.FirstOrDefault(x => x.Id == id && x.Hours <= giornata.DriverHoursLeft && !giornata.Done.Contains(x.Id));

        // Stanchezza alta: qualunque altra cosa renderebbe meno.
        if (career.Fatigue >= 55) return PerId("riposo") ?? PerId("corsa");
        // La forma è la base: sotto una certa soglia si allena e basta.
        if (career.Fitness < 70) return PerId("palestra") ?? PerId("corsa") ?? PerId("riposo");
        // Con la forma a posto si lavora sul nome — è quello che apre le porte
        // quando la classifica da sola non basta.
        return PerId("social") ?? PerId("instagram-allenamento") ?? PerId("domande-follower")
               ?? PerId("tifosi") ?? PerId("pr") ?? PerId("corsa") ?? PerId("riposo");
    }

    // ------------------------------------------------------------- resoconto

    private void Intestazione()
    {
        simLog.WriteLine("CORSACAREER — CARRIERA SIMULATA");
        simLog.WriteLine(new string('=', 78));
        simLog.WriteLine($"pilota: {career.Driver} · {career.Nationality} · talento {career.Talent.Archetype}");
        simLog.WriteLine($"contenuti: {contentIndex.Cars.Count} auto · {contentIndex.Tracks.Count} circuiti");
        simLog.WriteLine($"cassa iniziale: € {career.Cash:N0}");
        simLog.WriteLine("");
        simUltimoGradino = GradinoAttuale().Step;
        simUltimaCategoria = career.Championship;
    }

    private LadderRung GradinoAttuale() => CareerLadder.Current(career, contentIndex.Cars);

    private void Riga(string testo)
    {
        simLog.WriteLine($"{career.StoryDate:yyyy-MM-dd}  {testo}");
        DrenaAvvisi();
        ControllaCoerenza();
    }

    /// <summary>
    /// Gli avvisi che il programma avrebbe mostrato a schermo. Sono la voce del
    /// portale: quando un passo non parte, il motivo è quasi sempre scritto qui
    /// e senza leggerlo si vede solo che «non succede niente».
    /// </summary>
    private void DrenaAvvisi()
    {
        var avvisi = CareerMessages.Suppressed;
        while (simAvvisiLetti < avvisi.Count)
            simLog.WriteLine($"{career.StoryDate:yyyy-MM-dd}     · avviso: {avvisi[simAvvisiLetti++]}");
    }

    private void RaccontaSessione()
    {
        var ultima = career.RaceHistory.LastOrDefault();
        var test = career.TestHistory.LastOrDefault();
        if (ultima != null && ultima.StoryDate.Date == career.StoryDate.Date)
            Riga($"   → P{ultima.Position} · {ultima.Track} · " +
                 $"punti {career.Points} · cassa € {career.Cash:N0} · reputazione {career.Reputation}");
        else if (test != null)
            Riga($"   → test: {FormatLap(test.BestLapMilliseconds)} contro {FormatLap(career.EvaluationTargetMilliseconds)} · " +
                 $"{career.RookieEvaluationStatus} · cassa € {career.Cash:N0}");
    }

    /// <summary>
    /// I controlli che una carriera vera non può violare. Non sono gusti: sono
    /// gli stati che, se compaiono, rendono la carriera incoerente da leggere.
    /// </summary>
    private void ControllaCoerenza()
    {
        var gradino = GradinoAttuale();

        // Da dove arrivano i soldi: un salto grosso e improvviso va visto.
        if (simCassaPrecedente == int.MinValue) simCassaPrecedente = career.Cash;
        var salto = career.Cash - simCassaPrecedente;
        if (Math.Abs(salto) >= 20000)
            simLog.WriteLine($"{career.StoryDate:yyyy-MM-dd}     $ cassa {salto:+#,0;-#,0} → € {career.Cash:N0} " +
                             $"(premi {career.PrizeMoney:N0} · sponsor {career.SponsorMoney:N0} · budget sponsor {career.SponsorBudget:N0})");
        simCassaPrecedente = career.Cash;

        if (gradino.Step > simUltimoGradino + 1)
            Anomalia($"SALTO: da categoria {simUltimoGradino} a {gradino.Step} ({gradino.Name}) senza passare dai gradini in mezzo.");
        if (gradino.Step != simUltimoGradino)
        {
            // Scritto diretto: passare da Riga rientrerebbe qui dentro.
            simLog.WriteLine($"{career.StoryDate:yyyy-MM-dd}  ** categoria {simUltimoGradino} → {gradino.Step}: {gradino.Name} · {gradino.Tier} **");
            simUltimoGradino = gradino.Step;
        }
        if (!string.Equals(simUltimaCategoria, career.Championship, StringComparison.Ordinal))
        {
            simLog.WriteLine($"{career.StoryDate:yyyy-MM-dd}  ** campionato: {simUltimaCategoria} → {career.Championship} (livello {career.ChampionshipLevel}) **");
            simUltimaCategoria = career.Championship;
        }
        if (career.Cash < -1)
            Anomalia($"CASSA NEGATIVA: € {career.Cash:N0}. Il programma ha lasciato spendere denaro che non c'era.");
        if (career.Fitness is < 0 or > 100)
            Anomalia($"FORMA FUORI SCALA: {career.Fitness}.");
        if (career.Fatigue is < 0 or > 100)
            Anomalia($"STANCHEZZA FUORI SCALA: {career.Fatigue}.");
        if (career.Reputation is < 0 or > 100)
            Anomalia($"REPUTAZIONE FUORI SCALA: {career.Reputation}.");
        if (career.ContractActive && string.IsNullOrWhiteSpace(career.Car))
            Anomalia("CONTRATTO SENZA AUTO: risulta un sedile attivo ma nessuna vettura assegnata.");
    }

    private void Anomalia(string testo)
    {
        var riga = $"{career.StoryDate:yyyy-MM-dd}  !! {testo}";
        if (simAnomalie.Contains(testo)) return;
        simAnomalie.Add(testo);
        simLog.WriteLine(riga);
    }

    /// <summary>
    /// I controlli di verosimiglianza.
    ///
    /// «Plausibile» non è un'opinione: è un elenco di cose che in una carriera
    /// vera non succedono. Se una di queste scatta, la carriera prodotta dal
    /// programma non regge alla lettura, per quanto tutti i motori girino
    /// senza errori.
    /// </summary>
    private List<string> ControlliDiVerosimiglianza()
    {
        var problemi = new List<string>();
        var gradino = GradinoAttuale();
        var storico = career.RaceHistory ?? [];

        // 1. Si vince, ma non sempre. Un fenomeno chiude sotto il 45%: sopra,
        //    la vittoria smette di essere una notizia.
        if (career.Races >= 20)
        {
            var quota = career.Wins * 100.0 / Math.Max(1, career.Races);
            if (quota > 45)
                problemi.Add($"vittorie al {quota:0}% ({career.Wins} su {career.Races}): la vittoria non costa niente.");
        }

        // 2. Una stagione è fatta di una manciata di weekend, non di trenta.
        foreach (var stagione in storico.GroupBy(x => x.Season))
        {
            var gare = stagione.Count();
            // Una stagione di club nel kart e' densa: fino a una trentina di
            // domeniche e' ancora un calendario, sopra e' un tapis roulant.
            if (gare > 30) problemi.Add($"stagione {stagione.Key}: {gare} gare, non e' piu' un calendario.");
            if (gare is > 0 and < 4 && stagione.Key < career.Season)
                problemi.Add($"stagione {stagione.Key}: solo {gare} gare, non è una stagione.");
        }

        // 3. Non si corre a gennaio. Il calendario è quello dell'anno sportivo.
        var invernali = storico.Count(x => x.StoryDate.Month is 12 or 1 or 2);
        if (invernali > 0)
            problemi.Add($"{invernali} gare disputate fra dicembre e febbraio: la stagione non sta dentro l'anno sportivo.");

        // 4. La gavetta: un gradino non si lascia dopo due gare.
        foreach (var passo in storico
                     .Select(x => new
                     {
                         Gara = x,
                         Passo = PassoDellaVettura(x.Car)
                     })
                     .Where(x => x.Passo > 0)
                     .GroupBy(x => x.Passo))
        {
            var gare = passo.Count();
            if (gare < 8 && passo.Key < gradino.Step)
                problemi.Add($"categoria {passo.Key}: attraversata in {gare} gare, senza gavetta.");
        }

        // 5. I conti di un pilota, non di uno sponsor. Il tetto cresce con la
        //    categoria: in kart si sopravvive, in cima si guadagna.
        // Il tetto tiene conto di due cose: dove si corre — un pilota di
        // formula internazionale muove cifre che un kartista non vede — e da
        // quanto tempo si corre, perche' una carriera lunga accumula. Sopra
        // questa soglia il denaro smette di essere un vincolo e le scelte
        // economiche non contano piu' niente.
        // Tre voci: dove si corre, da quanto, e quanto si e' vinto. Un pilota
        // che vince porta premi e sponsor, ed e' giusto che abbia piu' mezzi di
        // uno che arriva sempre decimo — quello che non deve succedere e' che
        // il denaro diventi cosi' tanto da rendere ogni scelta indifferente.
        //
        // Il valore di una stagione dipende da dove la si corre: questo tetto
        // era tarato su carriere di poche annate e in kart, e a forza di
        // allungarle finiva per bocciare l'unica cosa che invece e' vera —
        // che ventiquattro stagioni in Formula 1 con un mondiale in bacheca
        // lasciano dei soldi. Il termine per stagione cresce quindi con la
        // categoria raggiunta: in kart una stagione vale poco piu' delle
        // spese, in cima vale un ingaggio.
        var valoreStagione = 60_000 + gradino.Step * 90_000;
        var tetto = gradino.Step * 250_000
                    + Math.Max(0, career.Season - 1) * valoreStagione
                    + career.Wins * 20_000;
        if (career.Cash > tetto)
            problemi.Add($"cassa € {career.Cash:N0} in categoria {gradino.Step}: oltre il verosimile (€ {tetto:N0}).");
        if (career.Cash < 0)
            problemi.Add($"cassa negativa: € {career.Cash:N0}.");

        // 6. Le prove: una per categoria, non un mestiere.
        var categorieToccate = storico.Select(x => PassoDellaVettura(x.Car)).Where(x => x > 0).Distinct().Count();
        if (career.TestHistory.Count > categorieToccate + 3)
            problemi.Add($"{career.TestHistory.Count} prove private per {categorieToccate} categorie: troppe.");

        // 7. Non si sale ogni anno per diritto: almeno una stagione deve
        //    chiudersi restando dov'era.
        if (career.SeasonArchive.Count >= 3 && career.SeasonArchive.All(x => x.FinalPosition == 1))
            problemi.Add($"{career.SeasonArchive.Count} stagioni chiuse tutte al primo posto: il campionato non oppone resistenza.");

        // 8. Il contratto deve chiedere qualcosa di attinente alla categoria
        //    che si corre.
        if (career.ContractActive && career.ContractObjective.Contains("kart", StringComparison.OrdinalIgnoreCase)
            && gradino.Path != LadderPath.Karting)
            problemi.Add($"obiettivo di contratto fermo al kart mentre si corre in {gradino.Name}: «{career.ContractObjective}».");

        return problemi;
    }

    /// <summary>Il gradino della vettura con cui è stata corsa una gara.</summary>
    private int PassoDellaVettura(string carId)
    {
        var auto = contentIndex.Cars.FirstOrDefault(x => x.Id.Equals(carId, StringComparison.OrdinalIgnoreCase));
        return auto == null ? 0 : CareerLadder.ForCar(auto.Category, auto.PowerHp, auto.MassKg).Step;
    }

    private int Resoconto()
    {
        var gradino = GradinoAttuale();
        simLog.WriteLine("");
        simLog.WriteLine(new string('=', 78));
        simLog.WriteLine("RESOCONTO");
        simLog.WriteLine($"  stagioni corse      {career.Season}");
        simLog.WriteLine($"  gare                {career.Races} · vittorie {career.Wins} · podi {career.Podiums}");
        simLog.WriteLine($"  test                {career.TestHistory.Count}");
        simLog.WriteLine($"  categoria raggiunta {gradino.Step} di {CareerLadder.Steps} — {gradino.Name}");
        simLog.WriteLine($"  campionato          {career.Championship} (livello {career.ChampionshipLevel})");
        simLog.WriteLine($"  squadra             {career.Team} · auto {UiText.Car(career.Car)}");
        simLog.WriteLine($"  cassa               € {career.Cash:N0}");
        // Tutti i totali sono di carriera: le voci di stagione si azzerano a
        // ogni annata, e mostrarle accanto alla cassa faceva sembrare che i
        // soldi arrivassero dal nulla.
        simLog.WriteLine($"    premi gara        € {career.LifetimePrizeMoney + career.PrizeMoney:N0}");
        simLog.WriteLine($"    sponsor           € {career.LifetimeSponsorMoney + career.SponsorMoney:N0}");
        simLog.WriteLine($"    stipendi          € {career.LifetimeSalary + career.SalaryPaid:N0}");
        simLog.WriteLine($"    quote pagate      € {career.EntryFeesPaid:N0}  ·  riparazioni € {career.LifetimeRepairCosts + career.RepairCosts:N0}  ·  trasferte € {career.LifetimeLogisticsCosts + career.LogisticsCosts:N0}");
        if (career.Retired)
            simLog.WriteLine($"  RITIRO              {career.RetiredOn:d MMM yyyy} — {career.RetirementReason}");
        simLog.WriteLine($"  reputazione {career.Reputation} · forma {career.Fitness} · seguito {career.Fanbase}");
        simLog.WriteLine("");

        var storia = new StringBuilder();
        foreach (var stagione in career.SeasonArchive)
            storia.AppendLine($"  stagione {stagione.Season}: {stagione.Championship} · {stagione.Points} punti · " +
                              $"{stagione.Wins} vittorie · posizione finale {stagione.FinalPosition}");
        if (storia.Length > 0) { simLog.WriteLine("ARCHIVIO STAGIONI"); simLog.Write(storia.ToString()); simLog.WriteLine(""); }

        if (simAnomalie.Count > 0)
        {
            simLog.WriteLine($"ANOMALIE ({simAnomalie.Count})");
            foreach (var anomalia in simAnomalie) simLog.WriteLine($"  · {anomalia}");
            simLog.WriteLine("");
        }

        var avvisi = CareerMessages.Suppressed;
        if (avvisi.Count > 0)
        {
            simLog.WriteLine($"AVVISI MOSTRATI AL GIOCATORE ({avvisi.Count}) — i primi 25");
            foreach (var avviso in avvisi.Take(25)) simLog.WriteLine($"  · {avviso}");
            simLog.WriteLine("");
        }

        var implausibili = ControlliDiVerosimiglianza();
        if (implausibili.Count > 0)
        {
            simLog.WriteLine($"NON VEROSIMILE ({implausibili.Count})");
            foreach (var problema in implausibili) simLog.WriteLine($"  · {problema}");
            simLog.WriteLine("");
        }

        var arrivato = gradino.Step >= 5;
        var pulita = simAnomalie.Count == 0 && implausibili.Count == 0;
        // Il traguardo va detto per quello che e': la frase era cablata sulla
        // formula nazionale, di quando la carriera non arrivava piu' in su, e
        // continuava a dirlo anche per una che chiude in Formula 1.
        var traguardo = career.Retired
            ? $"dal kart a {gradino.Name.ToLowerInvariant()}, fino al ritiro a {career.RetiredOn.Year - career.BirthYear} anni"
            : $"dal kart a {gradino.Name.ToLowerInvariant()}";
        simLog.WriteLine(arrivato && pulita
            ? $"ESITO: carriera plausibile — {traguardo}, senza incoerenze."
            : !arrivato
                ? $"ESITO: si ferma alla categoria {gradino.Step} ({gradino.Name})."
                : $"ESITO: arriva in fondo ma non è ancora verosimile: {simAnomalie.Count} incoerenze, {implausibili.Count} controlli non superati.");
        return arrivato && pulita ? 0 : 1;
    }
}
