using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class PaddockDialog : CareerDialog
{
    public PaddockDialog(CareerState career)
    {
        Text = "CorsaCareer — Paddock"; ClientSize = new Size(920, 580); StartPosition = FormStartPosition.CenterParent; BackColor = UiTheme.Background; ForeColor = UiTheme.TextPrimary; Font = UiTheme.Body;
        Controls.Add(new Label { Text = "PADDOCK · PERSONE", Left = 32, Top = 20, AutoSize = true, Font = UiTheme.Headline, ForeColor = UiTheme.Warning });
        Controls.Add(new Label { Text = $"{career.Team}  ·  {UiText.Car(career.Car)}  ·  compagno: {career.Teammate}  ·  sponsor: {career.Sponsor}", Left = 34, Top = 58, AutoSize = true, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary });
        var timeline = UiTheme.PrimaryButton("TIMELINE"); timeline.Dock = DockStyle.None; timeline.Left = 730; timeline.Top = 18; timeline.Width = 160; timeline.Height = 34;
        timeline.Click += (_, _) => { using var dialog = new TimelineDialog(career); dialog.ShowDialog(this); }; Controls.Add(timeline);
        var standings = UiTheme.PrimaryButton("CLASSIFICHE"); standings.Dock = DockStyle.None; standings.Left = 550; standings.Top = 18; standings.Width = 160; standings.Height = 34;
        standings.Click += (_, _) => { using var dialog = new StandingsDialog(career); dialog.ShowDialog(this); }; Controls.Add(standings);
        var visibleDrivers = career.Drivers.OrderByDescending(x => x.Points).ThenByDescending(x => x.Skill).ToList();
        var drivers = new ListBox { Left = 24, Top = 95, Width = 520, Height = 440, BackColor = UiTheme.SurfaceRaised, ForeColor = UiTheme.TextPrimary, BorderStyle = BorderStyle.None, Font = UiTheme.Body, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 62, IntegralHeight = false };
        foreach (var driver in visibleDrivers) drivers.Items.Add($"{driver.Name}  |  {driver.Team}  |  {driver.Points} pt  |  skill {driver.Skill}  |  gare {driver.Races}");
        if (drivers.Items.Count == 0) drivers.Items.Add("Nessun pilota registrato: completa una gara reale.");
        Controls.Add(drivers);
        var driverDetail = new Label { Left = 580, Top = 95, Width = 300, Height = 78, ForeColor = UiTheme.Warning, Font = UiTheme.BodyStrong, AutoEllipsis = true };
        Controls.Add(driverDetail);
        var right = new RichTextBox { Left = 580, Top = 185, Width = 300, Height = 350, ForeColor = UiTheme.TextPrimary, BackColor = UiTheme.Surface, BorderStyle = BorderStyle.None, Font = UiTheme.Body, ReadOnly = true, ScrollBars = RichTextBoxScrollBars.Vertical, Padding = new Padding(10) };
        var rivalries = career.Rivalries.OrderByDescending(x => x.Level).Take(8).Select(x => $"{x.Rival}: livello {x.Level}/100\nDuelli {x.Duels}  |  {career.Driver} {x.PlayerWins}-{x.RivalWins} {x.Rival}\n{x.Story}").ToList();
        var seasons = career.SeasonArchive.OrderByDescending(x => x.Season).Take(8).Select(x => $"S{x.Season} {x.Championship}: P{x.FinalPosition} — {x.Points} pt, {x.Wins} vittorie, premio € {x.Award:N0}").ToList();
        var arcs = career.StoryArcs.OrderByDescending(x => x.Importance).Take(4).Select(x => $"{x.Title} — {x.Status}\n{x.Summary}").ToList();
        var teams = career.TeamHistory.OrderByDescending(x => x.StoryDate).Take(8).Select(x => $"{x.StoryDate:dd/MM/yyyy}  {x.FromTeam} → {x.ToTeam}\n{x.Category} · {UiText.Car(x.Car)}\nMotivo: {x.Reason}").ToList();
        var cast = (career.StoryCast ?? []).Select(x =>
            $"{x.Name.ToUpperInvariant()} · {x.Role}\nFiducia {x.Trust}/100 · {x.Relationship}\n{(x.LastReactionDate == default ? "Nessuna scena ancora registrata." : $"Ultima reazione · {x.LastReactionDate:dd/MM/yyyy}: {x.LastReaction}")}\nMemoria: {(x.Memory == null || x.Memory.Count == 0 ? "nessun appunto" : x.Memory[0])}").ToList();
        var paddockBrief = "PERSONE CHIAVE · MEMORIA DEL PADDOCK\n\n" + (cast.Count == 0 ? "Il cast nascerà dal primo capitolo." : string.Join("\n\n", cast)) + "\n\nARCHI NARRATIVI\n\n" + (arcs.Count == 0 ? "Gli archi nasceranno dai risultati reali." : string.Join("\n\n", arcs)) + "\n\nRIVALITÀ\n\n" + (rivalries.Count == 0 ? "Nessuna rivalità registrata." : string.Join("\n\n", rivalries)) + "\n\nSTORICO SCUDERIE\n\n" + (teams.Count == 0 ? "Nessun cambio di sedile registrato." : string.Join("\n\n", teams)) + "\n\nCAMPIONATI ARCHIVIATI\n\n" + (seasons.Count == 0 ? "Nessuna stagione conclusa." : string.Join("\n", seasons));
        right.Text = paddockBrief; Controls.Add(right);
        void ShowDriver(PersistentDriver? driver)
        {
            if (driver == null) { driverDetail.Text = "SELEZIONA UN PILOTA\nIl dossier si popola dai risultati realmente importati."; return; }
            driverDetail.Text = $"{driver.Name} · {driver.Nationality}\n{driver.Team}  |  {UiText.Car(driver.Car)}\n{driver.Category} · {driver.Relationship}";
            right.Text = $"DOSSIER PILOTA\n\nSkill {driver.Skill}  |  velocità {driver.Speed}\nCostanza {driver.Consistency}  |  aggressività {driver.Aggression}\nSpecialità: {driver.Specialization}\nEsperienza: {driver.Experience}  |  reputazione: {driver.Reputation}\nGare: {driver.Races}  |  vittorie: {driver.Wins}  |  punti: {driver.Points}\nUltimo risultato: {(driver.LastPosition > 0 ? $"P{driver.LastPosition} · {driver.LastTrack}" : "nessun risultato reale importato")}\n\nIDENTITÀ PERSISTENTE\nTeam, nazionalità e statistiche vengono conservati nel salvataggio e aggiornati solo dai risultati osservati.\n\n{paddockBrief}";
        }
        drivers.DrawItem += (_, e) =>
        {
            if (e.Index < 0 || e.Index >= drivers.Items.Count) return;
            e.DrawBackground();
            var selected = (e.State & DrawItemState.Selected) != 0;
            using var brush = new SolidBrush(selected ? Color.FromArgb(37, 85, 128) : UiTheme.SurfaceRaised);
            e.Graphics.FillRectangle(brush, e.Bounds);
            if (e.Index < visibleDrivers.Count)
            {
                var d = visibleDrivers[e.Index];
                TextRenderer.DrawText(e.Graphics, d.Name, UiTheme.BodyStrong, new Rectangle(e.Bounds.Left + 14, e.Bounds.Top + 9, e.Bounds.Width - 28, 22), UiTheme.TextPrimary);
                TextRenderer.DrawText(e.Graphics, $"{d.Team}  ·  {d.Category}  ·  {d.Points} pt  ·  {d.Races} gare", UiTheme.Small, new Rectangle(e.Bounds.Left + 14, e.Bounds.Top + 34, e.Bounds.Width - 28, 18), UiTheme.TextSecondary);
            }
            else TextRenderer.DrawText(e.Graphics, drivers.Items[e.Index]?.ToString(), UiTheme.Body, e.Bounds, UiTheme.TextSecondary);
            using var pen = new Pen(selected ? UiTheme.Info : UiTheme.Border); e.Graphics.DrawRectangle(pen, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
        };
        drivers.SelectedIndexChanged += (_, _) => ShowDriver(drivers.SelectedIndex >= 0 && drivers.SelectedIndex < visibleDrivers.Count ? visibleDrivers[drivers.SelectedIndex] : null);
        void LayoutPaddock()
        {
            var width = Math.Max(760, ClientSize.Width);
            var height = Math.Max(520, ClientSize.Height);
            var gap = 36;
            var leftWidth = Math.Max(420, (width - 72) / 2);
            var rightLeft = 24 + leftWidth + gap;
            var rightWidth = Math.Max(320, width - rightLeft - 36);
            drivers.SetBounds(24, 95, leftWidth, Math.Max(260, height - 140));
            driverDetail.SetBounds(rightLeft, 95, rightWidth, 86);
            right.SetBounds(rightLeft, 195, rightWidth, Math.Max(220, height - 240));
            standings.SetBounds(Math.Max(24, width - 380), 18, 160, 34);
            timeline.SetBounds(Math.Max(24, width - 200), 18, 160, 34);
        }
        Resize += (_, _) => LayoutPaddock();
        LayoutPaddock();
        ShowDriver(visibleDrivers.FirstOrDefault());
    }
}
