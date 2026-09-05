namespace SnakeGame.Game;

public sealed class GameState
{
    public const int Size = 20;

    private readonly System.Collections.Generic.LinkedList<Cell> _snake = new();
    private readonly System.Collections.Generic.Queue<Direction> _input = new();
    private readonly System.Random _rng;
    private readonly System.Diagnostics.Stopwatch _watch = new();

    public GameState(GameMode mode, Difficulty difficulty, System.Random? rng = null)
        : this(mode, difficulty, rng, DefaultBody(), Direction.Right)
    {
        TrySpawnFood();
        _watch.Start();
    }

    private GameState(
        GameMode mode,
        Difficulty difficulty,
        System.Random? rng,
        System.Collections.Generic.IReadOnlyList<Cell> bodyHeadFirst,
        Direction direction)
    {
        if (bodyHeadFirst.Count == 0)
            throw new System.ArgumentException("Snake must have at least one cell.", nameof(bodyHeadFirst));

        Mode = mode;
        Difficulty = difficulty;
        _rng = rng ?? System.Random.Shared;
        CurrentDirection = direction;
        StartedAt = System.DateTimeOffset.UtcNow;
        foreach (var cell in bodyHeadFirst)
            _snake.AddLast(cell);
    }

    public GameMode Mode { get; }
    public Difficulty Difficulty { get; }
    public Direction CurrentDirection { get; private set; }
    public Cell? Food { get; private set; }
    public int Score { get; private set; }
    public int Steps { get; private set; }
    public int Length => _snake.Count;
    public bool IsPaused { get; private set; }
    public bool IsOver => Result is not null;
    public GameResult? Result { get; private set; }
    public System.DateTimeOffset StartedAt { get; }
    public System.DateTimeOffset? EndedAt { get; private set; }
    public System.TimeSpan Elapsed => _watch.Elapsed;
    public Cell Head => _snake.First!.Value;
    public System.Collections.Generic.IReadOnlyCollection<Cell> Snake => _snake;

    public static GameState CreateForTests(
        GameMode mode,
        System.Collections.Generic.IReadOnlyList<Cell> bodyHeadFirst,
        Direction direction,
        Cell? food,
        Difficulty difficulty = Difficulty.Easy)
    {
        var state = new GameState(mode, difficulty, new System.Random(1), bodyHeadFirst, direction)
        {
            Food = food
        };
        state._watch.Start();
        return state;
    }

    public void QueueDirection(Direction direction)
    {
        if (IsOver)
            return;

        var last = _input.Count > 0 ? System.Linq.Enumerable.Last(_input) : CurrentDirection;
        if (direction == last)
            return;
        if (_snake.Count > 1 && direction.IsOpposite(last))
            return;
        if (_input.Count >= 2)
            return;

        _input.Enqueue(direction);
    }

    public void Pause()
    {
        if (IsOver || IsPaused)
            return;
        IsPaused = true;
        _watch.Stop();
    }

    public void Resume()
    {
        if (IsOver || !IsPaused)
            return;
        IsPaused = false;
        _watch.Start();
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Quit() => Finish(GameResult.Quit);

    public void Step()
    {
        if (IsPaused || IsOver)
            return;

        if (_input.Count > 0)
            CurrentDirection = _input.Dequeue();

        var delta = CurrentDirection.Delta();
        var nx = Head.X + delta.X;
        var ny = Head.Y + delta.Y;

        if (Mode == GameMode.SolidWalls)
        {
            if (nx < 0 || nx >= Size || ny < 0 || ny >= Size)
            {
                Finish(GameResult.HitWall);
                return;
            }
        }
        else
        {
            nx = Wrap(nx);
            ny = Wrap(ny);
        }

        var newHead = new Cell(nx, ny);
        var grow = Food is { } food && food == newHead;

        if (!grow)
            _snake.RemoveLast();

        if (_snake.Contains(newHead))
        {
            Finish(GameResult.HitSelf);
            return;
        }

        _snake.AddFirst(newHead);
        Steps++;

        if (!grow)
            return;

        Score++;
        if (!TrySpawnFood())
            Finish(GameResult.Won);
    }

    public bool Occupies(Cell cell) => _snake.Contains(cell);

    public void PlaceFood(Cell cell)
    {
        if (cell.X < 0 || cell.X >= Size || cell.Y < 0 || cell.Y >= Size)
            throw new System.ArgumentOutOfRangeException(nameof(cell));
        Food = cell;
    }

    private bool TrySpawnFood()
    {
        var empty = EmptyCells();
        if (empty.Count == 0)
        {
            Food = null;
            return false;
        }

        Food = empty[_rng.Next(empty.Count)];
        return true;
    }

    private System.Collections.Generic.List<Cell> EmptyCells()
    {
        var occupied = new System.Collections.Generic.HashSet<Cell>(_snake);
        var empty = new System.Collections.Generic.List<Cell>(Size * Size - occupied.Count);
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var cell = new Cell(x, y);
                if (!occupied.Contains(cell))
                    empty.Add(cell);
            }
        }

        return empty;
    }

    private void Finish(GameResult result)
    {
        if (IsOver)
            return;
        Result = result;
        IsPaused = false;
        _watch.Stop();
        EndedAt = System.DateTimeOffset.UtcNow;
    }

    private static System.Collections.Generic.List<Cell> DefaultBody()
    {
        // Head at the centre, facing right, length 3.
        const int cx = Size / 2;
        const int cy = Size / 2;
        return [new Cell(cx, cy), new Cell(cx - 1, cy), new Cell(cx - 2, cy)];
    }

    private static int Wrap(int value) => (value % Size + Size) % Size;
}
