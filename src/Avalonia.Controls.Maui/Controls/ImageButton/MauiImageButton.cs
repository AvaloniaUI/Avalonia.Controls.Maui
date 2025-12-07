using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MauiAspect = Microsoft.Maui.Aspect;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Custom Avalonia Button with Image support for MAUI's ImageButton.
/// Displays an image and responds to a tap or click.
/// </summary>
public class MauiImageButton : Button
{
    private Image? _image;
    private MauiAspect _aspect = MauiAspect.AspectFit;

    public static readonly StyledProperty<IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<MauiImageButton, IImage?>(nameof(ImageSource));

    public static readonly StyledProperty<MauiAspect> AspectProperty =
        AvaloniaProperty.Register<MauiImageButton, MauiAspect>(nameof(Aspect), MauiAspect.AspectFit);

    static MauiImageButton()
    {
        ImageSourceProperty.Changed.AddClassHandler<MauiImageButton>((button, e) => button.UpdateImage());
        AspectProperty.Changed.AddClassHandler<MauiImageButton>((button, e) => button.ApplyAspect(e.GetNewValue<MauiAspect>()));
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

    public MauiAspect Aspect
    {
        get => GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
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

    public void UpdateAspect(MauiAspect aspect)
    {
        if (!Aspect.Equals(aspect))
        {
            SetCurrentValue(AspectProperty, aspect);
        }
        else
        {
            ApplyAspect(aspect);
        }
    }

    private void ApplyAspect(MauiAspect aspect)
    {
        _aspect = aspect;

        if (_image == null)
            return;

        _image.Stretch = aspect switch
        {
            MauiAspect.AspectFill => Stretch.UniformToFill,
            MauiAspect.Fill => Stretch.Fill,
            MauiAspect.Center => Stretch.None,
            _ => Stretch.Uniform // AspectFit
        };
    }

    public Image? GetImage() => _image;
}
