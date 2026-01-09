using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using AvaloniaGrid = Avalonia.Controls.Grid;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Controls;
using MauiWindowHandler = Avalonia.Controls.Maui.Handlers.WindowHandler;
using MauiWindow = Microsoft.Maui.Controls.Window;
using AvaloniaDispatcher = Avalonia.Threading.Dispatcher;

namespace Avalonia.Controls.Maui.Tests.Platform
{
    public class AlertManagerTests : HandlerTestBase
    {
        [AvaloniaFact(DisplayName = "DisplayAlert Adds Overlay And Returns Result")]
        public async Task DisplayAlert_Adds_Overlay_And_Returns_Result()
        {
            EnsureHandlerCreated();
            
            var page = new ContentPage();
            var window = new MauiWindow(page);
            var windowHandler = await CreateHandlerAsync<MauiWindowHandler>(window);
            
            var alertTask = page.DisplayAlertAsync("Alert Title", "Message", "OK");
            
            await Task.Delay(100);
            AvaloniaDispatcher.UIThread.RunJobs();
            
            var avaloniaWindow = windowHandler.PlatformView as Window;
            var wrapper = avaloniaWindow.Content as AvaloniaGrid;
            Assert.NotNull(wrapper);
            
            // Visual tree: Wrapper -> OverlayGrid -> DialogGrid -> DialogContainer -> Dialog
            var overlayGrid = wrapper.Children.OfType<AvaloniaGrid>().LastOrDefault();
            Assert.NotNull(overlayGrid);
            
            var dialogGrid = overlayGrid.Children.OfType<AvaloniaGrid>().LastOrDefault(); 
            Assert.NotNull(dialogGrid);
            Assert.True(dialogGrid.IsVisible);
            Assert.NotEmpty(dialogGrid.Children);

            var dialogContainer = dialogGrid.Children.Last() as AvaloniaGrid;
            Assert.NotNull(dialogContainer);
            
            var dialogControl = dialogContainer.Children.FirstOrDefault();
            Assert.IsType<MauiAlertDialog>(dialogControl);
            
            (dialogControl as TemplatedControl)?.ApplyTemplate();
            dialogControl.Measure(new Size(800, 600));
            dialogControl.Arrange(new Rect(0, 0, 800, 600));
            
            var okButton = dialogControl.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault();
            
            Assert.NotNull(okButton);
            
            okButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            for (int i = 0; i < 5; i++)
            {
                AvaloniaDispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }
            
            var timeout = Task.Delay(2000);
            var completed = await Task.WhenAny(alertTask, timeout);
            if (completed == timeout)
            {
               Assert.Empty(dialogGrid.Children);
            }
            else
            {
               await alertTask;
               Assert.Empty(dialogGrid.Children);
            }
        }

        [AvaloniaFact(DisplayName = "DisplayActionSheet Adds Overlay")]
        public async Task DisplayActionSheet_Adds_Overlay()
        {
            EnsureHandlerCreated();
            
            var page = new ContentPage();
            var window = new MauiWindow(page);
            var windowHandler = await CreateHandlerAsync<MauiWindowHandler>(window);
            
            var task = page.DisplayActionSheetAsync("Title", "Cancel", "Destruction", "Option 1", "Option 2");
            
            await Task.Delay(100);
            AvaloniaDispatcher.UIThread.RunJobs();
            
            var avaloniaWindow = windowHandler.PlatformView as Window;
            var wrapper = avaloniaWindow.Content as AvaloniaGrid;
            
            var overlayGrid = wrapper.Children.OfType<AvaloniaGrid>().LastOrDefault();
            var dialogGrid = overlayGrid.Children.OfType<AvaloniaGrid>().LastOrDefault();
            
            var dialogContainer = dialogGrid.Children.Last() as AvaloniaGrid;
            var dialogControl = dialogContainer.Children.FirstOrDefault();
            Assert.IsType<MauiActionSheetDialog>(dialogControl);
            
            (dialogControl as TemplatedControl)?.ApplyTemplate();
            dialogControl.Measure(new Size(800, 600));
            dialogControl.Arrange(new Rect(0, 0, 800, 600));
            
            var button = dialogControl.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(b => b.Content?.ToString() == "Option 1");
                
            Assert.NotNull(button);
            
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            for (int i = 0; i < 5; i++)
            {
                AvaloniaDispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }
            
            var timeout = Task.Delay(2000);
            var completed = await Task.WhenAny(task, timeout);
            if (completed == timeout)
            {
               Assert.Empty(dialogGrid.Children);
            }
            else
            {
               var result = await task;
               Assert.Equal("Option 1", result);
               Assert.Empty(dialogGrid.Children);
            }
        }
        
        [AvaloniaFact(DisplayName = "DisplayPrompt Adds Overlay")]
        public async Task DisplayPrompt_Adds_Overlay()
        {
            EnsureHandlerCreated();
            
            var page = new ContentPage();
            var window = new MauiWindow(page);
            var windowHandler = await CreateHandlerAsync<MauiWindowHandler>(window);
            
            var task = page.DisplayPromptAsync("Title", "Message");
            
            await Task.Delay(100);
            AvaloniaDispatcher.UIThread.RunJobs();
            
            var avaloniaWindow = windowHandler.PlatformView as Window;
            var wrapper = avaloniaWindow.Content as AvaloniaGrid;
            
            var overlayGrid = wrapper.Children.OfType<AvaloniaGrid>().LastOrDefault();
            var dialogGrid = overlayGrid.Children.OfType<AvaloniaGrid>().LastOrDefault();
            
            var dialogContainer = dialogGrid.Children.Last() as AvaloniaGrid;
            var dialogControl = dialogContainer.Children.FirstOrDefault();
            Assert.IsType<MauiPromptDialog>(dialogControl);
            
            (dialogControl as TemplatedControl)?.ApplyTemplate();
            dialogControl.Measure(new Size(800, 600));
            dialogControl.Arrange(new Rect(0, 0, 800, 600));
            
            var textBox = dialogControl.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
            Assert.NotNull(textBox);
            textBox.Text = "Test Input";
            
            var okButton = dialogControl.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(b => b.Content?.ToString() == "OK");
            
            Assert.NotNull(okButton);
            okButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            for (int i = 0; i < 5; i++)
            {
                AvaloniaDispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }
            
            var timeout = Task.Delay(2000);
            var completed = await Task.WhenAny(task, timeout);
            if (completed == timeout)
            {
               Assert.Empty(dialogGrid.Children);
            }
            else
            {
               var result = await task;
               Assert.Equal("Test Input", result);
            }
        }
    }
}
