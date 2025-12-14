using admin.ViewModels.Authentication;
using Avalonia.Input;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace admin.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
	public LoginView()
	{
		InitializeComponent();

		this.WhenActivated(disposables =>
		{
			// Bind Username property to TextBox
			this.Bind(ViewModel,
					vm => vm.Username,
					v => v.UsernameTextBox.Text)
				.DisposeWith(disposables);

			// Bind Password property to TextBox
			this.Bind(ViewModel,
					vm => vm.Password,
					v => v.PasswordTextBox.Text)
				.DisposeWith(disposables);

			// Bind LoginCommand to Button
			this.BindCommand(ViewModel,
					vm => vm.LoginCommand,
					v => v.LoginButton)
				.DisposeWith(disposables);

			// Enable/disable button based on IsLoading
			this.OneWayBind(ViewModel,
					vm => vm.IsLoading,
					v => v.LoginButton.IsEnabled,
					isLoading => !isLoading)
				.DisposeWith(disposables);

			// Focus username field when view is activated
			this.WhenAnyValue(x => x.ViewModel)
				.WhereNotNull()
				.Subscribe(_ =>
				{
					UsernameTextBox.Focus();
				})
				.DisposeWith(disposables);

			// Clear password when error occurs
			this.WhenAnyValue(x => x.ViewModel!.ErrorMessage)
				.WhereNotNull()
				.Subscribe(_ =>
				{
					PasswordTextBox.Clear();
					PasswordTextBox.Focus();
				})
				.DisposeWith(disposables);
		});
	}
}