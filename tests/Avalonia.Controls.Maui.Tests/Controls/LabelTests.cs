using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiLabelHandler = Avalonia.Controls.Maui.Handlers.LabelHandler;
using Avalonia.Controls.Maui.Platform;

namespace Avalonia.Controls.Maui.Tests.Controls;

public partial class LabelTests : WindowHandlerTestBase<MauiLabelHandler, MauiLabel>
{
    [AvaloniaFact(DisplayName = "Text Update Changes Label Desired Width")]
    public async Task TextUpdateChangesLabelDesiredWidth()
    {
        var label = new MauiLabel
        {
            Text = "Hi"
        };

        var handler = await CreateHandlerAsync(label);

        var result = await InvokeOnMainThreadAsync(() =>
        {
            var textBlock = (TextBlock)handler.PlatformView;

            // Measure initial text - control is already in a window via WindowHandlerTestBase
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var initial = textBlock.DesiredSize.Width;

            // Update to longer text
            label.Text = "This is a much longer text that should definitely be wider than Hi";
            handler.UpdateValue(nameof(ILabel.Text));

            // Re-measure after text change
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var updated = textBlock.DesiredSize.Width;

            return (Initial: initial, Updated: updated);
        });

        Assert.True(result.Updated > result.Initial, $"Expected new width ({result.Updated}) to be greater than initial width ({result.Initial})");
    }

        [AvaloniaFact(DisplayName = "UpdatingLabel In VerticalStackLayout Measures Correctly")]
    public async Task UpdatingLabelInVerticalStackLayoutMeasuresCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Hello World"
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { label }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var stackPanel = (LayoutPanel)layoutHandler.PlatformView;
            var textBlock = (TextBlock)label.Handler!.PlatformView!;

            // Measure the layout
            stackPanel.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            return new
            {
                LabelWidth = textBlock.DesiredSize.Width,
                LabelHeight = textBlock.DesiredSize.Height,
                LayoutWidth = stackPanel.DesiredSize.Width,
                LayoutHeight = stackPanel.DesiredSize.Height
            };
        });

        // Update the label text to something longer
        var newText = "Hello World! This is an updated text that is significantly longer.";
        await InvokeOnMainThreadAsync(() =>
        {
            label.Text = newText;
            label.FontSize = 24;
            label.Handler!.UpdateValue(nameof(ILabel.Text));
            label.Handler!.UpdateValue(nameof(ILabel.Font));
        }); 

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var stackPanel = (LayoutPanel)layoutHandler.PlatformView;
            var textBlock = (TextBlock)label.Handler!.PlatformView!;

            // Re-measure the layout
            stackPanel.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            return new
            {
                LabelWidth = textBlock.DesiredSize.Width,
                LabelHeight = textBlock.DesiredSize.Height,
                LayoutWidth = stackPanel.DesiredSize.Width,
                LayoutHeight = stackPanel.DesiredSize.Height
            };
        });

        // Label should have non-zero dimensions and be larger than before
        Assert.True(updatedResult.LabelWidth > 0, $"Label width should be > 0, was {updatedResult.LabelWidth}");
        Assert.True(updatedResult.LabelHeight > 0, $"Label height should be > 0, was {updatedResult.LabelHeight}");
        Assert.True(updatedResult.LabelWidth > initialResult.LabelWidth,
            $"Updated label width ({updatedResult.LabelWidth}) should be > initial label width ({initialResult.LabelWidth})");
        Assert.True(updatedResult.LabelHeight > initialResult.LabelHeight,
            $"Updated label height ({updatedResult.LabelHeight}) should be > initial label height ({initialResult.LabelHeight})");
    }

    [AvaloniaFact(DisplayName = "Label In VerticalStackLayout Measures Correctly")]
    public async Task LabelInVerticalStackLayoutMeasuresCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Hello World"
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { label }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var result = await InvokeOnMainThreadAsync(() =>
        {
            var stackPanel = (LayoutPanel)layoutHandler.PlatformView;
            var textBlock = (TextBlock)label.Handler!.PlatformView!;

            // Measure the layout
            stackPanel.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            return new
            {
                LabelWidth = textBlock.DesiredSize.Width,
                LabelHeight = textBlock.DesiredSize.Height,
                LayoutWidth = stackPanel.DesiredSize.Width,
                LayoutHeight = stackPanel.DesiredSize.Height
            };
        });

        // Label should have non-zero dimensions
        Assert.True(result.LabelWidth > 0, $"Label width should be > 0, was {result.LabelWidth}");
        Assert.True(result.LabelHeight > 0, $"Label height should be > 0, was {result.LabelHeight}");

        // Layout should be at least as big as the label
        Assert.True(result.LayoutWidth >= result.LabelWidth,
            $"Layout width ({result.LayoutWidth}) should be >= label width ({result.LabelWidth})");
        Assert.True(result.LayoutHeight >= result.LabelHeight,
            $"Layout height ({result.LayoutHeight}) should be >= label height ({result.LabelHeight})");
    }
}
