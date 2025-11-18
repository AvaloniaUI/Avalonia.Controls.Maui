using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls;

public class MauiIndicatorView : TemplatedControl
{
    private StackPanel? _indicatorPanel;

    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<MauiIndicatorView, int>(nameof(Count), defaultValue: 0);

    public static readonly StyledProperty<int> PositionProperty =
        AvaloniaProperty.Register<MauiIndicatorView, int>(nameof(Position), defaultValue: 0);

    public static readonly StyledProperty<double> IndicatorSizeProperty =
        AvaloniaProperty.Register<MauiIndicatorView, double>(nameof(IndicatorSize), defaultValue: 6.0);

    public static readonly StyledProperty<int> MaximumVisibleProperty =
        AvaloniaProperty.Register<MauiIndicatorView, int>(nameof(MaximumVisible), defaultValue: int.MaxValue);

    public static readonly StyledProperty<bool> HideSingleProperty =
        AvaloniaProperty.Register<MauiIndicatorView, bool>(nameof(HideSingle), defaultValue: true);

    public static readonly StyledProperty<IBrush?> IndicatorColorProperty =
        AvaloniaProperty.Register<MauiIndicatorView, IBrush?>(nameof(IndicatorColor), defaultValue: new SolidColorBrush(Color.FromRgb(211, 211, 211))); // LightGray

    public static readonly StyledProperty<IBrush?> SelectedIndicatorColorProperty =
        AvaloniaProperty.Register<MauiIndicatorView, IBrush?>(nameof(SelectedIndicatorColor), defaultValue: new SolidColorBrush(Colors.Black));

    public static readonly StyledProperty<bool> IsCircleShapeProperty =
        AvaloniaProperty.Register<MauiIndicatorView, bool>(nameof(IsCircleShape), defaultValue: true);

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    public int Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    public int MaximumVisible
    {
        get => GetValue(MaximumVisibleProperty);
        set => SetValue(MaximumVisibleProperty, value);
    }

    public bool HideSingle
    {
        get => GetValue(HideSingleProperty);
        set => SetValue(HideSingleProperty, value);
    }

    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public IBrush? SelectedIndicatorColor
    {
        get => GetValue(SelectedIndicatorColorProperty);
        set => SetValue(SelectedIndicatorColorProperty, value);
    }

    public bool IsCircleShape
    {
        get => GetValue(IsCircleShapeProperty);
        set => SetValue(IsCircleShapeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicatorPanel = e.NameScope.Find<StackPanel>("PART_IndicatorPanel");
        UpdateIndicators();
    }

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
