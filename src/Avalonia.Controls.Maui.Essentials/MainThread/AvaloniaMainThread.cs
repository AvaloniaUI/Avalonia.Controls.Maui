using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of main thread dispatching, providing the same API as
/// <c>Microsoft.Maui.ApplicationModel.MainThread</c> but backed by Avalonia's <see cref="Dispatcher.UIThread"/>.
/// </summary>
/// <remarks>
/// MAUI's static <c>MainThread</c> class cannot be extended on custom backends because it uses
/// private platform-specific methods with no extensibility points. Use this class instead
/// for reliable main thread dispatching on Linux and WebAssembly.
/// </remarks>
public static class AvaloniaMainThread
{
    /// <summary>
    /// Gets a value indicating whether the current thread is the UI thread.
    /// </summary>
    public static bool IsMainThread => Dispatcher.UIThread.CheckAccess();

    /// <summary>
    /// Invokes an action on the main UI thread. If already on the main thread, the action is executed immediately.
    /// </summary>
    /// <param name="action">The action to invoke on the main thread.</param>
    public static void BeginInvokeOnMainThread(Action action)
    {
        if (IsMainThread)
            action();
        else
            Dispatcher.UIThread.Post(action);
    }

    /// <summary>
    /// Invokes an action on the main UI thread asynchronously.
    /// </summary>
    /// <param name="action">The action to invoke on the main thread.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InvokeOnMainThreadAsync(Action action)
    {
        if (IsMainThread)
        {
            action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }

    /// <summary>
    /// Invokes a function on the main UI thread asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to invoke on the main thread.</param>
    /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation with the result.</returns>
    public static async Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
    {
        if (IsMainThread)
            return func();

        return await Dispatcher.UIThread.InvokeAsync(func);
    }

    /// <summary>
    /// Invokes an async function on the main UI thread asynchronously.
    /// </summary>
    /// <param name="funcTask">The async function to invoke on the main thread.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InvokeOnMainThreadAsync(Func<Task> funcTask)
    {
        if (IsMainThread)
        {
            await funcTask();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(funcTask);
    }

    /// <summary>
    /// Invokes an async function on the main UI thread asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="funcTask">The async function to invoke on the main thread.</param>
    /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation with the result.</returns>
    public static async Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
    {
        if (IsMainThread)
            return await funcTask();

        return await Dispatcher.UIThread.InvokeAsync(funcTask);
    }
}
