using AlohaAI.Models;

namespace AlohaAI.Services;

public interface IDatabaseService
{
    Task InitializeAsync();

    // UserProgress
    Task<UserProgress?> FindProgressAsync(string pathId, string moduleId, string lessonId);
    Task<List<UserProgress>> GetCompletedProgressAsync(string? pathId, string? moduleId);
    Task<UserProgress?> GetLastCompletedLessonAsync();
    Task SaveProgressAsync(UserProgress progress);
    Task DeleteAllProgressAsync();

    // UserStreak
    Task<UserStreak?> FindStreakByDateAsync(string date);
    Task<List<UserStreak>> GetAllStreaksAsync(bool descending);
    Task SaveStreakAsync(UserStreak streak);
    Task DeleteAllStreaksAsync();

    // UserSetting
    Task<UserSetting?> GetSettingAsync(string key);
    Task SaveSettingAsync(UserSetting setting);
}
