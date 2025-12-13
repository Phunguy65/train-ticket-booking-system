using admin.Views.UserManagement;
using ReactiveUI;

namespace admin.ViewModels.UserManagement;

public class UserFormViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment => nameof(UserFormView);

	public UserFormViewModel(IScreen screen)
	{
		HostScreen = screen;
	}
}