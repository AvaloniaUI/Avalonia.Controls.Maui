using System;
using System.Diagnostics;
using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages.WebViewGalleries
{
	public partial class WebViewGallery
	{
		public WebViewGallery()
		{
			InitializeComponent();
			FileStatusLabel.Text = "Load index.html, useragent.html, or video.html to exercise packaged asset navigation.";
			NavigationStatusLabel.Text = "Navigation events for this WebView will appear here.";
			CookieStatusLabel.Text = "Load httpbin's cookie inspector and confirm MauiCookie appears in the page.";
			EvalResultLabel.Text = "Run Eval Async to capture a JavaScript return value.";
			JavaScriptEnabledResult.Text = "JavaScript is enabled";
			UpdateProcessTerminatedSupportLabel();
		}

		protected override void OnAppearing()
		{
			MauiWebView.Navigating += OnMauiWebViewNavigating;
			MauiWebView.Navigated += OnMauiWebViewNavigated;
			CookieWebView.Navigated += OnCookieWebViewNavigated;

			if (WebViewHandler.SupportsProcessTerminated)
			{
				MauiWebView.ProcessTerminated += OnMauiWebViewProcessTerminated;
			}
		}

		protected override void OnDisappearing()
		{
			MauiWebView.Navigating -= OnMauiWebViewNavigating;
			MauiWebView.Navigated -= OnMauiWebViewNavigated;
			CookieWebView.Navigated -= OnCookieWebViewNavigated;

			if (WebViewHandler.SupportsProcessTerminated)
			{
				MauiWebView.ProcessTerminated -= OnMauiWebViewProcessTerminated;
			}
		}

		void OnUpdateHtmlSourceClicked(object sender, EventArgs args)
		{
			Random rnd = new();
			HtmlWebViewSource htmlWebViewSource = new();
			HtmlSourceWebView.Source = htmlWebViewSource;
			htmlWebViewSource.Html += $"<h1>Updated Content {rnd.Next()}!</h1><br>";
		}

		void OnGoBackClicked(object sender, EventArgs args)
		{
			Debug.WriteLine($"CanGoBack {MauiWebView.CanGoBack}");

			if (MauiWebView.CanGoBack)
			{
				MauiWebView.GoBack();
				return;
			}

			NavigationStatusLabel.Text = "Go Back ignored because there is no previous entry in the navigation history.";
		}

		void OnGoForwardClicked(object sender, EventArgs args)
		{
			Debug.WriteLine($"CanGoForward {MauiWebView.CanGoForward}");

			if (MauiWebView.CanGoForward)
			{
				MauiWebView.GoForward();
				return;
			}

			NavigationStatusLabel.Text = "Go Forward ignored because there is no later entry in the navigation history.";
		}

		void OnReloadClicked(object sender, EventArgs args)
		{
			NavigationStatusLabel.Text = "Reloading the current page.";
			MauiWebView.Reload();
		}

		void OnEvalClicked(object sender, EventArgs args)
		{
			NavigationStatusLabel.Text = "Executing alert('text') in the navigation sample.";
			MauiWebView.Eval("alert('text')");
		}

		void OnMauiWebViewNavigating(object? sender, Microsoft.Maui.Controls.WebNavigatingEventArgs e)
		{
			Debug.WriteLine($"Navigating - Url: {e.Url}, Event: {e.NavigationEvent}");
			NavigationStatusLabel.Text = $"Navigating ({e.NavigationEvent}): {e.Url}";
		}

		void OnMauiWebViewNavigated(object? sender, Microsoft.Maui.Controls.WebNavigatedEventArgs e)
		{
			Debug.WriteLine($"Navigated - Url: {e.Url}, Event: {e.NavigationEvent}, Result: {e.Result}");
			NavigationStatusLabel.Text = $"Navigated ({e.NavigationEvent}, {e.Result}): {e.Url}";
		}

		void OnMauiWebViewProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e)
		{
			if (e.PlatformArgs is null)
			{
				Debug.WriteLine("WebView process failed.");
				return;
			}

#if ANDROID
			var renderProcessGoneDetail = e.PlatformArgs.RenderProcessGoneDetail;
			Debug.WriteLine($"WebView process failed. DidCrash: {renderProcessGoneDetail?.DidCrash()}");
#elif WINDOWS
			var coreWebView2ProcessFailedEventArgs = e.PlatformArgs.CoreWebView2ProcessFailedEventArgs;
			Debug.WriteLine($"WebView process failed. ExitCode: {coreWebView2ProcessFailedEventArgs.ExitCode}");
#else
			Debug.WriteLine("WebView process failed.");
#endif
		}

		void UpdateProcessTerminatedSupportLabel()
		{
			ProcessTerminatedSupportLabel.Text = WebViewHandler.SupportsProcessTerminated
				? "ProcessTerminated is supported. Terminate the underlying web process to observe the event in debug output."
				: "ProcessTerminated is not currently supported by Avalonia NativeWebView, because the backend exposes no equivalent process-failure event.";
		}

		async void OnEvalAsyncClicked(object sender, EventArgs args)
		{
			MauiWebView.Eval("alert('text')");
			NavigationStatusLabel.Text = "Running EvaluateJavaScriptAsync on the current page.";

			var result = await MauiWebView.EvaluateJavaScriptAsync(
				"var test = function(){ return 'This string came from Javascript!'; }; test();");

			EvalResultLabel.Text = result;
		}

		void OnLoadHtmlFileClicked(object sender, EventArgs e)
		{
			LoadPackageAsset(input.Text);
		}

		void OnSetUserAgentClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(userAgent.Text))
			{
				FileWebView.ClearValue(Microsoft.Maui.Controls.WebView.UserAgentProperty);
			}
			else
			{
				FileWebView.UserAgent = userAgent.Text.Trim();
			}

			userAgent.Text = FileWebView.UserAgent ?? string.Empty;
			input.Text = "useragent.html";
			LoadPackageAsset(input.Text);
			FileStatusLabel.Text = string.IsNullOrWhiteSpace(FileWebView.UserAgent)
				? "Loaded useragent.html with the default user agent."
				: $"Loaded useragent.html with custom user agent: {FileWebView.UserAgent}";
		}

		void OnResetUserAgentClicked(object sender, EventArgs e)
		{
			FileWebView.ClearValue(Microsoft.Maui.Controls.WebView.UserAgentProperty);
			userAgent.Text = string.Empty;
			input.Text = "useragent.html";
			LoadPackageAsset(input.Text);
			FileStatusLabel.Text = "Reset the custom user agent and reloaded useragent.html.";
		}

		void LoadPackageAsset(string? assetPath)
		{
			var normalizedAssetPath = string.IsNullOrWhiteSpace(assetPath) ? "index.html" : assetPath.Trim();

			input.Text = normalizedAssetPath;
			FileWebView.Source = new UrlWebViewSource { Url = normalizedAssetPath };

			if (normalizedAssetPath != "useragent.html")
			{
				FileStatusLabel.Text = $"Loaded packaged asset: {normalizedAssetPath}";
			}
		}

		void OnAllowMixedContentClicked(object sender, EventArgs e)
		{
			MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetMixedContentMode(MixedContentHandling.AlwaysAllow);
		}

		void OnEnableZoomControlsClicked(object sender, EventArgs e)
		{
			MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().EnableZoomControls(true);
		}

		void OnJavaScriptEnabledClicked(object sender, EventArgs e)
		{
			bool isJavaScriptEnabled = MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().IsJavaScriptEnabled();

			if (isJavaScriptEnabled)
			{
				MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().JavaScriptEnabled(false);
				JavaScriptEnabledResult.Text = "JavaScript is disabled";
			}
			else
			{
				MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().JavaScriptEnabled(true);
				JavaScriptEnabledResult.Text = "JavaScript is enabled";
			}
		}

		void OnLoadHtml5VideoClicked(object sender, EventArgs e)
		{
			NavigationStatusLabel.Text = "Loading packaged video.html to verify relative video asset resolution.";
			MauiWebView.Source = new UrlWebViewSource { Url = "video.html" };
		}

		void OnLoadHttpBinClicked(object sender, EventArgs e)
		{
			// on httpbin.org find the section where you can load the cookies for the website.
			// The cookie that is set below should show up for this to succeed.
			CookieWebView.Cookies = new System.Net.CookieContainer();
			CookieWebView.Cookies.Add(new System.Net.Cookie("MauiCookie", "Hmmm Cookies!", "/", "httpbin.org"));

			CookieStatusLabel.Text = "Loading httpbin's cookie inspector with MauiCookie pre-seeded in the WebView cookie container.";
			CookieWebView.Source = new UrlWebViewSource { Url = "https://httpbin.org/#/Cookies/get_cookies" };
		}

		void OnCookieWebViewNavigated(object? sender, WebNavigatedEventArgs e)
		{
			CookieStatusLabel.Text = e.Result == WebNavigationResult.Success
				? "Loaded httpbin. Open the Cookies section on the page and confirm MauiCookie is present."
				: $"Cookie sample navigation failed with result: {e.Result}";
		}
	}
}
