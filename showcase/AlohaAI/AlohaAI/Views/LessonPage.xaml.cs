using AlohaAI.Helpers;

namespace AlohaAI.Views;

public partial class LessonPage : ContentPage
{
    public LessonPage(ViewModels.LessonViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.LessonViewModel.MarkdownContent) && sender is ViewModels.LessonViewModel vm)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                ContentArea.Children.Clear();
                if (!string.IsNullOrEmpty(vm.MarkdownContent))
                {
                    try
                    {
                        // Strip first H1 if it matches the title to avoid duplication
                        var content = vm.MarkdownContent;
                        var lines = content.Split('\n');
                        if (lines.Length > 0)
                        {
                            var firstLine = lines[0].Trim();
                            if (firstLine.StartsWith("# "))
                            {
                                var h1Text = firstLine.Substring(2).Trim();
                                if (string.Equals(h1Text, vm.Title, StringComparison.OrdinalIgnoreCase))
                                {
                                    content = string.Join('\n', lines.Skip(1));
                                }
                            }
                        }
                        var darkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
                        var rendered = MarkdownRenderer.Render(content, darkMode: darkMode);
                        ContentArea.Children.Add(rendered);
                    }
                    catch (Exception ex)
                    {
                        var fallbackDark = Application.Current?.RequestedTheme == AppTheme.Dark;
                        ContentArea.Children.Add(new Label
                        {
                            Text = vm.MarkdownContent,
                            FontSize = 15,
                            TextColor = Color.FromArgb(fallbackDark ? "#CCCCCC" : "#342D42"),
                            Padding = new Thickness(0, 8)
                        });
                        System.Diagnostics.Debug.WriteLine($"Markdown render error: {ex.Message}");
                    }
                }
            });
        }
        else if (e.PropertyName == nameof(ViewModels.LessonViewModel.Keywords) && sender is ViewModels.LessonViewModel vmk)
        {
            Application.Current.Dispatcher.Dispatch(() => PopulateKeywords(vmk.Keywords));
        }
        else if (e.PropertyName == nameof(ViewModels.LessonViewModel.IsCompleted) && sender is ViewModels.LessonViewModel vm2 && vm2.IsCompleted)
        {
            Application.Current.Dispatcher.Dispatch(async () => await ShowXpPopupAsync(vm2.LessonXp));
        }
    }

    private void PopulateKeywords(List<string> keywords)
    {
        KeywordsArea.Children.Clear();
        var colors = new[] { "#5B8FD4", "#7B68AE", "#E88BBF", "#7EB4F0", "#4CAF50" };
        for (int i = 0; i < keywords.Count; i++)
        {
            var color = Color.FromArgb(colors[i % colors.Length]);
            var chip = new Border
            {
                BackgroundColor = color.WithAlpha(0.15f),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Stroke = color.WithAlpha(0.3f),
                StrokeThickness = 1,
                Padding = new Thickness(12, 6),
                Margin = new Thickness(0, 0, 6, 6),
                Content = new Label
                {
                    Text = keywords[i],
                    FontSize = 12,
                    TextColor = color,
                    FontAttributes = FontAttributes.Bold
                }
            };
            KeywordsArea.Children.Add(chip);
        }
    }

    private async Task ShowXpPopupAsync(int xp)
    {
        XpPopupLabel.Text = $"+{xp} XP";
        XpPopup.IsVisible = true;
        XpPopup.Opacity = 0;
        XpPopup.Scale = 0.5;
        XpPopup.TranslationY = 0;

        await Task.WhenAll(
            XpPopup.FadeToAsync(1, 300, Easing.CubicOut),
            XpPopup.ScaleToAsync(1.2, 300, Easing.SpringOut));

        await XpPopup.ScaleToAsync(1, 150, Easing.CubicIn);
        await Task.Delay(800);

        await Task.WhenAll(
            XpPopup.FadeToAsync(0, 400, Easing.CubicIn),
            XpPopup.TranslateToAsync(0, -60, 400, Easing.CubicIn));

        XpPopup.IsVisible = false;
    }
}
