namespace MauiPlanets.Views;

public partial class PlanetsPage : ContentPage
{
    private const uint AnimationDuration = 800u;

    public PlanetsPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        lstPopularPlanets.ItemsSource = PlanetsService.GetFeaturedPlanets();
        lstAllPlanets.ItemsSource = PlanetsService.GetAllPlanets();

        // Reset the menu state
        MainContentGrid.TranslationX = 0;
        MainContentGrid.TranslationY = 0;
        MainContentGrid.Scale = 1;
        MainContentGrid.Opacity = 1;
    }

    async void Planets_SelectionChanged(object? sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Planet planet)
        {
            await Navigation.PushAsync(new PlanetDetailsPage(planet));
        }
    }

    async void ProfilePic_Clicked(object? sender, System.EventArgs e)
    {
        // Reveal our menu and move the main content out of the view
        _ = MainContentGrid.TranslateToAsync(-this.Width * 0.5, this.Height * 0.1, AnimationDuration, Easing.CubicIn);
        await MainContentGrid.ScaleToAsync(0.8, AnimationDuration);
        _ = MainContentGrid.FadeToAsync(0.8, AnimationDuration);
    }

    async void GridArea_Tapped(object? sender, System.EventArgs e)
    {
        await CloseMenu();
    }

    private async Task CloseMenu()
    {
        //Close the menu and bring back back the main content
        _ = MainContentGrid.FadeToAsync(1, AnimationDuration);
        _ = MainContentGrid.ScaleToAsync(1, AnimationDuration);
        await MainContentGrid.TranslateToAsync(0, 0, AnimationDuration, Easing.CubicIn);
    }
}
