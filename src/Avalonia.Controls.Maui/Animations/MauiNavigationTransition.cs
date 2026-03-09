using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui.Animations;

/// <summary>
/// Page transition effect.
/// </summary>
public class MauiNavigationTransition : IPageTransition
{
    private const double ParallaxRatio = 0.33;
    private const double DimOpacity = 0.8;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiNavigationTransition"/> class.
    /// </summary>
    public MauiNavigationTransition()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiNavigationTransition"/> class.
    /// </summary>
    /// <param name="duration">The duration of the animation.</param>
    public MauiNavigationTransition(TimeSpan duration)
    {
        Duration = duration;
    }

    /// <summary>
    /// Gets or sets the duration of the animation.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(350);

    /// <summary>
    /// Gets or sets the easing function for the transition.
    /// </summary>
    public Easing Easing { get; set; } = new SplineEasing(0.25, 0.1, 0.25, 1.0);

    /// <inheritdoc />
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var tasks = new List<Task>();
        var parent = GetVisualParent(from, to);
        var distance = parent.Bounds.Width > 0 ? parent.Bounds.Width : 500d;

        if (from != null)
        {
            // Push: old page shifts left ~33% and dims slightly
            // Pop: old page slides fully off-screen to the right
            var fromEndX = forward ? -distance * ParallaxRatio : distance;
            var fromEndOpacity = forward ? DimOpacity : 1d;

            var anim = new Animation.Animation
            {
                FillMode = FillMode.Forward,
                Easing = Easing,
                Duration = Duration,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(Visual.OpacityProperty, 1d)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, fromEndX),
                            new Setter(Visual.OpacityProperty, fromEndOpacity)
                        }
                    }
                }
            };
            tasks.Add(anim.RunAsync(from, cancellationToken));
        }

        if (to != null)
        {
            to.IsVisible = true;

            // Push: new page slides in from the right edge
            // Pop: new page slides back from -33% offset to center
            var toStartX = forward ? distance : -distance * ParallaxRatio;
            var toStartOpacity = forward ? 1d : DimOpacity;

            var anim = new Animation.Animation
            {
                FillMode = FillMode.Forward,
                Easing = Easing,
                Duration = Duration,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, toStartX),
                            new Setter(Visual.OpacityProperty, toStartOpacity)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(Visual.OpacityProperty, 1d)
                        }
                    }
                }
            };
            tasks.Add(anim.RunAsync(to, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (cancellationToken.IsCancellationRequested)
            return;

        if (from != null)
        {
            from.IsVisible = false;
            from.RenderTransform = null;
            from.Opacity = 1d;
        }

        if (to != null)
        {
            to.RenderTransform = null;
        }
    }

    private static Visual GetVisualParent(Visual? from, Visual? to)
    {
        var p1 = (from ?? to)!.GetVisualParent();
        var p2 = (to ?? from)!.GetVisualParent();

        if (p1 != null && p2 != null && p1 != p2)
            throw new ArgumentException("Controls for MauiNavigationTransition must have the same parent.");

        return p1 ?? throw new InvalidOperationException("Cannot determine visual parent.");
    }
}
