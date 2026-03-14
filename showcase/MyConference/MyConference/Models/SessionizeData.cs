using System.Text.Json.Serialization;

namespace MyConference.Models;

public class SessionizeData
{
    [JsonPropertyName("sessions")]
    public List<Session> Sessions { get; set; } = [];

    [JsonPropertyName("speakers")]
    public List<Speaker> Speakers { get; set; } = [];

    [JsonPropertyName("rooms")]
    public List<Room> Rooms { get; set; } = [];

    [JsonPropertyName("categories")]
    public List<Category> Categories { get; set; } = [];

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = [];
}

public class Question
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("question")]
    public string QuestionText { get; set; } = string.Empty;

    [JsonPropertyName("questionType")]
    public string? QuestionType { get; set; }

    [JsonPropertyName("sort")]
    public int Sort { get; set; }
}
