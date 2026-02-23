using System.Text.Json.Serialization;

namespace AlohaAI.Models;

public class Quiz
{
    [JsonPropertyName("moduleId")]
    public string ModuleId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Quiz";

    [JsonPropertyName("passingScore")]
    public int PassingScore { get; set; } = 70;

    [JsonPropertyName("questions")]
    public List<QuizQuestion> Questions { get; set; } = [];
}

public class QuizQuestion
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = [];

    [JsonPropertyName("correctIndex")]
    public int CorrectIndex { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("xp")]
    public int Xp { get; set; } = 5;
}

public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    FillInBlank,
    Sequence
}
