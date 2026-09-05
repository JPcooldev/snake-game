using SnakeGame.Game;

namespace SnakeGame.Data;

public sealed class GameRecord
{
    public required string Username { get; init; }
    public required GameMode Mode { get; init; }
    public required Difficulty Difficulty { get; init; }
    public int Score { get; init; }
    public int Steps { get; init; }
    public int Length { get; init; }
    public TimeSpan Duration { get; init; }
    public required GameResult Result { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset EndedAt { get; init; }
}

public sealed class LeaderboardRow
{
    public required string Username { get; init; }
    public int Score { get; init; }
    public int Steps { get; init; }
    public int Length { get; init; }
    public TimeSpan Duration { get; init; }
    public required string Result { get; init; }
    public DateTimeOffset EndedAt { get; init; }
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
