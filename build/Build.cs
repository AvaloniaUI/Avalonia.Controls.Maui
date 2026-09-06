using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using NukeExtensions;
using Serilog;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => IsLocalBuild ?
        Execute<Build>(x => x.CopyPackagesToNuGetCache) :
        Execute<Build>(x => x.CreateNugetPackages);

    // net10.0 to match this build project's own TFM, so generating SBOMs never needs a runtime
    // the build itself doesn't already require.
    [NuGetPackage("CycloneDX", "CycloneDX.dll", Framework = "net10.0")] readonly Tool CycloneDx = null!;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = Configuration.Release;
    [Parameter]
    readonly AbsolutePath Output = RootDirectory / "artifacts" / "packages";

    readonly AbsolutePath TestResultsDirectory = RootDirectory / "artifacts" / "test-results";

    readonly AbsolutePath SolutionFile = RootDirectory / "Avalonia.Controls.Maui.nupkg.slnf";

    Target OutputParameters => _ => _
    .Executes(() =>
    {
        Log.Information("Configuration: {Configuration}", Configuration);
        Log.Information("Output: {AbsolutePath}", Output);
        Log.Information("Version: {GetVersion}", GetVersion());
    });

    Target Compile => _ => _
        .DependsOn(OutputParameters)
        .Executes(() => DotNetTasks.DotNetBuild(c => c
            .SetProjectFile(SolutionFile)
            .SetVersion(GetVersion())
            .SetProperty("CopyLocalLockFileAssemblies", true)
            .SetConfiguration(Configuration)
        ));

    Target RunTests => _ => _
        .DependsOn(OutputParameters)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();

            DotNetTasks.DotNetTest(c => c
                .SetProjectFile(SolutionFile)
                .SetVerbosity(DotNetVerbosity.minimal)
                .SetConfiguration(Configuration)
                .SetResultsDirectory(TestResultsDirectory)
                .SetLoggers("trx;LogFileName=test-results.trx")
            );
        });

    Target CreateNugetPackages => _ => _
        .DependsOn(OutputParameters)
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(c => c
                .SetProject(SolutionFile)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetContinuousIntegrationBuild(true)
                .SetProperty("PackageVersion", GetVersion())
                .SetConfiguration(Configuration)
                .SetOutputDirectory(Output)
            );
        });

    // Source projects whose dependencies each shipped package carries, for packages that aren't
    // simply the pack output of the identically-named project. Avalonia.Controls.Maui additionally
    // ships the source generator's assembly under analyzers/dotnet/cs, so that project is a
    // constituent of the package and its dependencies belong in the package's SBOM.
    static readonly Dictionary<string, string[]> SbomConstituentProjects = new()
    {
        ["Avalonia.Controls.Maui"] = new[]
        {
            "Avalonia.Controls.Maui",
            "Avalonia.Controls.Maui.SourceGenerators"
        }
    };

    // Generates a per-package CycloneDX SBOM (EU Cyber Resilience Act evidence) and embeds it into
    // each .nupkg at _manifest/cyclonedx/bom.cdx.json. TriggeredBy makes the CI target
    // 'CreateNugetPackages' produce SBOMs without any change to the build commands.
    Target CreateSbom => _ => _
        .DependsOn(CreateNugetPackages)
        .TriggeredBy(CreateNugetPackages)
        .Executes(() =>
        {
            var sbomOutput = RootDirectory / "artifacts" / "sbom";
            sbomOutput.CreateOrCleanDirectory();

            // Mirror SbomGenerator.Generate's guard: producing zero SBOMs would look like success
            // while shipping packages without CRA evidence. GenerateForPackage is called per-nupkg
            // here (for the constituent-project mapping), so the empty-case guard must live here too.
            var packages = Output.GlobFiles("*.nupkg");
            if (packages.Count == 0)
                throw new InvalidOperationException(
                    $"SBOM: no .nupkg files found in {Output} - was CreateSbom run before packing?");

            foreach (var nupkg in packages)
            {
                var packageId = SbomGenerator.ReadPackageId(nupkg);
                var constituents = SbomConstituentProjects.TryGetValue(packageId, out var projects)
                    ? projects
                    : new[] { packageId };
                // Read the version back from the package rather than using GetVersion(): NuGet
                // normalises what pack was given (the four-part '11.0.0.0-cibuildN-alpha' becomes
                // '11.0.0-cibuildN-alpha'), and the SBOM must state the version that shipped.
                SbomGenerator.GenerateForPackage(CycloneDx, RootDirectory, nupkg, sbomOutput,
                    SbomGenerator.ReadPackageVersion(nupkg), packageId, constituents);
            }
        });

    Target CopyPackagesToNuGetCache => _ => _
        .DependsOn(CreateNugetPackages)
        // CreateSbom embeds the SBOM into each .nupkg, so copy to the cache only afterwards.
        .After(CreateSbom)
        .Executes(() => NugetCache.InstallLibraryToNuGetCache(
            Output.GlobFiles("*.nupkg"),
            RootDirectory,
            GetVersion()));

    string GetVersion() => VersionResolver
        .GetGitHubVersion(
            baseVersionNumber: VersionResolver.ReadBaseVersionFromProps(RootDirectory / "Directory.Build.props"),
            isPackingToLocalCache: RunningTargets.Concat(ScheduledTargets)
                .Any(t => t.Name == nameof(CopyPackagesToNuGetCache)))
        .ToString();
}
