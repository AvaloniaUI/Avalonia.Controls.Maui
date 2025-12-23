using System.Collections.ObjectModel;
using System.Windows.Input;
using ControlGallery.Pages;

namespace ControlGallery;

public partial class MainPage : FlyoutPage
{
    private List<SampleGroup> _allSamples = new List<SampleGroup>();

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
                new("NavigationPage", "Navigation stack", typeof(NavigationDemoPage)),
                new("TabbedPage", "Tabbed navigation", typeof(ControlGallery.Pages.TabbedPage)),
                new("TitleBar", "Custom window title bar", typeof(TitleBarPage))
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
                new("BoxView", "Simple colored rectangles", typeof(BoxViewPage)),
                new("Border", "Border with shapes and strokes", typeof(BorderPage)),
                new("Button", "Button control", typeof(ButtonPage)),
                new("CollectionView", "Collection display with templates", typeof(CollectionViewPage)),
                new("ContentView", "Custom content", typeof(ContentViewPage)),
                new("DatePicker", "Date picker control", typeof(DatePickerPage)),
                new("GraphicsView", "Custom drawing and graphics", typeof(GraphicsViewPage)),
                new("Image", "Image display with various sources", typeof(ImagePage)),
                new("ImageButton", "ImageButton control", typeof(ImageButtonPage)),
                new("CheckBox", "CheckBox control for selections", typeof(CheckBoxPage)),
                new("Picker", "Picker control", typeof(PickerPage)),
                new("ProgressBar", "Progress indicator control", typeof(ProgressBarPage)),
                new("RadioButton", "RadioButton control", typeof(RadioButtonPage)),
                new("ScrollView", "Scroll scenarios and behaviors", typeof(ScrollViewPage)),
                new("SearchBar", "Search input control", typeof(SearchBarPage)),
                new("Slider", "Slider control", typeof(SliderPage)),
                new("Stepper", "Numeric increment/decrement control", typeof(StepperPage)),
                new("Switch", "Toggle control with colors", typeof(SwitchPage)),
                new("SwipeView", "SwipeView control", typeof(SwipeViewPage)),
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
                new("Tooltips", "Tooltips on various controls", typeof(TooltipsPage)),
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
        Page? page = null;

        // Custom instantiation logic for specific pages if needed
        if (pageType == typeof(NavigationDemoPage))
        {
            page = new NavigationPage(new NavigationDemoPage());
        }
        else
        {
            page = (Page)Activator.CreateInstance(pageType)!;
        }

        Detail = page;
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
