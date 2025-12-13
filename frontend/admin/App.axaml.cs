using admin.Configuration;
using admin.ViewModels;
using admin.ViewModels.AuditLogs;
using admin.ViewModels.Authentication;
using admin.ViewModels.TrainManagement;
using admin.ViewModels.UserManagement;
using admin.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;

namespace admin;

public class App : Application
{
	public IServiceProvider Container { get; private set; } = null!;

	public override void Initialize()
	{
		var host = Host.CreateDefaultBuilder()
			.ConfigureServices((context, services) =>
			{
				services.UseMicrosoftDependencyResolver();
				var resolver = AppLocator.CurrentMutable;
				resolver.InitializeSplat();
				resolver.InitializeReactiveUI();
				services.Configure<AppOptions>(context.Configuration.GetSection(nameof(AppOptions)));
				RegisterDependencies(services);
			})
			.Build();
		Container = host.Services;
		Container.UseMicrosoftDependencyResolver();
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow { DataContext = Container.GetRequiredService<MainWindowViewModel>() };
		}

		base.OnFrameworkInitializationCompleted();
	}

	private void RegisterDependencies(IServiceCollection services)
	{
		services.AddTransient<LoginViewModel>();
		services.AddTransient<MainViewViewModel>();
		services.AddTransient<TrainListViewModel>();
		services.AddTransient<TrainFormViewModel>();
		services.AddTransient<UserListViewModel>();
		services.AddTransient<UserFormViewModel>();
		services.AddTransient<AuditLogViewModel>();
		services.AddTransient<MainWindowViewModel>();
	}
}