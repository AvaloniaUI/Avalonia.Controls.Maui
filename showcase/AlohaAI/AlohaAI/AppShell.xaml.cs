namespace AlohaAI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("pathdetail", typeof(Views.PathDetailPage));
		Routing.RegisterRoute("lesson", typeof(Views.LessonPage));
		Routing.RegisterRoute("quiz", typeof(Views.QuizPage));
		Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
		Routing.RegisterRoute("chat", typeof(Views.ChatPage));
	}
}
