namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Indicates that a method, property, or class has a pending or incomplete implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
public sealed class NotImplementedAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotImplementedAttribute"/> class with no reason specified.
    /// </summary>
    public NotImplementedAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotImplementedAttribute"/> class with a reason describing why the member is not implemented.
    /// </summary>
    /// <param name="reason">A description of why the member is not implemented.</param>
    public NotImplementedAttribute(string reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets the explanation of why the member is not implemented.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Gets or sets the tracking issue identifier associated with the incomplete implementation.
    /// </summary>
    public string? IssueId { get; set; }

    /// <summary>
    /// Gets or sets the dependency that blocks this implementation from being completed.
    /// </summary>
    public string? DependsOn { get; set; }
}