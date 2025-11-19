using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

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
        // Hook for testing - can be overridden or monitored
    }

    public void Pressed()
    {
        // Hook for testing - can be overridden or monitored
    }

    public void Released()
    {
        // Hook for testing - can be overridden or monitored
    }

    public void UpdateImageSource(object? nativeImageSource)
    {
        // Hook for testing image source updates
    }

    public void UpdateIsLoading(bool isLoading)
    {
        // Hook for testing image loading state
    }

    IImageSource? IImageSourcePart.Source => ImageSource;

    bool IImageSourcePart.IsAnimationPlaying => false;
}