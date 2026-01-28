namespace ControlGallery.Pages.ShellSamples;

public partial class ShellPage : Shell
{
    public ShellPage()
    {
        InitializeComponent();

        Routing.RegisterRoute("animaldetails", typeof(AnimalDetailPage));
    }
}
