namespace SnakeGame;

public sealed class AppSession
{
    public string Username { get; set; } = string.Empty;
    public SnakeGame.Game.GameMode LastMode { get; set; } = SnakeGame.Game.GameMode.SolidWalls;
    public SnakeGame.Game.Difficulty LastDifficulty { get; set; } = SnakeGame.Game.Difficulty.Medium;
}
