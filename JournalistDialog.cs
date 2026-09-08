using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class JournalistDialog : CareerDialog
{
    private readonly TextBox name = new() { Left = 205, Top = 78, Width = 270 };
    private readonly TextBox publication = new() { Left = 205, Top = 126, Width = 270 };
    private readonly TextBox tone = new() { Left = 205, Top = 174, Width = 270 };
    private readonly NumericUpDown memory = new() { Left = 205, Top = 222, Width = 90, Minimum = 1, Maximum = 99 };
    private readonly ComboBox preset = new() { Left = 205, Top = 42, Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly CareerState career;
    private readonly Action save;

    public JournalistDialog(CareerState career, Action save)
    {
        this.career = career; this.save = save;
        Text = "CorsaCareer — redazione"; ClientSize = new Size(520, 410); StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        career.Journalist ??= new JournalistProfile();
        preset.Items.AddRange(["Noa Minazuki", "Aoi Serizawa", "Personalizzato"]);
        preset.SelectedIndex = career.Journalist.Name switch { "Noa Minazuki" => 0, "Aoi Serizawa" => 1, _ => 2 };
        name.Text = career.Journalist.Name; publication.Text = career.Journalist.Publication; tone.Text = career.Journalist.Tone; memory.Value = Math.Clamp(career.Journalist.MemoryYears, 1, 99);
        var style = new ComboBox { Left = 205, Top = 258, Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
        style.Items.AddRange(["Cronaca classica analitica", "Paddock e interviste", "Redazione moderna"]);
        style.SelectedItem = style.Items.Contains(career.Journalist.NarrativeStyle) ? career.Journalist.NarrativeStyle : "Cronaca classica analitica";
        Controls.Add(new Label { Left = 24, Top = 262, Width = 170, Text = "Stile narrativo", ForeColor = Color.Gainsboro }); Controls.Add(style);
        Controls.Add(new Label { Text = "PROFILO REDAZIONE", Left = 24, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Add("Profilo rapido", preset, 42); Add("Giornalista", name, 78); Add("Testata", publication, 126); Add("Tono editoriale", tone, 174); Add("Memoria (anni)", memory, 222);
        preset.SelectedIndexChanged += (_, _) => ApplyPreset();
        Controls.Add(new Label { Text = "I nomi sono profili narrativi originali: articoli e audio restano basati esclusivamente sui fatti salvati.", Left = 24, Top = 300, Width = 460, Height = 35, ForeColor = Color.Gainsboro });
        var saveButton = new Button { Text = "Salva redazione", Left = 24, Top = 355, Width = 170, Height = 32, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        saveButton.Click += (_, _) => { if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(publication.Text)) { MessageBox.Show("Inserisci almeno giornalista e testata.", "Redazione", MessageBoxButtons.OK, MessageBoxIcon.Warning); DialogResult = DialogResult.None; return; } career.Journalist.Name = name.Text.Trim(); career.Journalist.Publication = publication.Text.Trim(); career.Journalist.Tone = string.IsNullOrWhiteSpace(tone.Text) ? "cronaca sportiva originale" : tone.Text.Trim(); career.Journalist.NarrativeStyle = style.SelectedItem?.ToString() ?? "Cronaca classica analitica"; career.Journalist.MemoryYears = (int)memory.Value; career.News.Add($"La redazione di {career.Journalist.Publication} rinnova il proprio dossier sulla carriera di {career.Driver}."); career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = "EDITORIAL_PROFILE", Headline = $"Redazione aggiornata: {career.Journalist.Name} firma i servizi di {career.Journalist.Publication}.", Track = "Redazione", Importance = 25 }); save(); }; Controls.Add(saveButton); AcceptButton = saveButton;
    }
    private void Add(string label, Control control, int top) { Controls.Add(new Label { Text = label, Left = 24, Top = top + 4, AutoSize = true, ForeColor = Color.Gainsboro }); Controls.Add(control); }
    private void ApplyPreset()
    {
        switch (preset.SelectedItem?.ToString())
        {
            case "Noa Minazuki": name.Text = "Noa Minazuki"; publication.Text = "Grand Prix CorsaCareer"; tone.Text = "telecronaca tecnica originale"; break;
            case "Aoi Serizawa": name.Text = "Aoi Serizawa"; publication.Text = "Paddock CorsaCareer"; tone.Text = "intervista e retroscena originali"; break;
        }
    }
}
