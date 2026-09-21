namespace CorsaCareer;

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
    /// <summary>
    /// Le attivita' che si svolgono a scuola, elencate una volta sola.
    ///
    /// Serve perche' esisteva gia' «scuola-kart», che e' una giornata al
    /// kartodromo e non ha niente a che vedere con la classe: cercare le
    /// attivita' scolastiche per prefisso faceva scattare la scena di Sae e
    /// Tooru dopo un pomeriggio passato al circuito.
    /// </summary>
    // La scuola è ora un impegno obbligatorio del calendario, non una voce
    // selezionabile fra le attività: l'elenco resta vuoto per non trattarla
    // erroneamente come un'azione facoltativa.
    public static readonly IReadOnlyList<string> Scolastiche = [];

    /// <summary>Vero se questa attivita' si svolge a scuola.</summary>
    public static bool AScuola(string id) =>
        Scolastiche.Contains(id, StringComparer.OrdinalIgnoreCase);

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
                    Line = "Due ore di sala pesi. Nessuno ti guarda, nessuno ti applaude, e domani ti fara' male tutto.",
                    Effects = [new(DayEffectKind.Fitness, 6), new(DayEffectKind.Fatigue, 12)]
                },
                new()
                {
                    Line = "Il tizio del bilanciere accanto ti chiede quanti anni hai. Dodici. Non ti crede e ti corregge la schiena.",
                    Effects = [new(DayEffectKind.Fitness, 6), new(DayEffectKind.Fatigue, 12)]
                },
                new()
                {
                    Line = "Ultima serie, e le braccia non rispondono piu'. E' esattamente il punto in cui serve farne un'altra.",
                    Effects = [new(DayEffectKind.Fitness, 6), new(DayEffectKind.Fatigue, 12)]
                }
            ]
        },
        new()
        {
            Id = "kart-training", Name = "Turni liberi in kart", Actor = DayActor.Driver, Focus = DayFocus.Fisico,
            Hours = 2, Cost = 180,
            Promise = "Open day nel kartodromo di casa: pista € 90, gomme e revisione minima € 90. Giri con altri concorrenti, senza risultato di gara.",
            Outcomes =
            [
                new()
                {
                    Line = "Due turni puliti nel kartodromo di casa. Gli altri girano per conto loro e tu impari a stare nel traffico senza una classifica.",
                    Effects = []
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
                    Line = "Sei chilometri lungo il fiume. Nessun pensiero per cinquanta minuti, che e' la parte che serve davvero.",
                    Effects = [new(DayEffectKind.Fitness, 3), new(DayEffectKind.Fatigue, 6)]
                },
                new()
                {
                    Line = "Parti piano e finisci forte, come ti ha detto Genji di fare anche in gara. Non e' un caso che lo dica per tutte e due le cose.",
                    Effects = [new(DayEffectKind.Fitness, 3), new(DayEffectKind.Fatigue, 6)]
                },
                new()
                {
                    Line = "Piove. Corri lo stesso, e per un quarto d'ora ti senti il tipo di persona che corre anche sotto la pioggia.",
                    Effects = [new(DayEffectKind.Fitness, 3), new(DayEffectKind.Fatigue, 6)]
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
                    Line = "Due ore a fare niente. Ti sembra tempo buttato finche' non ti alzi e ti accorgi che non lo era.",
                    Effects = [new(DayEffectKind.Fatigue, -18)]
                },
                new()
                {
                    Line = "Dormi sul divano con la televisione accesa. Ti sveglia tua madre, e ti dice che russavi.",
                    Effects = [new(DayEffectKind.Fatigue, -18)]
                },
                new()
                {
                    Line = "Stai steso a fissare il soffitto e a rifare la curva sette nella testa. Riposo a modo tuo.",
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
                    Line = "Il fisioterapista ti trova un nodo nella spalla destra e ti chiede se freni sempre con quella. Si', sempre.",
                    Effects = [new(DayEffectKind.Fatigue, -22), new(DayEffectKind.Fitness, 1)]
                },
                new()
                {
                    Line = "Un'ora sul lettino. Fa male mentre lo fanno e sta benissimo dopo, come quasi tutto quello che funziona.",
                    Effects = [new(DayEffectKind.Fatigue, -22), new(DayEffectKind.Fitness, 1)]
                },
                new()
                {
                    Line = "«Alla tua eta' non dovresti avere questi carichi.» Poi guarda il tuo casco sulla sedia e non dice altro.",
                    Effects = [new(DayEffectKind.Fatigue, -22), new(DayEffectKind.Fitness, 1)]
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
        // Studiare non fa vincere una gara, e proprio per questo e' la scelta
        // piu' difficile della giornata: due ore tolte alla pista che non
        // migliorano niente di quello che si vede. Si capisce a giugno.
        new()
        {
            Id = "studio", Name = "Studiare", Actor = DayActor.Driver, Focus = DayFocus.Altro, Hours = 2,
            Promise = "Due ore sui libri. Non ti fanno andare piu' forte, ti evitano la bocciatura — e una bocciatura toglie due ore al giorno per un anno intero.",
            Outcomes =
            [
                new()
                {
                    Line = "Due ore di matematica, e mezza te ne sei passata a guardare fuori dalla finestra. Ma qualcosa e' entrato.",
                    Weight = 2, Effects = [new(DayEffectKind.Fatigue, 3)]
                },
                new()
                {
                    Line = "Kanji e verbi inglesi. Hai rifatto gli esercizi che avevi sbagliato: domani in classe non ti chiami fuori.",
                    Weight = 3, Effects = [new(DayEffectKind.Fatigue, 4)]
                },
                new()
                {
                    Line = "Scienze. Una di quelle sere in cui la cosa torna, e resti li' anche dopo aver finito i compiti.",
                    Weight = 1, Effects = [new(DayEffectKind.Fatigue, 5)]
                },
                new()
                {
                    Line = "Studi sociali, e per un'ora non hai pensato una sola volta alla curva sette. E' gia' qualcosa.",
                    Weight = 2, Effects = [new(DayEffectKind.Fatigue, 3)]
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
                    Line = "Quattro ore a scaricare casse. Centoventi euro, e le mani che sanno di cartone fino a domani.",
                    Effects = [new(DayEffectKind.Money, 120), new(DayEffectKind.Fatigue, 14)]
                },
                new()
                {
                    Line = "Turno al magazzino. Il capo ti dice che sei sveglio e ti chiede se vuoi tornare sabato. Forse.",
                    Effects = [new(DayEffectKind.Money, 120), new(DayEffectKind.Fatigue, 14)]
                },
                new()
                {
                    Line = "Quattro ore in piedi. Pensavi di pensare alle gare e invece hai pensato solo a quando finiva.",
                    Effects = [new(DayEffectKind.Money, 120), new(DayEffectKind.Fatigue, 14)]
                }
            ]
        },

        // --------------------------------------------------------- la scuola
        //
        // Il pilota ha sedici anni: la scuola c'e', e finora non esisteva.
        // Non e' un dettaglio di colore — e' il posto dove la carriera viene
        // vista da fuori, dove due persone che non c'entrano niente col
        // paddock ti dicono come stai andando. E ha effetti veri: andarci
        // riposa la testa, saltarla per allenarsi ha un prezzo.
        // «Scuola» non e' piu' un'attivita' che si sceglie.
        //
        // Era una voce del catalogo come le altre, da quattro ore, e da quando
        // la giornata e' divisa in fasce il generatore poteva proporla in una
        // fascia lunga — cioe' offrire di ANDARE A SCUOLA il giorno di
        // Capodanno, quando la scuola e' chiusa. La scuola adesso e' un blocco
        // fisso della giornata, gestito dal calendario: non si sceglie, e chi
        // vuole guadagnarci qualcosa studia.
        new()
        {
            Id = "scuola-sae", Name = "Allenarti con Sae", Actor = DayActor.Driver, Focus = DayFocus.Fisico, Hours = 2,
            Promise = "Due ore di preparazione con la tua compagna di classe, che corre nella tua stessa categoria. Vi spingete a vicenda.",
            Outcomes =
            [
                new()
                {
                    Line = "Sae non molla mai un esercizio a meta'. Finisci distrutto e piu' forte di ieri.",
                    Weight = 7, Effects = [new(DayEffectKind.Fitness, 8), new(DayEffectKind.Fatigue, 12)]
                },
                new()
                {
                    Line = "Finisce in una gara di resistenza fra voi due. Nessuno dei due si e' fermato, e domani lo pagate entrambi.",
                    Weight = 3, Effects = [new(DayEffectKind.Fitness, 11), new(DayEffectKind.Fatigue, 20)]
                }
            ]
        },
        new()
        {
            Id = "scuola-tooru", Name = "Fare i conti con Tooru", Actor = DayActor.Driver, Focus = DayFocus.Altro, Hours = 2,
            Promise = "Due ore sul quaderno di Tooru a rimettere in ordine le spese. Non porta soldi: fa vedere dove se ne vanno.",
            Outcomes =
            [
                new()
                {
                    Line = "Tooru ha ritrovato due iscrizioni pagate due volte e si e' fatto restituire la differenza.",
                    Weight = 4, Effects = [new(DayEffectKind.Money, 90)]
                },
                new()
                {
                    Line = "Nessun errore nei conti. «Almeno adesso sai esattamente quanto ti manca», dice, e non e' un complimento.",
                    Weight = 6, Effects = [new(DayEffectKind.SportingReputation, 1)]
                }
            ]
        },
        new()
        {
            Id = "scuola-volantini", Name = "Volantini a scuola con Tooru", Actor = DayActor.Driver, Focus = DayFocus.Immagine, Hours = 2,
            Promise = "Tooru ha stampato dei volantini e Sae li distribuisce in cortile. Fa parlare di te dentro la scuola, e a volte fuori.",
            Outcomes =
            [
                new()
                {
                    Line = "Li hanno presi in pochi e due sono finiti nel cestino davanti a te. Tooru fa finta di niente.",
                    Weight = 4, IsSetback = true, Effects = [new(DayEffectKind.Popularity, 1)]
                },
                new()
                {
                    Line = "Mezza scuola sa che domenica corri. In tre hanno chiesto se possono venire a vedere.",
                    Weight = 5, Effects = [new(DayEffectKind.Popularity, 5)]
                },
                new()
                {
                    Line = "Un professore ha appeso il volantino in sala insegnanti. Suo cognato ha un'officina.",
                    Weight = 1, Effects = [new(DayEffectKind.Popularity, 7), new(DayEffectKind.SportingReputation, 2)]
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
            Id = "haru-gomme", Name = "Haru: produttore di pneumatici", Actor = DayActor.Agent, Hours = 3,
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
                    Line = "Telefonate, caffe', promesse vaghe. Ma qualche porta resta socchiusa.",
                    Effects = [new(DayEffectKind.SportingReputation, 1)]
                },
                new()
                {
                    Line = "Haru passa il pomeriggio a farsi dare numeri da gente che ne ha altri. Non porta a casa niente, oggi.",
                    Effects = [new(DayEffectKind.SportingReputation, 1)]
                },
                new()
                {
                    Line = "«Ho parlato con uno che conosce uno.» Detto da chiunque altro sarebbe niente. Detto da Haru, di solito, e' l'inizio.",
                    Effects = [new(DayEffectKind.SportingReputation, 1)]
                }
            ]
        },

        // --- Haru dietro una tastiera
        //
        // Il seguito non lo costruisce solo chi corre. Haru non ha una lira e
        // non sa guidare, ma ha un computer e tutto il pomeriggio: il sito, la
        // pagina, il profilo sono l'unica cosa che uno sponsor locale riesce a
        // guardare prima di dire di sì. E sono ore che il pilota non spende.
        new()
        {
            Id = "haru-sito", Name = "Haru: il sito del pilota", Actor = DayActor.Agent, Hours = 2,
            Promise = "Haru aggiorna il sito: risultati, foto, una pagina per gli sponsor. Cresce il seguito, e chi paga trova qualcosa da leggere.",
            Outcomes =
            [
                new()
                {
                    Line = "«Ho rifatto la home. Ci ho messo tre ore e nessuno se ne accorgerà.» Se ne accorgono in due.",
                    Weight = 2, Effects = [new(DayEffectKind.Popularity, 1)]
                },
                new()
                {
                    Line = "«Ho messo la pagina con i risultati in ordine.» Uno sponsor l'ha aperta due volte.",
                    Weight = 3, Effects = [new(DayEffectKind.Popularity, 2), new(DayEffectKind.SportingReputation, 1)]
                },
                new()
                {
                    Line = "«Guarda le visite.» Un giornalista locale ha copiato mezza scheda pilota. Male non fa.",
                    Weight = 1, Effects = [new(DayEffectKind.Popularity, 4), new(DayEffectKind.SportingReputation, 2)]
                }
            ]
        },
        new()
        {
            Id = "haru-pagina", Name = "Haru: la pagina Facebook", Actor = DayActor.Agent, Hours = 1,
            Promise = "Un post con le foto del weekend. Arriva ai genitori, agli amici, ai negozianti del quartiere: sono quelli che poi mettono i soldi.",
            Outcomes =
            [
                new()
                {
                    Line = "«Sei like. Uno è mia madre.» Haru lo dice ridendo, ma ci è rimasto male.",
                    Weight = 2, Effects = [new(DayEffectKind.Popularity, 1)]
                },
                new()
                {
                    Line = "Il gommista ha commentato «bravo ragazzo». Haru ha già segnato il nome.",
                    Weight = 3, Effects = [new(DayEffectKind.Popularity, 2)]
                },
                new()
                {
                    Line = "Il post è girato per tutto il quartiere. Due negozianti hanno chiesto quanto costa metterci il nome.",
                    Weight = 1, Effects = [new(DayEffectKind.Popularity, 3), new(DayEffectKind.Money, 150)]
                }
            ]
        },
        new()
        {
            Id = "haru-instagram", Name = "Haru: il profilo Instagram", Actor = DayActor.Agent, Hours = 2,
            Promise = "Haru monta le foto e scrive le didascalie. Non è il pilota a farlo, quindi non costa ore al pilota — ma si vede che non è lui.",
            Outcomes =
            [
                new()
                {
                    Line = "«Ho scritto io la didascalia.» Si capisce. Funziona lo stesso, poco.",
                    Weight = 3, Effects = [new(DayEffectKind.Popularity, 2)]
                },
                new()
                {
                    Line = "La foto in controluce sul rettilineo ha girato più di tutte le altre messe insieme.",
                    Weight = 2, Effects = [new(DayEffectKind.Popularity, 4)]
                },
                new()
                {
                    Line = "«Ci ha scritto una pagina di motori con ventimila follower.» Haru non dorme da due giorni.",
                    Weight = 1, Effects = [new(DayEffectKind.Popularity, 6), new(DayEffectKind.SportingReputation, 1)]
                }
            ]
        }
    ];

    /// <summary>Tutte le attività, indipendentemente da chi le svolge.</summary>
    public static IReadOnlyList<DayActivity> All() => [.. ForDriver(), .. ForAgent()];

    public static DayActivity? ById(string id) =>
        All().FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
}
