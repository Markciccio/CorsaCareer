namespace CorsaCareer;

public static class ContentCategoryRules
{
    // Una carriera non deve buttare fuori una piccola stradale solo perché non
    // è una vettura da competizione. Se è l'unico contenuto disponibile, una
    // utilitaria o una road car è un perfetto punto di partenza per test,
    // time attack e primi trofei locali. Restano escluse solo le voci marcate
    // espressamente come speciali/non guidabili.
    public static bool IsRaceable(ContentCarRecord car) =>
        !string.IsNullOrWhiteSpace(car.Id)
        && !car.Category.Equals("special", StringComparison.OrdinalIgnoreCase);
}
