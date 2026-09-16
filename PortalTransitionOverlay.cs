using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Breve stacco tra una scena/commento e il portale aggiornato. Non è una
/// finestra decisionale: dichiara chiaramente che il capitolo è cambiato e
/// lascia di nuovo il controllo al giocatore appena la dissolvenza termina.
/// </summary>
public sealed class PortalTransitionOverlay : Form
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 45 };
    private int ticks;

    public PortalTransitionOverlay(string nextTitle, string nextDetail)
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.FromArgb(3, 5, 8);
        Opacity = 0.97;
        TopMost = true;

        var centre = new Panel
        {
            Size = new Size(760, 230),
            BackColor = UiTheme.SurfaceRaised,
            Padding = new Padding(34, 28, 34, 26)
        };
        centre.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Warning, 2);
            e.Graphics.DrawRectangle(pen, 0, 0, centre.Width - 1, centre.Height - 1);
        };
        centre.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom, Height = 28, Font = UiTheme.Small,
            ForeColor = UiTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft,
            Text = "PORTALE AGGIORNATO · la scheda OGGI contiene il nuovo passo"
        });
        centre.Controls.Add(new Label
        {
            Dock = DockStyle.Fill, Font = new Font(UiTheme.FamilySans, 18F, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary, TextAlign = ContentAlignment.TopLeft,
            Text = nextDetail
        });
        centre.Controls.Add(new Label
        {
            Dock = DockStyle.Top, Height = 48, Font = new Font(UiTheme.FamilySemibold, 20F, FontStyle.Bold),
            ForeColor = UiTheme.Warning, TextAlign = ContentAlignment.TopLeft,
            Text = nextTitle
        });
        Controls.Add(centre);
        Resize += (_, _) => centre.Location = new Point((ClientSize.Width - centre.Width) / 2, (ClientSize.Height - centre.Height) / 2);

        timer.Tick += (_, _) =>
        {
            ticks++;
            // Una breve pausa rende leggibile il nuovo appuntamento, poi la
            // schermata torna al portale con una vera dissolvenza dal nero.
            if (ticks <= 12) return;
            Opacity = Math.Max(0, Opacity - 0.10);
            if (Opacity <= 0.02) Close();
        };
        Shown += (_, _) => timer.Start();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop();
        timer.Dispose();
        base.OnFormClosed(e);
    }
}
