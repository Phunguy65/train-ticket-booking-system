using admin.Services;
using admin.Views;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using sdk_client.Exceptions;
using sdk_client.Services;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace admin.ViewModels.Authentication;

public class LoginViewModel : ReactiveValidationObject, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment => nameof(LoginView);
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

	public ReactiveCommand<Unit, IRoutableViewModel> LoginCommand { get; }

	public LoginViewModel(IScreen? screen)
	{
		HostScreen = screen ?? Locator.Current.GetService<IScreen>() ?? throw new ArgumentNullException(nameof(screen));

		this.ValidationRule(
			vm => vm.Username,
			username => !string.IsNullOrWhiteSpace(username),
			"Tên đăng nhập không được để trống");

		this.ValidationRule(
			vm => vm.Username,
			username => string.IsNullOrWhiteSpace(username) || username.Length >= 3,
			"Tên đăng nhập phải có ít nhất 3 ký tự");

		this.ValidationRule(
			vm => vm.Password,
			password => !string.IsNullOrWhiteSpace(password),
			"Mật khẩu không được để trống");

		this.ValidationRule(
			vm => vm.Password,
			password => string.IsNullOrWhiteSpace(password) || password.Length >= 6,
			"Mật khẩu phải có ít nhất 6 ký tự");

		var canLogin = this.WhenAnyValue(
			x => x.IsLoading,
			loading => !loading
		).CombineLatest(
			this.IsValid(),
			(notLoading, isValid) => notLoading && isValid
		);

		LoginCommand = ReactiveCommand.CreateFromObservable(
			PerformLogin,
			canLogin
		);

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

				if (!apiClient.IsConnected)
				{
					await apiClient.ConnectAsync();
				}

				var authService = new AuthenticationService(apiClient);
				var loginResponse = await authService.LoginAsync(Username, Password);

				if (loginResponse == null)
				{
					throw new InvalidOperationException("Login response is null");
				}

				SessionManager.Instance.SetSession(loginResponse);

				IsLoading = false;

				return (IRoutableViewModel)new MainViewViewModel(HostScreen);
			}
			catch (Exception)
			{
				IsLoading = false;
				throw;
			}
		});
	}
}
