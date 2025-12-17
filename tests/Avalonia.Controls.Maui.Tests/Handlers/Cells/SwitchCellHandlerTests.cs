using Avalonia.Controls.Maui.Handlers.Cells;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers.Cells;

public class SwitchCellHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Platform View Initializes Correctly")]
    public async Task PlatformViewInitializesCorrectly()
    {
        var cell = new SwitchCell();
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiSwitchCell>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Text Maps Correctly")]
    public async Task TextMapsCorrectly()
    {
        var cell = new SwitchCell { Text = "Toggle Option" };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.Equal("Toggle Option", handler.PlatformView.Label.Text);
    }

    [AvaloniaFact(DisplayName = "On Maps Correctly When True")]
    public async Task OnMapsCorrectlyWhenTrue()
    {
        var cell = new SwitchCell { On = true };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.True(handler.PlatformView.ToggleSwitch.IsChecked);
    }

    [AvaloniaFact(DisplayName = "On Maps Correctly When False")]
    public async Task OnMapsCorrectlyWhenFalse()
    {
        var cell = new SwitchCell { On = false };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.False(handler.PlatformView.ToggleSwitch.IsChecked);
    }

    [AvaloniaFact(DisplayName = "On Updates Correctly")]
    public async Task OnUpdatesCorrectly()
    {
        var cell = new SwitchCell { On = false };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.False(handler.PlatformView.ToggleSwitch.IsChecked);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.On = true;
            handler.UpdateValue(nameof(SwitchCell.On));
        });

        Assert.True(handler.PlatformView.ToggleSwitch.IsChecked);
    }

    [AvaloniaFact(DisplayName = "Text Updates Correctly")]
    public async Task TextUpdatesCorrectly()
    {
        var cell = new SwitchCell { Text = "Initial" };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.Equal("Initial", handler.PlatformView.Label.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Text = "Updated";
            handler.UpdateValue(nameof(SwitchCell.Text));
        });

        Assert.Equal("Updated", handler.PlatformView.Label.Text);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Maps Correctly")]
    public async Task IsEnabledMapsCorrectly()
    {
        var cell = new SwitchCell { IsEnabled = false };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Updates Correctly")]
    public async Task IsEnabledUpdatesCorrectly()
    {
        var cell = new SwitchCell { IsEnabled = true };
        var handler = await CreateHandlerAsync<SwitchCellHandler>(cell);

        Assert.True(handler.PlatformView.IsEnabled);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.IsEnabled = false;
            handler.UpdateValue(nameof(Cell.IsEnabled));
        });

        Assert.False(handler.PlatformView.IsEnabled);
    }
}
