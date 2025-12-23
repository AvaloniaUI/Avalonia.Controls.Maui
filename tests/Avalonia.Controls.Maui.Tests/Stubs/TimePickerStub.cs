using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class TimePickerStub : StubBase, ITimePicker
{
    public TimeSpan? Time { get; set; } = TimeSpan.Zero;

    public string Format { get; set; } = "t";

    public double CharacterSpacing { get; set; }

    public Microsoft.Maui.Font Font { get; set; }

    public Color TextColor { get; set; } = null!;
}
