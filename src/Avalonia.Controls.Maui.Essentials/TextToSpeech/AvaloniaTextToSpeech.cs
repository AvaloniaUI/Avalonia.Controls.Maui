using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="ITextToSpeech"/> with platform-specific
/// backends for desktop (espeak-ng / spd-say) and browser (Web Speech API).
/// </summary>
public sealed partial class AvaloniaTextToSpeech : ITextToSpeech
{
    readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <inheritdoc/>
    public async Task<IEnumerable<Locale>> GetLocalesAsync() =>
        await PlatformGetLocalesAsync().ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SpeakAsync(string text, SpeechOptions? options = null, CancellationToken cancelToken = default)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentNullException(nameof(text), "Text cannot be null or empty string");

        options ??= new SpeechOptions();

        if (options.Pitch.HasValue && (options.Pitch.Value < 0f || options.Pitch.Value > 2f))
            throw new ArgumentOutOfRangeException(nameof(SpeechOptions.Pitch), "Pitch must be between 0 and 2.");

        if (options.Volume.HasValue && (options.Volume.Value < 0f || options.Volume.Value > 1f))
            throw new ArgumentOutOfRangeException(nameof(SpeechOptions.Volume), "Volume must be between 0 and 1.");

        if (options.Rate.HasValue && (options.Rate.Value < 0.1f || options.Rate.Value > 2f))
            throw new ArgumentOutOfRangeException(nameof(SpeechOptions.Rate), "Rate must be between 0.1 and 2.");

        await _semaphore.WaitAsync(cancelToken).ConfigureAwait(false);
        try
        {
            await PlatformSpeakAsync(text, options, cancelToken).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private partial Task<IEnumerable<Locale>> PlatformGetLocalesAsync();
    private partial Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken);
}
