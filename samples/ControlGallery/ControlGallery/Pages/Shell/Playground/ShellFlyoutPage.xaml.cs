namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellFlyoutPage : ContentPage
    {
        private bool _isCustomIcon;
        private bool _isHeaderTemplate;
        private bool _isFooterTemplate;
        private List<FlyoutItem> _dynamicFlyoutItems = new();
        private List<MenuItem> _dynamicMenuItems = new();

        public List<string> BehaviorItems { get; } = new() { "Disabled", "Flyout (Popover)", "Locked" };
        public List<string> BackgroundImageItems { get; } = new() { "None", "Banner (Light)", "Banner (Dark)", "DotNet Bot" };
        public List<string> AspectItems { get; } = new() { "AspectFit", "AspectFill", "Fill", "Center" };
        public List<string> HeaderBehaviorItems { get; } = new() { "Fixed", "Scroll" };
        public List<string> ScrollModeItems { get; } = new() { "Auto", "Enabled", "Disabled" };

        public ShellFlyoutPage()
        {
            InitializeComponent();
            BindingContext = this;
            
            // Initialize pickers
            BehaviorPicker.SelectedIndex = 1; // Flyout
            HeaderBehaviorPicker.SelectedIndex = 0; // Fixed
            ScrollModePicker.SelectedIndex = 0; // Auto
            BackgroundImagePicker.SelectedIndex = 0; // None
            AspectPicker.SelectedIndex = 0; // AspectFit
            
            WidthLabel.Text = $"Width: {WidthSlider.Value:F0}";
        }

        private void OnWidthChanged(object sender, ValueChangedEventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
                shell.FlyoutWidth = e.NewValue;

            if (WidthLabel != null)
                WidthLabel.Text = $"Width: {e.NewValue:F0}";
        }

        private void OnToggleFlyoutIcon(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null) return;

            _isCustomIcon = !_isCustomIcon;
            
            if (_isCustomIcon)
            {
               shell.FlyoutIcon = "dotnet_bot.png";
            }
            else
            {
               shell.FlyoutIcon = null; // Restore default (empty)
            }
        }

        private void OnSetBlueBackground(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
                shell.FlyoutBackgroundColor = Colors.LightBlue;
        }

        private void OnSetGradientBackground(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null) return;

            shell.FlyoutBackground = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Purple, Offset = 0.1f },
                    new GradientStop { Color = Colors.DeepPink, Offset = 0.6f },
                    new GradientStop { Color = Colors.Orange, Offset = 1.0f }
                }
            };
        }

        private void OnSetDefaultBackground(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
            {
                shell.FlyoutBackgroundColor = null;
                shell.FlyoutBackground = null;
            }
        }

        private void OnBackgroundImageChanged(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null || BackgroundImagePicker.SelectedIndex == -1) return;

            shell.FlyoutBackgroundImage = BackgroundImagePicker.SelectedIndex switch
            {
                0 => null,
                1 => ImageSource.FromFile("banner_light.png"),
                2 => ImageSource.FromFile("banner_dark.png"),
                3 => ImageSource.FromFile("dotnet_bot.png"),
                _ => null
            };
        }

        private void OnBackgroundImageAspectChanged(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null || AspectPicker.SelectedIndex == -1) return;

            shell.FlyoutBackgroundImageAspect = AspectPicker.SelectedIndex switch
            {
                0 => Aspect.AspectFit,
                1 => Aspect.AspectFill,
                2 => Aspect.Fill,
                3 => Aspect.Center,
                _ => Aspect.AspectFit
            };
        }

        private void OnSetRedBackdrop(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
                shell.FlyoutBackdrop = new SolidColorBrush(Color.FromRgba(255, 0, 0, 128));
        }

        private void OnSetDefaultBackdrop(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
                shell.FlyoutBackdrop = null;
        }

        private void OnBehaviorChanged(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null || BehaviorPicker.SelectedIndex == -1) return;

            shell.FlyoutBehavior = BehaviorPicker.SelectedIndex switch
            {
                0 => FlyoutBehavior.Disabled,
                1 => FlyoutBehavior.Flyout,
                2 => FlyoutBehavior.Locked,
                _ => FlyoutBehavior.Flyout
            };

            // Disable buttons if flyout is locked or disabled
            bool isEnabled = BehaviorPicker.SelectedIndex == 1; // Only enable for Flyout (Popover)
            OpenFlyoutButton.IsEnabled = isEnabled;
            CloseFlyoutButton.IsEnabled = isEnabled;
        }

        private void OnHeaderBehaviorChanged(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null || HeaderBehaviorPicker.SelectedIndex == -1) return;

            shell.FlyoutHeaderBehavior = HeaderBehaviorPicker.SelectedIndex == 0 
                ? FlyoutHeaderBehavior.Fixed 
                : FlyoutHeaderBehavior.Scroll;
        }

        private void OnScrollModeChanged(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null || ScrollModePicker.SelectedIndex == -1) return;

            shell.FlyoutVerticalScrollMode = (ScrollMode)ScrollModePicker.SelectedIndex;
        }

        private void OnToggleHeaderTemplate(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null) return;

            _isHeaderTemplate = !_isHeaderTemplate;
            if (_isHeaderTemplate)
            {
                shell.FlyoutHeaderTemplate = new DataTemplate(() =>
                {
                    return new Grid
                    {
                        HeightRequest = 100,
                        BackgroundColor = Colors.Black,
                        Children = { new Label { Text = "Custom Header Template", TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } }
                    };
                });
            }
            else
            {
                shell.FlyoutHeaderTemplate = null;
            }
        }

        private void OnToggleFooterTemplate(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null) return;

            _isFooterTemplate = !_isFooterTemplate;
            if (_isFooterTemplate)
            {
                shell.FlyoutFooterTemplate = new DataTemplate(() =>
                {
                    return new Label { Text = "Custom Footer Template", HeightRequest = 50, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                });
            }
            else
            {
                shell.FlyoutFooterTemplate = null;
            }
        }

        private void OnOpenFlyout(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
                shell.FlyoutIsPresented = true;
        }

        private void OnCloseFlyout(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null)
                shell.FlyoutIsPresented = false;
        }

        private void OnAddFlyoutItem(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null) return;

            var newItem = new FlyoutItem
            {
                Title = $"Item {_dynamicFlyoutItems.Count + 1}",
                Icon = "cat.png"
            };
            
            // Create a dummy page for the item
            newItem.Items.Add(new ShellContent 
            { 
                 ContentTemplate = new DataTemplate(() => new ContentPage 
                 { 
                     Title = newItem.Title,
                     Content = new Label { Text = $"Content for {newItem.Title}", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center } 
                 })
            });

            shell.Items.Add(newItem);
            _dynamicFlyoutItems.Add(newItem);
        }

        private void OnRemoveFlyoutItem(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null || _dynamicFlyoutItems.Count == 0) return;

            var lastItem = _dynamicFlyoutItems[^1];
            shell.Items.Remove(lastItem);
            _dynamicFlyoutItems.Remove(lastItem);
        }

        private void OnAddMenuItem(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell == null) return;

            var menuItem = new MenuItem
            {
                Text = $"Menu Item {_dynamicMenuItems.Count + 1}",
                IconImageSource = "home.png",
                Command = new Command(() => 
                {
                    Application.Current?.MainPage?.DisplayAlert("Menu Item Clicked", "You clicked a dynamic menu item", "OK");
                    shell.FlyoutIsPresented = false;
                })
            };

            shell.Items.Add(menuItem);
            _dynamicMenuItems.Add(menuItem);
        }

        private void OnRemoveMenuItem(object sender, EventArgs e)
        {
             var shell = this.GetShell();
             if (shell == null || _dynamicMenuItems.Count == 0) return;

            var lastItem = _dynamicMenuItems[^1];
            
            // Find the shell item that corresponds to this menu item
            // Since we added it via Items.Add(menuItem), it's wrapped.
            // We match by Title since default behavior maps Text->Title.
            var itemToRemove = shell.Items.FirstOrDefault(i => i.Title == lastItem.Text);
            
            if (itemToRemove != null)
            {
                shell.Items.Remove(itemToRemove);
            }
            
            _dynamicMenuItems.Remove(lastItem);
        }

        private void OnToggleItemVisibility(object sender, EventArgs e)
        {
            if (_dynamicFlyoutItems.Count > 0)
            {
                var item = _dynamicFlyoutItems[^1];
                item.IsVisible = !item.IsVisible;
            }
        }
    }
}
