namespace AlohaAI.Views;

public partial class HomePage : ContentPage
{
    public HomePage(ViewModels.HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.HomeViewModel vm)
        {
            MainContent.Opacity = 0;
            MainContent.TranslationY = 30;

            vm.LoadDataCommand.Execute(null);
            await Task.Delay(300); // Let data bind before animating in

            await Task.WhenAll(
                MainContent.FadeToAsync(1, 400, Easing.CubicOut),
                MainContent.TranslateToAsync(0, 0, 400, Easing.CubicOut));
        }
    }
}
