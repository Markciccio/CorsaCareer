using System.Text.Json;

namespace CorsaCareer;

public sealed class ImportedRaceResult
{
    public string Track { get; init; } = "";
    public string PlayerName { get; init; } = "";
    public string Car { get; init; } = "";
    public int Position { get; init; }
    public int StartingPosition { get; init; } = 1;
    public int Laps { get; init; }
    public int BestLapMilliseconds { get; init; }
    public int GapMilliseconds { get; init; }
    public int PitStops { get; init; }
    public double PenaltySeconds { get; init; }
    public double Damage { get; init; }
    public int QualificationPosition { get; init; }
    public string SessionName { get; init; } = "Gara";
    public bool Dnf { get; init; }
    public List<ImportedDriverResult> Classification { get; init; } = [];
}

public sealed class ImportedDriverResult
{
    public string Name { get; init; } = "";
    public string Car { get; init; } = "";
    public int Position { get; init; }
    public bool IsPlayer { get; init; }
    // Miglior giro del singolo partecipante letto dal referto: serve ad
    // assegnare il punto del giro veloce senza inventarlo. 0 = non disponibile.
    public int BestLapMilliseconds { get; init; }
}

public static class RaceResultParser
{
    public static bool TryParse(string json, out ImportedRaceResult result, int preferredSessionType = 3, string? expectedPlayerName = null)
    {
        result = new ImportedRaceResult();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var players = root.GetProperty("players").EnumerateArray().ToList();
            if (players.Count == 0) return false;
            var playerIndex = ReadPlayerIndex(root, players, expectedPlayerName); // fallback: AC serializza normalmente il locale per primo.
            if (playerIndex < 0 || playerIndex >= players.Count) return false;
            var player = players[playerIndex];
            var sessions = root.GetProperty("sessions").EnumerateArray().ToList();
            var session = sessions.FirstOrDefault(x => x.TryGetProperty("type", out var t) && t.GetInt32() == preferredSessionType);
            if (session.ValueKind == JsonValueKind.Undefined) return false;
            var hasClassification = session.TryGetProperty("raceResult", out var order) && order.ValueKind == JsonValueKind.Array;
            // Una sessione di test/pratica reale può registrare solo bestLaps
            // e telemetria, senza una classifica. Le gare invece richiedono
            // sempre raceResult per poter assegnare un esito.
            if (!hasClassification && preferredSessionType != 1) return false;
            var classification = hasClassification ? order.EnumerateArray().Select(x => x.GetInt32()).ToList() : new List<int>();
            var trackId = root.TryGetProperty("track", out var trackValue) ? trackValue.GetString() ?? "" : "";
            var carId = player.TryGetProperty("car", out var carValue) ? carValue.GetString() ?? "" : "";
            if ((preferredSessionType != 1 && classification.Count == 0) || classification.Any(index => index < 0 || index >= players.Count) || classification.Distinct().Count() != classification.Count || string.IsNullOrWhiteSpace(trackId) || string.IsNullOrWhiteSpace(carId)) return false;
            var position = classification.Count == 0 ? 0 : classification.IndexOf(playerIndex) + 1;
            var dnf = classification.Count > 0 && position == 0;
            if (dnf) position = classification.Count + 1;
            var qualifying = sessions.FirstOrDefault(x => x.TryGetProperty("type", out var t) && t.GetInt32() == 2);
            var qualifyingOrder = qualifying.ValueKind != JsonValueKind.Undefined && qualifying.TryGetProperty("raceResult", out var qualifyingResult)
                ? qualifyingResult.EnumerateArray().Select(x => x.GetInt32()).ToList() : new List<int>();
            var qualificationPosition = qualifyingOrder.IndexOf(playerIndex) + 1;
            var laps = 0;
            if (session.TryGetProperty("lapstotal", out var totals) && totals.ValueKind == JsonValueKind.Array && totals.GetArrayLength() > playerIndex) laps = totals[playerIndex].GetInt32();
            var bestLap = ExtractBestLap(session, playerIndex);
            if (root.TryGetProperty("extras", out var extras))
            {
                var best = extras.EnumerateArray().FirstOrDefault(x => x.TryGetProperty("name", out var n) && n.GetString() == "bestlap");
                if (best.ValueKind != JsonValueKind.Undefined && bestLap == 0 && best.TryGetProperty("time", out var time) && time.TryGetInt32(out var extraTime)) bestLap = extraTime;
            }
            var startingPosition = ReadIndexedInt(session, playerIndex, "grid", "startingPositions", "startPositions");
            if (startingPosition <= 0) startingPosition = qualificationPosition > 0 ? qualificationPosition : 1;
            var gap = ReadIndexedInt(session, playerIndex, "gaps", "gapMilliseconds", "gap");
            var pitStops = ReadIndexedInt(session, playerIndex, "pitstops", "pitStops", "pit_stop_count");
            var penalty = ReadIndexedDouble(session, playerIndex, "penalties", "penaltySeconds", "penalty");
            var damage = ReadIndexedDouble(session, playerIndex, "damage", "damages");
            result = new ImportedRaceResult
            {
                Track = trackId,
                PlayerName = player.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                Car = carId,
                Position = position, StartingPosition = startingPosition, Laps = laps, BestLapMilliseconds = bestLap, GapMilliseconds = gap, PitStops = pitStops, PenaltySeconds = penalty, Damage = damage, QualificationPosition = qualificationPosition, SessionName = session.TryGetProperty("name", out var sessionName) ? sessionName.GetString() ?? "Gara" : "Gara", Dnf = dnf,
                Classification = classification.Select((index, orderPosition) => new ImportedDriverResult
                {
                    Name = index >= 0 && index < players.Count && players[index].TryGetProperty("name", out var participantName) ? participantName.GetString() ?? "Pilota sconosciuto" : "Pilota sconosciuto",
                    Car = index >= 0 && index < players.Count && players[index].TryGetProperty("car", out var participantCar) ? participantCar.GetString() ?? "" : "",
                    Position = orderPosition + 1,
                    IsPlayer = index == playerIndex,
                    BestLapMilliseconds = ExtractBestLap(session, index)
                }).ToList()
            };
            return true;
        }
        // Un race_out.json può essere troncato se Assetto Corsa/CSP viene
        // chiuso durante la scrittura. Qualunque forma non interpretabile deve
        // restare un rifiuto controllato: mai propagare l'errore al timer UI e
        // mai avanzare la carriera con dati parziali.
        catch (JsonException) { return false; }
        catch (InvalidOperationException) { return false; }
        catch (KeyNotFoundException) { return false; }
        catch (FormatException) { return false; }
        catch (OverflowException) { return false; }
    }

    private static int ReadIndexedInt(JsonElement element, int index, params string[] names)
    {
        foreach (var name in names)
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > index && value[index].TryGetInt32(out var number)) return number;
        return 0;
    }

    private static int ReadPlayerIndex(JsonElement root, IReadOnlyList<JsonElement> players, string? expectedPlayerName)
    {
        foreach (var property in new[] { "playerIndex", "localPlayerIndex", "player_id" })
            if (root.TryGetProperty(property, out var value) && value.TryGetInt32(out var index)) return index;
        if (root.TryGetProperty("player", out var player))
        {
            if (player.TryGetInt32(out var index)) return index;
            if (player.ValueKind == JsonValueKind.String)
            {
                var name = player.GetString() ?? "";
                var match = players.Select((item, index) => (item, index)).FirstOrDefault(x => x.item.TryGetProperty("name", out var n) && string.Equals(n.GetString(), name, StringComparison.OrdinalIgnoreCase));
                if (match.item.ValueKind != JsonValueKind.Undefined) return match.index;
            }
        }
        if (!string.IsNullOrWhiteSpace(expectedPlayerName))
        {
            var expected = expectedPlayerName.Trim();
            var match = players.Select((item, index) => (item, index)).FirstOrDefault(x => x.item.TryGetProperty("name", out var n) && string.Equals(n.GetString()?.Trim(), expected, StringComparison.OrdinalIgnoreCase));
            if (match.item.ValueKind != JsonValueKind.Undefined) return match.index;
        }
        return 0;
    }

    private static int ExtractBestLap(JsonElement session, int index)
    {
        if (!session.TryGetProperty("bestLaps", out var laps) || laps.ValueKind != JsonValueKind.Array) return 0;
        if (laps.GetArrayLength() > index && laps[index].ValueKind == JsonValueKind.Number && laps[index].TryGetInt32(out var direct)) return direct;
        foreach (var item in laps.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            var carIndex = ReadObjectInt(item, "carId", "car", "player", "index");
            if (carIndex != index) continue;
            var time = ReadObjectInt(item, "time", "lapTime", "milliseconds");
            if (time > 0) return time;
        }
        return 0;
    }

    private static int ReadObjectInt(JsonElement element, params string[] names)
    {
        foreach (var name in names) if (element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number)) return number;
        return 0;
    }
    private static double ReadIndexedDouble(JsonElement element, int index, params string[] names)
    {
        foreach (var name in names)
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > index && value[index].TryGetDouble(out var number)) return number;
        return 0;
    }
}

public sealed class RaceHistoryEntry
{
    public int Season { get; set; }
    public int Round { get; set; }
    public DateTime DateUtc { get; set; }
    public DateTime StoryDate { get; set; }
    public string Track { get; set; } = "";
    public string Car { get; set; } = "";
    public int Position { get; set; }
    public int TeammatePosition { get; set; }
    public string TeammateName { get; set; } = "";
    public int StartingPosition { get; set; }
    public int QualificationPosition { get; set; }
    public string SessionName { get; set; } = "Gara";
    public int Laps { get; set; }
    public int BestLapMilliseconds { get; set; }
    public int GapMilliseconds { get; set; }
    public int PitStops { get; set; }
    public double PenaltySeconds { get; set; }
    public double Damage { get; set; }
    public bool Dnf { get; set; }
    public int Points { get; set; }
    public int Prize { get; set; }
    public int SponsorBonus { get; set; }
    // Snapshot economici e sportivi del momento: lo storico non deve
    // ricalcolare il passato usando i valori correnti della carriera.
    public int CashDelta { get; set; }
    public int CashAfter { get; set; }
    public int FitnessDelta { get; set; }
    public int FitnessAfter { get; set; }
    public int TrustDelta { get; set; }
    public int TrustAfter { get; set; }
    public int LogisticsPaid { get; set; }
    public int DamagePaid { get; set; }
    public int UnpaidCosts { get; set; }
    public string PhotoPath { get; set; } = "";
    public string ResultFile { get; set; } = "";
    public string ResultSha256 { get; set; } = "";
    public DateTime ImportedUtc { get; set; }
    public string SourceKind { get; set; } = "ASSETTO_CORSA_RACE_OUT";
    // Condizioni realmente scritte nel preset Content Manager di questa sessione:
    // sono fatti verificabili, non colore narrativo aggiunto dopo.
    public string FormatLabel { get; set; } = "";
    public string WeatherId { get; set; } = "";
    public string WeatherLabel { get; set; } = "";
    public double TemperatureC { get; set; }
    public int TimeOfDaySeconds { get; set; }
    public int PlannedLaps { get; set; }
    public double AiLevel { get; set; }
    /// <summary>
    /// Weekend preparato e non concluso: nessun referto utilizzabile. Non ha
    /// posizione d'arrivo — il racconto non deve mai attribuirgliene una.
    /// </summary>
    public bool Abandoned { get; set; }
    public List<RaceParticipantSnapshot> Classification { get; set; } = [];
}

public sealed class RaceParticipantSnapshot
{
    public string Name { get; set; } = "";
    public string Car { get; set; } = "";
    public int Position { get; set; }
    public bool IsPlayer { get; set; }
}

public sealed class TestSessionRecord
{
    public DateTime DateUtc { get; set; }
    public DateTime StoryDate { get; set; }
    public string Track { get; set; } = "";
    public string Car { get; set; } = "";
    public string SessionName { get; set; } = "Test in pista";
    public int Laps { get; set; }
    public int BestLapMilliseconds { get; set; }
    public string PhotoPath { get; set; } = "";
    public string ResultFile { get; set; } = "";
    public string ResultSha256 { get; set; } = "";
    public DateTime ImportedUtc { get; set; }
    public string SourceKind { get; set; } = "ASSETTO_CORSA_RACE_OUT";
    public string WeatherLabel { get; set; } = "";
    public double TemperatureC { get; set; }
    public int TimeOfDaySeconds { get; set; }
    /// <summary>Valori spiegabili nel calendario: un test deve lasciare una traccia, non solo un giro.</summary>
    public int TargetLapMilliseconds { get; set; }
    public int EvaluationScore { get; set; }
    public int CashDelta { get; set; }
    public int ReputationDelta { get; set; }
    public int CashAfter { get; set; }
    public int FitnessDelta { get; set; }
    public int FitnessAfter { get; set; }
    public int TrustDelta { get; set; }
    public int TrustAfter { get; set; }
    public string Outcome { get; set; } = "";
    public string ConsequenceSummary { get; set; } = "";
}

public sealed class CareerEventRecord
{
    public DateTime DateUtc { get; set; }
    public DateTime StoryDate { get; set; }
    public string Type { get; set; } = "";
    public string Headline { get; set; } = "";
    public string Track { get; set; } = "";
    public int Importance { get; set; }
    public string PhotoPath { get; set; } = "";
    public string PhotoView { get; set; } = "";
    public bool Archived { get; set; }
}

public sealed class StoryArcRecord
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime StartedStoryDate { get; set; }
    public DateTime UpdatedStoryDate { get; set; }
    public string Status { get; set; } = "In corso";
    public int Importance { get; set; }
    public List<string> EventTypes { get; set; } = [];
    public string Summary { get; set; } = "";
}

/// <summary>
/// La firma del giornale della carriera.
///
/// I nomi qui devono essere inventati e non corrispondere a giornalisti o piloti
/// reali, nemmeno per assonanza: i predecessori erano alias riconoscibili di
/// persone esistenti, uno con la biografia reale di un pilota realmente
/// vissuto. Un articolo generato non deve mettere parole in bocca a nessuno.
/// </summary>
public sealed class JournalistProfile
{
    public string Name { get; set; } = "Noa Minazuki";
    public string Publication { get; set; } = "Grand Prix CorsaCareer";
    public string Tone { get; set; } = "cronaca sportiva originale";
    public string NarrativeStyle { get; set; } = "Cronaca classica analitica";
    public int MemoryYears { get; set; } = 99;
    public string ExpertCommentator { get; set; } = "Vittorio Sanna";
    public string ExpertCommentatorBio { get; set; } = "ex pilota di formule minori, oggi commentatore tecnico — personaggio inventato";
}

public sealed class TeamHistoryEntry
{
    public DateTime StoryDate { get; set; }
    public string FromTeam { get; set; } = "";
    public string ToTeam { get; set; } = "";
    public string Car { get; set; } = "";
    public string Category { get; set; } = "";
    public string Reason { get; set; } = "";
}

public sealed class PersistentDriver
{
    public string Name { get; set; } = "";
    public string Nationality { get; set; } = "";
    public string Team { get; set; } = "";
    public string Car { get; set; } = "";
    public int Age { get; set; } = 25;
    public int Speed { get; set; } = 70;
    public int Consistency { get; set; } = 70;
    public int Aggression { get; set; } = 50;
    public int Experience { get; set; }
    public string Specialization { get; set; } = "all-rounder";
    public string Category { get; set; } = "";
    public string Relationship { get; set; } = "neutrale";
    public int Skill { get; set; } = 70;
    public int Reputation { get; set; } = 20;
    public int Races { get; set; }
    public int Wins { get; set; }
    public int Points { get; set; }
    public int LastPosition { get; set; }
    public string LastTrack { get; set; } = "";
}

public sealed class StandingEntry
{
    public string Driver { get; set; } = "";
    public int Points { get; set; }
    public int Races { get; set; }
    public int Wins { get; set; }
}

public sealed class RivalryRecord
{
    public string Rival { get; set; } = "";
    public int Level { get; set; }
    public int Duels { get; set; }
    public int PlayerWins { get; set; }
    public int RivalWins { get; set; }
    public string Story { get; set; } = "";
}

public sealed class SeasonSummary
{
    public int Season { get; set; }
    public string Championship { get; set; } = "";
    public string Tier { get; set; } = "";
    public DateTime CompletedUtc { get; set; }
    public int Points { get; set; }
    public int Wins { get; set; }
    public int FinalPosition { get; set; }
    public int Award { get; set; }

    /// <summary>Vero se la stagione e stata chiusa al primo posto.</summary>
    public bool TitleWon { get; set; }

    /// <summary>
    /// Vero se il titolo e stato conquistato sul gradino piu alto della propria
    /// strada: e il mondiale, il traguardo della carriera.
    /// </summary>
    public bool WorldTitle { get; set; }
}
