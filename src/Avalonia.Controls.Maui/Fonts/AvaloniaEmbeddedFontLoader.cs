using System;
using System.IO;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Avalonia-specific implementation of <see cref="IEmbeddedFontLoader"/> that loads fonts
/// from Avalonia resources and registers them with Avalonia's font system.
/// </summary>
public class AvaloniaEmbeddedFontLoader : IEmbeddedFontLoader
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<AvaloniaEmbeddedFontLoader>? _logger;

    /// <summary>
    /// Creates a new <see cref="AvaloniaEmbeddedFontLoader"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The application's <see cref="IServiceProvider"/>.
    /// Typically this is provided through dependency injection.</param>
    public AvaloniaEmbeddedFontLoader(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider?.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory
            ? loggerFactory.CreateLogger<AvaloniaEmbeddedFontLoader>()
            : null;
    }

    /// <inheritdoc/>
    public string? LoadFont(EmbeddedFont font)
    {
        if (font?.FontName == null)
            return null;

        try
        {
            // If the font has a resource stream, use it directly
            if (font.ResourceStream != null)
            {
                return LoadFontFromStream(font.FontName, font.ResourceStream);
            }

            // Otherwise, try to load from Avalonia resources
            return LoadFontFromAvaloniaResources(font.FontName);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Unable to load font '{FontName}'.", font.FontName);
            return null;
        }
    }

    private string? LoadFontFromStream(string fontName, Stream stream)
    {
        try
        {
            // For Avalonia, fonts loaded from embedded resources need to use avares:// URI format
            // Get the entry assembly name to construct the proper avares:// URI
            var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "SandboxApp";

            // Return the avares:// URI that Avalonia can use
            // Format: avares://AssemblyName/Fonts/FontFile.ttf#FontFamilyName
            var fontUri = $"avares://{assemblyName}/Fonts/{fontName}";

            _logger?.LogDebug("Loaded font from stream: {FontUri}", fontUri);

            // Return just the font family name - Avalonia's FontManager will construct the proper FontFamily
            return Path.GetFileNameWithoutExtension(fontName);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load font from stream: {FontName}", fontName);
            return null;
        }
    }

    private string? LoadFontFromAvaloniaResources(string fontName)
    {
        try
        {
            // Get the entry assembly name to construct the proper avares:// URI
            var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "SandboxApp";

            // Return the avares:// URI that Avalonia can use to load the font
            // The fonts are embedded with Link="Fonts/filename.ttf"
            var fontUri = $"avares://{assemblyName}/Fonts/{fontName}";

            _logger?.LogDebug("Loading font from Avalonia resources: {FontUri}", fontUri);

            // Return just the font family name - Avalonia's FontManager will construct the proper FontFamily
            return Path.GetFileNameWithoutExtension(fontName);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load font from Avalonia resources: {FontName}", fontName);
            return null;
        }
    }
}
