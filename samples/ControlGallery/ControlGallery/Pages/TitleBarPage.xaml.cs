namespace ControlGallery.Pages;

public partial class TitleBarPage : ContentPage
{
    public TitleBarPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateTitleBarInfo();
    }

    private void OnSetSimpleTitleBar(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            var titleBar = new TitleBar
            {
                Title = "Control Gallery",
                Subtitle = "MAUI on Avalonia",
                BackgroundColor = Colors.DodgerBlue,
                ForegroundColor = Colors.White
            };

            window.TitleBar = titleBar;
            UpdateTitleBarInfo();
        }
    }

    private void OnSetTitleBarWithIcon(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            var titleBar = new TitleBar
            {
                Title = "Control Gallery",
                Subtitle = "with Icon",
                Icon = "dotnet_bot.png",
                BackgroundColor = Colors.Purple,
                ForegroundColor = Colors.White
            };

            window.TitleBar = titleBar;
            UpdateTitleBarInfo();
        }
    }

    private void OnSetTitleBarWithContent(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            var searchEntry = new Entry
            {
                Placeholder = "Search...",
                WidthRequest = 250,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black
            };

            var titleBar = new TitleBar
            {
                Title = "Control Gallery",
                BackgroundColor = Color.FromArgb("#1a1a2e"),
                ForegroundColor = Colors.White,
                Content = searchEntry
            };

            window.TitleBar = titleBar;
            UpdateTitleBarInfo();
        }
    }

    private void OnSetTitleBarWithButtons(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            var leadingButton = new Button
            {
                Text = "Menu",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                Padding = new Thickness(8, 4)
            };

            var trailingStack = new HorizontalStackLayout
            {
                Spacing = 5
            };
            trailingStack.Add(new Button
            {
                Text = "Settings",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                Padding = new Thickness(8, 4)
            });
            trailingStack.Add(new Button
            {
                Text = "Help",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                Padding = new Thickness(8, 4)
            });

            var titleBar = new TitleBar
            {
                Title = "Control Gallery",
                Subtitle = "with Actions",
                BackgroundColor = Colors.ForestGreen,
                ForegroundColor = Colors.White,
                LeadingContent = leadingButton,
                TrailingContent = trailingStack
            };

            window.TitleBar = titleBar;
            UpdateTitleBarInfo();
        }
    }

    private void OnHideTitleBar(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.TitleBar is TitleBar titleBar)
        {
            titleBar.IsVisible = false;
            UpdateTitleBarInfo();
        }
    }

    private void OnShowTitleBar(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.TitleBar is TitleBar titleBar)
        {
            titleBar.IsVisible = true;
            UpdateTitleBarInfo();
        }
    }

    private void OnRemoveTitleBar(object sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            window.TitleBar = null;
            UpdateTitleBarInfo();
        }
    }

    private void UpdateTitleBarInfo()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.TitleBar is TitleBar titleBar)
        {
            var info = $"Title: {titleBar.Title ?? "N/A"}\n" +
                      $"Subtitle: {titleBar.Subtitle ?? "N/A"}\n" +
                      $"Visible: {titleBar.IsVisible}\n" +
                      $"Has Icon: {titleBar.Icon != null}\n" +
                      $"Has Content: {titleBar.Content != null}\n" +
                      $"Has Leading: {titleBar.LeadingContent != null}\n" +
                      $"Has Trailing: {titleBar.TrailingContent != null}";
            TitleBarInfoLabel.Text = info;
        }
        else
        {
            TitleBarInfoLabel.Text = "No custom TitleBar set";
        }
    }
}
