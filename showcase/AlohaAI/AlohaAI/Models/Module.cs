using System.Text.Json.Serialization;

namespace AlohaAI.Models;

public class Module
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("lessons")]
    public List<LessonInfo> Lessons { get; set; } = [];

    [JsonPropertyName("quiz")]
    public string Quiz { get; set; } = string.Empty;
}

public class LessonInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("file")]
    public string File { get; set; } = string.Empty;

    [JsonPropertyName("xp")]
    public int Xp { get; set; } = 10;
}
