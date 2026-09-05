namespace SnakeGame.Views;

public partial class UsernameView : Avalonia.Controls.UserControl
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

    private void OnNameKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
            Submit();
    }

    private void OnContinue(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Submit();

    private void Submit()
    {
        if (!SnakeGame.Game.UsernameRules.TryNormalize(NameBox.Text, out var name, out var error))
        {
            ErrorText.Text = error;
            ErrorText.IsVisible = true;
            return;
        }

        _window.RememberUser(name);
        _window.ShowMenu();
    }
}
