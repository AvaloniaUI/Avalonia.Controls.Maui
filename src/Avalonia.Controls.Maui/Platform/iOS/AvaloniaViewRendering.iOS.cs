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
    /// <para>
    /// Avalonia starts to render a view when the view is created. An embedded view has no size until UIKit lays
    /// it out, and a Metal layer without a size has no drawable. Avalonia fails the whole render pass when it
    /// cannot get a drawable, which also stops the other embedded views from rendering. This class keeps
    /// rendering off while the view has no size.
    /// </para>
    /// <para>
    /// Avalonia renders on its own thread, some frames after the layout that sizes the view. This class renders
    /// the first frame before Core Animation commits that layout. Core Animation displays layers after it lays
    /// out all of them, so a marker layer that needs display renders the first frame of all new views at once.
    /// </para>
    /// <para>
    /// Metal presents a frame asynchronously, and Avalonia does not report when the frame is on screen. After
    /// the first frame is rendered, this class waits for one refresh of the display, so that the frame is
    /// presented before Core Animation commits the layout.
    /// </para>
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
        /// Starts or stops rendering to match the size of the view, and requests the first frame once the view is
        /// in a window. Call this after the view is laid out and after it moves to a window.
        /// </summary>
        public void Update()
        {
            if (_isClosed)
                return;

            var bounds = _view.Bounds;
            var hasSize = bounds.Width > 0 && bounds.Height > 0;

            if (hasSize != _isRendering)
            {
                _isRendering = hasSize;
                if (hasSize)
                    _root.StartRendering();
                else
                    _root.StopRendering();
            }

            if (!hasSize || !_firstFramePending || _view.Window is null)
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
