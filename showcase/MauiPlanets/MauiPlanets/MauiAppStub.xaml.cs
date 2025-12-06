namespace MauiPlanets;

public partial class MauiAppStub : Application
{
	public MauiAppStub()
	{
		InitializeComponent();

        MainPage = new NavigationPage(new StartPage());
    }
}

