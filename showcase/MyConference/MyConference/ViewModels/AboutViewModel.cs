using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyConference.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public string EventName => EventConfig.EventName;
    public string EventTagline => EventConfig.EventTagline;
    public string EventDate => EventConfig.EventDate;
    public string EventLocation => EventConfig.EventLocation;
    public string EventWebsite => EventConfig.EventWebsite;
    public string AppVersion => $"v{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

    [RelayCommand]
    private async Task OpenWebsite()
    {
        try
        {
            await Browser.Default.OpenAsync(EventWebsite, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            // Ignore if browser not available
        }
    }
}
