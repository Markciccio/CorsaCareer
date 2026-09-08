using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class RaceReportDialog : CareerDialog
{
    public RaceReportDialog(CareerState career, ImportedRaceResult result, string photoPath, Action? listen = null, Action? openMedia = null)
    {
        Text = "CorsaCareer — servizio post-gara";
        ClientSize = new Size(980, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        Controls.Add(new Label { Text = "SERVIZIO POST-GARA", Left = 24, Top = 18, AutoSize = true, Font = new Font("Segoe UI", 21, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = $"{result.Track}  ·  {career.Driver}  ·  referto reale Assetto Corsa", Left = 26, Top = 52, AutoSize = true, ForeColor = Color.Gainsboro });

        var summary = new RichTextBox { Left = 24, Top = 86, Width = 430, Height = 190, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
        summary.Text = $"VERDETTO\n\n{(result.Dnf ? "RITIRO" : $"POSIZIONE  P{result.Position}")}\nPartenza: P{result.StartingPosition}   ·   Qualifica: {(result.QualificationPosition > 0 ? $"P{result.QualificationPosition}" : "n/d")}\nGiri: {result.Laps}   ·   Sessione: {result.SessionName}\nBest lap: {(result.BestLapMilliseconds > 0 ? FormatLap(result.BestLapMilliseconds) : "n/d")}\nDistacco: {(result.GapMilliseconds > 0 ? FormatGap(result.GapMilliseconds) : "n/d")}\nPit stop: {result.PitStops}   ·   Penalità: {result.PenaltySeconds:0.0}s\nDanni: {result.Damage:0.0}%";
        Controls.Add(summary);

        var consequences = new RichTextBox { Left = 24, Top = 292, Width = 430, Height = 190, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.Gainsboro };
        var teammate = career.RaceHistory.LastOrDefault()?.TeammatePosition > 0 ? $"Compagno: P{career.RaceHistory.Last().TeammatePosition}" : "Compagno: dato non disponibile";
        consequences.Text = $"CONSEGUENZE\n\nPunti campionato: {career.Points}\nBudget: € {career.Cash:N0}\nReputazione: {career.Reputation}/100\n{teammate}\nSponsor: {career.Sponsor} · bonus stagione € {career.SponsorMoney:N0}\nObiettivo contratto: {career.ContractObjectiveStatus}\nObiettivo sponsor: {career.SponsorObjectiveStatus}";
        Controls.Add(consequences);

        var classification = new ListBox { Left = 480, Top = 86, Width = 470, Height = 396, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
        foreach (var participant in result.Classification.OrderBy(x => x.Position)) classification.Items.Add($"P{participant.Position,2}  {participant.Name,-24}  {UiText.Car(participant.Car)}{(participant.IsPlayer ? "  ◀ TU" : "")}");
        if (classification.Items.Count == 0) classification.Items.Add("Classifica non disponibile nel referto.");
        Controls.Add(classification);

        if (File.Exists(photoPath))
        {
            var image = new PictureBox { Left = 480, Top = 495, Width = 190, Height = 105, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(12, 15, 20), BorderStyle = BorderStyle.FixedSingle, Image = Image.FromFile(photoPath) };
            Controls.Add(image);
            var caption = new Label { Text = $"{PhotoSource.Caption(photoPath, "RACE_FINISHED")} · {PhotoSource.View(photoPath)}", Left = 480, Top = 603, Width = 190, Height = 30, ForeColor = Color.FromArgb(245, 190, 65), AutoEllipsis = true, Font = new Font("Segoe UI", 8) };
            Controls.Add(caption);
        }
        var listenButton = new Button { Text = "Ascolta il servizio", Left = 690, Top = 505, Width = 150, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = listen != null };
        listenButton.Click += (_, _) => listen?.Invoke(); Controls.Add(listenButton);
        var mediaButton = new Button { Text = "Apri Media Center", Left = 690, Top = 548, Width = 150, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = openMedia != null };
        mediaButton.Click += (_, _) => openMedia?.Invoke(); Controls.Add(mediaButton);
        var close = new Button { Text = "Chiudi", Left = 850, Top = 548, Width = 100, Height = 34, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(70, 75, 85), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        Controls.Add(close); AcceptButton = close;
    }

    private static string FormatLap(int ms) => $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}";
    private static string FormatGap(int ms) => ms < 60000 ? $"{ms / 1000.0:0.000}s" : $"{ms / 60000:0}m {ms / 1000 % 60:00}s";
}
