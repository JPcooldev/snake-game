namespace SnakeGame.Tests;

public class GameStateTests
{
    private static readonly System.Collections.Generic.IReadOnlyList<SnakeGame.Game.Cell> Starter =
    [
        new SnakeGame.Game.Cell(10, 10),
        new SnakeGame.Game.Cell(9, 10),
        new SnakeGame.Game.Cell(8, 10)
    ];

    [Xunit.Fact]
    public void Step_moves_forward_and_drops_the_tail()
    {
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.SolidWalls, Starter, SnakeGame.Game.Direction.Right, food: new SnakeGame.Game.Cell(0, 0));
        game.Step();

        Xunit.Assert.Equal(new SnakeGame.Game.Cell(11, 10), game.Head);
        Xunit.Assert.Equal(3, game.Length);
        Xunit.Assert.False(game.Occupies(new SnakeGame.Game.Cell(8, 10)));
        Xunit.Assert.Equal(1, game.Steps);
        Xunit.Assert.Equal(0, game.Score);
        Xunit.Assert.False(game.IsOver);
    }

    [Xunit.Fact]
    public void Eating_food_grows_the_snake_and_spawns_another()
    {
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.SolidWalls, Starter, SnakeGame.Game.Direction.Right, food: new SnakeGame.Game.Cell(11, 10));
        game.Step();

        Xunit.Assert.Equal(1, game.Score);
        Xunit.Assert.Equal(4, game.Length);
        Xunit.Assert.True(game.Occupies(new SnakeGame.Game.Cell(8, 10)));
        Xunit.Assert.NotNull(game.Food);
        Xunit.Assert.False(game.Occupies(game.Food!.Value));
    }

    [Xunit.Fact]
    public void Reverse_into_the_body_is_ignored()
    {
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.SolidWalls, Starter, SnakeGame.Game.Direction.Right, food: new SnakeGame.Game.Cell(0, 0));
        game.QueueDirection(SnakeGame.Game.Direction.Left);
        game.Step();

        Xunit.Assert.Equal(new SnakeGame.Game.Cell(11, 10), game.Head);
        Xunit.Assert.Equal(SnakeGame.Game.Direction.Right, game.CurrentDirection);
    }

    [Xunit.Fact]
    public void Solid_walls_kill_on_the_border()
    {
        var body = new[] { new SnakeGame.Game.Cell(0, 5), new SnakeGame.Game.Cell(1, 5), new SnakeGame.Game.Cell(2, 5) };
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.SolidWalls, body, SnakeGame.Game.Direction.Left, food: new SnakeGame.Game.Cell(10, 10));
        game.Step();

        Xunit.Assert.True(game.IsOver);
        Xunit.Assert.Equal(SnakeGame.Game.GameResult.HitWall, game.Result);
        Xunit.Assert.Equal(0, game.Steps);
    }

    [Xunit.Fact]
    public void Wrap_mode_comes_out_the_other_side()
    {
        var body = new[] { new SnakeGame.Game.Cell(0, 5), new SnakeGame.Game.Cell(1, 5), new SnakeGame.Game.Cell(2, 5) };
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.Wrap, body, SnakeGame.Game.Direction.Left, food: new SnakeGame.Game.Cell(10, 10));
        game.Step();

        Xunit.Assert.False(game.IsOver);
        Xunit.Assert.Equal(new SnakeGame.Game.Cell(SnakeGame.Game.GameState.Size - 1, 5), game.Head);
    }

    [Xunit.Fact]
    public void Hitting_the_body_ends_the_run()
    {
        var body = new[]
        {
            new SnakeGame.Game.Cell(2, 2),
            new SnakeGame.Game.Cell(2, 1),
            new SnakeGame.Game.Cell(1, 1),
            new SnakeGame.Game.Cell(1, 2),
            new SnakeGame.Game.Cell(1, 3)
        };
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.SolidWalls, body, SnakeGame.Game.Direction.Left, food: new SnakeGame.Game.Cell(10, 10));
        game.Step();

        Xunit.Assert.True(game.IsOver);
        Xunit.Assert.Equal(SnakeGame.Game.GameResult.HitSelf, game.Result);
    }

    [Xunit.Fact]
    public void Pause_blocks_movement_and_step_count()
    {
        var game = SnakeGame.Game.GameState.CreateForTests(SnakeGame.Game.GameMode.SolidWalls, Starter, SnakeGame.Game.Direction.Right, food: new SnakeGame.Game.Cell(0, 0));
        game.Pause();
        game.Step();

        Xunit.Assert.Equal(new SnakeGame.Game.Cell(10, 10), game.Head);
        Xunit.Assert.Equal(0, game.Steps);
        Xunit.Assert.True(game.IsPaused);

        game.Resume();
        game.Step();
        Xunit.Assert.Equal(1, game.Steps);
        Xunit.Assert.False(game.IsPaused);
    }

    [Xunit.Fact]
    public void Food_never_spawns_on_the_snake()
    {
        for (var seed = 0; seed < 50; seed++)
        {
            var game = new SnakeGame.Game.GameState(SnakeGame.Game.GameMode.SolidWalls, SnakeGame.Game.Difficulty.Easy, new System.Random(seed));
            Xunit.Assert.NotNull(game.Food);
            Xunit.Assert.False(game.Occupies(game.Food!.Value));
        }
    }

    [Xunit.Fact]
    public void Username_rules_reject_short_and_weird_names()
    {
        Xunit.Assert.False(SnakeGame.Game.UsernameRules.TryNormalize("ab", out _, out _));
        Xunit.Assert.False(SnakeGame.Game.UsernameRules.TryNormalize("has-dash", out _, out _));
        Xunit.Assert.True(SnakeGame.Game.UsernameRules.TryNormalize("  jp_1  ", out var name, out _));
        Xunit.Assert.Equal("jp_1", name);
    }
}
