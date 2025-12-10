using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.Maui;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace ControlGallery;

internal sealed partial class Program
{
        private static Task Main(string[] args) => BuildAvaloniaApp()
                .WithInterFont()
                .With(
                  new FontManagerOptions
                  {
                        DefaultFamilyName = "avares://ControlGallery.Browser/Assets#Noto Sans",
                        FontFallbacks = new[]
                        {
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://ControlGallery.Browser/Assets#Noto Sans"),
                                },
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://ControlGallery.Browser/Assets#Noto Mono"),
                                },
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://ControlGallery.Browser/Assets#OpenMoji"),
                                        UnicodeRange = UnicodeRange.Parse("U+23??, U+26??, U+2700-27BF, U+2B??, U+1F1E6-1F1FF, U+1F300-1F5FF, U+1F600-1F64F, U+1F680-1F6FF, U+1F9??")
                                }
                        },
                  })
                .StartBrowserAppAsync("out");

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<AvaloniaApp>();
}
