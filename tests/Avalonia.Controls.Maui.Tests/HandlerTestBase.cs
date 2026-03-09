using Avalonia.Headless;
using Avalonia.Controls.Maui.Services;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using MauiRect = Microsoft.Maui.Graphics.Rect;
using MauiSize = Microsoft.Maui.Graphics.Size;

namespace Avalonia.Controls.Maui.Tests;

public abstract class HandlerTestBase : IAsyncDisposable
{
    private MauiApp? _mauiApp;
    private IServiceProvider? _servicesProvider;
    private IMauiContext? _mauiContext;
    private bool _isCreated;
    private readonly List<IElementHandler> _handlers = new();

    protected void EnsureHandlerCreated(Action<MauiAppBuilder>? additionalCreationActions = null)
    {
        if (_isCreated)
        {
            return;
        }

        _isCreated = true;

        var appBuilder = MauiApp.CreateBuilder();

        appBuilder = ConfigureBuilder(appBuilder);
        additionalCreationActions?.Invoke(appBuilder);

        _mauiApp = appBuilder.Build();
        _servicesProvider = _mauiApp.Services;

        _mauiContext = new ContextStub(_servicesProvider);
    }

    protected virtual MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
    {
        return mauiAppBuilder.ConfigureTestBuilder();
    }

    protected IMauiContext MauiContext
    {
        get
        {
            EnsureHandlerCreated();
            return _mauiContext!;
        }
    }

    protected IServiceProvider ApplicationServices
    {
        get
        {
            EnsureHandlerCreated();
            return _servicesProvider!;
        }
    }

    protected THandler CreateHandler<THandler>(IElement view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler, new()
        => CreateHandler<THandler, THandler>(view, mauiContext);

    protected THandler CreateHandler<THandler, TReturnHandler>(IElement view, IMauiContext? mauiContext = null)
        where THandler : IElementHandler, TReturnHandler, new()
        where TReturnHandler : IElementHandler
    {
        mauiContext ??= MauiContext;

        var handler = new THandler();
        _handlers.Add(handler);
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

            view.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, w, h));
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
        return await Dispatcher.UIThread.InvokeAsync(func);
    }

    protected async Task InvokeOnMainThreadAsync(Action action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }

    public virtual async ValueTask DisposeAsync()
    {
        foreach (var handler in _handlers)
        {
            if (handler is IViewHandler vh && vh.PlatformView != null)
            {
                await InvokeOnMainThreadAsync(() =>
                {
                    handler.DisconnectHandler();
                });
            }
        }
        _handlers.Clear();

        _mauiApp?.Dispose();
    }

    // Simple context stub for testing
    private class ContextStub : IMauiContext
    {
        public ContextStub(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

#pragma warning disable CS8766
        public IMauiHandlersFactory? Handlers => Services.GetService<IMauiHandlersFactory>();
#pragma warning restore CS8766

        public Microsoft.Maui.Animations.IAnimationManager? AnimationManager =>
            Services.GetService<Microsoft.Maui.Animations.IAnimationManager>();
    }
}