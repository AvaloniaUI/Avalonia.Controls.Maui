using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiPickerHandler = Avalonia.Controls.Maui.Handlers.PickerHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class PickerHandlerTests : HandlerTestBase<MauiPickerHandler, PickerStub>
{
    [AvaloniaFact(DisplayName = "Title Initializes Correctly")]
    public async Task TitleInitializesCorrectly()
    {
        var picker = new PickerStub
        {
            Title = "Choose item"
        };

        await ValidatePropertyInitValue(
            picker,
            () => picker.Title,
            GetNativeTitle,
            picker.Title);
    }

    [AvaloniaFact(DisplayName = "Title Color Initializes Correctly")]
    public async Task TitleColorInitializesCorrectly()
    {
        var picker = new PickerStub
        {
            Title = "Choose",
            TitleColor = Colors.Blue
        };

        var platformColor = await GetValueAsync(picker, GetNativeTitleColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, platformColor);
    }

    [AvaloniaFact(DisplayName = "Text Color Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var picker = new PickerStub
        {
            TextColor = Colors.Red
        };

        var platformColor = await GetValueAsync(picker, GetNativeTextColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaTheory(DisplayName = "Selected Index Initializes Correctly")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    public async Task SelectedIndexInitializesCorrectly(int selectedIndex)
    {
        var picker = new PickerStub
        {
            Items = new List<string> { "Item 1", "Item 2", "Item 3" },
            SelectedIndex = selectedIndex
        };

        var expectedIndex = selectedIndex >= 0 && selectedIndex < 3 ? selectedIndex : -1;

        await ValidatePropertyInitValue(
            picker,
            () => picker.SelectedIndex,
            GetNativeSelectedIndex,
            expectedIndex);
    }

    [AvaloniaFact(DisplayName = "Out of Bounds Selected Index Sets Negative")]
    public async Task OutOfBoundsSelectedIndexSetsNegative()
    {
        var picker = new PickerStub
        {
            Items = new List<string> { "Item 1", "Item 2", "Item 3" },
            SelectedIndex = 10
        };

        var platformIndex = await GetValueAsync(picker, GetNativeSelectedIndex);

        Assert.Equal(-1, platformIndex);
    }

    [AvaloniaFact(DisplayName = "Font Size Initializes Correctly")]
    public async Task FontSizeInitializesCorrectly()
    {
        var picker = new PickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(20)
        };

        var platformFontSize = await GetValueAsync(picker, GetNativeFontSize);

        Assert.Equal(20, platformFontSize);
    }

    [AvaloniaTheory(DisplayName = "Horizontal Text Alignment Initializes Correctly")]
    [InlineData(TextAlignment.Start)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.End)]
    public async Task HorizontalTextAlignmentInitializesCorrectly(TextAlignment alignment)
    {
        var picker = new PickerStub
        {
            HorizontalTextAlignment = alignment
        };

        var platformAlignment = await GetValueAsync(picker, GetNativeHorizontalAlignment);

        var expectedAlignment = alignment switch
        {
            TextAlignment.Start => Avalonia.Layout.HorizontalAlignment.Left,
            TextAlignment.Center => Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment.End => Avalonia.Layout.HorizontalAlignment.Right,
            _ => Avalonia.Layout.HorizontalAlignment.Left
        };

        Assert.Equal(expectedAlignment, platformAlignment);
    }

    [AvaloniaTheory(DisplayName = "Vertical Text Alignment Initializes Correctly")]
    [InlineData(TextAlignment.Start)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.End)]
    public async Task VerticalTextAlignmentInitializesCorrectly(TextAlignment alignment)
    {
        var picker = new PickerStub
        {
            VerticalTextAlignment = alignment
        };

        var platformAlignment = await GetValueAsync(picker, GetNativeVerticalAlignment);

        var expectedAlignment = alignment switch
        {
            TextAlignment.Start => Avalonia.Layout.VerticalAlignment.Top,
            TextAlignment.Center => Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment.End => Avalonia.Layout.VerticalAlignment.Bottom,
            _ => Avalonia.Layout.VerticalAlignment.Center
        };

        Assert.Equal(expectedAlignment, platformAlignment);
    }

    [AvaloniaTheory(DisplayName = "Selected Index Updates Correctly")]
    [InlineData(-1, 1)]
    [InlineData(0, 2)]
    [InlineData(2, 0)]
    public async Task SelectedIndexUpdatesCorrectly(int initialIndex, int newIndex)
    {
        var picker = new PickerStub
        {
            Items = new List<string> { "Item 1", "Item 2", "Item 3" },
            SelectedIndex = initialIndex
        };

        await ValidatePropertyUpdatesValue(
            picker,
            nameof(IPicker.SelectedIndex),
            GetNativeSelectedIndex,
            newIndex,
            initialIndex);
    }

    [AvaloniaFact(DisplayName = "Items Sets Items")]
    public async Task ItemsSetsItems()
    {
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var picker = new PickerStub
        {
            Items = items
        };

        var handler = await CreateHandlerAsync(picker);
        var platformItemsSource = await InvokeOnMainThreadAsync(() => handler.PlatformView?.ItemsSource);

        Assert.NotNull(platformItemsSource);
    }

    [AvaloniaFact(DisplayName = "Character Spacing Applies Template")]
    public async Task CharacterSpacingAppliesTemplate()
    {
        var picker = new PickerStub
        {
            CharacterSpacing = 4,
            Items = new List<string> { "Item 1", "Item 2" }
        };

        var handler = await CreateHandlerAsync(picker);

        var template = await InvokeOnMainThreadAsync(() => handler.PlatformView?.ItemTemplate);
        var selectionTemplate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectionBoxItemTemplate);

        Assert.IsType<FuncDataTemplate<string>>(template);
        Assert.IsType<FuncDataTemplate<string>>(selectionTemplate);

        var textBlock = await InvokeOnMainThreadAsync(() =>
        {
            var dataTemplate = (FuncDataTemplate<string>)template!;
            return dataTemplate.Build("Item 1");
        });

        var letterSpacing = await InvokeOnMainThreadAsync(() => (textBlock as TextBlock)?.LetterSpacing);

        Assert.NotNull(letterSpacing);
        Assert.True(letterSpacing > 0);
    }

    [AvaloniaFact(DisplayName = "Native Selection Updates Virtual View")]
    public async Task NativeSelectionUpdatesVirtualView()
    {
        var picker = new PickerStub
        {
            Items = new List<string> { "Item 1", "Item 2", "Item 3" },
            SelectedIndex = 0
        };

        var handler = await CreateHandlerAsync(picker);

        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView!.SelectedIndex = 2;
        });

        Assert.Equal(2, picker.SelectedIndex);
    }

    [AvaloniaFact(DisplayName = "Selected Index Applied After Items")]
    public async Task SelectedIndexAppliedAfterItems()
    {
        var picker = new PickerStub
        {
            Items = new List<string> { "Red", "Green", "Blue" },
            SelectedIndex = 1
        };

        var handler = await CreateHandlerAsync(picker);

        var selectedIndex = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedIndex ?? -1);

        Assert.Equal(1, selectedIndex);
    }

    [AvaloniaFact(DisplayName = "Initial Selected Index Not Cleared By Native Events")]
    public async Task InitialSelectedIndexNotCleared()
    {
        var picker = new PickerStub
        {
            Items = new List<string> { "Red", "Green", "Blue" },
            SelectedIndex = 2
        };

        var handler = await CreateHandlerAsync(picker);

        var virtualSelectedIndex = picker.SelectedIndex;
        var nativeSelectedIndex = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedIndex ?? -1);

        Assert.Equal(2, virtualSelectedIndex);
        Assert.Equal(2, nativeSelectedIndex);
    }

    // Platform-specific property getters
    string? GetNativeTitle(MauiPickerHandler handler)
    {
        return handler.PlatformView is MauiComboBox comboBox ? comboBox.Header?.ToString() : null;
    }

    Color? GetNativeTitleColor(MauiPickerHandler handler)
    {
        if (handler.PlatformView is not MauiComboBox comboBox)
            return null;

        // If HeaderTemplate is set, try to extract the color from it
        if (comboBox.HeaderTemplate is FuncDataTemplate<object?> template)
        {
            // Build the template with the header data to inspect the created control
            var control = template.Build(comboBox.Header);
            if (control is TextBlock textBlock && textBlock.Foreground is Avalonia.Media.SolidColorBrush brush)
            {
                return new Color(
                    brush.Color.R / 255f,
                    brush.Color.G / 255f,
                    brush.Color.B / 255f,
                    brush.Color.A / 255f);
            }
        }

        return null;
    }

    Color? GetNativeTextColor(MauiPickerHandler handler)
    {
        if (handler.PlatformView is not MauiComboBox comboBox)
            return null;

        if (comboBox.Foreground is Avalonia.Media.SolidColorBrush brush)
        {
            return new Color(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }

    int GetNativeSelectedIndex(MauiPickerHandler handler)
    {
        return handler.PlatformView is MauiComboBox comboBox ? comboBox.SelectedIndex : -1;
    }

    double GetNativeFontSize(MauiPickerHandler handler)
    {
        return handler.PlatformView is MauiComboBox comboBox ? comboBox.FontSize : 0;
    }

    Avalonia.Layout.HorizontalAlignment GetNativeHorizontalAlignment(MauiPickerHandler handler)
    {
        return handler.PlatformView is MauiComboBox comboBox
            ? comboBox.HorizontalContentAlignment
            : Avalonia.Layout.HorizontalAlignment.Left;
    }

    Avalonia.Layout.VerticalAlignment GetNativeVerticalAlignment(MauiPickerHandler handler)
    {
        return handler.PlatformView is MauiComboBox comboBox
            ? comboBox.VerticalContentAlignment
            : Avalonia.Layout.VerticalAlignment.Center;
    }
}
