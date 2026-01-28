namespace ControlGallery.Pages.ShellSamples;

public class AnimalSearchHandler : SearchHandler
{

    private IEnumerable<Animal> GetContextualAnimals(Shell? shell)
    {
        if (shell == null)
        {
            return AnimalData.AllAnimals;
        }
        
        var currentItem = shell.CurrentItem;
        var currentSection = currentItem?.CurrentItem;
        var currentContent = currentSection?.CurrentItem;
        var currentPage = shell.CurrentPage;
        
        // Check CurrentPage type
        if (currentPage is CatsPage) return AnimalData.Cats;
        if (currentPage is DogsPage) return AnimalData.Dogs;
        if (currentPage is MonkeysPage) return AnimalData.Monkeys;
        if (currentPage is BearsPage) return AnimalData.Bears;
        if (currentPage is ElephantsPage) return AnimalData.Elephants;

        // Check ShellContent.Content
        if (currentContent is ShellContent sc && sc.Content != null)
        {
            var contentPage = sc.Content;
            if (contentPage is CatsPage) return AnimalData.Cats;
            if (contentPage is DogsPage) return AnimalData.Dogs;
            if (contentPage is MonkeysPage) return AnimalData.Monkeys;
            if (contentPage is BearsPage) return AnimalData.Bears;
            if (contentPage is ElephantsPage) return AnimalData.Elephants;
        }
        
        return AnimalData.AllAnimals;
    }

    protected override void OnQueryChanged(string oldValue, string newValue)
    {
        base.OnQueryChanged(oldValue, newValue);

        if (string.IsNullOrWhiteSpace(newValue))
        {
            ItemsSource = null;
        }
        else
        {
            var shell = this.GetShell();
            var animals = GetContextualAnimals(shell);

            ItemsSource = animals
                .Where(animal => animal.Name.ToLower().Contains(newValue.ToLower()))
                .ToList();
        }
    }

    protected override async void OnItemSelected(object item)
    {
        base.OnItemSelected(item);

        if (item is Animal animal)
        {
            var shell = this.GetShell();
            
            if (shell == null)
            {
                return;
            }

            var navigationParameters = new Dictionary<string, object>
            {
                { "Animal", animal }
            };
            
            await shell.GoToAsync($"animaldetails", navigationParameters);
        }
    }
}