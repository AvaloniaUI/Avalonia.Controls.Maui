namespace AlohaAI.Services;

public interface IStreakService
{
    Task RecordActivityAsync();
    Task<int> GetCurrentStreakAsync();
    Task<int> GetBestStreakAsync();
    Task<int> GetTodayLessonsCountAsync();
}
