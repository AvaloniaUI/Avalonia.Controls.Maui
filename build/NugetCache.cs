using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using NuGet.Configuration;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

public class NugetCache
{
    /// <summary>
    /// Installs a library to the local NuGet cache.
    /// This is useful for testing packages without having to push them to a remote source.
    /// .NET SDK treats them as installed and available from any project.
    /// </summary>
    /// <param name="packageFiles">Collection of Nupkg files to install to the cache.</param>
    /// <param name="rootDirectory">Root directory to load nuget settings from.</param>
    public static void InstallLibraryToNuGetCache(
        IReadOnlyCollection<AbsolutePath> packageFiles,
        string rootDirectory,
        string version = "9999.0.0-localbuild")
    {
        var globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(
            Settings.LoadDefaultSettings(rootDirectory));

        if (packageFiles.Count == 0)
        {
            throw new InvalidOperationException("No nupkg files were found.");
        }

        foreach (var path in packageFiles)
        {
            using var f = File.Open(path.ToString(), FileMode.Open, FileAccess.Read);
            using var zip = new ZipArchive(f, ZipArchiveMode.Read);
            var nuspecEntry = zip.Entries.First(e => e.FullName.EndsWith(".nuspec") && e.FullName == e.Name);
            var packageId = XDocument.Load(nuspecEntry.Open()).Document!.Root!
                .Elements().First(x => x.Name.LocalName == "metadata")
                .Elements().First(x => x.Name.LocalName == "id").Value;

            var packagePath = Path.Combine(
                globalPackagesFolder,
                packageId.ToLowerInvariant(),
                version);

            if (Directory.Exists(packagePath))
                Directory.Delete(packagePath, true);
            Directory.CreateDirectory(packagePath);
            zip.ExtractToDirectory(packagePath);
            File.WriteAllText(Path.Combine(packagePath, ".nupkg.metadata"), @"{
  ""version"": 2,
  ""contentHash"": ""FnIKqnvWIoQ+6ZZcVGX0dZyFA9A5GaRFTfTK+bj3coj0Eb528+4GADTMTIb2pmx/lpi79ZXJAln1A+Lyr+i6Vw=="",
  ""source"": ""https://api.nuget.org/v3/index.json""
}");
            Log.Information("Package path is " + packagePath);
        }
    }

    public static void InstallNetTool(string packageId, string version, string packagesDirectory)
    {
        try
        {
            DotNetTasks.DotNetToolUninstall(c => c
                .SetGlobal(true)
                .SetPackageName(packageId));
        }
        catch
        {
            // Ignore
        }

        DotNetTasks.DotNetToolInstall(c => c
            .SetGlobal(true)
            .SetPackageName(packageId)
            .AddSources(packagesDirectory)
            .SetVersion(version)
            .SetProcessAdditionalArguments("--ignore-failed-sources", "--no-cache"));
        Log.Information("Installed {PackageId}", packageId);
    }
}
