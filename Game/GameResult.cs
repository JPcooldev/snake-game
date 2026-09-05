namespace SnakeGame.Game;

public enum GameResult
{
    HitWall,
    HitSelf,
    Won,
    Quit
}

public static class GameResultExtensions
{
    public static string ToDb(this GameResult result) => result switch
    {
        GameResult.HitWall => "wall",
        GameResult.HitSelf => "self",
        GameResult.Won => "won",
        GameResult.Quit => "quit",
        _ => "quit"
    };

    public static string ToLabel(this GameResult result) => result switch
    {
        GameResult.HitWall => "Hit a wall",
        GameResult.HitSelf => "Hit yourself",
        GameResult.Won => "Filled the board",
        GameResult.Quit => "Quit",
        _ => result.ToString()
    };
}
