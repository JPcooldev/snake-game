namespace SnakeGame.Views;

// This class represents the play setup view of the application
// It is used to display the mode and difficulty selection options

public partial class PlaySetupView : Avalonia.Controls.UserControl
{
    private readonly MainWindow _window = null!;

    public PlaySetupView() => InitializeComponent();

    public PlaySetupView(MainWindow window) : this()
    {
        _window = window;
        // set the checked state of the mode and difficulty options (defined in PlaySetupView.axaml)
        SolidMode.IsChecked = window.Session.LastMode == SnakeGame.Game.GameMode.SolidWalls;
        WrapMode.IsChecked = window.Session.LastMode == SnakeGame.Game.GameMode.Wrap;
        EasyDiff.IsChecked = window.Session.LastDifficulty == SnakeGame.Game.Difficulty.Easy;
        MediumDiff.IsChecked = window.Session.LastDifficulty == SnakeGame.Game.Difficulty.Medium;
        HardDiff.IsChecked = window.Session.LastDifficulty == SnakeGame.Game.Difficulty.Hard;
    }

    // handle the start button click
    private void OnStart(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // get the selected mode and difficulty
        var mode = WrapMode.IsChecked == true ? SnakeGame.Game.GameMode.Wrap : SnakeGame.Game.GameMode.SolidWalls;
        var difficulty = EasyDiff.IsChecked == true
            ? SnakeGame.Game.Difficulty.Easy
            : HardDiff.IsChecked == true
                ? SnakeGame.Game.Difficulty.Hard
                : SnakeGame.Game.Difficulty.Medium;
        // show the game view with the selected mode and difficulty
        _window.ShowGame(mode, difficulty);
    }

    // handle the back button click
    private void OnBack(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _window.ShowMenu();
}
