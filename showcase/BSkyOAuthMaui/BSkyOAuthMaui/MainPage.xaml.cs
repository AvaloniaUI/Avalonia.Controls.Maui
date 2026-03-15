// <copyright file="MainPage.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using AppBsky.Feed;
using CarpaNet;
using CarpaNet.Identity;
using CarpaNet.OAuth;
using CarpaNet.OAuth.Storage;

namespace BSkyOAuthMaui;

public partial class MainPage : ContentPage
{
    private const string ClientMetadataUrl = "https://drasticactions.vip/client-metadata.json";
    private const string RedirectUri = "vip.drasticactions:/callback";

    private readonly OAuthSession oauthSession;
    private readonly OauthStore sessionStore;

    private ATProtoOAuthClient? client;

    public MainPage()
    {
        InitializeComponent();

        this.sessionStore = new OauthStore();

        var config = new OAuthClientConfig
        {
            ClientId = ClientMetadataUrl,
            RedirectUri = RedirectUri,
            Scope = "atproto transition:generic",
            JsonOptions = ATProtoClientFactory.CreateJsonOptions(),
            SessionStore = this.sessionStore
        };

        this.oauthSession = new OAuthSession(config);
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var atIdentifier = new ATIdentifier(HandleEntry.Text ?? string.Empty);
        if (!atIdentifier.IsValid)
        {
            await DisplayAlertAsync("Error", "Invalid handle", "OK");
            return;
        }

        SetLoading(true);

        try
        {
            var authUrl = await this.oauthSession.AuthorizeAsync(atIdentifier);
            var callbackUri = new Uri(RedirectUri);

            var result = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), callbackUri);

            // Reconstruct the full callback URL from the result properties
            var queryParams = string.Join("&", result.Properties.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            var fullCallbackUrl = $"{RedirectUri}?{queryParams}";

            this.client = await this.oauthSession.CallbackAsync(fullCallbackUrl);

            if (this.client != null)
            {
                GetTimelineBtn.IsEnabled = true;
                StatusLabel.Text = $"Authenticated as {this.client.Did}";
                await DisplayAlertAsync("Success", $"Authenticated as {this.client.Did}", "OK");
            }
            else
            {
                await DisplayAlertAsync("Error", "Failed to authenticate", "OK");
            }
        }
        catch (TaskCanceledException)
        {
            // User cancelled the auth flow
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Authentication failed: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnLoadSessionClicked(object? sender, EventArgs e)
    {
        var input = await DisplayPromptAsync("Restore Session", "Enter your DID or handle", placeholder: "did:plc:... or alice.bsky.social");
        if (string.IsNullOrWhiteSpace(input))
            return;

        var atIdentifier = new ATIdentifier(input.Trim());
        if (!atIdentifier.IsValid)
        {
            await DisplayAlertAsync("Error", "Invalid DID or handle.", "OK");
            return;
        }

        SetLoading(true);

        try
        {
            var did = input.Trim();
            if (atIdentifier.IsHandle)
            {
                using var resolver = new IdentityResolver();
                var doc = await resolver.ResolveAsync(did);
                did = doc.Id;
            }

            var session = await this.oauthSession.RestoreSessionAsync(did);
            if (session != null)
            {
                this.client = session;
                GetTimelineBtn.IsEnabled = true;
                StatusLabel.Text = $"Session restored for {session.Did}";
                await DisplayAlertAsync("Success", $"Session restored for {session.Did}", "OK");
            }
            else
            {
                await DisplayAlertAsync("Error", "No saved session found for that identifier.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to restore session: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnGetTimelineClicked(object? sender, EventArgs e)
    {
        if (this.client == null)
            return;

        SetLoading(true);

        try
        {
            var parameters = new GetTimelineParameters { Limit = 25 };
            var timeline = await this.client.AppBskyFeedGetTimelineAsync(parameters);

            var items = new List<TimelineItem>();
            if (timeline.Feed != null)
            {
                foreach (var item in timeline.Feed)
                {
                    var author = item.Post?.Author;
                    var text = string.Empty;
                    if (item.Post?.Record is System.Text.Json.JsonElement recordElement)
                    {
                        if (recordElement.TryGetProperty("text", out var textElement))
                        {
                            text = textElement.GetString() ?? string.Empty;
                        }
                    }

                    items.Add(new TimelineItem
                    {
                        AuthorDisplay = $"@{author?.Handle} ({author?.DisplayName})",
                        Text = text
                    });
                }
            }

            TimelineView.ItemsSource = items;
            StatusLabel.Text = $"Loaded {items.Count} posts";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to get timeline: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        LoginBtn.IsEnabled = !isLoading;
        LoadSessionBtn.IsEnabled = !isLoading;
        GetTimelineBtn.IsEnabled = !isLoading && this.client != null;
    }
}

public class TimelineItem
{
    public string AuthorDisplay { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public sealed class OauthStore : IOAuthSessionStore
{
    private readonly string _directory;

    public OauthStore()
    {
        _directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BSkyOAuth");
        Directory.CreateDirectory(_directory);
    }

    public Task StoreAsync(string sub, OAuthSessionData data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, OAuthStoreJsonContext.Default.OAuthSessionData);
        File.WriteAllText(GetPath(sub), json);
        return Task.CompletedTask;
    }

    public Task<OAuthSessionData?> GetAsync(string sub, CancellationToken cancellationToken = default)
    {
        var path = GetPath(sub);
        if (!File.Exists(path))
            return Task.FromResult<OAuthSessionData?>(null);
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize(json, OAuthStoreJsonContext.Default.OAuthSessionData);
        return Task.FromResult(data);
    }

    public Task DeleteAsync(string sub, CancellationToken cancellationToken = default)
    {
        var path = GetPath(sub);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string GetPath(string sub) =>
        Path.Combine(_directory, $"oauth-{sub.Replace(":", "_")}.json");
}

[JsonSerializable(typeof(OAuthSessionData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class OAuthStoreJsonContext : JsonSerializerContext
{
}
