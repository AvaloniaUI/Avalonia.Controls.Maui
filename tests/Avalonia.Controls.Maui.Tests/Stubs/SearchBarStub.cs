using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of ISearchBar for testing purposes.
/// </summary>
public class SearchBarStub : StubBase, ISearchBar
{
    public string Text { get; set; } = string.Empty;

    public string Placeholder { get; set; } = string.Empty;

    public MauiGraphics.Color PlaceholderColor { get; set; } = null!;

    public MauiGraphics.Color TextColor { get; set; } = null!;

    public double CharacterSpacing { get; set; }

    public TextAlignment HorizontalTextAlignment { get; set; }

    public TextAlignment VerticalTextAlignment { get; set; }

    public Microsoft.Maui.Font Font { get; set; }

    public bool IsTextPredictionEnabled { get; set; }

    public bool IsSpellCheckEnabled { get; set; }

    public bool IsReadOnly { get; set; }

    public int MaxLength { get; set; } = int.MaxValue;

    public int CursorPosition { get; set; }

    public int SelectionLength { get; set; }

    public Keyboard Keyboard { get; set; } = Keyboard.Default;

    public MauiGraphics.Color CancelButtonColor { get; set; } = null!;

    public MauiGraphics.Color SearchIconColor { get; set; } = null!;

    public ReturnType ReturnType { get; set; } = ReturnType.Search;

    public TextTransform TextTransform { get; set; }

    public string UpdatedText
    {
        get => Text;
        set => Text = value;
    }

    public void SearchButtonPressed()
    {
        // Simulate search button press event for testing
        SearchButtonPressedCount++;
    }

    /// <summary>
    /// Tracks how many times SearchButtonPressed was called.
    /// </summary>
    public int SearchButtonPressedCount { get; private set; }
}
