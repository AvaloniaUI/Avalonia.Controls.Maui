using Avalonia.Layout;
using Avalonia.Controls.Maui.Platforms.Windows.Handlers;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Windows platform partial for <see cref="AvaloniaControlHandler{TVirtualView, TControl}"/>.
    /// Hosts the Avalonia control in a WinUI 3 swap chain panel.
    /// </summary>
    public partial class AvaloniaControlHandler<TVirtualView, TControl> : Microsoft.Maui.Handlers.ViewHandler<TVirtualView, AvaloniaControlHostView>
        where TVirtualView : class, IView
        where TControl : Avalonia.Controls.Control, new()
    {
        /// <summary>
        /// Creates the Windows platform host view.
        /// </summary>
        /// <returns>A new <see cref="AvaloniaControlHostView"/> for Windows.</returns>
        protected override AvaloniaControlHostView CreatePlatformView()
        {
            return new AvaloniaControlHostView();
        }

        /// <summary>
        /// Connects the handler and initializes the Avalonia control.
        /// </summary>
        /// <param name="platformView">The Windows platform view.</param>
        protected override void ConnectHandler(AvaloniaControlHostView platformView)
        {
            base.ConnectHandler(platformView);
            InitializeControl();
        }

        /// <summary>
        /// Disconnects the handler and cleans up the Avalonia control.
        /// </summary>
        /// <param name="platformView">The Windows platform view.</param>
        protected override void DisconnectHandler(AvaloniaControlHostView platformView)
        {
            CleanupControl();
            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Gets the desired size by measuring the hosted Avalonia control.
        /// </summary>
        /// <param name="widthConstraint">The maximum width constraint.</param>
        /// <param name="heightConstraint">The maximum height constraint.</param>
        /// <returns>The desired size of the hosted control.</returns>
        public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (AvaloniaControl is Layoutable control)
            {
                control.Measure(new Size(widthConstraint, heightConstraint));
                var size = new Size(control.DesiredSize.Width, control.DesiredSize.Height);
                base.GetDesiredSize(size.Width, size.Height);
                return new Microsoft.Maui.Graphics.Size(size.Width, size.Height);
            }

            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }
    }
}
