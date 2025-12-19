using Microsoft.Maui.Controls.Shapes;

namespace ControlGallery.Pages;

public partial class BorderPage : ContentPage
{
    private bool _isDashAnimating;
    private CancellationTokenSource? _dashAnimationCts;

    public BorderPage()
    {
        InitializeComponent();
    }

    private void OnCornerSliderChanged(object sender, ValueChangedEventArgs e)
    {
        UpdateCornerRadius();
    }

    private void OnSetAllCornersClicked(object sender, EventArgs e)
    {
        TopLeftSlider.Value = 30;
        TopRightSlider.Value = 30;
        BottomRightSlider.Value = 30;
        BottomLeftSlider.Value = 30;
    }

    private void UpdateCornerRadius()
    {
        var topLeft = (int)TopLeftSlider.Value;
        var topRight = (int)TopRightSlider.Value;
        var bottomRight = (int)BottomRightSlider.Value;
        var bottomLeft = (int)BottomLeftSlider.Value;

        TopLeftValue.Text = topLeft.ToString();
        TopRightValue.Text = topRight.ToString();
        BottomRightValue.Text = bottomRight.ToString();
        BottomLeftValue.Text = bottomLeft.ToString();

        InteractiveRoundRect.CornerRadius = new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
    }

    private void OnToggleDashAnimationClicked(object sender, EventArgs e)
    {
        if (_isDashAnimating)
        {
            StopDashAnimation();
        }
        else
        {
            StartDashAnimation();
        }
    }

    private async void StartDashAnimation()
    {
        _isDashAnimating = true;
        _dashAnimationCts = new CancellationTokenSource();
        DashAnimationButton.Text = "Stop Dash Loop";

        try
        {
            float offset = 0;
            while (!_dashAnimationCts.Token.IsCancellationRequested)
            {
                AnimatedBorder.StrokeDashOffset = offset;
                offset += 1;
                if (offset > 12) offset = 0;
                await Task.Delay(50, _dashAnimationCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void StopDashAnimation()
    {
        _isDashAnimating = false;
        _dashAnimationCts?.Cancel();
        _dashAnimationCts = null;
        DashAnimationButton.Text = "Start Dash Loop";
        AnimatedBorder.StrokeDashOffset = 0;
    }

    private async void OnPulseColorClicked(object sender, EventArgs e)
    {
        var originalColor = AnimatedBorder.Stroke;
        var colors = new[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Purple };
        
        foreach (var color in colors)
        {
            AnimatedBorder.Stroke = new SolidColorBrush(color);
            await Task.Delay(200);
        }
        
        AnimatedBorder.Stroke = originalColor;
    }

    private async void OnAnimateThicknessClicked(object sender, EventArgs e)
    {
        for (int i = 2; i <= 12; i += 2)
        {
            AnimatedBorder.StrokeThickness = i;
            await Task.Delay(100);
        }
        
        for (int i = 12; i >= 4; i -= 2)
        {
            AnimatedBorder.StrokeThickness = i;
            await Task.Delay(100);
        }
    }

    private void OnShapeRectangleClicked(object sender, EventArgs e)
    {
        ShapeSwitchBorder.StrokeShape = null;
        ShapeLabel.Text = "Rectangle";
    }

    private void OnShapeRoundRect10Clicked(object sender, EventArgs e)
    {
        ShapeSwitchBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) };
        ShapeLabel.Text = "RoundRect (10)";
    }

    private void OnShapeRoundRect30Clicked(object sender, EventArgs e)
    {
        ShapeSwitchBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(30) };
        ShapeLabel.Text = "RoundRect (30)";
    }

    private void OnShapePillClicked(object sender, EventArgs e)
    {
        ShapeSwitchBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(60) };
        ShapeLabel.Text = "Pill Shape";
    }

    private void OnShapeAsymmetricClicked(object sender, EventArgs e)
    {
        ShapeSwitchBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(40, 10, 40, 10) };
        ShapeLabel.Text = "Asymmetric";
    }

    private void OnShapeEllipseClicked(object sender, EventArgs e)
    {
        ShapeSwitchBorder.StrokeShape = new Ellipse();
        ShapeLabel.Text = "Ellipse";
    }
}
