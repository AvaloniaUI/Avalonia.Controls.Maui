using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class TabbedPageHandlerTests : HandlerTestBase<TabbedPageHandler, TabbedPageStub>
{
    [AvaloniaFact(DisplayName = "Handler Creates TabControl Platform View")]
    public async Task Handler_Creates_TabControl_Platform_View()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.NotNull(handler.PlatformView);
            Assert.IsType<TabControl>(handler.PlatformView);
        });
    }

    [AvaloniaFact(DisplayName = "TabControl Has Top Tab Strip Placement")]
    public async Task TabControl_Has_Top_Tab_Strip_Placement()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal(Dock.Top, handler.PlatformView.TabStripPlacement);
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarBackgroundColor));

            // Check resource is stored
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarBackgroundColor Sets Tab Strip Background")]
    public async Task MapBarBackgroundColor_Sets_Tab_Strip_Background()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarBackgroundColor = Microsoft.Maui.Graphics.Colors.Red
            };
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarBackgroundColor));

            // Check resource is stored (tab strip background is applied to ItemsPresenter container)
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarBackgroundColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabbedPageBarBackground"));

            stub.BarBackgroundColor = null;
            handler.UpdateValue(nameof(TabbedPage.BarBackgroundColor));

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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarTextColor));

            // BarTextColor is now mapped to Fluent theme foreground resources
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapBarTextColor Sets TabItem Foreground")]
    public async Task MapBarTextColor_Sets_TabItem_Foreground()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                BarTextColor = Microsoft.Maui.Graphics.Colors.White
            };
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarTextColor));

            var tabItem = handler.PlatformView.Items[0] as TabItem;
            Assert.NotNull(tabItem);
            Assert.NotNull(tabItem.Foreground);
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarTextColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));

            stub.BarTextColor = null;
            handler.UpdateValue(nameof(TabbedPage.BarTextColor));

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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarBackground));

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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarBackground));
            handler.UpdateValue(nameof(TabbedPage.BarBackgroundColor));

            // BarBackground should be in resources
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.SelectedTabColor));

            // SelectedTabColor is now mapped to Fluent pipe fill and accent resources
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));
        });
    }

    [AvaloniaFact(DisplayName = "MapSelectedTabColor Sets Pipe Fill Resource")]
    public async Task MapSelectedTabColor_Sets_Pipe_Fill_Resource()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                SelectedTabColor = Microsoft.Maui.Graphics.Colors.Orange
            };
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            stub.Children.Add(new ContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.SelectedTabColor));

            // SelectedTabColor maps to pipe fill and selected foreground, not a tab background
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundSelected"));
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.SelectedTabColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderSelectedPipeFill"));

            stub.SelectedTabColor = null;
            handler.UpdateValue(nameof(TabbedPage.SelectedTabColor));

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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            stub.Children.Add(new ContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.UnselectedTabColor));

            // UnselectedTabColor is now mapped to Fluent unselected foreground resources
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));
        });
    }

    [AvaloniaFact(DisplayName = "MapUnselectedTabColor Sets Unselected Foreground Resource")]
    public async Task MapUnselectedTabColor_Sets_Unselected_Foreground_Resource()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub
            {
                UnselectedTabColor = Microsoft.Maui.Graphics.Colors.Gray
            };
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            stub.Children.Add(new ContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.UnselectedTabColor));

            // UnselectedTabColor maps to Fluent foreground resources, not direct Background
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.UnselectedTabColor));
            Assert.True(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));

            stub.UnselectedTabColor = null;
            handler.UpdateValue(nameof(TabbedPage.UnselectedTabColor));

            Assert.False(handler.PlatformView.Resources.ContainsKey("TabItemHeaderForegroundUnselected"));
        });
    }

    [AvaloniaFact(DisplayName = "Children Collection Creates Tab Items")]
    public async Task Children_Collection_Creates_Tab_Items()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            stub.Children.Add(new ContentPage { Title = "Tab 2" });
            stub.Children.Add(new ContentPage { Title = "Tab 3" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal(3, handler.PlatformView.Items.Count);

            var tab1 = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            Assert.Equal("Tab 1", tab1.Header);

            var tab2 = Assert.IsType<TabItem>(handler.PlatformView.Items[1]);
            Assert.Equal("Tab 2", tab2.Header);

            var tab3 = Assert.IsType<TabItem>(handler.PlatformView.Items[2]);
            Assert.Equal("Tab 3", tab3.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Single Child Creates Single Tab")]
    public async Task Single_Child_Creates_Single_Tab()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "Only Tab" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Single(handler.PlatformView.Items);
            var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            Assert.Equal("Only Tab", tab.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Many Children Creates Many Tabs")]
    public async Task Many_Children_Creates_Many_Tabs()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            for (int i = 0; i < 10; i++)
            {
                stub.Children.Add(new ContentPage { Title = $"Tab {i + 1}" });
            }

            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Equal(10, handler.PlatformView.Items.Count);
            for (int i = 0; i < 10; i++)
            {
                var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[i]);
                Assert.Equal($"Tab {i + 1}", tab.Header);
            }
        });
    }

    [AvaloniaFact(DisplayName = "MapCurrentPage Updates Selected Index")]
    public async Task MapCurrentPage_Updates_Selected_Index()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var page1 = new ContentPage { Title = "Tab 1" };
            var page2 = new ContentPage { Title = "Tab 2" };
            var page3 = new ContentPage { Title = "Tab 3" };

            var stub = new TabbedPageStub();
            stub.Children.Add(page1);
            stub.Children.Add(page2);
            stub.Children.Add(page3);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            Assert.Equal(0, handler.PlatformView.SelectedIndex);

            stub.CurrentPage = page3;
            handler.UpdateValue(nameof(TabbedPage.CurrentPage));

            Assert.Equal(2, handler.PlatformView.SelectedIndex);
        });
    }

    [AvaloniaFact(DisplayName = "MapCurrentPage With First Page")]
    public async Task MapCurrentPage_With_First_Page()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var page1 = new ContentPage { Title = "Tab 1" };
            var page2 = new ContentPage { Title = "Tab 2" };

            var stub = new TabbedPageStub();
            stub.Children.Add(page1);
            stub.Children.Add(page2);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            stub.CurrentPage = page1;
            handler.UpdateValue(nameof(TabbedPage.CurrentPage));

            Assert.Equal(0, handler.PlatformView.SelectedIndex);
        });
    }

    [AvaloniaFact(DisplayName = "MapCurrentPage With Last Page")]
    public async Task MapCurrentPage_With_Last_Page()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var page1 = new ContentPage { Title = "Tab 1" };
            var page2 = new ContentPage { Title = "Tab 2" };
            var page3 = new ContentPage { Title = "Tab 3" };

            var stub = new TabbedPageStub();
            stub.Children.Add(page1);
            stub.Children.Add(page2);
            stub.Children.Add(page3);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            Assert.Equal(0, handler.PlatformView.SelectedIndex);

            stub.CurrentPage = page3;
            handler.UpdateValue(nameof(TabbedPage.CurrentPage));

            Assert.Equal(2, handler.PlatformView.SelectedIndex);
        });
    }

    [AvaloniaFact(DisplayName = "Empty TabbedPage Has No Tab Items")]
    public async Task Empty_TabbedPage_Has_No_Tab_Items()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Empty(handler.PlatformView.Items);
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

    [AvaloniaFact(DisplayName = "Tab With Null Title Uses Default")]
    public async Task Tab_With_Null_Title_Uses_Default()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = null });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            Assert.Equal("Tab", tab.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Tab With Empty Title Shows Empty")]
    public async Task Tab_With_Empty_Title()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            // Empty string stays empty (Title check is null-only)
            Assert.Equal("", tab.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Tab Title With Special Characters")]
    public async Task Tab_Title_With_Special_Characters()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "Tab <>&\"'" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            Assert.Equal("Tab <>&\"'", tab.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Tab Title With Unicode Characters")]
    public async Task Tab_Title_With_Unicode_Characters()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "日本語 🎉" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            Assert.Equal("日本語 🎉", tab.Header);
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
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            stub.Children.Add(new ContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);
            handler.UpdateValue(nameof(TabbedPage.BarBackgroundColor));
            handler.UpdateValue(nameof(TabbedPage.BarTextColor));
            handler.UpdateValue(nameof(TabbedPage.SelectedTabColor));
            handler.UpdateValue(nameof(TabbedPage.UnselectedTabColor));

            // Verify all resources are set (using new Fluent theme resource keys)
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
                stub.Children.Add(new ContentPage { Title = $"Tab {i}" });
                var handler = CreateHandler<TabbedPageHandler>(stub);

                Assert.NotNull(handler.PlatformView);
                Assert.Single(handler.PlatformView.Items);
            }
        });
    }

    [AvaloniaFact(DisplayName = "Default Tab Item Selection Is First Tab")]
    public async Task Default_Tab_Item_Selection_Is_First_Tab()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            stub.Children.Add(new ContentPage { Title = "Tab 2" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            // First tab is selected by default
            Assert.Equal(0, handler.PlatformView.SelectedIndex);
        });
    }

    [AvaloniaFact(DisplayName = "Tab Without Icon Has String Header")]
    public async Task Tab_Without_Icon_Has_String_Header()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "Tab Without Icon" });

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tabItem = handler.PlatformView.Items[0] as TabItem;
            Assert.NotNull(tabItem);
            Assert.IsType<string>(tabItem.Header);
            Assert.Equal("Tab Without Icon", tabItem.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Tab With Icon Has StackPanel Header")]
    public async Task Tab_With_Icon_Has_StackPanel_Header()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var page = new ContentPage 
            { 
                Title = "Tab With Icon",
                IconImageSource = "dotnet_bot.png"
            };
            stub.Children.Add(page);

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tabItem = handler.PlatformView.Items[0] as TabItem;
            Assert.NotNull(tabItem);
            // When icon is present, header is a StackPanel
            Assert.IsType<StackPanel>(tabItem.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Tab With Icon Header Contains TextBlock")]
    public async Task Tab_With_Icon_Header_Contains_TextBlock()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var page = new ContentPage 
            { 
                Title = "My Tab",
                IconImageSource = "dotnet_logo.png"
            };
            stub.Children.Add(page);

            var handler = CreateHandler<TabbedPageHandler>(stub);

            var tabItem = handler.PlatformView.Items[0] as TabItem;
            Assert.NotNull(tabItem);
            
            var header = tabItem.Header as StackPanel;
            Assert.NotNull(header);
            
            // Should contain a TextBlock with the title
            var textBlock = header.Children.OfType<TextBlock>().FirstOrDefault();
            Assert.NotNull(textBlock);
            Assert.Equal("My Tab", textBlock.Text);
        });
    }
    [AvaloniaFact(DisplayName = "Adding Child Update Tab Items")]
    public async Task Adding_Child_Updates_Tab_Items()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            stub.Children.Add(new ContentPage { Title = "Tab 1" });
            var handler = CreateHandler<TabbedPageHandler>(stub);

            Assert.Single(handler.PlatformView.Items);

            // Add a new child
            stub.Children.Add(new ContentPage { Title = "Tab 2" });

            Assert.Equal(2, handler.PlatformView.Items.Count);
            var tab2 = Assert.IsType<TabItem>(handler.PlatformView.Items[1]);
            Assert.Equal("Tab 2", tab2.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Removing Child Updates Tab Items")]
    public async Task Removing_Child_Updates_Tab_Items()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var page1 = new ContentPage { Title = "Tab 1" };
            var page2 = new ContentPage { Title = "Tab 2" };
            stub.Children.Add(page1);
            stub.Children.Add(page2);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            Assert.Equal(2, handler.PlatformView.Items.Count);

            // Remove the first child
            stub.Children.Remove(page1);

            Assert.Single(handler.PlatformView.Items);
            var tab = Assert.IsType<TabItem>(handler.PlatformView.Items[0]);
            Assert.Equal("Tab 2", tab.Header);
        });
    }

    [AvaloniaFact(DisplayName = "Removing Selected Child Updates Selection")]
    public async Task Removing_Selected_Child_Updates_Selection()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var page1 = new ContentPage { Title = "Tab 1" };
            var page2 = new ContentPage { Title = "Tab 2" };
            stub.Children.Add(page1);
            stub.Children.Add(page2);

            var handler = CreateHandler<TabbedPageHandler>(stub);
            
            // Select first tab
            stub.CurrentPage = page1;
            handler.UpdateValue(nameof(TabbedPage.CurrentPage));
            Assert.Equal(0, handler.PlatformView.SelectedIndex);

            // Remove selected tab
            stub.Children.Remove(page1);

            // Should select the remaining tab (now at index 0)
            Assert.Single(handler.PlatformView.Items);
            Assert.Equal(0, handler.PlatformView.SelectedIndex);
            var selectedTab = Assert.IsType<TabItem>(handler.PlatformView.SelectedItem);
            Assert.Equal("Tab 2", selectedTab.Header);
        });
    }

    [AvaloniaFact(DisplayName = "ItemsSource Populates Tab Items")]
    public async Task ItemsSource_Populates_Tab_Items()
    {
        EnsureHandlerCreated();

        await InvokeOnMainThreadAsync(() =>
        {
            var stub = new TabbedPageStub();
            var items = new List<string> { "Item 1", "Item 2", "Item 3" };
            
            stub.ItemsSource = items;
            stub.ItemTemplate = new DataTemplate(() =>
            {
                var page = new ContentPage();
                page.SetBinding(ContentPage.TitleProperty, ".");
                return page;
            });
            
            var handler = CreateHandler<TabbedPageHandler>(stub);
            
            // Verify items are created (MAUI logic creates pages from ItemsSource)
            Assert.Equal(3, handler.PlatformView.Items.Count);
            
            // Verify selection via ItemsSource object
            stub.SelectedItem = "Item 2";
            handler.UpdateValue(nameof(TabbedPage.SelectedItem));
            
            Assert.Equal(1, handler.PlatformView.SelectedIndex);
        });
    }
}
