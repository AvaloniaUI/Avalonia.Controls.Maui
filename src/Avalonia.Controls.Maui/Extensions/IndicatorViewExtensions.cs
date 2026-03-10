using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
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
    /// </summary>
    public static void UpdateNumberOfPages(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.NumberOfPages = indicator.Count;
        pipsPager.UpdateHideSingle(indicator);
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.Position"/> to <see cref="PipsPager.SelectedPageIndex"/>.
    /// </summary>
    public static void UpdateSelectedPageIndex(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.SelectedPageIndex = indicator.Position;
    }

    /// <summary>
    /// Forces the internal ListBox to re-apply the selected index and clears the size constraint.
    /// </summary>
    public static void ForceSelection(this PipsPager pipsPager, IIndicatorView indicator)
    {
        foreach (var descendant in pipsPager.GetVisualDescendants())
        {
            if (descendant is ListBox lb && lb.Name == "PART_PipsPagerList")
            {
                lb.SelectedIndex = -1;
                lb.SelectedIndex = indicator.Position;
                InvalidatePipsSize(lb);
                return;
            }
        }
    }

    /// <summary>
    /// Applies <see cref="IIndicatorView.HideSingle"/>: hides the pager when there is only one page.
    /// </summary>
    public static void UpdateHideSingle(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.IsVisible = !(indicator.Count <= 1 && indicator.HideSingle);
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.MaximumVisible"/> to <see cref="PipsPager.MaxVisiblePips"/>.
    /// </summary>
    public static void UpdateMaxVisiblePips(this PipsPager pipsPager, IIndicatorView indicator)
    {
        pipsPager.MaxVisiblePips = indicator.MaximumVisible;
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.IndicatorSize"/> to pip size and container resources.
    /// </summary>
    public static void UpdatePipSize(this PipsPager pipsPager, IIndicatorView indicator)
    {
        var size = indicator.IndicatorSize;
        var unselectedSize = size * (2.0 / 3.0);

        pipsPager.Resources["PipsPagerPipSize"] = unselectedSize;
        pipsPager.Resources["PipsPagerPipSizeSelected"] = size;
        pipsPager.Resources["PipsPagerPipSizePointerOver"] = size;

        // Skip container sizes when a custom IndicatorTemplate is active; those are set to NaN
        // by UpdateIndicatorTemplate so containers auto-size to the template content.
        var hasCustomTemplate = (indicator as IndicatorView)?.IndicatorTemplate != null;
        if (!hasCustomTemplate)
        {
            var containerMinor = Math.Max(size * 2.0, 12.0);
            var containerMajor = Math.Max(size * 2.0, 24.0);
            pipsPager.Resources["PipsPagerPipContainerMinorSize"] = containerMinor;
            pipsPager.Resources["PipsPagerPipContainerMajorSize"] = containerMajor;
        }

        pipsPager.InvalidateMeasure();
    }

    /// <summary>
    /// Maps <see cref="IIndicatorView.IndicatorColor"/> to the pip foreground brush resource.
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
    /// Maps <see cref="IIndicatorView.SelectedIndicatorColor"/> to the selected pip foreground brush resource.
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
    /// Maps <see cref="IIndicatorView.IndicatorsShape"/> to the pip corner radius resource.
    /// </summary>
    public static void UpdateIndicatorShape(this PipsPager pipsPager, IIndicatorView indicator)
    {
        var isCircle = indicator.IndicatorsShape is not Microsoft.Maui.Controls.Shapes.Rectangle;
        pipsPager.Resources["PipsPagerPipCornerRadius"] = new CornerRadius(isCircle ? 999 : 0);
    }

    /// <summary>
    /// Maps <see cref="IndicatorView.IndicatorTemplate"/> to the pip list's <see cref="ItemsControl.ItemTemplate"/>.
    /// </summary>
    public static void UpdateIndicatorTemplate(this PipsPager pipsPager, IIndicatorView indicator, IMauiContext mauiContext)
    {
        var mauiTemplate = (indicator as IndicatorView)?.IndicatorTemplate;

        void Apply(ListBox pipsList)
        {
            pipsList.ItemTemplate = mauiTemplate != null
                ? new MauiDataTemplateAdapter(mauiTemplate, mauiContext)
                : null;

            if (mauiTemplate != null)
            {
                pipsPager.Resources["PipsPagerPipContainerMinorSize"] = double.NaN;
                pipsPager.Resources["PipsPagerPipContainerMajorSize"] = double.NaN;
            }
            else
            {
                pipsPager.Resources.Remove("PipsPagerPipContainerMinorSize");
                pipsPager.Resources.Remove("PipsPagerPipContainerMajorSize");
            }

            // Rebuild the pip items so the ListBox recreates containers with the updated ItemTemplate.
            var pips = pipsPager.TemplateSettings.Pips;
            var count = pips.Count;
            if (count > 0)
            {
                var savedIndex = pipsPager.SelectedPageIndex;
                pips.Clear();
                for (var i = 1; i <= count; i++)
                    pips.Add(i);
                pipsPager.SelectedPageIndex = Math.Clamp(savedIndex, 0, count - 1);
            }

            InvalidatePipsSize(pipsList);
        }

        foreach (var descendant in pipsPager.GetVisualDescendants())
        {
            if (descendant is ListBox lb && lb.Name == "PART_PipsPagerList")
            {
                Apply(lb);
                return;
            }
        }

        // Defer until the template is applied.
        EventHandler<TemplateAppliedEventArgs>? handler = null;
        handler = (_, e) =>
        {
            pipsPager.TemplateApplied -= handler;
            var list = e.NameScope.Find<ListBox>("PART_PipsPagerList");
            if (list != null)
                Apply(list);
        };
        pipsPager.TemplateApplied += handler;
    }

    /// <summary>
    /// Clears the fixed size constraint on the pips ListBox, allowing the layout system to measure it from its content.
    /// </summary>
    internal static void InvalidatePipsSize(ListBox pipsList)
    {
        pipsList.Width = double.NaN;
        pipsList.Height = double.NaN;
    }
}
