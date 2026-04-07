using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaTextToSpeechTests
{
    [Fact]
    public async Task SpeakAsync_NullText_ThrowsArgumentNullException()
    {
        var sut = new AvaloniaTextToSpeech();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SpeakAsync(null!));
    }

    [Fact]
    public async Task SpeakAsync_EmptyText_ThrowsArgumentNullException()
    {
        var sut = new AvaloniaTextToSpeech();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SpeakAsync(string.Empty));
    }

    [Fact]
    public async Task SpeakAsync_WhitespaceOnlyText_ReachesRuntimeValidationPath()
    {
        var sut = new AvaloniaTextToSpeech();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.SpeakAsync("   ", cancelToken: cts.Token));
    }

    [Fact]
    public async Task SpeakAsync_InvalidPitch_ThrowsArgumentOutOfRange()
    {
        var sut = new AvaloniaTextToSpeech();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SpeakAsync("test", new SpeechOptions { Pitch = 3.0f }));
    }

    [Fact]
    public async Task SpeakAsync_InvalidVolume_ThrowsArgumentOutOfRange()
    {
        var sut = new AvaloniaTextToSpeech();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SpeakAsync("test", new SpeechOptions { Volume = 1.5f }));
    }

    [Fact]
    public async Task SpeakAsync_InvalidRate_ThrowsArgumentOutOfRange()
    {
        var sut = new AvaloniaTextToSpeech();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SpeakAsync("test", new SpeechOptions { Rate = 0.0f }));
    }

    [Fact]
    public async Task SpeakAsync_NegativePitch_ThrowsArgumentOutOfRange()
    {
        var sut = new AvaloniaTextToSpeech();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SpeakAsync("test", new SpeechOptions { Pitch = -0.5f }));
    }

    [Fact]
    public async Task SpeakAsync_ValidOptions_DoesNotThrowValidationError()
    {
        var sut = new AvaloniaTextToSpeech();

        // Valid boundary values should not throw validation errors.
        // SpeakAsync may throw PlatformNotSupportedException on systems without
        // TTS tools, but that is expected and not a validation failure.
        try
        {
            await sut.SpeakAsync("hello", new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 0.5f,
                Rate = 1.0f
            });
        }
        catch (PlatformNotSupportedException)
        {
            // Expected on systems without espeak-ng or spd-say
        }
    }

    [Fact]
    public async Task SpeakAsync_CancelledToken_ThrowsOperationCancelled()
    {
        var sut = new AvaloniaTextToSpeech();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.SpeakAsync("hello", cancelToken: cts.Token));
    }

    [Fact]
    public async Task GetLocalesAsync_ReturnsNonNull()
    {
        var sut = new AvaloniaTextToSpeech();

        var locales = await sut.GetLocalesAsync();

        Assert.NotNull(locales);
        // May be empty if no TTS tool is installed, but should never be null
    }

    [Fact]
    public void AvaloniaTextToSpeech_Implements_ITextToSpeech()
    {
        var sut = new AvaloniaTextToSpeech();

        Assert.IsAssignableFrom<ITextToSpeech>(sut);
    }
}
