namespace AlohaAI.Views;

public partial class QuizPage : ContentPage
{
    public QuizPage(ViewModels.QuizViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.QuizViewModel.ShowResults) && sender is ViewModels.QuizViewModel vm && vm.ShowResults)
        {
            // Animate results view entrance
            if (ResultsView != null)
            {
                ResultsView.Opacity = 0;
                ResultsView.Scale = 0.8;
                await Task.WhenAll(
                    ResultsView.FadeToAsync(1, 500, Easing.CubicOut),
                    ResultsView.ScaleToAsync(1, 500, Easing.SpringOut));
            }
        }
        else if (e.PropertyName == nameof(ViewModels.QuizViewModel.ShowExplanation) && sender is ViewModels.QuizViewModel vm2 && vm2.ShowExplanation)
        {
            // Animate explanation slide in
            if (ExplanationCard != null)
            {
                ExplanationCard.Opacity = 0;
                ExplanationCard.TranslationY = 20;
                await Task.WhenAll(
                    ExplanationCard.FadeToAsync(1, 300, Easing.CubicOut),
                    ExplanationCard.TranslateToAsync(0, 0, 300, Easing.CubicOut));
            }
        }
    }
}
