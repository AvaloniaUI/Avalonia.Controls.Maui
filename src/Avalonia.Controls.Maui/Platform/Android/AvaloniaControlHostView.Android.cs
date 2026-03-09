using Android.Content;
using Avalonia.Controls.Maui.Handlers;
using Color = Android.Graphics.Color;

namespace Avalonia.Controls.Maui.Platforms.Android.Handlers
{
    /// <summary>
    /// Android platform host view that renders an Avalonia control inside a .NET MAUI handler.
    /// Wraps the control in a <see cref="Avalonia.Controls.Border"/> for background support.
    /// </summary>
    public class AvaloniaControlHostView : Avalonia.Android.AvaloniaView, IAvaloniaControlHost
    {
        readonly Avalonia.Controls.Border _backgroundBorder = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHostView"/> class.
        /// </summary>
        /// <param name="context">The Android context in which the view is running.</param>
        public AvaloniaControlHostView(Context context) : base(context)
        {
            SetBackgroundColor(Color.Transparent);
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
