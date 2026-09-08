namespace CorsaCareer;

/// <summary>Che cosa succede alla fine di una stagione.</summary>
public enum SeasonVerdict
{
    /// <summary>Primi tre: si sale di livello e arrivano proposte nuove.</summary>
    Promosso,
    /// <summary>A metà gruppo: si resta, con altre proposte allo stesso livello.</summary>
    Confermato,
    /// <summary>In fondo: si retrocede, e per rientrare serve una nuova prova.</summary>
    Retrocesso
}

/// <summary>
/// Le regole con cui una carriera sale e scende.
///
/// Prima esisteva una regola sola — primi tre e sali — e nessuna conseguenza per
/// una stagione andata male: si restava dov'era, all'infinito. Una carriera vera
/// ha tre esiti, non due, e il terzo è quello che rende il secondo interessante.
///
/// Qui stanno tutte le soglie in un posto solo, perché sono la forma della
/// carriera e vanno lette insieme, non cercate in sei file diversi.
/// </summary>
public static class CareerProgression
{
    // ------------------------------------------------------- fine stagione

    /// <summary>Entro questa posizione si sale di livello.</summary>
    public const int PosizionePromozione = 3;

    /// <summary>
    /// Sotto questa frazione del gruppo si retrocede.
    ///
    /// L'ultimo quarto della classifica: in un campionato da dodici sono gli
    /// ultimi tre. Non è una punizione per una stagione storta — è il
    /// riconoscimento che quel livello, per adesso, è troppo alto.
    /// </summary>
    public const double FrazioneRetrocessione = 0.75;

    /// <summary>
    /// Il verdetto di fine stagione.
    /// </summary>
    /// <param name="posizioneFinale">Piazzamento in classifica, 1 = primo.</param>
    /// <param name="pilotiInCampionato">Quanti erano in classifica.</param>
    /// <param name="livello">Livello di campionato appena corso.</param>
    public static SeasonVerdict Verdetto(int posizioneFinale, int pilotiInCampionato, int livello)
    {
        if (posizioneFinale <= 0) return SeasonVerdict.Confermato;
        if (posizioneFinale <= PosizionePromozione) return SeasonVerdict.Promosso;
        // Dal livello più basso non si scende: sotto il campionato di zona non
        // c'è niente, e mandare a casa un esordiente non è una carriera, è la
        // fine di una carriera.
        if (ChampionshipLadder.Clamp(livello) <= 1) return SeasonVerdict.Confermato;
        var gruppo = Math.Max(4, pilotiInCampionato);
        return posizioneFinale > gruppo * FrazioneRetrocessione
            ? SeasonVerdict.Retrocesso
            : SeasonVerdict.Confermato;
    }

    /// <summary>Il livello di campionato dopo il verdetto.</summary>
    public static int LivelloDopo(int livello, SeasonVerdict verdetto) => verdetto switch
    {
        SeasonVerdict.Promosso => ChampionshipLadder.Clamp(livello + 1),
        SeasonVerdict.Retrocesso => ChampionshipLadder.Clamp(livello - 1),
        _ => ChampionshipLadder.Clamp(livello)
    };

    /// <summary>Come si racconta il verdetto al pilota.</summary>
    public static string Racconto(SeasonVerdict verdetto, int posizione, int livelloPrima, int livelloDopo) => verdetto switch
    {
        SeasonVerdict.Promosso =>
            $"P{posizione}: sei nei primi {PosizionePromozione}. Si sale a «{ChampionshipLadder.Name(livelloDopo)}» "
            + $"(livello {livelloDopo} di {ChampionshipLadder.Levels}): {ChampionshipLadder.Scope(livelloDopo)}. "
            + "Arriveranno proposte da squadre nuove.",
        SeasonVerdict.Retrocesso =>
            $"P{posizione}: hai chiuso in fondo. Si torna a «{ChampionshipLadder.Name(livelloDopo)}» "
            + $"(livello {livelloDopo} di {ChampionshipLadder.Levels}). Per rientrare serve una prova: "
            + "nessuno firma un pilota che arriva da una stagione così senza rivederlo girare.",
        _ =>
            $"P{posizione}: resti in «{ChampionshipLadder.Name(livelloDopo)}». "
            + ChampionshipLadder.PromotionRule(livelloDopo)
    };

    /// <summary>Il premio di fine stagione, in proporzione al livello e al piazzamento.</summary>
    public static int PremioFineStagione(int posizione, int livello)
    {
        if (posizione <= 0) return 0;
        var baseLivello = 6000 * ChampionshipLadder.Clamp(livello);
        var quota = posizione switch
        {
            1 => 3.0,
            2 => 2.0,
            3 => 1.4,
            <= 5 => 0.8,
            <= 10 => 0.35,
            _ => 0.1
        };
        return (int)Math.Round(baseLivello * quota);
    }

    // -------------------------------------------------- il salto anticipato

    /// <summary>Vittorie consecutive che fanno alzare il telefono a qualcuno.</summary>
    public const int VittorieDiFilaPerLaChiamata = 3;

    /// <summary>Podi consecutivi che valgono quanto una serie di vittorie.</summary>
    public const int PodiDiFilaPerLaChiamata = 5;

    /// <summary>Posizioni guadagnate in una sola gara perché si parli di te.</summary>
    public const int RimontaCheFaNotizia = 8;

    /// <summary>Seguito minimo perché una squadra superiore rischi su di te a stagione in corso.</summary>
    public const int InfluencerPerIlSalto = 50;

    /// <summary>Forma minima: una categoria superiore è più dura fisicamente.</summary>
    public const int FitnessPerIlSalto = 50;

    /// <summary>
    /// Se il pilota ha, adesso, i numeri per ricevere una chiamata da un
    /// campionato superiore anche a stagione in corso.
    ///
    /// Sono tre condizioni insieme, e devono esserlo: i risultati recenti
    /// accendono l'interesse, ma è quello che il pilota ha costruito fuori dalla
    /// pista — seguito e condizione — a rendere possibile che qualcuno rischi.
    /// È il motivo per cui vale la pena curare immagine e preparazione anche
    /// quando la classifica non basta.
    /// </summary>
    public static bool MeritaLaChiamata(int vittorieDiFila, int podiDiFila, int migliorRimonta,
        int influencer, int fitness, int livello)
    {
        if (ChampionshipLadder.IsTop(livello)) return false;
        if (influencer < InfluencerPerIlSalto) return false;
        if (fitness < FitnessPerIlSalto) return false;
        return vittorieDiFila >= VittorieDiFilaPerLaChiamata
               || podiDiFila >= PodiDiFilaPerLaChiamata
               || migliorRimonta >= RimontaCheFaNotizia;
    }

    /// <summary>La ragione della chiamata, detta come la direbbe chi telefona.</summary>
    public static string MotivoDellaChiamata(int vittorieDiFila, int podiDiFila, int migliorRimonta)
    {
        if (vittorieDiFila >= VittorieDiFilaPerLaChiamata)
            return $"{vittorieDiFila} vittorie di fila: in paddock non si parla d'altro";
        if (migliorRimonta >= RimontaCheFaNotizia)
            return $"una rimonta di {migliorRimonta} posizioni che qualcuno ha rivisto tre volte";
        return $"{podiDiFila} podi consecutivi: la costanza è la cosa che i team comprano più volentieri";
    }

    // ------------------------------------- dopo un salto andato male o bene

    /// <summary>
    /// Gare concesse a chi è salito a stagione in corso prima che il verdetto
    /// arrivi. Meno di così sarebbe un giudizio su un campione di niente.
    /// </summary>
    public const int GareDiProvaDopoIlSalto = 5;

    /// <summary>
    /// Come è andata la scommessa di chi ti ha preso a metà stagione.
    ///
    /// Chi sale a campionato iniziato non ha una classifica da difendere: ha una
    /// squadra che si è esposta. Il giudizio è sul rendimento immediato — podi e
    /// piazzamenti nella prima parte del gruppo — e arriva presto, come nella
    /// realtà.
    /// </summary>
    public static SeasonVerdict VerdettoDelSalto(IReadOnlyList<int> piazzamenti, int pilotiInGriglia)
    {
        var valide = piazzamenti.Where(x => x > 0).ToList();
        if (valide.Count < GareDiProvaDopoIlSalto) return SeasonVerdict.Confermato;
        var gruppo = Math.Max(4, pilotiInGriglia);
        var podi = valide.Count(x => x <= 3);
        var media = valide.Average();
        if (podi >= 2 || media <= gruppo * 0.35) return SeasonVerdict.Promosso;
        if (media > gruppo * 0.8) return SeasonVerdict.Retrocesso;
        return SeasonVerdict.Confermato;
    }
}
