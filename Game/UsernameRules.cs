namespace SnakeGame.Game;

public static class UsernameRules
{
    private static readonly System.Text.RegularExpressions.Regex Allowed = new(@"^[A-Za-z0-9_ ]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public static bool TryNormalize(string? raw, out string username, out string error)
    {
        username = (raw ?? string.Empty).Trim();
        if (username.Length is < 3 or > 16)
        {
            error = "Username must be 3–16 characters.";
            return false;
        }

        if (!Allowed.IsMatch(username))
        {
            error = "Use letters, digits, spaces, or underscores.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
