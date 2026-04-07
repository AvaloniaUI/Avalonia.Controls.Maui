using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

public sealed partial class AvaloniaTextToSpeech
{
    static string? _cachedTool;
    static bool _toolDetected;

    private partial async Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
    {
        var tool = await DetectToolAsync().ConfigureAwait(false);

        if (tool == "espeak-ng")
            return await GetEspeakLocalesAsync().ConfigureAwait(false);

        if (tool == "spd-say")
            return await GetSpdSayLocalesAsync().ConfigureAwait(false);

        return [];
    }

    private partial async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken)
    {
        var tool = await DetectToolAsync().ConfigureAwait(false);

        if (tool == "espeak-ng")
        {
            await SpeakWithEspeakAsync(text, options, cancelToken).ConfigureAwait(false);
            return;
        }

        if (tool == "spd-say")
        {
            await SpeakWithSpdSayAsync(text, options, cancelToken).ConfigureAwait(false);
            return;
        }

        throw new PlatformNotSupportedException(
            "Text-to-speech requires espeak-ng or speech-dispatcher. " +
            "Install with: sudo apt install espeak-ng (Debian/Ubuntu) or sudo dnf install espeak-ng (Fedora).");
    }

    static async Task<string?> DetectToolAsync()
    {
        if (_toolDetected)
            return _cachedTool;

        if (await IsToolAvailableAsync("espeak-ng").ConfigureAwait(false))
            _cachedTool = "espeak-ng";
        else if (await IsToolAvailableAsync("spd-say").ConfigureAwait(false))
            _cachedTool = "spd-say";

        _toolDetected = true;
        return _cachedTool;
    }

    static async Task<bool> IsToolAvailableAsync(string tool)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = tool,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    static async Task<string> RunProcessAsync(string fileName, string arguments, CancellationToken cancelToken = default)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancelToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancelToken);
        await process.WaitForExitAsync(cancelToken).ConfigureAwait(false);
        var output = await outputTask.ConfigureAwait(false);
        var error = await errorTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
            throw CreateProcessFailedException(fileName, process.ExitCode, error);

        return output;
    }

    // espeak-ng --voices output format (columns separated by 2+ spaces):
    // Pty  Language       Age/Gender  VoiceName          File          Other Languages
    //  5  af             M  Afrikaans            af
    //  5  en-gb          M  English_(Great_Britain) en-gb
    //  5  en-us          M  English (America)    en-us             (en 2)
    // The language tag (column 2) is always a single word with no spaces.
    // Voice names may contain spaces and parentheses.
    static async Task<IEnumerable<Locale>> GetEspeakLocalesAsync()
    {
        var output = await RunProcessAsync("espeak-ng", "--voices").ConfigureAwait(false);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var locales = new List<Locale>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            // Match: priority  language-tag  gender  voice-name...
            // Language tag is always the second whitespace-delimited token and contains only [a-z0-9-]
            var match = Regex.Match(line, @"^\s*\d+\s+([\w-]+)\s+[MF-]\s+(.+?)(?:\s{2,}|$)");
            if (!match.Success)
                continue;

            var languageTag = match.Groups[1].Value; // e.g. "en-us", "af"
            var voiceName = match.Groups[2].Value.Trim().Replace('_', ' '); // e.g. "English (America)"

            // Deduplicate by language tag — keep first occurrence
            if (!seen.Add(languageTag))
                continue;

            var (lang, country) = ParseLanguageTag(languageTag);
            locales.Add(new Locale(lang, country, voiceName, languageTag));
        }

        return locales;
    }

    // spd-say -L output format (columns separated by 2+ spaces):
    // NAME                   LANGUAGE       VARIANT
    // English_(America)      en-US          NONE
    // Voice names may contain spaces. Language code is always a BCP-47 tag (letters, hyphens).
    static async Task<IEnumerable<Locale>> GetSpdSayLocalesAsync()
    {
        var output = await RunProcessAsync("spd-say", "-L").ConfigureAwait(false);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var locales = new List<Locale>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.TrimStart().StartsWith("NAME", StringComparison.OrdinalIgnoreCase))
                continue;

            // Find the language code: a standalone BCP-47 tag (e.g. "en", "en-US", "pt-BR")
            // It's always after the voice name and separated by 2+ spaces
            var match = Regex.Match(line, @"^(.+?)\s{2,}([a-zA-Z]{2,3}(?:-[a-zA-Z]{2,})?)\s");
            if (!match.Success)
                continue;

            var name = match.Groups[1].Value.Trim().Replace('_', ' ');
            var languageTag = match.Groups[2].Value;

            if (!seen.Add(languageTag))
                continue;

            var (lang, country) = ParseLanguageTag(languageTag);
            locales.Add(new Locale(lang, country, name, languageTag));
        }

        return locales;
    }

    static (string language, string country) ParseLanguageTag(string tag)
    {
        var parts = tag.Split('-', '_');
        var language = parts[0];
        var country = parts.Length > 1 ? parts[1].ToUpperInvariant() : string.Empty;
        return (language, country);
    }

    static async Task SpeakWithEspeakAsync(string text, SpeechOptions options, CancellationToken cancelToken)
    {
        var args = new List<string>();

        // Locale.Id stores the full voice identifier (e.g. "en-gb", "es-la")
        // which espeak-ng uses directly as the -v parameter.
        if (options.Locale is { } locale && !string.IsNullOrEmpty(locale.Id))
            args.Add($"-v {locale.Id}");
        else if (options.Locale is { } loc && !string.IsNullOrEmpty(loc.Language))
            args.Add($"-v {loc.Language}");

        if (options.Pitch.HasValue)
        {
            var pitch = (int)(options.Pitch.Value * 49.5f);
            args.Add($"-p {pitch.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.Rate.HasValue)
        {
            var wpm = (int)(80 + (options.Rate.Value - 0.1f) * (370f / 1.9f));
            args.Add($"-s {wpm.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.Volume.HasValue)
        {
            var amplitude = (int)(options.Volume.Value * 200f);
            args.Add($"-a {amplitude.ToString(CultureInfo.InvariantCulture)}");
        }

        args.Add($"-- \"{EscapeShellArg(text)}\"");

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "espeak-ng",
            Arguments = string.Join(' ', args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        process.Start();

        // Register cancellation to kill the process immediately when token fires
        using var registration = cancelToken.Register(() =>
        {
            try { process.Kill(entireProcessTree: true); } catch { /* best effort */ }
        });

        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().ConfigureAwait(false);
        cancelToken.ThrowIfCancellationRequested();

        var error = await errorTask.ConfigureAwait(false);
        if (process.ExitCode != 0)
            throw CreateProcessFailedException("espeak-ng", process.ExitCode, error);
    }

    static async Task SpeakWithSpdSayAsync(string text, SpeechOptions options, CancellationToken cancelToken)
    {
        var args = new List<string> { "-w" }; // wait for completion

        // Locale.Id stores the full language tag for spd-say
        if (options.Locale is { } locale && !string.IsNullOrEmpty(locale.Id))
            args.Add($"-l {locale.Id}");
        else if (options.Locale is { } loc && !string.IsNullOrEmpty(loc.Language))
            args.Add($"-l {loc.Language}");

        if (options.Pitch.HasValue)
        {
            var pitch = (int)((options.Pitch.Value - 1f) * 100f);
            args.Add($"-p {pitch.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.Rate.HasValue)
        {
            var rate = (int)((options.Rate.Value - 1.05f) * (200f / 1.9f));
            args.Add($"-r {rate.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.Volume.HasValue)
        {
            var volume = (int)(options.Volume.Value * 200f - 100f);
            args.Add($"-i {volume.ToString(CultureInfo.InvariantCulture)}");
        }

        args.Add($"-- \"{EscapeShellArg(text)}\"");

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "spd-say",
            Arguments = string.Join(' ', args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        process.Start();

        // Register cancellation to kill the client and tell the daemon to stop
        using var registration = cancelToken.Register(() =>
        {
            try { process.Kill(entireProcessTree: true); } catch { /* best effort */ }
            // spd-say -S tells the speech-dispatcher daemon to stop speaking
            try
            {
                using var stop = Process.Start(new ProcessStartInfo
                {
                    FileName = "spd-say",
                    Arguments = "-S",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch { /* best effort */ }
        });

        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().ConfigureAwait(false);
        cancelToken.ThrowIfCancellationRequested();

        var error = await errorTask.ConfigureAwait(false);
        if (process.ExitCode != 0)
            throw CreateProcessFailedException("spd-say", process.ExitCode, error);
    }

    static InvalidOperationException CreateProcessFailedException(string processName, int exitCode, string error)
    {
        var detail = string.IsNullOrWhiteSpace(error)
            ? $"'{processName}' exited with code {exitCode}."
            : $"'{processName}' exited with code {exitCode}: {error.Trim()}";

        return new InvalidOperationException(detail);
    }

    static string EscapeShellArg(string arg) =>
        arg.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
