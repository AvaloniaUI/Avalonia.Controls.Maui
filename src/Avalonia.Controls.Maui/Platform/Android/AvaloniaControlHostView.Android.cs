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
        readonly EmbeddedPopupHost _popupHost;
        readonly AvaloniaViewFirstFrame _firstFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHostView"/> class.
        /// </summary>
        /// <param name="context">The Android context in which the view is running.</param>
        public AvaloniaControlHostView(Context context) : base(context)
        {
            SetBackgroundColor(Color.Transparent);
            // Embedded views are not at the window edges, so skip the system bar padding.
            Avalonia.Controls.TopLevel.SetAutoSafeAreaPadding(_backgroundBorder, false);
            Content = _backgroundBorder;
            _popupHost = EmbeddedPopupHost.Attach(this, _backgroundBorder);
            _firstFrame = AvaloniaViewFirstFrame.Attach(this, _backgroundBorder);
        }

        /// <inheritdoc/>
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            AvaloniaHostLayout.MeasureChildrenExactly(this, right - left, bottom - top);
            base.OnLayout(changed, left, top, right, bottom);
            _popupHost.OnLayout();
        }

        /// <inheritdoc/>
        protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
        {
            _firstFrame.OnDraw();
            base.DispatchDraw(canvas);
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
