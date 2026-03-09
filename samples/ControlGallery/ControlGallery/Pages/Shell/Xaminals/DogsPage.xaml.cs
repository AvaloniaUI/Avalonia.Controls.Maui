namespace ControlGallery.Pages.ShellSamples;

public partial class DogsPage : ContentPage
{
    public DogsPage()
    {
        InitializeComponent();
    }

    async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Animal animal)
        {
            await NavigateToDetails(animal);
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    private async Task NavigateToDetails(Animal animal)
    {
        var shell = this.GetShell();
        if (shell != null)
        {
            var navigationParameters = new Dictionary<string, object>
            {
                { "Animal", animal }
            };
            await shell.GoToAsync("animaldetails", navigationParameters);
        }
    }
}
