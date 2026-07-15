namespace BlazorHybrid.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BlazorHybridSmokeTest.AttachIfEnabled(blazorWebView);
    }
}
