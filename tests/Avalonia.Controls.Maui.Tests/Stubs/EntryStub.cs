using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Stubs
{
    public class EntryStub : StubBase, IEntry
    {
        public string Text { get; set; } = string.Empty;
        public Color TextColor { get; set; } = null!;
        public double CharacterSpacing { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsTextPredictionEnabled { get; set; } = true;
        public bool IsSpellCheckEnabled { get; set; } = true;
        public int MaxLength { get; set; } = int.MaxValue;
        public Keyboard Keyboard { get; set; } = Keyboard.Default;
        public string Placeholder { get; set; } = string.Empty;
        public Color PlaceholderColor { get; set; } = null!;
        public TextAlignment HorizontalTextAlignment { get; set; }
        public TextAlignment VerticalTextAlignment { get; set; }
        public int CursorPosition { get; set; }
        public int SelectionLength { get; set; }
        public bool IsPassword { get; set; }
        public ReturnType ReturnType { get; set; }
        public ClearButtonVisibility ClearButtonVisibility { get; set; }
        
        public Microsoft.Maui.Font Font { get; set; }
        
        public TextTransform TextTransform { get; set; } = TextTransform.None;

        public bool CompletedCalled { get; private set; }
        
        public void Completed() 
        {
            CompletedCalled = true;
        }
    }
}
