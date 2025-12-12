using admin.ViewModels.UserManagement;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace admin.Views.UserManagement;

public partial class UserListView : ReactiveUserControl<UserListViewModel>
{
    public UserListView()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }
}