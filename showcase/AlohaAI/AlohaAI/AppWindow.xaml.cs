using AlohaAI.Services;
using AlohaAI.Views;

namespace AlohaAI;

public partial class AppWindow : Window
{
	public AppWindow(IProgressService progressService)
	{
		InitializeComponent();
		Page = new LoadingPage(progressService);
		// X11 doesn't extend into the client area of window adorners, so we'll turn off this view since it will
		// Extend into the client app.
		if (OperatingSystem.IsLinux() || OperatingSystem.IsMacCatalyst())
		{
			this.TitleBar = null;
		}
	}
}