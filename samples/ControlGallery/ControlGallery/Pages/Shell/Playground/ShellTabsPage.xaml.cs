namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellTabsPage : ContentPage
    {
        private List<ShellSection> _dynamicSections = new();

        public ShellTabsPage()
        {
            InitializeComponent();
        }

        private void OnAddTab(object sender, EventArgs e)
        {
            var shell = this.GetShell();

            if (shell == null)
            {
                return;
            }

            if (shell.CurrentItem?.CurrentItem is ShellSection currentSection)
            {
                var newSection = new ShellSection
                {
                    Title = $"Dynamic {_dynamicSections.Count + 1}",
                    Icon = "dotnet_bot.png"
                };
                newSection.Items.Add(new ShellContent 
                { 
                    Content = new ShellCommonDetailPage { Title = newSection.Title } 
                });

                if (shell.CurrentItem != null)
                {
                    shell.CurrentItem.Items.Add(newSection);
                    _dynamicSections.Add(newSection);
                }
            }
        }

        private void OnRemoveTab(object sender, EventArgs e)
        {
            var shell = this.GetShell();

            if (shell == null)
            {
                return;
            }

            if (_dynamicSections.Count > 0)
            {
                var last = _dynamicSections[^1];
                if (shell.CurrentItem != null)
                {
                    shell.CurrentItem.Items.Remove(last);
                    _dynamicSections.Remove(last);
                }
            }
        }

        private void OnSelectFirstTab(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell?.CurrentItem?.Items.Count > 0)
            {
                shell.CurrentItem.CurrentItem = shell.CurrentItem.Items[0];
            }
        }

        private void OnSelectLastTab(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell?.CurrentItem?.Items.Count > 0)
            {
                shell.CurrentItem.CurrentItem = shell.CurrentItem.Items[^1];
            }
        }

        private void OnToggleTabVisibility(object sender, EventArgs e)
        {
             var shell = this.GetShell();
             if (shell?.CurrentItem != null)
             {
                 // Find the 'Search' tab (ShellSection)
                 var searchTab = shell.CurrentItem.Items.FirstOrDefault(s => s.Title == "Search");
                 if (searchTab != null)
                 {
                     searchTab.IsVisible = !searchTab.IsVisible;
                 }
             }
        }

        private void OnSetTabBarBackgroundColor(object sender, EventArgs e) => SetTabBarColor(Shell.TabBarBackgroundColorProperty, sender);
        private void OnClearTabBarBackgroundColor(object sender, EventArgs e) => ClearTabBarColor(Shell.TabBarBackgroundColorProperty);

        private void OnSetTabBarDisabledColor(object sender, EventArgs e) => SetTabBarColor(Shell.TabBarDisabledColorProperty, sender);
        private void OnClearTabBarDisabledColor(object sender, EventArgs e) => ClearTabBarColor(Shell.TabBarDisabledColorProperty);

        private void OnSetTabBarForegroundColor(object sender, EventArgs e) => SetTabBarColor(Shell.TabBarForegroundColorProperty, sender);
        private void OnClearTabBarForegroundColor(object sender, EventArgs e) => ClearTabBarColor(Shell.TabBarForegroundColorProperty);

        private void OnSetTabBarTitleColor(object sender, EventArgs e) => SetTabBarColor(Shell.TabBarTitleColorProperty, sender);
        private void OnClearTabBarTitleColor(object sender, EventArgs e) => ClearTabBarColor(Shell.TabBarTitleColorProperty);

        private void OnSetTabBarUnselectedColor(object sender, EventArgs e) => SetTabBarColor(Shell.TabBarUnselectedColorProperty, sender);
        private void OnClearTabBarUnselectedColor(object sender, EventArgs e) => ClearTabBarColor(Shell.TabBarUnselectedColorProperty);

        private void OnResetTabBarColors(object sender, EventArgs e)
        {
            ClearTabBarColor(Shell.TabBarBackgroundColorProperty);
            ClearTabBarColor(Shell.TabBarDisabledColorProperty);
            ClearTabBarColor(Shell.TabBarForegroundColorProperty);
            ClearTabBarColor(Shell.TabBarTitleColorProperty);
            ClearTabBarColor(Shell.TabBarUnselectedColorProperty);
        }

        private void SetTabBarColor(BindableProperty property, object sender)
        {
            if (sender is Button button && Color.TryParse(button.CommandParameter?.ToString() ?? string.Empty, out Color color))
            {
                var shell = this.GetShell();
                shell?.SetValue(property, color);
            }
        }

        private void ClearTabBarColor(BindableProperty property)
        {
            var shell = this.GetShell();
            shell?.ClearValue(property);
        }
    }
}