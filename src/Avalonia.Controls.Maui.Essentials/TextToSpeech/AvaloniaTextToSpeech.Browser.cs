using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

[SupportedOSPlatform("browser")]
public sealed partial class AvaloniaTextToSpeech
{
    private partial async Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
    {
        await SpeechSynthesisInterop.EnsureModuleLoadedAsync().ConfigureAwait(false);

        var json = await SpeechSynthesisInterop.GetVoicesJsonAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(json))
            return [];

        var voices = JsonSerializer.Deserialize(json, VoiceInfoJsonContext.Default.ListVoiceInfo);
        if (voices is null)
            return [];

        return voices.Select(v =>
        {
            var (lang, country) = ParseBcp47(v.Lang ?? string.Empty);
            return new Locale(lang, country, v.Name ?? string.Empty, v.VoiceURI ?? string.Empty);
        }).ToList();
    }

    private partial async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken)
    {
        await SpeechSynthesisInterop.EnsureModuleLoadedAsync().ConfigureAwait(false);

        var lang = BuildLanguageTag(options.Locale);
        var voiceId = options.Locale?.Id ?? string.Empty;
        var pitch = options.Pitch.HasValue ? (double)options.Pitch.Value : 1.0;
        var rate = options.Rate.HasValue ? (double)options.Rate.Value : 1.0;
        var volume = options.Volume.HasValue ? (double)options.Volume.Value : 1.0;

        using var registration = cancelToken.Register(() => SpeechSynthesisInterop.CancelSpeech());

        await SpeechSynthesisInterop.SpeakAsync(text, lang, voiceId, pitch, rate, volume)
            .ConfigureAwait(false);

        cancelToken.ThrowIfCancellationRequested();
    }

    static (string language, string country) ParseBcp47(string tag)
    {
        var parts = tag.Split('-', '_');
        var language = parts[0];
        var country = parts.Length > 1 ? parts[1].ToUpperInvariant() : string.Empty;
        return (language, country);
    }

    static string BuildLanguageTag(Locale? locale)
    {
        if (locale is null || string.IsNullOrWhiteSpace(locale.Language))
            return string.Empty;

        return string.IsNullOrWhiteSpace(locale.Country)
            ? locale.Language
            : $"{locale.Language}-{locale.Country}";
    }

    sealed class VoiceInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("lang")]
        public string? Lang { get; set; }

        [JsonPropertyName("voiceURI")]
        public string? VoiceURI { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }
    }

    [JsonSerializable(typeof(List<VoiceInfo>))]
    sealed partial class VoiceInfoJsonContext : JsonSerializerContext;
}
