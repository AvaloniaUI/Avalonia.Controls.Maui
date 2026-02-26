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
    /// <summary>
    /// Gets the style key override, resolving to the base <see cref="Button"/> type.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Button);
    private DockPanel? _contentPanel;
    private TextBlock? _textBlock;
    private Image? _image;
    private MauiContentLayout _contentLayout = new(MauiContentLayout.ImagePosition.Left, 10);

    /// <summary>Defines the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MauiButton, string?>(nameof(Text));

    /// <summary>Defines the <see cref="ImageSource"/> property.</summary>
    public static readonly StyledProperty<global::Avalonia.Media.IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<MauiButton, global::Avalonia.Media.IImage?>(nameof(ImageSource));

    /// <summary>Defines the <see cref="CharacterSpacing"/> property.</summary>
    public static readonly StyledProperty<double> CharacterSpacingProperty =
        AvaloniaProperty.Register<MauiButton, double>(nameof(CharacterSpacing));

    /// <summary>Defines the <see cref="ContentLayout"/> property.</summary>
    public static readonly StyledProperty<MauiContentLayout> ContentLayoutProperty =
        AvaloniaProperty.Register<MauiButton, MauiContentLayout>(nameof(ContentLayout), new MauiContentLayout(MauiContentLayout.ImagePosition.Left, 10));

    /// <summary>Defines the <see cref="LineBreakMode"/> property.</summary>
    public static readonly StyledProperty<Microsoft.Maui.LineBreakMode> LineBreakModeProperty =
        AvaloniaProperty.Register<MauiButton, Microsoft.Maui.LineBreakMode>(nameof(LineBreakMode), Microsoft.Maui.LineBreakMode.NoWrap);

    static MauiButton()
    {
        TextProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
        ImageSourceProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
        ContentLayoutProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.ApplyContentLayout(e.GetNewValue<MauiContentLayout>()));
        LineBreakModeProperty.Changed.AddClassHandler<MauiButton>((button, e) => button.UpdateContent());
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MauiButton"/>.
    /// </summary>
    public MauiButton()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        InitializeContent();
    }

    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the image displayed on the button.
    /// </summary>
    public global::Avalonia.Media.IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the character spacing for the button text.
    /// </summary>
    public double CharacterSpacing
    {
        get => GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout of the button's image and text content.
    /// </summary>
    public MauiContentLayout ContentLayout
    {
        get => GetValue(ContentLayoutProperty);
        set => SetValue(ContentLayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the line break mode for the button text.
    /// </summary>
    public Microsoft.Maui.LineBreakMode LineBreakMode
    {
        get => GetValue(LineBreakModeProperty);
        set => SetValue(LineBreakModeProperty, value);
    }

    private void InitializeContent()
    {
        _contentPanel = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        _image = new Image
        {
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 20,
            MaxWidth = 20,
            Margin = new Thickness(0)
        };

        Content = _contentPanel;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
    }

    private void UpdateContent()
    {
        if (_contentPanel == null || _textBlock == null || _image == null)
            return;

        var hasImage = ImageSource != null;
        var hasText = !string.IsNullOrEmpty(Text);
        var spacing = hasImage && hasText ? _contentLayout.Spacing : 0;

        _contentPanel.Children.Clear();
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
            UpdateLineBreakMode();
        }
        else
        {
            _textBlock.IsVisible = false;
        }

        if (hasImage)
        {
            _contentPanel.HorizontalAlignment = HorizontalAlignment.Center;
            _contentPanel.VerticalAlignment = VerticalAlignment.Center;
        }
        else
        {
            _contentPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            _contentPanel.VerticalAlignment = VerticalAlignment.Stretch;
        }
        
        _contentPanel.LastChildFill = true;

        switch (_contentLayout.Position)
        {
            case MauiContentLayout.ImagePosition.Top:
                if (hasImage)
                {
                    DockPanel.SetDock(_image, Dock.Top);
                    _image.HorizontalAlignment = HorizontalAlignment.Center;
                    _contentPanel.Children.Add(_image);
                }
                if (hasText)
                {
                    _textBlock.Margin = new Thickness(0, spacing, 0, 0);
                    _contentPanel.Children.Add(_textBlock);
                    UpdateLineBreakMode();
                }
                break;

            case MauiContentLayout.ImagePosition.Bottom:
                if (hasImage)
                {
                    DockPanel.SetDock(_image, Dock.Bottom);
                    _image.HorizontalAlignment = HorizontalAlignment.Center;
                    _contentPanel.Children.Add(_image);
                }
                if (hasText)
                {
                    _textBlock.Margin = new Thickness(0, 0, 0, spacing);
                    _contentPanel.Children.Add(_textBlock);
                    UpdateLineBreakMode();
                }
                break;

            case MauiContentLayout.ImagePosition.Right:
                if (hasImage)
                {
                    DockPanel.SetDock(_image, Dock.Right);
                    _image.VerticalAlignment = VerticalAlignment.Center;
                    _contentPanel.Children.Add(_image);
                }
                if (hasText)
                {
                    _textBlock.Margin = new Thickness(0, 0, spacing, 0);
                    _contentPanel.Children.Add(_textBlock);
                    UpdateLineBreakMode();
                }
                break;

            default: // Left
                if (hasImage)
                {
                    DockPanel.SetDock(_image, Dock.Left);
                    _image.VerticalAlignment = VerticalAlignment.Center;
                    _contentPanel.Children.Add(_image);
                }
                if (hasText)
                {
                    _textBlock.Margin = new Thickness(spacing, 0, 0, 0);
                    _contentPanel.Children.Add(_textBlock);
                    UpdateLineBreakMode();
                }
                break;
        }
    }

    private void UpdateLineBreakMode()
    {
        if (_textBlock == null)
            return;

        switch (LineBreakMode)
        {
            case Microsoft.Maui.LineBreakMode.NoWrap:
                _textBlock.TextWrapping = TextWrapping.NoWrap;
                _textBlock.TextTrimming = TextTrimming.None;
                break;
            case Microsoft.Maui.LineBreakMode.WordWrap:
                _textBlock.TextWrapping = TextWrapping.Wrap;
                _textBlock.TextTrimming = TextTrimming.None;
                break;
            case Microsoft.Maui.LineBreakMode.CharacterWrap:
                _textBlock.TextWrapping = TextWrapping.Wrap;
                _textBlock.TextTrimming = TextTrimming.None;
                break;
            case Microsoft.Maui.LineBreakMode.HeadTruncation:
                _textBlock.TextWrapping = TextWrapping.NoWrap;
                _textBlock.TextTrimming = TextTrimming.PrefixCharacterEllipsis;
                break;
            case Microsoft.Maui.LineBreakMode.MiddleTruncation:
                // MiddleTruncation is not supported by Avalonia TextBlock yet.
                // Fallback to CharacterEllipsis (TailTruncation).
                _textBlock.TextWrapping = TextWrapping.NoWrap;
                _textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                break;
            case Microsoft.Maui.LineBreakMode.TailTruncation:
                _textBlock.TextWrapping = TextWrapping.NoWrap;
                _textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                break;
        }
    }

    /// <summary>
    /// Updates the content layout of the button, controlling image and text positioning.
    /// </summary>
    /// <param name="layout">The content layout specifying image position and spacing.</param>
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

    /// <summary>
    /// Gets the internal <see cref="TextBlock"/> used to display the button text.
    /// </summary>
    /// <returns>The text block, or <c>null</c> if not initialized.</returns>
    public TextBlock? GetTextBlock() => _textBlock;

    /// <summary>
    /// Gets the internal <see cref="Image"/> used to display the button image.
    /// </summary>
    /// <returns>The image control, or <c>null</c> if not initialized.</returns>
    public Image? GetImage() => _image;
}
