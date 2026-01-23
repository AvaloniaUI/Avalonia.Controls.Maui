using Avalonia.Controls.Maui.Compatibility;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers.Cells;

public class TextCellHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Platform View Initializes Correctly")]
    public async Task PlatformViewInitializesCorrectly()
    {
        var cell = new TextCell();
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiTextCell>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Text Maps Correctly")]
    public async Task TextMapsCorrectly()
    {
        var cell = new TextCell { Text = "Test Text" };
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.Equal("Test Text", handler.PlatformView.PrimaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "Detail Maps Correctly")]
    public async Task DetailMapsCorrectly()
    {
        var cell = new TextCell { Text = "Title", Detail = "Detail Text" };
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.Equal("Detail Text", handler.PlatformView.SecondaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "Text Updates Correctly")]
    public async Task TextUpdatesCorrectly()
    {
        var cell = new TextCell { Text = "Initial" };
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.Equal("Initial", handler.PlatformView.PrimaryLabel.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Text = "Updated";
            handler.UpdateValue(nameof(TextCell.Text));
        });

        Assert.Equal("Updated", handler.PlatformView.PrimaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "Detail Updates Correctly")]
    public async Task DetailUpdatesCorrectly()
    {
        var cell = new TextCell { Detail = "Initial Detail" };
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.Equal("Initial Detail", handler.PlatformView.SecondaryLabel.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Detail = "Updated Detail";
            handler.UpdateValue(nameof(TextCell.Detail));
        });

        Assert.Equal("Updated Detail", handler.PlatformView.SecondaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Maps Correctly")]
    public async Task IsEnabledMapsCorrectly()
    {
        var cell = new TextCell { IsEnabled = false };
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Updates Correctly")]
    public async Task IsEnabledUpdatesCorrectly()
    {
        var cell = new TextCell { IsEnabled = true };
        var handler = await CreateHandlerAsync<TextCellHandler>(cell);

        Assert.True(handler.PlatformView.IsEnabled);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.IsEnabled = false;
            handler.UpdateValue(nameof(Cell.IsEnabled));
        });

        Assert.False(handler.PlatformView.IsEnabled);
    }
}
