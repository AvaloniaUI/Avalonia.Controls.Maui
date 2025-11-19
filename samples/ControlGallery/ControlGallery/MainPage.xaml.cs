using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ControlGallery;

public partial class MainPage : FlyoutPage
{
    public ICommand NavigateCommand { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        NavigateCommand = new Command<Type>(
            async (Type pageType) =>
            {
                Page page = (Page)Activator.CreateInstance(pageType)!;
                this.Detail = page;
            });

        this.FlyoutPageMenu.BindingContext = this;
    }
}
