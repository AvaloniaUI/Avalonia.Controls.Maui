namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellSearchPage : ContentPage
    {
        public ShellSearchPage()
        {
            InitializeComponent();
        }

        private void OnToggleSearchVisibility(object sender, EventArgs e)
        {
            var handler = Shell.GetSearchHandler(this);
            if (handler != null)
            {
                handler.SearchBoxVisibility = handler.SearchBoxVisibility == SearchBoxVisibility.Hidden 
                    ? SearchBoxVisibility.Expanded 
                    : SearchBoxVisibility.Hidden;
            }
        }

        private void OnClearSearch(object sender, EventArgs e)
        {
            var handler = Shell.GetSearchHandler(this);
            if (handler != null)
            {
                handler.Query = string.Empty;
            }
        }
    }
}