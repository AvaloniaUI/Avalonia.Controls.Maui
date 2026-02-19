using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
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

    internal ContentControl? _contentContainer;
    
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
            HorizontalAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Layout.VerticalAlignment.Stretch,
            MinWidth = 0,
            MinHeight = 0
        };

        return _contentContainer;
    }
    
    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);
        this.UpdateContent(VirtualView);
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
        handler.UpdateContent(content);
    }
}