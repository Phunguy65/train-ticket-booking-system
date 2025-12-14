using ReactiveUI;
using System.Reactive;

namespace admin.ViewModels.TrainManagement;

public class TrainListViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment { get; } = "trains";

	public ReactiveCommand<Unit, IRoutableViewModel> NavigateToTrainForm { get; }
	public ReactiveCommand<Unit, IRoutableViewModel> NavigateToUserList { get; }
	public ReactiveCommand<Unit, IRoutableViewModel> NavigateToAuditLog { get; }

	public TrainListViewModel(IScreen screen)
	{
		HostScreen = screen;
		NavigateToTrainForm = ReactiveCommand.CreateFromObservable(() =>
			HostScreen.Router.Navigate.Execute(new TrainFormViewModel(HostScreen))
		);
		NavigateToUserList = ReactiveCommand.CreateFromObservable(() =>
			HostScreen.Router.Navigate.Execute(new UserManagement.UserListViewModel(HostScreen))
		);
		NavigateToAuditLog = ReactiveCommand.CreateFromObservable(() =>
			HostScreen.Router.Navigate.Execute(new AuditLogs.AuditLogViewModel(HostScreen))
		);
	}
}