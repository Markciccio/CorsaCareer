using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Il dettaglio tecnico e il dossier del paddock.
///
/// Stava nella colonna di destra della home, dove occupava lo spazio che serviva
/// al calendario e all'ascesa. Sono dati che si consultano quando servono, non
/// che si leggono a ogni apertura: qui restano interi e raggiungibili.
/// </summary>
public sealed class TechnicalDossierDialog : CareerDialog
{
    public TechnicalDossierDialog(string nextSession, string standings, string dossier, string lastWeekend)
    {
        Text = "CorsaCareer — dossier tecnico";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(940, 720);
        MinimumSize = new Size(700, 520);
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
            BackColor = UiTheme.Background, Padding = new Padding(16)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 58));

        grid.Controls.Add(Riquadro("prossima sessione", nextSession, UiTheme.Warning, false), 0, 0);
        grid.Controls.Add(Riquadro("classifica campionato", standings, UiTheme.Positive, true), 1, 0);
        grid.Controls.Add(Riquadro("dossier del paddock", dossier, UiTheme.Info, false), 0, 1);
        grid.Controls.Add(Riquadro("ultimo weekend · dati tecnici", lastWeekend, UiTheme.Accent, true), 1, 1);

        var close = UiTheme.SecondaryButton("Chiudi");
        close.Dock = DockStyle.None;
        close.Click += (_, _) => Close();
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = UiTheme.Background, Padding = new Padding(16, 8, 16, 8) };
        close.Location = new Point(footer.Width - 160, 8);
        close.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        footer.Controls.Add(close);

        Controls.Add(footer);
        Controls.Add(grid);
        CancelButton = close;
    }

    private static Control Riquadro(string label, string text, Color accent, bool monospace)
    {
        var card = UiTheme.Card(label, out var content, accent);
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, UiTheme.Gutter, UiTheme.Gutter);
        var block = UiTheme.TextBlock(monospace: monospace);
        block.Text = string.IsNullOrWhiteSpace(text) ? "Nessun dato disponibile." : text;
        content.Controls.Add(block);
        return card;
    }
}
