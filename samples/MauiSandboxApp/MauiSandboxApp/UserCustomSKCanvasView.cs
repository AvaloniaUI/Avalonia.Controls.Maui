using SkiaSharp.Views.Maui.Controls;

namespace MauiSandboxApp
{
    /// <summary>
    /// SKCanvasView subclass that overrides MeasureOverride to return (100, 100).
    /// This simulates the suggested fix: platform views should override MeasureOverride
    /// to return availableSize instead of (0, 0), so that non-Stretch alignment works.
    /// </summary>
    public class UserCustomSKCanvasView : SKCanvasView
    {
        protected override Microsoft.Maui.Graphics.Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            return new Microsoft.Maui.Graphics.Size(100, 100);
        }
    }
}
