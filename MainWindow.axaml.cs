namespace SnakeGame;

public partial class MainWindow : Avalonia.Controls.Window
{
    public AppSession Session { get; } = new();
    public SnakeGame.Data.GameRepository Repository { get; }

    public MainWindow()
    {
        InitializeComponent();
        Repository = new SnakeGame.Data.GameRepository(SnakeGame.Data.AppPaths.DbPath);

        var settings = SnakeGame.Data.SettingsStore.Load();
        if (SnakeGame.Game.UsernameRules.TryNormalize(settings.LastUsername, out var name, out _))
        {
            Session.Username = name;
            ShowMenu();
        }
        else
        {
            ShowUsername();
        }

        KeyDown += OnWindowKeyDown;
    }

    public void ShowUsername() => Root.Content = new SnakeGame.Views.UsernameView(this);

    public void ShowMenu() => Root.Content = new SnakeGame.Views.MenuView(this);

    public void ShowPlaySetup() => Root.Content = new SnakeGame.Views.PlaySetupView(this);

    public void ShowGame(SnakeGame.Game.GameMode mode, SnakeGame.Game.Difficulty difficulty)
    {
        Session.LastMode = mode;
        Session.LastDifficulty = difficulty;
        Root.Content = new SnakeGame.Views.GameView(this, mode, difficulty);
    }

    public void ShowLeaderboard() => Root.Content = new SnakeGame.Views.LeaderboardView(this);

    public void RememberUser(string username)
    {
        Session.Username = username;
        SnakeGame.Data.SettingsStore.Save(new SnakeGame.Data.AppSettings { LastUsername = username });
    }

    private void OnWindowKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (Root.Content is SnakeGame.Views.GameView game)
            game.HandleKey(e);
    }
}
