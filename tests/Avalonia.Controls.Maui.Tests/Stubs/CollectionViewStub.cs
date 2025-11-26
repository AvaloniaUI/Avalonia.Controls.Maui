using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections;
using MauiGraphics = Microsoft.Maui.Graphics;
using MauiScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;
using MauiSelectionMode = Microsoft.Maui.Controls.SelectionMode;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of CollectionView for testing CollectionViewHandler.
/// Implements the MAUI interfaces that CollectionView inherits from.
/// </summary>
public class CollectionViewStub : StubBase
{
    private IEnumerable? _itemsSource;
    private DataTemplate? _itemTemplate;
    private object? _emptyView;
    private DataTemplate? _emptyViewTemplate;
    private MauiScrollBarVisibility _horizontalScrollBarVisibility = MauiScrollBarVisibility.Default;
    private MauiScrollBarVisibility _verticalScrollBarVisibility = MauiScrollBarVisibility.Default;
    private IItemsLayout? _itemsLayout;
    private bool _isGrouped;
    private DataTemplate? _groupHeaderTemplate;
    private DataTemplate? _groupFooterTemplate;
    private object? _selectedItem;
    private IList<object>? _selectedItems;
    private MauiSelectionMode _selectionMode = MauiSelectionMode.None;

    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set => SetProperty(ref _itemsSource, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => _itemTemplate;
        set => SetProperty(ref _itemTemplate, value);
    }

    public object? EmptyView
    {
        get => _emptyView;
        set => SetProperty(ref _emptyView, value);
    }

    public DataTemplate? EmptyViewTemplate
    {
        get => _emptyViewTemplate;
        set => SetProperty(ref _emptyViewTemplate, value);
    }

    public MauiScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => _horizontalScrollBarVisibility;
        set => SetProperty(ref _horizontalScrollBarVisibility, value);
    }

    public MauiScrollBarVisibility VerticalScrollBarVisibility
    {
        get => _verticalScrollBarVisibility;
        set => SetProperty(ref _verticalScrollBarVisibility, value);
    }

    public IItemsLayout? ItemsLayout
    {
        get => _itemsLayout;
        set => SetProperty(ref _itemsLayout, value);
    }

    public bool IsGrouped
    {
        get => _isGrouped;
        set => SetProperty(ref _isGrouped, value);
    }

    public DataTemplate? GroupHeaderTemplate
    {
        get => _groupHeaderTemplate;
        set => SetProperty(ref _groupHeaderTemplate, value);
    }

    public DataTemplate? GroupFooterTemplate
    {
        get => _groupFooterTemplate;
        set => SetProperty(ref _groupFooterTemplate, value);
    }

    public object? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public IList<object>? SelectedItems
    {
        get => _selectedItems;
        set => SetProperty(ref _selectedItems, value);
    }

    public MauiSelectionMode SelectionMode
    {
        get => _selectionMode;
        set => SetProperty(ref _selectionMode, value);
    }

    // Event counters for testing
    public int SelectionChangedCount { get; private set; }

    // Simulates selection change
    public void TriggerSelectionChanged()
    {
        SelectionChangedCount++;
    }
}
