using System.Globalization;

namespace CorsaCareer;

/// <summary>
/// Le statistiche di fine anno, quelle vere.
///
/// A fine stagione ogni campionato pubblica il suo bilancio: vittorie, pole,
/// podi, giri veloci, ritiri, posizioni guadagnate, il confronto con il
/// compagno di squadra, quanto e' costata e quanto ha reso. Prima il dossier di
/// fine stagione diceva quattro numeri — punti, vittorie, premio, budget — che
/// non raccontano una stagione: un anno con dieci secondi posti e uno con dieci
/// ritiri e una vittoria potevano avere lo stesso punteggio.
///
/// Qui i numeri vengono tutti dallo storico gare gia' registrato: non c'e'
/// niente di inventato e niente da salvare in piu'. Cambiando una gara cambiano
/// le statistiche, perche' sono la stessa cosa detta in un altro modo.
/// </summary>
public sealed class SeasonStatistics
{
    public int Season { get; init; }
    public string Championship { get; init; } = "";
    public string Categoria { get; init; } = "";

    // --- il campo
    public int Gare { get; init; }
    public int Vittorie { get; init; }
    public int Podi { get; init; }
    public int ZonaPunti { get; init; }
    public int Pole { get; init; }
    public int PrimeFile { get; init; }
    public int Punti { get; init; }
    public int Ritiri { get; init; }
    public int MigliorRisultato { get; init; }
    public int PeggiorRisultato { get; init; }
    public double PosizioneMedia { get; init; }
    public double QualificaMedia { get; init; }

    // --- come sono andate le gare
    public int PosizioniGuadagnate { get; init; }
    public int MigliorRimonta { get; init; }
    public string RimontaDove { get; init; } = "";
    public int GiriPercorsi { get; init; }
    public int SosteAiBox { get; init; }
    public double PenalitaSecondi { get; init; }

    // --- il compagno di squadra
    public string Compagno { get; init; } = "";
    public int ScontriVinti { get; init; }
    public int ScontriPersi { get; init; }

    // --- le condizioni
    public int GareSulBagnato { get; init; }
    public int PodiSulBagnato { get; init; }
    public double LivelloAvversariMedio { get; init; }

    // --- i conti
    public int Premi { get; init; }
    public int BonusSponsor { get; init; }
    public int CostiTrasferte { get; init; }
    public int CostiDanni { get; init; }
    public int SaldoStagione { get; init; }

    // --- il pilota
    public int FormaInizio { get; init; }
    public int FormaFine { get; init; }
    public int FiduciaInizio { get; init; }
    public int FiduciaFine { get; init; }

    /// <summary>Percentuale di gare portate a termine.</summary>
    public int ArriviPercento => Gare == 0 ? 0 : (int)Math.Round((Gare - Ritiri) * 100.0 / Gare);

    public static SeasonStatistics Compute(CareerState career, int season)
    {
        var gare = (career.RaceHistory ?? [])
            .Where(x => x.Season == season)
            .OrderBy(x => x.Round).ThenBy(x => x.StoryDate)
            .ToList();
        var arrivate = gare.Where(x => !x.Dnf && x.Position > 0).ToList();

        // La rimonta e' la differenza fra dove sei partito e dove sei arrivato:
        // conta solo quando entrambe le posizioni esistono davvero.
        var rimonte = arrivate
            .Where(x => x.StartingPosition > 0)
            .Select(x => (Guadagno: x.StartingPosition - x.Position, Gara: x))
            .ToList();
        var migliore = rimonte.OrderByDescending(x => x.Guadagno).FirstOrDefault();

        var conCompagno = gare.Where(x => x.TeammatePosition > 0 && !x.Dnf && x.Position > 0).ToList();

        return new SeasonStatistics
        {
            Season = season,
            Championship = career.Championship ?? "",
            Categoria = career.Tier ?? "",

            Gare = gare.Count,
            Vittorie = arrivate.Count(x => x.Position == 1),
            Podi = arrivate.Count(x => x.Position <= 3),
            ZonaPunti = gare.Count(x => x.Points > 0),
            Pole = gare.Count(x => x.QualificationPosition == 1),
            PrimeFile = gare.Count(x => x.QualificationPosition is > 0 and <= 2),
            Punti = gare.Sum(x => x.Points),
            Ritiri = gare.Count(x => x.Dnf),
            MigliorRisultato = arrivate.Count == 0 ? 0 : arrivate.Min(x => x.Position),
            PeggiorRisultato = arrivate.Count == 0 ? 0 : arrivate.Max(x => x.Position),
            PosizioneMedia = arrivate.Count == 0 ? 0 : Math.Round(arrivate.Average(x => (double)x.Position), 1),
            QualificaMedia = gare.Count(x => x.QualificationPosition > 0) == 0
                ? 0
                : Math.Round(gare.Where(x => x.QualificationPosition > 0).Average(x => (double)x.QualificationPosition), 1),

            PosizioniGuadagnate = rimonte.Sum(x => Math.Max(0, x.Guadagno)),
            MigliorRimonta = migliore.Gara == null ? 0 : Math.Max(0, migliore.Guadagno),
            RimontaDove = migliore.Gara == null || migliore.Guadagno <= 0 ? "" : NarrativeEngine.Capitalize(migliore.Gara.Track ?? ""),
            GiriPercorsi = gare.Sum(x => Math.Max(0, x.Laps)),
            SosteAiBox = gare.Sum(x => Math.Max(0, x.PitStops)),
            PenalitaSecondi = Math.Round(gare.Sum(x => x.PenaltySeconds), 1),

            Compagno = conCompagno.LastOrDefault()?.TeammateName ?? career.Teammate ?? "",
            ScontriVinti = conCompagno.Count(x => x.Position < x.TeammatePosition),
            ScontriPersi = conCompagno.Count(x => x.Position > x.TeammatePosition),

            GareSulBagnato = gare.Count(x => Bagnato(x.WeatherLabel)),
            PodiSulBagnato = arrivate.Count(x => Bagnato(x.WeatherLabel) && x.Position <= 3),
            LivelloAvversariMedio = gare.Count(x => x.AiLevel > 0) == 0
                ? 0
                : Math.Round(gare.Where(x => x.AiLevel > 0).Average(x => x.AiLevel), 0),

            Premi = gare.Sum(x => x.Prize),
            BonusSponsor = gare.Sum(x => x.SponsorBonus),
            CostiTrasferte = gare.Sum(x => x.LogisticsPaid),
            CostiDanni = gare.Sum(x => x.DamagePaid),
            SaldoStagione = gare.Sum(x => x.CashDelta),

            FormaInizio = gare.Count == 0 ? career.Fitness : gare[0].FitnessAfter - gare[0].FitnessDelta,
            FormaFine = gare.Count == 0 ? career.Fitness : gare[^1].FitnessAfter,
            FiduciaInizio = gare.Count == 0 ? career.TeamRelation : gare[0].TrustAfter - gare[0].TrustDelta,
            FiduciaFine = gare.Count == 0 ? career.TeamRelation : gare[^1].TrustAfter
        };
    }

    private static bool Bagnato(string? meteo) =>
        !string.IsNullOrWhiteSpace(meteo)
        && (meteo.Contains("piogg", StringComparison.OrdinalIgnoreCase)
            || meteo.Contains("bagnat", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Il bilancio come lo stamperebbe un annuario: titoli di sezione e righe
    /// «voce · valore». La resa grafica non sta qui.
    /// </summary>
    public IReadOnlyList<(string Sezione, string Voce, string Valore)> Righe()
    {
        var righe = new List<(string, string, string)>
        {
            ("IN PISTA", "Gare disputate", $"{Gare}"),
            ("IN PISTA", "Vittorie", $"{Vittorie}"),
            ("IN PISTA", "Podi", $"{Podi}"),
            ("IN PISTA", "Arrivi a punti", $"{ZonaPunti}"),
            ("IN PISTA", "Punti conquistati", $"{Punti}"),
            ("IN PISTA", "Pole position", $"{Pole}"),
            ("IN PISTA", "Partenze in prima fila", $"{PrimeFile}"),

            ("RENDIMENTO", "Miglior risultato", MigliorRisultato > 0 ? $"P{MigliorRisultato}" : "nessun arrivo"),
            ("RENDIMENTO", "Posizione media in gara", PosizioneMedia > 0 ? PosizioneMedia.ToString("0.0", CultureInfo.GetCultureInfo("it-IT")) : "—"),
            ("RENDIMENTO", "Posizione media in qualifica", QualificaMedia > 0 ? QualificaMedia.ToString("0.0", CultureInfo.GetCultureInfo("it-IT")) : "—"),
            ("RENDIMENTO", "Gare portate a termine", $"{Gare - Ritiri} su {Gare}  ({ArriviPercento}%)"),
            ("RENDIMENTO", "Ritiri", $"{Ritiri}"),
            ("RENDIMENTO", "Livello medio degli avversari", LivelloAvversariMedio > 0 ? $"{LivelloAvversariMedio:0}%" : "—"),

            ("IN GARA", "Posizioni guadagnate", $"{PosizioniGuadagnate}"),
            ("IN GARA", "Miglior rimonta", MigliorRimonta > 0 ? $"{MigliorRimonta} posizioni · {RimontaDove}" : "nessuna"),
            ("IN GARA", "Giri percorsi", $"{GiriPercorsi}"),
            ("IN GARA", "Soste ai box", $"{SosteAiBox}"),
            ("IN GARA", "Penalità accumulate", PenalitaSecondi > 0 ? $"{PenalitaSecondi:0.#} s" : "nessuna"),
            ("IN GARA", "Gare sul bagnato", GareSulBagnato > 0 ? $"{GareSulBagnato}  ({PodiSulBagnato} a podio)" : "nessuna")
        };

        if (ScontriVinti + ScontriPersi > 0)
        {
            var nome = string.IsNullOrWhiteSpace(Compagno) ? "il compagno di squadra" : Compagno;
            righe.Add(("CONFRONTO INTERNO", $"Testa a testa con {nome}", $"{ScontriVinti} - {ScontriPersi}"));
            righe.Add(("CONFRONTO INTERNO", "Verdetto",
                ScontriVinti > ScontriPersi ? "sei stato il primo pilota"
                : ScontriVinti < ScontriPersi ? "ti ha battuto nel confronto diretto"
                : "un pareggio, gara su gara"));
        }

        righe.Add(("BILANCIO", "Premi gara", $"€ {Premi:N0}"));
        righe.Add(("BILANCIO", "Bonus sponsor", $"€ {BonusSponsor:N0}"));
        righe.Add(("BILANCIO", "Trasferte", $"€ {CostiTrasferte:N0}"));
        righe.Add(("BILANCIO", "Danni pagati", $"€ {CostiDanni:N0}"));
        righe.Add(("BILANCIO", "Saldo della stagione", $"€ {SaldoStagione:N0}"));

        righe.Add(("IL PILOTA", "Forma fisica", $"{FormaInizio} → {FormaFine}  ({Segno(FormaFine - FormaInizio)})"));
        righe.Add(("IL PILOTA", "Fiducia della squadra", $"{FiduciaInizio} → {FiduciaFine}  ({Segno(FiduciaFine - FiduciaInizio)})"));

        return righe;
    }

    private static string Segno(int delta) => delta > 0 ? $"+{delta}" : delta.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Il confronto con l'anno prima, che e' il modo in cui una stagione viene
    /// letta davvero: non «dodici podi», ma «dodici contro quattro».
    /// </summary>
    public IReadOnlyList<string> ConfrontoCon(SeasonStatistics? precedente)
    {
        if (precedente == null || precedente.Gare == 0) return [];
        var righe = new List<string>();
        void Voce(string nome, int ora, int prima, bool piuEMeglio = true)
        {
            var delta = ora - prima;
            if (delta == 0) { righe.Add($"{nome}: {ora}, come l'anno scorso."); return; }
            var meglio = piuEMeglio ? delta > 0 : delta < 0;
            righe.Add($"{nome}: {ora} contro {prima} ({(delta > 0 ? "+" : "")}{delta}) — {(meglio ? "in crescita" : "in calo")}.");
        }
        Voce("Vittorie", Vittorie, precedente.Vittorie);
        Voce("Podi", Podi, precedente.Podi);
        Voce("Punti", Punti, precedente.Punti);
        Voce("Ritiri", Ritiri, precedente.Ritiri, piuEMeglio: false);
        if (PosizioneMedia > 0 && precedente.PosizioneMedia > 0)
        {
            var delta = PosizioneMedia - precedente.PosizioneMedia;
            righe.Add($"Posizione media: {PosizioneMedia:0.0} contro {precedente.PosizioneMedia:0.0} — "
                      + (Math.Abs(delta) < 0.05 ? "invariata." : delta < 0 ? "in miglioramento." : "in peggioramento."));
        }
        if (LivelloAvversariMedio > 0 && precedente.LivelloAvversariMedio > 0 && LivelloAvversariMedio > precedente.LivelloAvversariMedio)
            righe.Add($"E gli avversari erano più forti: livello medio {LivelloAvversariMedio:0}% contro {precedente.LivelloAvversariMedio:0}%.");
        return righe;
    }
}
