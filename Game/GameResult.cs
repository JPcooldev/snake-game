namespace SnakeGame.Game;

// This enum represents the result of a game and its methods (used to save to database)

// game results
// - HitWall: The snake hit a wall and died
// - HitSelf: The snake hit itself and died
// - Won: The snake filled the whole working area board
// - Quit: The game was quit
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
