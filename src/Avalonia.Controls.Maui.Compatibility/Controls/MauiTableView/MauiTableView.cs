using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Media;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Avalonia platform control for MAUI TableView.
/// Handles the rendering and interaction of a TableView within the Avalonia visual tree.
/// </summary>
public class MauiTableView : MauiView
{
    private readonly ItemsControl _itemsControl;
    private Microsoft.Maui.Controls.TableView? _tableView;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiTableView"/> class.
    /// </summary>
    public MauiTableView()
    {
        _itemsControl = new ItemsControl
        {
            // Use VirtualizingStackPanel
            ItemsPanel = new FuncTemplate<Panel?>(() => new VirtualizingStackPanel())
        };

        // Custom template to ensure ScrollViewer wraps ItemsPresenter for virtualization
        _itemsControl.Template = new FuncControlTemplate<ItemsControl>((parent, scope) =>
        {
            return new ScrollViewer
            {
                Name = "PART_ScrollViewer",
                AllowAutoHide = false,
                HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = Primitives.ScrollBarVisibility.Auto,
                Content = new ItemsPresenter
                {
                    Name = "PART_ItemsPresenter",
                    [~ItemsPresenter.ItemsPanelProperty] = parent[~ItemsControl.ItemsPanelProperty]
                }.RegisterInNameScope(scope)
            }.RegisterInNameScope(scope);
        });

        _itemsControl.ItemTemplate = CreateDefaultTemplate();

        Children.Add(_itemsControl);
    }

    /// <summary>
    /// Gets or sets the underlying MAUI TableView model.
    /// </summary>
    public Microsoft.Maui.Controls.TableView? TableView
    {
        get => _tableView;
        set
        {
            if (_tableView == value) return;

            if (_tableView != null)
            {
                _tableView.ModelChanged -= OnModelChanged;
            }

            _tableView = value;

            if (_tableView != null)
            {
                _tableView.ModelChanged += OnModelChanged;
                UpdateItems();
            }
            else
            {
                _itemsControl.ItemsSource = null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the MAUI context used for creating cell platform views.
    /// </summary>
    public IMauiContext? MauiContext { get; set; }

    /// <summary>
    /// Defines the <see cref="RowHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<int> RowHeightProperty =
        AvaloniaProperty.Register<MauiTableView, int>(nameof(RowHeight), -1);

    /// <summary>
    /// Gets or sets the uniform row height for cells when <see cref="HasUnevenRows"/> is false.
    /// </summary>
    public int RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HasUnevenRows"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasUnevenRowsProperty =
         AvaloniaProperty.Register<MauiTableView, bool>(nameof(HasUnevenRows), false);

    /// <summary>
    /// Gets or sets a value indicating whether rows can have variable heights.
    /// Default is true, allowing cells to size themselves.
    /// </summary>
    public bool HasUnevenRows
    {
        get => GetValue(HasUnevenRowsProperty);
        set => SetValue(HasUnevenRowsProperty, value);
    }

    /// <summary>
    /// Gets or sets the intent of the table (Data, Form, Settings, Menu), which may affect styling.
    /// </summary>
    public Microsoft.Maui.Controls.TableIntent Intent
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                // Trigger visual update if intent affects rendering
            }
        }
    } = Microsoft.Maui.Controls.TableIntent.Data;

    private void OnModelChanged(object? sender, EventArgs e)
    {
        UpdateItems();
    }

    private void UpdateItems()
    {
        if (_tableView?.Root == null)
        {
            _itemsControl.ItemsSource = null;
            return;
        }
        
        int estimatedCapacity = 0;
        foreach (var section in _tableView.Root)
        {
            estimatedCapacity++; // For section header
            estimatedCapacity += section.Count;
        }

        // Flatten the TableRoot/TableSection structure into a single list
        var items = new List<object>(estimatedCapacity);

        foreach (var section in _tableView.Root)
        {
            // Add section header if present
            if (!string.IsNullOrEmpty(section.Title))
            {
                items.Add(new TableSectionHeader 
                { 
                    Title = section.Title,
                    TextColor = section.TextColor.ToAvaloniaBrush()
                });
            }

            // Add all cells in the section
            foreach (var cell in section)
            {
                items.Add(cell);
            }
        }

        _itemsControl.ItemsSource = items;
    }

    private IDataTemplate CreateDefaultTemplate()
    {
        return new FuncDataTemplate<object>((item, _) =>
        {
            if (item is TableSectionHeader header)
            {
                var textBlock = new TextBlock
                {
                    Text = header.Title,
                    FontWeight = Media.FontWeight.Bold,
                    FontSize = 14,
                    Margin = new Thickness(16, 25, 16, 5),
                    Foreground = Brushes.Gray
                };

                if (header.TextColor != null)
                {
                    textBlock.Foreground = header.TextColor;
                }

                return textBlock;
            }

            if (item is Microsoft.Maui.Controls.Cell cell && MauiContext != null)
            {
                // Cells in MAUI are special and don't always behave like Views for ToPlatform
                // We need to ensure we use the correct method to create the renderer/handler
                var handler = MauiContext.Handlers.GetHandler(cell.GetType());
                if (handler != null)
                {
                    handler.SetMauiContext(MauiContext);
                    cell.Handler = handler;
                    handler.SetVirtualView(cell);
                    
                    var platformView = handler.PlatformView;
                    if (platformView is Control control)
                    {
                        control.HorizontalAlignment = HorizontalAlignment.Stretch;

                        // Use a Grid with two rows: content and separator
                        var grid = new Grid
                        {
                            RowDefinitions = new RowDefinitions("*,Auto")
                        };
                        
                        control.HorizontalAlignment = HorizontalAlignment.Stretch;
                        Grid.SetRow(control, 0);
                        grid.Children.Add(control);

                        // Add inset separator in Row 1
                        var separator = new Shapes.Rectangle
                        {
                            Height = 1,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Fill = Media.Brushes.LightGray,
                            Margin = new Thickness(16, 0, 0, 0)
                        };
                        Grid.SetRow(separator, 1);
                        grid.Children.Add(separator);

                        // Height logic following MAUI TableView behavior:
                        // - Cell.Height defaults to -1 (auto-size)
                        // - RowHeight defaults to -1 (auto-size)
                        // - HasUnevenRows defaults to false
                        // - DefaultCellHeight = 40 (per MAUI source)
                        // When HasUnevenRows=true: use Cell.Height if > 0, otherwise auto-size
                        // When HasUnevenRows=false: use RowHeight if > 0, otherwise auto-size (cells have MinHeight)
                        var capturedCell = cell; // Capture for closure
                        var tableView = this; // Capture for closure

                        void UpdateGridHeight()
                        {
                            int rowHeight = tableView.RowHeight;
                            bool hasUnevenRows = tableView.HasUnevenRows;
                            double cellHeight = capturedCell.Height;

                            if (hasUnevenRows)
                            {
                                // When uneven rows allowed, prefer explicit Cell.Height if set
                                grid.Height = cellHeight > 0 ? cellHeight : double.NaN;
                            }
                            else
                            {
                                // When uniform rows required, use RowHeight if explicitly set
                                grid.Height = rowHeight > 0 ? rowHeight : double.NaN;
                            }
                        }

                        // Set initial height
                        UpdateGridHeight();

                        void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
                        {
                            if (e.Property == RowHeightProperty || e.Property == HasUnevenRowsProperty)
                            {
                                UpdateGridHeight();
                            }
                        }

                        tableView.PropertyChanged += OnPropertyChanged;

                        // Clean up subscription when grid is detached
                        grid.DetachedFromVisualTree += (_, _) =>
                        {
                            tableView.PropertyChanged -= OnPropertyChanged;
                        };
                        
                        return grid;
                    }
                }
            }

            return new TextBlock
            {
                Text = item?.ToString() ?? string.Empty,
                Margin = new Thickness(15, 10)
            };
        });
    }

    private class TableSectionHeader
    {
        public string? Title { get; set; }
        public IBrush? TextColor { get; set; }
    }
}