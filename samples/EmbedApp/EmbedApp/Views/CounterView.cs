namespace EmbedApp.Views;

public class CounterView : Microsoft.Maui.Controls.View
{
    public static readonly BindableProperty CountProperty =
        BindableProperty.Create(nameof(Count), typeof(int), typeof(CounterView), 0);

    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
}
