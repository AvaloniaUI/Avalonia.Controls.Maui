using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;
using MButton = Microsoft.Maui.Controls.Button;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of IButton for testing ButtonHandler.
/// </summary>
public class ButtonStub : StubBase, IButton, IText, ITextStyle, IPadding, IButtonStroke, IImageSourcePart
{
    private string _text = string.Empty;
    private MauiGraphics.Color _textColor = MauiGraphics.Colors.Black;
    private double _characterSpacing;
    private Font _font = Font.Default;
    private Microsoft.Maui.Thickness _padding = Microsoft.Maui.Thickness.Zero;
    private MauiGraphics.Color _strokeColor = MauiGraphics.Colors.Transparent;
    private double _strokeThickness;
    private int _cornerRadius;
    private IImageSource? _imageSource;
    private MauiGraphics.Paint? _background;
    private MButton.ButtonContentLayout _contentLayout = new(MButton.ButtonContentLayout.ImagePosition.Left, 10);
    private Microsoft.Maui.LineBreakMode _lineBreakMode = Microsoft.Maui.LineBreakMode.NoWrap;

    // Event counters for testing
    public int ClickedCount { get; private set; }
    public int PressedCount { get; private set; }
    public int ReleasedCount { get; private set; }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public MauiGraphics.Color TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    public double CharacterSpacing
    {
        get => _characterSpacing;
        set => SetProperty(ref _characterSpacing, value);
    }

    public Font Font
    {
        get => _font;
        set => SetProperty(ref _font, value);
    }

    public Microsoft.Maui.Thickness Padding
    {
        get => _padding;
        set => SetProperty(ref _padding, value);
    }

    public MauiGraphics.Color StrokeColor
    {
        get => _strokeColor;
        set => SetProperty(ref _strokeColor, value);
    }

    public double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, value);
    }

    public int CornerRadius
    {
        get => _cornerRadius;
        set => SetProperty(ref _cornerRadius, value);
    }

    public IImageSource? ImageSource
    {
        get => _imageSource;
        set => SetProperty(ref _imageSource, value);
    }

    public new MauiGraphics.Paint? Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }

    public TextDecorations TextDecorations { get; set; } = TextDecorations.None;

    public TextTransform TextTransform { get; set; } = TextTransform.Default;

    public void Clicked()
    {
        ClickedCount++;
    }

    public void Pressed()
    {
        PressedCount++;
    }

    public void Released()
    {
        ReleasedCount++;
    }

    public void UpdateImageSource(object? nativeImageSource)
    {
        // Hook for testing image source updates
    }

    public void UpdateIsLoading(bool isLoading)
    {
        // Hook for testing image loading state
    }

    public MButton.ButtonContentLayout ContentLayout
    {
        get => _contentLayout;
        set => SetProperty(ref _contentLayout, value);
    }

    public Microsoft.Maui.LineBreakMode LineBreakMode
    {
        get => _lineBreakMode;
        set => SetProperty(ref _lineBreakMode, value);
    }

    IImageSource? IImageSourcePart.Source => ImageSource;

    bool IImageSourcePart.IsAnimationPlaying => false;
}
