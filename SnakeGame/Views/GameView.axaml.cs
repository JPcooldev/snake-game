namespace SnakeGame.Views;

// This class represents the game view of the application
// It is used to display the game board, timer, score, and game over screen

public partial class GameView : Avalonia.Controls.UserControl
{
    // colors for the game board, snake, food, and empty cells
    private static readonly Avalonia.Media.IBrush EmptyColor = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1a1f2b"));
    private static readonly Avalonia.Media.IBrush HeadColor = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#34d399"));
    private static readonly Avalonia.Media.IBrush BodyColor = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#065f46"));
    private static readonly Avalonia.Media.IBrush FoodColor = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#fb7185"));

    // stores the main window and the game mode and difficulty
    // default values:
    // - mode: Wrap
    // - difficulty: Medium
    private readonly MainWindow _window = null!;
    private readonly SnakeGame.Game.GameMode _mode = SnakeGame.Game.GameMode.Wrap;
    private readonly SnakeGame.Game.Difficulty _difficulty = SnakeGame.Game.Difficulty.Medium;

    // stores the game board cells, timer, and game state
    private readonly Avalonia.Controls.Border[,] _cells = new Avalonia.Controls.Border[SnakeGame.Game.GameState.Size, SnakeGame.Game.GameState.Size];
    private readonly Avalonia.Threading.DispatcherTimer _timer = new();
    private SnakeGame.Game.GameState _state = null!;
    private bool _saved;

    // constructor initializes the game view
    public GameView() => InitializeComponent();

    // constructor initializes the game view with the main window, game mode, and difficulty
    public GameView(MainWindow window, SnakeGame.Game.GameMode mode, SnakeGame.Game.Difficulty difficulty) : this()
    {
        _window = window;
        _mode = mode;
        _difficulty = difficulty;
        // build the game board
        BuildBoard();
        // create a new game state
        _state = new SnakeGame.Game.GameState(mode, difficulty);
        Focusable = true;
        // add a handler for the timer tick
        _timer.Tick += (_, _) => Tick();
        StartRun();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    // handle key down event
    public void HandleKey(Avalonia.Input.KeyEventArgs e)
    {
        // if the game is over, do nothing
        if (_state.IsOver)
            return;

        // handle the movement keys (W, A, S, D) or (Up, Left, Down, Right)
        // and the space bar to pause the game
        switch (e.Key)
        {
            case Avalonia.Input.Key.Up or Avalonia.Input.Key.W:
                _state.QueueDirection(SnakeGame.Game.Direction.Up);
                e.Handled = true;
                break;
            case Avalonia.Input.Key.Down or Avalonia.Input.Key.S:
                _state.QueueDirection(SnakeGame.Game.Direction.Down);
                e.Handled = true;
                break;
            case Avalonia.Input.Key.Left or Avalonia.Input.Key.A:
                _state.QueueDirection(SnakeGame.Game.Direction.Left);
                e.Handled = true;
                break;
            case Avalonia.Input.Key.Right or Avalonia.Input.Key.D:
                _state.QueueDirection(SnakeGame.Game.Direction.Right);
                e.Handled = true;
                break;
            case Avalonia.Input.Key.Space:
                TogglePause();
                e.Handled = true;
                break;
        }
    }

    // start the game run
    private void StartRun()
    {
        // reset the saved flag (new game is not saved yet)
        _saved = false;
        // set the timer interval based on the difficulty
        _timer.Interval = SnakeGame.Game.DifficultyExtensions.TickInterval(_difficulty);
        Overlay.IsVisible = false;
        PlayAgainButton.IsVisible = false;
        Render();
        UpdateHud();
        _timer.Start();
        Focus();
    }

    // handle the timer tick
    private void Tick()
    {
        // step the game state
        _state.Step();
        // render the game board
        Render();
        UpdateHud();
        if (_state.IsOver)
            EndRun();
    }

    // toggle the game pause state
    private void TogglePause()
    {
        // if the game is over, do nothing
        if (_state.IsOver)
            return;

        // toggle the game pause state
        _state.TogglePause();
        if (_state.IsPaused)
        {
            // stop the timer
            _timer.Stop();
            // set the overlay title and body (show user the game is paused)
            OverlayTitle.Text = "Paused";
            OverlayBody.Text = "Space to resume. Menu saves this run as quit.";
            PlayAgainButton.IsVisible = false;
            Overlay.IsVisible = true;
        }
        else
        {
            Overlay.IsVisible = false;
            _timer.Start();
        }

        UpdateHud();
    }

    // end the game run
    private void EndRun()
    {
        _timer.Stop();
        SaveIfNeeded();
        // set the overlay title and body (show user the game result)
        OverlayTitle.Text = "Game over";
        OverlayBody.Text =
            $"{SnakeGame.Game.GameResultExtensions.ToLabel(_state.Result!.Value)}\n" +
            $"Score {_state.Score}  ·  Steps {_state.Steps}  ·  Time {SnakeGame.Data.Formatters.Duration(_state.Elapsed)}";
        PlayAgainButton.IsVisible = true;
        Overlay.IsVisible = true;
    }

    // save the game if it is not saved yet
    private void SaveIfNeeded()
    {
        // if the game is not saved yet and the game is over, save the game
        if (_saved || _state.Result is null)
            return;

        // save the game record to the database
        _window.Database.Save(new SnakeGame.Data.GameRecord
        {
            Username = _window.Session.Username,
            Mode = _state.Mode,
            Difficulty = _state.Difficulty,
            Score = _state.Score,
            Steps = _state.Steps,
            Length = _state.Length,
            Duration = _state.Elapsed,
            Result = _state.Result.Value,
            StartedAt = _state.StartedAt,
            EndedAt = _state.EndedAt ?? System.DateTimeOffset.UtcNow
        });
        _saved = true;
    }

    // handle the play again button click (when paused or game over)
    private void OnPlayAgain(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _state = new SnakeGame.Game.GameState(_mode, _difficulty);
        StartRun();
    }

    // handle the menu button click (when paused or game over)
    private void OnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!_state.IsOver)
            _state.Quit();
        SaveIfNeeded();
        _window.ShowMenu();
    }

    // update the HUD (Heads Up Display)
    private void UpdateHud()
    {
        TimerText.Text = SnakeGame.Data.Formatters.Duration(_state.Elapsed);
        MetaText.Text = $"{SnakeGame.Game.GameModeExtensions.ToLabel(_state.Mode)} · {SnakeGame.Game.DifficultyExtensions.ToLabel(_state.Difficulty)}";
        ScoreText.Text = $"{_window.Session.Username}   {_state.Score}";
    }

    // build the game board
    private void BuildBoard()
    {
        Board.Rows = SnakeGame.Game.GameState.Size;
        Board.Columns = SnakeGame.Game.GameState.Size;
        Board.Children.Clear();
        // create the game board cells
        for (var y = 0; y < SnakeGame.Game.GameState.Size; y++)
        {
            for (var x = 0; x < SnakeGame.Game.GameState.Size; x++)
            {
                var cell = new Avalonia.Controls.Border
                {
                    Margin = new Avalonia.Thickness(1),
                    CornerRadius = new Avalonia.CornerRadius(3),
                    Background = EmptyColor
                };
                _cells[x, y] = cell;
                Board.Children.Add(cell);
            }
        }
    }

    // render the game board
    private void Render()
    {
        // clear the game board
        for (var y = 0; y < SnakeGame.Game.GameState.Size; y++)
        {
            for (var x = 0; x < SnakeGame.Game.GameState.Size; x++)
                _cells[x, y].Background = EmptyColor;
        }

        // render the snake
        var first = true;
        foreach (var cell in _state.Snake)
        {
            _cells[cell.X, cell.Y].Background = first ? HeadColor : BodyColor;
            first = false;
        }

        // render the food
        if (_state.Food is { } food)
            _cells[food.X, food.Y].Background = FoodColor;
    }
}
