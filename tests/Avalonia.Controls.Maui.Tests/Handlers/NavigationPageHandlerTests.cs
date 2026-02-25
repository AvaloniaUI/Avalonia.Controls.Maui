using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Tests.Stubs;

namespace Avalonia.Controls.Maui.Tests.Handlers
{
    public class NavigationPageHandlerTests : HandlerTestBase<NavigationViewHandler, NavigationPage>
    {
        [AvaloniaFact(DisplayName = "Hamburger Visible When Inside FlyoutPage Popover At Root")]
        public async Task Hamburger_Visible_When_Inside_FlyoutPage_Popover_At_Root()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new NavigationPage(rootPage);
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Menu" },
                Detail = navigationPage,
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover
            };

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.True(handler.PlatformView.HamburgerButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "Hamburger Hidden When NavigationPage Is Standalone")]
        public async Task Hamburger_Hidden_When_NavigationPage_Is_Standalone()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new NavigationPage(rootPage);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.False(handler.PlatformView.HamburgerButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "Hamburger Hidden When FlyoutBehavior Is Split")]
        public async Task Hamburger_Hidden_When_FlyoutBehavior_Is_Split()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new NavigationPage(rootPage);
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Menu" },
                Detail = navigationPage,
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split
            };

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.False(handler.PlatformView.HamburgerButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "Back Button Replaces Hamburger When Navigated Deeper")]
        public async Task Back_Button_Replaces_Hamburger_When_Navigated_Deeper()
        {
            var rootPage = new PageStub { Title = "Root" };
            var navigationPage = new NavigationPage(rootPage);
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Menu" },
                Detail = navigationPage,
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover
            };

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            // At root: hamburger visible, back hidden
            await InvokeOnMainThreadAsync(() =>
            {
                Assert.True(handler.PlatformView.HamburgerButton.IsVisible);
                Assert.False(handler.PlatformView.BackButton.IsVisible);
            });

            // Push a second page
            await InvokeOnMainThreadAsync(async () =>
            {
                await navigationPage.PushAsync(new ContentPage { Title = "Detail Page" });
            });

            // After push: hamburger hidden, back visible
            await InvokeOnMainThreadAsync(() =>
            {
                Assert.False(handler.PlatformView.HamburgerButton.IsVisible);
                Assert.True(handler.PlatformView.BackButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Are Added To Platform View")]
        public async Task ToolbarItems_Are_Added_To_Platform_View()
        {
            var page = new PageStub();
            page.ToolbarItems.Add(new ToolbarItem { Text = "Item 1" });
            page.ToolbarItems.Add(new ToolbarItem { Text = "Item 2" });

            var navigationPage = new NavigationPage(page);
            
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var navigationView = handler.PlatformView;
                Assert.NotNull(navigationView.ToolbarItemsContainer);
                
                var items = navigationView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();

                Assert.Equal(2, items.Count);
                
                Assert.Equal("Item 1", items[0].Content);
                Assert.Equal("Item 2", items[1].Content);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Update When Collection Changes")]
        public async Task ToolbarItems_Update_When_Collection_Changes()
        {
            var page = new PageStub();
            var item1 = new ToolbarItem { Text = "Item 1" };
            page.ToolbarItems.Add(item1);

            var navigationPage = new NavigationPage(page);
            
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var items = handler.PlatformView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();
                Assert.Single(items);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                page.ToolbarItems.Add(new ToolbarItem { Text = "Item 2" });
            });

            await InvokeOnMainThreadAsync(() =>
            {
                var items = handler.PlatformView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();
                Assert.Equal(2, items.Count);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                page.ToolbarItems.Remove(item1);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                 var items = handler.PlatformView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();
                 Assert.Single(items);
                 Assert.Equal("Item 2", items[0].Content);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Update When Property Changes")]
        public async Task ToolbarItems_Update_When_Property_Changes()
        {
            var page = new PageStub();
            var item1 = new ToolbarItem { Text = "Item 1", IsEnabled = true };
            page.ToolbarItems.Add(item1);

            var navigationPage = new NavigationPage(page);

            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var items = handler.PlatformView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();
                var btn = items[0];
                Assert.Equal("Item 1", btn.Content);
                Assert.True(btn.IsEnabled);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                item1.Text = "Updated Item";
            });

            await InvokeOnMainThreadAsync(() =>
            {
                var items = handler.PlatformView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();
                var btn = items[0];
                Assert.Equal("Updated Item", btn.Content);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                item1.IsEnabled = false;
            });

            await InvokeOnMainThreadAsync(() =>
            {
                var items = handler.PlatformView.ToolbarItemsContainer.Children
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() != "...")
                    .ToList();
                var btn = items[0];
                Assert.False(btn.IsEnabled);
            });
        }
        [AvaloniaFact(DisplayName = "ToolbarItems Are Sorted By Priority")]
        public async Task ToolbarItems_Are_Sorted_By_Priority()
        {
            var page = new PageStub();
            page.ToolbarItems.Add(new ToolbarItem { Text = "Item 2", Priority = 1 });
            page.ToolbarItems.Add(new ToolbarItem { Text = "Item 1", Priority = 0 });
            page.ToolbarItems.Add(new ToolbarItem { Text = "Item 3", Priority = 2 });

            var navigationPage = new NavigationPage(page);
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var container = handler.PlatformView.ToolbarItemsContainer;
                var buttons = container.Children.OfType<Button>().Where(b => b.Content?.ToString() != "...").ToList();

                Assert.Equal(3, buttons.Count);
                Assert.Equal("Item 1", buttons[0].Content);
                Assert.Equal("Item 2", buttons[1].Content);
                Assert.Equal("Item 3", buttons[2].Content);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Respect Order Primary vs Secondary")]
        public async Task ToolbarItems_Respect_Order_Primary_vs_Secondary()
        {
            var page = new PageStub();
            page.ToolbarItems.Add(new ToolbarItem { Text = "Primary", Order = ToolbarItemOrder.Primary });
            page.ToolbarItems.Add(new ToolbarItem { Text = "Secondary", Order = ToolbarItemOrder.Secondary });

            var navigationPage = new NavigationPage(page);
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var view = handler.PlatformView;
                
                // Check Primary
                var primaryContainer = view.ToolbarItemsContainer;
                var primaryBtn = primaryContainer.Children.OfType<Button>().FirstOrDefault(b => b.Content?.ToString() == "Primary");
                Assert.NotNull(primaryBtn);

                // Check Secondary (Overflow)
                var overflowMenu = view.ToolbarOverflowMenu;
                Assert.NotNull(overflowMenu);
                Assert.Single(overflowMenu.Items);
                
                var secondaryItem = overflowMenu.Items[0] as MenuItem;
                Assert.NotNull(secondaryItem);
                Assert.Equal("Secondary", secondaryItem.Header);

                // Verify Overflow Button is visible
                Assert.True(view.ToolbarOverflowButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Update When Priority Changes")]
        public async Task ToolbarItems_Update_When_Priority_Changes()
        {
            var page = new PageStub();
            var item1 = new ToolbarItem { Text = "Item A", Priority = 10 };
            var item2 = new ToolbarItem { Text = "Item B", Priority = 0 }; // Should be first
            page.ToolbarItems.Add(item1);
            page.ToolbarItems.Add(item2);

            var navigationPage = new NavigationPage(page);
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var buttons = handler.PlatformView.ToolbarItemsContainer.Children.OfType<Button>().Where(b => b.Content?.ToString() != "...").ToList();
                Assert.Equal("Item B", buttons[0].Content);
                Assert.Equal("Item A", buttons[1].Content);
            });

            // Change Priority
            await InvokeOnMainThreadAsync(() =>
            {
                item1.Priority = -1; // Should move to first
            });

            await InvokeOnMainThreadAsync(() =>
            {
                var buttons = handler.PlatformView.ToolbarItemsContainer.Children.OfType<Button>().Where(b => b.Content?.ToString() != "...").ToList();
                Assert.Equal("Item A", buttons[0].Content);
                Assert.Equal("Item B", buttons[1].Content);
            });
        }

        [AvaloniaFact(DisplayName = "ToolbarItems Update When Order Changes")]
        public async Task ToolbarItems_Update_When_Order_Changes()
        {
            var page = new PageStub();
            var item = new ToolbarItem { Text = "My Item", Order = ToolbarItemOrder.Primary };
            page.ToolbarItems.Add(item);

            var navigationPage = new NavigationPage(page);
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                // Initially Primary
                var primaryBtn = handler.PlatformView.ToolbarItemsContainer.Children.OfType<Button>().FirstOrDefault(b => b.Content?.ToString() == "My Item");
                Assert.NotNull(primaryBtn);
                Assert.Empty(handler.PlatformView.ToolbarOverflowMenu.Items);
                Assert.False(handler.PlatformView.ToolbarOverflowButton.IsVisible);
            });

            // Change to Secondary
            await InvokeOnMainThreadAsync(() =>
            {
                item.Order = ToolbarItemOrder.Secondary;
            });

            await InvokeOnMainThreadAsync(() =>
            {
                // Should be in Overflow now
                var overflowMenu = handler.PlatformView.ToolbarOverflowMenu;
                Assert.Single(overflowMenu.Items);
                var menuItem = overflowMenu.Items[0] as MenuItem;
                Assert.NotNull(menuItem);
                Assert.Equal("My Item", menuItem.Header);
                
                // Should not be in Primary container
                var primaryBtn = handler.PlatformView.ToolbarItemsContainer.Children.OfType<Button>().FirstOrDefault(b => b.Content?.ToString() == "My Item");
                Assert.Null(primaryBtn);
                
                Assert.True(handler.PlatformView.ToolbarOverflowButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "Multiple Secondary Items Are Added To Overflow Menu")]
        public async Task Multiple_Secondary_Items_Are_Added_To_Overflow_Menu()
        {
            var page = new PageStub();
            page.ToolbarItems.Add(new ToolbarItem { Text = "Sec 1", Order = ToolbarItemOrder.Secondary });
            page.ToolbarItems.Add(new ToolbarItem { Text = "Sec 2", Order = ToolbarItemOrder.Secondary });

            var navigationPage = new NavigationPage(page);
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                var overflowMenu = handler.PlatformView.ToolbarOverflowMenu;
                Assert.Equal(2, overflowMenu.Items.Count);
                
                var item1 = overflowMenu.Items[0] as MenuItem;
                var item2 = overflowMenu.Items[1] as MenuItem;
                
                Assert.NotNull(item1);
                Assert.NotNull(item2);
                Assert.Equal("Sec 1", item1.Header);
                Assert.Equal("Sec 2", item2.Header);
                Assert.True(handler.PlatformView.ToolbarOverflowButton.IsVisible);
            });
        }

        [AvaloniaFact(DisplayName = "Clearing ToolbarItems Hides Overflow Button")]
        public async Task Clearing_ToolbarItems_Hides_Overflow_Button()
        {
            var page = new PageStub();
            page.ToolbarItems.Add(new ToolbarItem { Text = "Sec 1", Order = ToolbarItemOrder.Secondary });

            var navigationPage = new NavigationPage(page);
            var handler = await CreateHandlerAsync<NavigationViewHandler>(navigationPage);

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.True(handler.PlatformView.ToolbarOverflowButton.IsVisible);
            });

            await InvokeOnMainThreadAsync(() =>
            {
                page.ToolbarItems.Clear();
            });

            await InvokeOnMainThreadAsync(() =>
            {
                Assert.False(handler.PlatformView.ToolbarOverflowButton.IsVisible);
                Assert.Empty(handler.PlatformView.ToolbarOverflowMenu.Items);
            });
        }
    }
}
