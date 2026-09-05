namespace SnakeGame.Data;

public static class AppPaths
{
    public static string DataDirectory
    {
        get
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string DbPath => Path.Combine(DataDirectory, "snake.db");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
}
