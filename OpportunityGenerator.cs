namespace CorsaCareer1991;

/// <summary>Fotografia dello stato su cui il generatore decide cosa proporre.</summary>
public sealed class OpportunityContext
{
    public string Driver { get; set; } = "";
    public int Cash { get; set; }
    public ReputationProfile Reputation { get; set; } = new();
    public string Tier { get; set; } = "Rookie";
    public string Championship { get; set; } = "";
    public bool HasActiveContract { get; set; }
    public string CurrentTeam { get; set; } = "";
    public int Season { get; set; } = 1;
    public DateTime Today { get; set; } = NarrativeCalendar.DefaultSeasonStart;
    public int Races { get; set; }

    /// <summary>
    /// Vero se fra le proposte aperte c'e gia una gara. Serve a garantirle un
    /// posto senza generarne una seconda quando ce n'e gia una da correre.
    /// </summary>
    public bool HasOpenRace { get; set; }

    /// <summary>
    /// Vero se in agenda c'e' gia' un round di campionato da correre.
    ///
    /// La gara open e' la rete di sicurezza di chi non ha un sedile. A chi ce
    /// l'ha non va proposta: al banco quarantasette gare su sessantuno erano
    /// gare open comprate una alla volta, il campionato non arrivava mai in
    /// fondo e la stagione non si chiudeva — tre anni di carriera senza un
    /// titolo, senza una promozione e con la stagione ferma a uno.
    /// </summary>
    public bool HasPlannedRound { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    /// <summary>Ultimi piazzamenti reali, dal più recente. 0 = ritiro.</summary>
    public List<int> RecentPositions { get; set; } = [];
    public int RecentFieldSize { get; set; } = 12;
    public string LastTrackName { get; set; } = "";
    public string LastWeather { get; set; } = "";
    public int LastPosition { get; set; }
    public int PointlessStreak { get; set; }
    public int TeamRelation { get; set; } = 40;
    public int SponsorRelation { get; set; } = 40;

    /// <summary>
    /// Forma fisica del pilota. Serve al salto di campionato: nessuno porta in
    /// una categoria superiore chi non regge la distanza.
    /// </summary>
    public int Fitness { get; set; } = 30;

    /// <summary>Livello di campionato attuale, per capire se c'è un gradino sopra.</summary>
    public int ChampionshipLevel { get; set; } = 1;

    /// <summary>
    /// Gradino di carriera della vettura attuale (1..7). I «tier» sono quattro
    /// per sette gradini, quindi dentro lo stesso tier convivono un kart con
    /// cambio e una Formula 4: ragionando per tier si poteva passare dal kart
    /// a una monoposto di vertice in due offerte. Serve a non far saltare più
    /// di un gradino alla volta.
    /// </summary>
    public int LadderStep { get; set; }

    /// <summary>
    /// Gare corse da quando si è salito di campionato l'ultima volta. Un salto
    /// per conoscenza è un'occasione rara: senza questa distanza minima nel
    /// banco arrivavano quattro promozioni di fila e si toccava il mondiale
    /// con tre vittorie in carriera.
    /// </summary>
    public int RacesSinceLevelUp { get; set; } = 999;
    /// <summary>
    /// Le gare gia' corse sul gradino attuale della scala.
    ///
    /// Senza questo dato un sedile di categoria superiore poteva arrivare dopo
    /// quattro gare in tutto: nel banco i piloti lasciavano il kart a tre mesi
    /// dall'esordio e passavano il resto della carriera in monoposto. La
    /// gavetta e' quello che rende credibile il salto.
    /// </summary>
    public int RacesAtStep { get; set; }

    /// <summary>La vettura su cui si corre adesso: serve a sapere in che disciplina si e'.</summary>
    public string CurrentCarId { get; set; } = "";

    /// <summary>
    /// Vittorie consecutive fino all'ultima gara. E' il segnale che fa alzare
    /// il telefono a una squadra di un campionato superiore, anche a stagione
    /// in corso: nessuno aspetta la classifica finale per prendersi uno che sta
    /// vincendo tutto.
    /// </summary>
    public int VittorieDiFila { get; set; }

    /// <summary>Podi consecutivi: la costanza vale quanto una serie di vittorie.</summary>
    public int PodiDiFila { get; set; }

    /// <summary>Posizioni guadagnate nella migliore rimonta recente.</summary>
    public int MigliorRimontaRecente { get; set; }
    /// <summary>
    /// Lo sponsor attuale, se esiste. Serve a non riproporre una sponsorizzazione
    /// a chi ne ha gia una: senza questo dato il generatore ne creava una nuova a
    /// ogni ciclo e il pilota poteva incassarle in serie, azzerando il vincolo
    /// economico su cui si regge ogni decisione.
    /// </summary>
    public string CurrentSponsor { get; set; } = "";
    /// <summary>L'interesse dello sponsor al momento della firma: un nuovo contratto deve valere di piu.</summary>
    public int CurrentSponsorAppeal { get; set; }

    /// <summary>
    /// Vero se il nome indica uno sponsor reale. La carriera parte con il
    /// segnaposto "In attesa di sponsor", che non deve valere come contratto.
    /// </summary>
    public static bool HasSponsor(string? name) =>
        !string.IsNullOrWhiteSpace(name) && !name.StartsWith("In attesa", StringComparison.OrdinalIgnoreCase);
    /// <summary>Categorie realmente disponibili fra i contenuti installati.</summary>
    /// <summary>
    /// Quante prove private sono già state fatte con la vettura attuale.
    ///
    /// Serve a concedere il test d'ingresso in una categoria una volta sola:
    /// preso il mezzo in mano, quello che resta da fare è correre.
    /// </summary>
    public int TestsWithCurrentCar { get; set; }

    /// <summary>
    /// La strada scelta al bivio dopo il kart: monoposto, durata o turismo.
    ///
    /// Vuota finché il pilota non ha scelto. Era un dato salvato e mai letto:
    /// chi sceglieva le monoposto riceveva comunque un trofeo turismo e poi
    /// una GT3, cioè tre discipline in due stagioni. Una carriera vera ne
    /// percorre una, e cambiarla è una decisione — non il risultato di quale
    /// vettura era meno potente fra quelle installate.
    /// </summary>
    public string ChosenPath { get; set; } = "";

    /// <summary>La strada scelta, quando è stata scelta davvero.</summary>
    public LadderPath? Path =>
        Enum.TryParse<LadderPath>(ChosenPath, ignoreCase: true, out var valore) ? valore : null;

    public List<string> AvailableTiers { get; set; } = [];
    public List<ContentTrackRecord> Tracks { get; set; } = [];
    public List<ContentCarRecord> Cars { get; set; } = [];
    public int OpenOpportunities { get; set; }
}

/// <summary>
/// Genera le opportunità dallo stato reale della carriera.
///
/// Prima le offerte venivano da <c>BuildOffers()</c>: tre voci con nomi di team
/// cablati, sempre le stesse, più una offerta aggiunta ogni due gare a
/// prescindere dai risultati. Non c'era alcun motivo per cui un team dovesse
/// interessarsi al pilota, e nessun modo di perdere un'occasione.
///
/// Qui ogni proposta ha requisiti verificabili su risultati, reputazione,
/// denaro, categoria e rapporti, e porta con sé la ragione per cui è arrivata.
/// Il generatore è deterministico: lo stesso stato produce le stesse proposte.
/// </summary>
public static class OpportunityGenerator
{
    /// <summary>Massimo di proposte contemporaneamente aperte: l'agenda deve restare leggibile.</summary>
    public const int MaxOpenOpportunities = 6;
    public const int DefaultValidityDays = 21;

    public static List<Opportunity> Generate(OpportunityContext context)
    {
        var results = new List<Opportunity>();
        var seed = StableSeed(context);
        var reputation = context.Reputation;

        // In difficoltà economica arrivano solo occasioni che portano denaro: è il
        // percorso di ricostruzione, non un vicolo cieco.
        var broke = CareerFinances.InTrouble(context.Cash);

        // --- gare di categoria minore: la rete di sicurezza, e ha un posto suo
        //
        // Viene generata PRIMA di contare i posti liberi. Test e sedili durano
        // dieci e sessanta giorni, una gara pochi: col tempo le proposte lunghe
        // riempivano tutti e sei i posti e la gara non nasceva piu'. Restava un
        // pilota con i soldi in tasca e nient'altro che test da comprare.
        //
        // La gara e' la ragione per cui esiste tutto il resto: se qualcuno deve
        // cedere il passo, sono le proposte lunghe.
        // Ma solo a chi non ha gia' un campionato da correre: il round firmato
        // viene prima, e una gara open comprata nel frattempo lo scavalca in
        // agenda perche' cade sempre piu' vicina.
        // Basta che un round sia in calendario, con o senza contratto: chi
        // compra il weekend uno alla volta e' proprio quello che si faceva
        // scavalcare il proprio campionato dalle gare riempitive.
        if (!context.HasOpenRace && !context.HasPlannedRound)
        {
            var entry = BuildEntryRace(context, seed);
            if (entry != null) results.Add(entry);
        }

        var slots = MaxOpenOpportunities - Math.Max(0, context.OpenOpportunities);
        if (slots <= 0) return results;

        // --- eventi promozionali: dipendono dalla popolarità, portano denaro
        if (reputation.PublicPopularity >= 18 || broke)
        {
            var promo = BuildPromotionalEvent(context, seed);
            if (promo != null) results.Add(promo);
        }

        // --- sponsorizzazione: dipende dall'interesse degli sponsor
        // Soglia bassa per gli sponsor: iniziano a farsi vivi presto, appena un
        // pilota mostra risultati, ed è il primo canale di denaro che si apre.
        // Una sponsorizzazione si firma una volta. Si torna sul mercato solo per
        // un salto reale di interesse, e in quel caso sostituisce la precedente.
        var hasSponsor = OpportunityContext.HasSponsor(context.CurrentSponsor);
        var worthSwitching = !hasSponsor || reputation.SponsorAppeal >= context.CurrentSponsorAppeal + 15;
        if (reputation.SponsorAppeal >= 30 && context.SponsorRelation >= 25 && worthSwitching)
        {
            var sponsor = BuildSponsorDeal(context, seed);
            if (sponsor != null) results.Add(sponsor);
        }

        if (!broke)
        {
            // --- il test: un passaggio, non un mestiere.
            //
            // Un test privato serve quando si entra in una categoria nuova o
            // quando la stagione deve ancora cominciare: è la giornata in cui
            // si prende le misure alla vettura. Non è quello che riempie una
            // carriera.
            //
            // Senza questa condizione il paddock ne proponeva uno ogni tre
            // settimane per sempre: nel banco di prova una carriera di otto
            // anni contava settantasette test e le gare sparivano dietro di
            // essi. Chi gioca vuole correre — e su un PC con Assetto Corsa
            // ogni test è una sessione da guidare davvero.
            // Uno solo, e solo entrando in una categoria nuova.
            //
            // La condizione precedente lasciava ancora passare un test ogni tre
            // settimane a chi correva da cliente — nessun round in agenda,
            // nessun contratto — e la timeline tornava a riempirsi di prove in
            // mezzo alle gare. Il test è la giornata in cui si prende le misure
            // a una vettura mai guidata: fatta quella, si corre.
            var primaVoltaConQuestaVettura = context.TestsWithCurrentCar == 0;
            var appenaSalito = context.RacesAtStep <= 1;
            if (primaVoltaConQuestaVettura && appenaSalito)
            {
                var test = BuildTest(context, seed);
                if (test != null) results.Add(test);
            }

            // --- gara su invito in categoria superiore
            //
            // Chi ha un campionato da correre non compra un'altra gara.
            //
            // La guardia c'era solo sulla gara di categoria minore. L'invito
            // invece arrivava sempre, cadeva prima del round successivo e lo
            // scavalcava in agenda: nel banco di prova un pilota sotto
            // contratto correva ventidue gare su invito e un solo round di
            // campionato in due anni, la stagione non finiva mai e la
            // classifica restava a zero punti. Un pilota con un sedile corre
            // il proprio campionato; l'invito è la strada di chi non ce l'ha.
            if ((reputation.PressStanding >= 30 || reputation.SportingPrestige >= 35)
                && !context.HasPlannedRound && !context.HasOpenRace)
            {
                var invitation = BuildInvitationRace(context, seed);
                if (invitation != null) results.Add(invitation);
            }

            // --- i sedili si trattano quando la stagione è finita.
            //
            // Il mercato non apre a metà campionato: nella realtà si firma
            // nella pausa, e chi firma comincia a correre l'anno dopo. Qui i
            // sedili arrivavano in qualunque momento e ognuno azzerava il
            // calendario in corso — il pilota ricominciava la stagione da capo
            // ogni volta, la stagione non finiva mai e in otto anni di storia
            // il contatore restava a «stagione 1». Con questa condizione la
            // carriera prende il ritmo vero: si corre l'anno, poi si tratta.
            var mercatoAperto = !context.HasPlannedRound;
            if (mercatoAperto)
            {
                // Il salto di campionato per conoscenza: raro, e non passa
                // dalla classifica. È la ricompensa di immagine, forma e conti.
                var stepUp = BuildChampionshipStepUp(context, seed);
                if (stepUp != null) results.Add(stepUp);

                // Il sedile di campionato: la forma dipende da quanto vale il pilota.
                var seat = BuildSeat(context, seed);
                if (seat != null) results.Add(seat);
            }

            // --- l'altra disciplina: in cima la scala finisce e comincia la mappa
            var switchSeat = BuildDisciplineSwitch(context, seed);
            if (switchSeat != null) results.Add(switchSeat);

            // --- sostituzione: solo se i team si fidano davvero, e solo se il
            // pilota e' libero quel fine settimana.
            //
            // Una sostituzione e' l'occasione rara: qualcuno si fa male e tu
            // sei li'. Mancando il controllo sull'agenda ne arrivava una dietro
            // l'altra appena la fiducia superava 55: al banco trentacinque
            // sostituzioni su sessantuno gare, ognuna fissata a pochi giorni,
            // ognuna davanti al round di campionato che restava li' non corso.
            // Chi ha un campionato da correre non fa il sostituto.
            if (reputation.TeamTrust >= 55 && context.Races >= 3 && !context.HasPlannedRound)
            {
                var substitute = BuildSubstituteDrive(context, seed);
                if (substitute != null) results.Add(substitute);
            }
        }

        return results
            .Where(x => x != null)
            .OrderByDescending(x => Attractiveness(x, context))
            .Take(slots)
            .ToList();
    }

    private static int Attractiveness(Opportunity opportunity, OpportunityContext context)
    {
        var score = opportunity.Kind switch
        {
            // Un salto di campionato è la cosa più importante che possa
            // arrivare: capita di rado e scade, quindi non deve mai essere la
            // proposta che resta fuori per mancanza di posti.
            OpportunityKind.ChampionshipStepUp => 110,
            OpportunityKind.ProfessionalSeat => 100,
            // Capita di rado e scade: non deve essere la proposta che resta
            // fuori per mancanza di posti. Ma viene dopo il sedile del proprio
            // campionato: cambiare disciplina e' una scelta in piu', non
            // un'alternativa al correre.
            OpportunityKind.DisciplineSwitch => 95,
            OpportunityKind.PartiallyFundedSeat => 85,
            OpportunityKind.SubstituteDrive => 80,
            OpportunityKind.FundedTest => 70,
            OpportunityKind.InvitationRace => 60,
            OpportunityKind.SponsorDeal => 58,
            OpportunityKind.PayDriverSeat => 45,
            OpportunityKind.PaidTest => 40,
            OpportunityKind.EntryRace => 30,
            _ => 20
        };
        // Se il patrimonio non basta, la proposta scende: resta visibile, ma non
        // occupa il posto di qualcosa di realmente accessibile.
        if (opportunity.Assess(context.Cash).Risk is FinancialRisk.Unaffordable) score -= 50;
        return score;
    }

    // ------------------------------------------------------------- costruttori

    /// <summary>
    /// Marca temporale per gli identificativi delle proposte. Prima gli id
    /// dipendevano dal numero di gare: se la carriera si fermava — nessuna gara
    /// nuova — ogni proposta generata aveva un id gia presente, il deduplicatore
    /// la scartava e non arrivava piu nessuna occasione. Uno stallo permanente.
    /// </summary>
    /// <summary>
    /// La firma di una proposta.
    ///
    /// Era la sola data: due proposte dello stesso tipo nello stesso giorno
    /// avevano lo stesso identificativo, e la seconda veniva scartata come
    /// duplicato. Per un pilota che compra un weekend alla volta — dove la
    /// data avanza di pochi giorni — le occasioni finivano e non tornavano
    /// piu'.
    ///
    /// Aggiungendo le gare corse la firma resta stabile per la stessa
    /// proposta, ma cambia man mano che la carriera avanza.
    /// </summary>
    /// <summary>
    /// Una data di gara dentro l'anno sportivo.
    ///
    /// Le proposte fissavano il proprio appuntamento a «oggi piu' due
    /// settimane»: una proposta di dicembre produceva una gara a gennaio, con
    /// i circuiti chiusi. Fuori stagione l'appuntamento slitta alla primavera.
    /// </summary>
    private static DateTime DataDiGara(DateTime today, int giorni)
    {
        var data = today.AddDays(giorni);
        var ultima = NarrativeCalendar.LastRaceSundayOfSeason(data.Year);
        if (data.Month >= NarrativeCalendar.SeasonFirstMonth && data <= ultima) return data;
        var anno = data.Month >= NarrativeCalendar.SeasonFirstMonth ? data.Year + 1 : data.Year;
        return new DateTime(anno, NarrativeCalendar.SeasonFirstMonth, 12);
    }

    private static string Stamp(OpportunityContext context) => $"{context.Today:yyyyMMdd}-{context.Races}";


    private static Opportunity? BuildEntryRace(OpportunityContext context, long seed)
    {
        var track = PickTrack(context, seed, 0);
        if (track == null) return null;
        var fee = CareerFinances.RaceEntryFee("Rookie");
        return new Opportunity
        {
            Id = $"entry-{Stamp(context)}",
            Kind = OpportunityKind.EntryRace,
            Title = $"Gara open a {track.Name}",
            ProposedBy = "Organizzazione locale",
            Justification = context.Races == 0
                ? "Le gare open accettano iscrizioni da chiunque abbia una licenza: è il modo più economico per accumulare referti."
                : $"Il circuito di {track.Name} apre una gara open: costa poco e serve a tenere il ritmo fra due impegni più importanti.",
            Tier = "Rookie",
            TrackId = track.Id,
            TrackName = track.Name,
            // Sedici giorni, non sei: il tempo avanza a settimane intere, e con
            // una finestra di sei giorni la gara scadeva prima che si potesse
            // accettarla. Le occasioni che restavano erano solo quelle lunghe —
            // test e sedili — e di gare non ne arrivava piu' nessuna.
            Date = DataDiGara(context.Today, 21),
            Deadline = context.Today.AddDays(16),
            Cost = fee,
            BestCaseReturn = fee * 4,
            LikelyReturn = (int)(fee * 1.6),
            WorstCaseReturn = 0,
            Objective = "Chiudere davanti almeno a metà del gruppo",
            SeasonRounds = 0
        };
    }

    private static Opportunity? BuildPromotionalEvent(OpportunityContext context, long seed)
    {
        // Quanto vale una giornata promozionale dipende dalla categoria.
        //
        // Era «3000 + popolarità × 220» per tutti: un ragazzino del kart con un
        // buon seguito incassava venticinquemila euro per una mattinata,
        // cinquanta volte la quota d'iscrizione di una gara. Il banco di prova
        // trovava carriere che non correvano più — conveniva la giornata
        // promozionale — e arrivavano a mezzo milione in cassa senza vincere
        // niente. Un pilota kart prende un rimborso spese; una figura da
        // Formula 1 prende un ingaggio.
        var perGiornata = context.LadderStep switch
        {
            <= 1 => 150,
            2 => 300,
            3 => 550,
            4 => 1200,
            5 => 2600,
            6 => 5000,
            _ => 9000
        };
        // La popolarità muove il compenso, non lo moltiplica per cinque: con la
        // formula precedente una giornata al vertice pagava ventottomila euro
        // e, ripetuta ogni tre settimane, da sola portava in cassa tre milioni
        // in otto anni — più di tutti i premi gara della carriera messi insieme.
        var pay = (int)Math.Round(perGiornata * (0.6 + context.Reputation.PublicPopularity / 250.0) / 50.0) * 50;
        return new Opportunity
        {
            Id = $"promo-{Stamp(context)}",
            Kind = OpportunityKind.PromotionalEvent,
            Title = "Giornata promozionale retribuita",
            ProposedBy = "Agenzia di eventi motoristici",
            Justification = context.Reputation.PublicPopularity >= 30
                ? $"Con {context.Reputation.PublicPopularity} punti di livello influencer il nome di {context.Driver} vende biglietti: un'agenzia offre un ingaggio."
                : "Un'agenzia cerca piloti disponibili per una giornata dimostrativa: paga poco, ma paga.",
            Tier = context.Tier,
            // Come per la gara open: quattro giorni sono meno del passo con cui
            // il tempo avanza, e questa e' la proposta che porta denaro a chi
            // e' rimasto a corto. Scadeva sempre prima di essere vista.
            Date = context.Today.AddDays(14),
            Deadline = context.Today.AddDays(11),
            Cost = 0,
            BestCaseReturn = pay,
            LikelyReturn = pay,
            WorstCaseReturn = pay,
            Objective = "Presenza e disponibilità: nessun risultato sportivo in gioco"
        };
    }

    private static Opportunity? BuildSponsorDeal(OpportunityContext context, long seed)
    {
        var appeal = context.Reputation.SponsorAppeal;
        // Uno sponsor paga per la vetrina che gli dai, e la vetrina è la
        // categoria in cui corri. La cifra era la stessa dal kart al vertice —
        // fino a ottantottomila euro a un ragazzino del kartodromo, più di
        // quanto rendesse un'intera stagione — ed era la prima fonte del
        // milione in cassa che rendeva la carriera senza attriti.
        var perGradino = context.LadderStep switch
        {
            <= 1 => 900,
            2 => 1800,
            3 => 3600,
            4 => 10000,
            5 => 26000,
            6 => 58000,
            _ => 130000
        };
        var interesse = 0.45 + appeal / 200.0 + context.Reputation.PublicPopularity / 320.0;
        var amount = (int)Math.Round(perGradino * interesse / 100.0) * 100;
        var sponsor = SponsorName(seed, appeal);
        if (string.Equals(sponsor, context.CurrentSponsor, StringComparison.OrdinalIgnoreCase))
            sponsor = SponsorName(seed + 7, appeal + 3);
        if (string.Equals(sponsor, context.CurrentSponsor, StringComparison.OrdinalIgnoreCase)) return null;
        var upgrade = OpportunityContext.HasSponsor(context.CurrentSponsor);
        return new Opportunity
        {
            Id = $"sponsor-{Stamp(context)}-{appeal:000}",
            Kind = OpportunityKind.SponsorDeal,
            Title = upgrade ? $"{sponsor} vuole sostituire {context.CurrentSponsor}" : $"{sponsor} propone una sponsorizzazione",
            ProposedBy = sponsor,
            Justification = context.LastPosition is > 0 and <= 3 && !string.IsNullOrWhiteSpace(context.LastTrackName)
                ? $"Dopo il P{context.LastPosition} di {context.LastTrackName}, {sponsor} ha chiesto un contatto: cerca visibilità in questa categoria."
                : $"{sponsor} segue la categoria e valuta un pilota su cui investire; l'interesse verso {context.Driver} è a {appeal}/100.",
            Tier = context.Tier,
            // La firma avviene quando il pilota accetta: la data non puo
            // precedere il termine entro cui deve rispondere.
            Date = context.Today.AddDays(12),
            Deadline = context.Today.AddDays(12),
            Cost = 0,
            BestCaseReturn = amount,
            LikelyReturn = amount,
            WorstCaseReturn = amount,
            Objective = "Rispettare gli impegni di immagine concordati"
        };
    }

    private static Opportunity? BuildTest(OpportunityContext context, long seed)
    {
        var track = PickTrack(context, seed, 1);
        if (track == null) return null;
        var trust = context.Reputation.TeamTrust;
        var tier = NextTier(context);
        var fee = CareerFinances.TestFee(tier);
        // La copertura del costo è la misura di quanto un team crede nel pilota.
        var covered = trust >= 70 ? 100 : trust >= 55 ? 70 : trust >= 40 ? 40 : 0;
        var team = TeamName(seed, trust);
        var funded = covered > 0;
        var promise = new ConditionalPromise
        {
            Id = $"promise-test-{Stamp(context)}",
            Description = $"{team} valuterebbe un sedile per la stagione successiva",
            MaxFieldPercent = 30,
            RewardSeasonCoverage = Math.Min(60, 20 + trust / 2),
            RewardDescription = $"copertura parziale della stagione in {tier}",
            PromisedBy = team,
            Deadline = context.Today.AddDays(60)
        };
        return new Opportunity
        {
            Id = $"test-{Stamp(context)}-{covered:000}",
            Kind = funded ? OpportunityKind.FundedTest : OpportunityKind.PaidTest,
            Title = $"Test privato a {track.Name} con {team}",
            ProposedBy = team,
            Justification = BuildTestJustification(context, team, covered),
            Tier = tier,
            TrackId = track.Id,
            TrackName = track.Name,
            Date = context.Today.AddDays(14),
            Deadline = context.Today.AddDays(10),
            Cost = fee,
            CoveredPercent = covered,
            BestCaseReturn = 0,
            LikelyReturn = 0,
            WorstCaseReturn = 0,
            Objective = "Restare entro il riferimento del team",
            Promise = promise
        };
    }

    private static string BuildTestJustification(OpportunityContext context, string team, int covered)
    {
        if (context.LastPosition is > 0 and <= 3 && !string.IsNullOrWhiteSpace(context.LastTrackName))
        {
            var weather = string.IsNullOrWhiteSpace(context.LastWeather) ? "" : $" con il cielo {context.LastWeather}";
            return $"Dopo il P{context.LastPosition} ottenuto a {context.LastTrackName}{weather}, {team} ha manifestato interesse e propone un test privato." +
                   (covered > 0 ? $" Il team coprirà il {covered}% della spesa." : " Il costo resta interamente a carico del pilota.");
        }
        if (context.Reputation.TeamTrust >= 55)
            return $"La fiducia dei team è salita a {context.Reputation.TeamTrust}/100: {team} apre le porte del proprio programma di test" +
                   (covered > 0 ? $" e copre il {covered}% del costo." : ", a spese del pilota.");
        // Questo ramo ignorava la copertura: un test coperto al 40% veniva
        // proposto con la motivazione "il costo è tutto suo", che contraddiceva
        // sia l'etichetta sia la cifra mostrata al giocatore.
        if (covered > 0)
            return $"{team} apre una giornata di test a piloti esterni e, vista la fiducia a {context.Reputation.TeamTrust}/100, " +
                   $"copre il {covered}% della spesa: il resto è a carico del pilota.";
        return $"{team} affitta una giornata di test a piloti esterni. Nessuno sta investendo su {context.Driver}: il costo è tutto suo.";
    }

    private static Opportunity? BuildInvitationRace(OpportunityContext context, long seed)
    {
        var track = PickTrack(context, seed, 2);
        if (track == null) return null;
        var tier = NextTier(context);
        var fee = CareerFinances.RaceEntryFee(tier);
        var team = TeamName(seed + 7, context.Reputation.PressStanding);
        return new Opportunity
        {
            Id = $"invite-{Stamp(context)}",
            Kind = OpportunityKind.InvitationRace,
            Title = $"Gara {tier} su invito a {track.Name}",
            ProposedBy = team,
            Justification = context.Reputation.PressStanding >= 45
                ? $"La stampa ha iniziato a citare {context.Driver} ({context.Reputation.PressStanding}/100 di considerazione): {team} offre un sedile per una gara singola."
                : $"{team} ha una vettura libera per una gara e cerca un pilota pagante: l'occasione esiste, il conto è del pilota.",
            Tier = tier,
            TrackId = track.Id,
            TrackName = track.Name,
            Date = DataDiGara(context.Today, 18),
            Deadline = context.Today.AddDays(12),
            Cost = fee,
            BestCaseReturn = (int)(fee * 2.2),
            LikelyReturn = (int)(fee * 0.7),
            WorstCaseReturn = 0,
            Objective = $"Farsi notare in {tier} con una prestazione da metà gruppo in su",
            Promise = new ConditionalPromise
            {
                Id = $"promise-invite-{Stamp(context)}",
                Description = $"{team} coprirebbe una parte della stagione successiva",
                MaxFieldPercent = 40,
                RewardSeasonCoverage = 35,
                RewardDescription = $"copertura del 35% di una stagione in {tier}",
                PromisedBy = team,
                Deadline = context.Today.AddDays(75)
            }
        };
    }

    /// <summary>
    /// L'auto di un sedile: deve appartenere alla categoria offerta.
    ///
    /// Prima si prendeva la prima vettura da gara installata, qualunque
    /// fosse: un sedile di "Categoria regionale" poteva consegnare una
    /// monoposto da settecento cavalli, e la scala di carriera veniva saltata
    /// in un colpo. Ora la vettura appartiene al gradino che il sedile
    /// dichiara, e se non ce n'e nessuna il sedile non viene proposto.
    /// </summary>
    /// <summary>Gare da correre in cima prima che un'altra disciplina venga a cercarti.</summary>
    public const int RacesBeforeDisciplineSwitch = 10;

    /// <summary>
    /// Il sedile in un'altra disciplina, allo stesso livello.
    ///
    /// In cima la scala finisce e comincia la mappa: dalla Formula 1 si va in
    /// Indy come Mansell, o a Le Mans come Alonso, e poi si torna. Non e' una
    /// promozione e non e' una retrocessione — e' un'altra strada allo stesso
    /// gradino, ed e' quello che rende possibile una carriera lunga davvero.
    ///
    /// Le condizioni servono a distinguere la scelta di un veterano
    /// dall'oscillazione di un esordiente: bisogna essere sul gradino piu' alto
    /// che i contenuti permettono, averci corso una stagione, ed essere qualcuno
    /// che un'altra serie ha interesse a chiamare.
    /// </summary>
    private static Opportunity? BuildDisciplineSwitch(OpportunityContext context, long seed)
    {
        if (context.LadderStep <= 0) return null;
        var vetta = CareerLadder.PopulatedSteps(context.Cars).LastOrDefault();
        if (vetta <= 0 || context.LadderStep < vetta) return null;
        if (context.RacesAtStep < RacesBeforeDisciplineSwitch) return null;
        if (context.Reputation.SportingPrestige < 60) return null;

        var attuale = context.Cars.FirstOrDefault(x => x.Id.Equals(context.CurrentCarId, StringComparison.OrdinalIgnoreCase));
        if (attuale == null) return null;
        var stradaAttuale = CareerLadder.ForCar(attuale.Category, attuale.PowerHp, attuale.MassKg).Path;

        // Stesso gradino, disciplina diversa. Nessun filtro sul tier: e' proprio
        // il tier che cambia quando si passa dalle monoposto alle chiuse.
        var altrove = context.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .Select(x => (Auto: x, Gradino: CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg)))
            .Where(x => x.Gradino.Step == context.LadderStep && x.Gradino.Path != stradaAttuale)
            .OrderByDescending(x => x.Auto.PowerHp)
            .ToList();
        if (altrove.Count == 0) return null;
        var scelta = altrove[(int)(Math.Abs(seed / 7) % altrove.Count)];

        var serie = CareerLadder.PathName(scelta.Gradino.Path);
        return new Opportunity
        {
            Id = $"switch-{Stamp(context)}-{scelta.Auto.Id}",
            Kind = OpportunityKind.DisciplineSwitch,
            Title = $"{scelta.Gradino.Name}: un sedile fuori dalla tua disciplina",
            ProposedBy = TeamName(seed + 29, context.Reputation.SportingPrestige),
            Justification =
                $"Una squadra delle {serie} ha chiesto di te. Non e' una promozione e non e' un passo indietro: "
                + $"e' {scelta.Gradino.Name}, lo stesso livello che corri adesso, con una vettura che non hai mai guidato "
                + $"({scelta.Auto.Name}). Chi lo ha fatto prima di te ci e' andato da campione e qualche volta e' tornato indietro."
                + "\nQuello che hai costruito in pista resta; quello che sai della macchina, no.",
            Tier = scelta.Gradino.Tier,
            Category = scelta.Auto.Category,
            CarId = scelta.Auto.Id,
            Date = DataDiGara(context.Today, 45),
            Deadline = context.Today.AddDays(28),
            Cost = 0,
            Salary = 30000 + context.Reputation.SportingPrestige * 1200,
            SeasonRounds = CareerScheduler.RoundsForTier(scelta.Gradino.Tier),
            BestCaseReturn = 60000 + context.Reputation.SportingPrestige * 1200,
            LikelyReturn = 30000 + context.Reputation.SportingPrestige * 600,
            WorstCaseReturn = 0,
            Objective = $"Reggere il confronto in {serie}, dove nessuno ti deve niente"
        };
    }

    /// <summary>
    /// Quante gare servono su un gradino prima che qualcuno offra quello sopra.
    ///
    /// I numeri disegnano la forma della carriera: sommati fanno una sessantina
    /// di gare per arrivare in cima, cioe' quattro o cinque stagioni. Il kart da
    /// noleggio si lascia presto, il 125 con cambio e' dove si costruisce un
    /// pilota e infatti e' il gradino piu' lungo del tronco comune.
    /// </summary>
    public static int RacesBeforeStepUp(int step) => step switch
    {
        <= 1 => 6,
        2 => 10,
        3 => 12,
        4 => 14,
        5 => 16,
        _ => 18
    };

    private static ContentCarRecord? SeatCar(OpportunityContext context, string tier)
    {
        // Un sedile può portare al massimo un gradino sopra quello attuale.
        //
        // I tier sono quattro per sette gradini: dentro «Categoria avanzata»
        // convivono Formula 3, Formula 2, GT3 e prototipi, e dentro «Formula /
        // top tier» c'è la monoposto di vertice. Ragionando per solo tier, nel
        // collaudo la carriera passava da due gare di kart alla Formula 1 e ci
        // restava per quattro anni. Il gradino è la misura giusta.
        // Un gradino sopra al massimo, e mai uno sotto.
        //
        // Il limite era solo superiore, e la scelta prendeva poi la vettura
        // meno potente fra quelle ammesse: dopo essere salito in Formula 3
        // arrivava un'offerta che riportava in Formula Vee, e la carriera
        // rimbalzava fra le due per anni — nel banco, fino a cinquantuno cambi
        // di vettura in sessantotto gare. Una carriera sale o resta: non torna
        // indietro perché qualcuno le offre un sedile più basso.
        bool NelGradinoGiusto(ContentCarRecord x)
        {
            if (context.LadderStep <= 0) return true;
            var passo = CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step;
            // Il gradino sopra è il primo che i contenuti riempiono davvero: se
            // dopo la Formula 3 è installata solo la Formula 1, il salto è
            // quello, altrimenti la carriera si ferma davanti a un gradino che
            // non esiste in nessun catalogo.
            if (!CareerLadder.WithinReach(passo, context.LadderStep, context.Cars)) return false;
            // E il salto arriva solo a gavetta fatta: prima bastava esistere,
            // e nel banco si lasciava il kart dopo quattro gare in tutto.
            if (passo > context.LadderStep && context.RacesAtStep < RacesBeforeStepUp(context.LadderStep))
                return false;
            return true;
        }

        // La strada scelta al bivio decide la disciplina.
        //
        // Il kart resta comune a tutti: è il tronco da cui si parte. Sopra, chi
        // ha scelto le monoposto passa alla formula d'ingresso, non a un
        // trofeo turismo — e se in quella direzione non c'è niente di
        // installato si ripiega su quello che c'è, dichiarandolo con la
        // proposta invece di far cambiare mestiere al pilota in silenzio.
        List<ContentCarRecord> SullaStrada(List<ContentCarRecord> candidate) =>
            CareerLadder.OnPath(candidate, context.ChosenPath);

        var adatte = SullaStrada(context.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .Where(x => TierOf(x) == tier)
            .Where(NelGradinoGiusto)
            .OrderBy(x => x.PowerHp)
            .ToList());
        if (adatte.Count > 0) return adatte[0];

        // Nessuna vettura entro un gradino in quel tier: si guarda comunque il
        // gradino immediatamente successivo, ovunque sia, prima di ripiegare.
        var vicine = SullaStrada(context.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .Where(NelGradinoGiusto)
            .Where(x => context.LadderStep <= 0
                        || CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step > context.LadderStep)
            .OrderBy(x => x.PowerHp)
            .ToList());
        if (vicine.Count > 0) return vicine[0];

        // Ultimo ripiego: una vettura del proprio gradino, in qualunque tier.
        //
        // Era il buco piu' grande rimasto. Il ripiego prendeva la vettura meno
        // potente fra tutte le installate senza guardare ne' il gradino ne' la
        // gavetta: al banco Emi riceveva un sedile in Formula Vee dopo due sole
        // gare di kart da noleggio, saltando tre gradini in un colpo, e ci
        // restava trentasette gare senza mai un podio. Adesso vale la stessa
        // regola di sopra — il proprio gradino, o il primo pieno sopra a
        // gavetta fatta — e se non c'e' niente non si offre nessun sedile:
        // meglio nessuna proposta che una carriera che salta la sua storia.
        return context.Cars
            .Where(ContentCategoryRules.IsRaceable)
            .Where(NelGradinoGiusto)
            .OrderBy(x => x.PowerHp)
            .FirstOrDefault();
    }

    /// <summary>Il gradino di una vettura, con le stesse regole della scala.</summary>
    private static string TierOf(ContentCarRecord car)
        => CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Tier;

    /// <summary>Quanto seguito serve perché qualcuno venga a cercarti.</summary>
    public const int StepUpMinimumInfluencer = 55;
    /// <summary>
    /// Sotto questa forma nessuno ti porta in una categoria superiore.
    ///
    /// Era 55, ma nel collaudo una carriera intera chiudeva a 54: la porta
    /// restava sbarrata per un punto, e la strada non si apriva mai. La soglia
    /// è il minimo per essere presentabile; a decidere quanto spesso qualcuno
    /// bussa è comunque la forma, che pesa sulla probabilità.
    /// </summary>
    public const int StepUpMinimumFitness = 45;
    /// <summary>Serve del margine: un salto di categoria costa comunque.</summary>
    public const int StepUpMinimumCash = 40000;

    /// <summary>Quante gare devono passare fra un salto per conoscenza e il successivo.</summary>
    public const int StepUpRaceCooldown = 12;

    /// <summary>
    /// Il salto di campionato che non passa dalla classifica.
    ///
    /// La strada sportiva resta arrivare nei primi tre. Questa è l'altra: un
    /// pilota molto seguito, in forma e con dei soldi in cassa viene notato da
    /// qualcuno, e quel qualcuno è sempre una persona del suo giro — Haru che
    /// incontra uno sponsor, il meccanico chiamato da un ex capo, la
    /// giornalista con un contatto. Serve a dare un peso reale a immagine,
    /// conti e preparazione: senza, quei numeri sarebbero decorazione.
    ///
    /// Non è garantita: le condizioni aprono la porta, il seme decide se
    /// quella settimana qualcuno bussa davvero.
    /// </summary>
    private static Opportunity? BuildChampionshipStepUp(OpportunityContext context, long seed)
    {
        var livello = ChampionshipLadder.Clamp(context.ChampionshipLevel);
        if (ChampionshipLadder.IsTop(livello)) return null;
        if (context.Races < 4) return null;

        var influencer = context.Reputation.PublicPopularity;
        if (influencer < StepUpMinimumInfluencer) return null;
        if (context.Fitness < StepUpMinimumFitness) return null;

        // La chiamata per merito: quando stai facendo qualcosa che si vede.
        //
        // Tre vittorie di fila, cinque podi consecutivi o una rimonta da otto
        // posizioni sono cose di cui in paddock si parla, e chi sta cercando un
        // pilota non aspetta la classifica finale per farsi vivo. Insieme
        // servono comunque seguito e condizione: e' quello che rende una
        // squadra disposta a rischiare a stagione in corso, ed e' il motivo per
        // cui vale la pena curare immagine e preparazione anche quando i
        // risultati da soli non basterebbero.
        //
        // Quando arriva per merito la chiamata non passa dal sorteggio e non
        // chiede denaro: e' la squadra a volerti, non il contrario.
        var perMerito = CareerProgression.MeritaLaChiamata(
            context.VittorieDiFila, context.PodiDiFila, context.MigliorRimontaRecente,
            influencer, context.Fitness, livello);

        if (!perMerito)
        {
            if (context.Cash < StepUpMinimumCash) return null;

            // Una promozione per conoscenza non arriva due volte di seguito: nel
            // banco un pilota ne collezionava quattro e toccava il mondiale con
            // tre vittorie in carriera.
            if (context.RacesSinceLevelUp < StepUpRaceCooldown) return null;

            // Più sei seguito, in forma e liquido, più spesso qualcuno si fa vivo —
            // ma quello che fai in pista pesa comunque: chi non ha mai un podio
            // resta una scommessa che pochi accettano.
            var merito = (influencer - StepUpMinimumInfluencer)
                         + (context.Fitness - StepUpMinimumFitness) / 2
                         + Math.Min(20, (int)((context.Cash - StepUpMinimumCash) / 25000))
                         + Math.Min(25, context.Podiums * 3 + context.Wins * 2);
            var probabilita = Math.Clamp(4 + merito / 4, 4, 25);
            if (seed % 100 >= probabilita) return null;
        }

        var prossimo = livello + 1;
        var tier = NextTier(context);
        var car = SeatCar(context, tier);
        if (car == null) return null;

        // Il racconto dipende dalla categoria che si va a correre: una chiamata
        // per la Formula 3 e una per una squadra endurance non arrivano dalle
        // stesse persone né per le stesse ragioni.
        var gradino = CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg);
        var storie = ContattiPerCategoria(gradino, context, influencer, prossimo);
        if (storie.Count == 0) storie = Contatti(context, influencer, prossimo);
        var storia = storie[(int)(seed / 7 % storie.Count)];
        // Chi ti vuole per merito copre il posto: la quota la paga la squadra.
        var quota = perMerito ? 0 : Math.Max(0, CareerFinances.SeasonEntryFee(tier) - storia.Copertura);

        var motivo = perMerito
            ? CareerProgression.MotivoDellaChiamata(context.VittorieDiFila, context.PodiDiFila, context.MigliorRimontaRecente)
            : "";
        var spiegazione = perMerito
            ? $"{storia.Squadra} ha chiamato per {motivo}."
              + $"\n\nNon aspettano la fine del campionato: ti vogliono adesso, a stagione in corso, e il posto lo pagano loro."
              + $"\nQuello che ti ha reso chiamabile non sono solo i risultati: livello influencer {influencer}/100 e forma {context.Fitness}/100 "
              + "sono il motivo per cui una squadra è disposta a rischiare su di te invece che su un altro."
              + $"\n\nAccettando passi a «{ChampionshipLadder.Name(prossimo)}» (livello {prossimo} di {ChampionshipLadder.Levels}): {ChampionshipLadder.Scope(prossimo)}."
              + $"\nLe prime {CareerProgression.GareDiProvaDopoIlSalto} gare decidono se resti: vanno bene e il posto è tuo, vanno male e si torna indietro."
            : storia.Racconto
              + $"\n\nNon arriva dalla classifica: arriva da come ti sei mosso fuori dalla pista. Livello influencer {influencer}/100, forma {context.Fitness}/100, cassa € {context.Cash:N0}."
              + $"\nAccettando passi a «{ChampionshipLadder.Name(prossimo)}» (livello {prossimo} di {ChampionshipLadder.Levels}): {ChampionshipLadder.Scope(prossimo)}.";

        return new Opportunity
        {
            Id = $"stepup-{Stamp(context)}-{prossimo}-{influencer:000}",
            Kind = OpportunityKind.ChampionshipStepUp,
            Title = perMerito
                ? $"{ChampionshipLadder.Name(prossimo)} · {storia.Squadra} ti vuole adesso"
                : $"{ChampionshipLadder.Name(prossimo)} · posto offerto da {storia.Squadra}",
            ProposedBy = storia.Squadra,
            Justification = spiegazione,
            Tier = tier, Category = car.Category, CarId = car.Id,
            Date = DataDiGara(context.Today, perMerito ? 14 : 30),
            Deadline = context.Today.AddDays(perMerito ? 10 : 18),
            Cost = quota, SeasonRounds = CareerScheduler.RoundsForTier(tier),
            Salary = storia.Stipendio,
            GrantsChampionshipLevel = prossimo,
            BestCaseReturn = 30000 + influencer * 400, LikelyReturn = 12000 + influencer * 150, WorstCaseReturn = 0,
            Objective = $"Reggere il confronto in {ChampionshipLadder.Name(prossimo).ToLowerInvariant()}"
        };
    }

    private sealed record ContattoStepUp(string Squadra, string Racconto, int Copertura, int Stipendio);

    /// <summary>
    /// Le chiamate che arrivano per una precisa categoria.
    ///
    /// Il salto non è un fatto astratto: chi ti cerca per un kart 125 non è
    /// chi ti cerca per una hypercar, e non ti cerca per gli stessi motivi.
    /// Qui ogni gradino della scala ha le sue voci — sempre passando da una
    /// persona del giro del pilota, mai da un ufficio senza volto.
    /// </summary>
    private static List<ContattoStepUp> ContattiPerCategoria(
        LadderRung gradino, OpportunityContext context, int influencer, int livello)
    {
        var campionato = ChampionshipLadder.Name(livello).ToLowerInvariant();
        var q = CareerFinances.SeasonEntryFee(NextTier(context));
        var seguito = $"{influencer}/100 di livello influencer";

        return gradino.Id switch
        {
            CareerLadder.TwoStroke =>
            [
                new("Minato Apex Racing", $"Genji Arakawa ha parlato con un preparatore di due tempi che gli deve un motore: ha un telaio fermo e nessuno che lo porti in pista nel {campionato}. «Il quattro tempi te l'ha insegnato tutto quello che poteva», dice Genji.", q / 3, 0),
                new("Hoshi Kart Works", $"Haru Senda ha convinto un rivenditore locale a coprire una parte della stagione: gli interessa il tuo pubblico, non i tuoi tempi. Con {seguito} sei il volto che cercava per il negozio.", q / 2, 0),
                new("Suzuhara Racing", $"Una squadra kart ha perso il suo pilota di punta a metà stagione. Rei Kisaragi ha fatto il tuo nome prima che aprissero le selezioni.", q / 3, 0)
            ],

            CareerLadder.Shifter =>
            [
                new("Hoshi Kart Works", $"Il gestore del kartodromo dove ti alleni ha un 125 con cambio invenduto e un posto nel {campionato}. Preferisce darlo a te che a un cliente di passaggio: sa come tratti i mezzi.", q / 3, 0),
                new("Kaido Motorsport", $"Miki Arisawa ha chiuso un accordo con un marchio di caschi: vogliono un pilota riconoscibile sul gradino più alto del kart, e il tuo nome gira ({seguito}).", q / 2, 0),
                new("Minato Apex Racing", $"Riku Hayase è salito di categoria e la sua vecchia squadra ha un sedile libero. Non è stato lui a farti il nome — ma non l'ha nemmeno impedito.", q / 3, 0)
            ],

            "formula-4" =>
            [
                new("Tsubasa Works", $"Rei Kisaragi ha ricevuto una chiamata dal programma giovani: cercano un pilota per la Formula 4 nel {campionato} e hanno visto i numeri delle tue presenze. È il salto dal kart alle ruote scoperte, e non capita spesso che te lo offrano.", q / 2, 0),
                new("Hoshino Race Team", $"Shigeo Kanda ha mostrato la tua telemetria a un ingegnere di Formula 4 durante un corso: gli è piaciuto quanto sei costante quando la macchina non aiuta. In monoposto conta più del giro secco.", q / 3, 0),
                new("Rinkai Racing Team", $"Un costruttore di monoposto junior cerca un pilota da mettere nei propri video promozionali. Haru Senda ha fatto notare che con {seguito} porti pubblico oltre che chilometri.", q * 2 / 3, 0),
                new("Suzuhara Racing", $"Noa Minazuki ha scritto un pezzo sul passaggio kart-formula e ti ha usato come esempio. Un team di Formula 4 l'ha letto e ha chiesto il tuo contatto.", q / 4, 0)
            ],

            "formula-3" =>
            [
                new("Kaido Motorsport", $"Una squadra di Formula 3 ha perso il secondo pilota per infortunio a stagione iniziata. Genji Arakawa conosce il loro capo meccanico da vent'anni e ha risposto prima che chiedessero: «ce l'ho io, uno».", q / 3, 0),
                new("Tsubasa Works", $"Miki Arisawa ha portato al tavolo uno sponsor tecnico che entra in Formula 3 e vuole legarsi a un pilota giovane con un seguito vero ({seguito}). Coprono una parte della stagione se il volto sei tu.", q / 2, 0),
                new("Hoshino Race Team", $"Il team principal ha visto il tuo onboard girare in rete. Non gli interessa il sorpasso: gli interessa che non hai chiuso gli occhi.", q / 3, 0)
            ],

            "formula-2" =>
            [
                new("Rinkai Racing Team", $"Una squadra di Formula 2 ha bisogno di un pilota che porti budget e visibilità senza essere un passeggero. Haru Senda ha passato tre settimane a costruire questo incontro.", q * 2 / 3, 0),
                new("Kaido Motorsport", $"Rei Kisaragi ti raccomanda a una squadra che conosce dai tempi del programma rookie. Lo fa controvoglia: sa che in Formula 2 gli errori si pagano davvero.", q / 2, 0),
                new("Minato Apex Racing", $"Un pilota titolare ha rotto il contratto a metà stagione. Noa Minazuki l'ha saputo prima dei comunicati e ti ha avvisato con un giorno di vantaggio.", q / 3, 0)
            ],

            "formula-1" =>
            [
                new("Tsubasa Works", $"È arrivata la chiamata che nessuno si aspetta: un test con la squadra di vertice, e un sedile se il test regge. Rei Kisaragi non fa complimenti — dice solo di non presentarti impreparato.", q / 2, 220000),
                new("Kaido Motorsport", $"Un costruttore cerca un pilota di casa da promuovere anche fuori dalla pista. Miki Arisawa ha fatto valere {seguito}: il progetto commerciale sei tu quanto il pilota.", q / 3, 260000),
                new("Rinkai Racing Team", $"Genji Arakawa ha ricevuto una telefonata da un vecchio collega che oggi sta in una squadra di vertice. Non ti ha detto cosa ha risposto: ti ha solo detto di tenere il telefono acceso.", q / 4, 200000)
            ],

            "cup" =>
            [
                new("Yamabuki Racing", $"Un trofeo monomarca cerca piloti per riempire la griglia del {campionato} e offre condizioni agevolate a chi porta pubblico. Haru Senda ha trattato la quota fino all'osso.", q / 2, 0),
                new("Serizawa Motorsport", $"Il concessionario che sponsorizza il monomarca ha visto i tuoi post e vuole la sua livrea addosso a te. Non capisce di gare, capisce di vetrine.", q * 2 / 3, 0),
                new("Suzuhara Racing", $"Genji Arakawa ha preparato per anni le vetture di questo trofeo. Una chiamata è bastata.", q / 3, 0)
            ],

            "tcr" =>
            [
                new("Hoshino Race Team", $"Una squadra turismo cerca un secondo pilota per il {campionato}: gare corte, contatto, tanta televisione. Miki Arisawa dice che è il posto giusto per chi ha {seguito}.", q / 2, 0),
                new("Kaido Motorsport", $"Il loro pilota è passato a un altro costruttore e sono rimasti scoperti a stagione iniziata. Noa Minazuki ti ha segnalato al direttore sportivo lo stesso pomeriggio.", q / 3, 0),
                new("Yamabuki Racing", $"Shigeo Kanda ha lavorato al progetto della vettura e sa che tipo di pilota serve: uno che non distrugga le gomme nel primo stint.", q / 3, 0)
            ],

            "gt4" =>
            [
                new("Rinkai Racing Team", $"Una squadra GT4 cerca un compagno per un gentleman driver che paga la stagione: serve qualcuno di veloce e presentabile. Haru Senda ha fatto vedere i tuoi numeri social ({seguito}) insieme ai tempi.", q / 3, 0),
                new("Tsubasa Works", $"Genji Arakawa conosce il capo squadra da quando montavano insieme sospensioni di kart. Gli ha detto che sai gestire una macchina pesante senza romperla.", q / 2, 0),
                new("Serizawa Motorsport", $"Un marchio di accessori entra nel GT e vuole un pilota giovane come immagine. Miki Arisawa ha portato la proposta già scritta.", q * 2 / 3, 0)
            ],

            "gt3" =>
            [
                new("Kaido Motorsport", $"Un equipaggio GT3 ha perso il pilota bronze a metà campionato. Rei Kisaragi ha risposto al telefono al posto tuo, poi te l'ha detto.", q / 3, 0),
                new("Hoshino Race Team", $"Il costruttore cerca un pilota da inserire nel proprio programma giovani GT. Contano i tempi, ma anche quanto porti in termini di attenzione: {seguito}.", q / 2, 0),
                new("Minato Apex Racing", $"Noa Minazuki ha intervistato il team manager e gli ha lasciato il tuo nome in fondo al blocco. L'ha ritrovato lui, due settimane dopo.", q / 4, 0)
            ],

            "prototipi" =>
            [
                new("Tsubasa Works", $"Un team prototipi cerca un terzo pilota per le gare lunghe del {campionato}: turni di notte, cambi al buio, nessuna gloria immediata. Shigeo Kanda dice che è la scuola migliore che esista.", q / 3, 60000),
                new("Rinkai Racing Team", $"Genji Arakawa ha lavorato a un prototipo trent'anni fa e non ha mai smesso di parlarne. Il suo vecchio capo adesso dirige questa squadra.", q / 2, 40000),
                new("Kaido Motorsport", $"Miki Arisawa ha chiuso un accordo con un partner tecnologico che entra nell'endurance e vuole un pilota comunicativo: {seguito} ha pesato quanto il curriculum.", q * 2 / 3, 50000)
            ],

            "hypercar" =>
            [
                new("Tsubasa Works", $"Il programma hypercar di un costruttore cerca un pilota da far crescere. Rei Kisaragi ti avvisa: da qui non si torna indietro, e nessuno aspetta chi non regge il passo.", q / 3, 300000),
                new("Hoshino Race Team", $"Una squadra ufficiale ha bisogno di un equipaggio completo per la stagione mondiale. Genji Arakawa ha fatto il tuo nome e ha smesso di rispondere alle domande.", q / 4, 280000),
                new("Rinkai Racing Team", $"Noa Minazuki ha scritto che meritavi una macchina vera. Qualcuno che decide l'ha letto e ha deciso di verificarlo.", q / 3, 320000)
            ],

            _ => []
        };
    }

    /// <summary>
    /// Chi bussa, e perché. Ogni caso passa da una persona del giro del pilota:
    /// una proposta che arriva da un nome senza volto non racconta niente.
    /// </summary>
    private static List<ContattoStepUp> Contatti(OpportunityContext context, int influencer, int livello)
    {
        var campionato = ChampionshipLadder.Name(livello).ToLowerInvariant();
        var stagione = CareerFinances.SeasonEntryFee(NextTier(context));
        return
        [
            new("Kaido Motorsport",
                $"Haru Senda ha incontrato il responsabile marketing di una casa automobilistica a una fiera: aveva già visto i tuoi post e sapeva il tuo nome prima che Haru lo dicesse. Cercano un pilota giovane e riconoscibile per il {campionato}, e sono disposti a metterci una parte del budget.",
                stagione / 2, 0),

            new("Hoshino Race Team",
                $"Genji Arakawa è stato chiamato da un suo ex capo officina, oggi in una squadra del {campionato}: hanno un sedile libero a stagione iniziata e si fidano più del giudizio di Genji che di un provino. «Gli ho detto che sai tornare ai box con qualcosa da spiegare», dice lui, come se fosse poco.",
                stagione / 3, 0),

            new("Rinkai Racing Team",
                $"Noa Minazuki ha girato il tuo nome a un direttore sportivo che le doveva un favore. Il pezzo che ha scritto su di te è stato letto più di quanto pensasse, e nel {campionato} qualcuno ha chiesto chi fossi.",
                stagione / 4, 0),

            new("Minato Apex Racing",
                $"Miki Arisawa ha chiuso un giro di telefonate che durava da settimane: uno sponsor tecnico entra nel {campionato} e vuole legare il proprio nome a un pilota con un seguito vero. Con {influencer}/100 di livello influencer, il pilota sei tu.",
                stagione * 2 / 3, 0),

            new("Tsubasa Works",
                $"Rei Kisaragi ha ricevuto una chiamata dal programma giovani di una squadra del {campionato}. Non è un favore: hanno visto i numeri delle tue presenze e hanno pensato che valga la pena rischiare una stagione su di te. Rei è più cauta di loro, e te lo dirà.",
                stagione / 2, 0),

            new("Suzuhara Racing",
                $"Un influencer con cui hai girato un video ha un fratello che corre nel {campionato}. Da lì è partito un passaparola che è arrivato fino al team manager, che adesso vuole incontrarti prima che lo faccia qualcun altro.",
                stagione / 3, 0),

            new("Yamabuki Racing",
                $"Haru Senda è stato richiamato dal gommista che ti aveva detto di no mesi fa: adesso fornisce una squadra del {campionato} e ha proposto il tuo nome come contropartita. Le porte che si chiudono qualche volta si riaprono da un'altra parte.",
                stagione / 3, 0),

            new("Serizawa Motorsport",
                $"Il gestore del kartodromo dove hai passato una giornata con i ragazzi ha parlato di te a un vecchio amico che oggi dirige una squadra del {campionato}. «Uno che insegna ai ragazzini la domenica non molla al terzo giro», gli ha detto.",
                stagione / 4, 0),

            new("Kaido Motorsport",
                $"Shigeo Kanda ha mostrato i tuoi dati a un ingegnere del {campionato} durante un corso di aggiornamento. Non gli interessava il tuo miglior giro: gli interessava quanto sei costante quando la macchina non aiuta.",
                stagione / 2, 0)
        ];
    }

    private static Opportunity? BuildSeat(OpportunityContext context, long seed)
    {
        var tier = NextTier(context);
        var car = SeatCar(context, tier);
        if (car == null) return null;
        var trust = context.Reputation.TeamTrust;
        var prestige = context.Reputation.SportingPrestige;
        var seasonFee = CareerFinances.SeasonEntryFee(tier);
        var team = TeamName(seed + 13, trust);
        var rounds = CareerScheduler.RoundsForTier(tier);

        // La forma del sedile è la misura di quanto il pilota vale sul mercato.
        if (prestige >= 65 && trust >= 65)
            return new Opportunity
            {
                Id = $"seat-pro-{Stamp(context)}-{prestige:000}",
                Kind = OpportunityKind.ProfessionalSeat,
                Title = $"Contratto {tier} con {team}",
                ProposedBy = team,
                Justification = $"Con {context.Wins} vittorie e un prestigio sportivo di {prestige}/100, {team} offre un sedile pagato: nessun contributo richiesto al pilota.",
                Tier = tier, Category = car.Category, CarId = car.Id,
                Date = DataDiGara(context.Today, 24), Deadline = context.Today.AddDays(15),
                Cost = 0, Salary = 40000 + prestige * 1800, SeasonRounds = rounds,
                BestCaseReturn = 40000 + prestige * 1800, LikelyReturn = 30000 + prestige * 1200, WorstCaseReturn = 20000,
                Objective = "Lottare per le posizioni di vertice del campionato"
            };

        if (trust >= 45 || prestige >= 45)
        {
            var covered = Math.Clamp(20 + trust / 2, 20, 70);
            return new Opportunity
            {
                Id = $"seat-part-{Stamp(context)}-{trust:000}",
                Kind = OpportunityKind.PartiallyFundedSeat,
                Title = $"Sedile {tier} con {team}, stagione parzialmente finanziata",
                ProposedBy = team,
                Justification = $"{team} è disposta a coprire il {covered}% della stagione: la fiducia dei team è a {trust}/100 e il prestigio sportivo a {prestige}/100.",
                Tier = tier, Category = car.Category, CarId = car.Id,
                Date = DataDiGara(context.Today, 28), Deadline = context.Today.AddDays(16),
                Cost = seasonFee, CoveredPercent = covered, SeasonRounds = rounds,
                BestCaseReturn = (int)(seasonFee * 1.3), LikelyReturn = (int)(seasonFee * 0.55), WorstCaseReturn = 0,
                Objective = "Chiudere la stagione nella prima metà della classifica",
                Promise = new ConditionalPromise
                {
                    Id = $"promise-seat-{Stamp(context)}-{trust:000}",
                    Description = $"{team} finanzierebbe interamente la stagione successiva",
                    MaxPosition = 3,
                    RewardSeasonCoverage = 100,
                    RewardDescription = "stagione successiva interamente coperta",
                    PromisedBy = team,
                    Deadline = context.Today.AddDays(240)
                }
            };
        }

        return new Opportunity
        {
            Id = $"seat-pay-{Stamp(context)}",
            Kind = OpportunityKind.PayDriverSeat,
            Title = $"Sedile {tier} da finanziare con {team}",
            ProposedBy = team,
            Justification = $"Nessun team sta investendo su {context.Driver}: {team} vende un sedile per l'intera stagione. Il conto è interamente del pilota.",
            Tier = tier, Category = car.Category, CarId = car.Id,
            Date = DataDiGara(context.Today, 30), Deadline = context.Today.AddDays(18),
            Cost = seasonFee, SeasonRounds = rounds,
            BestCaseReturn = (int)(seasonFee * 0.9), LikelyReturn = (int)(seasonFee * 0.35), WorstCaseReturn = 0,
            Objective = "Completare la stagione e costruire un curriculum"
        };
    }

    private static Opportunity? BuildSubstituteDrive(OpportunityContext context, long seed)
    {
        var track = PickTrack(context, seed, 3);
        if (track == null) return null;
        var tier = NextTier(context);
        var team = TeamName(seed + 23, context.Reputation.TeamTrust);
        return new Opportunity
        {
            Id = $"sub-{Stamp(context)}",
            Kind = OpportunityKind.SubstituteDrive,
            Title = $"Sostituzione in {tier} a {track.Name}",
            ProposedBy = team,
            Justification = $"Un pilota di {team} è indisponibile e la squadra cerca un sostituto affidabile: la fiducia dei team verso {context.Driver} è a {context.Reputation.TeamTrust}/100. Nessun costo richiesto.",
            Tier = tier,
            TrackId = track.Id,
            TrackName = track.Name,
            Date = context.Today.AddDays(7),
            Deadline = context.Today.AddDays(4),
            Cost = 0,
            BestCaseReturn = CareerFinances.RaceEntryFee(tier) * 2,
            LikelyReturn = CareerFinances.RaceEntryFee(tier) / 2,
            WorstCaseReturn = 0,
            Objective = "Non deludere chi ha chiamato all'ultimo momento"
        };
    }

    // ------------------------------------------------------------- supporto

    /// <summary>Categoria immediatamente superiore fra quelle installate, o quella attuale.</summary>
    public static string NextTier(OpportunityContext context)
    {
        var next = ProgressionEngine.NextAvailableTier(context.Tier, context.AvailableTiers);
        return string.IsNullOrWhiteSpace(next) ? context.Tier : next;
    }

    private static ContentTrackRecord? PickTrack(OpportunityContext context, long seed, int salt)
    {
        if (context.Tracks.Count == 0) return null;
        var ordered = context.Tracks.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
        return ordered[(int)(Math.Abs(seed / 3 + salt * 31) % ordered.Count)];
    }

    private static readonly string[] Teams =
    ["Kaido Motorsport", "Minato Apex Racing", "Tsubasa Works", "Rinkai Racing Team", "Suzuhara Racing", "Hoshino Race Team", "Yamabuki Racing"];

    private static readonly string[] Sponsors =
    ["Kanda Tools", "Rinkai Energy", "Hoshino Engineering", "Sakura Assicurazioni", "Tetsuya Componenti", "Minato Logistica"];

    public static string TeamName(long seed, int weight) => Teams[(int)(Math.Abs(seed / 5 + weight) % Teams.Length)];
    public static string SponsorName(long seed, int weight) => Sponsors[(int)(Math.Abs(seed / 11 + weight) % Sponsors.Length)];

    internal static long StableSeed(OpportunityContext context)
    {
        var text = $"{context.Driver}|{context.Season}|{context.Races}|{context.Tier}|{context.Reputation.Overall}|{context.Cash / 1000}";
        var hash = 2166136261L;
        foreach (var character in text) hash = (hash ^ character) * 16777619 % 2147483647;
        return Math.Abs(hash);
    }
}
