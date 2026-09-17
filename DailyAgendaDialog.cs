using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// La giornata per esteso: le stesse fasce del portale, con lo spazio per
/// leggere che cosa promette ogni cosa prima di sceglierla.
///
/// Questa schermata e quella della Home devono raccontare lo stesso giorno, e
/// per un po' non lo hanno fatto: qui la scuola durava dalle otto a mezzogiorno
/// e le ore libere erano un monte ore, là la scuola finiva alle tre e le ore
/// erano divise in fasce. Due descrizioni della stessa giornata sono sempre una
/// di troppo, e quella sbagliata la si scopre solo quando qualcuno ci gioca.
///
/// Adesso la fonte è una sola — <see cref="DaySlots"/> — e questa schermata è
/// soltanto un modo più largo di guardarla: stesso ordine, stesse fasce, ma con
/// la promessa di ogni attività scritta per intero invece che in un
/// suggerimento che compare passandoci sopra.
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

    private const int Larghezza = 820;

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
        Width = 940; Height = 760;
        Controls.Add(flow);
        Ricostruisci();
    }

    private int Eta => career.BirthYear <= 0 ? 12 : Math.Max(10, career.StoryDate.Year - career.BirthYear);

    private void Ricostruisci()
    {
        flow.SuspendLayout();
        foreach (Control c in flow.Controls) c.Dispose();
        flow.Controls.Clear();

        var giorno = DriverDay.EnsureToday(career);
        var data = career.StoryDate.ToString("dddd d MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
        var festa = CalendarioGiapponese.Festa(career.StoryDate);

        flow.Controls.Add(Riga(data.ToUpperInvariant() + (festa != null ? "  ·  " + festa.ToUpperInvariant() : ""),
            UiTheme.Kicker, UiTheme.Warning, 8));

        if (CalendarioGiapponese.PaeseFermo(career.StoryDate))
            flow.Controls.Add(Riga(
                "Il paese è fermo: circuiti e officine chiusi. Sono i giorni di capodanno e dell'Obon, e non si corre — "
                + "ma il tempo passa lo stesso, e quello che si fa a casa conta.",
                UiTheme.Prose, UiTheme.Accent, 12));

        // La soglia e' Scuola.Riguarda, non un diciassette scritto qui a mano:
        // erano due numeri diversi (17 contro Scuola.EtaDiFine, sedici) per la
        // stessa domanda, e un pilota di sedici o diciassette anni vedeva
        // ancora il livello scolastico anche se il vincolo era gia' finito.
        if (Scuola.Riguarda(career))
            flow.Controls.Add(Riga(
                career.RepeatingYear
                    ? $"{Scuola.Etichetta(career)} — stai ripetendo l'anno: il recupero arriva fino alle diciotto e ti mangia la prima fascia del pomeriggio."
                    : Scuola.ARischio(career)
                        ? $"{Scuola.Etichetta(career)} — sotto {Scuola.SogliaDiPromozione}, e a giugno si ripete l'anno."
                        : $"{Scuola.Etichetta(career)} — sei sopra la soglia. A giugno serve almeno {Scuola.SogliaDiPromozione}.",
                UiTheme.Prose,
                career.RepeatingYear || Scuola.ARischio(career) ? UiTheme.Accent : UiTheme.TextSecondary,
                14));

        Colonna("LA TUA GIORNATA", DaySlots.Pilota(career.StoryDate, Eta, career.RepeatingYear),
            giorno.FascePilota, DayActor.Driver, giorno);
        Colonna("LA GIORNATA DI HARU", DaySlots.Haru(career.StoryDate),
            giorno.FasceHaru, DayActor.Agent, giorno);

        var chiudi = UiTheme.SecondaryButton("TORNA AL PORTALE");
        chiudi.Dock = DockStyle.None; chiudi.Width = 260; chiudi.Height = 42;
        chiudi.Margin = new Padding(0, 20, 0, 0);
        chiudi.Click += (_, _) => Close();
        flow.Controls.Add(chiudi);
        flow.ResumeLayout();
    }

    private void Colonna(string titolo, IReadOnlyList<FasciaDelGiorno> fasce, List<string> occupate,
                         DayActor chi, DayPlan giorno)
    {
        flow.Controls.Add(Titolo(titolo));
        for (var i = 0; i < fasce.Count; i++)
            flow.Controls.Add(SchedaFascia(fasce[i], i, occupate, chi, giorno));
    }

    private Control SchedaFascia(FasciaDelGiorno fascia, int indice, List<string> occupate,
                                 DayActor chi, DayPlan giorno)
    {
        var gia = indice < occupate.Count ? occupate[indice] : "";
        var card = Scheda(out var dentro, fascia.Fissa || gia.Length > 0);

        if (fascia.Fissa)
        {
            dentro.Controls.Add(Riga($"{fascia.Orario} · {fascia.Nome.ToUpperInvariant()}", UiTheme.Kicker, UiTheme.TextMuted, 2, Larghezza - 44));
            dentro.Controls.Add(Riga("Obbligatoria: non si sceglie. È la ragione per cui il pomeriggio è corto.",
                UiTheme.Small, UiTheme.TextSecondary, 0, Larghezza - 44));
            return card;
        }

        if (gia.Length > 0)
        {
            dentro.Controls.Add(Riga($"{fascia.Orario} · {gia.ToUpperInvariant()}", UiTheme.Kicker, UiTheme.Positive, 2, Larghezza - 44));
            dentro.Controls.Add(Riga("Fatto. Questa fascia della giornata è passata.", UiTheme.Small, UiTheme.TextSecondary, 0, Larghezza - 44));
            return card;
        }

        // Un impegno già in agenda occupa la sua fascia: ha un nome, un motivo
        // e un prezzo se lo si salta.
        if (chi == DayActor.Driver)
        {
            var impegno = LifeCalendar.Today(career, content)
                .FirstOrDefault(x => x.Status is "planned" or "active"
                                     && OraDiInizio(x.StartTime) >= fascia.Dalle
                                     && OraDiInizio(x.StartTime) < fascia.Alle);
            if (impegno != null)
            {
                dentro.Controls.Add(Riga($"{fascia.Orario} · {(impegno.Required ? "OBBLIGATORIO" : "IN AGENDA")}",
                    UiTheme.Kicker, impegno.Required ? UiTheme.Warning : UiTheme.Info, 2, Larghezza - 44));
                dentro.Controls.Add(Riga(impegno.Title, UiTheme.BodyStrong, UiTheme.TextPrimary, 2, Larghezza - 44));
                dentro.Controls.Add(Riga(impegno.Detail + (impegno.TrackName.Length > 0 ? "\nLuogo: " + impegno.TrackName : ""),
                    UiTheme.Small, UiTheme.TextSecondary, 8, Larghezza - 44));
                dentro.Controls.Add(Pulsanti(
                    impegno.Kind switch
                    {
                        "track-training" => "APRI L'ALLENAMENTO",
                        "sponsor-visit" => "VAI DALLO SPONSOR",
                        "fitness" => "ALLENATI",
                        "recovery" => "RIPOSA",
                        _ => "FREQUENTA"
                    },
                    () =>
                    {
                        if (impegno.Kind == "track-training")
                        {
                            impegno.Status = "active"; save(); Close(); launchTraining();
                            return;
                        }
                        LifeCalendar.Complete(career, impegno); save(); Ricostruisci();
                    },
                    "SALTA", () => { LifeCalendar.Skip(career, impegno); save(); Ricostruisci(); }));
                return card;
            }
        }

        var possibili = (chi == DayActor.Agent ? DayActivityCatalog.ForAgent() : DayActivityCatalog.ForDriver())
            .Where(x => x.Hours <= fascia.Ore)
            .Where(x => DriverDay.CanDo(giorno, x, career.Cash, out _))
            .ToList();

        dentro.Controls.Add(Riga($"{fascia.Orario} · LIBERA", UiTheme.Kicker,
            chi == DayActor.Agent ? UiTheme.Info : UiTheme.TextPrimary, 6, Larghezza - 44));

        if (possibili.Count == 0)
        {
            dentro.Controls.Add(Riga("Niente che ci stia dentro: le ore non bastano, i soldi non bastano, o l'hai già fatto oggi.",
                UiTheme.Small, UiTheme.TextMuted, 0, Larghezza - 44));
            return card;
        }

        foreach (var attivita in possibili)
        {
            var testa = $"{attivita.Name}  ·  {attivita.Hours}h"
                        + (attivita.Cost > 0 ? $"  ·  € {attivita.Cost:N0}" : "")
                        + (attivita.IsCertain ? "  ·  esito sicuro" : "  ·  esito incerto");
            var b = UiTheme.SecondaryButton(testa);
            b.Dock = DockStyle.None; b.Width = Larghezza - 44; b.Height = 30;
            b.Font = UiTheme.Small; b.Margin = new Padding(0, 0, 0, 2);
            var scelta = attivita;
            b.Click += (_, _) => Esegui(scelta, indice, occupate);
            dentro.Controls.Add(b);
            dentro.Controls.Add(Riga(attivita.Promise, UiTheme.Small, UiTheme.TextMuted, 8, Larghezza - 52));
        }
        return card;
    }

    private void Esegui(DayActivity attivita, int indice, List<string> occupate)
    {
        var report = DayEngine.Perform(career, DriverDay.EnsureToday(career), attivita);
        if (report.Refused)
        {
            CareerMessages.Show(this, report.Refusal, "CorsaCareer — non si può", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        while (occupate.Count <= indice) occupate.Add("");
        occupate[indice] = attivita.Name;
        save();
        if (onScene != null) onScene(report);
        else
        {
            using var esito = new DriverActivityResultDialog(career, report);
            esito.ShowDialog(this);
        }
        Ricostruisci();
    }

    // -------------------------------------------------------------- mattoni

    private static int OraDiInizio(string orario) =>
        int.TryParse((orario ?? "").Split(':').FirstOrDefault(), out var ora) ? ora : -1;

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
        a.Dock = DockStyle.None; a.Width = 320; a.Height = 34; a.Margin = new Padding(0, 0, 10, 0);
        a.Click += (_, _) => primo();
        riga.Controls.Add(a);
        if (secondoTesto != null && secondo != null)
        {
            var b = UiTheme.SecondaryButton(secondoTesto);
            b.Dock = DockStyle.None; b.Width = 130; b.Height = 34; b.Margin = new Padding(0);
            b.Click += (_, _) => secondo();
            riga.Controls.Add(b);
        }
        return riga;
    }

    /// <summary>
    /// Una riga che si adatta a quanto testo contiene. Le etichette avevano
    /// un'altezza fissa e una promessa di tre righe veniva tagliata a metà —
    /// cioè spariva proprio l'unica cosa che serve per decidere.
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
