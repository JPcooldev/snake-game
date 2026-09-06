namespace SnakeGame.Data;

public static class Database
{
    public static Microsoft.Data.Sqlite.SqliteConnection Open(string path)
    {
        var dir = System.IO.Path.GetDirectoryName(path);
        // create a directory for sqlite database if it doesn't exist
        if (!string.IsNullOrEmpty(dir))
            System.IO.Directory.CreateDirectory(dir);

        var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}");
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        EnsureSchema(connection);
        return connection;
    }

    private static void EnsureSchema(Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS players (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              username TEXT NOT NULL COLLATE NOCASE UNIQUE,
              created_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS games (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              player_id INTEGER NOT NULL REFERENCES players(id),
              mode TEXT NOT NULL,
              difficulty TEXT NOT NULL,
              score INTEGER NOT NULL,
              steps INTEGER NOT NULL,
              length INTEGER NOT NULL,
              duration_ms INTEGER NOT NULL,
              result TEXT NOT NULL,
              started_at TEXT NOT NULL,
              ended_at TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }
}
