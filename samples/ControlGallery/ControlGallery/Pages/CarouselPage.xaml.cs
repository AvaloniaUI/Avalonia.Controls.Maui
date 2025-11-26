using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace ControlGallery.Pages;

public class CarouselItem
{
    public string Text { get; init; } = string.Empty;
    public Color Color { get; init; } = Colors.Transparent;
}

public partial class CarouselPage : ContentPage
{
    public ObservableCollection<CarouselItem> Items { get; } = new();
    public ObservableCollection<CarouselItem> EmptyItems { get; } = new();

    private int _horizontalPosition;
    private int _verticalPosition;
    private bool _horizontalIsDragging;
    private bool _verticalIsDragging;
    private bool _horizontalLoopEnabled = true;
    private bool _horizontalSwipeEnabled = true;
    private bool _verticalLoopEnabled = true;
    private bool _verticalSwipeEnabled = true;
    private bool _useEmptyTemplate;
    private CarouselItem? _currentItem;
    private CarouselItem? _currentItemSample;
    private string _currentItemSampleText = string.Empty;
    private string _positionSampleText = "0";
    private int _positionSamplePosition;
    private INotifyPropertyChanged? _horizontalPlatform;
    private INotifyPropertyChanged? _verticalPlatform;
    private ScrollBarVisibility _sampleHorizontalScrollBarVisibility = ScrollBarVisibility.Default;
    private ScrollBarVisibility _sampleVerticalScrollBarVisibility = ScrollBarVisibility.Default;

    public object? EmptyContent { get; private set; }
    public DataTemplate? EmptyTemplate { get; private set; }
    public int HorizontalPosition
    {
        get => _horizontalPosition;
        set
        {
            if (_horizontalPosition == value)
                return;
            _horizontalPosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHorizontalPrevious));
            OnPropertyChanged(nameof(HasHorizontalNext));
        }
    }

    public int VerticalPosition
    {
        get => _verticalPosition;
        set
        {
            if (_verticalPosition == value)
                return;
            _verticalPosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVerticalPrevious));
            OnPropertyChanged(nameof(HasVerticalNext));
        }
    }

    public bool HorizontalIsDragging
    {
        get => _horizontalIsDragging;
        set
        {
            if (_horizontalIsDragging == value)
                return;
            _horizontalIsDragging = value;
            OnPropertyChanged();
        }
    }

    public bool VerticalIsDragging
    {
        get => _verticalIsDragging;
        set
        {
            if (_verticalIsDragging == value)
                return;
            _verticalIsDragging = value;
            OnPropertyChanged();
        }
    }

    public bool UseEmptyTemplate
    {
        get => _useEmptyTemplate;
        set
        {
            if (_useEmptyTemplate == value)
                return;
            _useEmptyTemplate = value;
            OnPropertyChanged();
            ApplyEmptyViewSettings();
        }
    }

    public bool HorizontalLoopEnabled
    {
        get => _horizontalLoopEnabled;
        set
        {
            if (_horizontalLoopEnabled == value)
                return;
            _horizontalLoopEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHorizontalPrevious));
            OnPropertyChanged(nameof(HasHorizontalNext));
        }
    }

    public bool HorizontalSwipeEnabled
    {
        get => _horizontalSwipeEnabled;
        set
        {
            if (_horizontalSwipeEnabled == value)
                return;
            _horizontalSwipeEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHorizontalPrevious));
            OnPropertyChanged(nameof(HasHorizontalNext));
        }
    }

    public bool VerticalLoopEnabled
    {
        get => _verticalLoopEnabled;
        set
        {
            if (_verticalLoopEnabled == value)
                return;
            _verticalLoopEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVerticalPrevious));
            OnPropertyChanged(nameof(HasVerticalNext));
        }
    }

    public bool VerticalSwipeEnabled
    {
        get => _verticalSwipeEnabled;
        set
        {
            if (_verticalSwipeEnabled == value)
                return;
            _verticalSwipeEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVerticalPrevious));
            OnPropertyChanged(nameof(HasVerticalNext));
        }
    }

    public bool HasHorizontalPrevious => HorizontalLoopEnabled || HorizontalPosition > 0;
    public bool HasHorizontalNext => HorizontalLoopEnabled || HorizontalPosition < Items.Count - 1;
    public bool HasVerticalPrevious => VerticalLoopEnabled || VerticalPosition > 0;
    public bool HasVerticalNext => VerticalLoopEnabled || VerticalPosition < Items.Count - 1;
    public CarouselItem? CurrentItem
    {
        get => _currentItem;
        set
        {
            if (_currentItem == value)
                return;
            _currentItem = value;
            if (value != null)
            {
                var index = Items.IndexOf(value);
                if (index >= 0)
                {
                    HorizontalPosition = index;
                }
            }
            OnPropertyChanged();
        }
    }

    public int PositionSamplePosition
    {
        get => _positionSamplePosition;
        set
        {
            if (_positionSamplePosition == value)
                return;
            _positionSamplePosition = value;
            OnPropertyChanged();
        }
    }

    public string PositionSampleText
    {
        get => _positionSampleText;
        set
        {
            if (_positionSampleText == value)
                return;
            _positionSampleText = value;
            OnPropertyChanged();
        }
    }

    public CarouselItem? CurrentItemSample
    {
        get => _currentItemSample;
        set
        {
            if (_currentItemSample == value)
                return;
            _currentItemSample = value;
            CurrentItemSampleText = value?.Text ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public ScrollBarVisibility SampleHorizontalScrollBarVisibility
    {
        get => _sampleHorizontalScrollBarVisibility;
        set
        {
            if (_sampleHorizontalScrollBarVisibility == value)
                return;
            _sampleHorizontalScrollBarVisibility = value;
            OnPropertyChanged();
        }
    }

    public ScrollBarVisibility SampleVerticalScrollBarVisibility
    {
        get => _sampleVerticalScrollBarVisibility;
        set
        {
            if (_sampleVerticalScrollBarVisibility == value)
                return;
            _sampleVerticalScrollBarVisibility = value;
            OnPropertyChanged();
        }
    }

    public string CurrentItemSampleText
    {
        get => _currentItemSampleText;
        set
        {
            if (_currentItemSampleText == value)
                return;
            _currentItemSampleText = value;
            OnPropertyChanged();
        }
    }

    public CarouselPage()
    {
        InitializeComponent();
        
        EmptyContent = "No items available.";
        EmptyTemplate = Resources.TryGetValue("EmptyItemsTemplate", out var templateObj)
            ? templateObj as DataTemplate
            : null;
        
        PopulateItems();
        
        Items.CollectionChanged += OnItemsChanged;
        BindingContext = this;

        HorizontalCarousel.HandlerChanged += OnHorizontalHandlerChanged;
        VerticalCarousel.HandlerChanged += OnVerticalHandlerChanged;
        ApplyEmptyViewSettings();
        PositionSamplePosition = 0;
        CurrentItemSample = Items.FirstOrDefault();
        PositionSampleText = PositionSamplePosition.ToString();
        CurrentItemSampleText = CurrentItemSample?.Text ?? string.Empty;
    }

    void PopulateItems()
    {
        Items.Clear();

        var random = new Random();
        const int totalItems = 10;

        for (int i = 1; i <= totalItems; i++)
        {
            Items.Add(new CarouselItem
            {
                Text = $"Item {i}",
                Color = Color.FromRgb(random.Next(256), random.Next(256), random.Next(256))
            });
        }
    }

    void OnHorizontalPreviousClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (HorizontalLoopEnabled)
        {
            HorizontalPosition = (HorizontalPosition - 1 + Items.Count) % Items.Count;
        }
        else if (HorizontalPosition > 0)
        {
            HorizontalPosition -= 1;
        }
    }

    void OnHorizontalNextClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (HorizontalLoopEnabled)
        {
            HorizontalPosition = (HorizontalPosition + 1) % Items.Count;
        }
        else if (HorizontalPosition < Items.Count - 1)
        {
            HorizontalPosition += 1;
        }
    }

    void OnVerticalPreviousClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (VerticalLoopEnabled)
        {
            VerticalPosition = (VerticalPosition - 1 + Items.Count) % Items.Count;
        }
        else if (VerticalPosition > 0)
        {
            VerticalPosition -= 1;
        }
    }

    void OnVerticalNextClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (VerticalLoopEnabled)
        {
            VerticalPosition = (VerticalPosition + 1) % Items.Count;
        }
        else if (VerticalPosition < Items.Count - 1)
        {
            VerticalPosition += 1;
        }
    }

    void OnAddEmptyItemsClicked(object sender, EventArgs e)
    {
        PopulateCollection(EmptyItems, 5, "Empty Item");
    }

    void OnClearEmptyItemsClicked(object sender, EventArgs e)
    {
        EmptyItems.Clear();
    }

    void OnHorizontalHandlerChanged(object? sender, EventArgs e)
    {
        AttachDraggingListener(HorizontalCarousel, isHorizontal: true);
    }

    void OnVerticalHandlerChanged(object? sender, EventArgs e)
    {
        AttachDraggingListener(VerticalCarousel, isHorizontal: false);
    }

    void AttachDraggingListener(CarouselView view, bool isHorizontal)
    {
        if (isHorizontal && _horizontalPlatform != null)
            _horizontalPlatform.PropertyChanged -= OnPlatformDraggingChanged;
        if (!isHorizontal && _verticalPlatform != null)
            _verticalPlatform.PropertyChanged -= OnPlatformDraggingChanged;

        if (view.Handler?.PlatformView is not INotifyPropertyChanged platform)
            return;

        if (isHorizontal)
            _horizontalPlatform = platform;
        else
            _verticalPlatform = platform;

        platform.PropertyChanged -= OnPlatformDraggingChanged;
        platform.PropertyChanged += OnPlatformDraggingChanged;

        UpdateDragging(platform, isHorizontal);
    }

    void OnPlatformDraggingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsDragging" && sender is INotifyPropertyChanged platform)
        {
            if (platform == _horizontalPlatform)
                HorizontalIsDragging = ReadIsDragging(platform);

            if (platform == _verticalPlatform)
                VerticalIsDragging = ReadIsDragging(platform);
        }
    }

    void UpdateDragging(INotifyPropertyChanged platform, bool isHorizontal)
    {
        if (isHorizontal)
            HorizontalIsDragging = ReadIsDragging(platform);
        else
            VerticalIsDragging = ReadIsDragging(platform);
    }

    bool ReadIsDragging(INotifyPropertyChanged platform)
    {
        var prop = platform.GetType().GetRuntimeProperty("IsDragging");
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            return (bool)(prop.GetValue(platform) ?? false);
        }
        return false;
    }

    void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        HorizontalPosition = Math.Min(HorizontalPosition, Math.Max(Items.Count - 1, 0));
        VerticalPosition = Math.Min(VerticalPosition, Math.Max(Items.Count - 1, 0));
        RefreshNavigationState();

        if (Items.Count == 0)
        {
            CurrentItem = null;
            CurrentItemSample = null;
            PositionSamplePosition = 0;
            PositionSampleText = "0";
            CurrentItemSampleText = string.Empty;
        }
        else if (CurrentItem != null && !Items.Contains(CurrentItem))
        {
            CurrentItem = Items.FirstOrDefault();
        }
        else if (CurrentItemSample != null && !Items.Contains(CurrentItemSample))
        {
            CurrentItemSample = Items.FirstOrDefault();
            PositionSamplePosition = 0;
            PositionSampleText = PositionSamplePosition.ToString();
        }
    }

    void RefreshNavigationState()
    {
        OnPropertyChanged(nameof(HasHorizontalPrevious));
        OnPropertyChanged(nameof(HasHorizontalNext));
        OnPropertyChanged(nameof(HasVerticalPrevious));
        OnPropertyChanged(nameof(HasVerticalNext));
    }

    void ApplyEmptyViewSettings()
    {
        if (UseEmptyTemplate)
        {
            EmptyTemplate ??= Resources.TryGetValue("EmptyItemsTemplate", out var templateObj)
                ? templateObj as DataTemplate
                : null;
            EmptyContent = null;
        }
        else
        {
            EmptyTemplate = null;
            EmptyContent = "No items available.";
        }

        OnPropertyChanged(nameof(EmptyContent));
        OnPropertyChanged(nameof(EmptyTemplate));
    }

    void OnApplyPositionSampleClicked(object sender, EventArgs e)
    {
        if (int.TryParse(PositionSampleText, out var index))
        {
            index = Math.Clamp(index, 0, Math.Max(Items.Count - 1, 0));
            PositionSamplePosition = index;
            var item = Items.ElementAtOrDefault(index);
            CurrentItemSample = item;
            CurrentItemSampleText = item?.Text ?? string.Empty;
        }
    }

    void OnApplyCurrentItemSampleClicked(object sender, EventArgs e)
    {
        var match = Items.FirstOrDefault(i => string.Equals(i.Text, CurrentItemSampleText, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            CurrentItemSample = match;
            var idx = Items.IndexOf(match);
            PositionSamplePosition = idx >= 0 ? idx : PositionSamplePosition;
            PositionSampleText = PositionSamplePosition.ToString();
        }
    }

    void OnShowHorizontalScrollBarsClicked(object sender, EventArgs e)
    {
        SampleHorizontalScrollBarVisibility = ScrollBarVisibility.Always;
    }

    void OnHideHorizontalScrollBarsClicked(object sender, EventArgs e)
    {
        SampleHorizontalScrollBarVisibility = ScrollBarVisibility.Never;
    }

    void OnShowVerticalScrollBarsClicked(object sender, EventArgs e)
    {
        SampleVerticalScrollBarVisibility = ScrollBarVisibility.Always;
    }

    void OnHideVerticalScrollBarsClicked(object sender, EventArgs e)
    {
        SampleVerticalScrollBarVisibility = ScrollBarVisibility.Never;
    }

    void PopulateCollection(ObservableCollection<CarouselItem> target, int count, string prefix)
    {
        target.Clear();
        var random = new Random();
        for (int i = 1; i <= count; i++)
        {
            target.Add(new CarouselItem
            {
                Text = $"{prefix} {i}",
                Color = Color.FromRgb(random.Next(256), random.Next(256), random.Next(256))
            });
        }
    }
}
