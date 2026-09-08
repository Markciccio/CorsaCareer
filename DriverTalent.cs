namespace CorsaCareer1991;

/// <summary>
/// Che pilota sei nato.
///
/// Prima ogni carriera partiva dallo stesso pilota astratto e la differenza
/// nasceva solo da quello che accadeva dopo: due carriere condotte allo stesso
/// modo davano gli stessi risultati. Ma un pilota fulmineo e incostante e un
/// pilota metodico che non sbaglia mai non fanno la stessa carriera nemmeno
/// con le stesse macchine.
///
/// Sono doti innate: non si allenano e non cambiano: si scoprono correndo. Il
/// lavoro fuori dalla pista — forma, immagine, conti, fiducia — resta quello
/// che si costruisce, ed è lì che il giocatore decide.
/// </summary>
public sealed class DriverTalent
{
    /// <summary>Velocità pura sul giro secco. Alza il passo, sempre.</summary>
    public int RawPace { get; set; } = 50;

    /// <summary>
    /// Costanza. Riduce la dispersione fra una gara e l'altra: chi ce l'ha
    /// bassa alterna exploit e disastri, chi ce l'ha alta è sempre lì.
    /// </summary>
    public int Consistency { get; set; } = 50;

    /// <summary>Sensibilità sul bagnato: conta solo quando piove, e allora conta molto.</summary>
    public int WetSkill { get; set; } = 50;

    /// <summary>Combattimento ruota a ruota: partenze, sorpassi, difesa.</summary>
    public int Racecraft { get; set; } = 50;

    /// <summary>
    /// Quanto in fretta si impara una macchina nuova. Chi ce l'ha alta perde
    /// poco salendo di categoria; chi ce l'ha bassa ha bisogno di una stagione
    /// per trovarsi a suo agio.
    /// </summary>
    public int Adaptability { get; set; } = 50;

    /// <summary>
    /// Quanto arriva in fondo. Non è solo la macchina: è come tratta i freni,
    /// il cambio, i cordoli, e quanta fortuna ha. Bassa significa una carriera
    /// di ritiri con qualche lampo — veloce quando resta in pista, ma resta in
    /// pista di rado.
    /// </summary>
    public int Reliability { get; set; } = 50;

    /// <summary>Come lo chiamano nel paddock: serve al racconto, non ai calcoli.</summary>
    public string Archetype { get; set; } = "";

    /// <summary>Una riga che spiega il pilota a chi legge.</summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Genera le doti dal nome del pilota: stabile per quella carriera — la
    /// stessa persona non cambia indole a ogni avvio — ma diversa da un pilota
    /// all'altro.
    /// </summary>
    public static DriverTalent For(string driverName)
    {
        var seme = Seme(driverName);

        // Un profilo non è mai piatto: si distribuisce un totale, così a una
        // dote alta corrisponde quasi sempre una debolezza da qualche parte.
        // Senza questo vincolo sarebbero usciti solo fenomeni assoluti.
        int Dote(int salto, int minimo, int massimo)
            => minimo + (int)(Mescola(seme, salto) % (uint)(massimo - minimo + 1));

        var talento = new DriverTalent
        {
            RawPace = Dote(1, 30, 95),
            Consistency = Dote(2, 25, 92),
            WetSkill = Dote(3, 25, 95),
            Racecraft = Dote(4, 28, 92),
            Adaptability = Dote(5, 28, 90),
            Reliability = Dote(6, 25, 92)
        };
        var (nome, riga) = Etichetta(talento);
        talento.Archetype = nome;
        talento.Description = riga;
        return talento;
    }

    /// <summary>
    /// Il nome che il paddock dà a questo tipo di pilota, letto dalle doti
    /// vere: nessun archetipo viene scelto prima e poi riempito di numeri.
    /// </summary>
    private static (string Nome, string Riga) Etichetta(DriverTalent t)
    {
        // Non tutti sono predestinati, e va detto prima delle doti alte:
        // altrimenti un pilota scarso ovunque finiva etichettato «il tenace»,
        // che suona come una promessa. Una carriera mediocre è un esito
        // possibile, ed è quello che rende preziosa una carriera riuscita.
        var totale = t.RawPace + t.Consistency + t.WetSkill + t.Racecraft + t.Adaptability + t.Reliability;
        if (totale <= 260)
            return ("Il mediocre",
                "Non ha una dote che lo distingua: in pista è uno dei tanti. Potrà vivere di corse solo se troverà soldi, sponsor e occasioni che il cronometro da solo non gli darà — e non è affatto detto che arrivi in alto.");
        if (totale <= 320 && t.RawPace <= 50)
            return ("Il pilota pagante",
                "Il cronometro non lo porterà da nessuna parte, il portafoglio forse sì. Può salire di categoria finché qualcuno lo finanzia, e restare l'ultimo di ogni griglia in cui entra.");
        if (t.Reliability <= 38 && t.RawPace >= 65)
            return ("Il velocista sfortunato",
                "Quando arriva in fondo sorprende tutti; il problema è arrivarci. Rotture, contatti, weekend buttati: una carriera di occasioni sprecate da tutto tranne che dal suo piede.");
        if (totale <= 340 && t.Reliability >= 65)
            return ("Il gregario",
                "Onesto e affidabile, senza il guizzo che fa la differenza: il tipo di pilota che le squadre tengono volentieri come seconda guida.");

        if (t.RawPace >= 80 && t.Consistency <= 50)
            return ("Il fulmine incostante",
                "Velocissimo su un giro, imprevedibile sulla distanza: capace di un tempo che nessuno spiega e di un weekend da dimenticare.");
        if (t.Consistency >= 78 && t.Adaptability >= 65)
            return ("Il metodico",
                "Raramente il più veloce, quasi mai fuori posto: costruisce le stagioni sommando gare senza errori.");
        if (t.WetSkill >= 82)
            return ("Il pilota da pioggia",
                "Quando il grip sparisce trova una linea che gli altri non vedono: sul bagnato le differenze di macchina contano meno del suo istinto.");
        if (t.Racecraft >= 80)
            return ("Il combattente",
                "Vive nel traffico: parte meglio di tutti e non restituisce una posizione che sia una.");
        if (t.Adaptability >= 78)
            return ("L'adattabile",
                "Sale su qualunque macchina e dopo tre giri sembra averla sempre guidata. Le promozioni non lo spaventano.");
        if (t.RawPace >= 72)
            return ("Il veloce",
                "Ha il passo per stare davanti; il resto è mestiere da costruire.");
        return ("Il tenace",
            "Nessuna dote fuori scala: arriverà dove lo porteranno lavoro, testa e occasioni prese al momento giusto.");
    }

    /// <summary>Riepilogo leggibile delle doti, per le schermate del pilota.</summary>
    public string Summary() =>
        $"Velocità pura {RawPace}/100 · costanza {Consistency}/100 · bagnato {WetSkill}/100 · "
        + $"duelli {Racecraft}/100 · adattamento {Adaptability}/100 · affidabilità {Reliability}/100";

    private static long Seme(string nome)
    {
        var testo = (nome ?? "").Trim().ToLowerInvariant();
        var hash = 2166136261L;
        foreach (var c in testo) hash = (hash ^ c) * 16777619 % 2147483647;
        return Math.Abs(hash);
    }

    private static uint Mescola(long seme, int salto)
    {
        unchecked
        {
            var h = 2166136261u ^ (uint)(salto * 2654435761u);
            var valore = (ulong)seme;
            for (var i = 0; i < 8; i++)
            {
                h ^= (uint)(valore & 0xFF);
                h *= 16777619u;
                valore >>= 8;
            }
            return h;
        }
    }
}
