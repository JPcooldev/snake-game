namespace SnakeGame;

// This class represents the main window of the application
// It is used to display the main menu, game, leaderboard, and username view

public partial class MainWindow : Avalonia.Controls.Window
{
    // stores the session data (username, last mode, last difficulty)
    public AppSession Session { get; } = new();
    public SnakeGame.Data.GameDatabase Database { get; }

    public MainWindow()
    {
        // initialize the main window
        InitializeComponent();
        // create or get existing game database
        Database = new SnakeGame.Data.GameDatabase(SnakeGame.Data.AppPaths.DbPath);

        // load the settings from the JSON file (currently just username)
        var settings = SnakeGame.Data.SettingsStore.Load();
        // if the username is valid, show the menu
        if (SnakeGame.Game.UsernameRules.TryNormalize(settings.LastUsername, out var name, out _))
        {
            Session.Username = name;
            ShowMenu();
        }
        // if the username is not valid, show the username view
        else
        {
            ShowUsername();
        }

        KeyDown += OnWindowKeyDown;
    }
    // --- navigation methods ---

    // show username view
    public void ShowUsername() => Root.Content = new SnakeGame.Views.UsernameView(this);

    // show menu view
    public void ShowMenu() => Root.Content = new SnakeGame.Views.MenuView(this);

    // show play setup view (here the user can select the mode and difficulty)
    public void ShowPlaySetup() => Root.Content = new SnakeGame.Views.PlaySetupView(this);

    // show game view
    public void ShowGame(SnakeGame.Game.GameMode mode, SnakeGame.Game.Difficulty difficulty)
    {
        Session.LastMode = mode;
        Session.LastDifficulty = difficulty;
        // show the game view with the selected mode and difficulty
        Root.Content = new SnakeGame.Views.GameView(this, mode, difficulty);
    }

    // show leaderboard view
    public void ShowLeaderboard() => Root.Content = new SnakeGame.Views.LeaderboardView(this);

    // remember the username and save to settings
    public void RememberUser(string username)
    {
        Session.Username = username;
        SnakeGame.Data.SettingsStore.Save(new SnakeGame.Data.AppSettings { LastUsername = username });
    }

    // handle key down event
    private void OnWindowKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (Root.Content is SnakeGame.Views.GameView game)
            game.HandleKey(e);
    }
}
