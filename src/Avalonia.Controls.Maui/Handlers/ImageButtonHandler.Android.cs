using Android.Graphics.Drawables;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageButtonHandler
{
    partial class ImageButtonImageSourcePartSetter
    {
        /// <summary>
        /// Platform-specific implementation for Android.
        /// This stub ignores the Drawable since Avalonia handles the actual image rendering.
        /// </summary>
        public override void SetImageSource(Drawable? platformImage)
        {
            // Avalonia handles image rendering, so we ignore the native image
            // The object-based overload is used for actual Avalonia image setting
        }
    }
}
