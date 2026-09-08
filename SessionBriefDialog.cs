using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// Il foglio del weekend: le condizioni della sessione, prima di scendere in
/// pista.
///
/// Era una finestrella di sistema con dentro un blocco di testo — «Gara sprint ·
/// 17 giri · 19,6 km · Meteo: sereno · 30 °C · ore 12:00 · Prove 12' ...» — e
/// l'identificativo tecnico della vettura e del circuito, «kart_60_mini» e
/// «kart_narashino». Comunicava tutto e non si leggeva niente: era una scatola
/// di Windows in mezzo a una schermata di gioco.
///
/// Qui le stesse informazioni sono un foglio da paddock: il circuito e la
/// vettura con i loro nomi veri, la distanza, il meteo, il programma della
/// giornata e il livello degli avversari, ognuna al suo posto.
/// </summary>
public sealed class SessionBriefDialog : CareerDialog
{
    private Bitmap? tavola;

    public SessionBriefDialog(string titolo, string sottotitolo, SessionPlan plan,
        string vettura, string circuito, int avversari, string immagine, string pulsante)
    {
        Text = "CorsaCareer — il weekend";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var header = new Panel { Dock = DockStyle.Top, Height = 116, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 18, 40, 14) };
        header.Controls.Add(new Label
        {
            Text = "IL FOGLIO DEL WEEKEND", Dock = DockStyle.Top, Height = 24,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Accent
        });
        header.Controls.Add(new Label
        {
            Text = titolo.ToUpperInvariant(), Dock = DockStyle.Top, Height = 44,
            Font = UiTheme.Headline, ForeColor = UiTheme.Warning, AutoEllipsis = true
        });
        header.Controls.Add(new Label
        {
            Text = sottotitolo, Dock = DockStyle.Bottom, Height = 30,
            Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary
        });

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
            Padding = new Padding(34, 20, 34, 16), BackColor = UiTheme.Background
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));

        var fotoCard = UiTheme.Card("DOVE SI CORRE", out var fotoInner, UiTheme.Info);
        fotoCard.Dock = DockStyle.Fill;
        var foto = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Zoom };
        tavola = Carica(immagine);
        foto.Image = tavola;
        fotoInner.Controls.Add(foto);
        grid.Controls.Add(fotoCard, 0, 0);

        var datiCard = UiTheme.Card("CONDIZIONI DELLA SESSIONE", out var datiInner, UiTheme.Warning);
        datiCard.Dock = DockStyle.Fill;
        datiInner.Controls.Add(Tabella(plan, vettura, circuito, avversari));
        grid.Controls.Add(datiCard, 1, 0);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 86, BackColor = UiTheme.HeaderBackground, Padding = new Padding(34, 16, 34, 16) };
        var vai = UiTheme.PrimaryButton(pulsante);
        vai.Dock = DockStyle.Right; vai.Width = 300; vai.DialogResult = DialogResult.Yes;
        var annulla = UiTheme.SecondaryButton("NON ORA");
        annulla.Dock = DockStyle.Right; annulla.Width = 150; annulla.Height = 44;
        annulla.Margin = new Padding(0, 0, 12, 0);
        annulla.Click += (_, _) => { DialogResult = DialogResult.No; };
        footer.Controls.Add(vai);
        footer.Controls.Add(annulla);
        footer.Controls.Add(new Label
        {
            Text = plan.Notes.Count > 0 ? string.Join("  ·  ", plan.Notes) : "Le condizioni sono già scritte nel preset: in pista trovi esattamente questo.",
            Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        });
        AcceptButton = vai;

        Controls.Add(grid); Controls.Add(footer); Controls.Add(header);
        Disposed += (_, _) => tavola?.Dispose();
    }

    private static Control Tabella(SessionPlan plan, string vettura, string circuito, int avversari)
    {
        var righe = new List<(string, string)>
        {
            ("CIRCUITO", circuito),
            ("VETTURA", vettura),
            ("FORMATO", plan.FormatLabel),
            ("DISTANZA", plan.DistanceLabel),
            ("METEO", $"{plan.WeatherLabel} · {plan.TemperatureC:0} °C · ore {plan.TimeOfDayLabel}"),
            ("PROGRAMMA", $"prove {plan.PracticeMinutes}′ · qualifica {plan.QualifyingMinutes}′"),
            ("AVVERSARI", avversari > 0
                ? $"{avversari} in griglia · livello {plan.AiLevel:0}% · aggressività {plan.AiAggression:0}%"
                : $"nessun avversario: si gira da soli")
        };

        var tabella = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = righe.Count,
            BackColor = UiTheme.Surface, Padding = new Padding(6, 8, 6, 8)
        };
        tabella.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        tabella.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < righe.Count; i++)
        {
            tabella.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / righe.Count));
            tabella.Controls.Add(new Label
            {
                Text = righe[i].Item1, Dock = DockStyle.Fill, Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft
            }, 0, i);
            tabella.Controls.Add(new Label
            {
                Text = righe[i].Item2, Dock = DockStyle.Fill, Font = UiTheme.BodyStrong,
                ForeColor = UiTheme.TextPrimary, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
            }, 1, i);
        }
        return tabella;
    }

    private static Bitmap? Carica(string immagine)
    {
        if (string.IsNullOrWhiteSpace(immagine)) return null;
        var percorso = AssetPaths.File(immagine);
        if (!File.Exists(percorso)) return null;
        try { using var source = Image.FromFile(percorso); return new Bitmap(source); }
        catch { return null; }
    }
}
