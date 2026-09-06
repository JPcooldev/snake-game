namespace SnakeGame.Data;

// This class provides the paths to the data directory and the database file

public static class AppPaths
{
    public static string DataDirectory
    {
        get
        {
            var dir = System.IO.Path.Combine(System.AppContext.BaseDirectory, "data");
            System.IO.Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string DbPath => System.IO.Path.Combine(DataDirectory, "snake.db");
    public static string SettingsPath => System.IO.Path.Combine(DataDirectory, "settings.json");
}
