using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace ControlGallery.Pages;

public partial class ToolkitPopupPage : ContentPage
{
    public ToolkitPopupPage()
    {
        InitializeComponent();
    }

    private async void OnShowSimplePopupClicked(object? sender, EventArgs e)
    {
        var popup = new Popup { Padding = 20 };
        var closeButton = new Button { Text = "Close" };
        closeButton.Clicked += async (_, _) => await popup.CloseAsync();
        popup.Content = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                new Label { Text = "Hello from a popup", FontAttributes = FontAttributes.Bold },
                new Label { Text = "Tap outside the popup or the button below to close it." },
                closeButton,
            }
        };

        await this.ShowPopupAsync(popup);
    }

    private async void OnShowResultPopupClicked(object? sender, EventArgs e)
    {
        var popup = new Popup<string> { Padding = 20 };
        var choices = new VerticalStackLayout { Spacing = 8 };
        choices.Children.Add(new Label { Text = "Choose a color", FontAttributes = FontAttributes.Bold });

        foreach (var color in new[] { "Red", "Green", "Blue" })
        {
            var button = new Button { Text = color };
            button.Clicked += async (_, _) => await popup.CloseAsync(color);
            choices.Children.Add(button);
        }

        popup.Content = choices;

        var result = await this.ShowPopupAsync<string>(popup);
        ResultLabel.Text = result.WasDismissedByTappingOutsideOfPopup
            ? "Result: (dismissed)"
            : $"Result: {result.Result}";
    }

    private async void OnShowStyledPopupClicked(object? sender, EventArgs e)
    {
        var popup = new Popup
        {
            Padding = 24,
            Content = new Label { Text = "A popup with a rounded shape and a tinted overlay." },
        };

        var result = await this.ShowPopupAsync(popup, new PopupOptions
        {
            PageOverlayColor = Color.FromRgba(98, 0, 238, 0.3),
            Shape = new RoundRectangle
            {
                CornerRadius = 24,
                Stroke = Color.FromArgb("#6200EE"),
                StrokeThickness = 3,
            },
        });

        OptionsLabel.Text = result.WasDismissedByTappingOutsideOfPopup
            ? "Closed by: tapping outside"
            : "Closed by: code";
    }

    private async void OnShowModalPopupClicked(object? sender, EventArgs e)
    {
        var popup = new Popup { Padding = 20 };
        var closeButton = new Button { Text = "Close" };
        closeButton.Clicked += async (_, _) => await popup.CloseAsync();
        popup.Content = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                new Label { Text = "Tapping outside does not close this popup." },
                closeButton,
            }
        };

        var result = await this.ShowPopupAsync(popup, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
        });

        OptionsLabel.Text = result.WasDismissedByTappingOutsideOfPopup
            ? "Closed by: tapping outside"
            : "Closed by: the Close button";
    }
}
