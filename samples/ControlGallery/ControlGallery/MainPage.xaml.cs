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
            nameof(ActivityIndicatorPage) => new ActivityIndicatorPage(),
            nameof(BorderPage) => new BorderPage(),
            nameof(BoxViewPage) => new BoxViewPage(),
            nameof(ButtonPage) => new ButtonPage(),
            nameof(CheckBoxPage) => new CheckBoxPage(),
            nameof(ClipPage) => new ClipPage(),
            nameof(CollectionViewPage) => new CollectionViewPage(),
            nameof(ContentViewPage) => new ContentViewPage(),
            nameof(EllipsePage) => new EllipsePage(),
            nameof(FontsPage) => new FontsPage(),
            nameof(GraphicsViewPage) => new GraphicsViewPage(),
            nameof(ImageButtonPage) => new ImageButtonPage(),
            nameof(ImagePage) => new ImagePage(),
            nameof(IndicatorViewPage) => new IndicatorViewPage(),
            nameof(LinePage) => new LinePage(),
            nameof(NavigationDemoPage) => new NavigationPage(new NavigationDemoPage()),
            nameof(PathPage) => new PathPage(),
            nameof(PickerPage) => new PickerPage(),
            nameof(PolygonPage) => new PolygonPage(),
            nameof(PolylinePage) => new PolylinePage(),
            nameof(ProgressBarPage) => new ProgressBarPage(),
            nameof(RadioButtonPage) => new RadioButtonPage(),
            nameof(RectanglePage) => new RectanglePage(),
            nameof(RoundRectanglePage) => new RoundRectanglePage(),
            nameof(ScrollViewPage) => new ScrollViewPage(),
            nameof(SearchBarPage) => new SearchBarPage(),
            nameof(ShadowPage) => new ShadowPage(),
            nameof(SliderPage) => new SliderPage(),
            nameof(StepperPage) => new StepperPage(),
            nameof(SwipeViewPage) => new SwipeViewPage(),
            nameof(SwitchPage) => new SwitchPage(),
            nameof(TableViewPage) => new TableViewPage(),
            nameof(ThemePage) => new ThemePage(),
            nameof(TitleBarPage) => new TitleBarPage(),
            nameof(TransformationsPage) => new TransformationsPage(),
            "MainPage" when pageType.Namespace == "RpnCalculator" => new RpnCalculator.MainPage(),
            "MainPage" when pageType.Namespace == "Weather" => new Weather.MainPage(),
            "MainPage" when pageType.Namespace == "WordPuzzle" => new WordPuzzle.MainPage(),
            "SolitairePage" when pageType.Namespace == "SolitaireEncryption" => new SolitaireEncryption.SolitairePage(),
            "TipCalcPage" when pageType.Namespace == "TipCalc" => new TipCalc.TipCalcPage(),
            _ => throw new ArgumentException($"Unknown page type: {pageType.FullName}", nameof(pageType))
        };

        Detail = page;
    }
}
