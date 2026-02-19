namespace ControlGallery.Pages.ShellSamples.ShellPlayground;

public partial class ShellAppearancePage : ContentPage
{
    private bool _hasTitleView = true;
    private int _titleViewUpdateCount;

    public ShellAppearancePage()
    {
        InitializeComponent();
        Shell.SetTitleView(this, CreateTitleView("Custom TitleView", Colors.Yellow));
        BindingContext = this;
    }

    private static View CreateTitleView(string text, Color color)
    {
        return new Label
        {
            Text = text,
            TextColor = color,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
    }

    private void OnToggleNavBar(object sender, EventArgs e)
    {
        bool isVisible = Shell.GetNavBarIsVisible(this);
        Shell.SetNavBarIsVisible(this, !isVisible);

        var shell = this.GetShell();
        shell?.Handler?.UpdateValue("NavBarIsVisible");
    }

    private void OnToggleTabBar(object sender, EventArgs e)
    {
        bool isVisible = Shell.GetTabBarIsVisible(this);
        Shell.SetTabBarIsVisible(this, !isVisible);

        var shell = this.GetShell();
        shell?.Handler?.UpdateValue("TabBarIsVisible");
    }

    private void OnToggleTitleView(object sender, EventArgs e)
    {
        _hasTitleView = !_hasTitleView;
        Shell.SetTitleView(this, _hasTitleView
            ? CreateTitleView("Custom TitleView", Colors.Yellow)
            : null);

        var shell = this.GetShell();
        shell?.Handler?.UpdateValue("TitleView");
    }

    private void OnUpdateTitleView(object sender, EventArgs e)
    {
        _titleViewUpdateCount++;
        Shell.SetTitleView(this, CreateTitleView($"Updated TitleView ({_titleViewUpdateCount})", Colors.Orange));
        _hasTitleView = true;

        var shell = this.GetShell();
        shell?.Handler?.UpdateValue("TitleView");
    }

    private async void OnNavigate(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string mode)
            return;

        var sectionNav = Navigation;
        bool isModal = mode == "modal" || mode == "modalnotanimated";

        var page = new ContentPage
        {
            Title = $"Navigation: {mode}",
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = $"Mode: {mode}",
                        HorizontalOptions = LayoutOptions.Center,
                        FontSize = 18
                    },
                    new Button
                    {
                        Text = "Go Back",
                        Command = new Command(async () =>
                        {
                            if (isModal)
                                await sectionNav.PopModalAsync();
                            else
                                await sectionNav.PopAsync();
                        })
                    }
                }
            }
        };

        // Use the standard MAUI PresentationMode API.
        // Modal modes are handled by MauiAvaloniaWindow as fullscreen in-window overlays.
        var presentationMode = mode switch
        {
            "animated" => PresentationMode.Animated,
            "notanimated" => PresentationMode.NotAnimated,
            "modal" => PresentationMode.ModalAnimated,
            "modalnotanimated" => PresentationMode.ModalNotAnimated,
            _ => PresentationMode.Animated
        };
        Shell.SetPresentationMode(page, presentationMode);

        await sectionNav.PushAsync(page);
    }
}
