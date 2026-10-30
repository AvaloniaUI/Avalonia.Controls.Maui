using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Maui.Animations;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ControlsEntry = Microsoft.Maui.Controls.Entry;
using ControlsButton = Microsoft.Maui.Controls.Button;
using ControlsProgressBar = Microsoft.Maui.Controls.ProgressBar;

namespace Avalonia.Controls.Maui.Tests.CommunityToolkit;

public class ToolkitBehaviorTests : ToolkitTestBase
{
    [AvaloniaFact(DisplayName = "TextValidationBehavior Validates What The User Types")]
    public async Task TextValidationBehaviorValidatesTyping()
    {
        var validation = new TextValidationBehavior
        {
            MinimumLength = 3,
            Flags = global::CommunityToolkit.Maui.Behaviors.ValidationFlags.ValidateOnValueChanged,
            InvalidStyle = TextColorStyle(Colors.Red),
            ValidStyle = TextColorStyle(Colors.Green),
        };
        var entry = new ControlsEntry();
        entry.Behaviors.Add(validation);

        await RunInWindowAsync(entry, async window =>
        {
            Type(window, entry, "ab");
            await WaitUntilAsync(window, () => validation.IsNotValid && entry.TextColor == Colors.Red,
                message: "Two characters were not reported as invalid.");
            Assert.Equal(Colors.Red, PlatformTextColor(entry));

            Type(window, entry, "c");
            await WaitUntilAsync(window, () => validation.IsValid && entry.TextColor == Colors.Green,
                message: "Three characters were not reported as valid.");
            Assert.Equal(Colors.Green, PlatformTextColor(entry));
        });
    }

    [AvaloniaFact(DisplayName = "EmailValidationBehavior Validates What The User Types")]
    public async Task EmailValidationBehaviorValidatesTyping()
    {
        var validation = new EmailValidationBehavior { Flags = global::CommunityToolkit.Maui.Behaviors.ValidationFlags.ValidateOnValueChanged };
        var entry = new ControlsEntry();
        entry.Behaviors.Add(validation);

        await RunInWindowAsync(entry, async window =>
        {
            Type(window, entry, "name@");
            await WaitUntilAsync(window, () => validation.IsNotValid, message: "A partial email address was not reported as invalid.");

            Type(window, entry, "example.com");
            await WaitUntilAsync(window, () => validation.IsValid, message: "A full email address was not reported as valid.");
        });
    }

    [AvaloniaFact(DisplayName = "MaskedBehavior Formats What The User Types")]
    public async Task MaskedBehaviorFormatsTyping()
    {
        var entry = new ControlsEntry();
        entry.Behaviors.Add(new MaskedBehavior { Mask = "(XXX) XXX-XXXX" });

        await RunInWindowAsync(entry, async window =>
        {
            Type(window, entry, "5551234567");

            await WaitUntilAsync(window, () => entry.Text == "(555) 123-4567", message: $"The entry shows '{entry.Text}'.");
            await WaitUntilAsync(window, () => PlatformTextBox(entry).Text == "(555) 123-4567",
                message: $"The text box shows '{PlatformTextBox(entry).Text}'.");
        });
    }

    [AvaloniaFact(DisplayName = "MaxLengthReachedBehavior Runs Its Command At The Maximum Length")]
    public async Task MaxLengthReachedBehaviorRunsCommandAtMaximumLength()
    {
        string? reached = null;
        var entry = new ControlsEntry { MaxLength = 4 };
        entry.Behaviors.Add(new MaxLengthReachedBehavior { Command = new Command<string>(text => reached = text) });

        await RunInWindowAsync(entry, async window =>
        {
            Type(window, entry, "abc");
            await WaitUntilAsync(window, () => entry.Text == "abc");
            Assert.Null(reached);

            Type(window, entry, "d");
            await WaitUntilAsync(window, () => reached is not null, message: "The command did not run at the maximum length.");
            Assert.Equal("abcd", reached);
        });
    }

    [AvaloniaFact(DisplayName = "UserStoppedTypingBehavior Runs Its Command After The User Stops Typing")]
    public async Task UserStoppedTypingBehaviorRunsCommandAfterTyping()
    {
        string? stopped = null;
        var entry = new ControlsEntry();
        entry.Behaviors.Add(new UserStoppedTypingBehavior
        {
            // Long enough that typing finishes first, even while the test is still being compiled
            StoppedTypingTimeThreshold = 1000,
            Command = new Command<string>(text => stopped = text),
        });

        await RunInWindowAsync(entry, async window =>
        {
            Type(window, entry, "hello");
            Assert.Null(stopped);

            await WaitUntilAsync(window, () => stopped is not null, message: "The command did not run after typing stopped.");
            Assert.Equal("hello", stopped);
        });
    }

    [AvaloniaFact(DisplayName = "SetFocusOnEntryCompletedBehavior Moves Focus When Enter Is Pressed")]
    public async Task SetFocusOnEntryCompletedBehaviorMovesFocusOnEnter()
    {
        var first = new ControlsEntry();
        var second = new ControlsEntry();
        SetFocusOnEntryCompletedBehavior.SetNextElement(first, second);
        var page = new VerticalStackLayout { Children = { first, second } };

        await RunInWindowAsync(page, async window =>
        {
            Type(window, first, "done");
            window.Platform.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            window.Platform.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);

            await WaitUntilAsync(window, () => second.IsFocused, message: "The second entry did not get focus.");
            Assert.True(PlatformTextBox(second).IsFocused);
        });
    }

    [AvaloniaFact(DisplayName = "EventToCommandBehavior Runs Its Command When The Event Is Raised")]
    public async Task EventToCommandBehaviorRunsCommandForEvent()
    {
        var clicks = 0;
        var button = new ControlsButton { Text = "Click", HorizontalOptions = LayoutOptions.Start };
        button.Behaviors.Add(new EventToCommandBehavior
        {
            EventName = nameof(ControlsButton.Clicked),
            Command = new Command(() => clicks++),
        });

        await RunInWindowAsync(button, async window =>
        {
            Click(window, button);

            await WaitUntilAsync(window, () => clicks == 1, message: "The command did not run when the button was clicked.");
        });
    }

    [AvaloniaFact(DisplayName = "AnimationBehavior Fades A View When It Is Clicked")]
    public async Task AnimationBehaviorFadesViewWhenClicked()
    {
        var button = new ControlsButton { Text = "Fade", HorizontalOptions = LayoutOptions.Start };
        button.Behaviors.Add(new AnimationBehavior
        {
            EventName = nameof(ControlsButton.Clicked),
            AnimationType = new FadeAnimation { Opacity = 0.2, Length = 150 },
        });

        await RunInWindowAsync(button, async window =>
        {
            var platformButton = PlatformViewOf(button);
            var lowest = 1d;

            Click(window, button);
            await WaitUntilAsync(window, () =>
            {
                lowest = Math.Min(lowest, platformButton.Opacity);
                return lowest < 0.5 && button.Opacity == 1;
            }, message: $"The button did not fade out and back in. Lowest opacity: {lowest}.");

            Assert.Equal(1, platformButton.Opacity, 2);
        });
    }

    [AvaloniaFact(DisplayName = "ProgressBarAnimationBehavior Animates The Progress")]
    public async Task ProgressBarAnimationBehaviorAnimatesProgress()
    {
        var animation = new ProgressBarAnimationBehavior { Length = 200 };
        var progressBar = new ControlsProgressBar();
        progressBar.Behaviors.Add(animation);
        var completed = false;
        animation.AnimationCompleted += (_, _) => completed = true;

        await RunInWindowAsync(progressBar, async window =>
        {
            var platformProgressBar = Assert.IsAssignableFrom<Avalonia.Controls.ProgressBar>(
                PlatformViewOf(progressBar).GetSelfAndVisualDescendants().OfType<Avalonia.Controls.ProgressBar>().First());
            var sawPartialValue = false;

            animation.Progress = 1;
            await WaitUntilAsync(window, () =>
            {
                var fraction = (platformProgressBar.Value - platformProgressBar.Minimum) / (platformProgressBar.Maximum - platformProgressBar.Minimum);
                sawPartialValue |= fraction is > 0 and < 1;
                return completed;
            }, message: "The progress animation did not complete.");

            Assert.True(sawPartialValue, "The progress jumped to the end without animating.");
            Assert.Equal(1, progressBar.Progress, 2);
            Assert.Equal(platformProgressBar.Maximum, platformProgressBar.Value, 2);
        });
    }

    static Style TextColorStyle(Color color) => new(typeof(ControlsEntry))
    {
        Setters = { new Setter { Property = ControlsEntry.TextColorProperty, Value = color } },
    };

    static TextBox PlatformTextBox(ControlsEntry entry) =>
        PlatformViewOf(entry).GetSelfAndVisualDescendants().OfType<TextBox>().First();

    static Color? PlatformTextColor(ControlsEntry entry) =>
        PlatformTextBox(entry).Foreground is Avalonia.Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;

    /// <summary>
    /// Focuses the entry's text box and types <paramref name="text"/> into it one character at a time.
    /// </summary>
    static void Type(ToolkitWindow window, ControlsEntry entry, string text)
    {
        var textBox = PlatformTextBox(entry);
        if (!textBox.IsFocused)
        {
            textBox.Focus();
            textBox.CaretIndex = textBox.Text?.Length ?? 0;
        }

        foreach (var character in text)
        {
            window.Platform.KeyTextInput(character.ToString());
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }
    }
}
