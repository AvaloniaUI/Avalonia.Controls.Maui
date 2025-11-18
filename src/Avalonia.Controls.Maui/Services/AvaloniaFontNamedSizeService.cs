using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Dependency(typeof(Avalonia.Controls.Maui.Services.AvaloniaFontNamedSizeService))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Avalonia.Controls.Maui.Services;

#pragma warning disable CS0612 // Type or member is obsolete
[Obsolete]
internal class AvaloniaFontNamedSizeService : IFontNamedSizeService
{
    public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
    {
        // Standard font sizes for Avalonia
        // These values are based on common cross-platform font sizes
        // and are similar to Windows/WPF defaults

        if (useOldSizes)
        {
            return size switch
            {
                NamedSize.Default => GetDefaultSize(targetElementType),
                NamedSize.Micro => 10,
                NamedSize.Small => 12,
                NamedSize.Medium => 14,
                NamedSize.Large => 18,
                NamedSize.Body => 14,
                NamedSize.Caption => 12,
                NamedSize.Header => 14,
                NamedSize.Subtitle => 16,
                NamedSize.Title => 24,
                _ => throw new ArgumentOutOfRangeException(nameof(size))
            };
        }

        return size switch
        {
            NamedSize.Default => GetDefaultSize(targetElementType),
            NamedSize.Micro => 10,
            NamedSize.Small => 12,
            NamedSize.Medium => 14,
            NamedSize.Large => 18,
            NamedSize.Body => 14,
            NamedSize.Caption => 12,
            NamedSize.Header => 20,
            NamedSize.Subtitle => 16,
            NamedSize.Title => 24,
            _ => throw new ArgumentOutOfRangeException(nameof(size))
        };
    }

    private static double GetDefaultSize(Type targetElementType)
    {
        // Return appropriate default sizes based on element type
        if (typeof(Button).IsAssignableFrom(targetElementType))
            return 14;

        if (typeof(Label).IsAssignableFrom(targetElementType))
            return 12;

        if (typeof(Editor).IsAssignableFrom(targetElementType) ||
            typeof(Entry).IsAssignableFrom(targetElementType) ||
            typeof(SearchBar).IsAssignableFrom(targetElementType))
            return 14;

        return 14; // Default fallback
    }
}
#pragma warning restore CS0612 // Type or member is obsolete
