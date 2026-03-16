using MyConference.ViewModels;

namespace MyConference.Views;

public partial class SpeakersPage : ContentPage
{
    private readonly SpeakersViewModel _viewModel;

    public SpeakersPage(SpeakersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
