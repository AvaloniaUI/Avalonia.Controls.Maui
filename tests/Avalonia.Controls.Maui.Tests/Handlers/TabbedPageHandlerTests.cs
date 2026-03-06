using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));
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
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));

            stub.BarBackgroundColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));
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
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));

            stub.BarTextColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.BarTextColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));
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
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));

            stub.SelectedTabColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));
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
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));

            stub.UnselectedTabColor = null;
            handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));
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

            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));
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
}
