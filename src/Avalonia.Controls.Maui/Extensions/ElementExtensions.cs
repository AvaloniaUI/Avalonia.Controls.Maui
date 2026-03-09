using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using AvaloniaThickness = Avalonia.Thickness;
using MauiThickness = Microsoft.Maui.Thickness;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides extension methods for converting .NET MAUI elements to Avalonia elements.
/// </summary>
public static class ElementExtensions
{
    /// <summary>
    /// Converts a <see cref="MauiThickness"/> to an <see cref="AvaloniaThickness"/>.
    /// </summary>
    /// <remarks>
    /// .NET MAUI NaN values are converted to 0.
    /// </remarks>
    /// <param name="thickness">The thickness to convert.</param>
    /// <returns>Converted thickness as <see cref="AvaloniaThickness"/>.</returns>
    public static AvaloniaThickness ToPlatform(this MauiThickness thickness)
    {
        return new AvaloniaThickness(
                        double.IsNaN(thickness.Left) ? 0 : thickness.Left,
                        double.IsNaN(thickness.Top) ? 0 : thickness.Top,
                        double.IsNaN(thickness.Right) ? 0 : thickness.Right,
                        double.IsNaN(thickness.Bottom) ? 0 : thickness.Bottom);
    }

    private static HashSet<Type> handlersWithConstructors = new HashSet<Type>();

    private static IElementHandler? CreateTypeWithInjection(this Type viewType, IMauiContext mauiContext)
    {
        Type? handlerType = mauiContext.Handlers.GetHandlerType(viewType);
        if (handlerType == null)
        {
            return null;
        }

        return (IElementHandler)ActivatorUtilities.CreateInstance(mauiContext.Services, handlerType);
    }

    /// <summary>
    /// Creates or retrieves the <see cref="IElementHandler"/> for a .NET MAUI <see cref="IElement"/> using the specified <see cref="IMauiContext"/>.
    /// </summary>
    /// <param name="view">The .NET MAUI element to get a handler for.</param>
    /// <param name="context">The MAUI context used for handler resolution and dependency injection.</param>
    /// <returns>The <see cref="IElementHandler"/> associated with the element.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> or <paramref name="context"/> is <c>null</c>.</exception>
    /// <exception cref="HandlerNotFoundException">Thrown when no handler is registered for the element type.</exception>
    public static IElementHandler ToHandler(this IElement view, IMauiContext context)
    {
        if (view == null)
        {
            throw new ArgumentNullException("view");
        }

        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        if (view is IReplaceableView replaceableView)
        {
            view = replaceableView.ReplacedView;
        }

        IElementHandler? elementHandler = view.Handler;
        if (elementHandler?.MauiContext != null && elementHandler.MauiContext != context)
        {
            elementHandler = null;
        }

        if (elementHandler == null)
        {
            Type type = view.GetType();
            try
            {
                elementHandler = ((!handlersWithConstructors.Contains(type)) ? context.Handlers.GetHandler(type) : type.CreateTypeWithInjection(context));
            }
            catch (MissingMethodException)
            {
                elementHandler = type.CreateTypeWithInjection(context);
                if (elementHandler != null)
                {
                    handlersWithConstructors.Add(view.GetType());
                }
            }
        }

        if (elementHandler == null)
        {
            throw new HandlerNotFoundException(view);
        }

        elementHandler.SetMauiContext(context);
        view.Handler = elementHandler;
        if (elementHandler.VirtualView != view)
        {
            elementHandler.SetVirtualView(view);
        }

        return elementHandler;
    }

    /// <summary>
    /// Gets the platform view for a .NET MAUI <see cref="IElement"/> whose handler has already been set.
    /// </summary>
    /// <param name="view">The .NET MAUI element to get the platform view for.</param>
    /// <returns>The platform view object (typically an Avalonia control).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element's handler or platform view is not set.</exception>
    public static object ToPlatform(this IElement view)
    {
        if (view is IReplaceableView replaceableView && replaceableView.ReplacedView != view)
        {
            return replaceableView.ReplacedView.ToPlatform();
        }

        if (view.Handler == null)
        {
            throw new InvalidOperationException("MauiContext should have been set on parent.");
        }

        if (view.Handler is IViewHandler viewHandler)
        {
            object? containerView = viewHandler.ContainerView;
            if (containerView != null)
            {
                return containerView;
            }

            object? platformView = viewHandler.PlatformView;
            if (platformView != null)
            {
                return platformView;
            }
        }

        return view.Handler?.PlatformView ?? throw new InvalidOperationException($"Unable to convert {view} to {typeof(object)}");
    }

    /// <summary>
    /// Creates a handler for a .NET MAUI <see cref="IElement"/> using the specified <see cref="IMauiContext"/> and returns its platform view.
    /// </summary>
    /// <param name="view">The .NET MAUI element to convert to a platform view.</param>
    /// <param name="context">The MAUI context used for handler resolution.</param>
    /// <returns>The platform view object (typically an Avalonia control).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the platform view cannot be obtained.</exception>
    public static object ToPlatform(this IElement view, IMauiContext context)
    {
        IElementHandler elementHandler = view.ToHandler(context);
        object? platformView = elementHandler.PlatformView;
        if (platformView == null)
        {
            throw new InvalidOperationException($"Unable to convert {view} to {typeof(object)}");
        }

        return view.ToPlatform() ?? throw new InvalidOperationException($"Unable to convert {view} to {typeof(object)}");
    }

    private static void SetHandler(this object nativeElement, IElement element, IMauiContext context)
    {
        if (nativeElement == null)
        {
            throw new ArgumentNullException("nativeElement");
        }

        if (element == null)
        {
            throw new ArgumentNullException("element");
        }

        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        IElementHandler? elementHandler = element.Handler;
        if (elementHandler?.MauiContext != null && elementHandler.MauiContext != context)
        {
            elementHandler = null;
        }

        if (elementHandler == null)
        {
            elementHandler = context.Handlers.GetHandler(element.GetType());
        }

        if (elementHandler == null)
        {
            throw new Exception($"Handler not found for window {element}.");
        }

        elementHandler.SetMauiContext(context);
        element.Handler = elementHandler;
        if (elementHandler.VirtualView != element)
        {
            elementHandler.SetVirtualView(element);
        }
    }

    /// <summary>
    /// Connects a .NET MAUI <see cref="IApplication"/> to its handler using the specified platform object and context.
    /// </summary>
    /// <param name="platformApplication">The platform application object.</param>
    /// <param name="application">The .NET MAUI application to associate with its handler.</param>
    /// <param name="context">The MAUI context for handler resolution.</param>
    public static void SetApplicationHandler(this object platformApplication, IApplication application, IMauiContext context)
    {
        platformApplication.SetHandler(application, context);
    }

    /// <summary>
    /// Connects a .NET MAUI <see cref="IWindow"/> to its handler using the specified platform object and context.
    /// </summary>
    /// <param name="platformWindow">The platform window object.</param>
    /// <param name="window">The .NET MAUI window to associate with its handler.</param>
    /// <param name="context">The MAUI context for handler resolution.</param>
    public static void SetWindowHandler(this object platformWindow, IWindow window, IMauiContext context)
    {
        platformWindow.SetHandler(window, context);
    }

    /// <summary>
    /// Walks the parent chain of a .NET MAUI <see cref="IElement"/> to find the first ancestor of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of parent element to search for.</typeparam>
    /// <param name="element">The element whose parent chain to search.</param>
    /// <param name="includeThis">If <c>true</c>, includes the element itself in the search.</param>
    /// <returns>The first ancestor of type <typeparamref name="T"/>, or <c>default</c> if none is found.</returns>
    public static T? FindParentOfType<T>(this IElement element, bool includeThis = false) where T : IElement
    {
        if (includeThis && element is T)
        {
            return (T)element;
        }

        foreach (IElement? item in element.GetParentsPath())
        {
            if (item is T)
            {
                return (T)item;
            }
        }

        return default(T);
    }

    private static IEnumerable<IElement?> GetParentsPath(this IElement self)
    {
        IElement? current = self;
        while (current != null && !(current is IApplication))
        {
            current = current.Parent;
            yield return current;
        }
    }
}