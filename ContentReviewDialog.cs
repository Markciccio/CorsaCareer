using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class ContentReviewDialog : CareerDialog
{
    private readonly ContentIndexRecord index;
    private readonly Action save;
    private readonly Action? refresh;
    private readonly Action? openContentManager;
    private readonly Action? chooseRoot;
    private readonly ListBox list = new() { Left = 22, Top = 66, Width = 500, Height = 490, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly ComboBox category = new() { Left = 550, Top = 105, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label details = new() { Left = 550, Top = 155, Width = 300, Height = 210, AutoSize = false, ForeColor = Color.Gainsboro };
    private static readonly string[] Categories = ["kart", "rookie", "cup", "historic", "touring", "TCR", "GT4", "GT3", "GT2", "GT1", "prototype", "LMP", "hypercar", "formula junior", "formula 4", "formula 3", "formula 2", "formula", "road", "trackday", "hillclimb", "special"];
    public ContentReviewDialog(ContentIndexRecord index, Action save, Action? refresh = null, Action? openContentManager = null, Action? chooseRoot = null)
    {
        this.index = index; this.index.InstalledPackages ??= []; this.save = save; this.refresh = refresh; this.openContentManager = openContentManager; this.chooseRoot = chooseRoot; Text = "CorsaCareer — Revisione contenuti"; ClientSize = new Size(880, 680); StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "REVISIONE CONTENUTI", Left = 22, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 20), ForeColor = Color.FromArgb(245, 190, 65) });
        var raceable = ContentAvailability.RaceableCars(index.Cars);
        Controls.Add(new Label { Text = $"Le correzioni manuali restano dopo ogni nuova scansione.  ·  Auto: {index.Cars.Count}  ·  da gara: {raceable}  ·  Circuiti/layout: {index.Tracks.Count}  ·  pacchetti: {index.InstalledPackages.Count}  ·  avvisi scansione: {index.ScanWarnings.Count}", Left = 23, Top = 48, AutoSize = true, ForeColor = index.ScanWarnings.Count == 0 ? Color.Gainsboro : Color.FromArgb(255, 190, 70) });
        Controls.Add(new Label { Text = $"Radice analizzata: {(string.IsNullOrWhiteSpace(index.AssettoCorsaRoot) ? "non disponibile" : index.AssettoCorsaRoot)}", Left = 23, Top = 635, Width = 500, Height = 20, AutoEllipsis = true, ForeColor = Color.FromArgb(160, 165, 175) });
        var packageHistory = new Button { Text = "Archivio pacchetti", Left = 550, Top = 18, Width = 230, Height = 34, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; packageHistory.Click += (_, _) => { using var dialog = new ContentPackageHistoryDialog(index.InstalledPackages); dialog.ShowDialog(this); }; Controls.Add(packageHistory);
        category.Items.AddRange(Categories); category.SelectedIndexChanged += (_, _) => ApplyCategory(); Controls.Add(category);
        var openManager = new Button { Text = "Apri Content Manager", Left = 550, Top = 280, Width = 230, Height = 38, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = openContentManager != null }; openManager.Click += (_, _) => openContentManager?.Invoke(); Controls.Add(openManager);
        var install = new Button { Text = "Installa pacchetto mod ZIP", Left = 550, Top = 380, Width = 230, Height = 38, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; install.Click += (_, _) => InstallPackage(); Controls.Add(install);
        var openRoot = new Button { Text = "Apri cartella Assetto Corsa", Left = 550, Top = 330, Width = 230, Height = 38, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; openRoot.Click += (_, _) => OpenRoot(); Controls.Add(openRoot);
        var apply = new Button { Text = "Applica categoria", Left = 550, Top = 435, Width = 230, Height = 38, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; apply.Click += (_, _) => ApplyCategory(); Controls.Add(apply);
        var close = new Button { Text = "Chiudi", Left = 550, Top = 490, Width = 230, Height = 38, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; close.Click += (_, _) => { save(); DialogResult = DialogResult.OK; Close(); }; Controls.Add(close);
        var choose = new Button { Text = "Scegli radice Assetto Corsa", Left = 550, Top = 535, Width = 230, Height = 34, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = chooseRoot != null }; choose.Click += (_, _) => { chooseRoot?.Invoke(); DialogResult = DialogResult.OK; Close(); }; Controls.Add(choose);
        var download = new Button { Text = "Scarica ZIP verificato", Left = 550, Top = 580, Width = 230, Height = 34, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; download.Click += async (_, _) => await DownloadPackage(); Controls.Add(download);
        foreach (var car in index.Cars) list.Items.Add($"{car.Id}  ·  {car.Category}  ·  confidenza {car.Confidence}%");
        list.SelectedIndexChanged += (_, _) => ShowSelected(); Controls.Add(list); if (list.Items.Count > 0) list.SelectedIndex = 0;
    }
    private void ShowSelected()
    {
        if (list.SelectedIndex < 0 || list.SelectedIndex >= index.Cars.Count) return; var car = index.Cars[list.SelectedIndex]; category.SelectedItem = car.Category;
        var trackNames = index.Tracks.Take(8).Select(x => string.IsNullOrWhiteSpace(x.Layout) || x.Layout.Equals(x.Name, StringComparison.OrdinalIgnoreCase) ? x.Name : $"{x.Name} ({x.Layout})");
        var packageSummary = index.InstalledPackages.Count == 0 ? "nessun pacchetto registrato" : string.Join("\n", index.InstalledPackages.OrderByDescending(x => x.InstalledUtc).Take(3).Select(x => $"{x.Source} · {x.InstalledFiles} file · {x.InstalledUtc:dd/MM/yyyy}"));
        var warnings = index.ScanWarnings.Count == 0 ? "nessuno" : string.Join("\n", index.ScanWarnings.Take(5)) + (index.ScanWarnings.Count > 5 ? "\n…" : "");
        details.Text = $"{car.Name}\nMarca: {(string.IsNullOrWhiteSpace(car.Brand) ? "n/d" : car.Brand)}\nCategoria: {car.Category}\nConfidenza: {car.Confidence}%\nPotenza: {(car.PowerHp > 0 ? $"{car.PowerHp} CV" : "n/d")}\nMassa: {(car.MassKg > 0 ? $"{car.MassKg} kg" : "n/d")}\nSkin/livree: {car.Skins.Count}\n\nCIRCUITI RILEVATI ({index.Tracks.Count})\n{(index.Tracks.Count == 0 ? "nessuno" : string.Join("\n", trackNames))}{(index.Tracks.Count > 8 ? "\n…" : "")}\n\nAVVISI SCANSIONE\n{warnings}\n\nPROVENIENZA PACCHETTI\n{packageSummary}"; if (!Controls.Contains(details)) Controls.Add(details);
    }
    private void ApplyCategory()
    {
        if (list.SelectedIndex < 0 || category.SelectedItem is not string selected) return; var car = index.Cars[list.SelectedIndex]; car.Category = selected; car.Confidence = 100; index.CategoryOverrides[car.Id] = selected; var at = list.SelectedIndex; list.Items[at] = $"{car.Id}  ·  {car.Category}  ·  confidenza 100%"; list.SelectedIndex = at; details.Text = $"{car.Name}\nCategoria manuale salvata: {selected}"; save();
    }
    private void InstallPackage()
    {
        if (string.IsNullOrWhiteSpace(index.AssettoCorsaRoot) || !Directory.Exists(index.AssettoCorsaRoot))
        {
            MessageBox.Show("Prima scegli una radice valida di Assetto Corsa: il pacchetto non può essere installato senza una destinazione verificata.", "Installazione contenuti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var picker = new OpenFileDialog { Filter = "Pacchetti mod ZIP|*.zip", Title = "Seleziona un pacchetto Assetto Corsa verificato" };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        if (MessageBox.Show("Il pacchetto verrà estratto nella cartella Assetto Corsa rilevata. Continuare?", "Installazione contenuti", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        if (!ContentPackageInstaller.TryInstall(picker.FileName, index.AssettoCorsaRoot, out var count, out var error)) { MessageBox.Show(error, "Installazione non riuscita", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        RecordPackage(Path.GetFileName(picker.FileName), picker.FileName, count);
        save(); refresh?.Invoke(); MessageBox.Show($"Pacchetto installato: {count} file. La revisione verrà riaperta sulla scansione aggiornata.", "Installazione completata", MessageBoxButtons.OK, MessageBoxIcon.Information); DialogResult = DialogResult.OK; Close();
    }
    private void OpenRoot()
    {
        if (string.IsNullOrWhiteSpace(index.AssettoCorsaRoot) || !Directory.Exists(index.AssettoCorsaRoot))
        {
            MessageBox.Show("La radice di Assetto Corsa non è disponibile: aggiorna prima la scansione dei contenuti.", "Cartella non trovata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{index.AssettoCorsaRoot}\"", UseShellExecute = true });
    }
    private async Task DownloadPackage()
    {
        if (string.IsNullOrWhiteSpace(index.AssettoCorsaRoot) || !Directory.Exists(index.AssettoCorsaRoot))
        {
            MessageBox.Show("Prima scegli una radice valida di Assetto Corsa: il download non può installare file senza una destinazione verificata.", "Download verificato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var dialog = new VerifiedDownloadDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (!ContentDownloadService.ValidateRequest(dialog.Url, dialog.Sha256, out var validationError)) { MessageBox.Show(validationError, "Download verificato", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try
        {
            Cursor = Cursors.WaitCursor;
            var package = await ContentDownloadService.DownloadVerifiedAsync(dialog.Url, dialog.Sha256, Path.Combine(Path.GetTempPath(), "CorsaCareerDownloads"));
            if (!ContentPackageInstaller.TryInstall(package, index.AssettoCorsaRoot, out var count, out var installError)) { try { File.Delete(package); } catch { } MessageBox.Show(installError, "Installazione non riuscita", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            RecordPackage(dialog.Url, package, count, dialog.Sha256);
            try { File.Delete(package); } catch { }
            save(); refresh?.Invoke(); MessageBox.Show($"Download verificato e installato: {count} file. La revisione verrà riaperta sulla scansione aggiornata.", "Download completato", MessageBoxButtons.OK, MessageBoxIcon.Information); DialogResult = DialogResult.OK; Close();
        }
        catch (Exception error) { MessageBox.Show($"Download non completato: {error.Message}", "Download verificato", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { Cursor = Cursors.Default; }
    }
    private void RecordPackage(string source, string hashSourcePath, int count, string? knownHash = null)
    {
        try
        {
            var hash = string.IsNullOrWhiteSpace(knownHash) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(hashSourcePath))) : knownHash.Trim().ToUpperInvariant();
            index.InstalledPackages.Add(new ContentPackageRecord { Source = source, Sha256 = hash, InstalledFiles = count, InstalledUtc = DateTime.UtcNow });
            save();
        }
        catch { }
    }
}
