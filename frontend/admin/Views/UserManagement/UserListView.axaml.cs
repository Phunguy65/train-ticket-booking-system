using admin.ViewModels.UserManagement;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views.UserManagement;

public partial class UserListView : ReactiveUserControl<UserListViewModel>
{
	public UserListView()
	{
		this.WhenActivated(_ => { });
		InitializeComponent();
	}
}