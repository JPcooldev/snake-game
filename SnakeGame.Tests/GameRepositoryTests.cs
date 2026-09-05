using SnakeGame.Data;
using SnakeGame.Game;

namespace SnakeGame.Tests;

public class GameRepositoryTests
{
    [Fact]
    public void Save_then_leaderboard_returns_the_run()
    {
        var dir = Path.Combine(Path.GetTempPath(), "snake-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var repo = new GameRepository(Path.Combine(dir, "snake.db"));
            repo.Save(new GameRecord
            {
                Username = "jp",
                Mode = GameMode.SolidWalls,
                Difficulty = Difficulty.Medium,
                Score = 12,
                Steps = 40,
                Length = 15,
                Duration = TimeSpan.FromSeconds(18),
                Result = GameResult.HitSelf,
                StartedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                EndedAt = DateTimeOffset.UtcNow
            });

            var top = repo.Query(GameMode.SolidWalls, Difficulty.Medium, LeaderboardMetric.HighestScore, "jp");
            Assert.Single(top);
            Assert.Equal("jp", top[0].Username);
            Assert.Equal(12, top[0].Score);

            var ticks = repo.Query(GameMode.SolidWalls, Difficulty.Medium, LeaderboardMetric.FewestTicksToTen, "jp");
            Assert.Single(ticks);
            Assert.Equal(40, ticks[0].Steps);

            var wrap = repo.Query(GameMode.Wrap, Difficulty.Medium, LeaderboardMetric.HighestScore, "jp");
            Assert.Empty(wrap);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
