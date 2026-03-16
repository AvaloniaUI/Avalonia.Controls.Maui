using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using PlatformCollectionView = global::Avalonia.Controls.Maui.MauiCollectionView;

namespace BenchmarkApp.Tests;

internal static class BenchmarkUiHelpers
{
    public static async Task WaitForIdleAsync(CancellationToken cancellationToken, int delayMs = 25)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        if (delayMs > 0)
        {
            await Task.Delay(delayMs, cancellationToken);
        }

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);
    }

    public static ObservableCollection<CollectionViewBenchmarkItem> CreateItems(int count, int offset = 0)
    {
        var items = new ObservableCollection<CollectionViewBenchmarkItem>();

        for (int i = 0; i < count; i++)
        {
            var id = offset + i;
            items.Add(new CollectionViewBenchmarkItem(
                id,
                $"Item {id}",
                $"Detail {id % 10}"));
        }

        return items;
    }

    public static ObservableCollection<CollectionViewBenchmarkGroup> CreateGroups(int groupCount, int itemsPerGroup)
    {
        var groups = new ObservableCollection<CollectionViewBenchmarkGroup>();

        for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            groups.Add(new CollectionViewBenchmarkGroup(
                $"Group {groupIndex + 1}",
                CreateItems(itemsPerGroup, groupIndex * itemsPerGroup)));
        }

        return groups;
    }

    public static CollectionView CreateCollectionView(
        IEnumerable itemsSource,
        bool grouped = false,
        bool useEmptyTemplate = false)
    {
        var collectionView = new CollectionView
        {
            ItemsSource = itemsSource,
            ItemTemplate = CreateCollectionViewItemTemplate(),
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            HeightRequest = 420,
            RemainingItemsThreshold = 2
        };

        if (grouped)
        {
            collectionView.IsGrouped = true;
            collectionView.GroupHeaderTemplate = CreateCollectionViewGroupHeaderTemplate();
        }

        if (useEmptyTemplate)
        {
            collectionView.EmptyView = new CollectionViewBenchmarkItem(-1, "Empty", "No data");
            collectionView.EmptyViewTemplate = CreateCollectionViewEmptyViewTemplate();
        }
        else
        {
            collectionView.EmptyView = "No items";
        }

        return collectionView;
    }

    public static int GetPlatformItemCount(CollectionView collectionView)
    {
        return (collectionView.Handler?.PlatformView as PlatformCollectionView)?.GetItemsControl()?.ItemCount ?? 0;
    }

    public static void DisconnectElementTree(IView? view)
    {
        switch (view)
        {
            case Layout layout:
                foreach (var child in layout.Children.ToList())
                {
                    DisconnectElementTree(child);
                }
                break;
            case ScrollView scrollView when scrollView.Content is IView content:
                DisconnectElementTree(content);
                break;
            case ContentView contentView when contentView.Content is IView content:
                DisconnectElementTree(content);
                break;
            case Border border when border.Content is IView content:
                DisconnectElementTree(content);
                break;
        }

        if (view is VisualElement visualElement)
        {
            visualElement.Handler?.DisconnectHandler();
        }
    }

    private static DataTemplate CreateCollectionViewItemTemplate()
    {
        return new DataTemplate(() =>
        {
            var titleLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };
            titleLabel.SetBinding(Label.TextProperty, nameof(CollectionViewBenchmarkItem.Title));

            var detailLabel = new Label
            {
                FontSize = 12,
                Opacity = 0.8
            };
            detailLabel.SetBinding(Label.TextProperty, nameof(CollectionViewBenchmarkItem.Detail));

            return new VerticalStackLayout
            {
                Spacing = 2,
                Padding = new Thickness(12, 8),
                Children =
                {
                    titleLabel,
                    detailLabel
                }
            };
        });
    }

    private static DataTemplate CreateCollectionViewGroupHeaderTemplate()
    {
        return new DataTemplate(() =>
        {
            var label = new Label
            {
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Colors.LightGray,
                Padding = new Thickness(12, 6)
            };
            label.SetBinding(Label.TextProperty, nameof(CollectionViewBenchmarkGroup.Title));
            return label;
        });
    }

    private static DataTemplate CreateCollectionViewEmptyViewTemplate()
    {
        return new DataTemplate(() =>
        {
            var titleLabel = new Label
            {
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            };
            titleLabel.SetBinding(Label.TextProperty, nameof(CollectionViewBenchmarkItem.Title));

            var detailLabel = new Label
            {
                FontSize = 13,
                Opacity = 0.75,
                HorizontalTextAlignment = TextAlignment.Center
            };
            detailLabel.SetBinding(Label.TextProperty, nameof(CollectionViewBenchmarkItem.Detail));

            return new VerticalStackLayout
            {
                Padding = new Thickness(24),
                Spacing = 6,
                Children =
                {
                    titleLabel,
                    detailLabel
                }
            };
        });
    }
}

internal sealed record CollectionViewBenchmarkItem(int Id, string Title, string Detail);

internal sealed class CollectionViewBenchmarkGroup : ObservableCollection<CollectionViewBenchmarkItem>
{
    public CollectionViewBenchmarkGroup(string title, IEnumerable<CollectionViewBenchmarkItem> items)
        : base(items)
    {
        Title = title;
    }

    public string Title { get; }
}
