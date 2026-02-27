using Avalonia.Layout;
using Avalonia.Controls.Maui.Platforms.Android.Handlers;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Android platform partial for <see cref="AvaloniaControlHandler{TVirtualView, TControl}"/>.
    /// </summary>
    public partial class AvaloniaControlHandler<TVirtualView, TControl> : Microsoft.Maui.Handlers.ViewHandler<TVirtualView, AvaloniaControlHostView>
        where TVirtualView : class, IView
        where TControl : Avalonia.Controls.Control, new()
    {
        /// <summary>
        /// Creates the Android platform host view.
        /// </summary>
        /// <returns>A new <see cref="AvaloniaControlHostView"/> for Android.</returns>
        protected override AvaloniaControlHostView CreatePlatformView()
        {
            return new AvaloniaControlHostView(Context);
        }

        /// <summary>
        /// Connects the handler and initializes the Avalonia control.
        /// </summary>
        /// <param name="platformView">The Android platform view.</param>
        protected override void ConnectHandler(AvaloniaControlHostView platformView)
        {
            base.ConnectHandler(platformView);
            InitializeControl();
        }

        /// <summary>
        /// Disconnects the handler and cleans up the Avalonia control.
        /// </summary>
        /// <param name="platformView">The Android platform view.</param>
        protected override void DisconnectHandler(AvaloniaControlHostView platformView)
        {
            CleanupControl();
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }
    }
}
