namespace CorsaCareer;

/// <summary>Di che cosa è fatta una casella della cittadina.</summary>
public enum TownTile
{
    /// <summary>Asfalto: ci si cammina, ed è la via più veloce.</summary>
    Strada,
    /// <summary>Marciapiede: ci si cammina.</summary>
    Marciapiede,
    /// <summary>Edificio: non si attraversa.</summary>
    Edificio,
    /// <summary>Verde pubblico: ci si cammina, ma si va più piano.</summary>
    Parco,
    /// <summary>Canale: non si attraversa se non sui ponti.</summary>
    Canale,
    /// <summary>Ponte sul canale.</summary>
    Ponte,
    /// <summary>Passaggio a livello: si attraversa solo quando è aperto.</summary>
    PassaggioALivello,
    /// <summary>La porta del negozio da raggiungere.</summary>
    Destinazione,
    /// <summary>Da dove si parte.</summary>
    Partenza
}

/// <summary>
/// La cittadina di provincia in cui Haru va a cercare gli sponsor.
///
/// Serviva una mappa vera e non un fondale: la visita a uno sponsor era un
/// pulsante e un tiro di dado, e non c'era niente da fare fra il decidere e il
/// sapere com'è andata. Qui invece il tempo per arrivare è una risorsa come le
/// altre — si sceglie la strada, si perde tempo se si sbaglia, e presentarsi in
/// ritardo a un appuntamento pesa sulla trattativa esattamente come peserebbe
/// nella realtà.
///
/// La pianta non è disegnata a mano: nasce da un seme stabile, quindi la stessa
/// visita ha sempre la stessa città e ricaricare non regala una scorciatoia.
/// L'impianto è quello di una cittadina giapponese di provincia — una griglia
/// di isolati, un canale che la taglia con pochi ponti, la ferrovia con il
/// passaggio a livello, e il verde negli spazi rimasti.
/// </summary>
public sealed class TownMap
{
    /// <summary>Larghezza e altezza in caselle.</summary>
    public const int Larghezza = 40;
    public const int Altezza = 28;

    private readonly TownTile[,] celle = new TownTile[Larghezza, Altezza];

    public Point Partenza { get; private set; }
    public Point Destinazione { get; private set; }
    /// <summary>Il nome scritto sull'insegna del negozio da raggiungere.</summary>
    public string Insegna { get; }
    /// <summary>La riga della ferrovia, o -1 se questa pianta non ne ha.</summary>
    public int RigaFerrovia { get; private set; } = -1;
    /// <summary>La colonna del canale, o -1.</summary>
    public int ColonnaCanale { get; private set; } = -1;

    public TownTile this[int x, int y] =>
        x < 0 || y < 0 || x >= Larghezza || y >= Altezza ? TownTile.Edificio : celle[x, y];

    /// <summary>Le insegne dei negozi vicini, per dare vita agli isolati.</summary>
    public IReadOnlyList<(Point Cella, string Testo)> Insegne => insegne;
    private readonly List<(Point, string)> insegne = [];

    private TownMap(string insegna) { Insegna = insegna; }

    /// <summary>
    /// Costruisce la pianta. Lo stesso seme dà sempre la stessa città: è
    /// quello che rende onesto il cronometro.
    /// </summary>
    public static TownMap Genera(long seme, string insegna)
    {
        var m = new TownMap(insegna);
        var r = new SemeStabile(seme);

        // 1. Tutto edificato, poi si scavano le strade: è più semplice che il
        //    contrario e non lascia isolati irraggiungibili.
        for (var x = 0; x < Larghezza; x++)
            for (var y = 0; y < Altezza; y++)
                m.celle[x, y] = TownTile.Edificio;

        // 2. La griglia degli isolati. Le vie non sono equidistanti — una
        //    cittadina vera non è una scacchiera — ma il passo resta fra 5 e 8
        //    caselle, così non nascono isolati impossibili da aggirare.
        var colonne = new List<int>();
        for (var x = 2; x < Larghezza - 2; x += r.Fra(5, 8)) colonne.Add(x);
        if (colonne[^1] < Larghezza - 4) colonne.Add(Larghezza - 3);

        var righe = new List<int>();
        for (var y = 2; y < Altezza - 2; y += r.Fra(5, 8)) righe.Add(y);
        if (righe[^1] < Altezza - 4) righe.Add(Altezza - 3);

        foreach (var x in colonne)
            for (var y = 1; y < Altezza - 1; y++)
            {
                m.celle[x, y] = TownTile.Strada;
                if (x - 1 > 0) m.celle[x - 1, y] = TownTile.Marciapiede;
                if (x + 1 < Larghezza) m.celle[x + 1, y] = TownTile.Marciapiede;
            }

        foreach (var y in righe)
            for (var x = 1; x < Larghezza - 1; x++)
            {
                m.celle[x, y] = TownTile.Strada;
                if (y - 1 > 0 && m.celle[x, y - 1] == TownTile.Edificio) m.celle[x, y - 1] = TownTile.Marciapiede;
                if (y + 1 < Altezza && m.celle[x, y + 1] == TownTile.Edificio) m.celle[x, y + 1] = TownTile.Marciapiede;
            }

        // 3. Il canale. Taglia la città in verticale e lascia solo due ponti:
        //    è l'elemento che rende la scelta del percorso una scelta vera,
        //    perché sbagliare ponte costa mezzo minuto buono.
        var canale = colonne[r.Fra(1, Math.Max(2, colonne.Count - 1))] + 3;
        if (canale > 4 && canale < Larghezza - 4)
        {
            m.ColonnaCanale = canale;
            for (var y = 0; y < Altezza; y++) m.celle[canale, y] = TownTile.Canale;
            var pontiPossibili = righe.Where(y => y > 2 && y < Altezza - 3).ToList();
            if (pontiPossibili.Count > 0)
            {
                var primo = pontiPossibili[r.Fra(0, pontiPossibili.Count)];
                var secondo = pontiPossibili[r.Fra(0, pontiPossibili.Count)];
                m.celle[canale, primo] = TownTile.Ponte;
                m.celle[canale, secondo] = TownTile.Ponte;
            }
        }

        // 4. La ferrovia e il passaggio a livello: si apre e si chiude da solo
        //    durante la partita, e non si può forzare.
        var ferrovia = righe.Count > 2 ? righe[^2] + 3 : -1;
        if (ferrovia > 3 && ferrovia < Altezza - 3)
        {
            m.RigaFerrovia = ferrovia;
            for (var x = 0; x < Larghezza; x++)
                m.celle[x, ferrovia] = m.celle[x, ferrovia] == TownTile.Canale
                    ? TownTile.Canale
                    : TownTile.Edificio;
            foreach (var x in colonne)
                if (m.celle[x, ferrovia] != TownTile.Canale)
                    m.celle[x, ferrovia] = TownTile.PassaggioALivello;
        }

        // 5. Un po' di verde dove non c'è niente da costruire: serve a rendere
        //    la città leggibile, e attraversarlo è più lento della strada.
        for (var i = 0; i < 14; i++)
        {
            var px = r.Fra(2, Larghezza - 4);
            var py = r.Fra(2, Altezza - 4);
            for (var dx = 0; dx < r.Fra(2, 4); dx++)
                for (var dy = 0; dy < r.Fra(2, 4); dy++)
                    if (m[px + dx, py + dy] == TownTile.Edificio)
                        m.celle[px + dx, py + dy] = TownTile.Parco;
        }

        // 6. Partenza e destinazione, lontane fra loro: se il negozio fosse
        //    dietro l'angolo non ci sarebbe nessuna corsa da fare.
        var camminabili = new List<Point>();
        for (var x = 1; x < Larghezza - 1; x++)
            for (var y = 1; y < Altezza - 1; y++)
                if (m.celle[x, y] is TownTile.Strada or TownTile.Marciapiede)
                    camminabili.Add(new Point(x, y));

        var partenza = camminabili.OrderBy(p => p.X + p.Y).Skip(r.Fra(0, 6)).First();
        var arrivo = camminabili.OrderByDescending(p => p.X + p.Y).Skip(r.Fra(0, 6)).First();
        m.Partenza = partenza;
        m.Destinazione = arrivo;
        m.celle[partenza.X, partenza.Y] = TownTile.Partenza;
        m.celle[arrivo.X, arrivo.Y] = TownTile.Destinazione;

        // 7. Le insegne degli altri negozi: sono decorazione, ma sono la
        //    differenza fra una griglia di rettangoli e un posto.
        string[] botteghe = ["RAMEN", "UDON", "FERRAMENTA", "PESCE", "KOBAN", "SCUOLA", "TABACCHI", "LAVANDERIA", "BAGNI", "LIBRI"];
        for (var i = 0; i < botteghe.Length; i++)
        {
            for (var tentativo = 0; tentativo < 40; tentativo++)
            {
                var cx = r.Fra(1, Larghezza - 1);
                var cy = r.Fra(1, Altezza - 1);
                if (m.celle[cx, cy] != TownTile.Edificio) continue;
                if (m.insegne.Any(s => Math.Abs(s.Item1.X - cx) < 6 && Math.Abs(s.Item1.Y - cy) < 4)) continue;
                m.insegne.Add((new Point(cx, cy), botteghe[i]));
                break;
            }
        }

        return m;
    }

    /// <summary>Se una casella si può calpestare, dato lo stato del passaggio a livello.</summary>
    public bool Calpestabile(int x, int y, bool passaggioAperto) => this[x, y] switch
    {
        TownTile.Edificio => false,
        TownTile.Canale => false,
        TownTile.PassaggioALivello => passaggioAperto,
        _ => true
    };

    /// <summary>Quanto rallenta il passo questa casella: la strada è la via veloce.</summary>
    public double Attrito(int x, int y) => this[x, y] switch
    {
        TownTile.Parco => 0.62,
        TownTile.Marciapiede => 0.86,
        _ => 1.0
    };

    /// <summary>
    /// La distanza minima a piedi fra partenza e negozio, in caselle.
    ///
    /// Serve a fissare il tempo concesso: si dà il percorso migliore più un
    /// margine. Senza questo il limite sarebbe arbitrario e una pianta
    /// sfortunata renderebbe la visita impossibile.
    /// </summary>
    public int DistanzaMinima()
    {
        var visti = new bool[Larghezza, Altezza];
        var coda = new Queue<(Point P, int D)>();
        coda.Enqueue((Partenza, 0));
        visti[Partenza.X, Partenza.Y] = true;
        int[] dx = [1, -1, 0, 0];
        int[] dy = [0, 0, 1, -1];
        while (coda.Count > 0)
        {
            var (p, d) = coda.Dequeue();
            if (p == Destinazione) return d;
            for (var i = 0; i < 4; i++)
            {
                var nx = p.X + dx[i];
                var ny = p.Y + dy[i];
                if (nx < 0 || ny < 0 || nx >= Larghezza || ny >= Altezza) continue;
                if (visti[nx, ny]) continue;
                if (!Calpestabile(nx, ny, passaggioAperto: true)) continue;
                visti[nx, ny] = true;
                coda.Enqueue((new Point(nx, ny), d + 1));
            }
        }
        return -1;
    }

    /// <summary>
    /// Un generatore ripetibile. Non si usa Random: due partite con la stessa
    /// visita devono avere la stessa città, altrimenti il tempo concesso non
    /// vuol dire niente.
    /// </summary>
    private sealed class SemeStabile(long seme)
    {
        private ulong stato = (ulong)seme | 1;

        public int Fra(int minimo, int massimoEscluso)
        {
            if (massimoEscluso <= minimo) return minimo;
            stato ^= stato << 13; stato ^= stato >> 7; stato ^= stato << 17;
            return minimo + (int)(stato % (ulong)(massimoEscluso - minimo));
        }
    }
}
