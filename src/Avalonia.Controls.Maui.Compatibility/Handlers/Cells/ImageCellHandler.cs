using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Avalonia handler for <see cref="ImageCell"/>.
/// </summary>
public class ImageCellHandler : ElementHandler<ImageCell, MauiImageCell>
{
    /// <summary>
    /// Property mapper for <see cref="ImageCellHandler"/>.
    /// </summary>
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

    /// <summary>
    /// Command mapper for <see cref="ImageCellHandler"/>.
    /// </summary>
    public static CommandMapper<ImageCell, ImageCellHandler> CommandMapper =
        new(ElementCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCellHandler"/> class.
    /// </summary>
    public ImageCellHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCellHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ImageCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCellHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ImageCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform element for this handler.
    /// </summary>
    protected override MauiImageCell CreatePlatformElement()
    {
        var cell = new MauiImageCell();
        cell.AddHandler(global::Avalonia.Input.InputElement.TappedEvent, OnCellTapped, global::Avalonia.Interactivity.RoutingStrategies.Bubble, true);
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Maps the Text property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapText(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateText(imageCell);
    }

    /// <summary>
    /// Maps the Detail property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapDetail(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateDetail(imageCell);
    }

    /// <summary>
    /// Maps the TextColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapTextColor(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateTextColor(imageCell);
    }

    /// <summary>
    /// Maps the DetailColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapDetailColor(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateDetailColor(imageCell);
    }

    /// <summary>
    /// Maps the ImageSource property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapImageSource(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateImageSource(imageCell, handler.MauiContext);
    }

    /// <summary>
    /// Maps the IsEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapIsEnabled(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateIsEnabled(imageCell);
    }

    /// <summary>
    /// Maps the ContextActions property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ImageCell.</param>
    /// <param name="imageCell">The MAUI ImageCell virtual view.</param>
    public static void MapContextActions(ImageCellHandler handler, ImageCell imageCell)
    {
        handler.PlatformView.UpdateContextActions(imageCell);
    }
}

