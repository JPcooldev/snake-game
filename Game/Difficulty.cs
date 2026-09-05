namespace SnakeGame.Game;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public static class DifficultyExtensions
{
    public static TimeSpan TickInterval(this Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => TimeSpan.FromMilliseconds(180),
        Difficulty.Hard => TimeSpan.FromMilliseconds(70),
        _ => TimeSpan.FromMilliseconds(110)
    };

    public static string ToDb(this Difficulty difficulty) => difficulty.ToString().ToLowerInvariant();

    public static string ToLabel(this Difficulty difficulty) => difficulty.ToString();

    public static Difficulty ParseDb(string value) => value.ToLowerInvariant() switch
    {
        "easy" => Difficulty.Easy,
        "hard" => Difficulty.Hard,
        _ => Difficulty.Medium
    };
}
