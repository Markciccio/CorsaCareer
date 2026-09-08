using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class SoundtrackDialog : CareerDialog
{
    private readonly ListBox tracks = new();
    private readonly Label nowPlaying = new();
    private readonly TrackBar volume = new();
    private readonly System.Windows.Forms.Timer volumeDebounce = new() { Interval = 320 };

    public SoundtrackDialog(string? eventType = null)
    {
        Text = "CorsaCareer — colonna sonora";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 560);
        MinimumSize = new Size(620, 440);
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.TextPrimary;

        var header = new Panel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(26, 14, 26, 8), BackColor = UiTheme.HeaderBackground };
        header.Controls.Add(new Label { Text = "SOUNDTRACK · LIVE PADDOCK", Dock = DockStyle.Top, Height = 30, Font = UiTheme.Title, ForeColor = UiTheme.Accent });
        header.Controls.Add(new Label { Text = eventType == null ? "Scegli la musica della tua sessione." : $"Atmosfera suggerita: {SoundtrackService.SuggestedMood(eventType)}", Dock = DockStyle.Bottom, Height = 25, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary });

        tracks.Dock = DockStyle.Fill;
        tracks.Font = UiTheme.Body;
        tracks.BackColor = UiTheme.Surface;
        tracks.ForeColor = UiTheme.TextPrimary;
        tracks.BorderStyle = BorderStyle.None;
        foreach (var path in SoundtrackService.Tracks()) tracks.Items.Add(Path.GetFileNameWithoutExtension(path));
        tracks.SelectedIndexChanged += (_, _) => UpdateNowPlaying();
        if (tracks.Items.Count > 0) tracks.SelectedIndex = SuggestedIndex(eventType);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 118, Padding = new Padding(24, 10, 24, 16), BackColor = UiTheme.HeaderBackground };
        nowPlaying = new Label { Dock = DockStyle.Top, Height = 23, Font = UiTheme.Small, ForeColor = UiTheme.TextSecondary, AutoEllipsis = true };
        volume.Minimum = 0; volume.Maximum = 100; volume.Value = 70; volume.Width = 180; volume.TickFrequency = 10; volume.Left = 0; volume.Top = 30;
        // Scroll scatta a ogni tacca durante il trascinamento e sul backend
        // ffplay ogni cambio di volume riavvia la traccia: applicarlo subito
        // avrebbe prodotto una raffica di riavvii. Si aspetta la fine del gesto.
        volume.Scroll += (_, _) => { volumeDebounce.Stop(); volumeDebounce.Start(); };
        volumeDebounce.Tick += (_, _) => { volumeDebounce.Stop(); SoundtrackService.SetVolume(volume.Value * 10); };
        var play = UiTheme.PrimaryButton("▶  RIPRODUCI"); play.Width = 150; play.Left = 210; play.Top = 28;
        var pause = UiTheme.SecondaryButton("Ⅱ  PAUSA"); pause.Width = 120; pause.Left = 370; pause.Top = 28;
        // RIPRODUCI riparte sempre dall'inizio, quindi annulla un'eventuale
        // pausa: l'etichetta va riportata allo stato di riproduzione.
        play.Click += (_, _) => { PlaySelected(); pause.Text = "Ⅱ  PAUSA"; };
        pause.Click += (_, _) =>
        {
            // Un solo pulsante per le due azioni: l'etichetta segue lo stato
            // reale del servizio, così non resta "PAUSA" su un brano fermo.
            if (SoundtrackService.IsPaused) SoundtrackService.Resume(); else SoundtrackService.Pause();
            pause.Text = SoundtrackService.IsPaused ? "▶  RIPRENDI" : "Ⅱ  PAUSA";
            UpdateNowPlaying();
        };
        var stop = UiTheme.SecondaryButton("■  STOP"); stop.Width = 100; stop.Left = 500; stop.Top = 28;
        stop.Click += (_, _) => { SoundtrackService.SetEnabled(false); pause.Text = "Ⅱ  PAUSA"; nowPlaying.Text = "Nessun brano in riproduzione"; };
        footer.Controls.Add(nowPlaying); footer.Controls.Add(volume); footer.Controls.Add(play); footer.Controls.Add(pause); footer.Controls.Add(stop);

        Controls.Add(tracks); Controls.Add(footer); Controls.Add(header);
        // Un tick ancora in coda dopo la chiusura avrebbe riavviato la traccia
        // subito dopo lo Stop finale.
        // Chiudendo si torna al silenzio: prima veniva chiamato Stop, ma la
        // prima riproduzione automatica riaccendeva tutto un istante dopo.
        FormClosed += (_, _) => { volumeDebounce.Stop(); SoundtrackService.SetEnabled(false); };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) volumeDebounce.Dispose();
        base.Dispose(disposing);
    }

    private int SuggestedIndex(string? eventType)
    {
        if (tracks.Items.Count == 0) return -1;
        var mood = SoundtrackService.SuggestedMood(eventType ?? "");
        var keywords = mood switch { "vittoria" => new[] { "Proud", "Final", "Checkered" }, "crisi" => new[] { "Faded", "Static" }, "sponsor" => new[] { "Pitch", "Analysis" }, "griglia" => new[] { "Gridline", "Checkered" }, "concentrazione" => new[] { "Focus", "Paddock" }, _ => new[] { "Morning", "Horizon" } };
        for (var i = 0; i < tracks.Items.Count; i++) if (keywords.Any(k => tracks.Items[i]?.ToString()?.Contains(k, StringComparison.OrdinalIgnoreCase) == true)) return i;
        return 0;
    }

    private void PlaySelected()
    {
        if (tracks.SelectedIndex < 0) return;
        var path = SoundtrackService.Tracks()[tracks.SelectedIndex];
        // Avviare un brano da qui e' il consenso esplicito: da questo momento
        // le atmosfere possono suonare anche da sole.
        SoundtrackService.SetEnabled(true);
        SoundtrackService.Play(path, volume.Value * 10);
        UpdateNowPlaying();
    }

    private void UpdateNowPlaying() => nowPlaying.Text = tracks.SelectedItem == null
        ? "Nessun brano selezionato"
        : SoundtrackService.Current == null
            ? $"Selezionato: {tracks.SelectedItem}"
            : SoundtrackService.IsPaused
                ? $"In pausa: {tracks.SelectedItem} · backend {SoundtrackService.LastBackend}"
                : $"In riproduzione: {tracks.SelectedItem} · backend {SoundtrackService.LastBackend}";
}
