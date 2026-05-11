using Microsoft.Maui.Media;

namespace ControlGallery.Pages;

public partial class TextToSpeechPage : ContentPage
{
    readonly ITextToSpeech _tts;
    readonly List<Locale> _locales = [];
    CancellationTokenSource? _cts;

    public TextToSpeechPage()
    {
        InitializeComponent();
        _tts = TextToSpeech.Default;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLocalesAsync();
    }

    async Task LoadLocalesAsync()
    {
        try
        {
            var locales = (await _tts.GetLocalesAsync()).ToList();
            _locales.Clear();
            _locales.AddRange(locales);

            LocalePicker.Items.Clear();
            LocalePicker.Items.Add("(Default)");
            foreach (var locale in _locales)
            {
                // Use locale.Id (full tag like "en-us") instead of locale.Language (just "en")
                var display = string.IsNullOrEmpty(locale.Name)
                    ? locale.Id
                    : $"{locale.Name} ({locale.Id})";
                LocalePicker.Items.Add(display);
            }
            LocalePicker.SelectedIndex = 0;

            LocaleCountLabel.Text = $"Available locales: {_locales.Count}";
        }
        catch (Exception ex)
        {
            LocaleCountLabel.Text = $"Error loading locales: {ex.Message}";
        }
    }

    async void OnSpeakClicked(object? sender, EventArgs e)
    {
        var text = TextEditor.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            StatusLabel.Text = "Status: Enter some text first";
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        SpeakButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        StatusLabel.Text = "Status: Speaking...";

        try
        {
            var options = new SpeechOptions
            {
                Pitch = (float)PitchSlider.Value,
                Rate = (float)RateSlider.Value,
                Volume = (float)VolumeSlider.Value
            };

            if (LocalePicker.SelectedIndex > 0)
                options.Locale = _locales[LocalePicker.SelectedIndex - 1];

            await _tts.SpeakAsync(text, options, _cts.Token);
            StatusLabel.Text = "Status: Done";
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Status: Cancelled";
        }
        catch (PlatformNotSupportedException ex)
        {
            StatusLabel.Text = $"Status: {ex.Message}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Status: Error - {ex.Message}";
        }
        finally
        {
            SpeakButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }

    void OnStopClicked(object? sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    void OnPitchChanged(object? sender, ValueChangedEventArgs e) =>
        PitchLabel.Text = $"Pitch: {e.NewValue:F1}";

    void OnRateChanged(object? sender, ValueChangedEventArgs e) =>
        RateLabel.Text = $"Rate: {e.NewValue:F1}";

    void OnVolumeChanged(object? sender, ValueChangedEventArgs e) =>
        VolumeLabel.Text = $"Volume: {e.NewValue:F1}";
}
