using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json;

namespace Avalonia.Controls.Maui.Tests.Services;

[Collection(nameof(SecureStorageTestCollection))]
public class AvaloniaSecureStorageTests : IDisposable
{
    const string StorageDirectoryOverrideEnvironmentVariable = "AVALONIA_MAUI_SECURE_STORAGE_DIR";

    readonly AvaloniaSecureStorage _storage;
    readonly string _storageDirectory;
    readonly Func<bool> _originalIsLinuxPlatform;
    readonly Func<DateTimeOffset> _originalUtcNowProvider;
    readonly Func<string, LinuxSecretServiceSecureStorage> _originalLinuxSecretServiceFactory;

    static AvaloniaSecureStorageTests()
    {
        var storageDirectory = Path.Combine(
            Path.GetTempPath(),
            "Avalonia.Controls.Maui.Essentials.Tests",
            nameof(AvaloniaSecureStorageTests));

        Directory.CreateDirectory(storageDirectory);
        Environment.SetEnvironmentVariable(StorageDirectoryOverrideEnvironmentVariable, storageDirectory);
    }

    public AvaloniaSecureStorageTests()
    {
        _storageDirectory = Environment.GetEnvironmentVariable(StorageDirectoryOverrideEnvironmentVariable)!;
        _originalIsLinuxPlatform = AvaloniaSecureStorage.IsLinuxPlatform;
        _originalUtcNowProvider = AvaloniaSecureStorage.UtcNowProvider;
        _originalLinuxSecretServiceFactory = AvaloniaSecureStorage.LinuxSecretServiceFactory;
        DeleteArtifacts();
        _storage = new AvaloniaSecureStorage();
        _storage.RemoveAll();
    }

    public void Dispose()
    {
        AvaloniaSecureStorage.IsLinuxPlatform = _originalIsLinuxPlatform;
        AvaloniaSecureStorage.UtcNowProvider = _originalUtcNowProvider;
        AvaloniaSecureStorage.LinuxSecretServiceFactory = _originalLinuxSecretServiceFactory;
        _storage.RemoveAll();
        DeleteArtifacts();
    }

    [Fact]
    public void AvaloniaSecureStorage_Implements_ISecureStorage()
    {
        Assert.IsAssignableFrom<ISecureStorage>(_storage);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_Roundtrip()
    {
        await _storage.SetAsync("token", "my-secret-value");

        var result = await _storage.GetAsync("token");

        Assert.Equal("my-secret-value", result);
    }

    [Fact]
    public async Task GetAsync_Missing_Key_Returns_Null()
    {
        var result = await _storage.GetAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_Overwrites_Previous_Value()
    {
        await _storage.SetAsync("key", "first");
        await _storage.SetAsync("key", "second");

        var result = await _storage.GetAsync("key");

        Assert.Equal("second", result);
    }

    [Fact]
    public async Task Remove_Returns_True_For_Existing_Key()
    {
        await _storage.SetAsync("key", "value");

        var removed = _storage.Remove("key");

        Assert.True(removed);
    }

    [Fact]
    public void Remove_Returns_False_For_Missing_Key()
    {
        var removed = _storage.Remove("nonexistent");

        Assert.False(removed);
    }

    [Fact]
    public async Task Remove_Deletes_Value()
    {
        await _storage.SetAsync("key", "value");
        _storage.Remove("key");

        var result = await _storage.GetAsync("key");

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAll_Clears_All_Entries()
    {
        await _storage.SetAsync("key1", "value1");
        await _storage.SetAsync("key2", "value2");

        _storage.RemoveAll();

        Assert.Null(await _storage.GetAsync("key1"));
        Assert.Null(await _storage.GetAsync("key2"));
    }

    [Fact]
    public async Task SetAsync_NullKey_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _storage.SetAsync(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_NullValue_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _storage.SetAsync("key", null!));
    }

    [Fact]
    public async Task GetAsync_NullKey_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _storage.GetAsync(null!));
    }

    [Fact]
    public async Task Multiple_Keys_Coexist()
    {
        await _storage.SetAsync("key1", "value1");
        await _storage.SetAsync("key2", "value2");
        await _storage.SetAsync("key3", "value3");

        Assert.Equal("value1", await _storage.GetAsync("key1"));
        Assert.Equal("value2", await _storage.GetAsync("key2"));
        Assert.Equal("value3", await _storage.GetAsync("key3"));
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_With_Special_Characters()
    {
        var value = "p@$$w0rd!#%^&*()_+-={}[]|\\:\";<>?,./~`";
        await _storage.SetAsync("special", value);

        var result = await _storage.GetAsync("special");

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_With_Unicode()
    {
        var value = "Hello World! Hola Mundo!";
        await _storage.SetAsync("unicode", value);

        var result = await _storage.GetAsync("unicode");

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_With_Empty_String()
    {
        await _storage.SetAsync("empty", "");

        var result = await _storage.GetAsync("empty");

        Assert.Equal("", result);
    }

    [Fact]
    public async Task Persistence_Survives_New_Instance()
    {
        await _storage.SetAsync("persist", "survive");

        var newInstance = new AvaloniaSecureStorage();
        var result = await newInstance.GetAsync("persist");

        Assert.Equal("survive", result);
    }

    [Fact]
    public async Task GetAsync_InvalidCiphertext_Returns_Null_And_Removes_Corrupted_Entry()
    {
        await _storage.SetAsync("token", "value");

        var storagePath = GetStoragePath();
        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await File.ReadAllTextAsync(storagePath));
        payload!["token"] = JsonSerializer.SerializeToElement(new
        {
            Nonce = "invalid",
            Ciphertext = "invalid",
            Tag = "invalid"
        });

        await File.WriteAllTextAsync(storagePath, JsonSerializer.Serialize(payload));

        var reloadedStorage = new AvaloniaSecureStorage();
        Assert.Null(await reloadedStorage.GetAsync("token"));
        Assert.Null(await reloadedStorage.GetAsync("token"));
    }

    [Fact]
    public async Task GetAsync_InvalidStoreJson_Starts_With_Empty_Store_And_Backs_Up_File()
    {
        await File.WriteAllTextAsync(GetStoragePath(), "{ not-json");

        var reloadedStorage = new AvaloniaSecureStorage();
        Assert.Null(await reloadedStorage.GetAsync("missing"));

        Assert.True(Directory.GetFiles(_storageDirectory, "securestorage.dat.invalid-json.*.bak").Length > 0);
    }

    [Fact]
    public async Task Invalid_Key_File_Is_Rotated_And_New_Values_Can_Be_Stored()
    {
        await _storage.SetAsync("persist", "value");
        await File.WriteAllBytesAsync(GetKeyPath(), [1, 2, 3]);

        var reloadedStorage = new AvaloniaSecureStorage();
        Assert.Null(await reloadedStorage.GetAsync("persist"));

        await reloadedStorage.SetAsync("fresh", "value");
        Assert.Equal("value", await reloadedStorage.GetAsync("fresh"));
        Assert.True(Directory.GetFiles(_storageDirectory, ".securestorage.key.invalid-key.*.bak").Length > 0);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public async Task GetAsync_Retries_Linux_Secret_Service_After_Cooldown()
    {
        AvaloniaSecureStorage.IsLinuxPlatform = () => false;
        await _storage.SetAsync("token", "file-value");

        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var runner = new StubSecretToolRunner(
            new SecretToolCommandResult(
                Started: true,
                ExitCode: 1,
                StandardOutput: string.Empty,
                StandardError: "No secret service",
                StartException: null),
            new SecretToolCommandResult(
                Started: true,
                ExitCode: 0,
                StandardOutput: "native-value",
                StandardError: string.Empty,
                StartException: null));

        var nativeStore = new LinuxSecretServiceSecureStorage("TestApp", runner);
        AvaloniaSecureStorage.IsLinuxPlatform = () => true;
        AvaloniaSecureStorage.UtcNowProvider = () => now;
        AvaloniaSecureStorage.LinuxSecretServiceFactory = _ => nativeStore;

        Assert.Equal("file-value", await _storage.GetAsync("token"));

        now = now.AddSeconds(31);

        Assert.Equal("native-value", await _storage.GetAsync("token"));
        Assert.Equal(2, runner.Calls.Count);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public async Task GetAsync_Throws_When_Linux_Secret_Service_Is_Temporarily_Unavailable_And_No_Legacy_Value_Exists()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 1,
            StandardOutput: string.Empty,
            StandardError: "No secret service",
            StartException: null));

        AvaloniaSecureStorage.IsLinuxPlatform = () => true;
        AvaloniaSecureStorage.LinuxSecretServiceFactory = _ => new LinuxSecretServiceSecureStorage("TestApp", runner);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.GetAsync("token"));
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public async Task SetAsync_Does_Not_Write_File_Fallback_When_Linux_Secret_Service_Is_Temporarily_Unavailable()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 1,
            StandardOutput: string.Empty,
            StandardError: "No secret service",
            StartException: null));

        AvaloniaSecureStorage.IsLinuxPlatform = () => true;
        AvaloniaSecureStorage.LinuxSecretServiceFactory = _ => new LinuxSecretServiceSecureStorage("TestApp", runner);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.SetAsync("token", "value"));

        AvaloniaSecureStorage.IsLinuxPlatform = () => false;
        Assert.Null(await _storage.GetAsync("token"));
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public async Task Linux_Falls_Back_To_File_Backend_When_SecretTool_Is_Not_Installed()
    {
        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: false,
            ExitCode: -1,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: new Win32Exception("No such file or directory")));

        AvaloniaSecureStorage.IsLinuxPlatform = () => true;
        AvaloniaSecureStorage.LinuxSecretServiceFactory = _ => new LinuxSecretServiceSecureStorage("TestApp", runner);

        await _storage.SetAsync("token", "value");
        Assert.Equal("value", await _storage.GetAsync("token"));
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public async Task Remove_Returns_True_When_Only_Legacy_File_Backend_Contains_The_Key()
    {
        AvaloniaSecureStorage.IsLinuxPlatform = () => false;
        await _storage.SetAsync("token", "file-value");

        var runner = new StubSecretToolRunner(new SecretToolCommandResult(
            Started: true,
            ExitCode: 1,
            StandardOutput: string.Empty,
            StandardError: string.Empty,
            StartException: null));

        AvaloniaSecureStorage.IsLinuxPlatform = () => true;
        AvaloniaSecureStorage.LinuxSecretServiceFactory = _ => new LinuxSecretServiceSecureStorage("TestApp", runner);

        Assert.True(_storage.Remove("token"));

        AvaloniaSecureStorage.IsLinuxPlatform = () => false;
        Assert.Null(await _storage.GetAsync("token"));
    }

    static string GetStoragePath() => GetPrivateStaticField("StoragePath");

    static string GetKeyPath() => GetPrivateStaticField("KeyPath");

    static string GetPrivateStaticField(string fieldName)
    {
        var field = typeof(AvaloniaSecureStorage).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        return Assert.IsType<string>(field!.GetValue(null));
    }

    void DeleteArtifacts()
    {
        foreach (var file in Directory.GetFiles(_storageDirectory))
            File.Delete(file);
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
