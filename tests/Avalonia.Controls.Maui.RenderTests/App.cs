using Avalonia.Themes.Fluent;
using Avalonia.Styling;

namespace Avalonia.Controls.Maui.RenderTests;

public class App : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Light;
        Styles.Add(new FluentTheme());
        Styles.Add(new Avalonia.Themes.Simple.SimpleTheme());
        Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://Avalonia.Controls.Maui/Styles/Controls.axaml"))
        {
            Source = new Uri("avares://Avalonia.Controls.Maui/Styles/Controls.axaml")
        });
    }
}
