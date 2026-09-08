using System.Drawing;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class MediaDialog : CareerDialog
{
    private readonly ComboBox captureView = new() { Left = 450, Top = 590, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly ComboBox mediaFilter = new() { Left = 24, Top = 110, Width = 145, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly ComboBox sequencePicker = new() { Left = 177, Top = 110, Width = 207, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly ListBox feed = new() { Left = 24, Top = 146, Width = 360, Height = 356, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly RichTextBox article = new() { Left = 410, Top = 146, Width = 480, Height = 105, ForeColor = Color.Gainsboro, BackColor = Color.FromArgb(24, 28, 37), BorderStyle = BorderStyle.None, ReadOnly = true, ScrollBars = RichTextBoxScrollBars.Vertical, DetectUrls = false };
    private readonly PictureBox photo = new() { Left = 410, Top = 265, Width = 480, Height = 230, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(12, 15, 20) };
    private readonly Label sequenceStatus = new() { Left = 410, Top = 500, Width = 480, Height = 22, ForeColor = Color.FromArgb(245, 190, 65), AutoSize = false };
    private readonly Label sequenceLead = new() { Left = 410, Top = 108, Width = 480, Height = 32, ForeColor = Color.Gainsboro, AutoSize = false };
    private readonly Button previous = new() { Text = "‹ Indietro", Left = 410, Top = 525, Width = 100, Height = 32, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Button next = new() { Text = "Avanti ›", Left = 520, Top = 525, Width = 100, Height = 32, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Button archive = new() { Text = "Archivia media", Left = 630, Top = 525, Width = 150, Height = 32, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Button listenCurrent = new() { Text = "▶ Ascolta pagina", Left = 780, Top = 525, Width = 110, Height = 32, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Button restartSequence = new() { Text = "▶ Leggi edizione", Left = 505, Top = 58, Width = 180, Height = 28, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly List<CareerEventRecord> events;
    private List<CareerEventRecord> sequenceEvents = [];
    private readonly CareerState career;
    private readonly Action save;
    private bool suppressSequencePersistence;
    private bool sequenceStateDirty;
    public MediaDialog(CareerState career, Action? save = null)
    {
        this.career = career; this.save = save ?? (() => { });
        Text = "CorsaCareer — Media Center"; ClientSize = new Size(920, 640); StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        events = career.Events.OrderByDescending(x => x.DateUtc).ToList();
        FormClosed += (_, _) => { if (sequenceStateDirty) this.save(); };
        Controls.Add(new Label { Text = "EDICOLA", Left = 24, Top = 58, AutoSize = true, Font = UiTheme.Headline, ForeColor = UiTheme.Warning });
        Controls.Add(new Label { Text = "Servizi, immagini e articoli · solo fatti archiviati nella carriera", Left = 25, Top = 90, AutoSize = true, Font = UiTheme.Standfirst, ForeColor = UiTheme.TextSecondary });
        Controls.Add(new Label { Text = "ELENCO NOTIZIE", Left = 24, Top = 128, AutoSize = true, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = "SERVIZIO SELEZIONATO", Left = 410, Top = 128, AutoSize = true, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.FromArgb(245, 190, 65) });
        var newspaper = new Button { Text = "LEGGI PAGINA SELEZIONATA", Left = 685, Top = 18, Width = 205, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; newspaper.Click += (_, _) => OpenCurrentNewspaper(); Controls.Add(newspaper);
        var newsroom = new Button { Text = "Redazione", Left = 700, Top = 58, Width = 190, Height = 28, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; newsroom.Click += (_, _) => { using var dialog = new JournalistDialog(career, this.save); dialog.ShowDialog(this); }; Controls.Add(newsroom);
        restartSequence.Click += (_, _) => StartSequence(); Controls.Add(restartSequence);
        var audio = new Button { Text = "Servizi audio", Left = 505, Top = 18, Width = 180, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; audio.Click += (_, _) => { using var dialog = new AudioDialog(career, this.save); dialog.ShowDialog(this); }; Controls.Add(audio);
        var importLatest = new Button { Text = "Importa ultimo screenshot AC", Left = 24, Top = 18, Width = 280, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; importLatest.Click += (_, _) => ImportLatestAssettoScreenshot(); Controls.Add(importLatest);
        var import = new Button { Text = "Importa foto", Left = 315, Top = 18, Width = 175, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; import.Click += (_, _) => ImportPhoto(); Controls.Add(import);
        var capture = new Button { Text = "Cattura AC (3s)", Left = 24, Top = 590, Width = 180, Height = 30, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; capture.Click += async (_, _) => await CaptureAssettoCorsa(); Controls.Add(capture);
        var openArchive = new Button { Text = "Apri archivio magazine", Left = 215, Top = 590, Width = 210, Height = 30, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; openArchive.Click += (_, _) => OpenMagazineFolder(); Controls.Add(openArchive);
        captureView.Items.AddRange(["Camera TV / esterna", "Replay", "Cockpit", "Pit-lane / griglia"]); captureView.SelectedIndex = 0; Controls.Add(captureView);
        Controls.Add(new Label { Text = "Visuale da archiviare nell’articolo.", Left = 630, Top = 595, Width = 260, Height = 28, ForeColor = Color.Gainsboro, AutoSize = false });
        Controls.Add(new Label { Text = "AC REALI: ciò che è visibile · ILLUSTRAZIONI: sempre dichiarate", Left = 410, Top = 565, Width = 480, Height = 20, ForeColor = Color.FromArgb(245, 190, 65), AutoSize = false });
        mediaFilter.Items.AddRange(MediaSequenceService.Filters.ToArray());
        mediaFilter.SelectedIndexChanged += (_, _) => RefreshSequence();
        sequencePicker.Items.AddRange(["Rassegna consigliata", "Weekend e gare", "Test e sviluppo", "Mercato e carriera", "Archivio (archiviati)", "Timeline completa"]);
        sequencePicker.SelectedIndexChanged += (_, _) => RefreshSequence();
        feed.DrawMode = DrawMode.OwnerDrawFixed; feed.ItemHeight = 58; feed.BorderStyle = BorderStyle.None; feed.Font = UiTheme.Body;
        feed.DrawItem += (_, e) =>
        {
            if (e.Index < 0 || e.Index >= feed.Items.Count) return;
            e.DrawBackground();
            var selected = (e.State & DrawItemState.Selected) != 0;
            using var brush = new SolidBrush(selected ? Color.FromArgb(37, 85, 128) : UiTheme.SurfaceRaised);
            e.Graphics.FillRectangle(brush, e.Bounds);
            var item = e.Index < sequenceEvents.Count ? sequenceEvents[e.Index] : null;
            TextRenderer.DrawText(e.Graphics, item == null ? feed.Items[e.Index]?.ToString() : item.Headline, UiTheme.BodyStrong, new Rectangle(e.Bounds.Left + 12, e.Bounds.Top + 8, e.Bounds.Width - 24, 24), UiTheme.TextPrimary);
            if (item != null) TextRenderer.DrawText(e.Graphics, FeedText(item).Replace("\n", "  ·  "), UiTheme.Small, new Rectangle(e.Bounds.Left + 12, e.Bounds.Top + 33, e.Bounds.Width - 24, 18), UiTheme.TextSecondary);
            using var pen = new Pen(selected ? UiTheme.Info : UiTheme.Border); e.Graphics.DrawRectangle(pen, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
        };
        feed.SelectedIndexChanged += (_, _) => ShowEvent(feed.SelectedIndex);
        previous.Click += (_, _) => MoveSelection(-1); next.Click += (_, _) => MoveSelection(1); archive.Click += (_, _) => ArchiveCurrent(); listenCurrent.Click += (_, _) => ListenCurrent();
        Controls.Add(mediaFilter); Controls.Add(sequencePicker); Controls.Add(sequenceLead); Controls.Add(feed); Controls.Add(article); Controls.Add(photo); Controls.Add(sequenceStatus); Controls.Add(previous); Controls.Add(next); Controls.Add(archive); Controls.Add(listenCurrent);
        mediaFilter.SelectedIndex = 0;
        var savedMode = sequencePicker.Items.Contains(career.MediaSequenceMode) ? career.MediaSequenceMode : "Rassegna consigliata";
        var savedFilter = mediaFilter.Items.Contains(career.MediaFilter) ? career.MediaFilter : "Tutti i media";
        suppressSequencePersistence = true;
        mediaFilter.SelectedItem = savedFilter;
        sequencePicker.SelectedItem = savedMode;
        suppressSequencePersistence = false;
        void LayoutMedia()
        {
            var width = Math.Max(900, ClientSize.Width);
            var height = Math.Max(620, ClientSize.Height);
            var leftWidth = Math.Max(360, (width - 72) * 0.38f);
            var rightLeft = 24 + (int)leftWidth + 26;
            var rightWidth = Math.Max(460, width - rightLeft - 24);
            mediaFilter.SetBounds(24, 110, (int)leftWidth / 2 - 8, 30);
            sequencePicker.SetBounds(32 + (int)leftWidth / 2, 110, (int)leftWidth / 2 - 8, 30);
            feed.SetBounds(24, 146, (int)leftWidth, Math.Max(260, height - 240));
            sequenceLead.SetBounds(rightLeft, 144, rightWidth, 32);
            article.SetBounds(rightLeft, 182, rightWidth, Math.Max(120, (int)(height * 0.18)));
            var photoTop = 190 + article.Height;
            photo.SetBounds(rightLeft, photoTop, rightWidth, Math.Max(190, height - photoTop - 220));
            sequenceStatus.SetBounds(rightLeft, photo.Bottom + 8, rightWidth, 22);
            previous.SetBounds(rightLeft, photo.Bottom + 38, 105, 32);
            next.SetBounds(rightLeft + 112, photo.Bottom + 38, 105, 32);
            archive.SetBounds(rightLeft + 224, photo.Bottom + 38, 150, 32);
            listenCurrent.SetBounds(rightLeft + 382, photo.Bottom + 38, Math.Max(110, rightWidth - 382), 32);
            captureView.SetBounds(rightLeft + 210, height - 45, 180, 30);
            var captureButton = Controls.OfType<Button>().FirstOrDefault(x => x.Text.StartsWith("Cattura AC", StringComparison.Ordinal));
            var archiveButton = Controls.OfType<Button>().FirstOrDefault(x => x.Text.StartsWith("Apri archivio", StringComparison.Ordinal));
            captureButton?.SetBounds(24, height - 45, 180, 30);
            archiveButton?.SetBounds(215, height - 45, 210, 30);
            var visualHint = Controls.OfType<Label>().FirstOrDefault(x => x.Text.StartsWith("Visuale da archiviare", StringComparison.Ordinal));
            var provenanceHint = Controls.OfType<Label>().FirstOrDefault(x => x.Text.StartsWith("AC REALI", StringComparison.Ordinal));
            visualHint?.SetBounds(rightLeft + 400, height - 48, Math.Max(180, rightWidth - 400), 30);
            provenanceHint?.SetBounds(rightLeft, height - 72, rightWidth, 20);
        }
        Resize += (_, _) => LayoutMedia();
        LayoutMedia();
    }
    private void StartSequence()
    {
        if (sequenceEvents.Count == 0) { MessageBox.Show("Questa rassegna non ha ancora servizi disponibili.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        feed.SelectedIndex = 0;
        RememberSequencePosition();
        feed.Focus();
    }
    private void RefreshSequence()
    {
        var mode = sequencePicker.SelectedItem?.ToString() ?? "Rassegna consigliata";
        // La rassegna è un montaggio editoriale: prima l'antefatto, poi il fatto,
        // infine le conseguenze. L'indice resta disponibile, ma non decide il racconto.
        var filter = mediaFilter.SelectedItem?.ToString() ?? "Tutti i media";
        sequenceEvents = MediaSequenceService.Build(events, mode, filter).ToList();
        sequenceLead.Text = MediaSequenceService.Description(mode, sequenceEvents.Count);
        feed.BeginUpdate(); feed.Items.Clear();
        foreach (var item in sequenceEvents) feed.Items.Add(FeedText(item));
        if (sequenceEvents.Count == 0) feed.Items.Add(mode == "Archivio (archiviati)" ? "Nessun media archiviato." : "Nessun media disponibile in questa sequenza.");
        feed.EndUpdate();
        sequenceStatus.Text = sequenceEvents.Count == 0 ? $"{filter} · {mode}" : $"{filter} · {mode}  ·  1/{sequenceEvents.Count}";
        var savedPosition = string.Equals(mode, career.MediaSequenceMode, StringComparison.OrdinalIgnoreCase) ? career.MediaSequencePosition : 0;
        if (sequenceEvents.Count > 0) feed.SelectedIndex = Math.Clamp(savedPosition, 0, sequenceEvents.Count - 1); else { article.Text = "Seleziona un’altra sequenza editoriale."; photo.Image = null; UpdateNavigation(); }
    }
    private void MoveSelection(int delta)
    {
        if (sequenceEvents.Count == 0) return;
        var nextIndex = Math.Clamp(feed.SelectedIndex + delta, 0, sequenceEvents.Count - 1);
        feed.SelectedIndex = nextIndex;
        RememberSequencePosition();
    }
    private void ArchiveCurrent()
    {
        if (feed.SelectedIndex < 0 || feed.SelectedIndex >= sequenceEvents.Count) return;
        var item = sequenceEvents[feed.SelectedIndex];
        if (item.Archived) return;
        item.Archived = true; save();
        RefreshSequence();
        MessageBox.Show("Media archiviato: resta disponibile nella sequenza ‘Archivio (archiviati)’ e nel salvataggio, ma non verrà più proposto nelle rassegne normali.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void UpdateNavigation()
    {
        var valid = sequenceEvents.Count > 0 && feed.SelectedIndex >= 0 && feed.SelectedIndex < sequenceEvents.Count;
        previous.Enabled = valid && feed.SelectedIndex > 0; next.Enabled = valid && feed.SelectedIndex < sequenceEvents.Count - 1; archive.Enabled = valid && !sequenceEvents[feed.SelectedIndex].Archived; listenCurrent.Enabled = valid;
        if (valid) sequenceStatus.Text = $"{mediaFilter.SelectedItem} · {sequencePicker.SelectedItem}  ·  {feed.SelectedIndex + 1}/{sequenceEvents.Count}";
    }
    private CareerEventRecord? CurrentEvent() => feed.SelectedIndex >= 0 && feed.SelectedIndex < sequenceEvents.Count ? sequenceEvents[feed.SelectedIndex] : null;
    private void OpenCurrentNewspaper()
    {
        var item = CurrentEvent() ?? events.FirstOrDefault(x => !x.Archived) ?? events.FirstOrDefault();
        if (item == null) { MessageBox.Show("Non ci sono ancora eventi editoriali archiviati.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var page = new NewspaperDialog(career, item); page.ShowDialog(this);
    }
    private void ListenCurrent()
    {
        var item = CurrentEvent();
        if (item == null) return;
        try
        {
            if (career.PreferredVoice.StartsWith("Browser", StringComparison.OrdinalIgnoreCase)) NarrationService.OpenBrowserPortal(CareerArticleBuilder.Build(career, item));
            else NarrationService.Speak(AudioCommentaryBuilder.ForEvent(career, item).Text, career.PreferredVoice);
        }
        catch (Exception error) { MessageBox.Show($"Impossibile avviare il servizio audio: {error.Message}", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private async Task CaptureAssettoCorsa()
    {
        MessageBox.Show("Tra tre secondi verrà catturata la finestra reale di Assetto Corsa. Porta il simulatore in primo piano e scegli prima la camera TV, una visuale esterna o un replay: verrà archiviato esattamente ciò che è visibile, senza trasformare una visuale cockpit.", "Cattura reale", MessageBoxButtons.OK, MessageBoxIcon.Information);
        await Task.Delay(3000);
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "CorsaCareer", "media", "captures");
        var path = Path.Combine(dir, $"capture-{DateTime.Now:yyyyMMdd-HHmmss}.png");
        if (!ScreenCaptureService.TryCaptureAssettoCorsa(path, out var error)) { MessageBox.Show(error, "Cattura reale non riuscita", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var view = captureView.SelectedItem?.ToString() ?? "Camera TV / esterna";
        var captureHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)));
        File.WriteAllText(path + ".meta.json", System.Text.Json.JsonSerializer.Serialize(new { source = "Assetto Corsa foreground window", copiedUtc = DateTime.UtcNow, sha256 = captureHash, view, kind = "ASSETTO_CORSA_SCREENSHOT" }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        var item = new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SCREENSHOT_CAPTURED", Headline = $"Cattura reale della sessione Assetto Corsa archiviata nel Media Center ({view}).", Track = "Sessione reale", Importance = 40, PhotoPath = path, PhotoView = view };
        career.Events.Add(item); career.News.Add(item.Headline); save(); events.Insert(0, item); RefreshSequence();
        MessageBox.Show("Cattura reale archiviata nel Media Center.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void ImportPhoto()
    {
        using var picker = new OpenFileDialog { Filter = "Immagini|*.png;*.jpg;*.jpeg;*.bmp", Title = "Importa foto nel Media Center" };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "CorsaCareer", "media", "manual"); Directory.CreateDirectory(dir);
            var destination = Path.Combine(dir, $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(picker.FileName)}"); File.Copy(picker.FileName, destination, true);
            var photoHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(destination)));
            File.WriteAllText(destination + ".meta.json", System.Text.Json.JsonSerializer.Serialize(new { source = picker.FileName, copiedUtc = DateTime.UtcNow, sha256 = photoHash, kind = "MANUAL_PHOTO_IMPORT" }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            var photoEvent = new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "PHOTO_ARCHIVE", Headline = $"Foto archiviata nel Media Center: {Path.GetFileName(picker.FileName)}.", Track = "Archivio manuale", Importance = 30, PhotoPath = destination };
            career.Events.Add(photoEvent);
            career.News.Add($"Il Media Center archivia una nuova immagine: {Path.GetFileName(picker.FileName)}."); save();
            var wasEmpty = events.Count == 0;
            events.Insert(0, photoEvent);
            if (wasEmpty) feed.Items.Clear();
            RefreshSequence();
            MessageBox.Show("Foto archiviata nella carriera.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception error) { MessageBox.Show($"Importazione non riuscita: {error.Message}", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private void ImportLatestAssettoScreenshot()
    {
        try
        {
            var sourceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "screens");
            if (!Directory.Exists(sourceDir)) { MessageBox.Show("La cartella degli screenshot di Assetto Corsa non esiste ancora.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var source = Directory.EnumerateFiles(sourceDir, "*.*", SearchOption.AllDirectories)
                .Where(x => new[] { ".png", ".jpg", ".jpeg", ".bmp" }.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
                .Select(x => new FileInfo(x)).OrderByDescending(x => x.LastWriteTimeUtc).FirstOrDefault();
            if (source == null) { MessageBox.Show("Nessuno screenshot AC trovato nella cartella screens.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "CorsaCareer", "media", "captures"); Directory.CreateDirectory(dir);
            var destination = Path.Combine(dir, $"{DateTime.Now:yyyyMMdd_HHmmss}_{source.Name}"); File.Copy(source.FullName, destination, true);
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(destination)));
            File.WriteAllText(destination + ".meta.json", System.Text.Json.JsonSerializer.Serialize(new { source = source.FullName, copiedUtc = DateTime.UtcNow, sha256 = hash, view = "visuale non dichiarata", kind = "ASSETTO_CORSA_SCREENSHOT" }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            var item = new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "SCREENSHOT_CAPTURED", Headline = $"Screenshot reale importato da Assetto Corsa: {source.Name}.", Track = "Archivio AC", Importance = 40, PhotoPath = destination, PhotoView = "visuale non dichiarata" };
            career.Events.Add(item); career.News.Add(item.Headline); save(); events.Insert(0, item); RefreshSequence();
            MessageBox.Show("L’ultimo screenshot reale di Assetto Corsa è stato archiviato.", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception error) { MessageBox.Show($"Importazione screenshot non riuscita: {error.Message}", "Media Center", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private void OpenMagazineFolder()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "CorsaCareer", "media", "magazine");
        Directory.CreateDirectory(directory); Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{directory}\"", UseShellExecute = true });
    }
    private void ShowEvent(int index)
    {
        if (index < 0 || index >= sequenceEvents.Count) return;
        RememberSequencePosition();
        var item = sequenceEvents[index]; var storyDate = item.StoryDate == default ? item.DateUtc.ToLocalTime() : item.StoryDate;
        var isReal = !string.IsNullOrWhiteSpace(item.PhotoPath) && File.Exists(item.PhotoPath);
        var sourceLabel = isReal ? $"Fonte immagine: {PhotoSource.Caption(item.PhotoPath, item.Type)}{(!string.IsNullOrWhiteSpace(item.PhotoView) ? $" · visuale {item.PhotoView}" : "")}" : $"Fonte immagine: {PhotoSource.Caption(item.PhotoPath, item.Type)}";
        var race = NewspaperDialog.ResolveRace(career, item);
        var arc = career.StoryArcs.OrderByDescending(x => x.Importance).FirstOrDefault(x => x.Status == "In corso");
        var articleText = NewspaperDialog.BuildArticle(career, item, race, arc);
        article.Text = $"{item.Headline}\n\n{Preview(articleText)}\n\nData narrativa: {storyDate.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"))}\nImportanza editoriale: {item.Importance}/100 · Circuito: {item.Track}\n{sourceLabel}";
        try
        {
            if (!string.IsNullOrWhiteSpace(item.PhotoPath) && File.Exists(item.PhotoPath))
            {
                using var source = Image.FromFile(item.PhotoPath);
                photo.Image = new Bitmap(source);
            }
            else
            {
                var illustration = EditorialAssets.ForEvent(item);
                if (File.Exists(illustration)) using (var source = Image.FromFile(illustration)) photo.Image = new Bitmap(source); else photo.Image = null;
            }
        }
        catch { photo.Image = null; }
        UpdateNavigation();
    }

    private void RememberSequencePosition()
    {
        if (suppressSequencePersistence || feed.SelectedIndex < 0) return;
        career.MediaSequenceMode = sequencePicker.SelectedItem?.ToString() ?? "Rassegna consigliata";
        career.MediaFilter = mediaFilter.SelectedItem?.ToString() ?? "Tutti i media";
        career.MediaSequencePosition = feed.SelectedIndex;
        sequenceStateDirty = true;
    }
    private static string Preview(string articleText)
    {
        var compact = string.Join(" ", articleText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(x => !x.Contains("Servizio originale", StringComparison.OrdinalIgnoreCase)));
        return compact.Length <= 420 ? compact : compact[..420].TrimEnd() + "…";
    }
    private static string FeedText(CareerEventRecord item)
    {
        var date = item.StoryDate == default ? item.DateUtc.ToLocalTime() : item.StoryDate;
        var source = PhotoSource.Caption(item.PhotoPath, item.Type);
        return $"{date:dd/MM/yyyy}  |  {item.Type}  |  {item.Track}\n   {source}";
    }
}
