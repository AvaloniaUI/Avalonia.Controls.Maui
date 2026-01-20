using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Stubs
{
    public class EditorStub : StubBase, IEditor
    {
        public string Text { get; set; } = string.Empty;
        public Color TextColor { get; set; } = null!;
        public double CharacterSpacing { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsTextPredictionEnabled { get; set; }
        public bool IsSpellCheckEnabled { get; set; }
        public int MaxLength { get; set; } = int.MaxValue;
        public Keyboard Keyboard { get; set; } = Keyboard.Default;
        public string Placeholder { get; set; } = string.Empty;
        public Color PlaceholderColor { get; set; } = null!;
        public TextAlignment HorizontalTextAlignment { get; set; }
        public TextAlignment VerticalTextAlignment { get; set; }
        public int CursorPosition { get; set; }
        public int SelectionLength { get; set; }

        
        public Microsoft.Maui.Font Font { get; set; }
        
        // TextTransform property for testing
        public TextTransform TextTransform { get; set; } = TextTransform.None;
        
        // AutoSize property for testing
        public EditorAutoSizeOption AutoSize { get; set; } = EditorAutoSizeOption.Disabled;
        
        public void Completed() { }
    }
}
