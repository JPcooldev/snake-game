namespace SnakeGame.Data;

// Opens the SQLite file, ensures the schema exists, and saves/queries game records.

public sealed class GameDatabase
{
    private readonly string _dbPath;

    public GameDatabase(string dbPath)
    {
        _dbPath = dbPath;
        using var connection = Open();
    }

    // saves a game record to the database
    public void Save(GameRecord record)
    {
        using var connection = Open();
        using var tx = connection.BeginTransaction();
        var playerId = GetOrCreatePlayer(connection, tx, record.Username);

        using var insert = connection.CreateCommand();
        insert.Transaction = tx;
        insert.CommandText = """
            INSERT INTO games (
              player_id, mode, difficulty, score, steps, length,
              duration_ms, result, started_at, ended_at
            ) VALUES (
              @player, @mode, @diff, @score, @steps, @length,
              @duration, @result, @started, @ended
            );
            """;
        insert.Parameters.AddWithValue("@player", playerId);
        insert.Parameters.AddWithValue("@mode", SnakeGame.Game.GameModeExtensions.ToDb(record.Mode));
        insert.Parameters.AddWithValue("@diff", SnakeGame.Game.DifficultyExtensions.ToDb(record.Difficulty));
        insert.Parameters.AddWithValue("@score", record.Score);
        insert.Parameters.AddWithValue("@steps", record.Steps);
        insert.Parameters.AddWithValue("@length", record.Length);
        insert.Parameters.AddWithValue("@duration", (long)record.Duration.TotalMilliseconds);
        insert.Parameters.AddWithValue("@result", SnakeGame.Game.GameResultExtensions.ToDb(record.Result));
        insert.Parameters.AddWithValue("@started", record.StartedAt.ToString("O"));
        insert.Parameters.AddWithValue("@ended", record.EndedAt.ToString("O"));
        insert.ExecuteNonQuery();
        tx.Commit();
    }

    // queries the leaderboard for a given mode, difficulty, and metric
    public System.Collections.Generic.IReadOnlyList<LeaderboardRow> Query(
        SnakeGame.Game.GameMode mode,
        SnakeGame.Game.Difficulty difficulty,
        LeaderboardMetric metric,
        string currentUsername)
    {
        using var connection = Open();
        return metric switch
        {
            LeaderboardMetric.HighestScore => QueryFiltered(
                connection, mode, difficulty, "score DESC, steps ASC, duration_ms ASC"),
            LeaderboardMetric.LongestTime => QueryFiltered(
                connection, mode, difficulty, "duration_ms DESC, score DESC"),
            LeaderboardMetric.LongestSnake => QueryFiltered(
                connection, mode, difficulty, "length DESC, steps ASC"),
            LeaderboardMetric.PersonalBests => QueryPersonalBests(connection, currentUsername),
            _ => []
        };
    }

    // opens a connection to the database
    private Microsoft.Data.Sqlite.SqliteConnection Open()
    {
        var dir = System.IO.Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrEmpty(dir))
            System.IO.Directory.CreateDirectory(dir);

        var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        EnsureSchema(connection);
        return connection;
    }

    // ensures the schema is created
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

    // gets or creates a player in the database and returns the player id
    private static int GetOrCreatePlayer(
        Microsoft.Data.Sqlite.SqliteConnection connection,
        Microsoft.Data.Sqlite.SqliteTransaction tx,
        string username)
    {
        using (var find = connection.CreateCommand())
        {
            find.Transaction = tx;
            find.CommandText = "SELECT id FROM players WHERE username = @u COLLATE NOCASE;";
            find.Parameters.AddWithValue("@u", username);
            var existing = find.ExecuteScalar();
            if (existing is long id)
                return (int)id;
        }

        using var insert = connection.CreateCommand();
        insert.Transaction = tx;
        insert.CommandText = """
            INSERT INTO players (username, created_at)
            VALUES (@u, @t);
            SELECT last_insert_rowid();
            """;
        insert.Parameters.AddWithValue("@u", username);
        insert.Parameters.AddWithValue("@t", System.DateTimeOffset.UtcNow.ToString("O"));
        return System.Convert.ToInt32(insert.ExecuteScalar());
    }

    // queries the leaderboard for a given mode, difficulty, and metric
    // returns top 15 rows based on the order by clause
    private static System.Collections.Generic.List<LeaderboardRow> QueryFiltered(
        Microsoft.Data.Sqlite.SqliteConnection connection,
        SnakeGame.Game.GameMode mode,
        SnakeGame.Game.Difficulty difficulty,
        string orderBy)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"""
            SELECT p.username, g.score, g.steps, g.length, g.duration_ms, g.result, g.ended_at
            FROM games g
            JOIN players p ON p.id = g.player_id
            WHERE g.mode = @mode AND g.difficulty = @diff
            ORDER BY {orderBy}
            LIMIT 15;
            """;
        cmd.Parameters.AddWithValue("@mode", SnakeGame.Game.GameModeExtensions.ToDb(mode));
        cmd.Parameters.AddWithValue("@diff", SnakeGame.Game.DifficultyExtensions.ToDb(difficulty));

        return ReadRows(cmd);
    }

    // queries the personal bests for a given username
    private static System.Collections.Generic.List<LeaderboardRow> QueryPersonalBests(
        Microsoft.Data.Sqlite.SqliteConnection connection,
        string username)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT p.username, g.mode, g.difficulty, g.score, g.steps, g.length,
                   g.duration_ms, g.result, g.ended_at
            FROM games g
            JOIN players p ON p.id = g.player_id
            JOIN (
              SELECT player_id, mode, difficulty, MAX(score) AS best
              FROM games
              GROUP BY player_id, mode, difficulty
            ) best ON best.player_id = g.player_id
                 AND best.mode = g.mode
                 AND best.difficulty = g.difficulty
                 AND best.best = g.score
            WHERE p.username = @u COLLATE NOCASE
            GROUP BY g.mode, g.difficulty
            ORDER BY g.mode, g.difficulty;
            """;
        cmd.Parameters.AddWithValue("@u", username);
        return ReadRows(cmd, includeModeDifficulty: true);
    }

    // read the database and create a list of LeaderboardRow objects
    // includeModeDifficulty is used to include the mode and difficulty in the result (for personal bests)
    private static System.Collections.Generic.List<LeaderboardRow> ReadRows(
        Microsoft.Data.Sqlite.SqliteCommand cmd,
        bool includeModeDifficulty = false)
    {
        using var reader = cmd.ExecuteReader();
        var rows = new System.Collections.Generic.List<LeaderboardRow>();
        while (reader.Read())
        {
            rows.Add(new LeaderboardRow
            {
                Username = reader.GetString(0),
                Mode = includeModeDifficulty ? reader.GetString(1) : null,
                Difficulty = includeModeDifficulty ? reader.GetString(2) : null,
                Score = System.Convert.ToInt32(includeModeDifficulty ? reader.GetValue(3) : reader.GetValue(1)),
                Steps = System.Convert.ToInt32(includeModeDifficulty ? reader.GetValue(4) : reader.GetValue(2)),
                Length = System.Convert.ToInt32(includeModeDifficulty ? reader.GetValue(5) : reader.GetValue(3)),
                Duration = System.TimeSpan.FromMilliseconds(System.Convert.ToInt64(includeModeDifficulty ? reader.GetValue(6) : reader.GetValue(4))),
                Result = includeModeDifficulty ? reader.GetString(7) : reader.GetString(5),
                EndedAt = System.DateTimeOffset.Parse(includeModeDifficulty ? reader.GetString(8) : reader.GetString(6))
            });
        }

        return rows;
    }
}
