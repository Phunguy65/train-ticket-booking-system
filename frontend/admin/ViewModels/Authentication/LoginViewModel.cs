using admin.Views.Authentication;
using ReactiveUI;
using Splat;
using System;
using System.Reactive;

namespace admin.ViewModels.Authentication;

public class LoginViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment => nameof(LoginView);

	public ReactiveCommand<Unit, IRoutableViewModel> NavigateToTrainList { get; }

	public LoginViewModel(IScreen? screen)
	{
		HostScreen = screen ?? Locator.Current.GetService<IScreen>() ?? throw new ArgumentNullException(nameof(screen));

		NavigateToTrainList = ReactiveCommand.CreateFromObservable(() =>
			HostScreen.Router.Navigate.Execute(new TrainManagement.TrainListViewModel(HostScreen))
		);
	}
}