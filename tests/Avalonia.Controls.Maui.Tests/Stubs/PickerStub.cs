using Microsoft.Maui;
using System.Collections.Generic;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class PickerStub : StubBase, IPicker
{
    public string Title { get; set; } = string.Empty;

    public MauiGraphics.Color TitleColor { get; set; } = null!;

    public int SelectedIndex { get; set; } = -1;

    public double CharacterSpacing { get; set; }

    public Microsoft.Maui.Font Font { get; set; }

    public MauiGraphics.Color TextColor { get; set; } = null!;

    public IList<string> Items { get; set; } = new List<string>();

    public TextAlignment HorizontalTextAlignment { get; set; }

    public TextAlignment VerticalTextAlignment { get; set; }

    public int GetCount() => Items?.Count ?? 0;

    public string GetItem(int index) => Items?[index] ?? string.Empty;
}
