using Microsoft.Extensions.DependencyInjection;

namespace MauiSandboxApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell()) { Width = 800, Height = 600, Title = "Maui Sandbox" };
	}
}