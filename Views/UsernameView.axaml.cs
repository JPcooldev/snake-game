using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SnakeGame.Game;

namespace SnakeGame.Views;

public partial class UsernameView : UserControl
{
    private readonly MainWindow _window = null!;

    public UsernameView() => InitializeComponent();

    public UsernameView(MainWindow window) : this()
    {
        _window = window;
        NameBox.Text = window.Session.Username;
        NameBox.KeyDown += OnNameKeyDown;
        AttachedToVisualTree += (_, _) => NameBox.Focus();
    }

    private void OnNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Submit();
    }

    private void OnContinue(object? sender, RoutedEventArgs e) => Submit();

    private void Submit()
    {
        if (!UsernameRules.TryNormalize(NameBox.Text, out var name, out var error))
        {
            ErrorText.Text = error;
            ErrorText.IsVisible = true;
            return;
        }

        _window.RememberUser(name);
        _window.ShowMenu();
    }
}
