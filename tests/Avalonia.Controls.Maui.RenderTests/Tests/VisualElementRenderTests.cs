using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;
using MauiBinding = global::Microsoft.Maui.Controls.Binding;
using MauiContentView = global::Microsoft.Maui.Controls.ContentView;
using MauiFontAttributes = global::Microsoft.Maui.Controls.FontAttributes;
using MauiLabel = global::Microsoft.Maui.Controls.Label;
using MauiVerticalStackLayout = global::Microsoft.Maui.Controls.VerticalStackLayout;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class VisualElementRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_BindingContext_And_Resources()
    {
        var title = new MauiLabel
        {
            FontAttributes = MauiFontAttributes.Bold,
            FontSize = 20
        };
        title.SetBinding(MauiLabel.TextProperty, new MauiBinding(nameof(InheritedVisualElementModel.Title)));
        title.SetDynamicResource(MauiLabel.TextColorProperty, "InheritedVisualElementTextColor");

        var detail = new MauiLabel
        {
            Text = "Local resource color resolved",
            FontSize = 13,
            TextColor = Colors.DimGray
        };

        var control = new MauiContentView
        {
            BindingContext = new InheritedVisualElementModel("BindingContext resolved"),
            WidthRequest = 360,
            HeightRequest = 120,
            Padding = new Microsoft.Maui.Thickness(12),
            BackgroundColor = Colors.Honeydew,
            Resources =
            {
                ["InheritedVisualElementTextColor"] = Colors.DarkGreen
            },
            Content = new MauiVerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    title,
                    detail
                }
            }
        };

        await RenderToFile(control);
        CompareImages(tolerance: 0.06);
    }

    private sealed class InheritedVisualElementModel(string title)
    {
        public string Title { get; } = title;
    }
}
