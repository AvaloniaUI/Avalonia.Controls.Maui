using System.Reflection;
using Avalonia.Platform;
using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of IFileSystem that provides access to standard OS directories for cache and application data, and uses Avalonia's AssetLoader for embedded app package files.
/// </summary>
public class AvaloniaFileSystem : IFileSystem
{
    readonly string _cacheDirectory;
    readonly string _appDataDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaFileSystem"/> class, resolving cache and app data directory paths based on the entry assembly name.
    /// </summary>
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

    /// <summary>
    /// Gets the path to the application's cache directory under LocalApplicationData, creating the directory if it does not exist.
    /// </summary>
    public string CacheDirectory
    {
        get
        {
            Directory.CreateDirectory(_cacheDirectory);
            return _cacheDirectory;
        }
    }

    /// <summary>
    /// Gets the path to the application's data directory under ApplicationData, creating the directory if it does not exist.
    /// </summary>
    public string AppDataDirectory
    {
        get
        {
            Directory.CreateDirectory(_appDataDirectory);
            return _appDataDirectory;
        }
    }

    /// <summary>
    /// Determines whether a file with the specified name exists as an embedded Avalonia asset in the app package.
    /// </summary>
    /// <param name="filename">The name or relative path of the file to check.</param>
    /// <returns><see langword="true"/> if the file exists as an embedded asset; otherwise, <see langword="false"/>.</returns>
    public Task<bool> AppPackageFileExistsAsync(string filename)
    {
        return Task.FromResult(TryGetAssetUri(filename, out _));
    }

    /// <summary>
    /// Opens a read-only stream for the specified file from the app package using Avalonia's AssetLoader.
    /// </summary>
    /// <param name="filename">The name or relative path of the file to open.</param>
    /// <returns>A stream containing the contents of the specified app package file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file cannot be found in the app package.</exception>
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
