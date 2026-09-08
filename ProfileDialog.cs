using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer;

public sealed class DriverProfile
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Nationality { get; set; } = "Giappone";
    public int RaceNumber { get; set; } = 27;
    public string Nickname { get; set; } = "";
    public string AvatarPath { get; set; } = "";
    public string CareerMode { get; set; } = "Realistica";
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class ProfileDialog : CareerDialog
{
    private readonly TextBox first = new() { Width = 240 };
    private readonly TextBox last = new() { Width = 240 };
    private readonly TextBox nickname = new() { Width = 240 };
    private readonly NumericUpDown number = new() { Width = 80, Minimum = 0, Maximum = 99, Value = 27 };
    public DriverProfile Profile { get; private set; } = new();
    // Non affidiamo l'avvio di una carriera al solo DialogResult: alcuni percorsi
    // di automazione/chiusura della finestra possono restituire Cancel pur dopo
    // il click sul pulsante. Questo flag registra l'unico fatto che conta: il
    // profilo è stato validato e copiato dai campi.
    public bool WasSubmitted { get; private set; }

    public ProfileDialog(DriverProfile? existing = null)
    {
        Text = existing == null ? "Nuovo pilota" : "Modifica profilo pilota";
        // Il dialogo del profilo è una finestra di servizio: non deve ereditare
        // il fullscreen delle scene narrative (che rende i campi minuscoli).
        WindowState = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(0, 0);
        ClientSize = new Size(640, 460);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        if (existing != null) { first.Text = existing.FirstName; last.Text = existing.LastName; nickname.Text = existing.Nickname; number.Value = existing.RaceNumber; }
        var title = new Label { Text = "CREA IL TUO PILOTA", AutoSize = true, Left = 40, Top = 24, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(244, 180, 42) };
        Controls.Add(title);
        var subtitle = new Label { Text = "Il ritratto e la nazionalità sono già definiti dal catalogo giapponese.", AutoSize = true, Left = 40, Top = 56, ForeColor = Color.LightSteelBlue };
        Controls.Add(subtitle);
        Add("Nome", first, 100); Add("Cognome", last, 150); Add("Numero gara", number, 200); Add("Nickname (opzionale)", nickname, 250);
        var hint = new Label { Text = "Nazionalità: Giappone\nIl ritratto manga viene scelto automaticamente dal catalogo della carriera.\nOgni risultato deve provenire dalla pista o dalla modalità debug dichiarata.", AutoSize = true, Left = 40, Top = 305, ForeColor = Color.Gainsboro };
        Controls.Add(hint);
        // DialogResult non va assegnato al bottone prima della validazione: Windows
        // chiudeva la finestra prima che il profilo venisse materialmente copiato.
        var ok = new Button { Text = existing == null ? "Crea profilo" : "Salva profilo", Width = 180, Height = 38, Left = 40, Top = 390, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text))
            {
                MessageBox.Show("Inserisci nome e cognome.", "Profilo pilota", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Profile = new DriverProfile
            {
                FirstName = first.Text.Trim(),
                LastName = last.Text.Trim(),
                Nationality = "Giappone",
                RaceNumber = (int)number.Value,
                Nickname = nickname.Text.Trim(),
                AvatarPath = "",
                CareerMode = "Realistica"
            };
            WasSubmitted = true;
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(ok); AcceptButton = ok; CancelButton = new Button { DialogResult = DialogResult.Cancel };
    }

    private void Add(string caption, Control control, int top)
    {
        Controls.Add(new Label { Text = caption, AutoSize = false, Width = 205, Left = 40, Top = top + 5, ForeColor = Color.Gainsboro });
        control.Left = 270; control.Top = top; control.Width = control is NumericUpDown ? 110 : 300; Controls.Add(control);
    }
}
