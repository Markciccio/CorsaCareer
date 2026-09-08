namespace CorsaCareer1991;

public sealed record ActivityEffect(
    int Reputation = 0,
    int Fanbase = 0,
    int TeamRelation = 0,
    int SponsorRelation = 0,
    int Fatigue = 0,
    int Money = 0);

public sealed class ActivityRecord
{
    public int Season { get; set; }
    public int Round { get; set; }
    public DateTime StoryDate { get; set; }
    public string ActivityId { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Success { get; set; }
    public string Story { get; set; } = "";
    /// <summary>Decisione presa dal giocatore dentro l'attività, se prevista.</summary>
    public string ChoiceId { get; set; } = "";
    public string ChoiceLabel { get; set; } = "";
    public string ChoiceAngle { get; set; } = "";
    public int EffectiveRisk { get; set; }
    public int Days { get; set; }
    public int Reputation { get; set; }
    public int Fanbase { get; set; }
    public int TeamRelation { get; set; }
    public int SponsorRelation { get; set; }
    public int Fatigue { get; set; }
    public int Money { get; set; }
}

/// <summary>
/// Scelta dentro un'attività: è ciò che trasforma l'agenda da un elenco di
/// pulsanti in una decisione. Ogni opzione sposta il rischio e pesa gli effetti
/// in modo diverso, e produce un racconto diverso.
/// </summary>
public sealed record ActivityOption(
    string Id,
    string Label,
    string Description,
    int RiskDelta,
    ActivityEffect SuccessBonus,
    ActivityEffect SetbackPenalty,
    string Angle);

public sealed record ActivityDefinition(
    string Id,
    string Name,
    string Category,
    string Description,
    int Cost,
    int DurationDays,
    int Risk,
    ActivityEffect Success,
    ActivityEffect Setback,
    string SuccessStory,
    string SetbackStory,
    int MinimumTierRank = 0,
    string Question = "",
    IReadOnlyList<ActivityOption>? Options = null)
{
    public string CostLabel => Cost == 0 ? "nessun costo" : Cost > 0 ? $"€ -{Cost:N0}" : $"€ +{-Cost:N0}";
    public IReadOnlyList<ActivityOption> Choices => Options ?? [];
    public bool RequiresDecision => Choices.Count > 0;
}

public sealed class ActivityContext
{
    public string Tier { get; set; } = "Rookie";
    public int Season { get; set; } = 1;
    public int Round { get; set; }
    public int Fatigue { get; set; }
    public int Reputation { get; set; }
    public int Cash { get; set; }
    public int DaysAvailable { get; set; }
    /// <summary>Numero di volte in cui questa attività è già stata svolta in stagione.</summary>
    public int TimesDoneThisSeason { get; set; }
}

public sealed class ActivityOutcome
{
    public bool Performed { get; set; }
    public bool Success { get; set; }
    public string Reason { get; set; } = "";
    public ActivityEffect Effect { get; set; } = new();
    public string Story { get; set; } = "";
    public List<string> Notes { get; set; } = [];
    /// <summary>Scelta compiuta dal giocatore, se l'attività la prevedeva.</summary>
    public ActivityOption? Choice { get; set; }
    public int EffectiveRisk { get; set; }
}

/// <summary>
/// Vita professionale fra i weekend: la sezione della specifica che non aveva
/// nessuna controparte nel codice.
///
/// Regola di integrità: nessuna di queste attività può influenzare un risultato
/// di gara. I risultati arrivano soltanto da Assetto Corsa. Le attività agiscono
/// su reputazione, seguito, rapporto col team, rapporto con lo sponsor, budget e
/// stanchezza — cioè su mercato, contratti ed economia, non sulla guida.
///
/// L'esito è deterministico da un seme stabile (stagione, round, attività): la
/// stessa scelta nello stesso momento produce sempre lo stesso risultato, così il
/// comportamento è riproducibile e verificabile dai test.
/// </summary>
public static class OffTrackActivities
{
    public const int MaxFatigue = 100;
    /// <summary>Sopra questa soglia la stanchezza riduce i guadagni delle attività.</summary>
    public const int FatigueWarning = 60;

    private static readonly ActivityDefinition[] All =
    [
        new("fan-meeting", "Incontro con i tifosi", "Pubblico",
            "Una giornata di firme e foto con il pubblico: costruisce seguito ma toglie tempo alla preparazione.",
            1500, 2, 10,
            new ActivityEffect(Reputation: 2, Fanbase: 8, Fatigue: 6),
            new ActivityEffect(Fanbase: 2, Fatigue: 8),
            "L'incontro con i tifosi riempie il piazzale: il seguito del pilota cresce.",
            "L'affluenza è modesta e l'evento passa quasi inosservato.",
            Question: "Che formato scegli?",
            Options:
            [
                new("lungo", "Sessione a oltranza", "Resti finché c'è gente: il seguito cresce, la condizione paga.",
                    4, new ActivityEffect(Fanbase: 8, Fatigue: 6), new ActivityEffect(Fatigue: 8), "maratona con il pubblico"),
                new("mirato", "Breve e mirato", "Un'ora, foto e via: ordinato ed efficiente.",
                    -8, new ActivityEffect(Fanbase: 2), new ActivityEffect(Fanbase: -1), "incontro contenuto"),
                new("scuole", "Portalo in una scuola", "Cambi pubblico: meno tifosi, più immagine pubblica.",
                    0, new ActivityEffect(Reputation: 4, Fanbase: 4), new ActivityEffect(Fatigue: 4), "iniziativa nelle scuole")
            ]),

        new("autograph-session", "Sessione autografi", "Pubblico",
            "Un pomeriggio in concessionaria o al circuito, richiesto dagli sponsor commerciali.",
            0, 1, 8,
            new ActivityEffect(Fanbase: 5, SponsorRelation: 4, Fatigue: 3),
            new ActivityEffect(Fanbase: 1, Fatigue: 4),
            "La sessione autografi va bene e lo sponsor apprezza la disponibilità.",
            "Pochi presenti: lo sponsor si aspettava di più."),

        new("driving-school", "Giornata da istruttore", "Pubblico",
            "Insegnare in una scuola guida porta un compenso e migliora l'immagine professionale.",
            -6000, 2, 12,
            new ActivityEffect(Reputation: 1, Fanbase: 3, Fatigue: 7),
            new ActivityEffect(Fatigue: 9),
            "La giornata da istruttore è pagata e ben recensita dagli allievi.",
            "Giornata faticosa e organizzazione confusa: nessun ritorno d'immagine."),

        new("sponsor-event", "Evento dello sponsor", "Sponsor",
            "Presenza obbligata a un evento commerciale: salva il budget ma consuma il fine settimana libero.",
            -15000, 3, 15,
            new ActivityEffect(SponsorRelation: 10, Fanbase: 3, Fatigue: 10),
            new ActivityEffect(SponsorRelation: -4, Fatigue: 12),
            "L'evento dello sponsor va liscio: il rapporto commerciale si rafforza e arriva un contributo.",
            "Una dichiarazione fuori posto raffredda il rapporto con lo sponsor.",
            Question: "Come ti presenti all'evento?",
            Options:
            [
                new("commerciale", "Massima disponibilità", "Foto, interviste, clienti: lo sponsor è soddisfatto e la settimana è andata.",
                    -8, new ActivityEffect(SponsorRelation: 8, Money: 8000), new ActivityEffect(Fatigue: 6), "presenza commerciale piena"),
                new("tecnico", "Resta sul tecnico", "Parli di macchina e sviluppo: meno spendibile, più credibile.",
                    2, new ActivityEffect(Reputation: 4, TeamRelation: 4), new ActivityEffect(SponsorRelation: -5), "taglio tecnico"),
                new("breve", "Passaggio breve", "Onori l'impegno e torni al lavoro: nessuno esulta, nessuno protesta.",
                    -14, new ActivityEffect(Fatigue: -6), new ActivityEffect(SponsorRelation: -3), "presenza minima")
            ]),

        new("car-launch", "Presentazione della vettura", "Sponsor",
            "La presentazione ufficiale del progetto: molta stampa, molte aspettative da gestire.",
            0, 2, 20,
            new ActivityEffect(Reputation: 2, Fanbase: 6, TeamRelation: 3, SponsorRelation: 5, Fatigue: 5),
            new ActivityEffect(Reputation: -1, Fanbase: 2, Fatigue: 6),
            "La presentazione riesce: squadra e sponsor mostrano un progetto credibile.",
            "La presentazione è sottotono e la stampa nota le incertezze."),

        new("press-day", "Giornata stampa", "Media",
            "Interviste in serie: può costruire reputazione o generare un titolo scomodo.",
            0, 1, 28,
            new ActivityEffect(Reputation: 4, Fanbase: 4, Fatigue: 4),
            new ActivityEffect(Reputation: -3, Fanbase: 2, TeamRelation: -3, Fatigue: 5),
            "Le interviste vanno bene: il pilota comunica con lucidità e la redazione lo nota.",
            "Una frase infelice diventa il titolo del giorno e il box non è contento.",
            Question: "Su cosa imposti la giornata stampa?",
            Options:
            [
                new("difendi-squadra", "Difendi la squadra", "Copri il lavoro del box e sposti l'attenzione dai risultati: piace dentro, meno fuori.",
                    -10, new ActivityEffect(TeamRelation: 7), new ActivityEffect(Fanbase: -2), "difesa del progetto"),
                new("numeri", "Porta i tuoi numeri", "Metti sul tavolo il confronto interno: costruisce reputazione, ma il box si irrigidisce.",
                    4, new ActivityEffect(Reputation: 4, Fanbase: 3), new ActivityEffect(TeamRelation: -5), "rivendicazione dei propri dati"),
                new("ambizione", "Dichiara le ambizioni", "Dici dove vuoi arrivare: il mercato ascolta, e anche lo sponsor.",
                    10, new ActivityEffect(Reputation: 6, Fanbase: 6), new ActivityEffect(SponsorRelation: -6, TeamRelation: -4), "dichiarazione di ambizione")
            ]),

        new("podcast", "Ospite in un podcast motoristico", "Media",
            "Formato lungo, pubblico appassionato: ottimo per il seguito, rischioso sulle polemiche.",
            0, 1, 22,
            new ActivityEffect(Fanbase: 9, Reputation: 2, Fatigue: 3),
            new ActivityEffect(Fanbase: 3, Reputation: -2, Fatigue: 3),
            "La puntata gira molto negli ambienti del motorsport: il seguito cresce.",
            "Un passaggio dell'intervista viene estrapolato e crea rumore inutile.",
            Question: "Che registro tieni nella puntata?",
            Options:
            [
                new("tecnico", "Racconto tecnico", "Spieghi come si guida davvero: pubblico piccolo, molto fedele.",
                    -10, new ActivityEffect(Reputation: 5, Fanbase: 4), new ActivityEffect(), "racconto tecnico"),
                new("personale", "Racconto personale", "Parli del percorso e delle difficoltà: arriva a più gente.",
                    2, new ActivityEffect(Fanbase: 10), new ActivityEffect(Fanbase: 2), "racconto personale"),
                new("polemico", "Affronta le polemiche", "Rispondi alle critiche: massima visibilità, massimo rischio.",
                    18, new ActivityEffect(Fanbase: 14, Reputation: 4), new ActivityEffect(TeamRelation: -6, SponsorRelation: -6), "risposta alle critiche")
            ]),

        new("photoshoot", "Servizio fotografico", "Media",
            "Materiale per stampa e sponsor, con una giornata in studio o al circuito.",
            2500, 1, 10,
            new ActivityEffect(Fanbase: 5, SponsorRelation: 5, Fatigue: 3),
            new ActivityEffect(Fanbase: 1, Fatigue: 4),
            "Il servizio fotografico produce materiale che sponsor e testate riprendono.",
            "Le immagini non convincono e vengono usate poco."),

        new("engineer-debrief", "Riunione con gli ingegneri", "Tecnica",
            "Debrief tecnico approfondito sui dati raccolti: consolida il rapporto con la squadra.",
            0, 2, 8,
            new ActivityEffect(TeamRelation: 9, Reputation: 1, Fatigue: 5),
            new ActivityEffect(TeamRelation: 2, Fatigue: 6),
            "Il debrief è produttivo: gli ingegneri riconoscono al pilota una lettura precisa.",
            "La riunione si perde in dettagli e non porta conclusioni condivise.",
            Question: "Su cosa spingi nel debrief?",
            Options:
            [
                new("assetto", "Insisti sull'assetto", "Lavori sul bilanciamento: se hai ragione il box lo riconosce.",
                    4, new ActivityEffect(TeamRelation: 6, Reputation: 3), new ActivityEffect(TeamRelation: -3), "richiesta sull'assetto"),
                new("affidabilita", "Chiedi affidabilità", "Meno prestazione, meno ritiri: poco spettacolare, molto sensato.",
                    -8, new ActivityEffect(TeamRelation: 7), new ActivityEffect(TeamRelation: 1), "priorità all'affidabilità"),
                new("ascolta", "Ascolta e prendi appunti", "Lasci parlare gli ingegneri: nessun rischio, nessuno scatto.",
                    -12, new ActivityEffect(TeamRelation: 3), new ActivityEffect(), "ascolto del gruppo tecnico")
            ]),

        new("team-manager-meeting", "Incontro con il team manager", "Tecnica",
            "Una conversazione sul futuro: aspettative, ruolo in squadra e prospettive di rinnovo.",
            0, 1, 18,
            new ActivityEffect(TeamRelation: 8, Reputation: 2, Fatigue: 2),
            new ActivityEffect(TeamRelation: -5, Fatigue: 3),
            "L'incontro chiarisce il ruolo del pilota nel progetto e rafforza la fiducia.",
            "Le posizioni restano distanti: il rapporto con la squadra si irrigidisce."),

        new("simulator", "Sessione al simulatore", "Preparazione",
            "Lavoro al simulatore della squadra: non cambia il risultato in pista, ma il box lo apprezza.",
            4000, 2, 10,
            new ActivityEffect(TeamRelation: 6, Reputation: 1, Fatigue: 8),
            new ActivityEffect(TeamRelation: 1, Fatigue: 10),
            "La sessione al simulatore produce indicazioni utili al gruppo tecnico.",
            "Problemi al simulatore: la sessione si chiude senza indicazioni chiare."),

        new("technical-coaching", "Coaching tecnico", "Preparazione",
            "Un coach lavora sulla guida e sulla gestione della gara.",
            9000, 3, 12,
            new ActivityEffect(Reputation: 3, TeamRelation: 4, Fatigue: 9),
            new ActivityEffect(Fatigue: 11),
            "Il lavoro con il coach viene notato: il pilota appare più maturo.",
            "Il metodo del coach non funziona e il ciclo di lavoro va rivisto."),

        new("physical-training", "Blocco di preparazione fisica", "Preparazione",
            "Settimana dedicata all'allenamento: l'unica attività che riduce davvero la stanchezza.",
            3000, 4, 5,
            new ActivityEffect(Reputation: 1, Fatigue: -22),
            new ActivityEffect(Fatigue: -8),
            "Il blocco di preparazione va come previsto: il pilota recupera condizione.",
            "Un piccolo problema fisico limita il programma di allenamento."),

        new("rest", "Giorni di riposo", "Preparazione",
            "Nessun impegno: recupero puro, senza ritorno d'immagine.",
            0, 3, 0,
            new ActivityEffect(Fatigue: -16),
            new ActivityEffect(Fatigue: -16),
            "Giorni di stacco completo: la stanchezza rientra.",
            "Giorni di stacco completo: la stanchezza rientra."),

        new("contract-negotiation", "Negoziazione del contratto", "Carriera",
            "Sedersi al tavolo per rivedere le condizioni: può migliorare l'accordo o incrinare il rapporto.",
            0, 2, 32,
            new ActivityEffect(TeamRelation: 4, Reputation: 2, Money: 12000),
            new ActivityEffect(TeamRelation: -8, Reputation: -1),
            "La negoziazione porta un miglioramento delle condizioni economiche.",
            "La trattativa si chiude male: la squadra registra la richiesta come un problema.",
            MinimumTierRank: 1,
            Question: "Cosa metti sul tavolo?",
            Options:
            [
                new("stipendio", "Più stipendio", "La richiesta più diretta, e la più facile da rifiutare.",
                    8, new ActivityEffect(Money: 22000), new ActivityEffect(TeamRelation: -6), "richiesta economica"),
                new("test", "Più giornate di test", "Chiedi mezzi invece di denaro: la squadra lo legge come professionalità.",
                    -8, new ActivityEffect(TeamRelation: 8, Reputation: 3), new ActivityEffect(Money: -3000), "richiesta tecnica"),
                new("prima-guida", "Lo status di prima guida", "La posta più alta: cambia tutto oppure incrina il rapporto.",
                    16, new ActivityEffect(Reputation: 8, TeamRelation: 6), new ActivityEffect(TeamRelation: -12, SponsorRelation: -4), "braccio di ferro sul ruolo")
            ]),

        new("factory-visit", "Visita in fabbrica", "Carriera",
            "Una giornata nella sede del costruttore: contatti che contano per il mercato.",
            2000, 2, 15,
            new ActivityEffect(Reputation: 4, TeamRelation: 5, Fatigue: 6),
            new ActivityEffect(Reputation: 1, Fatigue: 7),
            "La visita in fabbrica apre contatti utili nel paddock.",
            "Visita di cortesia senza incontri significativi.",
            MinimumTierRank: 2)
    ];

    public static IReadOnlyList<ActivityDefinition> Catalog(string tier)
    {
        var rank = ProgressionEngine.TierRank(tier);
        return All.Where(x => x.MinimumTierRank <= rank).ToList();
    }

    public static ActivityDefinition? Find(string id) => All.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>Giorni disponibili fra due round, al netto del viaggio.</summary>
    public static int DaysBetweenRounds() => NarrativeCalendar.DaysBetweenRounds - 4;

    public static ActivityOutcome Resolve(ActivityDefinition definition, ActivityContext context, string? chosenOptionId = null)
    {
        var outcome = new ActivityOutcome();
        var choice = definition.Choices.FirstOrDefault(x => x.Id.Equals(chosenOptionId, StringComparison.OrdinalIgnoreCase));
        if (definition.RequiresDecision && choice == null)
        {
            outcome.Reason = $"{definition.Name} richiede una decisione: {definition.Question}";
            return outcome;
        }
        outcome.Choice = choice;
        if (context.DaysAvailable < definition.DurationDays)
        {
            outcome.Reason = $"Servono {definition.DurationDays} giorni ma ne restano {context.DaysAvailable} prima del prossimo weekend.";
            return outcome;
        }
        if (definition.Cost > 0 && context.Cash < definition.Cost)
        {
            outcome.Reason = $"Budget insufficiente: servono € {definition.Cost:N0}, disponibili € {context.Cash:N0}.";
            return outcome;
        }

        outcome.Performed = true;
        // Esito deterministico, ma dipendente anche dalla scelta: due decisioni
        // diverse nello stesso momento non possono produrre lo stesso esito.
        var seed = StableSeed(context.Season, context.Round, definition.Id + "|" + (choice?.Id ?? ""), context.TimesDoneThisSeason);
        var roll = (int)(seed % 100);
        // La stanchezza alta rende più probabile un esito negativo: è l'unico
        // effetto meccanico della fatica, e resta fuori dalla pista.
        var fatiguePenalty = context.Fatigue > FatigueWarning ? (context.Fatigue - FatigueWarning) / 4 : 0;
        // Ripetere la stessa attività nella stessa stagione rende meno.
        var repetitionPenalty = context.TimesDoneThisSeason * 6;
        var choiceRisk = choice?.RiskDelta ?? 0;
        var effectiveRisk = Math.Clamp(definition.Risk + fatiguePenalty + repetitionPenalty + choiceRisk, 0, 92);
        outcome.EffectiveRisk = effectiveRisk;
        outcome.Success = roll >= effectiveRisk;
        if (choice != null && choiceRisk != 0) outcome.Notes.Add($"Scelta «{choice.Label}»: rischio {(choiceRisk > 0 ? "aumentato" : "ridotto")} di {Math.Abs(choiceRisk)} punti.");
        if (fatiguePenalty > 0) outcome.Notes.Add($"Stanchezza {context.Fatigue}/100: rischio aumentato di {fatiguePenalty} punti.");
        if (repetitionPenalty > 0) outcome.Notes.Add($"Attività già svolta {context.TimesDoneThisSeason} volta/e in stagione: rischio aumentato di {repetitionPenalty} punti.");
        outcome.Notes.Add($"Rischio effettivo {effectiveRisk}% · esito calcolato dal momento della carriera, non casuale.");

        var raw = outcome.Success ? definition.Success : definition.Setback;
        // La scelta somma il proprio effetto: è ciò che la rende una decisione e
        // non una preferenza estetica.
        if (choice != null) raw = Combine(raw, outcome.Success ? choice.SuccessBonus : choice.SetbackPenalty);
        // Con molta stanchezza i guadagni si riducono; il recupero no.
        var gainFactor = context.Fatigue > FatigueWarning ? 0.6 : 1.0;
        if (gainFactor < 1.0) outcome.Notes.Add("I guadagni sono ridotti del 40% per la stanchezza accumulata.");
        outcome.Effect = new ActivityEffect(
            Reputation: Scale(raw.Reputation, gainFactor),
            Fanbase: Scale(raw.Fanbase, gainFactor),
            TeamRelation: Scale(raw.TeamRelation, gainFactor),
            SponsorRelation: Scale(raw.SponsorRelation, gainFactor),
            Fatigue: raw.Fatigue,
            Money: Scale(raw.Money, gainFactor) - definition.Cost);
        outcome.Story = outcome.Success ? definition.SuccessStory : definition.SetbackStory;
        if (choice != null) outcome.Story += $" Linea scelta: {choice.Angle}.";
        return outcome;
    }

    private static ActivityEffect Combine(ActivityEffect a, ActivityEffect b) => new(
        Reputation: a.Reputation + b.Reputation,
        Fanbase: a.Fanbase + b.Fanbase,
        TeamRelation: a.TeamRelation + b.TeamRelation,
        SponsorRelation: a.SponsorRelation + b.SponsorRelation,
        Fatigue: a.Fatigue + b.Fatigue,
        Money: a.Money + b.Money);

    private static int Scale(int value, double factor) => value <= 0 ? value : (int)Math.Round(value * factor);

    internal static long StableSeed(int season, int round, string activityId, int repetition)
    {
        var text = $"{season}|{round}|{activityId?.ToLowerInvariant()}|{repetition}";
        var hash = 2166136261L;
        foreach (var character in text) hash = (hash ^ character) * 16777619 % 2147483647;
        return Math.Abs(hash);
    }

    public static string FatigueLabel(int fatigue) => fatigue switch
    {
        <= 20 => "fresco",
        <= 40 => "in condizione",
        <= 60 => "affaticato",
        <= 80 => "molto affaticato",
        _ => "al limite"
    };

    public static string RelationLabel(int value) => value switch
    {
        >= 70 => "solido",
        >= 45 => "positivo",
        >= 25 => "neutro",
        >= 10 => "teso",
        _ => "critico"
    };
}
