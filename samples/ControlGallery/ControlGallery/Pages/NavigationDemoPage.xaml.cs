namespace ControlGallery.Pages;

public partial class NavigationDemoPage : ContentPage
{
    public NavigationDemoPage()
    {
        InitializeComponent();

        // Set BackButtonTitle for this page (shown when navigating to child pages)
        NavigationPage.SetBackButtonTitle(this, "Demo");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateStackInfo();
    }

    private void UpdateStackInfo()
    {
        if (Navigation?.NavigationStack != null)
        {
            StackInfoLabel.Text = $"Stack depth: {Navigation.NavigationStack.Count}";
        }
    }

    // Basic Navigation
    private async void OnPushSimplePage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NavigationChildPage("Simple Page", "This is a simple child page."));
    }

    private async void OnPushPageWithBackTitle(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("Custom Back", "This page has a custom BackButtonTitle. Push another page to see 'Custom Back' in the back button.");
        NavigationPage.SetBackButtonTitle(page, "Custom Back");
        await Navigation.PushAsync(page);
    }

    private async void OnPushPageWithoutNavBar(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("No Nav Bar", "This page has the navigation bar hidden.");
        NavigationPage.SetHasNavigationBar(page, false);
        await Navigation.PushAsync(page);
    }

    // Navigation Bar Colors
    private void OnSetBlueBar(object sender, EventArgs e)
    {
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.DodgerBlue;
            navPage.BarTextColor = Colors.White;
        }
    }

    private void OnSetDarkBar(object sender, EventArgs e)
    {
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Color.FromArgb("#1a1a2e");
            navPage.BarTextColor = Colors.White;
        }
    }

    private void OnSetGreenBar(object sender, EventArgs e)
    {
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.ForestGreen;
            navPage.BarTextColor = Colors.White;
        }
    }

    private void OnResetBar(object sender, EventArgs e)
    {
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = null;
            navPage.BarTextColor = null;
            navPage.BarBackground = null;
        }
    }

    // Gradient Background
    private void OnSetGradientBar(object sender, EventArgs e)
    {
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackground = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Purple, 0.0f),
                    new GradientStop(Colors.DeepPink, 0.5f),
                    new GradientStop(Colors.Orange, 1.0f)
                }
            };
            navPage.BarTextColor = Colors.White;
        }
    }

    private void OnSetRadialGradientBar(object sender, EventArgs e)
    {
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackground = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                Radius = 1.0,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.LightSkyBlue, 0.0f),
                    new GradientStop(Colors.DarkBlue, 1.0f)
                }
            };
            navPage.BarTextColor = Colors.White;
        }
    }

    // Title View and Icon
    private async void OnPushPageWithTitleIcon(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("With Icon", "This page has a title icon next to the title.");
        NavigationPage.SetTitleIconImageSource(page, "dotnet_bot.png");
        await Navigation.PushAsync(page);
    }

    private async void OnPushPageWithTitleView(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("Custom Title", "This page has a custom TitleView.");

        // Create a custom TitleView with search box
        var titleView = new HorizontalStackLayout
        {
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var icon = new Image
        {
            Source = "dotnet_bot.png",
            WidthRequest = 24,
            HeightRequest = 24,
            VerticalOptions = LayoutOptions.Center
        };

        var entry = new Entry
        {
            Placeholder = "Search...",
            WidthRequest = 200,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black
        };

        titleView.Add(icon);
        titleView.Add(entry);

        NavigationPage.SetTitleView(page, titleView);
        await Navigation.PushAsync(page);
    }

    // Icon Color
    private async void OnPushPageWithRedIcon(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("Red Icon", "The back button has a red color.");
        NavigationPage.SetIconColor(page, Colors.Red);
        await Navigation.PushAsync(page);
    }

    private async void OnPushPageWithBlueIcon(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("Blue Icon", "The back button has a blue color.");
        NavigationPage.SetIconColor(page, Colors.Blue);
        await Navigation.PushAsync(page);
    }

    // HasBackButton
    private async void OnPushPageWithoutBackButton(object sender, EventArgs e)
    {
        var page = new NavigationChildPage("No Back", "This page has the back button hidden. Use the button below to go back.")
        {
            ShowManualBackButton = true
        };
        NavigationPage.SetHasBackButton(page, false);
        await Navigation.PushAsync(page);
    }
}

/// <summary>
/// A simple child page for navigation demos.
/// </summary>
public partial class NavigationChildPage : ContentPage
{
    public bool ShowManualBackButton { get; set; }

    public NavigationChildPage(string title, string description)
    {
        Title = title;

        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 20,
            VerticalOptions = LayoutOptions.Center
        };

        layout.Add(new Label
        {
            Text = title,
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        });

        layout.Add(new Label
        {
            Text = description,
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });

        layout.Add(new BoxView
        {
            Color = Colors.LightGray,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill
        });

        var stackInfoLabel = new Label
        {
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Gray
        };
        layout.Add(stackInfoLabel);

        var pushAnotherButton = new Button
        {
            Text = "Push Another Page",
            BackgroundColor = Colors.DodgerBlue,
            TextColor = Colors.White
        };
        pushAnotherButton.Clicked += async (s, e) =>
        {
            var depth = Navigation?.NavigationStack?.Count ?? 0;
            if (Navigation != null)
            {
                await Navigation.PushAsync(new NavigationChildPage($"Page {depth + 1}", $"This is page {depth + 1} in the navigation stack."));
            }
        };
        layout.Add(pushAnotherButton);

        var popButton = new Button
        {
            Text = "Pop (Go Back)",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White
        };
        popButton.Clicked += async (s, e) =>
        {
            await Navigation.PopAsync();
        };
        layout.Add(popButton);

        Content = new ScrollView { Content = layout };

        // Update stack info when appearing
        Appearing += (s, e) =>
        {
            var depth = Navigation?.NavigationStack?.Count ?? 0;
            stackInfoLabel.Text = $"Current stack depth: {depth}";
        };
    }
}
