namespace ControlGallery.Pages.ShellSamples;

public partial class AnimalDetailPage : ContentPage, IQueryAttributable
{
    private Animal? _animal;
    public Animal Animal
    {
        get => _animal!;
        set
        {
            _animal = value;
            LoadAnimal(value);
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Animal", out var animalObj) && animalObj is Animal animal)
        {
            Animal = animal;
        }
        else
        {
            foreach (var key in query.Keys)
            {
            }
        }
    }

    public AnimalDetailPage()
    {
        InitializeComponent();
    }

    private void LoadAnimal(Animal animal)
    {
        if (animal != null)
        {
            AnimalImage.Source = animal.ImageUrl;
            AnimalName.Text = animal.Name;
            AnimalLocation.Text = animal.Location;
            AnimalDetails.Text = animal.Details;
            Title = animal.Name;
        }
    }
}