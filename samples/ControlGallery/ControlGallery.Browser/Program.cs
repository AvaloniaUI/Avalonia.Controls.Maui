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
                                        FontFamily = new FontFamily("avares://ControlGallery.Browser/Assets#Noto Sans JP"),
                                },
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://ControlGallery.Browser/Assets#Noto Mono"),
                                },
                        },
                  })
                .StartBrowserAppAsync("out");

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<AvaloniaApp>();
}
