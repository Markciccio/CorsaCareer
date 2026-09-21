using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Base delle schermate di carriera: ogni sezione è una destinazione di gioco,
/// non una finestra di servizio appoggiata sopra al racconto.
///
/// Da qui passa anche il modo in cui una schermata entra ed esce. WinForms non
/// ha transizioni: si ottengono muovendo l'opacità della finestra e disegnando
/// sopra un velo che si dissolve. Stando nella classe base, tutte le schermate
/// della carriera si comportano allo stesso modo senza doverlo ripetere.
///
/// L'ingresso si spegne da solo quando la finestra non è visibile — rendering
/// automatico e test — perché un'animazione che nessuno guarda è solo attesa.
/// </summary>
public abstract class CareerDialog : Form
{
    /// <summary>
    /// Come entra questa schermata. Se non viene impostato, si ricava dal tipo:
    /// una schermata di racconto entra come una scena, una di consultazione come
    /// un pannello.
    /// </summary>
    protected SceneEntrance Entrance { get; set; } = SceneEntrance.Fade;

    private readonly System.Windows.Forms.Timer sceneTimer = new() { Interval = CareerTransitions.FrameMilliseconds };
    private int frame;
    private bool entranceDone;
    private bool closing;
    private int riseOffset;
    private Point restingLocation;

    protected CareerDialog()
    {
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1120, 700);
        DoubleBuffered = true;
        Entrance = CareerTransitions.For(GetType().Name);
    }

    // ------------------------------------------------------------------ uscita rapida

    /// <summary>
    /// Vero se ESC chiude questa schermata. Le scene lo usano gia' per
    /// saltare; una schermata che dovesse pretendere una risposta puo'
    /// rifiutarlo sovrascrivendo questa proprieta'.
    /// </summary>
    protected virtual bool EscChiude => true;

    /// <summary>
    /// ESC chiude la schermata.
    ///
    /// Non lo faceva nessuna delle quarantatre finestre del programma: in
    /// WinForms ESC chiude solo se la finestra dichiara un CancelButton, e
    /// nessuna lo dichiarava. Per uscire bisognava ogni volta trovare il
    /// pulsante giusto, che in qualche schermata sta in fondo a una colonna
    /// scorrevole — e in tre casi (audio, centro carriera, giornale) non c'e'
    /// affatto: restava solo la X di Windows, in un programma che per il resto
    /// non ne ha l'aspetto.
    ///
    /// Sta nella classe base perche' la scorciatoia deve essere la stessa
    /// dappertutto: una schermata dove ESC non funziona e' peggio di nessuna
    /// scorciatoia.
    /// </summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape && EscChiude)
        {
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>
    /// Un pulsante di chiusura in alto a destra, per le schermate che non ne
    /// hanno uno proprio.
    ///
    /// Tre pannelli — audio, centro carriera, giornale — si aprivano a tutto
    /// schermo senza alcun modo dichiarato di tornare indietro: restava la X
    /// di Windows, in un programma che per il resto non ne ha l'aspetto. ESC
    /// adesso funziona dappertutto, ma una scorciatoia invisibile non e' un
    /// modo di uscire: va vista.
    ///
    /// Va chiamato in fondo al costruttore, quando gli altri controlli ci sono
    /// gia': si mette davanti a tutti e si tiene ancorato all'angolo.
    /// </summary>
    protected void AggiungiChiusura(string testo = "CHIUDI  (ESC)")
    {
        var chiudi = new Button
        {
            Text = testo,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Width = 150, Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.Small,
            Cursor = Cursors.Hand
        };
        chiudi.FlatAppearance.BorderColor = UiTheme.Border;
        chiudi.Click += (_, _) => Close();
        Controls.Add(chiudi);
        chiudi.BringToFront();
        // La posizione si fissa quando la finestra ha la misura vera: nel
        // costruttore e' ancora quella di ripiego, e il pulsante finirebbe in
        // mezzo alla schermata.
        void Sistema() => chiudi.Location = new Point(Math.Max(8, ClientSize.Width - chiudi.Width - 24), 16);
        Sistema();
        Shown += (_, _) => { Sistema(); chiudi.BringToFront(); };
        Resize += (_, _) => Sistema();
    }

    // --------------------------------------------------------------- ingresso

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!AnimationsEnabled) return;
        // Si parte quasi trasparenti: l'opacità va impostata prima che la
        // finestra compaia, altrimenti si vede un lampo a piena opacità.
        Opacity = CareerTransitions.MinimumOpacity;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!AnimationsEnabled)
        {
            Opacity = 1.0;
            entranceDone = true;
            return;
        }

        restingLocation = Location;
        frame = 0;
        sceneTimer.Tick += AdvanceEntrance;
        sceneTimer.Start();
    }

    private void AdvanceEntrance(object? sender, EventArgs e)
    {
        frame++;
        Opacity = CareerTransitions.EntranceOpacity(frame);

        // Il movimento di salita si ottiene spostando la finestra: una
        // schermata massimizzata non si può spostare, quindi in quel caso resta
        // la sola dissolvenza.
        if (Entrance == SceneEntrance.Rise && WindowState == FormWindowState.Normal)
        {
            riseOffset = CareerTransitions.RiseOffset(frame, Entrance);
            Location = new Point(restingLocation.X, restingLocation.Y + riseOffset);
        }

        if (Entrance is SceneEntrance.Curtain or SceneEntrance.Flash) Invalidate();

        if (frame >= CareerTransitions.EntranceFrames)
        {
            sceneTimer.Stop();
            sceneTimer.Tick -= AdvanceEntrance;
            Opacity = 1.0;
            if (Entrance == SceneEntrance.Rise && WindowState == FormWindowState.Normal)
                Location = restingLocation;
            entranceDone = true;
            riseOffset = 0;
            Invalidate();
        }
    }

    // ------------------------------------------------- il velo sopra la scena

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (entranceDone || !AnimationsEnabled) return;

        switch (Entrance)
        {
            case SceneEntrance.Curtain:
                DrawCurtain(e.Graphics);
                break;
            case SceneEntrance.Flash:
                DrawFlash(e.Graphics);
                break;
        }
    }

    /// <summary>Due bande che si ritirano verso l'alto e verso il basso.</summary>
    private void DrawCurtain(Graphics g)
    {
        var coverage = CareerTransitions.CurtainCoverage(frame);
        if (coverage <= 0.001) return;
        var height = (int)Math.Ceiling(ClientSize.Height * coverage);
        using var brush = new SolidBrush(UiTheme.Background);
        g.FillRectangle(brush, 0, 0, ClientSize.Width, height);
        g.FillRectangle(brush, 0, ClientSize.Height - height, ClientSize.Width, height);
        // Il filo d'accento sul bordo delle bande: è quello che fa leggere il
        // movimento come una sigla e non come un ritaglio.
        using var pen = new Pen(UiTheme.Accent, 2);
        g.DrawLine(pen, 0, height, ClientSize.Width, height);
        g.DrawLine(pen, 0, ClientSize.Height - height, ClientSize.Width, ClientSize.Height - height);
    }

    /// <summary>Lampo bianco che si esaurisce: annuncia, non illumina.</summary>
    private void DrawFlash(Graphics g)
    {
        var intensity = CareerTransitions.FlashIntensity(frame);
        if (intensity <= 0.001) return;
        using var brush = new SolidBrush(Color.FromArgb((int)(intensity * 235), Color.White));
        g.FillRectangle(brush, ClientRectangle);
    }

    // ----------------------------------------------------------------- uscita

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // La dissolvenza in uscita non deve poter impedire la chiusura: se
        // qualcosa va storto la finestra si chiude comunque.
        if (closing || !AnimationsEnabled || e.Cancel)
        {
            base.OnFormClosing(e);
            return;
        }

        base.OnFormClosing(e);
        if (e.Cancel) return;

        // L'esito va messo da parte PRIMA di annullare la chiusura.
        //
        // Windows Forms azzera DialogResult quando una chiusura viene
        // annullata: la dissolvenza in uscita annulla la chiusura per avere il
        // tempo di sfumare, e cosi' faceva sparire la risposta di OGNI finestra
        // dell'applicazione. Chi aveva premuto «FIRMA» vedeva tornare Cancel, e
        // il programma concludeva che non era stato scelto niente: da fuori era
        // un pulsante che non faceva niente. Ed era lo stesso difetto in ogni
        // schermata con un pulsante di conferma — per questo continuava a
        // ripresentarsi in posti diversi.
        var esito = DialogResult;
        e.Cancel = true;
        closing = true;
        var exitFrame = 0;
        var exitTimer = new System.Windows.Forms.Timer { Interval = CareerTransitions.FrameMilliseconds };
        exitTimer.Tick += (_, _) =>
        {
            // La finestra può essere stata smaltita mentre la dissolvenza era in
            // corso: rimandando la chiusura si apre una finestra di tempo in cui
            // qualcun altro può chiuderla davvero, e toccarla qui farebbe
            // cadere tutto con un oggetto già distrutto.
            if (IsDisposed || Disposing)
            {
                exitTimer.Stop();
                exitTimer.Dispose();
                return;
            }
            exitFrame++;
            Opacity = CareerTransitions.ExitOpacity(exitFrame);
            if (exitFrame < CareerTransitions.ExitFrames) return;
            exitTimer.Stop();
            exitTimer.Dispose();
            // Si restituisce la risposta che il giocatore aveva dato: impostare
            // DialogResult chiude da solo una finestra modale, e Close() copre
            // il caso in cui la finestra non lo sia.
            if (esito != DialogResult.None) DialogResult = esito;
            Close();
        };
        exitTimer.Start();
    }

    /// <summary>
    /// Le animazioni si spengono quando non c'è nessuno a guardarle: rendering
    /// fuori schermo, test, e la variabile CORSACAREER_NO_ANIM per chi le trova
    /// fastidiose.
    /// </summary>
    protected static bool AnimationsEnabled =>
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CORSACAREER_NO_ANIM"));

    protected override void Dispose(bool disposing)
    {
        if (disposing) sceneTimer.Dispose();
        base.Dispose(disposing);
    }
}
