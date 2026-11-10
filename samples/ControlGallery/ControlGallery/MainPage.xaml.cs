using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core;
using ControlGallery.Pages;
using ControlGallery.Pages.ShellSamples;
using ControlGallery.Pages.ShellSamples.ShellPlayground;
using ControlGallery.Pages.WebView;
using Microsoft.Maui.Controls.Shapes;

namespace ControlGallery;

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The property Expander binds to is kept by TrimmerRoots.xml")]
public partial class MainPage : FlyoutPage
{
    private const double WideBreakpoint = 1008;

    private readonly List<SampleGroup> _allSamples;
    private SampleItem? _currentItem;
    private Window? _window;

    public ObservableCollection<SampleGroup> FilteredSamples { get; } = new();

    public MainPage()
    {
        InitializeComponent();

        BindingContext = this;

        _allSamples = CreateSamples();
        UpdateMenu(string.Empty);

        ThemePicker.SelectedIndex = ToThemeIndex(Application.Current?.UserAppTheme ?? AppTheme.Unspecified);
        FlowDirectionPicker.SelectedIndex = 0;
        if (Application.Current is { } app)
            app.RequestedThemeChanged += (_, _) => ThemePicker.SelectedIndex = ToThemeIndex(app.UserAppTheme);

        NavigateTo(_allSamples[0][0]);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (_window is not null)
            _window.SizeChanged -= OnWindowSizeChanged;

        _window = Parent as Window;

        if (_window is not null)
        {
            _window.SizeChanged += OnWindowSizeChanged;
            UpdateAdaptiveLayout(_window.Width);
        }
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        UpdateAdaptiveLayout(_window?.Width ?? 0);
    }

    private void UpdateAdaptiveLayout(double width)
    {
        if (width <= 0)
            return;

        var behavior = width >= WideBreakpoint ? FlyoutLayoutBehavior.Split : FlyoutLayoutBehavior.Popover;
        if (FlyoutLayoutBehavior == behavior)
            return;

        FlyoutLayoutBehavior = behavior;

        if (behavior == FlyoutLayoutBehavior.Popover)
            IsPresented = false;
    }

    private void UpdateMenu(string searchText)
    {
        FilteredSamples.Clear();

        foreach (var group in _allSamples)
        {
            var filteredItems = group.Where(item =>
                string.IsNullOrWhiteSpace(searchText) ||
                item.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                item.Detail.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            var filteredGroup = new SampleGroup(group.Name, filteredItems);
            if (filteredGroup.Count > 0)
                FilteredSamples.Add(filteredGroup);
        }
    }

    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateMenu(e.NewTextValue ?? string.Empty);
    }

    private void OnSampleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: SampleItem item })
            NavigateTo(item);
    }

    private void OnSettingsExpandedChanged(object? sender, ExpandedChangedEventArgs e)
    {
        SettingsChevron.Rotation = e.IsExpanded ? 180 : 0;
    }

    private void OnThemePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (Application.Current is not { } app)
            return;

        app.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }

    private void OnFlowDirectionPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (Window is { } window)
            window.FlowDirection = FlowDirectionPicker.SelectedIndex == 1 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private static int ToThemeIndex(AppTheme theme) => theme switch
    {
        AppTheme.Light => 1,
        AppTheme.Dark => 2,
        _ => 0,
    };

    private async void NavigateTo(SampleItem item)
    {
        if (_currentItem is not null)
            _currentItem.IsSelected = false;

        _currentItem = item;
        item.IsSelected = true;

        if (FlyoutLayoutBehavior != FlyoutLayoutBehavior.Split)
            IsPresented = false;

        await Task.Yield();

        var page = item.CreatePage();

        if (Detail is NavigationPage navPage)
        {
            navPage.Navigation.InsertPageBefore(page, navPage.RootPage);
            await navPage.Navigation.PopToRootAsync(animated: false);
        }
        else
        {
            Detail = new NavigationPage(page);
        }
    }

    private static List<SampleGroup> CreateSamples() =>
    [
        new("", [
            new("Welcome", "About this gallery", Icons.Home, () => new WelcomePage()),
        ]),

        new("Apps", [
            new("RpnCalculator", "A Reverse Polish Notation calc", Icons.Calculator, () => new RpnCalculator.MainPage()),
            new("SolitaireEncryption", "Solitaire encryption algorithm", Icons.Shield, () => new SolitaireEncryption.SolitairePage()),
            new("Tip Calc", "TipCalc application", Icons.Number, () => new TipCalc.TipCalcPage()),
            new("Weather", "Retrieves weather data", Icons.Cloud, () => new Weather.MainPage()),
            new("Word Puzzle", "A word puzzle game", Icons.Puzzle, () => new WordPuzzle.MainPage()),
        ]),

        new("Services", [
            new("Fonts", "Font integration test", Icons.Header, () => new FontsPage()),
        ]),

        new("Pages", [
            new("Page", "Base page background and padding", Icons.Document, () => new PageBasePage()),
            new("NavigationPage", "Navigation stack with animated transitions", Icons.Navigation, () => new NavigationDemoPage()),
            new("TabbedPage", "Tabbed navigation", Icons.Tab, () => new ControlGallery.Pages.TabbedPage()),
            new("TitleBar", "Custom window title bar", Icons.Window, () => new TitleBarPage()),
            new("ToolbarItems", "Toolbar items and interactions", Icons.Menu, () => new ToolbarItemPage()),
            new("Modal Pages", "Present pages modally with PushModalAsync", Icons.Dialog, () => new ModalPage()),
            new("Popups", "Alerts, ActionSheets, and Prompts", Icons.Flyout, () => new PopupsPage()),
            new("Lifecycle Events", "View & VisualElement event tracing", Icons.Bell, () => new LifecycleEventsPage()),
        ]),

        new("Layout", [
            new("StackLayout", "Vertical and horizontal stacking", Icons.List, () => new StackLayoutPage()),
            new("Grid", "Rows, columns, and spanning", Icons.Grid, () => new GridPage()),
            new("FlexLayout", "Flexible box layout", Icons.Layout, () => new FlexLayoutPage()),
            new("AbsoluteLayout", "Absolute positioning of elements", Icons.Container, () => new AbsoluteLayoutPage()),
        ]),

        new("Views", [
            new("ActivityIndicator", "Animated busy indicator", Icons.Spinner, () => new ActivityIndicatorPage()),
            new("Border", "Custom strikes and shapes", Icons.Border, () => new BorderPage()),
            new("BoxView", "Decorative colored rectangles", Icons.Rectangle, () => new BoxViewPage()),
            new("Button", "Standard clickable button", Icons.CursorClick, () => new ButtonPage()),
            new("CheckBox", "Toggle selection control", Icons.Checkbox, () => new CheckBoxPage()),
            new("CarouselView", "A carousel for paged content", Icons.Slides, () => new CarouselViewPage()),
            new("CollectionView", "Modern templated list", Icons.List, () => new CollectionViewPage()),
            new("ContentView", "Reusable custom content", Icons.Container, () => new ContentViewPage()),
            new("DatePicker", "Date selection picker", Icons.Calendar, () => new DatePickerPage()),
            new("Entry", "Single-line text entry", Icons.TextInput, () => new EntryPage()),
            new("Editor", "Multi-line text editor", Icons.Document, () => new EditorPage()),
            new("Frame", "Bordered layout container", Icons.Viewbox, () => new FramePage()),
            new("GraphicsView", "Custom 2D drawing canvas", Icons.Canvas, () => new GraphicsViewPage()),
            new("SkiaSharp", "SKCanvasView and SKGLView drawing and touch", Icons.Brush, () => new SkiaSharpPage()),
            new("IndicatorView", "Page position indicators for CarouselView", Icons.HorizontalDots, () => new IndicatorViewPage()),
            new("Image", "Visual content display", Icons.Image, () => new ImagePage()),
            new("ImageButton", "Interactive image button", Icons.Image, () => new ImageButtonPage()),
            new("ListView", "Scrolling data items", Icons.List, () => new ListViewPage()),
            // TODO: Re-enable when Map is ready
            //new("Map", "Interactive maps and pins", Icons.Navigation, () => new MapPage()),
            new("Picker", "Item selection dropdown", Icons.Dropdown, () => new PickerPage()),
            new("ProgressBar", "Visual progress status", Icons.Progress, () => new ProgressBarPage()),
            new("RadioButton", "Single-select option list", Icons.Radio, () => new RadioButtonPage()),
            new("RefreshView", "Pull-to-refresh container", Icons.Refresh, () => new RefreshViewPage()),
            new("ScrollView", "Scrollable layout container", Icons.Scroll, () => new ScrollViewPage()),
            new("SearchBar", "Search text input", Icons.TextInput, () => new SearchBarPage()),
            new("Slider", "Range value selection", Icons.Slider, () => new SliderPage()),
            new("Stepper", "Discrete incremental changes", Icons.Number, () => new StepperPage()),
            new("SwipeView", "Swipe action container", Icons.Gesture, () => new SwipeViewPage()),
            new("Switch", "Binary toggle switch", Icons.Toggle, () => new SwitchPage()),
            new("TableView", "Form-based data table", Icons.Grid, () => new TableViewPage()),
            new("TimePicker", "Time selection picker", Icons.Clock, () => new TimePickerPage()),
        ]),

        new("Community Toolkit", [
            new("Expander", "Content that shows and hides", Icons.Expand, () => new ExpanderPage()),
            new("Animation Behaviors", "Event commands and animations", Icons.Transition, () => new AnimationBehaviorsPage()),
            new("AvatarView", "Initials or image avatars", Icons.Person, () => new AvatarViewPage()),
            new("DockLayout", "Dock children to the edges", Icons.Split, () => new DockLayoutPage()),
            new("Input Behaviors", "Masks, length and typing events", Icons.Keyboard, () => new InputBehaviorsPage()),
            new("LazyView", "Create content on demand", Icons.Lightning, () => new LazyViewPage()),
            new("Popup", "Popups over the page", Icons.Flyout, () => new ToolkitPopupPage()),
            new("RatingView", "Star and shape ratings", Icons.Star, () => new RatingViewPage()),
            new("StateContainer", "Loading, empty and error states", Icons.Layers, () => new StateContainerPage()),
            new("UniformItemsLayout", "Equal-size cells", Icons.Dots, () => new UniformItemsLayoutPage()),
            new("Validation Behaviors", "Validate text input", Icons.Shield, () => new ValidationBehaviorsPage()),
        ]),

        new("WebView", [
            new("Load Web", "Load remote web content", Icons.Window, () => new WebViewLoadWebPage()),
            new("Load Local", "Load packaged HTML assets", Icons.Document, () => new WebViewLoadLocalPage()),
            new("Inline HTML", "Render HtmlWebViewSource content", Icons.Terminal, () => new WebViewInlineHtmlPage()),
            new("Navigation", "Back, forward, and reload", Icons.Navigation, () => new WebViewNavigationPage()),
            new("Events", "Navigating and Navigated events", Icons.Bell, () => new WebViewEventsPage()),
            new("Cookies", "CookieContainer integration", Icons.Clipboard, () => new WebViewCookiesPage()),
            new("Playground", "Freeform WebView testing", Icons.Puzzle, () => new WebViewPlaygroundPage()),
        ]),

        new("Effects", [
            new("Effects", "RoutingEffect and PlatformEffect sample", Icons.Sparkle, () => new EffectsPage()),
            new("Clip", "Shape-based clipping samples", Icons.Viewbox, () => new ClipPage()),
            new("Shadow", "Soft elevation and offsets", Icons.Blur, () => new ShadowPage()),
            new("Transformations", "Play with scale, rotation, translation, anchors", Icons.Transform, () => new TransformationsPage()),
        ]),

        new("Shapes", [
            new("Rectangle", "Filled and rounded rectangles", Icons.Rectangle, () => new RectanglePage()),
            new("Ellipse", "Ellipses with fills and strokes", Icons.Ellipse, () => new EllipsePage()),
            new("Line", "Simple and dashed lines", Icons.Line, () => new LinePage()),
            new("Polygon", "Closed polygon shapes", Icons.Polygon, () => new PolygonPage()),
            new("Polyline", "Open polyline shapes", Icons.Polyline, () => new PolylinePage()),
            new("Path", "Paths with custom geometry", Icons.Brush, () => new PathPage()),
            new("RoundRectangle", "Custom corner radii", Icons.RoundRectangle, () => new RoundRectanglePage()),
        ]),

        new("Core", [
            new("Animations", "ViewExtensions animations", Icons.Transition, () => new AnimationPage()),
            new("Behaviors", "Validation Behaviors", Icons.Target, () => new BehaviorsPage()),
            new("Brushes", "Solid and Gradient brushes", Icons.Palette, () => new BrushesPage()),
            new("Drag & Drop", "Drag and drop gestures", Icons.DragDrop, () => new DragAndDropPage()),
            new("Gestures", "Tap, Swipe, Pan and more", Icons.Gesture, () => new GesturesPage()),
            new("Styles", "Styles and Style Classes", Icons.Theme, () => new StylesPage()),
            new("Tooltips", "Tooltips on various elements", Icons.Tooltip, () => new TooltipsPage()),
            new("Triggers", "Visual states and actions", Icons.Lightning, () => new TriggersPage()),
            new("Visual States", "VisualStateManager examples", Icons.Layers, () => new VisualStateManagerPage()),
        ]),

        new("Shell", [
            new("Shell", "Shell samples", Icons.Drawer, () => new ShellPlaygroundPage()),
            new("Xaminals", "Shell with navigation and search", Icons.Tree, () => new ShellPage()),
        ]),

        new("Essentials", [
            new("File Picker", "Pick files from the device", Icons.Document, () => new FilePickerPage()),
            new("Media Picker", "Pick photos and videos from the device", Icons.Image, () => new MediaPickerPage()),
            new("Preferences", "Key/value storage for app settings", Icons.Tune, () => new PreferencesPage()),
            new("Screenshot", "Capture window screenshots", Icons.Monitor, () => new ScreenshotPage()),
        ]),

        new("Embedding", [
            new("Avalonia Embed", "Embedding an Avalonia control in a MAUI app", Icons.Puzzle, () => new AvaloniaEmbedPage()),
            new("MAUI Avalonia View", "Create MAUI control with Avalonia", Icons.Cube, () => new MauiAvaloniaViewPage()),
        ]),

        new("Settings", [
            new("Theme", "Theme toggle and AppThemeBinding", Icons.Theme, () => new ThemePage()),
        ]),
    ];
}

/// <summary>A sample page in the gallery menu.</summary>
public sealed class SampleItem : INotifyPropertyChanged
{
    private static readonly PathGeometryConverter GeometryConverter = new();

    private bool _isSelected;

    public SampleItem(string title, string detail, string iconData, Func<Page> createPage)
    {
        Title = title;
        Detail = detail;
        Icon = (Geometry?)GeometryConverter.ConvertFromInvariantString(iconData);
        CreatePage = createPage;
    }

    public string Title { get; }

    public string Detail { get; }

    public Geometry? Icon { get; }

    public Func<Page> CreatePage { get; }

    /// <summary>Whether this sample is the page shown in the detail area.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>A titled group of samples in the gallery menu. A group with no name has no header.</summary>
public sealed class SampleGroup(string name, IEnumerable<SampleItem> items) : List<SampleItem>(items)
{
    public string Name { get; } = name;

    public bool HasName => !string.IsNullOrEmpty(Name);
}
