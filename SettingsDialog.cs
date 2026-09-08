using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class SettingsDialog : CareerDialog
{
    private readonly CareerState career;
    private readonly Action save;
    private readonly ComboBox voice = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 430 };
    private readonly ComboBox journalist = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 430 };
    private readonly TextBox publication = new() { Width = 430 };
    private readonly TextBox commentator = new() { Width = 430 };
    private readonly CheckBox startupNarration = new() { Text = "Leggi automaticamente Home e schede operative", AutoSize = true, ForeColor = Color.Gainsboro };
    private readonly ComboBox difficulty = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 430 };
    private readonly ComboBox distance = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 430 };
    private readonly CheckBox autoCalibrate = new() { Text = "Calibra il livello AI dai referti reali già importati", AutoSize = true, ForeColor = Color.Gainsboro };

    public SettingsDialog(CareerState career, Action save)
    {
        this.career = career; this.save = save;
        Text = "CorsaCareer — Impostazioni"; ClientSize = new Size(760, 790); StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(18, 21, 28); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        var top = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = Color.FromArgb(27, 31, 41) };
        top.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(220, 25, 45), 4); e.Graphics.DrawLine(pen, 0, 0, top.Width, 0); };
        top.Controls.Add(new Label { Text = "IMPOSTAZIONI", Left = 30, Top = 22, AutoSize = true, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.White });
        top.Controls.Add(new Label { Text = "Voce, redazione e atmosfera della tua carriera", Left = 33, Top = 67, AutoSize = true, ForeColor = Color.FromArgb(190, 196, 207) });
        Controls.Add(top);

        var audioCard = Card("AUDIO E NARRAZIONE", 30, 135, 700, 150);
        AddField(audioCard, "Voce sintetica", voice, 18);
        voice.Items.Add(NarrationVoiceCatalog.Automatic);
        voice.Items.Add(NarrationVoiceCatalog.Browser);
        foreach (var installed in NarrationVoiceCatalog.ItalianVoiceNames()) voice.Items.Add(installed);
        voice.SelectedIndex = FindVoiceIndex(career.PreferredVoice);
        audioCard.Controls.Add(new Label { Text = "Natural/Neural Windows oppure Browser per voci online con controlli nella pagina.", Left = 182, Top = 72, Width = 490, Height = 22, ForeColor = Color.FromArgb(190, 196, 207) });
        startupNarration.Left = 182; startupNarration.Top = 105; startupNarration.Checked = career.NarrationOnStartup; audioCard.Controls.Add(startupNarration);
        audioCard.Controls.Add(new Label { Text = "Legge solo il riepilogo essenziale con la voce italiana automatica; la Rubrica TV resta disponibile con la voce Google del browser.", Left = 182, Top = 128, Width = 500, Height = 34, ForeColor = Color.FromArgb(155, 165, 180) });
        var editorialCard = Card("REDAZIONE", 30, 305, 700, 190);
        AddField(editorialCard, "Giornalista", journalist, 18);
        journalist.Items.AddRange(["Noa Minazuki", "Aoi Serizawa", "Personalizzato"]);
        journalist.SelectedIndex = career.Journalist.Name switch { "Noa Minazuki" => 0, "Aoi Serizawa" => 1, _ => 2 };
        AddField(editorialCard, "Testata", publication, 68); publication.Text = career.Journalist.Publication;
        AddField(editorialCard, "Commentatore tecnico", commentator, 118); commentator.Text = career.Journalist.ExpertCommentator;
        journalist.SelectedIndexChanged += (_, _) => ApplyJournalistPreset();

        var weekendCard = Card("WEEKEND E DIFFICOLTÀ", 30, 515, 700, 212);
        AddField(weekendCard, "Difficoltà AI", difficulty, 18);
        difficulty.Items.AddRange([.. SessionProfiles.Difficulties]);
        difficulty.SelectedIndex = Math.Max(0, Array.IndexOf(SessionProfiles.Difficulties, career.DifficultyProfile));
        AddField(weekendCard, "Distanza di gara", distance, 62);
        distance.Items.AddRange([.. SessionProfiles.Distances]);
        distance.SelectedIndex = Math.Max(0, Array.IndexOf(SessionProfiles.Distances, career.DistanceProfile));
        autoCalibrate.Left = 182; autoCalibrate.Top = 106; autoCalibrate.Checked = career.AutoCalibrateAi; weekendCard.Controls.Add(autoCalibrate);
        weekendCard.Controls.Add(new Label
        {
            Text = "La distanza è calcolata sulla lunghezza reale del circuito; il meteo viene scelto fra quelli installati in base al mese narrativo. La calibrazione cambia solo il preset della sessione successiva: non altera punti, posizioni o referti già archiviati.",
            Left = 182, Top = 130, Width = 500, Height = 70, ForeColor = Color.FromArgb(190, 196, 207)
        });
        weekendCard.Controls.Add(new Label { Text = $"Correzione AI attuale: {career.AiCalibrationOffset:+#;-#;0} punti", Left = 18, Top = 130, Width = 155, Height = 40, ForeColor = Color.FromArgb(245, 190, 65) });

        var speech = new Button { Text = "Apri impostazioni vocali Windows", Left = 30, Top = 740, Width = 250, Height = 32, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        speech.Click += (_, _) => { try { Process.Start(new ProcessStartInfo { FileName = "ms-settings:speech", UseShellExecute = true }); } catch { } };
        var saveButton = new Button { Text = "Salva impostazioni", Left = 552, Top = 740, Width = 178, Height = 32, BackColor = Color.FromArgb(190, 22, 55), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
        saveButton.Click += (_, _) => SaveSettings();
        Controls.Add(speech); Controls.Add(saveButton); AcceptButton = saveButton;
    }

    private Panel Card(string title, int left, int top, int width, int height)
    {
        var card = new Panel { Left = left, Top = top, Width = width, Height = height, BackColor = Color.FromArgb(27, 31, 41), BorderStyle = BorderStyle.FixedSingle };
        card.Controls.Add(new Label { Text = title, Left = 18, Top = 14, AutoSize = true, Font = new Font("Segoe UI Semibold", 10), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(card); return card;
    }
    private static void AddField(Control parent, string label, Control control, int top)
    {
        parent.Controls.Add(new Label { Text = label, Left = 18, Top = top + 5, Width = 155, ForeColor = Color.Gainsboro });
        control.Left = 182; control.Top = top; parent.Controls.Add(control);
    }
    private int FindVoiceIndex(string preferred)
    {
        if (string.IsNullOrWhiteSpace(preferred) || preferred.Equals("Automatica", StringComparison.OrdinalIgnoreCase)) return 0;
        for (var i = 1; i < voice.Items.Count; i++) if ((voice.Items[i]?.ToString() ?? "").Contains(preferred, StringComparison.OrdinalIgnoreCase)) return i;
        return 0;
    }
    private void ApplyJournalistPreset()
    {
        switch (journalist.SelectedItem?.ToString())
        {
            case "Noa Minazuki": publication.Text = "Grand Prix CorsaCareer"; commentator.Text = "Vittorio Sanna"; break;
            case "Aoi Serizawa": publication.Text = "Paddock CorsaCareer"; commentator.Text = "Vittorio Sanna"; break;
        }
    }
    private void SaveSettings()
    {
        var selectedVoice = voice.SelectedItem?.ToString() ?? NarrationVoiceCatalog.Automatic;
        career.PreferredVoice = selectedVoice.StartsWith("Automatica", StringComparison.OrdinalIgnoreCase) ? "Automatica" : selectedVoice;
        career.NarrationOnStartup = startupNarration.Checked;
        career.Journalist.Name = journalist.SelectedItem?.ToString() == "Personalizzato" ? career.Journalist.Name : journalist.SelectedItem?.ToString() ?? career.Journalist.Name;
        career.Journalist.Publication = string.IsNullOrWhiteSpace(publication.Text) ? "Grand Prix CorsaCareer" : publication.Text.Trim();
        career.Journalist.ExpertCommentator = string.IsNullOrWhiteSpace(commentator.Text) ? "Vittorio Sanna" : commentator.Text.Trim();
        career.DifficultyProfile = difficulty.SelectedItem?.ToString() ?? SessionProfiles.DifficultyRealistic;
        career.DistanceProfile = distance.SelectedItem?.ToString() ?? SessionProfiles.DistanceRealistic;
        career.AutoCalibrateAi = autoCalibrate.Checked;
        save();
    }
}
