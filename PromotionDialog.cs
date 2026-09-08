using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// Bivio di fine stagione. Prima il passaggio di categoria assegnava d'ufficio
/// squadra, vettura e compagno: qui la decisione è del giocatore e la pagella
/// che la motiva è visibile voce per voce.
/// </summary>
public sealed class PromotionDialog : CareerDialog
{
    public TierChoice? Selected { get; private set; }

    public PromotionDialog(CareerState career, PromotionAssessment assessment, IReadOnlyList<TierChoice> choices)
    {
        Text = "CorsaCareer — decisione di fine stagione";
        ClientSize = new Size(880, 620);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(18, 21, 28);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        var top = new Panel { Dock = DockStyle.Top, Height = 108, BackColor = Color.FromArgb(27, 31, 41) };
        top.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(220, 25, 45), 4); e.Graphics.DrawLine(pen, 0, 0, top.Width, 0); };
        top.Controls.Add(new Label { Text = "FINE STAGIONE", Left = 30, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 22, FontStyle.Bold) });
        top.Controls.Add(new Label { Text = $"Stagione {career.Season} · {career.Championship} · {career.Tier}", Left = 33, Top = 64, AutoSize = true, ForeColor = Color.FromArgb(190, 196, 207) });
        Controls.Add(top);

        var verdict = new Label
        {
            Text = assessment.Verdict, Left = 30, Top = 124, Width = 820, Height = 44,
            ForeColor = assessment.Ready ? Color.FromArgb(120, 220, 140) : Color.FromArgb(245, 190, 65),
            Font = new Font("Segoe UI Semibold", 11)
        };
        Controls.Add(verdict);

        var reasons = new TextBox
        {
            Left = 30, Top = 172, Width = 820, Height = 138, Multiline = true, ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(27, 31, 41), ForeColor = Color.Gainsboro,
            ScrollBars = ScrollBars.Vertical,
            Text = "PAGELLA DELLA STAGIONE (solo dati reali della carriera)\r\n" + assessment.Explain().Replace("\n", "\r\n")
        };
        Controls.Add(reasons);

        Controls.Add(new Label { Text = "LA TUA DECISIONE", Left = 30, Top = 322, AutoSize = true, ForeColor = Color.FromArgb(220, 25, 45), Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold) });

        var top1 = 350;
        foreach (var choice in choices)
        {
            var card = new Panel { Left = 30, Top = top1, Width = 820, Height = 100, BackColor = Color.FromArgb(27, 31, 41), BorderStyle = BorderStyle.FixedSingle };
            var team = string.IsNullOrWhiteSpace(choice.Team) ? "squadra da definire alla firma" : choice.Team;
            card.Controls.Add(new Label { Text = $"{choice.Tier} · {team} · {choice.Role}", Left = 16, Top = 12, AutoSize = true, Font = new Font("Segoe UI Semibold", 11), ForeColor = Color.White });
            card.Controls.Add(new Label { Text = choice.Summary, Left = 16, Top = 38, Width = 600, Height = 36, ForeColor = Color.FromArgb(190, 196, 207) });
            card.Controls.Add(new Label { Text = $"Obiettivo: {choice.Objective} · prestigio {choice.Prestige}/100 · stipendio € {choice.Salary:N0}", Left = 16, Top = 74, AutoSize = true, ForeColor = Color.FromArgb(150, 158, 172), Font = new Font("Segoe UI", 8) });
            var pick = new Button
            {
                Text = "SCEGLI", Left = 660, Top = 32, Width = 140, Height = 38, FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(190, 22, 55), ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 10),
                DialogResult = DialogResult.OK
            };
            var captured = choice;
            pick.Click += (_, _) => { Selected = captured; };
            card.Controls.Add(pick);
            Controls.Add(card);
            top1 += 110;
        }

        var note = new Label
        {
            Text = "La categoria proposta esiste solo se fra i contenuti installati c'è una vettura adatta. Nessun risultato viene modificato da questa scelta.",
            Left = 30, Top = Math.Min(top1 + 6, 566), Width = 820, Height = 34, ForeColor = Color.FromArgb(150, 158, 172), Font = new Font("Segoe UI", 8)
        };
        Controls.Add(note);
    }
}
