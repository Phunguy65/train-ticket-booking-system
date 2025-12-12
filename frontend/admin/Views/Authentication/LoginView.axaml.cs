using admin.ViewModels.Authentication;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views.Authentication;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
	public LoginView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}