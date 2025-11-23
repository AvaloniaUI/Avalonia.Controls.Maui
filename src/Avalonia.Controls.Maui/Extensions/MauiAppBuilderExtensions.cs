using System.Collections;
using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures Avalonia-specific services for MAUI image handling
    /// </summary>
    public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder)
    {
        builder.ConfigureImageSources(services =>
        {
            services.AddService<IFileImageSource, AvaloniaFileImageSourceService>();
            services.AddService<IUriImageSource, AvaloniaUriImageSourceService>();
            services.AddService<IFontImageSource, AvaloniaFontImageSourceService>();
            // HACK: There is a Stream source service as well, but I don't know if we need to register it explicitly
            // services.AddService<IStreamImageSource, AvaloniaStreamImageSourceService>();
        });

        return builder;
    }

    public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder, Action<IImageSourceServiceCollection>? configureDelegate)
    {
        if (configureDelegate != null)
        {
            builder.Services.AddSingleton<ImageSourceRegistration>(new ImageSourceRegistration(configureDelegate));
        }

        builder.Services.AddSingleton<IImageSourceServiceProvider>(svcs => new ImageSourceServiceProvider(svcs.GetRequiredService<IImageSourceServiceCollection>(), svcs));
        builder.Services.AddSingleton<IImageSourceServiceCollection>(svcs => new ImageSourceServiceBuilder(svcs.GetServices<ImageSourceRegistration>()));

        return builder;
    }

    /// <summary>
    /// Configures all Avalonia-specific services for MAUI
    /// </summary>
    public static MauiAppBuilder UseAvaloniaApp(this MauiAppBuilder builder, bool useSingleViewLifetime = false)
    {
        Microsoft.Maui.Dispatching.DispatcherProvider.SetCurrent(new Avalonia.Controls.Maui.Dispatching.AvaloniaDispatcherProvider());

        // Initialize compatibility DependencyService
#pragma warning disable CS0612 // Type or member is obsolete
        // Register the MAUI Controls assembly to scan for [Dependency] attributes
        // This is needed for ValueConverterProvider and other services used by triggers
        Microsoft.Maui.Controls.DependencyService.Register(new[] { typeof(Microsoft.Maui.Controls.VisualElement).Assembly });
        Microsoft.Maui.Controls.DependencyService.SetToInitialized();
        Microsoft.Maui.Controls.DependencyService.Register<AvaloniaFontNamedSizeService>();
#pragma warning restore CS0612 // Type or member is obsolete

        // Set Avalonia AppInfo implementation for theme detection using reflection
        // HACK: There is no public API to set the AppInfo implementation, we need this to be opened or any alternative
        // TODO: Remove reflection when possible or replace with UnsafeAccessor in .NET 10
        var setCurrentMethod = typeof(Microsoft.Maui.ApplicationModel.AppInfo).GetMethod("SetCurrent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            [typeof(Microsoft.Maui.ApplicationModel.IAppInfo)]);
        setCurrentMethod?.Invoke(null, [new AvaloniaAppInfoImplementation()]);

        builder.Services.AddSingleton<Microsoft.Maui.Dispatching.IDispatcher>(services =>
        {
            var avaloniaDispatcher = Avalonia.Threading.Dispatcher.UIThread;
            return new Avalonia.Controls.Maui.Dispatching.AvaloniaDispatcher(avaloniaDispatcher);
        });

        // Register Avalonia-specific font services
        // Replace existing registrations to ensure our Avalonia implementations are used
        var fontRegistrar = new Avalonia.Controls.Maui.AvaloniaMauiFontRegistrar();
        builder.Services.Replace(ServiceDescriptor.Singleton<IFontRegistrar>(fontRegistrar));
        builder.Services.Replace(ServiceDescriptor.Singleton<IFontManager>(svcs => new Avalonia.Controls.Maui.FontManager(svcs.GetRequiredService<IFontRegistrar>(), svcs)));

        return builder
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Microsoft.Maui.IApplication, Avalonia.Controls.Maui.Handlers.ApplicationHandler>();

                // Use different window handlers based on the application lifetime
                if (useSingleViewLifetime)
                {
                    handlers.AddHandler<Microsoft.Maui.Controls.Window, Avalonia.Controls.Maui.Handlers.SingleViewWindowHandler>();
                }
                else
                {
                    handlers.AddHandler<Microsoft.Maui.Controls.Window, Avalonia.Controls.Maui.Handlers.WindowHandler>();
                }
                handlers.AddHandler<Microsoft.Maui.Controls.NavigationPage, Avalonia.Controls.Maui.Handlers.NavigationViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ContentView, Avalonia.Controls.Maui.Handlers.ContentViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ContentPresenter, Avalonia.Controls.Maui.Handlers.ContentPresenterHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Border, Avalonia.Controls.Maui.Handlers.BorderHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Image, Avalonia.Controls.Maui.Handlers.ImageHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Page, Avalonia.Controls.Maui.Handlers.PageHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Label, Avalonia.Controls.Maui.Handlers.LabelHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ScrollView, Avalonia.Controls.Maui.Handlers.ScrollViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Layout, Avalonia.Controls.Maui.Handlers.LayoutHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.BoxView, Avalonia.Controls.Maui.Handlers.BoxViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Button, Avalonia.Controls.Maui.Handlers.ButtonHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ImageButton, Avalonia.Controls.Maui.Handlers.ImageButtonHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Entry, Avalonia.Controls.Maui.Handlers.EntryHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Editor, Avalonia.Controls.Maui.Handlers.EditorHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.SearchBar, Avalonia.Controls.Maui.Handlers.SearchBarHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.CheckBox, Avalonia.Controls.Maui.Handlers.CheckBoxHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Switch, Avalonia.Controls.Maui.Handlers.SwitchHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.RadioButton, Avalonia.Controls.Maui.Handlers.RadioButtonHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Slider, Avalonia.Controls.Maui.Handlers.SliderHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.DatePicker, Avalonia.Controls.Maui.Handlers.DatePickerHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.TimePicker, Avalonia.Controls.Maui.Handlers.TimePickerHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Picker, Avalonia.Controls.Maui.Handlers.PickerHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ProgressBar, Avalonia.Controls.Maui.Handlers.ProgressBarHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ActivityIndicator, Avalonia.Controls.Maui.Handlers.ActivityIndicatorHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Stepper, Avalonia.Controls.Maui.Handlers.StepperHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Avalonia.Controls.Maui.Handlers.CollectionViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ListView, Avalonia.Controls.Maui.Handlers.ListViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, Avalonia.Controls.Maui.Handlers.CarouselViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.IndicatorView, Avalonia.Controls.Maui.Handlers.IndicatorViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.TableView, Avalonia.Controls.Maui.Handlers.TableViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.TextCell, Avalonia.Controls.Maui.Handlers.Cells.TextCellHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ImageCell, Avalonia.Controls.Maui.Handlers.Cells.ImageCellHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ViewCell, Avalonia.Controls.Maui.Handlers.Cells.ViewCellHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.SwitchCell, Avalonia.Controls.Maui.Handlers.Cells.SwitchCellHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.EntryCell, Avalonia.Controls.Maui.Handlers.Cells.EntryCellHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.TabbedPage, Avalonia.Controls.Maui.Handlers.TabbedViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.FlyoutPage, Avalonia.Controls.Maui.Handlers.FlyoutViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shell, Avalonia.Controls.Maui.Handlers.Shell.ShellHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ShellItem, Avalonia.Controls.Maui.Handlers.Shell.ShellItemHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ShellSection, Avalonia.Controls.Maui.Handlers.Shell.ShellSectionHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.ShellContent, Avalonia.Controls.Maui.Handlers.Shell.ShellContentHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Line, Avalonia.Controls.Maui.Handlers.Shapes.LineHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Rectangle, Avalonia.Controls.Maui.Handlers.Shapes.RectangleHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Ellipse, Avalonia.Controls.Maui.Handlers.Shapes.EllipseHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Polygon, Avalonia.Controls.Maui.Handlers.Shapes.PolygonHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Polyline, Avalonia.Controls.Maui.Handlers.Shapes.PolylineHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.Path, Avalonia.Controls.Maui.Handlers.Shapes.PathHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Shapes.RoundRectangle, Avalonia.Controls.Maui.Handlers.Shapes.RoundRectangleHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.MenuBar, Avalonia.Controls.Maui.Handlers.MenuBarHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.MenuBarItem, Avalonia.Controls.Maui.Handlers.MenuBarItemHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.MenuFlyout, Avalonia.Controls.Maui.Handlers.MenuFlyoutHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.MenuFlyoutItem, Avalonia.Controls.Maui.Handlers.MenuFlyoutItemHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.MenuFlyoutSubItem, Avalonia.Controls.Maui.Handlers.MenuFlyoutSubItemHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.MenuFlyoutSeparator, Avalonia.Controls.Maui.Handlers.MenuFlyoutSeparatorHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.Toolbar, Avalonia.Controls.Maui.Handlers.ToolbarHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.SwipeItem, Avalonia.Controls.Maui.Handlers.SwipeItemMenuItemHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.SwipeView, Avalonia.Controls.Maui.Handlers.SwipeViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.SwipeItemView, Avalonia.Controls.Maui.Handlers.SwipeItemViewHandler>();
                handlers.AddHandler<Microsoft.Maui.Controls.RefreshView, Avalonia.Controls.Maui.Handlers.RefreshViewHandler>();

            })
            .ConfigureImageSources();
    }

    class ImageSourceRegistration
    {
        private readonly Action<IImageSourceServiceCollection> _registerAction;

        public ImageSourceRegistration(Action<IImageSourceServiceCollection> registerAction)
        {
            _registerAction = registerAction;
        }

        internal void AddRegistration(IImageSourceServiceCollection builder)
        {
            _registerAction(builder);
        }
    }

    class ImageSourceServiceBuilder : MauiServiceCollection, IImageSourceServiceCollection
    {
        public ImageSourceServiceBuilder(IEnumerable<ImageSourceRegistration> registrationActions)
        {
            if (registrationActions != null)
            {
                foreach (var effectRegistration in registrationActions)
                {
                    effectRegistration.AddRegistration(this);
                }
            }
        }
    }

    class MauiServiceCollection : IMauiServiceCollection
    {
        readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();
        readonly Dictionary<Type, ServiceDescriptor> _descriptorDictionary = new Dictionary<Type, ServiceDescriptor>();

        public int Count => _descriptors.Count;

        public bool IsReadOnly => false;

        public ServiceDescriptor this[int index]
        {
            get => _descriptors[index];
            set => _descriptors[index] = value;
        }

        public void Add(ServiceDescriptor item)
        {
            if (_descriptors.Contains(item))
                return;

            _descriptors.Add(item);
            _descriptorDictionary[item.ServiceType] = item;
        }

        public void Clear()
        {
            _descriptors.Clear();
            _descriptorDictionary.Clear();
        }

        public bool Contains(ServiceDescriptor item) =>
            _descriptors.Contains(item);

        public IEnumerator<ServiceDescriptor> GetEnumerator() =>
            _descriptors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _descriptors.GetEnumerator();

        public bool Remove(ServiceDescriptor item)
        {
            var success = _descriptors.Remove(item);
            if (success)
                _descriptorDictionary.Remove(item.ServiceType);
            return success;
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) =>
            _descriptors.CopyTo(array, arrayIndex);

        public int IndexOf(ServiceDescriptor item) => _descriptors.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item)
        {
            _descriptors.Insert(index, item);
            _descriptorDictionary[item.ServiceType] = item;
        }

        public void RemoveAt(int index)
        {
            var descriptor = _descriptors[index];
            _descriptors.RemoveAt(index);
            _descriptorDictionary.Remove(descriptor.ServiceType);
        }

        public bool TryGetService(Type serviceType, out ServiceDescriptor? descriptor) =>
            _descriptorDictionary.TryGetValue(serviceType, out descriptor);
    }
}