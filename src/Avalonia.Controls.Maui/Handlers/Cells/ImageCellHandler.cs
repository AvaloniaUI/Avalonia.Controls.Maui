using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers.Cells;

public class ImageCellHandler : ElementHandler<ImageCell, MauiImageCell>
{
    public static IPropertyMapper<ImageCell, ImageCellHandler> Mapper =
        new PropertyMapper<ImageCell, ImageCellHandler>(ElementMapper)
        {
            [nameof(ImageCell.Text)] = MapText,
            [nameof(ImageCell.Detail)] = MapDetail,
            [nameof(ImageCell.TextColor)] = MapTextColor,
            [nameof(ImageCell.DetailColor)] = MapDetailColor,
            [nameof(ImageCell.ImageSource)] = MapImageSource,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            ["ContextActions"] = MapContextActions,
        };

    public static CommandMapper<ImageCell, ImageCellHandler> CommandMapper =
        new(ElementCommandMapper);

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

    protected override MauiImageCell CreatePlatformElement()
    {
        var cell = new MauiImageCell();
        cell.AddHandler(global::Avalonia.Input.InputElement.TappedEvent, OnCellTapped, global::Avalonia.Interactivity.RoutingStrategies.Bubble, true);
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    protected override void DisconnectHandler(MauiImageCell platformView)
    {
        platformView.RemoveHandler(global::Avalonia.Input.InputElement.TappedEvent, OnCellTapped);
        platformView.AttachedToVisualTree -= OnCellAttachedToVisualTree;
        platformView.DetachedFromVisualTree -= OnCellDetachedFromVisualTree;
        base.DisconnectHandler(platformView);
    }

    private void OnCellAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        VirtualView?.SendAppearing();
    }

    private void OnCellDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        VirtualView?.SendDisappearing();
    }

    private void OnCellTapped(object? sender, global::Avalonia.Input.TappedEventArgs e)
    {
        var cell = VirtualView;
        if (cell == null) return;

        if (cell.Command?.CanExecute(cell.CommandParameter) == true)
        {
            cell.Command.Execute(cell.CommandParameter);
        }
    }

    public static void MapText(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateText(imageCell);
    }

    public static void MapDetail(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateDetail(imageCell);
    }

    public static void MapTextColor(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateTextColor(imageCell);
    }

    public static void MapDetailColor(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateDetailColor(imageCell);
    }

    public static void MapImageSource(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateImageSource(imageCell, handler.MauiContext);
    }

    public static void MapIsEnabled(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateIsEnabled(imageCell);
    }

    public static void MapContextActions(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateContextActions(imageCell);
    }
}

