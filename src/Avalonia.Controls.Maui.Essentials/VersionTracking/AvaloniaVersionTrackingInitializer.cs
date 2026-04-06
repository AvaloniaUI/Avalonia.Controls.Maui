using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Essentials;

internal sealed class AvaloniaVersionTrackingInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        services.GetRequiredService<IVersionTracking>().Track();
    }
}
