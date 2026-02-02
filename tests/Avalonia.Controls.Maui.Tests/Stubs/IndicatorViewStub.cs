using System.Collections;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Test stub for <see cref="IIndicatorView"/>.
/// </summary>
public class IndicatorViewStub : StubBase, IIndicatorView
{
    /// <inheritdoc/>
    public int Count { get; set; }

    /// <inheritdoc/>
    public int Position { get; set; }

    /// <inheritdoc/>
    public bool HideSingle { get; set; } = true;

    /// <inheritdoc/>
    public int MaximumVisible { get; set; } = int.MaxValue;

    /// <inheritdoc/>
    public double IndicatorSize { get; set; } = 6.0;

    /// <inheritdoc/>
    public Paint? IndicatorColor { get; set; }

    /// <inheritdoc/>
    public Paint? SelectedIndicatorColor { get; set; }

    /// <inheritdoc/>
    public IShape IndicatorsShape { get; set; } = null!;

    /// <inheritdoc/>
    public IEnumerable? ItemsSource { get; set; }
}
