using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class NewspaperDialog : CareerDialog
{
    public NewspaperDialog(CareerState career, CareerEventRecord? story)
    {
        Text = "CorsaCareer — Pagina del giornale"; ClientSize = new Size(900, 700); StartPosition = FormStartPosition.CenterParent;
        var page = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 236, 213) }; Controls.Add(page);
        var background = AssetPaths.File("motorsport-paper-background.png");
        if (File.Exists(background)) { var image = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.StretchImage, Image = Image.FromFile(background) }; page.Controls.Add(image); image.SendToBack(); }
        var masthead = new Label { Text = "CORSA  |  WEEKEND MOTORSPORT", Left = 48, Top = 34, AutoSize = true, Font = new Font("Georgia", 24, FontStyle.Bold), ForeColor = Color.FromArgb(135, 20, 28), BackColor = Color.Transparent }; page.Controls.Add(masthead);
        var articleDate = story?.StoryDate != default ? story!.StoryDate : career.StoryDate;
        var date = new Label { Text = articleDate.ToString("dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT")), Left = 52, Top = 78, AutoSize = true, Font = new Font("Georgia", 10), ForeColor = Color.FromArgb(45, 45, 45), BackColor = Color.Transparent }; page.Controls.Add(date);
        var headline = new Label { Text = story?.Headline ?? "Nessun articolo reale disponibile", Left = 48, Top = 128, Width = 790, Height = 100, Font = new Font("Georgia", 28, FontStyle.Bold), ForeColor = Color.FromArgb(35, 35, 35), BackColor = Color.FromArgb(215, 243, 236, 213) }; page.Controls.Add(headline);
        var race = ResolveRace(career, story);
        var arc = career.StoryArcs.OrderByDescending(x => x.Importance).FirstOrDefault(x => x.Status == "In corso");
        var factualBody = BuildArticle(career, story, race, arc);
        var body = new RichTextBox { Text = factualBody, ReadOnly = true, BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.Vertical, Left = 52, Top = 250, Width = 420, Height = 330, Font = new Font("Georgia", 12), ForeColor = Color.FromArgb(45, 45, 45), BackColor = Color.FromArgb(243, 236, 213), DetectUrls = false }; page.Controls.Add(body);
        var imagePath = story != null && File.Exists(story.PhotoPath) ? story.PhotoPath : story == null ? AssetPaths.File("test-day-garage.png") : EditorialAssets.ForEvent(story);
        if (!File.Exists(imagePath)) imagePath = AssetPaths.File("test-day-garage.png");
        if (File.Exists(imagePath)) { var image = new PictureBox { Left = 515, Top = 260, Width = 330, Height = 230, SizeMode = PictureBoxSizeMode.Zoom, Image = Image.FromFile(imagePath), BorderStyle = BorderStyle.FixedSingle }; page.Controls.Add(image); }
        var caption = new Label { Text = PhotoSource.Caption(story?.PhotoPath ?? "", story?.Type ?? "").ToUpperInvariant(), Left = 515, Top = 500, Width = 330, Height = 32, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(135, 20, 28), BackColor = Color.FromArgb(220, 243, 236, 213), TextAlign = ContentAlignment.MiddleCenter }; page.Controls.Add(caption);
        var portraitPath = AssetPaths.File("fictional-driver-portrait.png");
        if (File.Exists(portraitPath)) { var portrait = new PictureBox { Left = 52, Top = 470, Width = 150, Height = 180, SizeMode = PictureBoxSizeMode.Zoom, Image = Image.FromFile(portraitPath) }; page.Controls.Add(portrait); }
        var footer = new Label { Text = "Archivio CorsaCareer — contenuti editoriali costruiti da eventi verificati", Left = 230, Top = 625, AutoSize = true, Font = new Font("Georgia", 9, FontStyle.Italic), ForeColor = Color.FromArgb(80, 80, 80), BackColor = Color.Transparent }; page.Controls.Add(footer);
    }
    public static string BuildArticle(CareerState career, CareerEventRecord? story, RaceHistoryEntry? race, StoryArcRecord? arc)
    {
        if (story == null) return "PRIMA PAGINA\n\nCompleta una gara reale per aprire il primo servizio del paddock. Quando Assetto Corsa registrerà il risultato, questa pagina raccoglierà cronaca, numeri, voci e conseguenze della giornata.\n\n\nArchivio editoriale\nNessun fatto viene inventato: il pezzo nasce dagli eventi salvati nella carriera.";
        var date = story.StoryDate == default ? career.StoryDate : story.StoryDate;
        var result = race == null ? "il risultato non è ancora collegato a un referto reale" : race.Dnf ? "il ritiro ha chiuso anzitempo la corsa" : $"il pilota ha chiuso in P{race.Position}, dalla casella P{race.StartingPosition}";
        var qualifying = race?.QualificationPosition > 0 ? $"In qualifica era partito da P{race.QualificationPosition};" : "La posizione di qualifica non è disponibile nel referto;";
        var pace = race?.BestLapMilliseconds > 0 ? $"il miglior giro è stato {FormatLap(race.BestLapMilliseconds)}" : "il miglior giro non è stato registrato";
        var teammate = race?.TeammatePosition > 0 ? $"il compagno {race.TeammateName} ha terminato in P{race.TeammatePosition}" : "il confronto con il compagno non è disponibile";
        var stops = race?.PitStops > 0 ? $"Sono state registrate {race.PitStops} soste ai box." : "Il referto non registra soste ai box.";
        var penalty = race?.PenaltySeconds > 0 ? $" Sul risultato pesa anche una penalità di {race.PenaltySeconds:0.0} secondi." : " Nessuna penalità è riportata nel referto.";
        var arcText = arc == null ? "Il paddock non ha ancora aperto un arco narrativo prioritario." : $"Il filo narrativo seguito dalla redazione è “{arc.Title}”. {arc.Summary}";
        var market = career.Offers.Count > 0 ? $"Nel mercato si contano {career.Offers.Count} proposte disponibili: ogni risultato modifica la percezione del pilota." : "Il mercato resta in osservazione, in attesa di un campione di risultati più ampio.";
        var contextTitle = race != null
            ? story.Type.Equals("FIRST_VICTORY", StringComparison.OrdinalIgnoreCase) ? "LA PRIMA VITTORIA"
            : story.Type.Equals("FIRST_PODIUM", StringComparison.OrdinalIgnoreCase) ? "IL PRIMO PODIO"
            : story.Type.Equals("MILESTONE_RACE", StringComparison.OrdinalIgnoreCase) ? "IL TRAGUARDO DELLA CARRIERA"
            : "IL GIORNO DELLA GARA"
            : story.Type.Equals("TRACK_TEST", StringComparison.OrdinalIgnoreCase) ? "GIORNATA DI TEST" : story.Type.Equals("CONTRACT_SIGNING", StringComparison.OrdinalIgnoreCase) ? "LA FIRMA NEL PADDOCK" : story.Type.Equals("CONTRACT_EXPIRED", StringComparison.OrdinalIgnoreCase) ? "IL MERCATO SI RIAPRE" : story.Type.Equals("PADDOCK_ROSTER", StringComparison.OrdinalIgnoreCase) ? "APERTURA DEL PADDOCK" : "DIARIO DAL PADDOCK";
        var contextLead = race != null
            ? $"{career.Driver}, con {career.Team} e la {UiText.Car(career.Car)}, arriva al weekend con un obiettivo semplice: trasformare il lavoro preparatorio in un riferimento cronometrico credibile."
            : $"Questo servizio documenta un evento di tipo {story.Type}: {story.Headline} Nel box {career.Team}, {career.Driver} prepara il prossimo capitolo con la {UiText.Car(career.Car)}. Il referto di gara non è ancora disponibile e quindi non vengono attribuiti risultati.";
        var raceFacts = race == null
            ? "Non è presente un referto di gara: posizione, punti, premio e bonus non vengono inventati."
            : $"A fine sessione, {result}. {qualifying} {pace}. {stops}{penalty}";
        var finances = race == null
            ? $"L’evento non assegna punti o premi. Il bilancio verificato della carriera resta di {career.Points} punti, {career.Wins} vittorie e {career.Podiums} podi."
            : $"La gara vale {race.Points} punti, €{race.Prize:N0} di premio e €{race.SponsorBonus:N0} di bonus sponsor. Il bilancio della carriera ora è di {career.Points} punti, {career.Wins} vittorie e {career.Podiums} podi.";
        return $"{date:dd MMMM yyyy}  |  {story.Track.ToUpperInvariant()}\n\n{contextTitle}\n{contextLead} La giornata non si misura soltanto con la posizione finale, ma anche con il lavoro nel box.\n\nCRONACA\n{raceFacts}\n\nIL CONFRONTO NEL BOX\nNel garage vicino, {teammate}. È il dato che ingegneri e sponsor guarderanno per primi: non una sentenza, ma il punto di partenza per il prossimo test.\n\nTECNICA E CONTABILITÀ\n{finances}\n\nPADDOCK E MERCATO\n{market} Il contratto con {career.Team} chiede: {career.ContractObjective}.\n\nIL RACCONTO PIÙ AMPIO\n{arcText}\n\nCHIUSA\nIl prossimo capitolo non è ancora scritto. Lo decideranno il prossimo test, il rapporto con gli ingegneri e il prossimo semaforo verde.\n\nServizio originale di {career.Journalist.Name}, {career.Journalist.Publication}. Testo redazionale generato dai fatti salvati della carriera.";
    }
    public static RaceHistoryEntry? ResolveRace(CareerState career, CareerEventRecord? story)
    {
        if (story == null || !IsRaceEvent(story.Type)) return null;
        var candidates = career.RaceHistory.Where(x => x.Track.Equals(story.Track, StringComparison.OrdinalIgnoreCase)).ToList();
        if (candidates.Count == 0) return null;
        // DateUtc is recorded at import time alongside the event.  Prefer the
        // closest referto instead of blindly taking the last race on this track.
        if (story.DateUtc != default)
            return candidates.OrderBy(x => Math.Abs((x.DateUtc - story.DateUtc).Ticks)).First();
        return candidates[^1];
    }
    private static bool IsRaceEvent(string type) => type.Equals("RACE_FINISHED", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_OR_NEXT_VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("VICTORY", StringComparison.OrdinalIgnoreCase)
        || type.Equals("FIRST_PODIUM", StringComparison.OrdinalIgnoreCase)
        || type.Equals("PODIUM", StringComparison.OrdinalIgnoreCase)
        || type.Equals("MILESTONE_RACE", StringComparison.OrdinalIgnoreCase)
        || type.Equals("RETIREMENT", StringComparison.OrdinalIgnoreCase);
    private static string FormatLap(int ms) => $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}";
}
