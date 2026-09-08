namespace CorsaCareer;

/// <summary>
/// Un'angolazione giornalistica: quanto è rilevante per questo evento e come si
/// scrive. Ogni angolazione può usare solo i dati di <see cref="StoryFacts"/>.
/// </summary>
public sealed class StoryAngle
{
    public string Name { get; init; } = "";
    public Func<StoryFacts, int> Relevance { get; init; } = _ => 0;
    public Func<StoryFacts, PhraseBank, string> Write { get; init; } = (_, _) => "";
}

/// <summary>
/// Catalogo delle angolazioni. La varietà nasce da due livelli: quali blocchi
/// entrano nell'articolo (dipende dai fatti) e come sono formulati (dipende dalla
/// banca di formulazioni). Due gare simili producono così articoli diversi.
/// </summary>
public static class StoryAngles
{
    private static string Place(StoryFacts f) => NarrativeEngine.Capitalize(f.Track);
    private static string Name(StoryFacts f) => string.IsNullOrWhiteSpace(f.Surname) ? f.Driver : f.Surname;
    private static string Lap(int ms) => RookieTargetEngine.Format(ms);

    public static readonly StoryAngle Fallback = new()
    {
        Name = "generico",
        Relevance = _ => 1,
        Write = (f, bank) => bank.Pick("angle-fallback",
            $"Il dossier di {f.Driver} resta aperto: {f.Races} gare disputate, {f.Wins} vittorie e {f.Podiums} podi archiviati, con una reputazione di {f.Reputation} punti su cento. Il prossimo capitolo nascerà da un referto reale, non da una previsione.",
            $"Per ora la cronaca si limita ai numeri già acquisiti: {f.Races} gare, {f.Wins} vittorie, {f.Podiums} podi, budget di € {f.Cash:N0}. Tutto il resto è materia per il prossimo weekend.",
            $"{f.TeamName} e {f.Driver} restano in attesa del fatto nuovo. Fino ad allora il quadro è quello dell'archivio: {f.Races} gare corse e una reputazione di {f.Reputation}/100.")
    };

    // ------------------------------------------------------------------- gara

    private static readonly StoryAngle Result = new()
    {
        Name = "risultato",
        Relevance = f => f.HasRace && !f.Abandoned ? 100 : 0,
        Write = (f, bank) =>
        {
            // Ogni frammento esiste solo se il dato esiste. Prima, quando un dato
            // mancava, al suo posto entrava una nota sul referto: ne uscivano
            // frasi sgrammaticate che raccontavano al lettore il formato del
            // salvataggio invece della gara.
            var partenza = f.StartingPosition > 0 ? $"partito P{f.StartingPosition}" : "";
            var distacco = f.GapMilliseconds > 0 ? $"a {f.GapMilliseconds / 1000.0:0.###} dal vincitore" : "";
            var giro = f.BestLapMilliseconds > 0 ? $"il suo giro migliore è stato {Lap(f.BestLapMilliseconds)}" : "";

            if (f.Dnf)
            {
                var danno = f.Damage > 0 ? $" La vettura ha riportato danni ({f.Damage:0.##})." : "";
                var apertura = partenza.Length > 0 ? $"{NarrativeEngine.Capitalize(partenza)}, {f.Driver}" : f.Driver;
                return bank.Pick("angle-result-dnf",
                    $"La gara di {Place(f)} si chiude prima del traguardo. {apertura} ha percorso {f.Laps} giri, poi si è fermato.{danno} Nessun punto, e la classifica resta dov'era.",
                    $"Niente bandiera a scacchi. {f.Laps} giri, poi il box.{danno} Per {f.Driver} restano zero punti e una casella vuota nella stagione {f.Season}.",
                    $"{f.Laps} giri, e a {Place(f)} finisce lì. {apertura} non vede il traguardo: nessun punto, nessun premio, un debrief che riparte da capo.");
            }

            // La coda tecnica si costruisce solo con quello che c'è.
            var coda = string.Join(", ", new[] { distacco, giro }.Where(x => x.Length > 0));
            var codaFrase = coda.Length > 0 ? $" {NarrativeEngine.Capitalize(coda)}." : "";
            var aperturaGara = partenza.Length > 0 ? $"{NarrativeEngine.Capitalize(partenza)}, " : "";
            var puntiFrase = f.Points > 0
                ? $"Sono {f.Points} punti."
                : "Non ci sono punti da portare a casa.";

            return bank.Pick("angle-result",
                $"{aperturaGara}{f.Driver} ha chiuso {f.PositionLabel} su {f.FieldSize}.{codaFrase} {puntiFrase}",
                $"{f.PositionLabel} su {f.FieldSize} al traguardo di {Place(f)}, dopo {f.Laps} giri.{codaFrase} {puntiFrase}",
                $"A {Place(f)} arriva {f.PositionLabel}. {aperturaGara}{f.Driver} ha tenuto {f.Laps} giri.{codaFrase} {puntiFrase}",
                $"Su {f.FieldSize} al via, {f.Driver} porta la {f.TeamName} a {f.PositionLabel}.{codaFrase} {puntiFrase}");
        }
    };

    /// <summary>
    /// Weekend preparato e non concluso. Non esiste una posizione d'arrivo, e il
    /// testo non deve inventarne una: il fatto raccontato è il ritiro.
    /// </summary>
    private static readonly StoryAngle Withdrawal = new()
    {
        Name = "ritiro-weekend",
        Relevance = f => f.Abandoned ? 130 : 0,
        Write = (f, bank) => bank.Pick("angle-withdrawal",
            $"Il weekend di {Place(f)} non è mai arrivato in fondo. {f.Driver} non ha completato la sessione: non c'è una posizione d'arrivo da registrare, e questo è il dato. Zero punti, nessun premio, e il round resta comunque disputato.",
            $"A {Place(f)} il round si chiude senza classifica. Non è un piazzamento deludente: è un weekend non portato a termine, e nel curriculum di un pilota pesa in modo diverso.",
            $"Nessun risultato da {Place(f)}. La sessione era stata preparata e non si è conclusa: il campionato registra il round come disputato, la casella dei punti resta vuota.",
            $"{Place(f)} entra nell'archivio come weekend abbandonato. Non ci sono tempi, non c'è un arrivo: c'è solo la trasferta da pagare e un round che non tornerà.")
    };

    /// <summary>Conseguenze del ritiro: è la parte che rende la scelta costosa.</summary>
    private static readonly StoryAngle WithdrawalCost = new()
    {
        Name = "costo-ritiro",
        Relevance = f => f.Abandoned ? 110 : 0,
        Write = (f, bank) => bank.Pick("angle-withdrawal-cost",
            $"Il conto lo si vede sul contorno: reputazione in calo, rapporto con {f.TeamName} raffreddato, {f.SponsorName} che chiede spiegazioni. Un ritiro dal weekend non è neutro, ed è il motivo per cui non conviene trattarlo come una via d'uscita.",
            $"Un weekend non concluso lascia tracce fuori dalla classifica: fiducia del box, pazienza dello sponsor e budget, perché la trasferta è stata comunque sostenuta.",
            $"Restano le conseguenze: {f.TeamName} annota, {f.SponsorName} annota, e la reputazione di {f.Driver} scende. Il round consumato è la parte più costosa, perché non si recupera.")
    };

    private static readonly StoryAngle Qualifying = new()
    {
        Name = "qualifica",
        Relevance = f => !f.HasRace || f.Abandoned || f.QualifyingPosition <= 0 ? 0 : f.QualifyingPosition == 1 ? 70 : Math.Abs(f.QualifyingPosition - f.Position) >= 3 ? 62 : 30,
        Write = (f, bank) =>
        {
            var moved = f.QualifyingPosition - f.Position;
            if (f.QualifyingPosition == 1)
                return bank.Pick("angle-quali-pole",
                    $"La pole position era arrivata il sabato, e non è un dettaglio: significa che il pacchetto {f.TeamName} sul giro secco c'era. {(f.Won ? "Trasformarla in vittoria è la parte difficile, ed è stata fatta." : $"Trasformarla in risultato è un'altra questione: il traguardo dice {f.PositionLabel}.")}",
                    $"Partire davanti a tutti cambia la lettura del weekend. {f.Driver} aveva firmato la qualifica; poi la gara ha detto {f.PositionLabel}, e la differenza fra le due cose è esattamente il materiale del debrief.");
            if (moved >= 3)
                return bank.Pick("angle-quali-gain",
                    $"Il salto è avvenuto in gara: dalla qualifica P{f.QualifyingPosition} al {f.PositionLabel} finale sono {moved} posizioni recuperate. È il tipo di progressione che si costruisce sulla gestione della gomma, non sull'ispirazione del singolo giro.",
                    $"C'è una notizia dentro il weekend: {moved} posizioni guadagnate rispetto alla qualifica P{f.QualifyingPosition}. Il passo di gara è stato migliore di quello sul giro secco, e questo è un dato tecnico utile a {f.TeamName}.");
            if (moved <= -3)
                return bank.Pick("angle-quali-loss",
                    $"Il sabato aveva promesso di più: dalla qualifica P{f.QualifyingPosition} al {f.PositionLabel} finale sono {Math.Abs(moved)} posizioni perse. Il degrado, la strategia o il traffico: il box ha tre ipotesi e i dati per sceglierne una.",
                    $"Qui il conto non torna: qualifica P{f.QualifyingPosition}, arrivo {f.PositionLabel}. Perdere {Math.Abs(moved)} posizioni in gara è il problema tecnico che questo weekend consegna a {f.TeamName}.");
            return bank.Pick("angle-quali-flat",
                $"Qualifica P{f.QualifyingPosition} e arrivo {f.PositionLabel}: il weekend è stato coerente dall'inizio alla fine, senza scossoni in un senso o nell'altro.",
                $"Fra il sabato e la domenica non è cambiato quasi nulla: da P{f.QualifyingPosition} a {f.PositionLabel}. Una gara lineare, che dice soprattutto quanto vale il pacchetto oggi.");
        }
    };

    private static readonly StoryAngle Teammate = new()
    {
        Name = "compagno",
        Relevance = f => !f.HasRace || f.Abandoned || f.TeammatePosition <= 0 ? 0 : Math.Abs(f.Position - f.TeammatePosition) <= 2 ? 78 : 52,
        Write = (f, bank) =>
        {
            var ahead = !f.Dnf && f.Position < f.TeammatePosition;
            var season = f.TeammateRacesWon + f.TeammateRacesLost > 0 ? $" In stagione il confronto diretto dice {f.TeammateRacesWon}-{f.TeammateRacesLost}." : "";
            return ahead
                ? bank.Pick("angle-teammate-ahead",
                    $"Dentro il box il verdetto è netto: {f.Driver} davanti a {f.TeammateName}, {f.PositionLabel} contro P{f.TeammatePosition}. È il confronto che i team guardano prima di qualunque classifica, perché è l'unico che usa la stessa macchina.{season}",
                    $"Con lo stesso materiale tecnico, {f.Driver} ha chiuso davanti a {f.TeammateName} ({f.PositionLabel} contro P{f.TeammatePosition}). Nel mercato dei piloti questo dato pesa più di dieci dichiarazioni.{season}",
                    $"{f.TeammateName} resta dietro: {f.PositionLabel} contro P{f.TeammatePosition}. Il confronto interno è la misura più onesta che esista, e questa volta premia {f.Driver}.{season}")
                : bank.Pick("angle-teammate-behind",
                    $"Il confronto interno però non aiuta: {f.TeammateName} ha chiuso P{f.TeammatePosition}, davanti a {f.Driver}. Con la stessa vettura non ci sono alibi tecnici da cercare.{season}",
                    $"C'è un dato scomodo in questo weekend: {f.TeammateName} è arrivato P{f.TeammatePosition}, {f.Driver} {f.PositionLabel}. Nel box lo sanno tutti, e sanno anche che è il numero che conta.{season}",
                    $"Sull'altro lato del box {f.TeammateName} ha fatto meglio, P{f.TeammatePosition}. È il tipo di sconfitta che non si vede in classifica generale ma che si sente nelle riunioni.{season}");
        }
    };

    private static readonly StoryAngle Championship = new()
    {
        Name = "campionato",
        Relevance = f => f.ChampionshipPosition <= 0 ? 0 : f.ChampionshipPosition == 1 ? 85 : f.RoundsLeft <= 2 ? 80 : f.ChampionshipPosition <= 3 ? 66 : 40,
        Write = (f, bank) =>
        {
            var gap = f.LeaderPoints - f.ChampionshipPoints;
            if (f.ChampionshipPosition == 1)
                return bank.Pick("angle-champ-leader",
                    $"In classifica generale {f.Driver} è primo con {f.ChampionshipPoints} punti. Comandare il {f.Championship} cambia il modo in cui si affrontano i {f.RoundsLeft} round rimasti: da qui in poi ogni weekend è anche una gestione del vantaggio.",
                    $"La classifica dice {f.Driver} in testa, {f.ChampionshipPoints} punti. Con {f.RoundsLeft} appuntamenti al termine, la domanda non è più se il passo c'è, ma se regge la pressione.");
            if (f.RoundsLeft <= 2 && f.RoundCount >= 4)
                return bank.Pick("angle-champ-endgame",
                    $"Restano {f.RoundsLeft} round e {f.Driver} è P{f.ChampionshipPosition} con {f.ChampionshipPoints} punti, a {gap} da {f.LeaderName}. Il margine per gli esperimenti è finito: contano solo i punti.",
                    $"Il finale di campionato è aritmetica: P{f.ChampionshipPosition}, {f.ChampionshipPoints} punti, {gap} di ritardo su {f.LeaderName}, {f.RoundsLeft} gare a disposizione. Ogni errore ora costa doppio.");
            // Senza round residui noti il testo non può promettere tempo per
            // recuperare: prima l'articolo scriveva «con 0 gare rimaste, il
            // margine esiste ancora».
            var horizon = f.RoundsLeft > 0
                ? $" e {f.RoundsLeft} round per recuperarli"
                : f.RoundCount > 0 ? " e il calendario esaurito" : "";
            var closing = f.RoundsLeft > 0
                ? $"Con {f.RoundsLeft} gare rimaste, il margine esiste ancora."
                : f.RoundCount > 0 ? "Il calendario è chiuso: questo è il bilancio definitivo." : "Il quadro va letto sui punti, non sul tempo residuo.";
            return bank.Pick("angle-champ",
                $"La classifica generale colloca {f.Driver} in P{f.ChampionshipPosition} con {f.ChampionshipPoints} punti; {f.LeaderName} guida con {f.LeaderPoints}. Sono {gap} punti di distanza{horizon}.",
                $"Nel {f.Championship} il quadro è questo: P{f.ChampionshipPosition} e {f.ChampionshipPoints} punti, con {f.LeaderName} davanti a quota {f.LeaderPoints}. I {gap} punti di ritardo hanno un peso preciso.",
                $"Dal punto di vista del campionato il weekend vale P{f.ChampionshipPosition}: {f.ChampionshipPoints} punti contro i {f.LeaderPoints} di {f.LeaderName}. {closing}");
        }
    };

    private static readonly StoryAngle Streak = new()
    {
        Name = "serie",
        Relevance = f => !f.HasRace || f.Abandoned ? 0 : f.Scored && f.PointlessStreak >= 3 ? 92 : f.PointlessStreak >= 3 ? 74 : f.PodiumStreak >= 3 ? 80 : 0,
        Write = (f, bank) =>
        {
            if (f.Scored && f.PointlessStreak >= 3)
                return bank.Pick("angle-streak-broken",
                    $"C'è un dettaglio che dà senso a questi {f.Points} punti: arrivavano da {f.PointlessStreak} gare senza muovere la classifica. Una serie negativa interrotta vale, per il morale di un box, più di quanto dica il piazzamento.",
                    $"Dopo {f.PointlessStreak} weekend a mani vuote, la casella dei punti torna a riempirsi. Non è una vittoria, ma è il momento in cui una stagione smette di scendere.",
                    $"Il digiuno durava da {f.PointlessStreak} gare. Interromperlo a {Place(f)} non cambia la classifica generale in modo drastico, ma cambia il tono delle riunioni del lunedì.");
            if (f.PointlessStreak >= 3)
                return bank.Pick("angle-streak-open",
                    $"Il problema non è questa domenica: è la sequenza. Sono {f.PointlessStreak} gare consecutive senza punti, e a un certo punto una serie del genere smette di essere sfortuna e diventa una questione tecnica o di gestione.",
                    $"La serie negativa si allunga: {f.PointlessStreak} gare senza punti. È il tipo di andamento che apre discussioni sul pacchetto, sul programma di sviluppo e — inevitabilmente — sul pilota.",
                    $"Con {f.PointlessStreak} weekend consecutivi senza raccolto, la stagione di {f.Driver} ha un problema strutturale da nominare, non un episodio da archiviare.");
            return bank.Pick("angle-streak-podium",
                $"Sono {f.PodiumStreak} podi consecutivi. Una serie così non nasce dal caso: nasce da una macchina in finestra e da un pilota che non sta commettendo errori.",
                $"{f.PodiumStreak} volte di fila fra i primi tre. È la continuità che trasforma una buona stagione in una candidatura credibile.");
        }
    };

    private static readonly StoryAngle Milestone = new()
    {
        Name = "traguardo",
        Relevance = f => f.MilestoneRaces > 0 ? 95 : f.IsFirstWin ? 98 : f.IsFirstPodium ? 90 : 0,
        Write = (f, bank) =>
        {
            if (f.IsFirstWin)
                return bank.Pick("angle-first-win",
                    $"È la prima vittoria della carriera. Ci sono volute {f.Races} gare, e questo numero racconta più di qualunque aggettivo: {f.Driver} ha costruito questo risultato accumulando referti, non aspettando l'occasione giusta.",
                    $"Prima vittoria in {f.Races} gare disputate. Da oggi il curriculum di {f.Driver} cambia categoria: non c'è più una promessa da valutare, c'è un pilota che ha vinto.",
                    $"Il primo successo arriva alla gara numero {f.Races}. Nel paddock questo genere di soglia si nota: cambia il modo in cui i team manager leggono un nome su una lista.");
            if (f.IsFirstPodium)
                return bank.Pick("angle-first-podium",
                    $"Primo podio della carriera, alla gara numero {f.Races}. Non è il traguardo finale, ma è la prima volta che il nome di {f.Driver} compare dove i team guardano davvero.",
                    $"Ci sono volute {f.Races} gare per il primo podio. È il risultato che apre le porte: da qui in avanti le aspettative su {f.Driver} non torneranno più al punto di partenza.");
            return bank.Pick("angle-milestone",
                $"Questo weekend segna anche la gara numero {f.MilestoneRaces} della carriera. È un traguardo che misura la resistenza più del talento: arrivare a {f.MilestoneRaces} referti archiviati significa aver superato stagioni difficili, non solo quelle buone.",
                $"Gara {f.MilestoneRaces}. Un numero che in questo mestiere si raggiunge solo restando: cambi di squadra, crisi, ripartenze — tutto sta dentro questa cifra.");
        }
    };

    private static readonly StoryAngle Rivalry = new()
    {
        Name = "rivalita",
        Relevance = f => f.Abandoned || string.IsNullOrWhiteSpace(f.RivalName) || f.RivalLevel < 30 ? 0 : f.RivalPosition > 0 && Math.Abs(f.RivalPosition - f.Position) <= 2 ? 84 : 48,
        Write = (f, bank) =>
        {
            if (f.RivalPosition > 0)
            {
                var ahead = !f.Dnf && f.Position < f.RivalPosition;
                return ahead
                    ? bank.Pick("angle-rival-ahead",
                        $"Il duello con {f.RivalName} si è deciso ancora una volta sul filo: {f.PositionLabel} contro P{f.RivalPosition}. Una rivalità a intensità {f.RivalLevel} su cento non si spegne con un sorpasso, ma questa volta il conto lo tiene {f.Driver}.",
                        $"E poi c'è {f.RivalName}, che questa volta ha chiuso dietro ({f.PositionLabel} contro P{f.RivalPosition}). Il livello di questa rivalità è arrivato a {f.RivalLevel}/100: significa che i due si guardano prima ancora di guardare la classifica.")
                    : bank.Pick("angle-rival-behind",
                        $"{f.RivalName} ha vinto il confronto diretto, P{f.RivalPosition} contro {f.PositionLabel}. In una rivalità arrivata a intensità {f.RivalLevel}/100 ogni scambio di questo tipo si accumula, e prima o poi presenta il conto.",
                        $"Il capitolo di oggi lo scrive {f.RivalName}, davanti con P{f.RivalPosition}. La rivalità è a {f.RivalLevel}/100 e non sembra destinata a raffreddarsi.");
            }
            return bank.Pick("angle-rival-context",
                $"Resta aperta la questione {f.RivalName}: una rivalità a intensità {f.RivalLevel}/100, nata dai referti e non dalle dichiarazioni, che continuerà a orientare la lettura di questa stagione.",
                $"Sul fondo del quadro c'è sempre {f.RivalName} — intensità {f.RivalLevel}/100 — e il confronto con lui è diventato uno dei metri con cui il paddock misura {f.Driver}.");
        }
    };

    private static readonly StoryAngle Conditions = new()
    {
        Name = "condizioni",
        Relevance = f => !f.HasRace || f.Abandoned || string.IsNullOrWhiteSpace(f.WeatherLabel) ? 0 : f.Cloudy ? 66 : 34,
        Write = (f, bank) =>
        {
            var hour = TimeSpan.FromSeconds(f.TimeOfDaySeconds).ToString(@"hh\:mm");
            var format = string.IsNullOrWhiteSpace(f.FormatLabel) ? "la gara" : f.FormatLabel.ToLowerInvariant();
            return bank.Pick("angle-conditions",
                $"Il contesto tecnico va detto: {format} con cielo {f.WeatherLabel}, {f.TemperatureC:0.#} gradi e partenza alle {hour}. Sono le condizioni scritte nel programma del weekend, e su una pista come {Place(f)} incidono su gomma e finestra di lavoro.",
                $"{NarrativeEngine.Capitalize(f.WeatherLabel)}, {f.TemperatureC:0.#} °C, via alle {hour}: {format} si è corsa così. Non è un dettaglio da bollettino, è il parametro che decide quanto dura la gomma.",
                $"Chi legge solo il piazzamento perde metà del quadro: {format}, {f.WeatherLabel}, {f.TemperatureC:0.#} gradi d'aria. Con {f.Laps} giri da coprire, la gestione contava almeno quanto il passo puro.");
        }
    };

    private static readonly StoryAngle Technical = new()
    {
        Name = "tecnica",
        Relevance = f => !f.HasRace || f.Abandoned ? 0 : f.PitStops > 0 || f.PenaltySeconds > 0 || f.Damage > 0 ? 70 : f.BestLapMilliseconds > 0 ? 26 : 0,
        Write = (f, bank) =>
        {
            var pieces = new List<string>();
            if (f.PitStops > 0) pieces.Add($"{f.PitStops} sosta/e ai box");
            if (f.PenaltySeconds > 0) pieces.Add($"{f.PenaltySeconds:0.#} secondi di penalità");
            if (f.Damage > 0) pieces.Add($"un danno registrato di {f.Damage:0.##}");
            if (f.BestLapMilliseconds > 0) pieces.Add($"un miglior giro di {Lap(f.BestLapMilliseconds)}");
            var detail = pieces.Count == 0 ? "pochi elementi" : string.Join(", ", pieces);
            return bank.Pick("angle-technical",
                $"La lettura tecnica del weekend mette in fila {detail}. Sono i numeri su cui il box costruisce il debrief, ed è da lì che si capisce se il weekend è stato una questione di passo o di gestione.",
                $"Nel dettaglio: {detail}. Ogni voce di questo elenco è un pezzo di lavoro per gli ingegneri di {f.TeamName}, e nessuna di esse si risolve con una dichiarazione.",
                $"Il foglio dei dati dice {detail}. È la parte del weekend che non finisce nei titoli ma che decide come andrà il prossimo.");
        }
    };

    private static readonly StoryAngle Economy = new()
    {
        Name = "economia",
        Relevance = f => !f.HasRace ? 0 : f.Cash < 60000 ? 76 : f.Prize > 0 ? 30 : f.Damage > 0.3 ? 58 : 0,
        Write = (f, bank) =>
        {
            if (f.Cash < 60000)
                return bank.Pick("angle-economy-tight",
                    $"C'è poi la questione che nessuno mette in copertina: il budget è scivolato a € {f.Cash:N0}. A questi livelli ogni riparazione e ogni trasferta diventano una scelta, e le scelte fatte per necessità raramente migliorano un risultato.",
                    $"Il conto economico è la parte scomoda di questa stagione: € {f.Cash:N0} residui. Quando il margine si assottiglia così, il programma tecnico si accorcia da solo.");
            if (f.Damage > 0.3)
                return bank.Pick("angle-economy-damage",
                    $"Il danno di {f.Damage:0.##} non è solo un dato tecnico: è una fattura. In un bilancio che oggi segna € {f.Cash:N0}, ogni weekend con materiale da ricostruire toglie risorse al prossimo.",
                    $"Riparare costa, e questo weekend ha lasciato un danno di {f.Damage:0.##}. Con € {f.Cash:N0} in cassa, il conto delle riparazioni entra direttamente nelle decisioni sportive.");
            return bank.Pick("angle-economy-prize",
                $"Sul piano economico il weekend porta € {f.Prize:N0} di premio gara, con un bilancio che resta a € {f.Cash:N0}. In questa categoria i conti non sono un dettaglio contabile: decidono quanti test si possono fare.",
                $"Il premio di € {f.Prize:N0} entra in un bilancio da € {f.Cash:N0}. Sono i numeri che stabiliscono quanto margine avrà {f.TeamName} nella seconda parte di stagione.");
        }
    };

    private static readonly StoryAngle SponsorAndTeam = new()
    {
        Name = "sponsor",
        Relevance = f => f.SponsorRelation <= 25 || f.TeamRelation <= 25 ? 82 : f.SponsorStatus == "Raggiunto" ? 44 : f.HasRace ? 22 : 30,
        Write = (f, bank) =>
        {
            if (f.SponsorRelation <= 25 && f.HasSponsor)
                return bank.Pick("angle-sponsor-crisis",
                    $"Fuori dalla pista la situazione è tesa: il rapporto con {f.SponsorName} è a {f.SponsorRelation} su cento. Uno sponsor che si raffredda non fa comunicati, riduce il budget — e il budget è la vera classifica di questo mestiere.",
                    $"{f.SponsorName} sta valutando. Con un rapporto commerciale a {f.SponsorRelation}/100 e l'obiettivo stagionale ancora «{f.SponsorStatus.ToLowerInvariant()}», il rinnovo non è una formalità.");
            if (f.TeamRelation <= 25 && f.HasTeam)
                return bank.Pick("angle-team-crisis",
                    $"Nel box l'aria è cambiata: il rapporto con {f.TeamName} è scivolato a {f.TeamRelation}/100. Quando la fiducia interna si incrina, il pilota è sempre la variabile più facile da sostituire.",
                    $"Il rapporto con {f.TeamName} è a {f.TeamRelation} punti su cento, e questo pesa su come vengono distribuite le risorse tecniche prima ancora che sul mercato.");
            if (f.SponsorStatus == "Raggiunto" && f.HasSponsor)
                return bank.Pick("angle-sponsor-ok",
                    $"Sul fronte commerciale l'obiettivo di {f.SponsorName} è stato raggiunto, con un rapporto a {f.SponsorRelation}/100. Significa bonus incassati e una trattativa di rinnovo che parte dalla posizione giusta.",
                    $"{f.SponsorName} può mettere una spunta: obiettivo stagionale centrato, rapporto a {f.SponsorRelation}/100. È il tipo di stabilità che permette di programmare invece di improvvisare.");
            // Senza squadra e senza sponsor non c'e nessun "contorno
            // contrattuale": raccontare la mancanza e piu onesto che nominare
            // un'etichetta di stato come se fosse un interlocutore.
            if (!f.HasTeam && !f.HasSponsor)
                return bank.Pick("angle-nobody",
                    "Non c'è un box che aspetti questo pilota, e non c'è un marchio sulla tuta. Nessuno ha ancora messo un euro su di lui: tutto quello che succede in pista se lo paga da solo.",
                    "Nessun contratto, nessuno sponsor, nessun obiettivo da rispettare se non quello che si dà da sé. È la condizione più libera e la più fragile che esista in questo mestiere.",
                    "Il paddock, per ora, guarda altrove. Non c'è una squadra da soddisfare né un finanziatore a cui rendere conto: c'è solo un pilota che deve dare a qualcuno un motivo per accorgersi di lui.");
            if (!f.HasTeam)
                return bank.Pick("angle-no-team",
                    $"Il pilota corre senza una squadra alle spalle: {f.SponsorName} è l'unico appoggio, con un rapporto a {f.SponsorRelation}/100. Tutto il resto è a carico suo.",
                    $"Nessun box lo ha ancora messo sotto contratto. L'unico legame è con {f.SponsorName}, e regge su {f.SponsorRelation} punti su cento.");
            if (!f.HasSponsor)
                return bank.Pick("angle-no-sponsor",
                    $"Con {f.TeamName} il rapporto è a {f.TeamRelation}/100, ma sulla tuta non c'è ancora un marchio: il fronte commerciale resta tutto da aprire.",
                    $"{f.TeamName} lo tiene, e il rapporto vale {f.TeamRelation}/100. Manca invece uno sponsor, e senza quello ogni stagione si costruisce sul filo.");
            return bank.Pick("angle-sponsor-neutral",
                $"Il contorno contrattuale resta questo: obiettivo «{f.ContractObjective}» in stato «{f.ContractObjectiveStatus.ToLowerInvariant()}», rapporto con {f.SponsorName} a {f.SponsorRelation}/100 e con {f.TeamName} a {f.TeamRelation}/100.",
                $"Sullo sfondo ci sono gli impegni: {f.SponsorName} osserva il rapporto ({f.SponsorRelation}/100) e l'obiettivo contrattuale «{f.ContractObjective}» resta «{f.ContractObjectiveStatus.ToLowerInvariant()}».");
        }
    };

    private static readonly StoryAngle People = new()
    {
        Name = "persone",
        Relevance = f => (f.HasRace || f.EventType.Contains("TEST", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(f.ManagerName) ? 57 : 0,
        Write = (f, bank) =>
        {
            var manager = string.IsNullOrWhiteSpace(f.ManagerMemory) ? "ha preso nota del risultato" : f.ManagerMemory;
            var mechanic = string.IsNullOrWhiteSpace(f.MechanicMemory) ? "ha preparato il debrief tecnico" : f.MechanicMemory;
            return bank.Pick("angle-people",
                $"Nel box non si è parlato soltanto di piazzamento. {f.ManagerName} {manager.ToLowerInvariant()}, mentre {f.MechanicName} {mechanic.ToLowerInvariant()}. Sono reazioni che restano nel dossier e orientano il prossimo appuntamento.",
                $"Il risultato ha due letture umane: {f.ManagerName} deve trasformarlo in una proposta credibile; {f.MechanicName} lo userà per il lavoro sulla vettura ({mechanic}). La loro memoria del weekend è parte della carriera quanto il cronometro.",
                $"A fine sessione {f.ManagerName} ha aggiornato il taccuino del mercato e {f.MechanicName} quello tecnico ({mechanic}). {f.Driver} non corre in un vuoto: ogni dato cambia il modo in cui queste persone lo accompagneranno al prossimo semaforo.");
        }
    };

    private static readonly StoryAngle Memory = new()
    {
        Name = "memoria",
        // Il collegamento con il passato è ciò che distingue una cronaca da un
        // bollettino: quando esiste una gara precedente vale più della tecnica.
        Relevance = f => !string.IsNullOrWhiteSpace(f.PreviousTrack) ? 76 : f.SeasonsCompleted > 0 ? 58 : 0,
        Write = (f, bank) =>
        {
            if (!string.IsNullOrWhiteSpace(f.PreviousTrack))
            {
                var before = f.PreviousDnf ? "un ritiro" : $"un {(f.PreviousPosition <= 3 ? "podio" : $"P{f.PreviousPosition}")}";
                var direction = f.Dnf ? "una battuta d'arresto"
                    : f.PreviousDnf ? "una risposta"
                    : f.Position < f.PreviousPosition ? "un passo avanti"
                    : f.Position > f.PreviousPosition ? "un passo indietro" : "una conferma";
                return bank.Pick("angle-memory-previous",
                    $"Vale la pena rileggere la sequenza. A {NarrativeEngine.Capitalize(f.PreviousTrack)} era arrivato {before}; oggi {Place(f)} consegna {direction}. Una carriera si giudica su questi collegamenti, non sul singolo referto.",
                    $"Il weekend precedente, a {NarrativeEngine.Capitalize(f.PreviousTrack)}, si era chiuso con {before}. Messo in fila con quello di oggi, il quadro è {direction}: ed è questo il dato che i team manager annotano.",
                    $"Chi segue questa carriera da qualche gara riconosce lo schema: a {NarrativeEngine.Capitalize(f.PreviousTrack)} {before}, qui {direction}. La stagione si sta scrivendo per accumulo.");
            }
            return bank.Pick("angle-memory-archive",
                $"L'archivio conserva {f.SeasonsCompleted} stagione/i completata/e — {f.BestPreviousSeason} il riferimento migliore. È la misura con cui va confrontato tutto quello che accade adesso.",
                $"Non è la prima stagione di questa carriera: ce ne sono {f.SeasonsCompleted} archiviate, con {f.BestPreviousSeason} come punto più alto. Ogni nuovo referto si legge in quel contesto.");
        }
    };

    private static readonly StoryAngle OffTrack = new()
    {
        Name = "fuoripista",
        Relevance = f => string.IsNullOrWhiteSpace(f.LastActivity) ? 0 : f.Fatigue >= 60 ? 72 : f.LastActivitySuccess ? 28 : 56,
        Write = (f, bank) =>
        {
            if (f.Fatigue >= 60)
                return bank.Pick("angle-offtrack-tired",
                    $"Va detto anche questo: la condizione di {f.Driver} è a {f.Fatigue}/100 di stanchezza accumulata, dopo {f.LastActivity.ToLowerInvariant()}. Un calendario di impegni riempito senza recupero si paga, e si paga in pista.",
                    $"L'agenda fuori dal circuito ha lasciato il segno — {f.LastActivity.ToLowerInvariant()}, stanchezza a {f.Fatigue}/100. Nessun preparatore firmerebbe un programma così alla vigilia di un weekend.");
            return f.LastActivitySuccess
                ? bank.Pick("angle-offtrack-good",
                    $"Fra i due weekend c'era stata {f.LastActivity.ToLowerInvariant()}, andata a buon fine: il seguito di {f.Driver} è a {f.Fanbase} e il rapporto con {f.TeamName} a {f.TeamRelation}/100. Il lavoro che non si vede conta nel mercato.",
                    $"Nel frattempo, lontano dalla pista: {f.LastActivity.ToLowerInvariant()}, con esito positivo. Sono i mattoni che costruiscono un seguito ({f.Fanbase}) e una credibilità professionale.")
                : bank.Pick("angle-offtrack-bad",
                    $"C'è anche un capitolo fuori dal circuito da mettere a bilancio: {f.LastActivity.ToLowerInvariant()} non è andata come previsto. Piccoli danni d'immagine che, sommati, arrivano al tavolo delle trattative.",
                    $"Non tutto è andato bene lontano dalla pista: {f.LastActivity.ToLowerInvariant()} si è chiusa male. In una carriera che si costruisce anche sulle relazioni, sono episodi che restano negli archivi.");
        }
    };

    /// <summary>
    /// Servizio dell'agenda: racconta l'attività, la decisione presa e le sue
    /// conseguenze. È l'angolazione che rende ogni voce dell'agenda una pagina
    /// del giornale e non solo una riga di registro.
    /// </summary>
    private static readonly StoryAngle ActivityReport = new()
    {
        Name = "agenda",
        Relevance = f => f.IsActivityStory ? 120 : 0,
        Write = (f, bank) =>
        {
            var choice = string.IsNullOrWhiteSpace(f.ActivityChoice) ? "" : $" La linea scelta è stata «{f.ActivityChoice}»: {f.ActivityAngle}.";
            var days = f.ActivityDays == 1 ? "una giornata" : $"{f.ActivityDays} giornate";
            return f.LastActivitySuccess
                ? bank.Pick("angle-activity-good",
                    $"{f.LastActivity} occupa {days} del calendario di {f.Driver}, fuori dal circuito ma dentro il mestiere.{choice} L'esito è positivo: {f.ActivityStory}",
                    $"Fra due weekend il lavoro non si ferma. {f.LastActivity} — area {f.ActivityCategory.ToLowerInvariant()}, {days} — si chiude come il box sperava.{choice} {f.ActivityStory}",
                    $"C'è una parte della carriera che non passa per il cronometro, e questa settimana è toccata a {f.LastActivity.ToLowerInvariant()}.{choice} È andata bene: {f.ActivityStory}")
                : bank.Pick("angle-activity-bad",
                    $"{f.LastActivity} non ha funzionato. {days} spese senza il ritorno previsto{(string.IsNullOrWhiteSpace(choice) ? "" : ",")}{choice} {f.ActivityStory}",
                    $"Passo falso lontano dalla pista: {f.LastActivity.ToLowerInvariant()} si chiude in perdita.{choice} {f.ActivityStory}",
                    $"Non tutto si aggiusta in pista, e non tutto si aggiusta fuori. {f.LastActivity} è andata male.{choice} {f.ActivityStory}");
        }
    };

    /// <summary>Conseguenze misurabili della voce di agenda: è il bilancio della decisione.</summary>
    private static readonly StoryAngle ActivityConsequences = new()
    {
        Name = "conseguenze",
        Relevance = f => f.IsActivityStory ? 100 : 0,
        Write = (f, bank) =>
        {
            var parts = new List<string>();
            if (f.ActivityReputation != 0) parts.Add($"reputazione {f.ActivityReputation:+#;-#;0}");
            if (f.ActivityFanbase != 0) parts.Add($"seguito {f.ActivityFanbase:+#;-#;0}");
            if (f.ActivityTeamRelation != 0) parts.Add($"rapporto con {f.TeamName} {f.ActivityTeamRelation:+#;-#;0}");
            if (f.ActivitySponsorRelation != 0) parts.Add($"rapporto con {f.SponsorName} {f.ActivitySponsorRelation:+#;-#;0}");
            if (f.ActivityFatigue != 0) parts.Add($"condizione {f.ActivityFatigue:+#;-#;0}");
            if (f.ActivityMoney != 0) parts.Add($"bilancio € {f.ActivityMoney:N0}");
            var ledger = parts.Count == 0 ? "nessuna conseguenza misurabile" : string.Join(", ", parts);
            var repetition = f.ActivityTimesDone > 1 ? $" È la {f.ActivityTimesDone}ª volta in stagione: il ritorno cala a ogni replica." : "";
            return bank.Pick("angle-activity-ledger",
                $"Il bilancio della decisione: {ledger}.{repetition} Nessuna di queste voci tocca un risultato di gara — quelli restano quelli prodotti da Assetto Corsa.",
                $"Sul registro della carriera restano {ledger}.{repetition} Sono numeri che pesano su mercato e contratti, non sulla pista.",
                $"Conseguenze messe a bilancio: {ledger}.{repetition} La pista continuerà a dire la sua per conto proprio.");
        }
    };

    private static readonly StoryAngle Outlook = new()
    {
        Name = "prospettiva",
        Relevance = _ => 20,
        Write = (f, bank) =>
        {
            var next = f.RoundsLeft > 0 ? $"Restano {f.RoundsLeft} round"
                : f.RoundCount > 0 ? "Il calendario è chiuso" : "Il prossimo appuntamento non è ancora fissato";
            return bank.Pick("angle-outlook",
                $"{next}. Da qui in avanti nulla è deciso a tavolino: la prossima pagina di questa carriera la scriverà la pista, non la vigilia.",
                $"{next} e una domanda sola sul tavolo: se questo passo è ripetibile. La risposta non la darà un'analisi, la darà la prossima sessione realmente conclusa.",
                f.HasTeam && f.HasSponsor
                    ? $"{next}. {f.TeamName} lavorerà sui dati raccolti, {f.SponsorName} guarderà i risultati e il mercato prenderà appunti. Tutto il resto è materia per il prossimo weekend."
                    : $"{next}. Nessuno, per ora, sta studiando questi dati al posto suo: il lavoro di analisi e la ricerca di chi lo sostenga sono la stessa cosa.",
                $"{next}: quello che conta è cosa dirà la prossima gara. Fino a quel momento, questa cronaca si ferma ai fatti già acquisiti.");
        }
    };

    public static readonly StoryAngle[] All =
    [
        Result, Withdrawal, WithdrawalCost, ActivityReport, ActivityConsequences, Milestone, Streak, Championship, Teammate, Rivalry, People,
        Qualifying, Conditions, Technical, Economy, SponsorAndTeam, Memory, OffTrack, Outlook
    ];
}
