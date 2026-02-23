using System.Reflection;
using Avalonia.Platform;
using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Essentials;

public class AvaloniaFileSystem : IFileSystem
{
    readonly string _cacheDirectory;
    readonly string _appDataDirectory;

    public AvaloniaFileSystem()
    {
        var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "AvaloniaApp";

        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName,
            "Cache");

        _appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            appName);
    }

    public string CacheDirectory
    {
        get
        {
            Directory.CreateDirectory(_cacheDirectory);
            return _cacheDirectory;
        }
    }

    public string AppDataDirectory
    {
        get
        {
            Directory.CreateDirectory(_appDataDirectory);
            return _appDataDirectory;
        }
    }

    public Task<bool> AppPackageFileExistsAsync(string filename)
    {
        return Task.FromResult(TryGetAssetUri(filename, out _));
    }

    public Task<Stream> OpenAppPackageFileAsync(string filename)
    {
        if (!TryGetAssetUri(filename, out var uri))
            throw new FileNotFoundException($"Unable to find '{filename}' in the app package.");

        return Task.FromResult(AssetLoader.Open(uri!));
    }

    static bool TryGetAssetUri(string filename, out Uri? uri)
    {
        uri = null;

        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            return false;

        var assemblyName = assembly.GetName().Name;
        var normalizedFilename = filename.Replace('\\', '/');
        var candidate = new Uri($"avares://{assemblyName}/{normalizedFilename}");

        if (AssetLoader.Exists(candidate))
        {
            uri = candidate;
            return true;
        }

        return false;
    }
}
