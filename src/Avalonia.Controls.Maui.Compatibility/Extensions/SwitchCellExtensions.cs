using Avalonia.Controls.Maui.Extensions;
using Avalonia.Styling;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Extension methods for <see cref="MauiSwitchCell"/>.
/// </summary>
public static class SwitchCellExtensions
{
    /// <summary>
    /// Updates the text of the label from the <see cref="SwitchCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateText(this MauiSwitchCell platformView, SwitchCell cell)
    {
        platformView.Label.Text = cell.Text ?? string.Empty;
    }

    /// <summary>
    /// Updates the toggle state from the <see cref="SwitchCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    /// <param name="isUpdating">A flag indicating if the update originates from the platform.</param>
    public static void UpdateOn(this MauiSwitchCell platformView, SwitchCell cell, bool isUpdating)
    {
        if (isUpdating) return;
        platformView.ToggleSwitch.IsChecked = cell.On;
    }

    /// <summary>
    /// Updates the color of the toggle switch from the <see cref="SwitchCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateOnColor(this MauiSwitchCell platformView, SwitchCell cell)
    {
        // SwitchExtensions.UpdateTrackColor expects an ISwitch. 
        // Since SwitchCell doesn't implement ISwitch, we can't directly use it.
        // However, we can reuse the logic by manually calling the underlying helper method
        // OR we can't easily reuse it without the ISwitch interface.
        // Let's modify SwitchExtensions to be more reusable OR reimplement the styling logic here.
        // Reimplementing logic similar to SwitchExtensions.UpdateTrackColor but for SwitchCell
        
        var trackColor = cell.OnColor;
        var toggleSwitch = platformView.ToggleSwitch;
        
        // Tag used in SwitchExtensions
        const string TrackStyleTag = "__maui_track_style__";

        // Remove existing styles
        for (int i = toggleSwitch.Styles.Count - 1; i >= 0; i--)
        {
            if (toggleSwitch.Styles[i] is Styling.Style style &&
                style.Resources != null &&
                style.Resources.ContainsKey(TrackStyleTag))
            {
                toggleSwitch.Styles.RemoveAt(i);
            }
        }

        if (trackColor != null)
        {
            var brush = trackColor.ToAvaloniaBrush();
            if (brush != null)
            {
                 // Checked state
                var checkedStyle = new global::Avalonia.Styling.Style(x => x.OfType<global::Avalonia.Controls.ToggleSwitch>().Class(":checked").Template().OfType<global::Avalonia.Controls.Border>().Name("SwitchKnobBounds"));
                checkedStyle.Setters.Add(new global::Avalonia.Styling.Setter(global::Avalonia.Controls.Border.BackgroundProperty, brush));
                checkedStyle.Setters.Add(new global::Avalonia.Styling.Setter(global::Avalonia.Controls.Border.BorderBrushProperty, brush));
                checkedStyle.Resources[TrackStyleTag] = true;
                toggleSwitch.Styles.Add(checkedStyle);

                // Checked + pointer over
                var checkedHoverStyle = new global::Avalonia.Styling.Style(x => x.OfType<global::Avalonia.Controls.ToggleSwitch>().Class(":checked").Class(":pointerover").Template().OfType<global::Avalonia.Controls.Border>().Name("SwitchKnobBounds"));
                checkedHoverStyle.Setters.Add(new global::Avalonia.Styling.Setter(global::Avalonia.Controls.Border.BackgroundProperty, brush));
                checkedHoverStyle.Setters.Add(new global::Avalonia.Styling.Setter(global::Avalonia.Controls.Border.BorderBrushProperty, brush));
                checkedHoverStyle.Resources[TrackStyleTag] = true;
                toggleSwitch.Styles.Add(checkedHoverStyle);

                // Checked + pressed
                var checkedPressedStyle = new global::Avalonia.Styling.Style(x => x.OfType<global::Avalonia.Controls.ToggleSwitch>().Class(":checked").Class(":pressed").Template().OfType<global::Avalonia.Controls.Border>().Name("SwitchKnobBounds"));
                checkedPressedStyle.Setters.Add(new global::Avalonia.Styling.Setter(global::Avalonia.Controls.Border.BackgroundProperty, brush));
                checkedPressedStyle.Setters.Add(new global::Avalonia.Styling.Setter(global::Avalonia.Controls.Border.BorderBrushProperty, brush));
                checkedPressedStyle.Resources[TrackStyleTag] = true;
                toggleSwitch.Styles.Add(checkedPressedStyle);
            }
        }
    }

    /// <summary>
    /// Updates the IsEnabled state from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateIsEnabled(this MauiSwitchCell platformView, Cell cell)
    {
        platformView.IsEnabled = cell.IsEnabled;
        // Reduce opacity for clearer disabled visual feedback
        platformView.Opacity = cell.IsEnabled ? 1.0 : 0.75;
    }

    /// <summary>
    /// Updates the context actions (right-click menu) from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateContextActions(this MauiSwitchCell platformView, Cell cell)
    {
        platformView.ContextMenu = CellExtensions.BuildContextMenu(cell);
    }
}
