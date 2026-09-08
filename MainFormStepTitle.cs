using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// L'ingresso in scena del titolo del passo attuale.
///
/// «Finalmente si comincia» è la frase che dice al giocatore dove si trova nella
/// storia, e compariva insieme a tutto il resto della scheda: si perdeva fra le
/// righe che la circondano.
///
/// Qui entra una parola alla volta, con un filo d'accento che si allunga sotto.
/// Non è una scrittura a macchina lettera per lettera — quella si legge più in
/// fretta di quanto venga scritta e diventa un'attesa: le parole arrivano già
/// intere, in mezzo secondo.
/// </summary>
public sealed partial class MainForm
{
    private System.Windows.Forms.Timer? stepTitleTimer;

    /// <summary>Fotogrammi fra una parola e la successiva.</summary>
    private const int WordFrames = 3;

    /// <summary>Durata del filo che si allunga, in fotogrammi.</summary>
    private const int UnderlineFrames = 16;

    /// <summary>
    /// L'ultimo titolo animato. Un aggiornamento della schermata che non cambia
    /// il passo non deve rianimare la stessa frase: la ripetizione la
    /// trasformerebbe da scena in disturbo.
    /// </summary>
    private string lastAnimatedTitle = "";

    private void AnimateStepTitle(Label target, string title)
    {
        StopStepTitleAnimation();

        if (!CareerTransitions.PulsesEnabled || string.IsNullOrWhiteSpace(title))
        {
            target.Text = title;
            return;
        }

        // Stessa frase di prima: resta dov'è, senza rientrare in scena.
        if (string.Equals(title, lastAnimatedTitle, StringComparison.Ordinal))
        {
            target.Text = title;
            return;
        }
        lastAnimatedTitle = title;

        var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) { target.Text = title; return; }

        var shown = 0;
        var frame = 0;
        target.Text = "";

        // Il filo d'accento si disegna sul fondo dell'etichetta e si allunga
        // mentre le parole entrano.
        var underline = 0.0;
        target.Paint += (_, e) =>
        {
            if (underline <= 0.001) return;
            var width = (int)(target.Width * Math.Min(1.0, underline));
            using var brush = new SolidBrush(UiTheme.Accent);
            e.Graphics.FillRectangle(brush, 0, target.Height - 3, width, 2);
        };

        stepTitleTimer = new System.Windows.Forms.Timer { Interval = CareerTransitions.FrameMilliseconds };
        stepTitleTimer.Tick += (_, _) =>
        {
            // L'etichetta può essere stata smaltita da un aggiornamento della
            // schermata mentre l'animazione era in corso.
            if (target.IsDisposed)
            {
                StopStepTitleAnimation();
                return;
            }

            frame++;
            if (frame % WordFrames == 0 && shown < words.Length)
            {
                shown++;
                target.Text = string.Join(' ', words.Take(shown));
            }

            underline = CareerTransitions.EaseOut(frame / (double)UnderlineFrames);
            target.Invalidate();

            if (shown >= words.Length && frame >= UnderlineFrames)
            {
                target.Text = title;
                underline = 1.0;
                target.Invalidate();
                StopStepTitleAnimation();
            }
        };
        stepTitleTimer.Start();
    }

    private void StopStepTitleAnimation()
    {
        if (stepTitleTimer == null) return;
        stepTitleTimer.Stop();
        stepTitleTimer.Dispose();
        stepTitleTimer = null;
    }
}
