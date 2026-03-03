using SurfingApp.Views;

namespace SurfingApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new HomeView();
	}
}
