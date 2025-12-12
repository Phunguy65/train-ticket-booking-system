using ReactiveUI;
using System.Reactive;

namespace admin.ViewModels.Authentication;

public class LoginViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment { get; } = "login";

	public ReactiveCommand<Unit, IRoutableViewModel> NavigateToTrainList { get; }

	public LoginViewModel(IScreen screen)
	{
		HostScreen = screen;
		NavigateToTrainList = ReactiveCommand.CreateFromObservable(
			() => HostScreen.Router.Navigate.Execute(new TrainManagement.TrainListViewModel(HostScreen))
		);
	}
}