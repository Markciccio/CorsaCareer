using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class ContentPackageHistoryDialog : CareerDialog
{
    private readonly ListBox list = new() { Left = 24, Top = 76, Width = 430, Height = 320, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly Label details = new() { Left = 480, Top = 76, Width = 290, Height = 320, ForeColor = Color.Gainsboro, AutoSize = false };
    private readonly IReadOnlyList<ContentPackageRecord> packages;
    public ContentPackageHistoryDialog(IReadOnlyList<ContentPackageRecord> packages)
    {
        this.packages = packages;
        Text = "CorsaCareer — pacchetti installati"; ClientSize = new Size(800, 450); StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "ARCHIVIO PACCHETTI INSTALLATI", Left = 24, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = "Provenienza e integrità dei contenuti aggiunti alla scansione", Left = 25, Top = 50, AutoSize = true, ForeColor = Color.Gainsboro });
        foreach (var package in packages.OrderByDescending(x => x.InstalledUtc)) list.Items.Add($"{package.Source} · {package.InstalledUtc:dd/MM/yyyy HH:mm}");
        if (list.Items.Count == 0) list.Items.Add("Nessun pacchetto registrato.");
        list.SelectedIndexChanged += (_, _) => ShowDetails(list.SelectedIndex); Controls.Add(list); Controls.Add(details);
        if (packages.Count > 0) list.SelectedIndex = 0;
        var close = new Button { Text = "Chiudi", Left = 24, Top = 410, Width = 120, Height = 30, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(65, 75, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; Controls.Add(close); AcceptButton = close;
    }
    private void ShowDetails(int index)
    {
        if (index < 0 || index >= packages.Count) { details.Text = "Seleziona un pacchetto."; return; }
        var package = packages.OrderByDescending(x => x.InstalledUtc).ElementAt(index);
        details.Text = $"PACCHETTO\n\nOrigine:\n{package.Source}\n\nSHA-256:\n{package.Sha256}\n\nFile installati: {package.InstalledFiles}\nInstallato: {package.InstalledUtc:G}";
    }
}
