using admin.ViewModels.UserManagement;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace admin.Views.UserManagement;

public partial class UserFormView : ReactiveUserControl<UserFormViewModel>
{
	public UserFormView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}