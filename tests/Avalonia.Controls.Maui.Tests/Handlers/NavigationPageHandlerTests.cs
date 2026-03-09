using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Media;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiNavigationPage = Microsoft.Maui.Controls.NavigationPage;
using MauiFlyoutPage = Microsoft.Maui.Controls.FlyoutPage;
using AvaloniaNavigationPage = Avalonia.Controls.NavigationPage;

namespace Avalonia.Controls.Maui.Tests.Handlers
{
    public class NavigationPageHandlerTests : HandlerTestBase<NavigationViewHandler, MauiNavigationPage>
    {
        [AvaloniaFact(DisplayName = "NavigationPage Creates AvaloniaNavigationPage Platform View")]
        public async Task NavigationPage_Creates_AvaloniaNavigationPage_Platform_View()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.NotNull(handler.PlatformView);
                Assert.IsType<AvaloniaNavigationPage>(handler.PlatformView);
            });
        }

        [AvaloniaFact(DisplayName = "Initial Page Is Pushed To Stack")]
        public async Task Initial_Page_Is_Pushed_To_Stack()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Single(navigationPage.Navigation.NavigationStack);
                Assert.Equal(rootPage, navigationPage.CurrentPage);
            });
        }

        [AvaloniaFact(DisplayName = "Push Adds Page To Stack")]
        public async Task Push_Adds_Page_To_Stack()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                var detailPage = new MauiContentPage { Title = "Detail" };
                await navigationPage.PushAsync(detailPage);

                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);
                Assert.Equal(detailPage, navigationPage.CurrentPage);
            });
        }

        [AvaloniaFact(DisplayName = "Pop Removes Page From Stack")]
        public async Task Pop_Removes_Page_From_Stack()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                await navigationPage.PushAsync(new MauiContentPage { Title = "Detail" });
                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);

                await navigationPage.PopAsync();
                Assert.Single(navigationPage.Navigation.NavigationStack);
                Assert.Equal(rootPage, navigationPage.CurrentPage);
            });
        }

        [AvaloniaFact(DisplayName = "HasBackButton Is False At Root")]
        public async Task HasBackButton_Is_False_At_Root()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                // At root, only 1 page on stack - no back button expected
                Assert.Single(navigationPage.Navigation.NavigationStack);
            });
        }

        [AvaloniaFact(DisplayName = "HasBackButton Is True After Push")]
        public async Task HasBackButton_Is_True_After_Push()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                await navigationPage.PushAsync(new MauiContentPage { Title = "Detail" });
                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);
            });
        }

        [AvaloniaFact(DisplayName = "Hamburger Present When Inside FlyoutPage Popover At Root")]
        public async Task Hamburger_Present_When_Inside_FlyoutPage_Popover_At_Root()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);
            var flyoutPage = new MauiFlyoutPage
            {
                Flyout = new MauiContentPage { Title = "Menu" },
                Detail = navigationPage,
                FlyoutLayoutBehavior = Microsoft.Maui.Controls.FlyoutLayoutBehavior.Popover
            };

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                // At root within FlyoutPage popover: stack depth is 1
                Assert.Single(navigationPage.Navigation.NavigationStack);
                Assert.Equal(rootPage, navigationPage.CurrentPage);
            });
        }

        [AvaloniaFact(DisplayName = "Back Button Replaces Hamburger When Navigated Deeper")]
        public async Task Back_Button_Replaces_Hamburger_When_Navigated_Deeper()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);
            var flyoutPage = new MauiFlyoutPage
            {
                Flyout = new MauiContentPage { Title = "Menu" },
                Detail = navigationPage,
                FlyoutLayoutBehavior = Microsoft.Maui.Controls.FlyoutLayoutBehavior.Popover
            };

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            // At root: 1 page
            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Single(navigationPage.Navigation.NavigationStack);
            });

            // Push a second page
            await InvokeOnMainThreadAsync(async () =>
            {
                await navigationPage.PushAsync(new MauiContentPage { Title = "Detail Page" });
            });

            // After push: 2 pages
            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Are Present On Page")]
        public async Task ToolbarItems_Are_Present_On_Page()
        {
            var page = new PageStub();
            page.ToolbarItems.Add(new Microsoft.Maui.Controls.ToolbarItem { Text = "Item 1" });
            page.ToolbarItems.Add(new Microsoft.Maui.Controls.ToolbarItem { Text = "Item 2" });

            var navigationPage = new MauiNavigationPage(page);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Equal(2, page.ToolbarItems.Count);
                Assert.Equal("Item 1", page.ToolbarItems[0].Text);
                Assert.Equal("Item 2", page.ToolbarItems[1].Text);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Update When Collection Changes")]
        public async Task ToolbarItems_Update_When_Collection_Changes()
        {
            var page = new PageStub();
            var item1 = new Microsoft.Maui.Controls.ToolbarItem { Text = "Item 1" };
            page.ToolbarItems.Add(item1);

            var navigationPage = new MauiNavigationPage(page);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Single(page.ToolbarItems);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                page.ToolbarItems.Add(new Microsoft.Maui.Controls.ToolbarItem { Text = "Item 2" });
            });

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Equal(2, page.ToolbarItems.Count);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                page.ToolbarItems.Remove(item1);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Single(page.ToolbarItems);
                Assert.Equal("Item 2", page.ToolbarItems[0].Text);
            });
        }

        [AvaloniaFact(DisplayName = "Page Title Is Accessible")]
        public async Task Page_Title_Is_Accessible()
        {
            var page = new PageStub { Title = "My Page" };
            var navigationPage = new MauiNavigationPage(page);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.Equal("My Page", navigationPage.CurrentPage?.Title);
            });
        }

        [AvaloniaFact(DisplayName = "HasNavigationBar Defaults To True")]
        public async Task HasNavigationBar_Defaults_To_True()
        {
            var page = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(page);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.True(MauiNavigationPage.GetHasNavigationBar(page));
            });
        }

        [AvaloniaFact(DisplayName = "HasNavigationBar Can Be Set To False")]
        public async Task HasNavigationBar_Can_Be_Set_To_False()
        {
            var page = new PageStub { Title = "Root" };
            MauiNavigationPage.SetHasNavigationBar(page, false);
            var navigationPage = new MauiNavigationPage(page);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.False(MauiNavigationPage.GetHasNavigationBar(page));
            });
        }

        [AvaloniaFact(DisplayName = "Multiple Pushes Build Stack Correctly")]
        public async Task Multiple_Pushes_Build_Stack_Correctly()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                var page2 = new MauiContentPage { Title = "Page 2" };
                var page3 = new MauiContentPage { Title = "Page 3" };

                await navigationPage.PushAsync(page2);
                await navigationPage.PushAsync(page3);

                Assert.Equal(3, navigationPage.Navigation.NavigationStack.Count);
                Assert.Equal(page3, navigationPage.CurrentPage);
            });
        }

        [AvaloniaFact(DisplayName = "IconColor Sets NavigationBarForeground Resource")]
        public async Task IconColor_Sets_NavigationBarForeground_Resource()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                var childPage = new MauiContentPage { Title = "Red Icon" };
                MauiNavigationPage.SetIconColor(childPage, MauiColors.Red);
                await navigationPage.PushAsync(childPage);

                Assert.True(handler.PlatformView.Resources.ContainsKey("NavigationBarForeground"));
                var brush = handler.PlatformView.Resources["NavigationBarForeground"] as ISolidColorBrush;
                Assert.NotNull(brush);
                Assert.Equal(MauiColors.Red.ToAvaloniaColor(), brush.Color);
            });
        }

        [AvaloniaFact(DisplayName = "IconColor Cleared When Navigating Back")]
        public async Task IconColor_Cleared_When_Navigating_Back()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                var childPage = new MauiContentPage { Title = "Red Icon" };
                MauiNavigationPage.SetIconColor(childPage, MauiColors.Red);
                await navigationPage.PushAsync(childPage);

                Assert.True(handler.PlatformView.Resources.ContainsKey("NavigationBarForeground"));

                await navigationPage.PopAsync();

                // Root page has no IconColor, so resource should be removed
                Assert.False(handler.PlatformView.Resources.ContainsKey("NavigationBarForeground"));
            });
        }

        [AvaloniaFact(DisplayName = "IconColor Takes Priority Over BarTextColor")]
        public async Task IconColor_Takes_Priority_Over_BarTextColor()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                // Set BarTextColor on the NavigationPage
                navigationPage.BarTextColor = MauiColors.Green;

                // Push a page with IconColor
                var childPage = new MauiContentPage { Title = "Blue Icon" };
                MauiNavigationPage.SetIconColor(childPage, MauiColors.Blue);
                await navigationPage.PushAsync(childPage);

                // IconColor should win
                var brush = handler.PlatformView.Resources["NavigationBarForeground"] as ISolidColorBrush;
                Assert.NotNull(brush);
                Assert.Equal(MauiColors.Blue.ToAvaloniaColor(), brush.Color);
            });
        }

        [AvaloniaFact(DisplayName = "BarBackgroundColor Set And Reset Does Not Crash")]
        public async Task BarBackgroundColor_Set_And_Reset_Does_Not_Crash()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                // Set a color
                navigationPage.BarBackgroundColor = MauiColors.ForestGreen;
                Assert.True(handler.PlatformView.Resources.ContainsKey("NavigationBarBackground"));

                // Reset to null — should not crash
                navigationPage.BarBackgroundColor = null;
                Assert.False(handler.PlatformView.Resources.ContainsKey("NavigationBarBackground"));
            });
        }

        [AvaloniaFact(DisplayName = "BarTextColor Set And Reset Does Not Crash")]
        public async Task BarTextColor_Set_And_Reset_Does_Not_Crash()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                // Set a color
                navigationPage.BarTextColor = MauiColors.White;
                Assert.True(handler.PlatformView.Resources.ContainsKey("NavigationBarForeground"));

                // Reset to null — should not crash
                navigationPage.BarTextColor = null;
                Assert.False(handler.PlatformView.Resources.ContainsKey("NavigationBarForeground"));
            });
        }

        [AvaloniaFact(DisplayName = "BarBackground Brush Sets Resource")]
        public async Task BarBackground_Brush_Sets_Resource()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                navigationPage.BarBackground = new Microsoft.Maui.Controls.SolidColorBrush(MauiColors.Purple);
                Assert.True(handler.PlatformView.Resources.ContainsKey("NavigationBarBackground"));

                // Reset
                navigationPage.BarBackground = null;
                Assert.False(handler.PlatformView.Resources.ContainsKey("NavigationBarBackground"));
            });
        }

        [AvaloniaFact(DisplayName = "Avalonia Back Button Pop Syncs To MAUI Stack")]
        public async Task Avalonia_Back_Button_Pop_Syncs_To_MAUI_Stack()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                // Push a page via MAUI
                var detailPage = new MauiContentPage { Title = "Detail" };
                await navigationPage.PushAsync(detailPage);
                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);

                // Simulate Avalonia-side pop (as if the user clicked the back button)
                await handler.PlatformView.PopAsync();

                // MAUI stack should be synced back to 1
                Assert.Single(navigationPage.Navigation.NavigationStack);
                Assert.Equal(rootPage, navigationPage.CurrentPage);
            });
        }

        [AvaloniaFact(DisplayName = "Push Pop Push Results In Correct Stack Depth")]
        public async Task Push_Pop_Push_Results_In_Correct_Stack_Depth()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new MauiNavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(async () =>
            {
                // Push page 1
                var page1 = new MauiContentPage { Title = "Page 1" };
                await navigationPage.PushAsync(page1);
                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);

                // Simulate Avalonia-side pop
                await handler.PlatformView.PopAsync();
                Assert.Single(navigationPage.Navigation.NavigationStack);

                // Push page 2 — should result in depth 2, not 3
                var page2 = new MauiContentPage { Title = "Page 2" };
                await navigationPage.PushAsync(page2);
                Assert.Equal(2, navigationPage.Navigation.NavigationStack.Count);
                Assert.Equal(page2, navigationPage.CurrentPage);
            });
        }
    }
}
