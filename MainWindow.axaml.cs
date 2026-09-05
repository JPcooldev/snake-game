using Avalonia.Controls;
using Avalonia.Input;
using SnakeGame.Data;
using SnakeGame.Game;
using SnakeGame.Views;

namespace SnakeGame;

public partial class MainWindow : Window
{
    public AppSession Session { get; } = new();
    public GameRepository Repository { get; }

    public MainWindow()
    {
        InitializeComponent();
        Repository = new GameRepository(AppPaths.DbPath);

        var settings = SettingsStore.Load();
        if (UsernameRules.TryNormalize(settings.LastUsername, out var name, out _))
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

    public void ShowUsername() => Root.Content = new UsernameView(this);

    public void ShowMenu() => Root.Content = new MenuView(this);

    public void ShowPlaySetup() => Root.Content = new PlaySetupView(this);

    public void ShowGame(GameMode mode, Difficulty difficulty)
    {
        Session.LastMode = mode;
        Session.LastDifficulty = difficulty;
        Root.Content = new GameView(this, mode, difficulty);
    }

    public void ShowLeaderboard() => Root.Content = new LeaderboardView(this);

    public void RememberUser(string username)
    {
        Session.Username = username;
        SettingsStore.Save(new AppSettings { LastUsername = username });
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (Root.Content is GameView game)
            game.HandleKey(e);
    }
}
