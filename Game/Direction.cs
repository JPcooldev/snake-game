namespace SnakeGame.Game;

// This enum represents the direction of the snake and its methods

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

    // 20x20 grid: The origin (0,0) is at the top-left corner.
    // Coordinates: (x, y) where x is column (0..19), y is row (0..19)
    //
    //          x →
    //        0  1  2  ...  19
    //      0 .  .  .  ...   .
    //   y  1 .  .  .  ...   .
    //   ↓  2 .  .  .  ...   .
    //     ...
    //     19 .  .  .  ...   .
    //
    // Thus:
    // - (0,19) = leftmost cell on the bottom row.
    // - (19,0) = rightmost cell on the top row.
    // 
    // Direction.Up:    decreases y  (y-1)
    // Direction.Down:  increases y  (y+1)
    // Direction.Left:  decreases x  (x-1)
    // Direction.Right: increases x  (x+1)
    //
    public static Cell Delta(this Direction direction) => direction switch
    {
        Direction.Up => new Cell(0, -1),
        Direction.Down => new Cell(0, 1),
        Direction.Left => new Cell(-1, 0),
        Direction.Right => new Cell(1, 0),
        _ => throw new System.ArgumentOutOfRangeException(nameof(direction), direction, null)
    };
}
