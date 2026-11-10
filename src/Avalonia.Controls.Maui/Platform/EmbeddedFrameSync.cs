#if ANDROID || IOS || MACCATALYST
using System;
using System.Diagnostics;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Platforms
{
    /// <summary>
    /// Renders the pending Avalonia frame of embedded views on demand.
    /// </summary>
    /// <remarks>
    /// Avalonia renders on its own thread, one or more display frames after the native layout that sizes an
    /// embedded view. The native frame that first shows the view therefore has no Avalonia content, and the
    /// content pops in later. A native host calls <see cref="RenderNow"/> from its layout to close that gap.
    /// </remarks>
    internal static class EmbeddedFrameSync
    {
        private static readonly TimeSpan s_timeout = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan s_pollInterval = TimeSpan.FromMilliseconds(2);

        /// <summary>
        /// Commits the pending composition changes and blocks until the render thread has rendered them.
        /// </summary>
        /// <param name="visual">A visual of the embedded view. All views that share its compositor are rendered.</param>
        /// <returns>
        /// True if the frame was rendered. False if the render thread did not render it in time, or if the
        /// dispatcher cannot run jobs now. The frame then arrives asynchronously as usual.
        /// </returns>
        public static bool RenderNow(Visual visual)
        {
            if (ElementComposition.GetElementVisual(visual)?.Compositor is not { } compositor)
                return false;

            var dispatcher = Dispatcher.UIThread;
            if (!dispatcher.CheckAccess())
                return false;

            var batch = compositor.RequestCompositionBatchCommitAsync();
            var started = Stopwatch.GetTimestamp();

            try
            {
                while (true)
                {
                    dispatcher.RunJobs(DispatcherPriority.Render);

                    if (batch.Rendered.Wait(s_pollInterval))
                        return true;

                    if (Stopwatch.GetElapsedTime(started) > s_timeout)
                        return false;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
#endif
