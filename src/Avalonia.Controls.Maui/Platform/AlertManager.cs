using Avalonia.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Avalonia.Controls.Maui.Platform;

internal class AlertManager
{
    internal class AlertRequestHelper : Microsoft.Maui.Controls.Platform.AlertManager.IAlertManagerSubscription
    {
        readonly Dictionary<Control, OverlayHelper> _helpers = new();

        public void OnAlertRequested(Page sender, AlertArguments arguments)
        {
            var overlayHelper = GetOverlayHelper(sender);
            if (overlayHelper is null)
            {
                arguments.SetResult(false);
                return;
            }

            overlayHelper.HandleAlert(arguments);
        }

        public void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
        {
            var overlayHelper = GetOverlayHelper(sender);
            if (overlayHelper is null)
            {
                arguments.SetResult(arguments.Cancel ?? string.Empty);
                return;
            }

            overlayHelper.HandleActionSheet(arguments);
        }

        public void OnPromptRequested(Page sender, PromptArguments arguments)
        {
            var overlayHelper = GetOverlayHelper(sender);
            if (overlayHelper is null)
            {
                arguments.SetResult(arguments.Cancel ?? string.Empty);
                return;
            }

            overlayHelper.HandlePrompt(arguments);
        }

        public void OnPageBusy(Page sender, bool enabled)
        {
            var overlayHelper = GetOverlayHelper(sender);
            overlayHelper?.SetBusy(enabled);
        }

        OverlayHelper? GetOverlayHelper(Page sender)
        {
            var platformHost = GetPlatformHost(sender);
            if (platformHost is null)
                return null;

            if (!_helpers.TryGetValue(platformHost, out var overlayHelper))
            {
                overlayHelper = new OverlayHelper(platformHost, () => _helpers.Remove(platformHost));
                _helpers[platformHost] = overlayHelper;
            }
            return overlayHelper;
        }

        /// <summary>
        /// Gets the platform host control that can contain overlays.
        /// For desktop apps, this is a Window.
        /// For single-view apps (WASM/mobile), this is the MauiAvaloniaContent.
        /// </summary>
        static Control? GetPlatformHost(Page? page)
        {
            var platformView = page?.Window?.Handler?.PlatformView;

            return platformView switch
            {
                // Desktop: Standard Avalonia Window
                Avalonia.Controls.Window window => window,
                // Single-view: MauiAvaloniaContent or any ContentControl
                MauiAvaloniaContent content => content,
                ContentControl cc => cc,
                // Fallback: Try to get the visual root
                Visual visual => TopLevel.GetTopLevel(visual) as Control,
                _ => null
            };
        }

        /// <summary>
        /// Helper class that manages overlay injection for both Window and ContentControl hosts.
        /// </summary>
        internal sealed class OverlayHelper : IDisposable
        {
            private readonly Control _host;
            private readonly Action _onDispose;
            private Grid? _overlayGrid;
            private Grid? _busyGrid;
            private Grid? _dialogGrid;
            private int _busyCount;

            internal OverlayHelper(Control host, Action onDispose)
            {
                _host = host;
                _onDispose = onDispose;
                
                // Subscribe to lifecycle events based on host type
                if (_host is Window window)
                {
                    window.Closed += OnHostClosed;
                }
                else
                {
                    _host.DetachedFromVisualTree += OnHostDetached;
                }
            }

            public void Dispose()
            {
                if (_host is Window window)
                {
                    window.Closed -= OnHostClosed;
                }
                else
                {
                    _host.DetachedFromVisualTree -= OnHostDetached;
                }
            }

            private void OnHostClosed(object? sender, EventArgs e)
            {
                Dispose();
                _onDispose();
            }

            private void OnHostDetached(object? sender, VisualTreeAttachmentEventArgs e)
            {
                Dispose();
                _onDispose();
            }

            private void EnsureOverlay()
            {
                var currentContent = GetHostContent();
                
                if (_overlayGrid != null && currentContent is Grid g && g.Children.Contains(_overlayGrid))
                    return;

                // Create base overlay implementation
                _overlayGrid = new Grid
                {
                    ZIndex = 9999, // High ZIndex
                    RowDefinitions = new RowDefinitions("1*")
                };

                // Busy Indicator Layer (Bottom)
                _busyGrid = new Grid
                {
                    IsVisible = false,
                    Background = new Avalonia.Media.SolidColorBrush(Media.Color.FromArgb(77, 0, 0, 0))
                };
                
                var progressRing = new ProgressRing
                {
                    IsIndeterminate = true,
                    Width = 60,
                    Height = 60,
                    HorizontalAlignment = Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Layout.VerticalAlignment.Center
                };
                
                if (Application.Current?.TryGetResource("ThemeForegroundBrush", Application.Current.ActualThemeVariant, out var foregroundBrush) == true 
                    && foregroundBrush is Media.IBrush brush)
                {
                    progressRing.Foreground = brush;
                }
                _busyGrid.Children.Add(progressRing);
                
                // Dialog Layer (Top)
                _dialogGrid = new Grid();

                _overlayGrid.Children.Add(_busyGrid);
                _overlayGrid.Children.Add(_dialogGrid);

                // Inject into host content
                var existingContent = currentContent;
                if (existingContent is Grid wrapper && wrapper.Tag as string == "OverlayWrapper")
                {
                    if (!wrapper.Children.Contains(_overlayGrid))
                        wrapper.Children.Add(_overlayGrid);
                }
                else
                {
                    // Wrap existing content with overlay
                    var wrapperGrid = new Grid { Tag = "OverlayWrapper" };
                    SetHostContent(null);
                    if (existingContent is Control c) wrapperGrid.Children.Add(c);
                    wrapperGrid.Children.Add(_overlayGrid);
                    SetHostContent(wrapperGrid);
                }
            }

            private object? GetHostContent()
            {
                return _host switch
                {
                    Window window => window.Content,
                    ContentControl cc => cc.Content,
                    _ => null
                };
            }

            private void SetHostContent(object? content)
            {
                switch (_host)
                {
                    case Window window:
                        window.Content = content;
                        break;
                    case ContentControl cc:
                        cc.Content = content;
                        break;
                }
            }

            public void SetBusy(bool busy)
            {
                 Dispatcher.UIThread.InvokeAsync(() =>
                 {
                     EnsureOverlay();
                     _busyCount = Math.Max(0, busy ? _busyCount + 1 : _busyCount - 1);
                     if (_busyGrid != null)
                        _busyGrid.IsVisible = _busyCount > 0;
                 });
            }
            
            private async Task<T> ShowDialogOverlay<T>(Control dialog, Task<T> resultTask)
            {
                 await Dispatcher.UIThread.InvokeAsync(() =>
                 {
                     EnsureOverlay();
                     
                     // Wrap dialog in a generic dim-background container
                     var dialogContainer = new Grid
                     {
                         Background = new Avalonia.Media.SolidColorBrush(Media.Color.FromArgb(128, 0, 0, 0)) // Dim background
                     };
                     dialogContainer.Children.Add(dialog); // Dialog UserControl should be Centered by itself
                     
                     _dialogGrid?.Children.Add(dialogContainer);
                 });
                 
                 try 
                 {
                     return await resultTask;
                 }
                 finally
                 {
                     await Dispatcher.UIThread.InvokeAsync(() =>
                     {
                         if (_dialogGrid?.Children.Count > 0)
                             _dialogGrid.Children.RemoveAt(_dialogGrid.Children.Count - 1); // Remove top-most
                     });
                 }
            }

            public async void HandleAlert(AlertArguments arguments)
            {
                var tcs = new TaskCompletionSource<bool>();
                
                var dialog = new MauiAlertDialog(
                    arguments.Title ?? string.Empty,
                    arguments.Message ?? string.Empty,
                    arguments.Accept,
                    arguments.Cancel);
                
                dialog.OnResult += (res) => tcs.TrySetResult(res ?? false);
                
                var result = await ShowDialogOverlay(dialog, tcs.Task);
                arguments.SetResult(result);
            }

            public async void HandleActionSheet(ActionSheetArguments arguments)
            {
                 var tcs = new TaskCompletionSource<string?>();
                 
                 var buttons = arguments.Buttons?.ToArray();
                 var dialog = new MauiActionSheetDialog(
                            arguments.Title ?? string.Empty,
                            arguments.Cancel,
                            arguments.Destruction,
                            buttons);
                 
                 dialog.OnResult += (res) => tcs.TrySetResult(res ?? arguments.Cancel);

                 var result = await ShowDialogOverlay(dialog, tcs.Task);
                 arguments.SetResult(result);
            }

            public async void HandlePrompt(PromptArguments arguments)
            {
                 var tcs = new TaskCompletionSource<string?>();
                 
                 var dialog = new MauiPromptDialog(
                            arguments.Title ?? string.Empty,
                            arguments.Message ?? string.Empty,
                            arguments.Accept,
                            arguments.Cancel,
                            arguments.Placeholder,
                            arguments.MaxLength,
                            arguments.Keyboard,
                            arguments.InitialValue);
                 
                 dialog.OnResult += (res) => tcs.TrySetResult(res);

                 var result = await ShowDialogOverlay(dialog, tcs.Task);
                 arguments.SetResult(result);
            }
        }
    }
}