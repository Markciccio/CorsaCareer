namespace CorsaCareer;

public static class ContentCategoryRules
{
    public static bool IsRaceable(ContentCarRecord car) => !new[] { "road", "special" }.Contains(car.Category, StringComparer.OrdinalIgnoreCase);
}
