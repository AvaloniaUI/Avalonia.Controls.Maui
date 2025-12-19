# Avalonia .NET MAUI Backend - Implementation Tracking

The purpose of this document is to track the implementation status of the **.NET MAUI backend** for Avalonia UI Framework.

---

## ActivityIndicator

Uses an animation to show that the app is engaged in a lengthy activity, without giving any indication of progress.

### Properties

| Property | Status |
|----------|--------|
| Color | ✅ Implemented |
| IsRunning | ✅ Implemented |

---

## Border

A container control that draws a border, background, or both, around another control.

### Properties

| Property | Status |
|----------|--------|
| Content | ✅ Implemented |
| Padding | ✅ Implemented |
| StrokeShape | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |
| StrokeMiterLimit | ✅ Implemented |

---

## BoxView

Draws a rectangle or square, of a specified width, height, and color.

### Properties

| Property | Status |
|----------|--------|
| Color | ✅ Implemented |
| CornerRadius | ✅ Implemented |

---

## Button

Displays text and responds to a tap or click that directs the app to carry out a task.

### Properties

| Property | Status |
|----------|--------|
| BorderColor | ✅ Implemented |
| BorderWidth | ✅ Implemented |
| CharacterSpacing | ✅ Implemented |
| Command | ✅ Implemented |
| CommandParameter | ✅ Implemented |
| ContentLayout | ✅ Implemented |
| CornerRadius | ✅ Implemented |
| FontAttributes | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ✅ Implemented |
| ImageSource | ✅ Implemented |
| IsPressed | ⏳ TODO |
| LineBreakMode | ⏳ TODO |
| Padding | ✅ Implemented |
| Text | ✅ Implemented |
| TextColor | ✅ Implemented |
| TextTransform | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Clicked | ✅ Implemented |
| Pressed | ✅ Implemented |
| Released | ✅ Implemented |

---

## CarouselView

Displays a scrollable list of data items, where users swipe to move through the collection.

### Properties

| Property | Status |
|----------|--------|
| CurrentItem | ⏳ TODO |
| CurrentItemChangedCommand | ⏳ TODO |
| CurrentItemChangedCommandParameter | ⏳ TODO |
| IsBounceEnabled | ⏳ TODO |
| IsScrollAnimated | ⏳ TODO |
| IsSwipeEnabled | ⏳ TODO |
| ItemsSource | ⏳ TODO |
| ItemTemplate | ⏳ TODO |
| Loop | ⏳ TODO |
| PeekAreaInsets | ⏳ TODO |
| Position | ⏳ TODO |

---

## CheckBox

Enables you to select a boolean value using a type of button that can either be checked or empty.

### Properties

| Property | Status |
|----------|--------|
| IsChecked | ✅ Implemented |
| Color | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| CheckedChanged | ✅ Implemented |

---

## CollectionView

Displays a scrollable list of selectable data items, using different layout specifications.

### Properties

| Property | Status |
|----------|--------|
| EmptyView | ✅ Implemented |
| EmptyViewTemplate | ✅ Implemented |
| Footer | ✅ Implemented |
| FooterTemplate | ✅ Implemented |
| Header | ✅ Implemented |
| HeaderTemplate | ✅ Implemented |
| ItemsLayout | ✅ Implemented |
| ItemsSource | ✅ Implemented |
| ItemTemplate | ✅ Implemented |
| ItemsUpdatingScrollMode | ✅ Implemented |
| RemainingItemsThreshold | ✅ Implemented |
| RemainingItemsThresholdReachedCommand | ⏳ TODO |
| SelectedItem | ✅ Implemented |
| SelectedItems | ✅ Implemented |
| SelectionMode | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| RemainingItemsThresholdReached | ✅ Implemented |
| SelectionChanged | ✅ Implemented |

### Notes

- Supports LinearItemsLayout (Vertical/Horizontal) and GridItemsLayout
- Grouping supported via IsGrouped, GroupHeaderTemplate, GroupFooterTemplate
- Custom CollectionViewStackPanel ensures items fill cross-axis in horizontal layouts

---

## ContentPresenter

Layout manager for templated views.

### Properties

| Property | Status |
|----------|--------|
| Content | ✅ Implemented |

---

## ContentView

A control that enables the creation of custom, reusable controls.

### Properties

| Property | Status |
|----------|--------|
| Content | ✅ Implemented  |

---

## DatePicker

Enables you to select a date with the platform date picker.

### Properties

| Property | Status |
|----------|--------|
| MinimumDate | ⏳ TODO |
| MaximumDate | ⏳ TODO |
| Date | ⏳ TODO |
| Format | ⏳ TODO |
| TextColor | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| CharacterSpacing | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| DateSelected | ⏳ TODO |

---

## Editor

Enables you to enter and edit multiple lines of text.

### Properties

| Property | Status |
|----------|--------|
| AutoSize | ⏳ TODO |
| CharacterSpacing | ⏳ TODO |
| CursorPosition | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| HorizontalTextAlignment | ⏳ TODO |
| IsTextPredictionEnabled | ⏳ TODO |
| Placeholder | ⏳ TODO |
| PlaceholderColor | ⏳ TODO |
| SelectionLength | ⏳ TODO |
| Text | ⏳ TODO |
| TextColor | ⏳ TODO |
| VerticalTextAlignment | ⏳ TODO |
| IsReadOnly | ⏳ TODO |
| IsSpellCheckEnabled | ⏳ TODO |
| Keyboard | ⏳ TODO |
| MaxLength | ⏳ TODO |
| TextTransform | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| TextChanged | ⏳ TODO |
| Completed | ⏳ TODO |

---

## Ellipse

Displays an ellipse or circle.

### Properties

| Property | Status |
|----------|--------|
| Fill | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |
| StrokeMiterLimit | ⏳ TODO |

---

## Entry

Enables you to enter and edit a single line of text.

### Properties

| Property | Status |
|----------|--------|
| CharacterSpacing | ⏳ TODO |
| ClearButtonVisibility | ⏳ TODO |
| CursorPosition | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| Keyboard | ⏳ TODO |
| HorizontalTextAlignment | ⏳ TODO |
| IsPassword | ⏳ TODO |
| IsTextPredictionEnabled | ⏳ TODO |
| IsReadOnly | ⏳ TODO |
| IsSpellCheckEnabled | ⏳ TODO |
| MaxLength | ⏳ TODO |
| Placeholder | ⏳ TODO |
| PlaceholderColor | ⏳ TODO |
| ReturnCommand | ⏳ TODO |
| ReturnCommandParameter | ⏳ TODO |
| ReturnType | ⏳ TODO |
| SelectionLength | ⏳ TODO |
| Text | ⏳ TODO |
| TextColor | ⏳ TODO |
| TextTransform | ⏳ TODO |
| VerticalTextAlignment | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| TextChanged | ⏳ TODO |
| Completed | ⏳ TODO |
| Focused | ⏳ TODO |
| Unfocused | ⏳ TODO |

---

## EntryCell (Deprecated)

Displays an entry with a label and placeholder, for use in a TableView or ListView.

### Properties

| Property | Status |
|----------|--------|
| Label | ✅ Implemented |
| Text | ✅ Implemented |
| Placeholder | ✅ Implemented |
| LabelColor | ✅ Implemented |
| HorizontalTextAlignment | ✅ Implemented |
| VerticalTextAlignment | ✅ Implemented |
| Keyboard | ✅ Implemented |
| IsEnabled | ✅ Implemented |
| ContextActions | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Completed | ✅ Implemented |
| Appearing | ✅ Implemented |
| Disappearing | ✅ Implemented |

---

## Frame (Deprecated)

Used to wrap a view or layout with a border that can be configured with color, shadow, and other options.

### Properties

| Property | Status |
|----------|--------|
| BorderColor | ✅ Implemented |
| CornerRadius | ⏳ TODO |
| HasShadow | ⏳ TODO |
| Content | ⏳ TODO |
| Padding | ⏳ TODO |

---

## GraphicsView

A graphics canvas on which 2D graphics can be drawn using types from the Microsoft.Maui.Graphics namespace.

### Properties

| Property | Status |
|----------|--------|
| Drawable | ✅ Implemented |

---

## Image

Displays an image that can be loaded from a local file, a URI, an embedded resource, or a stream.

### Properties

| Property | Status |
|----------|--------|
| Source | ✅ Implemented |
| Aspect | ✅ Implemented |
| IsOpaque | ⏳ TODO |
| IsAnimationPlaying | ✅ Implemented |
| IsLoading | ✅ Implemented |

---

## ImageButton

Displays an image and responds to a tap or click that directs an app to carry out a task.

### Properties

| Property | Status |
|----------|--------|
| Source | ✅ Implemented |
| Aspect | ✅ Implemented |
| BorderColor | ✅ Implemented |
| BorderWidth | ✅ Implemented |
| Command | ✅ Implemented |
| CommandParameter | ✅ Implemented |
| CornerRadius | ✅ Implemented |
| Padding | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Clicked | ✅ Implemented |
| Pressed | ✅ Implemented |
| Released | ✅ Implemented |

---

## ImageCell (Deprecated)

Displays an image with text and detail text, for use in a TableView or ListView.

### Properties

| Property | Status |
|----------|--------|
| Text | ✅ Implemented |
| Detail | ✅ Implemented |
| TextColor | ✅ Implemented |
| DetailColor | ✅ Implemented |
| ImageSource | ✅ Implemented |
| IsEnabled | ✅ Implemented |
| ContextActions | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Appearing | ✅ Implemented |
| Disappearing | ✅ Implemented |

---

## IndicatorView

Displays indicators that represent the number of items in a CarouselView.

### Properties

| Property | Status |
|----------|--------|
| Count | ⏳ TODO |
| HideSingle | ⏳ TODO |
| IndicatorColor | ⏳ TODO |
| IndicatorSize | ⏳ TODO |
| IndicatorsShape | ⏳ TODO |
| ItemsSourceBy | ⏳ TODO |
| MaximumVisible | ⏳ TODO |
| Position | ⏳ TODO |
| SelectedIndicatorColor | ⏳ TODO |

---

## Label

Displays single-line and multi-line text.

### Properties

| Property | Status |
|----------|--------|
| CharacterSpacing | ✅ Implemented |
| FontAttributes | ✅ Implemented |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ✅ Implemented |
| FontSize | ✅ Implemented |
| FormattedText | ✅ Implemented |
| HorizontalTextAlignment | ✅ Implemented |
| LineBreakMode | ✅ Implemented |
| LineHeight | ✅ Implemented |
| MaxLines | ✅ Implemented |
| Padding | ✅ Implemented |
| Text | ✅ Implemented |
| TextColor | ✅ Implemented |
| TextDecorations | ✅ Implemented |
| TextTransform | ✅ Implemented |
| TextType | ⏳ TODO |
| VerticalTextAlignment | ✅ Implemented |

---

## Line

Displays a line from a start point to an end point.

### Properties

| Property | Status |
|----------|--------|
| X1 | ✅ Implemented |
| Y1 | ✅ Implemented |
| X2 | ✅ Implemented |
| Y2 | ✅ Implemented |
| Fill | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |

---

## ListView (Deprecated)

Displays a scrollable list of selectable data items.

### Properties

| Property | Status |
|----------|--------|
| CachingStrategy | ⏳ TODO |
| Footer | ⏳ TODO |
| FooterTemplate | ⏳ TODO |
| GroupHeaderTemplate | ⏳ TODO |
| HasUnevenRows | ⏳ TODO |
| Header | ⏳ TODO |
| HeaderTemplate | ⏳ TODO |
| HorizontalScrollBarVisibility | ⏳ TODO |
| IsPullToRefreshEnabled | ⏳ TODO |
| IsRefreshing | ⏳ TODO |
| ItemsSource | ⏳ TODO |
| ItemTemplate | ⏳ TODO |
| RefreshCommand | ⏳ TODO |
| RowHeight | ⏳ TODO |
| SelectedItem | ⏳ TODO |
| SelectionMode | ⏳ TODO |
| SeparatorColor | ⏳ TODO |
| SeparatorVisibility | ⏳ TODO |
| VerticalScrollBarVisibility | ⏳ TODO |
| GroupDisplayBinding | ⏳ TODO |
| GroupShortNameBinding | ⏳ TODO |
| RefreshControlColor | ⏳ TODO |
| IsGroupingEnabled | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| ItemAppearing | ⏳ TODO |
| ItemDisappearing | ⏳ TODO |
| ItemSelected | ⏳ TODO |
| ItemTapped | ⏳ TODO |
| Refreshing | ⏳ TODO |
| Scrolled | ⏳ TODO |
| ScrollToRequested | ⏳ TODO |

---

## Map

Displays a map, and requires the Microsoft.Maui.Controls.Maps NuGet package to be installed in your app.

### Properties

| Property | Status |
|----------|--------|
| IsShowingUser | ⏳ TODO |
| IsScrollEnabled | ⏳ TODO |
| IsTrafficEnabled | ⏳ TODO |
| IsZoomEnabled | ⏳ TODO |
| ItemsSource | ⏳ TODO |
| ItemTemplate | ⏳ TODO |
| MapType | ⏳ TODO |
| Pins | ⏳ TODO |

---

## NavigationPage

A Page that manages the navigation and user-experience of a stack of other pages.

### Properties

| Property | Status |
|----------|--------|
| BarBackground | ✅ Done |
| BarBackgroundColor | ✅ Done |
| BarTextColor | ✅ Done |
| CurrentPage | ✅ Done |
| RootPage | ⏳ TODO |
| HasBackButton | ✅ Done |
| HasNavigationBar | ✅ Done |
| BackButtonTitle | ✅ Done |
| TitleView | ✅ Done |
| IconColor | ✅ Done |
| TitleIconImageSource | ✅ Done |

### Events

| Event | Status |
|-------|--------|
| Popped | ✅ Done (handled by MAUI NavigationPage) |
| PoppedToRoot | ✅ Done (handled by MAUI NavigationPage) |
| Pushed | ✅ Done (handled by MAUI NavigationPage) |

### Methods

| Method | Status |
|--------|--------|
| PopAsync | ✅ Done |
| PopAsync(bool) | ✅ Done |
| PopToRootAsync | ✅ Done (via MAUI NavigationPage) |
| PopToRootAsync(bool) | ✅ Done (via MAUI NavigationPage) |
| PushAsync | ✅ Done |
| PushAsync(bool) | ✅ Done |

---

## Page

Visual element that occupies the entire screen.

### Properties

| Property | Status |
|----------|--------|
| Background | ✅ Implemented |
| BackgroundImageSource | ✅ Implemented |
| Content | ✅ Implemented |
| Title | ⏳ TODO |
| IconImageSource | ⏳ TODO |
| Padding | ⏳ TODO |

---

## Path

Displays curves and complex shapes.

### Properties

| Property | Status |
|----------|--------|
| Data | ✅ Implemented |
| Fill | ✅ Implemented |
| RenderTransform | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |
| StrokeMitterLimit | ⏳ TODO |

---

## Picker

Displays a short list of items, from which an item can be selected.

### Properties

| Property | Status |
|----------|--------|
| CharacterSpacing | ✅ Implemented  |
| FontAttributes | ✅ Implemented  |
| FontFamily | ✅ Implemented  |
| FontSize | ✅ Implemented  |
| HorizontalTextAlignment | ✅ Implemented  |
| ItemsSource | ✅ Implemented  |
| ItemDisplayBinding | ✅ Implemented  |
| SelectedIndex | ⏳ TODO |
| SelectedItem | ✅ Implemented  |
| TextColor | ✅ Implemented  |
| Title | ✅ Implemented |
| TitleColor | ✅ Implemented |
| VerticalTextAlignment | ✅ Implemented  |

### Events

| Event | Status |
|-------|--------|
| SelectedIndexChanged | ⏳ TODO |

---

## Polygon

Displays a polygon.

### Properties

| Property | Status |
|----------|--------|
| Points | ✅ Implemented |
| FillRule | ⏳ TODO |
| Fill | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |
| StrokeMiterLimit | ⏳ TODO |

---

## Polyline

Displays a series of connected straight lines.

### Properties

| Property | Status |
|----------|--------|
| Points | ✅ Implemented |
| FillRule | ⏳ TODO |
| Fill | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |

---

## ProgressBar

Uses an animation to show that the app is progressing through a lengthy activity.

### Properties

| Property | Status |
|----------|--------|
| Progress | ✅ Implemented |
| ProgressColor | ✅ Implemented |

---

## RadioButton

A type of button that allows the selection of one option from a set.

### Properties

| Property | Status |
|----------|--------|
| BorderColor | ⏳ TODO |
| BorderWidth | ✅ Implemented |
| CharacterSpacing | ✅ Implemented |
| Content | ✅ Implemented |
| ContentTemplate | ✅ Implemented |
| CornerRadius | ✅ Implemented |
| FontAttributes | ✅ Implemented |
| FontAutoScalingEnabled | ✅ Implemented |
| FontFamily | ✅ Implemented |
| FontSize | ✅ Implemented |
| GroupName | ✅ Implemented |
| IsChecked | ✅ Implemented |
| TextColor | ✅ Implemented |
| TextTransform | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| CheckedChanged | ✅ Implemented |

---

## Rectangle

Displays a rectangle or square.

### Properties

| Property | Status |
|----------|--------|
| RadiusX | ✅ Implemented |
| RadiusY | ✅ Implemented |
| Fill | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |
| StrokeMiterLimit | ⏳ TODO |

---

## RefreshView

A container control that provides pull-to-refresh functionality for scrollable content.

### Properties

| Property | Status |
|----------|--------|
| Command | ⏳ TODO |
| CommandParameter | ⏳ TODO |
| IsRefreshing | ⏳ TODO |
| RefreshColor | ⏳ TODO |

---

## RoundRectangle

Displays a rectangle or square with rounded corners.

### Properties

| Property | Status |
|----------|--------|
| CornerRadius | ✅ Implemented |
| Fill | ✅ Implemented |
| Stroke | ✅ Implemented |
| StrokeThickness | ✅ Implemented |
| StrokeDashArray | ✅ Implemented |
| StrokeDashOffset | ✅ Implemented |
| StrokeLineCap | ✅ Implemented |
| StrokeLineJoin | ✅ Implemented |
| StrokeMiterLimit | ✅ Implemented |

---

## ScrollView

Provides scrolling of its content, which is typically a layout.

### Properties

| Property | Status |
|----------|--------|
| Content | ✅ Implemented |
| ContentSize | ✅ Implemented |
| HorizontalScrollBarVisibility | ✅ Implemented |
| Orientation | ✅ Implemented |
| ScrollX | ✅ Implemented |
| ScrollY | ✅ Implemented |
| VerticalScrollBarVisibility | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Scrolled | ✅ Implemented |

---

## SearchBar

A user input control used to initiate a search.

### Properties

| Property | Status |
|----------|--------|
| CancelButtonColor | ✅ Implemented |
| CharacterSpacing | ✅ Implemented |
| CursorPosition | ✅ Implemented |
| FontAttributes | ✅ Implemented |
| FontFamily | ✅ Implemented |
| FontSize | ✅ Implemented |
| HorizontalTextAlignment | ✅ Implemented |
| Placeholder | ✅ Implemented |
| PlaceholderColor | ✅ Implemented |
| SearchCommand | ✅ Implemented |
| SearchCommandParameter | ✅ Implemented |
| SelectionLength | ✅ Implemented |
| Text | ✅ Implemented |
| TextColor | ✅ Implemented |
| VerticalTextAlignment | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| TextChanged | ✅ Implemented |
| SearchButtonPressed | ✅ Implemented |

---

## Shell

A Page that provides fundamental UI features that most applications require, including flyout navigation, tabbed navigation, and search.

### Properties

| Property | Status |
|----------|--------|
| BackgroundColor | ✅ Implemented |
| CurrentItem | ✅ Implemented |
| CurrentPage | ✅ Implemented (via DisplayedPage) |
| CurrentState | ⏳ TODO |
| DisabledColor | ✅ Implemented |
| FlyoutBackdrop | ✅ Implemented |
| FlyoutBackground | ✅ Implemented |
| FlyoutBackgroundColor | ✅ Implemented |
| FlyoutBackgroundImage | ✅ Implemented |
| FlyoutBackgroundImageAspect | ✅ Implemented |
| FlyoutBehavior | ✅ Implemented (Disabled/Flyout/Locked) |
| FlyoutContent | ✅ Implemented |
| FlyoutContentTemplate | ✅ Implemented |
| FlyoutFooter | ✅ Implemented |
| FlyoutFooterTemplate | ✅ Implemented |
| FlyoutHeader | ✅ Implemented |
| FlyoutHeaderBehavior | ✅ Implemented |
| FlyoutHeaderTemplate | ✅ Implemented |
| FlyoutHeight | ✅ Implemented |
| FlyoutIcon | ✅ Implemented (via FlyoutIcon/Icon on ShellItems) |
| FlyoutIsPresented | ✅ Implemented |
| FlyoutItems | ✅ Implemented (via Items with FlyoutItemIsVisible) |
| FlyoutVerticalScrollMode | ✅ Implemented |
| FlyoutWidth | ✅ Implemented |
| ForegroundColor | ✅ Implemented |
| Items | ✅ Implemented |
| ItemTemplate | ✅ Implemented |
| MenuItemTemplate | ✅ Implemented |
| NavBarHasShadow | ✅ Implemented |
| NavBarIsVisible | ✅ Implemented |
| NavBarVisibilityAnimationEnabled | ⏳ TODO |
| PresentationMode | ⏳ TODO |
| SearchHandler | ⏳ TODO |
| TabBarBackgroundColor | ✅ Implemented |
| TabBarDisabledColor | ✅ Implemented (placeholder) |
| TabBarForegroundColor | ✅ Implemented |
| TabBarIsVisible | ✅ Implemented |
| TabBarTitleColor | ✅ Implemented (placeholder) |
| TabBarUnselectedColor | ✅ Implemented (placeholder) |
| TitleColor | ✅ Implemented |
| TitleView | ✅ Implemented |
| UnselectedColor | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Navigated | ⏳ TODO |
| Navigating | ⏳ TODO |

### Methods

| Method | Status |
|--------|--------|
| GoToAsync | ⏳ TODO |
| GoToAsync(bool) | ⏳ TODO |
| GoToAsync(ShellNavigationState) | ⏳ TODO |

### Notes

Shell provides comprehensive flyout and tabbed navigation functionality with a custom FlyoutContainer that supports:
- Sliding flyout panel with gesture support and configurable height
- Flyout backdrop overlay with customizable brush
- Flyout background image with aspect ratio support
- Flyout header with Fixed/Scroll/CollapseOnScroll behavior options
- Flyout vertical scroll mode (Auto/Enabled/Disabled)
- Hamburger menu and back button in the navigation bar
- NavBar visibility control with optional shadow
- Custom TitleView support for navigation bar
- Tab-based navigation for ShellItems with multiple sections
- TabBar styling (background, foreground, visibility)
- Navigation stack support (push/pop pages)
- Dynamic title updates based on current page
- Shell appearance colors (Background, Foreground, Title, Disabled, Unselected)

---

## Slider

Enables you to select a double value from a continuous range.

### Properties

| Property | Status |
|----------|--------|
| Maximum | ✅ Implemented |
| Minimum | ✅ Implemented |
| MaximumTrackColor | ✅ Implemented |
| MinimumTrackColor | ✅ Implemented |
| ThumbColor | ✅ Implemented |
| ThumbImageSource | ✅ Implemented |
| Value | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| ValueChanged | ✅ Implemented |

---

## Stepper

Enables you to select a double value from a range of incremental values.

### Properties

| Property | Status |
|----------|--------|
| Increment | ✅ Implemented |
| Maximum | ✅ Implemented |
| Minimum | ✅ Implemented |
| Value | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| ValueChanged | ✅ Implemented |

---

## SwipeView

A container control that wraps around an item of content, and provides context menu items that are revealed by a swipe gesture.

### Properties

| Property | Status |
|----------|--------|
| BottomItems | ✅ Implemented |
| LeftItems | ✅ Implemented |
| RightItems | ✅ Implemented |
| TopItems | ✅ Implemented |
| Threshold | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| SwipeStarted | ✅ Implemented |
| SwipeChanging | ✅ Implemented |
| SwipeEnded | ✅ Implemented |

---

## Switch

Enables you to select a boolean value using a type of button that can either be on or off.

### Properties

| Property | Status |
|----------|--------|
| IsToggled | ✅ Implemented |
| OnColor | ✅ Implemented |
| ThumbColor | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Toggled | ✅ Implemented |

---

## SwitchCell (Deprecated)

Displays a switch with a label, for use in a TableView or ListView.

### Properties

| Property | Status |
|----------|--------|
| Text | ✅ Implemented |
| On | ✅ Implemented |
| OnColor | ✅ Implemented |
| IsEnabled | ✅ Implemented |
| ContextActions | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| OnChanged | ✅ Implemented |
| Appearing | ✅ Implemented |
| Disappearing | ✅ Implemented |

---

## TableView (Deprecated)

Displays a table of scrollable items that can be grouped into sections.

### Properties

| Property | Status         |
|----------|----------------|
| Intent | ✅ Implemented  |
| Root | ✅ Implemented  |
| HasUnevenRows | ✅ Implemented  |
| RowHeight | ✅ Implemented  |

---

## TextCell (Deprecated)

Displays text and detail text, for use in a TableView or ListView.

### Properties

| Property | Status |
|----------|--------|
| Text | ✅ Implemented |
| Detail | ✅ Implemented |
| TextColor | ✅ Implemented |
| DetailColor | ✅ Implemented |
| IsEnabled | ✅ Implemented |
| Command | ✅ Implemented |
| CommandParameter | ✅ Implemented |
| ContextActions | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Tapped | ✅ Implemented |
| Appearing | ✅ Implemented |
| Disappearing | ✅ Implemented |

---

## TitleBar

A View control that provides title bar functionality for a window. The standard title bar height is 32px but can be set to a larger value. The title bar can be hidden by setting the IsVisible property, which will cause the window content to be displayed in the title bar region.

### Properties

| Property | Status |
|----------|--------|
| Icon | ✅ Done (via TemplatedView) |
| Title | ✅ Done |
| Subtitle | ✅ Done |
| ForegroundColor | ✅ Done (via TemplatedView) |
| LeadingContent | ✅ Done (via TemplatedView) |
| Content | ✅ Done (via TemplatedView) |
| TrailingContent | ✅ Done (via TemplatedView) |
| PassthroughElements | ✅ Done |

### Notes

- TitleBar is a TemplatedView that renders its own content
- Integrated with Window.TitleBar property
- Supports active/inactive visual states for window focus changes
- Custom title bar content is rendered through the TemplatedView pattern
- Window dragging is supported by clicking on non-interactive areas of the TitleBar
- PassthroughElements (including LeadingContent, Content, TrailingContent) receive input directly instead of triggering window drag
- Uses Avalonia's ExtendClientAreaToDecorationsHint for custom title bar support with system window buttons

---

## TimePicker

Enables you to select a time with the platform time picker.

### Properties

| Property | Status |
|----------|--------|
| CharacterSpacing | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| Format | ⏳ TODO |
| TextColor | ⏳ TODO |
| Time | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| PropertyChanged | ⏳ TODO |

---

## ViewCell (Deprecated)

A cell containing a developer-defined view, for use in a TableView or ListView.

### Properties

| Property | Status |
|----------|--------|
| View | ✅ Implemented |
| IsEnabled | ✅ Implemented |
| ContextActions | ✅ Implemented |

### Events

| Event | Status |
|-------|--------|
| Tapped | ✅ Implemented |
| Appearing | ✅ Implemented |
| Disappearing | ✅ Implemented |

---

## WebView

Displays web pages or local HTML content.

### Properties

| Property | Status |
|----------|--------|
| CanGoBack | ⏳ TODO |
| CanGoForward | ⏳ TODO |
| Cookies | ⏳ TODO |
| Source | ⏳ TODO |
| UserAgent | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| Navigated | ⏳ TODO |
| Navigating | ⏳ TODO |
| ProcessTerminated | ⏳ TODO |

### Methods

| Method | Status |
|--------|--------|
| Eval | ⏳ TODO |
| EvaluateJavaScriptAsync | ⏳ TODO |
| GoBack | ⏳ TODO |
| GoForward | ⏳ TODO |
| Reload | ⏳ TODO |

---

## Inherited Properties from View & VisualElement

All controls inherit these common properties from the View and VisualElement base classes:

| Property | Type | Status |
|----------|------|--------|
| AnchorX | double | ✅ Implemented |
| AnchorY | double | ✅ Implemented |
| AutomationId | string | ✅ Implemented |
| Background | Brush | ✅ Implemented |
| BackgroundColor | Color | ✅ Implemented |
| Behaviors | IList<Behavior> | ⏳ TODO |
| BindingContext | object | ⏳ TODO |
| Bounds | Rect | ⏳ TODO |
| Clip | Geometry | ✅ Implemented |
| DesiredSize | Size | ⏳ TODO |
| Effects | IList<Effect> | ⏳ TODO |
| FlowDirection | FlowDirection | ✅ Implemented |
| Frame | Rect | ⏳ TODO |
| GestureRecognizers | IList<IGestureRecognizer> | ⏳ TODO |
| Handler | IViewHandler | ⏳ TODO |
| Height | double | ✅ Implemented |
| HeightRequest | double | ✅ Implemented |
| HorizontalOptions | LayoutOptions | ✅ Implemented |
| InputTransparent | bool | ⏳ TODO |
| IsEnabled | bool | ✅ Implemented |
| IsFocused | bool | ⏳ TODO |
| IsLoaded | bool | ⏳ TODO |
| IsVisible | bool | ✅ Implemented |
| Margin | Thickness | ⏳ TODO |
| MaximumHeightRequest | double | ✅ Implemented |
| MaximumWidthRequest | double | ✅ Implemented |
| MinimumHeightRequest | double | ✅ Implemented |
| MinimumWidthRequest | double | ✅ Implemented |
| Opacity | double | ✅ Implemented |
| Parent | Element | ⏳ TODO |
| Resources | ResourceDictionary | ⏳ TODO |
| Rotation | double | ✅ Implemented |
| RotationX | double | ✅ Implemented |
| RotationY | double | ✅ Implemented |
| Scale | double | ✅ Implemented |
| ScaleX | double | ✅ Implemented |
| ScaleY | double | ✅ Implemented |
| Shadow | Shadow | ✅ Implemented |
| StyleId | string | ✅ Implemented |
| TranslationX | double | ✅ Implemented |
| TranslationY | double | ✅ Implemented |
| Triggers | IList<TriggerBase> | ⏳ TODO |
| VerticalOptions | LayoutOptions | ✅ Implemented |
| Width | double | ✅ Implemented |
| WidthRequest | double | ✅ Implemented |
| Window | Window | ⏳ TODO |
| X | double | ⏳ TODO |
| Y | double | ⏳ TODO |
| ZIndex | int | ✅ Implemented |

---

## Inherited Events from View & VisualElement

All controls inherit these common events from the View and VisualElement base classes:

| Event | Status |
|-------|--------|
| BindingContextChanged | ⏳ TODO |
| ChildAdded | ⏳ TODO |
| ChildRemoved | ⏳ TODO |
| ChildrenReordered | ⏳ TODO |
| DescendantAdded | ⏳ TODO |
| DescendantRemoved | ⏳ TODO |
| Focused | ⏳ TODO |
| HandlerChanged | ⏳ TODO |
| HandlerChanging | ⏳ TODO |
| Loaded | ⏳ TODO |
| ParentChanged | ⏳ TODO |
| ParentChanging | ⏳ TODO |
| PropertyChanged | ⏳ TODO |
| PropertyChanging | ⏳ TODO |
| SizeChanged | ⏳ TODO |
| Unfocused | ⏳ TODO |
| Unloaded | ⏳ TODO |

---

## Legend

- **⏳ TODO**: Feature is pending implementation
- **🔧 In Progress**: Feature is currently being implemented
- **✅ Implemented**: Feature is fully implemented and ready to use

---
