namespace CorsaCareer;

/// <summary>Il mercato non passa da zero a contratto: ogni scuderia osserva il pilota nel tempo.</summary>
public sealed class TeamInterest
{
    public string Team { get; set; } = "";
    public int Value { get; set; }
    public string Status { get; set; } = "Osservazione";
    public string Reason { get; set; } = "";
}

public static class TeamInterestService
{
    private static readonly string[] Teams = ["Serizawa Motorsport", "Kaido Motorsport", "Minato Apex Racing", "Hoshino Race Team"];

    public static void Refresh(CareerState career)
    {
        career.TeamInterests ??= [];
        var profile = career.ReputationProfile ?? new ReputationProfile();
        for (var i = 0; i < Teams.Length; i++)
        {
            var interest = career.TeamInterests.FirstOrDefault(x => x.Team == Teams[i]);
            if (interest == null) { interest = new TeamInterest { Team = Teams[i] }; career.TeamInterests.Add(interest); }
            var testBonus = career.RookieEvaluationScore / 3;
            interest.Value = Math.Clamp(18 + profile.TeamTrust / 2 + profile.SportingPrestige / 3 + profile.SponsorAppeal / 8 + testBonus - i * 4, 0, 100);
            interest.Status = interest.Value switch { >= 92 => "Contratto possibile", >= 83 => "Offerta parzialmente finanziata", >= 72 => "Invito a un test", _ => "Osservazione" };
            interest.Reason = interest.Status switch
            {
                "Invito a un test" => "Il passo e il feedback hanno acceso l'interesse.",
                "Offerta parzialmente finanziata" => "Il team vede un potenziale, ma chiede una garanzia economica.",
                "Contratto possibile" => "Prestazioni, affidabilità e immagine sono sufficienti per trattare.",
                _ => "Sta raccogliendo dati: ogni test e risultato pesa."
            };
        }
    }
}
