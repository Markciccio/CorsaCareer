namespace CorsaCareer;

/// <summary>
/// L'illustrazione che accompagna un momento della carriera.
///
/// La scelta guardava soltanto il tipo di evento, e il ripiego era una
/// fotografia di GT: un test con un kart a quattro tempi si presentava con un
/// pit stop di GT3, cioè con un'immagine che raccontava un'altra carriera.
///
/// Qui pesa anche il gradino della scala. Un'immagine che contraddice quello che
/// sta accadendo è peggio di nessuna immagine: se per quel momento non c'è
/// niente di coerente, questa funzione non restituisce nulla e la scheda lo
/// dichiara invece di riempire il buco.
/// </summary>
public static class EditorialAssets
{
    /// <summary>
    /// Illustrazione per un evento, tenendo conto del gradino su cui corre il
    /// pilota. Stringa vuota quando non esiste un'immagine coerente.
    /// </summary>
    public static string ForEvent(CareerEventRecord story, LadderRung? rung = null)
    {
        var type = story.Type ?? "";

        // I momenti che hanno un'immagine propria, indipendente dalla categoria:
        // sono scene di paddock, non di pista.
        var byType = type switch
        {
            "EVALUATION_STARTED" or "ENTRY_LEVEL_ASSIGNED" => "manga-01-rookie-dawn.png",
            "TEST_BRIEFING" => "manga-02-first-test.png",
            "EVALUATION_PASSED" or "CONTRACT_SIGNING" => "manga-sponsor-table.png",
            "EVALUATION_REVIEW" or "INVITATION_SETBACK" or "RETIREMENT" or "ACTIVITY_SETBACK"
                or "PROMISE_FAILED" or "TEAM_WARNING" or "SPONSOR_WARNING" => "manga-night-garage.png",
            "FINANCIAL_TROUBLE" or "SPONSOR_REVIEW" or "CONTRACT_EXPIRED" => "manga-08-financial-crisis.png",
            "PODIUM" or "FIRST_PODIUM" or "RACE_PODIUM" => "manga-09-first-podium.png",
            "FIRST_OR_NEXT_VICTORY" or "FIRST_VICTORY" or "VICTORY" or "RACE_WIN" => "manga-10-first-victory.png",
            "PROMOTION" or "TEAM_CHANGE" or "PROGRESSION_REVIEW" => "manga-11-promotion.png",
            "MARKET_INTEREST" or "OPPORTUNITY_ACCEPTED" => "manga-12-professional.png",
            "SEASON_AWARD" or "CHAMPIONSHIP_WON" => "manga-13-champion.png",
            "INVITATION_BREAKTHROUGH" or "INVITATION_OFFERED" or "SCHEDULE_UPDATED"
                or "RIVALRY_DUEL" or "RIVALRY_ESCALATION" or "TEAMMATE_DUEL" => "manga-rival-grid.png",
            "PHOTO_ARCHIVE" => "",
            _ => ""
        };
        if (byType.Length > 0) return Resolve(byType);

        // Le scene di pista dipendono dal mezzo. La fotografia di una GT sopra
        // una sessione con i kart e' semplicemente falsa.
        if (type == "TRACK_TEST")
            return rung?.Path == LadderPath.Karting || rung == null
                ? Resolve("calendar-rookie-rain.png")
                : Resolve("portal-hero-gt2.png");

        // Nessun ripiego generico: senza un'immagine coerente si dichiara la
        // mancanza, invece di mostrarne una che racconta un'altra carriera.
        return "";
    }

    private static string Resolve(string file) => AssetPaths.File(file);
}
