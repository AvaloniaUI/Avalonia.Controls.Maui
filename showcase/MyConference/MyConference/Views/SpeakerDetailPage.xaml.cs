using MyConference.ViewModels;

namespace MyConference.Views;

public partial class SpeakerDetailPage : ContentPage
{
    public SpeakerDetailPage(SpeakerDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
