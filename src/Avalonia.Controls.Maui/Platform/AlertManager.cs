using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Platform;

internal class AlertManager
{
    readonly List<AlertRequestHelper> _subscriptions = new();

    internal void Subscribe(Microsoft.Maui.Controls.Window window)
    {
        var platformWindow = window.Handler?.PlatformView as Avalonia.Controls.Window;

        if (platformWindow is null)
            return;

        if (_subscriptions.Any(s => s.PlatformView == platformWindow))
            return;

        _subscriptions.Add(new AlertRequestHelper(window, platformWindow));
    }

    internal void Unsubscribe(Microsoft.Maui.Controls.Window window)
    {
        var platformWindow = window.Handler?.PlatformView as Avalonia.Controls.Window;

        var toRemove = platformWindow is null ?
            _subscriptions.Where(s => s.VirtualView == window).ToList() :
            _subscriptions.Where(s => s.PlatformView == platformWindow).ToList();

        foreach (var helper in toRemove)
        {
            helper.Dispose();
            _subscriptions.Remove(helper);
        }
    }

    internal sealed class AlertRequestHelper : IDisposable
    {
        Task<bool>? _currentAlert;

        internal AlertRequestHelper(Microsoft.Maui.Controls.Window virtualView, Avalonia.Controls.Window platformView)
        {
            VirtualView = virtualView;
            PlatformView = platformView;

#pragma warning disable CS0618 // MessagingCenter is obsolete
            MessagingCenter.Subscribe<Page, AlertArguments>(PlatformView, Page.AlertSignalName, OnAlertRequested);
#pragma warning restore CS0618
        }

        public Microsoft.Maui.Controls.Window VirtualView { get; }
        public Avalonia.Controls.Window PlatformView { get; }

        public void Dispose()
        {
#pragma warning disable CS0618 // MessagingCenter is obsolete
            MessagingCenter.Unsubscribe<Page, AlertArguments>(PlatformView, Page.AlertSignalName);
#pragma warning restore CS0618
        }

        async void OnAlertRequested(Page sender, AlertArguments arguments)
        {
            if (!PageIsInThisWindow(sender))
                return;

            // Wait for any current alert to finish
            var currentAlert = _currentAlert;
            while (currentAlert != null)
            {
                await currentAlert;
                currentAlert = _currentAlert;
            }

            _currentAlert = ShowAlert(arguments);
            arguments.SetResult(await _currentAlert.ConfigureAwait(false));
            _currentAlert = null;
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

        bool PageIsInThisWindow(Page page) =>
            page?.Window == VirtualView;
    }
}
