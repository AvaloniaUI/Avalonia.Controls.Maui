using Avalonia.Controls;
using Avalonia.Controls.Maui.Essentials;
using NSubstitute;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaFilePickerTests
{
    [Fact]
    public async Task PickAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var picker = new AvaloniaFilePicker(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => picker.PickAsync(null));
    }

    [Fact]
    public async Task PickMultipleAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var picker = new AvaloniaFilePicker(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => picker.PickMultipleAsync(null));
    }
}
