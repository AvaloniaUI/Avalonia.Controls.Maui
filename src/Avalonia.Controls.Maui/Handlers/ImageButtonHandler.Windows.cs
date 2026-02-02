using Microsoft.UI.Xaml.Media;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageButtonHandler
{
    partial class ImageButtonImageSourcePartSetter
    {
        /// <summary>
        /// Platform-specific stub implementation for Windows.
        /// Avalonia handles image rendering directly via MapImageSource/LoadImageSourceAsync.
        /// </summary>
        public override void SetImageSource(ImageSource? platformImage)
        {
        }
    }
}
