using Avalonia.Controls.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Platforms.iOS.Handlers
{
    /// <summary>
    /// A platform-specific view for iOS that hosts Avalonia content within a .NET MAUI application. This class inherits from Avalonia.iOS.AvaloniaView and is responsible for rendering the Avalonia content on iOS devices, allowing developers to create cross-platform applications that utilize Avalonia's UI capabilities while leveraging the native performance and features of iOS.
    /// </summary>
    public class MauiAvaloniaView : Avalonia.iOS.AvaloniaView
    {
        readonly AvaloniaView _mauiView;
        readonly Avalonia.Controls.Border _backgroundBorder = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiAvaloniaView"/> class.
        /// </summary>
        /// <param name="mauiView">The Avalonia view to host within the .NET MAUI application.</param>
        public MauiAvaloniaView(AvaloniaView mauiView)
        {
            _mauiView = mauiView;
            Content = _backgroundBorder;
        }

        /// <summary>
        /// Updates the content of the Avalonia view.
        /// </summary>
        public void UpdateContent()
        {
            _backgroundBorder.Child = _mauiView.Content as AvaloniaControl;
        }

        /// <summary>
        /// Updates the background of the Avalonia content area.
        /// </summary>
        /// <param name="brush">The Avalonia brush to use as the background, or <see langword="null"/> to use the default theme background.</param>
        public void UpdateBackground(Avalonia.Media.IBrush? brush)
        {
            _backgroundBorder.Background = brush;
        }
    }
}
