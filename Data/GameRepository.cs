namespace SnakeGame.Data;

public sealed class GameRepository
{
    private readonly string _dbPath;

    public GameRepository(string dbPath)
    {
        _dbPath = dbPath;
        using var connection = Database.Open(_dbPath);
    }

    public void Save(GameRecord record)
    {
        using var connection = Database.Open(_dbPath);
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

    public System.Collections.Generic.IReadOnlyList<LeaderboardRow> Query(
        SnakeGame.Game.GameMode mode,
        SnakeGame.Game.Difficulty difficulty,
        LeaderboardMetric metric,
        string currentUsername)
    {
        using var connection = Database.Open(_dbPath);
        return metric switch
        {
            LeaderboardMetric.HighestScore => QueryFiltered(
                connection, mode, difficulty, "score DESC, steps ASC, duration_ms ASC"),
            LeaderboardMetric.LongestTime => QueryFiltered(
                connection, mode, difficulty, "duration_ms DESC, score DESC"),
            LeaderboardMetric.LongestSnake => QueryFiltered(
                connection, mode, difficulty, "length DESC, steps ASC"),
            LeaderboardMetric.FewestTicksToTen => QueryFiltered(
                connection, mode, difficulty, "steps ASC, duration_ms ASC", minScore: 10),
            LeaderboardMetric.PersonalBests => QueryPersonalBests(connection, currentUsername),
            _ => []
        };
    }

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

    private static System.Collections.Generic.List<LeaderboardRow> QueryFiltered(
        Microsoft.Data.Sqlite.SqliteConnection connection,
        SnakeGame.Game.GameMode mode,
        SnakeGame.Game.Difficulty difficulty,
        string orderBy,
        int? minScore = null)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"""
            SELECT p.username, g.score, g.steps, g.length, g.duration_ms, g.result, g.ended_at
            FROM games g
            JOIN players p ON p.id = g.player_id
            WHERE g.mode = @mode AND g.difficulty = @diff
              {(minScore is int min ? "AND g.score >= @minScore" : "")}
            ORDER BY {orderBy}
            LIMIT 15;
            """;
        cmd.Parameters.AddWithValue("@mode", SnakeGame.Game.GameModeExtensions.ToDb(mode));
        cmd.Parameters.AddWithValue("@diff", SnakeGame.Game.DifficultyExtensions.ToDb(difficulty));
        if (minScore is int score)
            cmd.Parameters.AddWithValue("@minScore", score);

        return ReadRows(cmd);
    }

    private static System.Collections.Generic.List<LeaderboardRow> QueryPersonalBests(Microsoft.Data.Sqlite.SqliteConnection connection, string username)
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

    private static System.Collections.Generic.List<LeaderboardRow> ReadRows(Microsoft.Data.Sqlite.SqliteCommand cmd, bool includeModeDifficulty = false)
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
