
#if !ANDROID && !IOS && !MACOS && !MACCATALYST
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Standard (desktop/non-platform) partial for <see cref="AvaloniaControlHandler{TVirtualView, TControl}"/>.
    /// </summary>
    public partial class AvaloniaControlHandler<TVirtualView, TControl> : ViewHandler<TVirtualView, Platforms.Standard.AvaloniaControlHostView>
        where TVirtualView : class, IView
        where TControl : Avalonia.Controls.Control, new()
    {
        /// <summary>
        /// Creates the standard platform host view.
        /// </summary>
        /// <returns>A new <see cref="Platforms.Standard.AvaloniaControlHostView"/>.</returns>
        protected override Platforms.Standard.AvaloniaControlHostView CreatePlatformView()
        {
            return new Platforms.Standard.AvaloniaControlHostView();
        }

        /// <summary>
        /// Connects the handler and initializes the Avalonia control.
        /// </summary>
        /// <param name="platformView">The standard platform view.</param>
        protected override void ConnectHandler(Platforms.Standard.AvaloniaControlHostView platformView)
        {
            base.ConnectHandler(platformView);
            InitializeControl();
        }

        /// <summary>
        /// Disconnects the handler and cleans up the Avalonia control.
        /// </summary>
        /// <param name="platformView">The standard platform view.</param>
        protected override void DisconnectHandler(Platforms.Standard.AvaloniaControlHostView platformView)
        {
            CleanupControl();
            base.DisconnectHandler(platformView);
        }
    }
}
#endif
