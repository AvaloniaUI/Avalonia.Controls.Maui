using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Services;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Renders MAUI <see cref="ToolbarItem"/> collections as buttons and overflow menu items,
/// suitable for use as a TopCommandBar on an Avalonia ContentPage.
/// </summary>
public class ToolbarCommandBar : StackPanel
{
    private readonly IList<ToolbarItem> _items;
    private readonly ContextMenu _overflowMenu;
    private readonly Button _overflowButton;

    /// <summary>
    /// Initializes a new instance of <see cref="ToolbarCommandBar"/> from a collection of toolbar items.
    /// </summary>
    /// <param name="items">The MAUI toolbar items to render.</param>
    public ToolbarCommandBar(IList<ToolbarItem> items)
    {
        _items = items;
        Orientation = Orientation.Horizontal;
        VerticalAlignment = VerticalAlignment.Stretch;
        Spacing = 8;
        Margin = new Thickness(0, 0, 8, 0);

        _overflowMenu = new ContextMenu();

        _overflowButton = new Button
        {
            Content = "...",
            Width = 30,
            Height = 30,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            IsVisible = false
        };
        _overflowButton.Click += (_, _) => _overflowMenu.Open(_overflowButton);
        Children.Add(_overflowButton);

        if (items is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged += OnItemsCollectionChanged;
        }

        Rebuild();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Rebuild();
    }

    private void Rebuild()
    {
        // Unsubscribe from previous items
        foreach (var child in Children)
        {
            if (child is Button btn && btn.DataContext is ToolbarItem oldItem)
            {
                btn.Click -= OnToolbarItemClicked;
                oldItem.PropertyChanged -= OnToolbarItemPropertyChanged;
            }
        }

        foreach (var item in _overflowMenu.Items)
        {
            if (item is MenuItem menuItem && menuItem.DataContext is ToolbarItem oldItem)
            {
                menuItem.Click -= OnToolbarItemClicked;
                oldItem.PropertyChanged -= OnToolbarItemPropertyChanged;
            }
        }

        Children.Clear();
        Children.Add(_overflowButton);
        _overflowMenu.Items.Clear();

        var sortedItems = _items.OrderBy(i => i.Priority).ToList();

        foreach (var item in sortedItems)
        {
            if (item.Order == ToolbarItemOrder.Secondary)
            {
                var menuItem = new MenuItem
                {
                    Header = item.Text,
                    IsEnabled = item.IsEnabled,
                    DataContext = item
                };
                menuItem.Click += OnToolbarItemClicked;
                item.PropertyChanged += OnToolbarItemPropertyChanged;
                _overflowMenu.Items.Add(menuItem);
            }
            else
            {
                var button = new Button
                {
                    Content = item.Text,
                    IsEnabled = item.IsEnabled,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(8, 0),
                    DataContext = item
                };
                button.Click += OnToolbarItemClicked;
                item.PropertyChanged += OnToolbarItemPropertyChanged;

                if (item.IconImageSource != null)
                {
                    UpdateToolbarItemIcon(button, item);
                }

                // Add before the overflow button
                Children.Insert(Children.Count - 1, button);
            }
        }

        _overflowButton.IsVisible = _overflowMenu.Items.Count > 0;
    }

    private void OnToolbarItemClicked(object? sender, Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ToolbarItem item && item is IMenuItemController controller)
        {
            controller.Activate();
        }
    }

    private void OnToolbarItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ToolbarItem item)
            return;

        if (e.PropertyName == nameof(ToolbarItem.Order) ||
            e.PropertyName == nameof(ToolbarItem.Priority))
        {
            Rebuild();
            return;
        }

        // Find the button for this item
        Button? button = null;
        foreach (var child in Children)
        {
            if (child is Button btn && btn.DataContext == item)
            {
                button = btn;
                break;
            }
        }

        if (button != null)
        {
            if (e.PropertyName == ToolbarItem.TextProperty.PropertyName)
                button.Content = item.Text;
            else if (e.PropertyName == ToolbarItem.IsEnabledProperty.PropertyName)
                button.IsEnabled = item.IsEnabled;
            else if (e.PropertyName == ToolbarItem.IconImageSourceProperty.PropertyName)
                UpdateToolbarItemIcon(button, item);
        }
        else
        {
            foreach (var child in _overflowMenu.Items)
            {
                if (child is MenuItem menuItem && menuItem.DataContext == item)
                {
                    if (e.PropertyName == ToolbarItem.TextProperty.PropertyName)
                        menuItem.Header = item.Text;
                    else if (e.PropertyName == ToolbarItem.IsEnabledProperty.PropertyName)
                        menuItem.IsEnabled = item.IsEnabled;
                    break;
                }
            }
        }
    }

    private static async void UpdateToolbarItemIcon(Button button, ToolbarItem item)
    {
        if (item.IconImageSource == null)
        {
            button.Content = item.Text;
            return;
        }

        try
        {
            var handler = item.Parent?.Handler ?? (item as Element)?.Parent?.Handler;
            var mauiContext = handler?.MauiContext;
            if (mauiContext == null)
                return;

            var imageSourceServiceProvider = mauiContext.Services.GetService<IImageSourceServiceProvider>();
            var service = imageSourceServiceProvider?.GetImageSourceService(item.IconImageSource.GetType());

            if (service is IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(item.IconImageSource, 1.0f);
                if (result?.Value is Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    var image = new Image
                    {
                        Source = bitmap,
                        Width = 20,
                        Height = 20
                    };
                    button.Content = image;
                    ToolTip.SetTip(button, item.Text);
                }
                else
                {
                    button.Content = item.Text;
                }
            }
            else
            {
                button.Content = item.Text;
            }
        }
        catch
        {
            button.Content = item.Text;
        }
    }
}
