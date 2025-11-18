using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public partial class LabelStub : StubBase, ILabel
{
    public string Text { get; set; } = string.Empty;

    public TextType TextType { get; set; } = TextType.Text;

    public MauiGraphics.Color TextColor { get; set; } = null!;

    public double CharacterSpacing { get; set; }

    public Microsoft.Maui.Thickness Padding { get; set; }

    public Microsoft.Maui.Font Font { get; set; }

    public TextAlignment HorizontalTextAlignment { get; set; }

    public TextAlignment VerticalTextAlignment { get; set; }

    public TextDecorations TextDecorations { get; set; }

    public double LineHeight { get; set; } = -1;
}
