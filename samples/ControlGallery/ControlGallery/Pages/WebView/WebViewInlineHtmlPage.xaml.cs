namespace ControlGallery.Pages.WebView;

public partial class WebViewInlineHtmlPage : ContentPage
{
    public WebViewInlineHtmlPage()
    {
        InitializeComponent();
        RefreshInlineHtml();
    }

    private void OnRefreshInlineHtmlClicked(object? sender, EventArgs e)
    {
        RefreshInlineHtml();
    }

    private void RefreshInlineHtml()
    {
        var generatedAt = DateTime.Now.ToString("HH:mm:ss");
        InlineHtmlWebView.Source = new HtmlWebViewSource
        {
            Html = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <style>
                        body {
                            margin: 0;
                            font-family: "Segoe UI", sans-serif;
                            background: linear-gradient(180deg, #fffaf0 0%, #ffe8c2 100%);
                            color: #5e3200;
                        }

                        main {
                            margin: 24px;
                            padding: 24px;
                            border-radius: 18px;
                            background: rgba(255, 255, 255, 0.92);
                            box-shadow: 0 16px 36px rgba(94, 50, 0, 0.16);
                        }

                        p {
                            line-height: 1.5;
                        }

                        code {
                            font-family: Consolas, monospace;
                            color: #8a3f00;
                        }
                    </style>
                </head>
                <body>
                    <main>
                        <p><strong>Inline HtmlWebViewSource</strong></p>
                        <h1>ControlGallery WebView</h1>
                        <p>This content was generated in code-behind and refreshed at <code>{{generatedAt}}</code>.</p>
                    </main>
                </body>
                </html>
                """
        };

        InlineHtmlStatusLabel.Text = $"Inline HTML refreshed at {generatedAt}.";
    }
}
