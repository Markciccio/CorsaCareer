using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// La decisione davanti a una gara: correre o saltare.
///
/// Avere i soldi non basta a rendere ovvia la scelta. Con 650 € in cassa, una
/// gara da 250 € oggi e un test da 500 € fra cinque giorni, correre oggi
/// significa rinunciare al test — e questa schermata lo dice prima, invece di
/// lasciarlo scoprire dopo.
///
/// Quando i soldi non ci sono, correre non è possibile: il pulsante non esiste,
/// e restano le alternative.
/// </summary>
public sealed class RaceChoiceDialog : CareerDialog
{
    /// <summary>Cosa ha deciso il giocatore.</summary>
    public RaceDecision Decision { get; private set; } = RaceDecision.Postpone;

    public RaceChoiceDialog(CareerState career, ScheduledEvent race, int entryFee)
    {
        Text = "CorsaCareer — corri o salti?";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var affordable = race.EntryFeePaid || entryFee <= career.Cash;

        Controls.Add(BuildBody(career, race, entryFee, affordable));
        Controls.Add(BuildFooter(career, race, entryFee, affordable));
        Controls.Add(BuildHeader(race, affordable));
    }

    private static Control BuildHeader(ScheduledEvent race, bool affordable)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 16, 40, 12) };
        panel.Controls.Add(new Label
        {
            Text = affordable ? "IL GIORNO DELLA GARA" : "NON TE LA PUOI PERMETTERE",
            Dock = DockStyle.Top, Height = 20, Font = UiTheme.Kicker,
            ForeColor = affordable ? UiTheme.Warning : UiTheme.Accent, UseMnemonic = false
        });
        var title = new Label
        {
            Text = $"{race.TrackName} · {NarrativeCalendar.Format(race.Date)}",
            Dock = DockStyle.Top, Height = 34, Font = UiTheme.HeadlineSmall,
            ForeColor = UiTheme.TextPrimary, UseMnemonic = false
        };
        panel.Controls.Add(title);
        panel.Controls.SetChildIndex(title, 1);
        return panel;
    }

    private static Control BuildBody(CareerState career, ScheduledEvent race, int entryFee, bool affordable)
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = UiTheme.Background, Padding = new Padding(40, 20, 30, 16)
        };
        var width = 900;

        if (!string.IsNullOrWhiteSpace(race.ProposedBy))
            flow.Controls.Add(Line($"{race.ProposedBy} tiene il posto in griglia.",
                UiTheme.Standfirst, UiTheme.TextSecondary, width, 0, 12));

        flow.Controls.Add(Line(
            $"Iscrizione € {entryFee:N0}   ·   in cassa € {career.Cash:N0}",
            UiTheme.BodyStrong, affordable ? UiTheme.TextPrimary : UiTheme.Accent, width, 0, 14));

        var baseFee = race.EntryFee > 0 ? race.EntryFee : entryFee;
        var includedTravel = Math.Max(0, entryFee - baseFee);
        flow.Controls.Add(Line("BILANCIO PRIMA DELLA PARTENZA", UiTheme.Kicker, UiTheme.Warning, width, 0, 6));
        flow.Controls.Add(Line($"Quota gara: € {baseFee:N0}  ·  trasferta e organizzazione: € {includedTravel:N0}  ·  costo certo totale: € {entryFee:N0}", UiTheme.Prose, UiTheme.TextSecondary, width, 0, 4));
        flow.Controls.Add(Line("Il premio e gli eventuali danni dipendono dal risultato reale: non sono inclusi nel costo certo.", UiTheme.Small, UiTheme.TextMuted, width, 0, 12));

        if (!affordable)
        {
            var missing = entryFee - career.Cash;
            flow.Controls.Add(Line($"Mancano € {missing:N0}. Il conto andrebbe in rosso, e non è previsto.",
                UiTheme.Prose, UiTheme.Accent, width, 0, 14));
            flow.Controls.Add(Line("COSA SI PUÒ FARE", UiTheme.Kicker, UiTheme.Warning, width, 0, 6));
            foreach (var option in new[]
                     {
                         "Mandare Haru a cercare uno sponsor: una trattativa può coprire la quota.",
                         "Guadagnare qualcosa con le attività del pilota, e riprovare fra qualche giorno.",
                         "Saltare questa gara e aspettare un'occasione meno cara."
                     })
                flow.Controls.Add(Line("·  " + option, UiTheme.Prose, UiTheme.TextSecondary, width, 0, 4));
        }
        else
        {
            // Il ragionamento che rende la scelta interessante: cosa resta dopo,
            // e quali impegni quella spesa metterebbe a rischio.
            flow.Controls.Add(Line("PRIMA DI DECIDERE", UiTheme.Kicker, UiTheme.Info, width, 0, 6));
            foreach (var note in RaceChoice.Considerations(career, race, entryFee))
                flow.Controls.Add(Line("·  " + note, UiTheme.Prose, UiTheme.TextSecondary, width, 0, 4));

            flow.Controls.Add(Line("SE SALTI", UiTheme.Kicker, UiTheme.Warning, width, 14, 6));
            var cost = RaceChoice.CostOfSkipping(career, race);
            flow.Controls.Add(Line(cost.Line, UiTheme.Prose, UiTheme.TextSecondary, width, 0, 4));
            flow.Controls.Add(Line("I soldi restano, ma il costo è un altro: " + string.Join("  ·  ", cost.Details),
                UiTheme.Small, UiTheme.TextMuted, width, 0, 0));
        }

        return flow;
    }

    private Control BuildFooter(CareerState career, ScheduledEvent race, int entryFee, bool affordable)
    {
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 84, BackColor = UiTheme.HeaderBackground, Padding = new Padding(40, 16, 40, 20) };

        var skip = UiTheme.SecondaryButton("SALTA QUESTA GARA");
        skip.Dock = DockStyle.Right;
        skip.Width = 280;
        skip.ForeColor = UiTheme.Warning;
        skip.Click += (_, _) => { Decision = RaceDecision.Skip; DialogResult = DialogResult.OK; Close(); };

        var back = UiTheme.SecondaryButton("DECIDO DOPO");
        back.Dock = DockStyle.Right;
        back.Width = 200;
        back.Margin = new Padding(0, 0, 12, 0);
        back.Click += (_, _) => { Decision = RaceDecision.Postpone; DialogResult = DialogResult.Cancel; Close(); };

        panel.Controls.Add(back);
        panel.Controls.Add(skip);

        // Correre è possibile solo se la quota è sostenibile: un pulsante che
        // manda il conto in rosso non deve esistere.
        if (affordable)
        {
            var race_ = UiTheme.PrimaryButton($"CORRI · € {entryFee:N0}");
            race_.Dock = DockStyle.Right;
            race_.Width = 300;
            race_.Margin = new Padding(0, 0, 12, 0);
            race_.Click += (_, _) => { Decision = RaceDecision.Race; DialogResult = DialogResult.OK; Close(); };
            panel.Controls.Add(race_);
        }

        CancelButton = back;
        return panel;
    }

    private static Label Line(string text, Font font, Color color, int width, int above, int below)
    {
        var label = new Label
        {
            Text = text, Font = font, ForeColor = color, AutoSize = false, Width = width,
            BackColor = Color.Transparent, UseMnemonic = false, Margin = new Padding(0, above, 0, below)
        };
        using var g = label.CreateGraphics();
        label.Height = (int)Math.Ceiling(g.MeasureString(text, font, width).Height) + 2;
        return label;
    }
}
