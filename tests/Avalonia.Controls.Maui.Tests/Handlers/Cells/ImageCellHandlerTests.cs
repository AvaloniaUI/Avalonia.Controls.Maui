using Avalonia.Controls.Maui.Handlers.Cells;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers.Cells;

public class ImageCellHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Platform View Initializes Correctly")]
    public async Task PlatformViewInitializesCorrectly()
    {
        var cell = new ImageCell();
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiImageCell>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Text Maps Correctly")]
    public async Task TextMapsCorrectly()
    {
        var cell = new ImageCell { Text = "User Name" };
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.Equal("User Name", handler.PlatformView.PrimaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "Detail Maps Correctly")]
    public async Task DetailMapsCorrectly()
    {
        var cell = new ImageCell { Text = "Title", Detail = "Role" };
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.Equal("Role", handler.PlatformView.SecondaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "Text Updates Correctly")]
    public async Task TextUpdatesCorrectly()
    {
        var cell = new ImageCell { Text = "Initial" };
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.Equal("Initial", handler.PlatformView.PrimaryLabel.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Text = "Updated";
            handler.UpdateValue(nameof(ImageCell.Text));
        });

        Assert.Equal("Updated", handler.PlatformView.PrimaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "Detail Updates Correctly")]
    public async Task DetailUpdatesCorrectly()
    {
        var cell = new ImageCell { Detail = "Initial Detail" };
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.Equal("Initial Detail", handler.PlatformView.SecondaryLabel.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Detail = "Updated Detail";
            handler.UpdateValue(nameof(ImageCell.Detail));
        });

        Assert.Equal("Updated Detail", handler.PlatformView.SecondaryLabel.Text);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Maps Correctly")]
    public async Task IsEnabledMapsCorrectly()
    {
        var cell = new ImageCell { IsEnabled = false };
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Updates Correctly")]
    public async Task IsEnabledUpdatesCorrectly()
    {
        var cell = new ImageCell { IsEnabled = true };
        var handler = await CreateHandlerAsync<ImageCellHandler>(cell);

        Assert.True(handler.PlatformView.IsEnabled);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.IsEnabled = false;
            handler.UpdateValue(nameof(Cell.IsEnabled));
        });

        Assert.False(handler.PlatformView.IsEnabled);
    }
}
