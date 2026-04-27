using System.Collections.ObjectModel;

namespace ControlGallery.Pages;

/// <summary>
/// Demonstrates the MAUI CarouselView mapped to the Avalonia Carousel control.
/// </summary>
public partial class CarouselViewPage : ContentPage
{
    private CarouselDemoItem? _boundCurrentItem;
    private int _boundPosition = 1;
    private CarouselDemoItem? _eventCurrentItem;
    private int _eventPosition;
    private int _gesturePosition;
    private bool _isAnimationEnabled = true;
    private bool _isGestureSwipeEnabled = true;
    private bool _isLoopEnabled = true;
    private Thickness _dynamicPeekAreaInsets = new(48, 0);
    private int _loopPosition;
    private double _peekInset = 48;
    private int _peekPosition = 1;
    private DataTemplate? _runtimeItemTemplate;
    private string _runtimeSourceName = string.Empty;
    private ObservableCollection<CarouselDemoItem> _runtimeSourceItems = [];
    private int _runtimeSourcePosition;
    private string _runtimeTemplateName = string.Empty;
    private int _runtimeTemplatePosition = 1;

    private readonly ObservableCollection<CarouselDemoItem> _runtimeSourceA = CreateItems("Source A");
    private readonly ObservableCollection<CarouselDemoItem> _runtimeSourceB = CreateItems("Source B");

    /// <summary>
    /// Initializes a new instance of the <see cref="CarouselViewPage"/> class.
    /// </summary>
    public CarouselViewPage()
    {
        InitializeComponent();

        RuntimeSourceItems = _runtimeSourceA;
        RuntimeSourceName = "Source A";
        RuntimeItemTemplate = GetTemplate("CarouselCardTemplate");
        RuntimeTemplateName = "Card";
        BoundCurrentItem = BoundItems[BoundPosition];
        EventCurrentItem = EventItems[EventPosition];
        AddEventLog("Events sample ready");
        BindingContext = this;
    }

    /// <summary>
    /// Gets the items used by the basic carousel sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> BasicItems { get; } = CreateItems("Basic");

    /// <summary>
    /// Gets the items used by the explicit ItemsSource sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> SourceItems { get; } = CreateItems("ItemsSource");

    /// <summary>
    /// Gets the items used by the EmptyView sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> EmptyItems { get; } = [];

    /// <summary>
    /// Gets or sets the active items used by the runtime ItemsSource sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> RuntimeSourceItems
    {
        get => _runtimeSourceItems;
        set
        {
            if (_runtimeSourceItems == value)
                return;

            _runtimeSourceItems = value;
            RuntimeSourcePosition = 0;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the source name used by the runtime ItemsSource sample.
    /// </summary>
    public string RuntimeSourceName
    {
        get => _runtimeSourceName;
        set
        {
            if (_runtimeSourceName == value)
                return;

            _runtimeSourceName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the runtime ItemsSource sample.
    /// </summary>
    public int RuntimeSourcePosition
    {
        get => _runtimeSourcePosition;
        set
        {
            var nextPosition = RuntimeSourceItems.Count > 0
                ? Math.Clamp(value, 0, RuntimeSourceItems.Count - 1)
                : 0;

            if (_runtimeSourcePosition == nextPosition)
                return;

            _runtimeSourcePosition = nextPosition;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the items used by the runtime ItemTemplate sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> RuntimeTemplateItems { get; } = CreateItems("Template");

    /// <summary>
    /// Gets or sets the active item template used by the runtime ItemTemplate sample.
    /// </summary>
    public DataTemplate? RuntimeItemTemplate
    {
        get => _runtimeItemTemplate;
        set
        {
            if (_runtimeItemTemplate == value)
                return;

            _runtimeItemTemplate = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the active template name used by the runtime ItemTemplate sample.
    /// </summary>
    public string RuntimeTemplateName
    {
        get => _runtimeTemplateName;
        set
        {
            if (_runtimeTemplateName == value)
                return;

            _runtimeTemplateName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the runtime ItemTemplate sample.
    /// </summary>
    public int RuntimeTemplatePosition
    {
        get => _runtimeTemplatePosition;
        set
        {
            var nextPosition = Math.Clamp(value, 0, RuntimeTemplateItems.Count - 1);
            if (_runtimeTemplatePosition == nextPosition)
                return;

            _runtimeTemplatePosition = nextPosition;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the items used by the gesture carousel sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> GestureItems { get; } = CreateItems("Swipe");

    /// <summary>
    /// Gets the items used by the loop carousel sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> LoopItems { get; } = CreateItems("Loop");

    /// <summary>
    /// Gets the items used by the peek and animation carousel sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> PeekItems { get; } = CreateItems("Peek");

    /// <summary>
    /// Gets the items used by the vertical carousel sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> VerticalItems { get; } = CreateItems("Vertical");

    /// <summary>
    /// Gets the items used by the Position and CurrentItem binding sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> BoundItems { get; } = CreateItems("Bound");

    /// <summary>
    /// Gets the items used by the events and logs sample.
    /// </summary>
    public ObservableCollection<CarouselDemoItem> EventItems { get; } = CreateItems("Events");

    /// <summary>
    /// Gets the event log entries for the events and logs sample.
    /// </summary>
    public ObservableCollection<string> EventLogs { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the gesture sample accepts swipe gestures.
    /// </summary>
    public bool IsGestureSwipeEnabled
    {
        get => _isGestureSwipeEnabled;
        set
        {
            if (_isGestureSwipeEnabled == value)
                return;

            _isGestureSwipeEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether carousel transitions are animated.
    /// </summary>
    public bool IsAnimationEnabled
    {
        get => _isAnimationEnabled;
        set
        {
            if (_isAnimationEnabled == value)
                return;

            _isAnimationEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the horizontal peek inset used by the peek and animation sample.
    /// </summary>
    public double PeekInset
    {
        get => _peekInset;
        set
        {
            if (Math.Abs(_peekInset - value) < double.Epsilon)
                return;

            _peekInset = value;
            DynamicPeekAreaInsets = new Thickness(value, 0);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the peek area insets used by the peek and animation sample.
    /// </summary>
    public Thickness DynamicPeekAreaInsets
    {
        get => _dynamicPeekAreaInsets;
        private set
        {
            if (_dynamicPeekAreaInsets == value)
                return;

            _dynamicPeekAreaInsets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the loop sample wraps selection.
    /// </summary>
    public bool IsLoopEnabled
    {
        get => _isLoopEnabled;
        set
        {
            if (_isLoopEnabled == value)
                return;

            _isLoopEnabled = value;
            LoopPosition = NormalizeLoopPosition(LoopPosition);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the gesture sample.
    /// </summary>
    public int GesturePosition
    {
        get => _gesturePosition;
        set
        {
            if (_gesturePosition == value)
                return;

            _gesturePosition = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the loop sample.
    /// </summary>
    public int LoopPosition
    {
        get => _loopPosition;
        set
        {
            var nextPosition = NormalizeLoopPosition(value);
            if (_loopPosition == nextPosition)
                return;

            _loopPosition = nextPosition;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the peek and animation sample.
    /// </summary>
    public int PeekPosition
    {
        get => _peekPosition;
        set
        {
            if (_peekPosition == value)
                return;

            _peekPosition = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the binding sample.
    /// </summary>
    public int BoundPosition
    {
        get => _boundPosition;
        set
        {
            var nextPosition = Math.Clamp(value, 0, BoundItems.Count - 1);
            if (_boundPosition == nextPosition)
                return;

            _boundPosition = nextPosition;
            BoundCurrentItem = BoundItems[_boundPosition];
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item in the binding sample.
    /// </summary>
    public CarouselDemoItem? BoundCurrentItem
    {
        get => _boundCurrentItem;
        set
        {
            if (_boundCurrentItem == value)
                return;

            _boundCurrentItem = value;

            if (value != null)
            {
                var index = BoundItems.IndexOf(value);
                if (index >= 0 && _boundPosition != index)
                {
                    _boundPosition = index;
                    OnPropertyChanged(nameof(BoundPosition));
                }
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item index in the events and logs sample.
    /// </summary>
    public int EventPosition
    {
        get => _eventPosition;
        set
        {
            var nextPosition = Math.Clamp(value, 0, EventItems.Count - 1);
            if (_eventPosition == nextPosition)
                return;

            _eventPosition = nextPosition;
            EventCurrentItem = EventItems[_eventPosition];
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected item in the events and logs sample.
    /// </summary>
    public CarouselDemoItem? EventCurrentItem
    {
        get => _eventCurrentItem;
        set
        {
            if (_eventCurrentItem == value)
                return;

            _eventCurrentItem = value;

            if (value != null)
            {
                var index = EventItems.IndexOf(value);
                if (index >= 0 && _eventPosition != index)
                {
                    _eventPosition = index;
                    OnPropertyChanged(nameof(EventPosition));
                }
            }

            OnPropertyChanged();
        }
    }

    private static ObservableCollection<CarouselDemoItem> CreateItems(string prefix)
    {
        return
        [
            new("01", $"{prefix} one", "First carousel page rendered through the Avalonia Carousel.", Color.FromArgb("#4F46E5")),
            new("02", $"{prefix} two", "Second page with a templated MAUI view inside the carousel.", Color.FromArgb("#0F766E")),
            new("03", $"{prefix} three", "Third page used to verify selection, loop, and gesture behavior.", Color.FromArgb("#B45309")),
            new("04", $"{prefix} four", "Fourth page gives loop and peek samples enough adjacent content.", Color.FromArgb("#BE123C"))
        ];
    }

    private DataTemplate GetTemplate(string key)
    {
        return (DataTemplate)Resources[key];
    }

    private int NormalizeLoopPosition(int position)
    {
        if (LoopItems.Count == 0)
            return 0;

        if (IsLoopEnabled)
            return (position % LoopItems.Count + LoopItems.Count) % LoopItems.Count;

        return Math.Clamp(position, 0, LoopItems.Count - 1);
    }

    private void OnGesturePreviousClicked(object? sender, EventArgs e)
    {
        GesturePosition = Math.Clamp(GesturePosition - 1, 0, GestureItems.Count - 1);
    }

    private void OnGestureNextClicked(object? sender, EventArgs e)
    {
        GesturePosition = Math.Clamp(GesturePosition + 1, 0, GestureItems.Count - 1);
    }

    private void OnLoopPreviousClicked(object? sender, EventArgs e)
    {
        LoopPosition--;
    }

    private void OnLoopNextClicked(object? sender, EventArgs e)
    {
        LoopPosition++;
    }

    private void OnSelectFirstClicked(object? sender, EventArgs e)
    {
        BoundPosition = 0;
    }

    private void OnSelectLastClicked(object? sender, EventArgs e)
    {
        BoundPosition = BoundItems.Count - 1;
    }

    private void OnUseFirstItemsSourceClicked(object? sender, EventArgs e)
    {
        RuntimeSourceItems = _runtimeSourceA;
        RuntimeSourceName = "Source A";
    }

    private void OnUseSecondItemsSourceClicked(object? sender, EventArgs e)
    {
        RuntimeSourceItems = _runtimeSourceB;
        RuntimeSourceName = "Source B";
    }

    private void OnAddRuntimeSourceItemClicked(object? sender, EventArgs e)
    {
        var nextNumber = RuntimeSourceItems.Count + 1;
        RuntimeSourceItems.Add(new CarouselDemoItem(
            $"{nextNumber:00}",
            $"{RuntimeSourceName} added {nextNumber}",
            "Added to the active ObservableCollection while the carousel is visible.",
            Color.FromArgb("#0891B2")));
    }

    private void OnAddEmptyItemClicked(object? sender, EventArgs e)
    {
        var nextNumber = EmptyItems.Count + 1;
        EmptyItems.Add(new CarouselDemoItem(
            $"{nextNumber:00}",
            $"Empty sample {nextNumber}",
            "Added to the EmptyView sample while the carousel is visible.",
            Color.FromArgb("#7C3AED")));
    }

    private void OnClearEmptyItemsClicked(object? sender, EventArgs e)
    {
        EmptyItems.Clear();
    }

    private void OnUseCardTemplateClicked(object? sender, EventArgs e)
    {
        RuntimeItemTemplate = GetTemplate("CarouselCardTemplate");
        RuntimeTemplateName = "Card";
    }

    private void OnUseCompactTemplateClicked(object? sender, EventArgs e)
    {
        RuntimeItemTemplate = GetTemplate("CarouselCompactTemplate");
        RuntimeTemplateName = "Compact";
    }

    private void OnEventsPreviousClicked(object? sender, EventArgs e)
    {
        EventPosition = Math.Clamp(EventPosition - 1, 0, EventItems.Count - 1);
    }

    private void OnEventsNextClicked(object? sender, EventArgs e)
    {
        EventPosition = Math.Clamp(EventPosition + 1, 0, EventItems.Count - 1);
    }

    private void OnClearEventsLogClicked(object? sender, EventArgs e)
    {
        EventLogs.Clear();
    }

    private void OnEventsPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        AddEventLog($"PositionChanged: {e.PreviousPosition} -> {e.CurrentPosition}");
    }

    private void OnEventsCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        AddEventLog($"CurrentItemChanged: {FormatItem(e.PreviousItem)} -> {FormatItem(e.CurrentItem)}");
    }

    private void AddEventLog(string message)
    {
        EventLogs.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}");

        while (EventLogs.Count > 20)
        {
            EventLogs.RemoveAt(EventLogs.Count - 1);
        }
    }

    private static string FormatItem(object? item)
    {
        return item is CarouselDemoItem carouselItem
            ? carouselItem.Title
            : "null";
    }
}

/// <summary>
/// Provides item data for the CarouselView sample.
/// </summary>
public sealed class CarouselDemoItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CarouselDemoItem"/> class.
    /// </summary>
    /// <param name="number">The visual item number.</param>
    /// <param name="title">The item title.</param>
    /// <param name="detail">The item detail text.</param>
    /// <param name="color">The item background color.</param>
    public CarouselDemoItem(string number, string title, string detail, Color color)
    {
        Number = number;
        Title = title;
        Detail = detail;
        Color = color;
    }

    /// <summary>
    /// Gets the visual item number.
    /// </summary>
    public string Number { get; }

    /// <summary>
    /// Gets the item title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the item detail text.
    /// </summary>
    public string Detail { get; }

    /// <summary>
    /// Gets the item background color.
    /// </summary>
    public Color Color { get; }
}
