namespace CorsaCareer;

/// <summary>I momenti della carriera che meritano una scena parlata.</summary>
public enum SceneKind
{
    /// <summary>Si firma un sedile: la squadra, le condizioni, cosa comporta.</summary>
    Firma,
    /// <summary>Comincia un campionato: regole, calendario, obiettivo.</summary>
    AperturaCampionato,
    /// <summary>Una vittoria.</summary>
    Vittoria,
    /// <summary>Un podio o un buon risultato.</summary>
    BuonRisultato,
    /// <summary>Una gara storta, un ritiro, un errore.</summary>
    Battuta,
    /// <summary>Fine stagione con promozione.</summary>
    Promozione,
    /// <summary>Fine stagione senza cambiamenti.</summary>
    Conferma,
    /// <summary>Fine stagione con retrocessione.</summary>
    Retrocessione,
    /// <summary>La chiamata da un campionato superiore, a stagione in corso.</summary>
    ChiamataDaSopra,
    /// <summary>Spiegazione di una soglia: cosa manca e a cosa serve.</summary>
    Spiegazione,
    /// <summary>Allarme classifica: si rischia di scendere.</summary>
    Allarme,
    /// <summary>Soldi: quote, sponsor, bilanci.</summary>
    Denaro,
    /// <summary>Immagine pubblica: seguito, stampa, social.</summary>
    Immagine,
    /// <summary>Tecnica: assetto, dati, come si guida quella macchina.</summary>
    Tecnica,
    /// <summary>Scuola: la vita che continua fuori dal circuito.</summary>
    Scuola,
    /// <summary>L'inizio di tutto: il kart rimesso insieme, la prima volta in pista.</summary>
    Origine,
    /// <summary>La cassa vuota, le iscrizioni che non si pagano.</summary>
    Crisi
}

/// <summary>Un personaggio, con il suo carattere e la sua funzione nel racconto.</summary>
public sealed record CastMember(
    string Id,
    string Nome,
    string Ruolo,
    string Personalita,
    string ComeParla,
    string RitrattoBase,
    IReadOnlyDictionary<string, string> Ritratti,
    IReadOnlyList<SceneKind> Scene,
    int Peso);

/// <summary>
/// Chi entra in scena, e perché quello e non un altro.
///
/// I personaggi c'erano già ma venivano scelti a mano, scena per scena, con il
/// nome del file del ritratto scritto dentro il testo: bastava che un file non
/// esistesse — ed è successo, il ritratto di Rei era proprio uno di quelli — e
/// il personaggio compariva senza faccia. Peggio ancora, non c'era nessuna
/// regola su chi dovesse parlare di cosa: l'ingegnere finiva a commentare gli
/// sponsor e l'amico a spiegare la telemetria.
///
/// Qui ogni personaggio dichiara che cosa sa e in quali momenti serve, e il
/// regista compone la scena scegliendo le persone giuste nell'ordine giusto —
/// chi apre di pancia, chi spiega, chi chiude. Il ritratto viene risolto
/// controllando che il file esista davvero, con un ripiego dichiarato: una
/// faccia sbagliata è meglio di nessuna faccia, e nessuna faccia è un difetto.
/// </summary>
public static class CastDirector
{
    public const string Haru = "haru-senda";
    public const string Rei = "rei-kisaragi";
    public const string Shigeo = "shigeo-kanda";
    public const string Genji = "genji-arakawa";
    public const string Riku = "riku-hayase";
    public const string Noa = "noa-minazuki";
    public const string Miki = "miki-arisawa";
    public const string Minoru = "minoru-nakahara";
    public const string Monami = "monami-todo";
    public const string Nobu = "nobu-yagi";

    public static readonly IReadOnlyList<CastMember> Compagnia =
    [
        new(Haru, "Haru Senda", "amico d'infanzia e cacciatore di sponsor",
            "Entusiasta fino all'eccesso, si emoziona prima di te e si dispera prima di te. È quello che resta quando il paddock si svuota.",
            "Parla di getto, si interrompe, esagera. Dice sempre «noi» e mai «tu»: la carriera la vive come sua.",
            "character-haru-senda.png",
            new Dictionary<string, string>
            {
                ["felice"] = "character-haru-senda-podium-elated.png",
                ["preoccupato"] = "character-haru-senda-anxious-data.png",
                ["sollevato"] = "character-haru-senda-relieved-small-sponsor-kart-paddock.png",
                ["documenti"] = "character-haru-senda with paper.png"
            },
            [SceneKind.Firma, SceneKind.Vittoria, SceneKind.BuonRisultato, SceneKind.Battuta,
             SceneKind.Promozione, SceneKind.Conferma, SceneKind.Retrocessione,
             SceneKind.ChiamataDaSopra, SceneKind.Spiegazione, SceneKind.Denaro],
            Peso: 100),

        new(Rei, "Rei Kisaragi", "responsabile del programma rookie, il tuo riferimento",
            "Misura le parole. Non fa complimenti facili e non nasconde le cose spiacevoli: se ti dice che sei andato bene, sei andato bene.",
            "Frasi complete e ordinate. Spiega sempre il perché di una regola, e chiude dicendo cosa fare adesso.",
            "character-rei-kisaragi-relieved.png",
            new Dictionary<string, string>
            {
                ["serena"] = "character-rei-kisaragi-relieved.png",
                ["severa"] = "character-rei-kisaragi-stern.png",
                ["preoccupata"] = "character-rei-kisaragi-concerned.png",
                ["cauta"] = "character-rei-kisaragi-first-contract-cautious.png"
            },
            [SceneKind.Firma, SceneKind.AperturaCampionato, SceneKind.Promozione, SceneKind.Conferma,
             SceneKind.Retrocessione, SceneKind.ChiamataDaSopra, SceneKind.Spiegazione,
             SceneKind.Allarme, SceneKind.Battuta],
            Peso: 95),

        new(Shigeo, "Shigeo Kanda", "ingegnere di pista",
            "Non discute con i dati e non si scalda mai. Il suo giudizio è sempre su una cosa specifica che si può correggere.",
            "Concreto e tecnico ma comprensibile: dice cosa è successo, dove, e come si allena.",
            "character-shigeo-kanda.png",
            new Dictionary<string, string>
            {
                ["neutro"] = "character-shigeo-kanda.png",
                ["svolta"] = "character-shigeo-kanda-breakthrough-formula.png"
            },
            [SceneKind.AperturaCampionato, SceneKind.Tecnica, SceneKind.BuonRisultato, SceneKind.Battuta,
             SceneKind.Promozione, SceneKind.Conferma, SceneKind.Retrocessione, SceneKind.Allarme,
             SceneKind.Spiegazione],
            Peso: 85),

        new(Genji, "Genji Arakawa", "meccanico e mentore, vecchia scuola",
            "Parla poco e ascolta il motore. Quando dice qualcosa è una frase sola, e di solito è quella che ti resta in testa.",
            "Poche parole, immagini concrete, nessuna teoria. Racconta per esempi di piloti che ha visto passare.",
            "character-genji-arakawa.png",
            new Dictionary<string, string>
            {
                ["neutro"] = "character-genji-arakawa.png",
                ["fiero"] = "character-genji-arakawa-proud.png",
                ["preoccupato"] = "character-genji-arakawa-worried.png",
                ["deluso"] = "character-genji-arakawa-frustrated.png",
                ["festa"] = "character-genji-arakawa-celebrating.png"
            },
            [SceneKind.Firma, SceneKind.Vittoria, SceneKind.Battuta, SceneKind.Tecnica, SceneKind.Retrocessione],
            Peso: 70),

        new(Riku, "Riku Hayase", "il rivale della tua generazione",
            "Non abbassa mai lo sguardo e ti provoca sempre, ma ti rispetta più di quanto ammetterebbe. Batterti quando sei in difficoltà non gli interessa.",
            "Tagliente, ironico, una frecciata e poi mezza frase sincera detta più piano.",
            "character-riku-hayase.png",
            new Dictionary<string, string>
            {
                ["neutro"] = "character-riku-hayase.png",
                ["spavaldo"] = "character-riku-hayase-smug.png",
                ["beffardo"] = "character-riku-hayase-taunting.png",
                ["arrabbiato"] = "character-riku-hayase-angry.png",
                ["sconfitto"] = "character-riku-hayase-defeated.png"
            },
            [SceneKind.Vittoria, SceneKind.Promozione, SceneKind.Retrocessione, SceneKind.Battuta],
            Peso: 60),

        new(Noa, "Noa Minazuki", "giornalista e fotografa",
            "Fa domande prima di scattare. Ti spiega come ti vede il mondo fuori dal box, che è una cosa che dal box non si vede.",
            "Professionale e diretta, spiega gli effetti pubblici di una scelta senza mai giudicarla.",
            "character-noa-minazuki.png",
            new Dictionary<string, string>
            {
                ["neutro"] = "character-noa-minazuki.png",
                ["entusiasta"] = "character-noa-minazuki-gt-lead-excited.png"
            },
            [SceneKind.Immagine, SceneKind.ChiamataDaSopra, SceneKind.Promozione, SceneKind.Vittoria],
            Peso: 65),

        new(Miki, "Miki Arisawa", "stratega e referente sponsor",
            "Vede il costo nascosto di ogni decisione. Non è cinica: è l'unica che fa i conti prima e non dopo.",
            "Numeri e conseguenze. Dice quanto costa una scelta e quanto rende, senza consigliare quale prendere.",
            "character-miki-arisawa.png",
            new Dictionary<string, string>
            {
                ["neutro"] = "character-miki-arisawa.png",
                ["sollevata"] = "character-miki-arisawa-sponsor-relieved.png",
                ["preoccupata"] = "character-miki-arisawa-sponsor-worried.png",
                ["rifiuto"] = "character-miki-arisawa-rejection-composed.png"
            },
            [SceneKind.Denaro, SceneKind.Firma, SceneKind.Immagine, SceneKind.Allarme],
            Peso: 60),

        new(Minoru, "Minoru Nakahara", "coordinatore del campionato",
            "Severo ma uguale con tutti. Non è dalla tua parte e non è contro di te: applica il regolamento.",
            "Istituzionale, asciutto, mai personale. Comunica decisioni, non opinioni.",
            "character-minoru-nakahara.png",
            new Dictionary<string, string> { ["neutro"] = "character-minoru-nakahara.png" },
            [SceneKind.AperturaCampionato, SceneKind.Retrocessione, SceneKind.ChiamataDaSopra],
            Peso: 40),

        // I due compagni di scuola. Non hanno un ritratto proprio: esistono
        // solo dentro le tavole di gruppo, e il regista lo sa — Ritratto()
        // restituisce vuoto e la scena mostra il luogo invece della faccia.
        // Meglio cosi' che prestargli il volto di qualcun altro.
        new(Monami, "Monami Todo", "compagna di classe, corre anche lei",
            "Corre nella tua stessa categoria e non te lo fa pesare mai. E' l'unica che capisce davvero cosa vuol dire arrivare a scuola dopo una gara persa.",
            "Diretta e pratica, senza retorica. Ti parla da pilota a pilota, e quando ti consola lo fa dicendoti una cosa vera.",
            "",
            new Dictionary<string, string>(),
            [SceneKind.Scuola, SceneKind.Battuta, SceneKind.Vittoria, SceneKind.Origine],
            Peso: 55),

        new(Nobu, "Nobu Yagi", "compagno di classe, tiene i conti",
            "Non guida e non gli interessa guidare: gli interessa che tu possa farlo. E' quello che ha capito prima di tutti che senza soldi non si corre.",
            "Concreto fino alla brutalita' sui numeri, imbarazzato su tutto il resto. Dice le cifre esatte e poi si scusa.",
            "",
            new Dictionary<string, string>(),
            [SceneKind.Scuola, SceneKind.Denaro, SceneKind.Crisi, SceneKind.Origine],
            Peso: 50)
    ];

    public static CastMember ById(string id) =>
        Compagnia.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? Compagnia[0];

    /// <summary>
    /// Chi parla in questa scena, nell'ordine in cui deve parlare.
    ///
    /// L'ordine non e' casuale ed e' sempre lo stesso, perche' e' il ritmo che
    /// rende leggibile una scena: prima chi reagisce di pancia (Haru), poi chi
    /// spiega e mette in prospettiva (Rei), poi chi entra nel merito tecnico o
    /// economico, e alla fine chi commenta da fuori — il rivale o la stampa.
    /// </summary>
    public static IReadOnlyList<CastMember> Assegna(SceneKind scena, int quanti = 4)
    {
        var candidati = Compagnia.Where(x => x.Scene.Contains(scena)).ToList();
        if (candidati.Count == 0) candidati = [ById(Rei)];
        return candidati
            .OrderByDescending(x => OrdineDiScena(x.Id, scena))
            .ThenByDescending(x => x.Peso)
            .Take(Math.Max(1, quanti))
            .ToList();
    }

    /// <summary>Quanto e' centrale un personaggio in una certa scena.</summary>
    private static int OrdineDiScena(string id, SceneKind scena) => (id, scena) switch
    {
        // La reazione emotiva apre sempre: e' l'amico che sta vivendo la cosa
        // insieme a te, e senza di lui la scena comincia con una spiegazione.
        (Haru, SceneKind.Vittoria) => 100,
        (Haru, SceneKind.Promozione) => 100,
        (Haru, SceneKind.Retrocessione) => 100,
        (Haru, SceneKind.ChiamataDaSopra) => 100,
        (Haru, SceneKind.Denaro) => 90,

        // Rei apre quando c'e' una regola da spiegare, perche' e' la sua voce.
        (Rei, SceneKind.AperturaCampionato) => 100,
        (Rei, SceneKind.Spiegazione) => 100,
        (Rei, SceneKind.Allarme) => 100,
        (Rei, SceneKind.Firma) => 95,
        (Rei, _) => 80,

        (Shigeo, SceneKind.Tecnica) => 100,
        (Shigeo, SceneKind.Battuta) => 85,
        (Shigeo, _) => 60,

        (Noa, SceneKind.Immagine) => 100,
        (Noa, _) => 45,

        (Miki, SceneKind.Denaro) => 100,
        (Miki, _) => 45,

        (Genji, SceneKind.Tecnica) => 70,
        (Genji, _) => 40,

        // Il rivale chiude sempre: la sua battuta funziona come ultima parola,
        // non come apertura.
        (Riku, _) => 10,
        (Minoru, _) => 20,

        // A scuola comandano loro: e' il loro mondo, non quello del paddock.
        (Monami, SceneKind.Scuola) => 100,
        (Nobu, SceneKind.Scuola) => 90,
        (Nobu, SceneKind.Crisi) => 95,
        (Nobu, SceneKind.Denaro) => 70,
        (Monami, _) => 35,
        (Nobu, _) => 30,
        _ => 50
    };

    /// <summary>
    /// Il file del ritratto per un personaggio e uno stato d'animo, garantito
    /// esistente.
    ///
    /// I nomi dei ritratti erano scritti a mano dentro i testi delle scene, e
    /// uno di quelli non esisteva: il personaggio compariva senza faccia e non
    /// se ne accorgeva nessuno finche' non lo si vedeva a schermo. Qui il file
    /// viene verificato, e se manca si ripiega sul ritratto base e poi su
    /// qualunque ritratto di quel personaggio.
    /// </summary>
    public static string Ritratto(string id, string umore = "")
    {
        var persona = ById(id);
        var candidati = new List<string>();
        if (!string.IsNullOrWhiteSpace(umore) && persona.Ritratti.TryGetValue(umore, out var scelto)) candidati.Add(scelto);
        candidati.Add(persona.RitrattoBase);
        candidati.AddRange(persona.Ritratti.Values);
        foreach (var file in candidati)
        {
            if (string.IsNullOrWhiteSpace(file)) continue;
            if (File.Exists(AssetPaths.File(file))) return file;
        }
        return "";
    }

    /// <summary>Una battuta gia' pronta con nome e ritratto giusti.</summary>
    public static AnimeDialogueLine Battuta(string id, string testo, string umore = "") =>
        new(ById(id).Nome, testo, Ritratto(id, umore), umore);
}
