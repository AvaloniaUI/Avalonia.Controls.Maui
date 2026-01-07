using Microsoft.Maui.Controls;
using System;

namespace ControlGallery.Pages
{
    public partial class VisualStateManagerPage : ContentPage
    {
        public VisualStateManagerPage()
        {
            InitializeComponent();
        }

        private void OnNeutralClicked(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(StatefulBox, "Neutral");
        }

        private void OnSuccessClicked(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(StatefulBox, "Success");
        }

        private void OnErrorClicked(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(StatefulBox, "Error");
        }
    }
}
