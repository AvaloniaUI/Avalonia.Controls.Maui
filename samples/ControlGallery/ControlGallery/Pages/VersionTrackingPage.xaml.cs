using System.Collections;
using System.Linq;
using Avalonia.Controls.Maui.Essentials;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui;

namespace ControlGallery.Pages;

public partial class VersionTrackingPage : ContentPage
{
    readonly IVersionTracking _versionTracking;

    public VersionTrackingPage()
    {
        InitializeComponent();
        _versionTracking = IPlatformApplication.Current?.Services?.GetRequiredService<IVersionTracking>()
            ?? throw new InvalidOperationException("IVersionTracking is not registered. Call UseAvaloniaApp() and UseAvaloniaEssentials().");
        RefreshStatus();
    }

    void OnRefreshClicked(object? sender, EventArgs e) => RefreshStatus();

    void RefreshStatus()
    {
        _versionTracking.Track();

        FirstLaunchEverLabel.Text = $"IsFirstLaunchEver: {_versionTracking.IsFirstLaunchEver}";
        FirstLaunchForCurrentVersionLabel.Text = $"IsFirstLaunchForCurrentVersion: {_versionTracking.IsFirstLaunchForCurrentVersion}";
        FirstLaunchForCurrentBuildLabel.Text = $"IsFirstLaunchForCurrentBuild: {_versionTracking.IsFirstLaunchForCurrentBuild}";
        CurrentVersionLabel.Text = $"CurrentVersion: {_versionTracking.CurrentVersion}";
        CurrentBuildLabel.Text = $"CurrentBuild: {_versionTracking.CurrentBuild}";
        PreviousVersionLabel.Text = $"PreviousVersion: {FormatValue(_versionTracking.PreviousVersion)}";
        PreviousBuildLabel.Text = $"PreviousBuild: {FormatValue(_versionTracking.PreviousBuild)}";
        FirstInstalledVersionLabel.Text = $"FirstInstalledVersion: {FormatValue(_versionTracking.FirstInstalledVersion)}";
        FirstInstalledBuildLabel.Text = $"FirstInstalledBuild: {FormatValue(_versionTracking.FirstInstalledBuild)}";
        VersionHistoryLabel.Text = $"VersionHistory: {FormatHistory(_versionTracking.VersionHistory)}";
        BuildHistoryLabel.Text = $"BuildHistory: {FormatHistory(_versionTracking.BuildHistory)}";
    }

    static string FormatValue(string? value) =>
        string.IsNullOrEmpty(value) ? "(none)" : value;

    static string FormatHistory(IEnumerable values)
    {
        var entries = values
            .Cast<object?>()
            .Select(value => value?.ToString())
            .Where(value => !string.IsNullOrEmpty(value))
            .Cast<string>()
            .ToArray();

        return entries.Length == 0 ? "(empty)" : string.Join(" -> ", entries);
    }
}
