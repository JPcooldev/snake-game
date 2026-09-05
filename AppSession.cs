using SnakeGame.Game;

namespace SnakeGame;

public sealed class AppSession
{
    public string Username { get; set; } = string.Empty;
    public GameMode LastMode { get; set; } = GameMode.SolidWalls;
    public Difficulty LastDifficulty { get; set; } = Difficulty.Medium;
}
