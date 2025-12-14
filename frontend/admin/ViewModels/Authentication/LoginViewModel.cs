using admin.Services;
using admin.Views;
using ReactiveUI;
using sdk_client.Exceptions;
using sdk_client.Services;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace admin.ViewModels.Authentication;

public class LoginViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment => nameof(LoginView);

	// Input properties
	private string _username = string.Empty;

	public string Username
	{
		get => _username;
		set => this.RaiseAndSetIfChanged(ref _username, value);
	}

	private string _password = string.Empty;

	public string Password
	{
		get => _password;
		set => this.RaiseAndSetIfChanged(ref _password, value);
	}

	// UI state properties
	private bool _isLoading;

	public bool IsLoading
	{
		get => _isLoading;
		set => this.RaiseAndSetIfChanged(ref _isLoading, value);
	}

	private string? _errorMessage;

	public string? ErrorMessage
	{
		get => _errorMessage;
		set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
	}

	// Commands
	public ReactiveCommand<Unit, IRoutableViewModel> LoginCommand { get; }

	public LoginViewModel(IScreen? screen)
	{
		HostScreen = screen ?? Locator.Current.GetService<IScreen>() ?? throw new ArgumentNullException(nameof(screen));

		// Create login command with validation
		var canLogin = this.WhenAnyValue(
			x => x.Username,
			x => x.Password,
			x => x.IsLoading,
			(username, password, loading) =>
				!string.IsNullOrWhiteSpace(username) &&
				!string.IsNullOrWhiteSpace(password) &&
				!loading
		);

		LoginCommand = ReactiveCommand.CreateFromObservable(
			PerformLogin,
			canLogin
		);

		// Handle errors from LoginCommand
		LoginCommand.ThrownExceptions.Subscribe(ex =>
		{
			ErrorMessage = ex is ApiException apiEx
				? $"Đăng nhập thất bại: {apiEx.Message}"
				: $"Lỗi: {ex.Message}";
			IsLoading = false;
		});
	}

	/// <summary>
	/// Performs the login operation asynchronously.
	/// Calls authentication service, stores session, and navigates to MainView on success.
	/// </summary>
	private IObservable<IRoutableViewModel> PerformLogin()
	{
		return Observable.StartAsync(async () =>
		{
			IsLoading = true;
			ErrorMessage = null;

			try
			{
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					throw new InvalidOperationException("API client not initialized");
				}

				// Connect to server if not already connected
				if (!apiClient.IsConnected)
				{
					await apiClient.ConnectAsync();
				}

				// Call authentication service
				var authService = new AuthenticationService(apiClient);
				var loginResponse = await authService.LoginAsync(Username, Password);

				if (loginResponse == null)
				{
					throw new InvalidOperationException("Login response is null");
				}

				// Store session
				SessionManager.Instance.SetSession(loginResponse);

				IsLoading = false;

				// Navigate to MainView
				return (IRoutableViewModel)new MainViewViewModel(HostScreen);
			}
			catch (Exception)
			{
				IsLoading = false;
				throw; // Re-throw to be handled by ThrownExceptions
			}
		});
	}
}