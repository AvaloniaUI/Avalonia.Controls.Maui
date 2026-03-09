using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Maui;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace Avalonia.Controls.Maui.Tests.TestUtilities;

public static class AvaloniaPropertyHelpers
{
    public static string? GetNativeText<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        return handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.Text,
            TextBox textBox => textBox.Text,
            ContentControl contentControl => contentControl.Content?.ToString(),
            _ => null
        };
    }

    public static MauiColor? GetNativeTextColor<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        var foreground = handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.Foreground,
            TextBox textBox => textBox.Foreground,
            _ => null
        };

        if (foreground is SolidColorBrush brush)
        {
            return new MauiColor(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }

    public static double GetNativeFontSize<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        return handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.FontSize,
            TextBox textBox => textBox.FontSize,
            _ => throw new NotImplementedException()
        };
    }

    public static double GetNativeCharacterSpacing<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        // Avalonia doesn't have direct character spacing, but it can be approximated via letter spacing
        // For now, return a placeholder
        return 0;
    }

    public static double GetNativeLineHeight<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        return handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.LineHeight,
            _ => throw new NotImplementedException()
        };
    }

    public static Microsoft.Maui.TextAlignment GetNativeHorizontalTextAlignment<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        var alignment = handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.TextAlignment,
            _ => Media.TextAlignment.Start
        };

        return alignment switch
        {
            Media.TextAlignment.Start => Microsoft.Maui.TextAlignment.Start,
            Media.TextAlignment.Left => Microsoft.Maui.TextAlignment.Start,
            Media.TextAlignment.Center => Microsoft.Maui.TextAlignment.Center,
            Media.TextAlignment.End => Microsoft.Maui.TextAlignment.End,
            Media.TextAlignment.Right => Microsoft.Maui.TextAlignment.End,
            _ => throw new NotImplementedException()
        };
    }

    public static Microsoft.Maui.Thickness GetNativePadding<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        var padding = handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.Padding,
            Border border => border.Padding,
            ContentControl contentControl => contentControl.Padding,
            MauiBorder borderView => borderView.Padding,
            _ => throw new NotImplementedException()
        };

        return new Microsoft.Maui.Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
    }

    public static MauiColor? GetNativeBackgroundColor<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        var background = handler.PlatformView switch
        {
            Panel panel => panel.Background,
            Border border => border.Background,
            ContentControl contentControl => contentControl.Background,
            TextBlock textBlock => textBlock.Background,
            MauiBorder control => control.Background,
            _ => throw new NotImplementedException()
        };

        if (background is SolidColorBrush brush)
        {
            return new MauiColor(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }


    public static IReadOnlyList<Avalonia.Controls.Documents.Inline>? GetNativeInlines<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        return handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.Inlines?.ToList(),
            _ => null
        };
    }

    public static int GetNativeInlineCount<THandler>(THandler handler)
        where THandler : IElementHandler
    {
        return handler.PlatformView switch
        {
            TextBlock textBlock => textBlock.Inlines?.Count ?? 0,
            _ => 0
        };
    }

    public static string? GetNativeRunText<THandler>(THandler handler, int index)
        where THandler : IElementHandler
    {
        if (handler.PlatformView is TextBlock textBlock && textBlock.Inlines != null)
        {
            var inline = textBlock.Inlines.ElementAtOrDefault(index);
            if (inline is Avalonia.Controls.Documents.Run run)
            {
                return run.Text;
            }
        }
        return null;
    }

    public static MauiColor? GetNativeRunForeground<THandler>(THandler handler, int index)
        where THandler : IElementHandler
    {
        if (handler.PlatformView is TextBlock textBlock && textBlock.Inlines != null)
        {
            var inline = textBlock.Inlines.ElementAtOrDefault(index);
            if (inline is Avalonia.Controls.Documents.Run run && run.Foreground is SolidColorBrush brush)
            {
                return new MauiColor(
                    brush.Color.R / 255f,
                    brush.Color.G / 255f,
                    brush.Color.B / 255f,
                    brush.Color.A / 255f);
            }
        }
        return null;
    }

}
