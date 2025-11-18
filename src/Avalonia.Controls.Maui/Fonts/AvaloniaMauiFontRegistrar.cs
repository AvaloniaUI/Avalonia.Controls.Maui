using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Avalonia-specific implementation of <see cref="IFontRegistrar"/> that manages font registration
/// for MAUI applications running on Avalonia.
/// </summary>
public class AvaloniaMauiFontRegistrar : IFontRegistrar
{
    private readonly Dictionary<string, FontRegistration> _fonts = new(StringComparer.Ordinal);
    private readonly HashSet<string> _registeredFontFamilies = new(StringComparer.Ordinal);
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<AvaloniaMauiFontRegistrar>? _logger;

    /// <summary>
    /// Represents a registered font with its metadata.
    /// </summary>
    private record FontRegistration(string Filename, string? Alias, Assembly? Assembly);

    /// <summary>
    /// Creates a new <see cref="AvaloniaMauiFontRegistrar"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The application's <see cref="IServiceProvider"/>.
    /// Typically this is provided through dependency injection.</param>
    public AvaloniaMauiFontRegistrar(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider?.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory
            ? loggerFactory.CreateLogger<AvaloniaMauiFontRegistrar>()
            : null;
    }

    /// <inheritdoc/>
    public void Register(string filename, string? alias, Assembly assembly)
    {
        var registration = new FontRegistration(filename, alias, assembly);

        // Register by filename
        _fonts[filename] = registration;

        // Also register by alias if provided
        if (!string.IsNullOrWhiteSpace(alias))
        {
            _fonts[alias!] = registration;
        }

        _logger?.LogDebug("Registered font: {Filename} with alias: {Alias}", filename, alias);
    }

    /// <inheritdoc/>
    public void Register(string filename, string? alias)
    {
        var registration = new FontRegistration(filename, alias, null);

        // Register by filename
        _fonts[filename] = registration;

        // Also register by alias if provided
        if (!string.IsNullOrWhiteSpace(alias))
        {
            _fonts[alias!] = registration;
        }

        _logger?.LogDebug("Registered native font: {Filename} with alias: {Alias}", filename, alias);
    }


    /// <inheritdoc/>
    public string? GetFont(string font)
    {
        if (string.IsNullOrWhiteSpace(font))
            return null;

        // Try to find the font registration
        if (!_fonts.TryGetValue(font, out var registration))
        {
            _logger?.LogWarning("Font '{Font}' not found in registry", font);
            return null;
        }

        try
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                _logger?.LogWarning("Could not determine entry assembly name");
                return null;
            }

            // Derive the font family name from the filename
            // For OpenSans-Regular.ttf -> "Open Sans"
            // For Roboto-Bold.ttf -> "Roboto"
            var filename = System.IO.Path.GetFileNameWithoutExtension(registration.Filename);
            
            // First, try to extract just the base name before any hyphen/underscore
            var separatorIndex = filename.IndexOfAny(new[] { '-', '_' });
            var baseName = separatorIndex > 0 ? filename.Substring(0, separatorIndex) : filename;
            
            // Insert spaces before capital letters (e.g., "OpenSans" -> "Open Sans")
            var fontFamilyName = System.Text.RegularExpressions.Regex.Replace(
                baseName, 
                "([a-z])([A-Z])", 
                "$1 $2"
            );

            // Return the font family in the format: fonts:AssemblyName#FontFamilyName
            // This matches how EmbeddedFontCollection fonts are referenced
            var fontUri = $"fonts:{assemblyName}#{fontFamilyName}";

            _logger?.LogDebug("Retrieved font: {Font} -> {FontUri} (from {Filename})", font, fontUri, registration.Filename);
            return fontUri;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error retrieving font '{Font}'", font);
            return null;
        }
    }

    /// <summary>
    /// Gets the full filename for a registered font.
    /// </summary>
    /// <param name="font">The font name or alias to look up.</param>
    /// <returns>The full filename if found; otherwise, null.</returns>
    public string? GetFontFilename(string font)
    {
        if (string.IsNullOrWhiteSpace(font))
            return null;

        if (_fonts.TryGetValue(font, out var registration))
        {
            return registration.Filename;
        }

        return null;
    }
}
