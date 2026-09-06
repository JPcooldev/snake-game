namespace SnakeGame;

// This class represents the session state of the application
// AppSession stores the username, last mode, and last difficulty
// Provides default values and server as a state for GUI

// Default values:
// - Username: empty string
// - LastMode: Wrap
// - LastDifficulty: Medium
public sealed class AppSession
{
    public string Username { get; set; } = string.Empty;
    public SnakeGame.Game.GameMode LastMode { get; set; } = SnakeGame.Game.GameMode.Wrap;
    public SnakeGame.Game.Difficulty LastDifficulty { get; set; } = SnakeGame.Game.Difficulty.Medium;
}
