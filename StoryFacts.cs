namespace CorsaCareer1991;

/// <summary>
/// Tutti i fatti verificati disponibili per raccontare un evento. Il generatore
/// narrativo può leggere solo questo: se un dato non è qui, non può comparire in
/// un articolo. È la barriera che impedisce di inventare biografie, incidenti o
/// dichiarazioni mai avvenute.
/// </summary>
public sealed class StoryFacts
{
    /// <summary>
    /// Vero se il pilota ha davvero una squadra. "Senza contratto" e
    /// un'etichetta di stato, non il nome di un team: usarla come soggetto
    /// produceva frasi come "Senza contratto lavorera sui dati raccolti".
    /// </summary>
    public bool HasTeam => !string.IsNullOrWhiteSpace(Team)
        && !Team.StartsWith("Senza contratto", StringComparison.OrdinalIgnoreCase);

    /// <summary>Vero se esiste uno sponsor, e non il segnaposto d'attesa.</summary>
    public bool HasSponsor => !string.IsNullOrWhiteSpace(Sponsor)
        && !Sponsor.StartsWith("In attesa", StringComparison.OrdinalIgnoreCase);

    /// <summary>Vero se c'e un contratto di cui parlare.</summary>
    public bool HasContract => HasTeam && !string.IsNullOrWhiteSpace(ContractObjective);

    /// <summary>
    /// Come chiamare la squadra dentro una frase.
    ///
    /// Team contiene l'etichetta di stato "Senza contratto" quando una squadra
    /// non c'e, e scriverla in un articolo produce frasi come "Senza contratto
    /// e Tizio: dove sta andando questo progetto". Questo nome invece si puo
    /// sempre scrivere: chi racconta usa questo, chi deve distinguere il caso
    /// usa HasTeam.
    /// </summary>
    public string TeamName => HasTeam ? Team : "il suo entourage";

    /// <summary>
    /// Come chiamare lo sponsor dentro una frase, con lo stesso criterio di
    /// <see cref="TeamName"/>.
    /// </summary>
    public string SponsorName => HasSponsor ? Sponsor : "i suoi sostenitori";

    // --- identità
    public string Driver { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Team { get; set; } = "";
    public string Teammate { get; set; } = "";
    public string Sponsor { get; set; } = "";
    public string Championship { get; set; } = "";
    public string Tier { get; set; } = "";
    public string Journalist { get; set; } = "";
    public string Publication { get; set; } = "";
    public string NarrativeStyle { get; set; } = "";
    public string ManagerName { get; set; } = "Marta Rinaldi";
    public string MechanicName { get; set; } = "Gianni Valli";
    public string RivalCharacterName { get; set; } = "Nico Valenti";
    public string ManagerMemory { get; set; } = "";
    public string MechanicMemory { get; set; } = "";

    // --- collocazione
    public string EventType { get; set; } = "";
    public DateTime StoryDate { get; set; }
    public int Season { get; set; }
    public int Round { get; set; }
    public int RoundCount { get; set; }
    public string Track { get; set; } = "";
    public string GrandPrix { get; set; } = "";

    // --- referto
    public bool HasRace { get; set; }
    public int Position { get; set; }
    public int FieldSize { get; set; }
    public bool Dnf { get; set; }
    /// <summary>Weekend preparato e non concluso: non esiste una posizione da citare.</summary>
    public bool Abandoned { get; set; }
    public int Points { get; set; }
    public int QualifyingPosition { get; set; }
    public int StartingPosition { get; set; }
    public int Laps { get; set; }
    public int PlannedLaps { get; set; }
    public int BestLapMilliseconds { get; set; }
    public int GapMilliseconds { get; set; }
    public int PitStops { get; set; }
    public double PenaltySeconds { get; set; }
    public double Damage { get; set; }
    public int TeammatePosition { get; set; }
    public string TeammateName { get; set; } = "";

    // --- condizioni scritte nel preset
    public string FormatLabel { get; set; } = "";
    public string WeatherLabel { get; set; } = "";
    public double TemperatureC { get; set; }
    public int TimeOfDaySeconds { get; set; }
    public double AiLevel { get; set; }

    // --- campionato
    public int ChampionshipPosition { get; set; }
    public int ChampionshipPoints { get; set; }
    public int LeaderPoints { get; set; }
    public string LeaderName { get; set; } = "";
    public int RoundsLeft { get; set; }

    // --- carriera
    public int Races { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int SeasonRaces { get; set; }
    public bool IsFirstWin { get; set; }
    public bool IsFirstPodium { get; set; }
    public int MilestoneRaces { get; set; }
    public List<int> RecentPositions { get; set; } = [];
    public int PointlessStreak { get; set; }
    public int PodiumStreak { get; set; }
    public int TeammateRacesWon { get; set; }
    public int TeammateRacesLost { get; set; }
    public int ExpectedPosition { get; set; }

    // --- contesto extra-pista
    public string RivalName { get; set; } = "";
    public int RivalLevel { get; set; }
    public int RivalPosition { get; set; }
    public string SponsorStatus { get; set; } = "";
    public int SponsorRelation { get; set; }
    public int TeamRelation { get; set; }
    public int Fanbase { get; set; }
    public int Fatigue { get; set; }
    public string ContractObjective { get; set; } = "";
    public string ContractObjectiveStatus { get; set; } = "";
    public int Reputation { get; set; }
    public int Cash { get; set; }
    public int Prize { get; set; }
    public string LastActivity { get; set; } = "";
    public bool LastActivitySuccess { get; set; }
    // Attività raccontata da questo articolo, quando l'evento è dell'agenda.
    public bool IsActivityStory { get; set; }
    public string ActivityCategory { get; set; } = "";
    public string ActivityChoice { get; set; } = "";
    public string ActivityAngle { get; set; } = "";
    public string ActivityStory { get; set; } = "";
    public int ActivityDays { get; set; }
    public int ActivityReputation { get; set; }
    public int ActivityFanbase { get; set; }
    public int ActivityTeamRelation { get; set; }
    public int ActivitySponsorRelation { get; set; }
    public int ActivityFatigue { get; set; }
    public int ActivityMoney { get; set; }
    public int ActivityTimesDone { get; set; }

    // --- memoria editoriale
    public string PreviousTrack { get; set; } = "";
    public int PreviousPosition { get; set; }
    public bool PreviousDnf { get; set; }
    public int SeasonsCompleted { get; set; }
    public string BestPreviousSeason { get; set; } = "";

    public string PositionLabel => Abandoned ? "weekend non concluso" : Dnf ? "ritiro" : Position > 0 ? $"P{Position}" : "posizione non disponibile";
    /// <summary>Condizioni non serene: rendono il contesto meteo una notizia.</summary>
    public bool Cloudy => !string.IsNullOrWhiteSpace(WeatherLabel)
        && (WeatherLabel.Contains("nuvol", StringComparison.OrdinalIgnoreCase) || WeatherLabel.Contains("nebbia", StringComparison.OrdinalIgnoreCase));
    public bool Scored => !Dnf && Points > 0;
    public bool OnPodium => !Dnf && Position is > 0 and <= 3;
    public bool Won => !Dnf && Position == 1;

    public static StoryFacts From(CareerState career, CareerEventRecord? story, IReadOnlyList<Round>? calendar = null)
    {
        var cast = career.StoryCast ?? [];
        var manager = cast.FirstOrDefault(x => x.Id.Equals(StoryCastService.Manager, StringComparison.OrdinalIgnoreCase));
        var mechanic = cast.FirstOrDefault(x => x.Id.Equals(StoryCastService.Mechanic, StringComparison.OrdinalIgnoreCase));
        var rivalCharacter = cast.FirstOrDefault(x => x.Id.Equals(StoryCastService.Rival, StringComparison.OrdinalIgnoreCase));
        var facts = new StoryFacts
        {
            Driver = career.Driver,
            Surname = LastWord(career.Driver),
            Team = career.Team,
            Teammate = career.Teammate,
            Sponsor = career.Sponsor,
            Championship = career.Championship,
            Tier = career.Tier,
            Journalist = career.Journalist?.Name ?? "",
            Publication = career.Journalist?.Publication ?? "",
            NarrativeStyle = career.Journalist?.NarrativeStyle ?? "",
            ManagerName = manager?.Name ?? "Marta Rinaldi",
            MechanicName = mechanic?.Name ?? "Gianni Valli",
            RivalCharacterName = rivalCharacter?.Name ?? "Nico Valenti",
            ManagerMemory = manager?.LastReaction ?? "",
            MechanicMemory = mechanic?.LastReaction ?? "",
            EventType = story?.Type ?? "",
            StoryDate = story != null && story.StoryDate != default ? story.StoryDate : career.StoryDate,
            Season = career.Season,
            Round = career.Round,
            RoundCount = calendar?.Count ?? 0,
            Track = story?.Track ?? "",
            Reputation = career.Reputation,
            Cash = career.Cash,
            Races = career.Races,
            Wins = career.Wins,
            Podiums = career.Podiums,
            SponsorStatus = career.SponsorObjectiveStatus,
            SponsorRelation = career.SponsorRelation,
            TeamRelation = career.TeamRelation,
            Fanbase = career.Fanbase,
            Fatigue = career.Fatigue,
            ContractObjective = career.ContractObjective,
            ContractObjectiveStatus = career.ContractObjectiveStatus,
            TeammateRacesWon = career.TeammateRacesWon,
            TeammateRacesLost = career.TeammateRacesLost,
            SeasonsCompleted = career.SeasonArchive.Count
        };

        var race = ResolveRace(career, story);
        if (race != null)
        {
            facts.HasRace = true;
            facts.Position = race.Position;
            facts.FieldSize = Math.Max(1, race.Classification.Count);
            facts.Dnf = race.Dnf;
            facts.Abandoned = race.Abandoned;
            facts.Points = race.Points;
            facts.QualifyingPosition = race.QualificationPosition;
            facts.StartingPosition = race.StartingPosition;
            facts.Laps = race.Laps;
            facts.PlannedLaps = race.PlannedLaps;
            facts.BestLapMilliseconds = race.BestLapMilliseconds;
            facts.GapMilliseconds = race.GapMilliseconds;
            facts.PitStops = race.PitStops;
            facts.PenaltySeconds = race.PenaltySeconds;
            facts.Damage = race.Damage;
            facts.TeammatePosition = race.TeammatePosition;
            facts.TeammateName = race.TeammateName;
            facts.FormatLabel = race.FormatLabel;
            facts.WeatherLabel = race.WeatherLabel;
            facts.TemperatureC = race.TemperatureC;
            facts.TimeOfDaySeconds = race.TimeOfDaySeconds;
            facts.AiLevel = race.AiLevel;
            facts.Prize = race.Prize;
            facts.Track = string.IsNullOrWhiteSpace(facts.Track) ? race.Track : facts.Track;

            var seasonRaces = career.RaceHistory.Where(x => x.Season == race.Season).OrderBy(x => x.Round).ToList();
            facts.SeasonRaces = seasonRaces.Count;
            var index = seasonRaces.FindIndex(x => ReferenceEquals(x, race));
            var earlier = index > 0 ? seasonRaces.Take(index).ToList() : new List<RaceHistoryEntry>();
            facts.RecentPositions = earlier.TakeLast(5).Select(x => x.Dnf ? 0 : x.Position).ToList();
            facts.IsFirstWin = race.Position == 1 && !race.Dnf && !earlier.Any(x => !x.Dnf && x.Position == 1)
                && !career.RaceHistory.Any(x => x.Season < race.Season && !x.Dnf && x.Position == 1);
            facts.IsFirstPodium = !race.Dnf && race.Position <= 3 && !earlier.Any(x => !x.Dnf && x.Position <= 3)
                && !career.RaceHistory.Any(x => x.Season < race.Season && !x.Dnf && x.Position <= 3);
            facts.MilestoneRaces = career.Races is 50 or 100 ? career.Races : 0;
            facts.PointlessStreak = CountStreak(earlier, x => x.Dnf || x.Points == 0);
            facts.PodiumStreak = CountStreak(earlier, x => !x.Dnf && x.Position <= 3) + (facts.OnPodium ? 1 : 0);
            var previous = earlier.LastOrDefault();
            if (previous != null)
            {
                facts.PreviousTrack = previous.Track;
                facts.PreviousPosition = previous.Position;
                facts.PreviousDnf = previous.Dnf;
            }
            facts.Round = Math.Max(0, race.Round - 1);
        }

        var standings = career.Standings.OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).ToList();
        if (standings.Count > 0)
        {
            facts.LeaderName = standings[0].Driver;
            facts.LeaderPoints = standings[0].Points;
            var own = standings.FindIndex(x => x.Driver.Equals(career.Driver, StringComparison.OrdinalIgnoreCase));
            facts.ChampionshipPosition = own >= 0 ? own + 1 : 0;
            facts.ChampionshipPoints = own >= 0 ? standings[own].Points : career.Points;
        }
        facts.RoundsLeft = Math.Max(0, facts.RoundCount - career.Round);

        var rival = career.Rivalries.OrderByDescending(x => x.Level).FirstOrDefault();
        if (rival != null)
        {
            facts.RivalName = rival.Rival;
            facts.RivalLevel = rival.Level;
            if (race != null)
                facts.RivalPosition = race.Classification.FirstOrDefault(x => x.Name.Equals(rival.Rival, StringComparison.OrdinalIgnoreCase))?.Position ?? 0;
        }

        facts.ExpectedPosition = ReputationEngine.ExpectedPosition(career.SeatPrestige, Math.Max(1, facts.FieldSize));

        var activity = career.ActivityHistory.LastOrDefault();
        if (activity != null) { facts.LastActivity = activity.Name; facts.LastActivitySuccess = activity.Success; }

        var bestSeason = career.SeasonArchive.Where(x => x.FinalPosition > 0).OrderBy(x => x.FinalPosition).FirstOrDefault();
        if (bestSeason != null) facts.BestPreviousSeason = $"stagione {bestSeason.Season} chiusa P{bestSeason.FinalPosition} in {bestSeason.Championship}";

        // Se l'evento appartiene all'agenda, il servizio racconta quella
        // attività: si aggancia il record corrispondente all'evento, non
        // semplicemente l'ultimo in ordine di tempo.
        if (facts.EventType is "ACTIVITY_DONE" or "ACTIVITY_SETBACK")
        {
            var told = story == null || story.StoryDate == default
                ? activity
                : career.ActivityHistory
                    .Where(x => x.StoryDate <= story.StoryDate.AddDays(1))
                    .OrderBy(x => Math.Abs((x.StoryDate - story.StoryDate).Ticks))
                    .FirstOrDefault() ?? activity;
            if (told != null) facts.ApplyActivity(told, career);
        }

        return facts;
    }

    private void ApplyActivity(ActivityRecord record, CareerState career)
    {
        var definition = OffTrackActivities.Find(record.ActivityId);
        IsActivityStory = true;
        LastActivity = record.Name;
        LastActivitySuccess = record.Success;
        ActivityCategory = definition?.Category ?? "";
        ActivityChoice = record.ChoiceLabel;
        ActivityAngle = record.ChoiceAngle;
        ActivityStory = record.Story;
        ActivityDays = record.Days;
        ActivityReputation = record.Reputation;
        ActivityFanbase = record.Fanbase;
        ActivityTeamRelation = record.TeamRelation;
        ActivitySponsorRelation = record.SponsorRelation;
        ActivityFatigue = record.Fatigue;
        ActivityMoney = record.Money;
        ActivityTimesDone = career.ActivityHistory.Count(x => x.Season == record.Season && x.ActivityId == record.ActivityId);
        StoryDate = record.StoryDate == default ? StoryDate : record.StoryDate;
    }


    private static RaceHistoryEntry? ResolveRace(CareerState career, CareerEventRecord? story)
    {
        if (career.RaceHistory.Count == 0) return null;
        // Un evento che non è una gara non deve agganciare un referto: altrimenti
        // il racconto di un test citerebbe punti e posizioni di un'altra domenica.
        // RACE_WITHDRAWAL è incluso: il weekend abbandonato ha una voce in
        // RaceHistory (senza posizione d'arrivo) e il racconto deve leggerla,
        // altrimenti il ritiro verrebbe narrato come un evento senza contesto.
        var raceEvents = new[] { "FIRST_VICTORY", "VICTORY", "FIRST_PODIUM", "PODIUM", "RETIREMENT", "RACE_FINISHED", "MILESTONE_RACE", "FASTEST_LAP", "SEASON_AWARD", "RIVALRY_DUEL", "TEAMMATE_DUEL", "RIVALRY_ESCALATION", "RACE_WITHDRAWAL" };
        if (story != null && !string.IsNullOrWhiteSpace(story.Type) && !raceEvents.Contains(story.Type, StringComparer.OrdinalIgnoreCase)) return null;
        if (story == null) return career.RaceHistory[^1];
        var candidates = career.RaceHistory.Where(x => string.IsNullOrWhiteSpace(story.Track) || x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase)).ToList();
        if (candidates.Count == 0) return career.RaceHistory[^1];
        if (story.DateUtc == default) return candidates[^1];
        return candidates.Where(x => x.DateUtc <= story.DateUtc.AddSeconds(2)).OrderByDescending(x => x.DateUtc).FirstOrDefault() ?? candidates[^1];
    }

    private static int CountStreak(List<RaceHistoryEntry> races, Func<RaceHistoryEntry, bool> predicate)
    {
        var streak = 0;
        for (var i = races.Count - 1; i >= 0; i--)
        {
            if (!predicate(races[i])) break;
            streak++;
        }
        return streak;
    }

    private static string LastWord(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "" : parts[^1];
    }
}
