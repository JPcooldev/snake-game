namespace SnakeGame.Data;

public sealed class AppSettings
{
    public string? LastUsername { get; set; }
}

public static class SettingsStore
{
    public static AppSettings Load()
    {
        try
        {
            if (!System.IO.File.Exists(AppPaths.SettingsPath))
                return new AppSettings();
            var json = System.IO.File.ReadAllText(AppPaths.SettingsPath);
            return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (System.IO.IOException)
        {
            return new AppSettings();
        }
        catch (System.Text.Json.JsonException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(AppPaths.SettingsPath, json);
    }
}
