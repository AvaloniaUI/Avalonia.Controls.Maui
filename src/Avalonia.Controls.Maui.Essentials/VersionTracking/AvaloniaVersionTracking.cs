using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="IVersionTracking"/> backed by
/// <see cref="IPreferences"/> and <see cref="IAppInfo"/>.
/// </summary>
public class AvaloniaVersionTracking : IVersionTracking
{
    const string VersionsKey = "VersionTracking.Versions";
    const string BuildsKey = "VersionTracking.Builds";

    readonly IPreferences _preferences;
    readonly IAppInfo _appInfo;
    readonly string _sharedName;
    readonly object _locker = new();

    Dictionary<string, List<string>>? _versionTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaVersionTracking"/> class.
    /// </summary>
    /// <param name="preferences">The preferences store used to persist version history.</param>
    /// <param name="appInfo">The app-info provider used for current version/build data.</param>
    public AvaloniaVersionTracking(IPreferences preferences, IAppInfo appInfo)
    {
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _appInfo = appInfo ?? throw new ArgumentNullException(nameof(appInfo));
        _sharedName = $"{_appInfo.PackageName}.microsoft.maui.essentials.versiontracking";

        Track();
    }

    /// <inheritdoc/>
    public void Track()
    {
        if (_versionTrail != null)
            return;

        lock (_locker)
        {
            if (_versionTrail != null)
                return;

            InitVersionTracking();
        }
    }

    /// <inheritdoc/>
    public bool IsFirstLaunchEver { get; private set; }

    /// <inheritdoc/>
    public bool IsFirstLaunchForCurrentVersion { get; private set; }

    /// <inheritdoc/>
    public bool IsFirstLaunchForCurrentBuild { get; private set; }

    /// <inheritdoc/>
    public string CurrentVersion => _appInfo.VersionString;

    /// <inheritdoc/>
    public string CurrentBuild => _appInfo.BuildString;

    /// <inheritdoc/>
    public string? PreviousVersion
    {
        get
        {
            Track();
            return GetPrevious(VersionsKey);
        }
    }

    /// <inheritdoc/>
    public string? PreviousBuild
    {
        get
        {
            Track();
            return GetPrevious(BuildsKey);
        }
    }

    /// <inheritdoc/>
    public string? FirstInstalledVersion
    {
        get
        {
            Track();
            return _versionTrail![VersionsKey].FirstOrDefault();
        }
    }

    /// <inheritdoc/>
    public string? FirstInstalledBuild
    {
        get
        {
            Track();
            return _versionTrail![BuildsKey].FirstOrDefault();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> VersionHistory
    {
        get
        {
            Track();
            return _versionTrail![VersionsKey].ToArray();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> BuildHistory
    {
        get
        {
            Track();
            return _versionTrail![BuildsKey].ToArray();
        }
    }

    /// <inheritdoc/>
    public bool IsFirstLaunchForVersion(string version) =>
        CurrentVersion == version && IsFirstLaunchForCurrentVersion;

    /// <inheritdoc/>
    public bool IsFirstLaunchForBuild(string build) =>
        CurrentBuild == build && IsFirstLaunchForCurrentBuild;

    void InitVersionTracking()
    {
        IsFirstLaunchEver =
            !_preferences.ContainsKey(VersionsKey, _sharedName) ||
            !_preferences.ContainsKey(BuildsKey, _sharedName);

        if (IsFirstLaunchEver)
        {
            _versionTrail = new Dictionary<string, List<string>>(StringComparer.Ordinal)
            {
                [VersionsKey] = new List<string>(),
                [BuildsKey] = new List<string>()
            };
        }
        else
        {
            _versionTrail = new Dictionary<string, List<string>>(StringComparer.Ordinal)
            {
                [VersionsKey] = ReadHistory(VersionsKey).ToList(),
                [BuildsKey] = ReadHistory(BuildsKey).ToList()
            };
        }

        IsFirstLaunchForCurrentVersion =
            !_versionTrail[VersionsKey].Contains(CurrentVersion) ||
            CurrentVersion != GetLastInstalled(VersionsKey);

        if (IsFirstLaunchForCurrentVersion)
        {
            _versionTrail[VersionsKey].RemoveAll(v => v == CurrentVersion);
            _versionTrail[VersionsKey].Add(CurrentVersion);
        }

        IsFirstLaunchForCurrentBuild =
            !_versionTrail[BuildsKey].Contains(CurrentBuild) ||
            CurrentBuild != GetLastInstalled(BuildsKey);

        if (IsFirstLaunchForCurrentBuild)
        {
            _versionTrail[BuildsKey].RemoveAll(b => b == CurrentBuild);
            _versionTrail[BuildsKey].Add(CurrentBuild);
        }

        if (IsFirstLaunchForCurrentVersion || IsFirstLaunchForCurrentBuild)
        {
            WriteHistory(VersionsKey, _versionTrail[VersionsKey]);
            WriteHistory(BuildsKey, _versionTrail[BuildsKey]);
        }
    }

    string GetLastInstalled(string key) => _versionTrail![key].LastOrDefault() ?? string.Empty;

    string[] ReadHistory(string key) =>
        _preferences.Get(key, (string?)null, _sharedName)?
            .Split(['|'], StringSplitOptions.RemoveEmptyEntries) ??
        Array.Empty<string>();

    void WriteHistory(string key, IEnumerable<string> history) =>
        _preferences.Set(key, string.Join("|", history), _sharedName);

    string? GetPrevious(string key)
    {
        var trail = _versionTrail![key];
        return trail.Count >= 2 ? trail[^2] : null;
    }
}
