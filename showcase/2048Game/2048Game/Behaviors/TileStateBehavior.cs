namespace _2048Game.Behaviors
{
    /// <summary>
    /// Legacy behavior - animations are now handled directly in MainPage.xaml.cs
    /// Kept for backwards compatibility in case referenced elsewhere.
    /// </summary>
    public class TileStateBehavior : Behavior<Border>
    {
        protected override void OnAttachedTo(Border bindable)
        {
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(Border bindable)
        {
            base.OnDetachingFrom(bindable);
        }
    }
}
