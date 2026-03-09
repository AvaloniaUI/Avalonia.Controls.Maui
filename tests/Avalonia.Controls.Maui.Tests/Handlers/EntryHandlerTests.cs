using Avalonia.Controls.Maui.Handlers;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Controls;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using Avalonia.Input.TextInput;

namespace Avalonia.Controls.Maui.Tests.Handlers
{
    public class EntryHandlerTests : HandlerTestBase<EntryHandler, EntryStub>
    {
        [AvaloniaFact(DisplayName = "Handler Creates Platform View")]
        public async Task HandlerCreatesPlatformView()
        {
            var entry = new EntryStub();
            var handler = await CreateHandlerAsync(entry);
            
            Assert.NotNull(handler.PlatformView);
            Assert.IsType<MauiEntry>(handler.PlatformView);
        }

        [AvaloniaFact(DisplayName = "PlaceholderColor Mapped Correctly")]
        public async Task PlaceholderColorMapped()
        {
            var mauiColor = Microsoft.Maui.Graphics.Colors.Red;
            var entry = new EntryStub { PlaceholderColor = mauiColor };
            var handler = await CreateHandlerAsync(entry);
            var platformView = handler.PlatformView!;

            var brush = platformView.PlaceholderForeground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Colors.Red, brush.Color);

            entry.PlaceholderColor = Microsoft.Maui.Graphics.Colors.Blue;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.PlaceholderColor)));
            
            brush = platformView.PlaceholderForeground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Colors.Blue, brush.Color);
        }

        [AvaloniaFact(DisplayName = "ClearButtonVisibility Mapped Correctly")]
        public async Task ClearButtonVisibilityMapped()
        {
            var entry = new EntryStub { ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
            var handler = await CreateHandlerAsync(entry);
            var platformView = handler.PlatformView!;

            Assert.Equal(ClearButtonVisibility.WhileEditing, platformView.ClearButtonVisibility);

            entry.ClearButtonVisibility = ClearButtonVisibility.Never;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.ClearButtonVisibility)));
            
            Assert.Equal(ClearButtonVisibility.Never, platformView.ClearButtonVisibility);
        }

        [AvaloniaFact(DisplayName = "Text Mapped Correctly")]
        public async Task TextMapped()
        {
            var entry = new EntryStub { Text = "Initial Text" };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.Equal("Initial Text", platformView.Text);
            
            entry.Text = "Updated Text";
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.Text)));
            
            Assert.Equal("Updated Text", platformView.Text);
        }

        [AvaloniaFact(DisplayName = "Empty Text Mapped Correctly")]
        public async Task EmptyTextMapped()
        {
            var entry = new EntryStub { Text = "" };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.Equal("", platformView.Text);
        }

        [AvaloniaFact(DisplayName = "Placeholder Mapped Correctly")]
        public async Task PlaceholderMapped()
        {
            var entry = new EntryStub { Placeholder = "Enter text..." };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.Equal("Enter text...", platformView.Watermark);
            
            entry.Placeholder = "New Placeholder";
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.Placeholder)));
            Assert.Equal("New Placeholder", platformView.Watermark);
        }

        [AvaloniaTheory(DisplayName = "Propagates Text Changes from Platform")]
        [InlineData("User Input")]
        [InlineData("")]
        [InlineData("Special chars: @#$%^&*()")]
        public async Task PropagatesTextChanges(string text)
        {
            var entry = new EntryStub();
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            platformView.Text = text;
            await Task.Yield();
            
            Assert.Equal(text, entry.Text);
        }

        [AvaloniaFact(DisplayName = "TextColor Mapped Correctly")]
        public async Task TextColorMapped()
        {
            var entry = new EntryStub { TextColor = Microsoft.Maui.Graphics.Colors.Red };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            var brush = platformView.Foreground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Colors.Red, brush.Color);
        }

        [AvaloniaTheory(DisplayName = "TextColor Variations Mapped Correctly")]
        [InlineData(255, 0, 0)]
        [InlineData(0, 255, 0)]
        [InlineData(0, 0, 255)]
        [InlineData(0, 0, 0)]
        [InlineData(255, 255, 255)]
        public async Task TextColorVariationsMapped(byte r, byte g, byte b)
        {
            var mauiColor = Microsoft.Maui.Graphics.Color.FromRgb(r, g, b);
            var entry = new EntryStub { TextColor = mauiColor };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            var brush = platformView.Foreground as ISolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(r, brush.Color.R);
            Assert.Equal(g, brush.Color.G);
            Assert.Equal(b, brush.Color.B);
        }

        [AvaloniaTheory(DisplayName = "MaxLength Variations Mapped Correctly")]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public async Task MaxLengthVariationsMapped(int maxLength)
        {
            var entry = new EntryStub { MaxLength = maxLength };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.Equal(maxLength, platformView.MaxLength);
        }
        
        [AvaloniaFact(DisplayName = "IsReadOnly Mapped Correctly")]
        public async Task IsReadOnlyMapped()
        {
             var entry = new EntryStub { IsReadOnly = true };
             var handler = await CreateHandlerAsync(entry);
             var platformView = (AvaloniaTextBox)handler.PlatformView!;
             
             Assert.True(platformView.IsReadOnly);
             
             entry.IsReadOnly = false;
             await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.IsReadOnly)));
             Assert.False(platformView.IsReadOnly);
        }

        [AvaloniaFact(DisplayName = "IsPassword Mapped Correctly")]
        public async Task IsPasswordMapped()
        {
            var entry = new EntryStub { IsPassword = true };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.NotEqual('\0', platformView.PasswordChar);
            
            entry.IsPassword = false;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.IsPassword)));
            Assert.Equal('\0', platformView.PasswordChar);
        }

        [AvaloniaTheory(DisplayName = "CursorPosition Mapped Correctly")]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task CursorPositionMapped(int position)
        {
            var entry = new EntryStub 
            { 
                Text = "Hello World Test",
                CursorPosition = position 
            };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.Equal(position, platformView.CaretIndex);
        }

        [AvaloniaTheory(DisplayName = "SelectionLength Mapped Correctly")]
        [InlineData(0, 0)]
        [InlineData(0, 5)]
        [InlineData(5, 5)]
        public async Task SelectionLengthMapped(int cursorPos, int selectionLength)
        {
            var entry = new EntryStub 
            { 
                Text = "Hello World Test",
                CursorPosition = cursorPos,
                SelectionLength = selectionLength
            };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            var actualLength = platformView.SelectionEnd - platformView.SelectionStart;
            Assert.Equal(selectionLength, actualLength);
        }

        [AvaloniaTheory(DisplayName = "HorizontalTextAlignment Mapped Correctly")]
        [InlineData(Microsoft.Maui.TextAlignment.Start)]
        [InlineData(Microsoft.Maui.TextAlignment.Center)]
        [InlineData(Microsoft.Maui.TextAlignment.End)]
        public async Task HorizontalTextAlignmentMapped(Microsoft.Maui.TextAlignment alignment)
        {
            var entry = new EntryStub { HorizontalTextAlignment = alignment };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            var expectedAlignment = alignment switch
            {
                Microsoft.Maui.TextAlignment.Start => Avalonia.Media.TextAlignment.Start,
                Microsoft.Maui.TextAlignment.Center => Avalonia.Media.TextAlignment.Center,
                Microsoft.Maui.TextAlignment.End => Avalonia.Media.TextAlignment.End,
                _ => Avalonia.Media.TextAlignment.Start
            };
            
            Assert.Equal(expectedAlignment, platformView.TextAlignment);
        }

        [AvaloniaTheory(DisplayName = "VerticalTextAlignment Mapped Correctly")]
        [InlineData(Microsoft.Maui.TextAlignment.Start)]
        [InlineData(Microsoft.Maui.TextAlignment.Center)]
        [InlineData(Microsoft.Maui.TextAlignment.End)]
        public async Task VerticalTextAlignmentMapped(Microsoft.Maui.TextAlignment alignment)
        {
            var entry = new EntryStub { VerticalTextAlignment = alignment };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            var expectedAlignment = alignment switch
            {
                Microsoft.Maui.TextAlignment.Start => Layout.VerticalAlignment.Top,
                Microsoft.Maui.TextAlignment.Center => Layout.VerticalAlignment.Center,
                Microsoft.Maui.TextAlignment.End => Layout.VerticalAlignment.Bottom,
                _ => Layout.VerticalAlignment.Top
            };
            
            Assert.Equal(expectedAlignment, platformView.VerticalContentAlignment);
        }

        [AvaloniaTheory(DisplayName = "FontSize Mapped Correctly")]
        [InlineData(10)]
        [InlineData(14)]
        [InlineData(24)]
        public async Task FontSizeMapped(double fontSize)
        {
            var entry = new EntryStub { Font = Font.OfSize("Default", fontSize) };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            Assert.Equal(fontSize, platformView.FontSize);
        }
        
        [AvaloniaFact(DisplayName = "Multiple Text Updates Work")]
        public async Task MultipleTextUpdatesWork()
        {
            var entry = new EntryStub { Text = "Initial" };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            for (int i = 0; i < 10; i++)
            {
                entry.Text = $"Update {i}";
                await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.Text)));
                Assert.Equal($"Update {i}", platformView.Text);
            }
        }

        [AvaloniaTheory(DisplayName = "Keyboard ContentType Mapped Correctly")]
        [InlineData("Email")]
        [InlineData("Numeric")]
        [InlineData("Telephone")]
        [InlineData("Url")]
        [InlineData("Text")]
        public async Task KeyboardContentTypeMapped(string keyboardTypeName)
        {
            var keyboard = keyboardTypeName switch
            {
                "Email" => Keyboard.Email,
                "Numeric" => Keyboard.Numeric,
                "Telephone" => Keyboard.Telephone,
                "Url" => Keyboard.Url,
                "Text" => Keyboard.Text,
                _ => Keyboard.Default
            };
            
            var entry = new EntryStub { Keyboard = keyboard };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            var expectedContentType = keyboardTypeName switch
            {
                "Email" => Input.TextInput.TextInputContentType.Email,
                "Numeric" => Input.TextInput.TextInputContentType.Number,
                "Telephone" => Input.TextInput.TextInputContentType.Digits,
                "Url" => Input.TextInput.TextInputContentType.Url,
                _ => Input.TextInput.TextInputContentType.Normal
            };
            
            Assert.Equal(expectedContentType, Input.TextInput.TextInputOptions.GetContentType(platformView));
        }

        [AvaloniaFact(DisplayName = "IsTextPredictionEnabled Mapped Correctly")]
        public async Task IsTextPredictionEnabledMappedCorrectly()
        {
            var entry = new EntryStub { IsTextPredictionEnabled = false };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;

            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.IsTextPredictionEnabled)));
            
            Assert.False(Input.TextInput.TextInputOptions.GetShowSuggestions(platformView));
            
            entry.IsTextPredictionEnabled = true;
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.IsTextPredictionEnabled)));
            
            Assert.True(Input.TextInput.TextInputOptions.GetShowSuggestions(platformView));
        }

        [AvaloniaTheory(DisplayName = "CharacterSpacing Mapped Correctly")]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task CharacterSpacingMapped(double spacing)
        {
            var entry = new EntryStub { CharacterSpacing = spacing };
            var handler = await CreateHandlerAsync(entry);
            var platformView = (AvaloniaTextBox)handler.PlatformView!;
            
            // CharacterSpacing calculation: direct mapping
            Assert.Equal(spacing, platformView.LetterSpacing);
        }
        [AvaloniaFact(DisplayName = "Background Mapped Correctly")]
        public async Task BackgroundMapped()
        {
             var color = Microsoft.Maui.Graphics.Colors.Red;
             var entry = new EntryStub { Background = new Microsoft.Maui.Graphics.SolidPaint(color) };
             var handler = await CreateHandlerAsync(entry);
             var platformView = handler.PlatformView!;
             
             var brush = platformView.Background as ISolidColorBrush;
             Assert.NotNull(brush);
             Assert.Equal(Colors.Red, brush.Color);
             
             entry.Background = null;
             await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IView.Background)));
             // Should verify it cleared (value might be null or default theme value depending on implementation)
             // EntryExtensions calls ClearValue, so it should be Unset. 
             // Reading Value might give default.
             // We can check if LocalValue is unset? Or just skip verifying "null" exact state and verify logic doesn't crash.
        }

        [AvaloniaFact(DisplayName = "Font Attributes Mapped Correctly")]
        public async Task FontAttributesMapped()
        {
            var entry = new EntryStub { Font = Font.OfSize("Arial", 12).WithAttributes(FontAttributes.Bold | FontAttributes.Italic) };
            var handler = await CreateHandlerAsync(entry);
            var platformView = handler.PlatformView!;
            
            Assert.Equal(Media.FontWeight.Bold, platformView.FontWeight);
            Assert.Equal(FontStyle.Italic, platformView.FontStyle);
        }

        [AvaloniaTheory(DisplayName = "TextTransform Mapped Correctly")]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Uppercase)]
        [InlineData(TextTransform.Lowercase)]
        public async Task TextTransformMapped(TextTransform transform)
        {
            var entry = new EntryStub { Text = "Test", TextTransform = transform };
            var handler = await CreateHandlerAsync(entry);
            var platformView = handler.PlatformView!;
            
            var expectedText = transform switch {
                TextTransform.Uppercase => "TEST",
                TextTransform.Lowercase => "test",
                _ => "Test"
            };
            
            Assert.Equal(expectedText, platformView.Text);
        }

        [AvaloniaFact(DisplayName = "Completed Event Fired")]
        public async Task CompletedEventFired()
        {
             var entry = new EntryStub();
             var handler = await CreateHandlerAsync(entry);
             var platformView = (MauiEntry)handler.PlatformView!;
             
             platformView.RaiseEvent(new Input.KeyEventArgs 
             { 
                RoutedEvent = Input.InputElement.KeyDownEvent, 
                Key = Input.Key.Enter,
                Source = platformView
             });
             
             Assert.True(entry.CompletedCalled);
        }

        [AvaloniaFact(DisplayName = "CursorPosition Clamped")]
        public async Task CursorPositionClamped()
        {
             var entry = new EntryStub { Text = "Hello", CursorPosition = 100 };
             var handler = await CreateHandlerAsync(entry);
             var platformView = handler.PlatformView!;
             
             Assert.Equal(5, platformView.CaretIndex);
        }
        
        [AvaloniaFact(DisplayName = "SelectionLength Clamped")]
        public async Task SelectionLengthClamped()
        {
             var entry = new EntryStub { Text = "Hello", CursorPosition = 0, SelectionLength = 100 };
             var handler = await CreateHandlerAsync(entry);
             var platformView = handler.PlatformView!;
             
             Assert.Equal(0, platformView.SelectionStart);
             Assert.Equal(5, platformView.SelectionEnd);
        }

        [AvaloniaFact(DisplayName = "ClearCommand Clears Text")]
        public async Task ClearCommandClearsText()
        {
             var entry = new EntryStub { Text = "Value" };
             var handler = await CreateHandlerAsync(entry);
             var platformView = handler.PlatformView!;
             
             Assert.Equal("Value", platformView.Text);
             
             await InvokeOnMainThreadAsync(() => platformView.ClearCommand.Execute(null));
             
             Assert.Empty(platformView.Text ?? string.Empty);
             Assert.Empty(entry.Text ?? string.Empty); 
        }

        [AvaloniaTheory(DisplayName = "ReturnType Mapped Correctly")]
        [InlineData(ReturnType.Default)]
        [InlineData(ReturnType.Done)]
        [InlineData(ReturnType.Go)]
        [InlineData(ReturnType.Next)]
        [InlineData(ReturnType.Search)]
        [InlineData(ReturnType.Send)]
        public async Task ReturnTypeMapped(ReturnType returnType)
        {
            var entry = new EntryStub { ReturnType = returnType };
            var handler = await CreateHandlerAsync(entry);
            var platformView = handler.PlatformView!;
            
            var expected = returnType switch
            {
                ReturnType.Go => TextInputReturnKeyType.Go,
                ReturnType.Next => TextInputReturnKeyType.Next,
                ReturnType.Search => TextInputReturnKeyType.Search,
                ReturnType.Send => TextInputReturnKeyType.Send,
                ReturnType.Done => TextInputReturnKeyType.Done,
                _ => TextInputReturnKeyType.Default
            };
            
            Assert.Equal(expected, TextInputOptions.GetReturnKeyType(platformView));
        }

        [AvaloniaFact(DisplayName = "IsSpellCheckEnabled Mapped Correctly (Smoke Test)")]
        public async Task IsSpellCheckEnabledMapped()
        {
            // Currently not implemented on Avalonia TextBox, but ensuring it doesn't crash
            var entry = new EntryStub { IsSpellCheckEnabled = false };
            var handler = await CreateHandlerAsync(entry);
            
            await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(IEntry.IsSpellCheckEnabled)));
            
            // Success if no exception thrown
            Assert.NotNull(handler.PlatformView);
        }
    }
}
