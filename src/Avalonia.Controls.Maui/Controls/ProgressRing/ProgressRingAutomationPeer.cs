using Avalonia.Automation.Peers;

namespace Avalonia.Controls.Maui
{
    /// <summary>
    /// Automation peer for ProgressRing to support screen readers and assistive technologies.
    /// </summary>
    public class ProgressRingAutomationPeer : RangeBaseAutomationPeer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressRingAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The ProgressRing control that owns this peer.</param>
        public ProgressRingAutomationPeer(ProgressRing owner) : base(owner)
        {
        }

        /// <summary>
        /// Gets the ProgressRing control that owns this peer.
        /// </summary>
        public new ProgressRing Owner => (ProgressRing)base.Owner;

        protected override string GetClassNameCore() => "ProgressRing";

        protected override AutomationControlType GetAutomationControlTypeCore() =>
            AutomationControlType.ProgressBar;

        protected override string GetNameCore()
        {
            var baseName = base.GetNameCore();
            
            if (!string.IsNullOrEmpty(baseName))
                return baseName;

            var ownerText = Owner.GetAccessibilityText();
            return !string.IsNullOrEmpty(ownerText) ? ownerText! : string.Empty;
        }
    }
}