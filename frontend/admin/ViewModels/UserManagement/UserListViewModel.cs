using admin.Views.UserManagement;
using ReactiveUI;
using System.Reactive;

namespace admin.ViewModels.UserManagement;

public class UserListViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment => nameof(UserListView);

	public ReactiveCommand<Unit, IRoutableViewModel> NavigateToUserForm { get; }

	public UserListViewModel(IScreen screen)
	{
		HostScreen = screen;
		NavigateToUserForm = ReactiveCommand.CreateFromObservable(() =>
			HostScreen.Router.Navigate.Execute(new UserFormViewModel(HostScreen))
		);
	}
}