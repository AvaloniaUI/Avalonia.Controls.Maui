using AlohaAI.Models;

namespace AlohaAI.Services;

public class ProgressService : IProgressService
{
    private readonly IDatabaseService _db;

    public ProgressService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
    }

    public async Task MarkLessonCompleteAsync(string pathId, string moduleId, string lessonId, int xp)
    {
        var existing = await _db.FindProgressAsync(pathId, moduleId, lessonId);

        if (existing != null)
        {
            existing.Completed = true;
            existing.CompletedAt = DateTime.UtcNow;
            existing.XpEarned = xp;
            await _db.SaveProgressAsync(existing);
        }
        else
        {
            await _db.SaveProgressAsync(new UserProgress
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
        var existing = await _db.FindProgressAsync(pathId, moduleId, "__quiz__");

        if (existing != null)
        {
            existing.QuizScore = score;
            existing.XpEarned = xp;
            existing.CompletedAt = DateTime.UtcNow;
            existing.Completed = true;
            await _db.SaveProgressAsync(existing);
        }
        else
        {
            await _db.SaveProgressAsync(new UserProgress
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
        var entry = await _db.FindProgressAsync(pathId, moduleId, lessonId);
        return entry is { Completed: true };
    }

    public async Task<int> GetCompletedLessonCountAsync(string pathId, string? moduleId = null)
    {
        var completed = await _db.GetCompletedProgressAsync(pathId, moduleId);
        return completed.Count(p => p.LessonId != "__quiz__");
    }

    public async Task<int> GetTotalXpAsync()
    {
        var all = await _db.GetCompletedProgressAsync(null, null);
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
        return await _db.GetLastCompletedLessonAsync();
    }

    public async Task ResetAllAsync()
    {
        await _db.DeleteAllProgressAsync();
        await _db.DeleteAllStreaksAsync();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _db.GetSettingAsync(key);
        return setting?.Value;
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        await _db.SaveSettingAsync(new UserSetting { Key = key, Value = value });
    }
}
