[![Hero](resources/banner_light.png)](https://avaloniaui.net?utm_source=github&utm_medium=referral&utm_content=readme_link)

# Avalonia.Controls.Maui

This repository contains the Avalonia-based Handlers and target files for .NET MAUI. With this, you can replace its native controls with drawn controls from Avalonia. It also lets you deploy to platforms previously unavailable to .NET MAUI UI applications, such as Linux and WASM, as well as through different frameworks, like macOS AppKit.

For info on how to build the project, reference our [build docs](/docs/build.md).

## Documentation

- [Configuration and Setup](/docs/config-and-setup.md)
- [Build](/docs/build.md)
- [Architecture](/docs/architecture.md)
- [Embedding Avalonia Controls in MAUI](/docs/embedding-avalonia-controls-in-maui.md)
- [Known Issues](/docs/known-issues.md)

## Projects

| Package | Purpose |
|---|---|
| [Avalonia.Controls.Maui](/src/Avalonia.Controls.Maui/) | Core handlers for all built-in .NET MAUI controls |
| [Avalonia.Controls.Maui.Desktop](/src/Avalonia.Controls.Maui.Desktop/) | Metapackage for desktop bootstrapping |
| [Avalonia.Controls.Maui.Essentials](/src/Avalonia.Controls.Maui.Essentials/) | Avalonia implementations of `Microsoft.Maui.Essentials` APIs |
| [Avalonia.Controls.Maui.Compatibility](/src/Avalonia.Controls.Maui.Compatibility/) | Handlers for deprecated MAUI controls (`Frame`, `ListView`, `TableView`) |
| [Avalonia.Controls.Maui.SkiaSharp.Views](/src/Avalonia.Controls.Maui.SkiaSharp.Views/) | Avalonia handlers for `SKCanvasView` and `SKGLView` |
| [Avalonia.Controls.Maui.SourceGenerators](/src/Avalonia.Controls.Maui.SourceGenerators/) | Build-time source generators |
