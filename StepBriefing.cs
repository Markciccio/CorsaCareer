namespace CorsaCareer;

/// <summary>
/// Il racconto del passo attuale: cosa sta succedendo adesso, detto a parole.
///
/// La home mostrava un elenco di dati — auto, pista, tempo da ottenere — e il
/// giocatore leggeva una scheda tecnica invece di capire dove si trovava nella
/// propria storia. I dati servono, ma vengono dopo: prima va detto qual è il
/// passo, perché conta e cosa si rischia.
///
/// Ogni riga nasce dallo stato reale della carriera. Niente qui inventa
/// risultati, tempi o promesse che il motore non abbia già registrato.
/// </summary>
public sealed class StepBriefing
{
    /// <summary>Sopra tutto: dove siamo nella carriera.</summary>
    public string Kicker { get; init; } = "";
    /// <summary>Il titolo del passo, quello grande.</summary>
    public string Title { get; init; } = "";
    /// <summary>Il racconto: due o tre paragrafi brevi.</summary>
    public List<string> Paragraphs { get; init; } = [];
    /// <summary>
    /// Quando accade, raccontato: non una data secca ma il giorno con quanto
    /// manca. È la prima cosa che un pilota vuole sapere del passo successivo.
    /// </summary>
    public string When { get; init; } = "";
    /// <summary>La frase che dice cosa serve fare, in grassetto.</summary>
    public string Demand { get; init; } = "";
    /// <summary>Cosa si rischia concretamente.</summary>
    public string Stake { get; init; } = "";
    /// <summary>I dati tecnici, che restano disponibili ma in secondo piano.</summary>
    public List<(string Label, string Value)> Facts { get; init; } = [];
}

public static class StepBriefingBuilder
{
    /// <summary>
    /// Il «quando» detto come lo direbbe una persona: il giorno, e quanto manca.
    /// Una data secca non fa capire se c'è tempo per prepararsi o se si corre
    /// domani, ed è esattamente l'informazione che serve per decidere.
    /// </summary>
    public static string WhenText(DateTime eventDate, DateTime today, string what)
    {
        var days = (int)(eventDate.Date - today.Date).TotalDays;
        var when = days switch
        {
            < 0 => "in attesa da " + Plural(-days, "giorno", "giorni"),
            0 => "oggi",
            1 => "domani",
            < 7 => $"fra {days} giorni",
            < 14 => "fra una settimana",
            < 31 => $"fra {days / 7} settimane",
            _ => $"fra {Plural(days / 30, "mese", "mesi")}"
        };
        return $"{what} {when}, {eventDate:dddd d MMMM}.";
    }

    private static string Plural(int count, string one, string many) =>
        count == 1 ? $"un {one}" : $"{count} {many}";

    /// <summary>
    /// Compone il racconto del momento. <paramref name="awaitingResult"/> ha la
    /// precedenza su tutto: se una sessione è aperta, l'unico passo che conta è
    /// portarne dentro il referto.
    /// </summary>
    public static StepBriefing Build(CareerState career, ScheduledEvent? next, SelectionTrial? selection,
        bool awaitingResult, ContentCarRecord? car, ContentTrackRecord? track, string targetLap)
    {
        var driver = string.IsNullOrWhiteSpace(career.Driver) ? "il pilota" : career.Driver;
        var cash = $"€ {career.Cash:N0}";

        if (awaitingResult)
            return new StepBriefing
            {
                Kicker = "SESSIONE APERTA",
                Title = "Il risultato è là fuori",
                Paragraphs =
                [
                    "La sessione è stata preparata e consegnata ad Assetto Corsa. Adesso non serve altro: " +
                    "quello che accade in pista decide, e questa schermata aspetta.",
                    "Finché il referto non entra, la carriera resta ferma su questo punto. Nessun risultato " +
                    "viene inventato per farla avanzare."
                ],
                Demand = "Concludi la sessione, poi importa il referto.",
                Stake = "Una sessione abbandonata resta comunque un tentativo speso.",
                Facts = [("STATO", "in attesa del referto reale")]
            };

        // Una selezione aperta è il passo più importante che esista: occupa
        // giorni consecutivi e decide la categoria.
        if (selection?.NextDay is { } day)
        {
            var rung = CareerLadder.ById(selection.TargetRungId);
            var done = selection.Days.Count(x => x.Completed);
            return new StepBriefing
            {
                Kicker = $"SELEZIONE · GIORNO {day.Day} DI {selection.Days.Count}",
                Title = day.Day == 1 ? "Ti hanno chiamato" : "Non è ancora finita",
                Paragraphs = day.Day == 1
                    ?
                    [
                        $"{selection.OrganisedBy} ha deciso di guardarti da vicino. Non è una gara e non è un favore: " +
                        $"è un provino di {selection.Days.Count} giornate, con {selection.Candidates} candidati " +
                        $"per {selection.Seats} posti.",
                        $"Chi passa entra in {rung.Name.ToLowerInvariant()}. Chi non passa torna da dove è venuto, " +
                        "con la quota già pagata."
                    ]
                    :
                    [
                        $"{done} prove su {selection.Days.Count} sono andate. Il dossier è aperto e nessuna " +
                        "giornata da sola ha deciso qualcosa: si può recuperare, e si può buttare via tutto.",
                        SelectionDayResult.KindBrief(day.Kind)
                    ],
                When = WhenText(day.StoryDate, career.StoryDate,
                    $"{SelectionDayResult.KindLabel(day.Kind)} a {selection.TrackName}"),
                Demand = day.Day == 1
                    ? SelectionDayResult.KindBrief(day.Kind)
                    : $"Oggi conta questo: {SelectionDayResult.KindLabel(day.Kind).ToLowerInvariant()}.",
                Stake = day.Day == 1 && selection.NetCost > 0
                    ? $"La quota è di € {selection.NetCost:N0} sui {cash} che hai. Si paga entrando, non uscendo."
                    : "Una giornata sprecata non si recupera: le prove sono queste e finiscono.",
                Facts =
                [
                    ("PROVA", SelectionDayResult.KindLabel(day.Kind)),
                    ("SEDE", selection.TrackName),
                    ("IN GIOCO", $"{selection.Seats} posti in {rung.Name.ToLowerInvariant()}")
                ]
            };
        }

        if (selection != null)
            return new StepBriefing
            {
                Kicker = "SELEZIONE · PROVE CONCLUSE",
                Title = "Adesso decidono loro",
                Paragraphs =
                [
                    $"Le {selection.Days.Count} giornate sono finite. Quello che c'era da mostrare è stato mostrato, " +
                    "e il resto non dipende più da te.",
                    $"{selection.OrganisedBy} ha i tempi, i giri e la gara. Il verdetto arriva dai numeri che hai lasciato."
                ],
                Demand = "Apri il dossier e leggi il verdetto.",
                Stake = "Il giudizio è già scritto nei referti: non cambia più.",
                Facts = [("SEDE", selection.TrackName), ("PROVE", $"{selection.Days.Count} su {selection.Days.Count}")]
            };

        if (next == null)
        {
            var offers = career.Offers?.Count ?? 0;
            if (offers > 0)
            {
                var best = career.Offers!.OrderByDescending(x => x.Prestige).First();
                return new StepBriefing
                {
                    Kicker = "IL PADDOCK HA DECISO DI PARLARTI",
                    Title = "Qualcuno ti vuole",
                    Paragraphs =
                    [
                        $"Non è più questione di farsi notare: {offers} " +
                        (offers == 1 ? "squadra ha messo" : "squadre hanno messo") + " un sedile sul tavolo. " +
                        "Il tempo che hai girato è servito a questo.",
                        $"{best.Team} è la proposta più pesante, ma nessun contratto è gratis: ognuno chiede " +
                        "qualcosa e promette qualcos'altro. Firmare apre un calendario, e da lì si corre davvero."
                    ],
                    Demand = "Leggi le offerte, confrontale e scegli il primo sedile.",
                    Stake = "Un sedile sbagliato pesa per tutta la stagione: obiettivi, compagno e budget vengono con lui.",
                    Facts =
                    [
                        ("PROPOSTE", $"{offers} sul tavolo"),
                        ("LA MIGLIORE", $"{best.Team} · {UiText.Car(best.Car)}"),
                        ("CASSA", cash)
                    ]
                };
            }
            return new StepBriefing
            {
                Kicker = "NESSUN APPUNTAMENTO FISSATO",
                Title = "Nessuno ti sta cercando",
                Paragraphs =
                [
                    "Non c'è niente in calendario. Nessun test, nessuna gara, nessuno che aspetti " +
                    $"di vedere {driver} in pista.",
                    "Non è un vicolo cieco: è il momento in cui una carriera si costruisce fuori dall'asfalto. " +
                    "Attività, sponsor e contatti sono l'unico modo per far comparire la prossima occasione."
                ],
                Demand = "Cerca un'occasione: agenda, sponsor o mercato.",
                Stake = $"Con {cash} in cassa il tempo non è gratis: ogni settimana ferma è una settimana persa.",
                Facts = [("CASSA", cash), ("APPUNTAMENTI", "nessuno")]
            };
        }

        var trackName = track?.Name ?? (string.IsNullOrWhiteSpace(next.TrackName) ? next.TrackId : next.TrackName);
        var carName = car?.Name ?? career.Car;

        if (next.Kind == ScheduledEventKind.Invitation)
        {
            var fee = next.EntryFee;
            var proposer = string.IsNullOrWhiteSpace(next.ProposedBy) ? "un organizzatore locale" : next.ProposedBy;
            return new StepBriefing
            {
                // Auto, circuito e data sono elencati come dati nella card:
                // qui serve la frase che dice di che occasione si tratta, non
                // la loro ripetizione in forma di titolo.
                Kicker = "PROSSIMO APPUNTAMENTO",
                Title = "Una gara vera, non un test",
                Paragraphs =
                [
                    $"{proposer} offre un posto in griglia a {trackName}. Il cronometro qui non basta: " +
                    "conta la partenza, il traffico, e dove sei quando cala la bandiera.",
                    "Non assegna punti di campionato e costa denaro. Ma è una gara vera, davanti a gente " +
                    "che guarda — ed è il tipo di occasione che riapre un mercato chiuso."
                ],
                When = WhenText(next.Date, career.StoryDate, $"Si corre a {trackName}"),
                Demand = "Decidi se vale la spesa: accetta la gara o rinuncia e prepara altri test.",
                Stake = fee > career.Cash
                    ? $"Servono € {fee:N0} e in cassa ce ne sono {cash}: così non è sostenibile."
                    : $"L'iscrizione è di € {fee:N0} sui {cash} che hai. Un ritiro li brucia senza restituire niente.",
                Facts =
                [
                    ("ISCRIZIONE", $"€ {fee:N0}"),
                    ("CASSA", cash)
                ]
            };
        }

        if (next.IsTest)
        {
            var attempts = career.EvaluationAttempts;
            var first = attempts <= 0 && career.TestHistory.Count == 0;
            var specs = car == null
                ? "specifiche da definire"
                : string.Join(" · ", new[]
                {
                    car.PowerHp > 0 ? $"{car.PowerHp} CV" : "potenza non dichiarata",
                    car.MassKg > 0 ? $"{car.MassKg} kg" : "peso non dichiarato"
                });
            return new StepBriefing
            {
                Kicker = first ? "IL PRIMO CRONOMETRO" : $"TENTATIVO {attempts + 1}",
                Title = first ? "La prima occasione" : "Il dossier è ancora aperto",
                Paragraphs = first
                    ?
                    [
                        "Fino a adesso c'erano solo intenzioni, qualche contatto e la speranza che qualcuno " +
                        "concedesse una possibilità. Adesso c'è una pista davanti a te.",
                        "Nessuno chiede di vincere un campionato: ti danno qualche giro, una macchina modesta " +
                        "e un tempo da battere. È poco, ed è abbastanza — perché nel paddock nessuno sa ancora " +
                        "chi sei, e questa è la prima occasione per cambiarlo.",
                        "Il cronometro è l'unica cosa che conta."
                    ]
                    :
                    [
                        $"Il riferimento non è caduto, ma il dossier è ancora aperto: {trackName} è " +
                        "un'altra occasione, e le occasioni non sono infinite.",
                        "Quello che è mancato l'altra volta non si recupera con la fortuna. Si recupera con un giro pulito."
                    ],
                When = WhenText(next.Date, career.StoryDate, $"Si scende in pista a {trackName}"),
                Demand = $"Gira in {targetLap} o meglio.",
                Stake = first
                    ? $"Hai {cash}. Se il tempo non arriva, hai consumato una parte del budget senza ottenere niente."
                    : $"Restano {cash}. Ogni tentativo pesa sulla cassa e sulla pazienza di chi ti guarda.",
                Facts =
                [
                    ("AUTO", $"{carName} · {specs}"),
                    ("TEMPO", $"{targetLap} o meglio"),
                    ("CASSA", cash)
                ]
            };
        }

        // Round di campionato.
        var round = next.Round > 0 ? $"round {next.Round}" : "prossimo round";
        return new StepBriefing
        {
            Kicker = $"{career.Championship.ToUpperInvariant()} · {round.ToUpperInvariant()}",
            Title = "Questa volta si corre per punti",
            Paragraphs =
            [
                $"Non è un test: c'è una classifica che si muove e {career.Teammate} nello stesso box, " +
                "con cui verrai confrontato gara dopo gara.",
                string.IsNullOrWhiteSpace(career.ContractObjective)
                    ? "Il team guarda i risultati, non le intenzioni."
                    : $"Il contratto chiede una cosa precisa: {career.ContractObjective.ToLowerInvariant()}."
            ],
            When = WhenText(next.Date, career.StoryDate, $"Si corre a {trackName}"),
            Demand = string.IsNullOrWhiteSpace(next.Objective) ? "Porta a casa il miglior risultato possibile." : next.Objective,
            Stake = "L'esito muove reputazione, denaro, sponsor e mercato. Un ritiro costa due volte.",
            Facts =
            [
                ("AUTO", carName),
                ("PUNTI", $"{career.Points} in classifica"),
                ("CASSA", cash)
            ]
        };
    }
}
