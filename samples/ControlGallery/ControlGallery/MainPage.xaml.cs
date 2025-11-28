using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ControlGallery.Pages;
using WordPuzzle;

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
            nameof(ActivityIndicatorPage) => new ActivityIndicatorPage(),
            nameof(ButtonPage) => new ButtonPage(),
            nameof(CheckBoxPage) => new CheckBoxPage(),
            nameof(ProgressBarPage) => new ProgressBarPage(),
            nameof(RectanglePage) => new RectanglePage(),
            nameof(EllipsePage) => new EllipsePage(),
            nameof(LinePage) => new LinePage(),
            nameof(PolygonPage) => new PolygonPage(),
            nameof(PolylinePage) => new PolylinePage(),
            nameof(PathPage) => new PathPage(),
            nameof(RoundRectanglePage) => new RoundRectanglePage(),
            nameof(ThemePage) => new ThemePage(),
            "MainPage" when pageType.Namespace == "WordPuzzle" => new WordPuzzle.MainPage(),
            _ => throw new ArgumentException($"Unknown page type: {pageType.FullName}", nameof(pageType))
        };

        Detail = page;
    }
}
