using System.Diagnostics;
using _2048Game.Data;
using _2048Game.Enums;
using _2048Game.ViewModels;
using Microsoft.Maui.Controls;

namespace _2048Game;

public partial class MainPage : ContentPage
{
    private MainPageViewModel CurrentViewModel;

    public MainPage()
    {
        InitializeComponent();
        CurrentViewModel = (MainPageViewModel)BindingContext;
    }

    void MainPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var bindingContext = (MainPageViewModel)BindingContext;

        if (e.PropertyName == nameof(MainPageViewModel.State))
        {
            if((bindingContext.State == LevelState.GameOver))
            {
                //GameOverAnimation.IsAnimationEnabled = true;
            }
            else if((bindingContext.State == LevelState.Playing))
            {
                //GameOverAnimation.IsAnimationEnabled = false;
            }
        }
        else if(e.PropertyName == nameof(MainPageViewModel.AddedScore))
        {
            this.Dispatcher.DispatchAsync(async () =>
            {
                AddedScoreLabel.IsVisible = true;
                await AddedScoreLabel.TranslateToAsync(0, -40, 500, Easing.Linear);
                await AddedScoreLabel.TranslateToAsync(0, 0, 0, Easing.Linear);
                AddedScoreLabel.IsVisible = false;
            });
        }
    }
    void OnSwiped(object sender, SwipedEventArgs e)
    {
        switch (e.Direction)
        {
            case SwipeDirection.Left:
                // Handle the swipe
                break;
            case SwipeDirection.Right:
                // Handle the swipe
                break;
            case SwipeDirection.Up:
                // Handle the swipe
                break;
            case SwipeDirection.Down:
                // Handle the swipe
                break;
        }
    }

    private SwipeDirection? swipedDirection;
    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                HandleTouch(e.TotalX, e.TotalY);
                break;
            case GestureStatus.Completed:
                HandleTouchEnd(swipedDirection);
                break;
        }
    }

    private void HandleTouch(double eTotalX, double eTotalY)
    {
        swipedDirection = null;
        const int delta = 10;
        if (eTotalX > delta)
        {
            swipedDirection = SwipeDirection.Right;
        }
        else if (eTotalX < -delta)
        {
            swipedDirection = SwipeDirection.Left;
        }
        else if (eTotalY > delta)
        {
            swipedDirection = SwipeDirection.Down;
        }
        else if (eTotalY < -delta)
        {
            swipedDirection = SwipeDirection.Up;
        }
    }
    private void HandleTouchEnd(SwipeDirection? swiped)
    {
        if (swiped == null)
        {
            return;
        }
        var currentViewModel = (MainPageViewModel)BindingContext;
        switch (swiped)
        {
            case SwipeDirection.Right:
                currentViewModel.RightSwipeCommand.Execute(null);
                break;
            case SwipeDirection.Left:
                currentViewModel.LeftSwipeCommand.Execute(null);
                break;
            case SwipeDirection.Up:
                currentViewModel.UpSwipeCommand.Execute(null);
                break;
            case SwipeDirection.Down:
                currentViewModel.DownSwipeCommand.Execute(null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    async void UndoButton_Clicked(System.Object sender, System.EventArgs e)
    {
        // TODO: Implement undo functionality
    }

    private void GameContainer_SizeChanged(object sender, EventArgs e)
    {
        const double originalWidth = 340.0;
        const double originalHeight = 336.0;

        var containerWidth = GameContainer.Width;
        var containerHeight = GameContainer.Height;

        if (containerWidth <= 0 || containerHeight <= 0)
            return;

        // Calculate scale to fit while maintaining aspect ratio
        var scaleX = containerWidth / originalWidth;
        var scaleY = containerHeight / originalHeight;
        var scale = Math.Min(scaleX, scaleY);

        // Apply uniform scale to both borders
        GameBorder.Scale = scale;
        GameOverBorder.Scale = scale;
    }
}

