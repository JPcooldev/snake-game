using SnakeGame.Game;

namespace SnakeGame.Tests;

public class GameStateTests
{
    private static readonly IReadOnlyList<Cell> Starter =
    [
        new Cell(10, 10),
        new Cell(9, 10),
        new Cell(8, 10)
    ];

    [Fact]
    public void Step_moves_forward_and_drops_the_tail()
    {
        var game = GameState.CreateForTests(GameMode.SolidWalls, Starter, Direction.Right, food: new Cell(0, 0));
        game.Step();

        Assert.Equal(new Cell(11, 10), game.Head);
        Assert.Equal(3, game.Length);
        Assert.False(game.Occupies(new Cell(8, 10)));
        Assert.Equal(1, game.Steps);
        Assert.Equal(0, game.Score);
        Assert.False(game.IsOver);
    }

    [Fact]
    public void Eating_food_grows_the_snake_and_spawns_another()
    {
        var game = GameState.CreateForTests(GameMode.SolidWalls, Starter, Direction.Right, food: new Cell(11, 10));
        game.Step();

        Assert.Equal(1, game.Score);
        Assert.Equal(4, game.Length);
        Assert.True(game.Occupies(new Cell(8, 10)));
        Assert.NotNull(game.Food);
        Assert.False(game.Occupies(game.Food!.Value));
    }

    [Fact]
    public void Reverse_into_the_body_is_ignored()
    {
        var game = GameState.CreateForTests(GameMode.SolidWalls, Starter, Direction.Right, food: new Cell(0, 0));
        game.QueueDirection(Direction.Left);
        game.Step();

        Assert.Equal(new Cell(11, 10), game.Head);
        Assert.Equal(Direction.Right, game.CurrentDirection);
    }

    [Fact]
    public void Solid_walls_kill_on_the_border()
    {
        var body = new[] { new Cell(0, 5), new Cell(1, 5), new Cell(2, 5) };
        var game = GameState.CreateForTests(GameMode.SolidWalls, body, Direction.Left, food: new Cell(10, 10));
        game.Step();

        Assert.True(game.IsOver);
        Assert.Equal(GameResult.HitWall, game.Result);
        Assert.Equal(0, game.Steps);
    }

    [Fact]
    public void Wrap_mode_comes_out_the_other_side()
    {
        var body = new[] { new Cell(0, 5), new Cell(1, 5), new Cell(2, 5) };
        var game = GameState.CreateForTests(GameMode.Wrap, body, Direction.Left, food: new Cell(10, 10));
        game.Step();

        Assert.False(game.IsOver);
        Assert.Equal(new Cell(GameState.Size - 1, 5), game.Head);
    }

    [Fact]
    public void Hitting_the_body_ends_the_run()
    {
        var body = new[]
        {
            new Cell(2, 2),
            new Cell(2, 1),
            new Cell(1, 1),
            new Cell(1, 2),
            new Cell(1, 3)
        };
        var game = GameState.CreateForTests(GameMode.SolidWalls, body, Direction.Left, food: new Cell(10, 10));
        game.Step();

        Assert.True(game.IsOver);
        Assert.Equal(GameResult.HitSelf, game.Result);
    }

    [Fact]
    public void Pause_blocks_movement_and_step_count()
    {
        var game = GameState.CreateForTests(GameMode.SolidWalls, Starter, Direction.Right, food: new Cell(0, 0));
        game.Pause();
        game.Step();

        Assert.Equal(new Cell(10, 10), game.Head);
        Assert.Equal(0, game.Steps);
        Assert.True(game.IsPaused);

        game.Resume();
        game.Step();
        Assert.Equal(1, game.Steps);
        Assert.False(game.IsPaused);
    }

    [Fact]
    public void Food_never_spawns_on_the_snake()
    {
        for (var seed = 0; seed < 50; seed++)
        {
            var game = new GameState(GameMode.SolidWalls, Difficulty.Easy, new Random(seed));
            Assert.NotNull(game.Food);
            Assert.False(game.Occupies(game.Food!.Value));
        }
    }

    [Fact]
    public void Username_rules_reject_short_and_weird_names()
    {
        Assert.False(UsernameRules.TryNormalize("ab", out _, out _));
        Assert.False(UsernameRules.TryNormalize("has-dash", out _, out _));
        Assert.True(UsernameRules.TryNormalize("  jp_1  ", out var name, out _));
        Assert.Equal("jp_1", name);
    }
}
