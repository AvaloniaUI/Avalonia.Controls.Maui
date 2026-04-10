using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Desktop implementation of secure storage using platform-native services when
/// available and a hardened AES-GCM file fallback otherwise.
/// </summary>
[UnconditionalSuppressMessage("Interoperability", "CA1416", Justification = "Linux native backend access is guarded at runtime, and tests may override the platform hook.")]
public sealed partial class AvaloniaSecureStorage
{
    const int EncryptionKeySizeBytes = 32;
    const string StorageDirectoryOverrideEnvironmentVariable = "AVALONIA_MAUI_SECURE_STORAGE_DIR";
    static readonly TimeSpan LinuxSecretServiceRetryDelay = TimeSpan.FromSeconds(30);

    static readonly string StoragePath = GetStoragePath("securestorage.dat");
    static readonly string KeyPath = GetStoragePath(".securestorage.key");
    static readonly Mutex ProcessLock = new(false, GetMutexName());
    internal static Func<bool> IsLinuxPlatform { get; set; } = OperatingSystem.IsLinux;
    internal static Func<DateTimeOffset> UtcNowProvider { get; set; } = static () => DateTimeOffset.UtcNow;
    internal static Func<string, LinuxSecretServiceSecureStorage> LinuxSecretServiceFactory { get; set; }
        = static appName => new LinuxSecretServiceSecureStorage(appName);

    readonly object _locker = new();
    Dictionary<string, EncryptedEntry>? _store;
    byte[]? _encryptionKey;
    LinuxSecretServiceSecureStorage? _linuxSecretService;
    bool _linuxSecretServiceNotSupported;
    DateTimeOffset _linuxSecretServiceRetryAfterUtc = DateTimeOffset.MinValue;

    private partial async Task<string?> PlatformGetAsync(string key)
    {
        if (!IsLinuxPlatform())
            return FileGet(key);

        return await LinuxGetAsync(key).ConfigureAwait(false);
    }

    private partial async Task PlatformSetAsync(string key, string value)
    {
        if (!IsLinuxPlatform())
        {
            FileSet(key, value);
            return;
        }

        await LinuxSetAsync(key, value).ConfigureAwait(false);
    }

    private partial bool PlatformRemove(string key)
    {
        if (!IsLinuxPlatform())
            return FileRemove(key);

        return LinuxRemove(key);
    }

    private partial void PlatformRemoveAll()
    {
        if (!IsLinuxPlatform())
        {
            FileRemoveAll();
            return;
        }

        LinuxRemoveAll();
    }

    async Task<string?> LinuxGetAsync(string key)
    {
        var nativeState = GetLinuxSecretServiceState(out var linuxSecretService);
        if (nativeState == LinuxSecretServiceState.NotSupported)
            return FileGet(key);

        if (nativeState == LinuxSecretServiceState.RetryLater)
        {
            var legacyValue = FileGet(key);
            if (legacyValue is not null)
                return legacyValue;

            throw CreateLinuxSecretServiceUnavailableException();
        }

        var nativeResult = await linuxSecretService!.GetAsync(key).ConfigureAwait(false);
        switch (nativeResult.Kind)
        {
            case LinuxSecureStorageResultKind.Success:
                ResetLinuxSecretServiceRetry();
                return nativeResult.Value;
            case LinuxSecureStorageResultKind.NotFound:
                ResetLinuxSecretServiceRetry();
                var legacyValue = FileGet(key);
                if (legacyValue is null)
                    return null;

                var migrationResult = await linuxSecretService.SetAsync(key, legacyValue).ConfigureAwait(false);
                switch (migrationResult.Kind)
                {
                    case LinuxSecureStorageResultKind.Success:
                        ResetLinuxSecretServiceRetry();
                        FileRemove(key);
                        break;
                    case LinuxSecureStorageResultKind.Unavailable:
                        MarkLinuxSecretServiceUnavailable();
                        break;
                    case LinuxSecureStorageResultKind.NotSupported:
                        MarkLinuxSecretServiceNotSupported();
                        break;
                    case LinuxSecureStorageResultKind.Failed:
                        throw new InvalidOperationException(migrationResult.Error);
                }

                return legacyValue;
            case LinuxSecureStorageResultKind.Unavailable:
                MarkLinuxSecretServiceUnavailable();
                legacyValue = FileGet(key);
                if (legacyValue is not null)
                    return legacyValue;

                throw CreateLinuxSecretServiceUnavailableException(nativeResult.Error);
            case LinuxSecureStorageResultKind.NotSupported:
                MarkLinuxSecretServiceNotSupported();
                return FileGet(key);
            case LinuxSecureStorageResultKind.Failed:
                throw new InvalidOperationException(nativeResult.Error);
            default:
                throw new InvalidOperationException("Unexpected Linux secure storage result.");
        }
    }

    async Task LinuxSetAsync(string key, string value)
    {
        var nativeState = GetLinuxSecretServiceState(out var linuxSecretService);
        if (nativeState == LinuxSecretServiceState.NotSupported)
        {
            FileSet(key, value);
            return;
        }

        if (nativeState == LinuxSecretServiceState.RetryLater)
            throw CreateLinuxSecretServiceUnavailableException();

        var nativeResult = await linuxSecretService!.SetAsync(key, value).ConfigureAwait(false);
        switch (nativeResult.Kind)
        {
            case LinuxSecureStorageResultKind.Success:
                ResetLinuxSecretServiceRetry();
                FileRemove(key);
                return;
            case LinuxSecureStorageResultKind.Unavailable:
                MarkLinuxSecretServiceUnavailable();
                throw CreateLinuxSecretServiceUnavailableException(nativeResult.Error);
            case LinuxSecureStorageResultKind.NotSupported:
                MarkLinuxSecretServiceNotSupported();
                FileSet(key, value);
                return;
            case LinuxSecureStorageResultKind.Failed:
                throw new InvalidOperationException(nativeResult.Error);
            default:
                throw new InvalidOperationException("Unexpected Linux secure storage result.");
        }
    }

    bool LinuxRemove(string key)
    {
        var nativeState = GetLinuxSecretServiceState(out var linuxSecretService);
        if (nativeState == LinuxSecretServiceState.NotSupported)
            return FileRemove(key);

        if (nativeState == LinuxSecretServiceState.RetryLater)
            throw CreateLinuxSecretServiceUnavailableException();

        var nativeResult = linuxSecretService!.RemoveAsync(key).GetAwaiter().GetResult();
        switch (nativeResult.Kind)
        {
            case LinuxSecureStorageResultKind.Success:
                ResetLinuxSecretServiceRetry();
                var removedFromFile = FileRemove(key);
                return nativeResult.Removed || removedFromFile;
            case LinuxSecureStorageResultKind.Unavailable:
                MarkLinuxSecretServiceUnavailable();
                throw CreateLinuxSecretServiceUnavailableException(nativeResult.Error);
            case LinuxSecureStorageResultKind.NotSupported:
                MarkLinuxSecretServiceNotSupported();
                return FileRemove(key);
            case LinuxSecureStorageResultKind.Failed:
                throw new InvalidOperationException(nativeResult.Error);
            default:
                throw new InvalidOperationException("Unexpected Linux secure storage result.");
        }
    }

    void LinuxRemoveAll()
    {
        var nativeState = GetLinuxSecretServiceState(out var linuxSecretService);
        if (nativeState == LinuxSecretServiceState.NotSupported)
        {
            FileRemoveAll();
            return;
        }

        if (nativeState == LinuxSecretServiceState.RetryLater)
            throw CreateLinuxSecretServiceUnavailableException();

        var nativeResult = linuxSecretService!.RemoveAllAsync().GetAwaiter().GetResult();
        switch (nativeResult.Kind)
        {
            case LinuxSecureStorageResultKind.Success:
                ResetLinuxSecretServiceRetry();
                FileRemoveAll();
                return;
            case LinuxSecureStorageResultKind.Unavailable:
                MarkLinuxSecretServiceUnavailable();
                throw CreateLinuxSecretServiceUnavailableException(nativeResult.Error);
            case LinuxSecureStorageResultKind.NotSupported:
                MarkLinuxSecretServiceNotSupported();
                FileRemoveAll();
                return;
            case LinuxSecureStorageResultKind.Failed:
                throw new InvalidOperationException(nativeResult.Error);
            default:
                throw new InvalidOperationException("Unexpected Linux secure storage result.");
        }
    }

    string? FileGet(string key)
    {
        using var _ = AcquireProcessLock();
        lock (_locker)
        {
            if (_store is null && !File.Exists(StoragePath))
                return null;

            EnsureLoaded();
            if (!_store!.TryGetValue(key, out var entry))
                return null;

            try
            {
                return Decrypt(entry);
            }
            catch (CryptographicException)
            {
                InvalidateCorruptedEntry(key);
                return null;
            }
            catch (FormatException)
            {
                InvalidateCorruptedEntry(key);
                return null;
            }
        }
    }

    void FileSet(string key, string value)
    {
        using var _ = AcquireProcessLock();
        lock (_locker)
        {
            EnsureLoaded();
            _store![key] = Encrypt(value);
            Save();
        }
    }

    bool FileRemove(string key)
    {
        using var _ = AcquireProcessLock();
        lock (_locker)
        {
            if (_store is null && !File.Exists(StoragePath))
                return false;

            EnsureLoaded();
            var removed = _store!.Remove(key);
            if (removed)
                Save();
            return removed;
        }
    }

    void FileRemoveAll()
    {
        using var _ = AcquireProcessLock();
        lock (_locker)
        {
            if (_store is null && !File.Exists(StoragePath))
            {
                if (File.Exists(KeyPath))
                    File.Delete(KeyPath);
                return;
            }

            EnsureLoaded();
            _store!.Clear();
            Save();
        }
    }

    LinuxSecretServiceState GetLinuxSecretServiceState([NotNullWhen(true)] out LinuxSecretServiceSecureStorage? linuxSecretService)
    {
        linuxSecretService = null;

        if (!IsLinuxPlatform())
            return LinuxSecretServiceState.NotSupported;

        lock (_locker)
        {
            if (_linuxSecretServiceNotSupported)
                return LinuxSecretServiceState.NotSupported;

            if (UtcNowProvider() < _linuxSecretServiceRetryAfterUtc)
                return LinuxSecretServiceState.RetryLater;

            _linuxSecretService ??= LinuxSecretServiceFactory(GetApplicationName());
            linuxSecretService = _linuxSecretService;
            return LinuxSecretServiceState.Ready;
        }
    }

    void MarkLinuxSecretServiceUnavailable()
    {
        lock (_locker)
        {
            _linuxSecretServiceRetryAfterUtc = UtcNowProvider().Add(LinuxSecretServiceRetryDelay);
            _linuxSecretService = null;
        }
    }

    void MarkLinuxSecretServiceNotSupported()
    {
        lock (_locker)
        {
            _linuxSecretServiceNotSupported = true;
            _linuxSecretServiceRetryAfterUtc = DateTimeOffset.MinValue;
            _linuxSecretService = null;
        }
    }

    void ResetLinuxSecretServiceRetry()
    {
        lock (_locker)
        {
            _linuxSecretServiceRetryAfterUtc = DateTimeOffset.MinValue;
        }
    }

    static InvalidOperationException CreateLinuxSecretServiceUnavailableException(string? error = null)
    {
        return new InvalidOperationException(error ?? "Linux secret service is temporarily unavailable.");
    }

    void EnsureLoaded()
    {
        if (_store is not null)
            return;

        _encryptionKey = LoadOrCreateKey(out var resetStore);
        _store = resetStore ? new Dictionary<string, EncryptedEntry>() : LoadStore();
    }

    byte[] LoadOrCreateKey(out bool resetStore)
    {
        resetStore = false;

        if (File.Exists(KeyPath))
        {
            var existingKey = File.ReadAllBytes(KeyPath);
            if (existingKey.Length == EncryptionKeySizeBytes)
            {
                TrySetSensitiveFileMode(KeyPath);
                return existingKey;
            }

            BackupCorruptedFile(KeyPath, "invalid-key");
            BackupCorruptedFile(StoragePath, "invalid-key");
            resetStore = true;
        }

        var key = new byte[EncryptionKeySizeBytes];
        RandomNumberGenerator.Fill(key);
        WriteSensitiveFileAtomically(KeyPath, key);
        return key;
    }

    Dictionary<string, EncryptedEntry> LoadStore()
    {
        if (!File.Exists(StoragePath))
            return new Dictionary<string, EncryptedEntry>();

        try
        {
            TrySetSensitiveFileMode(StoragePath);
            using var stream = File.OpenRead(StoragePath);
            return JsonSerializer.Deserialize(stream,
                SecureStorageJsonContext.Default.DictionaryStringEncryptedEntry)
                ?? new Dictionary<string, EncryptedEntry>();
        }
        catch (JsonException)
        {
            BackupCorruptedFile(StoragePath, "invalid-json");
            return new Dictionary<string, EncryptedEntry>();
        }
    }

    void Save()
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(
            _store!,
            SecureStorageJsonContext.Default.DictionaryStringEncryptedEntry);

        WriteSensitiveFileAtomically(StoragePath, json);
    }

    EncryptedEntry Encrypt(string plaintext)
    {
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

#pragma warning disable SYSLIB0053
        using var aes = new AesGcm(_encryptionKey!, tag.Length);
#pragma warning restore SYSLIB0053
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return new EncryptedEntry(
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(ciphertext),
            Convert.ToBase64String(tag));
    }

    string Decrypt(EncryptedEntry entry)
    {
        var nonce = Convert.FromBase64String(entry.Nonce);
        var ciphertext = Convert.FromBase64String(entry.Ciphertext);
        var tag = Convert.FromBase64String(entry.Tag);
        var plaintext = new byte[ciphertext.Length];

#pragma warning disable SYSLIB0053
        using var aes = new AesGcm(_encryptionKey!, tag.Length);
#pragma warning restore SYSLIB0053
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    void InvalidateCorruptedEntry(string key)
    {
        if (_store!.Remove(key))
            Save();
    }

    static IDisposable AcquireProcessLock()
    {
        try
        {
            ProcessLock.WaitOne();
        }
        catch (AbandonedMutexException)
        {
        }

        return new MutexLease(ProcessLock);
    }

    static string GetMutexName()
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(StoragePath));
        return $"AvaloniaSecureStorage_{Convert.ToHexString(hash)}";
    }

    static void WriteSensitiveFileAtomically(string path, ReadOnlySpan<byte> contents)
    {
        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir))
            throw new InvalidOperationException($"Could not determine directory for '{path}'.");

        Directory.CreateDirectory(dir);
        TrySetSensitiveDirectoryMode(dir);

        var tempPath = Path.Combine(dir, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(tempPath, new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                Options = FileOptions.WriteThrough
            }))
            {
                stream.Write(contents);
                stream.Flush(flushToDisk: true);
            }

            TrySetSensitiveFileMode(tempPath);
            File.Move(tempPath, path, overwrite: true);
            TrySetSensitiveFileMode(path);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    static void BackupCorruptedFile(string path, string suffix)
    {
        if (!File.Exists(path))
            return;

        var backupPath = $"{path}.{suffix}.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.bak";
        File.Move(path, backupPath, overwrite: true);
        TrySetSensitiveFileMode(backupPath);
    }

    static void TrySetSensitiveDirectoryMode(string path)
    {
        if (OperatingSystem.IsWindows())
            return;

        try
        {
            File.SetUnixFileMode(path,
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute);
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
    }

    static void TrySetSensitiveFileMode(string path)
    {
        if (OperatingSystem.IsWindows())
            return;

        try
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
    }

    static string GetStoragePath(string filename)
    {
        var overrideDirectory = Environment.GetEnvironmentVariable(StorageDirectoryOverrideEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overrideDirectory))
            return Path.Combine(overrideDirectory, filename);

        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            GetApplicationName());

        return Path.Combine(appDataDirectory, "Settings", filename);
    }

    static string GetApplicationName() => Assembly.GetEntryAssembly()?.GetName().Name ?? "AvaloniaApp";

    sealed class MutexLease(Mutex mutex) : IDisposable
    {
        readonly Mutex _mutex = mutex;
        bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _mutex.ReleaseMutex();
            _disposed = true;
        }
    }
}

enum LinuxSecretServiceState
{
    Ready,
    RetryLater,
    NotSupported
}

internal sealed record EncryptedEntry(string Nonce, string Ciphertext, string Tag);

[JsonSerializable(typeof(Dictionary<string, EncryptedEntry>), TypeInfoPropertyName = "DictionaryStringEncryptedEntry")]
internal sealed partial class SecureStorageJsonContext : JsonSerializerContext;
