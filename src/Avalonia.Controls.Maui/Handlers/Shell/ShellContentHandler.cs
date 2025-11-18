using System;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers.Shell;

public partial class ShellContentHandler : ElementHandler<ShellContent, AvaloniaControl>
{
    public static IPropertyMapper<ShellContent, ShellContentHandler> Mapper =
        new PropertyMapper<ShellContent, ShellContentHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ShellContent.Content)] = MapContent,
        };

    public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
        new CommandMapper<ShellContent, ShellContentHandler>(ElementHandler.ElementCommandMapper);

    ContentControl? _contentContainer;

    public ShellContentHandler() : base(Mapper, CommandMapper)
    {
    }

    public ShellContentHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override AvaloniaControl CreatePlatformElement()
    {
        _contentContainer = new ContentControl
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            MinWidth = 0,
            MinHeight = 0
        };

        return _contentContainer;
    }

    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);
        UpdateContent();
    }

    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (_contentContainer != null)
        {
            _contentContainer.Content = null;
        }

        base.DisconnectHandler(platformView);
    }

    public static void MapContent(ShellContentHandler handler, ShellContent content)
    {
        handler.UpdateContent();
    }

    private void UpdateContent()
    {
        if (_contentContainer == null || VirtualView == null || MauiContext == null)
            return;

        // Get the actual page content
        Microsoft.Maui.Controls.Page? page = null;

        if (VirtualView is IShellContentController contentController)
        {
            page = contentController.GetOrCreateContent();
        }
        else if (VirtualView.Content is Microsoft.Maui.Controls.Page contentPage)
        {
            page = contentPage;
        }

        if (page != null)
        {
            var pageHandler = page.ToHandler(MauiContext);
            if (pageHandler?.PlatformView is AvaloniaControl control)
            {
                _contentContainer.Content = control;
            }
        }
        else
        {
            _contentContainer.Content = null;
        }
    }
}
