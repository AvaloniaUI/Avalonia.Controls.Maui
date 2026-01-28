using ControlGallery.Pages.ShellSamples;
namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public class Feature
    {
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
    }

    public class FeaturesSearchHandler : SearchHandler
    {
        public List<Feature> Features { get; set; } = new ()
        {
            new Feature { Name = "Navigation", Route = "//navigation" },
            new Feature { Name = "Structure", Route = "//structure" },
            new Feature { Name = "Styling", Route = "//styling" },
            new Feature { Name = "Colors", Route = "//colors" },
            new Feature { Name = "Presentation Mode", Route = "//presentation" }
        };

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                ItemsSource = Features
                    .Where(f => f.Name.Contains(newValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);

            if (item is Feature feature)
            {
                var shell = this.GetShell();
                if (shell == null) return;
                
                 await shell.DisplayAlert("Search", $"You selected: {feature.Name}", "OK");
            }
        }  
    }
}