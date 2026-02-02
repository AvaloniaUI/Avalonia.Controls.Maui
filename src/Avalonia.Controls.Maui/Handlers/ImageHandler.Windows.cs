using Microsoft.UI.Xaml.Media;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageHandler
{
    partial class ImageImageSourcePartSetter
    {
        /// <summary>
        /// Platform-specific stub implementation for Windows.
        /// Avalonia handles image rendering directly via MapSource/LoadSourceAsync.
        /// </summary>
        public override void SetImageSource(ImageSource? platformImage)
        {
        }
    }
}
