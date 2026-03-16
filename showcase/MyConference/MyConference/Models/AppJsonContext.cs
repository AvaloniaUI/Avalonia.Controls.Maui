using System.Text.Json.Serialization;

namespace MyConference.Models;

[JsonSerializable(typeof(SessionizeData))]
[JsonSerializable(typeof(List<Session>))]
[JsonSerializable(typeof(List<Speaker>))]
[JsonSerializable(typeof(List<Room>))]
[JsonSerializable(typeof(List<Category>))]
[JsonSerializable(typeof(List<Question>))]
[JsonSerializable(typeof(HashSet<string>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class AppJsonContext : JsonSerializerContext;
