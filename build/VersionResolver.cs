using System;
using System.Linq;
using System.Xml.Linq;
using NuGet.Versioning;

#nullable enable

public static class VersionResolver
{
    /// <summary>
    /// Reads the base version from Directory.Build.props by finding the default DotNetTfm
    /// and resolving the corresponding MauiVersion.
    /// </summary>
    public static Version ReadBaseVersionFromProps(string propsFilePath)
    {
        var doc = XDocument.Load(propsFilePath);

        // Find the default DotNetTfm value (e.g. "net11.0")
        var dotNetTfm = doc.Descendants("DotNetTfm").FirstOrDefault()?.Value
            ?? throw new InvalidOperationException("DotNetTfm not found in Directory.Build.props");

        // Find the MauiVersion for that TFM
        foreach (var propertyGroup in doc.Root!.Elements("PropertyGroup"))
        {
            var condition = propertyGroup.Attribute("Condition")?.Value;
            if (condition != null && condition.Contains(dotNetTfm))
            {
                var mauiVersion = propertyGroup.Element("MauiVersion")?.Value;
                if (mauiVersion != null)
                {
                    return NuGetVersion.Parse(mauiVersion).Version;
                }
            }
        }

        throw new InvalidOperationException(
            $"MauiVersion for {dotNetTfm} not found in Directory.Build.props");
    }

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
