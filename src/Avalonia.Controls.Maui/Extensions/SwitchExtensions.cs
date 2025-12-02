using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.ToggleSwitch;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for mapping .NET MAUI switch properties to Avalonia <see cref="ToggleSwitch"/>.
/// Uses Avalonia's styling system for proper visual state handling and theme compatibility.
/// </summary>
public static class SwitchExtensions
{
    // Tag keys used to identify our custom styles for removal
    private const string TrackStyleTag = "__maui_track_style__";
    private const string ThumbStyleTag = "__maui_thumb_style__";

    /// <summary>
    /// Updates the platform IsChecked state from the virtual view.
    /// </summary>
    public static void UpdateIsOn(this PlatformView platformView, ISwitch virtualView)
    {
        platformView.IsChecked = virtualView.IsOn;
    }

    /// <summary>
    /// Updates the track color using TrackColor or OnColor (fallback).
    /// Uses Avalonia's style system for proper visual state handling.
    /// </summary>
    public static void UpdateTrackColor(
        this PlatformView platformView,
        ISwitch virtualView,
        Microsoft.Maui.Graphics.Color? fallbackColor = null)
    {
        var trackColor = virtualView.TrackColor;

        // Use OnColor as fallback if TrackColor is not set
        if (trackColor == default && fallbackColor is { } onColor)
        {
            trackColor = onColor;
        }
        
        RemoveTaggedStyles(platformView, TrackStyleTag);

        if (trackColor != default)
        {
            var brush = trackColor.ToPlatform();
            UpdateTrack(platformView, brush);
        }
    }

    /// <summary>
    /// Updates the thumb color using Avalonia's style system.
    /// </summary>
    public static void UpdateThumbColor(
        this PlatformView platformView,
        ISwitch virtualView)
    {
        var thumbColor = virtualView.ThumbColor;
        
        RemoveTaggedStyles(platformView, ThumbStyleTag);

        if (thumbColor != default)
        {
            var brush = thumbColor.ToPlatform();
            
            platformView.Foreground = brush;
            
            UpdateThumb(platformView, brush);
        }
        else
        {
            platformView.ClearValue(Primitives.TemplatedControl.ForegroundProperty);
        }
    }

    private static void UpdateTrack(PlatformView platformView, IBrush brush)
    {
        // Checked state
        var checkedStyle = new Style(x => x.OfType<PlatformView>().Class(":checked").Template().OfType<Border>().Name("SwitchKnobBounds"));
        checkedStyle.Setters.Add(new Setter(Border.BackgroundProperty, brush));
        checkedStyle.Setters.Add(new Setter(Border.BorderBrushProperty, brush));
        AddTaggedStyle(platformView, checkedStyle, TrackStyleTag);

        // Checked + pointer over (higher specificity)
        var checkedHoverStyle = new Style(x => x.OfType<PlatformView>().Class(":checked").Class(":pointerover").Template().OfType<Border>().Name("SwitchKnobBounds"));
        checkedHoverStyle.Setters.Add(new Setter(Border.BackgroundProperty, brush));
        checkedHoverStyle.Setters.Add(new Setter(Border.BorderBrushProperty, brush));
        AddTaggedStyle(platformView, checkedHoverStyle, TrackStyleTag);

        // Checked + pressed (highest specificity for checked states)
        var checkedPressedStyle = new Style(x => x.OfType<PlatformView>().Class(":checked").Class(":pressed").Template().OfType<Border>().Name("SwitchKnobBounds"));
        checkedPressedStyle.Setters.Add(new Setter(Border.BackgroundProperty, brush));
        checkedPressedStyle.Setters.Add(new Setter(Border.BorderBrushProperty, brush));
        AddTaggedStyle(platformView, checkedPressedStyle, TrackStyleTag);
    }
    
    private static void UpdateThumb(PlatformView platformView, IBrush brush)
    {
        // Off state
        var offBaseStyle = new Style(x => x.OfType<PlatformView>()
            .Class(":unchecked")
            .Template().OfType<Ellipse>().Name("SwitchKnobOff"));
        offBaseStyle.Setters.Add(new Setter(Shape.FillProperty, brush));
        AddTaggedStyle(platformView, offBaseStyle, ThumbStyleTag);

        // Off + Hover
        var offHoverStyle = new Style(x => x.OfType<PlatformView>()
            .Class(":unchecked").Class(":pointerover")
            .Template().OfType<Ellipse>().Name("SwitchKnobOff"));
        offHoverStyle.Setters.Add(new Setter(Shape.FillProperty, brush));
        AddTaggedStyle(platformView, offHoverStyle, ThumbStyleTag);

        // Off + Pressed
        var offPressedStyle = new Style(x => x.OfType<PlatformView>()
            .Class(":unchecked").Class(":pressed")
            .Template().OfType<Ellipse>().Name("SwitchKnobOff"));
        offPressedStyle.Setters.Add(new Setter(Shape.FillProperty, brush));
        AddTaggedStyle(platformView, offPressedStyle, ThumbStyleTag);
        
        // On state
        var onBaseStyle = new Style(x => x.OfType<PlatformView>()
            .Class(":checked")
            .Template().OfType<Ellipse>().Name("SwitchKnobOn"));
        onBaseStyle.Setters.Add(new Setter(Shape.FillProperty, brush));
        AddTaggedStyle(platformView, onBaseStyle, ThumbStyleTag);

        // On + Hover
        var onHoverStyle = new Style(x => x.OfType<PlatformView>()
            .Class(":checked").Class(":pointerover")
            .Template().OfType<Ellipse>().Name("SwitchKnobOn"));
        onHoverStyle.Setters.Add(new Setter(Shape.FillProperty, brush));
        AddTaggedStyle(platformView, onHoverStyle, ThumbStyleTag);

        // On + Pressed
        var onPressedStyle = new Style(x => x.OfType<PlatformView>()
            .Class(":checked").Class(":pressed")
            .Template().OfType<Ellipse>().Name("SwitchKnobOn"));
        onPressedStyle.Setters.Add(new Setter(Shape.FillProperty, brush));
        AddTaggedStyle(platformView, onPressedStyle, ThumbStyleTag);

        // Fallback
        var simpleKnobStyle = new Style(x => x.OfType<PlatformView>().Template().OfType<Panel>().Name("PART_SwitchKnob"));
        simpleKnobStyle.Setters.Add(new Setter(Panel.BackgroundProperty, brush));
        AddTaggedStyle(platformView, simpleKnobStyle, ThumbStyleTag);
    }

    private static void AddTaggedStyle(PlatformView platformView, Style style, string tag)
    {
        style.Resources[tag] = true;
        platformView.Styles.Add(style);
    }

    private static void RemoveTaggedStyles(PlatformView platformView, string tag)
    {
        for (int i = platformView.Styles.Count - 1; i >= 0; i--)
        {
            if (platformView.Styles[i] is Style style &&
                style.Resources != null &&
                style.Resources.ContainsKey(tag))
            {
                platformView.Styles.RemoveAt(i);
            }
        }
    }
}