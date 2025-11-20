using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Primitives;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.CheckBox;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for mapping MAUI ICheckBox properties to Avalonia CheckBox controls.
/// </summary>
public static class CheckBoxExtensions
{
    /// <summary>
    /// Updates the checked state of the Avalonia CheckBox from the MAUI ICheckBox.
    /// </summary>
    /// <param name="platformView">The Avalonia CheckBox control to update.</param>
    /// <param name="virtualCheckBox">The .NET MAUI ICheckBox providing the checked state.</param>
    public static void UpdateIsChecked(this PlatformView platformView, ICheckBox virtualCheckBox)
    {
        platformView.IsChecked = virtualCheckBox.IsChecked;
    }

    /// <summary>
    /// Updates the foreground and border brush of the Avalonia CheckBox from the MAUI ICheckBox.
    /// Clears the properties if no foreground is specified.
    /// </summary>
    /// <param name="platformView">The Avalonia CheckBox control to update.</param>
    /// <param name="virtualCheckBox">The .NET MAUI ICheckBox providing the foreground brush.</param>
    public static void UpdateForeground(this PlatformView platformView, ICheckBox virtualCheckBox)
    {
        if (virtualCheckBox.Foreground != null)
        {
            var brush = virtualCheckBox.Foreground.ToPlatform();
            platformView.BorderBrush = brush;
            platformView.Foreground = brush;
        }
        else
        {
            platformView.ClearValue(TemplatedControl.BorderBrushProperty);
            platformView.ClearValue(TemplatedControl.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the background brush of the Avalonia CheckBox from the MAUI IView.
    /// Clears the property if no background is specified.
    /// </summary>
    /// <param name="checkBox">The Avalonia CheckBox control to update.</param>
    /// <param name="view">The .NET MAUI IView providing the background brush.</param>
    public static void UpdateBackground(this PlatformView checkBox, IView view)
    {
        if (view.Background != null)
        {
            checkBox.Background = view.Background.ToPlatform();
        }
        else
        {
            checkBox.ClearValue(TemplatedControl.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the color of the Avalonia CheckBox from the .NET MAUI CheckBox.
    /// The Color property is on the concrete CheckBox class, not the ICheckBox interface.
    /// Uses reflection to access the Color property.
    /// </summary>
    /// <param name="platformView">The Avalonia CheckBox control to update.</param>
    /// <param name="virtualCheckBox">The .NET MAUI ICheckBox providing the color.</param>
    public static void UpdateColor(this PlatformView platformView, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] ICheckBox virtualCheckBox)
    {
        platformView.UpdateForeground(virtualCheckBox);
    }
}