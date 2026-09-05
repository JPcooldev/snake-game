using Avalonia.Controls;
using Avalonia.Interactivity;
using SnakeGame.Game;

namespace SnakeGame.Views;

public partial class PlaySetupView : UserControl
{
    private readonly MainWindow _window = null!;

    public PlaySetupView() => InitializeComponent();

    public PlaySetupView(MainWindow window) : this()
    {
        _window = window;

        SolidMode.IsChecked = window.Session.LastMode == GameMode.SolidWalls;
        WrapMode.IsChecked = window.Session.LastMode == GameMode.Wrap;
        EasyDiff.IsChecked = window.Session.LastDifficulty == Difficulty.Easy;
        MediumDiff.IsChecked = window.Session.LastDifficulty == Difficulty.Medium;
        HardDiff.IsChecked = window.Session.LastDifficulty == Difficulty.Hard;
    }

    private void OnStart(object? sender, RoutedEventArgs e)
    {
        var mode = WrapMode.IsChecked == true ? GameMode.Wrap : GameMode.SolidWalls;
        var difficulty = EasyDiff.IsChecked == true
            ? Difficulty.Easy
            : HardDiff.IsChecked == true
                ? Difficulty.Hard
                : Difficulty.Medium;
        _window.ShowGame(mode, difficulty);
    }

    private void OnBack(object? sender, RoutedEventArgs e) => _window.ShowMenu();
}
