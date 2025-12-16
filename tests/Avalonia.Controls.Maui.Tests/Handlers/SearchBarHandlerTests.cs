using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiSearchBarHandler = Avalonia.Controls.Maui.Handlers.SearchBarHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

/// <summary>
/// Tests for SearchBarHandler that maps ISearchBar to MauiSearchBar.
/// </summary>
public partial class SearchBarHandlerTests : HandlerTestBase<MauiSearchBarHandler, SearchBarStub>
{
    [AvaloniaFact(DisplayName = "Text Initializes Correctly")]
    public async Task TextInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            Text = "Hello World"
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.Text,
            GetNativeText,
            searchBar.Text);
    }

    [AvaloniaFact(DisplayName = "Empty Text Initializes Correctly")]
    public async Task EmptyTextInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            Text = string.Empty
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.Text,
            GetNativeText,
            searchBar.Text);
    }

    [AvaloniaTheory(DisplayName = "Text Updates Correctly")]
    [InlineData("Hello")]
    [InlineData("World")]
    [InlineData("")]
    public async Task TextUpdatesCorrectly(string text)
    {
        var searchBar = new SearchBarStub
        {
            Text = "Initial"
        };

        await ValidatePropertyUpdatesValue(
            searchBar,
            nameof(ISearchBar.Text),
            GetNativeText,
            text,
            "Initial");
    }

    [AvaloniaFact(DisplayName = "Placeholder Initializes Correctly")]
    public async Task PlaceholderInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            Placeholder = "Search..."
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.Placeholder,
            GetNativePlaceholder,
            searchBar.Placeholder);
    }

    [AvaloniaTheory(DisplayName = "Placeholder Updates Correctly")]
    [InlineData("Search items...")]
    [InlineData("Enter query")]
    [InlineData("")]
    public async Task PlaceholderUpdatesCorrectly(string placeholder)
    {
        var searchBar = new SearchBarStub
        {
            Placeholder = "Initial"
        };

        await ValidatePropertyUpdatesValue(
            searchBar,
            nameof(ISearchBar.Placeholder),
            GetNativePlaceholder,
            placeholder,
            "Initial");
    }

    [AvaloniaFact(DisplayName = "TextColor Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            TextColor = Colors.Red
        };

        var values = await GetValueAsync(searchBar, (handler) =>
        {
            return new
            {
                ViewValue = searchBar.TextColor,
                PlatformViewValue = GetNativeTextColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaTheory(DisplayName = "TextColor Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    public async Task TextColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var searchBar = new SearchBarStub
        {
            TextColor = Colors.White
        };

        var newColor = Color.FromRgb(r, g, b);

        var values = await GetValueAsync(searchBar, (handler) =>
        {
            searchBar.TextColor = newColor;
            handler.UpdateValue(nameof(ISearchBar.TextColor));

            return new
            {
                ViewValue = searchBar.TextColor,
                PlatformViewValue = GetNativeTextColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "CancelButtonColor Initializes Correctly")]
    public async Task CancelButtonColorInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            CancelButtonColor = Colors.Blue
        };

        var values = await GetValueAsync(searchBar, (handler) =>
        {
            return new
            {
                ViewValue = searchBar.CancelButtonColor,
                PlatformViewValue = GetNativeCancelButtonColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "IsReadOnly True Initializes Correctly")]
    public async Task IsReadOnlyTrueInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            IsReadOnly = true
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.IsReadOnly,
            GetNativeIsReadOnly,
            searchBar.IsReadOnly);
    }

    [AvaloniaFact(DisplayName = "IsReadOnly False Initializes Correctly")]
    public async Task IsReadOnlyFalseInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            IsReadOnly = false
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.IsReadOnly,
            GetNativeIsReadOnly,
            searchBar.IsReadOnly);
    }

    [AvaloniaTheory(DisplayName = "IsReadOnly Updates Correctly")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsReadOnlyUpdatesCorrectly(bool isReadOnly)
    {
        var searchBar = new SearchBarStub
        {
            IsReadOnly = !isReadOnly
        };

        await ValidatePropertyUpdatesValue(
            searchBar,
            nameof(ISearchBar.IsReadOnly),
            GetNativeIsReadOnly,
            isReadOnly,
            !isReadOnly);
    }

    [AvaloniaFact(DisplayName = "MaxLength Initializes Correctly")]
    public async Task MaxLengthInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            MaxLength = 50
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.MaxLength,
            GetNativeMaxLength,
            searchBar.MaxLength);
    }

    [AvaloniaTheory(DisplayName = "MaxLength Updates Correctly")]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(255)]
    public async Task MaxLengthUpdatesCorrectly(int maxLength)
    {
        var searchBar = new SearchBarStub
        {
            MaxLength = 50
        };

        await ValidatePropertyUpdatesValue(
            searchBar,
            nameof(ISearchBar.MaxLength),
            GetNativeMaxLength,
            maxLength,
            50);
    }

    [AvaloniaFact(DisplayName = "CharacterSpacing Initializes Correctly")]
    public async Task CharacterSpacingInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            CharacterSpacing = 2.5
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.CharacterSpacing,
            GetNativeCharacterSpacing,
            searchBar.CharacterSpacing);
    }

    [AvaloniaTheory(DisplayName = "HorizontalTextAlignment Initializes Correctly")]
    [InlineData(TextAlignment.Start)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.End)]
    public async Task HorizontalTextAlignmentInitializesCorrectly(TextAlignment alignment)
    {
        var searchBar = new SearchBarStub
        {
            HorizontalTextAlignment = alignment
        };

        var handler = await CreateHandlerAsync(searchBar);
        var platformView = handler.PlatformView;

        Assert.NotNull(platformView);

        var expectedAvalonia = alignment switch
        {
            TextAlignment.Start => Media.TextAlignment.Left,
            TextAlignment.Center => Media.TextAlignment.Center,
            TextAlignment.End => Media.TextAlignment.Right,
            _ => Media.TextAlignment.Left,
        };

        Assert.Equal(expectedAvalonia, platformView.HorizontalTextAlignment);
    }

    [AvaloniaTheory(DisplayName = "VerticalTextAlignment Initializes Correctly")]
    [InlineData(TextAlignment.Start)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.End)]
    public async Task VerticalTextAlignmentInitializesCorrectly(TextAlignment alignment)
    {
        var searchBar = new SearchBarStub
        {
            VerticalTextAlignment = alignment
        };

        var handler = await CreateHandlerAsync(searchBar);
        var platformView = handler.PlatformView;

        Assert.NotNull(platformView);

        var expectedAvalonia = alignment switch
        {
            TextAlignment.Start => Layout.VerticalAlignment.Top,
            TextAlignment.Center => Layout.VerticalAlignment.Center,
            TextAlignment.End => Layout.VerticalAlignment.Bottom,
            _ => Layout.VerticalAlignment.Center,
        };

        Assert.Equal(expectedAvalonia, platformView.VerticalContentAlignment);
    }

    [AvaloniaFact(DisplayName = "SearchIconColor Initializes Correctly")]
    public async Task SearchIconColorInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            SearchIconColor = Colors.Red
        };

        var values = await GetValueAsync(searchBar, (handler) =>
        {
            return new
            {
                ViewValue = searchBar.SearchIconColor,
                PlatformViewValue = GetNativeSearchIconColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "CursorPosition Initializes Correctly")]
    public async Task CursorPositionInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            Text = "Hello World",
            CursorPosition = 5
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.CursorPosition,
            GetNativeCursorPosition,
            searchBar.CursorPosition);
    }

    [AvaloniaFact(DisplayName = "SelectionLength Initializes Correctly")]
    public async Task SelectionLengthInitializesCorrectly()
    {
        var searchBar = new SearchBarStub
        {
            Text = "Hello World",
            SelectionLength = 5
        };

        await ValidatePropertyInitValue(
            searchBar,
            () => searchBar.SelectionLength,
            GetNativeSelectionLength,
            searchBar.SelectionLength);
    }

    [AvaloniaFact(DisplayName = "Handler Creates MauiSearchBar")]
    public async Task HandlerCreatesMauiSearchBar()
    {
        var searchBar = new SearchBarStub();

        var handler = await CreateHandlerAsync(searchBar);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiSearchBar>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Null TextColor Doesn't Crash")]
    public async Task NullTextColorDoesntCrash()
    {
        var searchBar = new SearchBarStub
        {
            TextColor = null!
        };

        await CreateHandlerAsync(searchBar);
    }

    [AvaloniaFact(DisplayName = "Null PlaceholderColor Doesn't Crash")]
    public async Task NullPlaceholderColorDoesntCrash()
    {
        var searchBar = new SearchBarStub
        {
            PlaceholderColor = null!
        };

        await CreateHandlerAsync(searchBar);
    }

    string GetNativeText(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.Text;
    }

    string GetNativePlaceholder(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.Placeholder;
    }

    Color? GetNativeTextColor(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);

        if (platformView.Foreground is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    Color? GetNativeCancelButtonColor(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);

        if (platformView.CancelButtonColor is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    bool GetNativeIsReadOnly(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.IsReadOnly;
    }

    int GetNativeMaxLength(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.MaxLength;
    }

    double GetNativeCharacterSpacing(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.CharacterSpacing;
    }

    Color? GetNativeSearchIconColor(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);

        if (platformView.SearchIconColor is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    int GetNativeCursorPosition(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.CursorPosition;
    }

    int GetNativeSelectionLength(MauiSearchBarHandler handler)
    {
        var platformView = handler.PlatformView;
        Assert.NotNull(platformView);
        return platformView.SelectionLength;
    }
}
