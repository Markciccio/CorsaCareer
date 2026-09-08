using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Regia unica della carriera. Non introduce dati nuovi: mette nello stesso
/// luogo storia, gara, persone, contratto, sponsor e collegamento ad AC, così
/// il giocatore capisce sia dove si trova sia qual è il prossimo gesto.
/// </summary>
public sealed class CareerHubDialog : CareerDialog
{
    public CareerHubDialog(
        CareerState career,
        ScheduledEvent? next,
        bool awaitingResult,
        string contentStatus,
        string primaryAction,
        Action continueStory,
        Action openCalendar,
        Action openMarket,
        Action openPaddock,
        Action openActivities,
        Action refreshContents,
        Action openCareerManager,
        string careersDirectory)
    {
        Text = "CorsaCareer — centro carriera";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1360, 820);
        MinimumSize = new Size(980, 650);
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var phase = CareerPhases.Current(career);
        var header = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = UiTheme.HeaderBackground, Padding = new Padding(28, 14, 28, 12) };
        header.Controls.Add(new Label { Text = $"{phase.Flash}  ·  {career.StoryDate.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"))}", Dock = DockStyle.Top, Height = 23, Font = UiTheme.Kicker, ForeColor = UiTheme.Accent });
        header.Controls.Add(new Label { Text = phase.Title, Dock = DockStyle.Bottom, Height = 42, Font = UiTheme.HeadlineSmall, ForeColor = UiTheme.TextPrimary });

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = UiTheme.HeaderBackground, Padding = new Padding(28, 14, 28, 14) };
        var primary = UiTheme.PrimaryButton(primaryAction);
        primary.Dock = DockStyle.Right; primary.Width = 330;
        primary.Click += (_, _) => { Hide(); continueStory(); };
        footer.Controls.Add(new Label
        {
            Text = awaitingResult
                ? "SESSIONE IN ATTESA · importa il referto reale oppure usa il debug dichiarato"
                : "Ogni scelta qui cambia il contesto della carriera; le prestazioni arrivano solo dal referto o dal debug dichiarato.",
            Dock = DockStyle.Fill, Font = UiTheme.Small, ForeColor = awaitingResult ? UiTheme.Warning : UiTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft
        });
        footer.Controls.Add(primary);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, BackColor = UiTheme.Background, Padding = new Padding(24, 18, 24, 14) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 22));

        var story = Card("IL PROSSIMO PASSO", UiTheme.Accent, out var storyBody);
        storyBody.Controls.Add(TextBlock(phase.Stake, UiTheme.Small, UiTheme.TextSecondary, DockStyle.Bottom, 52));
        storyBody.Controls.Add(TextBlock(phase.Objective, UiTheme.ProseStrong, UiTheme.TextPrimary, DockStyle.Fill, 0));
        storyBody.Controls.Add(TextBlock("OBIETTIVO IMMEDIATO", UiTheme.Kicker, UiTheme.Positive, DockStyle.Top, 25));
        body.Controls.Add(story, 0, 0);

        var nextCard = Card("GARA E CALENDARIO", UiTheme.Warning, out var nextBody);
        var nextText = next == null
            ? "Nessun appuntamento fissato. Il prossimo evento nascerà dall'esito o dalla scelta attuale."
            : $"{When(next.Date, career.StoryDate)} · {Label(next)}\n{NarrativeEngine.Capitalize(next.TrackName)}\n\n{next.Objective}\n\n{next.GeneratedBy}";
        nextBody.Controls.Add(Button("APRI IL CALENDARIO", openCalendar, DockStyle.Bottom));
        nextBody.Controls.Add(TextBlock(nextText, UiTheme.Body, UiTheme.TextPrimary, DockStyle.Fill, 0));
        body.Controls.Add(nextCard, 1, 0);

        var ac = Card("ASSETTO CORSA · SESSIONE", UiTheme.Info, out var acBody);
        var acText = awaitingResult
            ? "La sessione è pronta. Il prossimo referto associato deciderà i punti, il denaro, il mercato e la reazione dei personaggi."
            : string.IsNullOrWhiteSpace(contentStatus)
                ? "Contenuti pronti. La sessione successiva può essere preparata in Content Manager."
                : $"{contentStatus}\n\nPer sviluppare qui senza Assetto Corsa, prepara comunque la sessione e poi usa DEBUG · SIMULA SENZA AC: resterà marcata nello storico.";
        acBody.Controls.Add(Button("AGGIORNA CONTENUTI", refreshContents, DockStyle.Bottom));
        acBody.Controls.Add(TextBlock(acText, UiTheme.Body, awaitingResult ? UiTheme.Warning : UiTheme.TextSecondary, DockStyle.Fill, 0));
        body.Controls.Add(ac, 2, 0);

        var team = Card("SQUADRA E PERSONE", UiTheme.Info, out var teamBody);
        var cast = career.StoryCast ?? [];
        var mentor = cast.FirstOrDefault(x => x.Id == StoryCastService.Mechanic)?.Name ?? "Gianni Valli";
        var rival = cast.FirstOrDefault(x => x.Id == StoryCastService.Rival)?.Name ?? "Nico Valenti";
        teamBody.Controls.Add(Button("PADDOCK E RELAZIONI", openPaddock, DockStyle.Bottom));
        teamBody.Controls.Add(TextBlock(
            $"Team: {career.Team}\nCompagno: {career.Teammate}\nFiducia team: {career.TeamRelation}/100\n\nMentore: {mentor}\nRivale: {rival}\n\nCondizione: stanchezza {career.Fatigue}/100",
            UiTheme.Small, UiTheme.TextPrimary, DockStyle.Fill, 0));
        body.Controls.Add(team, 0, 1);

        var money = Card("SPONSOR E BUDGET", UiTheme.Positive, out var moneyBody);
        moneyBody.Controls.Add(Button("AGENDA E PREPARAZIONE", openActivities, DockStyle.Bottom));
        moneyBody.Controls.Add(TextBlock(
            $"Cassa: € {career.Cash:N0}\n{CareerFinances.Status(career.Cash)}\n\nSponsor: {career.Sponsor}\nRapporto sponsor: {career.SponsorRelation}/100\nPremi: € {career.PrizeMoney:N0} · costi: € {career.RepairCosts + career.LogisticsCosts:N0}",
            UiTheme.Small, UiTheme.TextPrimary, DockStyle.Fill, 0));
        body.Controls.Add(money, 1, 1);

        var market = Card("INGAGGI E PROPOSTE", UiTheme.Warning, out var marketBody);
        var openOffers = (career.Opportunities ?? []).Count(x => x.IsOpen);
        marketBody.Controls.Add(Button("MERCATO E SCOUTING", openMarket, DockStyle.Bottom));
        marketBody.Controls.Add(TextBlock(
            career.ContractActive
                ? $"Contratto: {career.Team}\nDurata: {career.ContractYears} anno/i · stipendio € {career.ContractSalary:N0}\nObiettivo: {career.ContractObjective}\n\nProposte aperte: {openOffers}"
                : $"Nessun contratto firmato.\nOfferte iniziali: {career.Offers.Count}\nProposte aperte: {openOffers}\n\nUna firma definisce calendario, compagno, auto, sponsor e aspettative.",
            UiTheme.Small, UiTheme.TextPrimary, DockStyle.Fill, 0));
        body.Controls.Add(market, 2, 1);

        var route = Card("LA STRADA", UiTheme.TextMuted, out var routeBody);
        var routeLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
        };
        routeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        routeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var routeArt = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = UiTheme.SurfaceRaised, Margin = new Padding(0, 0, 18, 0) };
        var routeArtPath = AssetPaths.File("manga-01-rookie-dawn.png");
        if (File.Exists(routeArtPath))
        {
            using var source = Image.FromFile(routeArtPath);
            routeArt.Image = new Bitmap(source);
        }
        routeLayout.Controls.Add(routeArt, 0, 0);
        routeLayout.Controls.Add(TextBlock(Route(career), UiTheme.Body, UiTheme.TextSecondary, DockStyle.Fill, 0), 1, 0);
        routeBody.Controls.Add(routeLayout);
        body.Controls.Add(route, 0, 2); body.SetColumnSpan(route, 2);

        // La gestione dei salvataggi vive qui dentro, non dietro un secondo
        // pulsante in barra che diceva quasi la stessa cosa: «centro carriera»
        // e «carriere» erano due voci indistinguibili per chi legge, e
        // costringevano a ricordare quale delle due contenesse cosa.
        var salvataggi = Card("LE TUE CARRIERE", UiTheme.Info, out var salvataggiBody);
        salvataggiBody.Controls.Add(Button("GESTISCI LE CARRIERE", openCareerManager, DockStyle.Bottom));
        var archiviate = ContaArchiviate(careersDirectory);
        salvataggiBody.Controls.Add(TextBlock(
            $"In corso: {(string.IsNullOrWhiteSpace(career.Driver) ? "senza nome" : career.Driver)} · "
            + $"{career.Races} gare · {career.Wins} vittorie\n"
            + (archiviate == 0
                ? "Nessuna carriera messa da parte."
                : $"Messe da parte: {archiviate}.")
            + "\n\nDa qui puoi cominciarne una nuova, riprendere una vecchia o fare pulizia. "
            + "Quella in corso viene sempre archiviata prima di essere sostituita.",
            UiTheme.Small, UiTheme.TextSecondary, DockStyle.Fill, 0));
        body.Controls.Add(salvataggi, 2, 2);

        Controls.Add(body); Controls.Add(footer); Controls.Add(header);
    }

    /// <summary>Quante carriere sono messe da parte, senza aprirle.</summary>
    private static int ContaArchiviate(string cartella)
    {
        try { return Directory.Exists(cartella) ? Directory.GetFiles(cartella, "*.json").Length : 0; }
        catch { return 0; }
    }

    private static Control Card(string label, Color accent, out Panel content)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Margin = new Padding(0, 0, UiTheme.Gutter, UiTheme.Gutter), Padding = new Padding(14, 12, 14, 12) };
        card.Paint += (_, e) => { using var pen = new Pen(accent); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
        var title = new Label { Text = label, Dock = DockStyle.Top, Height = 24, Font = UiTheme.Kicker, ForeColor = accent };
        content = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
        card.Controls.Add(content); card.Controls.Add(title);
        return card;
    }

    private static Label TextBlock(string text, Font font, Color color, DockStyle dock, int height) => new()
    {
        Text = text, Font = font, ForeColor = color, Dock = dock, Height = height, AutoEllipsis = false, UseMnemonic = false
    };

    private static Button Button(string text, Action action, DockStyle dock)
    {
        var button = UiTheme.SecondaryButton(text); button.Dock = dock; button.Height = 34; button.Click += (_, _) => action(); return button;
    }

    private static string When(DateTime date, DateTime today)
    {
        var days = (date.Date - today.Date).Days;
        return days switch { <= 0 => "OGGI", 1 => "DOMANI", < 7 => $"TRA {days} GIORNI", _ => date.ToString("d MMMM", CultureInfo.GetCultureInfo("it-IT")).ToUpperInvariant() };
    }

    private static string Label(ScheduledEvent next) => next.Kind switch
    {
        ScheduledEventKind.EvaluationTest => "TEST DI VALUTAZIONE",
        ScheduledEventKind.ConfirmationTest => "TEST DI CONFERMA",
        ScheduledEventKind.Invitation => "GARA SU INVITO",
        _ => "ROUND DI CAMPIONATO"
    };

    private static string Route(CareerState career)
    {
        var current = CareerPhases.Current(career).Id;
        string Mark(string id, string label) => CareerPhases.RankOf(current) >= CareerPhases.RankOf(id) ? $"● {label}" : $"○ {label}";
        return string.Join("     ", new[]
        {
            Mark(CareerPhases.Debut, "ROOKIE TEST"),
            Mark(CareerPhases.Contract, "PRIMO SEDILE"),
            Mark(CareerPhases.Climb, "PROMOZIONE"),
            Mark(CareerPhases.Winner, "PRIMA VITTORIA"),
            Mark(CareerPhases.Professional, "PROFESSIONISMO"),
            Mark(CareerPhases.Reference, "TITOLO / RIFERIMENTO")
        });
    }
}
