namespace AlohaAI.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ViewModels.ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.ProfileViewModel vm)
            vm.LoadDataCommand.Execute(null);

        MainContent.Opacity = 0;
        MainContent.TranslationY = 30;
        await Task.WhenAll(
            MainContent.FadeToAsync(1, 400, Easing.CubicOut),
            MainContent.TranslateToAsync(0, 0, 400, Easing.CubicOut));
    }
}
