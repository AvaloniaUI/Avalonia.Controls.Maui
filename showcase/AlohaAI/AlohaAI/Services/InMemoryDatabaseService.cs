using AlohaAI.Models;

namespace AlohaAI.Services;

public class InMemoryDatabaseService : IDatabaseService
{
    private readonly List<UserProgress> _progress = new();
    private readonly List<UserStreak> _streaks = new();
    private readonly List<UserSetting> _settings = new();
    private int _nextProgressId = 1;
    private int _nextStreakId = 1;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task<UserProgress?> FindProgressAsync(string pathId, string moduleId, string lessonId)
    {
        var result = _progress.FirstOrDefault(p =>
            p.PathId == pathId && p.ModuleId == moduleId && p.LessonId == lessonId);
        return Task.FromResult(result);
    }

    public Task<List<UserProgress>> GetCompletedProgressAsync(string? pathId, string? moduleId)
    {
        IEnumerable<UserProgress> query = _progress.Where(p => p.Completed);
        if (pathId != null)
            query = query.Where(p => p.PathId == pathId);
        if (moduleId != null)
            query = query.Where(p => p.ModuleId == moduleId);
        return Task.FromResult(query.ToList());
    }

    public Task<UserProgress?> GetLastCompletedLessonAsync()
    {
        var result = _progress
            .Where(p => p.Completed && p.LessonId != "__quiz__")
            .OrderByDescending(p => p.CompletedAt)
            .FirstOrDefault();
        return Task.FromResult(result);
    }

    public Task SaveProgressAsync(UserProgress progress)
    {
        if (progress.Id == 0)
        {
            progress.Id = _nextProgressId++;
            _progress.Add(progress);
        }
        else
        {
            var index = _progress.FindIndex(p => p.Id == progress.Id);
            if (index >= 0)
                _progress[index] = progress;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAllProgressAsync()
    {
        _progress.Clear();
        return Task.CompletedTask;
    }

    public Task<UserStreak?> FindStreakByDateAsync(string date)
    {
        var result = _streaks.FirstOrDefault(s => s.Date == date);
        return Task.FromResult(result);
    }

    public Task<List<UserStreak>> GetAllStreaksAsync(bool descending)
    {
        var result = descending
            ? _streaks.OrderByDescending(s => s.Date).ToList()
            : _streaks.OrderBy(s => s.Date).ToList();
        return Task.FromResult(result);
    }

    public Task SaveStreakAsync(UserStreak streak)
    {
        if (streak.Id == 0)
        {
            streak.Id = _nextStreakId++;
            _streaks.Add(streak);
        }
        else
        {
            var index = _streaks.FindIndex(s => s.Id == streak.Id);
            if (index >= 0)
                _streaks[index] = streak;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAllStreaksAsync()
    {
        _streaks.Clear();
        return Task.CompletedTask;
    }

    public Task<UserSetting?> GetSettingAsync(string key)
    {
        var result = _settings.FirstOrDefault(s => s.Key == key);
        return Task.FromResult(result);
    }

    public Task SaveSettingAsync(UserSetting setting)
    {
        var index = _settings.FindIndex(s => s.Key == setting.Key);
        if (index >= 0)
            _settings[index] = setting;
        else
            _settings.Add(setting);
        return Task.CompletedTask;
    }
}
