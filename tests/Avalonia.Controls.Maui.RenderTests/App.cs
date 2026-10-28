using Avalonia.Media;
using Avalonia.Styling;

namespace Avalonia.Controls.Maui.RenderTests;

public class App : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Light;
        Styles.Add(new Themes.Fluent.FluentTheme());
        Styles.Add(new ControlStyles());

        // FreeType hints glyphs and Core Text does not, so hinted text is drawn thinner on Linux
        // than on macOS. Unhinted, grayscale text looks the same on every platform.
        Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            TextOptions.SetTextHintingMode(window, TextHintingMode.None);
            TextOptions.SetTextRenderingMode(window, TextRenderingMode.Antialias);
        });
    }
}
