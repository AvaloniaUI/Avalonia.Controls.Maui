using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaDeviceDisplay
{
    private enum LinuxInhibitBackend
    {
        None,
        Portal,
        ScreenSaverDbus,
        SessionManagerDbus,
        PowerManagementDbus,
        SystemdInhibit,
        XdgScreensaver
    }

    private LinuxInhibitBackend _linuxInhibitBackend;
    private string? _linuxPortalRequestPath;
    private uint? _linuxInhibitCookie;
    private uint? _linuxSessionCookie;
    private Process? _linuxSystemdInhibitProcess;

    [SupportedOSPlatform("linux")]
    partial void UpdateKeepScreenOnLinux(bool value)
    {
        try
        {
            if (value)
            {
                if (_linuxInhibitBackend != LinuxInhibitBackend.None)
                    return;

                // Preferred on Wayland and sandboxed environments.
                if (TryInhibitViaPortal())
                {
                    _linuxInhibitBackend = LinuxInhibitBackend.Portal;
                    return;
                }

                // Session-level D-Bus inhibits used by common desktop environments.
                if (TryInhibitViaDbus())
                {
                    _linuxInhibitBackend = LinuxInhibitBackend.ScreenSaverDbus;
                    return;
                }

                if (TryInhibitViaSessionManager())
                {
                    _linuxInhibitBackend = LinuxInhibitBackend.SessionManagerDbus;
                    return;
                }

                if (TryInhibitViaPowerManagement())
                {
                    _linuxInhibitBackend = LinuxInhibitBackend.PowerManagementDbus;
                    return;
                }

                // Last process-based fallback for systems with systemd.
                if (TryInhibitViaSystemd())
                {
                    _linuxInhibitBackend = LinuxInhibitBackend.SystemdInhibit;
                    return;
                }

                // xdg-screensaver (works on many distros without gdbus)
                if (TryInhibitViaXdg(true))
                    _linuxInhibitBackend = LinuxInhibitBackend.XdgScreensaver;
            }
            else
            {
                switch (_linuxInhibitBackend)
                {
                    case LinuxInhibitBackend.Portal:
                        TryUninhibitViaPortal();
                        break;
                    case LinuxInhibitBackend.ScreenSaverDbus:
                        TryUninhibitViaDbus();
                        break;
                    case LinuxInhibitBackend.SessionManagerDbus:
                        TryUninhibitViaSessionManager();
                        break;
                    case LinuxInhibitBackend.PowerManagementDbus:
                        TryUninhibitViaPowerManagement();
                        break;
                    case LinuxInhibitBackend.SystemdInhibit:
                        StopSystemdInhibitProcess();
                        break;
                    case LinuxInhibitBackend.XdgScreensaver:
                        TryInhibitViaXdg(false);
                        break;
                }

                _linuxInhibitBackend = LinuxInhibitBackend.None;
                _linuxInhibitCookie = null;
                _linuxSessionCookie = null;
                _linuxPortalRequestPath = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update Keep Screen On Error: {ex.Message}");
        }
    }

    [SupportedOSPlatform("linux")]
    private bool TryInhibitViaPortal()
    {
        try
        {
            var token = $"avalonia_maui_{Environment.ProcessId}";
            var options = $"{{'reason': <'Keep Screen On'>, 'handle_token': <'{token}'>}}";
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = $"call --session --dest org.freedesktop.portal.Desktop --object-path /org/freedesktop/portal/desktop --method org.freedesktop.portal.Inhibit.Inhibit \"\" 8 \"{options}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var output = RunAndReadOutput(process);
            if (process.ExitCode != 0) return false;

            if (!TryParsePortalRequestPath(output, out var requestPath))
                return false;

            _linuxPortalRequestPath = requestPath;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Portal Inhibit Error: {ex.Message}");
            return false;
        }
    }

    [SupportedOSPlatform("linux")]
    private void TryUninhibitViaPortal()
    {
        if (string.IsNullOrWhiteSpace(_linuxPortalRequestPath))
            return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = $"call --session --dest org.freedesktop.portal.Desktop --object-path {_linuxPortalRequestPath} --method org.freedesktop.portal.Request.Close",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Portal Uninhibit Error: {ex.Message}");
        }
    }

    [SupportedOSPlatform("linux")]
    private bool TryInhibitViaDbus()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = "call --session --dest org.freedesktop.ScreenSaver --object-path /org/freedesktop/ScreenSaver --method org.freedesktop.ScreenSaver.Inhibit \"Avalonia.Controls.Maui\" \"Keep Screen On\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var output = RunAndReadOutput(process);

            if (process.ExitCode != 0) return false;

            if (!TryParseDbusCookie(output, out var cookie))
                return false;

            _linuxInhibitCookie = cookie;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"D-Bus Inhibit Error: {ex.Message}");
        }
        return false;
    }

    [SupportedOSPlatform("linux")]
    private void TryUninhibitViaDbus()
    {
        if (!_linuxInhibitCookie.HasValue)
            return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = $"call --session --dest org.freedesktop.ScreenSaver --object-path /org/freedesktop/ScreenSaver --method org.freedesktop.ScreenSaver.UnInhibit {_linuxInhibitCookie}",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"D-Bus Uninhibit Error: {ex.Message}");
        }
    }

    [SupportedOSPlatform("linux")]
    private bool TryInhibitViaSessionManager()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = "call --session --dest org.gnome.SessionManager --object-path /org/gnome/SessionManager --method org.gnome.SessionManager.Inhibit \"Avalonia.Controls.Maui\" 0 \"Keep Screen On\" 8",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var output = RunAndReadOutput(process);
            if (process.ExitCode != 0) return false;

            if (!TryParseDbusCookie(output, out var cookie))
                return false;

            _linuxSessionCookie = cookie;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SessionManager Inhibit Error: {ex.Message}");
            return false;
        }
    }

    [SupportedOSPlatform("linux")]
    private void TryUninhibitViaSessionManager()
    {
        if (!_linuxSessionCookie.HasValue)
            return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = $"call --session --dest org.gnome.SessionManager --object-path /org/gnome/SessionManager --method org.gnome.SessionManager.Uninhibit {_linuxSessionCookie.Value}",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SessionManager Uninhibit Error: {ex.Message}");
        }
    }

    [SupportedOSPlatform("linux")]
    private bool TryInhibitViaPowerManagement()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = "call --session --dest org.freedesktop.PowerManagement --object-path /org/freedesktop/PowerManagement/Inhibit --method org.freedesktop.PowerManagement.Inhibit.Inhibit \"Avalonia.Controls.Maui\" \"Keep Screen On\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var output = RunAndReadOutput(process);
            if (process.ExitCode != 0) return false;

            if (!TryParseDbusCookie(output, out var cookie))
                return false;

            _linuxSessionCookie = cookie;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PowerManagement Inhibit Error: {ex.Message}");
            return false;
        }
    }

    [SupportedOSPlatform("linux")]
    private void TryUninhibitViaPowerManagement()
    {
        if (!_linuxSessionCookie.HasValue)
            return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments = $"call --session --dest org.freedesktop.PowerManagement --object-path /org/freedesktop/PowerManagement/Inhibit --method org.freedesktop.PowerManagement.Inhibit.UnInhibit {_linuxSessionCookie.Value}",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PowerManagement Uninhibit Error: {ex.Message}");
        }
    }

    [SupportedOSPlatform("linux")]
    private bool TryInhibitViaSystemd()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "systemd-inhibit",
                Arguments = "--what=idle --who=\"Avalonia.Controls.Maui\" --why=\"Keep Screen On\" --mode=block sleep infinity",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var process = Process.Start(psi);
            if (process == null) return false;

            try
            {
                if (process.WaitForExit(250))
                {
                    process.Dispose();
                    return false;
                }

                _linuxSystemdInhibitProcess = process;
                return true;
            }
            catch
            {
                process.Dispose();
                throw;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"systemd-inhibit Error: {ex.Message}");
            return false;
        }
    }

    [SupportedOSPlatform("linux")]
    private void StopSystemdInhibitProcess()
    {
        if (_linuxSystemdInhibitProcess == null)
            return;

        try
        {
            if (!_linuxSystemdInhibitProcess.HasExited)
            {
                _linuxSystemdInhibitProcess.Kill(entireProcessTree: true);
                _linuxSystemdInhibitProcess.WaitForExit(3000);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"systemd-inhibit Stop Error: {ex.Message}");
        }
        finally
        {
            _linuxSystemdInhibitProcess.Dispose();
            _linuxSystemdInhibitProcess = null;
        }
    }

    /// <summary>
    /// Reads process stdout safely, draining stderr to prevent deadlocks.
    /// </summary>
    [SupportedOSPlatform("linux")]
    private static string RunAndReadOutput(Process process, int timeoutMs = 3000)
    {
        // Drain stderr asynchronously to prevent deadlock when both stdout and stderr are redirected
        process.StandardError.ReadToEnd();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(timeoutMs);
        return output;
    }

    private static bool TryParseDbusCookie(string? output, out uint cookie)
    {
        cookie = 0;
        if (string.IsNullOrWhiteSpace(output))
            return false;

        var match = DbusCookieRegex().Match(output);
        return match.Success && uint.TryParse(match.Groups[1].Value, out cookie);
    }

    private static bool TryParsePortalRequestPath(string? output, out string requestPath)
    {
        requestPath = string.Empty;
        if (string.IsNullOrWhiteSpace(output))
            return false;

        var match = PortalRequestPathRegex().Match(output);
        if (!match.Success)
            return false;

        requestPath = match.Groups[1].Value;
        return !string.IsNullOrWhiteSpace(requestPath);
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"uint32\s+(\d+)")]
    private static partial Regex DbusCookieRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"objectpath '([^']+)'")]
    private static partial Regex PortalRequestPathRegex();

    /// <summary>
    /// Fallback for distros without gdbus (e.g. minimal installs, some Wayland compositors).
    /// xdg-screensaver is part of xdg-utils, widely available across distros.
    /// </summary>
    [SupportedOSPlatform("linux")]
    private static bool TryInhibitViaXdg(bool inhibit)
    {
        try
        {
            var command = inhibit ? "suspend" : "resume";
            // xdg-screensaver suspend/resume requires a window ID; use root window 0
            var psi = new ProcessStartInfo
            {
                FileName = "xdg-screensaver",
                Arguments = $"{command} 0",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process == null) return false;
            process.WaitForExit(3000);
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"XDG Screensaver Error: {ex.Message}");
            return false;
        }
    }
}
