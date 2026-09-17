using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// La giornata, tutta in una schermata.
///
/// Prima le scelte di un giorno erano sparse in quattro posti: gli impegni
/// fissi stavano qui, le attività del pilota in «Attività del pilota», il
/// social in un terzo riquadro e il lavoro di Haru in un quarto. Ognuno
/// mostrava un pezzo del bilancio delle ore, e nessuno lo mostrava tutto:
/// dal riquadro del social non si poteva sapere quante ore restavano dopo la
/// palestra, perché la palestra era in un'altra finestra.
///
/// Qui c'è una giornata sola, con un solo conto delle ore. Il bilancio si
/// apre dicendo dove finiscono le ventiquattro: sonno, pasti, scuola. Quello
/// che resta è la parte che si decide, e sta tutta sotto — prima gli impegni
/// con un orario, poi le attività libere del pilota, poi quelle di Haru, che
/// ha una giornata sua e non attinge alle stesse ore.
/// </summary>
public sealed class DailyAgendaDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly ContentIndexRecord content;
    private readonly Action launchTraining;
    private readonly Action save;
    private readonly Action<DayReport>? onScene;

    private readonly FlowLayoutPanel flow = new()
    {
        Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
        AutoScroll = true, Padding = new Padding(28, 18, 28, 18)
    };

    private const int Larghezza = 780;

    public DailyAgendaDialog(CareerState career, ContentIndexRecord content, Action launchTraining, Action save,
                             Action<DayReport>? onScene = null)
    {
        this.career = career;
        this.content = content;
        this.launchTraining = launchTraining;
        this.save = save;
        this.onScene = onScene;

        Text = "CorsaCareer — la giornata";
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;
        Width = 900; Height = 720;
        Controls.Add(flow);
        RefreshItems();
    }

    private int Eta => career.BirthYear <= 0 ? 12 : Math.Max(10, career.StoryDate.Year - career.BirthYear);

    private void RefreshItems()
    {
        flow.SuspendLayout();
        foreach (Control c in flow.Controls) c.Dispose();
        flow.Controls.Clear();

        var giorno = DriverDay.EnsureToday(career);
        var data = career.StoryDate.ToString("dddd d MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));

        flow.Controls.Add(Riga("OGGI · " + data.ToUpperInvariant(), UiTheme.Kicker, UiTheme.Warning, 8));
        flow.Controls.Add(BilancioDelleOre(giorno));

        var impegni = LifeCalendar.Today(career, content).OrderBy(x => x.StartTime).ToList();
        if (impegni.Count > 0)
        {
            flow.Controls.Add(Titolo("IMPEGNI CON UN ORARIO"));
            flow.Controls.Add(Riga(
                "Hanno una fascia oraria fissa. Quelli obbligatori vengono saltati da soli se fai passare il giorno senza chiuderli.",
                UiTheme.Small, UiTheme.TextMuted, 10));
            foreach (var item in impegni) flow.Controls.Add(SchedaImpegno(item));
        }

        // --- le ore libere del pilota
        flow.Controls.Add(Titolo($"LE ORE LIBERE DEL PILOTA · {giorno.DriverHoursLeft} DI {giorno.DriverHoursTotal} ANCORA DA SPENDERE"));
        if (giorno.DriverHoursLeft <= 0)
            flow.Controls.Add(Riga("La giornata è finita. Quello che non hai fatto oggi non torna.", UiTheme.Prose, UiTheme.TextMuted, 12));
        foreach (var gruppo in new[] { DayFocus.Fisico, DayFocus.Immagine, DayFocus.Altro })
        {
            var attivita = DayActivityCatalog.ForDriver().Where(x => x.Focus == gruppo).ToList();
            if (attivita.Count == 0) continue;
            flow.Controls.Add(Riga(EtichettaFocus(gruppo), UiTheme.Kicker, UiTheme.Info, 6));
            foreach (var a in attivita) flow.Controls.Add(SchedaAttivita(a, giorno));
        }

        // --- Haru, che ha una giornata sua
        flow.Controls.Add(Titolo($"HARU SENDA · {giorno.AgentHoursLeft} DI {giorno.AgentHoursTotal} ORE"));
        flow.Controls.Add(Riga(
            "Haru va a scuola come te: quello che gli resta sono due o tre ore di pomeriggio. Le sue ore non sono le tue — "
            + "un pomeriggio in cui lui gira a cercare sponsor non è un pomeriggio che hai speso tu.",
            UiTheme.Prose, UiTheme.TextSecondary, 10));
        foreach (var a in DayActivityCatalog.ForAgent()) flow.Controls.Add(SchedaAttivita(a, giorno));

        var chiudi = UiTheme.SecondaryButton("TORNA AL PORTALE");
        chiudi.Width = 260; chiudi.Height = 42; chiudi.Margin = new Padding(0, 16, 0, 0);
        chiudi.Click += (_, _) => Close();
        flow.Controls.Add(chiudi);
        flow.ResumeLayout();
    }

    // ------------------------------------------------------- il conto delle ore

    /// <summary>
    /// Dove finiscono le ventiquattro ore di oggi.
    ///
    /// È la parte che mancava del tutto: il programma diceva «otto ore
    /// disponibili» senza mai dire otto su cosa, e sembrava che un ragazzo di
    /// dodici anni avesse otto ore di vita al giorno. Le fette fisse non si
    /// scelgono, ed è proprio per questo che vanno viste: sono la ragione per
    /// cui le altre sono poche.
    /// </summary>
    private Control BilancioDelleOre(DayPlan giorno)
    {
        var blocchi = DriverDay.BlocchiFissi(career.StoryDate, Eta);
        var card = new Panel
        {
            Width = Larghezza, BackColor = UiTheme.Surface, Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 18), AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        var dentro = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = Larghezza - 40, BackColor = Color.Transparent,
            Margin = new Padding(0), Padding = new Padding(0)
        };

        dentro.Controls.Add(Riga("LE VENTIQUATTRO ORE DI OGGI", UiTheme.Kicker, UiTheme.TextMuted, 8, Larghezza - 44));
        foreach (var b in blocchi)
            dentro.Controls.Add(Riga($"{b.Ore,2}h · {b.Nome} — {b.Perche}", UiTheme.Small, UiTheme.TextSecondary, 2, Larghezza - 44));

        var spese = giorno.DriverHoursTotal - giorno.DriverHoursLeft;
        dentro.Controls.Add(Riga(
            $"{giorno.DriverHoursTotal,2}h · Libere — le decidi tu"
            + (spese > 0 ? $"  ({spese} già spese, ne restano {giorno.DriverHoursLeft})" : ""),
            UiTheme.BodyStrong, giorno.DriverHoursLeft > 0 ? UiTheme.Positive : UiTheme.TextMuted, 6, Larghezza - 44));

        card.Controls.Add(dentro);
        return card;
    }

    private static string EtichettaFocus(DayFocus focus) => focus switch
    {
        DayFocus.Fisico => "CORPO E RECUPERO",
        DayFocus.Immagine => "NOME E SEGUITO",
        _ => "IL RESTO"
    };

    // ------------------------------------------------------------- le schede

    private Control SchedaImpegno(DailyCommitment item)
    {
        var fatto = item.Status is "done" or "skipped";
        var card = Scheda(out var dentro, fatto);

        var stato = item.Status switch
        {
            "done" => ("✓ FATTO", UiTheme.Positive),
            "skipped" => ("— SALTATO", UiTheme.Accent),
            _ => (item.Required ? "OBBLIGATORIO" : "FACOLTATIVO", item.Required ? UiTheme.Warning : UiTheme.TextMuted)
        };
        dentro.Controls.Add(Riga($"{item.StartTime} · {item.Hours}h · {stato.Item1}", UiTheme.Kicker, stato.Item2, 2, Larghezza - 44));
        dentro.Controls.Add(Riga(item.Title, UiTheme.BodyStrong, UiTheme.TextPrimary, 2, Larghezza - 44));
        dentro.Controls.Add(Riga(item.Detail + (item.TrackName.Length > 0 ? "\nLuogo: " + item.TrackName : ""),
            UiTheme.Small, UiTheme.TextSecondary, 8, Larghezza - 44));

        if (item.Status is "planned" or "active")
        {
            var azione = item.Kind switch
            {
                "track-training" => "APRI L'ALLENAMENTO",
                "sponsor-visit" => "INCONTRA LO SPONSOR",
                "fitness" => "ALLENATI",
                "recovery" => "RIPOSA",
                _ => "FREQUENTA"
            };
            dentro.Controls.Add(Pulsanti(
                azione, () =>
                {
                    if (item.Kind == "track-training")
                    {
                        item.Status = "active"; save(); Close(); launchTraining();
                        return;
                    }
                    LifeCalendar.Complete(career, item); save(); RefreshItems();
                },
                "SALTA", () => { LifeCalendar.Skip(career, item); save(); RefreshItems(); }));
        }
        return card;
    }

    private Control SchedaAttivita(DayActivity attivita, DayPlan giorno)
    {
        var possibile = DriverDay.CanDo(giorno, attivita, career.Cash, out var rifiuto);
        var card = Scheda(out var dentro, !possibile);

        var testata = $"{attivita.Hours}h"
                      + (attivita.Cost > 0 ? $" · € {attivita.Cost:N0}" : "")
                      + (attivita.Actor == DayActor.Agent ? " · ore di Haru" : "")
                      + (attivita.IsCertain ? " · esito sicuro" : " · esito incerto");
        dentro.Controls.Add(Riga(testata.ToUpperInvariant(), UiTheme.Kicker,
            possibile ? UiTheme.Info : UiTheme.TextMuted, 2, Larghezza - 44));
        dentro.Controls.Add(Riga(attivita.Name, UiTheme.BodyStrong,
            possibile ? UiTheme.TextPrimary : UiTheme.TextMuted, 2, Larghezza - 44));
        dentro.Controls.Add(Riga(attivita.Promise, UiTheme.Small, UiTheme.TextSecondary, 8, Larghezza - 44));

        if (possibile)
            dentro.Controls.Add(Pulsanti("FALLO", () => Esegui(attivita), null, null));
        else
            dentro.Controls.Add(Riga(rifiuto, UiTheme.Small, UiTheme.Accent, 4, Larghezza - 44));
        return card;
    }

    /// <summary>
    /// Svolge un'attività e la racconta.
    ///
    /// Passa dallo stesso motore delle altre schermate: le ore, il denaro e gli
    /// effetti sono decisi in un posto solo, e questa finestra non ne conosce
    /// nessuno.
    /// </summary>
    private void Esegui(DayActivity attivita)
    {
        var report = DayEngine.Perform(career, DriverDay.EnsureToday(career), attivita);
        if (report.Refused)
        {
            CareerMessages.Show(this, report.Refusal, "CorsaCareer — non si può", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        save();
        if (onScene != null) onScene(report);
        else
        {
            using var esito = new DriverActivityResultDialog(career, report);
            esito.ShowDialog(this);
        }
        RefreshItems();
    }

    // -------------------------------------------------------------- mattoni

    private Panel Scheda(out FlowLayoutPanel dentro, bool spento)
    {
        var card = new Panel
        {
            Width = Larghezza, BackColor = spento ? UiTheme.Surface : UiTheme.SurfaceRaised,
            Padding = new Padding(16, 12, 16, 12), Margin = new Padding(0, 0, 0, 10),
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        var colonna = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = Larghezza - 40,
            BackColor = Color.Transparent, Margin = new Padding(0), Padding = new Padding(0)
        };
        card.Controls.Add(colonna);
        dentro = colonna;
        return card;
    }

    private Control Titolo(string testo)
    {
        var l = Riga(testo, UiTheme.HeadlineSmall, UiTheme.TextPrimary, 6);
        l.Margin = new Padding(0, 18, 0, 6);
        return l;
    }

    private Control Pulsanti(string primoTesto, Action primo, string? secondoTesto, Action? secondo)
    {
        var riga = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent,
            Margin = new Padding(0, 4, 0, 0), Padding = new Padding(0)
        };
        var a = UiTheme.PrimaryButton(primoTesto);
        a.Width = 300; a.Height = 34; a.Margin = new Padding(0, 0, 10, 0);
        a.Click += (_, _) => primo();
        riga.Controls.Add(a);
        if (secondoTesto != null && secondo != null)
        {
            var b = UiTheme.SecondaryButton(secondoTesto);
            b.Width = 130; b.Height = 34; b.Margin = new Padding(0);
            b.Click += (_, _) => secondo();
            riga.Controls.Add(b);
        }
        return riga;
    }

    /// <summary>
    /// Una riga di testo che si adatta a quanto testo contiene.
    ///
    /// Le etichette avevano un'altezza fissa di quarantadue pixel: una frase di
    /// tre righe veniva tagliata a metà, e nelle schede delle attività la
    /// promessa — cioè l'unica cosa che serve per decidere — spariva.
    /// </summary>
    private Label Riga(string testo, Font font, Color colore, int sotto, int larghezza = Larghezza)
    {
        var l = new Label
        {
            Text = testo, Font = font, ForeColor = colore, BackColor = Color.Transparent,
            Width = larghezza, AutoSize = false, UseMnemonic = false,
            Margin = new Padding(0, 0, 0, sotto), Padding = new Padding(0)
        };
        using var g = CreateGraphics();
        l.Height = (int)Math.Ceiling(g.MeasureString(testo, font, larghezza).Height) + 4;
        return l;
    }
}
