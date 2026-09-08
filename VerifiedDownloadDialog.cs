using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

public sealed class VerifiedDownloadDialog : CareerDialog
{
    private readonly TextBox url = new() { Left = 24, Top = 78, Width = 510 };
    private readonly TextBox hash = new() { Left = 24, Top = 148, Width = 510 };
    public string Url => url.Text.Trim();
    public string Sha256 => hash.Text.Trim();
    public VerifiedDownloadDialog()
    {
        Text = "CorsaCareer — download verificato"; ClientSize = new Size(570, 280); StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "DOWNLOAD CONTENUTO VERIFICATO", Left = 24, Top = 20, AutoSize = true, Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = "URL HTTPS del modder", Left = 24, Top = 55, AutoSize = true, ForeColor = Color.Gainsboro }); Controls.Add(url);
        Controls.Add(new Label { Text = "SHA-256 pubblicato dal modder", Left = 24, Top = 125, AutoSize = true, ForeColor = Color.Gainsboro }); Controls.Add(hash);
        Controls.Add(new Label { Text = "Il file viene installato solo se l’hash coincide e il pacchetto supera il controllo ZIP.", Left = 24, Top = 185, Width = 520, Height = 32, ForeColor = Color.Gainsboro });
        var ok = new Button { Text = "Scarica e verifica", Left = 24, Top = 230, Width = 170, Height = 32, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; Controls.Add(ok); AcceptButton = ok;
    }
}
