using Avalonia;
using Avalonia.Controls.Maui.Controls;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using PipsPager = Avalonia.Controls.Maui.Controls.PipsPager;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for mapping <see cref="IIndicatorView"/> properties to <see cref="PipsPager"/>.
/// </summary>
public static class IndicatorViewExtensions
{
    /// <summary>
    /// Maps <see cref="IIndicatorView.Count"/> to <see cref="PipsPager.NumberOfPages"/>.
    /// Also applies <see cref="IIndicatorView.HideSingle"/> visibility.
    /// </summary>
    public static void UpdateNumberOfPages(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.NumberOfPages = indicator.Count;
        pipsPager.UpdateHideSingle(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.Position"/> to <see cref="PipsPager.SelectedPageIndex"/>.</summary>
    public static void UpdateSelectedPageIndex(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.SelectedPageIndex = indicator.Position;
    }

    /// <summary>
    /// Applies <see cref="IIndicatorView.HideSingle"/>: hides the pager when there is only one page.
    /// </summary>
    public static void UpdateHideSingle(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.IsVisible = !(indicator.Count <= 1 && indicator.HideSingle);
    }

    /// <summary>Maps <see cref="IIndicatorView.MaximumVisible"/> to <see cref="PipsPager.MaxVisiblePips"/>.</summary>
    public static void UpdateMaxVisiblePips(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.MaxVisiblePips = indicator.MaximumVisible;
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.IndicatorSize"/> to the pip size resources
    /// (<c>PipsPagerPipSize</c>, <c>PipsPagerPipSizeSelected</c>, <c>PipsPagerPipSizePointerOver</c>)
    /// and scales the pip container resources accordingly.
    /// </summary>
    public static void UpdatePipSize(this PipsPager pipsPager, IIndicatorView indicator)
    {
        // When a custom IndicatorTemplate is active, container sizes are NaN for auto-sizing.
        // Setting fixed pixel values here would break the template layout.
        if (pipsPager.IndicatorTemplate != null)
            return;

        var size = indicator.IndicatorSize;
        var unselectedSize = size * (2.0 / 3.0);
        var containerMinor = Math.Max(size * 2.0, 12.0);
        var containerMajor = Math.Max(size * 2.0, 24.0);

        pipsPager.Resources["PipsPagerPipSize"] = unselectedSize;
        pipsPager.Resources["PipsPagerPipSizeSelected"] = size;
        pipsPager.Resources["PipsPagerPipSizePointerOver"] = size;
        pipsPager.Resources["PipsPagerPipContainerMinorSize"] = containerMinor;
        pipsPager.Resources["PipsPagerPipContainerMajorSize"] = containerMajor;

        pipsPager.InvalidatePagerSize(containerMinor);
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.IndicatorColor"/> to the pip foreground brush resource
    /// (<c>PipsPagerSelectionIndicatorForeground</c>).
    /// </summary>
    public static void UpdatePipFill(this PipsPager pipsPager, IIndicatorView indicator)
    {
        var brush = indicator.IndicatorColor?.ToPlatform();
        if (brush is not null)
            pipsPager.Resources["PipsPagerSelectionIndicatorForeground"] = brush;
        else
            pipsPager.Resources.Remove("PipsPagerSelectionIndicatorForeground");
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.SelectedIndicatorColor"/> to the selected pip foreground brush resource
    /// (<c>PipsPagerSelectionIndicatorForegroundSelected</c>).
    /// </summary>
    public static void UpdateSelectedPipFill(this PipsPager pipsPager, IIndicatorView indicator)
    {
        var brush = indicator.SelectedIndicatorColor?.ToPlatform();
        if (brush is not null)
            pipsPager.Resources["PipsPagerSelectionIndicatorForegroundSelected"] = brush;
        else
            pipsPager.Resources.Remove("PipsPagerSelectionIndicatorForegroundSelected");
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.IndicatorsShape"/> to the pip corner radius resource
    /// (<c>PipsPagerPipCornerRadius</c>). Circle shapes use a large radius; rectangles use zero.
    /// </summary>
    public static void UpdateIndicatorShape(this PipsPager pipsPager, IIndicatorView indicator)
    {
        // IIndicatorView.IndicatorsShape (IShape?) is always null; use the concrete enum.
        var shape = (indicator as IndicatorView)?.IndicatorsShape ?? IndicatorShape.Circle;
        var isCircle = shape == IndicatorShape.Circle;
        pipsPager.Resources["PipsPagerPipCornerRadius"] = new CornerRadius(isCircle ? 999 : 0);
    }

    /// <summary>
    /// Maps <see cref="IndicatorView.IndicatorTemplate"/> to <see cref="PipsPager.IndicatorTemplate"/>.
    /// When set, each pip is rendered using the MAUI template with the page number (1-based) as binding context.
    /// </summary>
    public static void UpdateIndicatorTemplate(this PipsPager pipsPager, IIndicatorView indicator, IMauiContext context)
    {
        var template = (indicator as IndicatorView)?.IndicatorTemplate;
        pipsPager.IndicatorTemplate = template is not null
            ? new MauiDataTemplateAdapter(template, context)
            : null;
    }
}
