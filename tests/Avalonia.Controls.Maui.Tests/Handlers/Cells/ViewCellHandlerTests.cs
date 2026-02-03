using Avalonia.Controls.Maui.Compatibility;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers.Cells;

public class ViewCellHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Platform View Initializes Correctly")]
    public async Task PlatformViewInitializesCorrectly()
    {
        var cell = new ViewCell();
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiViewCell>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "View Maps Correctly")]
    public async Task ViewMapsCorrectly()
    {
        var label = new Microsoft.Maui.Controls.Label { Text = "Custom Content" };
        var cell = new ViewCell { View = label };
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.NotNull(handler.PlatformView.Child);
    }

    [AvaloniaFact(DisplayName = "Empty View Handles Correctly")]
    public async Task EmptyViewHandlesCorrectly()
    {
        var cell = new ViewCell { View = null };
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.Null(handler.PlatformView.Child);
    }

    [AvaloniaFact(DisplayName = "View Updates Correctly")]
    public async Task ViewUpdatesCorrectly()
    {
        var cell = new ViewCell { View = new Microsoft.Maui.Controls.Label { Text = "Initial" } };
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.NotNull(handler.PlatformView.Child);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.View = new Microsoft.Maui.Controls.Label { Text = "Updated" };
            handler.UpdateValue(nameof(ViewCell.View));
        });

        Assert.NotNull(handler.PlatformView.Child);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Maps Correctly")]
    public async Task IsEnabledMapsCorrectly()
    {
        var cell = new ViewCell 
        { 
            View = new Microsoft.Maui.Controls.Label { Text = "Test" },
            IsEnabled = false 
        };
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Updates Correctly")]
    public async Task IsEnabledUpdatesCorrectly()
    {
        var cell = new ViewCell 
        { 
            View = new Microsoft.Maui.Controls.Label { Text = "Test" },
            IsEnabled = true 
        };
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.True(handler.PlatformView.IsEnabled);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.IsEnabled = false;
            handler.UpdateValue(nameof(Cell.IsEnabled));
        });

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "Complex View Content Maps Correctly")]
    public async Task ComplexViewContentMapsCorrectly()
    {
        var stackLayout = new VerticalStackLayout
        {
            Children =
            {
                new Microsoft.Maui.Controls.Label { Text = "Title" },
                new Microsoft.Maui.Controls.Label { Text = "Subtitle" }
            }
        };
        var cell = new ViewCell { View = stackLayout };
        var handler = await CreateHandlerAsync<ViewCellHandler>(cell);

        Assert.NotNull(handler.PlatformView.Child);
    }
}

