using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers.Shell;

/// <summary>Avalonia handler for <see cref="ShellContent"/>.</summary>
public partial class ShellContentHandler : ElementHandler<ShellContent, AvaloniaControl>
{
    /// <summary>Property mapper for <see cref="ShellContentHandler"/>.</summary>
    public static IPropertyMapper<ShellContent, ShellContentHandler> Mapper =
        new PropertyMapper<ShellContent, ShellContentHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ShellContent.Content)] = MapContent,
        };

    /// <summary>Command mapper for <see cref="ShellContentHandler"/>.</summary>
    public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
        new CommandMapper<ShellContent, ShellContentHandler>(ElementHandler.ElementCommandMapper);

    /// <summary>Content control container for the shell content page.</summary>
    internal ContentControl? _contentContainer;

    /// <summary>Initializes a new instance of <see cref="ShellContentHandler"/>.</summary>
    public ShellContentHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ShellContentHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    public ShellContentHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaControl CreatePlatformElement()
    {
        _contentContainer = new ContentControl
        {
            HorizontalAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Layout.VerticalAlignment.Stretch,
            MinWidth = 0,
            MinHeight = 0
        };

        return _contentContainer;
    }
    
    /// <inheritdoc/>
    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);
        this.UpdateContent(VirtualView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (_contentContainer != null)
        {
            _contentContainer.Content = null;
        }

        base.DisconnectHandler(platformView);
    }
    
    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The shell content handler.</param>
    /// <param name="content">The MAUI ShellContent virtual view.</param>
    public static void MapContent(ShellContentHandler handler, ShellContent content)
    {
        handler.UpdateContent(content);
    }
}