using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>Com'è finita la camminata fino al negozio.</summary>
public sealed record TownWalkResult(bool Arrivato, int MinutiDiRitardo, int MinutiImpiegati)
{
    /// <summary>Puntuale: nessun ritardo.</summary>
    public bool InOrario => Arrivato && MinutiDiRitardo <= 0;

    /// <summary>
    /// Quanto pesa sulla trattativa. Arrivare puntuali è un piccolo vantaggio,
    /// arrivare tardi è un danno che cresce ma non azzera la visita: anche in
    /// ritardo si può convincere qualcuno, solo è più difficile.
    /// </summary>
    public int Modificatore => !Arrivato ? -100
        : MinutiDiRitardo <= 0 ? 6
        : -Math.Min(30, MinutiDiRitardo * 2);

    public string Racconto => !Arrivato
        ? "Non ci siamo arrivati."
        : MinutiDiRitardo <= 0
            ? $"Arrivati puntuali, con {Math.Abs(MinutiDiRitardo)} minuti di margine."
            : $"Arrivati con {MinutiDiRitardo} minuti di ritardo.";
}

/// <summary>
/// La strada fino allo sponsor, da fare a piedi.
///
/// Prima fra il decidere di andare da qualcuno e lo scoprire com'era andata non
/// c'era niente: un pulsante e un numero. Ma nel mondo che stiamo raccontando
/// arrivare da qualche parte è metà del problema — non c'è la macchina, ci sono
/// le biciclette e le gambe, e un appuntamento alle tre significa alle tre.
///
/// Non è un gioco d'azione: non si muore e non si perde nulla di irreversibile.
/// È una piccola pressione che rende la visita una cosa che hai fatto invece di
/// una cosa che è successa, e che dà un senso alla mappa della città in cui la
/// carriera è ambientata.
/// </summary>
public sealed class TownWalkDialog : CareerDialog
{
    private const int Cella = 22;
    private const int MinutiPerPasso = 1;

    private readonly TownMap mappa;
    private readonly string chiVa;
    private readonly int minutiConcessi;
    private readonly System.Windows.Forms.Timer orologio = new() { Interval = 90 };

    private double px, py;                 // posizione continua, in caselle
    private int dirX, dirY;                // ultima direzione, per disegnare la figura
    private double minutiTrascorsi;
    private bool arrivato, rinunciato;
    private int passoAnim;
    private readonly HashSet<Keys> premuti = [];

    public TownWalkResult Esito { get; private set; } = new(false, 0, 0);

    public TownWalkDialog(TownMap mappa, string chiVa, int minutiConcessi)
    {
        this.mappa = mappa;
        this.chiVa = chiVa;
        this.minutiConcessi = minutiConcessi;

        Text = $"CorsaCareer — verso {mappa.Insegna}";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;
        ClientSize = new Size(TownMap.Larghezza * Cella + 40, TownMap.Altezza * Cella + 148);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        KeyPreview = true;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

        px = mappa.Partenza.X + 0.5;
        py = mappa.Partenza.Y + 0.5;
        dirY = 1;

        orologio.Tick += (_, _) => Passo();
        orologio.Start();
    }

    // --------------------------------------------------------------- input

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape) { Rinuncia(); return; }
        premuti.Add(e.KeyCode);
        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        premuti.Remove(e.KeyCode);
        e.Handled = true;
    }

    private void Rinuncia()
    {
        if (arrivato) return;
        rinunciato = true;
        orologio.Stop();
        Esito = new TownWalkResult(false, 0, (int)minutiTrascorsi);
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // ------------------------------------------------------------ il passo

    /// <summary>
    /// Il passaggio a livello: chiuso per otto secondi ogni venticinque. Non è
    /// un ostacolo casuale — si vede arrivare e si può decidere di aggirarlo.
    /// </summary>
    private bool PassaggioAperto => (int)(minutiTrascorsi) % 25 < 17;

    private void Passo()
    {
        if (arrivato || rinunciato) return;

        var vx = 0.0; var vy = 0.0;
        if (premuti.Contains(Keys.Left) || premuti.Contains(Keys.A)) vx -= 1;
        if (premuti.Contains(Keys.Right) || premuti.Contains(Keys.D)) vx += 1;
        if (premuti.Contains(Keys.Up) || premuti.Contains(Keys.W)) vy -= 1;
        if (premuti.Contains(Keys.Down) || premuti.Contains(Keys.S)) vy += 1;

        if (vx != 0 || vy != 0)
        {
            var norma = Math.Sqrt(vx * vx + vy * vy);
            vx /= norma; vy /= norma;
            dirX = Math.Sign(vx); dirY = Math.Sign(vy);
            passoAnim++;

            var attrito = mappa.Attrito((int)px, (int)py);
            var velocita = 0.42 * attrito;

            // Gli assi si muovono separatamente: così si scivola lungo un muro
            // invece di incastrarsi in un angolo, che in una mappa a griglia
            // sarebbe la cosa più fastidiosa possibile.
            Muovi(vx * velocita, 0);
            Muovi(0, vy * velocita);
        }

        minutiTrascorsi += MinutiPerPasso * 0.34;

        if ((int)px == mappa.Destinazione.X && (int)py == mappa.Destinazione.Y)
        {
            arrivato = true;
            orologio.Stop();
            var impiegati = (int)Math.Round(minutiTrascorsi);
            Esito = new TownWalkResult(true, impiegati - minutiConcessi, impiegati);
            Invalidate();
            var messaggio = Esito.InOrario
                ? $"Arrivati.\n\nCi sono voluti {impiegati} minuti sui {minutiConcessi} che avevate: siete puntuali."
                : $"Arrivati, ma tardi.\n\nCi sono voluti {impiegati} minuti sui {minutiConcessi} che avevate: {Esito.MinutiDiRitardo} minuti di ritardo.";
            CareerMessages.Show(this, messaggio, $"CorsaCareer — {mappa.Insegna}");
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        // Il tempo non finisce mai del tutto: si può arrivare tardi quanto si
        // vuole. Fermare la scena a metà lascerebbe il giocatore senza esito.
        Invalidate();
    }

    private void Muovi(double dx, double dy)
    {
        var nx = px + dx;
        var ny = py + dy;
        const double raggio = 0.28;
        // Si controllano i quattro spigoli della figura, non il centro:
        // altrimenti mezzo corpo entrerebbe dentro i muri.
        foreach (var (ox, oy) in new[] { (-raggio, -raggio), (raggio, -raggio), (-raggio, raggio), (raggio, raggio) })
            if (!mappa.Calpestabile((int)(nx + ox), (int)(ny + oy), PassaggioAperto)) return;
        px = nx; py = ny;
    }

    // ------------------------------------------------------------ il disegno

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(UiTheme.Background);

        var ox = 20;
        var oy = 96;

        DisegnaTestata(g);

        for (var x = 0; x < TownMap.Larghezza; x++)
            for (var y = 0; y < TownMap.Altezza; y++)
                DisegnaCella(g, x, y, ox + x * Cella, oy + y * Cella);

        DisegnaInsegne(g, ox, oy);
        DisegnaBussola(g, ox, oy);
        DisegnaPersona(g, ox, oy);

        base.OnPaint(e);
    }

    private void DisegnaTestata(Graphics g)
    {
        var rimasti = minutiConcessi - (int)minutiTrascorsi;
        var tardi = rimasti < 0;

        using var titolo = new Font(UiTheme.FamilySemibold, 15F, FontStyle.Bold);
        using var etichetta = new Font(UiTheme.FamilySemibold, 8F, FontStyle.Bold);
        using var corpo = UiTheme.Body;

        using var sfondo = new SolidBrush(UiTheme.HeaderBackground);
        g.FillRectangle(sfondo, 0, 0, ClientSize.Width, 86);

        using var accento = new SolidBrush(UiTheme.Accent);
        g.DrawString("APPUNTAMENTO", etichetta, accento, 20, 12);

        using var primario = new SolidBrush(UiTheme.TextPrimary);
        g.DrawString(mappa.Insegna.ToUpperInvariant(), titolo, primario, 18, 26);

        using var secondario = new SolidBrush(UiTheme.TextSecondary);
        g.DrawString($"{chiVa} va a piedi.  Frecce o WASD per muoversi, ESC per rinunciare.",
            corpo, secondario, 20, 58);

        // Il cronometro: è l'unica cosa che deve saltare all'occhio.
        var colore = tardi ? UiTheme.Accent : rimasti < minutiConcessi / 4 ? UiTheme.Warning : UiTheme.Positive;
        using var pennello = new SolidBrush(colore);
        using var grande = new Font(UiTheme.FamilyMono, 22F, FontStyle.Bold);
        var testo = tardi ? $"+{-rimasti}'" : $"{rimasti}'";
        var larghezza = g.MeasureString(testo, grande).Width;
        g.DrawString(testo, grande, pennello, ClientSize.Width - larghezza - 22, 26);
        using var piccolo = new SolidBrush(UiTheme.TextMuted);
        var etichettaTempo = tardi ? "DI RITARDO" : "AL MINUTO";
        var lt = g.MeasureString(etichettaTempo, etichetta).Width;
        g.DrawString(etichettaTempo, etichetta, piccolo, ClientSize.Width - lt - 22, 12);

        if (!PassaggioAperto && mappa.RigaFerrovia >= 0)
        {
            using var avviso = new SolidBrush(UiTheme.Warning);
            g.DrawString("Le sbarre sono abbassate: passa un treno.", corpo, avviso, 20, 70);
        }
    }

    private void DisegnaCella(Graphics g, int x, int y, int sx, int sy)
    {
        var tile = mappa[x, y];
        Color colore = tile switch
        {
            TownTile.Strada => Color.FromArgb(52, 56, 66),
            TownTile.Marciapiede => Color.FromArgb(74, 78, 88),
            TownTile.Parco => Color.FromArgb(38, 74, 52),
            TownTile.Canale => Color.FromArgb(30, 62, 92),
            TownTile.Ponte => Color.FromArgb(96, 84, 66),
            TownTile.PassaggioALivello => PassaggioAperto ? Color.FromArgb(60, 64, 74) : Color.FromArgb(96, 44, 44),
            TownTile.Destinazione => Color.FromArgb(200, 60, 80),
            TownTile.Partenza => Color.FromArgb(60, 70, 92),
            _ => Color.FromArgb(28, 31, 39)
        };

        using var pennello = new SolidBrush(colore);
        g.FillRectangle(pennello, sx, sy, Cella, Cella);

        switch (tile)
        {
            case TownTile.Edificio:
                // Le finestre: due file di quadratini. È il dettaglio che fa
                // leggere il blocco come una casa e non come un buco.
                using (var finestra = new SolidBrush(Color.FromArgb(46, 50, 62)))
                {
                    g.FillRectangle(finestra, sx + 4, sy + 4, 5, 5);
                    g.FillRectangle(finestra, sx + 13, sy + 4, 5, 5);
                    g.FillRectangle(finestra, sx + 4, sy + 13, 5, 5);
                    g.FillRectangle(finestra, sx + 13, sy + 13, 5, 5);
                }
                using (var bordo = new Pen(Color.FromArgb(20, 22, 28)))
                    g.DrawRectangle(bordo, sx, sy, Cella, Cella);
                break;

            case TownTile.Strada:
                // La riga di mezzeria solo dove la strada prosegue dritta.
                using (var riga = new Pen(Color.FromArgb(120, 120, 110), 1.4f) { DashStyle = DashStyle.Dash })
                {
                    if (mappa[x, y - 1] is TownTile.Strada && mappa[x, y + 1] is TownTile.Strada)
                        g.DrawLine(riga, sx + Cella / 2f, sy, sx + Cella / 2f, sy + Cella);
                    else if (mappa[x - 1, y] is TownTile.Strada && mappa[x + 1, y] is TownTile.Strada)
                        g.DrawLine(riga, sx, sy + Cella / 2f, sx + Cella, sy + Cella / 2f);
                }
                break;

            case TownTile.Parco:
                using (var chioma = new SolidBrush(Color.FromArgb(56, 104, 68)))
                    g.FillEllipse(chioma, sx + 3, sy + 3, Cella - 6, Cella - 6);
                using (var scuro = new SolidBrush(Color.FromArgb(44, 84, 56)))
                    g.FillEllipse(scuro, sx + 7, sy + 7, Cella - 14, Cella - 14);
                break;

            case TownTile.Canale:
                using (var onda = new Pen(Color.FromArgb(64, 108, 148), 1.2f))
                {
                    g.DrawLine(onda, sx + 2, sy + 7, sx + Cella - 2, sy + 7);
                    g.DrawLine(onda, sx + 2, sy + 15, sx + Cella - 2, sy + 15);
                }
                break;

            case TownTile.Ponte:
                using (var asse = new Pen(Color.FromArgb(128, 112, 88), 1.2f))
                    for (var i = 3; i < Cella; i += 5)
                        g.DrawLine(asse, sx + i, sy, sx + i, sy + Cella);
                break;

            case TownTile.PassaggioALivello:
                using (var rotaia = new Pen(Color.FromArgb(150, 150, 158), 1.6f))
                {
                    g.DrawLine(rotaia, sx, sy + 8, sx + Cella, sy + 8);
                    g.DrawLine(rotaia, sx, sy + 14, sx + Cella, sy + 14);
                }
                if (!PassaggioAperto)
                    using (var sbarra = new Pen(Color.FromArgb(240, 200, 70), 2.4f))
                        g.DrawLine(sbarra, sx, sy + Cella / 2f, sx + Cella, sy + Cella / 2f);
                break;

            case TownTile.Destinazione:
                using (var tenda = new SolidBrush(Color.FromArgb(240, 220, 120)))
                    g.FillRectangle(tenda, sx + 3, sy + 3, Cella - 6, 6);
                using (var porta = new SolidBrush(Color.FromArgb(60, 24, 30)))
                    g.FillRectangle(porta, sx + 7, sy + 11, Cella - 14, Cella - 12);
                break;

            case TownTile.Partenza:
                using (var segno = new Pen(Color.FromArgb(120, 150, 200), 1.6f))
                    g.DrawRectangle(segno, sx + 4, sy + 4, Cella - 9, Cella - 9);
                break;
        }
    }

    private void DisegnaInsegne(Graphics g, int ox, int oy)
    {
        using var carattere = new Font(UiTheme.FamilySemibold, 6.2F, FontStyle.Bold);
        using var inchiostro = new SolidBrush(Color.FromArgb(150, 158, 172));
        foreach (var (cella, testo) in mappa.Insegne)
            g.DrawString(testo, carattere, inchiostro, ox + cella.X * Cella - 4, oy + cella.Y * Cella + 6);

        // L'insegna del posto dove si va, scritta grande: senza, la si cerca.
        using var grande = new Font(UiTheme.FamilySemibold, 8.5F, FontStyle.Bold);
        using var rosso = new SolidBrush(Color.FromArgb(250, 210, 220));
        var d = mappa.Destinazione;
        var etichetta = mappa.Insegna.ToUpperInvariant();
        var larghezza = g.MeasureString(etichetta, grande).Width;
        var lx = Math.Min(ox + d.X * Cella - larghezza / 2 + Cella / 2f, ox + TownMap.Larghezza * Cella - larghezza);
        using var fondo = new SolidBrush(Color.FromArgb(200, 120, 20, 34));
        g.FillRectangle(fondo, lx - 4, oy + d.Y * Cella - 17, larghezza + 8, 15);
        g.DrawString(etichetta, grande, rosso, lx, oy + d.Y * Cella - 16);
    }

    /// <summary>
    /// La freccia che indica dov'è il negozio. Senza, in una città di
    /// millecento caselle si gira a caso, e girare a caso non è una scelta.
    /// </summary>
    private void DisegnaBussola(Graphics g, int ox, int oy)
    {
        var dx = mappa.Destinazione.X + 0.5 - px;
        var dy = mappa.Destinazione.Y + 0.5 - py;
        var distanza = Math.Sqrt(dx * dx + dy * dy);
        if (distanza < 2.5) return;

        var cx = (float)(ox + px * Cella);
        var cy = (float)(oy + py * Cella);
        var ang = Math.Atan2(dy, dx);
        var r1 = 30f;
        var punta = new PointF(cx + (float)(Math.Cos(ang) * (r1 + 9)), cy + (float)(Math.Sin(ang) * (r1 + 9)));
        var b1 = new PointF(cx + (float)(Math.Cos(ang + 0.42) * r1), cy + (float)(Math.Sin(ang + 0.42) * r1));
        var b2 = new PointF(cx + (float)(Math.Cos(ang - 0.42) * r1), cy + (float)(Math.Sin(ang - 0.42) * r1));
        using var freccia = new SolidBrush(Color.FromArgb(150, 235, 90, 110));
        g.FillPolygon(freccia, [punta, b1, b2]);
    }

    private void DisegnaPersona(Graphics g, int ox, int oy)
    {
        var cx = (float)(ox + px * Cella);
        var cy = (float)(oy + py * Cella);

        using var ombra = new SolidBrush(Color.FromArgb(90, 0, 0, 0));
        g.FillEllipse(ombra, cx - 8, cy + 4, 16, 7);

        // Le gambe si alternano quando si cammina: due trattini bastano a far
        // sembrare che si stia andando da qualche parte.
        var oscilla = (passoAnim / 2) % 2 == 0 ? 1 : -1;
        using var pantaloni = new Pen(Color.FromArgb(46, 58, 92), 3f);
        g.DrawLine(pantaloni, cx - 3, cy + 2, cx - 3 - oscilla, cy + 8);
        g.DrawLine(pantaloni, cx + 3, cy + 2, cx + 3 + oscilla, cy + 8);

        using var giacca = new SolidBrush(Color.FromArgb(210, 66, 82));
        g.FillRectangle(giacca, cx - 6, cy - 7, 12, 11);

        using var pelle = new SolidBrush(Color.FromArgb(238, 206, 178));
        g.FillEllipse(pelle, cx - 6, cy - 17, 12, 12);

        using var capelli = new SolidBrush(Color.FromArgb(38, 32, 34));
        // I capelli seguono la direzione: se si va in su si vede la nuca.
        if (dirY < 0) g.FillEllipse(capelli, cx - 6, cy - 17, 12, 11);
        else
        {
            g.FillPie(capelli, cx - 6, cy - 18, 12, 12, 180, 180);
            using var occhi = new SolidBrush(Color.FromArgb(30, 26, 28));
            var scarto = dirX * 1.4f;
            g.FillEllipse(occhi, cx - 3.4f + scarto, cy - 10, 2.1f, 2.6f);
            g.FillEllipse(occhi, cx + 1.3f + scarto, cy - 10, 2.1f, 2.6f);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) orologio.Dispose();
        base.Dispose(disposing);
    }
}
