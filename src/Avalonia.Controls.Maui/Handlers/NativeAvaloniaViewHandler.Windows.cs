using Avalonia.Layout;
using Avalonia.Controls.Maui.Platforms.Windows.Handlers;
using AvaloniaView = Avalonia.Controls.Maui.Controls.AvaloniaView;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// A handler that connects the AvaloniaView control to its Windows platform-specific implementation,
    /// allowing Avalonia content to be hosted within a .NET MAUI application on Windows (WinUI 3).
    /// </summary>
    public partial class NativeAvaloniaViewHandler : Microsoft.Maui.Handlers.ViewHandler<AvaloniaView, MauiAvaloniaView>
    {
        /// <summary>
        /// Creates the platform-specific view for Windows. This method is called when the handler is
        /// initialized and is responsible for creating an instance of the MauiAvaloniaView, which hosts
        /// the Avalonia content in a WinUI 3 swap chain panel.
        /// </summary>
        /// <returns>The platform-specific view for Windows.</returns>
        protected override MauiAvaloniaView CreatePlatformView()
        {
            return new MauiAvaloniaView(VirtualView);
        }

        /// <summary>
        /// Connects the handler to the platform-specific view and pushes the current content into it.
        /// </summary>
        /// <param name="platformView">The platform-specific view for Windows.</param>
        protected override void ConnectHandler(MauiAvaloniaView platformView)
        {
            base.ConnectHandler(platformView);

            platformView.UpdateContent();
        }

        /// <summary>
        /// Disconnects the handler from the platform-specific view, detaching the hosted Avalonia
        /// content so it can be re-used. Swap chain resources are released by the panel itself when
        /// it is unloaded from the WinUI visual tree.
        /// </summary>
        /// <param name="platformView">The platform-specific view for Windows.</param>
        protected override void DisconnectHandler(MauiAvaloniaView platformView)
        {
            platformView.ClearContent();
            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Maps the Content property from the AvaloniaView to the MauiAvaloniaView.
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
        /// Gets the desired size of the platform-specific view based on the content of the AvaloniaView.
        /// Measures the Avalonia content so MAUI layout can accommodate it properly.
        /// </summary>
        /// <param name="widthConstraint">The maximum width that the view can occupy.</param>
        /// <param name="heightConstraint">The maximum height that the view can occupy.</param>
        /// <returns>The desired size of the platform-specific view.</returns>
        public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (VirtualView.Content is Layoutable control)
            {
                control.Measure(new Size(widthConstraint, heightConstraint));

                base.GetDesiredSize(control.DesiredSize.Width, control.DesiredSize.Height);

                return new Microsoft.Maui.Graphics.Size(control.DesiredSize.Width, control.DesiredSize.Height);
            }
            else
            {
                return base.GetDesiredSize(widthConstraint, heightConstraint);
            }
        }
    }
}
