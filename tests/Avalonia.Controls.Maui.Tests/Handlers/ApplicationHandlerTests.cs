using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using NSubstitute;
using MauiApplicationHandler = Avalonia.Controls.Maui.Handlers.ApplicationHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ApplicationHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Default Constructor Creates Handler")]
    public async Task DefaultConstructorCreatesHandler()
    {
        var handler = new MauiApplicationHandler();

        Assert.NotNull(handler);
    }

    [AvaloniaFact(DisplayName = "Constructor With Mapper Creates Handler")]
    public async Task ConstructorWithMapperCreatesHandler()
    {
        var customMapper = new PropertyMapper<IApplication, MauiApplicationHandler>();
        var handler = new MauiApplicationHandler(customMapper);

        Assert.NotNull(handler);
    }

    [AvaloniaFact(DisplayName = "Constructor With Null Mapper Uses Default")]
    public async Task ConstructorWithNullMapperUsesDefault()
    {
        var handler = new MauiApplicationHandler(null);

        Assert.NotNull(handler);
    }

    [AvaloniaFact(DisplayName = "Constructor With Mapper And CommandMapper Creates Handler")]
    public async Task ConstructorWithMapperAndCommandMapperCreatesHandler()
    {
        var customMapper = new PropertyMapper<IApplication, MauiApplicationHandler>();
        var customCommandMapper = new CommandMapper<IApplication, MauiApplicationHandler>();
        var handler = new MauiApplicationHandler(customMapper, customCommandMapper);

        Assert.NotNull(handler);
    }

    [AvaloniaFact(DisplayName = "Constructor With Null Mapper And CommandMapper Uses Defaults")]
    public async Task ConstructorWithNullMapperAndCommandMapperUsesDefaults()
    {
        var handler = new MauiApplicationHandler(null, null);

        Assert.NotNull(handler);
    }

    [AvaloniaFact(DisplayName = "Handler Has Terminate Command Key")]
    public void HandlerHasTerminateCommandKey()
    {
        Assert.Equal("Terminate", MauiApplicationHandler.TerminateCommandKey);
    }

    [AvaloniaFact(DisplayName = "Static Mapper Is Not Null")]
    public void StaticMapperIsNotNull()
    {
        Assert.NotNull(MauiApplicationHandler.Mapper);
    }

    [AvaloniaFact(DisplayName = "Static CommandMapper Is Not Null")]
    public void StaticCommandMapperIsNotNull()
    {
        Assert.NotNull(MauiApplicationHandler.CommandMapper);
    }

    [AvaloniaFact(DisplayName = "CreatePlatformElement Returns Application From Services")]
    public async Task CreatePlatformElementReturnsApplicationFromServices()
    {
        var application = new ApplicationStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<App>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Handler Sets Virtual View Correctly")]
    public async Task HandlerSetsVirtualViewCorrectly()
    {
        var application = new ApplicationStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        Assert.Same(application, handler.VirtualView);
    }

    [AvaloniaFact(DisplayName = "MapCloseWindow Does Nothing When Args Is Not IWindow")]
    public async Task MapCloseWindowDoesNothingWhenArgsIsNotIWindow()
    {
        var application = new ApplicationStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        // Should not throw when args is not IWindow
        await InvokeOnMainThreadAsync(() =>
        {
            MauiApplicationHandler.MapCloseWindow(handler, application, "not a window");
        });
    }

    [AvaloniaFact(DisplayName = "MapCloseWindow Does Nothing When Args Is Null")]
    public async Task MapCloseWindowDoesNothingWhenArgsIsNull()
    {
        var application = new ApplicationStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        // Should not throw when args is null
        await InvokeOnMainThreadAsync(() =>
        {
            MauiApplicationHandler.MapCloseWindow(handler, application, null);
        });
    }

    [AvaloniaFact(DisplayName = "MapActivateWindow Does Nothing When Args Is Not IWindow")]
    public async Task MapActivateWindowDoesNothingWhenArgsIsNotIWindow()
    {
        var application = new ApplicationStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        // Should not throw when args is not IWindow
        await InvokeOnMainThreadAsync(() =>
        {
            MauiApplicationHandler.MapActivateWindow(handler, application, "not a window");
        });
    }

    [AvaloniaFact(DisplayName = "MapActivateWindow Does Nothing When Args Is Null")]
    public async Task MapActivateWindowDoesNothingWhenArgsIsNull()
    {
        var application = new ApplicationStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        // Should not throw when args is null
        await InvokeOnMainThreadAsync(() =>
        {
            MauiApplicationHandler.MapActivateWindow(handler, application, null);
        });
    }

    [AvaloniaFact(DisplayName = "MapOpenWindow Does Not Throw")]
    public async Task MapOpenWindowDoesNotThrow()
    {
        var application = new ApplicationStub();
        var windowStub = new WindowStub();

        EnsureHandlerCreated(builder =>
        {
            builder.Services.AddSingleton<Application>(App.Current!);
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IApplication, MauiApplicationHandler>();
            });
        });

        var handler = await CreateHandlerAsync<MauiApplicationHandler>(application);

        await InvokeOnMainThreadAsync(() =>
        {
            MauiApplicationHandler.MapOpenWindow(handler, application, windowStub);
        });
    }
}
