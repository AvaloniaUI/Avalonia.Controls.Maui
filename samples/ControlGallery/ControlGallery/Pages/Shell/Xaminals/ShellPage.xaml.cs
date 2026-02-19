namespace ControlGallery.Pages.ShellSamples;

public partial class ShellPage : Shell
{
    public ShellPage()
    {
        InitializeComponent();

        Routing.RegisterRoute("animaldetails", typeof(AnimalDetailPage));

        AnimalSearch.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label { Padding = new Thickness(5) };
            label.SetBinding(Label.TextProperty, static (Animal a) => a.Name);
            return label;
        });
    }
}
