# ProgressRing Control

## Abstract
Progress indication has been a fundamental requirement for modern cross-platform applications. This proposal provides a **ProgressRing** control that fills this gap, offering both indeterminate and determinate progress modes.
Vision

Details:
* Core properties: IsActive, IsIndeterminate, Value, Minimum, Maximum
* Inherits from RangeBase for standard range control patterns
* Follows established naming conventions from Windows ecosystem

### Requirements

* **Must have efficient rendering performance**. Progress indicators are often displayed during performance-critical operations. Inefficient rendering could compound performance issues. The control must use caching and optimized invalidation ensure minimal overhead.
* **Must support both indeterminate and determinate modes**. Real-world applications require both: indeterminate mode for unknown-duration operations (app startup, network requests) and determinate mode for trackable progress (file downloads, processing steps).
* **Must have accessibility support**. Progress indicators are critical feedback mechanisms. Screen reader users must receive equivalent information about loading states and progress.
* **Must be theme-aware and customizable**. he control must adapt to light/dark themes automatically and allow custom styling through templates.

## API

```
public class ProgressRing : RangeBase
{

}
```

```
/// <summary>
/// Represents a control that indicates indeterminate or determinate progress.
/// </summary>
public class ProgressRing : RangeBase
{
    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing is showing progress.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing shows indeterminate progress.
    /// </summary>
    public bool IsIndeterminate { get; set; }
    
    // Inherited from RangeBase
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
* **ProgressRing**: Main control (logic, properties, accessibility)
* **ProgressRingVisual**: Rendering component (geometry, animations, visual presentation)
* **ProgressRingAutomationPeer**: Automation peer for ProgressRing to support screen readers and assistive technologies.

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