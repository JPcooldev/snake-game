using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SnakeGame.Data;
using SnakeGame.Game;

namespace SnakeGame.Views;

public partial class LeaderboardView : UserControl
{
    private readonly MainWindow _window = null!;
    private bool _ready;

    public LeaderboardView() => InitializeComponent();

    public LeaderboardView(MainWindow window) : this()
    {
        _window = window;

        ModeBox.ItemsSource = new[] { "Solid walls", "Wrap around" };
        DifficultyBox.ItemsSource = new[] { "Easy", "Medium", "Hard" };
        MetricBox.ItemsSource = new[]
        {
            "Highest score",
            "Longest survival time",
            "Longest snake",
            "Fewest ticks to score 10",
            "My personal bests"
        };

        ModeBox.SelectedIndex = window.Session.LastMode == GameMode.Wrap ? 1 : 0;
        DifficultyBox.SelectedIndex = window.Session.LastDifficulty switch
        {
            Difficulty.Easy => 0,
            Difficulty.Hard => 2,
            _ => 1
        };
        MetricBox.SelectedIndex = 0;
        _ready = true;
        Refresh();
    }

    private void OnFilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_ready)
            Refresh();
    }

    private void OnBack(object? sender, RoutedEventArgs e) => _window.ShowMenu();

    private void Refresh()
    {
        var mode = ModeBox.SelectedIndex == 1 ? GameMode.Wrap : GameMode.SolidWalls;
        var difficulty = DifficultyBox.SelectedIndex switch
        {
            0 => Difficulty.Easy,
            2 => Difficulty.Hard,
            _ => Difficulty.Medium
        };
        var metric = MetricBox.SelectedIndex switch
        {
            1 => LeaderboardMetric.LongestTime,
            2 => LeaderboardMetric.LongestSnake,
            3 => LeaderboardMetric.FewestTicksToTen,
            4 => LeaderboardMetric.PersonalBests,
            _ => LeaderboardMetric.HighestScore
        };

        var personal = metric == LeaderboardMetric.PersonalBests;
        PlayerHeader.Text = personal ? "Mode / difficulty" : "Player";
        CaptionText.Text = metric switch
        {
            LeaderboardMetric.FewestTicksToTen =>
                "Runs that reached at least 10 points, ranked by fewest snake ticks.",
            LeaderboardMetric.PersonalBests =>
                $"Best score for {_window.Session.Username} in each mode × difficulty.",
            _ => $"{MetricLabel(metric)} · {mode.ToLabel()} · {difficulty.ToLabel()}"
        };

        var rows = _window.Repository.Query(mode, difficulty, metric, _window.Session.Username);
        Rows.Children.Clear();

        if (rows.Count == 0)
        {
            Rows.Children.Add(new TextBlock
            {
                Text = "No runs yet.",
                Classes = { "muted" },
                Margin = new Avalonia.Thickness(0, 12, 0, 0)
            });
            return;
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("40,*,70,70,70,70,90,150"),
                Margin = new Avalonia.Thickness(0, 4, 0, 4)
            };
            AddCell(grid, 0, (i + 1).ToString());
            AddCell(grid, 1, personal ? $"{row.Mode} / {row.Difficulty}" : row.Username);
            AddCell(grid, 2, row.Score.ToString());
            AddCell(grid, 3, row.Steps.ToString());
            AddCell(grid, 4, row.Length.ToString());
            AddCell(grid, 5, Formatters.Duration(row.Duration));
            AddCell(grid, 6, row.Result);
            AddCell(grid, 7, row.EndedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            Rows.Children.Add(grid);
        }
    }

    private static void AddCell(Grid grid, int column, string text)
    {
        var block = new TextBlock
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(block, column);
        grid.Children.Add(block);
    }

    private static string MetricLabel(LeaderboardMetric metric) => metric switch
    {
        LeaderboardMetric.LongestTime => "Longest survival time",
        LeaderboardMetric.LongestSnake => "Longest snake",
        LeaderboardMetric.FewestTicksToTen => "Fewest ticks to score 10",
        LeaderboardMetric.PersonalBests => "Personal bests",
        _ => "Highest score"
    };
}
