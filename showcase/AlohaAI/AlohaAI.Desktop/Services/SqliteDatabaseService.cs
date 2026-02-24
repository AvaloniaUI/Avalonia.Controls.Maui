using AlohaAI.Models;
using SQLite;

namespace AlohaAI.Services;

public class SqliteDatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db != null) return _db;
        var path = Path.Combine(FileSystem.AppDataDirectory, "alohaai.db");
        _db = new SQLiteAsyncConnection(path);
        await _db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "UserProgress" (
                "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
                "PathId" TEXT,
                "ModuleId" TEXT,
                "LessonId" TEXT,
                "Completed" INTEGER,
                "CompletedAt" TEXT,
                "QuizScore" REAL,
                "XpEarned" INTEGER
            )
            """);
        await _db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "UserStreaks" (
                "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
                "Date" TEXT UNIQUE,
                "LessonsCompleted" INTEGER
            )
            """);
        await _db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "UserSettings" (
                "Key" TEXT PRIMARY KEY,
                "Value" TEXT
            )
            """);
        return _db;
    }

    public async Task InitializeAsync()
    {
        await GetDbAsync();
    }

    public async Task<UserProgress?> FindProgressAsync(string pathId, string moduleId, string lessonId)
    {
        var db = await GetDbAsync();
        var results = await db.QueryAsync<UserProgress>(
            "SELECT * FROM \"UserProgress\" WHERE \"PathId\" = ? AND \"ModuleId\" = ? AND \"LessonId\" = ?",
            pathId, moduleId, lessonId);
        return results.FirstOrDefault();
    }

    public async Task<List<UserProgress>> GetCompletedProgressAsync(string? pathId, string? moduleId)
    {
        var db = await GetDbAsync();
        if (pathId != null && moduleId != null)
        {
            return await db.QueryAsync<UserProgress>(
                "SELECT * FROM \"UserProgress\" WHERE \"PathId\" = ? AND \"ModuleId\" = ? AND \"Completed\" = 1",
                pathId, moduleId);
        }
        if (pathId != null)
        {
            return await db.QueryAsync<UserProgress>(
                "SELECT * FROM \"UserProgress\" WHERE \"PathId\" = ? AND \"Completed\" = 1",
                pathId);
        }
        return await db.QueryAsync<UserProgress>(
            "SELECT * FROM \"UserProgress\" WHERE \"Completed\" = 1");
    }

    public async Task<UserProgress?> GetLastCompletedLessonAsync()
    {
        var db = await GetDbAsync();
        var results = await db.QueryAsync<UserProgress>(
            "SELECT * FROM \"UserProgress\" WHERE \"Completed\" = 1 AND \"LessonId\" != '__quiz__' ORDER BY \"CompletedAt\" DESC LIMIT 1");
        return results.FirstOrDefault();
    }

    public async Task SaveProgressAsync(UserProgress progress)
    {
        var db = await GetDbAsync();
        if (progress.Id == 0)
        {
            await db.ExecuteAsync(
                "INSERT INTO \"UserProgress\" (\"PathId\", \"ModuleId\", \"LessonId\", \"Completed\", \"CompletedAt\", \"QuizScore\", \"XpEarned\") VALUES (?, ?, ?, ?, ?, ?, ?)",
                progress.PathId, progress.ModuleId, progress.LessonId, progress.Completed, progress.CompletedAt, progress.QuizScore, progress.XpEarned);
        }
        else
        {
            await db.ExecuteAsync(
                "UPDATE \"UserProgress\" SET \"PathId\" = ?, \"ModuleId\" = ?, \"LessonId\" = ?, \"Completed\" = ?, \"CompletedAt\" = ?, \"QuizScore\" = ?, \"XpEarned\" = ? WHERE \"Id\" = ?",
                progress.PathId, progress.ModuleId, progress.LessonId, progress.Completed, progress.CompletedAt, progress.QuizScore, progress.XpEarned, progress.Id);
        }
    }

    public async Task DeleteAllProgressAsync()
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync("DELETE FROM \"UserProgress\"");
    }

    public async Task<UserStreak?> FindStreakByDateAsync(string date)
    {
        var db = await GetDbAsync();
        var results = await db.QueryAsync<UserStreak>(
            "SELECT * FROM \"UserStreaks\" WHERE \"Date\" = ?", date);
        return results.FirstOrDefault();
    }

    public async Task<List<UserStreak>> GetAllStreaksAsync(bool descending)
    {
        var db = await GetDbAsync();
        var sql = descending
            ? "SELECT * FROM \"UserStreaks\" ORDER BY \"Date\" DESC"
            : "SELECT * FROM \"UserStreaks\" ORDER BY \"Date\" ASC";
        return await db.QueryAsync<UserStreak>(sql);
    }

    public async Task SaveStreakAsync(UserStreak streak)
    {
        var db = await GetDbAsync();
        if (streak.Id == 0)
        {
            await db.ExecuteAsync(
                "INSERT INTO \"UserStreaks\" (\"Date\", \"LessonsCompleted\") VALUES (?, ?)",
                streak.Date, streak.LessonsCompleted);
        }
        else
        {
            await db.ExecuteAsync(
                "UPDATE \"UserStreaks\" SET \"Date\" = ?, \"LessonsCompleted\" = ? WHERE \"Id\" = ?",
                streak.Date, streak.LessonsCompleted, streak.Id);
        }
    }

    public async Task DeleteAllStreaksAsync()
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync("DELETE FROM \"UserStreaks\"");
    }

    public async Task<UserSetting?> GetSettingAsync(string key)
    {
        var db = await GetDbAsync();
        var results = await db.QueryAsync<UserSetting>(
            "SELECT * FROM \"UserSettings\" WHERE \"Key\" = ?", key);
        return results.FirstOrDefault();
    }

    public async Task SaveSettingAsync(UserSetting setting)
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync(
            "INSERT OR REPLACE INTO \"UserSettings\" (\"Key\", \"Value\") VALUES (?, ?)",
            setting.Key, setting.Value);
    }
}
