using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlohaAI.Services;
using AlohaAI.ViewModels;
using AlohaAI.Views;
using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Essentials;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace AlohaAI.Browser;

public static class MauiProgram
{
public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseAvaloniaApp(true)
			.UseAvaloniaEssentials()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("CascadiaMono-Regular.ttf", "CascadiaMono");
			});

		// Services
		builder.Services.AddSingleton<IDatabaseService, InMemoryDatabaseService>();
		builder.Services.AddSingleton<IContentService, ContentService>();
		builder.Services.AddSingleton<IProgressService, ProgressService>();
		builder.Services.AddSingleton<IStreakService, StreakService>();

		// ViewModels
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<PathsViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<PathDetailViewModel>();
		builder.Services.AddTransient<LessonViewModel>();
		builder.Services.AddTransient<QuizViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<SearchViewModel>();
		builder.Services.AddTransient<ChatViewModel>();

		// Views
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<PathsPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<PathDetailPage>();
		builder.Services.AddTransient<LessonPage>();
		builder.Services.AddTransient<QuizPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<SearchPage>();
		builder.Services.AddTransient<ChatPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}