using Avalonia.Controls.Maui.Essentials;
using System.ComponentModel;
using System.Runtime.Versioning;

namespace Avalonia.Controls.Maui.Tests.Services;

[SupportedOSPlatform("linux")]
[Collection(nameof(SecureStorageTestCollection))]
public class LinuxSecretServiceSecureStorageTests
{
    [Fact]
    public async Task Get_Returns_Value_When_SecretTool_Succeeds()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 0,
            StandardOutput: "super-secret",
            StandardError: string.Empty,
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.GetAsync("token");

        Assert.Equal(LinuxSecureStorageResultKind.Success, result.Kind);
        Assert.Equal("super-secret", result.Value);
        Assert.Equal(["lookup", "application", "TestApp", "service", "Avalonia.Controls.Maui.Essentials.SecureStorage", "key", "token"], runner.Calls[0].Arguments);
    }

    [Fact]
    public async Task Set_Passes_Value_Through_Standard_Input()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 0,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.SetAsync("token", "value");

        Assert.Equal(LinuxSecureStorageResultKind.Success, result.Kind);
        Assert.Equal("value", runner.Calls[0].StandardInput);
        Assert.Equal("store", runner.Calls[0].Arguments[0]);
        Assert.Contains("--label=TestApp Secure Storage (token)", runner.Calls[0].Arguments);
    }

    [Fact]
    public async Task Get_Treats_Missing_Item_As_NotFound()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 1,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.GetAsync("token");

        Assert.Equal(LinuxSecureStorageResultKind.NotFound, result.Kind);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Get_Treats_DBus_Failures_As_Unavailable()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 1,
            StandardOutput: string.Empty,
            StandardError: "Cannot autolaunch D-Bus without X11 $DISPLAY",
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.GetAsync("token");

        Assert.Equal(LinuxSecureStorageResultKind.Unavailable, result.Kind);
        Assert.Contains("Cannot autolaunch D-Bus", result.Error);
    }

    [Fact]
    public async Task Get_Treats_Missing_SecretTool_As_NotSupported()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: false,
            ExitCode: -1,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: new Win32Exception("No such file or directory")));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.GetAsync("token");

        Assert.Equal(LinuxSecureStorageResultKind.NotSupported, result.Kind);
    }

    [Fact]
    public async Task RemoveAll_Clears_App_Scope()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 0,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.RemoveAllAsync();

        Assert.Equal(LinuxSecureStorageResultKind.Success, result.Kind);
        Assert.Equal(["clear", "application", "TestApp", "service", "Avalonia.Controls.Maui.Essentials.SecureStorage"], runner.Calls[0].Arguments);
    }

    [Fact]
    public async Task Get_Treats_Unexpected_Empty_NonZero_Exit_As_Unavailable()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 2,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.GetAsync("token");

        Assert.Equal(LinuxSecureStorageResultKind.Unavailable, result.Kind);
    }

    [Fact]
    public async Task RemoveAll_Treats_Unexpected_Empty_NonZero_Exit_As_Unavailable()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 2,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: null));

        var storage = new LinuxSecretServiceSecureStorage("TestApp", runner);

        var result = await storage.RemoveAllAsync();

        Assert.Equal(LinuxSecureStorageResultKind.Unavailable, result.Kind);
    }

    sealed class StubSecretToolRunner(params SecretToolCommandResult[] results) : ISecretToolRunner
    {
        readonly Queue<SecretToolCommandResult> _results = new(results);

        public List<Call> Calls { get; } = [];

        public Task<SecretToolCommandResult> RunAsync(IReadOnlyList<string> arguments, string? standardInput)
        {
            Calls.Add(new(arguments.ToArray(), standardInput));
            return Task.FromResult(_results.Dequeue());
        }
    }

    sealed record Call(string[] Arguments, string? StandardInput);
}
