using System;
using Microsoft.Maui.Controls;

namespace SandboxApp;

public partial class MainPage : ContentPage
{
    int count = 0;
    public MainPage()
    {
        InitializeComponent();
    }

    private void CounterBtn_Clicked(object sender, EventArgs e)
    {
        count++;
        if (count == 1)
            CounterLabel.Text = $"Clicked {count} time";
        else
            CounterLabel.Text = $"Clicked {count} times";

        CounterLabel.FontSize = 24;
    }
}
