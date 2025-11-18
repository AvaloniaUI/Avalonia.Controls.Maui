using System;
using NuGet.Versioning;

#nullable enable

public static class VersionResolver
{
    public static NuGetVersion GetGitHubVersion(Version baseVersionNumber, bool isPackingToLocalCache)
    {
        return GetVersion(
            baseVersionNumber,
            isPackingToLocalCache,
            Environment.GetEnvironmentVariable("GITHUB_REF_NAME"),
            Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER"));
    }

    public static NuGetVersion GetVersion(Version baseVersionNumber, bool isPackingToLocalCache, string? refName, string? runNumber)
    {
        // Release tag
        if (NuGetVersion.TryParse(refName, out var tagVersion))
        {
            return tagVersion;
        }
        // Release branch
        else if (NuGetVersion.TryParse(refName?.Replace("release/", "") ?? "", out var releaseVersion))
        {
            return releaseVersion;
        }
        // CI build number
        else if (int.TryParse(runNumber, out var ciRun))
        {
            return NuGetVersion.Parse(
                baseVersionNumber + "-cibuild" + ciRun.ToString("0000000") + "-alpha");
        }

        if (isPackingToLocalCache)
        {
            return NuGetVersion.Parse("9999.0.0-localbuild");
        }

        return NuGetVersion.Parse(baseVersionNumber + "-localbuild-alpha");
    }
}
