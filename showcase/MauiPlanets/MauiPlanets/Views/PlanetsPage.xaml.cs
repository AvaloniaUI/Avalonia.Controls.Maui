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
    }

    async void Planets_SelectionChanged(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Planet planet)
        {
            await Navigation.PushAsync(new PlanetDetailsPage(planet));
        }
    }

    async void ProfilePic_Clicked(System.Object sender, System.EventArgs e)
    {
        // Reveal our menu and move the main content out of the view
        _ = MainContentGrid.TranslateToAsync(-this.Width * 0.5, this.Height * 0.1, AnimationDuration, Easing.CubicIn);
        await MainContentGrid.ScaleToAsync(0.8, AnimationDuration);
        _ = MainContentGrid.FadeToAsync(0.8, AnimationDuration);
    }

    async void GridArea_Tapped(System.Object sender, System.EventArgs e)
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
