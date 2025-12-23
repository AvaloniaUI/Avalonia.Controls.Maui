using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class TabbedPageDemoPage : ContentPage
{
    public TabbedPageDemoPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Navigates back to the demo page, restoring the FlyoutPage structure.
    /// </summary>
    private static void NavigateBackToDemo()
    {
        var window = Application.Current?.Windows[0];
        if (window == null) return;

        // If the current page is a FlyoutPage, set its Detail
        if (window.Page is FlyoutPage flyoutPage)
        {
            flyoutPage.Detail = new TabbedPageDemoPage();
        }
        else
        {
            // Otherwise, restore the MainPage with TabbedPageDemoPage as Detail
            var mainPage = new MainPage();
            mainPage.Detail = new TabbedPageDemoPage();
            window.Page = mainPage;
        }
    }

    private void OnOpenBasicTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenIconsTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = new TabbedPage
        {
            Title = "Tabs with Icons",
            BarBackgroundColor = Colors.SlateBlue,
            BarTextColor = Colors.White
        };

        // Create pages with icons
        var page1 = new ContentPage
        {
            Title = "Home",
            IconImageSource = "dotnet_bot.png",
            Content = CreateTabContent("Home Tab", "This tab has an icon")
        };

        var page2 = new ContentPage
        {
            Title = "Settings",
            IconImageSource = "dotnet_logo.png",
            Content = CreateTabContent("Settings Tab", "Another tab with icon")
        };

        var page3 = new ContentPage
        {
            Title = "About",
            Content = CreateTabContent("About Tab", "This tab has no icon")
        };

        tabbedPage.Children.Add(page1);
        tabbedPage.Children.Add(page2);
        tabbedPage.Children.Add(page3);

        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenBlueBarTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        tabbedPage.BarBackgroundColor = Colors.Blue;
        tabbedPage.BarTextColor = Colors.White;
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenDarkBarTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        tabbedPage.BarBackgroundColor = Color.FromArgb("#1a1a2e");
        tabbedPage.BarTextColor = Colors.White;
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenWhiteTextTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        tabbedPage.BarBackgroundColor = Color.FromArgb("#16213e");
        tabbedPage.BarTextColor = Colors.White;
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenRedTextTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        tabbedPage.BarBackgroundColor = Color.FromArgb("#f5f5f5");
        tabbedPage.BarTextColor = Colors.Red;
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenCustomTabColorsTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        // Only set tab selection colors - no BarBackgroundColor to avoid overlap
        tabbedPage.SelectedTabColor = Colors.Orange;
        tabbedPage.UnselectedTabColor = Colors.LightGray;
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenGradientTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = CreateBasicTabbedPage();
        tabbedPage.BarBackground = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.Purple, 0f),
                new GradientStop(Colors.Blue, 0.5f),
                new GradientStop(Colors.Teal, 1f)
            }
        };
        tabbedPage.BarTextColor = Colors.White;
        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private void OnOpenDynamicTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = new TabbedPage
        {
            Title = "Dynamic Tabs",
            BarBackgroundColor = Colors.Indigo,
            BarTextColor = Colors.White
        };

        var controlPage = new ContentPage
        {
            Title = "Controls",
            Content = CreateDynamicControlsContent(tabbedPage)
        };

        tabbedPage.Children.Add(controlPage);
        tabbedPage.Children.Add(new ContentPage
        {
            Title = "Tab 1",
            Content = CreateTabContent("Tab 1", "This is the first dynamic tab.")
        });

        Application.Current?.Windows[0].Page = tabbedPage;
    }

    private TabbedPage CreateBasicTabbedPage()
    {
        var tabbedPage = new TabbedPage
        {
            Title = "TabbedPage Sample"
        };

        tabbedPage.Children.Add(new ContentPage
        {
            Title = "Home",
            Content = CreateTabContent("Home", "Welcome to the TabbedPage sample!")
        });

        tabbedPage.Children.Add(new ContentPage
        {
            Title = "Profile",
            Content = CreateTabContent("Profile", "User profile information goes here.")
        });

        tabbedPage.Children.Add(new ContentPage
        {
            Title = "Settings",
            Content = CreateTabContent("Settings", "Application settings and preferences.")
        });

        // Add a back button to each page
        foreach (var child in tabbedPage.Children)
        {
            if (child is ContentPage page)
            {
                var existingContent = page.Content;
                page.Content = new VerticalStackLayout
                {
                    Spacing = 20,
                    Padding = 20,
                    Children =
                    {
                        new Button
                        {
                            Text = "Back to Demo",
                            BackgroundColor = Colors.Gray,
                            TextColor = Colors.White,
                            Command = new Command(NavigateBackToDemo)
                        },
                        existingContent
                    }
                };
            }
        }

        return tabbedPage;
    }

    private View CreateTabContent(string title, string description)
    {
        return new VerticalStackLayout
        {
            Spacing = 15,
            Padding = 20,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Button
                {
                    Text = "Back to Demo",
                    BackgroundColor = Colors.Gray,
                    TextColor = Colors.White,
                    Command = new Command(NavigateBackToDemo)
                },
                new Label
                {
                    Text = title,
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = description,
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new BoxView
                {
                    Color = Colors.LightGray,
                    HeightRequest = 1,
                    HorizontalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 10)
                },
                new Label
                {
                    Text = $"Content for {title} tab",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }

    private View CreateDynamicControlsContent(TabbedPage tabbedPage)
    {
        var tabCounter = 2;

        var addButton = new Button
        {
            Text = "Add Tab",
            BackgroundColor = Colors.Green,
            TextColor = Colors.White
        };

        var removeButton = new Button
        {
            Text = "Remove Last Tab",
            BackgroundColor = Colors.Red,
            TextColor = Colors.White
        };

        var backButton = new Button
        {
            Text = "Back to Demo",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White
        };

        var tabCountLabel = new Label
        {
            Text = $"Current tab count: {tabbedPage.Children.Count}",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center
        };

        addButton.Clicked += (s, e) =>
        {
            tabbedPage.Children.Add(new ContentPage
            {
                Title = $"Tab {tabCounter}",
                Content = CreateTabContent($"Tab {tabCounter}", $"This is dynamic tab #{tabCounter}")
            });
            tabCounter++;
            tabCountLabel.Text = $"Current tab count: {tabbedPage.Children.Count}";
        };

        removeButton.Clicked += (s, e) =>
        {
            if (tabbedPage.Children.Count > 1)
            {
                tabbedPage.Children.RemoveAt(tabbedPage.Children.Count - 1);
                tabCountLabel.Text = $"Current tab count: {tabbedPage.Children.Count}";
            }
        };

        backButton.Clicked += (s, e) =>
        {
            NavigateBackToDemo();
        };

        return new VerticalStackLayout
        {
            Spacing = 15,
            Padding = 20,
            Children =
            {
                new Label
                {
                    Text = "Dynamic Tab Controls",
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                tabCountLabel,
                addButton,
                removeButton,
                new BoxView { HeightRequest = 20 },
                backButton
            }
        };
    }

    private void OnOpenSelectedItemTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = new TabbedPage
        {
            Title = "SelectedItem TabbedPage",
            BarBackgroundColor = Colors.Indigo,
            BarTextColor = Colors.White
        };

        // Define data items for the tabs
        var tabItems = new List<TabInfo>
        {
            new TabInfo { Title = "First", Description = "First tab content" },
            new TabInfo { Title = "Second", Description = "Second tab content" },
            new TabInfo { Title = "Third", Description = "Third tab content (Selected by default)" },
            new TabInfo { Title = "Fourth", Description = "Fourth tab content" }
        };

        // Set ItemsSource and ItemTemplate
        tabbedPage.ItemsSource = tabItems;
        tabbedPage.ItemTemplate = new DataTemplate(() =>
        {
            var page = new ContentPage();
            page.SetBinding(ContentPage.TitleProperty, "Title");
            
            var content = new VerticalStackLayout
            {
                Spacing = 15,
                Padding = 20,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Button
                    {
                        Text = "Back to Demo",
                        BackgroundColor = Colors.Gray,
                        TextColor = Colors.White,
                        Command = new Command(NavigateBackToDemo)
                    }
                }
            };

            var titleLabel = new Label { FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
            titleLabel.SetBinding(Label.TextProperty, "Title");
            content.Children.Add(titleLabel);

            var descLabel = new Label { FontSize = 16, HorizontalOptions = LayoutOptions.Center };
            descLabel.SetBinding(Label.TextProperty, "Description");
            content.Children.Add(descLabel);

            page.Content = content;
            return page;
        });

        Application.Current?.Windows[0].Page = tabbedPage;

        // Use Dispatcher to set SelectedItem after handler is fully connected
        tabbedPage.Dispatcher.Dispatch(() =>
        {
            tabbedPage.SelectedItem = tabItems[2];
        });
    }

    private void OnOpenItemsSourceTabbedPage(object sender, EventArgs e)
    {
        var tabbedPage = new TabbedPage
        {
            Title = "ItemsSource TabbedPage",
            BarBackgroundColor = Colors.Teal,
            BarTextColor = Colors.White
        };

        // Define data items for the tabs
        var tabItems = new List<TabInfo>
        {
            new TabInfo { Title = "Dashboard", Description = "Overview and statistics" },
            new TabInfo { Title = "Messages", Description = "Your inbox messages" },
            new TabInfo { Title = "Calendar", Description = "Upcoming events" },
            new TabInfo { Title = "Tasks", Description = "Your task list" }
        };

        // Set ItemsSource and ItemTemplate
        tabbedPage.ItemsSource = tabItems;
        tabbedPage.ItemTemplate = new DataTemplate(() =>
        {
            var page = new ContentPage();
            page.SetBinding(ContentPage.TitleProperty, "Title");
            
            var content = new VerticalStackLayout
            {
                Spacing = 15,
                Padding = 20,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Button
                    {
                        Text = "Back to Demo",
                        BackgroundColor = Colors.Gray,
                        TextColor = Colors.White,
                        Command = new Command(NavigateBackToDemo)
                    }
                }
            };

            var titleLabel = new Label { FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
            titleLabel.SetBinding(Label.TextProperty, "Title");
            content.Children.Add(titleLabel);

            var descLabel = new Label { FontSize = 16, HorizontalOptions = LayoutOptions.Center };
            descLabel.SetBinding(Label.TextProperty, "Description");
            content.Children.Add(descLabel);

            page.Content = content;
            return page;
        });

        Application.Current?.Windows[0].Page = tabbedPage;
    }
}

public class TabInfo
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

