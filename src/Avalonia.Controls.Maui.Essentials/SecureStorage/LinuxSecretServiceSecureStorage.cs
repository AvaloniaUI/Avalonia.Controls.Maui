using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace Avalonia.Controls.Maui.Essentials;

[SupportedOSPlatform("linux")]
internal sealed class LinuxSecretServiceSecureStorage
{
    const string ServiceAttributeName = "service";
    const string ServiceAttributeValue = "Avalonia.Controls.Maui.Essentials.SecureStorage";
    const string ApplicationAttributeName = "application";
    const string KeyAttributeName = "key";

    readonly string _appName;
    readonly ISecretToolRunner _runner;

    internal LinuxSecretServiceSecureStorage(string appName, ISecretToolRunner? runner = null)
    {
        _appName = appName;
        _runner = runner ?? new ProcessSecretToolRunner();
    }

    internal async Task<LinuxSecretStorageLookupResult> GetAsync(string key)
    {
        var result = await _runner.RunAsync(BuildKeyArguments("lookup", key), standardInput: null).ConfigureAwait(false);
        if (result.IsSuccess)
            return new(LinuxSecureStorageResultKind.Success, result.StandardOutput);

        if (IsNotSupported(result))
            return new(LinuxSecureStorageResultKind.NotSupported, null, BuildErrorMessage("lookup", result));

        if (IsNotFound(result))
            return new(LinuxSecureStorageResultKind.NotFound, null);

        return new(LinuxSecureStorageResultKind.Unavailable, null, BuildErrorMessage("lookup", result));
    }

    internal async Task<LinuxSecretStorageOperationResult> SetAsync(string key, string value)
    {
        var label = $"{_appName} Secure Storage ({key})";
        var arguments = new List<string>
        {
            "store",
            $"--label={label}"
        };
        arguments.AddRange(BuildAttributes(key));

        var result = await _runner.RunAsync(arguments, value).ConfigureAwait(false);
        if (result.IsSuccess)
            return new(LinuxSecureStorageResultKind.Success, null);

        if (IsNotSupported(result))
            return new(LinuxSecureStorageResultKind.NotSupported, BuildErrorMessage("store", result));

        return new(LinuxSecureStorageResultKind.Unavailable, BuildErrorMessage("store", result));
    }

    internal async Task<LinuxSecretStorageRemoveResult> RemoveAsync(string key)
    {
        var lookup = await GetAsync(key).ConfigureAwait(false);
        return lookup.Kind switch
        {
            LinuxSecureStorageResultKind.Success => await ClearEntryAsync(key).ConfigureAwait(false),
            LinuxSecureStorageResultKind.NotFound => new(LinuxSecureStorageResultKind.Success, false, null),
            LinuxSecureStorageResultKind.NotSupported => new(LinuxSecureStorageResultKind.NotSupported, false, lookup.Error),
            LinuxSecureStorageResultKind.Unavailable => new(LinuxSecureStorageResultKind.Unavailable, false, lookup.Error),
            _ => new(LinuxSecureStorageResultKind.Failed, false, lookup.Error)
        };
    }

    internal async Task<LinuxSecretStorageOperationResult> RemoveAllAsync()
    {
        var result = await _runner.RunAsync(BuildAppArguments("clear"), standardInput: null).ConfigureAwait(false);
        if (result.IsSuccess)
            return new(LinuxSecureStorageResultKind.Success, null);

        if (IsNotSupported(result))
            return new(LinuxSecureStorageResultKind.NotSupported, BuildErrorMessage("clear", result));

        return new(LinuxSecureStorageResultKind.Unavailable, BuildErrorMessage("clear", result));
    }

    async Task<LinuxSecretStorageRemoveResult> ClearEntryAsync(string key)
    {
        var result = await _runner.RunAsync(BuildKeyArguments("clear", key), standardInput: null).ConfigureAwait(false);
        if (result.IsSuccess)
            return new(LinuxSecureStorageResultKind.Success, true, null);

        if (IsNotSupported(result))
            return new(LinuxSecureStorageResultKind.NotSupported, false, BuildErrorMessage("clear", result));

        if (IsNotFound(result))
            return new(LinuxSecureStorageResultKind.Success, false, null);

        return new(LinuxSecureStorageResultKind.Unavailable, false, BuildErrorMessage("clear", result));
    }

    IReadOnlyList<string> BuildKeyArguments(string command, string key)
    {
        var arguments = new List<string> { command };
        arguments.AddRange(BuildAttributes(key));
        return arguments;
    }

    IReadOnlyList<string> BuildAppArguments(string command)
    {
        var arguments = new List<string> { command };
        arguments.AddRange(BuildAppAttributes());
        return arguments;
    }

    IReadOnlyList<string> BuildAttributes(string key)
    {
        var arguments = new List<string>(BuildAppAttributes())
        {
            KeyAttributeName,
            key
        };

        return arguments;
    }

    IReadOnlyList<string> BuildAppAttributes()
    {
        return
        [
            ApplicationAttributeName, _appName,
            ServiceAttributeName, ServiceAttributeValue
        ];
    }

    static bool IsNotFound(SecretToolCommandResult result)
    {
        if (!result.Started || result.ExitCode != 1)
            return false;

        return string.IsNullOrWhiteSpace(result.StandardOutput) && string.IsNullOrWhiteSpace(result.StandardError);
    }

    static bool IsNotSupported(SecretToolCommandResult result) => !result.Started;

    static string BuildErrorMessage(string operation, SecretToolCommandResult result)
    {
        if (result.StartException is not null)
            return $"Linux secret service {operation} failed: {result.StartException.Message}";

        var stderr = result.StandardError.Trim();
        if (stderr.Length > 0)
            return $"Linux secret service {operation} failed: {stderr}";

        return $"Linux secret service {operation} failed with exit code {result.ExitCode}.";
    }
}

internal interface ISecretToolRunner
{
    Task<SecretToolCommandResult> RunAsync(IReadOnlyList<string> arguments, string? standardInput);
}

[SupportedOSPlatform("linux")]
internal sealed class ProcessSecretToolRunner : ISecretToolRunner
{
    public async Task<SecretToolCommandResult> RunAsync(IReadOnlyList<string> arguments, string? standardInput)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "secret-tool",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        startInfo.Environment["LC_ALL"] = "C";
        startInfo.Environment["LANG"] = "C";

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var standardOutputTask = process.StandardOutput.ReadToEndAsync();
            var standardErrorTask = process.StandardError.ReadToEndAsync();

            if (standardInput is not null)
                await process.StandardInput.WriteAsync(standardInput).ConfigureAwait(false);

            process.StandardInput.Close();

            await process.WaitForExitAsync().ConfigureAwait(false);
            var standardOutput = await standardOutputTask.ConfigureAwait(false);
            var standardError = await standardErrorTask.ConfigureAwait(false);

            return new(true, process.ExitCode, standardOutput, standardError, null);
        }
        catch (Win32Exception ex)
        {
            return new(false, -1, string.Empty, string.Empty, ex);
        }
    }
}

internal readonly record struct SecretToolCommandResult(
    bool Started,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    Exception? StartException)
{
    internal bool IsSuccess => Started && ExitCode == 0;
}

internal enum LinuxSecureStorageResultKind
{
    Success,
    NotFound,
    NotSupported,
    Unavailable,
    Failed
}

internal readonly record struct LinuxSecretStorageLookupResult(
    LinuxSecureStorageResultKind Kind,
    string? Value,
    string? Error = null);

internal readonly record struct LinuxSecretStorageOperationResult(
    LinuxSecureStorageResultKind Kind,
    string? Error = null);

internal readonly record struct LinuxSecretStorageRemoveResult(
    LinuxSecureStorageResultKind Kind,
    bool Removed,
    string? Error = null);
