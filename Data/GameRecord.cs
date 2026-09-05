namespace SnakeGame.Data;

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

public enum LeaderboardMetric
{
    HighestScore,
    LongestTime,
    LongestSnake,
    FewestTicksToTen,
    PersonalBests
}
