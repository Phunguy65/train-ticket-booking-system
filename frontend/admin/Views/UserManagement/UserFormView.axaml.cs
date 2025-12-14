using admin.ViewModels.UserManagement;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views.UserManagement;

public partial class UserFormView : ReactiveUserControl<UserFormViewModel>
{
	public UserFormView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}