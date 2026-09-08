using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class StandingsDialog : CareerDialog
{
    private readonly ListBox standings = new() { Left = 24, Top = 76, Width = 430, Height = 430, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly RichTextBox details = new() { Left = 480, Top = 76, Width = 390, Height = 430, BackColor = Color.FromArgb(24, 28, 37), ForeColor = Color.Gainsboro, BorderStyle = BorderStyle.None, ReadOnly = true };
    private readonly CareerState career;
    public StandingsDialog(CareerState career)
    {
        this.career = career;
        Text = "CorsaCareer — classifiche e storico"; ClientSize = new Size(900, 560); StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "CLASSIFICHE E STORICO", Left = 24, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 21, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = $"S{career.Season:00} · {career.Championship} · dati derivati dai referti reali", Left = 26, Top = 52, AutoSize = true, ForeColor = Color.Gainsboro });
        foreach (var row in career.Standings.OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).Select((x, i) => $"P{i + 1,2}  {x.Driver,-25} {x.Points,3} pt · {x.Races} gare · {x.Wins} vittorie")) standings.Items.Add(row);
        if (standings.Items.Count == 0) standings.Items.Add("La classifica nascerà dal primo risultato reale.");
        standings.SelectedIndexChanged += (_, _) => ShowDetails(standings.SelectedIndex); Controls.Add(standings); Controls.Add(details);
        if (career.Standings.Count > 0) standings.SelectedIndex = 0;
        var close = new Button { Text = "Chiudi", Left = 24, Top = 520, Width = 120, Height = 30, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; Controls.Add(close); AcceptButton = close;
    }
    private void ShowDetails(int index)
    {
        var ordered = career.Standings.OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).ToList();
        if (index < 0 || index >= ordered.Count) { details.Text = "Seleziona un pilota."; return; }
        var selected = ordered[index];
        var races = career.RaceHistory.Where(x => x.Classification.Any(p => p.Name.Equals(selected.Driver, StringComparison.OrdinalIgnoreCase)) || x.Track.Length > 0 && selected.Driver.Equals(career.Driver, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.DateUtc).Take(8).ToList();
        details.Text = $"{selected.Driver}\n\nPunti: {selected.Points}\nGare: {selected.Races}\nVittorie: {selected.Wins}\n\nULTIMI REFERTI ARCHIVIATI\n{(races.Count == 0 ? "nessun referto disponibile" : string.Join("\n", races.Select(x => $"{x.Track}: {(x.Dnf ? "DNF" : $"P{x.Position}")} · {x.Points} pt")))}\n\nSTAGIONI CONCLUSE\n{(career.SeasonArchive.Count == 0 ? "nessuna stagione archiviata" : string.Join("\n", career.SeasonArchive.OrderByDescending(x => x.Season).Take(6).Select(x => $"S{x.Season} {x.Championship}: P{x.FinalPosition}, {x.Points} pt")))}";
    }
}
