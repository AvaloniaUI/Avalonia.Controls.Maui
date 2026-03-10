using System.Collections.Generic;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

// When Avalonia.Controls.PipsPager ships, remove this file entirely.
namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Automation peer for <see cref="PipsPager"/> that exposes page-selection state to assistive technologies.
/// </summary>
public class PipsPagerAutomationPeer : ControlAutomationPeer, ISelectionProvider
{
    /// <summary>Initializes a new instance of <see cref="PipsPagerAutomationPeer"/>.</summary>
    /// <param name="owner">The <see cref="PipsPager"/> that owns this peer.</param>
    public PipsPagerAutomationPeer(PipsPager owner) : base(owner)
    {
        owner.SelectedIndexChanged += OnSelectionChanged;
    }

    private new PipsPager Owner => (PipsPager)base.Owner;

    /// <inheritdoc/>
    public bool CanSelectMultiple => false;

    /// <inheritdoc/>
    public bool IsSelectionRequired => true;

    /// <inheritdoc/>
    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        var result = new List<AutomationPeer>();
        var owner = Owner;

        if (owner.SelectedPageIndex >= 0 && owner.SelectedPageIndex < owner.NumberOfPages)
        {
            var pipsList = owner.FindNameScope()?.Find<ItemsControl>(PipsPager.PipsPagerListPartName);
            if (pipsList != null)
            {
                var container = pipsList.ContainerFromIndex(owner.SelectedPageIndex);
                if (container is Control c)
                    result.Add(GetOrCreate(c));
            }
        }

        return result;
    }

    /// <inheritdoc/>
    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.List;

    /// <inheritdoc/>
    protected override string GetClassNameCore() => nameof(PipsPager);

    /// <inheritdoc/>
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Pips Pager" : name;
    }

    private void OnSelectionChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        RaisePropertyChangedEvent(
            SelectionPatternIdentifiers.SelectionProperty,
            e.OldIndex,
            e.NewIndex);
    }
}
