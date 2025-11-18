using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

public class MauiImageButton : Button
{
    private Image? _image;

    public static readonly StyledProperty<IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<MauiImageButton, IImage?>(nameof(ImageSource));

    static MauiImageButton()
    {
        ImageSourceProperty.Changed.AddClassHandler<MauiImageButton>((button, e) => button.UpdateImage());
    }

    public MauiImageButton()
    {
        InitializeContent();
    }

    public IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    private void InitializeContent()
    {
        _image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Content = _image;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
    }

    private void UpdateImage()
    {
        if (_image == null)
            return;

        _image.Source = ImageSource;
    }

    public Image? GetImage() => _image;
}
