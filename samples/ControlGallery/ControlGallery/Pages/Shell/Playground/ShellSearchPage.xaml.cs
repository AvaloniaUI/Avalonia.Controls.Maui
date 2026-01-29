namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellSearchPage : ContentPage
    {
        public ShellSearchPage()
        {
            InitializeComponent();
        }

        private void OnToggleSearchVisibility(object sender, EventArgs e)
        {
            var handler = Shell.GetSearchHandler(this);
            if (handler != null)
            {
                handler.SearchBoxVisibility = handler.SearchBoxVisibility == SearchBoxVisibility.Hidden 
                    ? SearchBoxVisibility.Expanded 
                    : SearchBoxVisibility.Hidden;
            }
        }

        private void OnClearSearch(object sender, EventArgs e)
        {
            var handler = Shell.GetSearchHandler(this);
            if (handler != null)
            {
                handler.Query = string.Empty;
            }
        }

        private void OnSetCustomItemTemplate(object sender, EventArgs e)
        {
            var handler = Shell.GetSearchHandler(this);
            if (handler != null)
            {
                handler.ItemTemplate = new DataTemplate(() =>
                {
                    var grid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = GridLength.Star }
                        },
                        Padding = new Thickness(10),
                        ColumnSpacing = 10
                    };

                    var icon = new Image
                    {
                        Source = "dotnet_bot.png",
                        WidthRequest = 32,
                        HeightRequest = 32,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var label = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        VerticalOptions = LayoutOptions.Center
                    };
                    label.SetBinding(Label.TextProperty, "Name");

                    grid.Add(icon, 0, 0);
                    grid.Add(label, 1, 0);

                    return grid;
                });
            }
        }

        private void OnResetItemTemplate(object sender, EventArgs e)
        {
            var handler = Shell.GetSearchHandler(this);
            if (handler != null)
            {
                handler.ItemTemplate = null;
            }
        }
    }
}