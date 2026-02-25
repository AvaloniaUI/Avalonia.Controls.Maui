namespace AlohaAI.Views;

public partial class PathsPage : ContentPage
{
    public PathsPage(ViewModels.PathsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.PathsViewModel vm)
            vm.LoadPathsCommand.Execute(null);

        MainContent.Opacity = 0;
        MainContent.TranslationY = 30;
        await Task.WhenAll(
            MainContent.FadeToAsync(1, 400, Easing.CubicOut),
            MainContent.TranslateToAsync(0, 0, 400, Easing.CubicOut));
    }
}
