using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Mappa iniziale della carriera. Ogni riga è un livello economico/sportivo e
/// contiene tutte le auto che la cartella reale di Assetto Corsa mette a
/// disposizione: i vuoti restano visibili, perché raccontano una carriera
/// possibile ma non ancora percorribile con i contenuti installati.
/// </summary>
public sealed class InstalledCareerAnalysisDialog : CareerDialog
{
    private readonly ContentIndexRecord index;
    private readonly Label choice = new() { AutoSize = false, Height = 42, ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
    public string SelectedPath { get; private set; } = "";
    public bool ContentChanged { get; private set; }

    public InstalledCareerAnalysisDialog(ContentIndexRecord index, ContentCarRecord? suggestedStart, string currentPath = "")
    {
        this.index = index;
        Text = "CorsaCareer — Mappa della carriera";
        ClientSize = new Size(1420, 940);
        BackColor = Color.FromArgb(15, 18, 25);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        Entrance = SceneEntrance.Curtain;
        SelectedPath = currentPath ?? "";

        var cars = index.Cars.Where(ContentCategoryRules.IsRaceable).ToList();
        Controls.Add(new Label { Text = "LA TUA MAPPA DI CARRIERA", Left = 38, Top = 25, Width = 900, Height = 42, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = $"Contenuto realmente trovato: {cars.Count} auto utilizzabili · {index.Tracks.Count} circuiti/layout. Ogni vettura compare in una classe; i dati stimati restano indicati nella revisione contenuti.", Left = 40, Top = 72, Width = 1320, Height = 28, ForeColor = Color.FromArgb(170, 180, 195) });

        var start = suggestedStart == null ? "Nessuna auto utilizzabile: la carriera resta in attesa." : $"INIZIO CONSIGLIATO  ·  {suggestedStart.Name}  ·  {CareerLadder.ForCar(suggestedStart.Category, suggestedStart.PowerHp, suggestedStart.MassKg).Name}";
        Controls.Add(new Label { Text = start, Left = 40, Top = 113, Width = 1320, Height = 30, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = suggestedStart == null ? Color.OrangeRed : Color.FromArgb(60, 215, 145) });

        Controls.Add(new Label { Text = "SCEGLI LA CARRIERA CHE PREFERISCI", Left = 40, Top = 162, Width = 700, Height = 25, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        var formula = CareerPanel("KART → MONOPOSTO", "Kart a quattro tempi, due tempi, cambio: poi Formula 4, Formula 3 e il vertice. È una carriera distinta.", cars, true);
        formula.Left = 38; formula.Top = 196; Controls.Add(formula);
        var closed = CareerPanel("UTILITARIE → TURISMO · GT · ENDURANCE", "Track day, trofei e turismo prima di GT4, GT3, prototipi e mondiale endurance. È una carriera distinta.", cars, false);
        closed.Left = 714; closed.Top = 196; Controls.Add(closed);

        choice.Left = 40; choice.Top = 820; choice.Width = 780;
        Controls.Add(choice);
        Controls.Add(ChoiceButton("INIZIO CON KART → MONOPOSTO", 845, 225, () => Select("SingleSeater", "Hai scelto KART → MONOPOSTO. La rookie evaluation partirà dal kart installato più adatto; Formula sarà la tua direzione.")));
        Controls.Add(ChoiceButton("INIZIO CON UTILITARIE → TURISMO", 1085, 285, () => Select("ClosedWheel", "Hai scelto UTILITARIE → TURISMO. La rookie evaluation partirà dall’auto stradale/track day installata più adatta.")));
        var continueButton = new Button { Text = "CONFERMA E CONTINUA", Left = 1120, Top = 875, Width = 258, Height = 38, BackColor = Color.FromArgb(224, 24, 58), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        continueButton.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        Controls.Add(continueButton);
        Select(SelectedPath, string.IsNullOrWhiteSpace(SelectedPath) ? "Scegli adesso: la prima auto e i primi test seguiranno questa carriera. Più avanti sponsor e team potranno proporti un cambio di specialità, che potrai accettare o rifiutare." : "Direzione già scelta: puoi confermarla o cambiarla qui. Sponsor e team potranno comunque proporti un passaggio all’altra carriera.");
    }

    private Panel CareerPanel(string title, string subtitle, List<ContentCarRecord> cars, bool formula)
    {
        var panel = new Panel { Width = 644, Height = 600, BackColor = Color.FromArgb(20, 24, 33), BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(new Label { Text = title, Left = 16, Top = 13, Width = 440, Height = 26, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        panel.Controls.Add(new Label { Text = subtitle, Left = 16, Top = 43, Width = 440, Height = 36, ForeColor = Color.Gainsboro });
        var cover = new PictureBox { Left = 485, Top = 12, Width = 140, Height = 67, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(28, 33, 44), Image = TryImage(formula ? "campionato-formula-minore.jpg" : "campionato-endurance.jpg") };
        panel.Controls.Add(cover);
        var rows = new FlowLayoutPanel { Left = 12, Top = 86, Width = 616, Height = 502, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(20, 24, 33) };
        var levels = formula
            ? new[] { (1, "LIVELLO 1 · KART 4 TEMPI"), (2, "LIVELLO 2 · KART 2 TEMPI"), (3, "LIVELLO 3 · KART CON CAMBIO"), (4, "LIVELLO 4 · FORMULA D’INGRESSO"), (5, "LIVELLO 5 · FORMULA 3 / NAZIONALE"), (6, "LIVELLO 6 · FORMULA 2 / INTERNAZIONALE"), (7, "LIVELLO 7 · FORMULA 1") }
            : new[] { (1, "LIVELLO 1 · UTILITARIE / TRACK DAY"), (2, "LIVELLO 2 · TROFEO CLUB"), (3, "LIVELLO 3 · TURISMO REGIONALE"), (4, "LIVELLO 4 · GT4 / TROFEI / TURISMO"), (5, "LIVELLO 5 · GT3 / GT2 / TURISMO ALTO"), (6, "LIVELLO 6 · PROTOTIPI"), (7, "LIVELLO 7 · MONDIALE ENDURANCE / HYPERCAR") };
        foreach (var (level, name) in levels)
        {
            var exact = cars.Where(x => HasCarForBranchLevel(x, formula, level)).OrderBy(x => x.Name).ToList();
            var present = exact.Count > 0 ? exact : NearbyEquivalents(cars, formula, level);
            var isEquivalent = exact.Count == 0 && present.Count > 0;
            var manual = present.Count == 0 ? new ManualAssignment(formula, level, name) : null;
            var note = present.Count == 0 ? MissingMessage(formula, level)
                : isEquivalent ? "Nessuna auto perfettamente classificata: qui usiamo un equivalente installato dello stesso ramo." : "";
            rows.Controls.Add(LevelCard(name, note, present, 585, present.Count == 0 ? MissingOptions(formula, level) : [], manual));
        }
        panel.Controls.Add(rows);
        return panel;
    }

    private Panel SharedJourneyPanel(List<ContentCarRecord> cars)
    {
        var panel = new Panel { Width = 1340, Height = 175, BackColor = Color.FromArgb(24, 31, 42), BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(new Label { Text = "TRATTO COMUNE · DUE GAVETTE, DUE SBOCCHI", Left = 16, Top = 12, Width = 780, Height = 25, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        panel.Controls.Add(new Label { Text = "Il kart è propedeutico alle monoposto; utilitarie e track day portano al turismo. Le miniature sono le immagini disponibili nella cartella di ogni auto.", Left = 16, Top = 41, Width = 1280, Height = 22, ForeColor = Color.Gainsboro });
        var kart = cars.Where(IsKart).OrderBy(x => CareerLadder.ForCar(x.Category, x.PowerHp, x.MassKg).Step).ThenBy(x => x.Name).ToList();
        var road = cars.Where(IsUtility).OrderBy(x => x.Name).ToList();
        var kartCard = LevelCard("KART → MONOPOSTO · LIVELLI 1–3", "Rental/4T → 2T → cambio: da qui si passa alle Formula.", kart, 775);
        kartCard.Left = 15; kartCard.Top = 72; panel.Controls.Add(kartCard);
        var roadCard = LevelCard("UTILITARIE / TRACK DAY → TURISMO", "La base della carriera turismo, trofei e GT.", road, 515);
        roadCard.Left = 810; roadCard.Top = 72; panel.Controls.Add(roadCard);
        return panel;
    }

    private sealed record MissingSuggestion(string Car, string Label, string Url, string Note = "");
    private sealed record ManualAssignment(bool Formula, int Level, string Title);

    private Panel LevelCard(string title, string note, List<ContentCarRecord> cars, int width, IReadOnlyList<MissingSuggestion>? suggestions = null, ManualAssignment? manual = null)
    {
        suggestions ??= [];
        var columns = Math.Max(1, (width - 24) / 135);
        var rows = Math.Max(1, (int)Math.Ceiling(cars.Count / (double)columns));
        var suggestionRows = suggestions.Sum(suggestion => string.IsNullOrWhiteSpace(suggestion.Note) ? 1 : 2);
        var height = Math.Max(70, 46 + rows * 64 + suggestionRows * 21 + (manual == null ? 0 : 35));
        var card = new Panel { Width = width, Height = height, Margin = new Padding(3, 3, 3, 7), BackColor = Color.FromArgb(34, 41, 55), BorderStyle = BorderStyle.FixedSingle };
        card.Controls.Add(new Label { Text = title, Left = 12, Top = 8, Width = width - 24, Height = 21, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        if (!string.IsNullOrWhiteSpace(note)) card.Controls.Add(new Label { Text = note, Left = 12, Top = 29, Width = width - 24, Height = 18, ForeColor = Color.FromArgb(170, 180, 195) });
        if (cars.Count == 0)
            card.Controls.Add(new Label { Text = "— nessuna auto installata —", Left = 12, Top = 48, Width = width - 24, Height = 18, ForeColor = Color.FromArgb(140, 145, 155) });
        else
        {
            var gallery = new FlowLayoutPanel { Left = 10, Top = 47, Width = width - 20, Height = rows * 63, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = Color.FromArgb(34, 41, 55) };
            foreach (var car in cars) gallery.Controls.Add(CarTile(car));
            card.Controls.Add(gallery);
        }
        var suggestionTop = height - suggestionRows * 21;
        for (var suggestionIndex = 0; suggestionIndex < suggestions.Count; suggestionIndex++)
        {
            var suggestion = suggestions[suggestionIndex];
            var top = suggestionTop;
            card.Controls.Add(new Label { Text = $"Alternativa {suggestionIndex + 1}: {suggestion.Car}", Left = 12, Top = top, Width = 220, Height = 18, ForeColor = Color.FromArgb(110, 210, 165) });
            var link = new LinkLabel { Text = suggestion.Label, Left = 235, Top = top, Width = width - 250, Height = 18, LinkColor = Color.FromArgb(125, 185, 250), ActiveLinkColor = Color.White, VisitedLinkColor = Color.FromArgb(125, 185, 250) };
            link.Click += (_, _) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = suggestion.Url, UseShellExecute = true });
            card.Controls.Add(link);
            if (!string.IsNullOrWhiteSpace(suggestion.Note))
                card.Controls.Add(new Label { Text = suggestion.Note, Left = 12, Top = top + 17, Width = width - 24, Height = 17, ForeColor = Color.FromArgb(160, 170, 185), Font = new Font("Segoe UI", 8.1f) });
            suggestionTop += string.IsNullOrWhiteSpace(suggestion.Note) ? 21 : 42;
        }
        if (manual != null)
        {
            var assign = new Button { Text = "USA UN'AUTO GIA INSTALLATA COME EQUIVALENTE", Left = 12, Top = height - 30, Width = width - 24, Height = 24, BackColor = Color.FromArgb(56, 76, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.3f, FontStyle.Bold) };
            assign.Click += (_, _) => AssignManualEquivalent(manual);
            card.Controls.Add(assign);
        }
        return card;
    }

    private void AssignManualEquivalent(ManualAssignment assignment)
    {
        var rung = RungFor(assignment.Formula, assignment.Level);
        if (rung == null) return;
        using var picker = new EquivalentCarPickerDialog(index.Cars.Where(ContentCategoryRules.IsRaceable).ToList(), assignment.Title, rung);
        if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedCar == null) return;
        var car = picker.SelectedCar;
        car.Category = $"manual:{rung.Id}";
        car.Confidence = 100;
        index.CategoryOverrides[car.Id] = car.Category;
        ContentChanged = true;
        DialogResult = DialogResult.Retry;
        Close();
    }

    private static LadderRung? RungFor(bool formula, int level)
    {
        if (formula && level <= 3)
            return level switch { 1 => CareerLadder.ById(CareerLadder.FourStroke), 2 => CareerLadder.ById(CareerLadder.TwoStroke), _ => CareerLadder.ById(CareerLadder.Shifter) };
        return CareerLadder.Rungs.FirstOrDefault(rung => rung.Step == level && (formula ? rung.Path == LadderPath.SingleSeater : rung.Path is LadderPath.Touring or LadderPath.Endurance));
    }

    private static Panel CarTile(ContentCarRecord car)
    {
        var tile = new Panel { Width = 130, Height = 58, Margin = new Padding(1), BackColor = Color.FromArgb(26, 31, 42) };
        tile.Controls.Add(new PictureBox { Left = 3, Top = 3, Width = 54, Height = 34, SizeMode = PictureBoxSizeMode.Zoom, Image = TryCarImage(car) });
        tile.Controls.Add(new Label { Text = Display(car), Left = 60, Top = 4, Width = 67, Height = 48, ForeColor = Color.White, Font = new Font("Segoe UI", 7.3f), AutoEllipsis = true });
        return tile;
    }

    private Button ChoiceButton(string text, int left, int width, Action action)
    {
        var button = new Button { Text = text, Left = left, Top = 825, Width = width, Height = 42, BackColor = Color.FromArgb(50, 62, 82), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        button.Click += (_, _) => action();
        return button;
    }

    private void Select(string path, string message) { SelectedPath = path ?? ""; choice.Text = message; }
    private static bool IsBase(ContentCarRecord car) { var rung = CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg); return rung.Path == LadderPath.Karting || rung.Id == CareerLadder.RoadRookie; }
    private static bool IsKart(ContentCarRecord car) => CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Path == LadderPath.Karting;
    private static bool IsUtility(ContentCarRecord car) => CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Id == CareerLadder.RoadRookie;
    private static bool IsFormula(ContentCarRecord car) => CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Path == LadderPath.SingleSeater;
    private static bool HasCarForBranchLevel(ContentCarRecord car, bool formula, int level)
    {
        var rung = CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg);
        if (formula) return level <= 3 ? IsKart(car) && rung.Step == level : IsFormula(car) && rung.Step == level;
        if (level <= 3) return level == 1 && IsUtility(car);
        return !IsFormula(car) && !IsKart(car) && rung.Step == level;
    }
    private static List<ContentCarRecord> NearbyEquivalents(List<ContentCarRecord> cars, bool formula, int level)
    {
        IEnumerable<ContentCarRecord> compatible = formula
            ? (level <= 3 ? cars.Where(IsKart) : cars.Where(IsFormula))
            : cars.Where(car => !IsFormula(car) && !IsKart(car));
        // Non facciamo mai passare una GT per Formula o un kart per turismo.
        // All'interno dello stesso ramo, invece, privilegiamo il gradino più
        // vicino: una F3 può coprire provvisoriamente una F2, una Touring Light
        // un trofeo club, una GT3 un campionato endurance minore.
        return compatible
            .OrderBy(car => Math.Abs(CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg).Step - level))
            .ThenBy(car => car.PowerHp == 0 ? int.MaxValue : car.PowerHp)
            .ThenBy(car => car.Name)
            .Take(3)
            .ToList();
    }
    private static string Display(ContentCarRecord car) => $"{car.Name}{(car.SpecificationsSource.StartsWith("stima", StringComparison.OrdinalIgnoreCase) ? " *" : "")}";
    private static int TextHeight(string text, int width) => TextRenderer.MeasureText(text, new Font("Segoe UI", 9), new Size(width, int.MaxValue), TextFormatFlags.WordBreak).Height;
    private static Image? TryImage(string asset)
    {
        try
        {
            var path = AssetPaths.File(asset);
            if (!File.Exists(path)) return null;
            using var source = Image.FromFile(path);
            return new Bitmap(source);
        }
        catch { return null; }
    }
    private static Image? TryCarImage(ContentCarRecord car)
    {
        try
        {
            var candidates = new[] { Path.Combine(car.SourcePath, "ui", "ui_car.png"), Path.Combine(car.SourcePath, "ui", "badge.png") };
            var path = candidates.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(path)) return null;
            using var source = Image.FromFile(path);
            return new Bitmap(source);
        }
        catch { return null; }
    }
    private static IReadOnlyList<MissingSuggestion> MissingOptions(bool formula, int level) => (formula, level) switch
    {
        (false, 2) =>
        [
            new("LADA Granta S1600", "Apri auto completa", "https://www.overtake.gg/downloads/lada-granta-s1600-and-touring-light.76978/", "Scarica lo ZIP dalla pagina e trascinalo in Content Manager."),
            new("Volkswagen Polo S1600", "Apri auto completa", "https://www.overtake.gg/downloads/volkswagen-polo-s1600-and-touring-light.82546/", "Scarica lo ZIP dalla pagina e trascinalo in Content Manager.")
        ],
        (false, 3) =>
        [
            new("LADA Kalina Touring Light", "Apri auto completa", "https://www.overtake.gg/downloads/lada-kalina-nfr-r1-s1600-and-touring-light.58427/", "Scarica lo ZIP dalla pagina e trascinalo in Content Manager."),
            new("Volkswagen Polo Touring Light", "Apri auto completa", "https://www.overtake.gg/downloads/volkswagen-polo-s1600-and-touring-light.82546/", "Scarica lo ZIP dalla pagina e trascinalo in Content Manager.")
        ],
        (true, 6) =>
        [
            new("Lola B99/50 Formula 3000 (1999)", "Apri mod gratuita OverTake", "https://www.overtake.gg/downloads/lola-b99-50-formula-3000-1999-gonchi-rodriguez.64709/", "Auto completa gratuita, equivalente storico alla Formula 2. OverTake richiede solo un account gratuito per il download."),
            new("Formula Challenge Suzuka Oval", "Apri mod gratuita AssettoCorsaMods", "https://assettocorsamods.io/cars/race/formula_challenge_suzuka_oval/", "Equivalente moderno: 450 cv, 675 kg, 1,50 kg/cv. Richiede Custom Shaders Patch.")
        ],
        (false, 6) =>
        [
            new("BR03", "Apri prototipo completo", "https://www.overtake.gg/downloads/br03.48030/", "Auto completa: scarica e installa l’archivio in Content Manager."),
            new("ORECA LMP2", "Apri auto LMP2", "https://www.overtake.gg/downloads/oreca-lmp2.75014/", "Se arriva in RAR, estrailo e installa manualmente la cartella content/cars.")
        ],
        (false, 7) =>
        [
            new("Hypercar / LMDh", "Apri ricerca AC Cars", "https://www.overtake.gg/downloads/categories/assetto-corsa.1/?prefix_id=0", "Scegli una pagina che indichi esplicitamente auto completa, non skin o campionato."),
            new("FARSIVE Prototype Pack", "Apri pacchetto prototipi completo", "https://www.overtake.gg/downloads/f-a-r-s-i-v-e-free-gen-1-prototype-pack.76483/", "Alternativa gratuita per correre con prototipi; non è un Hypercar WEC ufficiale.")
        ],
        _ => []
    };

    private static string MissingMessage(bool formula, int level) => (formula, level) switch
    {
        (true, 6) => "Nessuna Formula 2 attuale perfetta: sono proposte due monoposto gratuite complete di prestazioni vicine.",
        _ => "Classe non disponibile nella tua installazione."
    };
}
