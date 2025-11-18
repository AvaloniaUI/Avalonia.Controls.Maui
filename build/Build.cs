using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
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
    Target CopyPackagesToNuGetCache => _ => _
        .DependsOn(CreateNugetPackages)
        .Executes(() => NugetCache.InstallLibraryToNuGetCache(
            Output.GlobFiles("*.nupkg"),
            RootDirectory,
            GetVersion()));

    string GetVersion() => VersionResolver
        .GetGitHubVersion(
            baseVersionNumber: new Version(11, 3, 999),
            isPackingToLocalCache: RunningTargets.Concat(ScheduledTargets)
                .Any(t => t.Name == nameof(CopyPackagesToNuGetCache)))
        .ToString();
}
