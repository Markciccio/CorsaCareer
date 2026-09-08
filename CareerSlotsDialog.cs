using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class CareerSlotsDialog : CareerDialog
{
    private readonly string directory;
    private readonly string backupDirectory;
    private readonly CareerState current;
    private readonly Action<CareerState> load;
    // Selezione multipla: cancellare le vecchie carriere una per una era inutilmente lento.
    private readonly ListBox slots = new()
    {
        Left = 22, Top = 74, Width = 500, Height = 286,
        BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White,
        SelectionMode = SelectionMode.MultiExtended
    };
    private readonly TextBox name = new() { Left = 22, Top = 404, Width = 340, Height = 32, PlaceholderText = "Nome della copia" };
    private readonly Label summary = new() { Left = 22, Top = 366, Width = 500, Height = 32, ForeColor = Color.FromArgb(170, 178, 190), Font = new Font("Segoe UI", 8) };

    public CareerSlotsDialog(string directory, CareerState current, Action<CareerState> load, string? backupDirectory = null)
    {
        this.directory = directory;
        this.backupDirectory = backupDirectory ?? Path.Combine(directory, "..", "backups");
        this.current = current;
        this.load = load;

        Text = "CorsaCareer — Carriere salvate";
        ClientSize = new Size(560, 560);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37);
        ForeColor = Color.White;

        Controls.Add(new Label { Text = "CARICARE, ARCHIVIARE O CANCELLARE", Left = 22, Top = 18, AutoSize = true, Font = new Font("Segoe UI", 15), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = "Selezione multipla con Ctrl o Maiusc.", Left = 24, Top = 52, AutoSize = true, ForeColor = Color.FromArgb(150, 158, 172), Font = new Font("Segoe UI", 8) });
        Controls.Add(slots);
        Controls.Add(summary);
        Controls.Add(name);

        var save = new Button { Text = "Salva copia", Left = 372, Top = 403, Width = 150, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var open = new Button { Text = "Carica selezionata", Left = 22, Top = 448, Width = 180, Height = 32, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var restore = new Button { Text = "Ripristina ultimo backup", Left = 212, Top = 448, Width = 210, Height = 32, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var delete = new Button { Text = "Cancella selezionate", Left = 22, Top = 490, Width = 180, Height = 32, BackColor = Color.FromArgb(92, 34, 44), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var purge = new Button { Text = "Pulisci vecchi backup", Left = 212, Top = 490, Width = 210, Height = 32, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

        save.Click += (_, _) => SaveCopy();
        open.Click += (_, _) => LoadSelected();
        restore.Click += (_, _) => RestoreLatestBackup();
        delete.Click += (_, _) => DeleteSelected();
        purge.Click += (_, _) => PurgeBackups();
        slots.SelectedIndexChanged += (_, _) => RefreshSummary();

        Controls.Add(save); Controls.Add(open); Controls.Add(restore); Controls.Add(delete); Controls.Add(purge);
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        slots.Items.Clear();
        Directory.CreateDirectory(directory);
        foreach (var file in Directory.GetFiles(directory, "*.json").OrderByDescending(File.GetLastWriteTimeUtc))
            slots.Items.Add(Describe(file));
        RefreshSummary();
    }

    /// <summary>Voce dell'elenco: nome, data e dimensione, così si capisce cosa si sta cancellando.</summary>
    private static string Describe(string file)
    {
        try
        {
            var info = new FileInfo(file);
            return $"{Path.GetFileNameWithoutExtension(file)}   ·   {info.LastWriteTime:dd/MM/yyyy HH:mm}   ·   {info.Length / 1024.0:0.#} KB";
        }
        catch { return Path.GetFileNameWithoutExtension(file); }
    }

    private static string SlotName(string item) => item.Split("   ·   ")[0];

    private void RefreshSummary()
    {
        var total = slots.Items.Count;
        var backups = 0;
        try { if (Directory.Exists(backupDirectory)) backups = Directory.GetFiles(backupDirectory, "career-*.json").Length; } catch { }
        summary.Text = $"{total} carriera/e archiviata/e · {slots.SelectedItems.Count} selezionata/e · {backups} backup versionati";
    }

    private void SaveCopy()
    {
        var safe = string.Join("_", (string.IsNullOrWhiteSpace(name.Text) ? $"{current.Driver}_{DateTime.Now:yyyyMMdd_HHmm}" : name.Text)
            .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        if (safe.Length == 0) safe = "carriera";
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, safe + ".json"), JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true }));
        name.Clear();
        RefreshSlots();
    }

    private void LoadSelected()
    {
        if (slots.SelectedItems.Count != 1) { MessageBox.Show("Seleziona una sola carriera da caricare.", "Carriere salvate", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        var path = Path.Combine(directory, SlotName((string)slots.SelectedItems[0]!) + ".json");
        try
        {
            var state = JsonSerializer.Deserialize<CareerState>(File.ReadAllText(path));
            if (state != null) { load(state); DialogResult = DialogResult.OK; Close(); }
        }
        catch (Exception error) { MessageBox.Show($"Salvataggio non leggibile: {error.Message}", "Carriere salvate", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    /// <summary>
    /// Cancella le carriere archiviate selezionate. Non toccano mai la carriera
    /// in corso — quella vive in <c>career.json</c>, fuori da questa cartella —
    /// e l'operazione richiede una conferma che elenca i nomi.
    /// </summary>
    private void DeleteSelected()
    {
        var names = slots.SelectedItems.Cast<string>().Select(SlotName).ToList();
        if (names.Count == 0) { MessageBox.Show("Seleziona almeno una carriera da cancellare.", "Carriere salvate", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        var list = string.Join("\n", names.Take(12).Select(x => "· " + x)) + (names.Count > 12 ? $"\n… e altre {names.Count - 12}" : "");
        var answer = MessageBox.Show(
            $"Cancellare definitivamente {names.Count} carriera/e archiviata/e?\n\n{list}\n\nLa carriera attualmente in corso non viene toccata. L'operazione non è annullabile.",
            "Cancella carriere salvate", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (answer != DialogResult.Yes) return;

        var removed = 0;
        var failures = new List<string>();
        foreach (var slot in names)
        {
            var path = Path.Combine(directory, slot + ".json");
            try
            {
                if (!File.Exists(path)) continue;
                File.Delete(path);
                removed++;
                CareerLog.Info("carriere", $"carriera archiviata cancellata: {slot}");
            }
            catch (Exception error)
            {
                failures.Add($"{slot}: {error.Message}");
                CareerLog.Warn("carriere", $"cancellazione non riuscita per {slot}: {error.Message}");
            }
        }
        RefreshSlots();
        var report = $"Cancellate {removed} carriera/e.";
        if (failures.Count > 0) report += $"\n\nNon cancellate:\n{string.Join("\n", failures)}";
        MessageBox.Show(report, "Carriere salvate", MessageBoxButtons.OK, failures.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
    }

    /// <summary>
    /// Elimina i backup versionati più vecchi conservando i cinque più recenti:
    /// senza questo la cartella dei backup resta piena di file mai più usati.
    /// </summary>
    private void PurgeBackups()
    {
        const int keep = 5;
        try
        {
            if (!Directory.Exists(backupDirectory)) { MessageBox.Show("Nessuna cartella di backup presente.", "Pulizia backup", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var files = Directory.GetFiles(backupDirectory, "career-*.json").OrderByDescending(File.GetLastWriteTimeUtc).ToList();
            var stale = files.Skip(keep).ToList();
            if (stale.Count == 0) { MessageBox.Show($"Ci sono {files.Count} backup: nessuno da eliminare (se ne conservano {keep}).", "Pulizia backup", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var size = stale.Sum(x => new FileInfo(x).Length) / 1024.0;
            if (MessageBox.Show($"Eliminare {stale.Count} backup più vecchi ({size:0.#} KB) conservando i {keep} più recenti?", "Pulizia backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            var removed = 0;
            foreach (var file in stale)
            {
                try { File.Delete(file); removed++; } catch (Exception error) { CareerLog.Warn("carriere", $"backup non eliminato: {error.Message}"); }
            }
            CareerLog.Info("carriere", $"backup eliminati: {removed}");
            RefreshSummary();
            MessageBox.Show($"Eliminati {removed} backup. Ne restano {Math.Min(keep, files.Count)}.", "Pulizia backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception error) { MessageBox.Show($"Pulizia non completata: {error.Message}", "Pulizia backup", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void RestoreLatestBackup()
    {
        try
        {
            var backup = Directory.Exists(backupDirectory) ? Directory.GetFiles(backupDirectory, "career-*.json").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() : null;
            if (backup == null) { MessageBox.Show("Nessun backup versionato disponibile.", "Carriere salvate", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Ripristinare il backup più recente?\n\n{Path.GetFileName(backup)}\n\nLa carriera corrente resterà intatta finché non confermi il caricamento.", "Ripristino backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            var state = JsonSerializer.Deserialize<CareerState>(File.ReadAllText(backup));
            if (state == null) throw new InvalidDataException("Backup vuoto o non valido.");
            load(state); DialogResult = DialogResult.OK; Close();
        }
        catch (Exception error) { MessageBox.Show($"Backup non leggibile: {error.Message}", "Ripristino backup", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}
