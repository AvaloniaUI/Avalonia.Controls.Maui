using System.Collections.Generic;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Primitives;
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

        /// <summary>
        /// Gets the hosts of the popups that are shown in the overlay with <paramref name="anchorLayer"/>.
        /// </summary>
        /// <remarks>
        /// This includes popups that are opened from a popup in the overlay, such as the drop-down of a ComboBox in
        /// a flyout. Their targets are in the overlay, so they are not redirected.
        /// </remarks>
        public static IEnumerable<OverlayPopupHost> GetPopupHosts(Visual anchorLayer)
        {
            if (anchorLayer.FindAncestorOfType<VisualLayerManager>() is not { } layers)
                yield break;

            foreach (var layer in layers.GetVisualChildren())
            {
                foreach (var child in layer.GetVisualChildren())
                {
                    if (child is OverlayPopupHost { IsVisible: true } host)
                        yield return host;
                }
            }
        }

        /// <summary>Gets the popup that is shown in <paramref name="host"/>.</summary>
        public static Popup? GetPopup(OverlayPopupHost host) => (host.Content as StyledElement)?.Parent as Popup;
    }
}
