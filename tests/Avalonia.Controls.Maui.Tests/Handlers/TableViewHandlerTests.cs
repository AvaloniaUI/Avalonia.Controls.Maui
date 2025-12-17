using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class TableViewHandlerTests : HandlerTestBase<TableViewHandler, TableViewStub>
{
    [AvaloniaFact(DisplayName = "Platform View Initializes Correctly")]
    public async Task PlatformViewInitializesCorrectly()
    {
        var tableView = new TableViewStub();
        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<Avalonia.Controls.Maui.MauiTableView>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Root Initializes Correctly")]
    public async Task RootInitializesCorrectly()
    {
        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Test Section")
                {
                    new TextCell { Text = "Test Cell" }
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        Assert.Equal(tableView, handler.PlatformView.TableView);
    }

    [AvaloniaFact(DisplayName = "RowHeight Updates Correctly")]
    public async Task RowHeightUpdatesCorrectly()
    {
        var tableView = new TableViewStub
        {
            RowHeight = 60
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.Equal(60, handler.PlatformView.RowHeight);

        // Update and verify
        tableView.RowHeight = 80;
        TableViewHandler.MapRowHeight(handler, tableView);

        Assert.Equal(80, handler.PlatformView.RowHeight);
    }

    [AvaloniaFact(DisplayName = "HasUnevenRows Updates Correctly")]
    public async Task HasUnevenRowsUpdatesCorrectly()
    {
        var tableView = new TableViewStub
        {
            HasUnevenRows = false
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.False(handler.PlatformView.HasUnevenRows);

        // Update and verify
        tableView.HasUnevenRows = true;
        TableViewHandler.MapHasUnevenRows(handler, tableView);

        Assert.True(handler.PlatformView.HasUnevenRows);
    }

    [AvaloniaFact(DisplayName = "Intent Updates Correctly")]
    public async Task IntentUpdatesCorrectly()
    {
        var tableView = new TableViewStub
        {
            Intent = TableIntent.Menu
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.Equal(TableIntent.Menu, handler.PlatformView.Intent);

        // Update and verify
        tableView.Intent = TableIntent.Settings;
        TableViewHandler.MapIntent(handler, tableView);

        Assert.Equal(TableIntent.Settings, handler.PlatformView.Intent);
    }

    [AvaloniaFact(DisplayName = "TextCell Renders Correctly")]
    public async Task TextCellRendersCorrectly()
    {
        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Section")
                {
                    new TextCell { Text = "Primary Text", Detail = "Detail Text" }
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        Assert.Single(handler.PlatformView.TableView.Root);
        Assert.Single(handler.PlatformView.TableView.Root[0]);
    }

    [AvaloniaFact(DisplayName = "SwitchCell Renders Correctly")]
    public async Task SwitchCellRendersCorrectly()
    {
        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Settings")
                {
                    new SwitchCell { Text = "Toggle Option", On = true }
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        var cell = handler.PlatformView.TableView.Root[0][0] as SwitchCell;
        Assert.NotNull(cell);
        Assert.True(cell.On);
    }

    [AvaloniaFact(DisplayName = "EntryCell Renders Correctly")]
    public async Task EntryCellRendersCorrectly()
    {
        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Form")
                {
                    new EntryCell { Label = "Name", Placeholder = "Enter name" }
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        var cell = handler.PlatformView.TableView.Root[0][0] as EntryCell;
        Assert.NotNull(cell);
        Assert.Equal("Name", cell.Label);
    }

    [AvaloniaFact(DisplayName = "ImageCell Renders Correctly")]
    public async Task ImageCellRendersCorrectly()
    {
        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Team")
                {
                    new ImageCell { Text = "User Name", Detail = "Role" }
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        var cell = handler.PlatformView.TableView.Root[0][0] as ImageCell;
        Assert.NotNull(cell);
        Assert.Equal("User Name", cell.Text);
    }

    [AvaloniaFact(DisplayName = "ViewCell Renders Correctly")]
    public async Task ViewCellRendersCorrectly()
    {
        var viewCell = new ViewCell
        {
            View = new Microsoft.Maui.Controls.Label { Text = "Custom Content" }
        };

        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Custom")
                {
                    viewCell
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        var cell = handler.PlatformView.TableView.Root[0][0] as ViewCell;
        Assert.NotNull(cell);
        Assert.NotNull(cell.View);
    }

    [AvaloniaFact(DisplayName = "Multiple Sections Render Correctly")]
    public async Task MultipleSectionsRenderCorrectly()
    {
        var tableView = new TableViewStub
        {
            Root = new TableRoot
            {
                new TableSection("Section 1")
                {
                    new TextCell { Text = "Item 1" }
                },
                new TableSection("Section 2")
                {
                    new TextCell { Text = "Item 2" },
                    new TextCell { Text = "Item 3" }
                }
            }
        };

        var handler = await CreateHandlerAsync(tableView);

        Assert.NotNull(handler.PlatformView.TableView);
        Assert.Equal(2, handler.PlatformView.TableView.Root.Count);
        Assert.Single(handler.PlatformView.TableView.Root[0]);
        Assert.Equal(2, handler.PlatformView.TableView.Root[1].Count);
    }
}
