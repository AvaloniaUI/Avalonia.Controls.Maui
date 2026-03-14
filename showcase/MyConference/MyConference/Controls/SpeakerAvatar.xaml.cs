namespace MyConference.Controls;

public partial class SpeakerAvatar : ContentView
{
    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(SpeakerAvatar));

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(
            nameof(Size),
            typeof(double),
            typeof(SpeakerAvatar),
            40.0,
            propertyChanged: OnSizeChanged);

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public SpeakerAvatar()
    {
        InitializeComponent();
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not SpeakerAvatar avatar || avatar.Content is not Border border)
            return;

        var size = (double)newValue;
        border.WidthRequest = size;
        border.HeightRequest = size;
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        {
            CornerRadius = new CornerRadius(size / 2)
        };
    }
}
