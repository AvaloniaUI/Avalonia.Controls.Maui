using Avalonia.Controls.Maui.Essentials;
using Avalonia.Headless.XUnit;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaMainThreadTests
{
    [AvaloniaFact]
    public void IsMainThread_Returns_True_On_UI_Thread()
    {
        Assert.True(AvaloniaMainThread.IsMainThread);
    }

    [AvaloniaFact]
    public async Task IsMainThread_Returns_False_On_Background_Thread()
    {
        var result = await Task.Run(() => AvaloniaMainThread.IsMainThread);
        Assert.False(result);
    }

    [AvaloniaFact]
    public void BeginInvokeOnMainThread_Executes_Immediately_On_UI_Thread()
    {
        var executed = false;
        AvaloniaMainThread.BeginInvokeOnMainThread(() => executed = true);
        Assert.True(executed);
    }

    [AvaloniaFact]
    public async Task BeginInvokeOnMainThread_Dispatches_From_Background_Thread()
    {
        var tcs = new TaskCompletionSource<bool>();

        await Task.Run(() =>
        {
            AvaloniaMainThread.BeginInvokeOnMainThread(() =>
            {
                tcs.TrySetResult(AvaloniaMainThread.IsMainThread);
            });
        });

        var wasOnMainThread = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(wasOnMainThread);
    }

    [AvaloniaFact]
    public async Task InvokeOnMainThreadAsync_Action_Executes_On_UI_Thread()
    {
        var wasOnMainThread = false;

        await AvaloniaMainThread.InvokeOnMainThreadAsync(() =>
        {
            wasOnMainThread = AvaloniaMainThread.IsMainThread;
        });

        Assert.True(wasOnMainThread);
    }

    [AvaloniaFact]
    public async Task InvokeOnMainThreadAsync_Func_Returns_Value()
    {
        var result = await AvaloniaMainThread.InvokeOnMainThreadAsync(() => 42);
        Assert.Equal(42, result);
    }

    [AvaloniaFact]
    public async Task InvokeOnMainThreadAsync_FuncTask_Executes_On_UI_Thread()
    {
        var wasOnMainThread = false;

        await AvaloniaMainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Task.Yield();
            wasOnMainThread = AvaloniaMainThread.IsMainThread;
        });

        Assert.True(wasOnMainThread);
    }

    [AvaloniaFact]
    public async Task InvokeOnMainThreadAsync_FuncTaskT_Returns_Value()
    {
        var result = await AvaloniaMainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Task.Yield();
            return "hello";
        });

        Assert.Equal("hello", result);
    }

    [AvaloniaFact]
    public async Task InvokeOnMainThreadAsync_From_Background_Thread()
    {
        var result = await Task.Run(async () =>
        {
            return await AvaloniaMainThread.InvokeOnMainThreadAsync(() =>
            {
                return AvaloniaMainThread.IsMainThread;
            });
        });

        Assert.True(result);
    }
}
