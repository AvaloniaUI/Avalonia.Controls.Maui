using System.Text.Json.Serialization;

namespace MyConference.Models;

public class Speaker
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("tagLine")]
    public string? TagLine { get; set; }

    [JsonPropertyName("profilePicture")]
    public string? ProfilePicture { get; set; }

    [JsonPropertyName("isTopSpeaker")]
    public bool IsTopSpeaker { get; set; }

    [JsonPropertyName("links")]
    public List<SpeakerLink> Links { get; set; } = [];

    [JsonPropertyName("sessions")]
    public List<int> Sessions { get; set; } = [];

    [JsonPropertyName("categoryItems")]
    public List<int> CategoryItems { get; set; } = [];

    [JsonPropertyName("questionAnswers")]
    public List<QuestionAnswer> QuestionAnswers { get; set; } = [];
}

public class SpeakerLink
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("linkType")]
    public string? LinkType { get; set; }
}
