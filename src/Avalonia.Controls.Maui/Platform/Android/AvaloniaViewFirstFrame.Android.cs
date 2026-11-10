using Android.Graphics;
using Android.Views;

namespace Avalonia.Controls.Maui.Platforms.Android.Handlers
{
    /// <summary>
    /// Renders the first frame of an embedded Avalonia view before Android draws the window frame that shows it.
    /// </summary>
    /// <remarks>
    /// Avalonia renders to a <see cref="SurfaceView"/> that is behind the window, and makes the window
    /// transparent over it from the first draw. Avalonia renders on its own thread, some frames after Android
    /// creates the surface, so the area is empty until then. Android creates the surfaces of all new views
    /// before it draws the window, so the first host that draws renders the first frame of all of them at once.
    /// </remarks>
    internal sealed class AvaloniaViewFirstFrame : Java.Lang.Object, ISurfaceHolderCallback
    {
        private static bool s_firstFrameRequested;

        private readonly View _host;
        private readonly Visual _content;
        private bool _firstFramePending;

        private AvaloniaViewFirstFrame(View host, Visual content)
        {
            _host = host;
            _content = content;
        }

        /// <summary>Watches the Avalonia surface of <paramref name="host"/>.</summary>
        /// <param name="host">The view that hosts the Avalonia content.</param>
        /// <param name="content">A visual of the hosted Avalonia content.</param>
        public static AvaloniaViewFirstFrame Attach(ViewGroup host, Visual content)
        {
            var firstFrame = new AvaloniaViewFirstFrame(host, content);

            for (var i = 0; i < host.ChildCount; i++)
            {
                if (host.GetChildAt(i) is SurfaceView { Holder: { } holder })
                {
                    holder.AddCallback(firstFrame);
                    break;
                }
            }

            return firstFrame;
        }

        /// <summary>Renders the requested first frames. Call this before the host draws its children.</summary>
        public void OnDraw()
        {
            if (!s_firstFrameRequested)
                return;

            s_firstFrameRequested = false;
            EmbeddedFrameSync.RenderNow(_content);
        }

        /// <inheritdoc/>
        public void SurfaceCreated(ISurfaceHolder holder) => _firstFramePending = true;

        /// <inheritdoc/>
        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            if (!_firstFramePending || width <= 0 || height <= 0)
                return;

            _firstFramePending = false;
            s_firstFrameRequested = true;
            _host.Invalidate();
        }

        /// <inheritdoc/>
        public void SurfaceDestroyed(ISurfaceHolder holder) => _firstFramePending = false;
    }
}
