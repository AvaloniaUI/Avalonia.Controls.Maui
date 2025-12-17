using Avalonia.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Avalonia.Controls.Maui.Platform;

internal class AlertManager
{
    internal class AlertRequestHelper : Microsoft.Maui.Controls.Platform.AlertManager.IAlertManagerSubscription
    {
        readonly Dictionary<Window, WindowAlertHelper> _helpers = new();
        readonly Dictionary<Window, Panel> _busyOverlays = new();
        int _busyCount;

        public void OnAlertRequested(Page sender, AlertArguments arguments)
        {
            var helper = GetHelper(sender);
            if (helper is null)
            {
                arguments.SetResult(false);
                return;
            }

            helper.HandleAlert(sender, arguments);
        }

        public void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
        {
            var helper = GetHelper(sender);
            if (helper is null)
            {
                arguments.SetResult(arguments.Cancel ?? string.Empty);
                return;
            }

            helper.HandleActionSheet(sender, arguments);
        }

        public void OnPromptRequested(Page sender, PromptArguments arguments)
        {
            var helper = GetHelper(sender);
            if (helper is null)
            {
                arguments.SetResult(arguments.Cancel ?? string.Empty);
                return;
            }

            helper.HandlePrompt(sender, arguments);
        }

        public void OnPageBusy(Page sender, bool enabled)
        {
            var platformWindow = GetPlatformWindow(sender);
            if (platformWindow is null)
                return;

            _busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);

            Dispatcher.UIThread.InvokeAsync(() => UpdateBusyIndicator(platformWindow, _busyCount > 0));
        }

        void UpdateBusyIndicator(Window window, bool isBusy)
        {
            if (isBusy)
            {
                if (_busyOverlays.ContainsKey(window))
                    return;

                // Create overlay that covers the entire window
                var overlay = new Grid
                {
                    Background = new Avalonia.Media.SolidColorBrush(Media.Color.FromArgb(77, 0, 0, 0)), // 0.3 opacity black
                };

                var progressRing = new ProgressRing()
                {
                    IsIndeterminate = true,
                    Width = 60,
                    Height = 60,
                    HorizontalAlignment = Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Layout.VerticalAlignment.Center,
                };

                if (Application.Current?.TryGetResource("ThemeForegroundBrush", Application.Current.ActualThemeVariant, out var foregroundBrush) == true 
                    && foregroundBrush is Media.IBrush brush)
                {
                    progressRing.Foreground = brush;
                }

                overlay.Children.Add(progressRing);

                // Wrap existing content in a Grid with the overlay on top
                var existingContent = window.Content;
                var wrapperGrid = new Grid();
                
                window.Content = null;
                if (existingContent is Control existingControl)
                {
                    wrapperGrid.Children.Add(existingControl);
                }
                wrapperGrid.Children.Add(overlay);
                
                window.Content = wrapperGrid;
                _busyOverlays[window] = overlay;
            }
            else
            {
                if (_busyOverlays.TryGetValue(window, out var overlay))
                {
                    // Restore original content
                    if (window.Content is Grid wrapperGrid && wrapperGrid.Children.Count > 0)
                    {
                        wrapperGrid.Children.Remove(overlay);
                        if (wrapperGrid.Children.Count == 1)
                        {
                            var originalContent = wrapperGrid.Children[0];
                            wrapperGrid.Children.Clear();
                            window.Content = originalContent;
                        }
                    }
                    _busyOverlays.Remove(window);
                }
            }
        }

        WindowAlertHelper? GetHelper(Page sender)
        {
            var platformWindow = GetPlatformWindow(sender);
            var virtualWindow = sender?.Window;

            if (platformWindow is null)
            {
                // Fall back to an existing helper when the page has not yet attached to a window
                return _helpers.Values.FirstOrDefault();
            }

            if (_helpers.TryGetValue(platformWindow, out var helper))
            {
                if (virtualWindow is null || helper.VirtualView == virtualWindow)
                    return helper;

                RemoveHelper(platformWindow);
            }

            if (virtualWindow is null)
                return null;

            helper = new WindowAlertHelper(virtualWindow, platformWindow, () => RemoveHelper(platformWindow));
            _helpers[platformWindow] = helper;
            return helper;
        }

        void RemoveHelper(Window platformWindow)
        {
            if (_helpers.TryGetValue(platformWindow, out var helper))
            {
                helper.Dispose();
                _helpers.Remove(platformWindow);
            }
        }

        static Window? GetPlatformWindow(Page? page)
        {
            var platformView = page?.Window?.Handler?.PlatformView;

            return platformView switch
            {
                Avalonia.Controls.Window window => window,
                Visual visual => TopLevel.GetTopLevel(visual) as Window,
                _ => null
            };
        }

        internal sealed class WindowAlertHelper : IDisposable
        {
            Task? _currentPopup;
            readonly Action _onDispose;

            internal WindowAlertHelper(Microsoft.Maui.Controls.Window virtualView, Window platformView, Action onDispose)
            {
                VirtualView = virtualView;
                PlatformView = platformView;
                _onDispose = onDispose;
                PlatformView.Closed += OnClosed;
            }

            public Microsoft.Maui.Controls.Window VirtualView { get; }
            public Window PlatformView { get; }

            public void Dispose()
            {
                PlatformView.Closed -= OnClosed;
            }

            void OnClosed(object? sender, EventArgs e) => _onDispose();

            public async void HandleAlert(Page sender, AlertArguments arguments)
            {
                if (!IsPageInWindow(sender))
                {
                    arguments.SetResult(false);
                    return;
                }

                await WaitForCurrentPopup();
                var task = ShowAlert(arguments);
                _currentPopup = task;
                try
                {
                    arguments.SetResult(await task.ConfigureAwait(false));
                }
                catch
                {
                    arguments.SetResult(false);
                }
                finally
                {
                    _currentPopup = null;
                }
            }

            public async void HandleActionSheet(Page sender, ActionSheetArguments arguments)
            {
                if (!IsPageInWindow(sender))
                {
                    arguments.SetResult(arguments.Cancel);
                    return;
                }

                await WaitForCurrentPopup();
                var task = ShowActionSheet(arguments);
                _currentPopup = task;
                try
                {
                    arguments.SetResult(await task.ConfigureAwait(false));
                }
                catch
                {
                    arguments.SetResult(arguments.Cancel);
                }
                finally
                {
                    _currentPopup = null;
                }
            }

            public async void HandlePrompt(Page sender, PromptArguments arguments)
            {
                if (!IsPageInWindow(sender))
                {
                    arguments.SetResult(null);
                    return;
                }

                await WaitForCurrentPopup();
                var task = ShowPrompt(arguments);
                _currentPopup = task;
                try
                {
                    arguments.SetResult(await task.ConfigureAwait(false));
                }
                catch
                {
                    arguments.SetResult(null);
                }
                finally
                {
                    _currentPopup = null;
                }
            }

            async Task WaitForCurrentPopup()
            {
                var current = _currentPopup;
                while (current != null)
                {
                    await current.ConfigureAwait(false);
                    current = _currentPopup;
                }
            }

            async Task<bool> ShowAlert(AlertArguments arguments)
            {
                var tcs = new TaskCompletionSource<bool>();

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        var dialog = new MauiAlertDialog(
                            arguments.Title ?? string.Empty,
                            arguments.Message ?? string.Empty,
                            arguments.Accept,
                            arguments.Cancel);

                        var result = await dialog.ShowDialog<bool?>(PlatformView);
                        tcs.SetResult(result ?? false);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                return await tcs.Task;
            }

            async Task<string?> ShowActionSheet(ActionSheetArguments arguments)
            {
                var tcs = new TaskCompletionSource<string?>();

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        var buttons = arguments.Buttons?.ToArray();
                        var dialog = new MauiActionSheetDialog(
                            arguments.Title ?? string.Empty,
                            arguments.Cancel,
                            arguments.Destruction,
                            buttons);

                        var result = await dialog.ShowDialog<string?>(PlatformView);
                        tcs.SetResult(result ?? arguments.Cancel);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                return await tcs.Task;
            }

            async Task<string?> ShowPrompt(PromptArguments arguments)
            {
                var tcs = new TaskCompletionSource<string?>();

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        var dialog = new MauiPromptDialog(
                            arguments.Title ?? string.Empty,
                            arguments.Message ?? string.Empty,
                            arguments.Accept,
                            arguments.Cancel,
                            arguments.Placeholder,
                            arguments.MaxLength,
                            arguments.Keyboard,
                            arguments.InitialValue);

                        var result = await dialog.ShowDialog<string?>(PlatformView);
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                return await tcs.Task;
            }

            bool IsPageInWindow(Page page)
            {
                if (page == null)
                    return false;

                if (page.Window == VirtualView)
                    return true;

                return page.Window == null;
            }
        }
    }
}