namespace CorsaCareer;

/// <summary>
/// Come si apre una schermata.
///
/// Non e decorazione: il modo in cui una finestra entra dice al giocatore che
/// tipo di momento sta cominciando. Un capitolo che si apre non deve entrare
/// come un pannello di impostazioni.
/// </summary>
public enum SceneEntrance
{
    /// <summary>Dissolvenza breve. Il modo neutro: consultazione, elenchi, dettagli.</summary>
    Fade,
    /// <summary>Dissolvenza con salita: la schermata arriva da sotto. Per le destinazioni di gioco.</summary>
    Rise,
    /// <summary>Due bande che si aprono dal centro, come una sigla. Per i momenti di racconto.</summary>
    Curtain,
    /// <summary>Lampo bianco e ingresso secco. Per gli annunci: capitoli, promozioni, bivi.</summary>
    Flash,
    /// <summary>Nessuna animazione: la schermata compare. Serve al rendering automatico e ai test.</summary>
    None
}

/// <summary>
/// Le tempistiche delle transizioni, in un posto solo.
///
/// Sono volutamente brevi. Un'animazione che si fa notare due volte diventa un
/// ostacolo fra il giocatore e quello che voleva fare: la scena deve dare il
/// senso del passaggio, non farsi aspettare.
/// </summary>
public static class CareerTransitions
{
    /// <summary>Intervallo del timer: circa 60 fotogrammi al secondo.</summary>
    public const int FrameMilliseconds = 16;

    /// <summary>Durata dell'ingresso, in fotogrammi (circa 0,3 s).</summary>
    public const int EntranceFrames = 19;

    /// <summary>Durata dell'uscita: piu rapida dell'ingresso, perche chi chiude ha gia deciso.</summary>
    public const int ExitFrames = 9;

    /// <summary>Quanto sale una schermata in ingresso, in pixel.</summary>
    public const int RiseDistance = 34;

    /// <summary>
    /// Opacita minima da cui parte una dissolvenza. Sopra lo zero: partire dal
    /// nero pieno fa percepire uno sfarfallio invece di un ingresso.
    /// </summary>
    public const double MinimumOpacity = 0.05;

    /// <summary>
    /// Attenuazione dell'avanzamento: parte veloce e rallenta alla fine. E la
    /// curva che fa sembrare il movimento intenzionale invece che meccanico.
    /// </summary>
    public static double EaseOut(double t)
    {
        var clamped = Math.Clamp(t, 0.0, 1.0);
        return 1.0 - Math.Pow(1.0 - clamped, 3);
    }

    /// <summary>Attenuazione simmetrica, per le uscite.</summary>
    public static double EaseIn(double t)
    {
        var clamped = Math.Clamp(t, 0.0, 1.0);
        return clamped * clamped;
    }

    /// <summary>
    /// Opacita di una schermata al fotogramma indicato durante l'ingresso.
    /// </summary>
    public static double EntranceOpacity(int frame, int totalFrames = EntranceFrames)
    {
        if (totalFrames <= 0) return 1.0;
        var progress = EaseOut(frame / (double)totalFrames);
        return Math.Clamp(MinimumOpacity + (1.0 - MinimumOpacity) * progress, 0.0, 1.0);
    }

    /// <summary>Opacita durante l'uscita.</summary>
    public static double ExitOpacity(int frame, int totalFrames = ExitFrames)
    {
        if (totalFrames <= 0) return 0.0;
        return Math.Clamp(1.0 - EaseIn(frame / (double)totalFrames), 0.0, 1.0);
    }

    /// <summary>
    /// Scostamento verticale in ingresso: la schermata parte piu in basso e sale
    /// al proprio posto. Zero quando l'ingresso non prevede movimento.
    /// </summary>
    public static int RiseOffset(int frame, SceneEntrance entrance, int totalFrames = EntranceFrames)
    {
        if (entrance != SceneEntrance.Rise || totalFrames <= 0) return 0;
        var progress = EaseOut(frame / (double)totalFrames);
        return (int)Math.Round(RiseDistance * (1.0 - progress));
    }

    /// <summary>
    /// Apertura del sipario: frazione di schermo ancora coperta da ciascuna delle
    /// due bande, da 0,5 (chiuso) a 0 (aperto).
    /// </summary>
    public static double CurtainCoverage(int frame, int totalFrames = EntranceFrames)
    {
        if (totalFrames <= 0) return 0.0;
        var progress = EaseOut(frame / (double)totalFrames);
        return Math.Clamp(0.5 * (1.0 - progress), 0.0, 0.5);
    }

    /// <summary>
    /// Intensita del lampo, da 1 (schermo bianco) a 0. Si esaurisce nel primo
    /// terzo dell'ingresso: dopo, resta la schermata.
    /// </summary>
    public static double FlashIntensity(int frame, int totalFrames = EntranceFrames)
    {
        var flashFrames = Math.Max(1, totalFrames / 3);
        if (frame >= flashFrames) return 0.0;
        return Math.Clamp(1.0 - frame / (double)flashFrames, 0.0, 1.0);
    }

    /// <summary>
    /// Vero se gli effetti dentro una schermata — lampeggi, titoli che entrano —
    /// devono essere eseguiti. Si spengono nel rendering automatico e nei test:
    /// un'animazione che nessuno guarda e' solo tempo speso, e una che resta a
    /// meta blocca la verifica.
    /// </summary>
    public static bool PulsesEnabled =>
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CORSACAREER_NO_ANIM"));

    /// <summary>
    /// Intensita di un lampeggio, da 1 a 0. Sale in fretta e scende piano: e la
    /// forma che fa leggere il lampeggio come un richiamo e non come un difetto
    /// di disegno.
    /// </summary>
    public static double PulseIntensity(int frame, int totalFrames)
    {
        if (totalFrames <= 0 || frame < 0 || frame >= totalFrames) return 0.0;
        var t = frame / (double)totalFrames;
        // Primo quinto in salita, il resto in discesa.
        return t < 0.2
            ? Math.Clamp(t / 0.2, 0.0, 1.0)
            : Math.Clamp(1.0 - (t - 0.2) / 0.8, 0.0, 1.0);
    }

    /// <summary>
    /// L'ingresso adatto a una schermata, dedotto dal suo tipo. Le schermate di
    /// racconto entrano come una scena, quelle di consultazione come un pannello.
    /// </summary>
    public static SceneEntrance For(string typeName) => typeName switch
    {
        "PhaseIntroDialog" or "PromotionDialog" => SceneEntrance.Flash,
        "ChapterOneDialog" or "SeasonReportDialog" or "RaceReportDialog" => SceneEntrance.Curtain,
        "CareerHubDialog" or "PaddockDialog" or "MarketDialog" or "MediaDialog"
            or "NewspaperDialog" or "OpportunityDialog" or "OfferDialog"
            or "ActivitiesDialog" or "ComicGalleryDialog" => SceneEntrance.Rise,
        _ => SceneEntrance.Fade
    };
}
