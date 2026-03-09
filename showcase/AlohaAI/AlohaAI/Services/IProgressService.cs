using AlohaAI.Models;

namespace AlohaAI.Services;

public interface IProgressService
{
    Task InitializeAsync();
    Task MarkLessonCompleteAsync(string pathId, string moduleId, string lessonId, int xp);
    Task SaveQuizScoreAsync(string pathId, string moduleId, double score, int xp);
    Task<bool> IsLessonCompletedAsync(string pathId, string moduleId, string lessonId);
    Task<int> GetCompletedLessonCountAsync(string pathId, string? moduleId = null);
    Task<int> GetTotalXpAsync();
    Task<double> GetPathProgressAsync(string pathId, int totalLessons);
    Task<UserProgress?> GetLastCompletedLessonAsync();
    Task ResetAllAsync();
    Task<string?> GetSettingAsync(string key);
    Task SaveSettingAsync(string key, string value);
}
