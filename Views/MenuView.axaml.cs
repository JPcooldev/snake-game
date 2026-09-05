using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SnakeGame.Views;

public partial class MenuView : UserControl
{
    private readonly MainWindow _window = null!;

    public MenuView() => InitializeComponent();

    public MenuView(MainWindow window) : this()
    {
        _window = window;
        HelloText.Text = $"Playing as {_window.Session.Username}";
    }

    private void OnPlay(object? sender, RoutedEventArgs e) => _window.ShowPlaySetup();

    private void OnLeaderboard(object? sender, RoutedEventArgs e) => _window.ShowLeaderboard();

    private void OnChangeUser(object? sender, RoutedEventArgs e) => _window.ShowUsername();
}
