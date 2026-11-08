using Avalonia.Controls.Maui.Handlers;

namespace Avalonia.Controls.Maui.Platforms.Windows.Handlers
{
    /// <summary>
    /// Windows platform host view that renders an Avalonia control inside a .NET MAUI handler.
    /// Inherits from <see cref="Avalonia.WinUI.AvaloniaSwapChainPanel"/> and wraps the control
    /// in a <see cref="Avalonia.Controls.Border"/> for background support.
    /// </summary>
    public partial class AvaloniaControlHostView : Avalonia.WinUI.AvaloniaSwapChainPanel, IAvaloniaControlHost
    {
        readonly Avalonia.Controls.Border _backgroundBorder = new();
        readonly EmbeddedPopupHost _popupHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHostView"/> class.
        /// </summary>
        public AvaloniaControlHostView()
        {
            // Embedded views are not at the window edges, so skip the safe area padding.
            Avalonia.Controls.TopLevel.SetAutoSafeAreaPadding(_backgroundBorder, false);
            Content = _backgroundBorder;
            _popupHost = EmbeddedPopupHost.Attach(this, _backgroundBorder);
        }

        /// <inheritdoc/>
        public void SetControl(Avalonia.Controls.Control? control)
        {
            _backgroundBorder.Child = control;
        }

        /// <inheritdoc/>
        public void UpdateBackground(Avalonia.Media.IBrush? brush)
        {
            _backgroundBorder.Background = brush;
        }
    }
}
