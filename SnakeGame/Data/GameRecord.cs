namespace SnakeGame.Data;

// This class represents a record of a game
// It is used to store the game information in the database

// GameRecord is defined by the following properties:
// - Username: the username of the player who played the game
// - Mode: the game mode (SolidWalls or Wrap)
// - Difficulty: the difficulty of the game (Easy, Medium, Hard)
// - Score: the score of the game (number of food eaten)
// - Steps: the number of steps taken (number of ticks)
// - Length: the length of the snake
// - Duration: the duration of the game
// - Result: the result of the game (HitWall, HitSelf, Won, Quit)
// - StartedAt: the timestamp when the game started
// - EndedAt: the timestamp when the game ended

public sealed class GameRecord
{
    public required string Username { get; init; }
    public required SnakeGame.Game.GameMode Mode { get; init; }
    public required SnakeGame.Game.Difficulty Difficulty { get; init; }
    public int Score { get; init; }
    public int Steps { get; init; }
    public int Length { get; init; }
    public System.TimeSpan Duration { get; init; }
    public required SnakeGame.Game.GameResult Result { get; init; }
    public System.DateTimeOffset StartedAt { get; init; }
    public System.DateTimeOffset EndedAt { get; init; }
}

// LeaderboardRow is defined by the following properties:
// - Username: the username of the player who played the game
// - Score: the score of the game (number of food eaten)
// - Steps: the number of steps taken (number of ticks)
// - Length: the length of the snake
// - Duration: the duration of the game
// - Result: the result of the game (HitWall, HitSelf, Won, Quit)
// - EndedAt: the timestamp when the game ended
// - Mode: the game mode (SolidWalls or Wrap)
// - Difficulty: the difficulty of the game (Easy, Medium, Hard)

public sealed class LeaderboardRow
{
    public required string Username { get; init; }
    public int Score { get; init; }
    public int Steps { get; init; }
    public int Length { get; init; }
    public System.TimeSpan Duration { get; init; }
    public required string Result { get; init; }
    public System.DateTimeOffset EndedAt { get; init; }
    public string? Mode { get; init; }
    public string? Difficulty { get; init; }
}

// We can display different metrics on the leaderboard
// - HighestScore: the highest score
// - LongestTime: the longest time
// - LongestSnake: the longest snake
// - PersonalBests: the personal bests for the current user
public enum LeaderboardMetric
{
    HighestScore,
    LongestTime,
    LongestSnake,
    PersonalBests
}
