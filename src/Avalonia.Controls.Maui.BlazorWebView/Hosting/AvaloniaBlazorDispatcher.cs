using Microsoft.Maui.Dispatching;
using BlazorDispatcher = Microsoft.AspNetCore.Components.Dispatcher;

namespace Avalonia.Controls.Maui.BlazorWebView.Hosting;

internal sealed class AvaloniaBlazorDispatcher : BlazorDispatcher
{
    private readonly IDispatcher _dispatcher;

    public AvaloniaBlazorDispatcher(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override bool CheckAccess()
    {
        return !_dispatcher.IsDispatchRequired;
    }

    public override Task InvokeAsync(Action workItem)
    {
        return _dispatcher.DispatchAsync(workItem);
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        return _dispatcher.DispatchAsync(workItem);
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        return _dispatcher.DispatchAsync(workItem);
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        return _dispatcher.DispatchAsync(workItem);
    }
}
