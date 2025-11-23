namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Indicates that a method, property, or class has a pending or incomplete implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
public sealed class NotImplementedAttribute : Attribute
{
    public NotImplementedAttribute() { }
    
    public NotImplementedAttribute(string reason)
    {
        Reason = reason;
    }

    public string? Reason { get; }
    public string? IssueId { get; set; }
    public string? DependsOn { get; set; }
}