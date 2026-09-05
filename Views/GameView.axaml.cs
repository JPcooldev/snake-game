namespace SnakeGame.Views;

public partial class GameView : Avalonia.Controls.UserControl
{
    private static readonly Avalonia.Media.IBrush EmptyBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1a1f2b"));
    private static readonly Avalonia.Media.IBrush HeadBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#34d399"));
    private static readonly Avalonia.Media.IBrush BodyBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#065f46"));
    private static readonly Avalonia.Media.IBrush FoodBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#fb7185"));

    private readonly MainWindow _window = null!;
    private readonly SnakeGame.Game.GameMode _mode = SnakeGame.Game.GameMode.SolidWalls;
    private readonly SnakeGame.Game.Difficulty _difficulty = SnakeGame.Game.Difficulty.Medium;
    private readonly Avalonia.Controls.Border[,] _cells = new Avalonia.Controls.Border[SnakeGame.Game.GameState.Size, SnakeGame.Game.GameState.Size];
    private readonly Avalonia.Threading.DispatcherTimer _timer = new();
    private SnakeGame.Game.GameState _state = null!;
    private bool _saved;

    public GameView() => InitializeComponent();

    public GameView(MainWindow window, SnakeGame.Game.GameMode mode, SnakeGame.Game.Difficulty difficulty) : this()
    {
        _window = window;
        _mode = mode;
        _difficulty = difficulty;
        BuildBoard();
        _state = new SnakeGame.Game.GameState(mode, difficulty);
        Focusable = true;
        _timer.Tick += (_, _) => Tick();
        StartRun();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    public void HandleKey(Avalonia.Input.KeyEventArgs e)
    {
        if (_state.IsOver)
            return;

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

    private void StartRun()
    {
        _saved = false;
        _timer.Interval = SnakeGame.Game.DifficultyExtensions.TickInterval(_difficulty);
        Overlay.IsVisible = false;
        PlayAgainButton.IsVisible = false;
        Render();
        UpdateHud();
        _timer.Start();
        Focus();
    }

    private void Tick()
    {
        _state.Step();
        Render();
        UpdateHud();
        if (_state.IsOver)
            EndRun();
    }

    private void TogglePause()
    {
        if (_state.IsOver)
            return;

        _state.TogglePause();
        if (_state.IsPaused)
        {
            _timer.Stop();
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

    private void EndRun()
    {
        _timer.Stop();
        SaveIfNeeded();
        OverlayTitle.Text = "Game over";
        OverlayBody.Text =
            $"{SnakeGame.Game.GameResultExtensions.ToLabel(_state.Result!.Value)}\n" +
            $"Score {_state.Score}  ·  Steps {_state.Steps}  ·  Time {SnakeGame.Data.Formatters.Duration(_state.Elapsed)}";
        PlayAgainButton.IsVisible = true;
        Overlay.IsVisible = true;
    }

    private void SaveIfNeeded()
    {
        if (_saved || _state.Result is null)
            return;

        _window.Repository.Save(new SnakeGame.Data.GameRecord
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

    private void OnPlayAgain(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _state = new SnakeGame.Game.GameState(_mode, _difficulty);
        StartRun();
    }

    private void OnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!_state.IsOver)
            _state.Quit();
        SaveIfNeeded();
        _window.ShowMenu();
    }

    private void UpdateHud()
    {
        TimerText.Text = SnakeGame.Data.Formatters.Duration(_state.Elapsed);
        MetaText.Text = $"{SnakeGame.Game.GameModeExtensions.ToLabel(_state.Mode)} · {SnakeGame.Game.DifficultyExtensions.ToLabel(_state.Difficulty)}";
        ScoreText.Text = $"{_window.Session.Username}   {_state.Score}";
    }

    private void BuildBoard()
    {
        Board.Rows = SnakeGame.Game.GameState.Size;
        Board.Columns = SnakeGame.Game.GameState.Size;
        Board.Children.Clear();
        for (var y = 0; y < SnakeGame.Game.GameState.Size; y++)
        {
            for (var x = 0; x < SnakeGame.Game.GameState.Size; x++)
            {
                var cell = new Avalonia.Controls.Border
                {
                    Margin = new Avalonia.Thickness(1),
                    CornerRadius = new Avalonia.CornerRadius(3),
                    Background = EmptyBrush
                };
                _cells[x, y] = cell;
                Board.Children.Add(cell);
            }
        }
    }

    private void Render()
    {
        for (var y = 0; y < SnakeGame.Game.GameState.Size; y++)
        {
            for (var x = 0; x < SnakeGame.Game.GameState.Size; x++)
                _cells[x, y].Background = EmptyBrush;
        }

        var first = true;
        foreach (var cell in _state.Snake)
        {
            _cells[cell.X, cell.Y].Background = first ? HeadBrush : BodyBrush;
            first = false;
        }

        if (_state.Food is { } food)
            _cells[food.X, food.Y].Background = FoodBrush;
    }
}
