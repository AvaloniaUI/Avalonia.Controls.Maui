[![Hero](resources/banner_light.png)](https://avaloniaui.net?utm_source=github&utm_medium=referral&utm_content=readme_link)

# Avalonia.Controls.Maui

The repository contains the Avalonia-based Handlers and target files for .NET MAUI, replacing its native controls with drawn controls from Avalonia. It also lets you deploy to platforms previously unavailable to .NET MAUI UI applications, such as Linux and WASM.

## Building the Project

This project uses [Nuke](https://nuke.build/) for build automation. The build scripts are located in the `build/` directory.

### Available Build Targets

**NOTE**: You can also use the `build.cmd`, `build.ps1`, and `build.sh` scripts with the same targets. 

#### Local Development

For local development, run the default target which builds the packages.

```bash
dotnet run --project build/_build.csproj
```

You can also use the `CopyPackagesToNuGetCache` target, which will copy the NuGet Packages to your local `.nuget` folder with a version of `9999.0.0-localbuild`.

```bash
dotnet run --project build/_build.csproj -- --target CopyPackagesToNuGetCache
```

#### Individual Targets

You can run specific targets using the `--target` parameter:

**Build the solution:**
```bash
dotnet run --project build/_build.csproj -- --target Compile
```

**Run tests:**
```bash
dotnet run --project build/_build.csproj -- --target RunTests
```

**Create NuGet packages:**
```bash
dotnet run --project build/_build.csproj -- --target CreateNugetPackages
```

**Create packages and copy to NuGet cache:**
```bash
dotnet run --project build/_build.csproj -- --target CopyPackagesToNuGetCache
```