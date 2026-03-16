using System.Text.Json.Serialization;

namespace MyConference.Models;

public class Session
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("startsAt")]
    public DateTime? StartsAt { get; set; }

    [JsonPropertyName("endsAt")]
    public DateTime? EndsAt { get; set; }

    [JsonPropertyName("isServiceSession")]
    public bool IsServiceSession { get; set; }

    [JsonPropertyName("isPlenumSession")]
    public bool IsPlenumSession { get; set; }

    [JsonPropertyName("speakers")]
    public List<string> Speakers { get; set; } = [];

    [JsonPropertyName("categoryItems")]
    public List<int> CategoryItems { get; set; } = [];

    [JsonPropertyName("questionAnswers")]
    public List<QuestionAnswer> QuestionAnswers { get; set; } = [];

    [JsonPropertyName("roomId")]
    public int? RoomId { get; set; }

    [JsonPropertyName("liveUrl")]
    public string? LiveUrl { get; set; }

    [JsonPropertyName("recordingUrl")]
    public string? RecordingUrl { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("isInformed")]
    public bool IsInformed { get; set; }

    [JsonPropertyName("isConfirmed")]
    public bool IsConfirmed { get; set; }

    [JsonIgnore]
    public List<Speaker> SpeakerProfiles { get; set; } = [];

    [JsonIgnore]
    public string? RoomName { get; set; }

    [JsonIgnore]
    public bool IsFavorite { get; set; }

    [JsonIgnore]
    public string TimeSlot =>
        StartsAt.HasValue && EndsAt.HasValue
            ? $"{StartsAt.Value:HH:mm} \u2013 {EndsAt.Value:HH:mm}"
            : string.Empty;

    [JsonIgnore]
    public DateOnly? Day =>
        StartsAt.HasValue ? DateOnly.FromDateTime(StartsAt.Value) : null;

    [JsonIgnore]
    public string SpeakerNames =>
        string.Join(", ", SpeakerProfiles.Select(s => s.FullName));

    [JsonIgnore]
    public string? FirstSpeakerTagLine =>
        SpeakerProfiles.FirstOrDefault()?.TagLine;

    [JsonIgnore]
    public string StartTimeDisplay =>
        StartsAt.HasValue ? $"🕐 {StartsAt.Value:HH:mm}" : string.Empty;
}

public class QuestionAnswer
{
    [JsonPropertyName("questionId")]
    public int QuestionId { get; set; }

    [JsonPropertyName("answerValue")]
    public string? AnswerValue { get; set; }
}
