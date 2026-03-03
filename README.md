[![Hero](resources/banner_light.png)](https://avaloniaui.net?utm_source=github&utm_medium=referral&utm_content=readme_link)

# Avalonia.Controls.Maui

This repository contains the Avalonia-based Handlers and target files for .NET MAUI. With this, you can replace its native controls with drawn controls from Avalonia. It also lets you deploy to platforms previously unavailable to .NET MAUI UI applications, such as Linux and WASM, as well as through different frameworks, like macOS AppKit.

## Projects

[Avalonia.Controls.Maui](/src/Avalonia.Controls.Maui/)

[Avalonia.Controls.Maui.Desktop](/src/Avalonia.Controls.Maui.Desktop/)

[Avalonia.Controls.Maui.SourceGenerators](/src/Avalonia.Controls.Maui.SourceGenerators/)

[Avalonia.Controls.Maui.Essentials](/src/Avalonia.Controls.Maui.Essentials/)

[Avalonia.Controls.Maui.Graphics](/src/Avalonia.Controls.Maui.Graphics/)

[Avalonia.Controls.Maui.Maps.Mapsui](/src/Avalonia.Controls.Maui.Maps.Mapsui/)

## Building the Project

We use [Nuke](https://nuke.build/) for build automation. The build scripts are located in the `build/` directory.

### Build Targets

**NOTE**: You can also use the `build.cmd`, `build.ps1`, and `build.sh` scripts with the same targets. 

| Target | Command |
|--------|---------|
| Build the solution | `dotnet run --project build/_build.csproj -- --target Compile` |
| Run tests | `dotnet run --project build/_build.csproj -- --target RunTests` |
| Create NuGet packages | `dotnet run --project build/_build.csproj -- --target CreateNugetPackages` |
| Create packages and copy to NuGet cache | `dotnet run --project build/_build.csproj -- --target CopyPackagesToNuGetCache` |

The `CopyPackagesToNuGetCache` target will copy the NuGet Packages to your local `.nuget` folder with a version of `9999.0.0-localbuild`.
