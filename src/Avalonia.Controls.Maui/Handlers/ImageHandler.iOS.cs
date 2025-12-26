using Microsoft.Maui.Platform;
using UIKit;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageHandler
{
    partial class ImageImageSourcePartSetter
    {
        /// <summary>
        /// Platform-specific implementation for iOS/MacCatalyst.
        /// This stub ignores the UIImage since Avalonia handles the actual image rendering.
        /// </summary>
        public override void SetImageSource(UIImage? platformImage)
        {
            // Avalonia handles image rendering, so we ignore the native image
            // The object-based overload is used for actual Avalonia image setting
        }
    }
}
