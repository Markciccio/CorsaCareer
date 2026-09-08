namespace CorsaCareer1991;

/// <summary>
/// Le attività che si possono inserire in una giornata.
///
/// Sono poche di proposito. Una giornata deve essere due, tre, quattro
/// decisioni — non venticinque: un elenco lungo diventa una lista della spesa da
/// sbrigare, e la scelta smette di pesare.
///
/// Le attività che allenano o riposano hanno esito certo. Quelle che dipendono
/// da altre persone — tifosi, social, trattative — no: il risultato dipende da
/// chi sei in quel momento, e questo è il motivo per cui vale la pena
/// costruirsi un seguito prima di provarci.
/// </summary>
public static class DayActivityCatalog
{
    // ------------------------------------------------------------- il pilota

    public static IReadOnlyList<DayActivity> ForDriver() =>
    [
        new()
        {
            Id = "palestra", Name = "Palestra", Actor = DayActor.Driver, Focus = DayFocus.Fisico, Hours = 2,
            Promise = "Due ore di lavoro fisico. Alza la forma, ma stanca.",
            Outcomes =
            [
                new()
                {
                    Line = "Sessione completa: collo, core, riflessi. Domani si sentirà.",
                    Effects = [new(DayEffectKind.Fitness, 6), new(DayEffectKind.Fatigue, 10)]
                }
            ]
        },
        new()
        {
            Id = "corsa", Name = "Corsa", Actor = DayActor.Driver, Focus = DayFocus.Fisico, Hours = 1,
            Promise = "Un'ora di fondo. Meno efficace della palestra, ma costa meno fatica.",
            Outcomes =
            [
                new()
                {
                    Line = "Dieci chilometri senza forzare. Il fiato migliora.",
                    Effects = [new(DayEffectKind.Fitness, 3), new(DayEffectKind.Fatigue, 5)]
                }
            ]
        },
        new()
        {
            Id = "riposo", Name = "Riposo", Actor = DayActor.Driver, Focus = DayFocus.Fisico, Hours = 2,
            Promise = "Due ore senza fare niente. Recupera stanchezza, non alza nulla.",
            Outcomes =
            [
                new()
                {
                    Line = "Nessun allenamento, nessun impegno. Il corpo si riprende.",
                    Effects = [new(DayEffectKind.Fatigue, -18)]
                }
            ]
        },
        new()
        {
            Id = "massaggio", Name = "Massaggio", Actor = DayActor.Driver, Focus = DayFocus.Fisico, Hours = 1, Cost = 40,
            Promise = "Un'ora dal fisioterapista. Recupero rapido, ma si paga.",
            Outcomes =
            [
                new()
                {
                    Line = "Un'ora di lavoro sui muscoli: la schiena smette di tirare.",
                    Effects = [new(DayEffectKind.Fatigue, -14), new(DayEffectKind.Fitness, 1), new(DayEffectKind.Money, -40)]
                }
            ]
        },

        // --- le uscite pubbliche: esito incerto
        new()
        {
            Id = "tifosi", Name = "Incontro con i tifosi", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 2,
            Promise = "Due ore al circuito o in un locale. Può farti conoscere, o passare inosservato.",
            Outcomes =
            [
                new()
                {
                    Line = "Si presentano in tre, due sono lì per caso. Un pomeriggio buttato.",
                    Weight = 3, IsSetback = true,
                    Effects = [new(DayEffectKind.Fatigue, 6)]
                },
                new()
                {
                    Line = "Una ventina di persone, qualche foto, un paio di domande vere.",
                    Weight = 5,
                    Effects = [new(DayEffectKind.Popularity, 3), new(DayEffectKind.Fatigue, 6)]
                },
                new()
                {
                    Line = "Più gente del previsto. Qualcuno ha portato il figlio, e il figlio voleva il tuo autografo.",
                    Weight = 2,
                    Effects = [new(DayEffectKind.Popularity, 7), new(DayEffectKind.Fatigue, 8)]
                }
            ]
        },
        new()
        {
            Id = "social", Name = "Post sui social", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 1,
            Promise = "Un'ora per raccontare l'ultimo weekend. Può funzionare, o ritorcersi contro.",
            Outcomes =
            [
                new()
                {
                    Line = "Il tono non è passato: qualcuno legge un'accusa dove non c'era.",
                    Weight = 2, IsSetback = true,
                    Effects = [new(DayEffectKind.Popularity, -2)]
                },
                new()
                {
                    Line = "Poche reazioni. Il post scorre via come tutti gli altri.",
                    Weight = 5,
                    Effects = []
                },
                new()
                {
                    Line = "Il racconto piace: viene ripreso da un paio di pagine locali.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 3)]
                },
                new()
                {
                    Line = "Il post gira più del previsto. Qualcuno che conta lo ha letto.",
                    Weight = 1,
                    Effects = [new(DayEffectKind.Popularity, 8), new(DayEffectKind.SportingReputation, 2)]
                }
            ]
        },
        new()
        {
            Id = "pr", Name = "Relazioni pubbliche", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 3,
            Promise = "Mezza giornata fra circoli, officine e giornali locali. Lenta, ma apre porte.",
            Outcomes =
            [
                new()
                {
                    Line = "Tante strette di mano e nessuna conclusione.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Fatigue, 8)]
                },
                new()
                {
                    Line = "Un cronista locale prende nota del nome. È già qualcosa.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 2), new(DayEffectKind.SportingReputation, 2), new(DayEffectKind.Fatigue, 8)]
                },
                new()
                {
                    Line = "Un contatto serio: qualcuno vuole rivederti con calma.",
                    Weight = 2,
                    Effects = [new(DayEffectKind.Popularity, 4), new(DayEffectKind.SportingReputation, 4), new(DayEffectKind.Fatigue, 8)]
                }
            ]
        },
        // --- il lavoro sull'immagine, oltre al singolo post
        //
        // Costruirsi un seguito non è una cosa sola ripetuta: è accompagnare
        // chi tratta gli sponsor, farsi vedere accanto a chi ha già pubblico,
        // raccontare l'allenamento, rispondere a chi ti segue. Ognuna costa
        // ore e può andare male: è questo che la rende una scelta.
        new()
        {
            Id = "haru-accompagna", Name = "Accompagna Haru dagli sponsor", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 3,
            Promise = "Mezza giornata con Haru nelle visite. Vedere il pilota in faccia cambia la conversazione — o la irrigidisce.",
            Outcomes =
            [
                new()
                {
                    Line = "Il titolare parla solo con Haru e ti ignora. Sei rimasto in piedi per tre ore.",
                    Weight = 3, IsSetback = true,
                    Effects = [new(DayEffectKind.Fatigue, 8)]
                },
                new()
                {
                    Line = "Presenze cordiali, nessuna firma. Haru dice che serviva farti conoscere.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 2), new(DayEffectKind.Fatigue, 8)]
                },
                new()
                {
                    Line = "Il tuo racconto della gara convince più di qualsiasi cartellina: chiedono di rivedervi.",
                    Weight = 3,
                    Effects = [new(DayEffectKind.Popularity, 5), new(DayEffectKind.SportingReputation, 3), new(DayEffectKind.Fatigue, 8)]
                }
            ]
        },
        new()
        {
            Id = "youtube", Name = "Video con un influencer", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 3,
            Promise = "Una giornata su un canale che ha già pubblico. Grande portata, controllo zero su come vieni montato.",
            Outcomes =
            [
                new()
                {
                    Line = "Ti montano come la spalla comica della puntata. I commenti non parlano di guida.",
                    Weight = 3, IsSetback = true,
                    Effects = [new(DayEffectKind.Popularity, 4), new(DayEffectKind.SportingReputation, -3), new(DayEffectKind.Fatigue, 10)]
                },
                new()
                {
                    Line = "Video onesto, numeri normali. Qualche follower nuovo, niente di più.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 6), new(DayEffectKind.Fatigue, 10)]
                },
                new()
                {
                    Line = "Il pezzo in cui spieghi la staccata gira da solo: lo condividono anche gli addetti ai lavori.",
                    Weight = 3,
                    Effects = [new(DayEffectKind.Popularity, 14), new(DayEffectKind.SportingReputation, 3), new(DayEffectKind.Fatigue, 10)]
                }
            ]
        },
        new()
        {
            Id = "instagram-allenamento", Name = "Racconta l'allenamento sui social", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 1,
            Promise = "Un'ora per trasformare la palestra in contenuto. Poco tempo, resa piccola ma quasi sempre positiva.",
            Outcomes =
            [
                new()
                {
                    Line = "Le solite facce: i tuoi venti affezionati. Almeno loro ci sono sempre.",
                    Weight = 5,
                    Effects = [new(DayEffectKind.Popularity, 2)]
                },
                new()
                {
                    Line = "La serie sull'allenamento piace: qualcuno comincia a seguirti per il metodo, non per i risultati.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 5)]
                },
                new()
                {
                    Line = "Un preparatore conosciuto commenta il tuo lavoro sul collo. Da lì arrivano parecchi occhi nuovi.",
                    Weight = 2,
                    Effects = [new(DayEffectKind.Popularity, 9), new(DayEffectKind.SportingReputation, 2)]
                }
            ]
        },
        new()
        {
            Id = "domande-follower", Name = "Rispondi ai follower", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 1,
            Promise = "Un'ora a rispondere davvero, uno per uno. Non esplode mai, ma non si spegne più.",
            Outcomes =
            [
                new()
                {
                    Line = "Finisci nei commenti sbagliati e ci perdi mezz'ora e un po' di umore.",
                    Weight = 2, IsSetback = true,
                    Effects = [new(DayEffectKind.Popularity, -1)]
                },
                new()
                {
                    Line = "Conversazioni normali, qualche grazie sincero. Il rapporto tiene.",
                    Weight = 6,
                    Effects = [new(DayEffectKind.Popularity, 3)]
                },
                new()
                {
                    Line = "Una tua risposta lunga sulla paura in gara viene ricondivisa ovunque.",
                    Weight = 2,
                    Effects = [new(DayEffectKind.Popularity, 7)]
                }
            ]
        },
        new()
        {
            Id = "intervista-radio", Name = "Intervista a una radio locale", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 2,
            Promise = "Due ore fra viaggio e diretta. Pubblico locale, ma è gente che poi viene al circuito.",
            Outcomes =
            [
                new()
                {
                    Line = "Domande generiche e poco tempo. Esci senza aver detto niente di tuo.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 1), new(DayEffectKind.Fatigue, 5)]
                },
                new()
                {
                    Line = "Il conduttore si appassiona e ti lascia parlare: mezz'ora tutta per te.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 5), new(DayEffectKind.SportingReputation, 2), new(DayEffectKind.Fatigue, 5)]
                },
                new()
                {
                    Line = "Un piccolo sponsor sente la puntata e chiede il numero di Haru.",
                    Weight = 2,
                    Effects = [new(DayEffectKind.Popularity, 6), new(DayEffectKind.SportingReputation, 3), new(DayEffectKind.Money, 120), new(DayEffectKind.Fatigue, 5)]
                }
            ]
        },
        new()
        {
            Id = "scuola-kart", Name = "Giornata con i ragazzi del kartodromo", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 3,
            Promise = "Mezza giornata a insegnare a chi comincia. Non paga, ma nel paddock si sa chi lo fa.",
            Outcomes =
            [
                new()
                {
                    Line = "Pochi iscritti e molta pioggia. Resta una giornata spesa bene, e basta.",
                    Weight = 3,
                    Effects = [new(DayEffectKind.Popularity, 2), new(DayEffectKind.Fatigue, 9)]
                },
                new()
                {
                    Line = "I ragazzi ti stanno dietro tutto il giorno; i genitori fotografano tutto.",
                    Weight = 4,
                    Effects = [new(DayEffectKind.Popularity, 6), new(DayEffectKind.Fatigue, 9)]
                },
                new()
                {
                    Line = "Il gestore del kartodromo ti ringrazia pubblicamente: la scuola ti vuole come istruttore fisso.",
                    Weight = 3,
                    Effects = [new(DayEffectKind.Popularity, 8), new(DayEffectKind.SportingReputation, 4), new(DayEffectKind.Money, 90), new(DayEffectKind.Fatigue, 9)]
                }
            ]
        },
        new()
        {
            Id = "lavoro", Name = "Giornata di lavoro", Actor = DayActor.Driver, Focus = DayFocus.Altro, Hours = 4,
            Promise = "Mezza giornata a fare altro per mettere insieme qualcosa. Non fa carriera, ma paga.",
            Outcomes =
            [
                new()
                {
                    Line = "Quattro ore in officina a dare una mano. Pochi soldi, ma sono soldi.",
                    Effects = [new(DayEffectKind.Money, 70), new(DayEffectKind.Fatigue, 14)]
                }
            ]
        }
    ];

    // --------------------------------------------------------------- Haru

    /// <summary>
    /// Le attività di Haru Senda.
    ///
    /// Il rapporto rischio/rendimento è il punto: l'officina sotto casa dice
    /// quasi sempre di sì per pochi soldi, il produttore di pneumatici quasi
    /// sempre di no per molti. Dove far lavorare Haru è una strategia parallela
    /// a quella del pilota.
    /// </summary>
    public static IReadOnlyList<DayActivity> ForAgent() =>
    [
        new()
        {
            Id = "haru-officina", Name = "Haru: officina di quartiere", Actor = DayActor.Agent, Hours = 2,
            Promise = "Probabilità alta, cifra piccola. Il tipo di sponsor che si trova sotto casa.",
            Outcomes =
            [
                new()
                {
                    Line = "«Mi dispiace, quest'anno ho già dato.» Niente da fare.",
                    Weight = 2, IsSetback = true, Effects = []
                },
                new()
                {
                    Line = "«Cento euro te li do. L'adesivo però lo voglio bene in vista.»",
                    Weight = 6, Effects = [new(DayEffectKind.Money, 100)]
                },
                new()
                {
                    Line = "«Facciamo centocinquanta, e vengo a vederti domenica.»",
                    Weight = 2, Effects = [new(DayEffectKind.Money, 150), new(DayEffectKind.Popularity, 1)]
                }
            ]
        },
        new()
        {
            Id = "haru-ricambi", Name = "Haru: ricambista", Actor = DayActor.Agent, Hours = 3,
            Promise = "Probabilità media, cifra media. Serve qualche risultato per convincerli.",
            Outcomes =
            [
                new()
                {
                    Line = "«Il ragazzo non lo conosco. Fammi vedere qualche risultato.»",
                    Weight = 5, IsSetback = true, Effects = []
                },
                new()
                {
                    Line = "«Duecentocinquanta, e ti do anche una scorta di ricambi.»",
                    Weight = 4, Effects = [new(DayEffectKind.Money, 250)]
                },
                new()
                {
                    Line = "«Trecento, ma ti voglio ai nostri eventi.»",
                    Weight = 2, Effects = [new(DayEffectKind.Money, 300), new(DayEffectKind.Popularity, 2)]
                }
            ]
        },
        new()
        {
            Id = "haru-gomme", Name = "Haru: produttore di pneumatici", Actor = DayActor.Agent, Hours = 4,
            Promise = "Probabilità bassa, cifra alta. Una giornata intera per un forse.",
            Outcomes =
            [
                new()
                {
                    Line = "Tre ore di anticamera e una risposta in due righe: no.",
                    Weight = 8, IsSetback = true, Effects = []
                },
                new()
                {
                    Line = "«Mettiamo settecento sul progetto. Vediamo dove arriva.»",
                    Weight = 2, Effects = [new(DayEffectKind.Money, 700), new(DayEffectKind.SportingReputation, 3)]
                }
            ]
        },
        new()
        {
            Id = "haru-contatti", Name = "Haru: coltiva i contatti", Actor = DayActor.Agent, Hours = 2,
            Promise = "Nessun incasso immediato. Prepara il terreno per le trattative dei prossimi giorni.",
            Outcomes =
            [
                new()
                {
                    Line = "Telefonate, caffè, promesse vaghe. Ma qualche porta resta socchiusa.",
                    Effects = [new(DayEffectKind.SportingReputation, 1)]
                }
            ]
        }
    ];

    /// <summary>Tutte le attività, indipendentemente da chi le svolge.</summary>
    public static IReadOnlyList<DayActivity> All() => [.. ForDriver(), .. ForAgent()];

    public static DayActivity? ById(string id) =>
        All().FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
}
