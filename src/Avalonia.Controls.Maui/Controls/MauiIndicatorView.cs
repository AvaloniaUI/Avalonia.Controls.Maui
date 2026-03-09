using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Collections;
using System.Collections.Specialized;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// An Avalonia control that displays indicators representing items and their current position.
/// </summary>
/// <remarks>
/// This control is used as the platform view for the .NET MAUI IndicatorView control.
/// It displays a series of dots or squares that represent items, with one indicator
/// highlighted to show the current position.
/// </remarks>
public class MauiIndicatorView : TemplatedControl
{
    private StackPanel? _indicatorPanel;
    private IEnumerable? _itemsSource;

    /// <summary>
    /// Defines the <see cref="Count"/> property.
    /// </summary>
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<MauiIndicatorView, int>(nameof(Count), defaultValue: 0);

    /// <summary>
    /// Defines the <see cref="Position"/> property.
    /// </summary>
    public static readonly StyledProperty<int> PositionProperty =
        AvaloniaProperty.Register<MauiIndicatorView, int>(nameof(Position), defaultValue: 0);

    /// <summary>
    /// Defines the <see cref="IndicatorSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorSizeProperty =
        AvaloniaProperty.Register<MauiIndicatorView, double>(nameof(IndicatorSize), defaultValue: 6.0);

    /// <summary>
    /// Defines the <see cref="MaximumVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaximumVisibleProperty =
        AvaloniaProperty.Register<MauiIndicatorView, int>(nameof(MaximumVisible), defaultValue: int.MaxValue);

    /// <summary>
    /// Defines the <see cref="HideSingle"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HideSingleProperty =
        AvaloniaProperty.Register<MauiIndicatorView, bool>(nameof(HideSingle), defaultValue: true);

    /// <summary>
    /// Defines the <see cref="IndicatorColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorColorProperty =
        AvaloniaProperty.Register<MauiIndicatorView, IBrush?>(nameof(IndicatorColor), 
            defaultValue: new SolidColorBrush(Color.FromRgb(211, 211, 211))); // LightGray

    /// <summary>
    /// Defines the <see cref="SelectedIndicatorColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectedIndicatorColorProperty =
        AvaloniaProperty.Register<MauiIndicatorView, IBrush?>(nameof(SelectedIndicatorColor), 
            defaultValue: new SolidColorBrush(Colors.Black));

    /// <summary>
    /// Defines the <see cref="IsCircleShape"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCircleShapeProperty =
        AvaloniaProperty.Register<MauiIndicatorView, bool>(nameof(IsCircleShape), defaultValue: true);

    /// <summary>
    /// Defines the <see cref="IndicatorTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<Func<int, bool, Control>?> IndicatorTemplateProperty =
        AvaloniaProperty.Register<MauiIndicatorView, Func<int, bool, Control>?>(nameof(IndicatorTemplate));

    /// <summary>
    /// Gets or sets the number of indicators to display.
    /// </summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected indicator position (zero-based index).
    /// </summary>
    public int Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of each indicator in device-independent units.
    /// </summary>
    /// <remarks>
    /// The default value is 6.0.
    /// </remarks>
    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of visible indicators.
    /// </summary>
    /// <remarks>
    /// When Count exceeds MaximumVisible, only MaximumVisible indicators are shown.
    /// The default value is int.MaxValue.
    /// </remarks>
    public int MaximumVisible
    {
        get => GetValue(MaximumVisibleProperty);
        set => SetValue(MaximumVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to hide the indicators when only one exists.
    /// </summary>
    /// <remarks>
    /// The default value is true.
    /// </remarks>
    public bool HideSingle
    {
        get => GetValue(HideSingleProperty);
        set => SetValue(HideSingleProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to fill unselected indicators.
    /// </summary>
    /// <remarks>
    /// The default value is LightGray.
    /// </remarks>
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to fill the selected indicator.
    /// </summary>
    /// <remarks>
    /// The default value is Black.
    /// </remarks>
    public IBrush? SelectedIndicatorColor
    {
        get => GetValue(SelectedIndicatorColorProperty);
        set => SetValue(SelectedIndicatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether indicators are circles (true) or squares (false).
    /// </summary>
    /// <remarks>
    /// The default value is true (circles).
    /// </remarks>
    public bool IsCircleShape
    {
        get => GetValue(IsCircleShapeProperty);
        set => SetValue(IsCircleShapeProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom template function for creating indicator visuals.
    /// </summary>
    /// <remarks>
    /// The function receives the index and a boolean indicating if it's selected,
    /// and returns a Control to use as the indicator. When set, this overrides
    /// the default indicator rendering (IndicatorSize, IndicatorColor, etc.).
    /// </remarks>
    public Func<int, bool, Control>? IndicatorTemplate
    {
        get => GetValue(IndicatorTemplateProperty);
        set => SetValue(IndicatorTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of items that the indicators represent.
    /// </summary>
    /// <remarks>
    /// When set, the Count property is automatically updated to reflect the number of items.
    /// Supports observable collections for dynamic updates.
    /// </remarks>
    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (_itemsSource == value)
                return;

            // Unsubscribe from old collection
            if (_itemsSource is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            _itemsSource = value;

            // Subscribe to new collection
            if (_itemsSource is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnItemsSourceCollectionChanged;
            }

            UpdateCountFromItemsSource();
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCountFromItemsSource();
    }

    private void UpdateCountFromItemsSource()
    {
        if (_itemsSource == null)
        {
            Count = 0;
            return;
        }

        int count = 0;
        foreach (var _ in _itemsSource)
        {
            count++;
        }
        Count = count;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicatorPanel = e.NameScope.Find<StackPanel>("PART_IndicatorPanel");
        UpdateIndicators();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CountProperty ||
            change.Property == PositionProperty ||
            change.Property == IndicatorSizeProperty ||
            change.Property == MaximumVisibleProperty ||
            change.Property == HideSingleProperty ||
            change.Property == IndicatorColorProperty ||
            change.Property == SelectedIndicatorColorProperty ||
            change.Property == IsCircleShapeProperty ||
            change.Property == IndicatorTemplateProperty)
        {
            UpdateIndicators();
        }
    }

    /// <summary>
    /// Updates the visual indicators based on current property values.
    /// </summary>
    public void UpdateIndicators()
    {
        if (_indicatorPanel == null)
            return;

        _indicatorPanel.Children.Clear();

        // Handle edge cases
        var count = Math.Max(0, Count);
        var position = Math.Max(0, Math.Min(Position, count - 1));

        // Hide if there's only one item and HideSingle is true
        if (count <= 1 && HideSingle)
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;

        var visibleCount = Math.Min(count, Math.Max(0, MaximumVisible));

        for (int i = 0; i < visibleCount; i++)
        {
            var indicator = CreateIndicator(i, i == position);
            _indicatorPanel.Children.Add(indicator);
        }
    }

    private Control CreateIndicator(int index, bool isSelected)
    {
        Control indicator;
        
        // Use custom template if provided
        if (IndicatorTemplate != null)
        {
            try
            {
                indicator = IndicatorTemplate(index, isSelected);
            }
            catch
            {
                // Fall back to default rendering if template fails
                indicator = CreateDefaultIndicator(isSelected);
            }
        }
        else
        {
            indicator = CreateDefaultIndicator(isSelected);
        }

        // Add click handler to select this indicator
        indicator.PointerPressed += (sender, e) =>
        {
            Position = index;
            e.Handled = true;
        };
        
        // Make it clickable
        indicator.Cursor = new Input.Cursor(Input.StandardCursorType.Hand);

        return indicator;
    }

    private Control CreateDefaultIndicator(bool isSelected)
    {
        var brush = isSelected ? SelectedIndicatorColor : IndicatorColor;
        var size = Math.Max(0, IndicatorSize);

        if (IsCircleShape)
        {
            return new Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush,
                Margin = new Thickness(2)
            };
        }
        else
        {
            return new Rectangle
            {
                Width = size,
                Height = size,
                Fill = brush,
                Margin = new Thickness(2)
            };
        }
    }
}