using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;

namespace Avalonia.Controls.Maui.Tests.Stubs;

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
