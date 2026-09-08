using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>
/// Il benvenuto nella squadra.
///
/// Firmare e' il momento piu' importante di una carriera dopo una vittoria, e
/// non aveva nessuna scena. Un sedile professionistico chiudeva con una
/// finestrella di testo; un sedile cliente — cioe' il primo di tutti, quello
/// del kart — non diceva assolutamente niente: si premeva «firma» e lo schermo
/// restava identico. Da fuori sembrava un pulsante rotto, ed e' il difetto che
/// e' stato segnalato piu' volte.
///
/// Qui c'e' il simbolo della squadra, una tavola della scena, e soprattutto la
/// spiegazione di che cosa comporta: dove si corre, con che vettura, chi paga
/// cosa e che cosa succede adesso. Indipendentemente da quanto c'e' in cassa —
/// i soldi sono un problema del weekend, non della firma.
/// </summary>
public sealed class ContractSignedDialog : CareerDialog
{
    private Bitmap? logo;
    private Bitmap? tavola;

    public ContractSignedDialog(CareerState career, TeamOffer offer, string categoria,
        LadderRung gradino, int gareDaScegliere, IReadOnlyList<ScheduledEvent> calendario)
    {
        Text = $"CorsaCareer — benvenuto in {career.Team}";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Font = UiTheme.Body;

        // --- testata
        var header = new Panel { Dock = DockStyle.Top, Height = 118, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 18, 38, 14) };
        header.Controls.Add(new Label
        {
            Text = "COMPLIMENTI, HAI UN SEDILE", Dock = DockStyle.Top, Height = 26,
            Font = UiTheme.Kicker, ForeColor = UiTheme.Positive
        });
        header.Controls.Add(new Label
        {
            Text = $"BENVENUTO IN {career.Team.ToUpperInvariant()}", Dock = DockStyle.Top, Height = 44,
            Font = UiTheme.Headline, ForeColor = UiTheme.Warning, AutoEllipsis = true
        });
        header.Controls.Add(new Label
        {
            Text = offer.IsClientSeat
                ? "Non sei un dipendente: sei un cliente. Paghi il weekend, corri, e quello che vinci resta tuo."
                : "Da oggi qualcuno ti paga per guidare. Cambia tutto, comprese le aspettative.",
            Dock = DockStyle.Bottom, Height = 30, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary
        });

        // --- corpo a due colonne
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
            Padding = new Padding(32, 22, 32, 18), BackColor = UiTheme.Background
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));

        // colonna sinistra: il simbolo della squadra e la tavola della scena
        var visualCard = UiTheme.Card("LA SQUADRA", out var visualInner, UiTheme.Warning);
        visualCard.Dock = DockStyle.Fill;
        var visualLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = UiTheme.Surface };
        visualLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
        visualLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 54));

        var logoBox = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Zoom };
        logo = Carica(TeamLogoCatalog.LogoPath(career.Team));
        logoBox.Image = logo;
        visualLayout.Controls.Add(logoBox, 0, 0);

        var scenaBox = new PictureBox { Dock = DockStyle.Fill, BackColor = UiTheme.SurfaceRaised, SizeMode = PictureBoxSizeMode.Zoom };
        tavola = Carica(TavolaDellaFirma(career, gradino));
        scenaBox.Image = tavola;
        visualLayout.Controls.Add(scenaBox, 0, 1);
        visualInner.Controls.Add(visualLayout);
        grid.Controls.Add(visualCard, 0, 0);

        // colonna destra: che cosa comporta
        var infoCard = UiTheme.Card("COSA COMPORTA", out var infoInner, UiTheme.Accent);
        infoCard.Dock = DockStyle.Fill;
        var testo = new RichTextBox
        {
            Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Surface, ForeColor = UiTheme.TextPrimary, Font = UiTheme.Body
        };
        testo.Text = Riepilogo(career, offer, categoria, gradino, gareDaScegliere, calendario);
        infoInner.Controls.Add(testo);
        grid.Controls.Add(infoCard, 1, 0);

        // --- piede
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 78, BackColor = UiTheme.HeaderBackground, Padding = new Padding(38, 15, 38, 15) };
        var avanti = UiTheme.PrimaryButton(offer.IsClientSeat ? "SCEGLI IL PRIMO WEEKEND" : "COMINCIAMO");
        avanti.Dock = DockStyle.Right; avanti.Width = 300; avanti.DialogResult = DialogResult.OK;
        footer.Controls.Add(avanti);
        footer.Controls.Add(new Label
        {
            Text = offer.IsClientSeat
                ? "La quota di ogni weekend si paga al momento di sceglierlo, non adesso."
                : "Il calendario è pubblicato: da qui in poi ogni gara assegna punti.",
            Dock = DockStyle.Fill, Font = UiTheme.Body, ForeColor = UiTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        });
        AcceptButton = avanti;

        Controls.Add(grid); Controls.Add(footer); Controls.Add(header);
        Disposed += (_, _) => { logo?.Dispose(); tavola?.Dispose(); };
    }

    private static string Riepilogo(CareerState career, TeamOffer offer, string categoria,
        LadderRung gradino, int gareDaScegliere, IReadOnlyList<ScheduledEvent> calendario)
    {
        var righe = new List<string>
        {
            "DOVE CORRI",
            $"{gradino.Name} · categoria {gradino.Step} di {CareerLadder.Steps}",
            $"{career.Championship} · livello {ChampionshipLadder.Clamp(career.ChampionshipLevel)} di {ChampionshipLadder.Levels}",
            ChampionshipLadder.PromotionRule(career.ChampionshipLevel),
            "",
            "LA VETTURA",
            $"{UiText.Car(offer.Car)} · {categoria}",
            $"Livrea: {(string.IsNullOrWhiteSpace(career.Livery) ? "da definire" : career.Livery)}",
            $"Compagno di squadra: {career.Teammate}",
            ""
        };

        if (offer.IsClientSeat)
        {
            righe.AddRange([
                "COME FUNZIONA UN SEDILE CLIENTE",
                "Il team non ti assume: ti vende l'accesso a una vettura e a una gara.",
                $"Quota di ogni weekend: € {offer.NetRaceFee:N0}" +
                    (offer.TeamSupportPercent > 0 ? $"  (il team ne copre il {offer.TeamSupportPercent}%)" : "  (nessun contributo del team)"),
                "Nessuno stipendio: se vai male resta soltanto la quota bruciata.",
                "",
                "QUANTO SI PORTA A CASA",
                $"P1 € {offer.PrizeP1:N0} · P2 € {offer.PrizeP2:N0} · P3 € {offer.PrizeP3:N0} · P4–P5 € {offer.PrizeP4P5:N0}",
                "",
                "IL CALENDARIO",
                gareDaScegliere > 0
                    ? $"{gareDaScegliere} weekend aperti: scegli tu quali correre e quando."
                    : "Nessun weekend disponibile al momento: torna nelle opportunità fra qualche giorno."
            ]);
        }
        else
        {
            righe.AddRange([
                "IL CONTRATTO",
                $"{offer.Years} anno/i · " + (offer.Salary > 0
                    ? $"€ {offer.Salary:N0} all'anno, garantiti dal team"
                    : "nessuno stipendio: si corre per farsi vedere"),
                $"Obiettivo: {career.ContractObjective}",
                "Premi e sponsor restano separati dallo stipendio.",
                "",
                "IL CALENDARIO"
            ]);
            if (calendario.Count > 0)
            {
                var primo = calendario[0];
                righe.Add($"{calendario.Count} round confermati.");
                righe.Add($"Si comincia a {(string.IsNullOrWhiteSpace(primo.TrackName) ? primo.TrackId : primo.TrackName)}, {NarrativeCalendar.Format(primo.Date)}.");
            }
            else righe.Add("Il calendario parte dalla prossima stagione: quella in corso si chiude con il sedile attuale.");
        }

        righe.Add("");
        righe.Add("COSA CAMBIA ADESSO");
        righe.Add(offer.IsClientSeat
            ? "Prima eri un ragazzo che chiedeva di girare. Adesso hai una squadra alle spalle, "
              + "un compagno con cui essere confrontato, e un nome che comincia a comparire negli elenchi. "
              + "Ogni weekend che compri è una scommessa: se va bene ti ripaga, se va male l'hai pagata tu."
            : "Da adesso i risultati non sono più solo tuoi: c'è una squadra che ti ha scelto e che "
              + "ti giudicherà gara dopo gara. Il compagno di squadra è il primo metro di paragone, "
              + "e il paddock guarda quello prima della classifica.");

        return string.Join("\n", righe);
    }

    /// <summary>
    /// La tavola giusta per la firma: la disciplina in cui si corre davvero e il
    /// momento della trattativa. Se non c'e' niente di coerente non si mette
    /// un'immagine a caso — meglio nessuna tavola che una GT sopra un kart.
    /// </summary>
    private static string TavolaDellaFirma(CareerState career, LadderRung gradino)
    {
        var disciplina = gradino.Path switch
        {
            LadderPath.Karting => "kart",
            LadderPath.SingleSeater => gradino.Tier == "Formula / top tier" ? "formula-vertice"
                : gradino.Tier == "Categoria avanzata" ? "formula-alta" : "formula-minore",
            LadderPath.Endurance => gradino.Tier == "Categoria regionale" ? "gt" : "endurance",
            LadderPath.Touring => "turismo",
            _ => "kart"
        };
        var seme = (career.Driver ?? "").Length * 17 + career.Races * 3 + career.Season;
        foreach (var momento in new[] { "contratto", "trattativa" })
        {
            var scelte = IllustrationCatalog.Find(disciplina, momento, seme, 1);
            if (scelte.Count > 0) return AssetPaths.File(scelte[0]);
        }
        return "";
    }

    private static Bitmap? Carica(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try { using var source = Image.FromFile(path); return new Bitmap(source); }
        catch { return null; }
    }
}
