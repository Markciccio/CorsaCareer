using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Il bilancio di fine anno.
///
/// Prima erano quattro numeri — punti, vittorie, premio, budget — e non
/// raccontavano una stagione: un anno con dieci secondi posti e uno con dieci
/// ritiri e una vittoria potevano chiudersi con lo stesso punteggio. Adesso c'e'
/// l'annuario vero: pole, podi, arrivi a punti, rimonte, ritiri, il testa a
/// testa con il compagno, il bilancio economico, e il confronto con l'anno
/// prima — che e' il modo in cui una stagione si legge davvero.
/// </summary>
public sealed class SeasonReportDialog : CareerDialog
{
    public SeasonReportDialog(CareerState career, SeasonSummary summary, Action? listen = null, Action? openMedia = null)
    {
        Text = "CorsaCareer — bilancio di fine stagione";
        ClientSize = new Size(1000, 660);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        var statistiche = SeasonStatistics.Compute(career, summary.Season);
        var precedente = summary.Season > 1 ? SeasonStatistics.Compute(career, summary.Season - 1) : null;

        var titolo = summary.WorldTitle ? "CAMPIONE DEL MONDO"
            : summary.TitleWon ? "CAMPIONE"
            : "BILANCIO DI FINE STAGIONE";
        Controls.Add(new Label
        {
            Text = titolo, Left = 24, Top = 16, AutoSize = true,
            Font = new Font("Segoe UI", 21, FontStyle.Bold),
            ForeColor = summary.TitleWon ? Color.FromArgb(245, 190, 65) : Color.FromArgb(230, 232, 238)
        });
        Controls.Add(new Label
        {
            Text = $"Stagione {summary.Season}  ·  {summary.Championship}  ·  {statistiche.Categoria}  ·  chiusa in P{summary.FinalPosition}",
            Left = 26, Top = 52, AutoSize = true, ForeColor = Color.Gainsboro
        });

        // --- l'annuario, a due colonne per sezione
        var annuario = new RichTextBox
        {
            Left = 24, Top = 88, Width = 470, Height = 500, ReadOnly = true,
            BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(35, 40, 52),
            ForeColor = Color.White, Font = new Font("Consolas", 9.5f)
        };
        var testo = new System.Text.StringBuilder();
        var sezioneCorrente = "";
        foreach (var (sezione, voce, valore) in statistiche.Righe())
        {
            if (sezione != sezioneCorrente)
            {
                if (sezioneCorrente.Length > 0) testo.AppendLine();
                testo.AppendLine(sezione);
                testo.AppendLine(new string('─', 52));
                sezioneCorrente = sezione;
            }
            // Il valore allineato a destra: un annuario si legge in colonna.
            var etichetta = voce.Length > 34 ? voce[..34] : voce;
            testo.AppendLine(etichetta.PadRight(34) + valore);
        }
        annuario.Text = testo.ToString();
        Controls.Add(annuario);

        // --- il confronto con l'anno prima e la classifica finale
        Controls.Add(new Label
        {
            Text = precedente == null || precedente.Gare == 0 ? "LA PRIMA STAGIONE" : "RISPETTO ALL'ANNO SCORSO",
            Left = 516, Top = 66, AutoSize = true, ForeColor = Color.Gainsboro
        });
        var confronto = new RichTextBox
        {
            Left = 514, Top = 88, Width = 460, Height = 150, ReadOnly = true,
            BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White
        };
        var righeConfronto = statistiche.ConfrontoCon(precedente);
        confronto.Text = righeConfronto.Count > 0
            ? string.Join("\n", righeConfronto)
            : $"È il primo anno di {career.Driver}: da qui in poi ogni stagione avrà un termine di paragone.";
        Controls.Add(confronto);

        Controls.Add(new Label { Text = "CLASSIFICA FINALE", Left = 516, Top = 252, AutoSize = true, ForeColor = Color.Gainsboro });
        var classifica = new ListBox
        {
            Left = 514, Top = 274, Width = 460, Height = 314,
            BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White, BorderStyle = BorderStyle.None
        };
        foreach (var riga in career.Standings
                     .OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins)
                     .Select((x, i) => $"P{i + 1,2}  {x.Driver,-24} {x.Points,3} pt  ·  {x.Wins} vittorie"))
            classifica.Items.Add(riga);
        if (classifica.Items.Count == 0) classifica.Items.Add("Classifica non disponibile per questa stagione.");
        Controls.Add(classifica);

        var listenButton = new Button { Text = "Ascolta il bilancio", Left = 514, Top = 604, Width = 160, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = listen != null };
        listenButton.Click += (_, _) => listen?.Invoke(); Controls.Add(listenButton);
        var mediaButton = new Button { Text = "Apri Media Center", Left = 684, Top = 604, Width = 160, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = openMedia != null };
        mediaButton.Click += (_, _) => openMedia?.Invoke(); Controls.Add(mediaButton);
        var close = new Button { Text = "Chiudi", Left = 862, Top = 604, Width = 112, Height = 34, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(70, 75, 85), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        Controls.Add(close); AcceptButton = close;
    }
}
