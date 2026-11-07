using Avalonia.Controls.Embedding;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui.Platforms.EmbeddedPopups
{
    /// <summary>
    /// Helpers for the Avalonia root of a popup overlay.
    /// </summary>
    internal static class EmbeddedPopupOverlayRoot
    {
        /// <summary>Removes the backgrounds of <paramref name="root"/>, so that only the popups are visible.</summary>
        public static void MakeTransparent(EmbeddableControlRoot root)
        {
            root.Background = null;
            root.TransparencyBackgroundFallback = Avalonia.Media.Brushes.Transparent;
            root.ApplyTemplate();

            // The fallback border keeps the brush it got when the template was applied.
            foreach (var child in root.GetVisualDescendants())
            {
                if (child is Border { Name: "PART_TransparencyFallback" } fallback)
                    fallback.Background = null;
            }
        }
    }
}
