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

    public ImagePage()
    {
        InitializeComponent();
        InitializeControls();
    }

    void InitializeControls()
    {
        // Update URI status after a delay to simulate loading
        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), () =>
        {
            if (UriStatusLabel != null)
            {
                UriStatusLabel.Text = "Status: Loaded";
                UriStatusLabel.TextColor = Colors.Green;
            }
        });
    }

    void OnAspectButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (InteractiveImage is null || AspectStatusLabel is null)
            return;

        var aspectName = button.CommandParameter as string;
        var selectedAspect = aspectName switch
        {
            "AspectFit" => Aspect.AspectFit,
            "AspectFill" => Aspect.AspectFill,
            "Fill" => Aspect.Fill,
            "Center" => Aspect.Center,
            _ => Aspect.AspectFit
        };

        InteractiveImage.Aspect = selectedAspect;
        AspectStatusLabel.Text = $"Current Aspect: {aspectName}";
    }

    void OnOpacityChanged(object? sender, ValueChangedEventArgs e)
    {
        if (OpacityImage is null || OpacityLabel is null)
            return;

        var opacity = e.NewValue;
        OpacityImage.Opacity = opacity;
        OpacityLabel.Text = $"Opacity: {opacity:F2}";
    }

    void OnLoadRandomImage(object? sender, EventArgs e)
    {
        if (DynamicImage is null)
            return;

        // Simply set the source - IsLoading property will automatically update
        // and trigger the data bindings in XAML
        var imageUrl = _imageUrls[_currentImageIndex];
        _currentImageIndex = (_currentImageIndex + 1) % _imageUrls.Length;

        DynamicImage.Source = imageUrl;
    }

    void OnToggleGifAnimation(object? sender, EventArgs e)
    {
        if (GifImage is null || GifStatusLabel is null)
            return;

        GifImage.IsAnimationPlaying = !GifImage.IsAnimationPlaying;
        GifStatusLabel.Text = GifImage.IsAnimationPlaying ? "Status: Playing" : "Status: Paused";
    }
}
