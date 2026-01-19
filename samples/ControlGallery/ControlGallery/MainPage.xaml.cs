using System.Collections.ObjectModel;
using System.Windows.Input;
using ControlGallery.Pages;

namespace ControlGallery;

public partial class MainPage : FlyoutPage
{
    private List<SampleGroup> _allSamples = new List<SampleGroup>();

    // Static page factory for AOT compatibility - no reflection
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
        [typeof(NavigationDemoPage)] = () => new NavigationPage(new NavigationDemoPage()),
        [typeof(ControlGallery.Pages.TabbedPage)] = () => new ControlGallery.Pages.TabbedPage(),
        [typeof(TitleBarPage)] = () => new TitleBarPage(),
        [typeof(PopupsPage)] = () => new PopupsPage(),
        [typeof(ToolbarItemPage)] = () => new NavigationPage(new ToolbarItemPage()),
        // Layout
        [typeof(StackLayoutPage)] = () => new StackLayoutPage(),
        [typeof(GridPage)] = () => new GridPage(),
        [typeof(FlexLayoutPage)] = () => new FlexLayoutPage(),
        [typeof(AbsoluteLayoutPage)] = () => new AbsoluteLayoutPage(),
        // Views
        [typeof(ActivityIndicatorPage)] = () => new ActivityIndicatorPage(),
        [typeof(BorderPage)] = () => new BorderPage(),
        [typeof(BoxViewPage)] = () => new BoxViewPage(),
        [typeof(ButtonPage)] = () => new ButtonPage(),
        [typeof(CheckBoxPage)] = () => new CheckBoxPage(),
        [typeof(CollectionViewPage)] = () => new CollectionViewPage(),
        [typeof(ContentViewPage)] = () => new ContentViewPage(),
        [typeof(DatePickerPage)] = () => new DatePickerPage(),
        [typeof(EditorPage)] = () => new EditorPage(),
        [typeof(FramePage)] = () => new FramePage(),
        [typeof(GraphicsViewPage)] = () => new GraphicsViewPage(),
        [typeof(ImagePage)] = () => new ImagePage(),
        [typeof(ImageButtonPage)] = () => new ImageButtonPage(),
        [typeof(PickerPage)] = () => new PickerPage(),
        [typeof(ProgressBarPage)] = () => new ProgressBarPage(),
        [typeof(RadioButtonPage)] = () => new RadioButtonPage(),
        [typeof(ScrollViewPage)] = () => new ScrollViewPage(),
        [typeof(SearchBarPage)] = () => new SearchBarPage(),
        [typeof(SliderPage)] = () => new SliderPage(),
        [typeof(StepperPage)] = () => new StepperPage(),
        [typeof(SwipeViewPage)] = () => new SwipeViewPage(),
        [typeof(SwitchPage)] = () => new SwitchPage(),
        [typeof(TableViewPage)] = () => new TableViewPage(),
        [typeof(TimePickerPage)] = () => new TimePickerPage(),
        // Effects
        [typeof(ClipPage)] = () => new ClipPage(),
        [typeof(ShadowPage)] = () => new ShadowPage(),
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
        [typeof(GesturesPage)] = () => new GesturesPage(),
        [typeof(StylesPage)] = () => new StylesPage(),
        [typeof(TooltipsPage)] = () => new TooltipsPage(),
        [typeof(TriggersPage)] = () => new TriggersPage(),
        [typeof(VisualStateManagerPage)] = () => new VisualStateManagerPage(),
        // Settings
        [typeof(ThemePage)] = () => new ThemePage(),
    };

    public ObservableCollection<SampleGroup> FilteredSamples { get; private set; } = new ObservableCollection<SampleGroup>();
    public ICommand NavigateCommand { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        NavigateCommand = new Command<Type>(NavigateToPage);

        InitializeSamples();
        UpdateMenu(string.Empty);

        // Navigate to Welcome Page by default
        Detail = new WelcomePage();
    }

    private void UpdateMenu(string searchText)
    {
        var root = new TableRoot();

        foreach (var group in _allSamples)
        {
            var section = new TableSection(group.Name);
            bool hasItems = false;

            foreach (var item in group)
            {
                if (string.IsNullOrWhiteSpace(searchText) || 
                    item.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
                    item.Detail.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    var cell = new TextCell
                    {
                        Text = item.Title,
                        Detail = item.Detail,
                        Command = NavigateCommand,
                        CommandParameter = item.PageType
                    };

                    cell.SetAppThemeColor(TextCell.TextColorProperty, Colors.Black, Colors.White);
                    cell.SetAppThemeColor(TextCell.DetailColorProperty, Colors.Gray, Colors.LightGray);

                    section.Add(cell);
                    hasItems = true;
                }
            }

            if (hasItems)
            {
                root.Add(section);
            }
        }

        MenuTableView.Root = root;
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
                new("Popups", "Alerts, ActionSheets, and Prompts", typeof(PopupsPage))
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
                new("ActivityIndicator", "ActivityIndicator control", typeof(ActivityIndicatorPage)),
                new("Border", "Border with shapes and strokes", typeof(BorderPage)),
                new("BoxView", "Simple colored rectangles", typeof(BoxViewPage)),
                new("Button", "Button control", typeof(ButtonPage)),
                new("CheckBox", "CheckBox control for selections", typeof(CheckBoxPage)),
                new("CollectionView", "Collection display with templates", typeof(CollectionViewPage)),
                new("ContentView", "Custom content", typeof(ContentViewPage)),
                new("DatePicker", "Date picker control", typeof(DatePickerPage)),
                new("Editor", "Editor control", typeof(EditorPage)),
                new("Frame", "Frame control", typeof(FramePage)),
                new("GraphicsView", "Custom drawing and graphics", typeof(GraphicsViewPage)),
                new("Image", "Image display with various sources", typeof(ImagePage)),
                new("ImageButton", "ImageButton control", typeof(ImageButtonPage)),
                new("Picker", "Picker control", typeof(PickerPage)),
                new("ProgressBar", "Progress indicator control", typeof(ProgressBarPage)),
                new("RadioButton", "RadioButton control", typeof(RadioButtonPage)),
                new("ScrollView", "Scroll scenarios and behaviors", typeof(ScrollViewPage)),
                new("SearchBar", "Search input control", typeof(SearchBarPage)),
                new("Slider", "Slider control", typeof(SliderPage)),
                new("Stepper", "Numeric increment/decrement control", typeof(StepperPage)),
                new("SwipeView", "SwipeView control", typeof(SwipeViewPage)),
                new("Switch", "Toggle control with colors", typeof(SwitchPage)),
                new("TableView", "TableView with cell types", typeof(TableViewPage)),
                new("TimePicker", "TimePicker control", typeof(TimePickerPage))
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
                new("Gestures", "Tap, Swipe, Pan and more", typeof(GesturesPage)),
                new("Styles", "Styles and Style Classes", typeof(StylesPage)),
                new("Tooltips", "Tooltips on various controls", typeof(TooltipsPage)),
                new("Triggers", "Visual states and actions", typeof(TriggersPage)),
                new("Visual States", "VisualStateManager examples", typeof(VisualStateManagerPage)),
            }),

            new SampleGroup("Settings", new List<SampleItem>
            {
                new("Theme", "Theme toggle and AppThemeBinding", typeof(ThemePage))
            })
        };
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchBar = (SearchBar)sender;
        UpdateMenu(searchBar.Text ?? string.Empty);
    }

    private void NavigateToPage(Type pageType)
    {
        if (PageFactory.TryGetValue(pageType, out var factory))
        {
            Detail = factory();
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
