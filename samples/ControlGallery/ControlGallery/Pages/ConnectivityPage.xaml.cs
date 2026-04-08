using Microsoft.Maui.Networking;

namespace ControlGallery.Pages;

public partial class ConnectivityPage : ContentPage
{
    readonly IConnectivity _connectivity;
    int _eventCount;

    public ConnectivityPage()
    {
        InitializeComponent();
        _connectivity = Connectivity.Current;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
        UpdateStatus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    void UpdateStatus()
    {
        NetworkAccessLabel.Text = _connectivity.NetworkAccess.ToString();

        var profiles = _connectivity.ConnectionProfiles.ToList();
        ConnectionProfilesLabel.Text = profiles.Count > 0
            ? string.Join(", ", profiles)
            : "(none)";
    }

    void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        _eventCount++;
        EventCountLabel.Text = $"Events received: {_eventCount}";

        var profilesList = e.ConnectionProfiles.ToList();
        var profiles = profilesList.Count > 0 ? string.Join(", ", profilesList) : "None";
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var entry = $"[{timestamp}] Access: {e.NetworkAccess}, Profiles: {profiles}";

        EventLogLabel.Text = EventLogLabel.Text == "Waiting for connectivity changes..."
            ? entry
            : $"{entry}\n{EventLogLabel.Text}";

        UpdateStatus();
    }

    void OnRefreshClicked(object? sender, EventArgs e)
    {
        UpdateStatus();
    }
}
