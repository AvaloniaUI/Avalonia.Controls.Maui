using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Handlers.Cells;

/// <summary>
/// Handler for MAUI ImageCell (TextCell with an image)
/// </summary>
public class ImageCellHandler : ElementHandler<ImageCell, global::Avalonia.Controls.Border>
{
    public static IPropertyMapper<ImageCell, ImageCellHandler> Mapper =
        new PropertyMapper<ImageCell, ImageCellHandler>(ElementMapper)
        {
            [nameof(ImageCell.Text)] = MapText,
            [nameof(ImageCell.Detail)] = MapDetail,
            [nameof(ImageCell.TextColor)] = MapTextColor,
            [nameof(ImageCell.DetailColor)] = MapDetailColor,
            [nameof(ImageCell.ImageSource)] = MapImageSource,
        };

    public static CommandMapper<ImageCell, ImageCellHandler> CommandMapper =
        new(ElementCommandMapper);

    private StackPanel? _contentStack;
    private StackPanel? _textStack;
    private global::Avalonia.Controls.Image? _image;
    private TextBlock? _textBlock;
    private TextBlock? _detailBlock;

    public ImageCellHandler() : base(Mapper, CommandMapper)
    {
    }

    public ImageCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ImageCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override global::Avalonia.Controls.Border CreatePlatformElement()
    {
        _image = new global::Avalonia.Controls.Image
        {
            Width = 40,
            Height = 40,
            Stretch = global::Avalonia.Media.Stretch.UniformToFill,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };

        _textBlock = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        _detailBlock = new TextBlock
        {
            FontSize = 13,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center
        };

        _textStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center
        };

        _textStack.Children.Add(_textBlock);
        _textStack.Children.Add(_detailBlock);

        _contentStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 0,
            Margin = new Thickness(16, 12)
        };

        _contentStack.Children.Add(_image);
        _contentStack.Children.Add(_textStack);

        var border = new global::Avalonia.Controls.Border
        {
            Child = _contentStack,
            MinHeight = 64,
            Cursor = new global::Avalonia.Input.Cursor(global::Avalonia.Input.StandardCursorType.Hand)
        };

        // Handle cell taps
        border.Tapped += (s, e) =>
        {
            if (VirtualView?.Command?.CanExecute(VirtualView.CommandParameter) == true)
            {
                VirtualView.Command.Execute(VirtualView.CommandParameter);
            }
        };

        return border;
    }

    public static void MapText(ImageCellHandler handler, ImageCell imageCell)
    {
        if (handler._textBlock is null)
            return;

        handler._textBlock.Text = imageCell.Text ?? string.Empty;
        handler._textBlock.IsVisible = !string.IsNullOrEmpty(imageCell.Text);
    }

    public static void MapDetail(ImageCellHandler handler, ImageCell imageCell)
    {
        if (handler._detailBlock is null)
            return;

        handler._detailBlock.Text = imageCell.Detail ?? string.Empty;
        handler._detailBlock.IsVisible = !string.IsNullOrEmpty(imageCell.Detail);
    }

    public static void MapTextColor(ImageCellHandler handler, ImageCell imageCell)
    {
        if (handler._textBlock is null)
            return;

        if (imageCell.TextColor != null)
        {
            handler._textBlock.Foreground = new global::Avalonia.Media.SolidColorBrush(
                Color.FromArgb(
                    (byte)(imageCell.TextColor.Alpha * 255),
                    (byte)(imageCell.TextColor.Red * 255),
                    (byte)(imageCell.TextColor.Green * 255),
                    (byte)(imageCell.TextColor.Blue * 255)
                )
            );
        }
        else
        {
            handler._textBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    public static void MapDetailColor(ImageCellHandler handler, ImageCell imageCell)
    {
        if (handler._detailBlock is null)
            return;

        if (imageCell.DetailColor != null)
        {
            handler._detailBlock.Foreground = new global::Avalonia.Media.SolidColorBrush(
                Color.FromArgb(
                    (byte)(imageCell.DetailColor.Alpha * 255),
                    (byte)(imageCell.DetailColor.Red * 255),
                    (byte)(imageCell.DetailColor.Green * 255),
                    (byte)(imageCell.DetailColor.Blue * 255)
                )
            );
        }
        else
        {
            handler._detailBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    public static void MapImageSource(ImageCellHandler handler, ImageCell imageCell)
    {
        if (handler._image is null || handler.MauiContext is null)
            return;

        if (imageCell.ImageSource != null)
        {
            handler._image.IsVisible = true;

            // Load image asynchronously using Avalonia's image source services
            _ = LoadImageAsync(handler, imageCell);
        }
        else
        {
            handler._image.IsVisible = false;
            handler._image.Source = null;
        }
    }

    private static async Task LoadImageAsync(ImageCellHandler handler, ImageCell imageCell)
    {
        if (handler._image is null || handler.MauiContext is null || imageCell.ImageSource is null)
            return;

        try
        {
            var services = handler.MauiContext.Services;
            var imageSourceServiceProvider = services.GetRequiredService<IImageSourceServiceProvider>();
            var imageSourceService = imageSourceServiceProvider.GetRequiredImageSourceService(imageCell.ImageSource);

            var result = await imageSourceService.GetImageAsync(imageCell.ImageSource, 1.0f);

            if (handler._image != null && result is IImageSourceServiceResult<global::Avalonia.Media.Imaging.Bitmap> bitmapResult)
            {
                handler._image.Source = bitmapResult.Value;
            }
        }
        catch (Exception)
        {
            // If image loading fails, hide the image for now.
            if (handler._image != null)
            {
                handler._image.IsVisible = false;
                handler._image.Source = null;
            }
        }
    }
}
