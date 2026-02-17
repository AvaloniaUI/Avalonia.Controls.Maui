#if !ANDROID && !IOS && !WINDOWS && !MACOS && !MACCATALYST
using Avalonia.Controls;
using Avalonia.Controls.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers
{
    public partial class AvaloniaViewHandler : Microsoft.Maui.Handlers.ViewHandler<AvaloniaView, Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView>
    {
        protected override Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView CreatePlatformView()
        {
            return new Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView(VirtualView);
        }

        protected override void ConnectHandler(Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView platformView)
        {
            base.ConnectHandler(platformView);

            platformView.Content = VirtualView.Content as Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView;
        }
        
        public static void MapContent(AvaloniaViewHandler handler, AvaloniaView view)
        {
            handler.PlatformView?.UpdateContent();
        }
    }
}


namespace Avalonia.Controls.Maui.Platforms.Standard
{
    public class MauiAvaloniaView : ContentControl
    {
        readonly AvaloniaView _mauiView;

        public MauiAvaloniaView(AvaloniaView mauiView)
        {
            _mauiView = mauiView;
        }

        public void UpdateContent()
        {
            Content = _mauiView.Content as AvaloniaControl;
        }
    }
}
#endif