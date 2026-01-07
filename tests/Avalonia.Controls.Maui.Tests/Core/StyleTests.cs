using Avalonia.Headless.XUnit;
using MCContentPage = Microsoft.Maui.Controls.ContentPage;
using MCLabel = Microsoft.Maui.Controls.Label;
using MCButton = Microsoft.Maui.Controls.Button;
using MCStyle = Microsoft.Maui.Controls.Style;
using MCSetter = Microsoft.Maui.Controls.Setter;
using MCColors = Microsoft.Maui.Graphics.Colors;

namespace Avalonia.Controls.Maui.Tests.Core;

public class StyleTests
{
    [AvaloniaFact(DisplayName = "Implicit Style applies to control")]
    public void Implicit_Style_Applies_To_Control()
    {
        var page = new MCContentPage();
        var style = new MCStyle(typeof(MCLabel));
        style.Setters.Add(new MCSetter { Property = MCLabel.TextColorProperty, Value = MCColors.Red });
        page.Resources.Add(style);

        var label = new MCLabel();
        page.Content = label;

        // Verify implicit style application
        Assert.Equal(MCColors.Red, label.TextColor);
    }

    [AvaloniaFact(DisplayName = "Explicit Style applies when key referenced")]
    public void Explicit_Style_Applies_When_Key_Referenced()
    {
        var page = new MCContentPage();
        var style = new MCStyle(typeof(MCButton));
        style.Setters.Add(new MCSetter { Property = MCButton.BackgroundColorProperty, Value = MCColors.Green });
        page.Resources.Add("MyButtonStyle", style);

        var button = new MCButton { Style = style };
        page.Content = button;

        Assert.Equal(MCColors.Green, button.BackgroundColor);
    }

    [AvaloniaFact(DisplayName = "Style Inheritance (BasedOn) combines setters")]
    public void Style_Inheritance_Combines_Setters()
    {
        var baseStyle = new MCStyle(typeof(MCLabel));
        baseStyle.Setters.Add(new MCSetter { Property = MCLabel.TextColorProperty, Value = MCColors.Blue });
        baseStyle.Setters.Add(new MCSetter { Property = MCLabel.FontSizeProperty, Value = 20.0 });

        var derivedStyle = new MCStyle(typeof(MCLabel)) { BasedOn = baseStyle };
        derivedStyle.Setters.Add(new MCSetter { Property = MCLabel.TextColorProperty, Value = MCColors.Red }); // Override

        var label = new MCLabel { Style = derivedStyle };

        Assert.Equal(MCColors.Red, label.TextColor); // Overridden
        Assert.Equal(20.0, label.FontSize); // Inherited
    }
}
