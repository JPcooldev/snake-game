namespace SnakeGame.Views;

// This class represents the leaderboard view of the application
// It is used to display the leaderboard options and rows

public partial class LeaderboardView : Avalonia.Controls.UserControl
{
    private readonly MainWindow _window = null!;
    private bool _ready;

    public LeaderboardView() => InitializeComponent();

    public LeaderboardView(MainWindow window) : this()
    {
        _window = window;

        // show the mode, difficulty, and metric options
        ModeBox.ItemsSource = new[] { "Solid walls", "Wrap around" };
        DifficultyBox.ItemsSource = new[] { "Easy", "Medium", "Hard" };
        MetricBox.ItemsSource = new[]
        {
            "Highest score",
            "Longest survival time",
            "Longest snake",
            "My personal bests"
        };

        ModeBox.SelectedIndex = window.Session.LastMode == SnakeGame.Game.GameMode.Wrap ? 1 : 0;
        DifficultyBox.SelectedIndex = window.Session.LastDifficulty switch
        {
            SnakeGame.Game.Difficulty.Easy => 0,
            SnakeGame.Game.Difficulty.Hard => 2,
            _ => 1
        };
        MetricBox.SelectedIndex = 0;
        _ready = true;
        Refresh();
    }

    // handle the filter changed event
    private void OnFilterChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (_ready)
            Refresh();
    }

    // handle the back button click
    private void OnBack(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _window.ShowMenu();

    // refresh the leaderboard
    // get the selected mode, difficulty, and metric
    private void Refresh()
    {
        var mode = ModeBox.SelectedIndex == 1 ? SnakeGame.Game.GameMode.Wrap : SnakeGame.Game.GameMode.SolidWalls;
        var difficulty = DifficultyBox.SelectedIndex switch
        {
            0 => SnakeGame.Game.Difficulty.Easy,
            2 => SnakeGame.Game.Difficulty.Hard,
            _ => SnakeGame.Game.Difficulty.Medium
        };
        var metric = MetricBox.SelectedIndex switch
        {
            1 => SnakeGame.Data.LeaderboardMetric.LongestTime,
            2 => SnakeGame.Data.LeaderboardMetric.LongestSnake,
            3 => SnakeGame.Data.LeaderboardMetric.PersonalBests,
            _ => SnakeGame.Data.LeaderboardMetric.HighestScore
        };

        var personal = metric == SnakeGame.Data.LeaderboardMetric.PersonalBests;
        PlayerHeader.Text = personal ? "Mode / difficulty" : "Player";
        CaptionText.Text = metric switch
        {
            SnakeGame.Data.LeaderboardMetric.PersonalBests =>
                $"Best score for {_window.Session.Username} in each mode × difficulty.",
            _ => $"{MetricLabel(metric)} · {SnakeGame.Game.GameModeExtensions.ToLabel(mode)} · {SnakeGame.Game.DifficultyExtensions.ToLabel(difficulty)}"
        };

        // get the leaderboard rows
        var rows = _window.Database.Query(mode, difficulty, metric, _window.Session.Username);
        // clear the rows
        Rows.Children.Clear();

        // if there are no rows, show a message
        if (rows.Count == 0)
        {
            Rows.Children.Add(new Avalonia.Controls.TextBlock
            {
                Text = "No runs yet.",
                Classes = { "muted" },
                Margin = new Avalonia.Thickness(0, 12, 0, 0)
            });
            return;
        }

        // add the game runs rows to the grid
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var grid = new Avalonia.Controls.Grid
            {
                ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("40,*,70,70,70,70,90,150"),
                Margin = new Avalonia.Thickness(0, 4, 0, 4)
            };
            AddCell(grid, 0, (i + 1).ToString());
            AddCell(grid, 1, personal ? $"{row.Mode} / {row.Difficulty}" : row.Username);
            AddCell(grid, 2, row.Score.ToString());
            AddCell(grid, 3, row.Steps.ToString());
            AddCell(grid, 4, row.Length.ToString());
            AddCell(grid, 5, SnakeGame.Data.Formatters.Duration(row.Duration));
            AddCell(grid, 6, row.Result);
            AddCell(grid, 7, row.EndedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            Rows.Children.Add(grid);
        }
    }

    // add a cell to the grid
    private static void AddCell(Avalonia.Controls.Grid grid, int column, string text)
    {
        var block = new Avalonia.Controls.TextBlock
        {
            Text = text,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
        };
        Avalonia.Controls.Grid.SetColumn(block, column);
        grid.Children.Add(block);
    }

    // get the metric label
    private static string MetricLabel(SnakeGame.Data.LeaderboardMetric metric) => metric switch
    {
        SnakeGame.Data.LeaderboardMetric.LongestTime => "Longest survival time",
        SnakeGame.Data.LeaderboardMetric.LongestSnake => "Longest snake",
        SnakeGame.Data.LeaderboardMetric.PersonalBests => "Personal bests",
        _ => "Highest score"
    };
}
