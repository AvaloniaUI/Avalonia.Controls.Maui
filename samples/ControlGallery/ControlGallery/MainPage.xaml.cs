using System.Windows.Input;
using ControlGallery.Pages;

namespace ControlGallery;

public partial class MainPage : FlyoutPage
{
    public ICommand NavigateCommand { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        NavigateCommand = new Command<Type>(NavigateToPage);

        FlyoutPageMenu.BindingContext = this;
    }

    private void NavigateToPage(Type pageType)
    {
        Page page = pageType.Name switch
        {
            nameof(FontsPage) => new FontsPage(),
            nameof(NavigationDemoPage) => new NavigationPage(new NavigationDemoPage()),
            nameof(ActivityIndicatorPage) => new ActivityIndicatorPage(),
            nameof(BoxViewPage) => new BoxViewPage(),
            nameof(BorderPage) => new BorderPage(),
            nameof(ButtonPage) => new ButtonPage(),
            nameof(ImageButtonPage) => new ImageButtonPage(),
            nameof(CheckBoxPage) => new CheckBoxPage(),
            nameof(ImagePage) => new ImagePage(),
            nameof(ProgressBarPage) => new ProgressBarPage(),
            nameof(SwipeViewPage) => new SwipeViewPage(),
            nameof(RectanglePage) => new RectanglePage(),
            nameof(EllipsePage) => new EllipsePage(),
            nameof(TransformationsPage) => new TransformationsPage(),
            nameof(LinePage) => new LinePage(),
            nameof(PolygonPage) => new PolygonPage(),
            nameof(PolylinePage) => new PolylinePage(),
            nameof(PathPage) => new PathPage(),
            nameof(RoundRectanglePage) => new RoundRectanglePage(),
            nameof(ScrollViewPage) => new ScrollViewPage(),
            nameof(SearchBarPage) => new SearchBarPage(),
            nameof(SliderPage) => new SliderPage(),
            nameof(SwitchPage) => new SwitchPage(),
            nameof(RadioButtonPage) => new RadioButtonPage(),
            nameof(ContentViewPage) => new ContentViewPage(),
            nameof(ClipPage) => new ClipPage(),
            nameof(ShadowPage) => new ShadowPage(),
            nameof(TableViewPage) => new TableViewPage(),
            nameof(GraphicsViewPage) => new GraphicsViewPage(),
            nameof(ThemePage) => new ThemePage(),
            nameof(TitleBarPage) => new TitleBarPage(),
            nameof(CollectionViewPage) => new CollectionViewPage(),
            nameof(PickerPage) => new PickerPage(),
            nameof(StepperPage) => new StepperPage(),
            nameof(StackLayoutPage) => new StackLayoutPage(),
            nameof(GridPage) => new GridPage(),
            nameof(FlexLayoutPage) => new FlexLayoutPage(),
            nameof(AbsoluteLayoutPage) => new AbsoluteLayoutPage(),
            "MainPage" when pageType.Namespace == "RpnCalculator" => new RpnCalculator.MainPage(),

            "SolitairePage" when pageType.Namespace == "SolitaireEncryption" => new SolitaireEncryption.SolitairePage(),
            "TipCalcPage" when pageType.Namespace == "TipCalc" => new TipCalc.TipCalcPage(),
            "MainPage" when pageType.Namespace == "Weather" => new Weather.MainPage(),
            "MainPage" when pageType.Namespace == "WordPuzzle" => new WordPuzzle.MainPage(),
            _ => throw new ArgumentException($"Unknown page type: {pageType.FullName}", nameof(pageType))
        };

        Detail = page;
    }
}
