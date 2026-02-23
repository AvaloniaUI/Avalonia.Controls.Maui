using AlohaAI.Models;
using SQLite;

namespace AlohaAI.Services;

public class ProgressService : IProgressService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db != null) return _db;
        var path = Path.Combine(FileSystem.AppDataDirectory, "alohaai.db");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<UserProgress>();
        await _db.CreateTableAsync<UserStreak>();
        await _db.CreateTableAsync<UserSetting>();
        return _db;
    }

    public async Task InitializeAsync()
    {
        await GetDbAsync();
    }

    public async Task MarkLessonCompleteAsync(string pathId, string moduleId, string lessonId, int xp)
    {
        var db = await GetDbAsync();

        var existing = await db.Table<UserProgress>()
            .Where(p => p.PathId == pathId && p.ModuleId == moduleId && p.LessonId == lessonId)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Completed = true;
            existing.CompletedAt = DateTime.UtcNow;
            existing.XpEarned = xp;
            await db.UpdateAsync(existing);
        }
        else
        {
            await db.InsertAsync(new UserProgress
            {
                PathId = pathId,
                ModuleId = moduleId,
                LessonId = lessonId,
                Completed = true,
                CompletedAt = DateTime.UtcNow,
                XpEarned = xp
            });
        }
    }

    public async Task SaveQuizScoreAsync(string pathId, string moduleId, double score, int xp)
    {
        var db = await GetDbAsync();

        var existing = await db.Table<UserProgress>()
            .Where(p => p.PathId == pathId && p.ModuleId == moduleId && p.LessonId == "__quiz__")
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.QuizScore = score;
            existing.XpEarned = xp;
            existing.CompletedAt = DateTime.UtcNow;
            existing.Completed = true;
            await db.UpdateAsync(existing);
        }
        else
        {
            await db.InsertAsync(new UserProgress
            {
                PathId = pathId,
                ModuleId = moduleId,
                LessonId = "__quiz__",
                Completed = true,
                CompletedAt = DateTime.UtcNow,
                QuizScore = score,
                XpEarned = xp
            });
        }
    }

    public async Task<bool> IsLessonCompletedAsync(string pathId, string moduleId, string lessonId)
    {
        var db = await GetDbAsync();
        var entry = await db.Table<UserProgress>()
            .Where(p => p.PathId == pathId && p.ModuleId == moduleId && p.LessonId == lessonId && p.Completed)
            .FirstOrDefaultAsync();
        return entry != null;
    }

    public async Task<int> GetCompletedLessonCountAsync(string pathId, string? moduleId = null)
    {
        var db = await GetDbAsync();
        if (moduleId != null)
        {
            return await db.Table<UserProgress>()
                .Where(p => p.PathId == pathId && p.ModuleId == moduleId && p.Completed && p.LessonId != "__quiz__")
                .CountAsync();
        }
        return await db.Table<UserProgress>()
            .Where(p => p.PathId == pathId && p.Completed && p.LessonId != "__quiz__")
            .CountAsync();
    }

    public async Task<int> GetTotalXpAsync()
    {
        var db = await GetDbAsync();
        var all = await db.Table<UserProgress>().Where(p => p.Completed).ToListAsync();
        return all.Sum(p => p.XpEarned);
    }

    public async Task<double> GetPathProgressAsync(string pathId, int totalLessons)
    {
        if (totalLessons <= 0) return 0;
        var completed = await GetCompletedLessonCountAsync(pathId);
        return (double)completed / totalLessons;
    }

    public async Task<UserProgress?> GetLastCompletedLessonAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<UserProgress>()
            .Where(p => p.Completed && p.LessonId != "__quiz__")
            .OrderByDescending(p => p.CompletedAt)
            .FirstOrDefaultAsync();
    }

    public async Task ResetAllAsync()
    {
        var db = await GetDbAsync();
        await db.DeleteAllAsync<UserProgress>();
        await db.DeleteAllAsync<UserStreak>();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var db = await GetDbAsync();
        var setting = await db.Table<UserSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
        return setting?.Value;
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        var db = await GetDbAsync();
        var existing = await db.Table<UserSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
        if (existing != null)
        {
            existing.Value = value;
            await db.UpdateAsync(existing);
        }
        else
        {
            await db.InsertAsync(new UserSetting { Key = key, Value = value });
        }
    }
}
