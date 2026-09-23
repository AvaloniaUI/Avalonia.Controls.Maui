using Avalonia.Controls.Maui.BlazorWebView.Hosting;

namespace Avalonia.Controls.Maui.BlazorWebView.Tests.Hosting;

public class BlazorWebViewLoopbackHostTests
{
    [Fact]
    public async Task DisposeAsync_DoesNotWaitIndefinitelyForActiveRequests()
    {
        var host = BlazorWebViewLoopbackHost.Create(TimeSpan.FromMilliseconds(10));
        using var client = new HttpClient();
        var disposed = false;

        var requestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        try
        {
            host.Start(async context =>
            {
                requestStarted.SetResult();
                await releaseRequest.Task;
                context.Response.StatusCode = 204;
                context.Response.Close();
            });

            var requestTask = client.GetAsync(host.BaseUri);

            await requestStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await host.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            disposed = true;

            releaseRequest.SetResult();

            try
            {
                await requestTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // The listener is closed during disposal, so the pending client request
                // may complete as a transport failure depending on platform timing.
            }
        }
        finally
        {
            releaseRequest.TrySetResult();

            if (!disposed)
                await host.DisposeAsync();
        }
    }
}
