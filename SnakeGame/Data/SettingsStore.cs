namespace SnakeGame.Data;

// These classes function as cache for the application settings (currently just username)

public sealed class AppSettings
{
    // stores the last username used
    public string? LastUsername { get; set; }
}

public static class SettingsStore
{
    // loads the settings from the JSON file
    public static AppSettings Load()
    {
        try
        {
            // if the settings file does not exist, return a new instance of AppSettings
            if (!System.IO.File.Exists(AppPaths.SettingsPath))
                return new AppSettings();
            // read and return the settings from the JSON file
            var json = System.IO.File.ReadAllText(AppPaths.SettingsPath);
            return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        // if the settings file is not readable, return a new instance of AppSettings
        catch (System.IO.IOException)
        {
            return new AppSettings();
        }
        // if the settings file is not valid JSON, return a new instance of AppSettings
        catch (System.Text.Json.JsonException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        // serialize the settings to JSON
        // we use WriteIndented = true to make the JSON more readable
        var json = System.Text.Json.JsonSerializer.Serialize(
            settings,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
        );
        // write the settings to the JSON file
        System.IO.File.WriteAllText(AppPaths.SettingsPath, json);
    }
}
