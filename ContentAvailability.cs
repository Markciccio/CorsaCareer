namespace CorsaCareer;

public static class ContentAvailability
{
    public static int RaceableCars(IEnumerable<ContentCarRecord> cars) => cars.Count(ContentCategoryRules.IsRaceable);

    public static string HomeStatus(IEnumerable<ContentCarRecord> cars, int tracks)
        => HomeStatus(RaceableCars(cars), tracks);

    public static string HomeStatus(int cars, int tracks)
    {
        if (cars <= 0 && tracks <= 0) return "CONTENUTI MANCANTI  •  installa almeno un’auto e un circuito";
        if (cars <= 0) return "CONTENUTI MANCANTI  •  installa almeno un’auto";
        if (tracks <= 0) return "CONTENUTI MANCANTI  •  installa almeno un circuito";
        if (cars == 1) return "CONTENUTI LIMITATI  •  serve una seconda auto da gara per creare una griglia reale";
        return "";
    }

    /// <summary>
    /// I contenuti nel pacchetto <c>ac-finto</c> servono esclusivamente per
    /// percorrere la carriera senza Assetto Corsa installato. Non devono mai
    /// essere presentati come contenuti realmente disponibili al giocatore.
    /// </summary>
    public static bool IsDebugFixture(ContentIndexRecord index) =>
        index.AssettoCorsaRoot.Replace('/', '\\').Contains("\\ac-finto", StringComparison.OrdinalIgnoreCase)
        || index.Cars.Any(x => x.SourcePath.Replace('/', '\\').Contains("\\ac-finto\\", StringComparison.OrdinalIgnoreCase));
}
