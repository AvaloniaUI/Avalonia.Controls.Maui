using Avalonia.Controls.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace ControlGallery;

public partial class App : Application
{
		public App()
		{
				InitializeComponent();
		}

		protected override Window CreateWindow(IActivationState activationState)
		{
				return new Window(new NavigationPage(new MainPage()));
		}
}
