using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class AudioDialog : CareerDialog
{
    private readonly ListBox list = new() { Left = 22, Top = 72, Width = 300, Height = 390, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly TextBox script = new() { Left = 350, Top = 72, Width = 535, Height = 310, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(245, 241, 226), ForeColor = Color.FromArgb(35, 35, 35) };
    private readonly Label source = new() { Left = 350, Top = 402, Width = 535, Height = 40, ForeColor = Color.Gainsboro };
    private readonly Button stop = new() { Text = "Ferma", Left = 400, Top = 440, Width = 155, Height = 34, BackColor = Color.FromArgb(75, 80, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Button listen = new() { Text = "Ascolta servizio", Left = 570, Top = 440, Width = 155, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Button export = new() { Text = "Esporta WAV", Left = 740, Top = 440, Width = 145, Height = 34, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly ComboBox voice = new() { Left = 22, Top = 440, Width = 330, Height = 34, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly List<AudioScript> scripts = [];
    private readonly string audioDirectory;
    private readonly CareerState career;
    private readonly Action save;

    public AudioDialog(CareerState career, Action? save = null)
    {
        this.career = career; this.save = save ?? (() => { });
        Text = "CorsaCareer — Servizi audio"; ClientSize = new Size(910, 535); StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 28, 37); Font = new Font("Segoe UI", 10);
        FormClosed += (_, _) => NarrationService.Stop();
        audioDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "CorsaCareer", "audio");
        Controls.Add(new Label { Text = "SERVIZI AUDIO E TELECRONACA", Left = 22, Top = 18, AutoSize = true, Font = new Font("Segoe UI", 19), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = "Copioni originali, fattuali e pronti per una lettura locale/TTS opzionale", Left = 24, Top = 48, AutoSize = true, ForeColor = Color.Gainsboro });
        foreach (var item in career.Events.OrderByDescending(x => x.DateUtc)) { scripts.Add(AudioCommentaryBuilder.ForEvent(career, item)); list.Items.Add(item.Track + " — " + item.Type); }
        if (scripts.Count == 0) list.Items.Add("Nessuna gara reale archiviata");
        voice.Items.Add(NarrationVoiceCatalog.Automatic);
        voice.Items.Add(NarrationVoiceCatalog.Browser);
        foreach (var installedVoice in NarrationVoiceCatalog.ItalianVoiceNames()) voice.Items.Add(installedVoice);
        var preferredIndex = 0;
        if (!string.IsNullOrWhiteSpace(career.PreferredVoice) && !career.PreferredVoice.Equals("Automatica", StringComparison.OrdinalIgnoreCase))
        {
            for (var i = 1; i < voice.Items.Count; i++)
            {
                var optionText = voice.Items[i]?.ToString() ?? "";
                if (optionText.Length > 0 && (optionText.Contains(career.PreferredVoice, StringComparison.OrdinalIgnoreCase) || career.PreferredVoice.Contains(optionText, StringComparison.OrdinalIgnoreCase))) { preferredIndex = i; break; }
            }
        }
        voice.SelectedIndex = preferredIndex;
        voice.SelectedIndexChanged += (_, _) => { var selected = SelectedVoice(); career.PreferredVoice = string.IsNullOrWhiteSpace(selected) ? "Automatica" : selected; this.save(); };
        list.SelectedIndexChanged += (_, _) => ShowScript(list.SelectedIndex); stop.Click += (_, _) => NarrationService.Stop(); listen.Click += (_, _) => Listen(); export.Click += (_, _) => ExportWav(); Controls.Add(list); Controls.Add(script); Controls.Add(source); Controls.Add(voice); Controls.Add(stop); Controls.Add(listen); Controls.Add(export);
        Controls.Add(new Label { Text = $"{voice.Items.Count - 1} voci italiane rilevate · pause e prosodia attive.  |  L'audio non inventa risultati.", Left = 350, Top = 455, AutoSize = true, ForeColor = Color.FromArgb(245, 190, 65) });
        var manageVoices = new Button { Text = "Gestisci / installa voci Windows", Left = 22, Top = 480, Width = 300, Height = 32, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        manageVoices.Click += (_, _) => { try { Process.Start(new ProcessStartInfo { FileName = "ms-settings:speech", UseShellExecute = true }); } catch (Exception error) { MessageBox.Show($"Impossibile aprire le impostazioni vocali: {error.Message}", "Voci Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning); } };
        Controls.Add(manageVoices);
        Controls.Add(new Label { Text = "Per una voce Natural/Neural installala in Windows, poi riapri questo pannello.", Left = 350, Top = 485, Width = 535, Height = 28, ForeColor = Color.Gainsboro });
        if (scripts.Count > 0) list.SelectedIndex = 0;
    }

    private void ShowScript(int index)
    {
        if (index < 0 || index >= scripts.Count) return;
        script.Text = scripts[index].Text; source.Text = scripts[index].SourceType;
    }

    private void ExportWav()
    {
        var index = list.SelectedIndex;
        if (index < 0 || index >= scripts.Count) return;
        Directory.CreateDirectory(audioDirectory);
        var safe = string.Join("_", scripts[index].Title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var output = Path.Combine(audioDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}_{safe}.wav");
        try
        {
            NarrationService.ExportWav(scripts[index].Text, output, SelectedVoice());
            MessageBox.Show($"Esportazione avviata. Il WAV verrà salvato in:\n{output}", "Servizio audio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception error) { MessageBox.Show($"Impossibile avviare la voce locale: {error.Message}", "Servizio audio", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void Listen()
    {
        var index = list.SelectedIndex;
        if (index < 0 || index >= scripts.Count) return;
        try { NarrationService.Speak(scripts[index].Text, SelectedVoice()); }
        catch (Exception error) { MessageBox.Show($"Impossibile avviare la voce locale: {error.Message}", "Servizio audio", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private string SelectedVoice() => voice.SelectedItem?.ToString()?.StartsWith("Automatica", StringComparison.OrdinalIgnoreCase) == true ? "" : voice.SelectedItem?.ToString() ?? "";
}
