# Building Avalonia.Controls.Maui

## Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download)

## Quick Start

Restore required .NET workloads before building:

```bash
dotnet workload restore Avalonia.Controls.Maui.slnx
```

The default build target compiles the solution and copies packages to your local NuGet cache:

```bash
dotnet run --project build/_build.csproj
```

You can also use the wrapper scripts (`build.sh`, `build.cmd`, or `build.ps1`) with the same targets.

## Build Targets

This project uses [Nuke](https://nuke.build/) for build automation. All commands run from the repo root.

| Target | Command |
|---|---|
| Compile | `dotnet run --project build/_build.csproj -- --target Compile` |
| Run tests | `dotnet run --project build/_build.csproj -- --target RunTests` |
| Create NuGet packages | `dotnet run --project build/_build.csproj -- --target CreateNugetPackages` |
| Build and copy to local NuGet cache | `dotnet run --project build/_build.csproj -- --target CopyPackagesToNuGetCache` |

The `CopyPackagesToNuGetCache` target produces packages versioned `9999.0.0-localbuild` in your local `.nuget` folder.

## Running Tests Directly

```bash
dotnet test tests/Avalonia.Controls.Maui.Tests
dotnet test tests/Avalonia.Controls.Maui.RenderTests
```

## Building Individual Projects

To compile a single project without the full Nuke pipeline:

```bash
dotnet build src/Avalonia.Controls.Maui/Avalonia.Controls.Maui.csproj -f net11.0
```

## Solutions and Filters

| File | Purpose |
|---|---|
| `Avalonia.Controls.Maui.slnx` | Full solution (source, tests, samples) |
| `Avalonia.Controls.Maui.nupkg.slnf` | Packaging filter (source + tests, used by Nuke) |
| `Avalonia.Controls.Maui.tests.slnf` | Tests filter |

## Building from Local Source

You can replace the Avalonia and/or MAUI NuGet packages with local source checkouts for source-level debugging. This is controlled by `Directory.Build.props.user`, which is git-ignored and local to your machine.

### Setup

1. Clone the repositories you want to build from source alongside this repo:

   ```
   ~/dev/
   ├── Avalonia.Controls.Maui/   # this repo
   ├── avalonia/                  # Avalonia source checkout
   └── maui/                     # MAUI source checkout
   ```

2. Copy the example file and uncomment the paths you need:

   ```bash
   cp Directory.Build.props.user.example Directory.Build.props.user
   ```

3. Edit `Directory.Build.props.user` to point to your local checkouts:

   ```xml
   <Project>
     <PropertyGroup>
       <!-- Set to your local Avalonia repo path to build from source -->
       <AvaloniaSourcePath>$(MSBuildThisFileDirectory)../avalonia</AvaloniaSourcePath>

       <!-- Set to your local MAUI repo path to build from source -->
       <MauiSourcePath>$(MSBuildThisFileDirectory)../maui</MauiSourcePath>
     </PropertyGroup>
   </Project>
   ```

   You can set either or both paths. Omit (or comment out) a path to use the NuGet package for that dependency.

### How It Works

When `AvaloniaSourcePath` or `MauiSourcePath` is set, `build/SourceBuild.targets` automatically:

- Replaces NuGet `PackageReference` items with `ProjectReference` items pointing to the local source.
- Adds explicit transitive dependencies (e.g. `Microsoft.Extensions.*`) that NuGet packages normally bring in but ProjectReferences do not.
- Imports Avalonia's build tasks from the source tree (XAML compilation, source generators).

When `MauiSourcePath` is set, the build also:

- Defines the `MAUI_SOURCE_BUILD` compilation constant.
- Generates `InternalsVisibleTo` attribute files in the MAUI source tree so that MAUI's internal APIs are accessible to Avalonia.Controls.Maui projects. These files are auto-generated at build time and cleaned on `dotnet clean`.

### Preparing the Source Checkouts

Before building, make sure the source repos are restored and have been built at least once:

> [!NOTE]
> You must build Avalonia or MAUI at least once before attempting a source build against them.

```bash
# Avalonia
cd ../avalonia
dotnet build src/Avalonia.sln

# MAUI
cd ../maui
dotnet build src/Core/src/Core.csproj
dotnet build src/Controls/src/Core/Controls.Core.csproj
```

### Switching Back to NuGet

To go back to building against NuGet packages, comment out the paths in `Directory.Build.props.user` or delete the file. If you had MAUI source builds active, run a clean first to remove the generated files from the MAUI source tree:

```bash
dotnet clean
```
