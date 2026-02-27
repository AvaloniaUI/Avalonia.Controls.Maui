using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Generic base handler for hosting a custom Avalonia control inside a .NET MAUI view.
    /// Subclass this to create a handler that bridges a MAUI <typeparamref name="TVirtualView"/>
    /// to an Avalonia <typeparamref name="TControl"/>.
    /// </summary>
    /// <typeparam name="TVirtualView">The MAUI virtual view type (must implement <see cref="IView"/>).</typeparam>
    /// <typeparam name="TControl">The Avalonia control type to host (must have a parameterless constructor).</typeparam>
    public partial class AvaloniaControlHandler<TVirtualView, TControl>
        where TVirtualView : class, IView
        where TControl : Avalonia.Controls.Control, new()
    {
        /// <summary>
        /// Property mapper that maps MAUI view properties to the platform host view.
        /// Chains from <see cref="Microsoft.Maui.Handlers.ViewHandler.ViewMapper"/> and adds background mapping.
        /// </summary>
        public static IPropertyMapper<TVirtualView, AvaloniaControlHandler<TVirtualView, TControl>> Mapper =
            new PropertyMapper<TVirtualView, AvaloniaControlHandler<TVirtualView, TControl>>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper)
            {
                [nameof(IView.Background)] = MapBackground,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHandler{TVirtualView, TControl}"/> class
        /// using the default <see cref="Mapper"/>.
        /// </summary>
        public AvaloniaControlHandler() : base(Mapper)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHandler{TVirtualView, TControl}"/> class
        /// using a custom property mapper.
        /// </summary>
        /// <param name="mapper">The property mapper to use.</param>
        public AvaloniaControlHandler(IPropertyMapper mapper) : base(mapper)
        {
        }

        /// <summary>
        /// Gets the Avalonia control instance being hosted, or <see langword="null"/> if not yet created.
        /// </summary>
        public TControl? AvaloniaControl { get; private set; }

        /// <summary>
        /// Creates the Avalonia control instance. Override to customize construction.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TControl"/>.</returns>
        protected virtual TControl CreateAvaloniaControl() => new TControl();

        /// <summary>
        /// Creates the Avalonia control, sets it on the platform host view, and calls the lifecycle hook.
        /// </summary>
        protected void InitializeControl()
        {
            AvaloniaControl = CreateAvaloniaControl();

            if (PlatformView is IAvaloniaControlHost host)
            {
                host.SetControl(AvaloniaControl);
            }

            OnAvaloniaControlCreated(AvaloniaControl);
        }

        /// <summary>
        /// Calls the destroying lifecycle hook and removes the control from the platform host view.
        /// </summary>
        protected void CleanupControl()
        {
            OnAvaloniaControlDestroying(AvaloniaControl);

            if (PlatformView is IAvaloniaControlHost host)
            {
                host.SetControl(null);
            }

            AvaloniaControl = null;
        }

        /// <summary>
        /// Called after the Avalonia control has been created and set on the host view.
        /// Override to perform additional initialization.
        /// </summary>
        /// <param name="control">The newly created Avalonia control.</param>
        protected virtual void OnAvaloniaControlCreated(TControl control)
        {
        }

        /// <summary>
        /// Called before the Avalonia control is removed from the host view.
        /// Override to perform cleanup.
        /// </summary>
        /// <param name="control">The Avalonia control being destroyed, or <see langword="null"/> if it was never created.</param>
        protected virtual void OnAvaloniaControlDestroying(TControl? control)
        {
        }

        /// <summary>
        /// Maps the Background property from the MAUI virtual view to the Avalonia host view.
        /// </summary>
        /// <param name="handler">The handler instance.</param>
        /// <param name="view">The MAUI virtual view.</param>
        public static void MapBackground(AvaloniaControlHandler<TVirtualView, TControl> handler, TVirtualView view)
        {
            if (handler.PlatformView is not IAvaloniaControlHost host)
                return;

            if (view is Microsoft.Maui.Controls.View mauiView && mauiView.BackgroundColor is { } mauiColor)
            {
                var avBrush = new Avalonia.Media.SolidColorBrush(
                    new Avalonia.Media.Color(
                        (byte)(mauiColor.Alpha * 255),
                        (byte)(mauiColor.Red * 255),
                        (byte)(mauiColor.Green * 255),
                        (byte)(mauiColor.Blue * 255)));
                host.UpdateBackground(avBrush);
            }
            else
            {
                host.UpdateBackground(null);
            }
        }
    }
}
