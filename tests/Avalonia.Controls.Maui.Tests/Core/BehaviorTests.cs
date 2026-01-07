using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using MCEntry = Microsoft.Maui.Controls.Entry;
using MCBehavior = Microsoft.Maui.Controls.Behavior;
using MCBehaviorT = Microsoft.Maui.Controls.Behavior<Microsoft.Maui.Controls.Entry>;
using MCColors = Microsoft.Maui.Graphics.Colors;
using MCTextChangedEventArgs = Microsoft.Maui.Controls.TextChangedEventArgs;

namespace Avalonia.Controls.Maui.Tests.Core;

public class BehaviorTests
{
    [AvaloniaFact(DisplayName = "Behavior attached to Entry modifies property")]
    public void Behavior_Attached_To_Entry_Modifies_Property()
    {
        var entry = new MCEntry();
        var behavior = new TestPropertyBehavior { TargetValue = "Modified by Behavior" };
        
        entry.Behaviors.Add(behavior);
        
        Assert.Equal("Modified by Behavior", entry.Text);
    }

    [AvaloniaFact(DisplayName = "Behavior reacts to property changes")]
    public void Behavior_Reacts_To_Property_Changes()
    {
        var entry = new MCEntry();
        var behavior = new ValidationBehavior();
        entry.Behaviors.Add(behavior);

        entry.Text = "Invalid";
        Assert.Equal(MCColors.Red, entry.TextColor);

        entry.Text = "Valid";
        Assert.Equal(MCColors.Green, entry.TextColor);
    }

    private class TestPropertyBehavior : MCBehaviorT
    {
        public string TargetValue { get; set; } = "";

        protected override void OnAttachedTo(MCEntry bindable)
        {
            bindable.Text = TargetValue;
            base.OnAttachedTo(bindable);
        }
    }

    private class ValidationBehavior : MCBehaviorT
    {
        protected override void OnAttachedTo(MCEntry bindable)
        {
            bindable.TextChanged += OnTextChanged;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(MCEntry bindable)
        {
            bindable.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(bindable);
        }

        private void OnTextChanged(object? sender, MCTextChangedEventArgs e)
        {
            if (sender is MCEntry entry)
            {
                entry.TextColor = e.NewTextValue == "Valid" 
                    ? MCColors.Green 
                    : MCColors.Red;
            }
        }
    }
}
