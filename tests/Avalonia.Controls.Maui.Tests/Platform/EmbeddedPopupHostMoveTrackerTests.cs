using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;

namespace Avalonia.Controls.Maui.Tests.Platform;

public class EmbeddedPopupHostMoveTrackerTests
{
    [Fact]
    public void FirstPositionIsTheReference()
    {
        var tracker = new EmbeddedPopupHostMoveTracker();

        Assert.False(tracker.HasMoved(new Point(10, 200)));
        Assert.False(tracker.HasMoved(new Point(10, 200)));
    }

    [Fact]
    public void MovesUpToTheThresholdAreIgnored()
    {
        var tracker = new EmbeddedPopupHostMoveTracker();
        tracker.HasMoved(new Point(10, 200));

        Assert.False(tracker.HasMoved(new Point(10.5, 199.5)));
        Assert.False(tracker.HasMoved(new Point(10 + EmbeddedPopupHostMoveTracker.Threshold, 200)));
        Assert.False(tracker.HasMoved(new Point(10, 200 - EmbeddedPopupHostMoveTracker.Threshold)));
    }

    [Theory]
    [InlineData(10, 150)] // Scrolled down.
    [InlineData(10, 201.5)] // Scrolled up a little.
    [InlineData(40, 200)] // Moved horizontally.
    public void MovesBeyondTheThresholdOnEitherAxisAreDetected(double x, double y)
    {
        var tracker = new EmbeddedPopupHostMoveTracker();
        tracker.HasMoved(new Point(10, 200));

        Assert.True(tracker.HasMoved(new Point(x, y)));
    }

    [Fact]
    public void ComparesWithTheReferenceNotThePreviousPosition()
    {
        var tracker = new EmbeddedPopupHostMoveTracker();
        tracker.HasMoved(new Point(10, 200));

        // Small steps that each stay within the threshold still add up to a move.
        Assert.False(tracker.HasMoved(new Point(10, 199.4)));
        Assert.False(tracker.HasMoved(new Point(10, 199.2)));
        Assert.True(tracker.HasMoved(new Point(10, 198.8)));
    }

    [Fact]
    public void UnknownPositionIsNotAMoveAndKeepsTheReference()
    {
        var tracker = new EmbeddedPopupHostMoveTracker();

        Assert.False(tracker.HasMoved(null));
        Assert.False(tracker.HasMoved(new Point(10, 200)));
        Assert.False(tracker.HasMoved(null));
        Assert.True(tracker.HasMoved(new Point(10, 100)));
    }

    [Fact]
    public void ResetStartsFromTheNextPosition()
    {
        var tracker = new EmbeddedPopupHostMoveTracker();
        tracker.HasMoved(new Point(10, 200));

        tracker.Reset();

        // The popup reopened after the page scrolled: the new position is the reference.
        Assert.False(tracker.HasMoved(new Point(10, 100)));
        Assert.False(tracker.HasMoved(new Point(10, 100.5)));
        Assert.True(tracker.HasMoved(new Point(10, 50)));
    }
}
