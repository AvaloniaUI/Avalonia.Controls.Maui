namespace ControlGallery.Pages;

public partial class ImagePage : ContentPage
{
    private readonly string[] _imageUrls =
    [
        "https://picsum.photos/400/300?random=10",
        "https://picsum.photos/400/300?random=11",
        "https://picsum.photos/400/300?random=12",
        "https://picsum.photos/400/300?random=13",
        "https://picsum.photos/400/300?random=14"
    ];

    private int _currentImageIndex;

    Image? InteractiveImageControl => FindByName("InteractiveImage") as Image;
    Label? AspectStatusLabelControl => FindByName("AspectStatusLabel") as Label;
    Picker? AspectPickerControl => FindByName("AspectPicker") as Picker;
    Image? OpacityImageControl => FindByName("OpacityImage") as Image;
    Label? OpacityLabelControl => FindByName("OpacityLabel") as Label;
    Image? DynamicImageControl => FindByName("DynamicImage") as Image;
    Label? UriStatusLabelControl => FindByName("UriStatusLabel") as Label;
    Image? GifImageControl => FindByName("GifImage") as Image;
    Label? GifStatusLabelControl => FindByName("GifStatusLabel") as Label;
    Image? StreamImageControl => FindByName("StreamImage") as Image;
    Label? StreamStatusLabelControl => FindByName("StreamStatusLabel") as Label;

    public ImagePage()
    {
        InitializeComponent();
        InitializeControls();
    }

    void InitializeControls()
    {
        // Set initial aspect picker selection
        if (AspectPickerControl != null)
        {
            AspectPickerControl.SelectedIndex = 0; // AspectFit
        }

        // Update URI status after a delay to simulate loading
        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), () =>
        {
            if (UriStatusLabelControl != null)
            {
                UriStatusLabelControl.Text = "Status: Loaded";
                UriStatusLabelControl.TextColor = Colors.Green;
            }
        });
    }

    void OnAspectChanged(object? sender, EventArgs e)
    {
        if (AspectPickerControl is null || InteractiveImageControl is null || AspectStatusLabelControl is null)
            return;

        var selectedAspect = AspectPickerControl.SelectedIndex switch
        {
            0 => Aspect.AspectFit,
            1 => Aspect.AspectFill,
            2 => Aspect.Fill,
            3 => Aspect.Center,
            _ => Aspect.AspectFit
        };

        InteractiveImageControl.Aspect = selectedAspect;
        AspectStatusLabelControl.Text = $"Current Aspect: {GetAspectName(selectedAspect)}";
    }

    string GetAspectName(Aspect aspect) => aspect switch
    {
        Aspect.AspectFit => "AspectFit",
        Aspect.AspectFill => "AspectFill",
        Aspect.Fill => "Fill",
        Aspect.Center => "Center",
        _ => "Unknown"
    };

    void OnOpacityChanged(object? sender, ValueChangedEventArgs e)
    {
        if (OpacityImageControl is null || OpacityLabelControl is null)
            return;

        var opacity = e.NewValue;
        OpacityImageControl.Opacity = opacity;
        OpacityLabelControl.Text = $"Opacity: {opacity:F2}";
    }

    void OnLoadRandomImage(object? sender, EventArgs e)
    {
        if (DynamicImageControl is null)
            return;

        // Simply set the source - IsLoading property will automatically update
        // and trigger the data bindings in XAML
        var imageUrl = _imageUrls[_currentImageIndex];
        _currentImageIndex = (_currentImageIndex + 1) % _imageUrls.Length;

        DynamicImageControl.Source = imageUrl;
    }

    void OnToggleGifAnimation(object? sender, EventArgs e)
    {
        if (GifImageControl is null || GifStatusLabelControl is null)
            return;

        GifImageControl.IsAnimationPlaying = !GifImageControl.IsAnimationPlaying;
        GifStatusLabelControl.Text = GifImageControl.IsAnimationPlaying ? "Status: Playing" : "Status: Paused";
    }
}
