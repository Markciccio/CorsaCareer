namespace CorsaCareer1991;

/// <summary>
/// Le capacità di Haru Senda.
///
/// Non è un generatore di denaro: è una persona che migliora col tempo, e la sua
/// bravura cambia l'esito di una trattativa quanto i risultati del pilota. Senza
/// queste, mandarlo da un'azienda o da un'altra sarebbe la stessa cosa.
/// </summary>
public sealed class AgentSkills
{
    /// <summary>Quanto sa portare a casa una cifra migliore.</summary>
    public int Negotiation { get; set; } = 30;
    /// <summary>Quante porte riesce ad aprire: incide su chi lo riceve.</summary>
    public int Network { get; set; } = 25;
    /// <summary>Quanto viene creduto: incide sulle trattative difficili.</summary>
    public int Credibility { get; set; } = 30;
    /// <summary>Quanto capisce di corse: serve a parlare con chi le conosce.</summary>
    public int MotorsportKnowledge { get; set; } = 40;

    /// <summary>Una misura sola, per i confronti rapidi.</summary>
    public int Overall => (Negotiation + Network + Credibility + MotorsportKnowledge) / 4;

    public string Describe() =>
        $"negoziazione {Negotiation} · contatti {Network} · credibilità {Credibility} · conoscenza {MotorsportKnowledge}";
}

/// <summary>
/// Una visita che Haru propone di fare.
///
/// È una scena, non un pulsante: Haru dice dove vorrebbe andare, il giocatore
/// decide se mandarcelo, e solo dopo si scopre com'è andata. La probabilità è
/// dichiarata prima, perché la decisione sia informata.
/// </summary>
public sealed class SponsorVisit
{
    public string Id { get; init; } = "";
    /// <summary>Chi si va a trovare.</summary>
    public string Target { get; init; } = "";
    /// <summary>Che attività è: serve a capire perché direbbe sì o no.</summary>
    public string Trade { get; init; } = "";
    /// <summary>Come Haru la propone, con le sue parole.</summary>
    public string Pitch { get; init; } = "";
    /// <summary>Ore che porta via a Haru.</summary>
    public int Hours { get; init; } = 2;
    /// <summary>Quanto porterebbe, se andasse bene.</summary>
    public int Amount { get; init; }
    /// <summary>Probabilità di riuscita, da 0 a 100, calcolata sullo stato reale.</summary>
    public int Chance { get; init; }
    /// <summary>Da cosa dipende quella probabilità: il giocatore deve poterlo capire.</summary>
    public List<string> Reasons { get; init; } = [];
}

/// <summary>
/// Le trattative di Haru, come scene.
///
/// Prima una sponsorizzazione era una riga in un elenco e un numero che
/// cambiava. Qui è un piccolo racconto: Haru propone, tu decidi, lui va, e
/// quello che succede dipende da chi sei — non da un lancio di dadi.
/// </summary>
public static class SponsorVisits
{
    /// <summary>Quante visite Haru può proporre in un giorno.</summary>
    public const int VisitsPerDay = 3;

    private static readonly (string Target, string Trade, int Amount, int Base, string Pitch)[] Catalogue =
    [
        ("Officina Doppino", "riparazioni e saldature", 120, 62,
         "«Passo da Doppino. Viene a tutte le gare del sabato, magari ci dà una mano.»"),
        ("Ramen Kizuna", "trattoria di quartiere", 90, 66,
         "«Il posto dove mangiano tutti i meccanici. Il proprietario ti ha già visto correre.»"),
        ("Ferramenta Kuroda", "ferramenta di paese", 150, 52,
         "«Kuroda misura ogni parola, ma se dice sì poi non torna indietro.»"),
        ("Gomme Akatsuki", "pneumatici e assetti", 320, 36,
         "«Questi le corse le conoscono. Vorranno vedere dei tempi, non delle promesse.»"),
        ("Bevande Mikazuki", "distribuzione bevande", 380, 30,
         "«Cercano visibilità. Dipende tutto da quanta gente ti segue.»"),
        ("Trasporti Nagareboshi", "piccoli trasporti", 340, 32,
         "«Sono abituati a trattare. Con loro conta come gliela racconti.»"),
        ("Elettronica Habataki", "riparazione elettronica", 260, 38,
         "«Vogliono capire cosa ci guadagnano. Serve un ragionamento, non entusiasmo.»"),
        ("Assicurazioni Tomoshibi", "agenzia assicurativa", 460, 22,
         "«Difficile. Ma se entra uno così, entra per anni.»")
    ];

    /// <summary>
    /// Le visite che Haru propone oggi. L'elenco è stabile per la giornata: non
    /// cambia riaprendo la schermata, ma si rinnova domani.
    /// </summary>
    public static List<SponsorVisit> ForDay(CareerState career)
    {
        var profile = career.ReputationProfile ?? new ReputationProfile();
        var skills = career.Agent ?? new AgentSkills();
        var seed = Hash($"{career.Driver}|{career.StoryDate:yyyyMMdd}|haru");

        var visits = new List<SponsorVisit>();
        for (var i = 0; i < VisitsPerDay; i++)
        {
            var entry = Catalogue[(int)(Math.Abs(seed / (i + 3)) % Catalogue.Length)];
            if (visits.Any(x => x.Target == entry.Target)) continue;

            var reasons = new List<string>();
            var chance = entry.Base;

            // I risultati del pilota.
            if (career.Wins > 0) { chance += 12; reasons.Add($"{career.Wins} vittorie in carriera"); }
            else if (career.Podiums > 0) { chance += 6; reasons.Add($"{career.Podiums} podi"); }
            else if (career.Races == 0) { chance -= 18; reasons.Add("nessuna gara ancora disputata"); }

            // Il pubblico conta per chi cerca visibilità.
            if (entry.Trade.Contains("bevande") || entry.Trade.Contains("distribuzione"))
            {
                var pop = profile.PublicPopularity / 4;
                chance += pop;
                reasons.Add($"livello influencer {profile.PublicPopularity}/100");
            }

            // Le capacità di Haru.
            chance += skills.Credibility / 6;
            reasons.Add($"credibilità di Haru {skills.Credibility}/100");
            if (entry.Trade.Contains("pneumatici") || entry.Trade.Contains("assetti"))
            {
                chance += skills.MotorsportKnowledge / 8;
                reasons.Add($"Haru conosce l'ambiente ({skills.MotorsportKnowledge}/100)");
            }
            chance += skills.Network / 10;

            // La cifra che Haru riesce a strappare dipende da quanto sa trattare.
            var amount = entry.Amount + entry.Amount * skills.Negotiation / 200;

            visits.Add(new SponsorVisit
            {
                Id = $"visit-{career.StoryDate:yyyyMMdd}-{i}",
                Target = entry.Target,
                Trade = entry.Trade,
                Pitch = entry.Pitch,
                Hours = entry.Amount >= 300 ? 3 : 2,
                Amount = amount,
                Chance = Math.Clamp(chance, 5, 92),
                Reasons = reasons
            });
        }
        return visits;
    }

    /// <summary>
    /// Come va la visita. Deterministico rispetto alla giornata: ricaricare non
    /// permette di riprovare finché non esce il sì.
    /// </summary>
    public static SponsorReply Resolve(SponsorVisit visit, CareerState career)
    {
        var seed = Hash($"{career.Driver}|{career.StoryDate:yyyyMMdd}|{visit.Id}|esito");
        var roll = (int)(DriverDay.Unit(seed) * 100);
        var accepted = roll < visit.Chance;

        return new SponsorReply
        {
            Accepted = accepted,
            Amount = accepted ? visit.Amount : 0,
            Line = accepted ? AcceptLine(visit) : RefuseLine(visit),
            Reason = accepted
                ? $"Probabilità {visit.Chance}%: è andata."
                : $"Probabilità {visit.Chance}%: non è bastata."
        };
    }

    private static string AcceptLine(SponsorVisit visit) =>
        $"«Va bene. Metto € {visit.Amount:N0}. Però l'adesivo lo voglio dove si vede.»";

    private static string RefuseLine(SponsorVisit visit) =>
        $"«Il ragazzo mi piace, ma quest'anno con {visit.Trade} non ci sono margini. Mi dispiace davvero.»";

    /// <summary>
    /// Haru migliora lavorando. Anche un rifiuto insegna qualcosa: è il motivo
    /// per cui mandarlo in giro conviene comunque.
    /// </summary>
    public static void Learn(AgentSkills skills, bool accepted)
    {
        skills.Network = Math.Clamp(skills.Network + 1, 0, 100);
        if (accepted)
        {
            skills.Negotiation = Math.Clamp(skills.Negotiation + 2, 0, 100);
            skills.Credibility = Math.Clamp(skills.Credibility + 1, 0, 100);
        }
        else
        {
            // Un no insegna a preparare meglio la volta dopo.
            skills.Negotiation = Math.Clamp(skills.Negotiation + 1, 0, 100);
        }
    }

    private static long Hash(string text)
    {
        unchecked
        {
            var h = 1469598103934665603UL;
            foreach (var c in text) { h ^= c; h *= 1099511628211UL; }
            return (long)(h & 0x7FFFFFFFFFFFFFFF);
        }
    }
}
