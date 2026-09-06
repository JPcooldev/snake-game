namespace SnakeGame.Game;

// This class represents the username rules
// It is used to validate the username

public static class UsernameRules
{
    private static readonly System.Text.RegularExpressions.Regex Allowed = new(@"^[A-Za-z0-9_ ]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public static bool TryNormalize(string? raw, out string username, out string error)
    {
        // trim the raw username and check its length
        username = (raw ?? string.Empty).Trim();
        if (username.Length is < 3 or > 16)
        {
            error = "Username must be 3–16 characters.";
            return false;
        }

        // check if the username contains only allowed characters
        if (!Allowed.IsMatch(username))
        {
            error = "Use letters (a-z, A-Z), digits (0-9), spaces, or underscores.";
            return false;
        }
        // if the username is valid, return true and set the error to an empty string
        error = string.Empty;
        return true;
    }
}
