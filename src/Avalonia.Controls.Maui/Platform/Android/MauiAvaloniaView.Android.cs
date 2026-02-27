using Android.Content;
using Avalonia.Controls.Maui.Controls;
using Color = Android.Graphics.Color;

namespace Avalonia.Controls.Maui.Platforms.Android.Handlers
{
    /// <summary>
    /// A platform-specific view for Android that hosts Avalonia content within a .NET MAUI application. This class inherits from Avalonia.Android.AvaloniaView and is responsible for rendering the Avalonia content on Android devices, allowing developers to create cross-platform applications that utilize Avalonia's UI capabilities while leveraging the native performance and features of Android.
    /// </summary>
    public class MauiAvaloniaView : Avalonia.Android.AvaloniaView
    {
        readonly AvaloniaView _mauiView;
        readonly Avalonia.Controls.Border _backgroundBorder = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiAvaloniaView"/> class.
        /// </summary>
        /// <param name="context">The Android context in which the view is running.</param>
        /// <param name="mauiView">The Avalonia view to host within the .NET MAUI application.</param>
        public MauiAvaloniaView(Context context, AvaloniaView mauiView) : base(context)
        {
            _mauiView = mauiView;

            SetBackgroundColor(Color.Transparent);
            Content = _backgroundBorder;
        }

        /// <summary>
        /// Updates the content of the Avalonia view. This method is responsible for setting the Content property of the MauiAvaloniaView to the content of the AvaloniaView, allowing the Avalonia content to be displayed within the Android view hierarchy.
        /// </summary>
        public void UpdateContent()
        {
            _backgroundBorder.Child = _mauiView.Content as Avalonia.Controls.Control;
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
