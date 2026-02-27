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

#if ANDROID
        private Microsoft.Maui.Controls.VisualElement? _trackedParent;
#endif

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

#if ANDROID
            TrackParentBackground(null);
#endif

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
        /// Converts a MAUI <see cref="Microsoft.Maui.Graphics.Color"/> to an Avalonia <see cref="Avalonia.Media.SolidColorBrush"/>.
        /// </summary>
        /// <param name="color">The MAUI color to convert.</param>
        /// <returns>An Avalonia solid color brush.</returns>
        private static Avalonia.Media.SolidColorBrush ConvertColor(Microsoft.Maui.Graphics.Color color)
        {
            return new Avalonia.Media.SolidColorBrush(
                new Avalonia.Media.Color(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255)));
        }

        /// <summary>
        /// Maps the Background property from the MAUI virtual view to the Avalonia host view.
        /// On Android, if the view has no explicit background, walks the parent chain to find
        /// the nearest ancestor with a background color and syncs it to the Avalonia view.
        /// </summary>
        /// <param name="handler">The handler instance.</param>
        /// <param name="view">The MAUI virtual view.</param>
        public static void MapBackground(AvaloniaControlHandler<TVirtualView, TControl> handler, TVirtualView view)
        {
            if (handler.PlatformView is not IAvaloniaControlHost host)
                return;

            if (view is Microsoft.Maui.Controls.View mauiView)
            {
                if (mauiView.BackgroundColor is { } color)
                {
#if ANDROID
                    handler.TrackParentBackground(null);
#endif
                    host.UpdateBackground(ConvertColor(color));
                    return;
                }

                if (!Microsoft.Maui.Controls.Brush.IsNullOrEmpty(mauiView.Background)
                    && mauiView.Background is Microsoft.Maui.Controls.SolidColorBrush scb
                    && scb.Color is { } brushColor)
                {
#if ANDROID
                    handler.TrackParentBackground(null);
#endif
                    host.UpdateBackground(ConvertColor(brushColor));
                    return;
                }

#if ANDROID
                var (parentColor, parentElement) = ResolveParentBackground(mauiView);
                handler.TrackParentBackground(parentElement);
                host.UpdateBackground(parentColor != null ? ConvertColor(parentColor) : null);
                return;
#endif
            }

#if ANDROID
            handler.TrackParentBackground(null);
#endif
            host.UpdateBackground(null);
        }

#if ANDROID
        /// <summary>
        /// Walks the MAUI parent chain to find the nearest ancestor with a background color.
        /// </summary>
        /// <param name="view">The MAUI view to start searching from.</param>
        /// <returns>A tuple of the resolved color (or null) and the parent element that owns it.</returns>
        private static (Microsoft.Maui.Graphics.Color?, Microsoft.Maui.Controls.VisualElement?) ResolveParentBackground(Microsoft.Maui.Controls.View view)
        {
            var parent = view.Parent;
            while (parent != null)
            {
                if (parent is Microsoft.Maui.Controls.VisualElement ve)
                {
                    if (ve.BackgroundColor is { } color)
                        return (color, ve);

                    if (!Microsoft.Maui.Controls.Brush.IsNullOrEmpty(ve.Background)
                        && ve.Background is Microsoft.Maui.Controls.SolidColorBrush scb
                        && scb.Color is { } brushColor)
                        return (brushColor, ve);
                }

                parent = (parent as Microsoft.Maui.Controls.Element)?.Parent;
            }

            return (null, null);
        }

        /// <summary>
        /// Subscribes to a parent element's PropertyChanged event to track background changes,
        /// unsubscribing from any previously tracked parent.
        /// </summary>
        /// <param name="newParent">The new parent element to track, or null to stop tracking.</param>
        private void TrackParentBackground(Microsoft.Maui.Controls.VisualElement? newParent)
        {
            if (_trackedParent == newParent)
                return;

            if (_trackedParent != null)
                _trackedParent.PropertyChanged -= OnTrackedParentPropertyChanged;

            _trackedParent = newParent;

            if (_trackedParent != null)
                _trackedParent.PropertyChanged += OnTrackedParentPropertyChanged;
        }

        private void OnTrackedParentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Microsoft.Maui.Controls.VisualElement.BackgroundColor)
                or nameof(Microsoft.Maui.Controls.VisualElement.Background))
            {
                if (VirtualView is TVirtualView view)
                    MapBackground(this, view);
            }
        }
#endif
    }
}
