namespace Pump.Application.Common;

public static class AthleteProgressionCalculator
{
    private const int SMinimumXp = 10_000;
    private const int AMinimumXp = 5_000;
    private const int BMinimumXp = 2_000;

    public static string GetTier(int totalXp)
        => totalXp >= SMinimumXp ? "S"
            : totalXp >= AMinimumXp ? "A"
            : totalXp >= BMinimumXp ? "B"
            : "C";

    public static (int Level, double ProgressPercent) GetLevel(int totalXp)
    {
        var level = 1;
        var xpConsumed = 0;
        const int maxIterations = 1000;

        for (var i = 0; i < maxIterations; i++)
        {
            var xpForNextLevel = (int)(100 * Math.Pow(level, 1.5));
            if (totalXp < xpConsumed + xpForNextLevel)
            {
                var progress = (double)(totalXp - xpConsumed) / xpForNextLevel * 100;
                return (level, Math.Round(progress, 0));
            }

            xpConsumed += xpForNextLevel;
            level++;
        }

        return (level, 0);
    }
}
