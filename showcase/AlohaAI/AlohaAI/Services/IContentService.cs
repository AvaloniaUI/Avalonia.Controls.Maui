namespace AlohaAI.Services;

public interface IContentService
{
    Task<List<Models.LearningPath>> GetPathsAsync();
    Task<Models.LearningPath?> GetPathAsync(string pathId);
    Task<List<Models.Module>> GetModulesAsync(string pathId);
    Task<Models.Module?> GetModuleAsync(string pathId, string moduleId);
    Task<string> GetLessonContentAsync(string pathId, string moduleId, string lessonFile);
    Task<Models.Quiz?> GetQuizAsync(string pathId, string moduleId);
}
