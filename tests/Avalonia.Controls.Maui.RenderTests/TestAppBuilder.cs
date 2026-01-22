using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Fonts.Inter;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Handlers.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

[assembly: AvaloniaTestApplication(typeof(Avalonia.Controls.Maui.RenderTests.TestAppBuilder))]
namespace Avalonia.Controls.Maui.RenderTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        })
        .UseSkia()
        .WithInterFont()
        .With(new FontManagerOptions
        {
            // Use Inter font as default for consistent cross-platform rendering
            DefaultFamilyName = "fonts:Inter#Inter"
        });
}

public static class MauiTestAppBuilderExtensions
{
    public static MauiAppBuilder ConfigureTestBuilder(this MauiAppBuilder builder)
    {
        builder.UseAvaloniaApp();

        builder.Services.AddSingleton<IFontManager>(sp =>
            new FontManager(new FontRegistrar(), sp));

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<Microsoft.Maui.Controls.ActivityIndicator, ActivityIndicatorHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Border, BorderHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.BoxView, BoxViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Button, ButtonHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.CheckBox, CheckBoxHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.DatePicker, DatePickerHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Editor, EditorHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Entry, EntryHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.GraphicsView, GraphicsViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Image, ImageHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.ImageButton, ImageButtonHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Label, LabelHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Picker, PickerHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.ProgressBar, ProgressBarHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.RefreshView, RefreshViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.ScrollView, ScrollViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.SearchBar, SearchBarHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Slider, SliderHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Stepper, StepperHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.SwipeView, SwipeViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Switch, SwitchHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.TimePicker, TimePickerHandler>();
            
            // Shapes
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Ellipse, EllipseHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Line, LineHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Path, PathHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Polygon, PolygonHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Polyline, PolylineHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Rectangle, RectangleHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Shapes.RoundRectangle, RoundRectangleHandler>();

        });

        return builder;
    }
}

public class FontRegistrar : IFontRegistrar
{
    public void Register(string filename, string? alias, System.Reflection.Assembly assembly) { }
    public void Register(string filename, string? alias) { }
    public string? GetFont(string font) => font;
}