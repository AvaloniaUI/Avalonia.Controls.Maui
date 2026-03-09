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
    /// <summary>
    /// Gets the style key override, resolving to the base <see cref="Button"/> type.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Button);
    private Image? _image;
    private MauiAspect _aspect = MauiAspect.AspectFit;

    /// <summary>Defines the <see cref="ImageSource"/> property.</summary>
    public static readonly StyledProperty<IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<MauiImageButton, IImage?>(nameof(ImageSource));

    /// <summary>Defines the <see cref="Aspect"/> property.</summary>
    public static readonly StyledProperty<MauiAspect> AspectProperty =
        AvaloniaProperty.Register<MauiImageButton, MauiAspect>(nameof(Aspect), MauiAspect.AspectFit);

    static MauiImageButton()
    {
        ImageSourceProperty.Changed.AddClassHandler<MauiImageButton>((button, e) => button.UpdateImage());
        AspectProperty.Changed.AddClassHandler<MauiImageButton>((button, e) => button.ApplyAspect(e.GetNewValue<MauiAspect>()));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MauiImageButton"/>.
    /// </summary>
    public MauiImageButton()
    {
        InitializeContent();
    }

    /// <summary>
    /// Gets or sets the image displayed on the button.
    /// </summary>
    public IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the aspect ratio mode for the image.
    /// </summary>
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

    /// <summary>
    /// Updates the aspect ratio mode used to display the image.
    /// </summary>
    /// <param name="aspect">The aspect ratio mode to apply.</param>
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

    /// <summary>
    /// Gets the internal <see cref="Image"/> used to display the button image.
    /// </summary>
    /// <returns>The image control, or <c>null</c> if not initialized.</returns>
    public Image? GetImage() => _image;
}
