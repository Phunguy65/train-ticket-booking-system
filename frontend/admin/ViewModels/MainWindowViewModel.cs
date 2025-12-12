using ReactiveUI;
using System.Reactive;

namespace admin.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
	public RoutingState Router { get; } = new RoutingState();

	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

	public MainWindowViewModel()
	{
		Router.Navigate.Execute(new Authentication.LoginViewModel(this));
	}
}