using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class TimelineDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly ListBox entries = new() { Left = 24, Top = 74, Width = 390, Height = 450, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly Label details = new() { Left = 445, Top = 74, Width = 430, Height = 450, AutoSize = false, ForeColor = Color.Gainsboro };
    private readonly List<(DateTime Date, string Title, string Body, string Type)> timeline = [];
    public TimelineDialog(CareerState career)
    {
        this.career = career; Text = "CorsaCareer — Timeline"; ClientSize = new Size(900, 570); StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "TIMELINE DELLA CARRIERA", Left = 24, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 21), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = "Ogni voce deriva da un evento salvato o da un risultato reale importato.", Left = 25, Top = 51, AutoSize = true, ForeColor = Color.Gainsboro });
        foreach (var item in career.Events)
        {
            var date = item.StoryDate == default ? item.DateUtc.ToLocalTime() : item.StoryDate;
            timeline.Add((date, item.Headline, $"Tipo evento: {item.Type}\nCircuito/contesto: {item.Track}\nImportanza editoriale: {item.Importance}/100\n\n{item.Headline}", item.Type));
        }
        foreach (var race in career.RaceHistory)
        {
            var date = race.StoryDate == default ? race.DateUtc.ToLocalTime() : race.StoryDate;
            var hash = ReadHash(race.ResultFile);
            var seasonLabel = race.Season > 0 ? $"S{race.Season:00}" : "storico";
            timeline.Add((date, $"Gara {seasonLabel} R{race.Round}: {race.Track} — {(race.Dnf ? "DNF" : $"P{race.Position}")}", $"{ResultProvenance.Describe(race.SourceKind)}\nStagione: {(race.Season > 0 ? $"S{race.Season:00}" : "storico precedente alla migrazione")}\nCircuito: {race.Track}\nAuto: {UiText.Car(race.Car)}\nPartenza: P{race.StartingPosition}\nQualifica: {(race.QualificationPosition > 0 ? $"P{race.QualificationPosition}" : "non disponibile")}\nGiri: {race.Laps}\nMiglior giro: {(race.BestLapMilliseconds > 0 ? FormatLap(race.BestLapMilliseconds) : "non disponibile")}\nPunti: {race.Points}\n\nFonte verificabile: {race.SourceKind}\nFile risultato: {race.ResultFile}\nSHA-256: {hash}\nImportato: {(race.ImportedUtc == default ? race.DateUtc : race.ImportedUtc):G}", "RACE_RESULT"));
        }
        foreach (var test in career.TestHistory)
        {
            var date = test.StoryDate == default ? test.DateUtc.ToLocalTime() : test.StoryDate;
            var hash = ReadHash(test.ResultFile);
            timeline.Add((date, $"Test pista: {test.Track}", $"{ResultProvenance.Describe(test.SourceKind)}\nCircuito: {test.Track}\nAuto: {UiText.Car(test.Car)}\nGiri: {test.Laps}\nMiglior giro: {(test.BestLapMilliseconds > 0 ? FormatLap(test.BestLapMilliseconds) : "non disponibile")}\n\nFonte verificabile: {test.SourceKind}\nFile risultato: {test.ResultFile}\nSHA-256: {hash}\nImportato: {(test.ImportedUtc == default ? test.DateUtc : test.ImportedUtc):G}\n\nNessun punto o risultato di campionato assegnato.", "TRACK_TEST"));
        }
        foreach (var season in career.SeasonArchive)
            timeline.Add((season.CompletedUtc.ToLocalTime(), $"Stagione {season.Season}: {season.Championship}", $"Campionato archiviato.\nPosizione finale: P{season.FinalPosition}\nPunti: {season.Points}\nVittorie: {season.Wins}\nPremio: € {season.Award:N0}", "SEASON_ARCHIVE"));
        foreach (var move in career.TeamHistory)
            timeline.Add((move.StoryDate, $"Sedile: {move.ToTeam}", $"Cambio di scuderia.\nDa: {move.FromTeam}\nA: {move.ToTeam}\nAuto: {UiText.Car(move.Car)}\nCategoria: {move.Category}\nMotivo: {move.Reason}", "TEAM_CHANGE"));
        foreach (var item in timeline.OrderByDescending(x => x.Date)) entries.Items.Add($"{NarrativeDate(item.Date)}  ·  {item.Title}");
        entries.SelectedIndexChanged += (_, _) => ShowSelected(); Controls.Add(entries); Controls.Add(details);
        if (entries.Items.Count == 0) entries.Items.Add("La timeline si costruirà dal primo evento reale.");
        else entries.SelectedIndex = 0;
    }
    private void ShowSelected() { if (entries.SelectedIndex < 0 || entries.SelectedIndex >= timeline.Count) return; var item = timeline.OrderByDescending(x => x.Date).ElementAt(entries.SelectedIndex); details.Text = $"{item.Title}\n\nData narrativa: {NarrativeDate(item.Date)}\n\n{item.Body}"; }
    private static string NarrativeDate(DateTime date) => date.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
    private static string FormatLap(int ms) => $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}";
    private static string ReadHash(string resultFile)
    {
        return ResultIntegrity.Describe(resultFile);
    }
}
