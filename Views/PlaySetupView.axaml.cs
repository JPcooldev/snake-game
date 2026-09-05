namespace SnakeGame.Views;

public partial class PlaySetupView : Avalonia.Controls.UserControl
{
    private readonly MainWindow _window = null!;

    public PlaySetupView() => InitializeComponent();

    public PlaySetupView(MainWindow window) : this()
    {
        _window = window;

        SolidMode.IsChecked = window.Session.LastMode == SnakeGame.Game.GameMode.SolidWalls;
        WrapMode.IsChecked = window.Session.LastMode == SnakeGame.Game.GameMode.Wrap;
        EasyDiff.IsChecked = window.Session.LastDifficulty == SnakeGame.Game.Difficulty.Easy;
        MediumDiff.IsChecked = window.Session.LastDifficulty == SnakeGame.Game.Difficulty.Medium;
        HardDiff.IsChecked = window.Session.LastDifficulty == SnakeGame.Game.Difficulty.Hard;
    }

    private void OnStart(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mode = WrapMode.IsChecked == true ? SnakeGame.Game.GameMode.Wrap : SnakeGame.Game.GameMode.SolidWalls;
        var difficulty = EasyDiff.IsChecked == true
            ? SnakeGame.Game.Difficulty.Easy
            : HardDiff.IsChecked == true
                ? SnakeGame.Game.Difficulty.Hard
                : SnakeGame.Game.Difficulty.Medium;
        _window.ShowGame(mode, difficulty);
    }

    private void OnBack(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _window.ShowMenu();
}
