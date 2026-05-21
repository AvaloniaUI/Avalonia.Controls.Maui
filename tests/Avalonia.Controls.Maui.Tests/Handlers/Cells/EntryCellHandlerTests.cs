using Avalonia.Controls.Maui.Compatibility;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using MauiTextAlignment = Microsoft.Maui.TextAlignment;

namespace Avalonia.Controls.Maui.Tests.Handlers.Cells;

public class EntryCellHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Platform View Initializes Correctly")]
    public async Task PlatformViewInitializesCorrectly()
    {
        var cell = new EntryCell();
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiEntryCell>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Label Maps Correctly")]
    public async Task LabelMapsCorrectly()
    {
        var cell = new EntryCell { Label = "Username" };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal("Username", handler.PlatformView.Label.Text);
    }

    [AvaloniaFact(DisplayName = "Text Maps Correctly")]
    public async Task TextMapsCorrectly()
    {
        var cell = new EntryCell { Text = "john.doe" };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal("john.doe", handler.PlatformView.Input.Text);
    }

    [AvaloniaFact(DisplayName = "Placeholder Maps Correctly")]
    public async Task PlaceholderMapsCorrectly()
    {
        var cell = new EntryCell { Placeholder = "Enter name" };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal("Enter name", handler.PlatformView.Input.Watermark);
    }

    [AvaloniaFact(DisplayName = "Label Updates Correctly")]
    public async Task LabelUpdatesCorrectly()
    {
        var cell = new EntryCell { Label = "Initial" };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal("Initial", handler.PlatformView.Label.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Label = "Updated";
            handler.UpdateValue(nameof(EntryCell.Label));
        });

        Assert.Equal("Updated", handler.PlatformView.Label.Text);
    }

    [AvaloniaFact(DisplayName = "Text Updates Correctly")]
    public async Task TextUpdatesCorrectly()
    {
        var cell = new EntryCell { Text = "Initial" };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal("Initial", handler.PlatformView.Input.Text);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.Text = "Updated";
            handler.UpdateValue(nameof(EntryCell.Text));
        });

        Assert.Equal("Updated", handler.PlatformView.Input.Text);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Maps Correctly")]
    public async Task IsEnabledMapsCorrectly()
    {
        var cell = new EntryCell { IsEnabled = false };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Updates Correctly")]
    public async Task IsEnabledUpdatesCorrectly()
    {
        var cell = new EntryCell { IsEnabled = true };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.True(handler.PlatformView.IsEnabled);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.IsEnabled = false;
            handler.UpdateValue(nameof(Cell.IsEnabled));
        });

        Assert.False(handler.PlatformView.IsEnabled);
    }

    [AvaloniaTheory(DisplayName = "VerticalTextAlignment Maps Correctly")]
    [InlineData(MauiTextAlignment.Start, Avalonia.Layout.VerticalAlignment.Top)]
    [InlineData(MauiTextAlignment.Center, Avalonia.Layout.VerticalAlignment.Center)]
    [InlineData(MauiTextAlignment.End, Avalonia.Layout.VerticalAlignment.Bottom)]
    public async Task VerticalTextAlignmentMapsCorrectly(
        MauiTextAlignment alignment,
        Avalonia.Layout.VerticalAlignment expected)
    {
        var cell = new EntryCell { VerticalTextAlignment = alignment };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal(expected, handler.PlatformView.Input.VerticalContentAlignment);
    }

    [AvaloniaFact(DisplayName = "VerticalTextAlignment Updates Correctly")]
    public async Task VerticalTextAlignmentUpdatesCorrectly()
    {
        var cell = new EntryCell { VerticalTextAlignment = MauiTextAlignment.Start };
        var handler = await CreateHandlerAsync<EntryCellHandler>(cell);

        Assert.Equal(Avalonia.Layout.VerticalAlignment.Top, handler.PlatformView.Input.VerticalContentAlignment);

        await InvokeOnMainThreadAsync(() =>
        {
            cell.VerticalTextAlignment = MauiTextAlignment.End;
            handler.UpdateValue(nameof(EntryCell.VerticalTextAlignment));
        });

        Assert.Equal(Avalonia.Layout.VerticalAlignment.Bottom, handler.PlatformView.Input.VerticalContentAlignment);
    }
}
