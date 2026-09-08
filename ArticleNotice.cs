using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// L'avviso che segnala un articolo nuovo.
///
/// Prima ogni evento della carriera apriva il browser da solo: una prova, una
/// gara, una firma — una scheda ciascuna, senza chiedere niente a nessuno. In
/// una sessione un po' lunga il browser si riempiva di decine di schede, e
/// l'articolo — che dovrebbe essere il premio del momento — diventava
/// un'interruzione da chiudere.
///
/// Poi è stato un nastro rosso a tutta larghezza ancorato in cima alla
/// finestra: impossibile da ignorare, copriva la testata e spingeva giù il
/// resto della schermata. Adesso è un riquadro piccolo dentro la colonna
/// «OGGI», accanto a ciò che racconta. Chi vuole leggere clicca; chi sta
/// giocando prosegue, e alla prima azione successiva l'avviso sparisce da solo.
/// </summary>
public sealed class ArticleNotice : Panel
{
    private readonly Label kicker = new();
    private readonly Label headline = new();
    private readonly System.Windows.Forms.Timer blink = new() { Interval = 520 };
    private int pulses;
    private bool bright = true;

    /// <summary>Quanti lampeggi prima di restare acceso fisso.</summary>
    private const int PulseCount = 6;

    private static readonly Color Acceso = Color.FromArgb(44, 18, 26);
    private static readonly Color Spento = Color.FromArgb(30, 34, 43);

    /// <summary>Cosa aprire quando l'avviso viene cliccato.</summary>
    private Action? open;

    public ArticleNotice()
    {
        // Non più Dock.Top a tutta finestra: vive nel flusso della colonna, con
        // la larghezza che gli assegna chi lo inserisce.
        Height = 66;
        Cursor = Cursors.Hand;
        Padding = new Padding(16, 8, 12, 8);
        BackColor = Acceso;
        Margin = new Padding(0, 0, 0, 10);

        kicker = new Label
        {
            Dock = DockStyle.Top,
            Height = 18,
            Font = UiTheme.Kicker,
            ForeColor = UiTheme.Accent,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "NUOVO ARTICOLO  ·  CLICCA PER LEGGERLO",
            UseMnemonic = false,
            Cursor = Cursors.Hand
        };
        headline = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font(UiTheme.FamilySemibold, 9.5f, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true,
            UseMnemonic = false,
            Cursor = Cursors.Hand
        };

        Controls.Add(headline);
        Controls.Add(kicker);

        foreach (Control c in new Control[] { this, headline, kicker })
            c.Click += (_, _) => Open();

        blink.Tick += (_, _) =>
        {
            bright = !bright;
            BackColor = bright ? Acceso : Spento;
            if (!bright && ++pulses >= PulseCount)
            {
                blink.Stop();
                BackColor = Acceso;
            }
        };
    }

    /// <summary>
    /// Annuncia un articolo. L'azione viene eseguita solo se il giocatore
    /// clicca: da qui non si apre niente da solo.
    /// </summary>
    public void Announce(string title, Action openArticle)
    {
        if (CareerMessages.Unattended) return;
        open = openArticle;
        headline.Text = string.IsNullOrWhiteSpace(title) ? "Nuovo articolo dal paddock" : title;
        pulses = 0;
        bright = true;
        BackColor = Acceso;
        Visible = true;
        blink.Start();
    }

    private void Open()
    {
        var action = open;
        blink.Stop();
        open = null;
        action?.Invoke();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Una barra chiara sul bordo sinistro: la stessa marca che l'edizione
        // digitale usa sui suoi articoli.
        using var brush = new SolidBrush(UiTheme.Accent);
        e.Graphics.FillRectangle(brush, 0, 0, 4, Height);
        using var pen = new Pen(Color.FromArgb(90, UiTheme.Accent));
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) blink.Dispose();
        base.Dispose(disposing);
    }
}
