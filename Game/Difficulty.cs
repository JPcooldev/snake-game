namespace SnakeGame.Game;

// This enum represents the difficulty of the game and its methods

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public static class DifficultyExtensions
{
    // tick interval (milliseconds) based on difficulty
    // Easy:   180ms
    // Medium: 110ms
    // Hard:    70ms
    public static System.TimeSpan TickInterval(this Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => System.TimeSpan.FromMilliseconds(180),
        Difficulty.Hard => System.TimeSpan.FromMilliseconds(70),
        _ => System.TimeSpan.FromMilliseconds(110)
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
