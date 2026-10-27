using System.Reflection;
using Avalonia.Platform;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Avalonia.Controls.Maui.BlazorWebView.Services;

internal sealed class AvaloniaResourceFileProvider : IFileProvider
{
    private readonly string _contentRootDir;
    private readonly Assembly _assembly;

    public AvaloniaResourceFileProvider(string contentRootDir)
        : this(contentRootDir, Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
    {
    }

    internal AvaloniaResourceFileProvider(string contentRootDir, Assembly assembly)
    {
        _contentRootDir = NormalizePath(contentRootDir);
        _assembly = assembly;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var assetPath = Combine(_contentRootDir, NormalizePath(subpath));
        var assemblyName = _assembly.GetName().Name;

        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(assetPath))
            return new NotFoundFileInfo(Path.GetFileName(subpath));

        var assetUri = new Uri($"avares://{assemblyName}/{assetPath}");
        return AssetLoader.Exists(assetUri)
            ? new AvaloniaResourceFileInfo(assetUri, Path.GetFileName(assetPath))
            : new NotFoundFileInfo(Path.GetFileName(subpath));
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }

    private static string Combine(string left, string right)
    {
        if (string.IsNullOrEmpty(left))
            return right;

        if (string.IsNullOrEmpty(right))
            return left;

        return $"{left}/{right}";
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Trim('/');
    }

    private sealed class AvaloniaResourceFileInfo : IFileInfo
    {
        private readonly Uri _assetUri;

        public AvaloniaResourceFileInfo(Uri assetUri, string name)
        {
            _assetUri = assetUri;
            Name = name;
        }

        public bool Exists => true;

        public long Length
        {
            get
            {
                using var stream = CreateReadStream();
                return stream.CanSeek ? stream.Length : -1;
            }
        }

        public string? PhysicalPath => null;

        public string Name { get; }

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return AssetLoader.Open(_assetUri);
        }
    }
}
