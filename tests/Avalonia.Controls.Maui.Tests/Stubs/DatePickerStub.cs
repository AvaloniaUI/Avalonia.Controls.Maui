using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class DatePickerStub : StubBase, IDatePicker
{
    public DateTime? Date { get; set; } = DateTime.Today;

    public DateTime? MinimumDate { get; set; }

    public DateTime? MaximumDate { get; set; }

    public string Format { get; set; } = "D";

    public MauiGraphics.Color TextColor { get; set; } = null!;

    public Microsoft.Maui.Font Font { get; set; }

    public double CharacterSpacing { get; set; }
}
