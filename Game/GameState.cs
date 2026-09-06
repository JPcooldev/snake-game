namespace SnakeGame.Game;

// This class represents the game state
// It is used to store the game information and logic

public readonly record struct Cell(int X, int Y);

public sealed class GameState
{
    // grid size (20x20)
    public const int Size = 20;
    // snake body (linked list)
    private readonly System.Collections.Generic.LinkedList<Cell> _snake = new();
    // input queue (queue)
    private readonly System.Collections.Generic.Queue<Direction> _input = new();
    private readonly System.Random _rng;
    private readonly System.Diagnostics.Stopwatch _watch = new();

    public GameState(GameMode mode, Difficulty difficulty, System.Random? rng = null)
        : this(mode, difficulty, rng, InitialBody(), Direction.Right)
    {
        // spawn food on a random empty cell
        TrySpawnFood();
        // start the stopwatch
        _watch.Start();
    }

    private GameState(
        GameMode mode,
        Difficulty difficulty,
        System.Random? rng,
        System.Collections.Generic.IReadOnlyList<Cell> initialBody,
        Direction direction)
    {
        if (initialBody.Count == 0)
            throw new System.ArgumentException("Snake must have at least one cell.", nameof(initialBody));

        Mode = mode;
        Difficulty = difficulty;
        _rng = rng ?? System.Random.Shared;
        CurrentDirection = direction;
        StartedAt = System.DateTimeOffset.UtcNow;
        // initialize snake body (using DefaultBody() if not provided)
        foreach (var cell in initialBody)
            _snake.AddLast(cell);
    }

    private static System.Collections.Generic.List<Cell> InitialBody()
    {
        // Head at the centre, facing right, length 3.
        const int cx = Size / 2;
        const int cy = Size / 2;
        return [new Cell(cx, cy), new Cell(cx - 1, cy), new Cell(cx - 2, cy)];
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
        // if the game is over, do nothing
        if (IsOver)
            return;

        // get the last direction from the input queue
        var last = _input.Count > 0 ? System.Linq.Enumerable.Last(_input) : CurrentDirection;
        // if the direction is the same as the last direction, do nothing
        if (direction == last)
            return;
        // if the snake is longer than 1 cell and the direction is opposite to the last direction, do nothing
        if (_snake.Count > 1 && direction.IsOpposite(last))
            return;
        // if the input queue has 2 or more directions, do nothing
        // because the snake can only turn once per tick
        if (_input.Count >= 2)
            return;

        // add the direction to the input queue
        _input.Enqueue(direction);
    }

    public void Pause()
    {
        // if the game is over or already paused, do nothing
        if (IsOver || IsPaused)
            return;
        // set the game to paused
        IsPaused = true;
        // pause the stopwatch
        _watch.Stop();
    }

    public void Resume()
    {
        // if the game is over or not paused, do nothing
        if (IsOver || !IsPaused)
            return;
        // set the game to not paused
        IsPaused = false;
        // unpause the stopwatch
        _watch.Start();
    }

    public void TogglePause()
    {
        // if the game is paused, resume it
        if (IsPaused)
            Resume();
        // otherwise, pause it
        else
            Pause();
    }

    public void Quit() => Finish(GameResult.Quit);

    public void Step()
    {
        // if the game is paused or over, do nothing
        if (IsPaused || IsOver)
            return;

        // get the next direction from the input queue
        if (_input.Count > 0)
            CurrentDirection = _input.Dequeue();

        // get new position of the head
        var delta = CurrentDirection.Delta();
        var nx = Head.X + delta.X;
        var ny = Head.Y + delta.Y;

        // if the game mode is SolidWalls, check if the new head is out of bounds
        if (Mode == GameMode.SolidWalls)
        {
            if (nx < 0 || nx >= Size || ny < 0 || ny >= Size)
            {
                Finish(GameResult.HitWall);
                return;
            }
        }
        // if the game mode is Wrap, wrap the new head position around the edges of the board
        else
        {
            nx = Wrap(nx);
            ny = Wrap(ny);
        }

        var newHead = new Cell(nx, ny);
        // check if the new head lands on the food
        var grow = Food is { } food && food == newHead;

        // if the new head does not land on the food, remove the last cell from the snake body (move the snake forward)
        if (!grow)
            _snake.RemoveLast();

        // check if the new head lands on itself, if so, finish the game with HitSelf result
        if (_snake.Contains(newHead))
        {
            Finish(GameResult.HitSelf);
            return;
        }

        // add the new head to the snake body
        _snake.AddFirst(newHead);
        // increment the steps
        Steps++;

        // if the snake did not grow, do nothing
        if (!grow)
            return;

        // snake ate the food, increment the score
        Score++;
        // try to spawn new food
        // if no food is spawned, finish the game with Won result
        if (!TrySpawnFood())
            Finish(GameResult.Won);
    }

    public bool Occupies(Cell cell) => _snake.Contains(cell);

    private bool TrySpawnFood()
    {
        // get all empty cells in the grid
        var empty = EmptyCells();
        if (empty.Count == 0)
        {
            Food = null;
            return false;
        }

        // select a random empty cell and place the food on it
        Food = empty[_rng.Next(empty.Count)];
        return true;
    }

    private System.Collections.Generic.List<Cell> EmptyCells()
    {
        var occupied = new System.Collections.Generic.HashSet<Cell>(_snake);
        var empty = new System.Collections.Generic.List<Cell>(Size * Size - occupied.Count);
        // iterate over all cells in the grid and add the empty cells to the list
        // empty cells = not occupied by the snake
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
        // if the game is already over, do nothing
        if (IsOver)
            return;
        // set the game result
        Result = result;
        // set the game to not paused
        IsPaused = false;
        // stop the stopwatch
        _watch.Stop();
        EndedAt = System.DateTimeOffset.UtcNow;
    }
    
    // wrap the value around the edges of the board
    // mechanism:
    // - first apply modulo operation to get the remainder
    // - add Size (to make the number positive)
    // - apply modulo operation again to get the final value
    private static int Wrap(int value) => (value % Size + Size) % Size;
}
