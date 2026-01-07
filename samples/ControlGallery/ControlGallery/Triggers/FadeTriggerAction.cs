using Microsoft.Maui.Controls;

namespace ControlGallery.Triggers
{
    public class FadeTriggerAction : TriggerAction<VisualElement>
    {
        public int StartsFrom { get; set; } = 0;

        protected override void Invoke(VisualElement sender)
        {
            sender.Animate("FadeTriggerAction", new Animation((d) =>
            {
                var val = StartsFrom == 1 ? d : 1 - d;
                sender.Opacity = val;
            }),
            length: 1000, // milliseconds
            easing: Easing.Linear);
        }
    }
}
