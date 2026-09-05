using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SnakeGame.Data;
using SnakeGame.Game;

namespace SnakeGame.Views;

public partial class GameView : UserControl
{
    private static readonly IBrush EmptyBrush = new SolidColorBrush(Color.Parse("#1a1f2b"));
    private static readonly IBrush HeadBrush = new SolidColorBrush(Color.Parse("#34d399"));
    private static readonly IBrush BodyBrush = new SolidColorBrush(Color.Parse("#065f46"));
    private static readonly IBrush FoodBrush = new SolidColorBrush(Color.Parse("#fb7185"));

    private readonly MainWindow _window = null!;
    private readonly GameMode _mode = GameMode.SolidWalls;
    private readonly Difficulty _difficulty = Difficulty.Medium;
    private readonly Border[,] _cells = new Border[GameState.Size, GameState.Size];
    private readonly DispatcherTimer _timer = new();
    private GameState _state = null!;
    private bool _saved;

    public GameView() => InitializeComponent();

    public GameView(MainWindow window, GameMode mode, Difficulty difficulty) : this()
    {
        _window = window;
        _mode = mode;
        _difficulty = difficulty;
        BuildBoard();
        _state = new GameState(mode, difficulty);
        Focusable = true;
        _timer.Tick += (_, _) => Tick();
        StartRun();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    public void HandleKey(KeyEventArgs e)
    {
        if (_state.IsOver)
            return;

        switch (e.Key)
        {
            case Key.Up or Key.W:
                _state.QueueDirection(Direction.Up);
                e.Handled = true;
                break;
            case Key.Down or Key.S:
                _state.QueueDirection(Direction.Down);
                e.Handled = true;
                break;
            case Key.Left or Key.A:
                _state.QueueDirection(Direction.Left);
                e.Handled = true;
                break;
            case Key.Right or Key.D:
                _state.QueueDirection(Direction.Right);
                e.Handled = true;
                break;
            case Key.Space:
                TogglePause();
                e.Handled = true;
                break;
        }
    }

    private void StartRun()
    {
        _saved = false;
        _timer.Interval = _difficulty.TickInterval();
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
            $"{_state.Result!.Value.ToLabel()}\n" +
            $"Score {_state.Score}  ·  Steps {_state.Steps}  ·  Time {Formatters.Duration(_state.Elapsed)}";
        PlayAgainButton.IsVisible = true;
        Overlay.IsVisible = true;
    }

    private void SaveIfNeeded()
    {
        if (_saved || _state.Result is null)
            return;

        _window.Repository.Save(new GameRecord
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
            EndedAt = _state.EndedAt ?? DateTimeOffset.UtcNow
        });
        _saved = true;
    }

    private void OnPlayAgain(object? sender, RoutedEventArgs e)
    {
        _state = new GameState(_mode, _difficulty);
        StartRun();
    }

    private void OnMenu(object? sender, RoutedEventArgs e)
    {
        if (!_state.IsOver)
            _state.Quit();
        SaveIfNeeded();
        _window.ShowMenu();
    }

    private void UpdateHud()
    {
        TimerText.Text = Formatters.Duration(_state.Elapsed);
        MetaText.Text = $"{_state.Mode.ToLabel()} · {_state.Difficulty.ToLabel()}";
        ScoreText.Text = $"{_window.Session.Username}   {_state.Score}";
    }

    private void BuildBoard()
    {
        Board.Rows = GameState.Size;
        Board.Columns = GameState.Size;
        Board.Children.Clear();
        for (var y = 0; y < GameState.Size; y++)
        {
            for (var x = 0; x < GameState.Size; x++)
            {
                var cell = new Border
                {
                    Margin = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Background = EmptyBrush
                };
                _cells[x, y] = cell;
                Board.Children.Add(cell);
            }
        }
    }

    private void Render()
    {
        for (var y = 0; y < GameState.Size; y++)
        {
            for (var x = 0; x < GameState.Size; x++)
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
