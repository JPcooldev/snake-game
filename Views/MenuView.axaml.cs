namespace SnakeGame.Views;

// This class represents the menu view of the application
// It is used to display the main menu options

public partial class MenuView : Avalonia.Controls.UserControl
{
    private readonly MainWindow _window = null!;

    public MenuView() => InitializeComponent();

    public MenuView(MainWindow window) : this()
    {
        _window = window;
        // set the hello text to the username
        HelloText.Text = $"Playing as {_window.Session.Username}";
    }
    
    // handle redirect button click. user can select: 
    // - play game
    // - leaderboard
    // - change user
    
    private void OnPlay(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _window.ShowPlaySetup();

    private void OnLeaderboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _window.ShowLeaderboard();

    private void OnChangeUser(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _window.ShowUsername();
}
