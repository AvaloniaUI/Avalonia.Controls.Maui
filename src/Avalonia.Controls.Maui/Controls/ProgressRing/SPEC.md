# ProgressRing Control

## Abstract
Progress indication has been a fundamental requirement for modern cross-platform applications. This proposal provides a **ProgressRing** control that fills this gap, offering both indeterminate and determinate progress modes.

### Vision

Details:
* Core properties: IsActive, IsIndeterminate, Value, Minimum, Maximum
* Inherits from ProgressBar (which itself provides RangeBase) for standard range control patterns and built-in determinate/indeterminate plumbing
* Follows established naming conventions
* Indeterminate animation runs on the render/composition path (no UI-thread timer)

### Requirements

* **Must have efficient rendering performance**. Progress indicators are often displayed during performance-critical operations. Inefficient rendering could compound performance issues. The control must use caching and optimized invalidation ensure minimal overhead.
* **Must support both indeterminate and determinate modes**. Real-world applications require both: indeterminate mode for unknown-duration operations (app startup, network requests) and determinate mode for trackable progress (file downloads, processing steps).
* **Must have accessibility support**. Progress indicators are critical feedback mechanisms. Screen reader users must receive equivalent information about loading states and progress.
* **Must be theme-aware and customizable**. he control must adapt to light/dark themes automatically and allow custom styling through templates.
* **Animation must be render-thread friendly**. Indeterminate motion should not pause when the UI thread is busy; animations use Avalonia's render/compositor animation system, not a dispatcher timer.

## API

```
/// <summary>
/// Represents a control that indicates indeterminate or determinate progress.
/// </summary>
public class ProgressRing : ProgressBar
{
    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing is showing progress.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing shows indeterminate progress.
    /// </summary>
    public bool IsIndeterminate { get; set; }
    
    // Inherited from ProgressBar / RangeBase
    /// <summary>
    /// Gets or sets the current value (0-100 by default).
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum value of the range (default: 0).
    /// </summary>
    public double Minimum { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum value of the range (default: 100).
    /// </summary>
    public double Maximum { get; set; }
}
```

Implementation Details
* **ProgressRing**: Main control (inherits `ProgressBar`; logic, properties, accessibility)
* **ProgressRingVisual**: Custom rendering component (565 lines) providing precise control over progress ring visualization:
    * **Custom geometry rendering**: Uses `StreamGeometry` with `ArcTo` for pixel-perfect arc drawing with configurable start angle, sweep, and stroke properties
    * **Indeterminate animation**: Two coordinated keyframe animations using `Avalonia.Animation.Animation.RunAsync()`:
        - **Rotation**: Animates `RotationAngle` property (0→1080°, 3 full turns per cycle)
        - **Arc sweep**: Animates `ArcSweep` property (0→180° ease-out → hold → 180→0° ease-in) for visual interest
    * **Determinate mode**: Static arc with cached geometry, sweep angle calculated from `(Value - Minimum) / (Maximum - Minimum) × 360°`
    * **Performance optimizations**:
        - Properties marked with `AffectsRender` (not layout-affecting) for efficient invalidation
        - Geometry caching for determinate arcs to avoid recalculation
        - Animation lifecycle management with `CancellationTokenSource`
    * **IsActive lifecycle**: Stops animations cleanly by canceling token and resetting visual state
    * **RTL support**: Automatically mirrors rotation direction based on `FlowDirection`
* **ProgressRingAutomationPeer**: Automation peer for ProgressRing (derives from `ProgressBarAutomationPeer`) to support screen readers and assistive technologies

**Why Custom Visual vs Pure XAML**:
While a pure XAML approach would be conceptually simpler, Avalonia's `Arc` shape control doesn't support `RenderTransform` animations in `Style.Animations` blocks (results in "No animator registered" error). Custom geometry rendering via `ProgressRingVisual` provides necessary control for animated arc effects while maintaining proper animation integration through Avalonia's property system.

In this way:
* Easy visual customization through ControlTheme
* Testable business logic independent of rendering
* Potential for alternative visual implementations

## Usage Examples

Indeterminate (Loading)

```
<ProgressRing IsActive="True" />
```

Determinate (Progress)

```
<ProgressRing 
    IsActive="{Binding IsLoading}"
    IsIndeterminate="False"
    Value="{Binding DownloadProgress}" />
```

### Animation Lifecycle
* `IsActive=false` stops animations by canceling the `CancellationTokenSource`, which terminates both rotation and arc sweep animations. Properties reset to 0 for clean visual state.
* `IsIndeterminate=false` switches from animated mode to static determinate arc rendering with cached geometry based on `(Value-Minimum)/(Maximum-Minimum)`.
* `AnimationDuration` property (default 2s) controls the full indeterminate animation cycle. When changed while animating, the control cancels current animations, clears the animation cache, and restarts with the new duration.