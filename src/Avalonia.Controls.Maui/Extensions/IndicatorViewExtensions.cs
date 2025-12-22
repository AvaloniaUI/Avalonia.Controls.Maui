using Microsoft.Maui;
using Microsoft.Maui.Platform;
using MauiIndicatorView = Avalonia.Controls.Maui.Controls.MauiIndicatorView;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods to map <see cref="IIndicatorView"/> properties onto the
/// <see cref="MauiIndicatorView"/> platform control.
/// </summary>
public static class IndicatorViewExtensions
{
    /// <summary>
    /// Updates the indicator count on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the count value.</param>
    public static void UpdateCount(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        platformView.Count = indicatorView.Count;
    }

    /// <summary>
    /// Updates the current position on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the position value.</param>
    public static void UpdatePosition(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        platformView.Position = indicatorView.Position;
    }

    /// <summary>
    /// Updates the hide single behavior on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the hide single value.</param>
    /// <remarks>
    /// When HideSingle is true and Count is 1, the indicator view will be hidden.
    /// </remarks>
    public static void UpdateHideSingle(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        platformView.HideSingle = indicatorView.HideSingle;
    }

    /// <summary>
    /// Updates the maximum visible indicators on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the maximum visible value.</param>
    /// <remarks>
    /// Limits the number of indicators shown even if Count is higher.
    /// </remarks>
    public static void UpdateMaximumVisible(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        platformView.MaximumVisible = indicatorView.MaximumVisible;
    }

    /// <summary>
    /// Updates the indicator size on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the indicator size value.</param>
    /// <remarks>
    /// The default indicator size is 6.0 device-independent units.
    /// </remarks>
    public static void UpdateIndicatorSize(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        platformView.IndicatorSize = indicatorView.IndicatorSize;
    }

    /// <summary>
    /// Updates the indicator color on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the indicator color.</param>
    /// <remarks>
    /// This color is applied to unselected indicators.
    /// </remarks>
    public static void UpdateIndicatorColor(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        if (indicatorView.IndicatorColor != null)
        {
            platformView.IndicatorColor = indicatorView.IndicatorColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(MauiIndicatorView.IndicatorColorProperty);
        }
    }

    /// <summary>
    /// Updates the selected indicator color on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the selected indicator color.</param>
    /// <remarks>
    /// This color is applied to the currently selected indicator.
    /// </remarks>
    public static void UpdateSelectedIndicatorColor(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        if (indicatorView.SelectedIndicatorColor != null)
        {
            platformView.SelectedIndicatorColor = indicatorView.SelectedIndicatorColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(MauiIndicatorView.SelectedIndicatorColorProperty);
        }
    }

    /// <summary>
    /// Updates the indicator shape on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the indicator shape.</param>
    /// <remarks>
    /// Indicators can be either Circle (ellipse) or Square (rectangle).
    /// </remarks>
    public static void UpdateIndicatorsShape(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        // Check if the shape is a circle (Ellipse) or square (Rectangle)
        platformView.IsCircleShape = indicatorView.IndicatorsShape is Microsoft.Maui.Graphics.IShape shape &&
                                     shape.GetType().Name.Contains("Ellipse");
    }

    /// <summary>
    /// Updates the items source on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the items source.</param>
    /// <remarks>
    /// When ItemsSource is set, the control automatically updates its Count property
    /// based on the number of items in the collection. Observable collections are
    /// supported for dynamic updates when items are added or removed.
    /// Note: ItemsSource is on the IndicatorView control, not the IIndicatorView interface.
    /// </remarks>
    public static void UpdateItemsSource(this MauiIndicatorView platformView, IIndicatorView indicatorView)
    {
        if (indicatorView is Microsoft.Maui.Controls.IndicatorView concreteView)
        {
            platformView.ItemsSource = concreteView.ItemsSource;
        }
    }

    /// <summary>
    /// Updates the indicator template on the Avalonia MauiIndicatorView.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiIndicatorView control.</param>
    /// <param name="indicatorView">The .NET MAUI view providing the indicator template.</param>
    /// <param name="mauiContext">The MauiContext for platform conversion.</param>
    /// <remarks>
    /// When IndicatorTemplate is set, custom indicator visuals are created using the template.
    /// Note: IndicatorTemplate is on the IndicatorView control, not the IIndicatorView interface.
    /// </remarks>
    public static void UpdateIndicatorTemplate(this MauiIndicatorView platformView, IIndicatorView indicatorView, Microsoft.Maui.IMauiContext? mauiContext)
    {
        if (indicatorView is not Microsoft.Maui.Controls.IndicatorView concreteView)
        {
            platformView.IndicatorTemplate = null;
            return;
        }

        if (concreteView.IndicatorTemplate == null)
        {
            platformView.IndicatorTemplate = null;
            return;
        }

        // Create a Func that uses the MAUI DataTemplate to create indicator controls
        platformView.IndicatorTemplate = (index, isSelected) =>
        {
            if (mauiContext == null)
                return CreateDefaultIndicator(platformView, isSelected);

            try
            {
                // Create MAUI view from template
                var mauiView = concreteView.IndicatorTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return CreateDefaultIndicator(platformView, isSelected);

                // Set the binding context with anonymous type
                mauiView.BindingContext = new { Index = index, IsSelected = isSelected };

                // Convert to platform control
                var platformControl = (Control)mauiView.ToPlatform(mauiContext);
                return platformControl ?? CreateDefaultIndicator(platformView, isSelected);
            }
            catch
            {
                return CreateDefaultIndicator(platformView, isSelected);
            }
        };
    }

    private static Control CreateDefaultIndicator(MauiIndicatorView view, bool isSelected)
    {
        var brush = isSelected ? view.SelectedIndicatorColor : view.IndicatorColor;
        var size = Math.Max(0, view.IndicatorSize);

        if (view.IsCircleShape)
        {
            return new Shapes.Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush,
                Margin = new Thickness(2)
            };
        }
        else
        {
            return new Shapes.Rectangle
            {
                Width = size,
                Height = size,
                Fill = brush,
                Margin = new Thickness(2)
            };
        }
    }
}
