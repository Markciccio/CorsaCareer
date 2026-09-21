using System.Windows.Forms;

namespace CorsaCareer;

/// <summary>
/// Consente al pilota di usare una vettura già presente in content/cars come
/// sostituto di un gradino vuoto, senza copiare, spostare o modificare file di
/// Assetto Corsa. Viene salvata soltanto un'etichetta di classificazione.
/// </summary>
public sealed class EquivalentCarPickerDialog : CareerDialog
{
    private readonly List<ContentCarRecord> cars;
    private readonly ListBox list = new() { Left = 22, Top = 108, Width = 650, Height = 330, BackColor = Color.FromArgb(32, 39, 52), ForeColor = Color.White };
    private readonly Label details = new() { Left = 22, Top = 450, Width = 650, Height = 74, ForeColor = Color.Gainsboro };
    public ContentCarRecord? SelectedCar { get; private set; }

    public EquivalentCarPickerDialog(List<ContentCarRecord> availableCars, string levelTitle, LadderRung target)
    {
        cars = availableCars.OrderBy(car => car.Name).ToList();
        Text = "CorsaCareer — Equivalente manuale";
        ClientSize = new Size(700, 600);
        BackColor = Color.FromArgb(18, 22, 30);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        Controls.Add(new Label { Text = "SCEGLI UN'AUTO GIA INSTALLATA", Left = 22, Top = 20, Width = 650, Height = 32, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(245, 190, 65) });
        Controls.Add(new Label { Text = $"La vettura selezionata verrà usata come equivalente di: {levelTitle}. Non viene copiato o modificato alcun file: resta nella cartella Assetto Corsa e la scelta è salvata solo in CorsaCareer.", Left = 23, Top = 59, Width = 650, Height = 45, ForeColor = Color.Gainsboro });

        foreach (var car in cars)
        {
            var power = car.PowerHp > 0 ? $"{car.PowerHp} CV" : "CV n/d";
            var weight = car.MassKg > 0 ? $"{car.MassKg} kg" : "peso n/d";
            list.Items.Add($"{car.Name}  ·  {power}  ·  {weight}  ·  {car.Category}");
        }
        list.SelectedIndexChanged += (_, _) => ShowSelected(target);
        Controls.Add(list);

        var confirm = new Button { Text = "USA COME EQUIVALENTE", Left = 393, Top = 548, Width = 280, Height = 34, BackColor = Color.FromArgb(224, 24, 58), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        confirm.Click += (_, _) =>
        {
            if (list.SelectedIndex < 0) { CareerMessages.Show(this, "Scegli prima un'auto dalla tua installazione.", "Equivalente manuale", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            SelectedCar = cars[list.SelectedIndex];
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(confirm);
        var cancel = new Button { Text = "ANNULLA", Left = 22, Top = 548, Width = 150, Height = 34, BackColor = Color.FromArgb(55, 65, 82), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(cancel);
        if (cars.Count > 0) list.SelectedIndex = 0;
        else details.Text = "Non risultano auto utilizzabili nella cartella Assetto Corsa selezionata.";
    }

    private void ShowSelected(LadderRung target)
    {
        if (list.SelectedIndex < 0 || list.SelectedIndex >= cars.Count) return;
        var car = cars[list.SelectedIndex];
        var current = CareerLadder.ForCar(car.Category, car.PowerHp, car.MassKg);
        details.Text = $"Attuale: {current.Name} · Destinazione manuale: {target.Name}\n{car.Name} resterà fisicamente dov'è; CorsaCareer la considererà in questo gradino fino a una tua nuova modifica nella Revisione contenuti.";
    }
}
