using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia content control that implements a MAUI CarouselView with swipe gesture and keyboard navigation support.
/// </summary>
public class MauiCarouselView : ContentControl
{
    private Canvas? _containerCanvas;
    private ContentControl? _currentItemContainer;
    private ContentControl? _previewItemContainer;
    private Point? _pointerPressPosition;
    private bool _isDragging;
    private int _previewDirection; // -1 for previous, 1 for next
    private const double SwipeThreshold = 80; // Threshold to commit to transition
    private const double AnimationDuration = 250; // milliseconds

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MauiCarouselView, IEnumerable?>(nameof(ItemsSource));

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MauiCarouselView, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// Defines the <see cref="SelectedIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<MauiCarouselView, int>(
            nameof(SelectedIndex),
            defaultValue: 0,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="CurrentItem"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> CurrentItemProperty =
        AvaloniaProperty.Register<MauiCarouselView, object?>(
            nameof(CurrentItem),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="Loop"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> LoopProperty =
        AvaloniaProperty.Register<MauiCarouselView, bool>(nameof(Loop), defaultValue: false);

    /// <summary>
    /// Registers property change handlers for <see cref="ItemsSourceProperty"/>, <see cref="ItemTemplateProperty"/>, and <see cref="SelectedIndexProperty"/>.
    /// </summary>
    static MauiCarouselView()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MauiCarouselView>((cv, e) => cv.OnItemsSourceChanged(e));
        ItemTemplateProperty.Changed.AddClassHandler<MauiCarouselView>((cv, e) => cv.UpdateCurrentItem());
        SelectedIndexProperty.Changed.AddClassHandler<MauiCarouselView>((cv, e) => cv.OnSelectedIndexChanged(e));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiCarouselView"/> class and sets up pointer and keyboard event handlers.
    /// </summary>
    public MauiCarouselView()
    {
        // Set a default minimum size to ensure the control gets layout space
        MinHeight = 200;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        _containerCanvas = new Canvas
        {
            ClipToBounds = true
        };

        Content = _containerCanvas;

        // Set up pointer event handlers
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCaptureLost += OnPointerCaptureLost;
        KeyDown += OnKeyDown;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    /// <summary>
    /// Gets or sets the collection of items to display in the carousel.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render each item in the carousel.
    /// </summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the zero-based index of the currently selected item in the carousel.
    /// </summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the data object of the currently displayed item.
    /// </summary>
    public object? CurrentItem
    {
        get => GetValue(CurrentItemProperty);
        set => SetValue(CurrentItemProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the carousel wraps around when reaching the first or last item.
    /// </summary>
    public bool Loop
    {
        get => GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    /// <summary>
    /// Advances the carousel to the next item, wrapping to the first item if <see cref="Loop"/> is enabled.
    /// </summary>
    public void Next()
    {
        var items = GetItemsList();
        if (items == null || items.Count == 0)
            return;

        if (SelectedIndex < items.Count - 1)
        {
            SelectedIndex++;
        }
        else if (Loop)
        {
            SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Moves the carousel to the previous item, wrapping to the last item if <see cref="Loop"/> is enabled.
    /// </summary>
    public void Previous()
    {
        var items = GetItemsList();
        if (items == null || items.Count == 0)
            return;

        if (SelectedIndex > 0)
        {
            SelectedIndex--;
        }
        else if (Loop)
        {
            SelectedIndex = items.Count - 1;
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Update container sizes when bounds change
        if (change.Property == BoundsProperty && _containerCanvas != null)
        {

            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                UpdateContainerSizes();

                // If we have items but no current container, update now that we have a size
                if (_currentItemContainer == null && GetItemsList()?.Count > 0)
                {
                    UpdateCurrentItem();
                }
            }
        }
    }

    private void UpdateContainerSizes()
    {
        if (_containerCanvas == null || Bounds.Width <= 0)
            return;

        if (_currentItemContainer != null)
        {
            _currentItemContainer.Width = Bounds.Width;
            // Only set height if we have it, otherwise let content determine it
            if (Bounds.Height > 0)
            {
                _currentItemContainer.Height = Bounds.Height;
            }
        }

        if (_previewItemContainer != null)
        {
            _previewItemContainer.Width = Bounds.Width;
            if (Bounds.Height > 0)
            {
                _previewItemContainer.Height = Bounds.Height;
            }
        }
    }

    private void OnItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= OnCollectionChanged;
        }

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += OnCollectionChanged;
        }

        // Reset to first item when items source changes
        SelectedIndex = 0;
        UpdateCurrentItem();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCurrentItem();
    }

    private void OnSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateCurrentItem();
    }

    private void UpdateCurrentItem()
    {
        if (_containerCanvas == null)
        {
            return;
        }

        var items = GetItemsList();

        if (items == null || items.Count == 0 || SelectedIndex < 0 || SelectedIndex >= items.Count)
        {
            ClearCurrentItem();
            return;
        }

        // If we don't have a valid width yet, defer until we do
        // (Height can be determined by content)
        if (Bounds.Width <= 0)
        {
            return;
        }

        var item = items[SelectedIndex];
        CurrentItem = item;

        // Create new container for the current item
        if (item is not null)
            _currentItemContainer = CreateItemContainer(item);

        if (_currentItemContainer == null)
        {
            return;
        }

        // Clear and add to canvas
        _containerCanvas.Children.Clear();
        Canvas.SetLeft(_currentItemContainer, 0);
        Canvas.SetTop(_currentItemContainer, 0);
        _containerCanvas.Children.Add(_currentItemContainer);

        UpdateContainerSizes();
    }

    private void ClearCurrentItem()
    {
        if (_containerCanvas == null)
            return;

        _containerCanvas.Children.Clear();
        _currentItemContainer = null;
        _previewItemContainer = null;
        CurrentItem = null;
    }

    private ContentControl CreateItemContainer(object item)
    {
        var container = new ContentControl
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        if (ItemTemplate != null)
        {
            container.Content = ItemTemplate.Build(item);
        }
        else
        {
            container.Content = new TextBlock
            {
                Text = item?.ToString() ?? string.Empty,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        // Set width if we have it
        if (Bounds.Width > 0)
        {
            container.Width = Bounds.Width;
        }

        // Only set explicit height if we have it, otherwise let content measure itself
        if (Bounds.Height > 0)
        {
            container.Height = Bounds.Height;
        }

        return container;
    }

    private IList? GetItemsList()
    {
        if (ItemsSource == null)
            return null;

        return ItemsSource as IList ?? ItemsSource.Cast<object>().ToList();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);
        _pointerPressPosition = position;
        _isDragging = false;
        e.Pointer.Capture(this);
    }

    private void OnPointerMoved(object? sender, global::Avalonia.Input.PointerEventArgs e)
    {
        if (_pointerPressPosition == null || _currentItemContainer == null || _containerCanvas == null)
            return;

        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _pointerPressPosition.Value.X;
        var deltaY = currentPosition.Y - _pointerPressPosition.Value.Y;

        // Start dragging if movement is primarily horizontal
        if (!_isDragging && Math.Abs(deltaX) > 5 && Math.Abs(deltaX) > Math.Abs(deltaY))
        {
            _isDragging = true;
            e.Pointer.Capture(this);

            // Determine which direction we're swiping and prepare the preview item
            var items = GetItemsList();
            if (items != null && items.Count > 0)
            {
                int previewIndex = -1;

                if (deltaX < 0 && (SelectedIndex < items.Count - 1 || Loop))
                {
                    // Swiping left - show next item
                    _previewDirection = 1;
                    previewIndex = SelectedIndex < items.Count - 1 ? SelectedIndex + 1 : 0;
                }
                else if (deltaX > 0 && (SelectedIndex > 0 || Loop))
                {
                    // Swiping right - show previous item
                    _previewDirection = -1;
                    previewIndex = SelectedIndex > 0 ? SelectedIndex - 1 : items.Count - 1;
                }

                if (previewIndex >= 0)
                {
                    var previewItem = items[previewIndex];
                    if (previewItem == null)
                        return;
                    _previewItemContainer = CreateItemContainer(previewItem);

                    // Position preview item off-screen in the appropriate direction
                    var initialX = _previewDirection > 0 ? Bounds.Width : -Bounds.Width;
                    Canvas.SetLeft(_previewItemContainer, initialX);
                    Canvas.SetTop(_previewItemContainer, 0);

                    _containerCanvas.Children.Insert(0, _previewItemContainer); // Add behind current
                }
            }
        }

        if (_isDragging)
        {
            // Apply translation to current item
            Canvas.SetLeft(_currentItemContainer, deltaX);

            // Apply translation to preview item
            if (_previewItemContainer != null)
            {
                var initialX = _previewDirection > 0 ? Bounds.Width : -Bounds.Width;
                Canvas.SetLeft(_previewItemContainer, initialX + deltaX);
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        CompleteGesture();
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        CompleteGesture();
    }

    private void CompleteGesture()
    {
        if (_pointerPressPosition == null || !_isDragging)
        {
            _pointerPressPosition = null;
            _isDragging = false;
            return;
        }

        var deltaX = _currentItemContainer != null ? Canvas.GetLeft(_currentItemContainer) : 0;

        // Determine if we should commit to the transition
        bool shouldTransition = Math.Abs(deltaX) > SwipeThreshold;

        if (shouldTransition && _previewItemContainer != null)
        {
            // Animate to complete the transition
            AnimateTransition(true, () =>
            {
                // Update selected index after animation
                if (_previewDirection > 0)
                {
                    Next();
                }
                else
                {
                    Previous();
                }
            });
        }
        else
        {
            // Animate back to original position (rubberband)
            AnimateTransition(false, null);
        }

        _pointerPressPosition = null;
        _isDragging = false;
    }

    private void AnimateTransition(bool commit, Action? onComplete)
    {
        if (_containerCanvas == null || _currentItemContainer == null)
            return;

        var targetX = 0.0;
        var previewTargetX = 0.0;

        if (commit && _previewItemContainer != null)
        {
            // Complete the transition
            targetX = _previewDirection > 0 ? -Bounds.Width : Bounds.Width;
            previewTargetX = 0;
        }
        else
        {
            // Snap back
            targetX = 0;
            if (_previewItemContainer != null)
            {
                previewTargetX = _previewDirection > 0 ? Bounds.Width : -Bounds.Width;
            }
        }

        var currentStartX = Canvas.GetLeft(_currentItemContainer);
        var previewStartX = _previewItemContainer != null ? Canvas.GetLeft(_previewItemContainer) : 0;

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60fps
        };

        var startTime = DateTime.Now;
        timer.Tick += (s, e) =>
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / AnimationDuration, 1.0);

            // Ease-out animation
            var eased = 1 - Math.Pow(1 - progress, 3);

            // Update current item position
            var currentX = currentStartX + (targetX - currentStartX) * eased;
            Canvas.SetLeft(_currentItemContainer, currentX);

            // Update preview item position
            if (_previewItemContainer != null)
            {
                var previewX = previewStartX + (previewTargetX - previewStartX) * eased;
                Canvas.SetLeft(_previewItemContainer, previewX);
            }

            if (progress >= 1.0)
            {
                timer.Stop();

                if (!commit)
                {
                    // Remove preview item if we're snapping back
                    if (_previewItemContainer != null)
                    {
                        _containerCanvas.Children.Remove(_previewItemContainer);
                        _previewItemContainer = null;
                    }
                }

                onComplete?.Invoke();
            }
        };

        timer.Start();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
            case Key.Up:
                Previous();
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Down:
                Next();
                e.Handled = true;
                break;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            Previous();
            e.Handled = true;
        }
        else if (e.Delta.Y < 0)
        {
            Next();
            e.Handled = true;
        }
    }
}
