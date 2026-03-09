using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Windows stub partial for <see cref="AvaloniaControlHandler{TVirtualView, TControl}"/>.
    /// Windows/WinUI is not currently supported; this provides a minimal implementation
    /// so the project compiles for the Windows target framework.
    /// </summary>
    public partial class AvaloniaControlHandler<TVirtualView, TControl> : Microsoft.Maui.Handlers.ViewHandler<TVirtualView, Microsoft.UI.Xaml.Controls.Grid>
        where TVirtualView : class, IView
        where TControl : Avalonia.Controls.Control, new()
    {
        /// <summary>
        /// Creates a stub platform view for Windows.
        /// </summary>
        /// <returns>An empty <see cref="Microsoft.UI.Xaml.Controls.Grid"/>.</returns>
        protected override Microsoft.UI.Xaml.Controls.Grid CreatePlatformView()
        {
            return new Microsoft.UI.Xaml.Controls.Grid();
        }

        /// <summary>
        /// Connects the handler. Calls <see cref="InitializeControl"/> to set up the Avalonia control.
        /// </summary>
        /// <param name="platformView">The Windows platform view.</param>
        protected override void ConnectHandler(Microsoft.UI.Xaml.Controls.Grid platformView)
        {
            base.ConnectHandler(platformView);
            InitializeControl();
        }

        /// <summary>
        /// Disconnects the handler and cleans up the Avalonia control.
        /// </summary>
        /// <param name="platformView">The Windows platform view.</param>
        protected override void DisconnectHandler(Microsoft.UI.Xaml.Controls.Grid platformView)
        {
            CleanupControl();
            base.DisconnectHandler(platformView);
        }
    }
}
