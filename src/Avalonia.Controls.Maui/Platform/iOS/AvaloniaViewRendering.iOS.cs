using System;
using System.Threading;
using Avalonia.Controls.Embedding;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Avalonia.Controls.Maui.Platforms.iOS.Handlers
{
    /// <summary>
    /// Controls when the Avalonia content of an embedded UIKit view renders, so that the content is on screen in
    /// the same frame as the view.
    /// </summary>
    /// <remarks>
    /// Rendering stays off while the view has no size or no window, because a Metal layer without a drawable
    /// blocks the shared render loop. The first frame renders from a marker layer's display pass, before Core
    /// Animation commits the layout, then waits one display refresh so Metal presents it in time.
    /// </remarks>
    internal sealed class AvaloniaViewRendering
    {
        private static bool s_firstFrameRequested;

        private readonly UIView _view;
        private readonly EmbeddableControlRoot _root;
        private readonly FirstFrameLayer _firstFrameLayer;
        private bool _isRendering;
        private bool _isClosed;
        private bool _firstFramePending = true;

        private AvaloniaViewRendering(UIView view, EmbeddableControlRoot root)
        {
            _view = view;
            _root = root;
            _root.Closed += OnClosed;
            _root.StopRendering();

            _firstFrameLayer = new FirstFrameLayer(this) { Frame = new CGRect(0, 0, 1, 1) };
            _view.Layer.AddSublayer(_firstFrameLayer);
        }

        /// <summary>Takes control of the rendering of <paramref name="topLevel"/>, which <paramref name="view"/> hosts.</summary>
        /// <returns>The rendering control, or null if <paramref name="topLevel"/> is not an embedded root.</returns>
        public static AvaloniaViewRendering? Attach(UIView view, TopLevel? topLevel)
            => topLevel is EmbeddableControlRoot root ? new AvaloniaViewRendering(view, root) : null;

        /// <summary>
        /// Starts or stops rendering to match the size of the view and whether it is in a window, and requests the
        /// first frame. Call this after the view is laid out and after it moves to or from a window.
        /// </summary>
        public void Update()
        {
            if (_isClosed)
                return;

            var bounds = _view.Bounds;
            var canRender = bounds.Width > 0 && bounds.Height > 0 && _view.Window is not null;

            if (canRender != _isRendering)
            {
                _isRendering = canRender;
                if (canRender)
                    _root.StartRendering();
                else
                    _root.StopRendering();
            }

            if (!canRender || !_firstFramePending)
                return;

            if (UIApplication.SharedApplication.ApplicationState == UIApplicationState.Background)
                return;

            _firstFramePending = false;
            s_firstFrameRequested = true;
            _firstFrameLayer.SetNeedsDisplay();
        }

        private void RenderFirstFrame()
        {
            if (!s_firstFrameRequested || _isClosed)
                return;

            s_firstFrameRequested = false;
            if (!EmbeddedFrameSync.RenderNow(_root))
                return;

            var framesPerSecond = _view.Window?.Screen.MaximumFramesPerSecond ?? 0;
            Thread.Sleep(TimeSpan.FromSeconds(1.0 / (framesPerSecond > 0 ? framesPerSecond : 60)));
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _isClosed = true;
            _root.Closed -= OnClosed;
            _firstFrameLayer.RemoveFromSuperLayer();
        }

        private sealed class FirstFrameLayer : CALayer
        {
            private readonly AvaloniaViewRendering _owner;

            public FirstFrameLayer(AvaloniaViewRendering owner) => _owner = owner;

            public override void Display() => _owner.RenderFirstFrame();
        }
    }
}
