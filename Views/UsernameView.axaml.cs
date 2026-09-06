namespace SnakeGame.Views;

// This class represents the username view of the application
// It is used to display the username input field and submit button

public partial class UsernameView : Avalonia.Controls.UserControl
{
    private readonly MainWindow _window = null!;

    public UsernameView() => InitializeComponent();

    public UsernameView(MainWindow window) : this()
    {
        _window = window;
        NameBox.Text = window.Session.Username;
        // add a handler for the enter key
        NameBox.KeyDown += OnNameKeyDown;
        // lambda (sender, event args) => action
        // when the username view is attached to the visual tree, focus the name box
        AttachedToVisualTree += (_, _) => NameBox.Focus();
    }

    private void OnNameKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
            Submit();
    }

    // 
    private void OnContinue(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Submit();

    private void Submit()
    {
        // check if the username is valid
        if (!SnakeGame.Game.UsernameRules.TryNormalize(NameBox.Text, out var name, out var error))
        {
            ErrorText.Text = error;
            ErrorText.IsVisible = true;
            return;
        }

        // save the username to settings.json
        _window.RememberUser(name);
        _window.ShowMenu();
    }
}
