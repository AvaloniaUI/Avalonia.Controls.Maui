using System.Text.Json;
using System.Text.Json.Serialization;
using AlohaAI.Models;

namespace AlohaAI.Services;

public class ContentService : IContentService
{
    private PathsIndex? _cachedPaths;
    private readonly Dictionary<string, List<Module>> _cachedModules = new();
    private readonly Dictionary<string, Quiz> _cachedQuizzes = new();

    public async Task<List<LearningPath>> GetPathsAsync()
    {
        if (_cachedPaths != null)
            return _cachedPaths.Paths;

        var json = await LoadRawAssetAsync("paths.json");
        _cachedPaths = JsonSerializer.Deserialize<PathsIndex>(json, JsonContext.Default.PathsIndex) ?? new PathsIndex();
        return _cachedPaths.Paths;
    }

    public async Task<LearningPath?> GetPathAsync(string pathId)
    {
        var paths = await GetPathsAsync();
        return paths.FirstOrDefault(p => p.Id == pathId);
    }

    public async Task<List<Module>> GetModulesAsync(string pathId)
    {
        if (_cachedModules.TryGetValue(pathId, out var cached))
            return cached;

        var modules = new List<Module>();
        var pathJson = await LoadRawAssetAsync($"{pathId}/path.json");
        var pathMeta = JsonSerializer.Deserialize<PathModuleIndex>(pathJson, JsonContext.Default.PathModuleIndex);

        if (pathMeta?.Modules != null)
        {
            foreach (var moduleId in pathMeta.Modules)
            {
                var moduleJson = await LoadRawAssetAsync($"{pathId}/modules/{moduleId}/module.json");
                var module = JsonSerializer.Deserialize<Module>(moduleJson, JsonContext.Default.Module);
                if (module != null)
                    modules.Add(module);
            }
        }

        _cachedModules[pathId] = modules;
        return modules;
    }

    public async Task<Module?> GetModuleAsync(string pathId, string moduleId)
    {
        var modules = await GetModulesAsync(pathId);
        return modules.FirstOrDefault(m => m.Id == moduleId);
    }

    public async Task<string> GetLessonContentAsync(string pathId, string moduleId, string lessonFile)
    {
        return await LoadRawAssetAsync($"{pathId}/modules/{moduleId}/lessons/{lessonFile}");
    }

    public async Task<Quiz?> GetQuizAsync(string pathId, string moduleId)
    {
        var key = $"{pathId}/{moduleId}";
        if (_cachedQuizzes.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var json = await LoadRawAssetAsync($"{pathId}/modules/{moduleId}/quiz.json");
            var quiz = JsonSerializer.Deserialize<Quiz>(json, JsonContext.Default.Quiz);
            if (quiz != null)
                _cachedQuizzes[key] = quiz;
            return quiz;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string> LoadRawAssetAsync(string path)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}

internal class PathModuleIndex
{
    [System.Text.Json.Serialization.JsonPropertyName("modules")]
    public List<string> Modules { get; set; } = [];
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Quiz))]
[JsonSerializable(typeof(Module))]
[JsonSerializable(typeof(PathsIndex))]
[JsonSerializable(typeof(PathModuleIndex))]
internal partial class 
JsonContext : JsonSerializerContext
{
} 

