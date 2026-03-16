using Avalonia.Collections;

// When Avalonia.Controls.PipsPager ships, remove this file entirely.
namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Provides calculated values for use with the <see cref="PipsPager"/>'s control theme or template.
/// </summary>
public class PipsPagerTemplateSettings : AvaloniaObject
{
    private readonly AvaloniaList<int> _pips = new();

    internal PipsPagerTemplateSettings() { }

    /// <summary>Defines the <see cref="Pips"/> property.</summary>
    public static readonly DirectProperty<PipsPagerTemplateSettings, AvaloniaList<int>> PipsProperty =
        AvaloniaProperty.RegisterDirect<PipsPagerTemplateSettings, AvaloniaList<int>>(
            nameof(Pips),
            o => o.Pips);

    /// <summary>
    /// Gets the collection of pip indices.
    /// </summary>
    public AvaloniaList<int> Pips => _pips;
}
