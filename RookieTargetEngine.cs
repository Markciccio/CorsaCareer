namespace CorsaCareer;

public sealed class RookieTarget
{
    public int TargetMilliseconds { get; set; }
    public int ReferenceMilliseconds { get; set; }
    /// <summary>Come è stato ottenuto il riferimento: stima o tempo realmente girato.</summary>
    public string Basis { get; set; } = "";
    public bool Estimated { get; set; }
    public List<string> Notes { get; set; } = [];
}

/// <summary>
/// Tempo-obiettivo della rookie evaluation.
///
/// Prima era la costante <c>125000</c> (2:05.000), mai ricalcolata: lo stesso
/// obiettivo valeva per un kartodromo da 900 metri e per il Nordschleife, e per
/// una utilitaria come per una Formula 1. L'intero cancello d'ingresso della
/// carriera era quindi scollegato da pista e vettura.
///
/// Il riferimento viene ora derivato dalla lunghezza reale del circuito e dalle
/// prestazioni dichiarate dalla vettura installata, e quando esiste un tempo
/// realmente girato su quella combinazione quello ha la precedenza sulla stima.
/// </summary>
public static class RookieTargetEngine
{
    public const int FallbackTargetMilliseconds = 125000;
    /// <summary>Margine concesso al debuttante sul tempo di riferimento.</summary>
    public const double RookieMargin = 1.07;
    /// <summary>Entro questa soglia il paddock considera il pilota "vicino".</summary>
    public const double CloseMargin = 1.03;

    public static RookieTarget For(int trackLengthMeters, string carCategory, int powerHp, int massKg, int measuredReferenceMilliseconds = 0, string trackCategory = "permanent")
    {
        var target = new RookieTarget();

        if (trackLengthMeters <= 0)
        {
            target.ReferenceMilliseconds = FallbackTargetMilliseconds;
            target.TargetMilliseconds = FallbackTargetMilliseconds;
            target.Estimated = true;
            target.Basis = "valore predefinito: lunghezza del circuito non dichiarata";
            target.Notes.Add("Il circuito non dichiara la propria lunghezza nei metadati: l'obiettivo resta un valore predefinito e viene dichiarato come tale.");
            return target;
        }

        var speedKmh = ReferenceSpeedKmh(carCategory, powerHp, massKg, out var powerNote);
        var layoutFactor = LayoutFactor(trackCategory, trackLengthMeters, out var layoutNote);
        speedKmh *= layoutFactor;
        var referenceSeconds = trackLengthMeters / (speedKmh * 1000.0 / 3600.0);
        var stimato = (int)Math.Round(referenceSeconds * 1000);

        // Il tempo girato può stringere il riferimento, mai allargarlo.
        //
        // Prima un giro realmente segnato sostituiva la stima in ogni caso, e
        // il giro in questione era quello del pilota stesso: la soglia
        // diventava «il tuo ultimo tempo più il 7%», cioè una prova che si
        // supera sempre e che a ogni tentativo si allontanava dalla realtà. Nel
        // banco di prova bastavano sette prove per arrivare a un obiettivo di
        // 4:12 su un kartodromo da un chilometro e mezzo, superato ogni volta.
        //
        // Un giro più veloce della stima è invece un'informazione vera — quel
        // giro su quella pista con quell'auto è stato fatto — e alza l'asticella
        // per chi verrà dopo.
        var misuraUtile = measuredReferenceMilliseconds > 0 && measuredReferenceMilliseconds < stimato;
        target.ReferenceMilliseconds = misuraUtile ? measuredReferenceMilliseconds : stimato;
        target.TargetMilliseconds = (int)Math.Round(target.ReferenceMilliseconds * RookieMargin);
        target.Estimated = !misuraUtile;
        target.Basis = misuraUtile
            ? "miglior tempo realmente girato su questa combinazione auto/circuito"
            : $"stima da {trackLengthMeters / 1000.0:0.###} km a {speedKmh:0} km/h di media";
        target.Notes.Add(misuraUtile
            ? $"Riferimento misurato: {Format(target.ReferenceMilliseconds)} (più veloce della stima {Format(stimato)}). Obiettivo {Format(target.TargetMilliseconds)}."
            : $"Riferimento stimato: {Format(target.ReferenceMilliseconds)} su {trackLengthMeters / 1000.0:0.###} km. Obiettivo {Format(target.TargetMilliseconds)}.");
        if (!string.IsNullOrWhiteSpace(powerNote)) target.Notes.Add(powerNote);
        if (!string.IsNullOrWhiteSpace(layoutNote)) target.Notes.Add(layoutNote);
        if (measuredReferenceMilliseconds > 0 && !misuraUtile)
            target.Notes.Add($"Il miglior tempo girato qui ({Format(measuredReferenceMilliseconds)}) è più lento della stima: la soglia resta quella del box.");
        target.Notes.Add("È una stima dai metadati del contenuto installato, non un tempo girato: il box la userà solo come soglia d'ingresso.");
        return target;
    }

    /// <summary>Velocità media indicativa di categoria, corretta dal rapporto peso/potenza dichiarato.</summary>
    private static double ReferenceSpeedKmh(string carCategory, int powerHp, int massKg, out string note)
    {
        note = "";
        var normalized = (carCategory ?? "").Trim().ToLowerInvariant();
        // Velocità media su giro singolo, non media di gara: tarata su tempi reali
        // di riferimento (per esempio Monza 5,793 km — GT3 ≈ 1:48, formula ≈ 1:23).
        var (baseSpeed, typicalHpPerTonne) = normalized switch
        {
            "kart" => (72.0, 190.0),
            "road" => (130.0, 180.0),
            "touring" or "tcr" => (167.0, 250.0),
            "cup" => (172.0, 280.0),
            "gt4" => (175.0, 300.0),
            "historic" => (168.0, 330.0),
            "formula junior" or "formula 4" => (172.0, 260.0),
            "gt3" => (193.0, 400.0),
            "gt2" => (190.0, 420.0),
            "formula 3" => (196.0, 350.0),
            "gt1" => (200.0, 470.0),
            "formula 2" => (208.0, 500.0),
            "prototype" or "lmp" => (212.0, 520.0),
            "hypercar" => (212.0, 550.0),
            "formula" => (250.0, 900.0),
            _ => (160.0, 250.0)
        };
        if (powerHp <= 0 || massKg <= 0) return baseSpeed;
        var hpPerTonne = powerHp / (massKg / 1000.0);
        if (hpPerTonne is <= 0 or > 3000) return baseSpeed;
        // Correzione limitata: i metadati delle mod sono spesso approssimativi e
        // non devono poter stravolgere la soglia d'ingresso.
        var relative = (hpPerTonne - typicalHpPerTonne) / typicalHpPerTonne;
        var factor = Math.Clamp(1 + relative * 0.15, 0.85, 1.15);
        note = $"Peso/potenza dichiarato: {hpPerTonne:0} cv/t contro {typicalHpPerTonne:0} cv/t tipici della categoria (correzione ×{factor:0.##}).";
        return baseSpeed * factor;
    }

    /// <summary>
    /// Correzione per tipo di tracciato. Un cittadino o un kartodromo hanno una
    /// media molto più bassa di un permanente a pari vettura, e i tracciati molto
    /// lunghi sono in genere più tortuosi. È un'euristica dichiarata, non una
    /// misura: senza percorrere la pista non esiste un dato migliore.
    /// </summary>
    private static double LayoutFactor(string trackCategory, int trackLengthMeters, out string note)
    {
        note = "";
        var factor = (trackCategory ?? "").Trim().ToLowerInvariant() switch
        {
            "kartodromo" => 0.62,
            "cittadino" => 0.78,
            "hillclimb" => 0.70,
            _ => 1.0
        };
        // I tracciati oltre i dieci chilometri sono quasi sempre stradali storici.
        if (trackLengthMeters > 10000) factor *= 0.85;
        if (Math.Abs(factor - 1.0) > 0.001) note = $"Correzione per tipo di tracciato ({(string.IsNullOrWhiteSpace(trackCategory) ? "non dichiarato" : trackCategory)}{(trackLengthMeters > 10000 ? ", oltre 10 km" : "")}): ×{factor:0.##}.";
        return factor;
    }

    /// <summary>
    /// Esito della prova. La soglia "vicino" è relativa al riferimento: dieci
    /// secondi fissi erano irrilevanti su un tracciato lungo e insuperabili su un
    /// kartodromo.
    /// </summary>
    public static RookieVerdict Evaluate(int bestLapMilliseconds, int targetMilliseconds)
    {
        if (targetMilliseconds <= 0) targetMilliseconds = FallbackTargetMilliseconds;
        if (bestLapMilliseconds <= 0)
            return new RookieVerdict(0, false, false, "Nessun tempo utilizzabile nel referto: la valutazione resta aperta.");
        var ratio = bestLapMilliseconds / (double)targetMilliseconds;
        var passed = bestLapMilliseconds <= targetMilliseconds;
        var close = !passed && ratio <= CloseMargin;
        // Punteggio proporzionale allo scarto relativo, non ai secondi assoluti.
        var score = (int)Math.Clamp(Math.Round(100 - (ratio - 1) * 300), 10, 100);
        var gap = bestLapMilliseconds - targetMilliseconds;
        var summary = passed
            ? $"Obiettivo centrato: {Format(bestLapMilliseconds)} contro {Format(targetMilliseconds)} ({gap / 1000.0:+0.###;-0.###;0} s)."
            : close
                ? $"Molto vicino: {Format(bestLapMilliseconds)}, a {gap / 1000.0:0.###} s dall'obiettivo ({(ratio - 1) * 100:0.#}%)."
                : $"Ancora lontano: {Format(bestLapMilliseconds)}, a {gap / 1000.0:0.###} s dall'obiettivo ({(ratio - 1) * 100:0.#}%).";
        return new RookieVerdict(score, passed, close, summary);
    }

    public static string Format(int milliseconds) =>
        $"{milliseconds / 60000:00}:{milliseconds / 1000 % 60:00}.{milliseconds % 1000:000}";
}

public sealed record RookieVerdict(int Score, bool Passed, bool Close, string Summary);
