using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Avalonia templated control that displays a set of position indicators for a carousel view, highlighting the currently selected item.
/// </summary>
public class MauiIndicatorView : TemplatedControl
{
    private StackPanel? _indicatorPanel;

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
        AvaloniaProperty.Register<MauiIndicatorView, IBrush?>(nameof(IndicatorColor), defaultValue: new SolidColorBrush(Color.FromRgb(211, 211, 211))); // LightGray

    /// <summary>
    /// Defines the <see cref="SelectedIndicatorColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectedIndicatorColorProperty =
        AvaloniaProperty.Register<MauiIndicatorView, IBrush?>(nameof(SelectedIndicatorColor), defaultValue: new SolidColorBrush(Colors.Black));

    /// <summary>
    /// Defines the <see cref="IsCircleShape"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCircleShapeProperty =
        AvaloniaProperty.Register<MauiIndicatorView, bool>(nameof(IsCircleShape), defaultValue: true);

    /// <summary>
    /// Gets or sets the total number of indicators to display.
    /// </summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>
    /// Gets or sets the zero-based index of the currently selected indicator.
    /// </summary>
    public int Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the width and height of each indicator element in device-independent pixels.
    /// </summary>
    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of indicators that are visible at one time.
    /// </summary>
    public int MaximumVisible
    {
        get => GetValue(MaximumVisibleProperty);
        set => SetValue(MaximumVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the indicator view is hidden when there is only a single item.
    /// </summary>
    public bool HideSingle
    {
        get => GetValue(HideSingleProperty);
        set => SetValue(HideSingleProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to paint unselected indicators.
    /// </summary>
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to paint the currently selected indicator.
    /// </summary>
    public IBrush? SelectedIndicatorColor
    {
        get => GetValue(SelectedIndicatorColorProperty);
        set => SetValue(SelectedIndicatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether indicators are rendered as circles; when <see langword="false"/>, rectangles are used.
    /// </summary>
    public bool IsCircleShape
    {
        get => GetValue(IsCircleShapeProperty);
        set => SetValue(IsCircleShapeProperty, value);
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
            change.Property == IsCircleShapeProperty)
        {
            UpdateIndicators();
        }
    }

    /// <summary>
    /// Rebuilds the indicator elements in the panel based on the current <see cref="Count"/>, <see cref="Position"/>, and styling properties.
    /// </summary>
    public void UpdateIndicators()
    {
        if (_indicatorPanel == null)
            return;

        _indicatorPanel.Children.Clear();

        // Hide if there's only one item and HideSingle is true
        if (Count <= 1 && HideSingle)
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;

        var visibleCount = Math.Min(Count, MaximumVisible);

        for (int i = 0; i < visibleCount; i++)
        {
            var indicator = CreateIndicator(i == Position);
            _indicatorPanel.Children.Add(indicator);
        }
    }

    private Control CreateIndicator(bool isSelected)
    {
        var brush = isSelected ? SelectedIndicatorColor : IndicatorColor;

        if (IsCircleShape)
        {
            return new Ellipse
            {
                Width = IndicatorSize,
                Height = IndicatorSize,
                Fill = brush,
                Margin = new Thickness(2)
            };
        }
        else
        {
            return new Rectangle
            {
                Width = IndicatorSize,
                Height = IndicatorSize,
                Fill = brush,
                Margin = new Thickness(2)
            };
        }
    }
}
