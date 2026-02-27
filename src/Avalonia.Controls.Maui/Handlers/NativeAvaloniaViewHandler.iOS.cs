using Avalonia.Layout;
using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui.Handlers;
using AvaloniaControl = Avalonia.Controls.Control;
using Avalonia.Controls.Maui.Platforms.iOS;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// A handler that connects the AvaloniaView control to its platform-specific implementations, allowing Avalonia content to be hosted within a .NET MAUI application across multiple platforms.
    /// </summary>
    public partial class NativeAvaloniaViewHandler : Microsoft.Maui.Handlers.ViewHandler<AvaloniaView, Avalonia.Controls.Maui.Platforms.iOS.Handlers.MauiAvaloniaView>
    {
        /// <summary>
        /// Creates the platform-specific view for iOS. This method is called when the handler is initialized and is responsible for creating an instance of the MauiAvaloniaView, which hosts the Avalonia content on iOS.
        /// </summary>
        /// <returns>The platform-specific view for iOS.</returns>
        protected override Avalonia.Controls.Maui.Platforms.iOS.Handlers.MauiAvaloniaView CreatePlatformView()
        {
            return new Avalonia.Controls.Maui.Platforms.iOS.Handlers.MauiAvaloniaView(VirtualView);
        }

        /// <summary>
        /// Connects the handler to the platform-specific view. This method is called after the platform
        /// view has been created and is responsible for setting up any necessary event handlers or bindings between the AvaloniaView and the MauiAvaloniaView.
        /// </summary>
        /// <param name="platformView">The platform-specific view for iOS.</param>
        protected override void ConnectHandler(Avalonia.Controls.Maui.Platforms.iOS.Handlers.MauiAvaloniaView platformView)
        {
            base.ConnectHandler(platformView);

            platformView.UpdateContent();
        }

        /// <summary>
        /// Disconnects the handler from the platform-specific view. This method is called when the handler is being disposed and is responsible for cleaning up any event handlers or bindings that were set up in the ConnectHandler method, as well as disposing of the platform view if necessary.
        /// </summary>
        /// <param name="platformView">The platform-specific view for iOS.</param>
        protected override void DisconnectHandler(Avalonia.Controls.Maui.Platforms.iOS.Handlers.MauiAvaloniaView platformView)
        {
            platformView.Dispose();

            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Maps the Content property from the AvaloniaView to the MauiAvaloniaView. This method is called whenever the Content property of the AvaloniaView changes and is responsible for updating the content displayed in the MauiAvaloniaView accordingly.
        /// </summary>
        /// <param name="handler">The handler that manages the connection between the AvaloniaView and the MauiAvaloniaView.</param>
        /// <param name="view">The AvaloniaView instance whose content is being mapped.</param>
        public static void MapContent(NativeAvaloniaViewHandler handler, AvaloniaView view)
        {
            handler.PlatformView?.UpdateContent();
        }

        /// <summary>
        /// Maps the Background property from the AvaloniaView to the Avalonia content area background.
        /// Converts MAUI's <see cref="Microsoft.Maui.Controls.Brush"/> to an Avalonia brush.
        /// </summary>
        /// <param name="handler">The handler that manages the connection between the AvaloniaView and the MauiAvaloniaView.</param>
        /// <param name="view">The AvaloniaView instance whose background is being mapped.</param>
        public static void MapBackground(NativeAvaloniaViewHandler handler, AvaloniaView view)
        {
            if (handler.PlatformView is null)
                return;

            var mauiColor = view.BackgroundColor;
            if (mauiColor is not null)
            {
                var avBrush = new Avalonia.Media.SolidColorBrush(
                    new Avalonia.Media.Color(
                        (byte)(mauiColor.Alpha * 255),
                        (byte)(mauiColor.Red * 255),
                        (byte)(mauiColor.Green * 255),
                        (byte)(mauiColor.Blue * 255)));
                handler.PlatformView.UpdateBackground(avBrush);
            }
            else
            {
                handler.PlatformView.UpdateBackground(null);
            }
        }

        /// <summary>
        /// Gets the desired size of the platform-specific view based on the content of the AvaloniaView. This method is called during the layout process and is responsible for measuring the content of the AvaloniaView and returning the appropriate size for the MauiAvaloniaView to ensure that it can accommodate the Avalonia content properly.
        /// </summary>
        /// <param name="widthConstraint">The maximum width that the view can occupy.</param>
        /// <param name="heightConstraint">The maximum height that the view can occupy.</param>
        /// <returns>The desired size of the platform-specific view.</returns>
        public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (VirtualView.Content is Layoutable control)
            {
                control.Measure(new Size(widthConstraint, heightConstraint));

                var size = new Size(control.DesiredSize.Width, control.DesiredSize.Height);

                base.GetDesiredSize(size.Width, size.Height);

                return new Microsoft.Maui.Graphics.Size(size.Width, size.Height);
            }
            else
            {
                return base.GetDesiredSize(widthConstraint, heightConstraint);
            }
        }
    }
}