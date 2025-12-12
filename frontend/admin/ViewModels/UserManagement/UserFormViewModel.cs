using ReactiveUI;

namespace admin.ViewModels.UserManagement;

public class UserFormViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment { get; } = "user-form";

	public UserFormViewModel(IScreen screen)
	{
		HostScreen = screen;
	}
}