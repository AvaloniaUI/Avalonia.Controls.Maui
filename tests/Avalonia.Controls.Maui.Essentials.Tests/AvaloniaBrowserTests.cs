using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.ApplicationModel;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaBrowserTests
{
    [Fact]
    public void Constructor_Does_Not_Throw()
    {
        var browser = new AvaloniaBrowser();
        Assert.NotNull(browser);
    }
}
