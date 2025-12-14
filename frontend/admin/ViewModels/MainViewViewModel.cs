using ReactiveUI;
using System.Reactive;

namespace admin.ViewModels
{
	public class MainViewViewModel : ViewModelBase, IRoutableViewModel, IScreen
	{
		public string UrlPathSegment => nameof(Views.MainView);
		public RoutingState Router { get; } = new RoutingState();
		public IScreen HostScreen { get; }

		// Commands for menu navigation
		public ReactiveCommand<Unit, IRoutableViewModel> NavigateToTrainList { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> NavigateToUserList { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> NavigateToAuditLog { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> Logout { get; }

		public MainViewViewModel(IScreen hostScreen)
		{
			HostScreen = hostScreen;

			// Initialize with default view (TrainList)
			Router.Navigate.Execute(new TrainManagement.TrainListViewModel(this));

			// Setup navigation commands
			NavigateToTrainList = ReactiveCommand.CreateFromObservable(() =>
				Router.Navigate.Execute(new TrainManagement.TrainListViewModel(this))
			);

			NavigateToUserList = ReactiveCommand.CreateFromObservable(() =>
				Router.Navigate.Execute(new UserManagement.UserListViewModel(this))
			);

			NavigateToAuditLog =
				ReactiveCommand.CreateFromObservable(() =>
					Router.Navigate.Execute(new AuditLogs.AuditLogViewModel(this))
				);

			Logout = ReactiveCommand.CreateFromObservable(() =>
			{
				// Clear session
				Services.SessionManager.Instance.ClearSession();

				// Navigate back to log in
				return HostScreen.Router.Navigate.Execute(
					new Authentication.LoginViewModel(HostScreen)
				);
			});
		}
	}
}