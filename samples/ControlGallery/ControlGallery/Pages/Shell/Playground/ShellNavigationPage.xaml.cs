namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellNavigationPage : ContentPage
    {
        private BackButtonBehavior? _currentBehavior;

        public ShellNavigationPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


            var shell = this.GetShell();
            
            if (shell != null)
            {
                shell.Navigating += OnShellNavigating;
                shell.Navigated += OnShellNavigated;
            }

            if (BackButtonBehaviorPicker.SelectedIndex < 0 && BackButtonBehaviorPicker.Items.Count > 0)
            {
                 BackButtonBehaviorPicker.SelectedIndex = 0;
            }
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var shell = this.GetShell();
            if (shell != null)
            {
                shell.Navigating -= OnShellNavigating;
                shell.Navigated -= OnShellNavigated;
            }
        }

        private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            LogEvent($"Navigating: {e.Source} -> {e.Target.Location}");
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            LogEvent($"Navigated: {e.Source} -> {e.Current.Location}");
        }

        private void LogEvent(string message)
        {
            EventsLabel.Text += $"{DateTime.Now.ToString("HH:mm:ss")}: {message}\n";
        }

        private void OnClearLog(object sender, EventArgs e)
        {
            EventsLabel.Text = string.Empty;
        }

        private async void OnGoToDetailRelative(object sender, EventArgs e)
        {
            await NavigateTo("common_detail");
        }

        private async void OnGoToDetailWithParams(object sender, EventArgs e)
        {
            string data = DataEntry.Text ?? "No Data";
            await NavigateTo($"common_detail?message={Uri.EscapeDataString(data)}&title=Parameterized+Detail");
        }

        private async void OnGoToDetailAbsolute(object sender, EventArgs e)
        {
            await NavigateTo("common_detail");
        }

        private async Task NavigateTo(string route)
        {
            var shell = this.GetShell();
            if (shell != null)
            {
                await shell.GoToAsync(route);
            }
        }

        private async void OnPushModalAnimated(object sender, EventArgs e)
        {
            var page = new ShellCommonDetailPage { Title = "Modal Page" };
            Shell.SetPresentationMode(page, PresentationMode.ModalAnimated);
            await Navigation.PushAsync(page);
        }

        private async void OnTestBackButtonBehavior(object sender, EventArgs e)
        {
            var page = new ShellCommonDetailPage { Title = "Back Button Test" };

            // Apply the selected behavior to the destination page
            if (_currentBehavior != null)
            {
                Shell.SetBackButtonBehavior(page, _currentBehavior);
            }

            await Navigation.PushAsync(page);
        }

        private void OnBackButtonBehaviorChanged(object sender, EventArgs e)
        {
            if (BackButtonBehaviorPicker.SelectedIndex == -1) return;

            string selected = (string)BackButtonBehaviorPicker.SelectedItem;
            string description = string.Empty;

            switch (selected)
            {
                case "Disabled":
                    _currentBehavior = new BackButtonBehavior { IsEnabled = false };
                    description = "Back button is disabled (grayed out, not clickable)";
                    break;
                case "Hidden":
                    _currentBehavior = new BackButtonBehavior { IsVisible = false };
                    description = "Back button is hidden from view";
                    break;
                case "Text Override":
                    _currentBehavior = new BackButtonBehavior { TextOverride = "Back" };
                    description = "Back button shows custom text instead of arrow";
                    break;
                case "Command (Show Alert)":
                    _currentBehavior = new BackButtonBehavior
                    {
                        Command = new Command(async () =>
                        {
                            await Application.Current!.Windows[0].Page!.DisplayAlert("Back Button Pressed", "Custom command executed!", "OK");
                            await Shell.Current.GoToAsync("..");
                        })
                    };
                    description = "Custom command shows alert before navigating back";
                    break;
                case "Command with Parameter":
                    _currentBehavior = new BackButtonBehavior
                    {
                        Command = new Command<string>(async (param) =>
                        {
                            await Application.Current!.Windows[0].Page!.DisplayAlert("Back Button Pressed", $"Parameter: {param}", "OK");
                            await Shell.Current.GoToAsync("..");
                        }),
                        CommandParameter = "Hello from BackButtonBehavior!"
                    };
                    description = "Custom command with parameter passed to it";
                    break;
                default: // Default
                    _currentBehavior = null;
                    description = "Default: Standard back button behavior";
                    break;
            }

            BackButtonBehaviorDescription.Text = description;
        }
    }
}
