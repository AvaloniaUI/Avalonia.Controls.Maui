using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace Avalonia.Controls.Maui.BlazorWebView.Compatibility;

internal static class MauiBlazorWebViewCompatibility
{
    private const string MauiAssemblyName = "Microsoft.AspNetCore.Components.WebView.Maui";
    private const string DeveloperToolsTypeName = "Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebViewDeveloperTools";
    private const string StaticContentHotReloadManagerTypeName = "Microsoft.AspNetCore.Components.WebView.StaticContentHotReloadManager";

    private static readonly Lazy<CreateUrlLoadingEventArgsDelegate> CreateUrlLoadingEventArgsCallback = new(() =>
        GetCreateUrlLoadingEventArgsMethod().CreateDelegate<CreateUrlLoadingEventArgsDelegate>());
    private static readonly Lazy<Type?> DeveloperToolsType = new(() =>
        typeof(UrlLoadingEventArgs).Assembly.GetType(DeveloperToolsTypeName, throwOnError: false));
    private static readonly Lazy<Action<WebViewManager>?> AttachStaticContentHotReloadCallback = new(() =>
        GetStaticContentHotReloadMethod("AttachToWebViewManagerIfEnabled")
            ?.CreateDelegate<Action<WebViewManager>>());
    private static readonly Lazy<TryReplaceStaticContentHotReloadResponseDelegate?> ReplaceStaticContentHotReloadCallback = new(() =>
        GetStaticContentHotReloadMethod("TryReplaceResponseContent")
            ?.CreateDelegate<TryReplaceStaticContentHotReloadResponseDelegate>());

    internal static bool IsStaticContentHotReloadSupported =>
        AttachStaticContentHotReloadCallback.Value is not null &&
        ReplaceStaticContentHotReloadCallback.Value is not null;

    [DynamicDependency("CreateWithDefaultLoadingStrategy", typeof(UrlLoadingEventArgs))]
    public static UrlLoadingEventArgs CreateUrlLoadingEventArgs(Uri url, Uri appOriginUri)
    {
        return CreateUrlLoadingEventArgsCallback.Value(url, appOriginUri);
    }

    public static void SetContainerView(ViewHandler handler, object? containerView) =>
        ViewHandlerAccessors.SetContainerView(handler, containerView);

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, DeveloperToolsTypeName, MauiAssemblyName)]
    public static bool AreDeveloperToolsEnabled(IServiceProvider services, ILogger logger)
    {
        var developerToolsType = DeveloperToolsType.Value;
        if (developerToolsType is null)
            return false;

        try
        {
            var settings = services.GetService(developerToolsType);
            return settings is not null &&
                developerToolsType.GetProperty("Enabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetValue(settings) is true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to read the MAUI BlazorWebView developer-tools setting.");
            return false;
        }
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, StaticContentHotReloadManagerTypeName, MauiAssemblyName)]
    public static void AttachStaticContentHotReload(WebViewManager manager, ILogger logger)
    {
        try
        {
            AttachStaticContentHotReloadCallback.Value?.Invoke(manager);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to attach MAUI BlazorWebView static-content hot reload.");
        }
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, StaticContentHotReloadManagerTypeName, MauiAssemblyName)]
    public static bool TryReplaceStaticContentHotReloadResponse(
        string contentRootRelativePath,
        string requestAbsoluteUri,
        ref int statusCode,
        ref Stream content,
        IDictionary<string, string> headers,
        ILogger logger)
    {
        try
        {
            return ReplaceStaticContentHotReloadCallback.Value?.Invoke(
                contentRootRelativePath,
                requestAbsoluteUri,
                ref statusCode,
                ref content,
                headers) ?? false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to apply a MAUI BlazorWebView static-content hot-reload response.");
            return false;
        }
    }

    private static MethodInfo GetCreateUrlLoadingEventArgsMethod()
    {
        return typeof(UrlLoadingEventArgs).GetMethod(
            "CreateWithDefaultLoadingStrategy",
            BindingFlags.Static | BindingFlags.NonPublic,
            [typeof(Uri), typeof(Uri)])
            ?? throw new MissingMethodException(
                typeof(UrlLoadingEventArgs).FullName,
                "CreateWithDefaultLoadingStrategy(Uri, Uri)");
    }

    private static MethodInfo? GetStaticContentHotReloadMethod(string name)
    {
        return typeof(UrlLoadingEventArgs).Assembly
            .GetType(StaticContentHotReloadManagerTypeName, throwOnError: false)
            ?.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    }

    private delegate bool TryReplaceStaticContentHotReloadResponseDelegate(
        string contentRootRelativePath,
        string requestAbsoluteUri,
        ref int statusCode,
        ref Stream content,
        IDictionary<string, string> headers);

    private delegate UrlLoadingEventArgs CreateUrlLoadingEventArgsDelegate(Uri url, Uri appOriginUri);

    private static class ViewHandlerAccessors
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ContainerView")]
        public static extern void SetContainerView(ViewHandler handler, object? containerView);
    }
}
