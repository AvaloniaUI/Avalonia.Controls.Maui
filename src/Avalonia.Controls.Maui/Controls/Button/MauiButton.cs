using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MauiContentLayout = Microsoft.Maui.Controls.Button.ButtonContentLayout;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Custom Avalonia Button with Image and Text support for MAUI's Button
/// </summary>
public class MauiButton : Button
{
    private StackPanel? _contentPanel;
    private TextBlock? _textBlock;
    private Image? _image;
    private MauiContentLayout _contentLayout = new(MauiContentLayout.ImagePosition.Left, 10);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MauiButton, string?>(nameof(Text));

    public static readonly StyledProperty<global::Avalonia.Media.IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<MauiButton, global::Avalonia.Media.IImage?>(nameof(ImageSource));

    public static readonly StyledProperty<double> CharacterSpacingProperty =
        AvaloniaProperty.Register<MauiButton, double>(nameof(CharacterSpacing));

    public static readonly StyledProperty<MauiContentLayout> ContentLayoutProperty =
        AvaloniaProperty.Register<MauiButton, MauiContentLayout>(nameof(ContentLayout), new MauiContentLayout(MauiContentLayout.ImagePosition.Left, 10));

    static MauiButton()
    {
        TextProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
        ImageSourceProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
        ContentLayoutProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.ApplyContentLayout(e.GetNewValue<MauiContentLayout>()));
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

    public global::Avalonia.Media.IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public double CharacterSpacing
    {
        get => GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    public MauiContentLayout ContentLayout
    {
        get => GetValue(ContentLayoutProperty);
        set => SetValue(ContentLayoutProperty, value);
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
            MaxWidth = 20,
            Margin = new Thickness(0)
        };

        Content = _contentPanel;
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
    }

    private void UpdateContent()
    {
        if (_contentPanel == null || _textBlock == null || _image == null)
            return;

        var hasImage = ImageSource != null;
        var hasText = !string.IsNullOrEmpty(Text);
        var spacing = hasImage && hasText ? _contentLayout.Spacing : 0;

        _contentPanel.Children.Clear();
        _contentPanel.Orientation = _contentLayout.Position is MauiContentLayout.ImagePosition.Top or MauiContentLayout.ImagePosition.Bottom
            ? Orientation.Vertical
            : Orientation.Horizontal;

        _image.Margin = new Thickness(0);
        _textBlock.Margin = new Thickness(0);

        if (hasImage)
        {
            _image.Source = ImageSource;
            _image.IsVisible = true;
        }
        else
        {
            _image.Source = null;
            _image.IsVisible = false;
        }

        if (hasText)
        {
            _textBlock.Text = Text;
            _textBlock.IsVisible = true;
        }
        else
        {
            _textBlock.IsVisible = false;
        }

        switch (_contentLayout.Position)
        {
            case MauiContentLayout.ImagePosition.Top:
                if (hasImage)
                {
                    _contentPanel.Children.Add(_image);
                }
                if (hasText)
                {
                    _textBlock.Margin = new Thickness(0, spacing, 0, 0);
                    _contentPanel.Children.Add(_textBlock);
                }
                break;

            case MauiContentLayout.ImagePosition.Bottom:
                if (hasText)
                {
                    _contentPanel.Children.Add(_textBlock);
                }
                if (hasImage)
                {
                    _image.Margin = new Thickness(0, spacing, 0, 0);
                    _contentPanel.Children.Add(_image);
                }
                break;

            case MauiContentLayout.ImagePosition.Right:
                if (hasText)
                {
                    _contentPanel.Children.Add(_textBlock);
                }
                if (hasImage)
                {
                    _image.Margin = new Thickness(spacing, 0, 0, 0);
                    _contentPanel.Children.Add(_image);
                }
                break;

            default:
                if (hasImage)
                {
                    _contentPanel.Children.Add(_image);
                }
                if (hasText)
                {
                    _textBlock.Margin = new Thickness(spacing, 0, 0, 0);
                    _contentPanel.Children.Add(_textBlock);
                }
                break;
        }
    }

    public void UpdateContentLayout(MauiContentLayout layout)
    {
        if (!ContentLayout.Equals(layout))
        {
            SetCurrentValue(ContentLayoutProperty, layout);
        }
        else
        {
            ApplyContentLayout(layout);
        }
    }

    private void ApplyContentLayout(MauiContentLayout layout)
    {
        _contentLayout = layout;
        UpdateContent();
    }

    public TextBlock? GetTextBlock() => _textBlock;

    public Image? GetImage() => _image;
}
