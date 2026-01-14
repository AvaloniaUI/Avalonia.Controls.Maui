using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Tests.Stubs;

namespace Avalonia.Controls.Maui.Tests.Handlers
{
    public class EditorHandlerTests : HandlerTestBase<EditorHandler, EditorStub>
    {
        [AvaloniaFact(DisplayName = "Text Mapped Correctly")]
        public async Task TextMapped()
        {
            var editor = new EditorStub { Text = "Initial Text" };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal("Initial Text", platformView.Text);
            
            editor.Text = "Updated Text";
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.Text)));
            
            Assert.Equal("Updated Text", platformView.Text);
        }

        [AvaloniaFact(DisplayName = "Empty Text Mapped Correctly")]
        public async Task EmptyTextMapped()
        {
            var editor = new EditorStub { Text = "" };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal("", platformView.Text);
        }

        [AvaloniaFact(DisplayName = "Null Text Mapped As Empty")]
        public async Task NullTextMappedAsEmpty()
        {
            var editor = new EditorStub { Text = null! };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.True(string.IsNullOrEmpty(platformView.Text));
        }

        [AvaloniaFact(DisplayName = "Placeholder Mapped Correctly")]
        public async Task PlaceholderMapped()
        {
            var editor = new EditorStub { Placeholder = "Enter text..." };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal("Enter text...", platformView.Watermark);
            
            editor.Placeholder = "New Placeholder";
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.Placeholder)));
            Assert.Equal("New Placeholder", platformView.Watermark);
        }

        [AvaloniaFact(DisplayName = "Empty Placeholder Mapped Correctly")]
        public async Task EmptyPlaceholderMapped()
        {
            var editor = new EditorStub { Placeholder = "" };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal("", platformView.Watermark);
        }

        [AvaloniaTheory(DisplayName = "Propagates Text Changes from Platform")]
        [InlineData("User Input")]
        [InlineData("")]
        [InlineData("Multi\nLine\nText")]
        [InlineData("Special chars: @#$%^&*()")]
        [InlineData("Very long text that might be truncated in some scenarios but should work fine in a multi-line editor control")]
        public async Task PropagatesTextChanges(string text)
        {
            var editor = new EditorStub();
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            platformView.Text = text;
            await Task.Yield(); // Allow event to propagate
            
            // TextChanged event propagates update
            Assert.Equal(text, editor.Text);
        }

        [AvaloniaFact(DisplayName = "TextColor Mapped Correctly")]
        public async Task TextColorMapped()
        {
            var editor = new EditorStub { TextColor = Microsoft.Maui.Graphics.Colors.Red };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var brush = platformView.Foreground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Colors.Red, brush.Color);
        }

        [AvaloniaTheory(DisplayName = "TextColor Variations Mapped Correctly")]
        [InlineData(255, 0, 0)]     // Red
        [InlineData(0, 255, 0)]     // Green
        [InlineData(0, 0, 255)]     // Blue
        [InlineData(0, 0, 0)]       // Black
        [InlineData(255, 255, 255)] // White
        [InlineData(128, 128, 128)] // Gray
        public async Task TextColorVariationsMapped(byte r, byte g, byte b)
        {
            var mauiColor = Microsoft.Maui.Graphics.Color.FromRgb(r, g, b);
            var editor = new EditorStub { TextColor = mauiColor };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var brush = platformView.Foreground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(r, brush.Color.R);
            Assert.Equal(g, brush.Color.G);
            Assert.Equal(b, brush.Color.B);
        }
        
        [AvaloniaFact(DisplayName = "PlaceholderColor Mapped Correctly")]
        public async Task PlaceholderColorMapped()
        {
            var editor = new EditorStub { PlaceholderColor = Microsoft.Maui.Graphics.Colors.Blue };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var brush = platformView.PlaceholderForeground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Colors.Blue, brush.Color);
        }

        [AvaloniaTheory(DisplayName = "MaxLength Variations Mapped Correctly")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        public async Task MaxLengthVariationsMapped(int maxLength)
        {
            var editor = new EditorStub { MaxLength = maxLength };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal(maxLength, platformView.MaxLength);
        }
        
        [AvaloniaFact(DisplayName = "IsReadOnly Mapped Correctly")]
        public async Task IsReadOnlyMapped()
        {
             var editor = new EditorStub { IsReadOnly = true };
             var handler = await CreateHandlerAsync(editor);
             var platformView = (MauiEditor)handler.PlatformView!;
             
             Assert.True(platformView.IsReadOnly);
             
             editor.IsReadOnly = false;
             await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.IsReadOnly)));
             Assert.False(platformView.IsReadOnly);
        }

        [AvaloniaFact(DisplayName = "IsReadOnly Toggle Works")]
        public async Task IsReadOnlyToggleWorks()
        {
            var editor = new EditorStub { IsReadOnly = false };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.False(platformView.IsReadOnly);
            
            editor.IsReadOnly = true;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.IsReadOnly)));
            Assert.True(platformView.IsReadOnly);
            
            editor.IsReadOnly = false;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.IsReadOnly)));
            Assert.False(platformView.IsReadOnly);
        }

        [AvaloniaFact(DisplayName = "CursorPosition Updates VirtualView From PlatformView")]
        public async Task CursorPositionUpdatesVirtualViewFromPlatformView()
        {
            var editor = new EditorStub
            {
                Text = "Test Content",
                CursorPosition = 0
            };

            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            // Simulate platform selection change
            platformView.SelectionStart = 5;
            platformView.SelectionEnd = 5; // Cursor position only
            
            // Trigger event manually as test environment might not do it automatically
            platformView.RaiseSelectionChanged();

            Assert.Equal(5, editor.CursorPosition);
        }

        [AvaloniaFact(DisplayName = "SelectionLength Updates VirtualView From PlatformView")]
        public async Task SelectionLengthUpdatesVirtualViewFromPlatformView()
        {
            var editor = new EditorStub
            {
                Text = "Test Content",
                SelectionLength = 0
            };

            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            // Simulate platform selection change
            platformView.SelectionStart = 0;
            platformView.SelectionEnd = 4; // Select "Test"
            
            // Trigger event manually
            platformView.RaiseSelectionChanged();

            Assert.Equal(4, editor.SelectionLength);
            Assert.Equal(0, editor.CursorPosition);
        }

        [AvaloniaTheory(DisplayName = "CharacterSpacing Mapped Correctly")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task CharacterSpacingMapped(double spacing)
        {
            var editor = new EditorStub { CharacterSpacing = spacing };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            // Avalonia uses LetterSpacing
            Assert.Equal(spacing, platformView.LetterSpacing);
        }

        [AvaloniaTheory(DisplayName = "CursorPosition Mapped Correctly")]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task CursorPositionMapped(int position)
        {
            var editor = new EditorStub 
            { 
                Text = "Hello World Test",
                CursorPosition = position 
            };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal(position, platformView.CaretIndex);
        }

        [AvaloniaTheory(DisplayName = "SelectionLength Mapped Correctly")]
        [InlineData(0, 0)]
        [InlineData(0, 5)]
        [InlineData(5, 5)]
        public async Task SelectionLengthMapped(int cursorPos, int selectionLength)
        {
            var editor = new EditorStub 
            { 
                Text = "Hello World Test",
                CursorPosition = cursorPos,
                SelectionLength = selectionLength
            };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var actualLength = platformView.SelectionEnd - platformView.SelectionStart;
            if (selectionLength != actualLength)
            {
                System.Diagnostics.Debug.WriteLine($"Test Failed. Expected: {selectionLength}, Actual: {actualLength}. Text: '{platformView.Text}', SelStart: {platformView.SelectionStart}, SelEnd: {platformView.SelectionEnd}, CaretIndex: {platformView.CaretIndex}");
            }
            Assert.Equal(selectionLength, actualLength);
        }

        [AvaloniaTheory(DisplayName = "HorizontalTextAlignment Mapped Correctly")]
        [InlineData(Microsoft.Maui.TextAlignment.Start)]
        [InlineData(Microsoft.Maui.TextAlignment.Center)]
        [InlineData(Microsoft.Maui.TextAlignment.End)]
        public async Task HorizontalTextAlignmentMapped(Microsoft.Maui.TextAlignment alignment)
        {
            var editor = new EditorStub { HorizontalTextAlignment = alignment };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var expectedAlignment = alignment switch
            {
                Microsoft.Maui.TextAlignment.Start => Avalonia.Media.TextAlignment.Left,
                Microsoft.Maui.TextAlignment.Center => Avalonia.Media.TextAlignment.Center,
                Microsoft.Maui.TextAlignment.End => Avalonia.Media.TextAlignment.Right,
                _ => Avalonia.Media.TextAlignment.Left
            };
            
            Assert.Equal(expectedAlignment, platformView.TextAlignment);
        }

        [AvaloniaTheory(DisplayName = "VerticalTextAlignment Mapped Correctly")]
        [InlineData(Microsoft.Maui.TextAlignment.Start)]
        [InlineData(Microsoft.Maui.TextAlignment.Center)]
        [InlineData(Microsoft.Maui.TextAlignment.End)]
        public async Task VerticalTextAlignmentMapped(Microsoft.Maui.TextAlignment alignment)
        {
            var editor = new EditorStub { VerticalTextAlignment = alignment };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var expectedAlignment = alignment switch
            {
                Microsoft.Maui.TextAlignment.Start => Avalonia.Layout.VerticalAlignment.Top,
                Microsoft.Maui.TextAlignment.Center => Avalonia.Layout.VerticalAlignment.Center,
                Microsoft.Maui.TextAlignment.End => Avalonia.Layout.VerticalAlignment.Bottom,
                _ => Avalonia.Layout.VerticalAlignment.Top
            };
            
            Assert.Equal(expectedAlignment, platformView.VerticalContentAlignment);
        }

        [AvaloniaTheory(DisplayName = "FontSize Mapped Correctly")]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(14)]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(36)]
        public async Task FontSizeMapped(double fontSize)
        {
            var editor = new EditorStub { Font = Microsoft.Maui.Font.OfSize("Default", fontSize) };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.Equal(fontSize, platformView.FontSize);
        }

        [AvaloniaFact(DisplayName = "Handler Creates Platform View")]
        public async Task HandlerCreatesPlatformView()
        {
            var editor = new EditorStub();
            var handler = await CreateHandlerAsync(editor);
            
            Assert.NotNull(handler.PlatformView);
            Assert.IsType<MauiEditor>(handler.PlatformView);
        }

        [AvaloniaFact(DisplayName = "Default Values Are Set Correctly")]
        public async Task DefaultValuesAreSetCorrectly()
        {
            var editor = new EditorStub();
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            // AcceptsReturn should be true for multi-line editor
            Assert.True(platformView.AcceptsReturn);
            // TextWrapping should be Wrap
            Assert.Equal(TextWrapping.Wrap, platformView.TextWrapping);
        }

        [AvaloniaFact(DisplayName = "Multiple Text Updates Work")]
        public async Task MultipleTextUpdatesWork()
        {
            var editor = new EditorStub { Text = "Initial" };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            for (int i = 0; i < 10; i++)
            {
                editor.Text = $"Update {i}";
                await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.Text)));
                Assert.Equal($"Update {i}", platformView.Text);
            }
        }

        [AvaloniaTheory(DisplayName = "Keyboard ContentType Mapped Correctly")]
        [InlineData("Email")]
        [InlineData("Numeric")]
        [InlineData("Telephone")]
        [InlineData("Url")]
        [InlineData("Text")]
        [InlineData("Plain")]
        [InlineData("Chat")]
        public async Task KeyboardContentTypeMapped(string keyboardTypeName)
        {
            var keyboard = keyboardTypeName switch
            {
                "Email" => Keyboard.Email,
                "Numeric" => Keyboard.Numeric,
                "Telephone" => Keyboard.Telephone,
                "Url" => Keyboard.Url,
                "Text" => Keyboard.Text,
                "Plain" => Keyboard.Plain,
                "Chat" => Keyboard.Chat,
                _ => Keyboard.Default
            };
            
            var editor = new EditorStub { Keyboard = keyboard };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            var expectedContentType = keyboardTypeName switch
            {
                "Email" => Avalonia.Input.TextInput.TextInputContentType.Email,
                "Numeric" => Avalonia.Input.TextInput.TextInputContentType.Number,
                "Telephone" => Avalonia.Input.TextInput.TextInputContentType.Digits,
                "Url" => Avalonia.Input.TextInput.TextInputContentType.Url,
                _ => Avalonia.Input.TextInput.TextInputContentType.Normal
            };
            
            Assert.Equal(expectedContentType, Avalonia.Input.TextInput.TextInputOptions.GetContentType(platformView));
        }

        [AvaloniaFact(DisplayName = "Keyboard AutoCapitalization Disabled for Plain")]
        public async Task KeyboardAutoCapitalizationDisabledForPlain()
        {
            var editor = new EditorStub { Keyboard = Keyboard.Plain };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.False(Avalonia.Input.TextInput.TextInputOptions.GetAutoCapitalization(platformView));
        }

        [AvaloniaFact(DisplayName = "Keyboard ShowSuggestions Enabled for Chat")]
        public async Task KeyboardShowSuggestionsEnabledForChat()
        {
            var editor = new EditorStub { Keyboard = Keyboard.Chat };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            Assert.True(Avalonia.Input.TextInput.TextInputOptions.GetShowSuggestions(platformView));
        }

        [AvaloniaFact(DisplayName = "Editor Sets Multiline TextInputOption")]
        public async Task EditorSetsMultilineTextInputOption()
        {
            var editor = new EditorStub();
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;
            
            // Editor should always set multiline option
            Assert.True(Avalonia.Input.TextInput.TextInputOptions.GetMultiline(platformView));
        }
        [AvaloniaFact(DisplayName = "IsTextPredictionEnabled Mapped Correctly")]
        public async Task IsTextPredictionEnabledMappedCorrectly()
        {
            var editor = new EditorStub { IsTextPredictionEnabled = false };
            var handler = await CreateHandlerAsync(editor);
            var platformView = (MauiEditor)handler.PlatformView!;

            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.IsTextPredictionEnabled)));
            
            // Should be false when IsTextPredictionEnabled is false
            Assert.False(Avalonia.Input.TextInput.TextInputOptions.GetShowSuggestions(platformView));
            
            editor.IsTextPredictionEnabled = true;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEditor.IsTextPredictionEnabled)));
            
            // Should be true when IsTextPredictionEnabled is true
            Assert.True(Avalonia.Input.TextInput.TextInputOptions.GetShowSuggestions(platformView));
        }

    }
}
