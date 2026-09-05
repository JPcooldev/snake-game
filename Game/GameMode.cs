namespace SnakeGame.Game;

public enum GameMode
{
    SolidWalls,
    Wrap
}

public static class GameModeExtensions
{
    public static string ToDb(this GameMode mode) => mode switch
    {
        GameMode.Wrap => "wrap",
        _ => "solid"
    };

    public static string ToLabel(this GameMode mode) => mode switch
    {
        GameMode.Wrap => "Wrap around",
        _ => "Solid walls"
    };

    public static GameMode ParseDb(string value) =>
        string.Equals(value, "wrap", System.StringComparison.OrdinalIgnoreCase)
            ? GameMode.Wrap
            : GameMode.SolidWalls;
}
