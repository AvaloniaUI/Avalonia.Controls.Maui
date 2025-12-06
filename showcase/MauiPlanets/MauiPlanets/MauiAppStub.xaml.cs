namespace MauiPlanets;

public partial class MauiAppStub : Application
{
	public MauiAppStub()
	{
		InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new StartPage()));
    }
}

