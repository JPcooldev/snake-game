namespace SnakeGame.Tests;

public class GameRepositoryTests
{
    [Xunit.Fact]
    public void Save_then_leaderboard_returns_the_run()
    {
        var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "snake-tests-" + System.Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(dir);
        try
        {
            var repo = new SnakeGame.Data.GameRepository(System.IO.Path.Combine(dir, "snake.db"));
            repo.Save(new SnakeGame.Data.GameRecord
            {
                Username = "jp",
                Mode = SnakeGame.Game.GameMode.SolidWalls,
                Difficulty = SnakeGame.Game.Difficulty.Medium,
                Score = 12,
                Steps = 40,
                Length = 15,
                Duration = System.TimeSpan.FromSeconds(18),
                Result = SnakeGame.Game.GameResult.HitSelf,
                StartedAt = System.DateTimeOffset.UtcNow.AddMinutes(-1),
                EndedAt = System.DateTimeOffset.UtcNow
            });

            var top = repo.Query(SnakeGame.Game.GameMode.SolidWalls, SnakeGame.Game.Difficulty.Medium, SnakeGame.Data.LeaderboardMetric.HighestScore, "jp");
            Xunit.Assert.Single(top);
            Xunit.Assert.Equal("jp", top[0].Username);
            Xunit.Assert.Equal(12, top[0].Score);

            var ticks = repo.Query(SnakeGame.Game.GameMode.SolidWalls, SnakeGame.Game.Difficulty.Medium, SnakeGame.Data.LeaderboardMetric.FewestTicksToTen, "jp");
            Xunit.Assert.Single(ticks);
            Xunit.Assert.Equal(40, ticks[0].Steps);

            var wrap = repo.Query(SnakeGame.Game.GameMode.Wrap, SnakeGame.Game.Difficulty.Medium, SnakeGame.Data.LeaderboardMetric.HighestScore, "jp");
            Xunit.Assert.Empty(wrap);
        }
        finally
        {
            System.IO.Directory.Delete(dir, recursive: true);
        }
    }
}
