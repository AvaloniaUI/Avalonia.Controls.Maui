using System.Windows.Input;

namespace ControlGallery.Pages.ShellSamples.ShellPlayground
{
    public partial class ShellPlaygroundPage : Shell
    {
        public ICommand HelpCommand { get; }

        public ShellPlaygroundPage()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute("common_detail", typeof(ShellCommonDetailPage));

            HelpCommand = new Command(() => DisplayAlert("Help", "This sample demonstrates advanced Shell features in Avalonia.Controls.Maui.", "OK"));
            BindingContext = this;
        }
    }
}
