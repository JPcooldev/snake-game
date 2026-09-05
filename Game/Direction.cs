namespace SnakeGame.Game;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionExtensions
{
    public static bool IsOpposite(this Direction a, Direction b) => (a, b) switch
    {
        (Direction.Up, Direction.Down) or (Direction.Down, Direction.Up) => true,
        (Direction.Left, Direction.Right) or (Direction.Right, Direction.Left) => true,
        _ => false
    };

    public static Cell Delta(this Direction direction) => direction switch
    {
        Direction.Up => new Cell(0, -1),
        Direction.Down => new Cell(0, 1),
        Direction.Left => new Cell(-1, 0),
        Direction.Right => new Cell(1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };
}
