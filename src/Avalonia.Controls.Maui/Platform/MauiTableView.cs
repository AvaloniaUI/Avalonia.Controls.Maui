using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia platform control for MAUI TableView
/// </summary>
public class MauiTableView : MauiView
{
    private readonly ItemsControl _itemsControl;
    private readonly ScrollViewer _scrollViewer;
    private IList<object>? _flattenedItems;
    private Microsoft.Maui.Controls.TableView? _tableView;

    public MauiTableView()
    {
        _itemsControl = new ItemsControl
        {
            ItemTemplate = CreateDefaultTemplate()
        };

        _scrollViewer = new ScrollViewer
        {
            Content = _itemsControl,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        Children.Add(_scrollViewer);
    }

    public Microsoft.Maui.Controls.TableView? TableView
    {
        get => _tableView;
        set
        {
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
        }
    }

    public IMauiContext? MauiContext { get; set; }

    private void OnModelChanged(object? sender, EventArgs e)
    {
        UpdateItems();
    }

    private void UpdateItems()
    {
        if (_tableView?.Root == null)
        {
            _itemsControl.ItemsSource = null;
            _flattenedItems = null;
            return;
        }

        // Flatten the TableRoot/TableSection structure into a single list
        var items = new List<object>();

        foreach (var section in _tableView.Root)
        {
            // Add section header if it has a title
            if (!string.IsNullOrEmpty(section.Title))
            {
                items.Add(new TableSectionHeader { Title = section.Title });
            }

            // Add all cells in the section
            foreach (var cell in section)
            {
                items.Add(cell);
            }
        }

        _flattenedItems = items;
        _itemsControl.ItemsSource = items;
    }

    private IDataTemplate CreateDefaultTemplate()
    {
        return new FuncDataTemplate<object>((item, _) =>
        {
            if (item is TableSectionHeader header)
            {
                return new TextBlock
                {
                    Text = header.Title,
                    FontWeight = global::Avalonia.Media.FontWeight.Bold,
                    FontSize = 14,
                    Margin = new Thickness(8, 12, 8, 4)
                };
            }

            if (item is Cell cell && MauiContext != null)
            {
                try
                {
                    var platformView = cell.ToPlatform(MauiContext);
                    if (platformView is Control control)
                    {
                        return control;
                    }
                }
                catch (Exception)
                {
                    // TODO: Log error.
                }
            }

            return new TextBlock
            {
                Text = item?.ToString() ?? string.Empty,
                Margin = new Thickness(8, 4)
            };
        });
    }

    private class TableSectionHeader
    {
        public string? Title { get; set; }
    }
}
