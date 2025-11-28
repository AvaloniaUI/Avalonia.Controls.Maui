using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Custom Avalonia Button with Image and Text support for MAUI's Button
/// </summary>
public class MauiButton : Button
{
    private StackPanel? _contentPanel;
    private TextBlock? _textBlock;
    private Image? _image;

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MauiButton, string?>(nameof(Text));

    public static readonly StyledProperty<IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<MauiButton, IImage?>(nameof(ImageSource));

    public static readonly StyledProperty<double> CharacterSpacingProperty =
        AvaloniaProperty.Register<MauiButton, double>(nameof(CharacterSpacing));

    static MauiButton()
    {
        TextProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
        ImageSourceProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
    }

    public MauiButton()
    {
        InitializeContent();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public double CharacterSpacing
    {
        get => GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    private void InitializeContent()
    {
        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        _image = new Image
        {
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 20,
            MaxWidth = 20
        };

        Content = _contentPanel;
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
    }

    private void UpdateContent()
    {
        if (_contentPanel == null || _textBlock == null || _image == null)
            return;

        _contentPanel.Children.Clear();

        bool hasImage = ImageSource != null;
        bool hasText = !string.IsNullOrEmpty(Text);

        if (hasImage)
        {
            _image.Source = ImageSource;
            // Add spacing only when both image and text are present
            _image.Margin = hasText ? new Thickness(0, 0, 5, 0) : new Thickness(0);
            _contentPanel.Children.Add(_image);
        }

        if (hasText)
        {
            _textBlock.Text = Text;
            _contentPanel.Children.Add(_textBlock);
        }
    }

    public TextBlock? GetTextBlock() => _textBlock;

    public Image? GetImage() => _image;
}