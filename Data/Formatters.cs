namespace SnakeGame.Data;

public static class Formatters
{
    public static string Duration(System.TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00}";
        return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
    }
}
