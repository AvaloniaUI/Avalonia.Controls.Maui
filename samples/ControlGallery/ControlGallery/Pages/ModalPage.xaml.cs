namespace ControlGallery.Pages;

public partial class ModalPage : ContentPage
{
    public ModalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateModalStackInfo();
        ResultLabel.Text = "Tap a button to present a modal page.";
    }

    private void UpdateModalStackInfo()
    {
        if (Navigation?.ModalStack != null)
        {
            ModalStackInfoLabel.Text = $"Modal stack depth: {Navigation.ModalStack.Count}";
        }
    }

    private async void OnShowBasicModal(object? sender, EventArgs e)
    {
        var tcs = new TaskCompletionSource();
        var modal = new BasicModalContent("Basic Modal", "This is a basic modal page. Tap the button below to dismiss it.");
        modal.Disappearing += (s, args) => tcs.TrySetResult();
        await Navigation.PushModalAsync(modal);
        await tcs.Task;
        ResultLabel.Text = "Basic modal was dismissed.";
        UpdateModalStackInfo();
    }

    private async void OnShowColorPickerModal(object? sender, EventArgs e)
    {
        var tcs = new TaskCompletionSource<string?>();
        var modal = new ColorPickerModalContent(tcs);
        await Navigation.PushModalAsync(modal);

        var result = await tcs.Task;
        ResultLabel.Text = result != null ? $"Selected color: {result}" : "Color selection cancelled.";
        UpdateModalStackInfo();
    }

    private async void OnShowModalWithNavigation(object? sender, EventArgs e)
    {
        var tcs = new TaskCompletionSource();
        var modalContent = new ModalNavigationContent();
        var navPage = new NavigationPage(modalContent)
        {
            BarBackgroundColor = Colors.DodgerBlue,
            BarTextColor = Colors.White
        };
        navPage.Disappearing += (s, args) => tcs.TrySetResult();
        await Navigation.PushModalAsync(navPage);
        await tcs.Task;
        ResultLabel.Text = "Navigation modal was dismissed.";
        UpdateModalStackInfo();
    }

    private async void OnShowFullScreenModal(object? sender, EventArgs e)
    {
        var tcs = new TaskCompletionSource();
        var modal = new FullScreenModalContent();
        modal.Disappearing += (s, args) => tcs.TrySetResult();
        await Navigation.PushModalAsync(modal);
        await tcs.Task;
        ResultLabel.Text = "Full-screen modal was dismissed.";
        UpdateModalStackInfo();
    }
}

/// <summary>
/// A simple modal page with a dismiss button.
/// </summary>
public class BasicModalContent : ContentPage
{
    public BasicModalContent(string title, string description)
    {
        Title = title;
        BackgroundColor = Color.FromArgb("#F0F0F0");

        Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Padding = 30,
            Spacing = 20,
            Children =
            {
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
                    HorizontalOptions = LayoutOptions.Fill
                },
                CreateDismissButton()
            }
        };
    }

    private Button CreateDismissButton()
    {
        var button = new Button
        {
            Text = "Dismiss Modal",
            BackgroundColor = Colors.DodgerBlue,
            TextColor = Colors.White
        };
        button.Clicked += async (s, e) => await Navigation.PopModalAsync();
        return button;
    }
}

/// <summary>
/// A modal that lets the user pick a color and returns the result.
/// </summary>
public class ColorPickerModalContent : ContentPage
{
    private readonly TaskCompletionSource<string?> _tcs;

    public ColorPickerModalContent(TaskCompletionSource<string?> tcs)
    {
        _tcs = tcs;
        Title = "Pick a Color";
        BackgroundColor = Color.FromArgb("#F0F0F0");

        var layout = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Padding = 30,
            Spacing = 15,
            Children =
            {
                new Label
                {
                    Text = "Pick a Color",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Select a color below. The result will be passed back to the calling page.",
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };

        var colors = new[] { ("Red", Colors.Red), ("Green", Colors.Green), ("Blue", Colors.DodgerBlue), ("Orange", Colors.Orange), ("Purple", Colors.Purple) };

        foreach (var (name, color) in colors)
        {
            var button = new Button
            {
                Text = name,
                BackgroundColor = color,
                TextColor = Colors.White
            };
            var colorName = name;
            button.Clicked += async (s, e) =>
            {
                _tcs.TrySetResult(colorName);
                await Navigation.PopModalAsync();
            };
            layout.Children.Add(button);
        }

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White
        };
        cancelButton.Clicked += async (s, e) =>
        {
            _tcs.TrySetResult(null);
            await Navigation.PopModalAsync();
        };
        layout.Children.Add(cancelButton);

        Content = new ScrollView { Content = layout };
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Ensure the TCS completes even if the modal is dismissed via back gesture
        // or other platform mechanisms that bypass the button handlers.
        _tcs.TrySetResult(null);
    }
}

/// <summary>
/// A modal wrapped in a NavigationPage that supports pushing child pages.
/// </summary>
public class ModalNavigationContent : ContentPage
{
    public ModalNavigationContent()
    {
        Title = "Modal Navigation";

        var layout = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Padding = 30,
            Spacing = 20,
            Children =
            {
                new Label
                {
                    Text = "Modal with Navigation",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "This modal is wrapped in a NavigationPage. You can push additional pages within the modal stack.",
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };

        var pushButton = new Button
        {
            Text = "Push Page in Modal",
            BackgroundColor = Colors.DodgerBlue,
            TextColor = Colors.White
        };
        pushButton.Clicked += async (s, e) =>
        {
            var child = new ContentPage
            {
                Title = "Child in Modal",
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Padding = 30,
                    Spacing = 20,
                    Children =
                    {
                        new Label
                        {
                            Text = "Child Page",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = "This page was pushed inside the modal's NavigationPage. Use the back button to return.",
                            FontSize = 14,
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    }
                }
            };
            await Navigation.PushAsync(child);
        };
        layout.Children.Add(pushButton);

        var dismissButton = new Button
        {
            Text = "Dismiss Modal",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White
        };
        dismissButton.Clicked += async (s, e) => await Navigation.PopModalAsync();
        layout.Children.Add(dismissButton);

        Content = new ScrollView { Content = layout };
    }
}

/// <summary>
/// A full-screen modal with a dark overlay and centered content.
/// </summary>
public class FullScreenModalContent : ContentPage
{
    public FullScreenModalContent()
    {
        Title = "Full Screen";
        BackgroundColor = Colors.Black;
        NavigationPage.SetHasNavigationBar(this, false);

        Content = new Grid
        {
            Children =
            {
                new Image
                {
                    Source = "dotnet_bot.png",
                    Aspect = Aspect.AspectFit,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                },
                new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.End,
                    Padding = new Thickness(30, 30, 30, 50),
                    Spacing = 15,
                    Children =
                    {
                        new Label
                        {
                            Text = "Full-Screen Modal",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = "This modal takes over the entire screen. Tap below to dismiss.",
                            FontSize = 14,
                            TextColor = Colors.White,
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        CreateDismissButton()
                    }
                }
            }
        };
    }

    private Button CreateDismissButton()
    {
        var button = new Button
        {
            Text = "Close",
            BackgroundColor = Colors.White,
            TextColor = Colors.Black
        };
        button.Clicked += async (s, e) => await Navigation.PopModalAsync();
        return button;
    }
}
