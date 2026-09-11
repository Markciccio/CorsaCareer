namespace CorsaCareer;

/// <summary>
/// Il bilancio di una carriera intera, quando finisce.
///
/// Fino a ieri una carriera non finiva: arrivata in cima ripeteva la stessa
/// stagione all'infinito — al banco se ne contavano diciassette identiche di
/// fila. Mancava la cosa che dà senso a tutto il resto, cioè il momento in cui
/// si guarda indietro e si conta quello che si è fatto.
///
/// Qui i numeri vengono tutti dallo storico: non c'è niente di inventato e
/// niente da salvare in più. Una carriera senza titoli lo dice, e lo dice senza
/// consolare.
/// </summary>
public sealed class CareerEpilogue
{
    public string Pilota { get; init; } = "";
    public int EtaAlRitiro { get; init; }
    public int AnniDiCarriera { get; init; }
    public int Gare { get; init; }
    public int Vittorie { get; init; }
    public int Podi { get; init; }
    public int Pole { get; init; }
    public int Ritiri { get; init; }
    public int Punti { get; init; }
    public int Stagioni { get; init; }
    public int Titoli { get; init; }
    public int Mondiali { get; init; }
    public string CategoriaMassima { get; init; } = "";
    public int GradinoMassimo { get; init; }
    public int LivelloMassimo { get; init; }
    public int GiriPercorsi { get; init; }
    public int CategorieAttraversate { get; init; }
    public string PrimaVittoria { get; init; } = "";
    public string UltimaGara { get; init; } = "";

    public int PercentualePodi => Gare == 0 ? 0 : (int)Math.Round(Podi * 100.0 / Gare);
    public int PercentualeVittorie => Gare == 0 ? 0 : (int)Math.Round(Vittorie * 100.0 / Gare);

    public static CareerEpilogue Compute(CareerState career, IReadOnlyList<ContentCarRecord> vetture)
    {
        var gare = career.RaceHistory ?? [];
        var arrivate = gare.Where(x => !x.Dnf && x.Position > 0).ToList();
        var stagioni = career.SeasonArchive ?? [];

        // Il gradino più alto davvero raggiunto, non quello dichiarato adesso:
        // una carriera che finisce dopo una retrocessione ha comunque toccato
        // il suo massimo, ed è quello che si ricorda.
        var gradinoMassimo = 0;
        var nomeMassimo = "";
        foreach (var g in gare)
        {
            var auto = vetture.FirstOrDefault(x => x.Id.Equals(g.Car, StringComparison.OrdinalIgnoreCase));
            if (auto == null) continue;
            var rung = CareerLadder.ForCar(auto.Category, auto.PowerHp, auto.MassKg);
            if (rung.Step <= gradinoMassimo) continue;
            gradinoMassimo = rung.Step;
            nomeMassimo = rung.Name;
        }

        var primaVittoria = gare.FirstOrDefault(x => !x.Dnf && x.Position == 1);
        var ultima = gare.LastOrDefault();
        var da = gare.Count > 0 ? gare[0].StoryDate : career.CareerStart;
        var a = ultima?.StoryDate ?? career.StoryDate;

        return new CareerEpilogue
        {
            Pilota = career.Driver ?? "",
            EtaAlRitiro = career.BirthYear > 0 ? a.Year - career.BirthYear : 0,
            AnniDiCarriera = da == default ? 0 : Math.Max(1, (int)Math.Round((a - da).TotalDays / 365.25)),
            Gare = gare.Count,
            Vittorie = arrivate.Count(x => x.Position == 1),
            Podi = arrivate.Count(x => x.Position <= 3),
            Pole = gare.Count(x => x.QualificationPosition == 1),
            Ritiri = gare.Count(x => x.Dnf),
            Punti = gare.Sum(x => x.Points),
            Stagioni = stagioni.Count,
            Titoli = stagioni.Count(x => x.TitleWon),
            Mondiali = stagioni.Count(x => x.WorldTitle),
            CategoriaMassima = nomeMassimo,
            GradinoMassimo = gradinoMassimo,
            // Il livello piu' alto davvero raggiunto, non quello di adesso: chi
            // finisce dopo una retrocessione ha comunque corso lassu'. Qui i due
            // rami del ternario erano identici, quindi il campo diceva sempre e
            // solo il livello corrente — l'intenzione era rimasta a meta'.
            LivelloMassimo = Math.Max(
                ChampionshipLadder.Clamp(career.ChampionshipLevel),
                stagioni.Count == 0 ? 0 : stagioni.Max(x => ChampionshipLadder.Clamp(LivelloDi(x)))),
            GiriPercorsi = gare.Sum(x => Math.Max(0, x.Laps)),
            CategorieAttraversate = gare.Select(x => x.Car).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            PrimaVittoria = primaVittoria == null
                ? ""
                : $"{NarrativeEngine.Capitalize((primaVittoria.Track ?? "").Replace('_', ' '))}, {NarrativeCalendar.Format(primaVittoria.StoryDate)}",
            UltimaGara = ultima == null
                ? ""
                : $"{NarrativeEngine.Capitalize((ultima.Track ?? "").Replace('_', ' '))}, {NarrativeCalendar.Format(ultima.StoryDate)}"
        };
    }

    /// <summary>
    /// Il livello di campionato di una stagione archiviata.
    ///
    /// L'archivio non lo registra come numero: conserva il nome del campionato,
    /// che e' pero' sufficiente perche' i nomi li assegna la scala stessa.
    /// </summary>
    private static int LivelloDi(SeasonSummary stagione)
    {
        for (var livello = ChampionshipLadder.Levels; livello >= 1; livello--)
            if (string.Equals(ChampionshipLadder.Name(livello), stagione.Championship, StringComparison.OrdinalIgnoreCase))
                return livello;
        return 0;
    }

    /// <summary>
    /// Come verrà ricordata questa carriera. Non è un voto: è la frase che
    /// resta, e cambia molto fra chi ha vinto tutto e chi non è mai arrivato.
    /// </summary>
    public string Giudizio()
    {
        if (Mondiali > 1) return $"{Mondiali} volte campione del mondo. Un nome che resta nei libri, non nelle statistiche.";
        if (Mondiali == 1) return "Campione del mondo. Una volta sola, ma nessuno potrà mai toglierla.";
        if (Titoli >= 3) return $"{Titoli} titoli e mai il mondiale: una carriera che ha vinto tanto e si è fermata a un passo dal massimo.";
        if (Titoli > 0) return $"{Titoli} {(Titoli == 1 ? "titolo" : "titoli")} in bacheca. Non il vertice, ma nemmeno una carriera qualunque.";
        if (Vittorie >= 10) return $"{Vittorie} vittorie senza mai un titolo: la definizione di un pilota veloce in una squadra sbagliata.";
        if (Vittorie > 0) return $"{Vittorie} {(Vittorie == 1 ? "vittoria" : "vittorie")}. Poche, ma ci sono state, e quel giorno c'eri tu.";
        if (Podi > 0) return $"{Podi} podi e nessuna vittoria. È la carriera della maggior parte dei piloti, ed è comunque più di quella di quasi tutti.";
        return "Nessuna vittoria, nessun podio. Ci hai provato, e provarci è già più di quanto faccia chi resta a guardare.";
    }
}
