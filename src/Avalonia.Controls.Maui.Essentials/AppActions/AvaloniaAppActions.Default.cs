extern alias TmdsDBus;

using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TmdsDBus::Tmds.DBus;

namespace Avalonia.Controls.Maui.Essentials
{
    /// <summary>
    /// Defines the D-Bus interface used to trigger app actions externally.
    /// This allows Linux desktop environments to invoke actions via D-Bus.
    /// </summary>
    [DBusInterface("com.essentials.AppActions")]
    public interface IAppActionsDBus : IDBusObject
    {
        /// <summary>
        /// Invoked by D-Bus when an app action is triggered externally.
        /// </summary>
        /// <param name="arg">Serialized argument representing the app action.</param>
        Task OnAppActionAsync(string arg);
    }

    partial class AvaloniaAppActions
    {
        /// <summary>
        /// Indicates whether the current platform supports this implementation.
        /// Only Linux is supported (D-Bus + desktop actions).
        /// </summary>
        public bool PlatformIsSupported() => OperatingSystem.IsLinux();

        /// <summary>
        /// Registers app actions on Linux using D-Bus and .desktop files.
        /// </summary>
        public async Task<bool> PlatformSetAsync(IEnumerable<AppAction> actions)
        {
            if (OperatingSystem.IsLinux())
            {
                if (Application.Current?.ApplicationLifetime == null)
                    throw new InvalidOperationException("ApplicationLifetime is not initialized");

                if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                    throw new InvalidOperationException("Only IClassicDesktopStyleApplicationLifetime is supported");

                // We create a new instance of the Current class, to get a unique WMClass
                // this will force refreshing shortcuts in the desktop environment
                var current = new Current(desktop, actions, AppActionActivated);
                await current.InitializeAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Internal class responsible for:
        /// - Registering D-Bus service
        /// - Creating desktop entry files
        /// - Handling action invocation
        /// </summary>
        class Current(IClassicDesktopStyleApplicationLifetime desktop, IEnumerable<AppAction> actions, EventHandler<AppActionEventArgs>? onAppAction) : IAppActionsDBus
        {
            class SanitizedAction
            {
                public required AppAction Action { get; set; }
                public required string ActionId { get; set; }
            }

            List<SanitizedAction> _sanitizedActions = new List<SanitizedAction>();

            // D-Bus object path required by IDBusObject
            ObjectPath IDBusObject.ObjectPath => new(currentObjectPath);

            // Unique ID per app instance to avoid collisions
            string uid = Guid.NewGuid().ToString("N");

            // Unique identifiers for D-Bus service and object
            string currentObjectPath => "/com/essentials/AppActions" + uid;
            string currentInterface => "com.essentials.AppActions" + uid;
            string currentServiceBus => currentInterface + ".service";

            /// <summary>
            /// Initializes D-Bus service and desktop integration.
            /// </summary>
            public async Task InitializeAsync()
            {
                // Resolve dotnet executable path
                var dotnet = InstallPath().Replace("\n", string.Empty);

                if (string.IsNullOrWhiteSpace(dotnet))
                    throw new InvalidOperationException("Unable to find installed dotnet path");

                //When using ConfigureEssentials
                //We need the main window to set the WM_CLASS property,
                //so we wait until it's initialized
                var window = await WaitForMainWindowAsync();

                // Set WM_CLASS so desktop environments associate actions with the window
                X11Properties.SetWmClass(window, uid);

                var dll = AppContext.BaseDirectory;

                // Register D-Bus service and desktop actions
                await RegisterDBusService(dotnet, dll);
                await RegisterDesktopFiles(dotnet, dll);
            }

            /// <summary>
            /// Asynchronously waits for the application's main window to be initialized.
            /// </summary>
            /// <param name="timeoutMs">Maximum time to wait for the main window, in milliseconds. Default is 10000.</param>
            /// <returns>A task that completes with the application's main Window once it becomes available.</returns>
            /// <exception cref="TimeoutException">Thrown if the main window is not initialized before the specified timeout elapses.</exception>
            private async Task<Window> WaitForMainWindowAsync(int timeoutMs = 10000)
            {
                var elapsed = 0;
                const int delay = 50;

                while (desktop.MainWindow == null)
                {
                    await Task.Delay(delay);
                    elapsed += delay;

                    if (elapsed >= timeoutMs)
                        throw new TimeoutException("MainWindow was not initialized in time");
                }

                return desktop.MainWindow;
            }

            /// <summary>
            /// Registers the D-Bus service and exposes this object.
            /// Also writes a .service file so the session bus can activate it.
            /// </summary>
            private async Task RegisterDBusService(string dotnet, string dll)
            {
                var dbusFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $".local/share/dbus-1/services/{currentServiceBus}"
                );

                // D-Bus service definition file
                var dbus = $@"[D-BUS Service]
Name={currentInterface}
Exec={dotnet} {dll}
";

                var directory = Path.GetDirectoryName(dbusFile);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory!);

                await File.WriteAllTextAsync(dbusFile, dbus);

                // Ensure correct permissions
                Process.Start("chmod", $"644 {dbusFile}");

                // Connect to session bus and register object + service
                var connection = new Connection(Address.Session!);
                await connection.ConnectAsync();
                await connection.RegisterObjectAsync(this);
                await connection.RegisterServiceAsync(currentInterface, ServiceRegistrationOptions.ReplaceExisting);

                // Cleanup on app exit
                desktop.Exit += Desktop_Exit;

                void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
                {
                    desktop.Exit -= Desktop_Exit;
                    connection.Dispose();

                    if (File.Exists(dbusFile))
                        File.Delete(dbusFile);
                }
            }

            /// <summary>
            /// Creates a .desktop file with custom actions.
            /// These appear in Linux desktop menus (e.g., right-click app icon).
            /// </summary>
            private async Task RegisterDesktopFiles(string dotnet, string dll)
            {
                var name = Assembly.GetEntryAssembly()?.GetName().Name;
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidOperationException("Unable to determine application name from entry assembly");

                string desktopFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $".local/share/applications/{name}{uid}.desktop"
                );

                _sanitizedActions = actions
                     .Select((a, i) => new SanitizedAction()
                     {
                         Action = a,
                         ActionId = CreateActionId(a.Title, i)

                     }).ToList();

                // Base desktop entry
                string content = $@"[Desktop Entry]
Name={name}
Exec={dotnet} {dll} {uid}
Type=Application
Categories=Utility;
StartupNotify=true
StartupWMClass={uid}
Actions={string.Join(";", _sanitizedActions.Select(a => a.ActionId))};
";

                // Add individual actions
                foreach (var entry in _sanitizedActions)
                {
                    content += $@"[Desktop Action {entry.ActionId}]
Name={entry.Action.Title}
Exec=gdbus call --session --dest {currentInterface} --object-path {currentObjectPath} --method com.essentials.AppActions.OnAppAction ""{EscapeDesktopArg(entry.ActionId)}""
";

                    if (!string.IsNullOrWhiteSpace(entry.Action.Subtitle))
                        content += $"Comment={entry.Action.Subtitle}";
                }

                await File.WriteAllTextAsync(desktopFile, content);

                // Make file executable
                Process.Start("chmod", $"+x {desktopFile}");

                // Cleanup on exit
                desktop.Exit += Desktop_Exit;

                void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
                {
                    desktop.Exit -= Desktop_Exit;

                    if (File.Exists(desktopFile))
                        File.Delete(desktopFile);
                }
            }

            /// <summary>
            /// Called when a D-Bus app action is triggered.
            /// Converts the argument back into an AppAction and raises the event.
            /// </summary>
            public Task OnAppActionAsync(string arg)
            {
                var action = _sanitizedActions?.FirstOrDefault(a => a.ActionId == arg);
                if (action != null)
                    onAppAction?.Invoke(this, new AppActionEventArgs(action.Action));

                return Task.CompletedTask;
            }

            static string CreateActionId(string title, int index)
            {
                // Stable sanitized ID
                var sanitized = Regex.Replace(title.ToLowerInvariant(), @"[^a-z0-9]+", "-")
                    .Trim('-');

                if (string.IsNullOrWhiteSpace(sanitized))
                    sanitized = "action";

                return $"{sanitized}-{index}";
            }

            static string EscapeDesktopArg(string value)
            {
                // Minimal quoting for desktop Exec arguments
                return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            }

            /// <summary>
            /// Finds the installed dotnet executable using 'which dotnet'.
            /// Required to launch the app from desktop/D-Bus.
            /// </summary>
            private static string InstallPath()
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "dotnet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);

                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output;
                }

                return string.Empty;
            }
        }
    }
}