using Avalonia.Headless.XUnit;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using ControlsBorder = Microsoft.Maui.Controls.Border;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Avalonia.Controls.Maui.Tests.CommunityToolkit;

public class ToolkitViewTests : ToolkitTestBase
{
    [AvaloniaFact(DisplayName = "AvatarView Shows Its Initials At Its Requested Size")]
    public async Task AvatarViewShowsInitialsAtRequestedSize()
    {
        var avatar = new AvatarView
        {
            Text = "AB",
            WidthRequest = 60,
            HeightRequest = 60,
            HorizontalOptions = LayoutOptions.Start,
        };

        await RunInWindowAsync(avatar, window =>
        {
            var bounds = BoundsInWindow(window, avatar);
            Assert.Equal(60, bounds.Width, 1);
            Assert.Equal(60, bounds.Height, 1);

            var label = Assert.IsType<MauiLabel>(avatar.Content);
            var platformLabel = PlatformViewOf(label);
            Assert.True(platformLabel.IsVisible);
            Assert.True(platformLabel.Bounds.Width > 0 && platformLabel.Bounds.Height > 0);
            Assert.True(bounds.Contains(BoundsInWindow(window, label).Center));
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "AvatarView Switches Between Initials And An Image")]
    public async Task AvatarViewSwitchesBetweenInitialsAndImage()
    {
        var avatar = new AvatarView
        {
            Text = "AB",
            HorizontalOptions = LayoutOptions.Start,
        };

        await RunInWindowAsync(avatar, async window =>
        {
            Assert.IsType<MauiLabel>(avatar.Content);

            avatar.ImageSource = "dotnet_bot.png";
            await WaitUntilAsync(window, () => avatar.Content is Microsoft.Maui.Controls.Image image && image.Handler is not null,
                message: "The AvatarView did not show its image.");

            avatar.ImageSource = null;
            await WaitUntilAsync(window, () => avatar.Content is MauiLabel label && PlatformViewOf(label).IsVisible,
                message: "The AvatarView did not go back to its initials.");
        });
    }

    [AvaloniaFact(DisplayName = "RatingView Lays Out One Shape For Each Rating")]
    public async Task RatingViewLaysOutOneShapeForEachRating()
    {
        var rating = new RatingView
        {
            MaximumRating = 5,
            HorizontalOptions = LayoutOptions.Start,
        };

        await RunInWindowAsync(rating, window =>
        {
            var shapes = RatingShapes(rating);
            Assert.Equal(5, shapes.Count);

            var bounds = shapes.Select(shape => BoundsInWindow(window, shape)).ToList();
            Assert.All(bounds, b => Assert.True(b.Width > 0 && b.Height > 0));
            for (var i = 1; i < bounds.Count; i++)
                Assert.True(bounds[i].Left >= bounds[i - 1].Right, "The rating shapes overlap.");

            rating.MaximumRating = 3;
            window.Platform.UpdateLayout();

            Assert.Equal(3, RatingShapes(rating).Count);
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "RatingView Sets The Rating When A Shape Is Clicked")]
    public async Task RatingViewSetsRatingWhenShapeIsClicked()
    {
        var rating = new RatingView
        {
            MaximumRating = 5,
            FillColor = Colors.Gold,
            EmptyShapeColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Start,
        };
        var changes = new List<double>();
        rating.RatingChanged += (_, e) => changes.Add(e.Rating);

        await RunInWindowAsync(rating, async window =>
        {
            var shapes = RatingShapes(rating);

            Click(window, shapes[2]);
            await WaitUntilAsync(window, () => rating.Rating == 3, message: $"The rating is {rating.Rating} after clicking the third shape.");

            Assert.Equal([3d], changes);
            Assert.Equal(Colors.Gold, ShapeFill(shapes[0]));
            Assert.Equal(Colors.Gold, ShapeFill(shapes[2]));
            Assert.Equal(Colors.Gray, ShapeFill(shapes[3]));
        });
    }

    [AvaloniaFact(DisplayName = "Read-only RatingView Ignores Clicks")]
    public async Task ReadOnlyRatingViewIgnoresClicks()
    {
        var rating = new RatingView
        {
            MaximumRating = 5,
            Rating = 1,
            IsReadOnly = true,
            HorizontalOptions = LayoutOptions.Start,
        };

        await RunInWindowAsync(rating, window =>
        {
            Click(window, RatingShapes(rating)[4]);
            window.Platform.UpdateLayout();

            Assert.Equal(1, rating.Rating);
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "LazyView Shows Its View After Loading")]
    public async Task LazyViewShowsViewAfterLoading()
    {
        var lazyView = new LazyView<LazyContent> { HorizontalOptions = LayoutOptions.Fill };
        var below = new BoxView { HeightRequest = 10 };
        var page = new VerticalStackLayout { Children = { lazyView, below } };

        await RunInWindowAsync(page, async window =>
        {
            Assert.False(lazyView.HasLazyViewLoaded);
            var topBefore = BoundsInWindow(window, below).Top;

            await lazyView.LoadViewAsync();
            await WaitUntilAsync(window, () => lazyView.Content is LazyContent content && content.Handler is not null,
                message: "The LazyView did not show its view.");

            Assert.True(lazyView.HasLazyViewLoaded);
            var content = (LazyContent)lazyView.Content;
            Assert.Equal(40, BoundsInWindow(window, content).Height, 1);
            Assert.Equal(topBefore + 40, BoundsInWindow(window, below).Top, 1);
        });
    }

    static IReadOnlyList<ControlsBorder> RatingShapes(RatingView rating) =>
        ((IVisualTreeElement)rating).GetVisualTreeDescendants().OfType<ControlsBorder>().ToList();

    static Color? ShapeFill(ControlsBorder shape) =>
        (((IVisualTreeElement)shape).GetVisualTreeDescendants().OfType<Microsoft.Maui.Controls.Shapes.Path>().Single().Fill as SolidColorBrush)?.Color;

    public class LazyContent : BoxView
    {
        public LazyContent()
        {
            HeightRequest = 40;
            Color = Colors.Teal;
        }
    }
}
