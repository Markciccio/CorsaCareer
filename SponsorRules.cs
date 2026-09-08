namespace CorsaCareer;

public static class SponsorRules
{
    public static int RequiredResults(int rounds) => Math.Max(1, (int)Math.Ceiling(Math.Max(1, rounds) * 0.5));
    public static bool Qualifies(int position, bool dnf, int target) => !dnf && position > 0 && position <= Math.Max(1, target);
}
