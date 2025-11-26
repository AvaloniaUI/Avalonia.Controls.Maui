using System;
using System.Collections;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;

namespace Avalonia.Controls.Maui;

/// <summary>
/// A carousel control that displays items in a horizontal or vertical scrollable layout
/// with support for swiping, looping, and transitions.
/// </summary>
public class Carousel : SelectingItemsControl
{
    private const double SwipeThreshold = 80.0;
    private const double VelocityThreshold = 0.5;
    private const double DragStartThreshold = 8.0;

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingCarouselPanel());

    private IScrollable? _scroller;
    private Canvas? _gestureCanvas;
    private ContentControl? _currentItemContainer;
    private ContentControl? _previewItemContainer;
    private Point? _pointerPressPosition;
    private bool _isDragging;
    private int _previewDirection;
    private int _previewIndex = -1;
    private DateTime _pointerPressTime;

    static Carousel()
    {
        SelectionModeProperty.OverrideDefaultValue<Carousel>(SelectionMode.AlwaysSelected);
        ItemsPanelProperty.OverrideDefaultValue<Carousel>(DefaultPanel);
    }

    /// <summary>
    /// Defines the <see cref="PageTransition"/> property.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
        AvaloniaProperty.Register<Carousel, IPageTransition?>(nameof(PageTransition));

    /// <summary>
    /// Defines the <see cref="IsGestureEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGestureEnabledProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsGestureEnabled), defaultValue: true);

    /// <summary>
    /// Defines the <see cref="IsLoopingEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoopingEnabledProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsLoopingEnabled), defaultValue: false);
    
    /// <summary>
    /// Gets or sets the transition to use when moving between pages.
    /// </summary>
    public IPageTransition? PageTransition
    {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gesture-based navigation is enabled.
    /// </summary>
    public bool IsGestureEnabled
    {
        get => GetValue(IsGestureEnabledProperty);
        set => SetValue(IsGestureEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the carousel loops infinitely.
    /// </summary>
    public bool IsLoopingEnabled
    {
        get => GetValue(IsLoopingEnabledProperty);
        set => SetValue(IsLoopingEnabledProperty, value);
    }

    /// <summary>
    /// Gets whether the carousel is currently in horizontal orientation.
    /// Determined by PageTransition axis or Panel orientation.
    /// </summary>
    private bool IsHorizontal
    {
        get
        {
            if (PageTransition is PageSlide slide)
            {
                return slide.Orientation == PageSlide.SlideAxis.Horizontal;
            }
            
            return true;
        }
    }

    /// <summary>
    /// Moves to the next item in the carousel.
    /// </summary>
    public void Next()
    {
        if (ItemCount == 0) return;

        if (IsLoopingEnabled)
        {
            SelectedIndex = (SelectedIndex + 1) % ItemCount;
        }
        else if (SelectedIndex < ItemCount - 1)
        {
            ++SelectedIndex;
        }
    }

    /// <summary>
    /// Moves to the previous item in the carousel.
    /// </summary>
    public void Previous()
    {
        if (ItemCount == 0) return;

        if (IsLoopingEnabled)
        {
            SelectedIndex = (SelectedIndex - 1 + ItemCount) % ItemCount;
        }
        else if (SelectedIndex > 0)
        {
            --SelectedIndex;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);

        if (_scroller is not null)
        {
            // Keep the logical offset on the X axis for both orientations so the panel
            // (which reads offset.X) realizes the correct item.
            _scroller.Offset = new Vector(SelectedIndex, 0);
        }

        return result;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_gestureCanvas != null)
        {
            _gestureCanvas.PointerPressed -= OnPointerPressed;
            _gestureCanvas.PointerMoved -= OnPointerMoved;
            _gestureCanvas.PointerReleased -= OnPointerReleased;
            _gestureCanvas.PointerCaptureLost -= OnPointerCaptureLost;
        }

        _scroller = e.NameScope.Find<IScrollable>("PART_ScrollViewer");
        _gestureCanvas = e.NameScope.Find<Canvas>("PART_GestureCanvas");

        if (_gestureCanvas != null)
        {
            _gestureCanvas.PointerPressed += OnPointerPressed;
            _gestureCanvas.PointerMoved += OnPointerMoved;
            _gestureCanvas.PointerReleased += OnPointerReleased;
            _gestureCanvas.PointerCaptureLost += OnPointerCaptureLost;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedIndexProperty && _scroller is not null)
        {
            var value = change.GetNewValue<int>();
            // Keep the logical offset on the X axis for both orientations so the panel
            // (which reads offset.X) realizes the correct item.
            _scroller.Offset = new Vector(value, 0);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsGestureEnabled || ItemCount == 0) return;

        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _pointerPressPosition = e.GetPosition(_gestureCanvas);
            _pointerPressTime = DateTime.Now;
            _isDragging = false;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsGestureEnabled || _pointerPressPosition == null || _gestureCanvas == null || ItemCount == 0)
            return;

        var currentPosition = e.GetPosition(_gestureCanvas);
        var deltaX = currentPosition.X - _pointerPressPosition.Value.X;
        var deltaY = currentPosition.Y - _pointerPressPosition.Value.Y;

        var primaryDelta = IsHorizontal ? deltaX : deltaY;
        var secondaryDelta = IsHorizontal ? deltaY : deltaX;
        
        if (!_isDragging && Math.Abs(primaryDelta) > DragStartThreshold &&
            Math.Abs(primaryDelta) > Math.Abs(secondaryDelta))
        {
            _isDragging = true;
            e.Pointer.Capture(_gestureCanvas);

            int previewIndex = GetPreviewIndex(primaryDelta);

            if (previewIndex >= 0 && previewIndex < ItemCount)
            {
                CreateGestureContainers(previewIndex);
            }
        }

        if (_isDragging)
        {
            UpdateGesturePositions(primaryDelta);
            e.Handled = true;
        }
    }

    private int GetPreviewIndex(double delta)
    {
        int previewIndex = -1;

        // Horizontal: Delta < 0 (Swipe Left) = Next
        // Vertical: Delta < 0 (Swipe Up) = Next
        if (delta < 0)
        {
            _previewDirection = 1; 
            if (IsLoopingEnabled)
            {
                previewIndex = (SelectedIndex + 1) % ItemCount;
            }
            else if (SelectedIndex < ItemCount - 1)
            {
                previewIndex = SelectedIndex + 1;
            }
        }
        // Horizontal: Delta > 0 (Swipe Right) = Prev
        // Vertical: Delta > 0 (Swipe Down) = Prev
        else if (delta > 0)
        {
            _previewDirection = -1;
            if (IsLoopingEnabled)
            {
                previewIndex = (SelectedIndex - 1 + ItemCount) % ItemCount;
            }
            else if (SelectedIndex > 0)
            {
                previewIndex = SelectedIndex - 1;
            }
        }

        return previewIndex;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        CompleteGesture(e.GetPosition(_gestureCanvas));
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        CompleteGesture(null);
    }

    private void CompleteGesture(Point? releasePosition)
    {
        if (!IsGestureEnabled || _pointerPressPosition == null || !_isDragging || _gestureCanvas == null)
        {
            CleanupGesture();
            return;
        }

        var currentOffset = _currentItemContainer != null
            ? (IsHorizontal ? Canvas.GetLeft(_currentItemContainer) : Canvas.GetTop(_currentItemContainer))
            : 0;

        var elapsed = (DateTime.Now - _pointerPressTime).TotalMilliseconds;
        var velocity = Math.Abs(currentOffset) / Math.Max(elapsed, 1);

        bool shouldTransition = Math.Abs(currentOffset) > SwipeThreshold || velocity > VelocityThreshold;

        if (shouldTransition && _previewItemContainer != null && _gestureCanvas != null && _previewIndex >= 0)
        {
            var extent = IsHorizontal ? _gestureCanvas.Bounds.Width : _gestureCanvas.Bounds.Height;
            var remaining = Math.Max(0, extent - Math.Abs(currentOffset));
            var fraction = extent > 0 ? remaining / extent : 1.0;

            var originalTransition = PageTransition;
            if (originalTransition is PageSlide slide)
            {
                var scaledDuration = TimeSpan.FromMilliseconds(Math.Max(60.0, slide.Duration.TotalMilliseconds * fraction));
                PageTransition = new PageSlide(scaledDuration, slide.Orientation);
            }

            if (_previewDirection > 0)
            {
                Next();
            }
            else if (_previewDirection < 0)
            {
                Previous();
            }

            PageTransition = originalTransition;
            CleanupGesture();
        }
        else
        {
            CleanupGesture();
        }

        _pointerPressPosition = null;
        _isDragging = false;
    }

    private void CreateGestureContainers(int previewIndex)
    {
        if (_gestureCanvas == null || Items == null) return;

        var width = _gestureCanvas.Bounds.Width;
        var height = _gestureCanvas.Bounds.Height;

        if (width <= 0 || height <= 0) return;
        
        var itemsList = Items as IList;
        if (itemsList == null || SelectedIndex >= itemsList.Count) return;

        var currentItem = itemsList[SelectedIndex];
        _currentItemContainer = CreateItemContainer(currentItem, width, height);
        Canvas.SetLeft(_currentItemContainer, 0);
        Canvas.SetTop(_currentItemContainer, 0);
        
        // Add Current Container first (bottom of stack)
        _gestureCanvas.Children.Add(_currentItemContainer);

        var previewItem = itemsList[previewIndex];
        _previewIndex = previewIndex;
        _previewItemContainer = CreateItemContainer(previewItem, width, height);

        if (IsHorizontal)
        {
            var initialX = _previewDirection > 0 ? width : -width;
            Canvas.SetLeft(_previewItemContainer, initialX);
            Canvas.SetTop(_previewItemContainer, 0);
        }
        else
        {
            var initialY = _previewDirection > 0 ? height : -height;
            Canvas.SetLeft(_previewItemContainer, 0);
            Canvas.SetTop(_previewItemContainer, initialY);
        }

        // Add Preview Container last (top of stack) so it slides OVER/WITH the current one
        _gestureCanvas.Children.Add(_previewItemContainer);
    }

    private ContentControl CreateItemContainer(object? item, double width, double height)
    {
        var container = new ContentControl
        {
            Width = width,
            Height = height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            // Assign ContentTemplate so Avalonia realizes the DataTemplate properly
            ContentTemplate = ItemTemplate,
            Content = item 
        };

        if (ItemTemplate == null && item != null)
        {
             container.Content = new TextBlock
             {
                 Text = item.ToString(),
                 HorizontalAlignment = HorizontalAlignment.Center,
                 VerticalAlignment = VerticalAlignment.Center
             };
        }

        return container;
    }

    private void UpdateGesturePositions(double delta)
    {
        if (_gestureCanvas == null) return;

        if (IsHorizontal)
        {
            if (_currentItemContainer != null)
            {
                Canvas.SetLeft(_currentItemContainer, delta);
            }

            if (_previewItemContainer != null)
            {
                var initialX = _previewDirection > 0 ? _gestureCanvas.Bounds.Width : -_gestureCanvas.Bounds.Width;
                Canvas.SetLeft(_previewItemContainer, initialX + delta);
            }
        }
        else
        {
            if (_currentItemContainer != null)
            {
                Canvas.SetTop(_currentItemContainer, delta);
            }

            if (_previewItemContainer != null)
            {
                var initialY = _previewDirection > 0 ? _gestureCanvas.Bounds.Height : -_gestureCanvas.Bounds.Height;
                Canvas.SetTop(_previewItemContainer, initialY + delta);
            }
        }
    }

    private void CleanupGesture()
    {
        _gestureCanvas?.Children.Clear();
        _currentItemContainer = null;
        _previewItemContainer = null;
        _pointerPressPosition = null;
        _isDragging = false;
        _previewIndex = -1;
    }
}
