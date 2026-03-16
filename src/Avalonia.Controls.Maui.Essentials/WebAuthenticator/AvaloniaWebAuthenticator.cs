using Microsoft.Maui.Authentication;
using AvaloniaWebAuthenticatorOptions = Avalonia.Controls.WebAuthenticatorOptions;
using MauiWebAuthenticatorOptions = Microsoft.Maui.Authentication.WebAuthenticatorOptions;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Implements IWebAuthenticator using Avalonia's WebAuthenticationBroker to provide OAuth authentication flows on desktop, mobile, and browser platforms.
/// </summary>
public class AvaloniaWebAuthenticator : IWebAuthenticator
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    internal AvaloniaWebAuthenticator(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    /// <summary>
    /// Starts a web authentication flow using the platform's authentication broker.
    /// </summary>
    /// <param name="webAuthenticatorOptions">The options that configure the authentication flow, including the request URL and callback URL.</param>
    /// <returns>A <see cref="WebAuthenticatorResult"/> containing the parsed authentication response.</returns>
    public Task<WebAuthenticatorResult> AuthenticateAsync(MauiWebAuthenticatorOptions webAuthenticatorOptions)
        => AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

    /// <summary>
    /// Starts a web authentication flow using the platform's authentication broker with cancellation support.
    /// </summary>
    /// <param name="webAuthenticatorOptions">The options that configure the authentication flow, including the request URL and callback URL.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the authentication flow.</param>
    /// <returns>A <see cref="WebAuthenticatorResult"/> containing the parsed authentication response.</returns>
    public async Task<WebAuthenticatorResult> AuthenticateAsync(MauiWebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(webAuthenticatorOptions.Url);
        ArgumentNullException.ThrowIfNull(webAuthenticatorOptions.CallbackUrl);

        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var avaloniaOptions = new AvaloniaWebAuthenticatorOptions(webAuthenticatorOptions.Url, webAuthenticatorOptions.CallbackUrl)
        {
            NonPersistent = webAuthenticatorOptions.PrefersEphemeralWebBrowserSession,
        };

        var result = await WebAuthenticationBroker.AuthenticateAsync(topLevel, avaloniaOptions)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        return new WebAuthenticatorResult(result.CallbackUri, webAuthenticatorOptions.ResponseDecoder);
    }
}
