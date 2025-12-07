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
| Content | ⏳ TODO |
| Padding | ⏳ TODO |
| StrokeShape | ⏳ TODO |
| Stroke | ⏳ TODO |
| StrokeThickness | ⏳ TODO |
| StrokeDashArray | ⏳ TODO |
| StrokeDashOffset | ⏳ TODO |
| StrokeLineCap | ⏳ TODO |
| StrokeLineJoin | ⏳ TODO |
| StrokeMiterLimit | ⏳ TODO |

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
| EmptyView | ⏳ TODO |
| EmptyViewTemplate | ⏳ TODO |
| Footer | ⏳ TODO |
| FooterTemplate | ⏳ TODO |
| Header | ⏳ TODO |
| HeaderTemplate | ⏳ TODO |
| ItemsLayout | ⏳ TODO |
| ItemsSource | ⏳ TODO |
| ItemTemplate | ⏳ TODO |
| ItemsUpdatingScrollMode | ⏳ TODO |
| RemainingItemsThreshold | ⏳ TODO |
| RemainingItemsThresholdReachedCommand | ⏳ TODO |
| SelectedItem | ⏳ TODO |
| SelectedItems | ⏳ TODO |
| SelectionMode | ⏳ TODO |

---

## ContentView

A control that enables the creation of custom, reusable controls.

### Properties

| Property | Status |
|----------|--------|
| Content | ⏳ TODO |

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

## Frame (Deprecated)

Used to wrap a view or layout with a border that can be configured with color, shadow, and other options.

### Properties

| Property | Status |
|----------|--------|
| BorderColor | ⏳ TODO |
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
| CharacterSpacing | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| FormattedText | ⏳ TODO |
| HorizontalTextAlignment | ⏳ TODO |
| LineBreakMode | ⏳ TODO |
| LineHeight | ⏳ TODO |
| MaxLines | ⏳ TODO |
| Padding | ⏳ TODO |
| Text | ⏳ TODO |
| TextColor | ⏳ TODO |
| TextDecorations | ⏳ TODO |
| TextTransform | ⏳ TODO |
| TextType | ⏳ TODO |
| VerticalTextAlignment | ⏳ TODO |

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
| BarBackground | ⏳ TODO |
| BarBackgroundColor | ⏳ TODO |
| BarTextColor | ⏳ TODO |
| CurrentPage | ⏳ TODO |
| RootPage | ⏳ TODO |
| HasBackButton | ⏳ TODO |
| HasNavigationBar | ⏳ TODO |
| BackButtonTitle | ⏳ TODO |
| TitleView | ⏳ TODO |
| IconColor | ⏳ TODO |
| TitleIconImageSource | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| Popped | ⏳ TODO |
| PoppedToRoot | ⏳ TODO |
| Pushed | ⏳ TODO |

### Methods

| Method | Status |
|--------|--------|
| PopAsync | ⏳ TODO |
| PopAsync(bool) | ⏳ TODO |
| PopToRootAsync | ⏳ TODO |
| PopToRootAsync(bool) | ⏳ TODO |
| PushAsync | ⏳ TODO |
| PushAsync(bool) | ⏳ TODO |

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
| CharacterSpacing | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| HorizontalTextAlignment | ⏳ TODO |
| ItemsSource | ⏳ TODO |
| ItemDisplayBinding | ⏳ TODO |
| SelectedIndex | ⏳ TODO |
| SelectedItem | ⏳ TODO |
| TextColor | ⏳ TODO |
| Title | ⏳ TODO |
| TitleColor | ⏳ TODO |
| VerticalTextAlignment | ⏳ TODO |

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
| BorderWidth | ⏳ TODO |
| CharacterSpacing | ⏳ TODO |
| Content | ⏳ TODO |
| ContentTemplate | ⏳ TODO |
| CornerRadius | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| GroupName | ⏳ TODO |
| IsChecked | ⏳ TODO |
| TextColor | ⏳ TODO |
| TextTransform | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| CheckedChanged | ⏳ TODO |

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
| CancelButtonColor | ⏳ TODO |
| CharacterSpacing | ⏳ TODO |
| CursorPosition | ⏳ TODO |
| FontAttributes | ⏳ TODO |
| FontAutoScalingEnabled | ⏳ TODO |
| FontFamily | ⏳ TODO |
| FontSize | ⏳ TODO |
| HorizontalTextAlignment | ⏳ TODO |
| Placeholder | ⏳ TODO |
| PlaceholderColor | ⏳ TODO |
| SearchCommand | ⏳ TODO |
| SearchCommandParameter | ⏳ TODO |
| SelectionLength | ⏳ TODO |
| Text | ⏳ TODO |
| TextColor | ⏳ TODO |
| VerticalTextAlignment | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| TextChanged | ⏳ TODO |
| SearchButtonPressed | ⏳ TODO |

---

## Shell

A Page that provides fundamental UI features that most applications require, including flyout navigation, tabbed navigation, and search.

### Properties

| Property | Status |
|----------|--------|
| BackgroundColor | ⏳ TODO |
| CurrentItem | ⏳ TODO |
| CurrentPage | ⏳ TODO |
| CurrentState | ⏳ TODO |
| DisabledColor | ⏳ TODO |
| FlyoutBackdrop | ⏳ TODO |
| FlyoutBackground | ⏳ TODO |
| FlyoutBackgroundColor | ⏳ TODO |
| FlyoutBackgroundImage | ⏳ TODO |
| FlyoutBackgroundImageAspect | ⏳ TODO |
| FlyoutBehavior | ⏳ TODO |
| FlyoutContent | ⏳ TODO |
| FlyoutContentTemplate | ⏳ TODO |
| FlyoutFooter | ⏳ TODO |
| FlyoutFooterTemplate | ⏳ TODO |
| FlyoutHeader | ⏳ TODO |
| FlyoutHeaderBehavior | ⏳ TODO |
| FlyoutHeaderTemplate | ⏳ TODO |
| FlyoutHeight | ⏳ TODO |
| FlyoutIcon | ⏳ TODO |
| FlyoutIsPresented | ⏳ TODO |
| FlyoutItems | ⏳ TODO |
| FlyoutVerticalScrollMode | ⏳ TODO |
| FlyoutWidth | ⏳ TODO |
| ForegroundColor | ⏳ TODO |
| Items | ⏳ TODO |
| ItemTemplate | ⏳ TODO |
| MenuItemTemplate | ⏳ TODO |
| NavBarHasShadow | ⏳ TODO |
| NavBarIsVisible | ⏳ TODO |
| NavBarVisibilityAnimationEnabled | ⏳ TODO |
| PresentationMode | ⏳ TODO |
| SearchHandler | ⏳ TODO |
| TabBarBackgroundColor | ⏳ TODO |
| TabBarDisabledColor | ⏳ TODO |
| TabBarForegroundColor | ⏳ TODO |
| TabBarIsVisible | ⏳ TODO |
| TabBarTitleColor | ⏳ TODO |
| TabBarUnselectedColor | ⏳ TODO |
| TitleColor | ⏳ TODO |
| TitleView | ⏳ TODO |
| UnselectedColor | ⏳ TODO |

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
| ThumbImageSource | ⏳ TODO |
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
| Increment | ⏳ TODO |
| Maximum | ⏳ TODO |
| Minimum | ⏳ TODO |
| Value | ⏳ TODO |

### Events

| Event | Status |
|-------|--------|
| ValueChanged | ⏳ TODO |

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

## TableView (Deprecated)

Displays a table of scrollable items that can be grouped into sections.

### Properties

| Property | Status |
|----------|--------|
| Intent | ⏳ TODO |
| Root | ⏳ TODO |
| HasUnevenRows | ⏳ TODO |
| RowHeight | ⏳ TODO |

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