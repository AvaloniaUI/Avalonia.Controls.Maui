namespace BlazorHybrid.Sample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage())
        {
            Width = 1000,
            Height = 700,
            Title = "Avalonia MAUI Blazor Hybrid"
        };
    }
}
