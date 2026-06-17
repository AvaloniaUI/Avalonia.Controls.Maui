using Avalonia.Controls.Maui.Essentials;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaVersionTrackingTests
{
    [Fact]
    public void First_Launch_Tracks_Current_Version_And_Build()
    {
        var preferences = new TestPreferences();
        var appInfo = new TestAppInfo("Test.App", "1.0.0", "100");

        var sut = new AvaloniaVersionTracking(preferences, appInfo);

        Assert.True(sut.IsFirstLaunchEver);
        Assert.True(sut.IsFirstLaunchForCurrentVersion);
        Assert.True(sut.IsFirstLaunchForCurrentBuild);
        Assert.Equal("1.0.0", sut.CurrentVersion);
        Assert.Equal("100", sut.CurrentBuild);
        Assert.Null(sut.PreviousVersion);
        Assert.Null(sut.PreviousBuild);
        Assert.Equal("1.0.0", sut.FirstInstalledVersion);
        Assert.Equal("100", sut.FirstInstalledBuild);
        Assert.Equal(["1.0.0"], sut.VersionHistory);
        Assert.Equal(["100"], sut.BuildHistory);
    }

    [Fact]
    public void New_Version_Updates_Previous_And_History()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.1.0", "110"));

        Assert.False(sut.IsFirstLaunchEver);
        Assert.True(sut.IsFirstLaunchForCurrentVersion);
        Assert.True(sut.IsFirstLaunchForCurrentBuild);
        Assert.Equal("1.0.0", sut.PreviousVersion);
        Assert.Equal("100", sut.PreviousBuild);
        Assert.Equal("1.0.0", sut.FirstInstalledVersion);
        Assert.Equal("100", sut.FirstInstalledBuild);
        Assert.Equal(["1.0.0", "1.1.0"], sut.VersionHistory);
        Assert.Equal(["100", "110"], sut.BuildHistory);
    }

    [Fact]
    public void Same_Version_And_Build_Are_Not_Duplicated()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));

        Assert.False(sut.IsFirstLaunchEver);
        Assert.False(sut.IsFirstLaunchForCurrentVersion);
        Assert.False(sut.IsFirstLaunchForCurrentBuild);
        Assert.Equal(["1.0.0"], sut.VersionHistory);
        Assert.Equal(["100"], sut.BuildHistory);
    }

    [Fact]
    public void Downgrade_Treats_Known_Version_As_First_Launch()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "2.0.0", "200"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));

        Assert.False(sut.IsFirstLaunchEver);
        Assert.True(sut.IsFirstLaunchForCurrentVersion);
        Assert.True(sut.IsFirstLaunchForCurrentBuild);
        // Downgrade moves 1.0.0 to end of history, so 2.0.0 becomes previous and first-installed
        Assert.Equal("2.0.0", sut.PreviousVersion);
        Assert.Equal("200", sut.PreviousBuild);
        Assert.Equal("2.0.0", sut.FirstInstalledVersion);
        Assert.Equal("200", sut.FirstInstalledBuild);
    }

    [Fact]
    public void Build_Only_Change_Does_Not_Affect_Version_Flags()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "101"));

        Assert.False(sut.IsFirstLaunchEver);
        Assert.False(sut.IsFirstLaunchForCurrentVersion);
        Assert.True(sut.IsFirstLaunchForCurrentBuild);
        Assert.Equal("1.0.0", sut.CurrentVersion);
        Assert.Equal("101", sut.CurrentBuild);
        Assert.Null(sut.PreviousVersion);
        Assert.Equal("100", sut.PreviousBuild);
        Assert.Equal(["1.0.0"], sut.VersionHistory);
        Assert.Equal(["100", "101"], sut.BuildHistory);
    }

    [Fact]
    public void Three_Versions_Reports_Correct_Previous()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "2.0.0", "200"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "3.0.0", "300"));

        Assert.Equal("2.0.0", sut.PreviousVersion);
        Assert.Equal("200", sut.PreviousBuild);
        Assert.Equal("1.0.0", sut.FirstInstalledVersion);
        Assert.Equal("100", sut.FirstInstalledBuild);
        Assert.Equal(["1.0.0", "2.0.0", "3.0.0"], sut.VersionHistory);
        Assert.Equal(["100", "200", "300"], sut.BuildHistory);
    }

    [Fact]
    public void IsFirstLaunchForVersion_Returns_True_Only_For_Current()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "2.0.0", "200"));

        Assert.True(sut.IsFirstLaunchForVersion("2.0.0"));
        Assert.False(sut.IsFirstLaunchForVersion("1.0.0"));
        Assert.False(sut.IsFirstLaunchForVersion("3.0.0"));
    }

    [Fact]
    public void IsFirstLaunchForBuild_Returns_True_Only_For_Current()
    {
        var preferences = new TestPreferences();

        _ = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "100"));
        var sut = new AvaloniaVersionTracking(preferences, new TestAppInfo("Test.App", "1.0.0", "101"));

        Assert.True(sut.IsFirstLaunchForBuild("101"));
        Assert.False(sut.IsFirstLaunchForBuild("100"));
        Assert.False(sut.IsFirstLaunchForBuild("999"));
    }

    [Fact]
    public void Initializer_Tracks_Registered_VersionTracking_Service()
    {
        var probe = new TrackingProbe();
        var services = new ServiceCollection()
            .AddSingleton<IVersionTracking>(probe)
            .BuildServiceProvider();

        var sut = new AvaloniaVersionTrackingInitializer();

        sut.Initialize(services);

        Assert.Equal(1, probe.TrackCallCount);
    }

    [Fact]
    public void UseAvaloniaEssentials_Registers_Shared_Preferences_Instance()
    {
        var builder = MauiApp.CreateBuilder();

        builder.Services.AddSingleton<IAppInfo>(new TestAppInfo("Test.App", "1.0.0", "100"));
        builder.UseAvaloniaEssentials();

        using var services = builder.Services.BuildServiceProvider();

        var preferences = services.GetRequiredService<IPreferences>();

        Assert.Same(preferences, Preferences.Default);
    }

    sealed class TestAppInfo(string packageName, string versionString, string buildString) : IAppInfo
    {
        public string PackageName { get; } = packageName;
        public string Name { get; } = packageName;
        public string VersionString { get; } = versionString;
        public Version Version { get; } = Version.Parse(versionString);
        public string BuildString { get; } = buildString;
        public AppTheme RequestedTheme => AppTheme.Unspecified;
        public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;
        public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;
        public void ShowSettingsUI()
        {
        }
    }

    sealed class TestPreferences : IPreferences
    {
        readonly Dictionary<string, Dictionary<string, object?>> _store = new(StringComparer.Ordinal);

        public bool ContainsKey(string key, string? sharedName = null) =>
            GetContainer(sharedName, createIfMissing: false)?.ContainsKey(key) == true;

        public void Remove(string key, string? sharedName = null) =>
            GetContainer(sharedName, createIfMissing: false)?.Remove(key);

        public void Clear(string? sharedName = null)
        {
            var container = GetContainer(sharedName, createIfMissing: false);
            container?.Clear();
        }

        public void Set<T>(string key, T value, string? sharedName = null)
        {
            if (typeof(T) != typeof(string))
                throw new NotSupportedException($"Test preferences only support strings, received {typeof(T)}.");

            GetContainer(sharedName, createIfMissing: true)![key] = value;
        }

        public T Get<T>(string key, T defaultValue, string? sharedName = null)
        {
            var container = GetContainer(sharedName, createIfMissing: false);
            if (container is null || !container.TryGetValue(key, out var value) || value is not T typedValue)
                return defaultValue;

            return typedValue;
        }

        Dictionary<string, object?>? GetContainer(string? sharedName, bool createIfMissing)
        {
            var key = string.IsNullOrEmpty(sharedName) ? string.Empty : sharedName;

            if (createIfMissing)
            {
                if (!_store.TryGetValue(key, out var created))
                {
                    created = new Dictionary<string, object?>(StringComparer.Ordinal);
                    _store[key] = created;
                }

                return created;
            }

            _store.TryGetValue(key, out var existing);
            return existing;
        }
    }

    sealed class TrackingProbe : IVersionTracking
    {
        public int TrackCallCount { get; private set; }
        public bool IsFirstLaunchEver => false;
        public bool IsFirstLaunchForCurrentVersion => false;
        public bool IsFirstLaunchForCurrentBuild => false;
        public string CurrentVersion => "1.0.0";
        public string CurrentBuild => "1";
        public string? PreviousVersion => null;
        public string? PreviousBuild => null;
        public string? FirstInstalledVersion => "1.0.0";
        public string? FirstInstalledBuild => "1";
        public IReadOnlyList<string> VersionHistory => ["1.0.0"];
        public IReadOnlyList<string> BuildHistory => ["1"];

        public void Track() => TrackCallCount++;

        public bool IsFirstLaunchForVersion(string version) => false;

        public bool IsFirstLaunchForBuild(string build) => false;
    }
}
