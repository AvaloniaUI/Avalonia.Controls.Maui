
#if !ANDROID && !IOS && !WINDOWS && !MACOS && !MACCATALYST
using Avalonia.Controls;
using Avalonia.Controls.Maui.Handlers;

namespace Avalonia.Controls.Maui.Platforms.Standard
{
    /// <summary>
    /// Standard (desktop/non-platform) host view that renders an Avalonia control inside a .NET MAUI handler.
    /// Wraps the control in a <see cref="Border"/> for background support.
    /// </summary>
    public class AvaloniaControlHostView : ContentControl, IAvaloniaControlHost
    {
        readonly Border _backgroundBorder = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHostView"/> class.
        /// </summary>
        public AvaloniaControlHostView()
        {
            Content = _backgroundBorder;
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
#endif
