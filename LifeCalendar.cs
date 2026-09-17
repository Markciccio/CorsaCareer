namespace CorsaCareer;

/// <summary>Un impegno della vita quotidiana, separato da gare, test e contratti.</summary>
public sealed class DailyCommitment
{
    public string Id { get; set; } = "";
    public DateTime Date { get; set; }
    public string Kind { get; set; } = ""; // school | track-training
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string StartTime { get; set; } = "";
    public int Hours { get; set; }
    public string TrackId { get; set; } = "";
    public string TrackName { get; set; } = "";
    public string Status { get; set; } = "planned"; // planned | active | done | skipped
    public bool Required { get; set; } = true;
}

/// <summary>
/// Agenda quotidiana in stile carriera: non inventa gare, ma riempie i giorni
/// tra due appuntamenti con scuola e preparazione. Le assenze restano salvate.
/// </summary>
public static class LifeCalendar
{
    public static IReadOnlyList<DailyCommitment> Today(CareerState career, ContentIndexRecord content)
    {
        career.DailyCommitments ??= [];
        var date = career.StoryDate.Date;
        var result = career.DailyCommitments.Where(x => x.Date.Date == date).ToList();

        if (Age(career) <= 17 && date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            Add(career, result, new DailyCommitment
            {
                Id = $"school-{date:yyyyMMdd}", Date = date, Kind = "school",
                Title = "Scuola", StartTime = "08:00", Hours = 4,
                Detail = "Lezioni e compiti prima del kartodromo. Fa parte della giornata di un pilota di 12 anni."
            });

        // Un calendario non è una lista di gare: fra i banchi e il box ci sono
        // anche piccoli impegni che rendono il protagonista una persona.
        if (date.DayOfWeek == DayOfWeek.Tuesday)
            Add(career, result, new DailyCommitment { Id = $"sponsor-{date:yyyyMMdd}", Date = date, Kind = "sponsor-visit", Title = "Visita con Haru da uno sponsor", StartTime = "14:30", Hours = 2, Required = false, Detail = "Incontro locale: puoi farlo, rimandarlo senza colpe, oppure lasciare che Haru lavori da solo." });
        if (date.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Thursday)
            Add(career, result, new DailyCommitment { Id = $"fitness-{date:yyyyMMdd}", Date = date, Kind = "fitness", Title = "Preparazione fisica", StartTime = "17:30", Hours = 1, Required = false, Detail = "Corsa e core. Facoltativo, ma aiuta a non arrivare scarichi alla gara." });
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            Add(career, result, new DailyCommitment { Id = $"recovery-{date:yyyyMMdd}", Date = date, Kind = "recovery", Title = "Riposo e famiglia", StartTime = "16:00", Hours = 2, Required = false, Detail = "Tempo libero e recupero. Puoi saltarlo senza penalità, ma non recupererai energie." });

        var next = CareerScheduler.NextPlanned(career.Schedule ?? []);
        var hasRaceToday = next?.Date.Date == date;
        // Due pomeriggi in pista alla settimana, solo se non rubano il giorno a
        // una gara/test. Il mercoledì è la prima data: il secondo allenamento
        // arriva il venerdì quando il calendario lascia spazio.
        var trainingDay = date.DayOfWeek is DayOfWeek.Wednesday or DayOfWeek.Friday;
        if (trainingDay && !hasRaceToday && (next == null || (next.Date.Date - date).Days >= 2))
        {
            var car = content.Cars.FirstOrDefault(x => x.Id.Equals(career.Car, StringComparison.OrdinalIgnoreCase))
                      ?? content.Cars.FirstOrDefault(ContentCategoryRules.IsRaceable);
            if (car != null)
            {
                var track = CareerScheduler.PickTrack(content.Tracks, date.DayOfYear, car.Category);
                if (track != null)
                    Add(career, result, new DailyCommitment
                    {
                        Id = $"training-{date:yyyyMMdd}", Date = date, Kind = "track-training",
                        Title = "Allenamento in pista", StartTime = "15:30", Hours = 3,
                        Detail = "Sessione libera programmata: va svolta in Assetto Corsa oppure saltata con conseguenze.",
                        TrackId = track.Id, TrackName = track.Name
                    });
            }
        }

        Scrutinio(career);
        Prune(career);
        return result;
    }

    /// <summary>La soglia dello scrutinio: sotto questa, si ripete l'anno.</summary>
    public const int SogliaDiPromozione = 40;

    /// <summary>
    /// Lo scrutinio di giugno.
    ///
    /// E' il momento in cui tutte le lezioni saltate per andare in pista
    /// presentano il conto. Bocciato significa recupero pomeridiano per un anno
    /// intero: due ore in meno ogni giorno feriale — cioe' un quarto del tempo
    /// che si aveva per allenarsi, per farsi vedere, per guadagnare qualcosa.
    ///
    /// E significa anche che il paddock lo viene a sapere. Un ragazzo che non
    /// riesce a tenere insieme le due cose e' un ragazzo su cui una squadra ci
    /// pensa due volte prima di investire.
    ///
    /// Si giudica una volta per anno scolastico: il campione con l'ultimo anno
    /// gia' giudicato serve a questo.
    /// </summary>
    private static void Scrutinio(CareerState career)
    {
        var oggi = career.StoryDate.Date;
        if (oggi.Month != 6 || oggi.Day < 10) return;
        if (Age(career) > 17) return;
        if (career.LastSchoolYearJudged >= oggi.Year) return;
        career.LastSchoolYearJudged = oggi.Year;

        var profile = career.ReputationProfile ??= new ReputationProfile();
        if (career.SchoolPerformance >= SogliaDiPromozione)
        {
            var eraRipetente = career.RepeatingYear;
            career.RepeatingYear = false;
            // Si riparte da poco sopra la sufficienza: il credito dell'anno
            // buono non si porta dietro, come nella realta'.
            career.SchoolPerformance = 55;
            var promosso = eraRipetente
                ? $"{career.Driver} recupera l'anno: da settembre niente piu' pomeriggi di recupero."
                : $"{career.Driver} e' promosso. L'estate e' libera, e il kartodromo pure.";
            career.News.Add(promosso);
            career.Events.Add(new CareerEventRecord
            {
                DateUtc = DateTime.UtcNow, StoryDate = oggi, Type = "SCHOOL_PASSED",
                Headline = promosso, Importance = 40
            });
            return;
        }

        career.SchoolFailures++;
        career.RepeatingYear = true;
        career.SchoolPerformance = 45;
        profile.Professionalism = Math.Clamp(profile.Professionalism - 10, 0, 100);
        profile.PublicPopularity = Math.Clamp(profile.PublicPopularity - 4, 0, 100);
        profile.SponsorAppeal = Math.Clamp(profile.SponsorAppeal - 5, 0, 100);
        profile.SyncLegacyFields(career);
        var bocciato = $"{career.Driver} e' bocciato. Da settembre due ore di recupero ogni pomeriggio: "
                       + $"{DriverDay.OreDiRecupero} ore al giorno che non si passano in pista.";
        career.News.Add(bocciato);
        career.Events.Add(new CareerEventRecord
        {
            DateUtc = DateTime.UtcNow, StoryDate = oggi, Type = "SCHOOL_FAILED",
            Headline = bocciato, Importance = 78
        });
    }

    public static void Complete(CareerState career, DailyCommitment item)
    {
        if (!item.Status.Equals("planned", StringComparison.OrdinalIgnoreCase) && !item.Status.Equals("active", StringComparison.OrdinalIgnoreCase)) return;
        item.Status = "done";
        var profile = career.ReputationProfile ??= new ReputationProfile();
        if (item.Kind == "school")
        {
            profile.Professionalism = Math.Clamp(profile.Professionalism + 1, 0, 100);
            // Esserci conta quanto studiare, quasi: chi frequenta arriva a
            // giugno con qualche punto di margine anche senza aprire un libro.
            career.SchoolPerformance = Math.Clamp(career.SchoolPerformance + 2, 0, 100);
            career.News.Add("Scuola conclusa: una giornata normale tenuta insieme alla carriera.");
        }
        else if (item.Kind == "track-training")
        {
            career.Fitness = Math.Clamp(career.Fitness + 6, 0, DriverDay.MaxFitness);
            career.Fatigue = Math.Clamp(career.Fatigue + 12, 0, DriverDay.MaxFatigue);
            profile.PublicPopularity = Math.Clamp(profile.PublicPopularity + 1, 0, 100);
            career.News.Add($"Allenamento in pista completato a {item.TrackName}: forma +6, stanchezza +12.");
        }
        else if (item.Kind == "sponsor-visit")
        {
            profile.SponsorAppeal = Math.Clamp(profile.SponsorAppeal + 3, 0, 100);
            profile.PublicPopularity = Math.Clamp(profile.PublicPopularity + 1, 0, 100);
            career.News.Add("Visita con Haru completata: un contatto locale ha ascoltato il progetto.");
        }
        else if (item.Kind == "fitness")
        {
            career.Fitness = Math.Clamp(career.Fitness + 3, 0, DriverDay.MaxFitness);
            career.Fatigue = Math.Clamp(career.Fatigue + 5, 0, DriverDay.MaxFatigue);
        }
        else
            career.Fatigue = Math.Clamp(career.Fatigue - 10, 0, DriverDay.MaxFatigue);
        profile.SyncLegacyFields(career);
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = item.Kind == "school" ? "SCHOOL_DONE" : "TRAINING_DONE", Headline = item.Title + " completato.", Track = item.TrackName, Importance = 28 });
    }

    public static void Skip(CareerState career, DailyCommitment item, bool automatic = false)
    {
        if (!item.Status.Equals("planned", StringComparison.OrdinalIgnoreCase) && !item.Status.Equals("active", StringComparison.OrdinalIgnoreCase)) return;
        item.Status = "skipped";
        var profile = career.ReputationProfile ??= new ReputationProfile();
        if (item.Kind == "track-training")
        {
            career.Fitness = Math.Clamp(career.Fitness - 5, 0, DriverDay.MaxFitness);
            profile.PublicPopularity = Math.Clamp(profile.PublicPopularity - 2, 0, 100);
        }
        else if (item.Kind == "school")
        {
            profile.Professionalism = Math.Clamp(profile.Professionalism - 2, 0, 100);
            profile.PublicPopularity = Math.Clamp(profile.PublicPopularity - 1, 0, 100);
            // Un'assenza pesa il doppio di una presenza: recuperare costa piu'
            // di quanto costi tenersi in pari.
            career.SchoolPerformance = Math.Clamp(career.SchoolPerformance - 4, 0, 100);
        }
        profile.SyncLegacyFields(career);
        var reason = automatic ? "non hai chiuso la giornata prima di far scorrere il calendario" : "hai scelto di saltarlo";
        career.News.Add($"{item.Title}: {reason}.");
        career.Events.Add(new CareerEventRecord { DateUtc = DateTime.UtcNow, StoryDate = career.StoryDate, Type = item.Kind == "school" ? "SCHOOL_SKIPPED" : "TRAINING_SKIPPED", Headline = $"{item.Title} saltato: {reason}.", Track = item.TrackName, Importance = 44 });
    }

    public static void ResolveUnfinished(CareerState career, ContentIndexRecord content)
    {
        foreach (var item in Today(career, content).Where(x => x.Required && x.Status is "planned" or "active").ToList())
            Skip(career, item, automatic: true);
    }

    /// <summary>Testo compatto per Home: orario, durata, obbligo e conseguenza.</summary>
    public static string ProgramText(CareerState career, ContentIndexRecord content)
    {
        var items = Today(career, content).OrderBy(x => x.StartTime).ToList();
        if (items.Count == 0) return "PROGRAMMA DI OGGI · giornata libera: scegli tu se allenarti, riposare o aiutare Haru.";
        var lines = new List<string> { "PROGRAMMA DI OGGI" };
        foreach (var x in items)
        {
            var status = x.Status == "done" ? "✓ fatto" : x.Status == "skipped" ? "— saltato" : x.Required ? "OBBLIGATORIO" : "FACOLTATIVO";
            var consequence = x.Kind switch
            {
                "school" => "assenza: affidabilità -2, followers -1",
                "track-training" => "salto: forma -5, followers -2",
                "fitness" => "salto: nessuna penalità",
                "recovery" => "salto: non recuperi energie",
                _ => "salto: nessuna penalità"
            };
            lines.Add($"{x.StartTime} · {x.Title} · {x.Hours}h · {status} · {consequence}");
        }
        return string.Join("\n", lines);
    }

    private static void Add(CareerState career, List<DailyCommitment> result, DailyCommitment item)
    {
        if (career.DailyCommitments.Any(x => x.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase))) return;
        career.DailyCommitments.Add(item); result.Add(item);
    }

    private static int Age(CareerState career) => career.BirthYear <= 0 ? 12 : Math.Max(10, career.StoryDate.Year - career.BirthYear);

    private static void Prune(CareerState career)
    {
        var cutoff = career.StoryDate.Date.AddDays(-180);
        career.DailyCommitments.RemoveAll(x => x.Date.Date < cutoff && x.Status is "done" or "skipped");
    }
}
