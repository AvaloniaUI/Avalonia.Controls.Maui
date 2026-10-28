using Avalonia.Styling;

namespace Avalonia.Controls.Maui.RenderTests;

public class App : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Light;
        Styles.Add(new Themes.Fluent.FluentTheme());
        Styles.Add(new ControlStyles());
    }
}
