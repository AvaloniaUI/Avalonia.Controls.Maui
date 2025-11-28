using Avalonia.Controls.Maui.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using MauiRect = Microsoft.Maui.Graphics.Rect;

namespace Avalonia.Controls.Maui.Tests;

/// <summary>
/// Base class for handler tests that uses the MauiAvaloniaApplication for services and handlers.
/// </summary>
public abstract class HandlerTestBase
{
    /// <summary>
    /// Gets the current test application.
    /// </summary>
    protected static App TestApp => (App)MauiAvaloniaApplication.Current;

    /// <summary>
    /// Gets the MAUI context from the application.
    /// </summary>
    protected IMauiContext MauiContext => TestApp.ApplicationContext
        ?? throw new InvalidOperationException("ApplicationContext is not initialized. Ensure the app has been started.");

    /// <summary>
    /// Gets the application services.
    /// </summary>
    protected IServiceProvider ApplicationServices => TestApp.Services;

    protected THandler CreateHandler<THandler>(IElement view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler, new()
        => CreateHandler<THandler, THandler>(view, mauiContext);

    protected THandler CreateHandler<THandler, TReturnHandler>(IElement view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler, TReturnHandler, new()
        where TReturnHandler : IElementHandler
    {
        mauiContext ??= MauiContext;

        var handler = new THandler();
        InitializeViewHandler(view, handler, mauiContext);

        return handler;
    }

    protected void InitializeViewHandler(IElement element, IElementHandler handler, IMauiContext? mauiContext = null)
    {
        mauiContext ??= MauiContext;
        handler.SetMauiContext(mauiContext);
        element.Handler = handler;
        handler.SetVirtualView(element);

        if (element is IView view && handler is IViewHandler viewHandler)
        {
            var size = view.Measure(view.Width, view.Height);
            var w = size.Width;
            var h = size.Height;

            view.Arrange(new MauiRect(0, 0, w, h));
        }
    }

    protected Task<THandler> CreateHandlerAsync<THandler>(IElement view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler, new() =>
        InvokeOnMainThreadAsync(() => CreateHandler<THandler>(view, mauiContext));

    protected Task<TValue> GetValueAsync<TValue, THandler>(IView view, Func<THandler, TValue> func)
        where THandler : IElementHandler, new()
    {
        return InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<THandler>(view);
            return func(handler);
        });
    }

    protected async Task ValidatePropertyInitValue<TValue, THandler>(
        IView view,
        Func<TValue> GetValue,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedValue)
        where THandler : IElementHandler, new()
    {
        var values = await GetValueAsync(view, (THandler handler) =>
        {
            return new
            {
                ViewValue = GetValue(),
                PlatformViewValue = GetPlatformValue(handler)
            };
        });

        Assert.Equal(expectedValue, values.ViewValue);
        Assert.Equal(expectedValue, values.PlatformViewValue);
    }

    protected async Task ValidatePropertyUpdatesValue<TValue, THandler>(
        IView view,
        string property,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedSetValue,
        TValue expectedUnsetValue)
        where THandler : IElementHandler, new()
    {
        var propInfo = view.GetType().GetProperty(property);

        // set initial values
        propInfo?.SetValue(view, expectedSetValue);

        var (handler, viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<THandler>(view);
            return (handler, (TValue?)propInfo?.GetValue(view), GetPlatformValue(handler));
        });

        Assert.Equal(expectedSetValue, viewVal);
        Assert.Equal(expectedSetValue, nativeVal);

        await ValidatePropertyUpdatesAfterInitValue(handler, property, GetPlatformValue, expectedSetValue, expectedUnsetValue);
    }

    protected async Task ValidatePropertyUpdatesAfterInitValue<TValue, THandler>(
        THandler handler,
        string property,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedSetValue,
        TValue expectedUnsetValue)
        where THandler : IElementHandler
    {
        var view = handler.VirtualView;
        var propInfo = handler.VirtualView?.GetType().GetProperty(property);

        // confirm can update
        var (viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
        {
            propInfo?.SetValue(view, expectedUnsetValue);
            handler.UpdateValue(property);

            return ((TValue?)propInfo?.GetValue(view), GetPlatformValue(handler));
        });

        Assert.Equal(expectedUnsetValue, viewVal);
        Assert.Equal(expectedUnsetValue, nativeVal);

        // confirm can revert
        (viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
        {
            propInfo?.SetValue(view, expectedSetValue);
            handler.UpdateValue(property);

            return ((TValue?)propInfo?.GetValue(view), GetPlatformValue(handler));
        });

        Assert.Equal(expectedSetValue, viewVal);
        Assert.Equal(expectedSetValue, nativeVal);
    }

    protected async Task ValidateUnrelatedPropertyUnaffected<TValue, THandler>(
        IView view,
        Func<THandler, TValue> GetPropertyValue,
        string unrelatedPropertyName,
        Action SetUnrelatedProperty)
        where THandler : IElementHandler, new()
    {
        var handler = await CreateHandlerAsync<THandler>(view);
        var initialValue = await InvokeOnMainThreadAsync(() => GetPropertyValue(handler));

        await InvokeOnMainThreadAsync(() =>
        {
            SetUnrelatedProperty();
            handler.UpdateValue(unrelatedPropertyName);
        });

        var valueAfter = await InvokeOnMainThreadAsync(() => GetPropertyValue(handler));
        Assert.Equal(initialValue, valueAfter);
    }

    protected async Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
    {
        return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(func);
    }

    protected async Task InvokeOnMainThreadAsync(Action action)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(action);
    }
}
