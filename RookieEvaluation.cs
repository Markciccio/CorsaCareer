namespace CorsaCareer1991;

public sealed record RookieEvaluationResult(int Score, string Status);

/// <summary>
/// Valuta il debutto usando esclusivamente i dati del referto reale.
/// Non genera prestazioni mancanti: un dato non disponibile vale neutro.
/// </summary>
public static class RookieEvaluationEngine
{
    public static RookieEvaluationResult Evaluate(int position, bool dnf, int classificationSize, int qualificationPosition, int teammatePosition, int laps)
    {
        var finishScore = dnf ? 20 : RelativeScore(position, classificationSize, 70);
        var qualifyingScore = qualificationPosition > 0 ? RelativeScore(qualificationPosition, classificationSize, 60) : 60;
        var teammateScore = teammatePosition <= 0 ? 60 : position <= 0 || dnf ? 35 : position < teammatePosition ? 100 : position == teammatePosition ? 60 : 25;
        var reliabilityScore = dnf ? 20 : laps > 0 ? 100 : 60;
        var score = Math.Clamp((finishScore * 45 + qualifyingScore * 25 + teammateScore * 20 + reliabilityScore * 10) / 100, 0, 100);
        var status = dnf ? "Da rivalutare dopo il debutto" : score >= 75 ? "Promettente" : score >= 50 ? "Da sviluppare" : "Sotto osservazione";
        return new RookieEvaluationResult(score, status);
    }

    private static int RelativeScore(int position, int size, int neutral)
    {
        if (position <= 0 || size <= 1) return neutral;
        return Math.Clamp(100 - ((position - 1) * 80 / (size - 1)), 20, 100);
    }
}
