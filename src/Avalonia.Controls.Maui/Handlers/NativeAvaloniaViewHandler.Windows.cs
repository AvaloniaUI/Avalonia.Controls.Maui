using Avalonia.Controls.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Windows stub partial for <see cref="NativeAvaloniaViewHandler"/>.
    /// Windows/WinUI is not currently supported; this provides a minimal implementation
    /// so the project compiles for the Windows target framework.
    /// </summary>
    public partial class NativeAvaloniaViewHandler : Microsoft.Maui.Handlers.ViewHandler<AvaloniaView, Microsoft.UI.Xaml.Controls.Grid>
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
        /// Maps the Content property. No-op on Windows.
        /// </summary>
        /// <param name="handler">The handler instance.</param>
        /// <param name="view">The AvaloniaView instance.</param>
        public static void MapContent(NativeAvaloniaViewHandler handler, AvaloniaView view)
        {
        }

        /// <summary>
        /// Maps the Background property. No-op on Windows.
        /// </summary>
        /// <param name="handler">The handler instance.</param>
        /// <param name="view">The AvaloniaView instance.</param>
        public static void MapBackground(NativeAvaloniaViewHandler handler, AvaloniaView view)
        {
        }
    }
}
