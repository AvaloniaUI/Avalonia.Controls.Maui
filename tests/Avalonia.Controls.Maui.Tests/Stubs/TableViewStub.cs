using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;

namespace Avalonia.Controls.Maui.Tests.Stubs;

// Inside the namespace so it wins over Avalonia.Controls.TableView (added in Avalonia 12.1).
using TableView = Microsoft.Maui.Controls.TableView;

/// <summary>
/// TableView test stub implementing minimal requirements for handler testing.
/// </summary>
public class TableViewStub : TableView
{
    public TableViewStub() : base()
    {
        // Set explicit sizes to avoid NaN measurement issues in tests
        WidthRequest = 400;
        HeightRequest = 600;
    }
}
