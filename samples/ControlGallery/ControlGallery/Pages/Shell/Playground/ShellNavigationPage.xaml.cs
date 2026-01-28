namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellNavigationPage : ContentPage
    {
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

        private void OnBackButtonBehaviorChanged(object sender, EventArgs e)
        {
            if (BackButtonBehaviorPicker.SelectedIndex == -1) return;

            var behavior = new BackButtonBehavior();
            string selected = (string)BackButtonBehaviorPicker.SelectedItem;

            switch (selected)
            {
                case "Disabled":
                    behavior.IsEnabled = false;
                    break;
                case "Command (Show Alert)":
                    behavior.IsEnabled = true;
                    behavior.Command = new Command(async () => 
                    {
                        await DisplayAlert("Back Button Pressed", "Custom behavior executed", "OK");
                        await Navigation.PopAsync();
                    });
                    break;
                default: // Default
                    behavior = null;
                    break;
            }

            Shell.SetBackButtonBehavior(this, behavior);
        }
    }
}
