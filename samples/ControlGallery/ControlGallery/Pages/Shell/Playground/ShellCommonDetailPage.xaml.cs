namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellCommonDetailPage : ContentPage, IQueryAttributable
    {
        public ShellCommonDetailPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("message"))
            {
                ParameterLabel.Text = $"Message: {query["message"]}";
            }
            if (query.ContainsKey("title"))
            {
                Title = query["title"].ToString();
                TitleLabel.Text = Title;
            }
        }

        private async void OnGoBack(object sender, EventArgs e)
        {
            var shell = this.GetShell();
            if (shell != null && shell.Navigation.NavigationStack.Count > 1)
            {
                await shell.GoToAsync("..");
            }
            else
            {
                try
                {
                    await Navigation.PopAsync();
                }
                catch (Exception)
                {
                    try
                    {
                        await Navigation.PopModalAsync();
                    }
                    catch (Exception)
                    {
                        // Final fallback - shell-wide absolute back if everything else fails
                        if (shell != null)
                            await shell.GoToAsync("..");
                    }
                }
            }
        }
    }
}
