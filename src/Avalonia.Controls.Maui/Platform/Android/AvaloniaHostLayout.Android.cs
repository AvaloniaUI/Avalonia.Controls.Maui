using Android.Views;

namespace Avalonia.Controls.Maui.Platforms.Android.Handlers
{
    internal static class AvaloniaHostLayout
    {
        /// <summary>
        /// Measures the children of <paramref name="host"/> at its arranged size. MAUI arranges views
        /// without re-measuring them, so the Avalonia surface would otherwise keep its desired size.
        /// </summary>
        public static void MeasureChildrenExactly(ViewGroup host, int width, int height)
        {
            for (var i = 0; i < host.ChildCount; i++)
            {
                var child = host.GetChildAt(i);
                if (child is null || child.Visibility == ViewStates.Gone)
                    continue;

                if (child.MeasuredWidth == width && child.MeasuredHeight == height)
                    continue;

                child.Measure(
                    View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
                    View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
            }
        }
    }
}
