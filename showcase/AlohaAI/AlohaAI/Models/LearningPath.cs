using System.Text.Json.Serialization;

namespace AlohaAI.Models;

public class LearningPath
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#5B8FD4";

    [JsonPropertyName("moduleCount")]
    public int ModuleCount { get; set; }

    [JsonPropertyName("estimatedHours")]
    public int EstimatedHours { get; set; }
}

public class PathsIndex
{
    [JsonPropertyName("paths")]
    public List<LearningPath> Paths { get; set; } = [];
}
