using Avalonia.Platform;

namespace Avalonia.Controls.Maui.Services;

internal static class AvaloniaResourceHelper
{
    /// <summary>
    /// Tries to resolve a MAUI image filename to an Avalonia resource URI.
    /// </summary>
    internal static bool TryResolveResourceUri(string fileName, out Uri? uri)
    {
        uri = null;
        var resourcePath = GetResourcePath(fileName);

        // MAUI images are embedded in the MAUI app's assembly. That is usually the entry assembly,
        // but not when a separate head project (such as a browser app) starts the MAUI app.
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        var appAssembly = Microsoft.Maui.Controls.Application.Current?.GetType().Assembly;

        return TryResolveResourceUri(entryAssembly, resourcePath, out uri)
            || (appAssembly != entryAssembly && TryResolveResourceUri(appAssembly, resourcePath, out uri));
    }

    private static bool TryResolveResourceUri(System.Reflection.Assembly? assembly, string resourcePath, out Uri? uri)
    {
        uri = null;
        if (assembly == null)
            return false;

        try
        {
            var candidate = new Uri($"avares://{assembly.GetName().Name}{resourcePath}");

            // Verify the resource actually exists by trying to open it
            using var stream = AssetLoader.Open(candidate);
            uri = candidate;
            return true;
        }
        catch
        {
            // Resource not found
            return false;
        }
    }

    private static string GetResourcePath(string fileName)
    {
        if (fileName.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            return fileName.StartsWith("/") ? fileName : "/" + fileName;
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        // The Avalonia.Controls.Target task embeds images under the /Images/ folder.
        return $"/Images/{nameWithoutExtension}{extension}";
    }
}
