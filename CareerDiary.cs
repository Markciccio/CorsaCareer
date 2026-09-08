using System.Globalization;

namespace CorsaCareer1991;

/// <summary>Una voce del diario: un fatto della carriera con la sua data.</summary>
public sealed record DiaryEntry(
    DateTime Date,
    string DateLabel,
    string Kicker,
    string Title,
    string Summary,
    int Importance,
    string EventType,
    int EventIndex,
    bool StartsNewDay,
    string Interval);

/// <summary>
/// Diario della carriera.
///
/// Prima il giornale mostrava un solo articolo, e le altre voci vivevano in una
/// barra di scorciatoie separata («articoli e servizi») che non aveva né date né
/// ordine cronologico. Il risultato era che la carriera non si leggeva come una
/// storia: c'era il fatto corrente e un menu.
///
/// Qui ogni evento della carriera diventa una voce datata, in ordine
/// cronologico inverso, con l'intervallo rispetto alla voce precedente. Ogni voce
/// resta collegata al proprio evento, così aprirla produce l'articolo completo.
/// </summary>
public static class CareerDiary
{
    private static readonly CultureInfo It = CultureInfo.GetCultureInfo("it-IT");

    /// <summary>
    /// Costruisce il diario dagli eventi della carriera. Gli eventi archiviati
    /// restano esclusi; l'indice riporta alla posizione reale in
    /// <c>career.Events</c>, così la voce apre il proprio articolo.
    /// </summary>
    public static List<DiaryEntry> Build(CareerState career, int limit = 40)
    {
        var events = career.Events ?? [];
        var ordered = events
            .Select((story, index) => (story, index))
            .Where(x => x.story != null && !x.story.Archived)
            .OrderByDescending(x => x.story.StoryDate == default ? career.StoryDate : x.story.StoryDate)
            .ThenByDescending(x => x.story.DateUtc)
            .ThenByDescending(x => x.index)
            .Take(Math.Max(1, limit))
            .ToList();

        var entries = new List<DiaryEntry>();
        DateTime? previousDate = null;
        foreach (var (story, index) in ordered)
        {
            var date = story.StoryDate == default ? career.StoryDate : story.StoryDate;
            var startsNewDay = previousDate == null || previousDate.Value.Date != date.Date;
            // L'intervallo è calcolato verso la voce precedente in ordine di
            // lettura, cioè quella più recente: dice quanto tempo è passato.
            var interval = previousDate == null ? "" : DescribeInterval(date, previousDate.Value);
            entries.Add(new DiaryEntry(
                Date: date,
                DateLabel: FormatDate(date),
                Kicker: KickerFor(story.Type),
                Title: string.IsNullOrWhiteSpace(story.Headline) ? KickerFor(story.Type) : story.Headline,
                Summary: SummaryFor(story, career),
                Importance: story.Importance,
                EventType: story.Type ?? "",
                EventIndex: index,
                StartsNewDay: startsNewDay,
                Interval: interval));
            previousDate = date;
        }
        return entries;
    }

    public static string FormatDate(DateTime date) => date.ToString("dddd d MMMM yyyy", It);

    /// <summary>Distanza fra due voci, letta dalla più recente alla precedente.</summary>
    public static string DescribeInterval(DateTime earlier, DateTime later)
    {
        var days = (int)Math.Round((later.Date - earlier.Date).TotalDays);
        if (days <= 0) return "";
        if (days == 1) return "il giorno prima";
        if (days < 7) return $"{days} giorni prima";
        if (days < 30)
        {
            var weeks = days / 7;
            return weeks == 1 ? "una settimana prima" : $"{weeks} settimane prima";
        }
        if (days < 365)
        {
            var months = Math.Max(1, days / 30);
            return months == 1 ? "un mese prima" : $"{months} mesi prima";
        }
        var years = Math.Max(1, days / 365);
        return years == 1 ? "un anno prima" : $"{years} anni prima";
    }

    /// <summary>Etichetta di sezione del giornale per il tipo di evento.</summary>
    public static string KickerFor(string? eventType) => (eventType ?? "").ToUpperInvariant() switch
    {
        "FIRST_VICTORY" or "VICTORY" => "VITTORIA",
        "FIRST_PODIUM" or "PODIUM" => "PODIO",
        "RETIREMENT" => "RITIRO IN GARA",
        "RACE_WITHDRAWAL" => "WEEKEND NON CONCLUSO",
        "RACE_FINISHED" => "CRONACA DEL WEEKEND",
        "MILESTONE_RACE" => "TRAGUARDO DI CARRIERA",
        "FASTEST_LAP" => "GIRO VELOCE",
        "SEASON_AWARD" => "FINE CAMPIONATO",
        "PROMOTION" => "PROMOZIONE",
        "NEW_SEASON" => "NUOVA STAGIONE",
        "PROGRESSION_REVIEW" => "PAGELLA DI STAGIONE",
        "CALENDAR_PUBLISHED" => "CALENDARIO",
        "SCHEDULE_UPDATED" => "AGENDA",
        "TRACK_TEST" => "TEST IN PISTA",
        "TEST_BRIEFING" => "BRIEFING",
        "EVALUATION_PASSED" => "VALUTAZIONE SUPERATA",
        "EVALUATION_REVIEW" => "VALUTAZIONE APERTA",
        "EVALUATION_ABANDONED" => "PROVA NON COMPLETATA",
        "SESSION_ANNULLED" => "SESSIONE ANNULLATA",
        "ROOKIE_EVALUATION" => "VALUTAZIONE DEL DEBUTTO",
        "ENTRY_LEVEL_ASSIGNED" => "INGRESSO NEL PADDOCK",
        "PADDOCK_ROSTER" => "PADDOCK",
        "CONTRACT_OBJECTIVE" => "OBIETTIVO CONTRATTUALE",
        "CONTRACT_EXPIRED" => "MERCATO",
        "MARKET_INTEREST" => "MERCATO PILOTI",
        "SPONSOR_OBJECTIVE" => "SPONSOR",
        "SPONSOR_REVIEW" or "SPONSOR_WARNING" => "SPONSOR IN REVISIONE",
        "TEAM_WARNING" => "RAPPORTI CON IL TEAM",
        "RIVALRY_DUEL" or "RIVALRY_ESCALATION" => "RIVALITÀ",
        "TEAMMATE_DUEL" => "CONFRONTO INTERNO",
        "REPUTATION_CHANGE" => "REPUTAZIONE",
        "AI_CALIBRATION" => "BOX TECNICO",
        "MONTHLY_RECAP" => "RIEPILOGO DEL MESE",
        "ACTIVITY_DONE" or "ACTIVITY_SETBACK" => "FUORI DALLA PISTA",
        "WEEKEND_CANCELLED" => "WEEKEND ANNULLATO",
        _ => "DAL PADDOCK"
    };

    /// <summary>
    /// Riga di contesto della voce: usa solo dati già registrati, senza comporre
    /// l'articolo completo — il diario può contenere decine di voci.
    /// </summary>
    private static string SummaryFor(CareerEventRecord story, CareerState career)
    {
        var place = string.IsNullOrWhiteSpace(story.Track) ? "" : NarrativeEngine.Capitalize(story.Track);
        var race = career.RaceHistory?.LastOrDefault(x =>
            !string.IsNullOrWhiteSpace(story.Track) &&
            x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase) &&
            x.StoryDate.Date == story.StoryDate.Date);
        if (race != null)
        {
            if (race.Abandoned) return $"{place} · weekend non concluso · nessun punto";
            var outcome = race.Dnf ? "ritiro" : $"P{race.Position}";
            var conditions = string.IsNullOrWhiteSpace(race.WeatherLabel) ? "" : $" · {race.WeatherLabel}, {race.TemperatureC:0.#} °C";
            return $"{place} · {outcome} · {race.Points} punti · {race.Laps} giri{conditions}";
        }
        return string.IsNullOrWhiteSpace(place) ? "" : place;
    }
}
