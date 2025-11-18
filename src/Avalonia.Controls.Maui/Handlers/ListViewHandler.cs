using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Handlers;

public class ListViewHandler : ViewHandler<Microsoft.Maui.Controls.ListView, ListBox>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.ListView, ListViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.ListView, ListViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.ListView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.ListView.ItemTemplate)] = MapItemTemplate,
            [nameof(Microsoft.Maui.Controls.ListView.SelectedItem)] = MapSelectedItem,
            // Note: HasUnevenRows, RowHeight, IsGroupingEnabled, GroupHeaderTemplate,
            // Header, Footer, SeparatorVisibility, and SeparatorColor
            // require custom implementation with Avalonia's ListBox
        };

    public static CommandMapper<Microsoft.Maui.Controls.ListView, ListViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    public ListViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public ListViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ListViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override ListBox CreatePlatformView()
    {
        var listBox = new ListBox
        {
            SelectionMode = SelectionMode.Single
        };
        return listBox;
    }

    protected override void ConnectHandler(ListBox platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectionChanged += OnSelectionChanged;
    }

    protected override void DisconnectHandler(ListBox platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        // Update the SelectedItem property on the ListView
        var selectedItem = PlatformView.SelectedItem;

        // Update the MAUI ListView's SelectedItem
        if (VirtualView.SelectedItem != selectedItem)
        {
            VirtualView.SelectedItem = selectedItem;
        }
    }

    public override bool NeedsContainer => false;

    public static void MapItemsSource(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler.PlatformView.ItemsSource = listView.ItemsSource;
    }

    public static void MapItemTemplate(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (listView.ItemTemplate != null)
        {
            // Create an Avalonia DataTemplate from the MAUI DataTemplate
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                // Create MAUI view/cell from template
                var templateContent = listView.ItemTemplate.CreateContent();
                if (templateContent == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                Microsoft.Maui.Controls.View? mauiView = null;

                // Check if template content is a Cell (like ViewCell, TextCell, etc.)
                if (templateContent is Microsoft.Maui.Controls.Cell cell)
                {
                    // Set the binding context on the cell
                    cell.BindingContext = item;

                    // For ViewCell, extract the View property
                    if (cell is Microsoft.Maui.Controls.ViewCell viewCell)
                    {
                        mauiView = viewCell.View;
                    }
                    else
                    {
                        // For other cell types (TextCell, ImageCell, etc.), 
                        // we need to get the platform view from the cell's handler
                        var cellHandler = cell.ToHandler(handler.MauiContext);
                        if (cellHandler?.PlatformView is Control cellControl)
                        {
                            return cellControl;
                        }
                        return new TextBlock { Text = item?.ToString() ?? string.Empty };
                    }
                }
                else if (templateContent is Microsoft.Maui.Controls.View view)
                {
                    // Template content is directly a View (not wrapped in a Cell)
                    mauiView = view;
                }

                if (mauiView == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                // Set the binding context
                mauiView.BindingContext = item;

                // Convert to platform control
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                return platformControl ?? new TextBlock { Text = item?.ToString() ?? string.Empty };
            });

            handler.PlatformView.ItemTemplate = avaloniaTemplate;
        }
        else
        {
            // Default template - just show ToString()
            handler.PlatformView.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock { Text = item?.ToString() ?? string.Empty });
        }
    }

    public static void MapSelectedItem(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Sync the SelectedItem from MAUI ListView to the platform control
        handler.PlatformView.SelectedItem = listView.SelectedItem;
    }
}
