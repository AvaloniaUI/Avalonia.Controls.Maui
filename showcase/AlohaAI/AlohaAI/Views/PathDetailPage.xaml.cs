namespace AlohaAI.Views;

public partial class PathDetailPage : ContentPage
{
    private bool _firstAppear = true;

    public PathDetailPage(ViewModels.PathDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Refresh data when returning from lesson/quiz (not on first load — PathId setter handles that)
        if (!_firstAppear && BindingContext is ViewModels.PathDetailViewModel vm)
            vm.LoadDataCommand.Execute(null);
        _firstAppear = false;

        try
        {
            if (MainContent != null)
            {
                MainContent.Opacity = 0;
                MainContent.TranslationY = 30;
                await Task.WhenAll(
                    MainContent.FadeToAsync(1, 400, Easing.CubicOut),
                    MainContent.TranslateToAsync(0, 0, 400, Easing.CubicOut));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PathDetail animation error: {ex.Message}");
        }
    }
}
