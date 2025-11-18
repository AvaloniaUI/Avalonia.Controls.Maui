using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ButtonStub : StubBase, IButton, IText, ITextStyle, IPadding
{
    public string Text { get; set; } = string.Empty;

    public MauiGraphics.Color TextColor { get; set; } = null!;

    public double CharacterSpacing { get; set; }

    public Microsoft.Maui.Font Font { get; set; }

    public Microsoft.Maui.Thickness Padding { get; set; }

    public void Clicked()
    {
    }

    public void Pressed()
    {
    }

    public void Released()
    {
    }

    public TextDecorations TextDecorations { get; set; }

    public TextTransform TextTransform { get; set; } = TextTransform.Default;

    public MauiGraphics.Color StrokeColor { get; set; } = null!;

    public double StrokeThickness { get; set; }

    public int CornerRadius { get; set; }

    public IImageSource? ImageSource { get; set; }
}
