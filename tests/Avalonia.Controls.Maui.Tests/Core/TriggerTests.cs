using Avalonia.Headless.XUnit;
using MCEntry = Microsoft.Maui.Controls.Entry;
using MCLabel = Microsoft.Maui.Controls.Label;
using MCButton = Microsoft.Maui.Controls.Button;
using MCTrigger = Microsoft.Maui.Controls.Trigger;
using MCDataTrigger = Microsoft.Maui.Controls.DataTrigger;
using MCEventTrigger = Microsoft.Maui.Controls.EventTrigger;
using MCSetter = Microsoft.Maui.Controls.Setter;
using MCTriggerAction = Microsoft.Maui.Controls.TriggerAction<Microsoft.Maui.Controls.VisualElement>;
using MCVisualElement = Microsoft.Maui.Controls.VisualElement;
using MCBinding = Microsoft.Maui.Controls.Binding;
using MCColors = Microsoft.Maui.Graphics.Colors;
using MCIButtonController = Microsoft.Maui.Controls.IButtonController;

namespace Avalonia.Controls.Maui.Tests.Core;

public class TriggerTests
{
    [AvaloniaFact(DisplayName = "Property Trigger applies setter when condition met")]
    public void PropertyTrigger_Applies_Setter_When_Condition_Met()
    {
        var entry = new MCEntry();
        var trigger = new MCTrigger(typeof(MCEntry))
        {
            Property = MCEntry.TextProperty,
            Value = "TriggerMe"
        };
        trigger.Setters.Add(new MCSetter { Property = MCEntry.TextColorProperty, Value = MCColors.Red });
        entry.Triggers.Add(trigger);

        entry.Text = "TriggerMe";
        Assert.Equal(MCColors.Red, entry.TextColor);

        entry.Text = "Normal";
        Assert.NotEqual(MCColors.Red, entry.TextColor);
    }

    [AvaloniaFact(DisplayName = "Data Trigger applies setter when binding condition met")]
    public void DataTrigger_Applies_Setter_When_Binding_Condition_Met()
    {
        var label = new MCLabel { BindingContext = new { IsActive = true } };
        var trigger = new MCDataTrigger(typeof(MCLabel))
        {
            Binding = new MCBinding("IsActive"),
            Value = true
        };
        trigger.Setters.Add(new MCSetter { Property = MCLabel.TextProperty, Value = "Active" });
        label.Triggers.Add(trigger);

        // Initial application
        Assert.Equal("Active", label.Text);

        // Change context
        label.BindingContext = new { IsActive = false };
        Assert.NotEqual("Active", label.Text);
    }

    [AvaloniaFact(DisplayName = "Event Trigger executes action")]
    public void EventTrigger_Executes_Action()
    {
        var button = new MCButton();
        var trigger = new MCEventTrigger { Event = "Clicked" };
        var action = new TestTriggerAction();
        trigger.Actions.Add(action);
        button.Triggers.Add(trigger);

        // Explicitly cast to IButtonController to access SendClicked if it's an explicit implementation
        ((MCIButtonController)button).SendClicked();

        Assert.True(action.Invoked);
    }

    private class TestTriggerAction : MCTriggerAction
    {
        public bool Invoked { get; private set; }

        protected override void Invoke(MCVisualElement sender)
        {
            Invoked = true;
        }
    }
}
