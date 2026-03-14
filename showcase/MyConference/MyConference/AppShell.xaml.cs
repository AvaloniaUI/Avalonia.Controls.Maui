namespace MyConference;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("sessiondetail", typeof(Views.SessionDetailPage));
        Routing.RegisterRoute("speakerdetail", typeof(Views.SpeakerDetailPage));
    }
}
