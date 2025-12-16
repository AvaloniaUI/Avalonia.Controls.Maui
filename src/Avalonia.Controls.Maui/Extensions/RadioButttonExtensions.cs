using Avalonia.Controls.Primitives;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.MauiRadioButton;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods for updating <see cref="PlatformView"/> properties from MAUI
/// <see cref="IRadioButton"/> values.
/// </summary>
public static class RadioButttonExtensions
{
    /// <summary>
    /// Updates the checked state of the platform radio button.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateIsChecked(this PlatformView platformView, IRadioButton radioButton)
    {
        platformView.IsChecked = radioButton.IsChecked;
    }

    /// <summary>
    /// Updates the content of the platform radio button, including support for MAUI view content
    /// via <see cref="IRadioButton.PresentedContent"/> and basic text transformations.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="handler">The handler providing access to <see cref="IMauiContext"/>.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateContent(this PlatformView platformView, IRadioButtonHandler handler, IRadioButton radioButton)
    {
        // Check if a custom ControlTemplate is applied
        bool hasControlTemplate = false;
        if (radioButton is TemplatedView templatedView)
        {
            hasControlTemplate = templatedView.ControlTemplate != null;
        }

        // If PresentedContent is IView, convert it to a platform control.
        if (radioButton.PresentedContent is IView presentedView)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
            platformView.Content = (Control)presentedView.ToPlatform(handler.MauiContext);
            // Hide default visuals only if there's an explicit ControlTemplate
            platformView.ShowIndicator = !hasControlTemplate;
            return;
        }

        // Regular content without PresentedContent, show the default visuals
        platformView.ShowIndicator = true;

        if (radioButton.Content is null)
        {
            platformView.Content = null;
            return;
        }

        // Handle string content
        if (radioButton.Content is string text)
        {
            platformView.Content = text;
            
            // Apply CharacterSpacing if needed
            if (radioButton.CharacterSpacing != 0)
            {
                platformView.UpdateCharacterSpacing(radioButton);
            }

            return;
        }

        // Handle arbitrary content (IView that's not in a ControlTemplate)
        if (radioButton.Content is IView contentView)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
            platformView.Content = (Control)contentView.ToPlatform(handler.MauiContext);
            return;
        }

        platformView.Content = radioButton.Content;
    }

    /// <summary>
    /// Updates the text color (foreground) of the platform radio button.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateTextColor(this PlatformView platformView, IRadioButton radioButton)
    {
        if (radioButton.TextColor != null)
            platformView.Foreground = radioButton.TextColor.ToPlatform();
        else
            platformView.ClearValue(TemplatedControl.ForegroundProperty);
    }

    /// <summary>
    /// Updates the stroke color (border brush) of the platform radio button.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateStrokeColor(this PlatformView platformView, IRadioButton radioButton)
    {
        if (radioButton.StrokeColor != null)
            platformView.BorderBrush = radioButton.StrokeColor.ToPlatform();
        else
            platformView.ClearValue(TemplatedControl.BorderBrushProperty);
    }

    /// <summary>
    /// Updates the stroke thickness (border thickness) of the platform radio button.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateStrokeThickness(this PlatformView platformView, IRadioButton radioButton)
    {
        platformView.BorderThickness = new Thickness(radioButton.StrokeThickness);
    }

    /// <summary>
    /// Updates the corner radius of the platform radio button.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateCornerRadius(this PlatformView platformView, IRadioButton radioButton)
    {
        platformView.CornerRadius = new CornerRadius(radioButton.CornerRadius);
    }

    /// <summary>
    /// Updates the group name used for mutual exclusion among radio buttons.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateGroupName(this PlatformView platformView, IRadioButton radioButton)
    {
        if (radioButton is Microsoft.Maui.Controls.RadioButton mauiRadioButton)
        {
            platformView.GroupName = mauiRadioButton.GroupName;
        }
        else
        {
            platformView.GroupName = string.Empty;
        }
    }

    /// <summary>
    /// Updates the value associated with the radio button by storing it on the platform view.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateValue(this PlatformView platformView, IRadioButton radioButton)
    {
        if (radioButton is BindableObject bindable && 
            radioButton is Microsoft.Maui.Controls.RadioButton)
        {
            var value = bindable.GetValue(Microsoft.Maui.Controls.RadioButton.ValueProperty);
            platformView.Tag = value;
        }
    }

    /// <summary>
    /// Updates character spacing for text content. Converts string content to TextBlock when
    /// CharacterSpacing is non-zero, or updates existing TextBlock letter spacing.
    /// </summary>
    /// <param name="platformView">The Avalonia radio button.</param>
    /// <param name="radioButton">The cross-platform radio button.</param>
    public static void UpdateCharacterSpacing(this PlatformView platformView, IRadioButton radioButton)
    {
        var letterSpacing = radioButton.CharacterSpacing;

        if (platformView.Content is TextBlock textBlock)
        {
            // Update existing TextBlock's letter spacing
            textBlock.LetterSpacing = letterSpacing;
        }
        else if (platformView.Content is string text && radioButton.CharacterSpacing != 0)
        {
            // Convert string to TextBlock to apply letter spacing
            platformView.Content = new TextBlock
            {
                Text = text,
                LetterSpacing = letterSpacing
            };
        }
    }
}