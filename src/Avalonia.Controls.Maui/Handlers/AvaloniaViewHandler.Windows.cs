using Avalonia.Maui.Controls;
using Avalonia.Maui.Windows;
using Microsoft.Maui.Handlers;
using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers
{
    public partial class AvaloniaViewHandler : Microsoft.Maui.Handlers.ViewHandler<AvaloniaView, MauiAvaloniaView>
    {
        protected override MauiAvaloniaView CreatePlatformView()
        {
            return new MauiAvaloniaView(VirtualView);
        }

        protected override void ConnectHandler(MauiAvaloniaView platformView)
        {
            base.ConnectHandler(platformView);

            platformView.Content = VirtualView.Content as AvaloniaControl;
        }
        
        public static void MapContent(AvaloniaViewHandler handler, AvaloniaView view)
        {
            handler.PlatformView?.UpdateContent();
        }
    }
}