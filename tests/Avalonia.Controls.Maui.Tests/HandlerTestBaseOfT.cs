using Microsoft.Maui;
using System;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Tests;

public abstract partial class HandlerTestBase<THandler, TStub> : HandlerTestBase
    where THandler : IElementHandler, new()
    where TStub : IView
{
    protected Task<THandler> CreateHandlerAsync(TStub view)
    {
        return CreateHandlerAsync<THandler>(view);
    }

    protected Task<TValue> GetValueAsync<TValue>(TStub view, Func<THandler, TValue> func)
    {
        return InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<THandler>(view);
            return func(handler);
        });
    }

    protected Task ValidatePropertyInitValue<TValue>(
        TStub view,
        Func<TValue> GetValue,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedValue)
    {
        return ValidatePropertyInitValue<TValue, THandler>(view, GetValue, GetPlatformValue, expectedValue);
    }

    protected Task ValidatePropertyUpdatesValue<TValue>(
        TStub view,
        string property,
        Func<THandler, TValue> GetPlatformValue,
        TValue expectedSetValue,
        TValue expectedUnsetValue)
    {
        return ValidatePropertyUpdatesValue<TValue, THandler>(view, property, GetPlatformValue, expectedSetValue, expectedUnsetValue);
    }

    protected Task ValidateUnrelatedPropertyUnaffected<TValue>(
        TStub view,
        Func<THandler, TValue> GetPlatformValue,
        string property,
        Action SetUnrelatedProperty)
    {
        return ValidateUnrelatedPropertyUnaffected<TValue, THandler>(view, GetPlatformValue, property, SetUnrelatedProperty);
    }
}
