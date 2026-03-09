using System.Collections.ObjectModel;
using System.Windows.Input;
using ControlGallery.Pages;
using ControlGallery.Pages.ShellSamples;
using ControlGallery.Pages.ShellSamples.ShellPlayground;

namespace ControlGallery;

public partial class MainPage : FlyoutPage
{
    private List<SampleGroup> _allSamples = new List<SampleGroup>();
    private string _lastSearchText = string.Empty;

    private static readonly Dictionary<Type, Func<Page>> PageFactory = new()
    {
        // Apps
        [typeof(RpnCalculator.MainPage)] = () => new RpnCalculator.MainPage(),
        [typeof(SolitaireEncryption.SolitairePage)] = () => new SolitaireEncryption.SolitairePage(),
        [typeof(TipCalc.TipCalcPage)] = () => new TipCalc.TipCalcPage(),
        [typeof(Weather.MainPage)] = () => new Weather.MainPage(),
        [typeof(WordPuzzle.MainPage)] = () => new WordPuzzle.MainPage(),
        // Services
        [typeof(FontsPage)] = () => new FontsPage(),
        // Pages
        [typeof(NavigationDemoPage)] = () => new NavigationDemoPage(),
        [typeof(ControlGallery.Pages.TabbedPage)] = () => new ControlGallery.Pages.TabbedPage(),
        [typeof(TitleBarPage)] = () => new TitleBarPage(),
        [typeof(PopupsPage)] = () => new PopupsPage(),
        [typeof(ToolbarItemPage)] = () => new ToolbarItemPage(),
        [typeof(ShellPage)] = () => new ShellPage(),
        [typeof(ShellPlaygroundPage)] = () => new ShellPlaygroundPage(),
        // Views
        [typeof(ActivityIndicatorPage)] = () => new ActivityIndicatorPage(),
        [typeof(BorderPage)] = () => new BorderPage(),
        [typeof(BoxViewPage)] = () => new BoxViewPage(),
        [typeof(ButtonPage)] = () => new ButtonPage(),
        [typeof(CheckBoxPage)] = () => new CheckBoxPage(),
        [typeof(CollectionViewPage)] = () => new CollectionViewPage(),
        [typeof(ContentViewPage)] = () => new ContentViewPage(),
        [typeof(DatePickerPage)] = () => new DatePickerPage(),
        [typeof(EntryPage)] = () => new EntryPage(),
        [typeof(EditorPage)] = () => new EditorPage(),
        [typeof(FramePage)] = () => new FramePage(),
        [typeof(GraphicsViewPage)] = () => new GraphicsViewPage(),
        [typeof(ImagePage)] = () => new ImagePage(),
        [typeof(ImageButtonPage)] = () => new ImageButtonPage(),
        [typeof(ListViewPage)] = () => new ListViewPage(),
        [typeof(MapPage)] = () => new MapPage(),
        [typeof(PickerPage)] = () => new PickerPage(),
        [typeof(ProgressBarPage)] = () => new ProgressBarPage(),
        [typeof(RadioButtonPage)] = () => new RadioButtonPage(),
        [typeof(ScrollViewPage)] = () => new ScrollViewPage(),
        [typeof(SearchBarPage)] = () => new SearchBarPage(),
        [typeof(SliderPage)] = () => new SliderPage(),
        [typeof(StepperPage)] = () => new StepperPage(),
        [typeof(RefreshViewPage)] = () => new RefreshViewPage(),
        [typeof(SwipeViewPage)] = () => new SwipeViewPage(),
        [typeof(SwitchPage)] = () => new SwitchPage(),
        [typeof(TableViewPage)] = () => new TableViewPage(),
        [typeof(TimePickerPage)] = () => new TimePickerPage(),
        // Effects
        [typeof(ClipPage)] = () => new ClipPage(),
        [typeof(ShadowPage)] = () => new ShadowPage(),
        // Layout
        [typeof(StackLayoutPage)] = () => new StackLayoutPage(),
        [typeof(GridPage)] = () => new GridPage(),
        [typeof(FlexLayoutPage)] = () => new FlexLayoutPage(),
        [typeof(AbsoluteLayoutPage)] = () => new AbsoluteLayoutPage(),
        [typeof(TransformationsPage)] = () => new TransformationsPage(),
        // Shapes
        [typeof(RectanglePage)] = () => new RectanglePage(),
        [typeof(EllipsePage)] = () => new EllipsePage(),
        [typeof(LinePage)] = () => new LinePage(),
        [typeof(PolygonPage)] = () => new PolygonPage(),
        [typeof(PolylinePage)] = () => new PolylinePage(),
        [typeof(PathPage)] = () => new PathPage(),
        [typeof(RoundRectanglePage)] = () => new RoundRectanglePage(),
        // Core
        [typeof(AnimationPage)] = () => new AnimationPage(),
        [typeof(BehaviorsPage)] = () => new BehaviorsPage(),
        [typeof(BrushesPage)] = () => new BrushesPage(),
        [typeof(DragAndDropPage)] = () => new DragAndDropPage(),
        [typeof(GesturesPage)] = () => new GesturesPage(),
        [typeof(StylesPage)] = () => new StylesPage(),
        [typeof(TooltipsPage)] = () => new TooltipsPage(),
        [typeof(TriggersPage)] = () => new TriggersPage(),
        [typeof(VisualStateManagerPage)] = () => new VisualStateManagerPage(),
        [typeof(LifecycleEventsPage)] = () => new LifecycleEventsPage(),
        // Essentials
        [typeof(ScreenshotPage)] = () => new ScreenshotPage(),
        [typeof(PreferencesPage)] = () => new PreferencesPage(),
        [typeof(FilePickerPage)] = () => new FilePickerPage(),
        [typeof(Pages.Essentials.DeviceDisplayPage)] = () => new Pages.Essentials.DeviceDisplayPage(),
        [typeof(Pages.Essentials.DeviceInfoPage)] = () => new Pages.Essentials.DeviceInfoPage(),
        [typeof(Pages.Essentials.BatteryPage)] = () => new Pages.Essentials.BatteryPage(),
        // Settings
        [typeof(ThemePage)] = () => new ThemePage(),
        // Embedding
        [typeof(AvaloniaEmbedPage)] = () => new AvaloniaEmbedPage(),
        [typeof(MauiAvaloniaViewPage)] = () => new MauiAvaloniaViewPage(),
    };

    public ObservableCollection<SampleGroup> FilteredSamples { get; private set; } = new ObservableCollection<SampleGroup>();
    public ICommand NavigateCommand { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        BindingContext = this;
        NavigateCommand = new Command<Type>(NavigateToPage);

        InitializeSamples();
        UpdateMenu(string.Empty);

        // Navigate to Welcome Page by default
        Detail = new NavigationPage(new WelcomePage());
    }

    private void UpdateMenu(string searchText)
    {
        FilteredSamples.Clear();

        foreach (var group in _allSamples)
        {
            var filteredItems = group.Where(item =>
                string.IsNullOrWhiteSpace(searchText) ||
                item.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                item.Detail.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filteredItems.Count > 0)
            {
                FilteredSamples.Add(new SampleGroup(group.Name, filteredItems));
            }
        }
    }

    private void OnMenuSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SampleItem selectedItem)
        {
            NavigateToPage(selectedItem.PageType);

            // Clear selection so the same item can be tapped again
            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
    }

    private void InitializeSamples()
    {
        _allSamples = new List<SampleGroup>
        {
            new SampleGroup("Apps", new List<SampleItem>
            {
                new("RpnCalculator", "A Reverse Polish Notation calc", typeof(RpnCalculator.MainPage)),
                new("SolitaireEncryption", "Solitaire encryption algorithm", typeof(SolitaireEncryption.SolitairePage)),
                new("Tip Calc", "TipCalc application", typeof(TipCalc.TipCalcPage)),
                new("Weather", "Retrieves weather data", typeof(Weather.MainPage)),
                new("Word Puzzle", "A word puzzle game", typeof(WordPuzzle.MainPage))
            }),

            new SampleGroup("Services", new List<SampleItem>
            {
                new("Fonts", "Font integration test", typeof(FontsPage))
            }),

            new SampleGroup("Pages", new List<SampleItem>
            {
                new("NavigationPage", "Navigation stack with animated transitions", typeof(NavigationDemoPage)),
                new("TabbedPage", "Tabbed navigation", typeof(ControlGallery.Pages.TabbedPage)),
                new("TitleBar", "Custom window title bar", typeof(TitleBarPage)),
                new("ToolbarItems", "Toolbar items and interactions", typeof(ToolbarItemPage)),
                new("Popups", "Alerts, ActionSheets, and Prompts", typeof(PopupsPage)),
                new("Lifecycle Events", "View & VisualElement event tracing", typeof(LifecycleEventsPage))
            }),

            new SampleGroup("Layout", new List<SampleItem>
            {
                new("StackLayout", "Vertical and horizontal stacking", typeof(StackLayoutPage)),
                new("Grid", "Rows, columns, and spanning", typeof(GridPage)),
                new("FlexLayout", "Flexible box layout", typeof(FlexLayoutPage)),
                new("AbsoluteLayout", "Absolute positioning of elements", typeof(AbsoluteLayoutPage))
            }),

            new SampleGroup("Views", new List<SampleItem>
            {
                new("ActivityIndicator", "Animated busy indicator", typeof(ActivityIndicatorPage)),
                new("Border", "Custom strikes and shapes", typeof(BorderPage)),
                new("BoxView", "Decorative colored rectangles", typeof(BoxViewPage)),
                new("Button", "Standard clickable button", typeof(ButtonPage)),
                new("CheckBox", "Toggle selection control", typeof(CheckBoxPage)),
                new("CollectionView", "Modern templated list", typeof(CollectionViewPage)),
                new("ContentView", "Reusable custom content", typeof(ContentViewPage)),
                new("DatePicker", "Date selection picker", typeof(DatePickerPage)),
                new("Entry", "Single-line text entry", typeof(EntryPage)),
                new("Editor", "Multi-line text editor", typeof(EditorPage)),
                new("Frame", "Bordered layout container", typeof(FramePage)),
                new("GraphicsView", "Custom 2D drawing canvas", typeof(GraphicsViewPage)),
                new("Image", "Visual content display", typeof(ImagePage)),
                new("ImageButton", "Interactive image button", typeof(ImageButtonPage)),
                new("ListView", "Scrolling data items", typeof(ListViewPage)),
                // TODO: Re-enable when Map is ready
                //new("Map", "Interactive maps and pins", typeof(MapPage)),
                new("Picker", "Item selection dropdown", typeof(PickerPage)),
                new("ProgressBar", "Visual progress status", typeof(ProgressBarPage)),
                new("RadioButton", "Single-select option list", typeof(RadioButtonPage)),
                new("RefreshView", "Pull-to-refresh container", typeof(RefreshViewPage)),
                new("ScrollView", "Scrollable layout container", typeof(ScrollViewPage)),
                new("SearchBar", "Search text input", typeof(SearchBarPage)),
                new("Slider", "Range value selection", typeof(SliderPage)),
                new("Stepper", "Discrete incremental changes", typeof(StepperPage)),
                new("SwipeView", "Swipe action container", typeof(SwipeViewPage)),
                new("Switch", "Binary toggle switch", typeof(SwitchPage)),
                new("TableView", "Form-based data table", typeof(TableViewPage)),
                new("TimePicker", "Time selection picker", typeof(TimePickerPage))
            }),

            new SampleGroup("Effects", new List<SampleItem>
            {
                new("Clip", "Shape-based clipping samples", typeof(ClipPage)),
                new("Shadow", "Soft elevation and offsets", typeof(ShadowPage)),
                new("Transformations", "Play with scale, rotation, translation, anchors", typeof(TransformationsPage))
            }),

            new SampleGroup("Shapes", new List<SampleItem>
            {
                new("Rectangle", "Filled and rounded rectangles", typeof(RectanglePage)),
                new("Ellipse", "Ellipses with fills and strokes", typeof(EllipsePage)),
                new("Line", "Simple and dashed lines", typeof(LinePage)),
                new("Polygon", "Closed polygon shapes", typeof(PolygonPage)),
                new("Polyline", "Open polyline shapes", typeof(PolylinePage)),
                new("Path", "Paths with custom geometry", typeof(PathPage)),
                new("RoundRectangle", "Custom corner radii", typeof(RoundRectanglePage))
            }),

            new SampleGroup("Core", new List<SampleItem>
            {
                new("Animations", "ViewExtensions animations", typeof(AnimationPage)),
                new("Behaviors", "Validation Behaviors", typeof(BehaviorsPage)),
                new("Brushes", "Solid and Gradient brushes", typeof(BrushesPage)),
                new("Drag & Drop", "Drag and drop gestures", typeof(DragAndDropPage)),
                new("Gestures", "Tap, Swipe, Pan and more", typeof(GesturesPage)),
                new("Styles", "Styles and Style Classes", typeof(StylesPage)),
                new("Tooltips", "Tooltips on various elements", typeof(TooltipsPage)),
                new("Triggers", "Visual states and actions", typeof(TriggersPage)),
                new("Visual States", "VisualStateManager examples", typeof(VisualStateManagerPage)),
            }),

            new SampleGroup("Shell", new List<SampleItem>
            {
                new("Shell", "Shell samples", typeof(ShellPlaygroundPage)),
                new("Xaminals", "Shell with navigation and search", typeof(ShellPage)),
            }),

            new SampleGroup("Essentials", new List<SampleItem>
            {
                new("File Picker", "Pick files from the device", typeof(FilePickerPage)),
                new("Preferences", "Key/value storage for app settings", typeof(PreferencesPage)),
                new("Screenshot", "Capture window screenshots", typeof(ScreenshotPage)),
            }),

            new SampleGroup("Essentials Device", new List<SampleItem>
            {
                new("Device Display", "Screen metrics and orientation", typeof(ControlGallery.Pages.Essentials.DeviceDisplayPage)),
                new("Device Info", "Hardware and platform details", typeof(ControlGallery.Pages.Essentials.DeviceInfoPage)),
                new("Battery", "Charge level and power status", typeof(ControlGallery.Pages.Essentials.BatteryPage)),
            }),

            new SampleGroup("Embedding", new List<SampleItem>
            {
                new("Avalonia Embed", "Embedding an Avalonia control in a MAUI app", typeof(AvaloniaEmbedPage)),
                new("MAUI Avalonia View", "Create MAUI control with Avalonia", typeof(MauiAvaloniaViewPage))
            }),

            new SampleGroup("Settings", new List<SampleItem>
            {
                new("Theme", "Theme toggle and AppThemeBinding", typeof(ThemePage))
            })
        };
    }

    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        _lastSearchText = e.NewTextValue ?? string.Empty;
        UpdateMenu(_lastSearchText);
    }

    private void NavigateToPage(Type pageType)
    {
        if (PageFactory.TryGetValue(pageType, out var factory))
        {
            var page = factory();

            if (Detail is NavigationPage navPage)
            {
                navPage.Navigation.InsertPageBefore(page, navPage.RootPage);
                navPage.Navigation.PopToRootAsync(animated: false);
            }
            else
            {
                Detail = new NavigationPage(page);
            }

            IsPresented = false;
        }
    }
}

public class SampleItem
{
    public string Title { get; }
    public string Detail { get; }
    public Type PageType { get; }

    public SampleItem(string title, string detail, Type pageType)
    {
        Title = title;
        Detail = detail;
        PageType = pageType;
    }
}

public class SampleGroup : List<SampleItem>
{
    public string Name { get; }

    public SampleGroup(string name, List<SampleItem> items) : base(items)
    {
        Name = name;
    }
}