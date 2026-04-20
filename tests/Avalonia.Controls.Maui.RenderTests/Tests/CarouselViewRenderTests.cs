using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Controls;
using CrossPlatformCarouselView = global::Microsoft.Maui.Controls.CarouselView;
using MauiColor = global::Microsoft.Maui.Graphics.Color;
using MauiColors = global::Microsoft.Maui.Graphics.Colors;
using MauiDataTemplate = global::Microsoft.Maui.Controls.DataTemplate;
using MauiGrid = global::Microsoft.Maui.Controls.Grid;
using MauiLabel = global::Microsoft.Maui.Controls.Label;
using MauiTextAlignment = global::Microsoft.Maui.TextAlignment;
using MauiThickness = global::Microsoft.Maui.Thickness;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class CarouselViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_CarouselView_SelectedItem()
    {
        var carouselView = new CrossPlatformCarouselView
        {
            WidthRequest = 280,
            HeightRequest = 160,
            ItemsSource = new[] { "First page", "Second page", "Third page" },
            ItemTemplate = CreateItemTemplate(),
            IsSwipeEnabled = true,
            Loop = true,
            PeekAreaInsets = new MauiThickness(24, 0),
            Position = 1,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
            {
                ItemSpacing = 8,
                SnapPointsAlignment = SnapPointsAlignment.Center,
                SnapPointsType = SnapPointsType.MandatorySingle
            }
        };

        await RenderToFile(carouselView);
        CompareImages(tolerance: 0.05);
    }

    [AvaloniaFact]
    public async Task Render_CarouselView_EmptyView()
    {
        var carouselView = new CrossPlatformCarouselView
        {
            WidthRequest = 280,
            HeightRequest = 160,
            ItemsSource = Array.Empty<string>(),
            EmptyView = "No carousel items",
            EmptyViewTemplate = CreateEmptyViewTemplate()
        };

        await RenderToFile(carouselView);
        CompareImages(tolerance: 0.05);
    }

    static MauiDataTemplate CreateItemTemplate()
    {
        return new MauiDataTemplate(() =>
        {
            var label = new MauiLabel
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = MauiTextAlignment.Center,
                TextColor = MauiColors.White,
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = MauiTextAlignment.Center
            };
            label.SetBinding(MauiLabel.TextProperty, ".");

            return new MauiGrid
            {
                BackgroundColor = MauiColor.FromRgb(0, 120, 140),
                Margin = new MauiThickness(6),
                Padding = new MauiThickness(16),
                Children = { label }
            };
        });
    }

    static MauiDataTemplate CreateEmptyViewTemplate()
    {
        return new MauiDataTemplate(() =>
        {
            var label = new MauiLabel
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 22,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = MauiTextAlignment.Center,
                TextColor = MauiColor.FromRgb(55, 65, 81),
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = MauiTextAlignment.Center
            };
            label.SetBinding(MauiLabel.TextProperty, ".");

            return new MauiGrid
            {
                BackgroundColor = MauiColor.FromRgb(249, 250, 251),
                Margin = new MauiThickness(6),
                Padding = new MauiThickness(16),
                Children = { label }
            };
        });
    }
}
