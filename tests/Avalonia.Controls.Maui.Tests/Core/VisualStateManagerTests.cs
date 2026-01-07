using Avalonia.Headless.XUnit;
using MCButton = Microsoft.Maui.Controls.Button;
using MCVisualStateManager = Microsoft.Maui.Controls.VisualStateManager;
using MCVisualStateGroupList = Microsoft.Maui.Controls.VisualStateGroupList;
using MCVisualStateGroup = Microsoft.Maui.Controls.VisualStateGroup;
using MCVisualState = Microsoft.Maui.Controls.VisualState;
using MCSetter = Microsoft.Maui.Controls.Setter;
using MCColors = Microsoft.Maui.Graphics.Colors;

namespace Avalonia.Controls.Maui.Tests.Core;

public class VisualStateManagerTests
{
    [AvaloniaFact(DisplayName = "GoToState applies setters for specific state")]
    public void GoToState_Applies_Setters_For_Specific_State()
    {
        var button = new MCButton();
        var groups = new MCVisualStateGroupList();
        var commonGroup = new MCVisualStateGroup { Name = "CommonStates" };
        
        var normalState = new MCVisualState { Name = "Normal" };
        normalState.Setters.Add(new MCSetter { Property = MCButton.BackgroundColorProperty, Value = MCColors.Blue });
        
        var disabledState = new MCVisualState { Name = "Disabled" };
        disabledState.Setters.Add(new MCSetter { Property = MCButton.BackgroundColorProperty, Value = MCColors.Gray });
        
        commonGroup.States.Add(normalState);
        commonGroup.States.Add(disabledState);
        groups.Add(commonGroup);
        
        MCVisualStateManager.SetVisualStateGroups(button, groups);

        // Initial state application (auto-applied by logic usually, but let's force expected flow)
        MCVisualStateManager.GoToState(button, "Normal");
        Assert.Equal(MCColors.Blue, button.BackgroundColor);

        MCVisualStateManager.GoToState(button, "Disabled");
        Assert.Equal(MCColors.Gray, button.BackgroundColor);
    }
}
