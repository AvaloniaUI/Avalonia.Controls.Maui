using Avalonia.Controls.Maui.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using MauiRect = Microsoft.Maui.Graphics.Rect;
using MauiWindow = Microsoft.Maui.Controls.Window;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;

namespace Avalonia.Controls.Maui.Tests;

/// <summary>
/// Test base class that places controls inside a MAUI Window with a ContentPage.
/// This ensures controls are properly realized in the visual tree and can be measured.
/// Uses the MauiAvaloniaApplication for services and handlers.
/// </summary>
public abstract class WindowHandlerTestBase : IAsyncDisposable
{
    private MauiWindow? _testWindow;
    private MauiContentPage? _testPage;

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

    /// <summary>
    /// Creates a handler for the given view, placing it inside a Window and ContentPage.
    /// </summary>
    protected THandler CreateHandler<THandler>(Microsoft.Maui.Controls.View view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler
    {
        mauiContext ??= MauiContext;

        // Create the window and page structure
        _testPage = new MauiContentPage
        {
            Content = view
        };

        _testWindow = new MauiWindow(_testPage)
        {
            Width = 800,
            Height = 600
        };

        // Create and connect the window handler first
        var handlerFactory = mauiContext.Services.GetRequiredService<IMauiHandlersFactory>();
        var windowHandler = handlerFactory.GetHandler(typeof(MauiWindow)) as IWindowHandler;

        if (windowHandler != null)
        {
            windowHandler.SetMauiContext(mauiContext);
            _testWindow.Handler = windowHandler;
            windowHandler.SetVirtualView(_testWindow);
        }

        // The view's handler should be created automatically through the visual tree
        // But let's make sure we have a handler
        if (view.Handler is not THandler handler)
        {
            // If the handler wasn't created through the visual tree, create it manually
            handler = (THandler)handlerFactory.GetHandler(view.GetType())!;
            InitializeViewHandler(view, handler, mauiContext);
        }
        else
        {
            handler = (THandler)view.Handler;
        }

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

    protected Task<THandler> CreateHandlerAsync<THandler>(Microsoft.Maui.Controls.View view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler =>
        InvokeOnMainThreadAsync(() => CreateHandler<THandler>(view, mauiContext));

    protected Task<TValue> GetValueAsync<TValue, THandler>(Microsoft.Maui.Controls.View view, Func<THandler, TValue> func)
        where THandler : IElementHandler
    {
        return InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<THandler>(view);
            return func(handler);
        });
    }

    protected async Task ValidatePropertyInitValue<TValue, THandler>(
        Microsoft.Maui.Controls.View view,
        Func<TValue> GetValue,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedValue)
        where THandler : IElementHandler
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
        Microsoft.Maui.Controls.View view,
        string property,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedSetValue,
        TValue expectedUnsetValue)
        where THandler : IElementHandler
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
        Microsoft.Maui.Controls.View view,
        Func<THandler, TValue> GetPropertyValue,
        string unrelatedPropertyName,
        Action SetUnrelatedProperty)
        where THandler : IElementHandler
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

    public virtual ValueTask DisposeAsync()
    {
        _testWindow?.Handler?.DisconnectHandler();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Generic test base class that places controls inside a MAUI Window.
/// Uses the MauiAvaloniaApplication for services and handlers.
/// </summary>
public abstract partial class WindowHandlerTestBase<THandler, TView> : WindowHandlerTestBase
    where THandler : IElementHandler
    where TView : Microsoft.Maui.Controls.View
{
    protected Task<THandler> CreateHandlerAsync(TView view)
    {
        return CreateHandlerAsync<THandler>(view);
    }

    protected Task<TValue> GetValueAsync<TValue>(TView view, Func<THandler, TValue> func)
    {
        return InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<THandler>(view);
            return func(handler);
        });
    }

    protected Task ValidatePropertyInitValue<TValue>(
        TView view,
        Func<TValue> GetValue,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedValue)
    {
        return ValidatePropertyInitValue<TValue, THandler>(view, GetValue, GetPlatformValue, expectedValue);
    }

    protected Task ValidatePropertyUpdatesValue<TValue>(
        TView view,
        string property,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedSetValue,
        TValue expectedUnsetValue)
    {
        return ValidatePropertyUpdatesValue<TValue, THandler>(view, property, GetPlatformValue, expectedSetValue, expectedUnsetValue);
    }

    protected Task ValidateUnrelatedPropertyUnaffected<TValue>(
        TView view,
        Func<THandler, TValue> GetPlatformValue,
        string property,
        Action SetUnrelatedProperty)
    {
        return ValidateUnrelatedPropertyUnaffected<TValue, THandler>(view, GetPlatformValue, property, SetUnrelatedProperty);
    }
}
