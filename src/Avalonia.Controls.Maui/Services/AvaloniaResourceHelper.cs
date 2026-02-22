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
        try
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            if (assembly == null)
                return false;

            var assemblyName = assembly.GetName().Name;
            var resourcePath = GetResourcePath(fileName);
            var candidate = new Uri($"avares://{assemblyName}{resourcePath}");

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
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        // The Avalonia.Controls.Target task embeds images under the /Images/ folder.
        return $"/Images/{nameWithoutExtension}{extension}";
    }
}
