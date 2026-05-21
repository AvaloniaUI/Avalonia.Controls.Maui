using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaImage = Avalonia.Controls.Image;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
using AvaloniaStackPanel = Avalonia.Controls.StackPanel;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;
using MauiTabbedPage = Microsoft.Maui.Controls.TabbedPage;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class TabbedPageHandlerTests : HandlerTestBase<TabbedPageHandler, TabbedPageStub>
{
    [AvaloniaFact(DisplayName = "Handler Creates AvaloniaTabbedPage Platform View")]
    public async Task Handler_Creates_AvaloniaTabbedPage_Platform_View()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.NotNull(handler.PlatformView);
            Assert.IsType<AvaloniaTabbedPage>(handler.PlatformView);
        });
    }

    [AvaloniaFact(DisplayName = "MapBarBackgroundColor Stores Color As Resource")]
    public async Task MapBarBackgroundColor_Stores_Color_As_Resource()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarBackgroundColor = Microsoft.Maui.Graphics.Colors.Red
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabStripBackground"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarBackgroundColor Clears Resource When Null")]
    public async Task MapBarBackgroundColor_Clears_Resource_When_Null()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarBackgroundColor = Microsoft.Maui.Graphics.Colors.Blue
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabStripBackground"));

            stub.BarBackgroundColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabbedPageTabStripBackground"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarTextColor Stores Color As Resource")]
    public async Task MapBarTextColor_Stores_Color_As_Resource()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarTextColor = Microsoft.Maui.Graphics.Colors.White
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarTextColor));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarTextColor Clears Resource When Null")]
    public async Task MapBarTextColor_Clears_Resource_When_Null()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarTextColor = Microsoft.Maui.Graphics.Colors.White
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarTextColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));

            stub.BarTextColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.BarTextColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarBackground Stores Brush As Resource")]
    public async Task MapBarBackground_Stores_Brush_As_Resource()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarBackground = new LinearGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Microsoft.Maui.Graphics.Colors.Blue, 0f),
                        new GradientStop(Microsoft.Maui.Graphics.Colors.Green, 1f)
                    }
                }
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackground));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabStripBackground"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarBackground Takes Precedence Over BarBackgroundColor")]
    public async Task MapBarBackground_Takes_Precedence_Over_BarBackgroundColor()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarBackgroundColor = Microsoft.Maui.Graphics.Colors.Red,
                BarBackground = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Blue)
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackground));
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabStripBackground"));
        });
    }

    [AvaloniaFact(DisplayName = "MapSelectedTabColor Stores Color In Resources")]
    public async Task MapSelectedTabColor_Stores_Color_In_Resources()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                SelectedTabColor = Microsoft.Maui.Graphics.Colors.Orange
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapSelectedTabColor Removes Resource When Null")]
    public async Task MapSelectedTabColor_Removes_Resource_When_Null()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                SelectedTabColor = Microsoft.Maui.Graphics.Colors.Orange
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));

            stub.SelectedTabColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapUnselectedTabColor Stores Color In Resources")]
    public async Task MapUnselectedTabColor_Stores_Color_In_Resources()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                UnselectedTabColor = Microsoft.Maui.Graphics.Colors.Gray
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });
            stub.Children.Add(new MauiContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundUnselected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapUnselectedTabColor Removes Resource When Null")]
    public async Task MapUnselectedTabColor_Removes_Resource_When_Null()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                UnselectedTabColor = Microsoft.Maui.Graphics.Colors.Gray
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundUnselected"));

            stub.UnselectedTabColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundUnselected"));
        });
    }

    [AvaloniaFact(DisplayName = "Children Collection Creates Pages")]
    public async Task Children_Collection_Creates_Pages()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });
            stub.Children.Add(new MauiContentPage { Title = "Tab 2" });
            stub.Children.Add(new MauiContentPage { Title = "Tab 3" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal(3, stub.Children.Count);
            Assert.Equal("Tab 1", stub.Children[0].Title);
            Assert.Equal("Tab 2", stub.Children[1].Title);
            Assert.Equal("Tab 3", stub.Children[2].Title);
        });
    }

    [AvaloniaFact(DisplayName = "Child IconImageSource Creates Icon Header")]
    public async Task Child_IconImageSource_Creates_Icon_Header()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new MauiContentPage
            {
                Title = "Home",
                IconImageSource = "missing.png"
            });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            var header = GetTabHeader(handler, 0);

            Assert.IsType<AvaloniaStackPanel>(header);
            Assert.Equal("Home", GetHeaderText(header));
        });
    }

    [AvaloniaFact(DisplayName = "Child Without IconImageSource Creates Text Header")]
    public async Task Child_Without_IconImageSource_Creates_Text_Header()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new MauiContentPage { Title = "Plain" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal("Plain", GetTabHeader(handler, 0));
        });
    }

    [AvaloniaFact(DisplayName = "Child Title Change Updates Tab Header")]
    public async Task Child_Title_Change_Updates_Tab_Header()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var page = new MauiContentPage { Title = "Before" };
            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var handler = CreateHandler<TabbedPageHandler>(stub);

            page.Title = "After";

            Assert.Equal("After", GetTabHeader(handler, 0));
        });
    }

    [AvaloniaFact(DisplayName = "Child Title Change Preserves Icon Header")]
    public async Task Child_Title_Change_Preserves_Icon_Header()
    {
        EnsureHandlerCreated();

        var iconPath = await InvokeOnMainThreadAsync(CreateTempIconFile);

        try
        {
            var page = new MauiContentPage
            {
                Title = "Before",
                IconImageSource = ImageSource.FromFile(iconPath),
                Content = new Microsoft.Maui.Controls.Label { Text = "Home content" }
            };

            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var handler = await CreateHandlerAsync<TabbedPageHandler>(stub);
            Assert.NotNull(await WaitForHeaderIconAsync(handler, 0));

            await InvokeOnMainThreadAsync(() =>
            {
                page.Title = "After";
                Assert.NotNull(page.IconImageSource);
            });

            Assert.NotNull(await WaitForHeaderIconAsync(handler, 0));

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Equal("After", GetHeaderText(GetTabHeader(handler, 0)));
            });
        }
        finally
        {
            if (File.Exists(iconPath))
                File.Delete(iconPath);
        }
    }

    [AvaloniaFact(DisplayName = "Child IconImageSource Change Updates Tab Header")]
    public async Task Child_IconImageSource_Change_Updates_Tab_Header()
    {
        EnsureHandlerCreated();

        MauiContentPage page = null!;
        TabbedPageHandler handler = null!;

        await InvokeOnMainThreadAsync(() =>
        {
            page = new MauiContentPage { Title = "Dynamic" };
            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            handler = CreateHandler<TabbedPageHandler>(stub);
            Assert.Equal("Dynamic", GetTabHeader(handler, 0));
        });

        await InvokeOnMainThreadAsync(() =>
        {
            page.IconImageSource = "missing.png";
        });

        var iconHeader = await WaitForHeaderPanelAsync(handler, 0);
        Assert.NotNull(iconHeader);
        Assert.Equal("Dynamic", GetHeaderText(iconHeader));

        await InvokeOnMainThreadAsync(() =>
        {
            page.IconImageSource = null;
        });

        Assert.Equal("Dynamic", await WaitForHeaderStringAsync(handler, 0));
    }

    [AvaloniaFact(DisplayName = "Child IconImageSource Change Preserves Platform Pages")]
    public async Task Child_IconImageSource_Change_Preserves_Platform_Pages()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var page = new MauiContentPage { Title = "Dynamic" };
            var stub = new TabbedPageStub();
            stub.Children.Add(page);
            stub.Children.Add(new MauiContentPage { Title = "Second" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            var firstPlatformPage = handler.PlatformView.Pages!.ElementAt(0);
            var secondPlatformPage = handler.PlatformView.Pages!.ElementAt(1);

            page.IconImageSource = "missing.png";

            Assert.Same(firstPlatformPage, handler.PlatformView.Pages!.ElementAt(0));
            Assert.Same(secondPlatformPage, handler.PlatformView.Pages!.ElementAt(1));
        });
    }

    [AvaloniaFact(DisplayName = "Child Title Change Does Not Restore Cleared Icon")]
    public async Task Child_Title_Change_Does_Not_Restore_Cleared_Icon()
    {
        EnsureHandlerCreated();

        var iconPath = await InvokeOnMainThreadAsync(CreateTempIconFile);

        try
        {
            var page = new MauiContentPage
            {
                Title = "Before",
                IconImageSource = ImageSource.FromFile(iconPath),
                Content = new Microsoft.Maui.Controls.Label { Text = "Home content" }
            };

            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var handler = await CreateHandlerAsync<TabbedPageHandler>(stub);
            Assert.NotNull(await WaitForHeaderIconAsync(handler, 0));

            await InvokeOnMainThreadAsync(() =>
            {
                page.Title = "After";
                page.IconImageSource = null;
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            });

            Assert.Equal("After", await WaitForHeaderStringAsync(handler, 0));
        }
        finally
        {
            if (File.Exists(iconPath))
                File.Delete(iconPath);
        }
    }

    [AvaloniaFact(DisplayName = "Child Title Change Does Not Restore Stale Header After Pages Changed")]
    public async Task Child_Title_Change_Does_Not_Restore_Stale_Header_After_Pages_Changed()
    {
        EnsureHandlerCreated();

        var iconPath = await InvokeOnMainThreadAsync(CreateTempIconFile);

        try
        {
            var page = new MauiContentPage
            {
                Title = "Before",
                IconImageSource = ImageSource.FromFile(iconPath),
                Content = new Microsoft.Maui.Controls.Label { Text = "Home content" }
            };

            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var handler = await CreateHandlerAsync<TabbedPageHandler>(stub);
            Assert.NotNull(await WaitForHeaderIconAsync(handler, 0));

            await InvokeOnMainThreadAsync(() =>
            {
                page.Title = "After";
                stub.Children.Add(new MauiContentPage { Title = "Second" });
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            });

            Assert.NotNull(await WaitForHeaderIconAsync(handler, 0));

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Equal("After", GetHeaderText(GetTabHeader(handler, 0)));
            });
        }
        finally
        {
            if (File.Exists(iconPath))
                File.Delete(iconPath);
        }
    }

    [AvaloniaFact(DisplayName = "ItemsSource ItemTemplate Uses Child IconImageSource")]
    public async Task ItemsSource_ItemTemplate_Uses_Child_IconImageSource()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                ItemsSource = new[]
                {
                    new TabInfoWithIcon { Title = "Inbox", Icon = ImageSource.FromFile("missing.png") }
                },
                ItemTemplate = new DataTemplate(() =>
                {
                    var page = new MauiContentPage();
                    page.SetBinding(MauiContentPage.TitleProperty, nameof(TabInfoWithIcon.Title));
                    page.SetBinding(MauiContentPage.IconImageSourceProperty, nameof(TabInfoWithIcon.Icon));
                    return page;
                })
            };

            var handler = CreateHandler<TabbedPageHandler>(stub);
            var header = GetTabHeader(handler, 0);

            Assert.IsType<AvaloniaStackPanel>(header);
            Assert.Equal("Inbox", GetHeaderText(header));
        });
    }

    [AvaloniaFact(DisplayName = "ItemsSource Set After Handler Tracks Child Page Changes")]
    public async Task ItemsSource_Set_After_Handler_Tracks_Child_Page_Changes()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var page = new MauiContentPage();
                    page.SetBinding(MauiContentPage.TitleProperty, nameof(TabInfoWithIcon.Title));
                    page.SetBinding(MauiContentPage.IconImageSourceProperty, nameof(TabInfoWithIcon.Icon));
                    return page;
                })
            };

            var handler = CreateHandler<TabbedPageHandler>(stub);

            stub.ItemsSource = new[]
            {
                new TabInfoWithIcon { Title = "Inbox", Icon = ImageSource.FromFile("missing.png") }
            };
            handler.UpdateValue(nameof(MauiTabbedPage.ItemsSource));

            var childPage = Assert.Single(stub.Children);
            childPage.Title = "Archive";

            Assert.Equal("Archive", GetHeaderText(GetTabHeader(handler, 0)));
        });
    }

    [AvaloniaFact(DisplayName = "Child IconImageSource Loads Into Rendered Tab Header")]
    public async Task Child_IconImageSource_Loads_Into_Rendered_Tab_Header()
    {
        EnsureHandlerCreated();

        var iconPath = await InvokeOnMainThreadAsync(CreateTempIconFile);

        try
        {
            var page = new MauiContentPage
            {
                Title = "Home",
                IconImageSource = ImageSource.FromFile(iconPath),
                Content = new Microsoft.Maui.Controls.Label { Text = "Home content" }
            };

            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var handler = await CreateHandlerAsync<TabbedPageHandler>(stub);
            var icon = await WaitForHeaderIconAsync(handler, 0);

            Assert.NotNull(icon);

            await InvokeOnMainThreadAsync(() =>
            {
                var window = new AvaloniaWindow
                {
                    Content = handler.PlatformView,
                    Width = 320,
                    Height = 240
                };
                try
                {
                    window.Show();
                    Avalonia.Threading.Dispatcher.UIThread.RunJobs();

                    var bitmap = new RenderTargetBitmap(
                        new Avalonia.PixelSize(320, 240),
                        new Avalonia.Vector(96, 96));
                    bitmap.Render(handler.PlatformView);

                    Assert.Equal(new Avalonia.PixelSize(320, 240), bitmap.PixelSize);
                    Assert.NotNull(icon.Source);
                    Assert.True(icon.Bounds.Width > 0 || icon.DesiredSize.Width > 0);
                }
                finally
                {
                    window.Close();
                }
            });
        }
        finally
        {
            if (File.Exists(iconPath))
                File.Delete(iconPath);
        }
    }

    [AvaloniaFact(DisplayName = "Child Uri IconImageSource Loads Into Tab Header")]
    public async Task Child_Uri_IconImageSource_Loads_Into_Tab_Header()
    {
        EnsureHandlerCreated();

        var iconPath = await InvokeOnMainThreadAsync(CreateTempIconFile);

        try
        {
            var page = new MauiContentPage
            {
                Title = "Uri",
                IconImageSource = ImageSource.FromUri(new Uri(iconPath)),
                Content = new Microsoft.Maui.Controls.Label { Text = "URI icon content" }
            };

            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var handler = await CreateHandlerAsync<TabbedPageHandler>(stub);

            Assert.NotNull(await WaitForHeaderIconAsync(handler, 0));
        }
        finally
        {
            if (File.Exists(iconPath))
                File.Delete(iconPath);
        }
    }

    [AvaloniaFact(DisplayName = "Disconnect Invalidates Pending Tab Icon Load")]
    public async Task Disconnect_Invalidates_Pending_Tab_Icon_Load()
    {
        var imageService = new DelayedStreamImageSourceService();
        EnsureHandlerCreated();

        AvaloniaTabbedPage platformView = null!;
        AvaloniaStackPanel headerPanel = null!;

        await InvokeOnMainThreadAsync(() =>
        {
            var page = new MauiContentPage
            {
                Title = "Pending",
                IconImageSource = ImageSource.FromStream(() => new MemoryStream([1, 2, 3])),
                Content = new Microsoft.Maui.Controls.Label { Text = "Pending icon content" }
            };

            var stub = new TabbedPageStub();
            stub.Children.Add(page);

            var mauiContext = new ImageSourceServiceMauiContext(ApplicationServices, imageService);
            var handler = CreateHandler<TabbedPageHandler>(stub, mauiContext);
            platformView = handler.PlatformView;
            headerPanel = Assert.IsType<AvaloniaStackPanel>(GetTabHeader(handler, 0));

            Assert.Empty(headerPanel.Children.OfType<AvaloniaImage>());

            ((IElementHandler)handler).DisconnectHandler();

            Assert.Empty(platformView.Pages!);
        });

        imageService.Complete();
        var resultDisposed = false;
        for (var i = 0; i < 40; i++)
        {
            await InvokeOnMainThreadAsync(() => Avalonia.Threading.Dispatcher.UIThread.RunJobs());
            if (imageService.ResultDisposed)
            {
                resultDisposed = true;
                break;
            }

            await Task.Delay(25);
        }

        Assert.True(resultDisposed);

        await InvokeOnMainThreadAsync(() =>
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Empty(platformView.Pages!);
            Assert.Empty(headerPanel.Children.OfType<AvaloniaImage>());
        });
    }

    [AvaloniaFact(DisplayName = "MapCurrentPage Updates Selected Index")]
    public async Task MapCurrentPage_Updates_Selected_Index()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var page1 = new MauiContentPage { Title = "Tab 1" };
            var page2 = new MauiContentPage { Title = "Tab 2" };
            var page3 = new MauiContentPage { Title = "Tab 3" };

            var stub = new TabbedPageStub();
            stub.Children.Add(page1);
            stub.Children.Add(page2);
            stub.Children.Add(page3);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            Assert.Equal(0, handler.PlatformView.SelectedIndex);

            stub.CurrentPage = page3;
            handler.UpdateValue(nameof(MauiTabbedPage.CurrentPage));

            Assert.Equal(2, handler.PlatformView.SelectedIndex);
        });
    }

    [AvaloniaFact(DisplayName = "Empty TabbedPage Has No Children")]
    public async Task Empty_TabbedPage_Has_No_Children()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Empty(stub.Children);
        });
    }

    [AvaloniaFact(DisplayName = "Empty TabbedPage Has Negative One Selected Index")]
    public async Task Empty_TabbedPage_Has_No_Selected_Index()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal(-1, handler.PlatformView.SelectedIndex);
        });
    }

    [AvaloniaFact(DisplayName = "Multiple Color Properties Can Be Set Together")]
    public async Task Multiple_Color_Properties_Can_Be_Set_Together()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarBackgroundColor = Microsoft.Maui.Graphics.Colors.DarkBlue,
                BarTextColor = Microsoft.Maui.Graphics.Colors.White,
                SelectedTabColor = Microsoft.Maui.Graphics.Colors.Yellow,
                UnselectedTabColor = Microsoft.Maui.Graphics.Colors.LightGray
            };
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });
            stub.Children.Add(new MauiContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));
            handler.UpdateValue(nameof(MauiTabbedPage.BarTextColor));
            handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));
            handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabStripBackground"));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundSelected"));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageTabItemHeaderForegroundUnselected"));
        });
    }

    [AvaloniaFact(DisplayName = "Handler Can Be Created Multiple Times")]
    public async Task Handler_Can_Be_Created_Multiple_Times()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            for (int i = 0; i < 3; i++)
            {
                var stub = new TabbedPageStub();
                stub.Children.Add(new MauiContentPage { Title = $"Tab {i}" });
                var handler = CreateHandler<TabbedPageHandler>(stub);

                Assert.NotNull(handler.PlatformView);
                Assert.Single(stub.Children);
            }
        });
    }

    [AvaloniaFact(DisplayName = "Adding Child Updates Children Count")]
    public async Task Adding_Child_Updates_Children_Count()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new MauiContentPage { Title = "Tab 1" });
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Single(stub.Children);

            stub.Children.Add(new MauiContentPage { Title = "Tab 2" });

            Assert.Equal(2, stub.Children.Count);
        });
    }

    [AvaloniaFact(DisplayName = "Removing Child Updates Children Count")]
    public async Task Removing_Child_Updates_Children_Count()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var page1 = new MauiContentPage { Title = "Tab 1" };
            var page2 = new MauiContentPage { Title = "Tab 2" };
            stub.Children.Add(page1);
            stub.Children.Add(page2);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            Assert.Equal(2, stub.Children.Count);

            stub.Children.Remove(page1);

            Assert.Single(stub.Children);
            Assert.Equal("Tab 2", stub.Children[0].Title);
        });
    }

    [AvaloniaFact(DisplayName = "ItemsSource Populates Children")]
    public async Task ItemsSource_Populates_Children()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var items = new List<string> { "Item 1", "Item 2", "Item 3" };

            stub.ItemsSource = items;
            stub.ItemTemplate = new DataTemplate(() =>
            {
                var page = new MauiContentPage();
                page.SetBinding(MauiContentPage.TitleProperty, ".");
                return page;
            });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal(3, stub.Children.Count);

            stub.SelectedItem = "Item 2";
            handler.UpdateValue(nameof(MauiTabbedPage.SelectedItem));

            Assert.Equal(1, handler.PlatformView.SelectedIndex);
        });
    }

    private static object? GetTabHeader(TabbedPageHandler handler, int index)
    {
        var pages = handler.PlatformView.Pages ?? throw new InvalidOperationException("TabbedPage pages are not initialized.");
        return pages.ElementAt(index).Header;
    }

    private static string? GetHeaderText(object? header)
    {
        return header switch
        {
            string text => text,
            AvaloniaStackPanel panel => panel.Children
                .OfType<AvaloniaTextBlock>()
                .FirstOrDefault()
                ?.Text,
            _ => null
        };
    }

    private async Task<AvaloniaStackPanel?> WaitForHeaderPanelAsync(TabbedPageHandler handler, int index)
    {
        for (var i = 0; i < 40; i++)
        {
            var header = await InvokeOnMainThreadAsync(() => GetTabHeader(handler, index));
            if (header is AvaloniaStackPanel panel)
                return panel;

            await Task.Delay(25);
        }

        return null;
    }

    private async Task<string?> WaitForHeaderStringAsync(TabbedPageHandler handler, int index)
    {
        for (var i = 0; i < 40; i++)
        {
            var header = await InvokeOnMainThreadAsync(() => GetTabHeader(handler, index));
            if (header is string text)
                return text;

            await Task.Delay(25);
        }

        return null;
    }

    private async Task<AvaloniaImage?> WaitForHeaderIconAsync(TabbedPageHandler handler, int index)
    {
        for (var i = 0; i < 40; i++)
        {
            var icon = await InvokeOnMainThreadAsync(() =>
            {
                return GetTabHeader(handler, index) is AvaloniaStackPanel panel
                    ? panel.Children.OfType<AvaloniaImage>().FirstOrDefault(x => x.Source is not null)
                    : null;
            });

            if (icon is not null)
                return icon;

            await Task.Delay(25);
        }

        return null;
    }

    private static string CreateTempIconFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Avalonia.Controls.Maui.Tests");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"tabbed-page-icon-{Guid.NewGuid():N}.png");
        var bitmap = new RenderTargetBitmap(
            new Avalonia.PixelSize(16, 16),
            new Avalonia.Vector(96, 96));

        using (var context = bitmap.CreateDrawingContext())
        {
            context.FillRectangle(
                Avalonia.Media.Brushes.Red,
                new Avalonia.Rect(0, 0, 16, 16));
        }

        bitmap.Save(path);
        return path;
    }

    private sealed class DelayedStreamImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IStreamImageSource>
    {
        private readonly TaskCompletionSource<IImageSourceServiceResult<Bitmap>?> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private Avalonia.Controls.Maui.Services.ImageSourceServiceResult? _result;

        public bool ResultDisposed => _result?.IsDisposed == true;

        public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
            IImageSource imageSource,
            float scale = 1,
            CancellationToken cancellationToken = default)
        {
            return _completion.Task;
        }

        public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
            IStreamImageSource imageSource,
            float scale = 1,
            CancellationToken cancellationToken = default)
        {
            return _completion.Task;
        }

        public void Complete()
        {
            var bitmap = new RenderTargetBitmap(
                new Avalonia.PixelSize(16, 16),
                new Avalonia.Vector(96, 96));

            using (var context = bitmap.CreateDrawingContext())
            {
                context.FillRectangle(
                    Avalonia.Media.Brushes.Blue,
                    new Avalonia.Rect(0, 0, 16, 16));
            }

            _result = new Avalonia.Controls.Maui.Services.ImageSourceServiceResult(bitmap);
            _completion.TrySetResult(_result);
        }
    }

    private sealed class ImageSourceServiceMauiContext : IMauiContext
    {
        public ImageSourceServiceMauiContext(IServiceProvider services, IImageSourceService imageSourceService)
        {
            Services = new ImageSourceServiceProviderOverride(services, imageSourceService);
        }

        public IServiceProvider Services { get; }

        public IMauiHandlersFactory Handlers => Services.GetRequiredService<IMauiHandlersFactory>();
    }

    private sealed class ImageSourceServiceProviderOverride : IImageSourceServiceProvider
    {
        private readonly IImageSourceService _imageSourceService;

        public ImageSourceServiceProviderOverride(IServiceProvider hostServiceProvider, IImageSourceService imageSourceService)
        {
            HostServiceProvider = hostServiceProvider;
            _imageSourceService = imageSourceService;
        }

        public IServiceProvider HostServiceProvider { get; }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IImageSourceServiceProvider))
                return this;

            if (serviceType == typeof(IImageSourceService<IStreamImageSource>))
                return _imageSourceService;

            return HostServiceProvider.GetService(serviceType);
        }

        public IImageSourceService? GetImageSourceService(Type imageSource)
        {
            return imageSource == typeof(IStreamImageSource)
                ? _imageSourceService
                : null;
        }
    }

    private sealed class TabInfoWithIcon
    {
        public string Title { get; init; } = string.Empty;
        public ImageSource? Icon { get; init; }
    }
}
