# Security Policy

## Supported Versions

Security fixes are applied to the latest release only. Older versions do not receive backported security patches.

| Version | Supported |
|---|---|
| Latest release | Yes |
| Older releases | No |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Report vulnerabilities privately using [GitHub's private vulnerability reporting](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/security/advisories/new), or send an email to **tim@avaloniaui.net or javier@avaloniaui.net**.

Include as much of the following as possible:

- A description of the vulnerability and its potential impact
- The affected component, file, or version
- Steps to reproduce or a proof-of-concept
- Any suggested mitigations, if known

We aim to acknowledge reports within **5 business days** and to provide a resolution timeline within **14 business days**. We will keep you informed of progress and credit you in the advisory unless you prefer to remain anonymous.

## Scope

This policy covers the packages published from this repository:

- `Avalonia.Controls.Maui`
- `Avalonia.Controls.Maui.Desktop`
- `Avalonia.Controls.Maui.Essentials`
- `Avalonia.Controls.Maui.Compatibility`
- `Avalonia.Controls.Maui.SkiaSharp.Views`
- `Avalonia.Controls.Maui.SourceGenerators`

Security issues in **Avalonia** itself should be reported to the [Avalonia repository](https://github.com/AvaloniaUI/Avalonia/security). Issues in **.NET MAUI** should be reported to [Microsoft](https://www.microsoft.com/en-us/msrc).

## Known Considerations

- **Network requests in `Avalonia.Controls.Maui.Maps.Mapsui`:** The map handler performs an HTTP request to a third-party geolocation API (`ip-api.com`) to determine an initial map center when no location is provided. This request is made using the device's IP address. If this behavior is a concern for your application, avoid using the map handler's automatic location fallback or configure an explicit initial position.
