using System.IO;
using System.Reflection;
using Avalonia.Platform;

namespace Avalonia.Controls.Maui.Handlers;

internal readonly record struct ResolvedWebViewSource(Uri? Uri, string? Html, Uri? BaseUri)
{
    internal bool IsHtmlContent => Html is not null;
}

internal static class WebViewSourceResolver
{
    public static bool TryResolve(string? url, out ResolvedWebViewSource source, Assembly? assembly = null)
    {
        source = default;

        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            source = new ResolvedWebViewSource(absoluteUri, null, null);
            return true;
        }

        if (TryLoadHtmlAsset(url, out var html, out var baseUri, assembly))
        {
            source = new ResolvedWebViewSource(null, html, baseUri);
            return true;
        }

        return false;
    }

    public static Uri? ResolveBaseUri(string? baseUrl, Assembly? assembly = null)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var absoluteUri))
            return absoluteUri;

        return CreateAssetUri(baseUrl, assembly, preserveTrailingSlash: true);
    }

    private static bool TryLoadHtmlAsset(string assetPath, out string? html, out Uri? baseUri, Assembly? assembly = null)
    {
        html = null;
        baseUri = null;

        if (!IsHtmlAsset(assetPath) || !TryResolveAssetUri(assetPath, out var assetUri, assembly))
            return false;

        using var stream = AssetLoader.Open(assetUri!);
        using var reader = new StreamReader(stream);

        html = reader.ReadToEnd();
        baseUri = CreateDirectoryUri(assetPath, assembly);
        return true;
    }

    private static bool TryResolveAssetUri(string assetPath, out Uri? assetUri, Assembly? assembly = null)
    {
        assetUri = CreateAssetUri(assetPath, assembly);
        return assetUri is not null && AssetLoader.Exists(assetUri);
    }

    private static Uri? CreateDirectoryUri(string assetPath, Assembly? assembly = null)
    {
        var normalizedPath = NormalizePath(assetPath);
        var slashIndex = normalizedPath.LastIndexOf('/');

        if (slashIndex < 0)
            return CreateAssetUri("/", assembly, preserveTrailingSlash: true);

        return CreateAssetUri(normalizedPath[..(slashIndex + 1)], assembly, preserveTrailingSlash: true);
    }

    private static Uri? CreateAssetUri(string assetPath, Assembly? assembly = null, bool preserveTrailingSlash = false)
    {
        var ownerAssembly = assembly ?? Assembly.GetEntryAssembly();
        var assemblyName = ownerAssembly?.GetName().Name;

        if (string.IsNullOrWhiteSpace(assemblyName))
            return null;

        var normalizedPath = NormalizePath(assetPath);
        if (string.IsNullOrEmpty(normalizedPath))
            return new Uri($"avares://{assemblyName}/");

        if (preserveTrailingSlash &&
            !normalizedPath.EndsWith("/", StringComparison.Ordinal) &&
            !Path.HasExtension(normalizedPath))
        {
            normalizedPath += "/";
        }

        return new Uri($"avares://{assemblyName}/{normalizedPath}");
    }

    private static bool IsHtmlAsset(string assetPath)
    {
        var extension = Path.GetExtension(assetPath);
        return extension.Equals(".htm", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".xhtml", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }
}
