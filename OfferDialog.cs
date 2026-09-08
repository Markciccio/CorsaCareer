using System.Drawing;
using System.Windows.Forms;

namespace CorsaCareer1991;

/// <summary>Contatto sponsor: l'interesse è una stima, non una promessa.</summary>
public sealed class SponsorProspect
{
    public string Name { get; set; } = "Azienda locale";
    public string Sector { get; set; } = "attività locale";
    public int BaseInterest { get; set; } = 25;
    public int Contribution { get; set; } = 100;
    public int DurationRounds { get; set; } = 1;
    public string ContactLevel { get; set; } = "locale";
    public bool Contacted { get; set; }
    public string Status { get; set; } = "Da contattare";
    /// <summary>La citta dove ha sede.</summary>
    public string City { get; set; } = "";
    /// <summary>Chilometri dalla citta di partenza del pilota.</summary>
    public int DistanceKm { get; set; }
    /// <summary>Costo del viaggio per presentarsi di persona.</summary>
    public int TravelCost { get; set; }
    /// <summary>Il carattere di chi decide: cambia quale approccio funziona.</summary>
    public string Temperament { get; set; } = "misura ogni parola";
    public int PerceivedStars => Math.Clamp((BaseInterest + 14) / 20, 1, 5);
}

public sealed class TeamOffer
{
    public string Team { get; set; } = "Shinjin Racing";
    public string Car { get; set; } = "";
    public string Category { get; set; } = "";
    public string Sponsor { get; set; } = "Hoshino Engineering";
    public string Teammate { get; set; } = "Kenta Ogawa";
    public int Salary { get; set; }
    public int Years { get; set; } = 1;
    public int Prestige { get; set; } = 20;
    public string Objective { get; set; } = "Completa la stagione";
    public string Origin { get; set; } = "Scuderia fittizia generata dal manager";
    public string Livery { get; set; } = "";
    /// <summary>
    /// Il primo gradino non è un contratto: il pilota compra una giornata o un
    /// pacchetto di gare. Solo più avanti il mercato può trasformare il sedile
    /// in un rapporto finanziato o in uno stipendio.
    /// </summary>
    public bool IsClientSeat { get; set; }
    public int RaceFee { get; set; }
    public int TeamSupportPercent { get; set; }
    public int PrizeP1 { get; set; }
    public int PrizeP2 { get; set; }
    public int PrizeP3 { get; set; }
    public int PrizeP4P5 { get; set; }
    /// <summary>
    /// Che tipo di squadra e': costo, premi, visibilita' e supporto tecnico non
    /// sono uguali per tutti, ed e' quello che rende una scelta una scelta.
    /// </summary>
    public string ProfileId { get; set; } = "";
    public TeamProfile Profile => TeamProfile.ById(ProfileId);

    public int NetRaceFee => Math.Max(0, (int)Math.Round(RaceFee * (100 - Math.Clamp(TeamSupportPercent, 0, 100)) / 100.0));
    public string FundingLabel => IsClientSeat
        ? TeamSupportPercent <= 0 ? "cliente · paghi il 100%" : $"cliente · supporto team {TeamSupportPercent}%"
        : Salary > 0 ? $"pilota stipendiato · € {Salary:N0}/anno" : "sedile finanziato";
    public override string ToString() => IsClientSeat
        ? $"{Team}  |  {Category} · quota € {NetRaceFee:N0}"
        : $"{Team}  |  {Category} · € {Salary:N0}/anno";
}

public sealed class OfferDialog : CareerDialog
{
    private readonly ListBox offers = new() { Left = 24, Top = 58, Width = 650, Height = 190, BackColor = Color.FromArgb(35, 40, 52), ForeColor = Color.White };
    private readonly Label details = new() { Left = 24, Top = 258, Width = 650, Height = 64, ForeColor = Color.Gainsboro, AutoSize = false };
    public TeamOffer? SelectedOffer => offers.SelectedItem as TeamOffer;

    public OfferDialog(IReadOnlyList<TeamOffer> available)
    {
        Text = "Prime offerte dal paddock"; ClientSize = new Size(710, 385); StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(24, 28, 37); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "La rookie evaluation ha attirato l'interesse di queste scuderie. Scegli il tuo primo contratto.", Left = 24, Top = 20, AutoSize = true, ForeColor = Color.Gainsboro });
        foreach (var offer in available) offers.Items.Add(offer);
        if (offers.Items.Count > 0) offers.SelectedIndex = 0; offers.SelectedIndexChanged += (_, _) => ShowDetails(); Controls.Add(offers); Controls.Add(details); ShowDetails();
        var accept = new Button { Text = "Firma il contratto", Left = 24, Top = 335, Width = 180, Height = 38, BackColor = Color.FromArgb(176, 20, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK, Enabled = available.Count > 0 };
        accept.Click += (_, _) => { if (SelectedOffer == null) DialogResult = DialogResult.None; }; Controls.Add(accept); AcceptButton = accept;
    }
    private void ShowDetails()
    {
        if (SelectedOffer == null) { details.Text = "Nessun contratto disponibile: installa almeno un’auto da gara e premi Aggiorna contenuti."; return; }
        var offer = SelectedOffer;
        details.Text = $"ORIGINE: {offer.Origin}\nLIVREA: {(string.IsNullOrWhiteSpace(offer.Livery) ? "non dichiarata" : offer.Livery)}\nSPONSOR: {offer.Sponsor}   ·   COMPAGNO: {offer.Teammate}\nCONTRATTO: {offer.Years} anno/i   ·   PRESTIGIO: {offer.Prestige}/100   ·   OBIETTIVO: {offer.Objective}";
    }
}
