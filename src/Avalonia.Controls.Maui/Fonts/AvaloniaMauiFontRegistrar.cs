using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
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
            var assemblyName = string.Empty;

            if (registration.Assembly is not null)
            {
                assemblyName = registration.Assembly.GetName().Name ?? string.Empty;
            }
            else
            {
                assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
            }

            if (string.IsNullOrEmpty(assemblyName))
            {
                _logger?.LogWarning("Could not determine entry assembly name");
                return null;
            }

            // The Avalonia.Controls.Maui.targets link MauiFont files to Assets/Fonts/
            var fontPathUri = new Uri($"avares://{assemblyName}/Assets/Fonts/{registration.Filename}");
            var fontStream = AssetLoader.Open(fontPathUri);
            var fontFamilyName = GetFontFamilyName(fontStream);
            if (string.IsNullOrEmpty(fontFamilyName))
            {
                _logger?.LogWarning("Could not determine font family name for font '{Font}'", font);
                return null;
            }

            var fontUri = $"fonts:{assemblyName}#{fontFamilyName}";

            _logger?.LogDebug("Retrieved font: {Font} -> {FontUri} (from {Filename})", font, fontUri, registration.Filename);

            // Return the full URI, as the same font family name can exist in multiple font files
            // Ex. OpenSans-Regular.ttf and OpenSans-Semibold.ttf both define "Open Sans" family
            return $"{fontPathUri}#{fontFamilyName}";
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error retrieving font '{Font}'", font);
            return null;
        }
    }

    /// <summary>
    /// Extracts the font family name from a font stream by parsing the font's 'name' table.
    /// For ttf and otf font formats.
    /// </summary>
    /// <param name="fontStream">The font stream to read.</param>
    /// <returns>The font family name if found; otherwise, null.</returns>
    internal string? GetFontFamilyName(Stream fontStream)
    {
        using (BinaryReader reader = new BinaryReader(fontStream))
        {
            // Read the offset table
            reader.ReadUInt32(); // sfnt version
            ushort numTables = ReadUInt16BigEndian(reader);
            reader.ReadUInt16(); // searchRange
            reader.ReadUInt16(); // entrySelector
            reader.ReadUInt16(); // rangeShift

            // Find the 'name' table
            uint nameTableOffset = 0;
            uint nameTableLength = 0;

            for (int i = 0; i < numTables; i++)
            {
                string tag = Encoding.ASCII.GetString(reader.ReadBytes(4));
                reader.ReadUInt32(); // checksum
                uint offset = ReadUInt32BigEndian(reader);
                uint length = ReadUInt32BigEndian(reader);

                if (tag == "name")
                {
                    nameTableOffset = offset;
                    nameTableLength = length;
                    break;
                }
            }

            if (nameTableOffset == 0)
                return null;

            // Read the 'name' table
            fontStream.Seek(nameTableOffset, SeekOrigin.Begin);

            ushort format = ReadUInt16BigEndian(reader);
            ushort count = ReadUInt16BigEndian(reader);
            ushort stringOffset = ReadUInt16BigEndian(reader);

            // Look for font family name (nameID = 1)
            for (int i = 0; i < count; i++)
            {
                ushort platformID = ReadUInt16BigEndian(reader);
                ushort encodingID = ReadUInt16BigEndian(reader);
                ushort languageID = ReadUInt16BigEndian(reader);
                ushort nameID = ReadUInt16BigEndian(reader);
                ushort length = ReadUInt16BigEndian(reader);
                ushort offset = ReadUInt16BigEndian(reader);

                if (nameID == 1) // Font Family name
                {
                    long pos = fontStream.Position;
                    fontStream.Seek(nameTableOffset + stringOffset + offset, SeekOrigin.Begin);

                    byte[] nameBytes = reader.ReadBytes(length);

                    // Decode based on platform
                    string familyName;
                    if (platformID == 3) // Windows
                    {
                        familyName = Encoding.BigEndianUnicode.GetString(nameBytes);
                    }
                    else if (platformID == 1) // Macintosh
                    {
                        familyName = Encoding.ASCII.GetString(nameBytes);
                    }
                    else
                    {
                        familyName = Encoding.UTF8.GetString(nameBytes);
                    }

                    return familyName;
                }
            }
        }

        return null;
    }

    private ushort ReadUInt16BigEndian(BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(2);
        Array.Reverse(bytes);
        return BitConverter.ToUInt16(bytes, 0);
    }

    private uint ReadUInt32BigEndian(BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(4);
        Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
}
